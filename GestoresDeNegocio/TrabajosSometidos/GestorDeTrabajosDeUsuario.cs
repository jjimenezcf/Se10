using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.TrabajosSometidos;
using ModeloDeDto.TrabajosSometidos;
using System;
using Utilidades;
using Gestor.Errores;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ModeloDeDto;
using System.Threading.Tasks;
using Dapper;
using GestoresDeNegocio.Entorno;
using Microsoft.Data.SqlClient;
using System.Threading;

namespace GestoresDeNegocio.TrabajosSometidos
{

    public class EnumParametroTu : enumParametro
    {
        public static string terminando = nameof(terminando);
    }

    public class ResultadoDelProceso
    {
        public bool EsOk { get; }
        public string Mensaje { get; }

        public ResultadoDelProceso(bool resultado, string mensaje)
        {
            EsOk = resultado;
            Mensaje = mensaje;
        }
    }


    public class EntornoDeTrabajo
    {
        public GestorDeTrabajosDeUsuario GestorDelEntorno { get; }
        public TrabajoDeUsuarioDtm TrabajoDeUsuario { get; }
        public ContextoSe contextoDelProceso { get; set; }
        public ContextoSe ContextoDelEntorno => GestorDelEntorno.Contexto;
        public bool Test { get; set; } = false;

        public bool HayErrores
        {
            get
            {
                var gestor = GestorDeErroresDeUnTrabajo.Gestor(ContextoDelEntorno, ContextoDelEntorno.Mapeador);
                var filtro = new ClausulaDeFiltrado { Clausula = nameof(ErrorDeUnTrabajoDtm.IdTrabajoDeUsuario), Criterio = enumCriteriosDeFiltrado.igual, Valor = TrabajoDeUsuario.Id.ToString() };
                return gestor.LeerRegistros(0, 1, new List<ClausulaDeFiltrado> { filtro }).Count > 0;
            }
        }


        public DateTime? FechaDeResometimiento { get; set; }

        public bool ProcesoIniciadoPorLaCola { get; set; }
        public UsuarioDtm Sometedor { get; }
        public UsuarioDtm Ejecutor { get; }
        public TrabajoSometidoDtm TrabajoSometido { get; }

        public List<TipoDtoElmento> Enlaces = new List<TipoDtoElmento>();

        public EntornoDeTrabajo(GestorDeTrabajosDeUsuario gestorDelEntorno, TrabajoDeUsuarioDtm trabajoUsuario)
        {
            TrabajoDeUsuario = trabajoUsuario;
            gestorDelEntorno.Contexto.EsElContextosDeUnEntorno = true;
            Test = gestorDelEntorno.Contexto.Test;
            TrabajoSometido = gestorDelEntorno.Contexto.Set<TrabajoSometidoDtm>().First(p => p.Id.Equals(trabajoUsuario.IdTrabajo));
            Sometedor = gestorDelEntorno.Contexto.Set<UsuarioDtm>().First(p => p.Id.Equals(trabajoUsuario.IdSometedor));
            Ejecutor = gestorDelEntorno.Contexto.Set<UsuarioDtm>().First(p => p.Id.Equals(trabajoUsuario.IdEjecutor));
            gestorDelEntorno.Contexto.NombreDelTrabajo = TrabajoSometido.Nombre;
            GestorDelEntorno = gestorDelEntorno;
        }

        public TrazaDeUnTrabajoDtm CrearTraza(string traza)
        {
            return GestorDeTrazasDeUnTrabajo.AnotarTraza(ContextoDelEntorno, TrabajoDeUsuario, traza);
        }

        public TrazaDeUnTrabajoDtm ActualizarTraza(TrazaDeUnTrabajoDtm trazaDtm, string traza)
        {
            trazaDtm.Traza = traza;
            return GestorDeTrazasDeUnTrabajo.ActualizarTraza(ContextoDelEntorno, trazaDtm);
        }

        public void AnotarError(Exception e, bool mostrarPila = false)
        {
            var emitidoPorMi = e.Data.Contains(GestorDeErrores.Datos.EmitidoPorMi) && (bool)e.Data[GestorDeErrores.Datos.EmitidoPorMi];

            if (mostrarPila || !emitidoPorMi)
                GestorDeErroresDeUnTrabajo.AnotarError(ContextoDelEntorno, TrabajoDeUsuario, e);
            else
                GestorDeErroresDeUnTrabajo.CrearError(ContextoDelEntorno, TrabajoDeUsuario, e.Message, e.MensajeCompleto(mostrarPila: false));
        }

        public void AnotarError(string error, Exception e, bool mostrarPila = false)
        {
            var emitidoPorMi = e.Data.Contains(GestorDeErrores.Datos.EmitidoPorMi) && (bool)e.Data[GestorDeErrores.Datos.EmitidoPorMi];

            if (mostrarPila || !emitidoPorMi)
                GestorDeErroresDeUnTrabajo.CrearError(ContextoDelEntorno, TrabajoDeUsuario, error, GestorDeErrores.Detalle(e));
            else
                GestorDeErroresDeUnTrabajo.CrearError(ContextoDelEntorno, TrabajoDeUsuario, error, e.Message);

        }

        public void AnotarError(string error, string detalleDeError)
        {
            GestorDeErroresDeUnTrabajo.CrearError(ContextoDelEntorno, TrabajoDeUsuario, error, detalleDeError);
        }

        public bool IniciarTransaccion()
        {
            return GestorDelEntorno.IniciarTransaccion();
        }
        public void RollBack(bool transaccion)
        {
            GestorDelEntorno.Rollback(transaccion);
        }
        public void Commit(bool transaccion)
        {
            GestorDelEntorno.Commit(transaccion);
        }

        public void PonerSemaforo()
        {
            try
            {
                GestorDeSemaforoDeTrabajos.PonerSemaforo(TrabajoDeUsuario, ContextoDelEntorno.DatosDeConexion.Login);
                CrearTraza($"Trabajo iniciado por el usuario {ContextoDelEntorno.DatosDeConexion.Login}");
            }
            catch (Exception exc)
            {
                if (exc.Message.Contains($"AK_{Tablas.SEMAFORO}_{ICampos.ID_TRABAJO}"))
                    QuitarSemaforo("Error al poner el semaforo");
                throw;
            }
        }

        public void QuitarSemaforo(string traza)
        {
            GestorDeSemaforoDeTrabajos.QuitarSemaforo(TrabajoDeUsuario);
            CrearTraza(traza);
        }

        internal void ComunicarError(Exception e)
        {
            AnotarError(e);
            if (TrabajoSometido.ComunicarError)
            {
                GestorDeCorreos.CrearCorreoPara(ContextoDelEntorno
                    , new List<string> { Sometedor.eMail }
                    , $"Error al ejecutar el trabajo {TrabajoSometido.Nombre}"
                    , $"BD: '{ContextoDelEntorno.DatosDeConexion.ServidorBd}.{ContextoDelEntorno.DatosDeConexion.Bd}'." + Environment.NewLine +
                      $"Servidor web: '{ContextoDelEntorno.DatosDeConexion.ServidorWeb}'." + Environment.NewLine +
                      $"Usuario: '{ContextoDelEntorno.DatosDeConexion.Login}'." + Environment.NewLine +
                      $"Acceda al mantenimiento de trabajos de usuario para gestionar los errores" + Environment.NewLine +
                      e.MensajeCompleto(mostrarPila: true)
                    , new List<TipoDtoElmento> { new TipoDtoElmento { TipoDto = typeof(TrabajoDeUsuarioDto).FullName, IdElemento = TrabajoDeUsuario.Id, Referencia = TrabajoSometido.Nombre } }
                    , null);
            }
        }

        internal void ComunicarFinalizacion()
        {
            if (TrabajoSometido.ComunicarFin)
            {
                Enlaces.Add(new TipoDtoElmento
                {
                    TipoDto = typeof(TrabajoDeUsuarioDto).FullName,
                    IdElemento = TrabajoDeUsuario.Id,
                    Referencia = TrabajoSometido.Nombre
                }
                           );

                GestorDeCorreos.CrearCorreoPara(ContextoDelEntorno
                    , new List<string> { Sometedor.eMail }
                    , $"Trabajo {TrabajoSometido.Nombre} finalizado{(TrabajoDeUsuario.Estado == enumEstadosDeUnTrabajo.conErrores.ToDtm() ? " con errores" : "")}"
                    , $"El trabajo {TrabajoSometido.Nombre} de fecha {TrabajoDeUsuario.Encolado} ha finalizado, acceda a la traza{(TrabajoDeUsuario.Estado == enumEstadosDeUnTrabajo.conErrores.ToDtm() ? " y a los errores" : "")} para ver el resultado"
                    , Enlaces
                    , null);
            }
        }
    }

    public static class ExtensorDeTrabajosDeUsuario
    {
        public static Task ProcesarCola(this GestorDeTrabajosDeUsuario gestorDelEntorno, UsuarioDtm usuario)
        {
            if (!CacheDeVariable.Cola_Activa) Task.FromResult(new ResultadoDelProceso(true, "Cola no activa"));

            gestorDelEntorno.Contexto.AsignarUsuario(usuario);
            bool trazar = CacheDeVariable.Cola_Trazar;
            var trabajosPorEjecutar = LeerTrabajoPendiente(trazar);
            gestorDelEntorno.Contexto.IniciarTraza("Ejecutar cola", debugar: trazar);
            try
            {
                while (trabajosPorEjecutar.Count == 1)
                {
                    if (trazar) gestorDelEntorno.Contexto.IniciarTraza(trabajosPorEjecutar[0].Nombre);
                    try
                    {
                        GestorDeTrabajosDeUsuario.Iniciar(gestorDelEntorno.Contexto, trabajosPorEjecutar[0].Id, iniciadoPorLaCola: true);
                        if (trazar)
                            gestorDelEntorno.Contexto.CerrarTraza($"Trabajo '{trabajosPorEjecutar[0].Nombre}' con Id '{trabajosPorEjecutar[0].Id}' finalizado correctamente");
                    }
                    catch (Exception exc)
                    {
                        if (trazar)
                        {
                            gestorDelEntorno.Contexto.AnotarExcepcion(exc);
                            gestorDelEntorno.Contexto.CerrarTraza($"Trabajo '{trabajosPorEjecutar[0].Nombre}' con Id '{trabajosPorEjecutar[0].Id}' finalizado con errores");
                        }
                    }
                    finally
                    {
                        trabajosPorEjecutar = LeerTrabajoPendiente(trazar);
                    }
                }
                gestorDelEntorno.Contexto.AnotarTraza("Ningun trabajo leido tras ejecutar", TrabajosDeUsuarioSql.LeerTrabajoPendiente);
            }
            catch (Exception e)
            {
                gestorDelEntorno.Contexto.AnotarExcepcion(e);
                throw;
            }
            finally
            {
                gestorDelEntorno.Contexto.CerrarTraza();
                var gestorDeCorreos = GestorDeCorreos.Gestor(gestorDelEntorno.Contexto, gestorDelEntorno.Contexto.Mapeador);
                gestorDeCorreos.EnviarCorreoPendientes();
                TrabajosDeAgenda.NotificacionesDeAgenda(gestorDelEntorno.Contexto);
                TrabajosDeEntorno.SometerBorrarTrazas(gestorDelEntorno.Contexto);
                TrabajosDeEntorno.SometerSincronizarArchivadores(gestorDelEntorno.Contexto);
                TrabajosDeEntorno.SometerGenerarSeguridad(gestorDelEntorno.Contexto);
                TrabajosDeEntorno.SometerVaciarCache(gestorDelEntorno.Contexto);
                TrabajosDeEntorno.SometerEliminarCorreos(gestorDelEntorno.Contexto);
            }

            return Task.FromResult(new ResultadoDelProceso(true, ""));
        }

        private static List<TrabajoDeUsuarioDapper> LeerTrabajoPendiente(bool trazar)
        {
            var consulta = new ConsultaSql<TrabajoDeUsuarioDapper>(TrabajosDeUsuarioSql.LeerTrabajoPendiente, hayQueDebugar: trazar, fichero: nameof(LeerTrabajoPendiente));
            var trabajos = consulta.LanzarConsulta(new DynamicParameters(null));
            return trabajos;
        }

        public static void EjecutarTrabajo(this ContextoSe contexto, string nombre)
        {
            var trabajo = new TrabajoDeUsuarioDtm();
            trabajo.IdTrabajo = contexto.SeleccionarPorPropiedad<TrabajoSometidoDtm>(nameof(TrabajoSometidoDtm.Nombre), nombre).Id;
            trabajo.IdEjecutor = contexto.DatosDeConexion.IdUsuario;
            trabajo.IdSometedor = contexto.DatosDeConexion.IdUsuario;
            trabajo.Estado = enumEstadosDeUnTrabajo.Pendiente.ToDtm();
            trabajo.Planificado = DateTime.Now;
            trabajo = trabajo.Insertar(contexto);
            GestorDeTrabajosDeUsuario.Iniciar(contexto, trabajo.Id, false);
        }

        public static void EjecutarTrabajo(this TrabajoDeUsuarioDtm trabajo, ContextoSe contexto)
        {
            GestorDeTrabajosDeUsuario.Iniciar(contexto, trabajo.Id, false);
        }

        public static TrazaDeUnTrabajoDtm Actualizar(this TrazaDeUnTrabajoDtm trazaDtm, ContextoSe contexto)
        {
            return GestorDeTrazasDeUnTrabajo.ActualizarTraza(contexto, trazaDtm);
        }

        public static TrazaDeUnTrabajoDtm Actualizar(this TrazaDeUnTrabajoDtm trazaDtm, ContextoSe contexto, string mensaje)
        {
            trazaDtm.Traza = mensaje;
            return trazaDtm.Actualizar(contexto);
        }

        public static void LanzarProceso(this EntornoDeTrabajo entorno, Action operation, int maxAttempts = 3, int delaySeconds = 60)
        {
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    operation();
                    return; // Si la operación es exitosa, salimos del método
                }
                catch (SqlException ex) when (ex.Number == 3930) // Código de error para "New transaction is not allowed..."
                {
                    if (attempt == maxAttempts)
                    {
                        throw; // Si es el último intento, lanzar la excepción
                    }

                    entorno.CrearTraza($"Intento {attempt} fallido. Esperando {delaySeconds} segundos antes de reintentar.");
                    Thread.Sleep(TimeSpan.FromSeconds(delaySeconds));
                }
            }

            throw new Exception("Operación fallida después de múltiples intentos");
        }

    }

    public class GestorDeTrabajosDeUsuario : GestorDeElementos<ContextoSe, TrabajoDeUsuarioDtm, TrabajoDeUsuarioDto>
    {

        public static string PeticionDelControlador = nameof(PeticionDelControlador);

        public class MapearNegocio : Profile
        {
            public MapearNegocio()
            {
                CreateMap<TrabajoDeUsuarioDtm, TrabajoDeUsuarioDto>()
                .ForMember(dto => dto.Trabajo, dtm => dtm.MapFrom(x => x.Trabajo.Nombre))
                .ForMember(dto => dto.Ejecutor, dtm => dtm.MapFrom(x => x.Ejecutor == null ? null : $"({x.Ejecutor.Login})- {x.Ejecutor.Nombre} {x.Ejecutor.Apellido}"))
                .ForMember(dto => dto.Sometedor, dtm => dtm.MapFrom(x => x.Sometedor == null ? null : $"({x.Sometedor.Login}) {x.Sometedor.Apellido} {x.Sometedor.Nombre}"))
                .ForMember(dto => dto.Estado, dtm => dtm.MapFrom(x => TrabajoSometido.ToDto(x.Estado)));


                CreateMap<TrabajoDeUsuarioDto, TrabajoDeUsuarioDtm>()
                .ForMember(dtm => dtm.Ejecutor, dto => dto.Ignore())
                .ForMember(dtm => dtm.Sometedor, dto => dto.Ignore())
                .ForMember(dtm => dtm.Trabajo, dto => dto.Ignore())
                .ForMember(dtm => dtm.Estado, dto => dto.MapFrom(x => TrabajoSometido.ToDtm(x.Estado)));
            }
        }

        public GestorDeTrabajosDeUsuario(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        public static GestorDeTrabajosDeUsuario GestorTu(ContextoSe contexto)
        {
            return new GestorDeTrabajosDeUsuario(contexto, contexto.Mapeador);
        }


        public static List<TrabajoDeUsuarioDtm> Pendientes(ContextoSe contexto, TrabajoSometidoDtm ts)
        {
            var filtro1 = new ClausulaDeFiltrado { Clausula = nameof(TrabajoDeUsuarioDtm.IdTrabajo), Criterio = enumCriteriosDeFiltrado.igual, Valor = ts.Id.ToString() };
            var filtro2 = new ClausulaDeFiltrado { Clausula = nameof(TrabajoDeUsuarioDtm.Estado), Criterio = enumCriteriosDeFiltrado.igual, Valor = enumEstadosDeUnTrabajo.Pendiente.ToDtm() };
            var filtros = new List<ClausulaDeFiltrado> { filtro1, filtro2 };
            var parametros = new ParametrosDeNegocio(enumTipoOperacion.LeerSinBloqueo, aplicarJoin: false);
            var gestor = GestorTu(contexto);
            var trabajosDtm = gestor.LeerRegistros(0, -1, filtros, null, parametros);
            return trabajosDtm;
        }

        internal static TrabajoDeUsuarioDtm CrearSiNoEstaPendiente(ContextoSe contexto, TrabajoSometidoDtm ts, Dictionary<string, object> datosDeCreacion)
        {
            var filtro1 = new ClausulaDeFiltrado { Clausula = nameof(TrabajoDeUsuarioDtm.IdTrabajo), Criterio = enumCriteriosDeFiltrado.igual, Valor = ts.Id.ToString() };
            var filtro2 = new ClausulaDeFiltrado { Clausula = nameof(TrabajoDeUsuarioDtm.Estado), Criterio = enumCriteriosDeFiltrado.igual, Valor = enumEstadosDeUnTrabajo.Pendiente.ToDtm() };
            var filtros = new List<ClausulaDeFiltrado> { filtro1, filtro2 };
            var parametros = new ParametrosDeNegocio(enumTipoOperacion.LeerSinBloqueo, aplicarJoin: false);
            var gestor = GestorTu(contexto);
            var trabajosDtm = gestor.LeerRegistros(0, -1, filtros, null, parametros);

            if (trabajosDtm.Count == 0)
                return Crear(contexto, ts, datosDeCreacion);

            if (trabajosDtm.Count >= 1)
            {
                var parametrosDeEjecucion = datosDeCreacion.ContainsKey(nameof(TrabajoDeUsuarioDtm.Parametros)) ? datosDeCreacion[nameof(TrabajoDeUsuarioDtm.Parametros)].ToString() : new List<string>().ToJson();
                foreach (var trabajo in trabajosDtm)
                {
                    if (trabajo.Parametros == parametrosDeEjecucion)
                        return trabajo;
                }
            }

            return Crear(contexto, ts, datosDeCreacion);
        }

        internal static TrabajoDeUsuarioDtm Crear(ContextoSe contexto, TrabajoSometidoDtm ts, Dictionary<string, object> datosDeCreacion)
        {
            var tu = new TrabajoDeUsuarioDtm();
            tu.IdSometedor = contexto.DatosDeConexion.IdUsuario;
            tu.IdEjecutor = ts.IdEjecutor == null ? tu.IdSometedor : (int)ts.IdEjecutor;
            tu.IdTrabajo = ts.Id;
            tu.Estado = enumEstadosDeUnTrabajo.Pendiente.ToDtm();
            tu.Planificado = datosDeCreacion.ContainsKey(nameof(TrabajoDeUsuarioDtm.Planificado)) ? (DateTime)datosDeCreacion[nameof(TrabajoDeUsuarioDtm.Planificado)] : DateTime.Now;
            tu.Parametros = datosDeCreacion.ContainsKey(nameof(TrabajoDeUsuarioDtm.Parametros)) ? datosDeCreacion[nameof(TrabajoDeUsuarioDtm.Parametros)].ToString() : new List<string>().ToJson();
            tu.Periodicidad = (int)datosDeCreacion.LeerValor(nameof(TrabajoDeUsuarioDtm.Periodicidad), 0);
            return Crear(contexto, tu);
        }

        private static TrabajoDeUsuarioDtm Crear(ContextoSe contexto, TrabajoDeUsuarioDtm tu)
        {
            var gestor = GestorTu(contexto);
            tu = gestor.PersistirRegistro(tu, new ParametrosDeNegocio(enumTipoOperacion.Insertar));
            return tu;
        }

        public static void Iniciar(ContextoSe contextoDelEntorno, int idTrabajoDeUsuario, bool iniciadoPorLaCola)
        {
            var gestorDelEntorno = GestorTu(contextoDelEntorno);
            var trabajoDeUsuarioDtm = gestorDelEntorno.LeerRegistroPorId(idTrabajoDeUsuario, true, true, true, aplicarJoin: false);

            //El trabajo ya ha sido iniciado por otro hilo o proceso
            if (trabajoDeUsuarioDtm.Iniciado is not null)
            {
                return;
            }

            var entorno = new EntornoDeTrabajo(gestorDelEntorno, trabajoDeUsuarioDtm);
            entorno.ProcesoIniciadoPorLaCola = iniciadoPorLaCola;
            entorno.Test = contextoDelEntorno.Test;
            entorno.PonerSemaforo();
            try
            {
                trabajoDeUsuarioDtm.Iniciado = DateTime.Now;
                trabajoDeUsuarioDtm.Estado = TrabajoSometido.ToDtm(enumEstadosDeUnTrabajo.iniciado);
                trabajoDeUsuarioDtm = entorno.GestorDelEntorno.PersistirRegistro(trabajoDeUsuarioDtm, new ParametrosDeNegocio(enumTipoOperacion.Modificar));
            }
            catch (Exception e)
            {
                entorno.AnotarError(e);
                entorno.QuitarSemaforo("Iniciación cancelada");
                entorno.ContextoDelEntorno.EsElContextosDeUnEntorno = false;
                throw;
            }
            EjecutarTrabajo(entorno);
        }

        private static void EjecutarTrabajo(EntornoDeTrabajo entorno)
        {
            try
            {
                var metodo = ApiDeEnsamblados.ObtenerMetodoEstatico(entorno.TrabajoSometido.Dll, entorno.TrabajoSometido.Clase, entorno.TrabajoSometido.Metodo);

                //ContextoSe.ObtenerContextoParaUnTs(entorno.ContextoDelEntorno, ejecutadoPorLaCola: true)
                var contextoDelProceso = ContextoSe.Crear(entorno.ContextoDelEntorno, iniciadoPorLaCola: entorno.ProcesoIniciadoPorLaCola);

                entorno.contextoDelProceso = contextoDelProceso;
                entorno.contextoDelProceso.AsignarUsuario(entorno.Ejecutor);
                contextoDelProceso.Entorno = entorno;
                contextoDelProceso.TrazarEnElTrabajo = entorno.CrearTraza;

                //var tran = false;
                //if (!entorno.contextoDelProceso.Test)
                //    tran = entorno.contextoDelProceso.IniciarTransaccion();
                entorno.contextoDelProceso.IniciarTraza($"Proceso_{entorno.TrabajoSometido.Metodo}");
                try
                {
                    metodo.Invoke(null, new object[] { entorno });
                    //entorno.contextoDelProceso.Commit(tran);
                }
                catch
                {
                    //entorno.contextoDelProceso.Rollback(tran);
                    throw;
                }
                finally
                {
                    entorno.contextoDelProceso.CerrarTraza();
                }


                entorno.TrabajoDeUsuario.Estado = !entorno.HayErrores
                ? TrabajoSometido.ToDtm(enumEstadosDeUnTrabajo.Terminado)
                : TrabajoSometido.ToDtm(enumEstadosDeUnTrabajo.conErrores);
                entorno.ComunicarFinalizacion();
            }
            catch (Exception e)
            {
                entorno.TrabajoDeUsuario.Estado = TrabajoSometido.ToDtm(enumEstadosDeUnTrabajo.Error);
                if (e.InnerException != null)
                {
                    entorno.ComunicarError(e.InnerException);
                    throw e.InnerException;
                }

                entorno.ComunicarError(e);
                throw;
            }
            finally
            {
                entorno.TrabajoDeUsuario.Terminado = DateTime.Now;
                var parametros = new ParametrosDeNegocio(enumTipoOperacion.Modificar);
                parametros.Parametros[enumParametro.accion] = EnumParametroTu.terminando;
                entorno.GestorDelEntorno.PersistirRegistro(entorno.TrabajoDeUsuario, parametros);
                entorno.ContextoDelEntorno.EsElContextosDeUnEntorno = false;
                entorno.QuitarSemaforo($"Trabajo finalizado: {(entorno.TrabajoDeUsuario.Estado == TrabajoSometido.ToDtm(enumEstadosDeUnTrabajo.Terminado) ? "sin errores" : "con errores")}");

                if (entorno.TrabajoDeUsuario.Periodicidad > 0)
                    Resometer(entorno.ContextoDelEntorno, entorno.TrabajoDeUsuario.Id, aplicarLaPeriodicidad: true, fechaPlanificada: entorno.FechaDeResometimiento);
            }
        }

        public static void Bloquear(ContextoSe contexto, int idTrabajoDeUsuario)
        {
            var gestor = GestorTu(contexto);
            var tuDtm = gestor.LeerRegistroPorId(idTrabajoDeUsuario, true, true, true, aplicarJoin: true);
            try
            {
                if (tuDtm.Estado != TrabajoSometido.ToDtm(enumEstadosDeUnTrabajo.Pendiente))
                    throw new Exception($"El trabajo no se puede bloquear, ha de estar en estado pendiente y está en estado {TrabajoSometido.ToDto(tuDtm.Estado)}");
                tuDtm.Estado = TrabajoSometido.ToDtm(enumEstadosDeUnTrabajo.Bloqueado);
                gestor.PersistirRegistro(tuDtm, new ParametrosDeNegocio(enumTipoOperacion.Modificar));
                GestorDeTrazasDeUnTrabajo.AnotarTraza(contexto, tuDtm, $"Trabajo bloqueado por el usuario {contexto.DatosDeConexion.Login}");
            }
            catch (Exception e)
            {
                GestorDeErroresDeUnTrabajo.AnotarError(contexto, tuDtm, e);
                GestorDeTrazasDeUnTrabajo.AnotarTraza(contexto, tuDtm, $"El usuario {contexto.DatosDeConexion.Login} no ha podido bloquear el trabajo");
                throw;
            }
        }

        public static void Desbloquear(ContextoSe contexto, int idTrabajoDeUsuario)
        {
            var gestor = GestorTu(contexto);
            var tu = gestor.LeerRegistroPorId(idTrabajoDeUsuario, true, true, true, aplicarJoin: true);
            try
            {
                if (tu.Estado != TrabajoSometido.ToDtm(enumEstadosDeUnTrabajo.Bloqueado))
                    throw new Exception($"El trabajo no se puede desbloquear, ha de estar en estado bloqueado y está en estado {TrabajoSometido.ToDto(tu.Estado)}");
                tu.Estado = TrabajoSometido.ToDtm(enumEstadosDeUnTrabajo.Pendiente);
                gestor.PersistirRegistro(tu, new ParametrosDeNegocio(enumTipoOperacion.Modificar));
                GestorDeTrazasDeUnTrabajo.AnotarTraza(contexto, tu, $"Trabajo desbloqueado por el usuario {contexto.DatosDeConexion.Login}");
            }
            catch (Exception e)
            {
                GestorDeErroresDeUnTrabajo.AnotarError(contexto, tu, e);
                GestorDeTrazasDeUnTrabajo.AnotarTraza(contexto, tu, $"El usuario {contexto.DatosDeConexion.Login} no ha podido desbloquear el trabajo");
                throw;
            }
        }

        public static void Resometer(ContextoSe contexto, int idTrabajoDeUsuario, bool aplicarLaPeriodicidad, DateTime? fechaPlanificada = null)
        {
            var gestor = GestorTu(contexto);
            var tu = gestor.LeerRegistroPorId(idTrabajoDeUsuario, true, true, true, aplicarJoin: true);

            if (tu.Estado != TrabajoSometido.ToDtm(enumEstadosDeUnTrabajo.Error) &&
                tu.Estado != TrabajoSometido.ToDtm(enumEstadosDeUnTrabajo.conErrores) &&
                tu.Estado != TrabajoSometido.ToDtm(enumEstadosDeUnTrabajo.Terminado) &&
                tu.Estado != TrabajoSometido.ToDtm(enumEstadosDeUnTrabajo.iniciado)
               )
                throw new Exception($"El trabajo no se puede resometer, ha de estar en estado terminado, iniciado, con errores o erroneo y está en estado {TrabajoSometido.ToDto(tu.Estado)}");

            var fechaDePlanificacion = fechaPlanificada != null
                ? fechaPlanificada.Fecha()
                : aplicarLaPeriodicidad ? tu.Planificado.AddSeconds(tu.Periodicidad) : DateTime.Now.AddSeconds(60);

            while (true)
            {
                if (fechaDePlanificacion >= DateTime.Now) break;
                fechaDePlanificacion = aplicarLaPeriodicidad ? fechaDePlanificacion.AddSeconds(tu.Periodicidad) : DateTime.Now.AddSeconds(60);
            }

            var tr = new TrabajoDeUsuarioDtm();
            tr.IdSometedor = contexto.DatosDeConexion.IdUsuario;
            tr.IdEjecutor = tu.IdEjecutor;
            tr.IdTrabajo = tu.IdTrabajo;
            tr.Estado = enumEstadosDeUnTrabajo.Pendiente.ToDtm();
            tr.Planificado = fechaDePlanificacion;
            tr.Parametros = tu.Parametros;
            tr.Periodicidad = tu.Periodicidad;
            Crear(contexto, tr);
        }

        protected override IQueryable<TrabajoDeUsuarioDtm> AplicarJoins(IQueryable<TrabajoDeUsuarioDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(p => p.Ejecutor);
            consulta = consulta.Include(p => p.Sometedor);
            consulta = consulta.Include(p => p.Trabajo);
            return consulta;
        }

        protected override IQueryable<TrabajoDeUsuarioDtm> AplicarFiltros(IQueryable<TrabajoDeUsuarioDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);

            var filtro = filtros.FirstOrDefault(x => x.Clausula.Equals(nameof(ElementoDtm.Nombre), StringComparison.CurrentCultureIgnoreCase));
            if (filtro is not null)
            {
                consulta = consulta.Where<TrabajoDeUsuarioDtm>(x => Contexto.Set<TrabajoSometidoDtm>().Where(t => t.Nombre.Contains(filtro.Valor)).Any(y => y.Id == x.IdTrabajo));
                filtro.Aplicado = true;
            }

            return consulta;
        }

        protected override void AntesDeMapearElRegistroParaInsertar(TrabajoDeUsuarioDto elemento, ParametrosDeNegocio opciones)
        {
            base.AntesDeMapearElRegistroParaInsertar(elemento, opciones);
            if (elemento.Estado.IsNullOrEmpty())
                elemento.Estado = enumEstadosDeUnTrabajo.Pendiente.ToDto();
            if (elemento.Parametros.IsNullOrEmpty())
                elemento.Parametros = "[]";
        }

        private void ValidarAntesDeEliminar(TrabajoDeUsuarioDtm registro, ParametrosDeNegocio parametros)
        {
            var RegistroEnBD = ((TrabajoDeUsuarioDtm)parametros.registroEnBd);
            if (RegistroEnBD.Iniciado.HasValue && !RegistroEnBD.Terminado.HasValue)
            {
                GestorDeErrores.Emitir("Un trabajo en ejecución no se puede eliminar");
            }
        }

        private void ValidarAntesDeModificar(TrabajoDeUsuarioDtm registro, ParametrosDeNegocio parametros)
        {
            var RegistroEnBD = ((TrabajoDeUsuarioDtm)parametros.registroEnBd);
            if (RegistroEnBD.IdSometedor != registro.IdSometedor)
                GestorDeErrores.Emitir("No se puede modificar el sometedor de un trabajo");

            if (RegistroEnBD.Encolado.Subtract(registro.Encolado).TotalSeconds > 2)
                GestorDeErrores.Emitir("No se puede modificar la fecha de entrada de un trabajo en la cola");

            if (!registro.Iniciado.HasValue && registro.Terminado.HasValue)
                GestorDeErrores.Emitir("No se se puede terminar un trabajo que aun no se ha iniciado");

            if (registro.Terminado.HasValue && !SeEstaTerminando(parametros.Parametros))
                GestorDeErrores.Emitir("No se se puede modificar un trabajo terminado");

            if (RegistroEnBD.Iniciado.HasValue && !SeEstaTerminando(parametros.Parametros))
                GestorDeErrores.Emitir("Un trabajo en ejecución no se puede modificar");
        }

        private bool SeEstaTerminando(Dictionary<string, object> parametros)
        {
            if (!parametros.ContainsKey(enumParametro.accion))
                return false;

            return (string)parametros[enumParametro.accion] == EnumParametroTu.terminando;
        }

        protected override void AntesDePersistir(TrabajoDeUsuarioDtm trabajo, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(trabajo, parametros);

            if (parametros.Insertando)
            {
                if (!Contexto.DatosDeConexion.EsAdministrador && (bool)parametros.Parametros.LeerValor(PeticionDelControlador, false))
                    GestorDeErrores.Emitir("Un usuario no administrador no puede solicitar crear un trabajo sometido directamente desde las interface");
                trabajo.Encolado = DateTime.Now;
                trabajo.Iniciado = default;
                trabajo.Terminado = default;
                extJson.ValidarJson(trabajo.Parametros);
                if (trabajo.Planificado.Millisecond > 0 || trabajo.Planificado.Second > 0)
                {
                    trabajo.Planificado = trabajo.Planificado.AddMilliseconds(1000 - trabajo.Planificado.Millisecond);
                    trabajo.Planificado = trabajo.Planificado.AddSeconds(60 - trabajo.Planificado.Second);
                    trabajo.Planificado.AddMinutes(1);
                }
            }
            else if (parametros.Eliminando)
            {
                ValidarAntesDeEliminar(trabajo, parametros);
                GestorDeTrazasDeUnTrabajo.EliminarTrazas(Contexto, ((TrabajoDeUsuarioDtm)parametros.registroEnBd).Id);
                GestorDeErroresDeUnTrabajo.EliminarErrores(Contexto, ((TrabajoDeUsuarioDtm)parametros.registroEnBd).Id);
            }
            else if (parametros.Modificando)
                ValidarAntesDeModificar(trabajo, parametros);
        }

        protected override void AntesDeMapearElElemento(TrabajoDeUsuarioDtm registro, ParametrosDeNegocio parametros)
        {
            base.AntesDeMapearElElemento(registro, parametros);
        }
    }
}

