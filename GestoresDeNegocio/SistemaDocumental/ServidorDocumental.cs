using System.IO;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos;
using System;
using GestorDeElementos;
using GestoresDeNegocio.Entorno;
using System.Collections.Generic;
using ServicioDeDatos.Elemento;
using ModeloDeDto;
using Utilidades;
using Gestor.Errores;
using System.Reflection;
using GestoresDeNegocio.TrabajosSometidos;
using Newtonsoft.Json;
using ServicioDeDatos.TrabajosSometidos;
using ModeloDeDto.SistemaDocumental;
using System.Linq;
using ServicioDeDatos.Seguridad;
using GestorDeElementos.Extensores;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Negocio;
using GestoresDeNegocio.Seguridad;
using System.IO.Compression;
using Newtonsoft.Json.Linq;

namespace GestoresDeNegocio.SistemaDocumental
{
    public enum enumOperacionesConArchivos { Copiar, Mover, Enlazar }

    public static class ServidorDocumental //: GestorDeElementos<ContextoSe, ArchivoDtm, ArchivoDto>
    {

        public static string NuevoArchivo(string nombre, string ruta = null)
        {
            if (ruta.IsNullOrEmpty()) ruta = GestorDeVariables.RutaDeDescarga;
            if (!Directory.Exists(ruta))
                Directory.CreateDirectory(ruta);
            var rutaConFichero = $@"{ruta}\{nombre}";
            //if (File.Exists(Path.Combine(ruta, nombre)))
            //    throw new Exception($"El {rutaConFichero} fichero ya existe");
            var file = File.Create(rutaConFichero);
            file.Close();
            return rutaConFichero;
        }
        public static string SalvarFichero(string rutaArchivo, string contenido, bool sanitizar = true)
        {
            bool intentar = true;
            var veces = 0;
            while (intentar && veces < 20)
            {
                rutaArchivo = ApiDeArchivos.ObtenerNombreUnico(rutaArchivo);
                try
                {
                    using (StreamWriter writer = new StreamWriter(rutaArchivo))
                    {
                        writer.Write(sanitizar ? extHtml.SanitizeContenido(contenido) : contenido);
                    }
                    intentar = false;
                }
                catch (IOException e)
                {
                    if (!e.Message.Contains("because it is being used by another process"))
                        throw;
                    veces++;
                }
            }

            if (veces >= 20)
            {
                throw new IOException($"No se pudo guardar el archivo después de {veces} intentos: {rutaArchivo}");
            }

            return rutaArchivo;
        }


        public static int SubirArchivo(this ContextoSe contexto, string rutaConFichero, bool sanitizar = true)
        {
            var archivo = ApiDeArchivos.SubirArchivoInterno(contexto, rutaConFichero, sincronizar: false, nombreFicheroParaAlmacenar: null, copiar: false, sanitizar: sanitizar);
            return archivo.Id;
        }

        public static int SubirArchivo(this ContextoSe contexto, Stream stream, string ruta)
        {
            using (var fileStream = File.Create(ruta))
            {
                stream.Seek(0, SeekOrigin.Begin);
                stream.CopyTo(fileStream);
                stream.Flush();
                fileStream.Close();
            }
            return SubirArchivo(contexto, ruta);
        }

        internal static ArchivoDto ObtenerArchivoSiEstaAnexado(List<ArchivoDto> anexados, string rutaConFichero)
        {
            var nombreFichero = Path.GetFileName(rutaConFichero);
            foreach (var anexado in anexados)
            {
                if (nombreFichero.Equals(anexado.Nombre))
                    return anexado;
            }
            return null;
        }

        public static ArchivoDtm AnexarArchivo(ContextoSe contexto, enumNegocio negocio, int idElemento, string rutaConFichero, bool sanitizar)
        =>
        AnexarArchivo(contexto, negocio, idElemento, rutaConFichero, copiar: false, quitarExtensionHtml:true, sanitizar: sanitizar);

        public static ArchivoDtm CopiarArchivo(ContextoSe contexto, enumNegocio negocio, int idElemento, string rutaConFichero, bool sanitizar)
        {
            ValidarPermisos(contexto, negocio, idElemento);
            var archivo = ApiDeArchivos.SubirArchivoInterno(contexto, rutaConFichero, sincronizar: false, nombreFicheroParaAlmacenar: null, copiar: true, quitarExtensionHtml: true, sanitizar: sanitizar);
            GestorDeVinculos.Vincular(contexto, negocio, enumNegocio.Archivos, idElemento, archivo.Id);
            if (negocio == enumNegocio.Archivador) GestorDeArchivadores.ExportarArchivo(contexto, idArchivador: idElemento, archivo);
            if (negocio == enumNegocio.Carpeta) GestorDeCarpetas.ExportarArchivo(contexto, idCarpeta: idElemento, archivo);
            return archivo;
        }

        public static ArchivoDtm AnexarArchivo(ContextoSe contexto, enumNegocio negocio, int idElemento, string rutaConFichero, bool copiar, bool quitarExtensionHtml , bool sanitizar )
        {
            ValidarPermisos(contexto, negocio, idElemento);

            var nombreOriginal = Path.GetFileName(rutaConFichero);
            var nombreActualizado = ProponerNombreDeArchivo(contexto, negocio, idElemento, nombreOriginal);
            if (nombreOriginal != nombreActualizado)
            {
                var rutaActualizada = Path.Combine(Path.GetDirectoryName(rutaConFichero), nombreActualizado);
                ApiDeArchivos.MoverArchivo(rutaConFichero, rutaActualizada, sobreescribir: true, copiarSiBloqueado: true);
                rutaConFichero = rutaActualizada;
            }
            var archivo = ApiDeArchivos.SubirArchivoInterno(contexto, rutaConFichero, sincronizar: false, nombreFicheroParaAlmacenar: null, copiar, quitarExtensionHtml, sanitizar: sanitizar);
            GestorDeVinculos.Vincular(contexto, negocio, enumNegocio.Archivos, idElemento, archivo.Id);
            if (negocio == enumNegocio.Archivador) GestorDeArchivadores.ExportarArchivo(contexto, idArchivador: idElemento, archivo);
            if (negocio == enumNegocio.Carpeta) GestorDeCarpetas.ExportarArchivo(contexto, idCarpeta: idElemento, archivo);
            return archivo;
        }

        private static void ValidarPermisos(ContextoSe contexto, enumNegocio negocio, int idElemento)
        {
            if (negocio == enumNegocio.Carpeta)
            {
                var carpeta = contexto.SeleccionarPorId<CarpetaDtm>(idElemento);
                var modo = ApiDePermisos.LeerModoDeAcceso(contexto, enumNegocio.Archivador, carpeta.IdArchivador);
                if (modo == enumModoDeAccesoDeDatos.Consultor)
                    GestorDeErrores.Emitir($"El archivador de la carpeta '{carpeta.Nombre}' no es editables");

                var archivador = contexto.SeleccionarPorId<ArchivadorDtm>(carpeta.IdArchivador);
                var resultado = archivador.DegradarPermisosDeGestor(contexto);
                if (resultado.Degradado)
                    GestorDeErrores.Emitir(resultado.Mensaje);
            }
            else
            {
                var gestor = negocio.CrearGestor(contexto);
                gestor.ValidarPermisosDePersistencia(enumTipoOperacion.Modificar, gestor.Negocio, idElemento, new Dictionary<string, object>() { { ltrParametrosNeg.ValidarEtapaDocumental, true } });
            }
        }

        private static ArchivoDtm Copiar(this ArchivoDtm archivo, ContextoSe contexto, enumNegocio negocio, int idDestino)
        {
            if (negocio == enumNegocio.Carpeta)
            {
                var carpeta = contexto.SeleccionarPorId<CarpetaDtm>(idDestino);
                var modo = ApiDePermisos.LeerModoDeAcceso(contexto, enumNegocio.Archivador, carpeta.IdArchivador);
                if (modo == enumModoDeAccesoDeDatos.Consultor)
                    GestorDeErrores.Emitir($"El archivador de la carpeta '{carpeta.Nombre}' no es editables");

                var archivador = contexto.SeleccionarPorId<ArchivadorDtm>(carpeta.IdArchivador);
                var resultado = archivador.DegradarPermisosDeGestor(contexto);
                if (resultado.Degradado)
                    GestorDeErrores.Emitir(resultado.Mensaje);
            }

            var nuevoNombre = ProponerNombreDeArchivo(contexto, negocio, idDestino, archivo.Nombre);
            var gestor = negocio.CrearGestor(contexto);
            gestor.ValidarPermisosDePersistencia(enumTipoOperacion.Eliminar, gestor.Negocio, idDestino, new Dictionary<string, object>() { { ltrParametrosNeg.ValidarEtapaDocumental, true } });
            var nuevoArchivo = ApiDeArchivos.SubirArchivoInterno(contexto, Path.Combine(archivo.AlmacenadoEn, $"{archivo.Id}.{enumExtensiones.se}"), sincronizar: false, nombreFicheroParaAlmacenar: nuevoNombre, copiar: true, sanitizar: false);
            GestorDeVinculos.Vincular(contexto, negocio, enumNegocio.Archivos, idDestino, nuevoArchivo.Id);
            if (negocio == enumNegocio.Archivador) GestorDeArchivadores.ExportarArchivo(contexto, idArchivador: idDestino, archivo);
            if (negocio == enumNegocio.Carpeta) GestorDeCarpetas.ExportarArchivo(contexto, idCarpeta: idDestino, archivo);
            return nuevoArchivo;
        }

        private static void Mover(this ArchivoDtm archivo, ContextoSe contexto, enumNegocio negocio, int idOrigen, enumNegocio destino, int idDestino)
        {
            if (destino == enumNegocio.Carpeta)
            {
                var carpeta = contexto.SeleccionarPorId<CarpetaDtm>(idDestino);
                var modo = ApiDePermisos.LeerModoDeAcceso(contexto, enumNegocio.Archivador, carpeta.IdArchivador);
                if (modo == enumModoDeAccesoDeDatos.Consultor)
                    GestorDeErrores.Emitir($"El archivador de la carpeta '{carpeta.Nombre}' no es editables");

                var archivador = contexto.SeleccionarPorId<ArchivadorDtm>(carpeta.IdArchivador);
                var resultado = archivador.DegradarPermisosDeGestor(contexto);
                if (resultado.Degradado)
                    GestorDeErrores.Emitir(resultado.Mensaje);
            }

            var gestor = destino.CrearGestor(contexto);
            gestor.ValidarPermisosDePersistencia(enumTipoOperacion.Eliminar, gestor.Negocio, idDestino, new Dictionary<string, object>() { { ltrParametrosNeg.ValidarEtapaDocumental, true } });

            var nuevoNombre = ProponerNombreDeArchivo(contexto, gestor.Negocio, idDestino, archivo.Nombre);

            GestorDeVinculos.Vincular(contexto, gestor.Negocio, enumNegocio.Archivos, idDestino, archivo.Id);
            GestorDeVinculos.BorrarVinculo(contexto, negocio, enumNegocio.Archivos, idOrigen, archivo.Id, new Dictionary<string, object>() { { ltrParametrosNeg.ValidarEtapaDocumental, true } });

            if (nuevoNombre != archivo.Nombre)
            {
                archivo.Nombre = nuevoNombre;
                try
                {
                    archivo.Modificar(contexto);
                }
                finally
                {
                    VinculoSql.BlanquearCacheDeAnexados(contexto, negocio.TipoDtm(), idOrigen);
                    VinculoSql.BlanquearCacheDeAnexados(contexto, gestor.Negocio.TipoDtm(), idDestino);
                }
            }

            if (destino == enumNegocio.Archivador) GestorDeArchivadores.ExportarArchivo(contexto, idArchivador: idDestino, archivo);
            if (destino == enumNegocio.Carpeta) GestorDeCarpetas.ExportarArchivo(contexto, idCarpeta: idDestino, archivo);
        }

        public static void Enlazar(this ArchivoDtm archivo, ContextoSe contexto, enumNegocio negocio, int idDestino, bool validarPermisos = true)
        {
            if (negocio == enumNegocio.Carpeta)
            {
                var carpeta = contexto.SeleccionarPorId<CarpetaDtm>(idDestino);
                var modo = ApiDePermisos.LeerModoDeAcceso(contexto, enumNegocio.Archivador, carpeta.IdArchivador);
                if (modo == enumModoDeAccesoDeDatos.Consultor)
                    GestorDeErrores.Emitir($"El archivador de la carpeta '{carpeta.Nombre}' no es editables");

                var archivador = contexto.SeleccionarPorId<ArchivadorDtm>(carpeta.IdArchivador);
                var resultado = archivador.DegradarPermisosDeGestor(contexto);
                if (resultado.Degradado)
                    GestorDeErrores.Emitir(resultado.Mensaje);
            }

            if (validarPermisos)
            {
                var gestor = negocio.CrearGestor(contexto);
                gestor.ValidarPermisosDePersistencia(enumTipoOperacion.Eliminar, gestor.Negocio, idDestino, new Dictionary<string, object>() { { ltrParametrosNeg.ValidarEtapaDocumental, true } });
            }
            GestorDeVinculos.Vincular(contexto, negocio, enumNegocio.Archivos, idDestino, archivo.Id, new Dictionary<string, object> { { ltrParametrosNeg.ValidarPermisosDePersistencia, validarPermisos } });
        }

        public static string ProponerNombreDeArchivo(ContextoSe contexto, enumNegocio negocio, int idDestino, string nombreDelFichero)
        {
            var extension = Path.GetExtension(nombreDelFichero);
            var nombreSinExtension = Path.GetFileNameWithoutExtension(nombreDelFichero);

            var vinculos = VinculoSql.LeerVinculosCon(contexto, NegociosDeSe.TipoDtm(negocio), enumNegocio.Archivos, ApiDeRegistroDtm.EsquemaTabla(typeof(ArchivoDtm)), idDestino);
            var archivosDtm = GestorDeArchivos.Gestor(contexto, contexto.Mapeador).LeerAnexadosAlRegistro(negocio, idDestino, new Dictionary<string, object> {
                    { ltrParametrosNeg.IncluirOriginales, false},
                    { ltrParametrosNeg.Peticion, enumPeticion.epLeerAnexados }
                });
            var nombresExistentes = archivosDtm.Select(v => Path.GetFileNameWithoutExtension(v.Nombre)).ToHashSet(StringComparer.CurrentCultureIgnoreCase);

            int secuencia = 0;
            var nombrePropuesto = nombreSinExtension;

            while (nombresExistentes.Contains(nombrePropuesto))
            {
                secuencia++;
                nombrePropuesto = $"{nombreSinExtension}_{secuencia}";
            }

            return $"{nombrePropuesto}{extension}";
        }


        private static void ValidarNombreDuplicado(ContextoSe contexto, enumNegocio negocio, int idElemento, string nombreDelFichero)
        {
            var vinculos = VinculoSql.LeerVinculosCon(contexto, NegociosDeSe.TipoDtm(negocio), enumNegocio.Archivos, ApiDeRegistroDtm.EsquemaTabla(typeof(ArchivoDtm)), idElemento);
            foreach (var vinculo in vinculos)
                if (GestorDeArchivos.LeerRegistroPorId(contexto, vinculo.idElemento2).Nombre.Equals(nombreDelFichero))
                    GestorDeErrores.Emitir($"Existe un fichero anexado con el mismo nombre: {nombreDelFichero}");
        }

        internal static void ValidarNombreDuplicado(ContextoSe contexto, ArchivoDtm archivo, bool excepcionSiNoVinculado)
        {
            foreach (var vinculadoCon in NegociosDeSe.VinculadosConArchivos())
            {
                var vinculados = VinculoSql.LeerVinculosAl(contexto, vinculadoCon.TipoDtm(), enumNegocio.Archivos, typeof(ArchivoDtm), archivo.Id, filtros: null);
                if (vinculados.Count >= 1)
                {
                    ValidarNombreDuplicado(contexto, vinculadoCon, vinculados[0].idElemento1, archivo.Nombre);
                    return;
                }
            }

            if (excepcionSiNoVinculado) GestorDeErrores.Emitir($"El fichero {archivo.Nombre} no está vinculado a ningún elemento");
        }

        public static ArchivoDtm ImportarArchivoParaAnexar(ContextoSe contexto, enumNegocio negocio, int idElemento, string rutaConFichero)
        {
            var archivo = ApiDeArchivos.SubirArchivoInterno(contexto, rutaConFichero, sincronizar: true, nombreFicheroParaAlmacenar: null, copiar: false, sanitizar: true);
            GestorDeVinculos.Vincular(contexto, negocio, enumNegocio.Archivos, idElemento, archivo.Id);
            return archivo;
        }

        internal static void EliminarArchivo(ContextoSe contexto, int id, string almacenadoEn)
        {
            try
            {
                File.Delete($@"{almacenadoEn}\{id}.se");
            }
            catch (Exception exc)
            {
                contexto.Traza.AnotarExcepcion(exc);
                throw;
            }
        }

        public static void EliminarFichero(ContextoSe contexto, string ruta)
        {
            if (File.Exists(ruta))
                try
                {
                    File.Delete(ruta);
                }
                catch (Exception exc)
                {
                    contexto.Traza.AnotarExcepcion(exc);
                    throw;
                }
        }

        public static string DescargarArchivo(ContextoSe contexto, int idArchivo, bool solicitadoPorLaCola, bool erroSiNoEstaEnLaruta = false)
        {
            var gestor = GestorDeArchivos.Gestor(contexto, contexto.Mapeador);
            var archivo = gestor.LeerRegistroPorId(idArchivo, true, false, false, aplicarJoin: false);

            return ApiDeArchivos.DescargarArchivo(archivo, contexto, solicitadoPorLaCola, erroSiNoEstaEnLaruta);
        }

        //private static ArchivoDtm SubirArchivoInterno(ContextoSe contexto, string rutaConFichero, bool sincronizar = false, string nombreFicheroParaAlmacenar = null, bool copiar = false)
        //{
        //    return ApiDeArchivos.SubirArchivoInterno(contexto, rutaConFichero, sincronizar, nombreFicheroParaAlmacenar, copiar);
        //}

        internal static List<int> ImportarFicheros(ContextoSe contexto, IEnumerable<string> ficheros, enumNegocio negocio, int idElemento)
        {
            var importados = new List<int>();
            var gestor = GestorDeArchivos.Gestor(contexto, contexto.Mapeador);
            var anexados = gestor.LeerAnexados(negocio, idElemento, new Dictionary<string, object> { { ltrParametrosNeg.IncluirOriginales, true } });
            foreach (var rutaConFichero in ficheros)
            {
                var hayQueImportar = HayQueImportar(contexto, anexados, rutaConFichero);
                if (!hayQueImportar.importar)
                    continue;
                try
                {
                    var copiado = ImportarArchivoParaAnexar(contexto, negocio, idElemento, hayQueImportar.fichero);
                    importados.Add(copiado.Id);
                }
                catch (Exception e)
                {
                    contexto.Traza.AnotarExcepcion(e);
                    //TODO: Anotar excepción en Archivador_Traza
                }
            }
            return importados;
        }

        private static (bool importar, string fichero) HayQueImportar(ContextoSe contexto, List<ArchivoDto> anexados, string rutaConFichero)
        {
            var archivoAnexado = ObtenerArchivoSiEstaAnexado(anexados, rutaConFichero);
            //Si hay archivo, significa que en el fichero de windows existe con el mismo nombre en el elemento
            if (archivoAnexado != null)
            {
                var sinc = ArchivoSincronizadoSql.Leer(contexto, archivoAnexado.Id, errorSiNoHay: false);

                //si no está sincronizado hay que importar, pero con otro nombre para no machacar el que existe, por tanto se cambia
                if (sinc == null)
                {
                    rutaConFichero = CambiarNombreAlFicheroDeWindows(anexados, rutaConFichero);
                }
                else
                {
                    //Si la fecha de sincronización es la mima que la última modificación, no hay que importar nada
                    FileInfo fileInfo = new FileInfo(rutaConFichero);
                    if ((sinc.SincronizadoEl - File.GetCreationTime(rutaConFichero)).Seconds <= 1 && sinc.Longitud - fileInfo.Length == 0)
                        return (false, rutaConFichero);

                    //si no es la misma, le cambio el nombre para importarlo, ya que eso es debido a que o le han cambiado el nombre en la web, o lo han modificado en el directorio
                    rutaConFichero = CambiarNombreAlFicheroDeWindows(anexados, rutaConFichero);
                }
            }
            return (true, rutaConFichero);
        }

        private static string CambiarNombreAlFicheroDeWindows(List<ArchivoDto> anexados, string rutaConFichero)
        {
            var ruta = Path.GetDirectoryName(rutaConFichero);
            var nombre = Path.GetFileNameWithoutExtension(rutaConFichero);
            var ext = Path.GetExtension(rutaConFichero);
            var contador = 1;
            while (true)
            {
                var nuevoNombre = Path.Combine($@"{ruta}\{nombre}_{contador}{ext}");
                if (!File.Exists(nuevoNombre) && anexados.Where(x => x.Nombre.Equals($@"{nombre}_{contador}{ext}", StringComparison.CurrentCultureIgnoreCase)).Count() == 0)
                {
                    File.Move(rutaConFichero, nuevoNombre);
                    return nuevoNombre;
                }
                contador++;
            }
        }


        internal static void ExportarAnexado(GestorDeArchivos gestor, enumNegocio negocio, int idElemento, string directorio, List<int> importados, bool primeraVez)
        {
            var anexados = gestor.LeerAnexados(negocio, idElemento, new Dictionary<string, object> { { ltrParametrosNeg.IncluirOriginales, true } });
            foreach (var anexado in anexados)
            {
                if (importados.Contains(anexado.Id))
                    continue;

                var archivo = gestor.MapearRegistro(anexado, new ParametrosDeNegocio(enumTipoOperacion.MapearElDtoAlDtm));
                if (primeraVez)
                {
                    //Inicializo el registro de sincronización, ya que el directorio puede haber sido borrado estando el archivo ya sincronizado
                    ArchivoSincronizadoSql.Quitar(gestor.Contexto, archivo.Id);
                    ApiDeArchivos.Respaldar(gestor.Contexto, archivo, directorio);
                }
                else
                    ApiDeArchivos.ExportarArchivo(gestor.Contexto, archivo, directorio);
            }
        }

        public static void SometerGenerarZip(ContextoSe contexto, string nombreArchivador, string parametrosJson)
        {
            if (parametrosJson.IsNullOrEmpty())
                GestorDeErrores.Emitir($"No se han proporcionado los parámetros para someter el trabajo de {nameof(enumTrabajosDeArchivadores.GenerarZip)}");


            Dictionary<string, object> parametros = parametrosJson.ToDiccionarioDeParametros();
            parametros[ltrDeUnArchivo.NombreDeArchivo] = nombreArchivador;
            parametrosJson = parametros.ToJson();

            var dll = Assembly.GetExecutingAssembly().GetName().Name;
            var clase = typeof(ServidorDocumental).FullName;
            var ts = GestorDeTrabajosSometido.CrearObtener(contexto, enumTrabajosDeArchivadores.GenerarZip.Descripcion(), dll, clase, nameof(enumTrabajosDeArchivadores.GenerarZip), comunicarFin: false);
            var tu = GestorDeTrabajosDeUsuario.Crear(contexto, ts, new Dictionary<string, object> { { nameof(TrabajoDeUsuarioDtm.Parametros), parametrosJson } });
        }

        public static void GenerarZip(EntornoDeTrabajo entorno)
        {
            Dictionary<string, object> parametros = entorno.TrabajoDeUsuario.Parametros.ToDiccionarioDeParametros();

            var idsDeArchivosJson = parametros[ltrDeUnArchivo.IdsDeArchivos].ToString();
            List<int> idsDeArchivos = JsonConvert.DeserializeObject<List<long>>(idsDeArchivosJson).ConvertAll(x => (int)x).ToList();

            var ficheroZip = Path.Combine(CacheDeVariable.Cfg_RutaDeDescarga, extCadenas.NormalizarFichero((string)parametros[ltrDeUnArchivo.NombreDeArchivo]).Replace(" ", Simbolos.Subrrallado)) + ".Zip";
            ficheroZip = ApiDeArchivos.ObtenerNombreUnico(ficheroZip);

            var contexto = entorno.contextoDelProceso;
            var otorgado = entorno.Ejecutor.OtorgarAdministrador(contexto);
            contexto.IniciarTraza(nameof(GenerarZip));
            var tran = contexto.IniciarTransaccion();
            try
            {
                ArchivadorDtm archivador = TrabajosDelSistemaDocumental.CrearArchivadorZip(entorno, (string)parametros[ltrDeUnArchivo.NombreDeArchivo]);
                TrabajosDelSistemaDocumental.CrearZipConArchivos(entorno.contextoDelProceso, idsDeArchivos, ficheroZip);
                archivador.AnexarZip(entorno.contextoDelProceso, ficheroZip);
                TrabajosDelSistemaDocumental.EnviarCorreoConElArchivador(entorno, archivador);
                contexto.Commit(tran);
            }
            catch (Exception e)
            {
                contexto.Rollback(tran);
                entorno.AnotarError(e);
            }
            finally
            {
                if (otorgado) entorno.Ejecutor.AnularAdministrador(contexto, otorgado);
                contexto.CerrarTraza();
            }
        }

        public static string DescomprimirZip(ContextoSe contexto, ArchivoDtm archivo)
        {
            var rutaConZip = archivo.DescargarArchivo(CacheDeVariable.CFG_Ruta_Ficheros_De_Zip, usarCacheado: false, ponerTickAlNombre: true);
            string nombreZip = Path.GetFileNameWithoutExtension(rutaConZip);
            string rutaDestino = Path.Combine(CacheDeVariable.CFG_Ruta_Ficheros_De_Zip, nombreZip);
            if (Directory.Exists(rutaDestino))
                Directory.Delete(rutaDestino, true);

            Directory.CreateDirectory(rutaDestino);
            try
            {
                ZipFile.ExtractToDirectory(rutaConZip, rutaDestino);
            }
            finally
            {
                File.Delete(rutaConZip);
                File.Delete(Path.Combine(CacheDeVariable.CFG_Ruta_Ficheros_De_Zip, $"{archivo.Id}.{ApiDeArchivos.ExtensionSe}"));
            }
            return rutaDestino;
        }

        public static void ImportarDirectorio(ContextoSe contexto, ArchivadorDtm archivador, string rutaBase, bool remplazar, bool renombrar, bool eliminarArchivo, bool eliminarCarpeta)
        {
            var pilaDirectorios = new Stack<(string Ruta, CarpetaDtm Carpeta)>();
            pilaDirectorios.Push((rutaBase, null));

            while (pilaDirectorios.Count > 0)
            {
                var (rutaActual, carpetaActual) = pilaDirectorios.Pop();

                try
                {
                    // Importar archivos del directorio actual
                    string[] archivos = Directory.GetFiles(rutaActual);
                    ImportarArchivos(contexto, archivador, carpetaActual, rutaActual, archivos, remplazar, renombrar, eliminarArchivo);

                    // Procesar subdirectorios
                    string[] rutasHijas = Directory.GetDirectories(rutaActual);
                    foreach (string rutaHija in rutasHijas)
                    {
                        CarpetaDtm carpetaHija = CrearObtenerCarpetaHijaSegunRuta(contexto, archivador, carpetaActual, rutaHija);
                        pilaDirectorios.Push((rutaHija, carpetaHija));
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error al procesar el directorio {rutaActual}:{Environment.NewLine}{ex.MensajeCompleto(mostrarPila: true)}");
                }
            }
        }

        private static CarpetaDtm CrearObtenerCarpetaHijaSegunRuta(ContextoSe contexto, ArchivadorDtm archivador, CarpetaDtm carpetaPadre, string rutaHija)
        {
            var nombreCarpeta = Path.GetFileName(rutaHija);
            var subdirectorio = nombreCarpeta.ToLower();

            var consulta = carpetaPadre == null
                ? contexto.Set<CarpetaDtm>().Where(x => x.IdArchivador == archivador.Id && x.Nombre.ToLower() == subdirectorio && x.IdPadre == null)
                : contexto.Set<CarpetaDtm>().Where(x => x.IdArchivador == archivador.Id && x.Nombre.ToLower() == subdirectorio && x.IdPadre == carpetaPadre.Id);

            var carpetaHija = consulta.FirstOrDefault();
            if (carpetaHija == null)
            {
                carpetaHija = new CarpetaDtm
                {
                    Nombre = nombreCarpeta,
                    IdPadre = carpetaPadre?.Id,
                    IdArchivador = archivador.Id
                }.Insertar(contexto);
            }

            return carpetaHija;
        }

        private static void ImportarArchivos(ContextoSe contexto, ArchivadorDtm archivador, CarpetaDtm carpeta, string ruta, string[] archivos, bool remplazar, bool renombrar, bool eliminarArchivo)
        {
            var anexados = carpeta == null ? archivador.LeerAnexados(contexto) : carpeta.LeerAnexados(contexto);
            foreach (string archivo in archivos)
            {
                var nombreDelArchivo = Path.GetFileName(archivo).ToLower();
                var anexado = anexados.FirstOrDefault(x => x.Nombre.ToLower() == nombreDelArchivo);
                var renombrado = "";
                if (anexado is not null)
                {
                    if (remplazar && !renombrar)
                    {
                        var quitado = IntentarQuitar(contexto, archivador, carpeta, anexado);
                        if (!quitado)
                        {
                            remplazar = false;
                            renombrar = true;
                        }
                    }
                    if (!remplazar && renombrar)
                    {
                        renombrado = nombreDelArchivo;
                        while (true)
                        {
                            var aux = ApiDeArchivos.ProponerNombreDeArchivo(anexados, renombrado);
                            var fichero = Path.Combine(ruta, aux);
                            if (!File.Exists(fichero))
                            {
                                renombrado = aux;
                                break;
                            }
                            renombrado = aux;
                        }

                        File.Move(archivo, Path.Combine(ruta, renombrado));
                    }
                    else if (!remplazar && !renombrar) continue;
                }

                AnexarArchivo(contexto,
                    carpeta == null ? enumNegocio.Archivador : enumNegocio.Carpeta,
                    carpeta == null ? archivador.Id : carpeta.Id,
                    Path.Combine(ruta, renombrado.IsNullOrEmpty() ? nombreDelArchivo : renombrado), copiar: false, quitarExtensionHtml: true, sanitizar: true);
            }

            if (eliminarArchivo)
            {
                foreach (var anexado in anexados)
                {
                    var esta = false;
                    foreach (string archivo in archivos)
                    {
                        var nombreDelArchivo = Path.GetFileName(archivo).ToLower();
                        if (nombreDelArchivo == anexado.Nombre.ToLower())
                        {
                            esta = true;
                            break;
                        }
                    }
                    if (!esta) IntentarQuitar(contexto, archivador, carpeta, anexado);
                }
            }
        }

        private static bool IntentarQuitar(ContextoSe contexto, ArchivadorDtm archivador, CarpetaDtm carpeta, ArchivoDtm anexado)
        {
            try
            {
                if (carpeta == null)
                    archivador.QuitarAnexado(contexto, anexado.Id);
                else
                    carpeta.QuitarAnexado(contexto, anexado.Id);
                return true;
            }
            catch (Exception e)
            {
                archivador.CrearTraza(contexto, $"Al importar no se ha podido eliminar el archivo: '{(carpeta == null ? "" : $"{carpeta.Expresion}.")}{anexado.Nombre}'", e.Message);
            }
            return false;
        }

        public static void SometerExportacion(ContextoSe contexto, string parametros)
        {
            if (parametros.IsNullOrEmpty())
                GestorDeErrores.Emitir($"No se han proporcionado los parámetros para someter el trabajo de {Exportacion}");

            var dll = Assembly.GetExecutingAssembly().GetName().Name;
            var clase = typeof(ServidorDocumental).FullName;
            var ts = GestorDeTrabajosSometido.CrearObtener(contexto, enumTrabajosSometidos.ExportarExcell, dll, clase, nameof(SometerExportacion), comunicarFin: false);
            var tu = GestorDeTrabajosDeUsuario.Crear(contexto, ts, new Dictionary<string, object> { { nameof(TrabajoDeUsuarioDtm.Parametros), parametros } });
        }

        public static void Exportacion(EntornoDeTrabajo entorno)
        {
            Dictionary<string, object> parametros = entorno.TrabajoDeUsuario.Parametros.ToDiccionarioDeParametros();
            var idPlantilla = (int)(long)parametros[ltrParametrosEp.IdPlantilla];

            var idCg = (int)parametros.LeerValor(ltrParametrosEp.IdCg, (long)0);
            parametros[nameof(ArchivadorDtm)] = idCg == 0 ? null : new ArchivadorDtm
            {
                IdCg = (int)(long)parametros[ltrParametrosEp.IdCg],
                IdTipo = GestorDeTiposDeArchivadores.Cfg_Id_Tipo_De_Archivador_De_Exportacion(entorno.contextoDelProceso),
                Nombre = (string)parametros[ltrParametrosEp.Archivador],
                Descripcion = (string)parametros[ltrParametrosEp.Motivo]
            }.InsertarSinValidarPermisos(entorno.contextoDelProceso);

            var ficheroConRuta = idPlantilla == 0
                ? ExportacionEstandard(entorno, parametros)
                : ExportacionConPlantilla(entorno, parametros, idPlantilla);

            TipoDtoElmento elementoAdjunto = null;

            if (parametros[nameof(ArchivadorDtm)] is not null)
            {
                var archivador = (ArchivadorDtm)parametros[nameof(ArchivadorDtm)];
                var archivo = AnexarArchivo(entorno.contextoDelProceso, enumNegocio.Archivador, archivador.Id, ficheroConRuta, sanitizar: false);
                BloquearArchivo(entorno.contextoDelProceso, enumNegocio.Archivador.IdNegocio(), archivador.Id, archivo.Id, ltrDeUnArchivo.Exportacion);
                elementoAdjunto = new TipoDtoElmento
                {
                    TipoDto = typeof(ArchivadorDto).FullName,
                    IdElemento = archivador.Id,
                    Referencia = archivador.Expresion
                };

                if (!archivador.EsGestor<TipoDeArchivadorDtm>(entorno.contextoDelProceso))
                {
                    GestorDePemisosDelElemento.CrearPermisos(entorno.contextoDelProceso, enumNegocio.Archivador, new List<int> { archivador.Id });
                    new PermisosPorElementoDtm
                    {
                        IdNegocio = enumNegocio.Archivador.IdNegocio(),
                        IdElemento = archivador.Id,
                        IdUsuario = entorno.TrabajoDeUsuario.IdEjecutor,
                        IdPermiso = 2,
                        Calculado = false
                    }.Insertar(entorno.contextoDelProceso, new Dictionary<string, object> { { ltrParametrosNeg.ValidarPermisosDePersistencia, false } });
                }
            }

            var correo = entorno.contextoDelProceso.SeleccionarPorId<UsuarioDtm>(entorno.TrabajoDeUsuario.IdEjecutor).eMail;
            GestorDeCorreos.CrearCorreoPara(entorno.contextoDelProceso
                , new List<string> { correo }
                , "Exportación solicitada"
                , "Se le adjunta el archivador con la exportación solicitada"
                , elementoAdjunto == null ? new List<TipoDtoElmento>() : new List<TipoDtoElmento> { elementoAdjunto }
                , elementoAdjunto == null ? new List<string> { ficheroConRuta } : new List<string>()
                );
        }

        private static string ExportacionConPlantilla(EntornoDeTrabajo entorno, Dictionary<string, object> parametros, int idPlantilla)
        {
            var contexto = entorno.contextoDelProceso;
            var idNegocio = (int)(long)parametros[ltrParametrosEp.idNegocio];

            parametros[nameof(TrabajoDeUsuarioDtm)] = entorno.TrabajoDeUsuario;
            parametros[nameof(ContextoSe)] = entorno.ContextoDelEntorno;

            var plantilla = contexto.SeleccionarPorId<PlantillaDeExportacionDtm>(idPlantilla);
            var accion = new AccionDtm { Nombre = plantilla.Nombre, Metodo = plantilla.Metodo, Dll = plantilla.Dll, Clase = plantilla.Clase, ClaseDeAccion = enumClaseDeAccion.DLL.ToString() };
            var salida = accion.Ejecutar(contexto, plantilla, NegociosDeSe.ToEnumerado(idNegocio), entrada: parametros, parametros: null);
            return salida[nameof(ObjetoParaExportar.FicheroConRuta)].ToString();
        }

        public static string ExportacionEstandard(EntornoDeTrabajo entorno, Dictionary<string, object> parametros)
        {

            if (!parametros.ContainsKey(nameof(ElementoDto)))
                GestorDeErrores.Emitir("No se ha indicado el ElementoDto de exportación");

            if (!parametros.ContainsKey(nameof(RegistroDtm)))
                GestorDeErrores.Emitir("No se ha indicado el Registro de exportación");

            var nombre = (string)parametros.LeerValor(ltrParametrosEp.negocio, "");
            var negocio = NegociosDeSe.ToEnumerado(nombre, true);
            var gestor = NegociosDeSe.CrearGestor(entorno.contextoDelProceso, parametros[nameof(RegistroDtm)].ToString(), parametros[nameof(ElementoDto)].ToString(), negocio);

            var cantidad = !parametros.ContainsKey(ltrFiltros.cantidad) ? -1 : parametros[ltrFiltros.cantidad].ToString().Entero();
            var posicion = !parametros.ContainsKey(ltrFiltros.posicion) ? 0 : parametros[ltrFiltros.posicion].ToString().Entero();
            List<ClausulaDeFiltrado> filtros = !parametros.ContainsKey(ltrFiltros.filtro) || parametros[ltrFiltros.filtro].ToString().IsNullOrEmpty() ? new List<ClausulaDeFiltrado>() : JsonConvert.DeserializeObject<List<ClausulaDeFiltrado>>(parametros.LeerValor<string>(ltrParametrosEp.Filtro));
            List<ClausulaDeOrdenacion> orden = !parametros.ContainsKey(ltrFiltros.orden) || parametros[ltrFiltros.orden].ToString().IsNullOrEmpty() ? new List<ClausulaDeOrdenacion>() : JsonConvert.DeserializeObject<List<ClausulaDeOrdenacion>>(parametros.LeerValor<string>(ltrParametrosEp.Orden));

            var opcionesDeMapeo = new Dictionary<string, object>
            {
                { ltrParametrosNeg.AplicarJoin, true },
                { ltrParametrosNeg.Peticion, enumPeticion.epExportar }
            };

            JObject columnasJobject = (JObject)negocio.LeerParametroDeUsuario<JObject>(entorno.contextoDelProceso, enumParametrosDeUsuario.USU_Colunas_Del_Grid);
            List<VisibilidadDeColumna> visibilidadDeColumnas = columnasJobject.HasValues
                ? columnasJobject[ltrParametrosDeUsuarios.columnasJson].ToObject<List<VisibilidadDeColumna>>()
                : null;

            opcionesDeMapeo[ltrParametrosNeg.ColumnasDelGrid] = visibilidadDeColumnas is null
                ? new List<string>()
                : visibilidadDeColumnas.Select(x => x.Propiedad).ToList();

            dynamic elementos = gestor.LeerElementos(posicion, cantidad, filtros, orden, opcionesDeMapeo);
            return GenerarExcel(entorno.contextoDelProceso, elementos, visibilidadDeColumnas);
        }

        private static string GenerarExcel<T>(ContextoSe contexto, List<T> elementos, List<VisibilidadDeColumna> visibilidadDeColumnas)
        {
            var negocio = NegociosDeSe.NegocioDeUnDto(typeof(T));
            contexto.IniciarTraza(nameof(GenerarExcel), debugar: true);
            try
            {
                if (negocio == enumNegocio.No_Definido)
                {
                    return elementos.ToExcel(contexto, contexto.ObtenerRutaExcel(), fichero: typeof(T).Name, null, null);
                }

                JObject disposicionJobject = (JObject)negocio.LeerParametroDeUsuario<JObject>(contexto, enumParametrosDeUsuario.USU_Disposicion_Del_Encolumnado);
                List<DisposicionDeColumna> disposicionDeColumnas = disposicionJobject.HasValues ? disposicionJobject[ltrParametrosDeUsuarios.encolumnado].ToObject<List<DisposicionDeColumna>>() : null;

                JObject ordenJobject = (JObject)negocio.LeerParametroDeUsuario<JObject>(contexto, enumParametrosDeUsuario.USU_Ordenacion_Del_Resultado);
                List<OrdenDeColumna> ordenDeColumnas = ordenJobject.HasValues ? ordenJobject[ltrParametrosDeUsuarios.ordenacion].ToObject<List<OrdenDeColumna>>() : null;


                if (visibilidadDeColumnas == null)
                {
                    JObject columnasJobject = (JObject)negocio.LeerParametroDeUsuario<JObject>(contexto, enumParametrosDeUsuario.USU_Colunas_Del_Grid);
                    visibilidadDeColumnas = columnasJobject.HasValues ? columnasJobject[ltrParametrosDeUsuarios.columnasJson].ToObject<List<VisibilidadDeColumna>>() : null;
                }

                contexto.AnotarTraza("Usuario ejecutor", contexto.DatosDeConexion.IdUsuario.ToString());
                contexto.AnotarTraza("Registros leidos", elementos.Count.ToString());
                contexto.AnotarTraza("Información de columnas visibles", visibilidadDeColumnas?.Count.ToString() ?? "no definido");
                contexto.AnotarTraza("Disposición del encolumnado", disposicionDeColumnas?.Count.ToString() ?? "No definido");


                var patron = ApiParaDtos.PatronUrl(typeof(T));

                return elementos.ToExcel(contexto, contexto.ObtenerRutaExcel(), fichero: negocio.Singular(), visibilidadDeColumnas, disposicionDeColumnas, patron);

            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        public static string DescargarExcel<T>(ContextoSe contexto, List<T> elementos)
        {
            var ficheroConRuta = GenerarExcel(contexto, elementos, visibilidadDeColumnas: null);
            return ApiDeArchivos.UrlDeArchivo(ApiDeArchivos.DescargarArchivo(ficheroConRuta));
        }

        public static string ObtenerRutaExcel(this ContextoSe contexto)
        {
            string ruta = GestorDeVariables.RutaDeExportaciones;
            var fecha = DateTime.Now;
            string rutaDeExportacion = $@"{ruta}\{fecha.Year}-{fecha.Month}-{fecha.Day}\{contexto.DatosDeConexion.Login}";
            if (!Directory.Exists(rutaDeExportacion))
                Directory.CreateDirectory(rutaDeExportacion);
            return rutaDeExportacion;
        }

        public static void ProcesarOperacion(ContextoSe contexto, enumOperacionesConArchivos operacion, int idNegocio, int idOrigen, enumNegocio negocioDestino, int idDestino, List<long> idsDeArchivos, bool validarPermisos = true, bool errorSiNoVinculado = true)
        {
            var negocioOrigen = NegociosDeSe.ToEnumerado(idNegocio);
            try
            {
                foreach (var id in idsDeArchivos)
                {
                    var archivo = contexto.SeleccionarPorId<ArchivoDtm>(Convert.ToInt32(id));
                    var registroOrigen = (RegistroConNombreDtm)negocioOrigen.LeerRegistro(contexto, idOrigen);
                    var referencia = registroOrigen.GetType().ImplementaUnElemento()
                        ? ((ElementoDtm)registroOrigen).Referencia(contexto)
                        : negocioOrigen == enumNegocio.Carpeta
                        ?
                        ((CarpetaDtm)registroOrigen).Referencia(contexto)
                        : registroOrigen.Expresion;


                    if (!GestorDeVinculos.Existe(contexto, negocioOrigen, enumNegocio.Archivos, idOrigen, archivo.Id))
                    {
                        if (!errorSiNoVinculado)
                            return;

                        var ori = $"{negocioOrigen}: {registroOrigen.Nombre}";
                        var des = $"{negocioDestino}: {(RegistroConNombreDtm)negocioDestino.LeerRegistro(contexto, idDestino)}";
                        GestorDeErrores.Emitir($"Está intentando '{operacion}' el archivo '{archivo.Nombre}' de '{ori}' a '{des}', y no está vinculado a '{ori}'");
                    }

                    if (operacion == enumOperacionesConArchivos.Copiar)
                    {
                        var nuevo = archivo.Copiar(contexto, negocioDestino, idDestino);
                        var registroDestino = (RegistroConNombreDtm)negocioDestino.LeerRegistro(contexto, idDestino);
                        var destino = registroDestino.GetType().ImplementaUnElemento() ? ((ElementoDtm)registroDestino).Referencia(contexto) : registroDestino.Expresion;
                        archivo.AuditarCopiar(contexto, nuevo, referencia, destino);
                    }

                    if (operacion == enumOperacionesConArchivos.Mover)
                    {
                        if (negocioOrigen == negocioDestino && idOrigen == idDestino)
                        {
                            GestorDeErrores.Emitir("No puede mover el archivo ya que el destino indicado es el mismo");
                        }

                        archivo.Mover(contexto, negocioOrigen, idOrigen, negocioDestino, idDestino);
                        var mensaje = ltrDeAuditoriaDeArchivo.Mover.Replace("[0]", contexto.DatosDeConexion.Login)
                            .Replace("[1]", DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"))
                            .Replace("[2]", $"Origen: '{referencia}'");
                        archivo.AuditarOperacion(contexto, mensaje);
                    }

                    if (operacion == enumOperacionesConArchivos.Enlazar)
                    {
                        archivo.Enlazar(contexto, negocioDestino, idDestino, validarPermisos);
                        archivo.AuditarEnlazar(contexto, referencia);
                    }
                }
            }
            finally
            {
                VinculoSql.BlanquearCacheDeAnexados(contexto, negocioOrigen.TipoDtm(), idOrigen);
                VinculoSql.BlanquearCacheDeAnexados(contexto, negocioDestino.TipoDtm(), idDestino);
            }

        }

        public static void BloquearArchivo(ContextoSe contexto, int idNegocio, int idElemento, int idArchivo, string motivo, bool validarSiEstaTerminado = true)
        {
            var archivo = contexto.SeleccionarPorId<ArchivoDtm>(idArchivo);
            var negocio = NegociosDeSe.ToEnumerado(idNegocio); var registro = negocio != enumNegocio.Carpeta
            ? negocio.RegistroPorId(contexto, idElemento)
            : enumNegocio.Carpeta.RegistroPorId(contexto, idElemento);

            if (!GestorDeVinculos.Existe(contexto, negocio, enumNegocio.Archivos, idElemento, idArchivo))
            {
                GestorDeErrores.Emitir($"Está intentando bloquear el archivo '{archivo.Nombre}' y no está vinculado directamente a '{registro.Nombre}' edite su '{(negocio == enumNegocio.Archivador ? enumNegocio.Carpeta.Singular(true) : enumNegocio.Archivador.Singular(true))}' y bloqueelo ahí");
            }

            if (motivo.IsNullOrEmpty())
                GestorDeErrores.Emitir($"Está intentando bloquear el archivo '{archivo.Nombre}' y no ha indicado el motivo");



            if (motivo != ltrDeUnArchivo.CorreoImportado &&
                motivo != ltrDeUnArchivo.Exportacion &&
                motivo != ltrDeUnArchivo.ZipGenerado)
                ValidarPermisosDeOperacion(contexto, archivo, negocio, registro, "bloquear", validarSiEstaTerminado);

            var bloqueo = contexto.SeleccionarPorFk<BloqueoDeUnArchivoDtm>(nameof(BloqueoDeUnArchivoDtm.IdArchivo), archivo.Id, errorSiNoHay: false);
            try
            {
                if (bloqueo is null)
                {
                    new BloqueoDeUnArchivoDtm
                    {
                        IdArchivo = archivo.Id,
                        Bloqueado = true
                    }.Insertar(contexto, new Dictionary<string, object> { { nameof(AuditoriaDeUnArchivoDtm.Auditoria), motivo } });
                }
                else
                {
                    if (bloqueo.Bloqueado) GestorDeErrores.Emitir($"Está intentando bloquear el archivo '{archivo.Nombre}' y éste ya está bloqueado");
                    bloqueo.Bloqueado = true;
                    bloqueo.Modificar(contexto, new Dictionary<string, object> { { nameof(AuditoriaDeUnArchivoDtm.Auditoria), motivo } });
                }
            }
            finally
            {
                VinculoSql.BlanquearCacheDeAnexados(contexto, negocio.TipoDtm(), idElemento);
            }
        }

        public static void BloquearArchivos(ContextoSe contexto, int idNegocio, int idElemento, List<long> idsDeArchivos, string motivo)
        {
            foreach (var idArchivo in idsDeArchivos)
            {
                BloquearArchivo(contexto, idNegocio, idElemento, Convert.ToInt32(idArchivo), motivo);
            }
        }

        public static void DesbloquearArchivos(ContextoSe contexto, int idNegocio, int idElemento, List<long> idsDeArchivos, string motivo)
        {
            foreach (var idArchivo in idsDeArchivos)
            {
                DesbloquearArchivo(contexto, idNegocio, idElemento, Convert.ToInt32(idArchivo), motivo);
            }
        }
        public static void DesbloquearArchivo(ContextoSe contexto, int idNegocio, int idElemento, int idArchivo, string motivo)
        {
            var archivo = contexto.SeleccionarPorId<ArchivoDtm>(idArchivo);

            if (motivo.IsNullOrEmpty())
                GestorDeErrores.Emitir($"Está intentando desbloquear el archivo '{archivo.Nombre}' y no ha indicado el motivo");

            var negocio = NegociosDeSe.ToEnumerado(idNegocio);
            var registro = negocio != enumNegocio.Carpeta
            ? negocio.RegistroPorId(contexto, idElemento)
            : enumNegocio.Carpeta.RegistroPorId(contexto, idElemento);

            if (!GestorDeVinculos.Existe(contexto, negocio, enumNegocio.Archivos, idElemento, idArchivo))
            {
                GestorDeErrores.Emitir($"Está intentando desbloquear el archivo '{archivo.Nombre}' y no está vinculado directamente a '{registro.Nombre}' edite su '{(negocio == enumNegocio.Archivador ? enumNegocio.Carpeta.Singular(true) : enumNegocio.Archivador.Singular(true))}' y bloqueelo ahí");
            }

            ValidarPermisosDeOperacion(contexto, archivo, negocio, registro, operacion: "desbloquear");

            var bloqueo = contexto.SeleccionarPorFk<BloqueoDeUnArchivoDtm>(nameof(BloqueoDeUnArchivoDtm.IdArchivo), archivo.Id, errorSiNoHay: false);
            if (bloqueo is null || !bloqueo.Bloqueado)
                GestorDeErrores.Emitir($"Está intentando desbloquear el archivo '{archivo.Nombre}' y éste no está bloqueado");

            bloqueo.Bloqueado = false;
            try
            {
                bloqueo.Modificar(contexto, new Dictionary<string, object> { { nameof(AuditoriaDeUnArchivoDtm.Auditoria), motivo } });
            }
            finally
            {
                VinculoSql.BlanquearCacheDeAnexados(contexto, negocio.TipoDtm(), idElemento);
            }
        }

        public static void ValidarPermisosDeOperacion(ContextoSe contexto, ArchivoDtm archivo, enumNegocio negocio, RegistroConNombreDtm registro, string operacion, bool validarSiEstaTerminado = true)
        {
            if (negocio == enumNegocio.Carpeta)
                registro = contexto.SeleccionarPorId<ArchivadorDtm>(GestorDeCarpetas.LeerRegistroPorId(contexto, registro.Id).IdArchivador);

            if (!((IElementoDtm)registro).EsGestor(contexto))
                GestorDeErrores.Emitir($"Está intentando {operacion} el archivo '{archivo.Nombre}' y no es gestor del elemento '{((ElementoDtm)registro).Referencia()}' del negocio '{negocio.Singular()}'");

            if ((negocio.UsaBaja() || enumNegocio.Carpeta == negocio) && ((IUsaBaja)registro).Baja)
                GestorDeErrores.Emitir($"Está intentando {operacion} el archivo '{archivo.Nombre}' del elemento '{((ElementoDtm)registro).Referencia()}' del negocio '{negocio.Singular()}' y está de baja");

            if (negocio.UsaFlujo())
            {
                if (((IElementoDeProcesoDtm)registro).Estado(contexto).Cancelado)
                    GestorDeErrores.Emitir($"Está intentando {operacion} el archivo '{archivo.Nombre}' del elemento '{((ElementoDtm)registro).Referencia()}' del negocio '{negocio.Singular()}' que está cancelado");

                if (validarSiEstaTerminado && ((IElementoDeProcesoDtm)registro).Estado(contexto).Terminado)
                    GestorDeErrores.Emitir($"Está intentando {operacion} el archivo '{archivo.Nombre}' del elemento '{((ElementoDtm)registro).Referencia()}' del negocio '{negocio.Singular()}' que está terminado");
            }

            if (negocio == enumNegocio.Archivador)
                ValidarQueLosVinculadosEstanVivos(contexto, archivo, registro, operacion, validarSiEstaTerminado);
        }

        private static void ValidarQueLosVinculadosEstanVivos(ContextoSe contexto, ArchivoDtm archivo, RegistroConNombreDtm registro, string operacion, bool validarSiEstaTerminado)
        {
            var lista = enumNegocio.Archivador.VinculosCon(contexto);
            foreach (var tipoDtm in lista)
            {
                var negocioDelVinculado = NegociosDeSe.NegocioDeUnDtm(tipoDtm);
                var vinculados = VinculoSql.LeerVinculosAl(contexto, tipoDtm, enumNegocio.Archivador, typeof(ArchivadorDtm), registro.Id, filtros: null);
                foreach (var vinculo in vinculados)
                {
                    if (negocioDelVinculado.UsaFlujo())
                    {
                        var registroVinculado = negocioDelVinculado.RegistroPorId(contexto, vinculo.idElemento1);

                        if (((IElementoDeProcesoDtm)registroVinculado).Estado(contexto).Cancelado)
                            GestorDeErrores.Emitir($"Está intentando {operacion} el archivo '{archivo.Nombre}' del archivador '{((ElementoDtm)registro).Referencia()}' del elemento '{((ElementoDtm)registroVinculado).Referencia()}' del negocio '{negocioDelVinculado.Singular()}' que está cancelado");

                        if (validarSiEstaTerminado && ((IElementoDeProcesoDtm)registroVinculado).Estado(contexto).Terminado)
                            GestorDeErrores.Emitir($"Está intentando {operacion} el archivo '{archivo.Nombre}' del archivador '{((ElementoDtm)registro).Referencia()}' del elemento '{((ElementoDtm)registroVinculado).Referencia()}' del negocio '{negocioDelVinculado.Singular()}' que está terminado");
                    }
                }
            }
        }
    }
}
