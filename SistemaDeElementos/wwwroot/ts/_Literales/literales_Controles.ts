// Tipos de control, expansores (espanes), modales, ampliaciones y datos del crud/mantenimiento.
// Depende de literales_Basicos y literales_Enumerados.
// Orden de carga: ListaDeScripts.cs. Al definirse, un literal solo puede leer los de ficheros anteriores de esa lista.

const ltrTipoControl = {
    Editor: 'editor',
    Check: 'check',
    SelectorDeFiltro: 'selector',
    SelectorDeElemento: 'selector-de-elemento',
    SelectorDeArchivos: 'selector-de-archivos',
    ListaDeElementos: 'lista-de-elemento',
    ListaDinamica: 'lista-dinamica',
    Link: 'link',
    ListaDeValores: 'lista-de-valores',
    ListaDeMenu: 'lista-de-menu',
    restrictorDeFiltro: 'restrictor-filtro',
    restrictorDeEdicion: 'restrictor-edicion',
    Archivo: 'archivo',
    UrlDeArchivo: 'url-archivo',
    SelectorDeUnArchivo: 'selector-de-un-archivo',
    VisorDeArchivo: 'visor-archivo',
    SelectorDeFecha: 'selector-de-fecha',
    SelectorDeFechaHora: 'selector-de-fecha-hora',
    FiltroEntreFechas: 'filtro-entre-fechas',
    FiltroEntreImportes: 'filtro-entre-importes',
    FiltroEntreRangos: 'filtro-entre-rangos',
    ConEditor: 'filtro-con-editor',
    ConLvEditor: 'filtro-con-lv-editor',
    CirculoEnCelda: 'circulo-en-celda',
    AreaDeTexto: 'area-de-texto',
    opcion: 'opcion',
    gridDeDetalle: 'grid-de-detalle',
    ampliacion: 'ampliacion-de-elemento',
    DesplegableDeFiltro: 'desplegable-de-filtro',
    GridModal: 'grid-modal',
    TablaBloque: 'tabla-bloque',
    Bloque: 'bloque',
    ZonaDeMenu: 'zona-menu',
    ZonaDeDatos: 'zona-de-datos',
    ZonaDeFiltro: 'zona-de-filtro',
    Menu: 'menu',
    VistaCrud: 'vista-crud',
    DescriptorDeCrud: 'descriptor-crud',
    Label: 'label',
    Referencia: 'referencia',
    ReferenciaPost: 'referencia-post',
    Lista: 'lista',
    Plantilla: 'plantilla',
    Mantenimiento: 'mantenimiento',
    Historial: 'historial',
    pnlCreador: 'panel-creador',
    pnlEditor: 'panel-editor',
    pnlExportacion: 'panel-exportacion',
    pnlEnviarCorreo: 'panel-enviar-correo',
    pnlTransitar: 'panel-transitar',
    pnlImprimir: 'panel-imprimir',
    pnlBorrado: 'panel-borrado',
    ImagenDelCanvas: 'imagen-de-canva',
    spanDeControles: 'span-de-controles'
};

const ltrEspanes = {
    Opcion: {
        crear: 'crear',
        crearRef: 'crear-ref',
        crearDt: 'creardt-ref',
        vincular: 'vincular-ref',
        crearRelacion: 'mcr-ref'
    },
    Atributos: {
        titulo: 'titulo',
    },
    Entorno: {
        Eventos: 'eventos'
    },
    observaciones: 'observaciones',
    direcciones: 'direcciones',
    tareas: 'tareas',
    TiposDeElemento: {
        Plantillas: 'detalle-plantillas',
        Clases: 'detalle-clases',
        ref: {
            CrearPlantilla: `detalle-plantillas-mcr-${enumPostfijoControl.Referencia}`,
            CrearClase: `detalle-clases-mcr-${enumPostfijoControl.Referencia}`
        },
    },
    Expedientes: {
        ppts: 'ppts',
        apuntes: 'apuntes',
        tareas: 'tareas',
        ctrsVenta: 'contratosVenta',
        ctrsCompra: 'contratosCompra',
        facturasRec: 'facturasRec',
        facturasEmt: 'facturaemt',
        ActividadesFormativas: 'actividadesformativas'
    },
    Tercero: {
        Sociedad: {
            Buzones: 'Buzones',
            CuentasBancarias: 'cuentasdemisociedad',
            TarjetasBancarias: 'tarjetasdemisociedad',
            Contactos: 'contactos',
            Facturador: 'facturador'
        },
        Interlocutor: {
            CuentasBancarias: 'cuentasdeinterlocutor'
        },
        Cliente: {
            CentroAdministrativo: 'centro-administrativo',
            ClienteWeb: 'UsuariosDeCliente',
            PuestoDeCliente: 'PuestosDeCliente',
            CuentasBancarias: 'cuentasdecliente'
        },
        Proveedor: {
            CuentasBancarias: 'cuentasdeproveedor'
        },
        Trabajador: {
            CuentasBancarias: 'cuentasdetrabajador'
        },
    },
    Juridico: {
        Contratos: {
            lotes: 'lotes',
            PlanificadorDeVentas: 'planificadorDeVentas',
            FacturasRec: 'facturasrec',
        },
        PlfdorDeVenta: {
            Lineas: 'lineasDeUnPlfVenta'
        }
    },
    Gasto: {
        FacturasRec: {
            Lineas: 'lineasdeunafar',
            Pagos: 'pagos',
        },
        RemesasPag: {
            Pagos: 'pagos',
        }
    },
    Logistica: {
        Pedidos: {
            Lineas: 'lineasdeunpedido',
            Entregas: 'entregasdeunpedido',
            Devoluciones: 'devolucionesdeunpedido'
        },
        Regularizaciones: {
            Lineas: 'lineasdeunaregularizacion'
        },
        Ventas: {
            Lineas: 'lineasdeunaventa',
            Devoluciones: 'devolucionesdeunaventa'
        },
        DevolucionesDeVenta: {
            Lineas: 'lineasdeunadevoluciondeventa'
        }
    },
    Venta: {
        Presupuestos: {
            Facturas: 'facturas',
            Partes: 'partes',
            Lineas: 'lineasdeunppt'
        },
        PartesTr: {
            Lineas: 'lineasdeunptr',
            Asignaciones: 'asignaciones'
        },
        FacturasEmt: {
            Lineas: 'lineasdeunafae',
            Cobros: 'CobrosDeFae',
            Abonos: 'AbonosDeFae'
        },
        PlfDeVenta: {
            Lineas: 'lineasdeunaplv',
        },
        RemesasFae: {
            Facturas: 'facturas',
        }
    }
};

const ltrGridDeUnExpansor = {
    Trazas: `${ltrTipoControl.gridDeDetalle}-trazas-contenedor`.toLowerCase(),
    Eventos: `${ltrTipoControl.gridDeDetalle}-eventos-contenedor`.toLowerCase()
};

const ltrModalDeVincular = {
    registrosDeEs: 'registroses'
};

const ltrModalDeFiltrado = {
    Venta: {
        PlfDeVenta: {
            FiltroDePlanificaciones: 'filtrodeplanificaciones'
        },
        ParteTr: {
            FiltroDePartesTr: 'filtrodepartestr'
        },
        FacturaEmt: {
            FiltroDeFacturasEmt: 'filtrodefacturasemt'
        }
    },
    Administracion: {
        Tarea: {
            FiltrosDeRelacion: 'filtrosDeRelacionesConTareas'
        }
    }
};

const ltrModalDeEditarRelacion = {
    Negocio: {
        PlantillasDeNegocio: 'plantillas',
        TipoDeElemento: {
            Plantillas: 'plantillas'
        }
    }
};

const ltrModalDeCrearRelacion = {
    Venta: {
        ParteTr: {
            Lineas: ltrEspanes.Venta.PartesTr.Lineas
        },
        PlfDeVenta: {
            Lineas: ltrEspanes.Venta.PlfDeVenta.Lineas
        },
        Presupuesto: {
            Lineas: ltrEspanes.Venta.Presupuestos.Lineas
        }
    },
    Logistica: {
        Pedidos: {
            Lineas: ltrEspanes.Logistica.Pedidos.Lineas
        },
        Regularizaciones: {
            Lineas: ltrEspanes.Logistica.Regularizaciones.Lineas
        },
        Ventas: {
            Lineas: ltrEspanes.Logistica.Ventas.Lineas
        },
        DevolucionesDeVenta: {
            Lineas: ltrEspanes.Logistica.DevolucionesDeVenta.Lineas
        }
    }
};

const ltrAmpliaciones = {
    controlador: literal.controlador,
    AccionTrasCrear: literal.accion,
    tipoDto: 'tipoDto',
    Comunes: {
        CrearDireccion: 'DireccionAlCrear'
    },
    sociedades: {
        parametros: 'Parametros',
        informacionContable: 'InformacionContable',
    },
    contratos: {
        datosDeVenta: 'CtrVenta',
        datosDeCompra: 'CtrCompra',
        saldos: 'Importes',
        avalsolicitado: 'avalsolicitado',
        avance: 'avance',
        prorroga: 'prorroga',
        DatosDelContratoDtm: 'DatosDelContratoDtm',
        MatriculaDeGuarderia: 'MatriculaDeGuarderia',
    },
    tareas: {
        planificacion: 'planificaciones',
    }
};

const ltrEdicion = {
    Editando: 'editando',
    IdDelEspan: 'IdDelEspan',
    TituloVisorConHistorial: 'Mostrando el historial'
};

const ltrMantenimiento = {
    CheckDeSeleccion: 'chksel',
    idCuerpoDePagina: 'cuerpo-de-pagina',
    // final del id del título de la cabecera del mantenimiento ({IdHtml}.menu.del.flujo, DescriptorDeMantenimiento.RenderMenuDelMnt)
    sufijoIdDelTitulo: '.menu.del.flujo',
    mostrarSoloSeleccionadas: 'Solo las seleccionadas',
    mostrarTodasLasFilas: 'Todas las filas',
    Indicadores: 'indicadores',
    Espanes: 'espanes',
    Filtros: 'filtros'
};

const ltrAccionesModal = {
    BloquearCg: 'bloquear-cg',
    ProponerCg: 'proponer-cg',
    ProponerSolicitante: 'proponer-solicitante',
    ProponerElTipo: 'proponer-el-tipo'
};

const ltrModal = {
    PanelDeControl: {
        CambiarPassword: 'cambiar-password',
        MiUsuario: 'mi-usuario',
        ia: 'ia',
        SubirCertificado:'subir-certificado'
    }
}

const ltrCrud = {
    Enumerados: {
        Edicion: {
            Carga: {
                Principal: "DatosDto",
                Ampliaciones: "Ampliaciones"
            }
        }
    }
}
