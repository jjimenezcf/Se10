using System;
using System.IO;
using Utilidades;
using Gestor.Errores;
using ImageMagick;
using ModeloDeDto.SistemaDocumental;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.SistemaDocumental;
using System.Linq;
using ServicioDeDatos.Entorno;
using System.Collections.Generic;
using ServicioDeDatos.Seguridad;
using GestorDeElementos.Extensores;
using ServicioDeDatos.Utilidades;
using ProcesadorOcr;
using System.Threading.Tasks;

namespace GestorDeElementos
{
    public static class ApiDeArchivos
    {
        private static readonly string _rutaDeDescarga = enumRutas.RutaDeDescarga;
        private static readonly string _rutaDeImagenes = enumRutas.RutaDeImagenes;
        private static readonly string _rutaDeNails = enumRutas.RutaDeNails;
        private const string _extThumb = "tnl";
        private static readonly string _extSe = enumExtensiones.se.ToString();
        public const string NombreDelFicheroNoEncontrado = "FicheroNoEncontrado.png";
        public static readonly string ExtensionSe = _extSe;

        private const string _extZip = "zip";
        private static List<string> _comprimido = new List<string> { _extZip };

        private static readonly string _noEncontrado = Path.Combine(_rutaDeImagenes, NombreDelFicheroNoEncontrado);
        private static readonly string _bloqueado = Path.Combine(_rutaDeImagenes, "FicheroBloqueado.png");
        private static readonly string _thumbnail = Path.Combine(_rutaDeNails, "tnl.png");
        private static readonly string _pdfnail = Path.Combine(_rutaDeNails, "pdf.png");
        private static readonly string _docnail = Path.Combine(_rutaDeNails, "doc.png");
        private static readonly string _xlsnail = Path.Combine(_rutaDeNails, "xls.png");
        private static readonly string _pptnail = Path.Combine(_rutaDeNails, "ppt.png");
        private static readonly string _txtnail = Path.Combine(_rutaDeNails, "txt.png");
        private static readonly string _zipnail = Path.Combine(_rutaDeNails, "zip.png");

        private static readonly string _dwgnail = Path.Combine(_rutaDeNails, "dwg.png");
        private static readonly string _msgnail = Path.Combine(_rutaDeNails, "msg.png");
        private static readonly string _7znail = Path.Combine(_rutaDeNails, "7z.png");

        public static string FicheroNoEncontrado => _noEncontrado;
        public static string FicheroBloqueado => _bloqueado;

        public static string SolicitarDescargarArchivo(enumNegocio negocio, int idElemento, int idArchivo)
        {
            return $"/{enumControladoresSistemaDocumental.Archivos}/{enumAccionesSistemaDocumental.epDescargarThumsnail}?negocio={negocio}&idElemento={idElemento}&idArchivo={idArchivo}";
        }

        public static string DescargarUrlDeArchivo(int id, string nombreFichero, string almacenadoEn, bool solicitadoPorLaCola)
        {
            var rutaDeDescarga = !solicitadoPorLaCola ? _rutaDeDescarga : CacheDeVariable.Cfg_RutaDeDescarga;
            var archivo = DescargarArchivo(id, nombreFichero, almacenadoEn, rutaDeDescarga, usarCacheado: true, ponerTickAlNombre: true);
            return UrlDeArchivo(archivo);
        }

        public static string UrlDeArchivo(string archivo)
        {
            var rutaUrlBase = _rutaDeDescarga.Replace(@".\wwwroot\", "/"); // --> "/Archivos"
            string urlArchivoRelativa = $@"{rutaUrlBase}/{Path.GetFileName(archivo)}";
            return urlArchivoRelativa;
        }

        public static string DescargarThumbnail(ContextoSe contexto, int idArchivo, string nombreFichero, string almacenadoEn, int ancho, int alto, string mimeType)
        {
            if (!mimeType.Contains("image"))
            {
                var nail = ObtenerThumbnails(mimeType);
                return DevolverFichero(nail, _rutaDeDescarga, nombreFichero, ponerTickAlNombre: true);
            }
            if (!Directory.Exists(_rutaDeDescarga))
            {
                Directory.CreateDirectory(_rutaDeDescarga);
            }
            var thumbnails = $"{idArchivo}.{_extThumb}";
            var cacheado = Path.Combine(_rutaDeDescarga, thumbnails);
            if (!File.Exists(cacheado))
                cacheado = CrearThumbnails(contexto, idArchivo, nombreFichero, almacenadoEn, ancho, alto);

            return DevolverFichero(cacheado, _rutaDeDescarga, nombreFichero, ponerTickAlNombre: true);
        }

        private static string ObtenerThumbnails(string mimeType)
        {
            if (mimeType.Contains("pdf"))
                return _pdfnail;
            if (mimeType.Contains("vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                || mimeType.Contains("ms-excel"))
                return _xlsnail;
            if (mimeType.Contains("openxmlformats-officedocument.wordprocessingml.document"))
                return _docnail;
            if (mimeType.Contains("openxmlformats-officedocument.presentationml.presentation"))
                return _pptnail;
            if (mimeType.Contains("text/plain"))
                return _txtnail;
            if (mimeType.Contains("x-zip-compressed"))
                return _zipnail;
            if (mimeType.Contains("x-7z-compressed"))
                return _7znail;
            if (mimeType.Contains("octet-stream"))
                return _msgnail;

            //

            return _thumbnail;
        }

        public static void ExportarArchivo(ContextoSe contexto, ArchivoDtm archivo, string directorio)
        {
            var sinc = ArchivoSincronizadoSql.Leer(contexto, archivo.Id, errorSiNoHay: false);
            if (sinc != null)
            {
                //Si se ha sincronizado anteriormente en la misma ruta
                if (Path.Combine(directorio, archivo.Nombre).Equals(sinc.Ruta, StringComparison.CurrentCultureIgnoreCase))
                {
                    var fichero = Path.Combine(directorio, archivo.Nombre);

                    //Si es el mismo fichero no lo exporto
                    if (File.Exists(fichero) && sinc.SincronizadoEl == File.GetCreationTime(fichero) && sinc.SincronizadoEl == File.GetLastWriteTime(fichero))
                        return;

                    //Si no existe el fichero en la ruta, y ni es un firmado ni es el original de un firmado es que se ha borrado, por tanto lo elimino del sistema
                    var borrable = contexto.SeleccionarPorPropiedad<FirmadoDtm>(nameof(FirmadoDtm.IdOriginal), archivo.Id.ToString(), errorSiNoHay: false) == null &&
                                   contexto.SeleccionarPorPropiedad<FirmadoDtm>(nameof(FirmadoDtm.IdFirmado), archivo.Id.ToString(), errorSiNoHay: false) == null;

                    if (!borrable)
                    {
                        ArchivoSincronizadoSql.Quitar(contexto, archivo.Id);
                        Respaldar(contexto, archivo, directorio);
                        return;
                    }

                    var gestor = NegociosDeSe.CrearGestor(contexto, typeof(ArchivoDtm), typeof(ArchivoDto));
                    if (!File.Exists(fichero))
                    {
                        gestor.EliminarRegistroPorId(archivo.Id, new Dictionary<string, object>() { { ltrParametrosNeg.CopiaSeguridad, true } });
                        return;
                    }

                }
                //Si se había sincronizado anteriormente, pero en otra ruta, elimino el registro de sincronización para sincronizarlo de nuevo.
                else ArchivoSincronizadoSql.Quitar(contexto, archivo.Id);
            }

            if (!Directory.Exists(directorio))
                CrearDirectorio(contexto, directorio);

            Respaldar(contexto, archivo, directorio);
        }

        public static void Respaldar(ContextoSe contexto, ArchivoDtm archivo, string directorio)
        {
            var sinc = ArchivoSincronizadoSql.Leer(contexto, archivo.Id, errorSiNoHay: false);
            var rutaConFichero = Path.Combine(directorio, archivo.Nombre);
            var copiado = false;
            if (File.Exists($@"{archivo.AlmacenadoEn}\{archivo.Id}.se"))
                try
                {
                    File.Copy($@"{archivo.AlmacenadoEn}\{archivo.Id}.se", rutaConFichero, true);
                    copiado = true;
                    var info = ObtenerInformacionDelFichero(archivo.Id, rutaConFichero);
                    info.Propietario = contexto.DatosDeConexion.Login;

                    if (sinc == null) ArchivoSincronizadoSql.Crear(contexto, info);
                }
                catch (Exception exc)
                {
                    if (copiado) File.Delete(rutaConFichero);
                    contexto.Traza.AnotarExcepcion(exc);
                    throw;
                }
        }

        public static ArchivoSincronizadoDtm ObtenerInformacionDelFichero(int idArchivo, string rutaConFichero)
        {
            var info = new ArchivoSincronizadoDtm();
            info.CreadoEl = File.GetCreationTime(rutaConFichero);
            info.ModificadoEl = File.GetCreationTime(rutaConFichero);
            FileInfo fileInfo = new FileInfo(rutaConFichero);
            //FileSecurity fileSecurity = fileInfo.GetAccessControl();
            //IdentityReference identityReference = fileSecurity.GetOwner(typeof(NTAccount));
            info.Propietario = "";
            info.Longitud = fileInfo.Length;
            info.Ruta = rutaConFichero;
            info.IdArchivo = idArchivo;
            //info.SincronizadoEl = DateTime.UtcNow;
            return info;
        }

        public static bool EstaDescargado(this ArchivoDtm archivo, string ruta) => Path.Exists(Path.Combine(ruta, $"{archivo.Id}.{_extSe}"));

        public static void EliminarDescarga(this ArchivoDtm archivo, string ruta)
        {
            File.Delete(Path.Combine(ruta, $"{archivo.Id}.{_extSe}"));
        }

        public static string ObtenerRutaArchivo(ArchivoDtm archivo)
        {
            var rutaArchivo = ApiDeArchivos.DescargarArchivo(archivo.Id, archivo.Nombre, archivo.AlmacenadoEn, enumRutas.RutaDeDescarga, usarCacheado: true, ponerTickAlNombre: false);
            if (rutaArchivo.Contains(ApiDeArchivos.FicheroNoEncontrado))
            {
                throw new FileNotFoundException($"El archivo '{archivo.Nombre}' no se ha encontrado.");
            }
            return rutaArchivo;
        }

        public static string DescargarArchivo(this ArchivoDtm archivo, ContextoSe contexto, bool solicitadoPorLaCola, bool erroSiNoEstaEnLaruta = false, bool modificarNombreOriginal = true, TrazaSql traza = null)
        {
            var rutaDeDescarga = !solicitadoPorLaCola ? enumRutas.RutaDeDescarga : CacheDeVariable.Cfg_RutaDeDescarga;
            var rutaConFichero = archivo.DescargarArchivo(rutaDeDescarga, usarCacheado: true, ponerTickAlNombre: modificarNombreOriginal, traza);

            if (erroSiNoEstaEnLaruta && rutaConFichero == ApiDeArchivos.FicheroNoEncontrado)
                GestorDeErrores.Emitir($"El fichero {archivo.Nombre} no está en la ruta indicada del gestor documental");

            return rutaConFichero;
        }

        public static string DescargarArchivo(this ArchivoDtm archivo, string rutaDeDescarga, bool usarCacheado = true, bool ponerTickAlNombre = true, TrazaSql traza = null)
        =>
        DescargarArchivo(archivo.Id, archivo.Nombre, archivo.AlmacenadoEn, rutaDeDescarga, usarCacheado, ponerTickAlNombre, traza);

        public static string DescargarArchivo(int id, string nombreFichero, string almacenadoEn, string rutaDeDescarga, bool usarCacheado = true, bool ponerTickAlNombre = true, TrazaSql traza = null)
        {
            rutaDeDescarga = rutaDeDescarga.NormalizeRuta();
            traza?.AnotarMensaje("Ruta normalizada", rutaDeDescarga);
            if (!Directory.Exists(rutaDeDescarga))
                try
                {
                    traza?.AnotarMensaje("Crear directorio De Descarga", rutaDeDescarga);
                    Directory.CreateDirectory(rutaDeDescarga);
                }
                catch (Exception e)
                {
                    GestorDeErrores.Emitir($"Error al crear un directorio {rutaDeDescarga}", e);
                }

            var ficheroCacheado = $"{id}.{_extSe}";
            var rutaFicheroEnGd = Path.Combine(almacenadoEn, ficheroCacheado);
            traza?.AnotarMensaje("ruta del fichero", rutaFicheroEnGd);
            var cacheado = Path.Combine(rutaDeDescarga, ficheroCacheado);

            if (!File.Exists(rutaFicheroEnGd))
                return _noEncontrado;

            if (!usarCacheado && File.Exists(cacheado)) File.Delete(cacheado);


            traza?.AnotarMensaje("ruta donde copiar", cacheado);
            if (!File.Exists(cacheado))
            {
                if (!CopiarFichero(rutaFicheroEnGd, cacheado, traza))
                    return _bloqueado;
            }

            traza?.AnotarMensaje("nombre de fichero", nombreFichero);
            nombreFichero = nombreFichero.ToLower().Replace(enumExtensiones.html.ToString().ToLower(), enumExtensiones.txt.ToString().ToLower());
            return DevolverFichero(cacheado, rutaDeDescarga, nombreFichero, ponerTickAlNombre, traza);
        }

        public static string DescargarArchivo(string ficheroConRutaEnLaGd, string rutaDeDescarga = null, bool incluirTick = true)
        {
            if (!rutaDeDescarga.IsNullOrEmpty() && !Directory.Exists(rutaDeDescarga))
                Directory.CreateDirectory(rutaDeDescarga);

            var ficheroParaDescargar = $@"{(rutaDeDescarga.IsNullOrEmpty() ? _rutaDeDescarga : rutaDeDescarga)}\{(incluirTick ? DateTime.Now.Ticks : "")}{Path.GetFileName(ficheroConRutaEnLaGd)}";

            if (!File.Exists(ficheroConRutaEnLaGd))
                return _noEncontrado;

            if (!File.Exists(ficheroParaDescargar))
            {
                if (!CopiarFichero(ficheroConRutaEnLaGd, ficheroParaDescargar))
                    return _bloqueado;
            }

            if (!CopiarFichero(ficheroConRutaEnLaGd, ficheroParaDescargar))
                return _bloqueado;

            return ficheroParaDescargar;
        }

        private static bool CopiarFichero(string ficheroConRutaOrigen, string ficheroConRutaDestino, TrazaSql traza = null)
        {
            var contadorEspera = 0;
            var copiado = false;

            while (contadorEspera <= 2 && !copiado)
            {
                try
                {
                    traza?.AnotarMensaje("Sentencia de copia", $"File.Copy({ficheroConRutaOrigen}, {ficheroConRutaDestino}, true);");
                    File.Copy(ficheroConRutaOrigen, ficheroConRutaDestino, true);
                    copiado = true;
                }
                catch (Exception e)
                {
                    traza?.AnotarExcepcion(e);
                    contadorEspera += 1;
                    System.Threading.Thread.Sleep(500);
                }
            }
            return copiado;
        }

        public static bool CrearDirectorio(ContextoSe contexto, string ruta)
        {
            if (!Directory.Exists(ruta))
            {
                try
                {
                    Directory.CreateDirectory(ruta);
                }
                catch (Exception exc)
                {
                    contexto.Traza.AnotarExcepcion(exc);
                    throw;
                }
                return true;
            }
            return false;
        }


        public static void EliminarCacheado(ContextoSe contexto, int idArchivo)
        {
            try
            {
                if (File.Exists($"{_rutaDeDescarga}/{idArchivo}.{_extSe}"))
                    File.Delete($"{_rutaDeDescarga}/{idArchivo}.{_extSe}");

                if (File.Exists($"{_rutaDeDescarga}/{idArchivo}.{_extThumb}"))
                    File.Delete($"{_rutaDeDescarga}/{idArchivo}.{_extThumb}");
            }
            catch (Exception exc)
            {
                contexto.Traza.AnotarExcepcion(exc);
            }
        }

        private static string PdfToImagen(ContextoSe contexto, string rutaConFichero, string almacenadoEn, int idArchivo)
        {
            var ficheroPng = $@"{almacenadoEn}\{idArchivo}.png";
            try
            {
                var settings = new MagickReadSettings();
                // Settings the density to 300 dpi will create an image with a better quality
                settings.Density = new Density(300, 300);
                using (var images = new MagickImageCollection())
                {
                    // Add all the pages of the pdf file to the collection
                    images.Read($@"{rutaConFichero}.pdf", settings);
                    images[0].Write(ficheroPng);

                    /*
                    var page = 1;
                    foreach (var image in images)
                    {
                        images[0].Format = MagickFormat.Png;
                        images[0].Write($@"{archivo.AlmacenadoEn}\{pdf}.png");
                        images[0].Format = MagickFormat.Ptif;
                        images[0].Write($@"{archivo.AlmacenadoEn}\{pdf}.tif");
                        page++;
                    }
                    */
                }
            }
            catch (Exception e)
            {
                contexto.Traza.AnotarExcepcion(e);
            }
            return ficheroPng;
        }

        private static string CrearThumbnails(ContextoSe contexto, int idArchivo, string nombreFichero, string almacenadoEn, int ancho, int alto)
        {
            //var rutaActual = Directory.GetCurrentDirectory();

            var rutaFicheroEnGd = Path.Combine(almacenadoEn, $"{idArchivo}.se");
            if (!File.Exists(rutaFicheroEnGd))
            {
                contexto.AnotarTraza($"El fichero {nombreFichero} no se ha localizado en la gestión documental",
                    $"El archivo con ID: {idArchivo} no está en la ruta {almacenadoEn}");
            }

            var fichero = !File.Exists(rutaFicheroEnGd)
            ? _noEncontrado
            : Path.Combine(_rutaDeDescarga, $"{idArchivo}.{_extThumb}");

            if (File.Exists(rutaFicheroEnGd)) try
                {
                    using (var image = new MagickImage(rutaFicheroEnGd))
                    {
                        var size = new MagickGeometry((uint)(ancho == 0 ? 64 : ancho), (uint)(alto == 0 ? 64 : alto));
                        size.IgnoreAspectRatio = true;
                        image.Resize(size);
                        image.Write(fichero);
                    }
                }
                catch (Exception e)
                {
                    contexto.Traza.AnotarExcepcion(e);
                    fichero = _thumbnail;
                }

            return fichero;

        }

        public static string Extension(string nombre)
        {
            return Path.GetExtension(nombre);
        }

        public static bool EsUnComprimido(string nombre)
        {
            return _comprimido.Contains(Path.GetExtension(nombre).ToLower().Replace(".", ""));
        }

        private static string DevolverFichero(string cacheado, string rutaDeDescarga, string nombreFichero, bool ponerTickAlNombre, TrazaSql traza = null)
        {
            if (cacheado.Equals(_thumbnail))
                return cacheado;

            if (!Directory.Exists(rutaDeDescarga))
                Directory.CreateDirectory(rutaDeDescarga);

            nombreFichero = Path.GetFileNameWithoutExtension(nombreFichero).NormalizarFichero() + Path.GetExtension(nombreFichero);


            var ficherpParaDevolverConRuta = Path.Combine(rutaDeDescarga, $"{Path.GetFileNameWithoutExtension(nombreFichero)}{Path.GetExtension(nombreFichero)}");
            traza?.AnotarMensaje("nombre de fichero a devolver", ficherpParaDevolverConRuta);

            if (!ponerTickAlNombre)
            {
                if (Path.Exists(ficherpParaDevolverConRuta))
                    return ficherpParaDevolverConRuta;
                else if (!CopiarFichero(cacheado, ficherpParaDevolverConRuta, traza))
                    return _bloqueado;

                return ficherpParaDevolverConRuta;
            }

            ficherpParaDevolverConRuta = Path.Combine(rutaDeDescarga, $"{Path.GetFileNameWithoutExtension(nombreFichero)}_{DateTime.Now.Ticks}{Path.GetExtension(nombreFichero)}");

            if (!CopiarFichero(cacheado, ficherpParaDevolverConRuta))
                return _bloqueado;

            return ficherpParaDevolverConRuta;
        }

        public static void ValidarQueEstaAnexado(ContextoSe contexto, enumNegocio negocio, int idElemento, int idArchivo)
        {
            var vinculo = VinculoSql.LeerVinculo(contexto, NegociosDeSe.TipoDtm(negocio), enumNegocio.Archivos, idElemento, idArchivo, false);
            if (vinculo == null)
                GestorDeErrores.Emitir($"El archivo no está anexado al elemento del negocio {negocio.ToNombre()}");
        }

        public static void ValidarQueNoEstaFirmado(ContextoSe contexto, int idArchivo)
        {
            var firmado = contexto.Set<FirmadoDtm>().Where(x => x.IdFirmado == idArchivo).FirstOrDefault();
            if (firmado != null)
            {
                var original = contexto.Set<ArchivoDtm>().Where(y => y.Id == firmado.IdOriginal).First();
                GestorDeErrores.Emitir($"El archivo que intenta firmar, es un archivo firmado por el usuario {contexto.SeleccionarPorId<UsuarioDtm>(firmado.IdUsuario).Expresion}");
            }

        }

        public static FirmadoDtm ValidarQueEstaFirmadoPorElUsuarioConectado(ContextoSe contexto, int idArchivo)
        {
            var firmado = contexto.Set<FirmadoDtm>().Where(x => x.IdFirmado == idArchivo).FirstOrDefault();
            if (firmado == null)
            {
                var original = contexto.Set<ArchivoDtm>().Where(y => y.Id == firmado.IdOriginal).First();
                GestorDeErrores.Emitir($"El archivo {original.Nombre} no está firmado");
            }

            if (firmado.IdUsuario != contexto.DatosDeConexion.IdUsuario)
            {
                GestorDeErrores.Emitir($"El archivo no puede ser eliminado ya que no se firmo por ud's");
            }

            ApiDeCertificados.ValidarUsoDelCertificado(contexto, firmado.IdCertificado);

            return firmado;

        }

        public static List<ArchivoDtm> LeerAnexados<T>(this T elemento, ContextoSe contexto)
        where T : IRegistro
        {
            var vinculos = VinculoSql.LeerVinculosCon(contexto, elemento.GetType(), enumNegocio.Archivos, ApiDeRegistroDtm.EsquemaTabla(typeof(ArchivoDtm)), elemento.Id);

            if (vinculos.Count == 0)
                return new List<ArchivoDtm>();

            var idsDeArchivos = string.Join(",", vinculos.Select(v => v.idElemento2));

            var filtros = new List<ClausulaDeFiltrado>();
            var filtro = new ClausulaDeFiltrado { Clausula = nameof(ArchivoDtm.Id), Criterio = enumCriteriosDeFiltrado.esAlgunoDe, Valor = idsDeArchivos };
            filtros.Add(filtro);

            var anexados = ((List<ArchivoDtm>)contexto.CrearGestorDeUnDtm<ArchivoDtm>().LeerRegistros(0, -1, filtros, false));

            return anexados;
        }


        public static ArchivoDtm QuitarAnexado(this IRegistro registro, ContextoSe contexto, int idArchivo, bool validarPersistencia = true, bool QuitarDeRestoDeAnexados = true)
        {
            var negocio = NegociosDeSe.ToEnumerado(NegociosDeSe.LeerNegocioPorDtm(registro.GetType().FullName));
            var archivo = contexto.SeleccionarPorId<ArchivoDtm>(idArchivo);
            var gestor = negocio == enumNegocio.Carpeta ? enumNegocio.Archivador.CrearGestor(contexto) : negocio.CrearGestor(contexto);
            var objetoPadre = negocio == enumNegocio.Carpeta ? contexto.SeleccionarPorId<CarpetaDtm>(registro.Id) : registro;
            ArchivadorDtm archivador = null;
            if (negocio == enumNegocio.Carpeta)
            {
                if (validarPersistencia)
                {
                    var modo = ApiDePermisos.LeerModoDeAcceso(contexto, enumNegocio.Archivador, ((CarpetaDtm)objetoPadre).IdArchivador);
                    if (modo == enumModoDeAccesoDeDatos.Consultor)
                        GestorDeErrores.Emitir($"El archivador de la carpeta '{((INombre)objetoPadre).Nombre}' no es editables");

                    archivador = contexto.SeleccionarPorId<ArchivadorDtm>(((CarpetaDtm)objetoPadre).IdArchivador);
                    var resultado = archivador.DegradarPermisosDeGestor(contexto);
                    if (resultado.Degradado)
                        GestorDeErrores.Emitir(resultado.Mensaje);
                }
            }
            else
            {
                if (validarPersistencia) gestor.ValidarPermisosDePersistencia(enumTipoOperacion.Eliminar, gestor.Negocio, registro.Id);
                ValidarQueEstaAnexado(contexto, gestor.Negocio, registro.Id, idArchivo);
            }

            if (archivo.IdUsuaCrea != contexto.DatosDeConexion.IdUsuario)
            {
                bool esAdministrador = negocio == enumNegocio.Carpeta ? archivador.EsAdministrador(contexto) : ((IElementoDtm)registro).EsAdministrador(contexto);

                var puedeBorrar = NegociosDeSe.NegocioDeUnDtm(registro.GetType()).UsaArchivos() && esAdministrador;
                if (!puedeBorrar)
                {
                    var owner = contexto.SeleccionarPorId<UsuarioDtm>(archivo.IdUsuaCrea);
                    GestorDeErrores.Emitir($"El archivo '{archivo.Nombre}' no lo adjuntó Ud., sólo lo puede quitar el propietario '{owner.Login}' o un administrador del archivador");
                }
            }

            try
            {
                AntesDeQuitarVinculo(gestor, objetoPadre, archivo, new Dictionary<string, object> { { ltrDeUnArchivo.QuitarTodosLosAnexados, QuitarDeRestoDeAnexados } });
                VinculoSql.QuitarVinculo(contexto, objetoPadre.GetType(), enumNegocio.Archivos, objetoPadre.Id, archivo.Id);
                DespuesDeQuitarVinculo(gestor, objetoPadre, archivo, new Dictionary<string, object>());
                if (archivo.NumeroDeReferencias(contexto, excluirAuditoria: true) == 0)
                    archivo.Eliminar(contexto, new Dictionary<string, object> { { ltrParametrosNeg.ValidarPermisosDePersistencia, validarPersistencia } });
            }
            finally
            {
                VinculoSql.BlanquearCacheDeAnexados(contexto, negocio.TipoDtm(), registro.Id);
                EliminarCacheado(contexto, idArchivo);
            }
            return archivo;
        }

        public static void Renombrar(this ArchivoDtm archivo, ContextoSe contexto, string nuevo)
        {
            archivo.Nombre = nuevo;
            archivo.Modificar(contexto, enumAccionesSistemaDocumental.RenombrarArchivo);
        }

        private static void AntesDeQuitarVinculo(IGestor gestor, IRegistro elemento, ArchivoDtm archivo, Dictionary<string, object> parametros)
        {
            var metodo = ApiDeEnsamblados.MetodoEstatico(ApiDeEnsamblados.GestoresDeNegocio, gestor.GetType().FullName, ApiDeEnsamblados.AntesDeQuitarVinculo);
            if (metodo != null)
            {
                var negocio = elemento.GetType().NegocioDeUnDtm();
                var entorno = new EntornoDeUnaAccion(gestor.Contexto, elemento, negocio, parametros);
                entorno.Contexto.Accion = new AccionDtm { Dll = ApiDeEnsamblados.GestoresDeNegocio, Clase = gestor.GetType().FullName, Metodo = ApiDeEnsamblados.AntesDeQuitarVinculo, Nombre = "Antes de quitar un vínculo", ClaseDeAccion = enumClaseDeAccion.DLL.ToString() };
                entorno.AsignarParametros(new Dictionary<string, object> {
                        {nameof(ltrParametrosNeg.Negocio), negocio},
                        {nameof(ltrParametrosNeg.Vinculado), enumNegocio.Archivos},
                        {nameof(ltrParametrosNeg.IdElemento), elemento.Id},
                        {nameof(ltrParametrosNeg.IdVinculado), archivo.Id}
                    });
                try
                {
                    EntornoDeUnaAccion.EjecutarAccion(entorno);
                }
                finally
                {
                    entorno.Contexto.Accion = null;
                }
            }

            metodo = ApiDeEnsamblados.MetodoEstatico(ApiDeEnsamblados.GestoresDeNegocio, ApiDeEnsamblados.Clase_GestorDeArchivos, ApiDeEnsamblados.AntesDeQuitarVinculo);
            if (metodo != null)
            {
                var entorno = new EntornoDeUnaAccion(gestor.Contexto, archivo, enumNegocio.Archivos, parametros);
                entorno.Contexto.Accion = new AccionDtm { Dll = ApiDeEnsamblados.GestoresDeNegocio, Clase = ApiDeEnsamblados.Clase_GestorDeArchivos, Metodo = ApiDeEnsamblados.AntesDeQuitarVinculo, Nombre = "Antes de quitar un vínculo", ClaseDeAccion = enumClaseDeAccion.DLL.ToString() };
                entorno.AsignarParametros(new Dictionary<string, object> {
                        {nameof(ltrParametrosNeg.Negocio), enumNegocio.Archivos},
                        {nameof(ltrParametrosNeg.Vinculado), elemento.GetType().NegocioDeUnDtm()},
                        {nameof(ltrParametrosNeg.IdElemento), archivo.Id},
                        {nameof(ltrParametrosNeg.IdVinculado), elemento.Id}
                    });
                try
                {
                    EntornoDeUnaAccion.EjecutarAccion(entorno);
                }
                finally
                {
                    entorno.Contexto.Accion = null;
                }
            }

        }
        private static void DespuesDeQuitarVinculo(IGestor gestor, IRegistro elemento, ArchivoDtm archivo, Dictionary<string, object> parametros)
        {
            var metodo = ApiDeEnsamblados.MetodoEstatico(ApiDeEnsamblados.GestoresDeNegocio, gestor.GetType().FullName, ApiDeEnsamblados.DespuesDeQuitarVinculo);
            if (metodo != null)
            {
                var negocio = elemento.GetType().NegocioDeUnDtm();
                var entorno = new EntornoDeUnaAccion(gestor.Contexto, elemento, negocio, parametros);
                entorno.Contexto.Accion = new AccionDtm { Dll = ApiDeEnsamblados.GestoresDeNegocio, Clase = gestor.GetType().FullName, Metodo = ApiDeEnsamblados.DespuesDeQuitarVinculo, Nombre = "Despues de quitar un vínculo", ClaseDeAccion = enumClaseDeAccion.DLL.ToString() };
                entorno.AsignarParametros(new Dictionary<string, object> {
                        {nameof(ltrParametrosNeg.Negocio), negocio},
                        {nameof(ltrParametrosNeg.Vinculado), enumNegocio.Archivos},
                        {nameof(ltrParametrosNeg.IdElemento), elemento.Id},
                        {nameof(ltrParametrosNeg.IdVinculado), archivo.Id}
                    });
                try
                {
                    EntornoDeUnaAccion.EjecutarAccion(entorno);
                }
                finally
                {
                    entorno.Contexto.Accion = null;
                }
            }

            metodo = ApiDeEnsamblados.MetodoEstatico(ApiDeEnsamblados.GestoresDeNegocio, ApiDeEnsamblados.Clase_GestorDeArchivos, ApiDeEnsamblados.DespuesDeQuitarVinculo);
            if (metodo != null)
            {
                var entorno = new EntornoDeUnaAccion(gestor.Contexto, archivo, enumNegocio.Archivos, parametros);
                entorno.Contexto.Accion = new AccionDtm { Dll = ApiDeEnsamblados.GestoresDeNegocio, Clase = ApiDeEnsamblados.Clase_GestorDeArchivos, Metodo = ApiDeEnsamblados.DespuesDeQuitarVinculo, Nombre = "Despues de quitar un vínculo", ClaseDeAccion = enumClaseDeAccion.DLL.ToString() };
                entorno.AsignarParametros(new Dictionary<string, object> {
                        {nameof(ltrParametrosNeg.Negocio), enumNegocio.Archivos},
                        {nameof(ltrParametrosNeg.Vinculado), elemento.GetType().NegocioDeUnDtm()},
                        {nameof(ltrParametrosNeg.IdElemento), archivo.Id},
                        {nameof(ltrParametrosNeg.IdVinculado), elemento.Id}
                    });
                try
                {
                    EntornoDeUnaAccion.EjecutarAccion(entorno);
                }
                finally
                {
                    entorno.Contexto.Accion = null;
                }
            }

            if (elemento.GetType() != typeof(CarpetaDtm) && gestor.Negocio.UsaTrazas())
            {
                ((IUsaTraza)elemento).CrearTraza(gestor.Contexto, "Archivo eliminado", $"El usuario '{gestor.Contexto.DatosDeConexion.Login}' ha eliminado el archivo '{archivo.Nombre}'");
            }
        }

        public static bool ValidarNombre(string nombreArchivo)
        {
            // Verificar si el nombre está vacío o es nulo
            if (nombreArchivo.IsNullOrEmpty())
            {
                GestorDeErrores.Emitir($"Debe indicar un nombrede archivo");
            }

            // Verificar si contiene caracteres inválidos para nombres de archivo
            if (nombreArchivo.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                GestorDeErrores.Emitir($"El nombre del archivo '{nombreArchivo}' contiene caracteres no válidos");
            }

            // Verificar si el nombre contiene una ruta
            if (nombreArchivo.IndexOfAny(new char[] { '\\', '/' }) >= 0)
            {
                GestorDeErrores.Emitir($"El nombre del archivo '{nombreArchivo}' no puede contener rutas");
            }

            // Verificar si el nombre es igual a algún nombre reservado de Windows
            string nombreSinExtension = Path.GetFileNameWithoutExtension(nombreArchivo);
            string[] nombresReservados = { "CON", "PRN", "AUX", "NUL",
                                           "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
                                           "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9" };

            if (Array.IndexOf(nombresReservados, nombreSinExtension.ToUpper()) >= 0)
            {
                GestorDeErrores.Emitir($"El nombre del archivo '{nombreArchivo}' es reservado por el S.O.");
            }

            // Si pasa todas las verificaciones, el nombre es válido
            return true;
        }

        public static string ObtenerNombreUnico(string rutaArchivo)
        {
            string directorio = Path.GetDirectoryName(rutaArchivo);
            string extension = Path.GetExtension(rutaArchivo);
            int contador = 1;

            while (File.Exists(rutaArchivo))
            {
                string nombreArchivo = Path.GetFileNameWithoutExtension(rutaArchivo);
                string nuevoNombre = $"{IncrementarNombreArchivo(nombreArchivo)}{extension}";
                rutaArchivo = Path.Combine(directorio, nuevoNombre);
                contador++;
            }

            return rutaArchivo;
        }

        private static string IncrementarNombreArchivo(string nombreArchivo)
        {
            // Encuentra la última ocurrencia de '_'
            int ultimoGuionBajo = nombreArchivo.LastIndexOf('_');

            // Si no hay '_' o es el último carácter, devuelve el nombre original
            if (ultimoGuionBajo == -1 || ultimoGuionBajo == nombreArchivo.Length - 1)
                return nombreArchivo + "_1";

            // Extrae la parte después del último '_'
            string ultimaParte = nombreArchivo.Substring(ultimoGuionBajo + 1);

            // Intenta convertir la última parte a un entero
            if (int.TryParse(ultimaParte, out int numero))
            {
                // Si es un número, incrementa y reemplaza
                return $"{nombreArchivo.Substring(0, ultimoGuionBajo)}_{numero + 1}";
            }

            // Si no es un número, devuelve el nombre original
            return nombreArchivo + "_1";
        }

        public static string ProponerNombreDeArchivo<T>(this T elemento, ContextoSe contexto, string nombreBase)
        where T : IElementoDtm
        =>
        ProponerNombreDeArchivo(elemento.LeerAnexados(contexto), nombreBase.NormalizarFichero());


        public static string ProponerNombreDeArchivo(List<ArchivoDtm> anexados, string nombreArchivo)
        {
            var extension = Path.GetExtension(nombreArchivo);
            var contador = 0;

            while (anexados.Any(archivo => archivo.Nombre == nombreArchivo))
            {
                contador++;
                nombreArchivo = $"{Path.GetFileNameWithoutExtension(nombreArchivo.Replace($"_{contador - 1}", ""))}_{contador}{extension}";
            }

            return nombreArchivo;
        }

        public static ArchivoDtm Archivo<T>(this T elemento, ContextoSe contexto, bool aplicarJoin = false, bool errorSiNoHay = true)
            where T : IUsaArchivo
        {
            if (elemento.Archivo != null) return elemento.Archivo;

            if (elemento.IdArchivo is null && errorSiNoHay)
                GestorDeErrores.Emitir($"Elemento '{(typeof(T).ImplementaUsaReferencia() ? ((IUsaReferencia)elemento).Referencia : ((INombre)elemento).Nombre)}' no tiene archivo asociado");

            if (elemento.IdArchivo is null) return null;

            elemento.Archivo = contexto.SeleccionarPorId<ArchivoDtm>((int)elemento.IdArchivo, aplicarJoin);

            var fichero = $@"{elemento.Archivo.AlmacenadoEn}\{elemento.IdArchivo}.se";
            if (!File.Exists(fichero))
                GestorDeErrores.Emitir($"Elemento '{(typeof(T).ImplementaUsaReferencia() ? ((IUsaReferencia)elemento).Referencia : ((INombre)elemento).Nombre)}' no se ha localizado en la ruta '{elemento.Archivo.AlmacenadoEn}'");

            return elemento.Archivo;
        }

        public static bool EstaElFicheroBloqueado(string filePath)
        {
            try
            {
                using (FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {
                return true;
            }

            return false;
        }

        public static ArchivoDtm SubirArchivoInterno(ContextoSe contexto, string rutaConFichero, bool sincronizar = false, string nombreFicheroParaAlmacenar = null, bool copiar = false, bool quitarExtensionHtml = true, bool sanitizar = true)
        {
            var rutaServidorDeArchivos = CacheDeVariable.Cfg_ServidorDeArchivos;

            if (!Directory.Exists(rutaServidorDeArchivos))
                CrearDirectorio(contexto, rutaServidorDeArchivos);

            // Obtener la extensión del archivo
            string extension = Path.GetExtension(rutaConFichero).ToLower();

            if (extension.IsNullOrEmpty())
                extension = enumExtensiones.txt.ToString();

            // Si la extensión es .html, cambiarla a .txt
            if (quitarExtensionHtml && ExtensorDeTipoDeArchivos.EsHtml(extension))
            {
                string directorio = Path.GetDirectoryName(rutaConFichero);
                string nombreSinExtension = Path.GetFileNameWithoutExtension(rutaConFichero);
                string nuevaRuta = Path.Combine(directorio, nombreSinExtension + ".txt");

                // Renombrar el archivo en el disco
                if (File.Exists(rutaConFichero))
                {
                    rutaConFichero = MoverArchivo(rutaConFichero, nuevaRuta, sobreescribir: true, copiarSiBloqueado: true);
                }
            }

            var fecha = DateTime.Now;
            var almacenarEn = Path.Combine(new string[] { rutaServidorDeArchivos, fecha.Year.ToString(), fecha.Month.ToString(), fecha.Day.ToString(), fecha.Hour.ToString(), contexto.DatosDeConexion.IdUsuario.ToString() });
            Directory.CreateDirectory(almacenarEn);
            var fichero = nombreFicheroParaAlmacenar.IsNullOrEmpty() ? Path.GetFileName(rutaConFichero) : nombreFicheroParaAlmacenar;
            var archivo = new ArchivoDtm { Nombre = fichero, AlmacenadoEn = almacenarEn };
            if (sanitizar) rutaConFichero = SanitizeFile(rutaConFichero);
            var tran = contexto.IniciarTransaccion();
            try
            {
                archivo.Insertar(contexto, new Dictionary<string, object> { { ltrDeUnArchivo.Sincronizar, sincronizar }, { ltrDeUnArchivo.rutaDelArchivo, rutaConFichero } });
                var rutaDestino = Path.Combine(archivo.AlmacenadoEn, $"{archivo.Id}.{enumExtensiones.se}");
                if (sincronizar || copiar)
                    File.Copy(rutaConFichero, rutaDestino, true);
                else
                    MoverArchivo(rutaConFichero, rutaDestino, sobreescribir: true, copiarSiBloqueado: true);
                contexto.Commit(tran);
                return archivo;
            }
            catch (Exception)
            {
                contexto.Rollback(tran);
                throw;
            }
            finally
            {
                if (!sincronizar && !copiar)
                {
                    if (File.Exists(rutaConFichero))
                        File.Delete(rutaConFichero);
                }
            }
        }

        private static string SanitizeFile(string rutaConFichero)
        {
            if (extHtml.EsHtml(rutaConFichero))
            {
                var rutaSanitizada = extHtml.SanitizeFile(rutaConFichero);
                return rutaSanitizada;
            }

            if (extPdf.EsPdf(rutaConFichero))
            {
                var rutaSanitizada = extPdf.SanitizePdf(rutaConFichero);
                return rutaSanitizada;
            }

            if (extDocx.EsDoc(rutaConFichero))
            {
                GestorDeErrores.Emitir($"El archivo que está intentando subir es un Doc, formato antiguo, reconviertalo a docx o a pdf antes de almacenarlo");
            }

            if (extDocx.EsDocx(rutaConFichero))
            {
                var rutaSanitizada = extDocx.SanitizeDocx(rutaConFichero);
                return rutaSanitizada;
            }

            if (extImagenes.EsImagen(rutaConFichero))
            {
                var rutaSanitizada = extImagenes.SanitizeImage(rutaConFichero);
                return rutaSanitizada;
            }

            return rutaConFichero;
        }

        public static string MoverArchivo(string rutaConFichero, string rutaActualizada, bool sobreescribir = true, bool copiarSiBloqueado = true)
        {
            try
            {
                File.Move(rutaConFichero, rutaActualizada, sobreescribir);
            }
            catch (IOException)
            {
                if (!copiarSiBloqueado)
                    throw;
                try
                {
                    // Si no se puede mover, intenta copiar
                    File.Copy(rutaConFichero, rutaActualizada, sobreescribir);
                }
                catch (Exception)
                {
                    // Si la copia también falla, lanza una excepción
                    throw new IOException($"No se pudo mover ni copiar el archivo: {rutaConFichero}");
                }
            }
            return rutaActualizada;
        }

        public static string ExtraerTextoPlano(string rutaArchivo)
        {
            var extension = ApiDeEnsamblados.ToEnumerado<enumExtensiones>(Path.GetExtension(rutaArchivo).Replace(".", ""), errorSiNoEsValido: false);
            if (extension == null)
            {
                throw new Exception($"La extensión '{Path.GetExtension(rutaArchivo)}' no es válida.");
            }
            var contenido = "";
            switch (extension)
            {
                case enumExtensiones.pdf:
                    contenido = Task.Run(() => new ProcesadorOcr.ProcesadorOcr().ProcesarFichero(idArchivo: 0, rutaArchivo)).Result;
                    break;
                case enumExtensiones.docx:
                    contenido = extDocx.ToTexto(rutaArchivo);
                    break;
                case enumExtensiones.rtf:
                    contenido = extRtf.ToTexto(rutaArchivo);
                    break;
                case enumExtensiones.html:
                    contenido = extHtml.ToTexto(rutaArchivo);
                    break;
                case enumExtensiones.txt:
                    contenido = File.ReadAllText(rutaArchivo);
                    break;
                default:
                    if (extTexto.EsTexto(rutaArchivo))
                    {
                        contenido = File.ReadAllText(rutaArchivo);
                        break;
                    }
                    throw new Exception($"La extensión '{extension}' del archivo a resumir no es válida");
            }

            return contenido;
        }

        public static string RegistrarDescargaConGuid(this ArchivoDtm archivo, ContextoSe contexto, DateTime? caducaEl, int? maximoDeDescargas, bool auditar = true)
        {
            var guid = Guid.NewGuid();
            var creadoEl = DateTime.Now;
            if (caducaEl.HasValue && (DateTime)caducaEl <= creadoEl)
            {
                GestorDeErrores.Emitir($"No puede registrar una descarga con fecha de caducida '{caducaEl.Fecha().ToString("yyy-MM-dd HH:mm")}', es anterior al momento actual");
            }

            if (caducaEl is null && maximoDeDescargas is null)
            {
                caducaEl = creadoEl.AddHours(1);
            }

            var registrar = new DescargaConGuidDtm
            {
                IdArchivo = archivo.Id,
                Guid = guid,
                IdUsuario = contexto.Usuario.Id,
                CreadoEl = creadoEl,
                CaducaEl = caducaEl,
                MaximoDescargas = maximoDeDescargas
            }.InsertarComoAdministrador(contexto);

            if (auditar) new AuditoriaDeUnArchivoDtm
            {
                IdArchivo = archivo.Id,
                Auditoria = ltrDeAuditoriaDeArchivo.DescargaConGuid.Replace("[0]", contexto.Usuario.Login).Replace("[1]", guid.ToString())
            }.InsertarComoAdministrador(contexto);

            return guid.ToString();
        }
    }
}
