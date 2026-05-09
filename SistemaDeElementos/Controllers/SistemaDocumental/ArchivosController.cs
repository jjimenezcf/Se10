using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Entorno;
using GestoresDeNegocio.Negocio;
using GestoresDeNegocio.SistemaDocumental;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModeloDeDto.SistemaDocumental;
using MVCSistemaDeElementos.UtilidadesIu;
using Newtonsoft.Json;
using NuGet.Protocol.Core.Types;
using RtfPipe;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Gastos;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Utilidades;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Utilidades;
using static SistemaDeElementos.Middleware.EliminarFicherosMiddelware;

namespace MVCSistemaDeElementos.Controllers;


public class ArchivosController : EntidadController<ContextoSe, ArchivoDtm, ArchivoDto>
{

    public ArchivosController(GestorDeArchivos gestorDeArchivos, GestorDeErrores gestorDeErrores)
        : base
        (
            gestorDeArchivos,
            gestorDeErrores
        )
    {
    }

    [AllowAnonymous]
    public ActionResult epDescargaConGuid(string guid, int id)
    {
        // Obtener información de la petición
        string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        string userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
        string referer = HttpContext.Request.Headers["Referer"].ToString();
        var tran = Contexto.IniciarTransaccion();
        Contexto.IniciarTraza(nameof(epDescargaConGuid));
        var error = false;
        try
        {
            Contexto.AsignarUsuario(ExtensorDeUsuarios.Administrador(Contexto));
            ((GestorDeArchivos)_GestorDeElementos).ValidarDescargaConGuid(guid, id);
            Contexto.AnotarTraza("Descarga realizada", $"Descarga realizada por la ip: {ipAddress}");
            return DescargarArchivo(id, auditarDescarga: false, errorSiNoEsta: true);
        }
        catch (Exception ex)
        {
            error = true;
            Contexto.AnotarExcepcion(ex);

            var masInformacion = $@"
                <p>IP: {ipAddress}</p>
                <p>User Agent: {userAgent}</p>
                <p>Referer: {referer}</p>
                </body></html>";
            return DevolverPaginaWebConMensaje(ex.Message, masInformacion);
        }
        finally
        {
            if (!error)
                Contexto.Commit(tran);
            else
                Contexto.Rollback(tran);
            Contexto.CerrarTraza();
            Contexto.QuitarUsuario();
        }
    }


    public JsonResult epRegistrarDescargaConGuid(int id, DateTime? caducaEl)
    {
        {
            var resultado = new Resultado();
            var tran = Contexto.IniciarTransaccion();

            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(_GestorDeElementos.Contexto, _GestorDeElementos.Mapeador, HttpContext);
                var archivo = _GestorDeElementos.LeerRegistroPorId(id, false, false, false, false);

                if (caducaEl == null) caducaEl = DateTime.Now.AddHours(1);
                resultado.Datos = ((GestorDeArchivos)_GestorDeElementos).RegistrarDescargaConGuid(id, caducaEl: caducaEl, maximoDeDescargas: null);
                resultado.Mensaje = $"La URL para descargar el archivo se ha copiado al porta papeles con validez hasta '{((DateTime)caducaEl).ToString(extFechas.DiaHora)}'";
                resultado.Estado = enumEstadoPeticion.Ok;
                _GestorDeElementos.Commit(tran);
            }
            catch (Exception e)
            {
                _GestorDeElementos.Rollback(tran);
                ApiController.PrepararError(e, resultado, $"Error al generar un Guid para descarga.");
            }
            finally
            {
                Contexto.CerrarTraza();
            }

            return new JsonResult(resultado);
        }
    }

    [AllowAnonymous]
    public IActionResult epDescargarArchivoPorGuid(string negocio, int idElemento, int idArchivo, string guid)
    {
        Contexto.IniciarTraza(nameof(epDescargarArchivoPorGuid));
        try
        {
            Contexto.AsignarUsuario(ExtensorDeUsuarios.Administrador(Contexto));
            ValidarConsultaPorGuid(NegociosDeSe.ToEnumerado(negocio), idElemento, guid);
            return epDescargarArchivo(negocio, idElemento, idArchivo, false);
        }
        catch (Exception ex)
        {
            return DevolverPaginaWebConMensaje(ex.Message);
        }
        finally
        {
            Contexto.CerrarTraza();
            Contexto.QuitarUsuario();
        }
    }

    public FileStreamResult epDescargarArchivo(string negocio, int idElemento, int idArchivo, bool auditar)
    {
        ApiController.CumplimentarDatosDeUsuarioDeConexion(_GestorDeElementos.Contexto, _GestorDeElementos.Mapeador, HttpContext);

        var enumNegocio = NegociosDeSe.ToEnumerado(negocio, nullValido: true);
        if (enumNegocio != enumNegocio.No_Definido)
        {
            var gestor = enumNegocio.CrearGestor(Contexto);
            IElementoDtm elemento;
            if (enumNegocio == enumNegocio.Carpeta)
            {
                var carpeta = (CarpetaDtm)gestor.LeerRegistroPorId(idElemento, aplicarJoin: false);
                elemento = Contexto.SeleccionarPorId<ArchivadorDtm>(carpeta.IdArchivador);
            }
            else elemento = (ElementoDtm)gestor.LeerRegistroPorId(idElemento, aplicarJoin: false);

            if (!elemento.EsConsultor(Contexto))
                throw new Exception($"No tiene acceso al elemento {elemento.Referencia(Contexto)}");
        }

        return DescargarArchivo(idArchivo, auditarDescarga: auditar);
    }

    public FileStreamResult epDescargarArchivoParaCrear(int idArchivo)
    {
        ApiController.CumplimentarDatosDeUsuarioDeConexion(_GestorDeElementos.Contexto, _GestorDeElementos.Mapeador, HttpContext);
        var archivo = _GestorDeElementos.LeerRegistroPorId(idArchivo, false, false, false, false);

        //if (archivo.IdUsuaCrea != Contexto.DatosDeConexion.IdUsuario)
        //    throw new Exception("No tiene acceso al archivo, ya que no lo ha subido ud");

        //if (((GestorDeArchivos)_GestorDeElementos).ExisteAlgunVinculoAl(archivo))
        //    throw new Exception("El archivo está anexado a un elemento, acceda a él consultando los anexados a dicho elemento");

        var ruta = ApiDeArchivos.DescargarArchivo(archivo.Id, archivo.Nombre, archivo.AlmacenadoEn, enumRutas.RutaDeDescarga, usarCacheado: true, ponerTickAlNombre: true);

        return DevolverStream(ruta, MimeTypeMap.GetMimeType(Path.GetExtension(archivo.Nombre)), archivo.Nombre);
    }

    public FileStreamResult epDescargarRtfToHtml(int idArchivo)
    {
        ApiController.CumplimentarDatosDeUsuarioDeConexion(_GestorDeElementos.Contexto, _GestorDeElementos.Mapeador, HttpContext);
        try
        {
            var archivo = _GestorDeElementos.LeerRegistroPorId(idArchivo, false, false, false, false);
            var rutaHtml = Path.Combine(CacheDeVariable.Cfg_RutaDeDescarga, $"{Path.GetFileNameWithoutExtension(archivo.Nombre)}.{enumExtensiones.html}");
            var rutaRtf = "";
            var pendienteDeBorrar = new Ficheros(HttpContext);
            if (!Path.Exists(rutaHtml))
            {
                rutaRtf = ApiDeArchivos.DescargarArchivo(archivo.Id, archivo.Nombre, archivo.AlmacenadoEn, enumRutas.RutaDeDescarga, usarCacheado: true, ponerTickAlNombre: false);
                if (rutaRtf.Contains(ApiDeArchivos.FicheroNoEncontrado))
                {
                    throw new FileNotFoundException($"El archivo '{archivo.Nombre}' no se ha encontrado.");
                }

                string contenidoRtf = System.IO.File.ReadAllText(rutaRtf);
                string contenidoHtml = Rtf.ToHtml(contenidoRtf);
                System.IO.File.WriteAllText(rutaHtml, contenidoHtml);
                rutaHtml = extHtml.SanitizeFile(rutaHtml);
                pendienteDeBorrar.Add(rutaRtf);
            }
            pendienteDeBorrar.Add(rutaHtml);
            return DevolverStream(rutaHtml, MimeTypeMap.GetMimeType(enumExtensiones.html.ToString()), Path.GetFileName(rutaHtml));
        }
        catch (Exception ex)
        {
            return DevolverErrorDeFichero(ex);
        }
    }

    public FileStreamResult epDescargarDocxToHtml(int idArchivo)
    {
        ApiController.CumplimentarDatosDeUsuarioDeConexion(_GestorDeElementos.Contexto, _GestorDeElementos.Mapeador, HttpContext);
        try
        {
            var archivo = _GestorDeElementos.LeerRegistroPorId(idArchivo, false, false, false, false);
            var rutaHtml = Path.Combine(CacheDeVariable.Cfg_RutaDeDescarga, $"{Path.GetFileNameWithoutExtension(archivo.Nombre)}.{enumExtensiones.html}");
            var rutaDocx = "";
            var pendienteDeBorrar = new Ficheros(HttpContext);
            if (!Path.Exists(rutaHtml))
            {
                rutaDocx = ApiDeArchivos.DescargarArchivo(archivo.Id, archivo.Nombre, archivo.AlmacenadoEn, enumRutas.RutaDeDescarga, usarCacheado: true, ponerTickAlNombre: false);
                if (rutaDocx.Contains(ApiDeArchivos.FicheroNoEncontrado))
                {
                    throw new FileNotFoundException($"El archivo '{archivo.Nombre}' no se ha encontrado.");
                }

                string contenidoHtml = extDocx.ToHtml(rutaDocx);
                System.IO.File.WriteAllText(rutaHtml, contenidoHtml);
                rutaHtml = extHtml.SanitizeFile(rutaHtml);
                pendienteDeBorrar.Add(rutaDocx);
            }
            pendienteDeBorrar.Add(rutaHtml);
            return DevolverStream(rutaHtml, MimeTypeMap.GetMimeType(enumExtensiones.html.ToString()), Path.GetFileName(rutaHtml));
        }
        catch (Exception ex)
        {
            return DevolverErrorDeFichero(ex);
        }
    }

    public FileStreamResult epDescargarXlsxToHtml(int idArchivo)
    {
        ApiController.CumplimentarDatosDeUsuarioDeConexion(_GestorDeElementos.Contexto, _GestorDeElementos.Mapeador, HttpContext);
        try
        {
            var archivo = _GestorDeElementos.LeerRegistroPorId(idArchivo, false, false, false, false);
            var rutaHtml = Path.Combine(CacheDeVariable.Cfg_RutaDeDescarga, $"{Path.GetFileNameWithoutExtension(archivo.Nombre)}.{enumExtensiones.html}");
            var rutaXlsx = "";
            var pendienteDeBorrar = new Ficheros(HttpContext);
            if (!Path.Exists(rutaHtml))
            {
                rutaXlsx = ApiDeArchivos.DescargarArchivo(archivo.Id, archivo.Nombre, archivo.AlmacenadoEn, enumRutas.RutaDeDescarga, usarCacheado: true, ponerTickAlNombre: false);
                if (rutaXlsx.Contains(ApiDeArchivos.FicheroNoEncontrado))
                {
                    throw new FileNotFoundException($"El archivo '{archivo.Nombre}' no se ha encontrado.");
                }

                string contenidoHtml = extXlsx.ToHtml(rutaXlsx);
                System.IO.File.WriteAllText(rutaHtml, contenidoHtml);
                rutaHtml = extHtml.SanitizeFile(rutaHtml);
                pendienteDeBorrar.Add(rutaXlsx);
            }
            pendienteDeBorrar.Add(rutaHtml);
            return DevolverStream(rutaHtml, MimeTypeMap.GetMimeType(enumExtensiones.html.ToString()), Path.GetFileName(rutaHtml));
        }
        catch (Exception ex)
        {
            return DevolverErrorDeFichero(ex);
        }
    }

    public FileStreamResult epDescargarCsvToHtml(int idArchivo)
    {
        ApiController.CumplimentarDatosDeUsuarioDeConexion(_GestorDeElementos.Contexto, _GestorDeElementos.Mapeador, HttpContext);
        try
        {
            var archivo = _GestorDeElementos.LeerRegistroPorId(idArchivo, false, false, false, false);
            var rutaHtml = Path.Combine(CacheDeVariable.Cfg_RutaDeDescarga, $"{Path.GetFileNameWithoutExtension(archivo.Nombre)}.{enumExtensiones.html}");
            var rutaCsv = "";
            var pendienteDeBorrar = new Ficheros(HttpContext);
            if (!Path.Exists(rutaHtml))
            {
                rutaCsv = ApiDeArchivos.DescargarArchivo(archivo.Id, archivo.Nombre, archivo.AlmacenadoEn, enumRutas.RutaDeDescarga, usarCacheado: true, ponerTickAlNombre: false);
                if (rutaCsv.Contains(ApiDeArchivos.FicheroNoEncontrado))
                {
                    throw new FileNotFoundException($"El archivo '{archivo.Nombre}' no se ha encontrado.");
                }

                string contenidoHtml = extCsv.ToHtml(rutaCsv);
                System.IO.File.WriteAllText(rutaHtml, contenidoHtml);
                pendienteDeBorrar.Add(rutaCsv);
            }
            pendienteDeBorrar.Add(rutaHtml);
            return DevolverStream(rutaHtml, MimeTypeMap.GetMimeType(enumExtensiones.html.ToString()), Path.GetFileName(rutaHtml));
        }
        catch (Exception ex)
        {
            return DevolverErrorDeFichero(ex);
        }
    }

    public FileStreamResult epDescargarZipToHtml(int idArchivo)
    {
        ApiController.CumplimentarDatosDeUsuarioDeConexion(_GestorDeElementos.Contexto, _GestorDeElementos.Mapeador, HttpContext);
        try
        {
            var archivo = _GestorDeElementos.LeerRegistroPorId(idArchivo, false, false, false, false);
            var rutaHtml = Path.Combine(CacheDeVariable.Cfg_RutaDeDescarga, $"{Path.GetFileNameWithoutExtension(archivo.Nombre)}.{enumExtensiones.html}");
            var rutaZip = "";
            var pendienteDeBorrar = new Ficheros(HttpContext);
            if (!Path.Exists(rutaHtml))
            {
                rutaZip = ApiDeArchivos.DescargarArchivo(archivo.Id, archivo.Nombre, archivo.AlmacenadoEn, enumRutas.RutaDeDescarga, usarCacheado: true, ponerTickAlNombre: false);
                if (rutaZip.Contains(ApiDeArchivos.FicheroNoEncontrado))
                {
                    throw new FileNotFoundException($"El archivo '{archivo.Nombre}' no se ha encontrado.");
                }
                var mime = MimeTypeMap.GetMimeType(Path.GetExtension(rutaZip));
                //string contenidoHtml = mime == MimeTypeMap.GetMimeType(enumExtensiones.zip.ToString()) ? Ext7z.ToHtml(rutaZip) : Ext7z.ToHtml(rutaZip);
                string contenidoHtml = ext7z.ToHtml(rutaZip);
                System.IO.File.WriteAllText(rutaHtml, contenidoHtml);
                rutaHtml = extHtml.SanitizeFile(rutaHtml);
                pendienteDeBorrar.Add(rutaZip);
            }
            pendienteDeBorrar.Add(rutaHtml);
            return DevolverStream(rutaHtml, MimeTypeMap.GetMimeType(enumExtensiones.html.ToString()), Path.GetFileName(rutaHtml));
        }
        catch (Exception ex)
        {
            return DevolverErrorDeFichero(ex);
        }
    }

    public FileStreamResult epDescargarHtmlSanitizado(int idArchivo)
    {
        ApiController.CumplimentarDatosDeUsuarioDeConexion(_GestorDeElementos.Contexto, _GestorDeElementos.Mapeador, HttpContext);
        try
        {
            var archivo = _GestorDeElementos.LeerRegistroPorId(idArchivo, false, false, false, false);
            var rutaHtmlSanitizado = Path.Combine(CacheDeVariable.Cfg_RutaDeDescarga, $"{Path.GetFileNameWithoutExtension(archivo.Nombre)}.{enumExtensiones.html}");
            var rutaOriginal = "";
            var pendienteDeBorrar = new Ficheros(HttpContext);

            if (!Path.Exists(rutaHtmlSanitizado))
            {
                rutaOriginal = ApiDeArchivos.DescargarArchivo(archivo.Id, archivo.Nombre, archivo.AlmacenadoEn, enumRutas.RutaDeDescarga, usarCacheado: true, ponerTickAlNombre: false);
                pendienteDeBorrar.Add(rutaOriginal);
                if (rutaOriginal.Contains(ApiDeArchivos.FicheroNoEncontrado))
                {
                    throw new FileNotFoundException($"El archivo '{archivo.Nombre}' no se ha encontrado.");
                }

                string contenido = System.IO.File.ReadAllText(rutaOriginal);
                // Comprobar si es texto plano y convertir a HTML si es necesario
                if (extHtml.EsHtml(contenido))
                {
                    System.IO.File.WriteAllText(rutaHtmlSanitizado, contenido);
                    rutaHtmlSanitizado = extHtml.SanitizeFile(rutaHtmlSanitizado);
                }
                else
                {
                    rutaHtmlSanitizado = rutaOriginal;
                }
            }

            pendienteDeBorrar.Add(rutaHtmlSanitizado);
            return DevolverStream(rutaHtmlSanitizado, MimeTypeMap.GetMimeType(Path.GetExtension(rutaOriginal)), Path.GetFileName(rutaHtmlSanitizado));
        }
        catch (Exception ex)
        {
            return DevolverErrorDeFichero(ex);
        }
    }

    private class ArchivoResultado
    {
        public int IdArchivo { get; set; }
        public string Nombre { get; set; }
    }

    public async Task<JsonResult> epProcesarAccion(int idArchivo, string accion, enumNegocio negocio, int idElemento)
    {
        var resultado = new Resultado();
        var pendienteDeBorrar = new Ficheros(HttpContext);
        var tran = Contexto.IniciarTransaccion();

        var enumAccion = ApiDeEnsamblados.DescripcionToEnumerado<enumAccionVisorArchivo>(accion);
        try
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(_GestorDeElementos.Contexto, _GestorDeElementos.Mapeador, HttpContext);
            var archivo = _GestorDeElementos.LeerRegistroPorId(idArchivo, false, false, false, false);
            var rutaArchivo = ApiDeArchivos.ObtenerRutaArchivo(archivo);
            pendienteDeBorrar.Add(rutaArchivo);


            string contenidoProcesado = await ProcesarArchivo(negocio, enumAccion, idArchivo, rutaArchivo);

            if (!string.IsNullOrEmpty(contenidoProcesado))
            {
                resultado = GuardarResultadoProcesado(contenidoProcesado, archivo, enumAccion, idElemento, pendienteDeBorrar);
            }
            else
            {
                resultado.Datos = await System.IO.File.ReadAllTextAsync(rutaArchivo);
            }

            resultado.Estado = enumEstadoPeticion.Ok;
            _GestorDeElementos.Commit(tran);
        }
        catch (Exception e)
        {
            _GestorDeElementos.Rollback(tran);
            ApiController.PrepararError(e, resultado, $"Error al '{enumAccion.Descripcion()}'.");
        }
        finally
        {
            Contexto.CerrarTraza();
        }

        return new JsonResult(resultado);
    }


    private async Task<string> ProcesarArchivo(enumNegocio negocio, enumAccionVisorArchivo enumAccion, int idArchivo, string rutaArchivo)
    {
        var procesadorOcr = new ProcesadorOcr.ProcesadorOcr();
        if (enumAccion == enumAccionVisorArchivo.PasarOcr)
        {
            return await procesadorOcr.ProcesarFichero(idArchivo, rutaArchivo);
        }
        var iaUsada = ExtensorDeUsuarios.IaUsada(Contexto);
        var ia = ExtensorDeIa.CrearIa(iaUsada);

        string resultadoExitoso = null;
        try
        {
            if (enumAccion == enumAccionVisorArchivo.Resumir)
            {
                resultadoExitoso = await Resumir(negocio, ia, rutaArchivo);
            }
            else
            {
                resultadoExitoso = await ExtensorDeIa.ProcesarFactura(idArchivo, rutaArchivo, ia, procesadorOcr);
            }
        }
        catch (Exception e)
        {
            if (e.Message.Contains("API key not valid"))
                GestorDeErrores.Emitir($"La ApiKey para la ia '{iaUsada.Nombre}' no es válida, actualicela");
            if (e.Message.Contains("Too Many Request"))
                GestorDeErrores.Emitir($"Demasadas peticiones a la ia '{iaUsada.Nombre}', actualice a una versión de pago o pruebe más adelante");
            throw;
        }

        return resultadoExitoso;
    }

    private async Task<string> Resumir(enumNegocio negocio, IIa ia, string rutaArchivo)
    {
        string mimeType = MimeTypeMap.GetMimeType(Path.GetExtension(rutaArchivo));

        bool esIaGeminis = ia.GetType() == typeof(IaGeminis);
        bool esIaMistral = ia.GetType() == typeof(IaMistral);
        bool esTipoMimeAdmitido = (esIaGeminis || esIaMistral) && ((IIaTiposMimesAdmitidos)ia).TiposMimeAdmitidosParaResumen.Contains(mimeType);

        if ((esIaGeminis || esIaMistral) && esTipoMimeAdmitido)
        {
            return await ExtensorDeIa.ResumirFichero((IIaTiposMimesAdmitidos)ia, negocio, rutaArchivo);
        }
        else
        {
            string contenido = ApiDeArchivos.ExtraerTextoPlano(rutaArchivo);
            return await ExtensorDeIa.ResumirContenido(ia, negocio, contenido);
        }
    }

    private Resultado GuardarResultadoProcesado(string contenidoProcesado, ArchivoDtm archivo, enumAccionVisorArchivo enumAccion, int idElemento, Ficheros pendientesDeBorrar)
    {
        var resultado = new Resultado();
        var nombreArchivoIa = $"{enumAccion}_{Path.GetFileNameWithoutExtension(archivo.Nombre).NormalizarFichero()}.{enumExtensiones.txt}";
        var rutaProcesado = Path.Combine(CacheDeVariable.Cfg_RutaDeDescarga, nombreArchivoIa);
        rutaProcesado = ServidorDocumental.SalvarFichero(rutaProcesado, contenidoProcesado);
        pendientesDeBorrar.Add(rutaProcesado);

        if (enumAccion == enumAccionVisorArchivo.AnalizarFactura)
        {
            if (idElemento > 0)
            {
                var archivoAnexado = Contexto.SeleccionarPorId<FacturaRecDtm>(idElemento).AnexarArchivo(Contexto, rutaProcesado);
                resultado.Datos = new ArchivoResultado { IdArchivo = archivoAnexado.Id, Nombre = archivoAnexado.Nombre };
                return resultado;
            }
            var idArchivoSubido = Contexto.SubirArchivo(rutaProcesado, sanitizar: true);
            resultado.Datos = new ArchivoResultado { IdArchivo = idArchivoSubido, Nombre = Path.GetFileName(rutaProcesado) };
            return resultado;
        }

        resultado.Datos = Contexto.SubirArchivo(rutaProcesado, sanitizar: true);
        return resultado;
    }


    private FileStreamResult DevolverErrorDeFichero(Exception ex)
    {
        var emptyStream = new MemoryStream();

        // Crear un FileStreamResult con el stream vacío
        var result = new FileStreamResult(emptyStream, MimeTypeMap.ApplicationOctetStream)
        {
            FileDownloadName = "error.txt"
        };

        // Agregar el mensaje de error como un encabezado personalizado
        result.FileDownloadName = "error.txt";
        Response.Headers.Append("X-Error-Message", ex.Message);

        return result;
    }

    public FileStreamResult epDescargarThumsnail(string negocio, int idElemento, int idArchivo, int ancho, int alto)
    {
        ApiController.CumplimentarDatosDeUsuarioDeConexion(_GestorDeElementos.Contexto, _GestorDeElementos.Mapeador, HttpContext);
        //todo: validar acceso al negocio en consulta
        var ruta = ApiDeArchivos.FicheroNoEncontrado;
        var mimeType = MimeTypeMap.GetMimeType(Path.GetExtension(ruta));
        var nombreFichero = ApiDeArchivos.NombreDelFicheroNoEncontrado;
        Contexto.IniciarTraza(nameof(epDescargarThumsnail));
        try
        {
            var archivo = _GestorDeElementos.LeerRegistroPorId(idArchivo, false, false, false, false);
            mimeType = MimeTypeMap.GetMimeType(Path.GetExtension(archivo.Nombre));
            nombreFichero = archivo.Nombre;
            ruta = ApiDeArchivos.DescargarThumbnail(Contexto, idArchivo, archivo.Nombre, archivo.AlmacenadoEn, ancho, alto, mimeType);
        }
        catch (Exception e)
        {
            if (_GestorDeElementos.Contexto.Traza != null)
                _GestorDeElementos.Contexto.Traza.AnotarExcepcion(e);
        }
        finally
        {
            Contexto.CerrarTraza();
        }

        return DevolverStream(ruta, mimeType, nombreFichero);
    }

    public JsonResult epLeerCertificados(string negocio, int idElemento, int idArchivo)
    {
        var r = new Resultado();

        try
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(_GestorDeElementos.Contexto, _GestorDeElementos.Mapeador, HttpContext);
            var negocioSe = NegociosDeSe.ToEnumerado(negocio);

            r.Datos = ((GestorDeArchivos)_GestorDeElementos).LeerCertificadosDisponibles(negocioSe, idElemento, idArchivo);
            r.ModoDeAcceso = ((List<CertificadosDisponiblesDto>)r.Datos).Count == 0 ? enumModoDeAccesoDeDatos.Consultor.Render() : enumModoDeAccesoDeDatos.Gestor.Render();

            r.Estado = enumEstadoPeticion.Ok;
        }
        catch (Exception e)
        {
            ApiController.PrepararError(e, r, "Error al obtener los certificados.");
        }

        return new JsonResult(r);
    }

    public JsonResult epLeerDatosDeFirma(string negocio, int idElemento, int idArchivo)
    {
        var r = new Resultado();

        try
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(_GestorDeElementos.Contexto, _GestorDeElementos.Mapeador, HttpContext);
            var negocioSe = NegociosDeSe.ToEnumerado(negocio);

            r.Datos = ((GestorDeArchivos)_GestorDeElementos).LeerDatosDeFirma(negocioSe, idElemento, idArchivo);
            r.ModoDeAcceso = ((enumModoDeAccesoDeDatos)r.Datos.ModoDeAcceso).Render();
            r.Estado = enumEstadoPeticion.Ok;
            r.Mensaje = "Datos de la firma leida";
        }
        catch (Exception e)
        {
            ApiController.PrepararError(e, r, "Error al obtener la información de la firma del archivo.");
        }

        return new JsonResult(r);
    }

    [AllowAnonymous]
    public JsonResult epLeerAnexadosPorGuid(string negocio, int idElemento, int posicion, int cantidad, string guid)
    {
        Contexto.IniciarTraza(nameof(epLeerAnexadosPorGuid));
        try
        {
            Contexto.AsignarUsuario(ExtensorDeUsuarios.Administrador(Contexto));
            ValidarConsultaPorGuid(NegociosDeSe.ToEnumerado(negocio), idElemento, guid);
            return epLeerAnexados(negocio, idElemento, posicion, cantidad, guid);
        }
        finally
        {
            Contexto.CerrarTraza();
            Contexto.QuitarUsuario();
        }
    }

    public JsonResult epLeerAnexados(string negocio, int idElemento, int posicion, int cantidad, string guid)
    {
        var r = new Resultado();
        try
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(_GestorDeElementos.Contexto, _GestorDeElementos.Mapeador, HttpContext);
            var negocioSe = NegociosDeSe.ToEnumerado(negocio);

            if (negocioSe == enumNegocio.Carpeta && posicion == 0)
                VinculoSql.BlanquearCacheDeAnexados(Contexto, enumNegocio.Carpeta.TipoDtm(), idElemento);
            List<ArchivoDto> anexados;
            try
            {
                anexados = ((GestorDeArchivos)_GestorDeElementos).LeerAnexados(negocioSe, idElemento, posicion, cantidad, guid, new Dictionary<string, object> {
                    { ltrParametrosNeg.IncluirOriginales, false},
                    { ltrParametrosNeg.Peticion, enumPeticion.epLeerAnexados }
                });
            }
            catch (Exception exc)
            {
                var contiene = exc.Message.Contains("The given key");
                if (contiene && exc.Message.Contains($"{guid}-{idElemento}"))
                    if (!contiene) throw;
                anexados = new List<ArchivoDto>();
                r.Mensaje = "No se han podido leer los archivos anexos, recargue la página";
            }
            r.Datos = anexados;
            r.Estado = enumEstadoPeticion.Ok;
            r.ModoDeAcceso = ((GestorDeArchivos)_GestorDeElementos).ValidarPermisosSobreElAnexado(negocioSe, idElemento).Render();
        }
        catch (Exception e)
        {
            ApiController.PrepararError(e, r, "No se ha podido leer.");
        }

        return new JsonResult(r);
    }

    public JsonResult epLeerVinculosAl(int idArchivo)
    {
        var r = new Resultado();

        try
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(_GestorDeElementos.Contexto, _GestorDeElementos.Mapeador, HttpContext);
            var vinculos = ((GestorDeArchivos)_GestorDeElementos).CualquierVinculadoAl(Contexto.SeleccionarPorId<ArchivoDtm>(idArchivo), detalleDelVinculado: false, excluirCancelados: true);

            r.Datos = vinculos;
            r.Estado = enumEstadoPeticion.Ok;
            r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
        }
        catch (Exception e)
        {
            ApiController.PrepararError(e, r, "No se ha podido leer los vínculos al archivo.");
        }

        return new JsonResult(r);

    }

    public JsonResult epQuitarAnexado(string negocio, int idElemento, int idArchivo)
    {
        var r = new Resultado();

        var tran = _GestorDeElementos.IniciarTransaccion();
        try
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(_GestorDeElementos.Contexto, _GestorDeElementos.Mapeador, HttpContext);
            var negocioSe = NegociosDeSe.ToEnumerado(negocio);

            ((GestorDeArchivos)_GestorDeElementos).QuitarAnexado(negocioSe, idElemento, idArchivo, enumPeticion.epQuitarAnexado);

            r.Estado = enumEstadoPeticion.Ok;
            r.Mensaje = "Archivo eliminado";
            _GestorDeElementos.Commit(tran);
        }
        catch (Exception e)
        {
            _GestorDeElementos.Rollback(tran);
            ApiController.PrepararError(e, r, "No se ha podido eliminar.");
        }

        return new JsonResult(r);
    }

    public JsonResult epFirmarArchivo(int idNegocio, int idElemento, int idArchivo, int idCertificado, string password)
    {
        var r = new Resultado();

        var tran = _GestorDeElementos.IniciarTransaccion();
        Contexto.IniciarTraza(nameof(epFirmarArchivo));
        try
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(_GestorDeElementos.Contexto, _GestorDeElementos.Mapeador, HttpContext);
            var negocioSe = NegociosDeSe.ToEnumerado(idNegocio);

            ((GestorDeArchivos)_GestorDeElementos).FirmarAnexado(negocioSe, idElemento, idArchivo, idCertificado, password, new Dictionary<string, object>());

            r.Estado = enumEstadoPeticion.Ok;
            r.Mensaje = "Archivo firmado";
            r.ModoDeAcceso = ApiDePermisos.LeerModoDeAcceso(Contexto, negocioSe, idElemento).Render();
            _GestorDeElementos.Commit(tran);
        }
        catch (Exception e)
        {
            _GestorDeElementos.Rollback(tran);
            ApiController.PrepararError(e, r, "No se ha podido firmar el archivo.");
        }
        finally
        {
            Contexto.CerrarTraza();
        }

        return new JsonResult(r);
    }

    public JsonResult epAnularFirma(int idNegocio, int idElemento, int idArchivo)
    {
        var r = new Resultado();

        var tran = _GestorDeElementos.IniciarTransaccion();
        Contexto.IniciarTraza(nameof(epAnularFirma));
        try
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(_GestorDeElementos.Contexto, _GestorDeElementos.Mapeador, HttpContext);
            var negocioSe = NegociosDeSe.ToEnumerado(idNegocio);

            ((GestorDeArchivos)_GestorDeElementos).AnularFirma(negocioSe, idElemento, idArchivo);

            r.Estado = enumEstadoPeticion.Ok;
            r.Mensaje = "Firma anulada";
            r.ModoDeAcceso = ApiDePermisos.LeerModoDeAcceso(Contexto, negocioSe, idElemento).Render();
            _GestorDeElementos.Commit(tran);
        }
        catch (Exception e)
        {
            _GestorDeElementos.Rollback(tran);
            ApiController.PrepararError(e, r, "No se ha podido anular la firma del archivo.");
        }
        finally
        {
            Contexto.CerrarTraza();
        }

        return new JsonResult(r);
    }

    public JsonResult epBloquearArchivo(int idNegocio, int idElemento, int idArchivo, string motivo)
    {
        var r = new Resultado();
        var tran = _GestorDeElementos.IniciarTransaccion();
        Contexto.IniciarTraza(GetType().Name + "_" + nameof(epBloquearArchivo));
        try
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
            var negocio = idNegocio == 0 ? enumNegocio.No_Definido : NegociosDeSe.ToEnumerado(idNegocio);
            ServidorDocumental.BloquearArchivo(Contexto, idNegocio, idElemento, idArchivo, motivo);
            r.Consola = $"bloqueo realizada correctamente";
            r.Estado = enumEstadoPeticion.Ok;
            _GestorDeElementos.Commit(tran);
        }
        catch (Exception e)
        {
            _GestorDeElementos.Rollback(tran);
            ApiController.PrepararError(e, r, $"Error al bloquear el archivo.");
        }
        finally
        {
            Contexto.CerrarTraza();
        }
        return new JsonResult(r);
    }

    public JsonResult epDesbloquearArchivo(int idNegocio, int idElemento, int idArchivo, string motivo)
    {
        var r = new Resultado();
        var tran = _GestorDeElementos.IniciarTransaccion();
        Contexto.IniciarTraza(GetType().Name + "_" + nameof(epDesbloquearArchivo));
        try
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
            var negocio = idNegocio == 0 ? enumNegocio.No_Definido : NegociosDeSe.ToEnumerado(idNegocio);
            ServidorDocumental.DesbloquearArchivo(Contexto, idNegocio, idElemento, idArchivo, motivo);
            r.Consola = $"desbloqueo realizada correctamente";
            r.Estado = enumEstadoPeticion.Ok;
            _GestorDeElementos.Commit(tran);
        }
        catch (Exception e)
        {
            _GestorDeElementos.Rollback(tran);
            ApiController.PrepararError(e, r, $"Error al desbloquear el archivo.");
        }
        finally
        {
            Contexto.CerrarTraza();
        }
        return new JsonResult(r);
    }

    public async Task<IActionResult> epSubirTrozo(IFormFile chunk, string fileName, string chunkNumber, string totalChunks)
    {
        ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
        Contexto.IniciarTraza(GetType().Name + "_" + nameof(epSubirTrozo));
        try
        {
            if (chunk == null || chunk.Length == 0)
                return BadRequest("No se ha proporcionado ningún fragmento de archivo.");

            var tempDir = Path.Combine(CacheDeVariable.CFG_Ruta_Ficheros_De_Zip, $"{Contexto.DatosDeConexion.IdUsuario}", "TrozosSubidos", fileName);
            Directory.CreateDirectory(tempDir);

            var chunkPath = Path.Combine(tempDir, $"chunk_{chunkNumber}.part");
            using (var stream = new FileStream(chunkPath, FileMode.Create))
            {
                await chunk.CopyToAsync(stream);
            }
            var idArchivo = 0;

            // Verificar si todos los fragmentos han sido subidos
            if (chunkNumber.Entero() == totalChunks.Entero())
            {
                try
                {
                    idArchivo = await ComponerSubirArchivo(fileName, totalChunks, tempDir);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error al subir el archivo: {ex.Message}", ex);
                }
            }

            return Ok(new { message = $"Fragmento {chunkNumber} de {totalChunks} recibido correctamente.", idArchivo });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al procesar el fragmento del archivo.", error = ex.Message });
        }
        finally
        {
            Contexto.CerrarTraza();
        }
    }

    private async Task<int> ComponerSubirArchivo(string fileName, string totalChunks, string tempDir)
    {
        try
        {
            var finalPath = Path.Combine(GestorDeVariables.RutaDeDescarga, fileName);
            using (var outputStream = new FileStream(finalPath, FileMode.Create))
            {
                for (int i = 1; i <= totalChunks.Entero(); i++)
                {
                    var partPath = Path.Combine(tempDir, $"chunk_{i}.part");
                    using (var inputStream = new FileStream(partPath, FileMode.Open))
                    {
                        await inputStream.CopyToAsync(outputStream);
                    }
                    System.IO.File.Delete(partPath);
                }
            }
            var idArchivo = Contexto.SubirArchivo(finalPath, sanitizar: true);
            Directory.Delete(tempDir, true);
            return idArchivo;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error en ComponerSubirArchivo: {ex.Message}", ex);
        }
    }

    [HttpPost]
    public JsonResult epProcesarArchivos(string operacion, int idNegocio, int idOrigen, int idDestino, string enumNegocioDestino)
    {
        var r = new Resultado();
        var tran = _GestorDeElementos.IniciarTransaccion();
        Contexto.IniciarTraza(GetType().Name + "_" + nameof(epProcesarArchivos));
        try
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);

            var body = ApiController.LeerBody(HttpContext);
            var negocio = idNegocio == 0 ? enumNegocio.No_Definido : NegociosDeSe.ToEnumerado(idNegocio);
            List<long> idsDeArchivos = JsonConvert.DeserializeObject<List<long>>(body.parametros[ltrDeUnArchivo.IdsDeArchivos].ToString());
            var enumOpercion = ApiDeEnsamblados.ToEnumerado<enumOperacionesConArchivos>(operacion);
            ServidorDocumental.ProcesarOperacion(Contexto, enumOpercion, idNegocio, idOrigen, ApiDeEnsamblados.ToEnumerado<enumNegocio>(enumNegocioDestino), idDestino, idsDeArchivos);
            r.Consola = $"'{operacion}' realizada correctamente";
            r.Estado = enumEstadoPeticion.Ok;
            _GestorDeElementos.Commit(tran);
        }
        catch (Exception e)
        {
            _GestorDeElementos.Rollback(tran);
            ApiController.PrepararError(e, r, $"Error al realizar la operación de '{operacion}'.");
        }
        finally
        {
            Contexto.CerrarTraza();
        }
        return new JsonResult(r);
    }
    [HttpPost]
    public JsonResult epOperacionConArchivos(string operacion, int idNegocioOrigen, int idElementoOrigen, int idNegocioDestino, int idElementoDestino)
    {
        var r = new Resultado();
        var tran = _GestorDeElementos.IniciarTransaccion();
        Contexto.IniciarTraza(GetType().Name + "_" + nameof(epOperacionConArchivos));
        try
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);

            var body = ApiController.LeerBody(HttpContext);
            var negocioOrigen = NegociosDeSe.ToEnumerado(idNegocioOrigen);
            List<long> idsDeArchivos = JsonConvert.DeserializeObject<List<long>>(body.parametros[ltrDeUnArchivo.IdsDeArchivos].ToString());
            var enumOpercion = ApiDeEnsamblados.ToEnumerado<enumOperacionesConArchivos>(operacion);
            ServidorDocumental.ProcesarOperacion(Contexto, enumOpercion, idNegocioOrigen, idElementoOrigen, NegociosDeSe.ToEnumerado(idNegocioDestino), idElementoDestino, idsDeArchivos);
            r.Consola = $"'{operacion}' realizada correctamente";
            r.Estado = enumEstadoPeticion.Ok;
            _GestorDeElementos.Commit(tran);
        }
        catch (Exception e)
        {
            _GestorDeElementos.Rollback(tran);
            ApiController.PrepararError(e, r, $"Error al realizar la operación de '{operacion}'.");
        }
        finally
        {
            Contexto.CerrarTraza();
        }
        return new JsonResult(r);
    }

    [HttpPost]
    public JsonResult epGenerarZip(int idNegocio, int idOrigen, string nombre)
    {
        var r = new Resultado();
        var tran = _GestorDeElementos.IniciarTransaccion();
        Contexto.IniciarTraza(GetType().Name + "_" + nameof(epGenerarZip));
        try
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);

            var body = ApiController.LeerBody(HttpContext);
            var negocio = idNegocio == 0 ? enumNegocio.No_Definido : NegociosDeSe.ToEnumerado(idNegocio);
            ServidorDocumental.SometerGenerarZip(Contexto, nombre, body.parametrosJson);
            r.Consola = $"'Sometida la generación del archivador '{nombre}'";
            r.Estado = enumEstadoPeticion.Ok;
            _GestorDeElementos.Commit(tran);
        }
        catch (Exception e)
        {
            _GestorDeElementos.Rollback(tran);
            ApiController.PrepararError(e, r, $"Error al realizar al someter el trabajo para generar el Zip.");
        }
        finally
        {
            Contexto.CerrarTraza();
        }
        return new JsonResult(r);
    }

    [HttpPost]
    public JsonResult epBloquearArchivos(int idNegocio, int idOrigen, string motivo)
    {
        var r = new Resultado();
        var tran = _GestorDeElementos.IniciarTransaccion();
        Contexto.IniciarTraza(GetType().Name + "_" + nameof(epBloquearArchivos));
        try
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
            var body = ApiController.LeerBody(HttpContext);
            var negocio = idNegocio == 0 ? enumNegocio.No_Definido : NegociosDeSe.ToEnumerado(idNegocio);
            List<long> idsDeArchivos = JsonConvert.DeserializeObject<List<long>>(body.parametros[ltrDeUnArchivo.IdsDeArchivos].ToString());
            ServidorDocumental.BloquearArchivos(Contexto, idNegocio, idOrigen, idsDeArchivos, motivo);
            r.Consola = $"Archivos bloqueados correctamente";
            r.Estado = enumEstadoPeticion.Ok;
            _GestorDeElementos.Commit(tran);
        }
        catch (Exception e)
        {
            _GestorDeElementos.Rollback(tran);
            ApiController.PrepararError(e, r, $"Error al bloquear los archivos.");
        }
        finally
        {
            Contexto.CerrarTraza();
        }
        return new JsonResult(r);
    }
    [HttpPost]
    public JsonResult epDesbloquearArchivos(int idNegocio, int idOrigen, string motivo)
    {
        var r = new Resultado();
        var tran = _GestorDeElementos.IniciarTransaccion();
        Contexto.IniciarTraza(GetType().Name + "_" + nameof(epBloquearArchivos));
        try
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
            var body = ApiController.LeerBody(HttpContext);
            var negocio = idNegocio == 0 ? enumNegocio.No_Definido : NegociosDeSe.ToEnumerado(idNegocio);
            List<long> idsDeArchivos = JsonConvert.DeserializeObject<List<long>>(body.parametros[ltrDeUnArchivo.IdsDeArchivos].ToString());
            ServidorDocumental.DesbloquearArchivos(Contexto, idNegocio, idOrigen, idsDeArchivos, motivo);
            r.Consola = $"Archivos desbloqueados correctamente";
            r.Estado = enumEstadoPeticion.Ok;
            _GestorDeElementos.Commit(tran);
        }
        catch (Exception e)
        {
            _GestorDeElementos.Rollback(tran);
            ApiController.PrepararError(e, r, $"Error al desbloquear los archivos.");
        }
        finally
        {
            Contexto.CerrarTraza();
        }
        return new JsonResult(r);
    }

    [HttpPost]
    public JsonResult epAnexarArchivo(string negocio, int idElemento, IFormFile fichero)
    {
        var r = new Resultado();

        var tran = _GestorDeElementos.IniciarTransaccion();
        try
        {
            if (fichero == null)
            {
                GestorDeErrores.Emitir("No se ha identificado el fichero");
            }

            ApiController.CumplimentarDatosDeUsuarioDeConexion(_GestorDeElementos.Contexto, _GestorDeElementos.Mapeador, HttpContext);
            var rutaConFichero = $@"{GestorDeVariables.RutaDeDescarga}\{fichero.FileName}";

            using (var stream = new FileStream(rutaConFichero, FileMode.Create))
            {
                fichero.CopyTo(stream);
            }

            var negocioSe = NegociosDeSe.ToEnumerado(negocio);
            if (negocioSe == enumNegocio.No_Definido)
            {
                throw new Exception($"No está definido el negocio {negocio}");
            }

            r.Datos = ServidorDocumental.AnexarArchivo(_GestorDeElementos.Contexto, negocioSe, idElemento, rutaConFichero, sanitizar: true);
            r.Estado = enumEstadoPeticion.Ok;
            r.Mensaje = "fichero anexado";
            _GestorDeElementos.Commit(tran);
        }
        catch (Exception e)
        {
            _GestorDeElementos.Rollback(tran);
            ApiController.PrepararError(e, r, "No se ha podido anexar el fichero.");
        }


        return new JsonResult(r);
    }

    [HttpGet("movil/[controller]/anexos/{negocio}/{idElemento:int}")]
    public JsonResult GetArchivos([FromRoute] int idElemento, [FromRoute] string negocio)
    {
        if (Enum.TryParse(negocio, true, out enumNegocio enumNegocio))
        {
            var negocioSe = NegociosDeSe.LeerNegocioPorEnumerado(enumNegocio);
            var ret = epLeerAnexados(negocioSe.Nombre, idElemento, 0, 10, "se-Move");
            return MappearResultadoToArchivoMovilOutput(ret);
        }

        var r = new Resultado
        {
            Estado = enumEstadoPeticion.Error,
            Mensaje = "Error al parsear el negocio"
        };
        return new JsonResult(r);

    }

    [HttpPost("movil/[controller]/anexos/{negocio}/{idElemento:int}/upload")]
    [RequestSizeLimit(20971520)] // 20 megabytes in bytes
    //[DisableRequestSizeLimit,
    // RequestFormLimits(MultipartBodyLengthLimit = int.MaxValue,
    // ValueLengthLimit = int.MaxValue)]
    public async Task<JsonResult> UploadFile([FromRoute] int idElemento, [FromRoute] string negocio, [FromForm] IFormFile file)
    {
        var r = new Resultado();
        var tran = _GestorDeElementos.IniciarTransaccion();
        try
        {
            if (file is null)
            {
                GestorDeErrores.Emitir("No se ha identificado el fichero");
            }

            ApiController.CumplimentarDatosDeUsuarioDeConexion(_GestorDeElementos.Contexto, _GestorDeElementos.Mapeador, HttpContext);
            var filePath = $@"{GestorDeVariables.RutaDeDescarga}\{file.FileName}";

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            if (Enum.TryParse(negocio, true, out enumNegocio negocioSe))
            {
                var resultDtm = ServidorDocumental.AnexarArchivo(_GestorDeElementos.Contexto, negocioSe, idElemento, filePath, sanitizar: true);
                var resultDto = _GestorDeElementos.MapearElemento(resultDtm);
                r.Datos = Mapeador.Map<ArchivoDto, ArchivoMovilOutput>(resultDto);
                r.Estado = enumEstadoPeticion.Ok;
                r.Mensaje = "fichero anexado";
                _GestorDeElementos.Commit(tran);
            }
            else
            {
                GestorDeErrores.Emitir($"No está definido el negocio {negocio}");
            }
        }
        catch (Exception e)
        {
            _GestorDeElementos.Rollback(tran);
            ApiController.PrepararError(e, r, "No se ha podido anexar el fichero.");
        }

        return new JsonResult(r);
    }

    [HttpGet("movil/[controller]/{id:int}/download")]
    public IActionResult GetFileDownload([FromRoute] int id)
    {
        ApiController.CumplimentarDatosDeUsuarioDeConexion(_GestorDeElementos.Contexto, _GestorDeElementos.Mapeador, HttpContext);
        var file = _GestorDeElementos.LeerRegistroPorId(id, false, false, false, false);
        return GetFileStreamResult(id, file);
    }


    private IActionResult GetFileStreamResult(int id, ArchivoDtm file)
    {
        var filePath = $@"{file.AlmacenadoEn}\{id}.se";
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound();
        }

        var cd = new ContentDisposition
        {
            FileName = Uri.EscapeDataString(file.Nombre),
            Inline = false
        };
        Response.Headers.Append("Content-Disposition", cd.ToString());
        Response.Headers.Append("X-Content-Type-Options", "nosniff");

        var mimeType = MimeTypeMap.GetMimeType(Path.GetExtension(file.Nombre));

        return new FileStreamResult(new FileStream(filePath, FileMode.Open, FileAccess.Read), mimeType);
    }

    private JsonResult MappearResultadoToArchivoMovilOutput(JsonResult ret)
    {
        var result = (Resultado)ret.Value;
        if (result is not null && result.Estado == enumEstadoPeticion.Ok)
        {
            result.Datos = Mapeador.Map<List<ArchivoDto>, List<ArchivoMovilOutput>>(result.Datos);
        }

        return ret;
    }


}
