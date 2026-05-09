using System;

namespace Utilidades
{
    public static class AsignacionesDePartes
    {
        public static readonly string Menu = "Asignaciones";
        public static readonly string TituloMantenimiento = "Gestión de asignaciones";
        public static readonly string TituloEdicion = "Consulta de asignación";
        public static readonly string Icono = "AsignacionDePtr.svg";
    }

    public static class ltrEndPoint
    {
        public const string Controller = nameof(Controller);
        public const string epModificarRelacion = nameof(epModificarRelacion);
        public const string epCrearRelacion = nameof(epCrearRelacion);
        public const string epBorrarRelacionPorId = nameof(epBorrarRelacionPorId);
        public const string epModificarRelacionPorPost = nameof(epModificarRelacionPorPost);
        public const string epDescargaConGuid = nameof(epDescargaConGuid);
    }

    public static class ltrComandos
    {
        public const string ProponerCg = "proponer-cg";
        public const string ProponerElTipo = "proponer-el-tipo";
        public const string ProponerSolicitante = "proponer-solicitante";
    }
    public enum enumNameSpaceTs
    {
        MapearAlControl,
        Crud,
        ApiDelCrud,
        Formulario,
        Entorno,
        EntornoSe,
        Terceros,
        Callejero,
        Negocio,
        SistemaDocumental,
        TrabajosSometido,
        Seguridad,
        Administracion,
        ApiDeAgenda,
        ApiDeCertificados,
        ApiDePassword,
        ApiDeDireccion,
        Guarderias,
        Juridico,
        ApiDeArchivos,
        MaestrosTecnico,
        Contabilidad,
        Presupuesto,
        Venta,
        Gasto,
        Logistica
    }

    public static class enumFicheroDeApi
    {
        public static enumNameSpaceTs? EspacioDeNombre(enumNegocio negocio)
        {
            if (negocio == enumNegocio.Presupuesto) return enumNameSpaceTs.Venta;
            if (negocio == enumNegocio.Contrato) return enumNameSpaceTs.Juridico;
            return null;
        }
        public static string Tipo(enumNegocio negocio)
        {
            if (negocio == enumNegocio.Presupuesto) return $"{EspacioDeNombre(negocio)}/ApiDeTipoDePpt";
            if (negocio == enumNegocio.Contrato) return $"{EspacioDeNombre(negocio)}/ApiDeTipoDeCtr";
            return null;
        }
    }

    public enum enumControladoresCallejero
    {
        Barrios
            , BarriosDeUnaCalle
            , Calles
            , CallesDeUnaZona
            , CallesDeUnBarrio
            , CodigosPostales
            , CpsDeUnaCalle
            , CpsDeUnaProvincia
            , CpsDeUnMunicipio
            , ImportarCallejero
            , Municipios
            , Paises
            , Provincias
            , TiposDeVia
            , Zonas
            , ZonasDeUnaCalle
    }

    public static class enumVistasCallejero
    {
        public const string CrudPaises = nameof(CrudPaises);
        public const string CrudProvincias = nameof(CrudProvincias);
        public const string CrudCalles = nameof(CrudCalles);
        public const string CrudMunicipios = nameof(CrudMunicipios);
        public const string CrudBarrios = nameof(CrudBarrios);
        public const string CrudZonas = nameof(CrudZonas);
        public const string CrudTiposDeVia = nameof(CrudTiposDeVia);
        public const string CrudCodigosPostales = nameof(CrudCodigosPostales);
        public const string CrudCpsDeUnaProvincia = nameof(CrudCpsDeUnaProvincia);
        public const string CrudCpsDeUnMunicipio = nameof(CrudCpsDeUnMunicipio);
        public const string CrudCallesDeUnBarrio = nameof(CrudCallesDeUnBarrio);
        public const string CrudCallesDeUnaZona = nameof(CrudCallesDeUnaZona);
        public const string ImportarCallejero = nameof(ImportarCallejero);
    }


    public enum enumControladoresEntorno
    {
        ArbolDeMenu
        , Inicializador
        , Menus
        , PermisosDeUnUsuario
        , Usuarios
        , Variables
        , VistaMvc
        , Acciones
        , Agendas
        , VisorDeAgenda
        , JerarqiaMenus
        , TrabajosSometido
        , TrabajosDeUsuario
        , ErroresDeUnTrabajo
        , TrazasDeUnTrabajo
        , Correos
        , Certificados
        , MiCorreo
    }
    public static class enumVistasEntorno
    {
        public const string CrudMenu = nameof(CrudMenu);
        public const string CrudUsuario = nameof(CrudUsuario);
        public const string CrudPuestoDeTrabajo = nameof(CrudPuestoDeTrabajo);
        public const string CrudVistaMvc = nameof(CrudVistaMvc);
        public const string AplicarSeguridad = nameof(AplicarSeguridad);
        public const string InicializarBD = nameof(InicializarBD);
        public const string CrudDeAcciones = nameof(CrudDeAcciones);
        public const string CrudAgendas = nameof(CrudAgendas);
        public const string InicializarMaestros = nameof(InicializarMaestros);
        public const string InicializarEntorno = nameof(InicializarEntorno);



        public const string CrudJerarqiaMenus = nameof(CrudJerarqiaMenus);
        public const string CrudDeTrabajosSometido = nameof(CrudDeTrabajosSometido);
        public const string CrudDeTrabajosDeUsuario = nameof(CrudDeTrabajosDeUsuario);
        public const string CrudDeErroresDeUnTrabajo = nameof(CrudDeErroresDeUnTrabajo);
        public const string CrudDeTrazasDeUnTrabajo = nameof(CrudDeTrazasDeUnTrabajo);
        public const string CrudDeCorreos = nameof(CrudDeCorreos);
        public const string VisorDeAgenda = nameof(VisorDeAgenda);
        public const string CrudCertificados = nameof(CrudCertificados);
        public const string MiCalendario = nameof(MiCalendario);
        public const string CrudDeMiCorreo = nameof(CrudDeMiCorreo);
        public const string CrudDeMiCorreoImap = nameof(CrudDeMiCorreoImap);
        public const string CrudDeMiCorreoApiKey = nameof(CrudDeMiCorreoApiKey);
        public const string Subcribirme = nameof(Subcribirme);


        public const string CrudVariable = nameof(CrudVariable);
    }

    public enum enumControladoresMt
    {
        Naturalezas,
        Unidades,
        Unitarios
    }

    public enum enumControladoresContables
    {
        Cuentas,
        IvasSoportado,
        IvasRepercutido,
        Irpfs,
        Preasientos
    }

    public enum enumControladoresTerceros
    {
        Sociedades,
        Bancos,
        CentrosGestores,
        Personas,
        Contactos,
        Interlocutores,
        Procuradores,
        Abogados,
        ClasesDeJuzgado,
        Juzgados,
        Proveedores,
        Clientes,
        Trabajadores,
        CuentasDeMiSociedad,
        TarjetasDeMiSociedad,
        CuentasDeCliente,
        CuentasDeProveedor,
        CentrosAdministrativos
    }
    public static class enumVistasTerceros
    {
        public const string CrudSociedades = nameof(CrudSociedades);
        public const string CrudCentrosGestores = nameof(CrudCentrosGestores);
        public const string CrudPersonas = nameof(CrudPersonas);
        public const string CrudTerceros = nameof(CrudTerceros);
        public const string CrudInterlocutores = nameof(CrudInterlocutores);
        public const string CrudProcuradores = nameof(CrudProcuradores);
        public const string CrudAbogados = nameof(CrudAbogados);
        public const string CrudClasesDeJuzgado = nameof(CrudClasesDeJuzgado);
        public const string CrudJuzgados = nameof(CrudJuzgados);
        public const string CrudProveedores = nameof(CrudProveedores);
        public const string CrudClientes = nameof(CrudClientes);
        public const string CrudTrabajadores = nameof(CrudTrabajadores);
        public const string CrudBancos = nameof(CrudBancos);
    }


    public enum enumControladoresNegocio
    {
        Auditoria
        , Negocio
        , ParametrosDeNegocio
        , PlantillasDeCreacion
        , PlantillasDeExportacion
        , PlantillasDeFiltrado
        , TiposDeElemento
        , Estados
        , Transiciones
        , AccionesDeTransicion
        , AccionesDeRelacion
        , ClasesDelNegocio
    }

    public static class enumVistasNegocio
    {
        public const string CrudDeEstados = nameof(CrudDeEstados);
        public const string CrudDeTransiciones = nameof(CrudDeTransiciones);
        public const string CrudDeAccionesDeTransicion = nameof(CrudDeAccionesDeTransicion);
        public const string CrudDeNegocios = nameof(CrudDeNegocios);
        public const string CrudDePlantillasDeExportacion = nameof(CrudDePlantillasDeExportacion);
        public const string CrudDeAuditoria = nameof(CrudDeAuditoria);
        public const string CrudDeParametrosDeNegocio = nameof(CrudDeParametrosDeNegocio);
        public const string CrudDeAccionesDeRelacion = nameof(CrudDeAccionesDeRelacion);


        public static string CrudDeTipos(enumNegocio negocio)
        {
            switch (negocio)
            {
                case enumNegocio.Archivador:
                    return enumVistasSistemaDocumental.TiposDeArchivador;
                case enumNegocio.Registro:
                    return enumVistasAdministrativo.TiposDeRegistroEs;
                case enumNegocio.Tarea:
                    return enumVistasAdministrativo.TiposDeTarea;
                case enumNegocio.Expediente:
                    return enumVistasAdministrativo.TiposDeExpediente;
                case enumNegocio.Pleito:
                    return enumVistasJuridicos.TiposDePleito;
                case enumNegocio.Contrato:
                    return enumVistasJuridicos.TiposDeContrato;
                case enumNegocio.Presupuesto:
                    return enumVistasVentas.TiposDePresupuesto;
                case enumNegocio.ParteDeTrabajo:
                    return enumVistasVentas.TiposDeParteTr;
                case enumNegocio.FacturaEmitida:
                    return enumVistasVentas.TiposDeFacturaEmt;
                case enumNegocio.PlanificacionDeVenta:
                    return enumVistasVentas.TiposDePlanificacionDeVenta;
                case enumNegocio.RemesaFae:
                    return enumVistasVentas.TiposDeRemesaFae;
                case enumNegocio.CircuitoDoc:
                    return enumVistasSistemaDocumental.TiposDeCircuitoDoc;
                case enumNegocio.Pago:
                    return enumVistasGastos.TiposDePago;
                case enumNegocio.RemesaPag:
                    return enumVistasGastos.TiposDeRemesaPag;
                case enumNegocio.FacturaRecibida:
                    return enumVistasGastos.TiposDeFacturaRec;
                case enumNegocio.Pedido:
                    return enumVistasLogisticas.TiposDePedido;
                case enumNegocio.Preasiento:
                    return enumVistasContables.TiposDePreasiento;
            }
            throw new Exception($"No se ha definido la vista de tipos a la que navegar para el negocio {negocio}");
        }
    }

    public enum enumControladoresSeguridad
    {
        Acceso
        , ClaseDePermiso
        , TipoDePermiso
        , Permisos
        , PermisosDeUnPuesto
        , PermisosDeUnRol
        , PuestoDeTrabajo
        , PuestosDeUnRol
        , PuestosDeUnUsuario
        , Rol
        , RolesDeUnPermiso
        , RolesDeUnPuesto
        , UsuariosDeUnPuesto
        , PermisosDeUnUsuario
        , PermisosHeredados
    }

    public static class enumVistasSeguridad
    {
        public const string CrudPermiso = nameof(CrudPermiso);
        public const string CrudPuestoDeTrabajo = nameof(CrudPuestoDeTrabajo);
        public const string CrudRol = nameof(CrudRol);
        public const string CrudClaseDePermiso = nameof(CrudClaseDePermiso);
        public const string CrudPuestosDeUnUsuario = nameof(CrudPuestosDeUnUsuario);
        public const string CrudRolesDeUnPuesto = nameof(CrudRolesDeUnPuesto);
        public const string CrudPermisosDeUnRol = nameof(CrudPermisosDeUnRol);
        public const string CrudPermisosDeUnUsuario = nameof(CrudPermisosDeUnUsuario);
        public const string CrudRolesDeUnPermiso = nameof(CrudRolesDeUnPermiso);
        public const string CrudPuestosDeUnRol = nameof(CrudPuestosDeUnRol);
        public const string CrudUsuariosDeUnPuesto = nameof(CrudUsuariosDeUnPuesto);
        public const string CrudPermisosHeredados = nameof(CrudPermisosHeredados);
        public const string Conectar = nameof(Conectar);
        public const string NuevaContrasena = nameof(NuevaContrasena);
    }

    public enum enumControladoresTrabajosSometidos
    {
        Correos
        , ErroresDeUnTrabajo
        , TrabajosDeUsuario
        , TrabajosSometido
        , TrazasDeUnTrabajo
    }


    public enum enumControladoresSistemaDocumental
    {
        Archivadores
      , CircuitosDoc
      , Archivos
      , Carpetas
    }

    public static class enumVistasSistemaDocumental
    {
        public const string TiposDeArchivador = nameof(TiposDeArchivador);
        public const string CrearArchivadoresEco = nameof(CrearArchivadoresEco);
        public const string CrudArchivadores = nameof(CrudArchivadores);
        public const string CrudCarpetas = nameof(CrudCarpetas);
        public const string TiposDeCircuitoDoc = nameof(TiposDeCircuitoDoc);
        public const string CrudCircuitosDoc = nameof(CrudCircuitosDoc);
        public const string CrudEstimacionesDirectas = nameof(CrudEstimacionesDirectas);
        public const string CrudLotesContables = nameof(CrudLotesContables);
        public const string CrudFichadas = nameof(CrudFichadas);
        public const string CrudActividadesFormativas = nameof(CrudActividadesFormativas);
        public const string MaestrosDeCircuitosDoc = nameof(MaestrosDeCircuitosDoc);
        public const string DescriptorDeConsultaDeCad = nameof(DescriptorDeConsultaDeCad);
    }
    public static class enumAccionesSistemaDocumental
    {
        public const string epDescargarThumsnail = nameof(epDescargarThumsnail);
        public const string RenombrarPlantilla = nameof(RenombrarPlantilla);   
        public const string RenombrarArchivo = nameof(RenombrarArchivo);
    }


    public enum enumControladoresAdministrativos
    {
        RegistrosEs,
        Tareas,
        Expedientes
    }
    public static class enumVistasAdministrativo
    {
        public const string TiposDeRegistroEs = nameof(TiposDeRegistroEs);
        public const string TiposDeTarea = nameof(TiposDeTarea);
        public const string CrudTareas = nameof(CrudTareas);
        public const string CrudRegistrosEs = nameof(CrudRegistrosEs);
        public const string CrudExpedientes = nameof(CrudExpedientes);
        public const string CrudActividades = nameof(CrudActividades);
        public const string TiposDeExpediente = nameof(TiposDeExpediente);
        public const string MaestrosDeExpedientes = nameof(MaestrosDeExpedientes);
        public const string MaestrosDeTareas = nameof(MaestrosDeTareas);
        public const string MaestrosDeProcedimientos = nameof(MaestrosDeProcedimientos);
        public const string DescriptorDeConsultaDeTarea = nameof(DescriptorDeConsultaDeTarea);
    }

    public enum enumControladoresJuridicos
    {
        Pleitos,
        Recobros,
        Contratos,
        LotesDeUnContrato,
        UnitariosDeUnLote,
        PlanificadorDeVentas,
        PlanificadorDeCompras
    }
    public static class enumVistasJuridicos
    {
        public const string CrudPleitos = nameof(CrudPleitos);
        public const string TiposDePleito = nameof(TiposDePleito);
        public const string TiposDeContrato = nameof(TiposDeContrato);
        public const string CrudContratos = nameof(CrudContratos);
        public const string InicializarEtapas = nameof(InicializarEtapas);
        public const string SometerTrabajos = nameof(SometerTrabajos);
        public const string MaestrosDeContratos = nameof(MaestrosDeContratos);
        public const string CrudLotes = nameof(CrudLotes);
        public const string CrudUnitariosDeUnLote = nameof(CrudUnitariosDeUnLote);
        public const string CrudPlanificadorDeVentas = nameof(CrudPlanificadorDeVentas);
        public const string CrudPlanificadorDeCompras = nameof(CrudPlanificadorDeCompras);
    }

    public enum enumControladoresVentas
    {
        Presupuestos,
        PartesTr,
        FacturasEmt,
        PlanificacionesDeVenta,
        AsignacionesDePtr,
        RemesasFae,
        FacturasEmtDeUnaRemesa
    }


    public enum enumControladoresGastos
    {
        Pagos,
        RemesasPag,
        PagosDeUnaRemesa,
        FacturasRec
    }
    public enum enumControladoresGuarderias
    {
        AulasDeGuarderia,
        Infantes,
        CursosDeGuarderia,
        InfantesDeUnCurso
    }

    public enum enumControladoresLogistica
    {
        PlanificacionesDeCompra,
        Pedidos,
        Albaranes,
        Almacenes
    }

    public static class ltrVistasTitulos
    {
        public const string CrudDeFacturasAeat = "Consultar en AEAT";
    }

    public static class enumVistasVentas
    {
        public const string CrudPresupuestos = nameof(CrudPresupuestos);
        public const string TiposDePresupuesto = nameof(TiposDePresupuesto);
        public const string MaestrosDePresupuestos = nameof(MaestrosDePresupuestos);
        public const string TiposDeParteTr = nameof(TiposDeParteTr);
        public const string MaestrosDePartesTr = nameof(MaestrosDePartesTr);
        public const string TiposDeFacturaEmt = nameof(TiposDeFacturaEmt);
        public const string MaestrosDeFacturasEmt = nameof(MaestrosDeFacturasEmt);
        public const string MaestrosDeRemesasFae = nameof(MaestrosDeRemesasFae);        
        public const string TiposDePlanificacionDeVenta = nameof(TiposDePlanificacionDeVenta);
        public const string MaestrosDePlanificacionesDeVenta = nameof(MaestrosDePlanificacionesDeVenta);
        public const string CrudPlanificacionesDeVenta = nameof(CrudPlanificacionesDeVenta);
        public const string CrudPartesDeTrabajo = nameof(CrudPartesDeTrabajo);
        public const string CrudFacturasEmt = nameof(CrudFacturasEmt);
        public const string CrudFacturasAeat = nameof(CrudFacturasAeat);
        public const string SometerTrabajos = nameof(SometerTrabajos);
        public const string CrudAsignacionesPtr = nameof(CrudAsignacionesPtr);
        public const string TiposDeRemesaFae = nameof(TiposDeRemesaFae);        
        public const string CrudRemesasFae = nameof(CrudRemesasFae);
        public const string CrudFacturasEmtDeUnaRemesa = nameof(CrudFacturasEmtDeUnaRemesa);
        public const string epValidarQr = nameof(epValidarQr);        
    }

    public static class enumVistasLogisticas
    {
        public const string TiposDePedido = nameof(TiposDePedido);
        public const string CrudPedidos = nameof(CrudPedidos);
        public const string MaestrosDePedidos = nameof(MaestrosDePedidos);
    }

    public static class enumVistasGastos
    {
        public const string TiposDePago = nameof(TiposDePago);
        public const string TiposDeRemesaPag = nameof(TiposDeRemesaPag);
        public const string TiposDeFacturaRec = nameof(TiposDeFacturaRec);
        public const string CrudPagos = nameof(CrudPagos);
        public const string CrudRemesasPag = nameof(CrudRemesasPag);
        public const string CrudFacturasRec = nameof(CrudFacturasRec);
        public const string MaestrosDePagos = nameof(MaestrosDePagos);
        public const string MaestrosDeRemesasPag = nameof(MaestrosDeRemesasPag);
        public const string MaestrosDeFacturasRec = nameof(MaestrosDeFacturasRec);
        public const string CrudPagosDeUnaRemesa = nameof(CrudPagosDeUnaRemesa);
    }
    public static class enumVistasGuarderias
    {
        public const string CrudCursosDeGuarderia = nameof(CrudCursosDeGuarderia);
        public const string CrudAulasDeGuarderia = nameof(CrudAulasDeGuarderia);
        public const string CrudInfantes = nameof(CrudInfantes);
        public const string CrudInfantesDeUnCurso = nameof(CrudInfantesDeUnCurso);
        public const string DescriptorDeConsultaDeInfante = nameof(DescriptorDeConsultaDeInfante);
    }
    
    public static class enumVistasMts
    {
        public const string CrudUnidades = nameof(CrudUnidades);
        public const string CrudNaturalezas = nameof(CrudNaturalezas);
        public const string CrudUnitarios = nameof(CrudUnitarios);
        public const string CrudTablasDePrecio = nameof(CrudTablasDePrecio);
    }

    public static class enumVistasContables
    {
        public const string CrudCuentas = nameof(CrudCuentas);
        public const string CrudIvasSoportado = nameof(CrudIvasSoportado);
        public const string CrudIvasRepercutido = nameof(CrudIvasRepercutido);
        public const string CrudIrpfs = nameof(CrudIrpfs);
        public const string MaestrosDePreasientos = nameof(MaestrosDePreasientos);
        public const string CrudPreasientos = nameof(CrudPreasientos);
        public const string TiposDePreasiento = nameof(TiposDePreasiento);
    }


}
