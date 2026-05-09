using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Negocio;
using GestoresDeNegocio.Seguridad;
using GestoresDeNegocio.SistemaDocumental;
using GestoresDeNegocio.Terceros;
using GestoresDeNegocio.TrabajosSometidos;
using ModeloDeDto.Negocio;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.TrabajosSometidos;
using Utilidades;

namespace GestoresDeNegocio.Entorno
{
    public class enumTrabajosSometidos
    {
        public const string ExportarExcell = "Exportar a excel";
        public const string SincronizarArchivador = "Sincronizar archivador";
    }

    public class TrabajosDeEntorno
    {
        private static string _trabajoDeBorradoDeTrazas = "Borrar trazas";
        private static string _trabajoDeEliminarCorreos = "Eliminar correos enviados";
        private static string _trabajoDeVaciarCache = "Vacia la cache de memoria y la del árbol de menú";
        private static string _trabajoDeSincronizarArchivadores = "Sincronizar archivadores";
        private static string _trabajoDeGenerarSeguridadParaElUsuario = "Generar seguridad para el usuario";
        private static string _trabajoDeGenerarSeguridadParaLosUsuarios = "Generar seguridad para la lista de usuarios";
        private static string _TrabajoDeGenerarSeguridad = "Generar seguridad";

        public static TrabajoDeUsuarioDtm SometerSincronizarArchivadores(ContextoSe contexto)
        {
            var dll = Assembly.GetExecutingAssembly().GetName().Name;
            var clase = typeof(TrabajosDeEntorno).FullName;
            var ts = GestorDeTrabajosSometido.CrearObtener(contexto, _trabajoDeSincronizarArchivadores, dll, clase, nameof(SincronizarArchivadores), comunicarFin: false);
            var datosDeCreacion = new Dictionary<string, object> { { nameof(TrabajoDeUsuarioDtm.Planificado), DateTime.Now.AddDays(1) } };
            return GestorDeTrabajosDeUsuario.CrearSiNoEstaPendiente(contexto, ts, datosDeCreacion);
        }

        public static void SincronizarArchivadores(EntornoDeTrabajo entorno)
        {
            entorno.CrearTraza("Inicio del proceso");
            var gestor = GestorDeArchivadores.Gestor(entorno.ContextoDelEntorno, entorno.ContextoDelEntorno.Mapeador);
            var filtros = new List<ClausulaDeFiltrado>();
            filtros.Add(new ClausulaDeFiltrado { Clausula = nameof(ArchivadorDtm.SincronizarCon), Criterio = enumCriteriosDeFiltrado.noEsNulo });
            var archivadores = gestor.LeerRegistros(0, -1, filtros, null, new ParametrosDeNegocio(enumTipoOperacion.LeerSinBloqueo, true));
            var sincronizado = 0;
            var errores = 0;
            var trazaDtm = entorno.CrearTraza("Archivador a sincronizar");
            var contexto = entorno.contextoDelProceso;
            foreach (var archivador in archivadores)
            {
                var tran = contexto.IniciarTransaccion();
                try
                {
                    entorno.ActualizarTraza(trazaDtm, $"Sincronizando el archivador {archivador.Nombre}");
                    GestorDeArchivadores.SincronizarArchivador(entorno.contextoDelProceso, archivador);
                    contexto.Commit(tran);
                    sincronizado++;
                }
                catch (Exception exc)
                {
                    errores++;
                    entorno.AnotarError(exc);
                    contexto.Rollback(tran);
                }
            }
            entorno.CrearTraza($"Se han sincronizado {sincronizado} y {errores} no se han podido sincronizar correctamente");
        }

        public static void SometerBorrarTrazas(ContextoSe contexto)
        {
            var dll = Assembly.GetExecutingAssembly().GetName().Name;
            var clase = typeof(TrabajosDeEntorno).FullName;
            var ts = GestorDeTrabajosSometido.CrearObtener(contexto, _trabajoDeBorradoDeTrazas, dll, clase, nameof(BorrarTrazas), comunicarFin: false);
            GestorDeTrabajosDeUsuario.CrearSiNoEstaPendiente(contexto, ts, new Dictionary<string, object> { { nameof(TrabajoDeUsuarioDtm.Planificado), DateTime.Now.AddDays(1) } });
        }
        public static void BorrarTrazas(EntornoDeTrabajo entorno)
        {
            entorno.CrearTraza("Inicio del proceso");

            string[] directorios = new string[]
            {
                CacheDeVariable.Cfg_RutaDeDescarga,
                CacheDeVariable.CFG_Ruta_Raiz_De_Excepciones,
                CacheDeVariable.CFG_Ruta_Ficheros_De_Zip,
                CacheDeVariable.CFG_Ruta_Ficheros_De_Debug,
                CacheDeVariable.CFG_Ruta_Ficheros_A_Firmar,
                CacheDeVariable.CFG_Ruta_Ficheros_De_Certificados,
                GestorDeVariables.RutaDeExportaciones,
                enumRutas.RutaDeDescarga
            };

            foreach (var directorio in directorios)
            {
                if (!Directory.Exists(directorio))
                    continue;

                var ficheros = Directory.GetFiles(directorio, "*", SearchOption.AllDirectories);
                BorrarFicherosDelDirectorio(entorno, directorio, ficheros, directorio == enumRutas.RutaDeDescarga ? -1 : -7);
                BorrarSubdirectoriosVacios(entorno, directorio);
            }
        }

        private static void BorrarSubdirectoriosVacios(EntornoDeTrabajo entorno, string directorio)
        {
            foreach (var dir in Directory.GetDirectories(directorio, "*", SearchOption.AllDirectories).Reverse())
            {
                if (Directory.GetFiles(dir).Length == 0 && Directory.GetDirectories(dir).Length == 0)
                {
                    try
                    {
                        Directory.Delete(dir, false);
                        entorno.CrearTraza($"Directorio vacío eliminado: {dir}");
                    }
                    catch (Exception e)
                    {
                        entorno.AnotarError($"Error al eliminar el directorio vacío: {dir}", e);
                    }
                }
            }
        }


        private static void BorrarFicherosDelDirectorio(EntornoDeTrabajo entorno, string directorio, string[] ficheros, int diasAntesDeHoy)
        {
            int borrados = 0, errores = 0;
            var trazaDtm = entorno.CrearTraza($"Procesando el directorio: '{directorio}'");
            var fechaLimite = DateTime.Now.Date.AddDays(diasAntesDeHoy);

            foreach (var fichero in ficheros)
            {
                if (File.Exists(fichero))
                {
                    FileInfo fileInfo = new FileInfo(fichero);
                    DateTime creationDate = fileInfo.CreationTime;
                    DateTime modificationDate = fileInfo.LastWriteTime;

                    // Comprobar si el archivo es más antiguo que 7 días
                    if (creationDate < fechaLimite && modificationDate < fechaLimite)
                    {
                        try
                        {
                            File.Delete(fichero);
                            borrados++;
                        }
                        catch (Exception e)
                        {
                            entorno.AnotarError($"Error al borrar el fichero {Path.GetFileName(fichero)}", e);
                            errores++;
                        }
                    }
                }
            }

            entorno.CrearTraza($"Se han borrado {borrados} y {errores} no se han podido borrar");
        }


        public static void SometerEliminarCorreos(ContextoSe contexto)
        {
            var dll = Assembly.GetExecutingAssembly().GetName().Name;
            var clase = typeof(TrabajosDeEntorno).FullName;
            var ts = GestorDeTrabajosSometido.CrearObtener(contexto, _trabajoDeEliminarCorreos, dll, clase, nameof(EliminarCorreos), comunicarFin: false);
            GestorDeTrabajosDeUsuario.CrearSiNoEstaPendiente(contexto, ts, new Dictionary<string, object> { { nameof(TrabajoDeUsuarioDtm.Planificado), DateTime.Now.AddDays(1) } });
        }

        public static void EliminarCorreos(EntornoDeTrabajo entorno)
        {
            entorno.CrearTraza("Inicio del proceso");

            var gestorDeCorreos = GestorDeCorreos.Gestor(entorno.contextoDelProceso, entorno.contextoDelProceso.Mapeador);
            var correos = gestorDeCorreos.EliminarCorreos();

            entorno.CrearTraza($"Correos eliminados: {correos}");
        }
        public static TrabajoDeUsuarioDtm SometerVaciarCache(ContextoSe contexto)
        {
            var dll = Assembly.GetExecutingAssembly().GetName().Name;
            var clase = typeof(TrabajosDeEntorno).FullName;
            var ts = GestorDeTrabajosSometido.CrearObtener(contexto, _trabajoDeVaciarCache, dll, clase, nameof(VaciarCache), comunicarFin: false);
            var datosDeCreacion = new Dictionary<string, object> { { nameof(TrabajoDeUsuarioDtm.Planificado), DateTime.Now.AddHours(8) } };
            return GestorDeTrabajosDeUsuario.CrearSiNoEstaPendiente(contexto, ts, datosDeCreacion);
        }

        public static void VaciarCache(EntornoDeTrabajo entorno) => EliminarCache(entorno);

        public static TrabajoDeUsuarioDtm SometerGenerarSeguridadParaLosUsuario(ContextoSe contexto, List<int> idsUsuarios)
        {
            var dll = Assembly.GetExecutingAssembly().GetName().Name;
            var clase = typeof(TrabajosDeEntorno).FullName;
            var ts = GestorDeTrabajosSometido.CrearObtener(contexto, _trabajoDeGenerarSeguridadParaLosUsuarios, dll, clase, nameof(GenerarSeguridadParaLosUsuario), comunicarFin: false);
            var listaUsuarios = new List<Parametro> { new Parametro(nameof(ltrDeUnUsuario.Usuarios), idsUsuarios) }.ToJson();

            var pendientes = GestorDeTrabajosDeUsuario.Pendientes(contexto, ts);
            foreach (var pendiente in pendientes)
            {
                if (listaUsuarios == pendiente.Parametros) return pendiente;
            }

            var datosDeCreacion = new Dictionary<string, object>
            {
                { nameof(TrabajoDeUsuarioDtm.Parametros), listaUsuarios }
            };
            return GestorDeTrabajosDeUsuario.Crear(contexto, ts, datosDeCreacion);
        }

        public static TrabajoDeUsuarioDtm SometerGenerarSeguridadParaElUsuario(ContextoSe contexto, int idUsuario)
        {
            var dll = Assembly.GetExecutingAssembly().GetName().Name;
            var clase = typeof(TrabajosDeEntorno).FullName;
            var ts = GestorDeTrabajosSometido.CrearObtener(contexto, _trabajoDeGenerarSeguridadParaElUsuario, dll, clase, nameof(GenerarSeguridadParaElUsuario), comunicarFin: false);

            var usuario = new List<Parametro> { new Parametro(nameof(IRegistro.Id), idUsuario) }.ToJson();
            var pendientes = GestorDeTrabajosDeUsuario.Pendientes(contexto, ts);
            foreach (var pendiente in pendientes)
            {
                if (usuario == pendiente.Parametros) return pendiente;
            }

            var datosDeCreacion = new Dictionary<string, object>
            {
                { nameof(TrabajoDeUsuarioDtm.Parametros), usuario }
            };
            return GestorDeTrabajosDeUsuario.Crear(contexto, ts, datosDeCreacion);
        }

        public static TrabajoDeUsuarioDtm SometerGenerarSeguridad(ContextoSe contexto, DateTime? momento = null)
        {
            var dll = Assembly.GetExecutingAssembly().GetName().Name;
            var clase = typeof(TrabajosDeEntorno).FullName;
            var ts = GestorDeTrabajosSometido.CrearObtener(contexto, _TrabajoDeGenerarSeguridad, dll, clase, nameof(GenerarSeguridad), comunicarFin: false);
            var datosDeCreacion = new Dictionary<string, object> { { nameof(TrabajoDeUsuarioDtm.Planificado), momento ?? DateTime.Now.AddDays(1) } };
            return GestorDeTrabajosDeUsuario.CrearSiNoEstaPendiente(contexto, ts, datosDeCreacion);
        }

        public static void GenerarSeguridad(EntornoDeTrabajo entorno)
        {
            var traza = entorno.CrearTraza("Inicio del proceso");
            try
            {
                traza = entorno.CrearTraza("Procesar seguridad de tipos");
                GestorDeTiposDeElemento<ContextoSe, TipoDeElementoDtm, TipoDeElementoDto>.GenerarSeguridad(entorno.contextoDelProceso, (mensaje) => traza.Actualizar(entorno.ContextoDelEntorno, mensaje));

                traza = entorno.CrearTraza("Procesar seguridad por Cg");
                GestorDeCentrosGestores.GenerarSeguridad(entorno.contextoDelProceso, (mensaje) => traza.Actualizar(entorno.ContextoDelEntorno, mensaje));

                traza = entorno.CrearTraza("Procesar seguridad por negocio");
                GestorDeNegocios.GenerarSeguridad(entorno.contextoDelProceso, (mensaje) => traza.Actualizar(entorno.ContextoDelEntorno, mensaje));

                traza = entorno.CrearTraza("Procesar seguridad por elementos");
                GestorDePemisosDelElemento.GenerarSeguridad(entorno.contextoDelProceso, (mensaje) => traza.Actualizar(entorno.ContextoDelEntorno, mensaje));

                traza = entorno.CrearTraza("Procesar seguridad por estado");
                GestorDeEstados.GenerarSeguridad(entorno.contextoDelProceso, (mensaje) => traza.Actualizar(entorno.ContextoDelEntorno, mensaje));

                traza = entorno.CrearTraza("Procesar seguridad por transicion");
                GestorDeTransiciones.GenerarSeguridad(entorno.contextoDelProceso, (mensaje) => traza.Actualizar(entorno.ContextoDelEntorno, mensaje));
            }
            catch (Exception e)
            {
                entorno.AnotarError($"Error al generar la seguridad", e);
            }
            finally
            {
                entorno.CrearTraza($"fin del proceso");
                EliminarCache(entorno);
            }
        }

        public static void EliminarCache(EntornoDeTrabajo entorno)
        {
            ServicioDeCaches.EliminarTodas();
            GestorDeArbolDeMenu.LimpiarCacheDeArbolDeMenu();
        }

        public static void GenerarSeguridadParaLosUsuario(EntornoDeTrabajo entorno)
        {
            var contexto = entorno.contextoDelProceso;
            Dictionary<string, object> parametros = entorno.TrabajoDeUsuario.Parametros.ToDiccionarioDeParametros();
            var idsDeUsuario = parametros.LeerValor<List<long>>(ltrDeUnUsuario.Usuarios);
            var traza = entorno.CrearTraza("Inicio del proceso");
            var otorgado = entorno.Ejecutor.OtorgarAdministrador(contexto);
            try
            {
                foreach (var idUsuario in idsDeUsuario)
                {
                    var tran = contexto.IniciarTransaccion();
                    try
                    {
                        var usuario = contexto.SeleccionarPorId<UsuarioDtm>(Convert.ToInt32(idUsuario));
                        GenerarSeguridadPorUsuario(entorno, Convert.ToInt32(idUsuario), usuario, traza);
                        contexto.Commit(tran);
                    }
                    catch (Exception e)
                    {
                        entorno.AnotarError($"Error al generar la seguridad", e);
                        contexto.Rollback(tran);
                        if (entorno.ContextoDelEntorno.Test)
                            throw;
                    }
                }
            }
            finally
            {
                entorno.Ejecutor.AnularAdministrador(contexto, otorgado);
                entorno.CrearTraza($"fin del proceso");
                EliminarCache(entorno);
            }
        }

        public static void GenerarSeguridadParaElUsuario(EntornoDeTrabajo entorno)
        {
            Dictionary<string, object> parametros = entorno.TrabajoDeUsuario.Parametros.ToDiccionarioDeParametros();
            var contexto = entorno.contextoDelProceso;
            var idUsuario = Convert.ToInt32(parametros.LeerValor<long>(nameof(IRegistro.Id)));
            var usuario = contexto.SeleccionarPorId<UsuarioDtm>(idUsuario);
            var traza = entorno.CrearTraza("Inicio del proceso");
            var otorgado = entorno.Ejecutor.OtorgarAdministrador(contexto);
            var tran = contexto.IniciarTransaccion();
            try
            {
                GenerarSeguridadPorUsuario(entorno, idUsuario, usuario, traza);
                contexto.Commit(tran);
            }
            catch (Exception e)
            {
                entorno.AnotarError($"Error al generar la seguridad", e);
                contexto.Rollback(tran);
                if (entorno.ContextoDelEntorno.Test)
                    throw;
            }
            finally
            {
                entorno.Ejecutor.AnularAdministrador(contexto, otorgado);
                entorno.CrearTraza($"fin del proceso");
                GestorDeArbolDeMenu.LimpiarCacheDeArbolDeMenu();
            }
        }

        private static void GenerarSeguridadPorUsuario(EntornoDeTrabajo entorno, int idUsuario, UsuarioDtm usuario, TrazaDeUnTrabajoDtm traza)
        {
            traza = entorno.CrearTraza($"Procesar seguridad de tipos para el usuario {usuario.Expresion}");
            GestorDeTiposDeElemento<ContextoSe, TipoDeElementoDtm, TipoDeElementoDto>.GenerarSeguridadParaElUsuario(entorno.contextoDelProceso, idUsuario, (mensaje) => traza.Actualizar(entorno.ContextoDelEntorno, mensaje));

            traza = entorno.CrearTraza("Procesar seguridad por Cg");
            GestorDeCentrosGestores.GenerarSeguridadParaElUsuario(entorno.contextoDelProceso, idUsuario, (mensaje) => traza.Actualizar(entorno.ContextoDelEntorno, mensaje));

            traza = entorno.CrearTraza("Procesar seguridad por Negocio");
            GestorDeNegocios.GenerarSeguridadParaElUsuario(entorno.contextoDelProceso, idUsuario, (mensaje) => traza.Actualizar(entorno.ContextoDelEntorno, mensaje));

            traza = entorno.CrearTraza("Procesar seguridad por elementos");
            GestorDePemisosDelElemento.GenerarSeguridadParaElUsuario(entorno.contextoDelProceso, idUsuario, (mensaje) => traza.Actualizar(entorno.ContextoDelEntorno, mensaje));

            traza = entorno.CrearTraza("Procesar seguridad por estado");
            GestorDeEstados.GenerarSeguridadParaElUsuario(entorno.contextoDelProceso, idUsuario, (mensaje) => traza.Actualizar(entorno.ContextoDelEntorno, mensaje));

            traza = entorno.CrearTraza("Procesar seguridad por transición");
            GestorDeTransiciones.GenerarSeguridadParaElUsuario(entorno.contextoDelProceso, idUsuario, (mensaje) => traza.Actualizar(entorno.ContextoDelEntorno, mensaje));


            ServicioDeCaches.EliminarTodas();
        }
    }
}
