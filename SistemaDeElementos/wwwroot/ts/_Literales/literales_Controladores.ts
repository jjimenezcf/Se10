// Controladores MVC, vistas y páginas de los mantenimientos.
// No depende de ningún otro fichero de literales.
// Orden de carga: ListaDeScripts.cs. Al definirse, un literal solo puede leer los de ficheros anteriores de esa lista.

const ltrPaginas = {
    Terceros: {
        Persona: "crud_personadto_mantenimiento",
        Sociedad: "crud_sociedaddto_mantenimiento",
        Interlocutor: "crud_interlocutordto_mantenimiento",
        Cliente: 'crud_clientedto_mantenimiento',
        Proveedor: 'crud_proveedordto_mantenimiento',
        Procurador: "crud_procuradordto_mantenimiento",
        Abogado: "crud_abogadodto_mantenimiento",
        Trabajador: "crud_trabajadordto_mantenimiento",
        CentroGestor: "cg"
    },
    Callejero: {
        googleMaps: "MostrarDireccion",
    }
};

const ltrVistas = {
    Entorno: {
        Conectar: 'Conectar',
        // página de inicio con el dashboard (no es un crud): nombre de vista con el que se pide su manual (Entorno/Home/PanelDeControl.md)
        PanelDeControl: 'PanelDeControl',
    },
    // vista con la que se pide el manual de las páginas de consulta por guid (acción Consultar de EntidadController; ApiDeManuales.VistaDeConsultaPorGuid)
    ConsultaPorGuid: 'Consultar'
}

const ltrControladores = {
    Comunes: {
        Home: 'Home',
        Trazas: 'Trazas',
        Observaciones: 'Observaciones',
        Hitos: 'Hitos'
    },
    SisDoc: {
        Archivos: 'Archivos',
        Archivador: 'Archivadores',
        CircuitosDoc: 'CircuitosDoc'
    },
    Contabilidad: {
        Cuentas: 'Cuentas',
        Preasientos: 'Preasientos'
    },
    Terceros: {
        Sociedad: {
            Tarjetas: 'TarjetasDeMiSociedad'
        },
        Sociedades: 'Sociedades',
        CentrosGestores: 'CentrosGestores',
        Clientes: 'Clientes',
        Interlocutores: 'Interlocutores',
        Proveedores: 'Proveedores',
        InformacionContableDeSociedad: 'InformacionContableDeSociedad'
    },
    Callejero: {
        Provincia: 'Provincias',
        Municipio: 'Municipios',
        Calles: 'Calles'
    },
    Entorno: {
        Acciones: 'Acciones',
        MiCorreo: 'MiCorreo',
        Correos: 'Correos',
        MensajesWeb: 'MensajesWeb',
        VisorDeAgenda: 'VisorDeAgenda',
        ArbolDeMenu: 'ArbolDeMenu',
        Menus: 'Menus',
        Acceso: 'Acceso',
        Usuarios:'Usuarios'
    },
    Negocio: {
        TiposDeElemento: 'TiposDeElemento',
        PlantillasDeFiltrado: 'PlantillasDeFiltrado',
        PlantillasDeCreacion: 'PlantillasDeCreacion',
        PlantillasDeExportacion: 'PlantillasDeExportacion',
        PlantillasDeNegocio: 'PlantillasDeNegocio',
        Estados: 'Estados',
        AccionesDeTrn: 'AccionesDeTrn'
    },
    Administracion: {
        Expedientes: 'Expedientes',
        Tareas: 'Tareas'
    },
    Venta: {
        Presupuestos: 'Presupuestos',
        LineasDeFactura: 'LineasDeUnaFae',
        FacturasEmt: 'FacturasEmt',
        RemesasFae: 'RemesasFae'
    },
    Gasto: {
        Pagos: 'Pagos',
        FacturasRec: 'FacturasRec',
        RemesasPag: 'RemesasPag',
        LineasDeFactura: 'LineasDeUnaFar',
    },
    Logistica: {
        Pedidos: 'Pedidos',
        LineasDeUnPedido: 'LineasDeUnPedido',
        EntregasDeUnPedido: 'EntregasDeUnPedido',
        DevolucionesDeUnPedido: 'DevolucionesDeUnPedido',
        Ventas: 'Ventas',
        DevolucionesDeVenta: 'DevolucionesDeVenta'
    },
    Guarderias: {
        Cursos: 'CursosDeGuarderia',
    },
    Juridico: {
        Contratos: 'Contratos',
        PlanificadorDeVentas: 'PlanificadorDeVentas',
        DatosDelContrato: 'CtrVentas'
    }
};
