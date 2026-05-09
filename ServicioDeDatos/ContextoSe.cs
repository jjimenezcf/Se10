using AutoMapper;
using Gestor.Errores;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ServicioDeCorreos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.Utilidades;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Utilidades;
using Z.EntityFramework.Extensions;
using DbCommandInterceptor = Z.EntityFramework.Extensions.DbCommandInterceptor;

namespace ServicioDeDatos
{
    public class Literal
    {
        internal static readonly string usuario = "jjimenezcf@gmail.com";
        internal static readonly string Version_0 = "0.0.0";
        public static readonly string CadenaDeConexion = nameof(CadenaDeConexion);
        public static readonly string VariableNoDefinida = nameof(VariableNoDefinida);
        public static readonly string ParametroNoDefinido = nameof(ParametroNoDefinido);
        public const string menosUno = "-1";
        public const string Cero = "0";
        public const string Uno = "1";
        public const string True = "S";
        public class Vista
        {
            internal static string Catalogo = "CatalogoDelSe";
        }
    }

    public class DatosDeConexion
    {
        public string ServidorWeb { get; set; }
        public string ServidorBd { get; set; }
        public string Bd { get; set; }
        public string Login { get; internal set; }
        public int IdUsuario { get; internal set; }
        public bool EsAdministrador { get; internal set; }
        public bool EsClienteWeb { get; internal set; }
        public string Version { get; set; }
        public string Menu { get; set; }
        public bool CreandoModelo { get; set; } = false;

        public bool UsarBundle { get; set; } = false;

    }

    public class ConstructorDelContexto : IDesignTimeDbContextFactory<ContextoSe>
    {
        public ContextoSe CreateDbContext(string[] arg)
        {

            var datosDeConexion = ContextoSe.ObtenerDatosDeConexion();

            var opciones = new DbContextOptionsBuilder<ContextoSe>();
            opciones.UseSqlServer(datosDeConexion.CadenaConexion);
            object[] parametros = { opciones.Options, datosDeConexion.Configuracion };

            return (ContextoSe)Activator.CreateInstance(typeof(ContextoSe), parametros);
        }

    }

    public static class Transaccion
    {

        public static bool IniciarTransaccion(this ContextoSe contexto, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            if (contexto.Database.CurrentTransaction == null)
            {
                contexto.Transaccion = contexto.Database.BeginTransaction(isolationLevel);
                return true;
            }
            return false;
        }

        public static void Commit(this ContextoSe contexto, bool transaccion)
        {
            if (transaccion)
            {
                if (contexto.Test)
                    contexto.Transaccion.Rollback();
                else
                    contexto.Transaccion.Commit();
                contexto.Transaccion.Dispose();
                contexto.Transaccion = null;
            }
        }
        public static void Rollback(this ContextoSe contexto, bool transaccion, Exception exc = null)
        {
            if (transaccion && contexto.HayTransaccion())
            {
                contexto.Transaccion.Rollback();
                contexto.Transaccion.Dispose();
                contexto.Transaccion = null;
            }

            if (contexto.Traza is not null && exc != null) contexto.Traza.AnotarExcepcion(exc);

        }
        public static bool HayTransaccion(this ContextoSe contexto) => contexto.Transaccion != null;

        public static DbTransaction GetDbTransaction(this IDbContextTransaction source)
        {
            if (source == null)
                return null;

            return (source as IInfrastructure<DbTransaction>).Instance;
        }
    }

    public partial class ContextoSe : DbContext
    {

        public static readonly string Login_Admin = "admin.se";

        //private static ConcurrentDictionary<string, ContextoSe> _CacheDeContextos { get; set; }
        public DatosDeConexion DatosDeConexion { get; private set; }
        public IConfiguration Configuracion { get; private set; }

        private UsuarioDtm _usuario = null;
        public UsuarioDtm Usuario => _usuario = _usuario is null ? Set<UsuarioDtm>().Where(x => x.Id == DatosDeConexion.IdUsuario).First() : _usuario;

        private IConfigurationSection OpcionesDeEF => Configuracion.GetSection(nameof(ltrAppSetting.OpcionesDeEF));

        public IServiceProvider ServiceProvider { get; private set; }

        public DbContextOptions OpcionesDelContexto { get; private set; }

        public bool Test { get; set; }

        public bool TrabajoSometido { get; set; } = false;

        public object Entorno { get; set; }

        public Func<string, ServicioDeDatos.TrabajosSometidos.TrazaDeUnTrabajoDtm> TrazarEnElTrabajo { get; set; }

        public IMapper Mapeador { get; set; }

        internal IDbContextTransaction Transaccion { get; set; }

        public bool HayTransaccion => Transaccion != null;

        private Guid? _IdOperacion = null;
        public Guid? IdOperacion { get { return _IdOperacion is null ? (_IdOperacion = Guid.NewGuid()) : _IdOperacion; } }

        private bool _debug = false;
        public bool Debuggar
        {
            get
            {
                if (!_debug) return CacheDeVariable.Cfg_HayQueDebuggar;
                return true;
            }
            set
            {
                _debug = value;
            }
        }
        private string _ficheroDebug { get; set; }

        public bool EsElContextosDeUnEntorno { get; set; } = false;

        public bool EsElContextoDeUnaAccion => Accion is not null;
        public AccionDtm Accion { get; set; }

        public string NombreDelTrabajo { get; set; }

        public string GuidDeConsulta { get; set; } = null;

        public bool EsConsultaPorGuid => GuidDeConsulta != null;

        private string ObtenerVersion => CacheDeVariable.Cfg_Version;

        public TrazaSql Traza { get; set; }

        private InterceptadorDeConsultas Interceptor;

        public void AsignarUsuario(UsuarioDtm usuario)
        {
            if (usuario is null)
                GestorDeErrores.Emitir(ltrDeUnUsuario.Logout);

            if (!usuario.Activo)
            {
                if (usuario.Login == ContextoSe.Login_Admin)
                {
                    usuario.Activo = true;
                }
                else GestorDeErrores.Emitir($"El usuario {usuario.Login} no está activo");
            }

            DatosDeConexion.IdUsuario = usuario.Id;
            DatosDeConexion.EsAdministrador = usuario.EsAdministrador;
            DatosDeConexion.Login = usuario.Login;
            DatosDeConexion.EsClienteWeb = UsuarioDtm.EsClienteWeb(this);
            _usuario = usuario;
        }

        public UsuarioDtm AsignarLogin(string login, bool emitirError)
        {
            DatosDeConexion.Login = login;
            var usuario = Set<UsuarioDtm>().FirstOrDefault(usuario => usuario.Login == login);
            if ((usuario is null || usuario.Activo == false) && !emitirError)
                return usuario;

            AsignarUsuario(usuario);
            return usuario;
        }

        public bool AsignarRolDeAdministrador()
        {
            if (DatosDeConexion.EsAdministrador)
                return false;
            DatosDeConexion.EsAdministrador = true;
            return true;
        }

        public bool QuitarRolDeAdministrador()
        {
            if (!DatosDeConexion.EsAdministrador) return false;
            DatosDeConexion.EsAdministrador = false;
            return true;
        }

        public void QuitarUsuario()
        {
            DatosDeConexion.IdUsuario = 0;
            DatosDeConexion.EsAdministrador = false;
            DatosDeConexion.Login = null;
            _usuario = null;
        }

        public bool HayRegistros<T>(ContextoSe contexto) where T : RegistroDtm
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Dtm_HayRegistros);
            var i = typeof(T).FullName;
            if (!cache.ContainsKey(i))
            {
                cache[i] = contexto.Set<T>().Any(x => true);
            }
            return (bool)cache[i];
        }


        public static void IncluirServiciosParaElCorreo(IServiceCollection services)
        {
            services.AddSingleton<IGraphTokenService, GraphTokenService>();
            services.AddTransient<IDistribuidorOfice365, Oficce365>();
            services.AddTransient<MailJet.Tenante>();
            services.AddTransient<IDistribuidorMailJet, MailJet>();
            services.AddTransient<SmtpMail.Tenante>();
            services.AddTransient<IDistribuidorSmtp, SmtpMail>();
        }

        //public static ContextoSe ObtenerContextoParaUnTs(ContextoSe contexto, bool ejecutadoPorLaCola)
        //{
        //    if (contexto.Test) return contexto;

        //    var (configuracion, cadenaConexion) = ObtenerDatosDeConexion();

        //    if (contexto.ServiceProvider is null)
        //    {
        //        var servicios = new ServiceCollection().AddSingleton<IConfiguration>(configuracion);
        //        IncluirServiciosParaElCorreo(servicios);
        //        return ObtenerContextoParaUnTs(contexto, ejecutadoPorLaCola, servicios.BuildServiceProvider());
        //    }

        //    return ObtenerContextoParaUnTs(contexto, ejecutadoPorLaCola, contexto.ServiceProvider);

        //}

        //private static ContextoSe ObtenerContextoParaUnTs(ContextoSe contexto, bool ejecutadoPorLaCola, IServiceProvider proveedorDeServicios)
        //{
        //    var optionsBuilder = new DbContextOptionsBuilder<ContextoSe>();

        //    var (configuracion, cadenaConexion) = ObtenerDatosDeConexion();

        //    optionsBuilder.UseSqlServer(cadenaConexion);

        //    var nuevoContexto = new ContextoSe(optionsBuilder.Options, configuracion, proveedorDeServicios)
        //    {
        //        Mapeador = contexto.Mapeador,
        //        DatosDeConexion = contexto.DatosDeConexion,
        //        Traza = contexto.Traza,
        //        Interceptor = contexto.Interceptor,
        //        Test = contexto.Test,
        //        TrabajoSometido = ejecutadoPorLaCola
        //    };
        //    optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

        //    return nuevoContexto;
        //}

        public static (IConfigurationRoot Configuracion, string CadenaConexion) ObtenerDatosDeConexion()
        {

            var generador = new ConfigurationBuilder()
                   .SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json");

            var configuracion = generador.Build();
            var cadenaDeConexion = configuracion.GetConnectionString(Literal.CadenaDeConexion);
            return (configuracion, cadenaDeConexion);
        }

        public static ContextoSe Crear(ContextoSe contexto, bool iniciadoPorLaCola = false)
        {
            // Obtener el IServiceScopeFactory del ServiceProvider principal.
            var scopeFactory = contexto.ServiceProvider.GetRequiredService<IServiceScopeFactory>();

            // Crear un nuevo ámbito (scope)
            var scope = scopeFactory.CreateScope();

            // Usar el ServiceProvider del nuevo scope para obtener el contexto.
            // Esto garantiza que el nuevo contexto tenga sus propios servicios, incluyendo un nuevo DbContext.
            var nuevoContexto = scope.ServiceProvider.GetRequiredService<ContextoSe>();

            nuevoContexto.DatosDeConexion = contexto.DatosDeConexion;

            // Asignar propiedades que necesites replicar, como el Mapeador o el Usuario.
            nuevoContexto.Mapeador = contexto.Mapeador;
            nuevoContexto.AsignarUsuario(contexto.Usuario);
            nuevoContexto.Test = contexto.Test;
            nuevoContexto.TrabajoSometido = iniciadoPorLaCola;

            return nuevoContexto;
        }

        public ContextoSe(DbContextOptions options) : base(options)
        {
        }

        public ContextoSe(DbContextOptions opcionesDelContexto, IConfiguration configuracion) :
        this(opcionesDelContexto, configuracion, null)
        { }

        public ContextoSe(DbContextOptions opcionesDelContexto, IConfiguration configuracion, IServiceProvider serviceProvider) :
        base(opcionesDelContexto)
        {
            Configuracion = configuracion;
            OpcionesDelContexto = opcionesDelContexto;
            ServiceProvider = serviceProvider;

            Interceptor = new InterceptadorDeConsultas();
            DbInterception.Add(Interceptor);
            ChangeTracker.AutoDetectChangesEnabled = false;
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            InicializarDatosDeConexion();
        }

        public void AplicarMigraciones() => Database.Migrate();

        public void BorrarBd()
        {
            string borrar = $@"
begin
  EXEC msdb.dbo.sp_delete_database_backuphistory @database_name = N'{DatosDeConexion.Bd}'
  USE [master];
  ALTER DATABASE [{DatosDeConexion.Bd}] SET  SINGLE_USER WITH ROLLBACK IMMEDIATE;
  USE [master];
  DROP DATABASE [{DatosDeConexion.Bd}];
end";
            Database.ExecuteSqlRaw(borrar);

        }

        /// <summary>
        /// NECESARIO PARA EJECUTAR LAS MIGRACIONES EN UN PROYECTO APARTE
        /// </summary>
        /// <param name="options"></param>
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            var conexion = Configuracion.GetConnectionString(Literal.CadenaDeConexion);

            var enableSensitiveDataLogging = OpcionesDeEF != null ? OpcionesDeEF[ltrAppSetting.OpcionesDeEF.EnableSensitiveDataLogging].EsTrue() : false;
            var enableDetailedErrors = OpcionesDeEF != null ? OpcionesDeEF[ltrAppSetting.OpcionesDeEF.EnableDetailedErrors].EsTrue() : false;

            options.UseSqlServer(conexion, x =>
            {
                x.MigrationsHistoryTable("__Migraciones", "ENTORNO");
                x.MigrationsAssembly("Migraciones");
            });

            if (enableSensitiveDataLogging) options.EnableSensitiveDataLogging();
            if (enableDetailedErrors) options.EnableDetailedErrors();

            options.LogTo(Console.WriteLine, LogLevel.Information)
                   .ConfigureWarnings(w => w.Ignore(SqlServerEventId.SavepointsDisabledBecauseOfMARS));

            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        }

        public class BlankTriggerAddingConvention : IModelFinalizingConvention
        {
            public virtual void ProcessModelFinalizing(
                IConventionModelBuilder modelBuilder,
                IConventionContext<IConventionModelBuilder> context)
            {
                foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
                {
                    var table = StoreObjectIdentifier.Create(entityType, StoreObjectType.Table);
                    if (table != null
                        && entityType.GetDeclaredTriggers().All(t => t.GetDatabaseName(table.Value) == null)
                        && (entityType.BaseType == null
                            || entityType.GetMappingStrategy() != RelationalAnnotationNames.TphMappingStrategy))
                    {
                        entityType.Builder.HasTrigger(table.Value.Name + "_Trigger");
                    }

                    foreach (var fragment in entityType.GetMappingFragments(StoreObjectType.Table))
                    {
                        if (entityType.GetDeclaredTriggers().All(t => t.GetDatabaseName(fragment.StoreObject) == null))
                        {
                            entityType.Builder.HasTrigger(fragment.StoreObject.Name + "_Trigger");
                        }
                    }
                }
            }
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Conventions.Add(_ => new BlankTriggerAddingConvention());
        }

        public void InicializarDatosDeConexion()
        {

            DatosDeConexion = new DatosDeConexion();
            DatosDeConexion.ServidorWeb = Environment.MachineName;
            DatosDeConexion.ServidorBd = Database.GetDbConnection().DataSource;
            DatosDeConexion.Bd = Database.GetDbConnection().Database;
            DatosDeConexion.Login = null;
            DatosDeConexion.IdUsuario = 0;
            DatosDeConexion.Version = GestorDeMetadatos.ExisteTabla(Esquemas.ENTORNO, Tablas.VARIABLE) ? ObtenerVersion : "0.0";
            DatosDeConexion.UsarBundle = bool.TryParse(Configuracion[ltrAppSetting.UsarBundle], out bool usarBundle) && usarBundle;
        }

        public void IniciarTraza(string ruta, string fichero, bool? debugar = null)
        {
            IniciarTraza(fichero, debugar, ruta);
        }

        public void IniciarTraza(string fichero, bool? debugar = null, string ruta = null)
        {
            if (debugar.EsFalse())
                return;

            var valorAnterior = Debuggar;
            if (debugar.EsTrue() || DatosDeConexion.CreandoModelo)
            {
                Debuggar = true;
            }

            if (!Debuggar)
                return;

            _ficheroDebug = fichero.Replace(" ", "_");

            try
            {
                if (Traza == null)
                {
                    CrearTraza(NivelDeTraza.Siempre, _ficheroDebug, ruta);
                }
                else
                {
                    if (!Traza.Fichero.StartsWith(_ficheroDebug))
                    {
                        CerrarTraza();
                        CrearTraza(NivelDeTraza.Siempre, fichero.Replace(" ", "_"));
                    }

                    if (!Traza.Abierta)
                        Traza.Abrir(true);
                }
            }
            finally
            {
                Debuggar = valorAnterior;
            }
        }

        public void CerrarTraza(string mensaje = null)
        {
            if (Traza != null)
            {
                if (!Traza.Abierta)
                    Traza.Abrir(true);
                try
                {
                    Traza.CerrarTraza(mensaje.IsNullOrEmpty() ? "Conexión cerrada" : mensaje);
                }
                finally
                {
                    Debuggar = false;
                    Traza = null;
                    _ficheroDebug = null;
                }
            }
        }

        public void AnotarExcepcion(Exception e)
        {
            if (Traza != null)
            {
                if (!Traza.Abierta)
                    Traza.Abrir(true);

                Traza.AnotarExcepcion(e);
            }
        }
        public void AnotarTraza(string traza, string mensaje)
        {
            if (Traza != null)
            {
                if (!Traza.Abierta)
                    Traza.Abrir(true);

                Traza.AnotarMensaje(traza, mensaje);
            }
        }

        public string RegistrarLogPorFecha(string ruta, string nombreFichero, string contenido, bool borrar = true)
        {
            nombreFichero = nombreFichero.NormalizarFichero();
            string rutaCompleta = Path.Combine(ruta, nombreFichero);

            if (Path.GetExtension(rutaCompleta).IsNullOrEmpty())
                rutaCompleta = rutaCompleta + "." + enumExtensiones.txt;


            if (File.Exists(rutaCompleta))
            {
                if (borrar)
                    File.Delete(rutaCompleta);
                else
                    throw new IOException($"El archivo '{nombreFichero}' ya existe en la ruta especificada.");
            }
            else
            {
                using (StreamWriter writer = new StreamWriter(rutaCompleta))
                {
                    writer.Write(contenido);
                }
            }
            return rutaCompleta;
        }

        public void EliminarLogPorFecha(string ruta, string nombreFichero)
        {
            nombreFichero = nombreFichero.NormalizarFichero();
            string rutaCompleta = Path.Combine(ruta, nombreFichero);

            if (File.Exists(rutaCompleta))
            {
                File.Delete(rutaCompleta);
            }
        }

        public void AnotarParametros(List<object> parametros)
        {
            var mensaje = "";
            foreach (var p in parametros)
            {
                if (p != null) mensaje = mensaje + Environment.NewLine + p.ToString();
            }
            AnotarTraza("Parémetros de la petición:", mensaje);
        }

        private void CrearTraza(NivelDeTraza nivel, string fichero, string ruta = null)
        {
            Traza = TrazaSql.CrearTraza(fichero, nivel, ruta: ruta);
            Interceptor.Traza = Traza;
        }

    }

    public class InterceptadorDeConsultas : DbCommandInterceptor
    {
        public TrazaSql Traza { get; set; }
        private DbCommand _sentenciaSql { get; set; }
        private Stopwatch _cronoSql;


        public override void NonQueryExecuting(DbCommand sentenciaSql, DbCommandInterceptionContext<int> interceptionContext)
        {
            base.NonQueryExecuting(sentenciaSql, interceptionContext);
            _sentenciaSql = sentenciaSql;
            _cronoSql = new Stopwatch();
            _cronoSql.Start();
        }
        public override void NonQueryExecuted(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            base.NonQueryExecuted(command, interceptionContext);
            RegistrarTraza();
        }
        public override void NonQueryError(DbCommand sentenciaSql, DbCommandInterceptionContext<int> interceptionContext, Exception exception)
        {
            base.NonQueryError(sentenciaSql, interceptionContext, exception);
            RegistrarError(exception);
        }


        public override void ReaderExecuting(DbCommand sentenciaSql, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
            base.ReaderExecuting(sentenciaSql, interceptionContext);
            _sentenciaSql = sentenciaSql;
            _cronoSql = new Stopwatch();
            _cronoSql.Start();
        }
        public override void ReaderExecuted(DbCommand sentenciaSql, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
            base.ReaderExecuted(sentenciaSql, interceptionContext);
            RegistrarTraza();
        }
        public override void ReaderError(DbCommand sentenciaSql, DbCommandInterceptionContext<DbDataReader> interceptionContext, Exception exception)
        {
            base.ReaderError(sentenciaSql, interceptionContext, exception);
            RegistrarError(exception);
        }



        public override void ScalarExecuting(DbCommand sentenciaSql, DbCommandInterceptionContext<object> interceptionContext)
        {
            base.ScalarExecuting(sentenciaSql, interceptionContext);
            RegistrarTraza();
        }
        public override void ScalarExecuted(DbCommand sentenciaSql, DbCommandInterceptionContext<object> interceptionContext)
        {
            base.ScalarExecuted(sentenciaSql, interceptionContext);
            RegistrarTraza();
        }
        public override void ScalarError(DbCommand sentenciaSql, DbCommandInterceptionContext<object> interceptionContext, Exception exception)
        {
            base.ScalarError(sentenciaSql, interceptionContext, exception);
            RegistrarError(exception);
        }

        private void RegistrarTraza()
        {
            if (_cronoSql != null)
            {
                _cronoSql.Stop();

                if (Traza != null)
                    Traza.AnotarTrazaSql(_sentenciaSql.CommandText, _sentenciaSql.Parameters, _cronoSql.ElapsedMilliseconds);
            }
        }

        private void RegistrarError(Exception excepcion)
        {
            if (_cronoSql != null)
            {
                _cronoSql.Stop();

                if (Traza != null)
                    Traza.AnotarExcepcion(excepcion, _sentenciaSql.CommandText, _sentenciaSql.Parameters);
            }
        }
    }

    public class EntornoDeUnaAccion
    {
        public ContextoSe Contexto { get; set; }
        public IRegistro Registro { get; set; }
        public enumNegocio Negocio { get; set; }
        public TransicionDtm Transicion { get; set; }
        public AccionDeNegocioDtm Evento { get; set; }
        public PlantillaDeExportacionDtm Plantilla => Registro is PlantillaDeExportacionDtm ? (PlantillaDeExportacionDtm)Registro : null;
        public AccionDtm Accion => Contexto.Accion;
        public Dictionary<string, object> Entrada { get; set; }
        public Dictionary<string, object> Salida { get; set; } = new Dictionary<string, object>();

        private Dictionary<string, object> _parametros;
        public Dictionary<string, object> Parametros => _parametros;

        public bool EsEventodeNegocio => Evento != null;

        public EntornoDeUnaAccion(ContextoSe contexto, enumNegocio negocio, Dictionary<string, object> entrada)
            : this(contexto, null, negocio, entrada)
        {
        }

        public EntornoDeUnaAccion(ContextoSe contexto, IRegistro registro, enumNegocio negocio, Dictionary<string, object> entrada)
        {
            Contexto = contexto;
            Registro = registro;
            Negocio = negocio;
            Entrada = entrada;
        }
        public void AsignarParametros(string parametros)
        {
            _parametros = extJson.ToDiccionarioDeParametros(parametros);
        }
        public void AsignarParametros(Dictionary<string, object> parametros)
        {
            _parametros = parametros;
        }
        public static Dictionary<string, object> EjecutarAccion(EntornoDeUnaAccion entorno)
        {
            if (!entorno.Accion.Dll.IsNullOrEmpty())
            {
                ApiDeEnsamblados.EjecutarAccionDeEntorno(entorno, entorno.Accion.Dll, entorno.Accion.Clase, entorno.Accion.Metodo);
                return entorno.Salida;
            }

            if (!entorno.Accion.Pa.IsNullOrEmpty())
                return EjecutarPa(entorno);

            return EjecutarSql(entorno);
        }

        private static Dictionary<string, object> EjecutarSql(EntornoDeUnaAccion entorno)
        {
            return entorno.Salida;
        }

        private static Dictionary<string, object> EjecutarPa(EntornoDeUnaAccion entorno)
        {
            return entorno.Salida;
        }
    }
}


