using System;
using System.Collections.Generic;
using AutoMapper;
using Gestor.Errores;
using GestoresDeNegocio.Entorno;
using Microsoft.AspNetCore.Http;
using ModeloDeDto.Entorno;
using ServicioDeDatos;
using ServicioDeDatos.Entorno;
using Microsoft.AspNetCore.Mvc;
using ModeloDeDto;
using Utilidades;
using System.Reflection;
using GestorDeElementos;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Elemento;
using Newtonsoft.Json;
using System.IO;
using MVCSistemaDeElementos.Descriptores;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using ModeloDeDto.Negocio;
using System.Linq;
using System.Web;
using System.Net.Http;
using System.Threading.Tasks;
using GestorDeElementos.Extensores;
using ServicioDeDatos.SistemaDocumental;

namespace MVCSistemaDeElementos.Controllers
{
    public static class ApiController
    {

        public static void CumplimentarDatosDeUsuarioDeConexion(ContextoSe contexto, IMapper mapeador, HttpContext httpContext, Dictionary<string,object> parametros = null)
        {
            if (contexto.Test) return;

            if (!ApiController.HayUsuarioEnLaRequest(httpContext) && contexto.Usuario.EsAdministrador)
                return;

                if (parametros != null && parametros.LeerValor(ltrParametrosEp.ConsultarConGuid, false))
            {
                contexto.AsignarUsuario(ExtensorDeUsuarios.Administrador(contexto));
            }
            else
            {
                contexto.AsignarLogin(ObtenerUsuarioDeLaRequest(httpContext), emitirError: true);
            }
            contexto.Mapeador = mapeador;

            contexto.AnotarTraza("Petición", $"Método: {httpContext.Request.Method}" + Environment.NewLine +
                $"Ruta: {httpContext.Request.Path}" + Environment.NewLine +
                $"Parámetros: {httpContext.Request.QueryString}");
        }

        public static bool HayUsuarioEnLaRequest(HttpContext httpContext)
        {
            if (httpContext == null || httpContext.User == null) return false;

            var caracter = httpContext.User.FindFirst(nameof(UsuarioDto.Login));
            if (caracter == null)
                return false;

            return !httpContext.User.FindFirst(nameof(UsuarioDto.Login)).Value.IsNullOrEmpty();
        }

        internal static (int idNegocio, int idElemento) ObtenerNegocioYelemento(List<ClausulaDeFiltrado> filtros)
        {
            var idNegocio = 0;
            var idElemento = 0;
            foreach (var f in filtros)
            {
                if (f.Clausula.ToLower() == NegocioPor.idNegocio.ToLower())
                    idNegocio = f.Valor.Entero();
                else if (f.Clausula.ToLower() == ltrFiltros.enumNegocio.ToLower())
                    idNegocio = ApiDeEnsamblados.ToEnumerado<enumNegocio>(f.Valor).IdNegocio();
                else if (f.Clausula.ToLower() == ltrParametrosEp.negocio)
                    idNegocio = ApiDeEnsamblados.ToEnumerado<enumNegocio>(f.Valor).IdNegocio();
                else if (f.Clausula.ToLower() == nameof(IUsaElemento.IdElemento).ToLower())
                    idElemento = f.Valor.Entero();
            }

            if (idNegocio == 0)
                GestorDeErrores.Emitir("Debe indicar el negocio");
            if (idElemento == 0)
                GestorDeErrores.Emitir("Debe indicar el elemento");
            return (idNegocio, idElemento);
        }

        public static string ObtenerUsuarioDeLaRequest(HttpContext httpContext)
        {
            if (httpContext == null)
                return null;

            if (httpContext.User == null)
                GestorDeErrores.Emitir("Conexión no establecidad");

            var caracter = httpContext.User.FindFirst(nameof(UsuarioDto.Login));
            if (caracter == null)
                GestorDeErrores.Emitir("Usuario no definido");

            return httpContext.User.FindFirst(nameof(UsuarioDto.Login)).Value;
        }

        internal static async Task ValidarUrl(string url)
        {
            if (!url.IsNullOrEmpty())
            {
                try
                {
                    using var client = new HttpClient();
                    client.Timeout = TimeSpan.FromSeconds(30);
                    var response = await client.GetAsync(url);
                    if (!response.IsSuccessStatusCode)
                        GestorDeErrores.Emitir($"La url proporcionada '{url}' no es válida, estado devuelto: {response.StatusCode}");
                }
                catch (HttpRequestException e)
                {
                    GestorDeErrores.Emitir($"La url proporcionada '{url}' no es válida", e);
                    throw;
                }
                catch (Exception e)
                {
                    GestorDeErrores.Emitir($"La url proporcionada '{url}' no es válida", e);
                    throw;
                }
            }
            else
            {
                GestorDeErrores.Emitir($"La url proporcionada '{url}' no es válida");
            }
        }

        internal static async Task<string> ValidarUrlAsync(string url)
        {
            if (!url.IsNullOrEmpty())
            {
                HttpClient client = new HttpClient();
                HttpResponseMessage response;

                response = await client.GetAsync(url);
                if (response.StatusCode != HttpStatusCode.OK)
                    return $"La url proporcionada no es válida, estado devuelto: {response.StatusCode}";
            }
            return "";
        }

        public static FileStreamResult DescargarArchivo(ContextoSe contexto, int idArchivo, string rutaDedDescarga, bool usarCacheado)
        {
            var archivo = contexto.SeleccionarPorId<ArchivoDtm>(idArchivo);
            archivo.AuditarDescarga(contexto);
            if (!usarCacheado && archivo.EstaDescargado(rutaDedDescarga))
                archivo.EliminarDescarga(rutaDedDescarga);

            var ruta = ApiDeArchivos.DescargarArchivo(archivo.Id, archivo.Nombre, archivo.AlmacenadoEn, rutaDedDescarga, usarCacheado: true, ponerTickAlNombre: true);

            return DevolverStream(ruta, MimeTypeMap.GetMimeType(Path.GetExtension(archivo.Nombre)), archivo.Nombre);
        }

        private static FileStreamResult DevolverStream(string ruta, string mimeType, string nombreFichero)
        {
            var stream = new FileStream(ruta, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            return new FileStreamResult(stream, mimeType)
            {
                FileDownloadName = nombreFichero
            };
        }

        internal static (string parametrosJson, Dictionary<string, object> parametros) LeerBody(HttpContext httpContext)
        {
            StreamReader reader = new StreamReader(httpContext.Request.Body);
            var parametrosJson = reader.ReadToEnd();
            var decodedString = HttpUtility.UrlDecode(parametrosJson);
            var parametros = decodedString.ToDiccionarioDeParametros();
            return (decodedString, parametros);
        }

        public static (IEnumerable<T> elementos, int total) LeerDatosParaElGrid<T>(
            Func<IEnumerable<T>> Leer
          , Func<int> Contar)
        where T : ElementoDto
        {
            int total;
            IEnumerable<T> elementos;
            elementos = Leer();
            total = Contar();
            return (elementos, total);
        }

        public static List<Dictionary<string, object>> ElementosLeidos<T>(List<T> elementos, Func<enumModoDeAccesoDeDatos> LeerModoAccesoAlElemento)
        where T : ElementoDto
        {
            var listaDeElementos = new List<Dictionary<string, object>>();
            if (elementos.Count > 0)
            {
                PropertyInfo[] propiedades = elementos[0].GetType().GetProperties();

                foreach (T elemento in elementos)
                {
                    var registro = new Dictionary<string, object>();
                    foreach (PropertyInfo propiedad in propiedades)
                    {
                        object valor = elemento.GetType().GetProperty(propiedad.Name).GetValue(elemento);
                        registro[propiedad.Name] = valor == null ? "" : valor;
                    }
                    var ma = LeerModoAccesoAlElemento();
                    registro[nameof(Resultado.ModoDeAcceso)] = ma.Render();
                    listaDeElementos.Add(registro);
                }
            }

            return listaDeElementos;
        }

        public static void PrepararError(Exception e, Resultado r, string asunto)
        {
            r.Estado = enumEstadoPeticion.Error;
            if (e.Message == ltrDeUnUsuario.Logout)
            {
                r.logout = true;
            }
            else
            {
                r.Consola = e.Data.Contains(GestorDeErrores.Datos.Consola) ? e.Data[GestorDeErrores.Datos.Consola].ToString() : GestorDeErrores.Detalle(e);
                PrepararMensaje(e, r, asunto);
            }
        }

        private static void PrepararMensaje(Exception e, Resultado r, string asunto)
        {
            if (e.Data.Contains(GestorDeErrores.Datos.Mostrar) && (bool)e.Data[GestorDeErrores.Datos.Mostrar] == true)
                r.Mensaje = e.Message;
            else
            {
                r.Mensaje = $"{asunto}";
                if (e.InnerException != null && r.Mensaje.Trim() == asunto.Trim())
                {
                    r.Mensaje = e.InnerException.Message;
                }
            }
        }
        public static T LeerPorId<C, R, T>(GestorDeElementos<C, R, T> gestor, int id, Dictionary<string, object> parametros)
        where C : ContextoSe
        where R : RegistroDtm
        where T : ElementoDto
        {
            parametros.Add(ltrParametrosDto.DescargarGestionDocumental, true);

            if (ApiDeEnsamblados.ImplementaInterface(gestor.GetType(), typeof(IGestorGenerico).FullName))
            {
                var idNegocio = (int)parametros.LeerValor<long>(ltrParametrosEp.idNegocio);
                var negocio = NegociosDeSe.ToEnumerado(idNegocio);
                ((IGestorGenerico)gestor).AsignarNegocio(negocio);
            }

            return gestor.LeerElementoPorId(id, parametros);
        }

        public static JsonResult BorrarPorId<C, R, T>(GestorDeElementos<C, R, T> gestor, int id, string parametrosJson, HttpContext httpContext, Func<T, ParametrosDeNegocio> AntesDeBorrar)
        where C : ContextoSe
        where R : RegistroDtm
        where T : ElementoDto
        {
            var r = new Resultado();
            var tran = gestor.IniciarTransaccion();
            try
            {
                CumplimentarDatosDeUsuarioDeConexion(gestor.Contexto, gestor.Mapeador, httpContext);
                var parametros = extJson.ToDiccionarioDeParametros(parametrosJson);
                var elemento = LeerPorId(gestor, id, parametros);
                AntesDeBorrar(elemento);
                parametros[ltrParametrosNeg.EsUnaPeticion] = true;
                parametros[ltrParametrosNeg.Peticion] = enumPeticion.epBorrarPorId;
                gestor.PersistirElementoDto(elemento, new ParametrosDeNegocio(enumTipoOperacion.Eliminar, parametros));

                r.Total = 1;
                r.Consola = "elemento eliminado";
                //r.Mensaje = $"Se ha eliminado el elemento";
                r.Estado = enumEstadoPeticion.Ok;
                gestor.Commit(tran);
            }
            catch (Exception e)
            {
                gestor.Rollback(tran);
                PrepararError(e, r, "Error en el proceso de eliminar de elemento.");
            }

            return new JsonResult(r);
        }

        public static JsonResult BorrarRelacionPorId<C, R, T>(GestorDeRelaciones<C, R, T> gestor, int id, string parametrosJson, HttpContext httpContext, Func<T, ParametrosDeNegocio> AntesDeBorrar)
        where C : ContextoSe
        where R : RelacionDtm
        where T : ElementoDto
        {
            var r = new Resultado();
            var tran = gestor.IniciarTransaccion();
            try
            {
                CumplimentarDatosDeUsuarioDeConexion(gestor.Contexto, gestor.Mapeador, httpContext);
                var parametros = extJson.ToDiccionarioDeParametros(parametrosJson);
                var elemento = LeerPorId(gestor, id, parametros);
                AntesDeBorrar(elemento);
                parametros[ltrParametrosNeg.EsUnaPeticion] = true;
                parametros[ltrParametrosNeg.Peticion] = enumPeticion.epBorrarRelacionPorId;
                gestor.BorrarRelacion(id, parametros);

                r.Total = 1;
                r.Consola = "Relación eliminada";
                //r.Mensaje = $"Se ha eliminado la relación";
                r.Estado = enumEstadoPeticion.Ok;
                gestor.Commit(tran);
            }
            catch (Exception e)
            {
                gestor.Rollback(tran);
                PrepararError(e, r, "Error en el proceso de eliminar relación.");
            }

            return new JsonResult(r);
        }
        public static JsonResult ProcesarElemento<T>(ContextoSe contexto, string elementoJson, HttpContext httpContext, Func<ContextoSe, T, object> metodo)
        where T : ElementoDto
        {
            var r = new Resultado();
            var tran = contexto.IniciarTransaccion();
            try
            {
                CumplimentarDatosDeUsuarioDeConexion(contexto, contexto.Mapeador, httpContext);
                var elemento = JsonConvert.DeserializeObject<T>(elementoJson);
                r.Datos = metodo(contexto, elemento);
                r.Estado = enumEstadoPeticion.Ok;
                r.Mensaje = "método ejecutado";
                contexto.Commit(tran);
            }
            catch (Exception e)
            {
                contexto.Rollback(tran);
                PrepararError(e, r, "No se ha podido ejecutar el método.");
            }

            return new JsonResult(r);
        }

        public static JsonResult CrearElemento<C, R, T>(GestorDeElementos<C, R, T> gestor, string elementoJson, string parametrosDeCreacion, HttpContext httpContext, Func<T, ParametrosDeNegocio> AntesDePersistir)
        where C : ContextoSe
        where R : RegistroDtm
        where T : ElementoDto
        {
            var r = new Resultado();
            var tran = gestor.IniciarTransaccion();
            try
            {
                CumplimentarDatosDeUsuarioDeConexion(gestor.Contexto, gestor.Mapeador, httpContext);
                var elemento = JsonConvert.DeserializeObject<T>(elementoJson);
                var parametros = AntesDePersistir(elemento);
                parametros.EsUnaPeticion = true;
                parametros.Peticion = enumPeticion.PersistirElemento;
                parametros.Parametros = extJson.ToDiccionarioDeParametros(parametrosDeCreacion);
                r.Datos = gestor.PersistirElementoDto(elemento, parametros);
                r.Estado = enumEstadoPeticion.Ok;
                r.Mensaje = "Petición realizada";
                gestor.Commit(tran);
            }
            catch (Exception e)
            {
                gestor.Rollback(tran);
                PrepararError(e, r, "No se ha podido realizar la petición.");
            }

            return new JsonResult(r);
        }

        public static JsonResult PersistirElemento<C, R, T>(GestorDeElementos<C, R, T> gestor, string elementoJson, HttpContext httpContext, Func<T, ParametrosDeNegocio> AntesDePersistir)
        where C : ContextoSe
        where R : RegistroDtm
        where T : ElementoDto
        {
            var r = new Resultado();
            var tran = gestor.IniciarTransaccion();
            try
            {
                CumplimentarDatosDeUsuarioDeConexion(gestor.Contexto, gestor.Mapeador, httpContext);
                var elemento = JsonConvert.DeserializeObject<T>(elementoJson);
                var parametros = AntesDePersistir(elemento);
                parametros.EsUnaPeticion = true;
                parametros.Peticion = enumPeticion.PersistirElemento;
                r.Datos = gestor.PersistirElementoDto(elemento, parametros);
                r.Estado = enumEstadoPeticion.Ok;
                r.Mensaje = "Petición realizada";
                gestor.Commit(tran);
            }
            catch (Exception e)
            {
                gestor.Rollback(tran);
                PrepararError(e, r, "No se ha podido realizar la petición.");
            }

            return new JsonResult(r);
        }

        public static IEnumerable<T> LeerElementos<C, R, T>(GestorDeElementos<C, R, T> gestor, int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden, Dictionary<string, object> opcionesDeMapeo)
        where C : ContextoSe
        where R : RegistroDtm
        where T : ElementoDto
        {
            opcionesDeMapeo[nameof(ltrParametrosNeg.Peticion)] = enumPeticion.epLeerElementos;
            return gestor.LeerElementos(posicion, cantidad, filtros, orden, opcionesDeMapeo);
        }


        public static int BuscarNegocio(List<ClausulaDeFiltrado> filtros, Dictionary<string, object> parametros)
        {
            var idNegocio = Convert.ToInt32(parametros.LeerValor(ltrParametrosEp.idNegocio, (long)0));
            if (idNegocio > 0)
                return idNegocio;

            var negocio = (string)parametros.LeerValor(ltrParametrosEp.negocio, "");
            if (!negocio.IsNullOrEmpty())
                return NegociosDeSe.ToEnumerado(negocio).IdNegocio();

            foreach (var filtro in filtros.Where(filtro => filtro.Clausula.Equals(nameof(EstadoDto.IdNegocio), StringComparison.CurrentCultureIgnoreCase)))
            {
                return filtro.Valor.Entero();
            }

            return 0;
        }

        public static enumNegocio AsignarNegocioDeLaRequest<C, R, T>(GestorDeElementos<C, R, T> gestor, HttpContext httpContext)
        where C : ContextoSe
        where R : RegistroDtm
        where T : ElementoDto
        {
            if (!httpContext.Request.Query.ContainsKey(ltrParametrosEp.negocio))
                GestorDeErrores.Emitir("No se ha indicado el negocio del que se solicitan los estados");

            var nombre = httpContext.Request.Query[ltrParametrosEp.negocio].ToString();
            var negocio = NegociosDeSe.ToEnumerado(nombre);
            if (negocio == enumNegocio.No_Definido)
                GestorDeErrores.Emitir($"El negocio {nombre} no está definido");

            ((IGestorGenerico)gestor).AsignarNegocio(negocio);
            return negocio;
        }

        public static string PrepararDescriptor<TElemento>(Controller controlador, ControllerContext contextoDelCtrl, DescriptorDeCrud<TElemento> descriptor, ContextoSe contexto, HttpContext httpContext)
        where TElemento : ElementoDto
        {
            //var negocio = NegociosDeSe.NegocioDeUnDto(typeof(TElemento));
            //if (descriptor.NegocioDtm == null && negocio != enumNegocio.No_Definido)
            //{
            //    descriptor.Negocio = negocio;
            //    descriptor.NegocioDtm = GestorDeNegocios.LeerNegocio(contexto, NegociosDeSe.NegocioDeUnDto(typeof(TElemento)));
            //}

            var gestorDeVista = GestorDeVistaMvc.Gestor(contexto, contexto.Mapeador);
            var vista = gestorDeVista.LeerVistaMvc($"{descriptor.Controlador}.{descriptor.Vista}");

            if (descriptor.Creador != null) descriptor.Creador.AbrirEnModal = vista.MostrarEnModal;
            if (descriptor.Editor != null) descriptor.Editor.AbrirEnModal = vista.MostrarEnModal;

            CumplimentarDatosDeUsuarioDeConexion(contexto, contexto.Mapeador, httpContext);
            descriptor.GestorDeUsuario = GestorDeUsuarios.Gestor(contexto, contexto.Mapeador);
            descriptor.UsuarioConectado = descriptor.GestorDeUsuario.LeerRegistroCacheado(nameof(UsuarioDtm.Login), contexto.DatosDeConexion.Login, errorSiNoHay: true, errorSiHayMasDeUno: true, aplicarJoin: false);

            var destino = $"../{descriptor.RutaBase}/{descriptor.Vista}";
            if (!controlador.ExisteLaVista(destino))
                throw new Exception($"La vista {destino}.cshtml no está definida en el directorio de Views");

            string nombreDeLaVista = contextoDelCtrl.RouteData.Values["action"].ToString();
            string nombreDelControlador = contextoDelCtrl.RouteData.Values["controller"].ToString();

            if (!descriptor.UsuarioConectado.EsAdministrador)
            {
                var hayPermisos = descriptor.GestorDeUsuario.TienePermisoFuncional(descriptor.UsuarioConectado, $"{nombreDelControlador}.{nombreDeLaVista}");
                if (!hayPermisos)
                    throw new Exception($"Solicite permisos de acceso a {destino}");

                hayPermisos = descriptor.GestorDeUsuario.TienePermisoDeDatos(enumModoDeAccesoDeDatos.Consultor, descriptor.Negocio);
                if (!hayPermisos)
                    throw new Exception($"Solicite al menos permisos de consulta sobre los elementos de negocio {descriptor.Negocio.ToNombre()}");
            }
            return destino;
        }


        public static string PrepararPagina(Controller controlador, ControllerContext contextoDelCtrl, DescriptorDePaginaDeConsulta pagina)
        {
            var destino = $"../{pagina.RutaBase}/{pagina.Vista}";
            if (!controlador.ExisteLaVista(destino))
                throw new Exception($"La vista {destino}.cshtml no está definida en el directorio de Views");

            string nombreDeLaVista = contextoDelCtrl.RouteData.Values["action"].ToString();
            string nombreDelControlador = contextoDelCtrl.RouteData.Values["controller"].ToString();

            return destino;
        }

        public static FileStream DevolverFichero(HttpResponse Response, ContextoSe contexto, string ruta, string mimeType, string nombreFichero)
        {
            var contentDisposition = new System.Net.Mime.ContentDisposition
            {
                Inline = true,
                FileName = nombreFichero.Replace("\r", "")
            };

            try
            {
                Response.Headers.Append("Content-Disposition", contentDisposition.ToString());
            }
            catch (Exception exc)
            {
                contexto.AnotarExcepcion(exc);
                Response.Headers.Append("Content-Disposition", "");
            }


            // You don't need to dispose the stream. It will be disposed by the FileStreamResult.WriteFile method
            // Fuente: "https://stackoverflow.com/questions/3084366/how-do-i-dispose-my-filestream-when-implementing-a-file-download-in-asp-net"
            return new FileStream(ruta, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        }

        public static bool ExisteLaVista(this ControllerBase controller, string name)
        {
            var services = controller.HttpContext.RequestServices;
            var viewEngine = services.GetRequiredService<ICompositeViewEngine>();
            var result = viewEngine.GetView(null, $"{name.Replace("..", "Views")}.cshtml", true);
            return result.Success;
        }

        internal static JsonResult CrearElemento<T>(ContextoSe contexto, HttpContext httpContext, Type tipoDtm, string elementoJson)
        {
            var r = new Resultado();
            var tran = contexto.IniciarTransaccion();
            try
            {
                CumplimentarDatosDeUsuarioDeConexion(contexto, contexto.Mapeador, httpContext);
                var gestor = NegociosDeSe.CrearGestor(contexto, tipoDtm, typeof(T));
                var elemento = JsonConvert.DeserializeObject<T>(elementoJson);
                var parametros = new ParametrosDeNegocio(enumTipoOperacion.Insertar);
                parametros.EsUnaPeticion = true;
                parametros.Peticion = enumPeticion.PersistirElemento;
                r.Datos = gestor.PersistirElementoDto(elemento, parametros);
                r.Estado = enumEstadoPeticion.Ok;
                r.Mensaje = "Petición realizada";
                gestor.Commit(tran);
            }
            catch (Exception e)
            {
                contexto.Rollback(tran);
                PrepararError(e, r, "No se ha podido realizar la petición.");
            }

            return new JsonResult(r);
        }

        internal static JsonResult ModificarElemento<T>(ContextoSe contexto, HttpContext httpContext, Type tipoDtm, string elementoJson)
        {
            var r = new Resultado();
            var tran = contexto.IniciarTransaccion();
            try
            {
                CumplimentarDatosDeUsuarioDeConexion(contexto, contexto.Mapeador, httpContext);
                var gestor = NegociosDeSe.CrearGestor(contexto, tipoDtm, typeof(T));
                var elemento = JsonConvert.DeserializeObject<T>(elementoJson);
                var parametros = new ParametrosDeNegocio(enumTipoOperacion.Modificar);
                parametros.EsUnaPeticion = true;
                parametros.Peticion = enumPeticion.PersistirElemento;
                r.Datos = gestor.PersistirElementoDto(elemento, parametros);
                r.Estado = enumEstadoPeticion.Ok;
                r.Mensaje = "Petición realizada";
                gestor.Commit(tran);
            }
            catch (Exception e)
            {
                contexto.Rollback(tran);
                PrepararError(e, r, "No se ha podido realizar la petición.");
            }

            return new JsonResult(r);
        }

        internal static JsonResult BorrarElemento<T>(ContextoSe contexto, HttpContext httpContext, int id, Type tipoDtm)
        {
            var r = new Resultado();
            var tran = contexto.IniciarTransaccion();
            try
            {
                CumplimentarDatosDeUsuarioDeConexion(contexto, contexto.Mapeador, httpContext);
                var gestor = NegociosDeSe.CrearGestor(contexto, tipoDtm, typeof(T));
                var parametrosNeg = new ParametrosDeNegocio(enumTipoOperacion.Eliminar);
                parametrosNeg.EsUnaPeticion = true;
                parametrosNeg.Peticion = enumPeticion.epBorrarPorId;
                r.Datos = gestor.PersistirRegistro(gestor.LeerRegistroPorId(id, aplicarJoin: false), parametrosNeg);
                r.Estado = enumEstadoPeticion.Ok;
                r.Mensaje = "Petición realizada";
                gestor.Commit(tran);
            }
            catch (Exception e)
            {
                contexto.Rollback(tran);
                PrepararError(e, r, "No se ha podido realizar la petición.");
            }

            return new JsonResult(r);
        }

    }

}
