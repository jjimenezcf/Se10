using GestoresDeNegocio.Callejero;
using GestoresDeNegocio.Contabilidad;
using GestoresDeNegocio.Entorno;
using GestoresDeNegocio.Expediente;
using GestoresDeNegocio.Gastos;
using GestoresDeNegocio.Guarderias;
using GestoresDeNegocio.Juridico;
using GestoresDeNegocio.Logistica;
using GestoresDeNegocio.MaestrosTecnico;
using GestoresDeNegocio.Negocio;
using GestoresDeNegocio.Presupuesto;
using GestoresDeNegocio.RegistroEs;
using GestoresDeNegocio.Seguridad;
using GestoresDeNegocio.SistemaDocumental;
using GestoresDeNegocio.Tarea;
using GestoresDeNegocio.Terceros;
using GestoresDeNegocio.TrabajosSometidos;
using GestoresDeNegocio.Ventas;
using Microsoft.Extensions.DependencyInjection;

namespace GestoresDeNegocio;

public static class ServiceExtensions
{

    //public static ServiceCollection Servicios()
    //{
    //    var (configuracion, cadenaConexion) = ContextoSe.ObtenerDatosDeConexion();
    //    var servicios = new ServiceCollection();
    //    servicios.AddSingleton<IConfiguration>(configuracion);
    //    servicios.ConfigureGestoresDeNegocio();
    //    ContextoSe.IncluirServiciosParaElCorreo(servicios);
    //    return servicios;
    //}

    public static void ConfigureGestoresDeNegocio(this IServiceCollection services)
    {
        services.ConfigureEntorno();
        services.ConfigureSeguridad();
        services.ConfigureNegocio();
        services.ConfigureCallejero();
        services.ConfigureTrabajosSometidos();
        services.ConfigureTerceros();
        services.ConfigureSistemaDocumental();
        services.ConfigureRegistroEs();
        services.ConfigureTareas();
        services.ConfigureExpedientes();
        services.ConfigureJuridica();
        services.ConfigureMaestrosTecnicos();
        services.ConfigureContabilidad();
        services.ConfigurePresupuestos();
        services.ConfigureVentas();
        services.ConfigureGastos();
        services.ConfigureLogistica();
        services.ConfigureGuarderias();
    }

    public static void ConfigureEntorno(this IServiceCollection services)
    {
        services.AddScoped<GestorDeAcciones>();
        services.AddScoped<GestorDeAgendas>();
        services.AddScoped<GestorDeArbolDeMenu>();
        services.AddScoped<GestorDeCertificados>();
        services.AddScoped<GestorDeEventosDeAgenda>();
        services.AddScoped<GestorDeMenus>();
        services.AddScoped<GestorDePermisosDeUnUsuario>();
        services.AddScoped<GestorDeUsuarios>();
        services.AddScoped<GestorDeVariables>();
        services.AddScoped<GestorDeVistaMvc>();
        services.AddScoped<GestorDeMiCorreo>();
        services.AddScoped<GestorDeParametrosVistaPorUsuario>();
    }

    public static void ConfigureSeguridad(this IServiceCollection services)
    {
        services.AddScoped<GestorDeClaseDePermisos>();
        services.AddScoped<GestorDePermisos>();
        services.AddScoped<GestorDePermisosDeUnRol>();
        services.AddScoped<GestorDePermisosDirectos>();
        services.AddScoped<GestorDePermisosHeredados>();
        services.AddScoped<GestorDePermisosPorCg>();
        services.AddScoped<GestorDePermisosPorElemento>();
        services.AddScoped<GestorDePermisosPorEstado>();
        services.AddScoped<GestorDePermisosPorNegocio>();
        services.AddScoped<GestorDePermisosPorTipo>();
        services.AddScoped<GestorDePermisosPorTransicion>();
        services.AddScoped<GestorDePuestosDeTrabajo>();
        services.AddScoped<GestorDePuestosDeUnRol>();
        services.AddScoped<GestorDePuestosDeUnUsuario>();
        services.AddScoped<GestorDeRoles>();
        services.AddScoped<GestorDeRolesDeUnPermiso>();
        services.AddScoped<GestorDeRolesDeUnPuesto>();
        services.AddScoped<GestorDeTipoPermiso>();
        services.AddScoped<GestorDeUsuariosDeUnPuesto>();
    }

    public static void ConfigureNegocio(this IServiceCollection services)
    {
        services.AddScoped<GestorDeAccionesDeRelacion>();
        services.AddScoped<GestorDeAccionesDeTrn>();
        services.AddScoped<GestorDeAccionesDeNegocio>();
        services.AddScoped<GestorDeEstados>();
        services.AddScoped<GestorDeNegocios>();
        services.AddScoped<GestorDeParametrosDeNegocio>();
        services.AddScoped<GestorDeClasesDelNegocio>();
        services.AddScoped<GestorDePlantillasDeCreacion>();
        services.AddScoped<GestorDePlantillasDeExportacion>();
        services.AddScoped<GestorDePlantillasDeFiltrado>();
        services.AddScoped<GestorDePlantillasDeExportacion>();
        services.AddScoped<GestorDeTransiciones>();
        services.AddScoped<GestorDePlantillasDeNegocio>();
    }

    public static void ConfigureCallejero(this IServiceCollection services)
    {
        services.AddScoped<GestorDeBarrios>();
        services.AddScoped<GestorDeBarriosDeUnaCalle>();
        services.AddScoped<GestorDeCalles>();
        services.AddScoped<GestorDeCallesDeUnaZona>();
        services.AddScoped<GestorDeCallesDeUnBarrio>();
        services.AddScoped<GestorDeCodigosPostales>();
        services.AddScoped<GestorDeCpsDeUnaCalle>();
        services.AddScoped<GestorDeCpsDeUnaProvincia>();
        services.AddScoped<GestorDeCpsDeUnMunicipio>();
        services.AddScoped<GestorDeMunicipios>();
        services.AddScoped<GestorDePaises>();
        services.AddScoped<GestorDeProvincias>();
        services.AddScoped<GestorDeTiposDeVia>();
        services.AddScoped<GestorDeZonas>();
        services.AddScoped<GestorDeZonasDeUnaCalle>();

    }

    public static void ConfigureTrabajosSometidos(this IServiceCollection services)
    {
        services.AddScoped<GestorDeCorreos>();
        services.AddScoped<GestorDeErroresDeUnTrabajo>();
        services.AddScoped<GestorDeTrabajosDeUsuario>();
        services.AddScoped<GestorDeTrabajosSometido>();
        services.AddScoped<GestorDeTrazasDeUnTrabajo>();
    }

    public static void ConfigureTerceros(this IServiceCollection services)
    {
        services.AddScoped<GestorDeClasesDeJuzgado>();
        services.AddScoped<GestorDeSociedades>();
        services.AddScoped<GestorDeCuentasDeMiSociedad>();
        services.AddScoped<GestorDeTarjetasDeMiSociedad>();
        services.AddScoped<GestorDeCentrosGestores>();
        services.AddScoped<GestorDeNegociosDeUnCg>();
        services.AddScoped<GestorDePersonas>();
        services.AddScoped<GestorDeInterlocutores>();
        services.AddScoped<GestorDeContactos>();
        services.AddScoped<GestorDeJuzgados>();
        services.AddScoped<GestorDeProcuradores>();
        services.AddScoped<GestorDeAbogados>();
        services.AddScoped<GestorDeProveedores>();
        services.AddScoped<GestorDeCuentasDeInterlocutor>();
        services.AddScoped<GestorDeCuentasDeTrabajador>();
        services.AddScoped<GestorDeCuentasDeProveedor>();
        services.AddScoped<GestorDeClientes>();
        services.AddScoped<GestorDeCuentasDeCliente>();
        services.AddScoped<GestorDeTrabajadores>();
        services.AddScoped<GestorDeBancos>();
        services.AddScoped<GestorDeUsuariosDeCliente>();
        services.AddScoped<GestorDePuestosDeCliente>();
        services.AddScoped<GestorDeParametrosDeMiSociedad>();
        services.AddScoped<GestorDeCentrosAdministrativos>();
        services.AddScoped<GestorDeBuzonesDeMiSociedad>();
        services.AddScoped<GestorDeFacturadorDeSociedades>();
    }

    public static void ConfigureSistemaDocumental(this IServiceCollection services)
    {
        services.AddScoped<GestorDeArchivadores>();
        services.AddScoped<GestorDeArchivos>();
        services.AddScoped<GestorDeCarpetas>();
        services.AddScoped<GestorDeCircuitosDoc>();
        services.AddScoped<GestorDeFirmados>();
        services.AddScoped<GestorDeDatosDeActividadesFormativas>();
        services.AddScoped<GestorDeVoluntariosDeActividades>();
        services.AddScoped<GestorDeInscritosEnActividades>();
    }

    public static void ConfigureRegistroEs(this IServiceCollection services)
    {
        services.AddScoped<GestorDeRegistrosEs>();
    }

    public static void ConfigureTareas(this IServiceCollection services)
    {
        services.AddScoped<GestorDeTareas>();
        services.AddScoped<GestorDePlfDeTareas>();
    }

    public static void ConfigureExpedientes(this IServiceCollection services)
    {
        services.AddScoped<GestorDeExpedientes>();
        services.AddScoped<GestorDeApuntesDeExpediente>();
        services.AddScoped<GestorDeDatosJuridicos>();
    }

    public static void ConfigureJuridica(this IServiceCollection services)
    {
        services.ConfigurePleitos();
        services.ConfigureContratos();
    }

    public static void ConfigurePleitos(this IServiceCollection services)
    {
        services.AddScoped<GestorDePleitos>();
        services.AddScoped<GestorDeRecobros>();
        services.AddScoped<GestorDeMinutas>();
    }
    
    public static void ConfigureContratos(this IServiceCollection services)
    {
        services.AddScoped<GestorDeAvalesSolicitados>();
        services.AddScoped<GestorDeAvances>();
        services.AddScoped<GestorDeContratos>();
        services.AddScoped<GestorDeDatosDelContrato>();
        services.AddScoped<GestorDeLineasDeUnPlfVenta>();
        services.AddScoped<GestorDeLotesDeUnContrato>();
        services.AddScoped<GestorDelPlanificadorDeVentas>();
        services.AddScoped<GestorDeProrrogas>();
        services.AddScoped<GestorDeSaldosDelContrato>();
        services.AddScoped<GestorDeUnitariosDeUnLote>();
        services.AddScoped<GestorDeMatriculasDeGuarderia>();
    }

    public static void ConfigureMaestrosTecnicos(this IServiceCollection services)
    {
        services.AddScoped<GestorDeNaturalezas>();
        services.AddScoped<GestorDeTarifas>();
        services.AddScoped<GestorDeUnidades>();
        services.AddScoped<GestorDeUnitarios>();
    }

    public static void ConfigureContabilidad(this IServiceCollection services)
    {
        services.AddScoped<GestorDeCuentas>();
        services.AddScoped<GestorDeIvasRepercutido>();
        services.AddScoped<GestorDeIvasSoportado>();
        services.AddScoped<GestorDeIrpfs>();
        services.AddScoped<GestorDePreasientos>();
    }

    public static void ConfigurePresupuestos(this IServiceCollection services)
    {
        services.AddScoped<GestorDeLineasDeUnPpt>();
        services.AddScoped<GestorDePptsDeVenta>();
        services.AddScoped<GestorDePresupuestos>();
    }

    public static void ConfigureVentas(this IServiceCollection services)
    {
        services.AddScoped<GestorDeFacturasEmt>();
        services.AddScoped<GestorDePartesTr>();
        services.AddScoped<GestorDePlanificacionesDeVenta>();
        services.AddScoped<GestorDeLineasDeUnaPlfVenta>();
        services.AddScoped<GestorDeLineasDeUnaFae>();
        services.AddScoped<GestorDeLineasDeUnPtr>();
        services.AddScoped<GestorDeAsignacionesDePtr>();
        services.AddScoped<GestorDeCobrosDeFae>();
        services.AddScoped<GestorDeRectificativasEmt>();
        services.AddScoped<GestorDePeriodosEmt>();
        services.AddScoped<GestorDeIrpfsEmt>();
        services.AddScoped<GestorDeVerifactu>();
        services.AddScoped<GestorDeRemesasFae>();
        services.AddScoped<GestorDeFacturasEmtDeUnaRemesa>();
        services.AddScoped<GestorDeAbonosDeFae>();
        services.AddScoped<Facturador>();
    }

    public static void ConfigureGastos(this IServiceCollection services)
    {
        services.AddScoped<GestorDeRemesasPag>();
        services.AddScoped<GestorDePagos>();
        services.AddScoped<GestorDePagosDeUnaRemesa>();
        services.AddScoped<GestorDeFacturasRec>();
        services.AddScoped<GestorDeLineasDeUnaFar>();
    }


    public static void ConfigureLogistica(this IServiceCollection services)
    {
        services.AddScoped<GestorDePedidos>();
        services.AddScoped<GestorDeLineasDeUnPedido>();
    }

    public static void ConfigureGuarderias(this IServiceCollection services)
    {
        services.AddScoped<GestorDeProfesDeCursoDeGuarderia>();
        services.AddScoped<GestorDeInfantesDeUnCurso>();
        services.AddScoped<GestorDeCursosDeGuarderia>();
        services.AddScoped<GestorDeAulasDeGuarderia>();
        services.AddScoped<GestorDeInfantes>();
    }
}