using GestoresDeNegocio.TrabajosSometidos;
using ServicioDeDatos.TrabajosSometidos;
using ServicioDeDatos;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using Utilidades;
using ServicioDeDatos.SistemaDocumental;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using ModeloDeDto.SistemaDocumental;
using System.IO;
using ServicioDeDatos.Negocio;
using System.IO.Compression;
using GestoresDeNegocio.Seguridad;
using ServicioDeDatos.Seguridad;
using ModeloDeDto;
using ServicioDeDatos.Entorno;
using Gestor.Errores;
using GestoresDeNegocio.Gastos;

namespace GestoresDeNegocio.SistemaDocumental
{
    public enum enumTrabajosDeArchivadores
    {
        [Description("Importa un Zip a un archivador")]
        ImportarZip,
        [Description("Generar Zip")]
        GenerarZip,
        [Description("Exportar un archivador")]
        ExportarArchivador,
        [Description("Sincroniza un archivador con una carpeta del servidor")]
        SincronizarArchivador,
        [Description("Procesar archivos facturas con la IA")]
        ProcesarFarConIa
    }

    public static class TrabajosDelSistemaDocumental
    {
        public static void SometerExportacionDeArchivadores(ContextoSe contexto, List<int> ids)
        {
            var dll = Assembly.GetExecutingAssembly().GetName().Name;
            var clase = typeof(TrabajosDelSistemaDocumental).FullName;
            var ts = GestorDeTrabajosSometido.CrearObtener(contexto, enumTrabajosDeArchivadores.ExportarArchivador.Descripcion(), dll, clase, nameof(enumTrabajosDeArchivadores.ExportarArchivador), comunicarFin: false);
            foreach (var idArchivador in ids)
            {
                var parametrosEntrada = new Dictionary<string, object> { { nameof(ArchivadorDtm.Id), idArchivador } };
                var datosDeCreacion = new Dictionary<string, object>
                {
                    { nameof(TrabajoDeUsuarioDtm.Parametros), parametrosEntrada.ToJson() }
                };
                GestorDeTrabajosDeUsuario.Crear(contexto, ts, datosDeCreacion);
            }
        }

        public static void ExportarArchivador(EntornoDeTrabajo entorno)
        {
            var contexto = entorno.contextoDelProceso;
            Dictionary<string, object> parametros = entorno.TrabajoDeUsuario.Parametros.ToDiccionarioDeParametros();
            var idArchivador = (int)parametros.LeerValor<long>(nameof(ArchivadorDtm.Id));
            var archivador = contexto.SeleccionarPorId<ArchivadorDtm>(idArchivador);
            var otorgado = entorno.Ejecutor.OtorgarAdministrador(contexto);
            contexto.IniciarTraza(nameof(ExportarArchivador));

            var idSemaforo = SemaforoDeProcesoSql.PonerSemaforo(entorno.ContextoDelEntorno, enumNegocio.Archivador.IdNegocio(), idArchivador, enumOpercionesDeSemaforo.EARC, archivador.Referencia).Id;
            var tran = contexto.IniciarTransaccion();
            try
            {
                ArchivadorDtm destino = CrearArchivadorZip(entorno, $"Exportación de: {archivador.Referencia}");
                ExportarArchivador(entorno, archivador, destino);
                EnviarCorreoConElArchivador(entorno, destino);
                contexto.Commit(tran);
            }
            catch (Exception e)
            {
                contexto.Rollback(tran);
                entorno.AnotarError(e);
            }
            finally
            {
                SemaforoDeProcesoSql.QuitarSemaforo(entorno.ContextoDelEntorno, idSemaforo);
                if (otorgado) entorno.Ejecutor.AnularAdministrador(contexto, otorgado);
                contexto.CerrarTraza();
            }
        }

        private static void ExportarArchivador(EntornoDeTrabajo entorno, ArchivadorDtm origen, ArchivadorDtm destino)
        {
            var contexto = entorno.contextoDelProceso;
            var carpetas = origen.LeerJerarquiaDeCarpetas(contexto, new Dictionary<string, object>());

            var rutaInicial = Path.Combine(CacheDeVariable.Cfg_RutaDeDescarga, extCadenas.NormalizarFichero(origen.Referencia).Replace(" ", Simbolos.Subrrallado));
            if (Path.Exists(rutaInicial))
            {
                GestorDeErrores.Emitir($"No se puede exportar ya que actualmente existe la rura '{rutaInicial}', inténtelo más tarde");
            }

            string ficheroZip = null;
            try
            {
                Directory.CreateDirectory(rutaInicial);
                foreach (var carpeta in carpetas)
                {
                    var ruta = GestorDeCarpetas.ObtenerRuta(origen.SincronizarCon, carpetas.ToNodosDeJerarquia(mostrarNumero: false), carpeta);
                    DescargarCarpeta(contexto, rutaInicial, carpeta, ruta.StartsWith('\\') ? ruta.Substring(1) : ruta);
                }

                var archivos = origen.LeerAnexados(contexto);
                DescargarArchivosEnRuta(rutaInicial, archivos);

                ficheroZip = ComprimirDirectorio(rutaInicial, origen.Referencia);

                destino.AnexarZip(contexto, ficheroZip);
            }
            finally
            {
                if (File.Exists(ficheroZip))
                    File.Delete(ficheroZip);
                if (Path.Exists(rutaInicial))
                    Directory.Delete(rutaInicial, recursive: true);
            }
        }

        private static string ComprimirDirectorio(string directorioOrigen, string nombreFichero)
        {
            // Asegurarse de que el nombre del fichero ZIP termine con .zip
            if (!nombreFichero.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                nombreFichero += ".zip";
            }

            // Crear el archivo ZIP en el directorio CacheDeVariable.Cfg_RutaDeDescarga
            var archivoZip = Path.Combine(CacheDeVariable.Cfg_RutaDeDescarga, nombreFichero);


            using (FileStream zipToOpen = new FileStream(archivoZip, FileMode.Create))
            {
                using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Create))
                {
                    // Obtener todos los archivos en el directorio y subdirectorios
                    string[] archivos = Directory.GetFiles(directorioOrigen, "*", SearchOption.AllDirectories);

                    foreach (string archivo in archivos)
                    {
                        // Calcular la ruta relativa del archivo dentro del ZIP
                        string entryName = archivo.Substring(directorioOrigen.Length + 1);

                        // Crear la entrada en el ZIP
                        archive.CreateEntryFromFile(archivo, entryName);
                    }
                }
            }

            return archivoZip;
        }

        private static void DescargarCarpeta(ContextoSe contexto, string rutaInicial, NodoDeCarpetaDtm carpeta, string ruta)
        {
            var rutaDeCarpeta = Path.Combine(rutaInicial, ruta);
            Directory.CreateDirectory(rutaDeCarpeta);
            var archivos = contexto.SeleccionarPorId<CarpetaDtm>(carpeta.Id).LeerAnexados(contexto);
            DescargarArchivosEnRuta(rutaDeCarpeta, archivos);
        }

        private static void DescargarArchivosEnRuta(string ruta, List<ArchivoDtm> archivos)
        {
            string incluir = null;
            foreach (var archivo in archivos)
            {
                try
                {
                    incluir = archivo.DescargarArchivo(ruta, usarCacheado: false, ponerTickAlNombre: false);
                }
                finally
                {
                    var rutaId = Path.Combine(Path.GetDirectoryName(incluir), $"{archivo.Id}.{ApiDeArchivos.ExtensionSe}");
                    if (File.Exists(rutaId))
                        File.Delete(rutaId);
                }
            }
        }

        public static TrabajoDeUsuarioDtm SometerProcesarFarConIa(ContextoSe contexto, int idArchivador, int idCg, int idTipo, int? idProveedor, int? idCarpeta)
        {
            var dll = Assembly.GetExecutingAssembly().GetName().Name;
            var clase = typeof(TrabajosDelSistemaDocumental).FullName;
            var ts = GestorDeTrabajosSometido.CrearObtener(contexto, enumTrabajosDeArchivadores.ProcesarFarConIa.Descripcion(), dll, clase, nameof(enumTrabajosDeArchivadores.ProcesarFarConIa), comunicarFin: true);
            var parametrosEntrada = new Dictionary<string, object> {
                        { nameof(ProcesarFarConIaDto.IdArchivador), idArchivador },
                        { nameof(ProcesarFarConIaDto.IdCgPropuesto), idCg },
                        { nameof(ProcesarFarConIaDto.IdTipoFarPropuesto), idTipo },
                        { nameof(ProcesarFarConIaDto.IdProveedor), idProveedor },
                        { nameof(ProcesarFarConIaDto.IdCarpetaSeleccionada), idCarpeta }
                };
            var datosDeCreacion = new Dictionary<string, object>
                {
                    { nameof(TrabajoDeUsuarioDtm.Parametros), parametrosEntrada.ToJson() },
                    { nameof(TrabajoDeUsuarioDtm.Planificado), DateTime.Now.AddMinutes(-1) }
                };
            return GestorDeTrabajosDeUsuario.Crear(contexto, ts, datosDeCreacion);
        }

        public static void ProcesarFarConIa(EntornoDeTrabajo entorno)
        {
            var contexto = entorno.contextoDelProceso;
            Dictionary<string, object> parametros = entorno.TrabajoDeUsuario.Parametros.ToDiccionarioDeParametros();
            var idArchivador = (int)parametros.LeerValor<long>(nameof(ProcesarFarConIaDto.IdArchivador));
            var idCg = (int)parametros.LeerValor<long>(nameof(ProcesarFarConIaDto.IdCgPropuesto));
            var idTipo = (int)parametros.LeerValor<long>(nameof(ProcesarFarConIaDto.IdTipoFarPropuesto));
            var idProveedor = (int?)parametros.LeerValor<long?>(nameof(ProcesarFarConIaDto.IdProveedor));
            var idCarpeta = (int?)parametros.LeerValor<long?>(nameof(ProcesarFarConIaDto.IdCarpetaSeleccionada));
            var archivador = contexto.SeleccionarPorId<ArchivadorDtm>(idArchivador);
            var otorgado = entorno.Ejecutor.OtorgarAdministrador(contexto);
            contexto.IniciarTraza(nameof(ProcesarFarConIa));

            var idSemaforo = SemaforoDeProcesoSql.PonerSemaforo(entorno.ContextoDelEntorno, enumNegocio.Archivador.IdNegocio(), idArchivador, enumOpercionesDeSemaforo.IPFA, archivador.Referencia).Id;
            try
            {
                TrabajosDeFacturasRec.ProcesarArchivosFar(entorno, archivador, idCg, idTipo, idProveedor, idCarpeta);
                entorno.CrearTraza($"Fin del proceso realizado");
            }
            catch (Exception e)
            {
                entorno.AnotarError(e);
            }
            finally
            {
                SemaforoDeProcesoSql.QuitarSemaforo(entorno.ContextoDelEntorno, idSemaforo);
                if (otorgado) entorno.Ejecutor.AnularAdministrador(contexto, otorgado);
                contexto.CerrarTraza();
            }
        }

        public static TrabajoDeUsuarioDtm SometerImportarZip(ContextoSe contexto, int idArchivador, int idArchivoZip, bool remplazar, bool renombrar, bool eliminarArchivo, bool eliminarCarpeta)
        {
            if (enumNegocio.Archivador.Parametro(enumParametrosDeArchivadores.ARC_Someter_Importar_ZIP, emitirError: false, crearParametro: true, valorPorDefecto: "S").Valor.EsTrue())
            {
                var dll = Assembly.GetExecutingAssembly().GetName().Name;
                var clase = typeof(TrabajosDelSistemaDocumental).FullName;
                var ts = GestorDeTrabajosSometido.CrearObtener(contexto, enumTrabajosDeArchivadores.ImportarZip.Descripcion(), dll, clase, nameof(enumTrabajosDeArchivadores.ImportarZip), comunicarFin: true);
                var parametrosEntrada = new Dictionary<string, object> {
                        { nameof(ImportarZipDto.IdArchivador), idArchivador },
                        { nameof(ImportarZipDto.IdArchivo), idArchivoZip },
                        { nameof(ImportarZipDto.Remplazar), remplazar },
                        { nameof(ImportarZipDto.Renombrar), renombrar },
                        { nameof(ImportarZipDto.EliminarArchivo), eliminarArchivo }
                };
                var datosDeCreacion = new Dictionary<string, object>
                {
                    { nameof(TrabajoDeUsuarioDtm.Parametros), parametrosEntrada.ToJson() },
                    { nameof(TrabajoDeUsuarioDtm.Planificado), DateTime.Now.AddMinutes(-1) }
                };

                var trabajo = GestorDeTrabajosDeUsuario.Crear(contexto, ts, datosDeCreacion);
                var archivo = contexto.SeleccionarPorId<ArchivoDtm>(idArchivoZip);
                contexto.SeleccionarPorId<ArchivadorDtm>(idArchivador).CrearTraza(contexto, "Pendiente de importar zip", $"Se ha sometido el trabajo de importación para el archivo '{archivo.Nombre}'");

                return trabajo;
            }
            else
            {
                var archivador = contexto.SeleccionarPorId<ArchivadorDtm>(idArchivador);
                ImportarZipInterno(contexto, archivador, idArchivoZip, remplazar, renombrar, eliminarArchivo, eliminarCarpeta);
                return null;
            }
        }

        public static void ImportarZip(EntornoDeTrabajo entorno)
        {
            var contexto = entorno.contextoDelProceso;
            Dictionary<string, object> parametros = entorno.TrabajoDeUsuario.Parametros.ToDiccionarioDeParametros();
            var idArchivador = (int)parametros.LeerValor<long>(nameof(ImportarZipDto.IdArchivador));
            var idArchivoZip = (int)parametros.LeerValor<long>(nameof(ImportarZipDto.IdArchivo));
            var remplazar = parametros.LeerValor<bool>(nameof(ImportarZipDto.Remplazar));
            var renombrar = parametros.LeerValor<bool>(nameof(ImportarZipDto.Renombrar));
            var eliminarArchivo = parametros.LeerValor<bool>(nameof(ImportarZipDto.EliminarArchivo));
            var eliminarCarpeta = false;
            var archivador = contexto.SeleccionarPorId<ArchivadorDtm>(idArchivador);

            contexto.IniciarTraza(nameof(enumTrabajosDeArchivadores.ImportarZip));
            var otorgado = entorno.Ejecutor.OtorgarAdministrador(contexto);
            var tran = contexto.IniciarTransaccion();
            try
            {
                ImportarZipInterno(contexto, archivador, idArchivoZip, remplazar, renombrar, eliminarArchivo, eliminarCarpeta);
                entorno.CrearTraza($"Fin del proceso realizado");
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

        public static void ImportarZipInterno(ContextoSe contexto, ArchivadorDtm archivador, int idArchivoZip, bool remplazar, bool renombrar, bool eliminarArchivo, bool eliminarCarpeta)
        {
            var archivoZip = contexto.SeleccionarPorId<ArchivoDtm>(idArchivoZip);
            var idSemaforo = SemaforoDeProcesoSql.PonerSemaforo(contexto, enumNegocio.Archivador.IdNegocio(), archivador.Id, enumOpercionesDeSemaforo.IZIP, archivador.Referencia).Id;
            try
            {
                var rutaDescomprimida = ServidorDocumental.DescomprimirZip(contexto, archivoZip);
                try
                {
                    ServidorDocumental.ImportarDirectorio(contexto, archivador, rutaBase: rutaDescomprimida, remplazar, renombrar, eliminarArchivo, eliminarCarpeta);
                    archivador.CrearTraza(contexto, "Zip Importado", $"remplazar: {remplazar}, renombrar: {renombrar}, eliminarArchivo: {eliminarArchivo}, eliminarCarpeta: {eliminarCarpeta}");
                }
                finally
                {
                    Directory.Delete(rutaDescomprimida, recursive: true);
                }
            }
            finally
            {
                SemaforoDeProcesoSql.QuitarSemaforo(contexto, idSemaforo);
            }
        }

        public static ArchivadorDtm CrearArchivadorZip(EntornoDeTrabajo entorno, string nombre)
        {
            var archivador = new ArchivadorDtm
            {
                Nombre = nombre,
                Descripcion = $"Fichero solicitado por {entorno.Sometedor.Expresion}",
                IdCg = ExtensionCentrosGestores.Cfg_CG_De_Documentacion(entorno.contextoDelProceso).Id,
                IdTipo = GestorDeTiposDeArchivadores.Cfg_Id_Tipo_De_Archivador_Zip(entorno.contextoDelProceso),
            }
            .InsertarComoAdministrador(entorno.contextoDelProceso, new Dictionary<string, object> { { ltrParametrosNeg.CrearPermisosDelElemento, true } }, accionEjecutada: nameof(ltrDeUnArchivador.Accion_GenerarZip));

            GestorDePemisosDelElemento.OtorgarPermisoDe(entorno.contextoDelProceso, enumNegocio.Archivador, archivador.Id, new List<int> { entorno.Sometedor.Id }, enumModoDeAccesoDeDatos.Consultor);
            return archivador;
        }

        public static void AnexarZip(this ArchivadorDtm archivador, ContextoSe contexto, string ficheroZip)
        {

            RomperZipEnPartes(ficheroZip);

            string[] partes = Directory.GetFiles(Path.GetDirectoryName(ficheroZip), Path.GetFileNameWithoutExtension(ficheroZip) + "_*.zip");

            if (partes.Length > 0)
            {
                foreach (string partFilePath in partes)
                {
                    var archivo = archivador.AnexarArchivo(contexto, partFilePath);
                    ServidorDocumental.BloquearArchivo(contexto, enumNegocio.Archivador.IdNegocio(), archivador.Id, archivo.Id, ltrDeUnArchivo.ZipGenerado);
                }
            }
            else
            {
                var archivoZip = archivador.AnexarArchivo(contexto, ficheroZip);
                ServidorDocumental.BloquearArchivo(contexto, enumNegocio.Archivador.IdNegocio(), archivador.Id, archivoZip.Id, ltrDeUnArchivo.ZipGenerado);
            }
        }

        public static void CrearZipConArchivos(ContextoSe contexto, List<int> idsDeArchivos, string ficheroZip)
        {
            using (ZipArchive zip = ZipFile.Open(ficheroZip, ZipArchiveMode.Create))
            {
                foreach (var idDeArchivo in idsDeArchivos)
                {
                    string incluir = null;
                    var archivo = contexto.SeleccionarPorId<ArchivoDtm>(idDeArchivo);
                    try
                    {
                        incluir = ApiDeArchivos.DescargarArchivo(archivo, contexto, solicitadoPorLaCola: true, erroSiNoEstaEnLaruta: true);
                        zip.CreateEntryFromFile(incluir, Path.GetFileName(incluir));
                    }
                    finally
                    {
                        if (incluir != null && File.Exists(incluir))
                            File.Delete(incluir);
                        if (archivo != null)
                        {
                            var ruta = Path.Combine(Path.GetDirectoryName(incluir), $"{archivo.Id}.{ApiDeArchivos.ExtensionSe}");
                            if (File.Exists(ruta))
                                File.Delete(ruta);
                        }
                    }
                }
            }
        }

        public static void EnviarCorreoConElArchivador(EntornoDeTrabajo entorno, ArchivadorDtm archivador)
        {
            TipoDtoElmento elementoAdjunto = new TipoDtoElmento
            {
                TipoDto = typeof(ArchivadorDto).FullName,
                IdElemento = archivador.Id,
                Referencia = archivador.Expresion
            };
            var correo = entorno.contextoDelProceso.SeleccionarPorId<UsuarioDtm>(entorno.TrabajoDeUsuario.IdEjecutor).eMail;
            GestorDeCorreos.CrearCorreoPara(entorno.contextoDelProceso
                , new List<string> { correo }
                , "Zip generado"
                , "Se le adjunta el archivador con el Zip generado"
                , new List<TipoDtoElmento> { elementoAdjunto }
                , new List<string>()
                );
        }

        private static void RomperZipEnPartes(string ficheroZip)
        {
            long tamanoMaximo = CacheDeVariable.Cfg_Tamano_Maximo_Zip;
            FileInfo infoDelZip = new FileInfo(ficheroZip);
            if (infoDelZip.Length > tamanoMaximo)
            {
                int contador = 1;
                using (FileStream streamZip = new FileStream(ficheroZip, FileMode.Open, FileAccess.Read))
                {
                    while (streamZip.Position < streamZip.Length)
                    {
                        string parte = Path.Combine(Path.GetDirectoryName(ficheroZip), Path.GetFileNameWithoutExtension(ficheroZip) + Simbolos.Subrrallado + contador.ToString().PadLeft(3, '0') + ".zip");
                        using (ZipArchive partArchive = ZipFile.Open(parte, ZipArchiveMode.Create))
                        {
                            byte[] buffer = new byte[4096];
                            int bytesRead;
                            long bytesWritten = 0;
                            while ((bytesRead = streamZip.Read(buffer, 0, buffer.Length)) > 0 && bytesWritten + bytesRead <= tamanoMaximo)
                            {
                                ZipArchiveEntry entry = partArchive.CreateEntry(Path.GetFileName(ficheroZip));
                                using (Stream entryStream = entry.Open())
                                {
                                    entryStream.Write(buffer, 0, bytesRead);
                                    entryStream.Close(); // Cerrar la entrada actual
                                }
                                bytesWritten += bytesRead;
                            }
                        }
                        contador++;
                    }
                }
            }
        }

    }
}
