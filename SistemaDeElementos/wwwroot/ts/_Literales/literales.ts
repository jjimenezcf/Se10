const newLine = '\n';

const TipoMensaje = { Info: 'informativo', Error: 'Error', Warning: 'Revision' };

const ltrSimbolos = {
    dosPuntosConEspacio: ': ',
    separadorDeRangos: ';',
    separadorDeDosFechas: '-',
    separadorDeDDMMYYYY: '/',
    separadorDeHHMMSS: ':',
    separadorDeCss: ';',
    separadorDeParametrosJs: '#',
    separadorDeValores: '|',
    separadorDeEnteros: ',',
    ArchivoCancelado: '(_)',
    igual: '='
};

enum enumNegocio {
    No_Definido = 'No_Definido',
    Usuario = 'Usuario',
    VistaMvc = 'VistaMvc',
    Agenda = 'Agenda',
    EventoDeAgenda = 'EventoDeAgenda',
    Variable = 'Variable',
    Accion = 'Accion',
    Menu = 'Menu',
    Puesto = 'Puesto',
    PermisosDeUnUsuario = 'PermisosDeUnUsuario',
    Permiso = 'Permiso',
    Negocio = 'Negocio',
    Rol = 'Rol',
    ClasePermiso = 'ClasePermiso',
    PermisosDeUnPuesto = 'PermisosDeUnPuesto',
    PermisosDeUnRol = 'PermisosDeUnRol',
    PuestosDeUnRol = 'PuestosDeUnRol',
    PuestosDeUnUsuario = 'PuestosDeUnUsuario',
    RolesDeUnPermiso = 'RolesDeUnPermiso',
    RolesDeUnPuesto = 'RolesDeUnPuesto',
    TipoPermiso = 'TipoPermiso',
    Archivos = 'Archivos',
    Pais = 'Pais',
    Provincia = 'Provincia',
    Municipio = 'Municipio',
    TipoDeVia = 'TipoDeVia',
    Calle = 'Calle',
    Barrio = 'Barrio',
    Zona = 'Zona',
    Correo = 'Correo',
    Sociedad = 'Sociedad',
    CentroGestor = 'CentroGestor',
    Archivador = 'Archivador',
    CircuitoDoc = 'CircuitoDoc',
    Carpeta = 'Carpeta',
    Persona = 'Persona',
    Contacto = 'Contacto',
    Interlocutor = 'Interlocutor',
    Registro = 'Registro',
    Tarea = 'Tarea',
    Expediente = 'Expediente',
    Procurador = 'Procurador',
    Abogado = 'Abogado',
    Juzgado = 'Juzgado',
    Banco = 'Banco',
    Pleito = 'Pleito',
    Certificado = 'Certificado',
    Unitario = 'Unitario',
    Proveedor = 'Proveedor',
    Cliente = 'Cliente',
    Trabajador = 'Trabajador',
    Presupuesto = 'Presupuesto',
    ParteDeTrabajo = 'ParteDeTrabajo',
    Contrato = 'Contrato',
    LoteDeUnContrato = 'LoteDeUnContrato',
    PlanificadorDeVenta = 'PlanificadorDeVenta',
    FacturaEmitida = 'FacturaEmitida',
    PlanificacionDeVenta = 'PlanificacionDeVenta',
    PlanificacionDeCompra = 'PlanificacionDeCompra',
    RemesaFae = 'RemesaFae',
    Pago = 'Pago',
    RemesaPag = 'RemesaPag',
    FacturaRecibida = 'FacturaRecibida',
    Infante = 'Infante',
    CursoDeGuarderia = 'CursoDeGuarderia',
    Preasiento = 'Preasiento'
}

const literal = {
    ClaveDeEncriptacion: 'SistemaDeElementos',
    controlador: 'controlador',
    accion: 'accion',
    vista: 'vista',
    parametros: 'parametros',
    urlAccedida: 'urlAccedida',
    idNegocio: 'id-negocio',
    nombreNegocio: 'nombre-negocio',
    negocio: 'negocio',
    enumNegocio: 'enumNegocio',
    idCg: 'idcg',
    Cg: 'cg',
    IdSolicitante: 'idsolicitante',
    Solicitante: 'solicitante',
    id: 'id',
    IdTipo: 'idtipo',
    Tipo: 'Tipo',
    true: 'true',
    false: 'false',
    cero: '0',
    uno: '1',
    Hoy: 'Hoy',
    undefined: 'undefined',
    nombre: 'nombre',
    expresion: 'expresion',
    padre: 'padre',
    ModoDeAcceso: 'ModoDeAcceso',
    filtro: {
        clausulaId: 'id',
        idEditado: 'ideditado',
        vincularCon: 'vincularCon',
        PorVariable: 'porvariable',
        criterio: {
            igual: 'igual',
            diferente: 'diferente',
            entreFechas: 'entrefechas',
            entreImportes: 'entreimportes',
            entreRangos: 'entrerangos'
        }
    },
    menos1: '-1',
    ExpanDeArchivos: "detalle-archivos"
};

const ltrPrefijo = {
    Modal: 'modal-'
}

const ltrPosfijo = {
    Firma: '-firma'
}

const ltrVariables = {
    Cuentas: {
        Sueldos: 'Sueldos'
    }
};


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
    },
    Gastos: {
        FacturaRec: "crud_facturarecdto_mantenimiento"
    }
};

const enumClaseDeContrato = {
    Venta: 'Venta',
    Compra: 'Compra',
    Subvencion: 'Subvencion',
    Colaboracion: 'Colaboracion',
    Marco: 'Marco',
    MatriculaDeGuarderia: 'MatriculaDeGuarderia'
};


const enumModoDePagoContado = {
    Contado: 'Contado',
    Tarjeta: 'Tarjeta',
    Domiciliacion: 'Domiciliacion'
};

const enumTipoDeTercero = {
    Autonomo: 'Autonomo',
    Empresa: 'Empresa',
    Nie: 'Nie',
    Invalido: 'Invalido',
}


const enumPostfijoTipoModal = {
    ModalDeCrearRelacion: 'modaldecrearrelacion',
    ModalDeCrearVinculo: 'modaldecrearvinculo',
    ModalParaVincular: 'modalparavincular',
    ModalDeEditarRelacion: 'modaldeeditarrelacion',
    ModalDeCrearDetalle: 'modaldecreardetalle'
}

const enumPostfijoControl = {
    Referencia: 'ref.ref',
    CrearPost: 'crear.ref'
}

const enumTipoDeModal = {
    ModalDeSeleccion: 'modal-de-seleccion',
    ModalDeRelacion: 'modal-de-relacion',
    ModalDeMensaje: 'modal-de-mensaje',
    ModalDeConsulta: 'modal-de-consulta',
    ModalParaSeleccionar: 'modal-para-seleccionar',
    ModalDeCrearRelacion: 'modal-de-crear-relacion',
    ModalDeEditarRelacion: 'modal-de-editar-relacion',
    ModalDeCreacion: 'modal-de-creacion',
    ModalDeEdicion: 'modal-de-edicion',
    ModalDeBorrado: 'modal-de-borrado',
    ModalDeCorreo: 'modal-de-correo',
    ModalDeExportacion: 'modal-de-exportacion',
    ModalDeVisorDeArchivos: 'modal-de-visor-de-archivo',
    ModalDeFirmaDeArchivos: 'modal-de-firma-de-archivo',
    ModalDeDatosDeFirma: 'modal-de-datos-de-firma',
    ModalDeDatosDeBloqueo: 'modal-de-datos-de-bloqueo',
    ModalParaVincular: 'modal-para-vincular',
    ModalParaPedirDatos: 'modal-para-pedir-datos',
    ModalMiCertificado: 'modal-mi-certificado',
    ModalDeFiltrado: 'modal-de-filtrado',
    ModalDeTransitar: 'modal-de-transitar',
    ModalDeImprimir: 'modal-de-imprimir',
    ModalDeCrearDetalle: "modal-de-crear-detalle",
    ModalDeCrearVinculo: "modal-de-crear-vinculo"
};

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
    Tipos: {
        Ampliacion: 'Ampliaciones',
        Detalles: 'Detalles',
        Bloques: 'Bloques'
    },
    Opcion: {
        crear: 'crear',
        crearRef: 'crear-ref',
        crearDt: 'creardt-ref',
        vincular: 'vincular-ref',
        Editar: 'editar'
    },
    Atributos: {
        titulo: 'titulo',
    },
    Entorno: {
        Eventos: 'eventos'
    },
    auditoria: 'audt',
    hitos: 'hitos',
    trazas: 'trazas',
    observaciones: 'observaciones',
    direcciones: 'direcciones',
    solicitantes: 'solicitantes',
    tareas: 'tareas',
    archivadores: 'archivadores',
    archivos: 'archivos',
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
        CrearValoracion: 'ppts',
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
            registrosEs: 'registroes',
            lotes: 'lotes',
            PlanificadorDeVentas: 'planificadorDeVentas',
            CopiarPlfDeVenta: 'planificadorDeVentas',
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
            Lineas: 'lineasdeunpedido'
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
            Abonos: 'AbonosDeFae',
            Rectificadas: 'rectificadas'
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
    Tercero: {
        Cliente: {
            CuentasBancarias: ltrEspanes.Tercero.Cliente.CuentasBancarias
        },
        Sociedad: {
            Buzones: ltrEspanes.Tercero.Sociedad.Buzones,
            CuentasBancarias: ltrEspanes.Tercero.Sociedad.CuentasBancarias,
            TarjetasBancarias: ltrEspanes.Tercero.Sociedad.TarjetasBancarias
        }
    },
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
        }
    }
};

const ltrVistas = {
    Entorno: {
        Conectar: 'Conectar',
    }
}

const ltrControladores = {
    Comunes: {
        Home: 'Home',
        Trazas: 'Trazas',
        Archivadores: 'Archivadores',
        Observaciones: 'Observaciones',
        Hitos: 'Hitos',
        Base: 'Base'
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
        Proveedores: 'Proveedores'
    },
    MaestrosTecnico: {
        Tarifas: 'Tarifas',
    },
    Callejero: {
        Provincia: 'Provincias',
        Municipio: 'Municipios'
    },
    Entorno: {
        Acciones: 'Acciones',
        MiCorreo: 'MiCorreo',
        VisorDeAgenda: 'VisorDeAgenda',
        ArbolDeMenu: 'ArbolDeMenu',
        Menus: 'Menus',
        Acceso: 'Acceso'
    },
    Negocio: {
        TiposDeElemento: 'TiposDeElemento',
        AccionesDeRelacion: 'AccionesDeRelacion',
        PlantillasDeFiltrado: 'PlantillasDeFiltrado',
        PlantillasDeCreacion: 'PlantillasDeCreacion',
        PlantillasDeExportacion: 'PlantillasDeExportacion',
        PlantillasDeNegocio: 'PlantillasDeNegocio'

    },
    Administracion: {
        Expedientes: 'Expedientes',
        RegistrosEs: 'RegistrosEs',
        Tareas: 'Tareas'
    },
    Venta: {
        Presupuestos: 'Presupuestos',
        LineasDeFactura: 'LineasDeUnaFae',
        FacturasEmt: 'FacturasEmt',
        RemesasFae: 'RemesasFae',
        FacturasDeUnaRemesa: 'FacturasDeUnaRemesa'
    },
    Gasto: {
        Pagos: 'Pagos',
        FacturasRec: 'FacturasRec',
        RemesasPag: 'RemesasPag',
        LineasDeFactura: 'LineasDeUnaFar',
    },
    Guarderias: {
        Cursos: 'CursosDeGuarderia',
    },
    Juridico: {
        Contratos: 'Contratos',
        LotesDeUnContrato: 'LotesDeUnContrato',
        PlanificadorDeVentas: 'PlanificadorDeVentas',
        DatosDelContrato: 'CtrVentas'
    }
};

const atMenu = {
    abierto: 'menu-abierto',
    plegado: 'menu-plegado',
};


const atControl = {
    Guid: 'guid',
    propiedad: 'propiedad',
    metadatos: 'metadatos',
    mascara: 'pattern',
    placeholder: 'placeholder',
    EsFecha: 'es-fecha',
    formato: 'formato',
    estilo: 'style',
    anteriorDisplay: 'anterior-display',
    ordenarPor: 'ordenar-por',
    name: 'name',
    class: 'class',
    criterio: 'criterio-de-filtro',
    filtro: 'control-de-filtro',
    tablaDeDatos: 'tabla-de-datos',
    id: literal.id,
    idElemento: 'idElemento',
    idVinculado: 'idVinculado',
    crudModal: 'crud-modal',
    propiedadRestrictora: 'propiedad-restrictora',
    filtrarPor: 'filtrar-por',
    mostrarPropiedad: 'propiedad-mostrar',
    mostrarExpresion: 'mostrar-expresion',
    controlador: literal.controlador,
    accion: literal.accion,
    vista: literal.vista,
    vistaMvc: 'vistamvc',
    obligatorio: 'obligatorio',
    SoloParaTs: 'solo-para-ts',
    modoOrdenacion: 'modo-ordenacion',
    ordenInicial: 'orden-inicial',
    expresionElemento: 'expresion-elemento',
    expresion: 'expresion',
    tipo: 'tipo',
    tipoColumna: 'tipo-control',
    tipoModal: 'tipomodal',
    imagenVinculada: 'imagen-vinculada',
    valorInput: 'value',
    valorPorDefecto: 'valor-de-defecto',
    parametrosParaNavegador: 'parametros-para-navegar',
    idDelElemento: 'idDelElemento',
    restrictor: 'restrictor',
    nombreModal: 'idModal',
    editable: 'editable',
    ContenidoEn: 'contenido-en',
    AplicarJoin: 'aplicar-join',
    BlanquearControlAsociado: 'blanquear',
    cantidadPorLeer: 'cantidad-a-leer',
    menuFlotante: 'menu-flotante',
    conNavegador: 'con-navegador',
    tablaDeControles: 'tabla-de-controles',
    MenuFlotante: 'menu-flotante',
    NoMapeablesAlDto: 'propiedades-no-mapeables',
    PermisosNecesarios: 'permisos-necesarios',
    negocio: literal.negocio,
    transiciones: 'transiciones',
    esAmpliacion: 'es-ampliacion',
    esDetalle: 'es-detalle',
    objeto: 'datos-del-objeto',
    TrasMapear: 'tras-mapear',
    soloConsulta: 'solo-consulta',
    esAlmacenable: 'es-almacenable',
    title: 'title',
    NombreDeAccion: 'NombreDeAccion',
    eventoJs: {
        onclick: 'onclick'
    }
};

const atNombre = {
    contenedorControl: 'contenedor-control'
}

const atRef = {
    enConsultaOcultar: 'en-consulta-ocultar',
    onclick: 'al-pulsar'

};

const atCheck = {
    filtrarPorFalse: 'filtrar-por-false',
    chequeado: 'chequeado',
    MostrarColumna: 'mostrar-columna',
    Columna: 'columna'
};

const atMantenimniento = {
    zonaDeFiltro: 'zona-de-filtro',
    controlador: literal.controlador,
    negocio: literal.negocio,
    dto: 'dto',
    idNegocio: literal.idNegocio,
    idVista: 'id-vista',
    zonaMenu: 'zona-de-menu',
    gridDelMnt: 'grid-del-mnt',
    permiteCrear: 'permite-crear',
    permiteEditar: 'permite-editar',
    permiteBorrar: 'permite-borrar',

};


const enumModoOrdenacion = {
    ascedente: 'ascendente',
    descendente: 'descendente',
    sinOrden: 'sin-orden'
};

const enumFormato = {
    Moneda: 'moneda',
    Numero: 'numero',
    Numero_2: 'numero.2',
    Numero_6: 'numero.6',
    Porcentaje: 'porcentaje',
    FechaHora: 'yyyy-MM-dd HH:mm:ss',
    ddMMyyyyHHmm: 'dd-MM-yyyy HH:mm',
    base64: 'base64'
}

const enumTipoControl = {
    string: 'string',
    number: 'number'
}


const atGrid = {
    id: atMantenimniento.gridDelMnt,
    EsFecha: 'es-fecha',
    formato: 'formato',
    cargando: 'cargando',
    zonaNavegador: 'zona-de-navegador',
    cabeceraTabla: 'cabecera-de-tabla',
    idSeleccionado: 'id-seleccionado',
    nombreSeleccionado: 'nombre-Seleccionado',
    ordenacion: 'ordenacion',
    tamanoFijo: 'tamano-fijo',
    tamanoFijoAlHacerVisible: '10em',
    tamanoFijoChkSel: '24px',
    ultimaPaginaMostrada: 'ultima-pagina-mostrada',
    navegador: {
        cantidad: 'cantidad-a-mostrar',
        pagina: 'pagina',
        leidos: 'leidos',
        posicion: 'posicion',
        total: 'total-en-bd',
        titulo: 'title'
    },
    paginaDeDatos: 'pagina-de-datos',
    accion: {
        buscar: 'buscar',
        siguiente: 'sigiente',
        anterior: 'anterior',
        restaurar: 'restaurar',
        irA: 'irA',
        ultima: 'ultima',
        ordenar: 'ordenar',
        seleccionadas: 'seleccionadas',
        historial: 'historial'
    },
    selector: 'selector',
    idCtrlCantidad: 'nav_2_reg',
    idInfo: 'info',
    idMensaje: 'mensaje',
    headers: 'headers',
    RecalcularPorcentajes: 'RecalcularPorcentajes'
};

const atArchivo = {
    idArchivo: 'id-archivo',
    visibleEnVisorAlCrear: 'visible-en-visor-al-crear',
    firmado: 'firmado',
    bloqueado: 'bloqueado',
    visorArchivo: 'visor-del-archivo',
    trasSeleccionar: 'tras-seleccionar',
    controlador: literal.controlador,
    nombre: 'nombre-archivo',
    canvas: 'canvas-vinculado',
    imagen: 'imagen-vinculada',
    barra: 'barra-vinculada',
    contenedorBarra: 'contenedor-barra',
    infoArchivo: 'info-archivo',
    estado: 'estado-subida',
    situacion: {
        subiendo: 'subiendo',
        subido: 'subido',
        error: 'error',
        pendiente: 'pendiente',
        sinArchivo: 'sin-archivo'
    },
    rutaDestino: 'ruta-destino',
    extensionesValidas: 'accept',
    limiteEnByte: 'limite-en-byte',
    url: 'src',
    archivosSeleccionados: 'archivos-seleccionados',
    AplicarOperacion: 'aplicar',
    Operacion: {
        Copiar: 'copiar',
        Mover: 'mover',
        Enlazar: 'enlazar',
        BloqueoMultiple: 'BloqueoMultiple',
        GenerarZip: 'GenerarZip',
    },
    Original: 'original',
    IdOriginal: 'id-original',
    EsDeUnArchivador: 'es-de-archivador'
};

const atOpcionDeMenu = {
    permisosNecesarios: atControl.PermisosNecesarios,
    permiteMultiSeleccion: 'permite-multi-seleccion',
    numeroMaximoSeleccionable: 'numero-maximo-seleccionable',
    clase: 'clase',
    bloqueada: 'bloqueada',
    oculta: 'oculta'
};

const atSelectorDeFiltro = {
    popiedadBuscar: 'propiedadBuscar',
    criterioBuscar: 'criterioBuscar',
    idEditorMostrar: 'ideditormostrar',
    idGridModal: 'id-grid-modal',
    mostrarPropiedad: atControl.mostrarPropiedad,
    refCheckDeSeleccion: 'refCheckDeSeleccion',
    idModal: 'id-modal',
    idBtnSelector: 'idBtnSelector',
    ListaDeSeleccionados: 'ids-seleccionados',
    selector: 'selector',
    propiedadParaFiltrar: 'propiedadFiltrar'
};

const atSelectorDeElementos = {
    ModalPadre: 'modal-padre',
    EditorAsociado: 'idEditor',
    Boton: 'idBotonSelector',
    IdEditorDeFiltro: 'idEditorDelFiltro',
    ValorAlEntrar: 'valor-al-entrar',
    Seleccionados: 'idseleccionados',
    CerrarAutomaticamente: 'cerrar-automaticamente'
};

const atSelectorDeFecha = {
    hora: 'idDeLaHora',
    milisegundos: 'milisegundos',
    ProponerFechaEn: 'ProponerFechaEn'
};

const atEntreFechas = {
    idHoraDesde: 'idhoradesde',
    idFechaHasta: 'idfechahasta',
    idHoraHasta: 'idhorahasta',
    valorFechaHasta: 'valorFechaHasta',
    idfechahasta: 'idfechahasta',
    idhorahasta: 'idhorahasta'
};


const atEntreRangos = {
    idRangoHasta: 'idRangoHasta',
    valorHasta: 'valorHasta'
};

const atEntreImportes = {
    idImporteHasta: 'idImporteHasta',
    valorHasta: 'valorHasta'
};

const atListas = {
    claseElemento: 'clase-elemento',
    mostrarExpresion: atControl.mostrarExpresion,
    yaCargado: 'ya-cargada',
    idDeLaLista: 'list',
    identificador: 'identificador',
    expresionPorDefecto: 'nombre',
    ContenidoEn: atControl.ContenidoEn,
    RestringidoPor: 'restringido-por',
    noSeleccionado: literal.menos1,
    guardarEn: 'guardar-en',
    negocio: literal.negocio,
    trasSeleccionar: 'tras-seleccionar',
    trasBlanquear: 'tras-blanquear',
    antesDeBuscar: 'antes-de-buscar',
    RestrictorFijo: 'restrictor-fijo',
    ReadOnly: "readonly"
};

const atListasDeElemento = {
    claseElemento: atListas.claseElemento,
    mostrarExpresion: atListas.mostrarExpresion,
    yaCargado: atListas.yaCargado,
    cargarBajoDemanda: 'cargar-bajo-demanda',
    Cargando: 'cargando',
    expresionPorDefecto: atListas.expresionPorDefecto,
    idNegocio: 'id-negocio',
    trasCargar: 'tras-cargar',
    Seleccionar: 'Seleccionar ...',
    AutoPosicionamiento: 'auto-posicionamiento',
    RestrictorFijo: atListas.RestrictorFijo,
    Controlador: atControl.controlador
};

const atListasDeValores = {
    OnChange: 'OnChange',
    OnReset: 'OnReset'
};

const atListasDinamicas = {
    claseElemento: atListas.claseElemento,
    buscarPor: 'como-buscar',
    criterio: atControl.criterio,
    mostrarExpresion: atListas.mostrarExpresion,
    longitudNecesaria: 'longitud',
    idSeleccionado: 'idseleccionado',
    idSelAlEntrar: 'idAlEntrar',
    cargando: 'cargando',
    expresionPorDefecto: atListas.expresionPorDefecto,
    ultimaCadenaBuscada: 'ultima-busqueda',
    cantidad: atControl.cantidadPorLeer,
    RestringidoPor: atListas.RestringidoPor,
    RestrictorFijo: atListas.RestrictorFijo,
    SoloEnAlta: 'solo-en-alta',
    PropiedadRestrictora: atControl.propiedadRestrictora,
    ContenidoEn: atListas.ContenidoEn,
    BlanquearControlAsociado: 'blanquear-controles-dependientes',
    BlanquearAlSalir: 'blanquear-al-salir',
    AplicarJoin: atControl.AplicarJoin,
    FiltrosJs: 'filtros-js'
};


const atListasDinamicasDto = {
    guardarEn: atListas.guardarEn,
    conNavegador: atControl.conNavegador
};

const atRestrictor = {
    mostrarPropiedad: atControl.mostrarPropiedad,
    noRestringido: literal.menos1,
    conNavegador: atControl.conNavegador,
    propiedadRestrictora: atControl.propiedadRestrictora,
    idRestrictor: atControl.restrictor,
    RestringidoPor: 'restringido-por'
};

const atGridDeDetalle = {
    controlador: atControl.controlador,
    accionDeConsulta: 'accion-de-consulta',
    conCapa: 'con-capa',
    accionDeLeerPorId: 'accion-de-leer-por-id',
    EsAccion: 'columna-accion',
    Accion: literal.accion,
    restrictor: atControl.restrictor,
    restrictorFijo: 'restrictor-fijo',
    campoRestrictor: 'campo-restrictor',
    AplicarJoin: atControl.AplicarJoin,
    cantidad: atControl.cantidadPorLeer,
    orden: atControl.ordenarPor,
    filtrarPara: 'filtrar-para',
    gridDeRelacionAsociado: 'grid-de-relacion-asociado',
    modalParaEditarRelacion: 'modal-para-editar-relacion',
    idNegocio: literal.idNegocio,
    CargarPorEvento: 'cargar-por-evento',
    IdNegocioVinculado: 'id-negocio-vinculado',
    OcultarSiVacio: 'ocultar-si-vacio',
    TrasCargarGrid: 'tras-cargar-grid'
};

const atModal = {
    idElemento1: 'id-elemento1',
    idNegocio: literal.idNegocio,
    idElemento: atControl.idElemento,
    idNegocioVinculado: atGridDeDetalle.IdNegocioVinculado,
    trasAbrir: 'accion-tras-abrir',
    trasAceptar: 'accion-tras-aceptar',
    trasCerrar: 'accion-tras-cerrar',
    soloConsulta: 'solo-consulta',
    trasModificar: 'tras-modificar',
    NombreNegocio: literal.nombreNegocio,
    trasFirmar: 'tras-firmar',
    referenciaDeFiltrado: 'referencia-de-filtrado',
    controlador: literal.controlador,
    accion: 'accion-controlador',
};

const atModalParaImputar = {
    faltaRestrictor: 'falta-restrictor',
    filtrarPor: 'filtrar-por',
    criterio: 'criterio-por',
    idNegocio: literal.idNegocio
}

const atCriterio = {
    contiene: 'contiene',
    comienza: 'comienza',
    igual: 'igual',
    restringido: 'igual',
    noEstaRelacionado: 'noEstaRelacionado',
    esAlgunoDe: 'esAlgunoDe'
};

const atNavegar = {
    navegarAlCrud: 'navegar-al-crud',
    idRestrictor: atControl.restrictor,
    orden: 'orden',
    soloMapearEnElFiltro: 'solo-mapear-en-el-filtro',
    paraqueNavegar: 'paraque-navegar'
};

const enumParaQueNavegar = {
    gestionar: 'gestionar',
    crear: 'crear',
    editar: 'editar',
    consultar: 'consultar',
    seleccionar: 'seleccionar',
    gestionarRelaciones: 'gestionar-relaciones',
    gestionarDependencias: 'gestionar-dependencias'
};

const ltrStyle = {
    display: {
        none: 'none',
        block: 'block'
    },
    visibility: {
        hidden: 'hidden'
    }
};

const TagName = {
    input: 'input',
    select: 'select'
};

const ltrIconos = {
    papelera: 'Papelera.png',
    descargar: 'DescargarArchivo.png',
    firmar: 'firmar.svg',
    anularFirma: 'anularfirma',
    bloquearArchivo: 'bloqueararchivo.svg',
    desbloquearArchivo: 'desbloqueararchivo.svg'
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
    presupuestos: {
        PptDeVenta: 'PptDeVenta',
    },
    tareas: {
        planificacion: 'planificaciones',
    },
    pleitos: {
        recobro: 'recobro',
    }
};

const ltrCss = {
    Mensajes: {
        Informativo: 'mensaje-informativo',
        Error: 'mensaje-error'
    },
    crud: {
        cuerpo: 'cuerpo',
        cuerpoSoloConGrid: 'cuerpo-solo-con-grid',
        cabecera: 'cuerpo-cabecera',
        datos: 'cuerpo-datos',
        grid: {
            filto: 'cuerpo-datos-filtro',
            grid: 'cuerpo-datos-grid',
            tabla: 'cuerpo-datos-tabla',
            modificarTamano: 'modificar-tamano-columna',
            ordenarColumna: 'ordenar-columna',
            TablaConGraficos: 'contenedor-tabla-con-graficos',
            Splitter: 'splitter-tabla',
            ContenedorTabla: 'div-grid-tabla',
            Graficos: 'div-graficos'
        },
        dto: {
            tbody: 'dto-tbody',
            fila: 'dto-tr',
            celda: 'dto-td'
        },
        menuDeDetalle: 'menu-de-detalle',
        mostrarDetalle: 'menu-de-detalle-oculto',
        ocultarDetalle: 'menu-de-detalle-visible',
        mostrarTotales: 'menu-de-totales-visible',
        pie: 'cuerpo-pie',
        contenedorEdicionCreacion: 'contenedor-edicion-cabecera',
        creacion: 'cuerpo-creacion',
        edicion: 'cuerpo-edicion',
        edicionSoloConsulta: 'cuerpo-edicion-solo-consulta',
        PanelHistorial: {
            historial: 'cuerpo-historial',
            ContenedorCabecera: 'contenedor-historial-cabecera',
            ContenedorCuerpo: 'contenedor-historial-cuerpo',
            ContenedorPie: 'contenedor-historial-pie',
            Color: 'color'
        },
        editarTrasCrear: 'editar-tras-crear',
        datosPrincipales: 'datos-principales-dto',
        padreContenedorDatosPrincipales: 'padre-contenedor-datos-principales',
        contenedorDatosPrincipales: 'contenedor-datos-principales',
        editando: 'editando',
        mostrandoHistorial: 'mostrando-historial',
        creando: 'creando',
        panelCreacion: {
            VisorOculto: 'visor-oculto',
            contenedorPie: 'contenedor-edicion-pie',
            ContenedorDeDatosMasVisor: 'contenedor-creacion-cuerpo-datos-visor',
            ContenedorCabecera: 'contenedor-edicion-cabecera',
            ContenedorVisor: 'contenedor-edicion-editor-visor',
            ContenedorDeDatosDto: 'contenedor-edicion-editor-datos',
            VisorDeCreacion: 'visor-de-edicion',
            ImagenIa: 'img-ia',
            ImagenOcr: 'img-ocr',
            ImagenArchivo: 'img-archivo'
        },
        panelDeEdicion: {
            ContenedorDeAcciones: 'acciones-del-panel-edicion',
            VisorOculto: 'visor-oculto',
            VisorDeEdicion: 'visor-de-edicion',
            ContenedorVisor: 'contenedor-edicion-editor-visor',
            ContenedorDelVisorDeArchivoConHistorial: 'contenedor-del-visor-de-archivo-con-historial',
            ContenedorHistorial: 'contenedor-edicion-editor-historial',
            VisorDeHistorial: 'visor-edicion-historial',
            MenuDeHistorial: 'menu-edicion-historial',
            ContenedorCabecera: 'contenedor-edicion-cabecera',
            ContenedorDeDatosDto: 'contenedor-edicion-editor-datos',
            ContenedorDeDatosMasVisor: 'contenedor-edicion-editor',
            VisorDeNombreAnexados: 'visor-nombre-archivo',
            FiltroSelectorDeArchivos: 'filtro-selector-archivos',
            ProcesarConIa: 'procesar-con-ia',
            PasarOcr: 'pasar-ocr',
            Acciones: {
                plegar: 'img-detalle-dto-plegar',
                Desplegar: 'img-detalle-dto-desplegar',
                Visor: 'img-detalle-dto-visor',
                Devolver: 'img-detalle-dto-retroceder',
                Avanzar: 'img-detalle-dto-avanzar',
                MostrarVisor: 'mostrar-visor',
                OcultarVisor: 'ocultar-visor',
                SinVisor: 'sin-visor'
            },
            OcultarBoton: 'ocultar-boton',
            DeshabilitarBoton: 'deshabilitar-boton',
            HabilitarBoton: 'habilitar-boton',
            SelectorDeArchivosEnConsulta: 'archivos-en-consulta',
            ConsultaConGuid: 'consulta-con-guid',
            ojoTachado: 'tachado',
            iconoConsultaConGuid: 'icono-consultar-con-guid'
        },
        contenedorTitulo: 'contenedor-titulo',
        tabla: 'div-tabla',
        thead: 'div-thead',
        tbody: 'div-tbody',
        columna: 'div-th',
        encolumnado: 'div-tr',
        fila: 'div-tr',
        celda: 'div-td',
        tituloColumna: 'div-de-columna',
        tituloResaltado: 'div-de-columna-resaltado',
        arrastrando: 'arrastrando',
        modal: {
            dinamica: 'modal-mostrar-propiedad',
            boton: 'btn-modal',
            pie: 'modal-footer-izquierda',
            cuerpo: 'modal-body',
            ContenedorDeHistorialIa: 'contenedor-historial-ia',
            LineaDelhistorialIa: 'item-historial-ia'
        }
    },
    formulario: {
        selectorDeArchivo: 'formulario-selector-archivo',
        dto: 'formulario-contenedor-edicion-dto',
        archivoSobreLi: 'archivo-sobre-li'
    },
    ia: {
        mapeado: 'ia-mapeado',
        Disponible: 'ia-disponible',
        Seleccionada: 'ia-seleccionada'
    },
    PanelDeControl: {
        Menu: {
            Oculto: 'menu-pnlctr-oculto',
        }
    },
    controlNoVisible: 'controlNoVisible',
    crtlValido: 'propiedad-valida',
    crtlNoValido: 'propiedad-no-valida',
    propiedad: 'propiedad',
    propiedadId: 'propiedad-id',
    filaVisible: 'fila-visible',
    filaNoVisible: 'fila-no-visible',
    celdaVisible: 'celda-visible',
    celdaNoVisible: 'celda-no-visible',
    divVisible: 'div-visible',
    divNoVisible: 'div-no-visible',
    trPropiedad: 'tr-propiedad',
    tdPropiedad: 'td-propiedad',
    divPropiedad: 'div-propiedad',
    contenedorDeEtiqueta: 'contenedor-etiqueta',
    obligatorio: 'obligatorio',
    ordenAscendente: 'ordenada-ascendente',
    ordenDescendente: 'ordenada-descendente',
    sinOrden: 'ordenada-sin-orden',
    selectorElemento: 'selector-de-elemento',
    barraVerde: 'barra-verde',
    barraRoja: 'barra-roja',
    barraAzul: 'barra-azul',
    contenedorModal: 'contenedor-modal',
    contenidoModal: 'contenido-modal',
    contenidoModalAmpliado: 'contenido-modal-ampliado',
    cabeceraModal: 'contenido-cabecera',
    cuerpoModal: 'contenido-cuerpo',
    pieModal: 'contenido-pie',
    soloLectura: 'solo-lectura',
    columnaOculta: 'columna-oculta',
    columnaCabecera: 'columna-cabecera',
    filaDelGrid: 'cuerpo-datos-tbody-tr',
    filaSeleccionada: 'fila-seleccionada',
    filaCancelada: 'fila-cancelada',
    filaTerminada: 'fila-terminada',
    filaEstado: 'fila-estado',
    tablaEdicion: 'tabla-edicion',
    cuerpoDeLaTabla: 'cuerpo-datos-tbody',
    cuerpoDeLaTablaGrid: 'cuerpo-datos-grid-tbody',
    sinCapaDeBloqueo: 'sin-capa-de-bloqueo',
    conCapaDeBloqueo: 'con-capa-de-bloqueo',
    nodoDeJerarquia: 'nodo-de-jerarquia',
    nodoDeJerarquiaDeBaja: 'nodo-de-jerarquia-de-baja',
    cambioDeColorElBoton: 'cambio-de-color',
    nodoSeleccionado: 'nodo-seleccionado',
    contenedorDeArchivos: 'contenedor-de-archivos',
    archivosSeleccionado: 'archivos-seleccionados',
    imagenEnBoton: 'imagen-en-boton',
    infoArchivo: 'formulario-visor-datos-archivo',
    contenedorInfoArchivo: 'contenedor-visor-info-archivo',
    contenedorDeOpcion: 'contenedor-de-opcion',
    contenedorVisor: 'contenedor-de-visor',
    contenedorVisorOculto: 'contenedor-de-visor-oculto',
    contenedorVisorRef: 'contenedor-de-visor-ref',
    contenedorVisorImg: 'contenedor-de-visor-img',
    imagen100_100: 'imagen100_100',
    refJs: 'refJs',
    archivoFirmado: 'archivo-firmado',
    borrarAnexado: 'borrar-anexado',
    firmarArchivo: 'firmar-archivo',
    bloquearArchivo: 'bloquear-archivo',
    desbloquearArchivo: 'desbloquear-archivo',
    tamanoImagen: 'imagen-boton',
    anularFirma: 'anular-firmar',
    descargarAnexado: 'descargar-anexado',
    eliminarSeleccionado: 'eliminar-seleccionado',
    contenedorEdicionCuerpo: 'contenedor-edicion-cuerpo',
    contenedorConMasDeUnControl: 'div-con-mas-propiedades',
    mostrarSoloElPrimerControl: 'mostrar-solo-primer-control',
    controlOculto: 'control-oculto',
    Modal: {
        Boton: 'boton-modal',
        BotonPorDefecto: 'boton-por-defecto',
        ContenidoCuerpo: 'contenido-cuerpo',
        ContenedorEdicionCuerpo: 'contenedor-edicion-cuerpo'
    },
    controlesDto: {
        hora: 'hora-dto',
        oculto: 'control-oculto',
        etiqueta: 'etiqueta-dto'
    },
    Agenda: {
        EventoAjustadoAlContenedor: 'evento-ajustado-al-contenedor'
    },
    modalesDeFiltro: {
        conFiltro: 'con-filtro'
    },
    Bloque: {
        Contenedor: 'grid-span'
    },
    Ampliacion: {
        Contenedor: 'ampliacion-dto',
        CerrarAmpliacion: 'img-ampliacion-dto-cerrar',
        AbrirAmpliacion: 'img-ampliacion-dto-abrir'
    },
    Detalle: {
        Contenedor: 'detalle-dto',
        CerrarDetalle: 'img-detalle-dto-cerrar',
        AbrirDetalle: 'img-detalle-dto-abrir',
        ColumnaAccion: atGridDeDetalle.EsAccion
    },
    Espan: {
        Cabecera: 'grid-span-cabecera',
        noVisible: 'espan-no-visible',
        conContenido: 'con-Contenido',
        cssExpansor: 'span-expansor',
        cssVisorArchivos: 'contenedor-de-visor',
        barra: 'barra-de-carga',
        animarBarrra: 'barra-animada',
        valorTrue: 'valor-true',
        valorFalse: 'valor-false'
    },
    Archivos: {
        SelectorDeArchivos: 'selector-de-archivos',
        SelectorDeArchivosEnConsulta: 'archivos-en-consulta',
        ContenedorDeArchivo: 'contenedor-de-archivos',
        UnaColumna: 'columnas-de-archivos-1',
        DosColumnas: 'columnas-de-archivos-2',
        TresColumnas: 'columnas-de-archivos-3',
        CuatroColumnas: 'columnas-de-archivos-4',
        CancelarOperacion: 'cancelar-operacion',
        Pegar: 'pegar-archivos',
        Copiar: 'copiar-archivos',
        Mover: 'mover-archivos',
        Enlazar: 'enlazar-archivos',
        Cancelado: 'archivo-cancelado',
        ComponenteParaAnexar: 'componente-para-anexar'
    },
    Archivador: {
        CrearFactura: 'crear-factura-rec',
        VerFactura: 'ver-factura-rec'
    },
    Resalto: {
        ConResalto: 'con-resalto',
        Verde: 'resalto-verde',
        Violeta: 'resalto-violeta'
    }
};

const Ajax = {
    Permisos: {
        controlador: 'PermisosDelElemento'
    },
    Mensajes: {
        NoMostrar: 'no_mostrar'
    },
    Entorno: {
        MiCorreo: {
            controlador: ltrControladores.Entorno.MiCorreo,
            PeticionDeAcceso: 'epPeticionDeAcceso',
            crudAsync: 'CrudDeMiCorreoAsync',
            crud: 'CrudDeMiCorreo',
            crudApiKey: 'CrudDeMiCorreoApiKey',
            DescargarAdjunto: 'epDescargarAdjunto'
        },
        Agenda: {
            visor: {
                controlador: ltrControladores.Entorno.VisorDeAgenda,
                crud: 'MiCalendario'
            }
        },
        ArbolMenu: {
            controlador: ltrControladores.Entorno.ArbolDeMenu,
            accion: 'epSolicitarMenuHtml',
            Inicializar: 'InicializarEntorno'
        },
        Acceso: {
            controlador: ltrControladores.Entorno.Acceso,
            Logout: 'Logout',
            RegistrarConsultaConGuid: 'epRegistrarConsultaConGuid',
            ValidarConsultaPorGuid: 'epValidarConsultaPorGuid',
            Consultar: 'Consultar'
        }
    },
    SisDoc: {
        CircuitosDoc: {
            controlador: ltrControladores.SisDoc.CircuitosDoc,
            Fichar: 'epFichar'
        }
    },
    Ventas: {
        Facturas: {
            controlador: ltrControladores.Venta.FacturasEmt,
            accion: {
                DescargarDeclaracionResponsable: 'epDescargarDeclaracionResponsable',
                ValidarExisteDeclaracionResponsable: 'epValidarDeclaracionResponsable'
            }
        }
    },
    Modal: {
        controlador: ltrControladores.Comunes.Home,
        accion: 'epSolicitarModalHtml',
        parametro: 'modal'
    },
    Archivos: {
        controlador: ltrControladores.SisDoc.Archivos,
        accion: {
            SubirArchivo: 'epSubirArchivo',
            AnexarArchivo: 'epAnexarArchivo',
            QuitarAnexado: 'epQuitarAnexado',
            FirmarArchivo: 'epFirmarArchivo',
            RegistrarDescargaConGuid: 'epRegistrarDescargaConGuid',
            DescargaConGuid: 'epDescargaConGuid',
            AnularFirma: 'epAnularFirma',
            LeerAnexados: 'epLeerAnexados',
            LeerAnexadosPorGuid: 'epLeerAnexadosPorGuid',
            DescargarZipToHtml: 'epDescargarZipToHtml',
            DescargarDocxToHtml: 'epDescargarDocxToHtml',
            DescargarXlsxToHtml: 'epDescargarXlsxToHtml',
            DescargarRtfToHtml: 'epDescargarRtfToHtml',
            DescargarCsvToHtml: 'epDescargarCsvToHtml',
            DescargarParaCrear: 'epDescargarArchivoParaCrear',
            ProcesarAccion: 'epProcesarAccion',
            DescargarHtmlSanitizado: 'epDescargarHtmlSanitizado',
            Descargar: 'epDescargarArchivo',
            DescargarPorGuid: 'epDescargarArchivoPorGuid',
            Thumsnail: 'epDescargarThumsnail',
            LeerCertificados: 'epLeerCertificados',
            LeerDatosDeFirma: 'epLeerDatosDeFirma',
            ProcesarArchivos: 'epProcesarArchivos',
            OperacionConArchivos: 'epOperacionConArchivos',
            BloquearArchivo: 'epBloquearArchivo',
            DesbloquearArchivo: 'epDesbloquearArchivo',
            BloquearArchivos: 'epBloquearArchivos',
            DesbloquearArchivos: 'epDesbloquearArchivos',
            GenerarZip: 'epGenerarZip',
            SubirTrozo: 'epSubirTrozo',
            LeerVinculosAl: 'epLeerVinculosAl'
        },
        parametro: {
            idArchivo: 'idArchivo',
            Destino: enumNegocio.Archivador,
            Cantidad: '5'
        },
    },
    EndPoint: {
        ProcesarOpcionMf: 'epProcesarOpcionMf',
        ProcesarPeticion: 'epProcesarPeticion',
        CrearElemento: 'epCrearElemento',
        CrearElementoPorPost: 'epCrearElementoPorPost',
        CrearNodo: 'epCrearNodo',
        PersistirNodo: 'epPersistirNodo',
        LeerGridEnHtml: 'epLeerGridHtml',
        LeerDatosParaElGrid: 'epLeerDatosPost',
        Totales: 'epTotales',
        LeerElemento: 'epLeerElemento',
        LeerElementos: 'epLeerElementos',
        LeerElementosPorGuid: 'epLeerElementosPorGuid',
        LeerLosEventosDel: 'epLeerLosEventosDel',
        LeerLosEventosPorGuid: 'epLeerLosEventosPorGuid',
        LeerPorId: 'epLeerPorId',
        LeerPorIdPorGuid: 'epLeerPorIdPorGuid',
        LeerPorNombre: 'epLeerPorNombre',
        Bloquear: 'epBloquear',
        Desbloquear: 'epDesbloquear',
        DarDeAlta: 'epDarDeAlta',
        DarDeBaja: 'epDarDeBaja',
        ModificarPorId: 'epModificarPorPost',
        BorrarPorId: 'epBorrarPorId',
        BorrarRelacion: 'epBorrarRelacionPorId',
        RecargarModalEnHtml: 'epRecargarModalEnHtml',
        Leer: 'epLeerParaSelector',
        CargarLista: 'epCargarLista',
        CargaDinamica: 'epCargaDinamica',
        CrearRelaciones: 'epCrearRelaciones',
        Imputar: 'epImputar',
        CrearRelacion: 'epCrearRelacion',
        CrearRelacionPost: 'epCrearRelacionPost',
        CrearVinculo: 'epCrearVinculo',
        CrearDetalle: 'epCrearDetalle',
        Vincular: 'epVincular',
        BorrarVinculo: 'epBorrarVinculo',
        ModificarRelacion: 'epModificarRelacionPorPost',
        LeerDatosParaInicializarVista: 'epLeerDatosParaInicializarVista',
        LeerModoDeAccesoAlElemento: 'epLeerModoDeAccesoAlElemento',
        Exportar: 'epExportar',
        EnviarCorreo: 'epEnviarPorCorreo',
        Transitar: 'epTransitar',
        TransitarSeleccionados: 'epTransitarSeleccionados',
        Imprimir: 'epImprimir',
        LeerJerarquia: 'epLeerJerarquia',
        LeerNodoSeleccionado: 'epLeerNodoSeleccionado',
        SometerTrabajo: 'epSometerTrabajoDeUsuario',
        LeerVinculosCon: 'epLeerVinculosCon',
        LeerVinculosConPorGuid: 'epLeerVinculosConPorGuid',
        LeerVinculosConElNegocio: 'epLeerVinculosConElNegocio',
        PersistirAmpliacion: 'epPersistirAmpliacionPorPost',
        LeerAmpliacionPorIdNegocio: 'epLeerAmpliacionPorIdNegocio',
        LeerAmpliacionPorNegocio: 'epLeerAmpliacionPorNegocio',
        Genericas: {
            LeerPlantillaPorTipo: 'epLeerPlantillas',
            LeerTipo: 'epLeerTipo',
            LeerTipoPorGuid: 'epLeerTipoPorGuid',
            ObtenerUrlAlExpediente: 'epObtenerUrlAlExpediente',
            ObtenerUrlAlProveedor: 'epObtenerUrlAlProveedor',
            ObtenerUrlAlCliente: 'epObtenerUrlAlCliente',
        },
        Entorno: {
            MiCorreo: {
                Archivar: 'epArchivarCorreo',
                DescargarAdjunto: 'epDescargarAdjunto',
                ImprimirCorreo: 'epImprimirCorreo',
                EliminarCorreo: 'epEliminarCorreo',
                AsociarAlElemento: 'epAsociarAlElemento'
            },
            Agenda: {
                IrAlElementoDelEvento: 'epIrAlElementoDelEvento'
            },
            ArbolMenu: {
                GuardarMenuAccedido: 'epGuardarMenuAccedido',
                GuardarRegistroAccedido: 'epGuardarRegistroAccedido'
            },
            Menu: {
                SeleccionarIa: 'epSeleccionarIa'
            }
        },
        Contabilidad: {
            Preasientos: {
                ObtenerUrlAlOrigen: 'epObtenerUrlAlOrigen',
                ObtenerUrlAlLoteContable: 'epObtenerUrlAlLoteContable',
                CrearLoteContable: 'epCrearLoteContable'
            }
        },
        Guarderias: {
            Infantes: {
                AsociarCurso: 'epAsociarCurso'
            },
        },
        SisDoc: {
            Archivadores: {
                ImportarZip: 'epImportarZip',
                ProcesarFarConIa: 'epProcesarFarConIa',
                Copiar: 'epCopiarArchivador'
            }
        },
        Administracion: {
            Tarea: {
                Copiar: 'epCopiarUnaTarea',
                Renombrar: 'epRenombarUnaFar'
            }
        },
        Gasto: {
            RemesasPag: {
                PagarRemesa: 'epPagarRemesa',
                RetrocederRemesa: 'epRetrocederPagoRemesa'
            },
            FacturaRec: {
                ImportarFarXml: 'epImportarFarDesdeXml',
                ImportarPrvXml: 'epImportarPrvDesdeXml',
                CrearFarConIa: 'epCrearFarConIa',
                Generar: 'epGenerarUnaFar',
                Rectificar: 'epRectificarUnaFar',
                Renombrar: 'epRenombarUnaFar',
                CambiarProveedor: 'epCambiarProveedor',
                ObtenerUrlAlExpediente: 'epObtenerUrlAlExpediente'
            }
        },
        Venta: {
            Presupuesto: {
                Generar: 'epGenerarUnPpt',
                Asociar: 'epAsociarUnPpt',
                Renombrar: 'epRenombarUnPpt',
                ObtenerUrlAlExpediente: 'epObtenerUrlAlExpediente'
            },
            AsignacionPtr: {
                AplicarDatosDeEjecucion: 'epAplicarDatosDeEjecucion'
            },
            FacturaEmt: {
                CopiarFae: 'epCopiarUnaFae',
                CambiarVencimiento: 'epCambiarVencimiento',
                CambiarDatos: 'epCambiarDatos',
                HacerRectificativa: 'epHacerRectificativa',
                FacturarTareas: 'epFacturarTareas',
                IrARectificadaPor: 'epIrARectificadaPor',
                IrARectificoA: 'epIrARectificoA'
            },
            RemesasFae: {
                CargarRemesa: 'epCargarRemesa',
                AnularCargoRemesa: 'epAnularCargoRemesa'
            }
        },
        Contrato: {
            GenerarPlanificador: 'epGenerarPlanificadores',
            PrepararPartesTr: 'epPrepararPartesTr',
            EmitirPrefacturasPorPartesTr: 'epEmitirPrefacturasPorPartesTr',
            EmitirPrefacturasPorContrato: 'epEmitirPrefacturasPorContratos',
        },
        Terceros: {
            Sociedad: {
                AsociarCertificado: 'epAsociarCertificado',
                LeerDatosDeSociedad: 'epLeerDatosDeSociedad'
            },
            Interlocutor: {
                LeerCuentaDeIngreso: 'epLeerCuentaDeIngreso'
            },
            CG: {
                LeerDireccion: 'epLeerDireccion'
            }
        },
        MaestrosTecnicos: {
            Tarifa: {
                LeerTarifa: 'epLeerTarifa'
            }
        },
        Negocio: {
            LeerTipos: 'epLeerTipos',
            TiposDeElemento: {
                DescargarPlantilla: 'epDescargarPlantilla',
            },
            PlantillasDeNegocio: {
                DescargarPlantilla: 'epDescargarPlantilla',
                DescargarEtiquetas: 'epDescargarEtiquetas',
            },
        },
    },
    EpDeAcceso: {
        ReferenciarFoto: 'epReferenciarFoto',
        ValidarAcceso: 'epValidarAcceso',
        SolicitarNuevaContrasena: 'epSolicitarNuevaContrasena',
        ActualizarContrasena: 'epActualizarContrasena'
    },
    Param: {
        Nombre: 'nombre',
        elementoJson: 'elementoJson',
        idModal: 'idModal',
        idGrid: 'idGrid',
        opcionMf: 'opcionMf',
        peticion: 'peticion',
        esContextual: 'esContextual',
        idNegocio: 'idNegocio',
        idVista: 'idVista',
        Vista: 'vistaMvc',
        idVinculado: atControl.idVinculado,
        enumerado: 'enumerado',
        idElemento1: 'idElemento1',
        idElemento2: 'idElemento2',
        VinculadosA: 'vinculadosA',
        ids: 'ids',
        idTipo: 'idTipo',
        modo: 'modo',
        descriptor: 'descriptor',
        accion: literal.accion,
        posicion: 'posicion',
        cantidad: 'cantidad',
        obtenerSeguridad: 'obtenerseguridad',
        cargarListaDinamica: 'cargarListaDinamica',
        leerPorIdParaEditar: 'leerPorIdParaEditar',
        BorrarRelacion: 'BorrarRelacion',
        cargarListaDeElementos: 'cargarListaDeElementos',
        creandoEnCrud: 'creandoEnCrud',
        filtro: 'filtro',
        filtrarConIa: 'filtrarConIa',
        fraseDeFiltrado: 'fraseDeFiltrado',
        nuevaPregunta: 'nuevaPregunta',
        aplicarJoin: 'AplicarJoin',
        orden: 'orden',
        ColumnasOpcionales: 'ColumnasOpcionales',
        AmpliacionesSolicitadas: 'AmpliacionesSolicitadas',
        DatosPrincipales: 'DatosPrincipales',
        seleccionadas: 'seleccionadas',
        usuario: 'usuario',
        id: literal.id,
        parametros: 'parametrosJson',
        filtros: 'filtrosJson',
        idsJson: 'idsJson',
        claseElemento: 'claseElemento',
        fichero: 'fichero',
        rutaDestino: 'rutaDestino',
        extensiones: 'extensionesValidas',
        login: 'login',
        password: 'password',
        nombreDeNegocio: literal.negocio,
        enumNegocio: literal.enumNegocio,
        idElemento: atControl.idElemento,
        propiedadId: 'propiedadId',
        datosPeticion: 'datosPeticion',
        tipoJson: 'tipoJson',
        json: 'json',
        nodoPadre: 'idPadre',
        otros: 'otros',
        operacion: 'operacion',
        ordenarPor: 'ordenarpor',
        nodoSeleccionado: 'nodoSeleccionado',
        SoloEnAlta: 'SoloEnAlta',
        idTransicion: 'idTransicion',
        fitrarPara: 'filtrarpara',
        guid: 'guid',
        idDondeImputar: 'idDondeImputar',
        idPlantilla: 'idPlantilla',
        idAdjunto: 'idAdjunto',
        idParte: 'idParte',
        idMail: 'idMail',
        Exportacion: {
            Sometido: 'sometido',
            Receptores: 'receptores'
        },
        MiCorreo: {
            Buzon: 'buzon',
        },
        parametrosDeCreacion: 'parametrosDeCreacion',
        Bloquear: 'bloquear',
        RowVersion: 'RowVersion'
    },
    Parametros: {
        MensajeSiHayMasDeUno: 'MensajeSiHayMasDeUno',
        MensajeSiNoHay: 'MensajeSiNoHay',
        ErrorSiNoHay: 'ErrorSiNoHay',
        ErrorSiHayMasDeUno: 'ErrorSiHayMasDeUno',
        SoloActivos: 'SoloActivos',
        LeerPorNombre: 'LeerPorNombre',
        ConsultarConGuid: 'ConsultarConGuid'
    },
    persisitencia: {
        modificar: 'Modificar',
        eliminar: 'Eliminar',
    },
    Callejero: {
        Importacion: 'importarCallejero',
        accion: {
            importar: 'epImportarCallejero'
        },
        Parametros: {
            Calificador: 'enumCalificadorDireccion'
        }
    },
    TrabajosSometidos: {
        controlador: 'TrabajosDeUsuario',
        accion: {
            iniciar: 'epIniciarTrabajoDeUsuario',
            bloquear: 'epBloquearTrabajoDeUsuario',
            desbloquear: 'epDesbloquearTrabajoDeUsuario',
            resometer: 'epResometerTrabajoDeUsuario'
        },
        parametro: {
            trabajo: 'trabajo',
            idSometedor: 'idsometedor',
            idArchivador: 'idarchivador',
            parametros: 'parametros'
        },
        trabajo: {
            sincronizarArchivador: 'Sincronizar archivador'
        }
    },
    Usuarios: {
        controlador: 'usuarios',
        accion: {
            LeerUsuarioDeConexion: 'epLeerUsuarioDeConexion',
            LeerCertificados: 'epLeerCertificados',
            SubirMiCertificado: 'epSubirMiCertificado',
            CambiarPassword: 'epCambiarPassword'
        }
    },
    Planificador: {
        idPlanificador: 'idPlanificador'
    },
    Error: 'Error',
    jsonResultError: 1,
    jsonResultOk: 0,
    jsonUndefined: undefined,
    eventoLoad: 'load',
    eventoError: 'error',
    eventoProgreso: 'progress'
};


const ltrUrls = {
    Entorno: {
        Trasiciones: 'Transiciones/CrudDeTransiciones',
        VisorDeAgenda: 'VisorDeAgenda/VisorDeAgenda',
    },
    Terceros: {
        Personas: "Personas/CrudPersonas",
        Sociedades: "Sociedades/CrudSociedades",
        Interlocutor: "Interlocutores/CrudInterlocutores",
        Procurador: "Procuradores/CrudProcuradores",
        Proveedor: "Proveedores/CrudProveedores",
        Cliente: "Clientes/CrudClientes",
        Trabajador: "Trabajadores/CrudTrabajadores",
        Abogado: "Abogados/CrudAbogados",
        CentrosGestores: 'CentrosGestores/CrudCentrosGestores',
    },
    Callejero: {
        Barrios: 'Barrios/CrudBarrios',
        Zonas: 'Zonas/CrudZonas',
        Cps: 'CodigosPostales/CrudCodigosPostales'
    },
    SistemaDocumental: {
        Archivadores: 'Archivadores/CrudArchivadores',
        Carpetas: 'Carpetas/CrudCarpetas',
        Descargar: `${Ajax.Archivos.controlador}/${Ajax.Archivos.accion.Descargar}`,
    },
    Administracion: {
        Tareas: 'Tareas/CrudTareas',
        RegistroEs: 'RegistrosEs/CrudRegistrosEs',
        Expediente: 'Expedientes/CrudExpedientes',
    },
    Juridico: {
        ContratosDeVenta: `Contratos/CrudContratos?clase=${enumClaseDeContrato.Venta}`,
        ContratosDeCompra: `Contratos/CrudContratos?clase=${enumClaseDeContrato.Compra}`,
    },
    Ventas: {
        Presupuestos: 'Presupuestos/CrudPresupuestos',
        PlfDeVenta: 'PlanificacionesDeVenta/CrudPlanificacionesDeVenta',
        PartesTr: 'PartesTr/CrudPartesDeTrabajo',
        FacturasEmt: 'FacturasEmt/CrudFacturasEmt'
    },
    Gastos: {
        FacturasRec: 'FacturasRec/CrudFacturasRec',
        Pagos: 'Pagos/CrudPagos'
    }
};


const ltrEdicion = {
    Editando: 'editando',
    IdDelEspan: 'IdDelEspan'
};

const ltrMantenimiento = {
    posicion: 'posicion',
    CheckDeSeleccion: 'chksel',
    idCuerpoDePagina: 'cuerpo-de-pagina',
    mostrarSoloSeleccionadas: 'Solo las seleccionadas',
    mostrarTodasLasFilas: 'Todas las filas',
    Indicadores: 'indicadores',
    Espanes: 'espanes',
    Filtros: 'filtros'
};

const ltrEventos = {
    OpcionesDelGrid: {
        SeleccionarTodo: 'seleccionar-todo',
        AnularSeleccion: 'anular-seleccion',
        AnularOrden: 'anular-ordenacion',
        AplicarOrdenInicial: 'aplicar-orden-inicial',
        MostrarLasSeleccionadas: 'mostrar-solo-seleccionadas',
        RecargarGrid: 'recargar-grid',
        ResetearVista: 'resetear-vista'
    },
    ModalDeFiltrado: {
        Abrir: 'abrir-filtro',
        TeclaPulsada: 'tecla-pulsada',
        Cerrar: 'cerrar-filtro',
        AplicarFiltro: 'aplicar-filtro'
    },
    ModalDePedirDatos: {
        TrasAbrir: 'accion-tras-abrir',
        AlCerrar: 'al-cerrar',
        AlAceptar: 'al-aceptar',
    },
    ModalDeOcultarColumnas: {
        TrasAbrir: 'accion-tras-abrir',
        AlCerrar: 'al-cerrar',
        AlAceptar: 'al-aceptar',
    },
    ModalDeTotales: {
        TrasAbrir: 'accion-tras-abrir',
        AlCerrar: 'al-cerrar'
    },
    ModalDeMensaje: {
        AlCerrar: 'cerrar-modal',
        TrasAbrir: 'tras-abrir-modal',
    },
    ModalSeleccionDeFiltro: {
        Abrir: 'abrir-modal-seleccion',
        Seleccionar: 'seleccionar-elementos',
        Cerrar: 'cerrar-modal-seleccion',
        Buscar: 'buscar-elementos',
        FilaPulsada: 'fila-pulsada',
        ObtenerAnteriores: 'obtener-anteriores',
        ObtenerSiguientes: 'obtener-siguientes',
        ObtenerUltimos: 'obtener-ultimos',
        OrdenarPor: atControl.ordenarPor,
        TeclaPulsada: 'tecla-pulsada'
    },
    ModalExportacion: {
        Exportar: 'exportar',
        Cerrar: 'cerrar-exportacion',
        PulsarSometer: 'pulsar-someter',
        SalirListaCorreos: 'salir-lista-correos'

    },
    ModalEnviarCorreo: {
        Enviar: 'enviar-correo',
        Cerrar: 'cerrar-enviar-correo',
        SeleccionarUsuarios: 'seleccionar-usuarios'
    },
    ModalTransitar: {
        Transitar: 'transitar',
        Cerrar: 'cerrar',
        Seleccionar: 'seleccionar',
        Abrir: 'abrir'
    },
    ModalImprimir: {
        Imprimir: 'imprimir',
        Cerrar: 'cerrar',
        Abrir: 'abrir'
    },
    ModalParaRelacionar: {
        Relacionar: 'nuevas-relaciones',
        Cerrar: 'cerrar-relacionar',
        Buscar: 'buscar-elementos',
        FilaPulsada: 'fila-pulsada',
        ObtenerAnteriores: 'obtener-anteriores',
        ObtenerSiguientes: 'obtener-siguientes',
        ObtenerUltimos: 'obtener-ultimos',
        OrdenarPor: atControl.ordenarPor,
        TeclaPulsada: 'tecla-pulsada'
    },
    ModalParaImputar: {
        Imputar: 'imputar',
        Cerrar: 'cerrar',
        Buscar: 'buscar-elementos',
        FilaPulsada: 'fila-pulsada',
        ObtenerAnteriores: 'obtener-anteriores',
        ObtenerSiguientes: 'obtener-siguientes',
        ObtenerUltimos: 'obtener-ultimos',
        OrdenarPor: atControl.ordenarPor,
        TeclaPulsada: 'tecla-pulsada'
    },
    ModalParaSeleccionarElementos: {
        Buscar: 'buscar-elementos',
        FilaPulsada: 'fila-pulsada',
        ObtenerAnteriores: 'obtener-anteriores',
        ObtenerSiguientes: 'obtener-siguientes',
        ObtenerUltimos: 'obtener-ultimos',
        OrdenarPor: atControl.ordenarPor,
        Cerrar: 'cerrar-seleccionar',
        TeclaPulsada: 'tecla-pulsada'
    },
    ModalParaConsultaDeRelaciones: {
        Cerrar: 'cerrar-consulta',
        Buscar: 'buscar-elementos',
        FilaPulsada: 'fila-pulsada',
        ObtenerAnteriores: 'obtener-anteriores',
        ObtenerSiguientes: 'obtener-siguientes',
        ObtenerUltimos: 'obtener-ultimos',
        OrdenarPor: atControl.ordenarPor,
        TeclaPulsada: 'tecla-pulsada'
    },
    ModalCreacion: {
        Crear: 'crear-elemento',
        Cerrar: 'cerrar-modal',
    },
    ModalEdicion: {
        Modificar: 'modificar-elemento',
        Cerrar: 'cerrar-modal',
        AbrirModalDePermisos: 'abrir-modal-de-permisos',
        DarDeAlta: 'dar-de-alta',
        DarDeBaja: 'dar-de-baja'
    },
    ModalBorrar: {
        Borrar: 'borrar-elemento',
        Cerrar: 'cerrar-modal-de-borrado',
    },
    ListaDinamica: {
        Cargar: 'cargar-lista-dinamica',
        perderFoco: 'perder-foco-lista-dinamica',
        obtenerFoco: 'obtener-foco-lista-dinamica'
    },
    Mnt: {
        Crear: 'crear-elemento',
        Editar: 'editar-elemento',
        Historial: 'historial-elemento',
        Exportar: 'exportar-elementos',
        EnviarCorreo: 'enviar-elementos',
        Transitar: 'transitar-elementos',
        Borrar: 'eliminar-elemento',
        Relacionar: 'relacionar-elementos',
        Dependencias: 'gestionar-dependencias',
        AbrirModalParaRelacionar: 'abrir-modal-para-relacionar',
        AbrirModalParaImputar: 'abrir-modal-para-imputar',
        AbrirModalParaConsultarRelaciones: 'abrir-modal-para-consultar-relaciones',
        Buscar: 'buscar-elementos',
        ObtenerAnteriores: 'obtener-anteriores',
        ObtenerSiguientes: 'obtener-siguientes',
        ObtenerUltimos: 'obtener-ultimos',
        CompartirElemento: 'compartir-elemento',
        EnviarElemento: 'enviar-elemento',
        OrdenarPor: atControl.ordenarPor,
        FilaPulsada: 'fila-pulsada',
        DesplazarColumnaDerecha: 'desplaza-columna-derecha',
        DesplazarColumnaIzquierda: 'desplaza-columna-izquierda',
        AumentarTamano: 'aumentar-tamano',
        ReducirTamano: 'reducir-tamano',
        CambiarSelector: 'cambiar-selector',
        OcultarMostrarFiltro: 'ocultar-mostrar-filtro',
        OcultarMostrarBloque: 'ocultar-mostrar-bloque',
        OcultarMostrarAmpliacion: 'ocultar-mostrar-ampliacion',
        OcultarMostrarDetalle: 'ocultar-mostrar-detalle',
        MostrarOcultarVisorDeDetalle: 'mostrar-ocultar-visor-detalle',
        TeclaPulsada: 'tecla-pulsada',
        MostrarAuditoria: 'mostrar-auditoria',
        OcultarMostrarColumnas: 'ocultar-mostrar-columnas',
        OpcionMenuFlotante: 'opcion-menu-flotante'
    },
    Creacion: {
        Crear: 'nuevo-elemento',
        Cerrar: 'cerrar-creacion',
    },
    Edicion: {
        Modificar: 'modificar-elemento',
        Cerrar: 'cancelar-edicion',
        MostrarPrimero: 'mostrar-primero',
        MostrarAnterior: 'mostrar-anterior',
        MostrarSiguiente: 'mostrar-siguiente',
        MostrarUltimo: 'mostrar-ultimo',
        procesarOpcionMf: 'procesar-menu-flotante',
        TrasCargarGrid: 'tras-cargar-grid',
        PlegarTodo: 'plegar-todo',
        DesplegarTodo: 'desplegar-todo',
        MostrarOcultarVisor: 'mostrar-ocultar-visor',
        SiguienteArchivo: 'siguiente-archivo',
        AnteriorArchivo: 'anterior-archivo',
        Retroceder: 'retroceder',
        Avanzar: 'avanzar',
        ZoomMas: 'zoom-mas',
        ZoomMenos: 'zoom-menos',
        DescargarArchivo: 'descargar-archivo',
        CompartirConWhatsApp: 'compartir-con-whatsapp',
        CompartirConGuid: 'compartir-con-guid',
        CompartirElemento: 'compartir-elemento',
        ConsultarConGuid: 'consultar-con-guid',
        ResumirArchivo: 'resumir-archivo',
        PasarOcr: 'pasar-ocr',
        FacturasRec: {
            Analizar: 'analizar-factura',
        }
    },
    Historial: {
        Cerrar: 'cerrar-historial',
        OcultarMostrarFiltro: 'ocultar-mostrar-filtro',
        CargarHistorial: 'cargar-historial',
        AplicarOrdenInicial: 'aplicar-orden-inicial',
        AnularOrdenacion: 'anular-ordenacion',
        MostrarDetalle: 'mostrar-detalle'
    },
    Formulario: {
        Aceptar: 'aceptar',
        Cerrar: 'cerrar',
        OcultarMostrarBloque: 'ocultar-mostrar-bloque',
        AbrirFiltro: 'abrir-filtro',
        CerrarFiltro: 'cerrar-filtro',
        AplicarFiltro: 'aplicar-filtro',
        TeclaPulsada: 'tecla-pulsada',
        OpcionMenuFlotante: 'opcion-menu-flotante'
    },
    Expansores: {
        OcultarMostrarBloque: 'ocultar-mostrar-bloque',
        NavegarDesdeEdicion: 'navegar-desde-edicion',
        NavegarAEditar: 'navegar-a-editar',
        AbrirModalParaCrearYVincular: 'abrir-modal-crear-y-vincular',
        AbrirModalParaCrearDetalle: 'abrir-modal-crear-detalle',
        AbrirModalParaVincular: 'abrir-modal-para-vincular',
        AbrirModalDeRelacionParaCrear: 'abrir-modal-crear-relacion',
        AbrirModalDeDetalleParaCrear: 'abrir-modal-crear-detalle',
        AbrirModalDeRelacionParaEditar: 'abrir-modal-editar-relacion',
        BorrarRelacion: 'eliminar-relacion',
        MostrarBloque: 'mostrar-bloque',
        MostrarPropiedad: 'mostrar-propiedad',
        TrasAbrirModal: "tras-abrir-modal",
        TrasCargarExpansor: "tras-cargar-expansor",
        AbrirAgenda: "abrir-agenda"
    },
    SelectorDeElementos: {
        Seleccionar: 'opcion-seleccionar',
        PerderFoco: 'perder-foco',
        ObtenerFoco: 'obtener-foco'
    },
    TrabajoDeUsuario: {
        iniciar: 'iniciar-trabajo',
        bloquear: 'bloquear-trabajo',
        desbloquear: 'desbloquear-trabajo',
        resometer: 'resometer-trabajo',
        traza: 'traza-trabajo',
        errores: 'errores-trabajo'
    },
    ModalDeRelacionar: {
        CrearRelacion: 'crear-relacion',
        ModificarRelacion: 'modificar-relacion',
        Cerrar: 'cerrar-modal-relacion',
        CrearVinculo: 'crear-vinculo',
        CrearDetalle: 'crear-detalle',
        Vincular: 'vincular'
    },
    Jerarquia: {
        CrearNodo: 'crear-nodo',
        ModificarNodo: 'modificar-nodo',
        EliminarNodo: 'eliminar-nodo',
        CancelarModificacion: 'cancelar-modificacion',
        MostarJerarquia: 'mostrar-jerarquia',
        PlegarJerarquia: 'plegar-jerarquia'
    },
    Sociedad: {
        EditarContacto: 'editar-contacto',
    },
    Archivo: {
        Descargar: 'boton-descargar',
        Eliminar: 'boton-eliminar',
        Firmar: 'boton-firmar',
        Bloquear: 'boton-bloquear',
        ModalDeCambioDeNombre: {
            modificarNombreArchivo: 'modificarNombreArchivo(ctdArchivosAnexados.id)'
        },
        ModalDeFirmar: {
        }
    },
    Interlocutor: {
        CrearPersona: 'crear-persona',
        CrearSociedad: 'crear-sociedad',
        CrearContacto: 'crear-contacto',
    }
};


const enumCssOpcionMenu = {
    DeElemento: 'opcion-menu-de-elemento',
    DeVista: 'opcion-menu-de-vista',
    Basico: 'opcion-menu-basica',
    Bloqueada: 'opcion-mf-bloqueada',
    OcultarLi: 'ocultar-li',
    OcultarHr: 'ocultar-hr',
};

const enumCalificadorDireccion = {
    Correspondencia: 'correspondencia',
    Contacto: 'contacto',
    Fiscal: 'fiscal',
    Ejecucion: 'ejecucion',
    Entrega: 'ejecucion'
};

const enumModoTrabajo = {
    creando: 'nuevo',
    exportando: 'exportando',
    enviandoCorreo: 'enviando-correo',
    transitando: 'transitando',
    editando: 'editando',
    consultando: 'consultando',
    copiando: 'copiando',
    borrando: 'borrando',
    mantenimiento: 'mantenimiento',
    historial: 'historial',
};

const ltrAccionesModal = {
    BloquearCg: 'bloquear-cg',
    ProponerCg: 'proponer-cg',
    ProponerSolicitante: 'proponer-solicitante',
    ProponerElTipo: 'proponer-el-tipo'
};

const ltrParametrosNeg = {
    ObtenerCertificado: 'ObtenerCertificado',
    Administracion: {
        Tareas: {
            VincularA: {
                Expediente: 'VincularAlExpediente'
            }
        }
    }
};

const ltrOperacion = {
    nameOf: 'ltrOperacion',
    CargarDatos: 'CargarDatos',
    Exportar: 'Exportar',
    ModificoParaTransitar: 'ModificoParaTransitar',
    ModificoParaImprimir: 'ModificoParaImprimir',
    ModificoParaNavegar: 'ModificoParaNavegar',
    ModificarPorId: 'ModificarPorId',
    AccionTrasGuardar: 'AccionTrasGuardar',
    HayCambios: 'hayCambios'
}

const ltrEtiquetas = {
    Negocio: {
        PlantillaPorTipo: {
            Plantilla: 'Plantilla'
        },
        PlantillaDeNegocio: {
            Plantilla: 'Plantilla'
        }
    },
    Juridico: {
        Contratos: {
            Avance: {
                Pagado: 'Pagado'
            }
        }
    },
    SisDoc: {
        Archivador: {
            GestionarCarpetas: 'Mostrar carpetas',
            CrearCarpetas: 'Crear carpetas'
        }
    },
    Gasto: {
        Pagos: {
            VenceEl: 'Vence el',
            PagarEl: 'Pagar el',
            AbonadoEl: 'Abonado el',
            PagadoEl: 'Pagado el'
        },
        Remesas: {
            PagadoEl: 'Pagado el',
            SeDeberiaHaberPagadoEl: 'Se debería haber pagado el',
            SePagaraEl: '(Pdte.) Se pagará el'
        }
    },
    Venta: {
        Remesas: {
            CargadaEl: 'Cargada el',
            SeDeberiaHaberCargadoEl: 'Se debería haber cargado el',
            SeCargaraEl: '(Pdte.) Se cargará el'
        },
        Cobros: {
            Cobrar: 'Cobrar'
        },
        Abonos: {
            Abonar: 'Abonar'
        },
        FacturaEmt: {
            Cobrado: 'Cobrado',
            Abonado: 'Abonado',
        }
    }
}

const ltrModal = {
    SisDoc: {
        Archivo: {
            Bloqueo: {
                Opcion: {
                    Bloquear: 'Bloquear'
                }
            },
            DesBloqueo: {
                Opcion: {
                    Desbloquear: 'Desbloquear'
                }
            }
        }
    }
}
const ltrTitle = {
    Gasto: {
        FacturaRec: {
            linea: {
                BaseImponible: {
                    SoloIva: "Indica la BI que le afecta al iva a seleccionar",
                    SoloIRPF: "Indica la BI que le afecta al irpf a seleccionar",
                }
            }
        }
    },
    Venta: {
        FacturaEmt: {
            Cobrado: 'Cobrado',
            Abonado: 'Abonado',
        }

    }
}

enum enumIa {
    IaPerplexity = 'IaPerplexity',
    IaApyHub = 'IaApyhub'
}

const ltrPropiedades = {
    enumTipoPermiso: 'enumtipopermiso',
    baja: 'baja',
    DelSistema: 'DelSistema',
    NombreModificable: 'NombreModificable',
    Informacion: 'Informacion',
    RecargarGrid: 'recargar',
    Dialogos: {
        Exportacion: {
            LasMostaradas: 'las-mostradas'
        }
    },
    Ia: {
        IdArchivo: "idArchivo",
        Nombre: 'nombre',
        Json: {
            Proveedor: 'proveedor',
            Nif: 'Nif',
            eMail: 'eMail',
            Numero: 'NumeroFactura',
            Concepto: 'Concepto',
            Telefono: 'Telefono',
            CodigoPostal: 'CodigoPostal',
            Pais: 'Pais',
            Provincia: 'Provincia',
            Municipio: 'Municipio',
            TipodeVia: 'TipodeVia',
            Calle: 'Calle',
            NumeroPolicia: 'NumeroPolicia',
            RestoDireccion: 'RestoDireccion',
            FormaDePago: 'FormaDePago',
            ClaseDePago: 'ClaseDePago',
            NumeroIban: 'CuentaBancaria',
            FacturadaEl: 'fecha',
            VenceEl: 'FechaVencimiento',
            Total: 'total',
            Bi: 'bi',
            Iva: 'totalIva',
            Irpf: 'totalIrpf'
        }
    },
    Entorno: {
        Usuario: {
            id: 'idusuario',
            nombre: 'nombre-usuario',
            IdArchivoCertificado: 'idarchivodelcertificado',
            PassworDelCertificado: 'passwordelcertificado',
            DatosCertificado: 'datoscertificado',
            eMail: 'email',
            Activo: 'activo',
            Administrador: 'EsAdministrador'
        },
        Agenda: {
            Id: literal.id,
        },
        EventoDeAgenda: {
            inicio: 'inicio',
            fin: 'fin',
            eventoDeDia: 'eventodedia',
            EsDelSistema: 'esdelsistema',
            Agenda: 'agenda',
            IdAgenda: 'idagenda'
        },
        Menu: {
            activo: 'activo',
            iaSeleccionada: 'iaSeleccionada'
        },
        Vista: {
            id: 'idvistamvc',
            Indicadores: {
                SiempreEnConsulta: 'SiempreEnConsulta',
                CapaConVinculados: 'CapaConVinculados',
                TamanoDelVisor: 'tamano-del-visor',
                TamanoDeGraficos: 'tamano-de-graficos',
                IaUsada: 'IA_Usada',
                MostrarVisorAlIniciar: 'MostrarVisorAlIniciar',
                UsaTotalizador: 'UsaTotalizador'
            }
        },
        seguridad: {
            rol: {
                id: 'idrol',
                idpermiso: 'idpermiso'
            },
            Puesto: {
                id: 'idpuesto',
            },
        },
        MiCorreo: {
            Id: 'id',
            Asunto: 'Asunto',
            Cuerpo: 'Cuerpo',
            CuerpoHtml: 'CuerpoHtml',
            Fecha: 'Fecha',
            IdMensaje: 'IdMensaje',
            Adjuntos: 'Adjuntos',
            ConAdjuntos: 'ConAdjuntos',
            Emisor: 'Emisor',
            Buzon: 'Buzon'
        },
        EnviarElemento: {
            IdUsuario: 'idusuario',
            Usuario: 'usuario',
            Asunto: 'Asunto',
            Cuerpo: 'Cuerpo',
            OtorgarGestor: 'OtorgarGestor'
        },
    },
    Maestros: {
        unitario: {
            venta: 'venta',
            coste: 'coste',
            clase: 'Clase',
            idnaturaleza: 'IdNaturaleza',
            idunidad: 'IdUnidad',
            nombre: 'nombre'
        },
        Tarifa: {
            IdElemento: 'IdElemento',
            IdProveedor: 'IdProveedor',
            Referencia: 'Referencia',
            Tarifa: 'Tarifa'
        },
        Contabilidad: {
            IvaR: {
                Clase: 'clase',
                Porcentaje: 'Porcentaje',
                Exento: 'exento',
                DescripcionFiscal: 'DescripcionFiscal'
            },
            IvaS: {
                Porcentaje: 'Porcentaje',
                Exento: 'exento',
            },
            Irpf: {
                Porcentaje: 'Porcentaje'
            },
            Cuenta: {
                Id: 'id',
                Codigo: 'Codigo'
            },
            CuentaBancaria: {
                Iban: 'Iban',
                Entidad: 'entidad',
                Oficina: 'oficina',
                Dc: 'dcccc',
                Numero: 'numero',
                NumeroIban: 'NumeroIban',
                Alias: 'Alias',
                Banco: 'Banco',
                Activa: 'Activa'
            }
        },
    },
    Sometidos: {
        Correo: {
            Asunto: 'asunto',
            CuerpoMensaje: 'cuerpoMensaje'
        },
        TrabajoDeUsuario: {
            estado: 'estado',
            idsometedor: 'idsometedor'
        }
    },
    Negocio: {
        idNegocio: 'idnegocio',
        nombre: literal.nombreNegocio,
        propiedad: literal.negocio,
        PlantillaDeExportacion: {
            Plantilla: 'Nombre',
            IdPlantilla: 'Id',
            IdCg: literal.idCg,
            listaDePlantillas: 'listaDePlantillas',
            listaDeCgs: 'listaDeCgs',
            NombreArchivador: 'archivador',
            DescripcionArchivador: 'motivo'
        },
        PlantillaDeCreacion: {
            Plantilla: 'Plantilla',
            IdPlantilla: 'Id',
            Remplazar: 'Remplazar'
        },
        PlantillaDeFiltrado: {
            Plantilla: 'Plantilla',
            IdPlantilla: 'Id',
            Valor: 'valor',
            Vista: 'Vista',
            DisposicionDelEncolumnado: 'disposicion-del-encolumnado',
            OrdenacionDelResultado: 'ordenacion-del-resultado',
            GuardarColumnasDelGrid: 'guardar-columnas-del-grid',
            TamanoDelEncolumnado: 'tamano-del-encolumnado',
            OrdenacionGuardada: atGrid.accion.ordenar
        },
        PlantillaPorTipo: {
            Id: literal.id,
            IdPermiso: 'idpermiso',
            IdAccion: 'idaccion',
            Accion: 'accion',
            Plantilla: 'plantilla'
        },
        PlantillaDeNegocio: {
            Id: literal.id,
            IdPermiso: 'idpermiso',
            IdAccion: 'idaccion',
            Accion: 'accion',
            Plantilla: 'plantilla'
        },
        Totales: {
            Comun: {
                Procesados: 'Procesados',
            }
        }
    },
    Selector: {
        IdElemento: atControl.idElemento,
        Elemento: 'Elemento',
    },
    Callejero: {
        Direccion: {
            Pais: 'pais',
            IdPais: 'idpais',
            Provincia: 'provincia',
            IdProvincia: 'idprovincia',
            Municipio: 'Municipio',
            IdMunicipio: 'idMunicipio',
            Barrio: 'barrio',
            IdBarrio: 'idbarrio',
            Zona: 'zona',
            IdZona: 'idzona',
            Numero: 'numero',
            Escalera: 'escalera',
            Piso: 'piso',
            Puerta: 'puerta',
            Cp: 'codigopostal',
            IdCp: 'idcp',
            Calle: 'calle',
            TipoDeVia: 'tipoDeVia',
            Resto: 'RestoDeDireccion',
            NombreDireccion: 'nombredireccion',
            Url: 'url',
            Calificador: 'calificador'
        },
        Calle: {
            Pais: 'pais',
            Provincia: 'provincia',
            Municipio: 'Municipio',
            IdMunicipio: 'IdMunicipio',
            TipoDeVia: 'tipodevia',
            IdPais: 'idpais',
            IdProvincia: 'idprovincia',
            Zona: 'zona',
            Cp: 'codigopostal',
            Nombre: literal.nombre
        },
        Pais: {
            Iso2: 'ISO2',
            EsUE: 'EsUE'
        }
    },
    Elemento: {
        Id: literal.id,
        Expresion: literal.expresion,
        RowVersion: 'RowVersion',
        Elemento: 'elemento',
        EstaCancelada: 'estacancelada',
        EstaTerminada: 'estaterminada',
        IdElemento: atControl.idElemento,
        ModoDeAcceso: literal.ModoDeAcceso,
        EsAdministrador: 'esadministrador',
        EsInterventor: 'esinterventor',
        EsGestor: 'esgestor',
        IdArchivo: 'idArchivo',
        IdCreador: 'idCreador',
        Textos: 'textos',
        FitrarPorBaja: 'filtrarporbaja',
        Baja: 'baja',
        Bloqueado: 'bloqueado',
        Bloqueador: 'bloqueador',
        DarDeAlta: 'Dar de alta',
        DarDeBaja: 'Dar de baja',
        Nombre: literal.nombre,
        ClaseDeElemento: 'ClaseDeElemento',
        IdClaseDeElemento: 'IdClaseDeElemento',
        Referencia: 'referencia',
        Descripcion: 'descripcion',
        ConCg: {
            IdCg: literal.idCg,
            Cg: literal.Cg,
            IdSociedadDelCg: 'idsociedaddelcg',
            IdSociedad: 'idsociedad',
        },
        ConLineas: {
            Anotacion: 'anotacion'
        },
        ConTipo: {
            IdTipo: literal.IdTipo,
            Tipo: literal.Tipo,
        },
        DeProceso: {
            IdEstado: 'idestado',
            Estado: 'estado',
            IdEstadoAnterior: 'idestadoanterior',
            EstadoAnterior: 'estadoanterior',
            IdTransicionAplicable: 'idtransicionaplicable',
            TransicionAplicable: 'transicionaplicable',
            TransicionesDisponibles: 'TransicionesDisponibles',
            Expresion: literal.expresion,
        },
        Historial: {
            Clase: 'clase',
            Suceso: 'suceso',
            Detalle: 'detalle',
            AccionJs: 'accionjs',
            IdRegistro: 'idregistro',
            IdElemento: 'idelemento',
            Negocio: 'negocio'
        },
        Detalle: {
            Titulo: 'titulo'
        }
    },
    TipoDeElemento: {
        IdPermisoDeAdministrador: 'IdPermisoDeAdministrador',
        IdPermisoDeGestor: 'IdPermisoDeGestor',
        IdPermisoDeConsultor: 'IdPermisoDeConsultor',
        IdPermisoDeInterventor: 'IdPermisoDeInterventor',
        Mascara: 'mascara',
        Marcador: 'Marcador',
        ModoDeAcceso: literal.ModoDeAcceso,
        HayClases: 'HayClases',
        Activo: 'Activo',
        NombreModificable: 'NombreModificable',
        EditarTrasCrear: 'EditarTrasCrear',
    },
    Transiciones: {
        DelSistema: 'delsistema',
        Origen: 'origen',
        Asunto: 'asunto',
        Detalle: 'detalleAsunto',
        Usuarios: 'Usuarios',
        Destino: 'idestadodestino',
        Archivo: 'IdArchivo',

    },
    SisDoc: {
        CopiarArchivo: {
            Enlazar: 'EnlazarArchivos',
            Copiar: 'CopiarArchivos'
        },
        PlantillasDisponibles: {
            Abrir: 'abrir',
            Plantillas: 'plantillas',
            IdPlantilla: 'idplantilla',
            Plantilla: 'plantilla',
            Clase: 'clase'
        },
        ComoArchivar: {
            IdArchivador: 'IdArchivador',
            Archivador: 'Elemento',
            IdCarpeta: 'IdCarpeta',
            CrearArchivador: 'creararchivador',
            CarpetaDestino: 'CarpetaDestino',
            AbrirAlAsociar: 'abriralasociar',
        },
        ComoVincular: {
            IdTarea: 'IdTarea',
            Tarea: 'Tarea',
            CrearTarea: 'creartarea',

            IdRegistroEs: 'IdRegistroEs',
            RegistroEs: 'RegistroEs',
            CrearRegistroEs: 'crearregistroes',

            IdFacturaRec: 'IdFacturaRec',
            FacturaRec: 'FacturaRec',
            CrearFacturaRec: 'crearfacturarec',

            IdExpediente: 'IdExpediente',
            Expediente: 'Expediente',
            CrearExpediente: 'crearexpediente',

            AbrirAlAsociar: 'abriralasociar',
            CarpetaDestino: 'CarpetaDestino',
            ArchivadorDestino: 'ArchivadorDestino',
            TareaDestino: 'TareaDestino',
            IdCarpetaDestino: 'IdCarpetaDeDestino',
            IdArchivadorDestino: 'IdArchivadorDeDestino',
            IdTareaDestino: 'IdTareaDeDestino'
        },
        Archivador: {
            IdArchivador: 'idarchivador',
            NombreDelArchivador: 'nombre-archivador',
            Nombre: 'nombre',
            Descripcion: 'descripcion',
            propiedad: 'archivador',
            IdCg: literal.idCg,
            Cg: literal.Cg,
            Sincronizar: 'sincronizar',
            NombreModificable: 'NombreModificable',
            ConCarpetas: 'ConCarpetas',
            EsUnCorreo: 'EsUnCorreo',
            Carpetas: 'Carpetas',
            Indicadores: {
                PermiteSincronizar: 'PermiteSincronizar',
                IdTipoArchivadorDeFacturaRec: 'IdTipoArchivadorDeFacturaRec'
            }
        },
        CircuitosDoc: {
            EsLoteContable: 'EsLoteContable',
            Indicadores: {
                IdTipoActividadFormativa: 'IdTipoActividadFormativa',
                TipoActividadFormativa: 'TipoActividadFormativa',
                IdTipoEstimacionDirecta: 'IdTipoEstimacionDirecta',
                TipoEstimacionDirecta: 'TipoEstimacionDirecta',
                IdTipoLoteContable: 'IdTipoLoteContable',
                TipoLoteContable: 'TipoLoteContable',
                IdTipoFichada: 'IdTipoFichada',
                TipoFichada: 'TipoFichada',
            }
        },
        Archivo: {
            idArchivo: 'idArchivo',
            idOriginal: 'idoriginal',
            EstaBloqueado: 'EstaBloqueado',
            EsDeUnArchivadorVinculado: 'EsDeUnArchivadorVinculado',
            Original: 'original',
            Destino: 'destino',
            ArchivadorDestino: 'ArchivadorDestino',
            IdsDeArchivos: 'IdsDeArchivos',
            IdOrigenDiferente: 'IdOrigenDiferente',
            Auditoria: 'auditoria',
            PadreBloqueado: 'PadreBloqueado',
            CaducaEl: 'CaducaEl'
        },
        ImportarZip: {
            IdArchivador: 'IdArchivador'
        },
        ProcesarFarConIa: {
            IdArchivador: 'IdArchivador',
            CarpetaSeleccionada: "CarpetaSeleccionada"
        },
        Carpeta: {
            Nombre: literal.nombre,
            Archivador: 'archivador'
        },
        Bloqueo: {
            idArchivo: 'idArchivo',
            Motivo: 'Motivo',
            Auditoria: 'Auditoria'
        }
    },
    RegistroEs: {
        IdCg: literal.idCg,
        Cg: literal.Cg,
        IdSolicitante: literal.IdSolicitante,
        Solicitante: literal.Solicitante
    },
    Contabilidad: {
        CrearLote: {
            Ejercicio: 'ejercicio',
            Descontabilizar: 'descontabilizar',
            RespetarFechaContable: 'RespetarFechaContable',
            FechaContable: 'FechaContable'
        }
    },
    Terceros: {
        Cg: {
            idSociedad: 'idsociedad',
            Responsable: 'responsable',
            eMail: 'email'
        },
        Trabajador: {
            eMail: 'email',
            Telefono: 'telefono',
            Cuenta: 'Cuenta',
            IdCuenta: 'IdCuenta',
            Iban: 'Iban',
            Entidad: 'entidad',
            Oficina: 'oficina',
            Dc: 'dcccc',
            Numero: 'numero',
            ClaseDeCuenta: 'clase',
            CuentaActiva: 'activa',
            AliasDeCuenta: 'alias'
        },
        Persona: {
            CrearInterlocutor: 'CrearInterlocutor',
            CrearProcurador: 'CrearProcurador',
            CrearAbogado: 'CrearAbogado',
            CrearProveedor: 'CrearProveedor',
            CrearCliente: 'CrearCliente',
            CrearTrabajador: 'CrearTrabajador',
            IdCliente: 'idcliente',
            IdInterlocutor: 'idinterlocutor',
            IdProveedor: 'idProveedor',
            IdAbogado: 'idabogado',
            IdProcurador: 'idprocurador',
            Indicadores: {
                TercerosJudiciales: 'TercerosJudiciales',
            }
        },
        Sociedad: {
            Expresion: literal.expresion,
            idSociedad: 'idsociedad',
            idAgenda: 'IdAgenda',
            Agenda: 'agenda',
            EsUnaDeMisSociedades: 'EsUnaDeMisSociedades',
            UsaVerifactu: 'UsaVerifactu',
            VerifactuEnProductivo: 'VerifactuEnProductivo',
            Nombre: literal.nombre,
            NIF: 'Nif',
            sociedad: 'sociedad',
            Telefono: 'telefono',
            eMail: 'eMail',
            CrearInterlocutor: 'CrearInterlocutor',
            CrearProcurador: 'CrearProcurador',
            CrearAbogado: 'CrearAbogado',
            CrearProveedor: 'CrearProveedor',
            CrearCliente: 'CrearCliente',
            CrearTrabajador: 'CrearTrabajador',
            responsable: 'responsable',
            IdCliente: 'idcliente',
            IdInterlocutor: 'idinterlocutor',
            IdProveedor: 'idProveedor',
            IdAbogado: 'idabogado',
            IdProcurador: 'idprocurador',
            EsInterlocutor: 'esinterlocutor',
            TipoDeTercero: 'TipoDeTercero',
            CuentaBancaria: {
                Activa: 'activa',
                Alias: 'alias',
                Clase: 'clase',
                Iban: 'Iban',
                Entidad: 'entidad',
                Oficina: 'oficina',
                Dc: 'dcccc',
                Numero: 'numero',
            },
            Tarjeta: {
                Activa: 'activa',
                Alias: 'alias',
                CuentaDeCargo: 'CuentaDeCargo',
                IdCuentaDeCargo: 'IdCuentaDeCargo'
            },
            Indicadores: {
                TercerosJudiciales: 'TercerosJudiciales',
            }
        },
        CertificadoDeUnaSociedad: {
            idSociedad: 'idsociedad',
        },
        Juzgado: {
            clase: 'Clase',
            municipio: 'Municipio',
            calificador: 'Calificador',
            nombre: 'Nombre'
        },
        Cliente: {
            Expresion: literal.expresion,
            Telefono: 'Telefono',
            eMail: 'eMail',
            Id: literal.id,
            IdInterlocutor: 'IdInterlocutor',
            Iban: 'Iban',
            Entidad: 'entidad',
            Oficina: 'oficina',
            Dc: 'dcccc',
            Numero: 'numero',
            ClaseDeCuenta: 'clase',
            CuentaActiva: 'activa',
            AliasDeCuenta: 'alias',
            Vat: 'VAT',
            EsIntraComunitario: 'EsIntraComunitario',
            TipoDeTercero: 'TipoDeTercero'
        },
        Proveedor: {
            Nif: 'NIF',
            Expresion: 'Expresion',
            Telefono: 'Telefono',
            eMail: 'eMail',
            Id: literal.id,
            IdInterlocutor: 'IdInterlocutor',
            Iban: 'Iban',
            Entidad: 'entidad',
            Oficina: 'oficina',
            Dc: 'dcccc',
            Numero: 'numero',
            ClaseDeCuenta: 'clase',
            CuentaActiva: 'activa',
            AliasDeCuenta: 'alias',
            NumeroIban: 'NumeroIban',
            IdUnidad: 'IdUnidad',
            IdNaturaleza: 'IdNaturaleza',
            Concepto: 'Concepto',
            IdIva: 'IdIvaS',
            IdIrpf: 'IdIrpf',
            cgPropuesto: 'cgPropuesto',
            tipoPropuesto: 'tipoFarPropuesto',
            idcgPropuesto: 'idcgPropuesto',
            idtipoPropuesto: 'idtipoFarPropuesto',
            biPropuesto: 'biPropuesto',
            ModoDePago: 'ModoDePago',
            DomiciliadaEn: 'DomiciliadaEn',
            Tarjeta: 'Tarjeta'

        },
        Interlocutor: {
            //Expresion: 'Expresion',
            //Telefono: 'Telefono',
            //eMail: 'eMail',
            //Id: literal.id,
            Iban: 'Iban',
            Entidad: 'entidad',
            Oficina: 'oficina',
            Dc: 'dcccc',
            Numero: 'numero',
            ClaseDeCuenta: 'clase',
            CuentaActiva: 'activa',
            AliasDeCuenta: 'alias',
            IdSociedad: 'IdSociedad'
        }
    },
    Certificado: {
        idCertificado: 'idcertificado',
        propiedad: 'certificado',
    },
    Tarea: {
        IdCg: literal.idCg,
        Cg: literal.Cg,
        IdSolicitante: literal.IdSolicitante,
        Solicitante: literal.Solicitante,
        Etapa: 'Etapa',
        Etapas: 'Etapas',
        UsaPlanificacion: 'UsaPlanificacion',
        IdResponsable: 'IdResponsable',
        Responsable: 'responsable',
        IdExpediente: 'IdExpediente',
        Expediente: 'Expediente',
        IdFacturaEmt: 'IdFacturaEmt',
        FacturaEmt: 'FacturaEmt',
        Planificacion: {
            PlfDeInicio: 'PlfDeInicio',
            PlfDeFin: 'PlfDeFin',
            Iniciada: 'Iniciada',
            Finalizada: 'Finalizada',
            Duracion: 'Duracion',
            MedidoEn: 'MedidoEn'
        },
        VinculadoA: {
            IdExpediente: 'IdExpediente'
        }
    },
    Expediente: {
        usaPpts: 'usappts',
        usaTareas: 'usaTareas',
        scDeCompra: 'ScDeCompra',
        scDeVenta: 'ScDeVenta',
        Etapas: 'Etapas',
        ClaseDeExpediente: 'ClaseDeExpediente',
        Indicadores: {
            IdTipoActividad: 'IdTipoActividad',
            TipoActividad: 'TipoActividad'
        }
    },
    Guarderias: {
        Infante: {
            IdCurso: 'idcurso',
            IdInfante: 'idInfante'
        },
    },
    Juridico: {
        CopiarPlfDeVenta: {
            ClaseDeContrato: 'ClaseDeContrato',
            Planificador: 'Planificador',
            Contrato: 'Contrato',
            Inicio: 'Inicio',
            Hasta: 'Hasta'
        },
        PlanificadorDeVenta: {
            IdContrato: 'idcontrato',
            IdTipoDePlanificacion: 'idtipodeplanificacion',
            TipoDePlanificacion: 'tipodeplanificacion',
            IdTipoDeParte: 'idtipodeparte',
            TipoDeParte: 'tipodeparte',
            IdTipoDeFactura: 'idtipodefactura',
            TipoDeFactura: 'tipodeparte',
            inicio: 'inicio',
            hasta: 'hasta',
            IdSociedadDelCg: 'IdSociedadDelCg',
            CgDeLaPlanificacion: 'CgDeLaPlanificacion',
            lote: 'lote',
            idlote: 'idlote',
            linea: {
                orden: 'orden',
                coste: 'coste',
                venta: 'venta',
                cantidad: 'cantidad',
                clase: 'Clase',
                naturaleza: 'Naturaleza',
                idnaturaleza: 'IdNaturaleza',
                idunidad: 'IdUnidad',
                unidad: 'Unidad',
                unitario: 'unitario',
                selectorDeIvaR: 'IvaRepercutido',
                ImporteSinDto: 'ImporteSinDto',
                ImporteDeDto: 'ImporteDeDto',
                ImporteDeIva: 'ImporteDeIva',
                ImporteDeLinea: 'ImporteDeLinea',
                descuentoPorLinea: 'descuento',
                iva: 'iva',
                tipoDeLinea: 'tipoDeLinea',
                concepto: 'concepto',
                ivarepercutido: 'ivarepercutido'
            }
        },
        lote: {
            idContrato: 'idcontrato',
            vigenteDesde: 'vigentedesde',
            vigenteHasta: 'vigentehasta',
            unitarios: {
                unitario: 'unitario',
                coste: 'coste',
                venta: 'venta'
            }
        },
        Minuta: {
            creadoEl: 'creadoel',
            orden: 'orden',
            concepto: 'concepto'
        },
        TipoCtr: {
            TipoFacturaEmt: 'tipofacturaemt',
            ClaseDeCtr: 'clasedecontrato'
        },
        Contrato: {
            Id: literal.id,
            Expresion: literal.expresion,
            ClaseDeContrato: 'ClaseDeContrato',
            Etapa: 'Etapa',
            idExpediente: 'idexpediente',
            DatosDeVenta: {
                Cliente: 'Cliente',
                Contacto: 'Contacto',
                Telefono: 'Telefono',
                eMail: 'eMail',
                InicioContrato: 'InicioContrato',
                FinContrato: 'FinContrato'
            },
            DatosDeCompra: {
                Proveedor: 'Proveedor',
                Contacto: 'Contacto',
                Telefono: 'Telefono',
                eMail: 'eMail',
                InicioContrato: 'InicioContrato',
                FinContrato: 'FinContrato'
            },
            MatriculaDeGuarderia: {
                Cliente: 'Cliente',
                Contacto: 'Contacto',
                Telefono: 'Telefono',
                eMail: 'eMail',
                Infante: 'Infante',
                Curso: 'Curso'
            },
            Saldos: {
                Importe: 'Importe',
                Adendado: 'Adendado',
                Bloqueo: 'bloqueo'
            },
            Prorrogas: {
                ClaseDeProrroga: 'ClaseDeProrroga',
                FechaUltimaProrroga: 'FechaUltimaProrroga',
                Meses: 'Meses'
            },
            AvalSolicitado: {
                ImporteAval: 'ImporteAval'
            },
            Avance: {
                Cobrado: 'Cobrado'
            }
        }
    },
    Logistica: {
        Pedido: {
            id: literal.id,
            idExpediente: 'idexpediente',
            Expediente: 'Expediente',
            IdProveedor: 'idproveedor',
            Proveedor: 'proveedor',
            Etapas: 'Etapas',
            Contrato: 'Contrato',
            Indicadores: {
                Naturaleza: 'Naturaleza',
                UnidadDeMedida: 'UnidadDeMedida',
                TipoDeLinea: 'TipoDeLinea',
                ClaseDeUnitario: 'ClaseDeUnitario'
            },
            linea: {
                orden: 'orden',
                tipoDeLinea: 'TipoDeLinea',
                concepto: 'concepto',
                unitario: 'unitario',
                anotacion: 'anotacion',
                descuentoPorLinea: 'descuento',
                ImporteSinDto: 'ImporteSinDto',
                ImporteDeDto: 'ImporteDeDto',
                ImporteDeLinea: 'ImporteDeLinea',
                precio: 'precio',
                cantidad: 'cantidad',
                clase: 'Clase',
                naturaleza: 'Naturaleza',
                unidad: 'Unidad'
            }
        },
        RenombraPedido: {
            IdElemento: 'idElemento',
            Nombre: literal.nombre
        },
    },
    Venta: {
        TipoPpt: {
            TipoFacturaEmt: 'tipofacturaemt',
            TipoParteTr: 'tipopartetr',
            ClaseDePpt: 'clasedepresupuesto'
        },
        Presupuesto: {
            id: literal.id,
            ivaPropuesto: 'iva',
            descuentoPropuesto: 'descuento',
            idExpediente: 'idexpediente',
            IdSolicitante: literal.IdSolicitante,
            Solicitante: literal.Solicitante,
            Expediente: 'Expediente',
            IdTipoFacturaPorDefecto: 'IdTipoFacturaPorDefecto',
            TipoFacturaPorDefecto: 'TipoFacturaPorDefecto',
            IdTipoPartePorDefecto: 'IdTipoPartePorDefecto',
            TipoPartePorDefecto: 'TipoPartePorDefecto',
            Etapas: 'Etapas',
            Indicadores: {
                Naturaleza: 'Naturaleza',
                UnidadDeMedida: 'UnidadDeMedida',
                TipoDeLinea: 'TipoDeLinea',
                ClaseDeUnitario: 'ClaseDeUnitario'
            },
            linea: {
                orden: 'orden',
                tipoDeLinea: 'TipoDeLinea',
                concepto: 'concepto',
                unitario: 'unitario',
                anotacion: 'anotacion',
                ivaPorLinea: 'iva',
                descuentoPorLinea: 'descuento',
                ImporteSinDto: 'ImporteSinDto',
                ImporteDeDto: 'ImporteDeDto',
                ImporteDeIva: 'ImporteDeIva',
                ImporteDeLinea: 'ImporteDeLinea',
                selectorDeIvaR: 'IvaRepercutido',
                precio: 'precio',
                cantidad: 'cantidad',
                clase: 'Clase',
                naturaleza: 'Naturaleza',
                unidad: 'Unidad'
            }
        },
        RenombraPpt: {
            IdElemento: 'idElemento',
            Nombre: literal.nombre
        },
        PlfDeVenta: {
            IdTipo: literal.IdTipo,
            Tipo: literal.Tipo,
            IdContrato: 'idcontrato',
            Contrato: 'contrato',
            IdCliente: 'idcliente',
            Cliente: 'cliente',
            TipoDeParte: 'TipoDeParte',
            TipoDeFactura: 'TipoDeFactura',
            IdTipoDeFactura: 'IdTipoDeFactura',
            IdTipoDeParte: 'IdTipoDeParte',
            linea: {
                orden: 'orden',
                coste: 'coste',
                venta: 'venta',
                cantidad: 'cantidad',
                clase: 'Clase',
                idnaturaleza: 'IdNaturaleza',
                idunidad: 'IdUnidad',
                naturaleza: 'Naturaleza',
                unidad: 'Unidad',
                unitario: 'unitario',
                selectorDeIvaR: 'IvaRepercutido',
                ImporteSinDto: 'ImporteSinDto',
                ImporteDeDto: 'ImporteDeDto',
                ImporteDeIva: 'ImporteDeIva',
                ImporteDeLinea: 'ImporteDeLinea',
                descuentoPorLinea: 'descuento',
                tipoDeLinea: 'tipoDeLinea',
                concepto: 'concepto',
                iva: 'iva',
            },
            ConOSinContrato: 'filtroporconosincontrato',
            ConOSinPlanificador: 'filtroporconosinplanificador'
        },
        ParteTr: {
            id: literal.id,
            ivaPropuesto: 'iva',
            descuentoPropuesto: 'descuento',
            idExpediente: 'idexpediente',
            IdCliente: 'IdCliente',
            Cliente: 'Cliente',
            Etapa: 'Etapa',
            IdPresupuesto: 'idpresupuesto',
            linea: {
                orden: 'orden',
                tipoDeLinea: 'TipoDeLinea',
                concepto: 'concepto',
                unitario: 'unitario',
                anotacion: 'anotacion',
                ivaPorLinea: 'iva',
                descuentoPorLinea: 'descuento',
                ImporteSinDto: 'ImporteSinDto',
                ImporteDeDto: 'ImporteDeDto',
                ImporteDeIva: 'ImporteDeIva',
                ImporteDeLinea: 'ImporteDeLinea',
                selectorDeIvaR: 'IvaRepercutido',
                precio: 'precio',
                cantidad: 'cantidad',
                clase: 'Clase',
                naturaleza: 'Naturaleza',
                unidad: 'Unidad',
                iva: 'iva',
            },
            Asignacion: {
                Duracion: 'duracion',
                MedidoEn: 'medidoen',
                PlfDeInicio: 'plfdeinicio',
                Iniciada: 'iniciada',
                PlfDeFin: 'plfdefin',
                Finalizada: 'finalizada'
            },
            ConOSinContrato: 'filtroporconosincontrato',
            ConOSinTarea: 'filtroporconosintarea'
        },
        FacturaEmt: {
            id: literal.id,
            ivaPropuesto: 'iva',
            descuentoPropuesto: 'descuento',
            idExpediente: 'idexpediente',
            Presupuesto: 'presupuesto',
            IdCliente: 'IdCliente',
            Cliente: 'Cliente',
            IdContrato: 'IdContrato',
            Contrato: 'Contrato',
            IdPresupuesto: 'idpresupuesto',
            ConOSinContrato: 'filtroporconosincontrato',
            TotalSinIva: 'totalsiniva',
            APagar: 'APagar',
            TotalIrpf: 'totalirpf',
            VenceEl: 'VenceEl',
            Etapas: 'Etapas',
            EsRectificativa: 'EsRectificativa',
            EsPeriodica: 'EsPeriodica',
            ConIrpf: 'ConIrpf',
            ClaseRectificativa: 'ClaseRectificativa',
            enumClaseRectificativa: 'enumClaseRectificativa',
            MotivoDeRectificacion: 'MotivoDeRectificacion',
            IdRectificativa: 'idRectificativa',
            Contacto: 'contacto',
            Telefono: 'telefono',
            eMail: 'email',
            IdParteTr: 'IdParteTr',
            UsaCentroAdministrativo: 'UsaCentroAdministrativo',
            CentroAdministrativo: 'CentroAdministrativo',
            IdCentroAdministrativo: 'IdCentroAdministrativo',
            EstaComunicandose: 'EstaComunicandose',
            linea: {
                orden: 'orden',
                tipoDeLinea: 'TipoDeLinea',
                concepto: 'concepto',
                unitario: 'unitario',
                anotacion: 'anotacion',
                ivaPorLinea: 'iva',
                descuentoPorLinea: 'descuento',
                ImporteSinDto: 'ImporteSinDto',
                ImporteDeDto: 'ImporteDeDto',
                ImporteDeIva: 'ImporteDeIva',
                ImporteDeLinea: 'ImporteDeLinea',
                selectorDeIvaR: 'IvaRepercutido',
                precio: 'precio',
                cantidad: 'cantidad',
                clase: 'Clase',
                naturaleza: 'Naturaleza',
                unidad: 'Unidad',
                iva: 'iva'
            },
            Irpf: {
                BiSujeta: 'BiSujeta',
                TipoIrpf: 'TipoIrpf',
                Irpf: 'Irpf',
                Importe: 'Importe'
            },
            Verifactu: {
                Url: 'url',
            },
            PorAbonar: 'PorAbonar',
            Cobro: {
                Cobrado: 'cobrado',
                Pendiente: 'pendiente',
                Clase: 'clase',
                CobradoEl: 'cobradoel',
                CuentaDeIngreso: 'CuentaDeIngreso',
                CuentaDeCargo: 'CuentaDeCargo'
            },
            Abono: {
                Abonado: 'cobrado', //usamos el mismo campo del json
                Importe: 'importe',
                Pendiente: 'pendiente',
                Cliente: 'cliente',
                IdCliente: 'idcliente',
                Abono: 'abono',
                IdAbono: 'idabono',
                Clase: 'clase',
                AbonadoEl: 'abonadoel',
                CuentaDeIngreso: 'CuentaDeCargo',
                CuentaDeAbono: 'CuentaDeAbono'
            },
            Indicadores: {
                UsaVerifactu: 'UsaVerifactu',
            }
        },
        PeticionDeFacturaEmt: {
            IdFactura: 'idfactura'
        },
        RemesaFae: {
            Acreedor: 'Acreedor',
            Presentador: 'Presentador',
            NifDelPresentador: 'NifDelPresentador',
            CuentaDeAbono: 'CuentaDeAbono',
            IdCuentaDeAbono: 'IdCuentaDeAbono',
            Entidad: 'Entidad',
            Oficina: 'Oficina',
            Etapas: 'Etapas',
            CargarEl: 'cargarel',
            CargadaEl: 'cargadael'
        },
        FacturasEmtDeUnaRemesa: {
            Etapas: 'Etapas',
            Factura: 'Factura',
            CargadaEl: 'cargadael',
            DevueltoEl: 'devueltoel',
            Motivo: 'motivo',
            EstaCargada: 'estacargada'
        }
    },
    Gasto: {
        Pago: {
            Nombre: literal.nombre,
            Clase: 'clase',
            Modo: 'ModoDePago',
            CuentaDePago: 'CuentaDePago',
            TarjetaDePago: 'TarjetaDePago',
            Iban: 'iban',
            Entidad: 'entidad',
            Oficina: 'oficina',
            DcCcc: 'dcccc',
            Numero: 'numero',
            Alias: 'alias',
            Acreedor: 'solicitante',
            IdProveedor: 'idproveedor',
            Proveedor: 'proveedor',
            IdTrabajador: 'idtrabajador',
            Trabajador: 'trabajador',
            BancoAcreedor: 'BancoAcreedor',
            BancoDePago: 'BancoDePago',
            Etapas: 'Etapas',
            PagarEl: 'pagarel',
            PagadoEl: 'pagadoEl',
            Importe: 'importe',
            IdFacturaRec: 'IdFacturaRec',
            FacturaRec: 'FacturaRec',
            IdNaturaleza: 'IdNaturaleza',
            Naturaleza: 'Naturaleza',
            IdPreasiento: 'IdPreasiento',
            EsAbono: 'EsAbono',
            AbonadoEl: 'AbonadoEl',
            IdCliente: 'IdCliente',
            IdFacturaEmt: 'IdFacturaEmt'
        },
        RemesaPag: {
            Deudor: 'Deudor',
            Presentador: 'Presentador',
            NifDelPresentador: 'NifDelPresentador',
            CuentaDePago: 'CuentaDePago',
            IdCuentaDePago: 'IdCuentaDePago',
            Entidad: 'Entidad',
            Oficina: 'Oficina',
            Etapas: 'Etapas',
            PagarEl: 'pagarEl',
            PagadoEl: 'pagadoEl'
        },
        FacturaRec: {
            Nombre: literal.nombre,
            Etapas: 'Etapas',
            BICalculada: 'BICalculada',
            Numero: 'Numero',
            BaseImponible: 'baseimponible',
            FacturadaEl: 'FacturadaEl',
            RecibidaEl: 'RecibidaEl',
            SelectorNaturaleza: 'Naturaleza',
            SelectorUnidad: 'Unidad',
            SelectorIva: 'IvaSoportado',
            SelectorIrpf: 'IrpfAplicado',
            IrpfDeLinea: 'Irpf',
            IdFacturaRec: 'IdFacturaRec',
            IdArchivo: 'IdArchivo',
            Proveedor: 'Proveedor',
            IdProveedor: 'IdProveedor',
            Expediente: 'Expediente',
            IdExpediente: 'IdExpediente',
            Interlocutor: 'Interlocutor',
            IdInterlocutor: 'IdInterlocutor',
            TotalDelPago: 'totaldelPago',
            TotalPagado: 'TotalPagado',
            TotalPagosEnCurso: 'TotalPagosEnCurso',
            TotalRectificado: 'TotalRectificado',
            TotalDevuelto: 'TotalDevuelto',
            VenceEl: 'venceel',
            ContabilizadaEl: 'ContabilizadaEl',
            IdContrato: 'IdContrato',
            Contrato: 'Contrato',
            IdRectificada: 'IdRectificada',
            Rectificada: 'Rectificada',
            EsIncorporada: 'esincorporada',
            Pagada: 'Pagada',
            ModoDePago: 'ModoDePago',
            Tarjeta: 'Tarjeta',
            DomiciliadaEn: 'DomiciliadaEn',
            IdTarjeta: 'IdTarjeta',
            IdDomiciliadaEn: 'IdDomiciliadaEn',
            Indicadores: {
                Naturaleza: 'Naturaleza',
                UnidadDeMedida: 'UnidadDeMedida',
                ComoTratarLaFechaDeRecepcion: 'ComoTratarLaFechaDeRecepcion'
            },
            linea: {
                orden: 'orden',
                Clase: 'Clase',
                concepto: 'concepto',
                selectorIva: 'IvaSoportado',
                Anotacion: 'anotacion',
                Cantidad: 'cantidad',
                Unidad: 'unidad',
                IdIvaS: 'IdIvaS',
                PorcentajeIva: 'PorcentajeIva',
                selectorNaturaleza: 'Naturaleza',
                selectorIrpf: 'Irpf',
                IdIrpf: 'IdIrpf',
                PorcentajeIrpf: 'PorcentajeIrpf',
                BaseImponible: 'BaseImponible',
                ImporteDeIva: 'ImporteDeIva',
                ImporteDeIrpf: 'ImporteDeIrpf'
            },
        },
        RenombraFar: {
            IdElemento: 'idElemento',
            Nombre: literal.nombre
        },
        ImportarFar: {
            Tipo: 'TipoFarPropuesto',
            Cg: 'CgPropuesto'
        },
        PagosDeUnaRemesa: {
            Etapas: 'Etapas',
            Pago: 'Pago',
            PagadoEl: 'pagadoel',
            PagarEl: 'pagarel',
            AnuladoEl: 'anuladoel',
            Motivo: 'motivo',
            EstaPagado: 'EstaPagado',
            EstaAnulado: 'EstaAnulado'
        }
    },
};

const ltrDatosPropuestos = {
    CGsAccesibles: 'CGsAccesibles',
    CGPropuesto: 'CGPropuesto',
    TiposAccesibles: 'TiposAccesibles',
    TipoPropuesto: 'TipoPropuesto',
    Nombre: literal.nombre,
    Descripcion: ltrPropiedades.Elemento.Descripcion,
    Otros: 'Otros',
    Plantillas: 'Plantillas'
}

const ltrValores = {
    Crud: {
        Edicion: {
            diasDeConsulta: 'dias-para-consultar-por-gid'
        }
    },
    Elemento: {
        Historial: {
            MostrarObservacion: 'MostrarObservacion',
            MostrarEvento: 'MostrarEvento',
            MostrarCorreo: 'MostrarCorreo',
            AbrirArchivador: 'AbrirArchivador',
            AbrirTarea: 'AbrirTarea',
            AbrirPpt: 'AbrirPpt',
            Descargar: 'Descargar'
        },
    },
    Terceros: {
        Cliente: {
            ClaseDeCuenta: {
                Pago: 'Pago'
            }
        },
        Proveedor: {
            ClaseDeCuenta: {
                Ingreso: 'Ingreso'
            }
        },
        Interlocutor: {
            ClaseDeCuenta: {
                Ingreso: 'Ingreso'
            }
        },
        Trabajador: {
            ClaseDeCuenta: {
                Ingreso: 'Ingreso'
            }
        },
        Sociedad: {
            CuentaBancaria: {
                Clase: {
                    Ambas: 'Ambas'
                }
            }
        }
    },
    Venta: {
        FacturasEmt: {
            Cobro: {
                Clase: {
                    Contado: 'Contado',
                    Transferencia: 'Transferencia',
                    CartaDePago: 'CartaDePago',
                    Remesa: 'Remesa',
                }
            },
            Abono: {
                Clase: {
                    Contado: 'Contado',
                    Transferencia: 'Transferencia',
                    Remesa: 'Remesa',
                }
            },
            Rectificativas: { Clase: { Total: 'OR', Complementaria: 'OC' } }
        }
    },
    Gasto: {
        Pago: {
            Clase: {
                Contado: 'Contado',
                Transferencia: 'Transferencia',
                Remesa: 'Remesa',
            },
            ModoDePago: {
                Contado: enumModoDePagoContado.Contado,
                Tarjeta: enumModoDePagoContado.Tarjeta,
                Domiciliacion: enumModoDePagoContado.Domiciliacion
            }
        },
        Factura: {
            ComoTratarLaFechaDeRecepcion: {
                MismaFecha: 'mismafecha',
                FechaDeHoy: 'fechadehoy',
                FechaDeHoySi15: 'fechadehoy15',
                FechaDeHoySi30: 'fechadehoy30'
            }
        }
    }
};

const ltrClaveDeEstado = {
    historial: 'historial',
    restrictoresDeUnPost: 'restrictoresDeUnPost',
    restrictoresUrl: 'restrictores',
    idSeleccionado: 'idSeleccionado',
    paginaDestino: 'pagina-destino',
    paginaActual: 'pagina-actual',
    guid: 'guid',
    paginaOrigen: 'pagina-origen',
    urlActual: 'url-actual',
    //soloMapearEnElFiltro: 'solo-mapear-en-el-filtro',
    filtrosUrl: 'filtros',
    filtrosDeUnPost: 'filtrosDeUnPost',
    paraqueNavegar: 'paraque-navegar',
    EditarAlVolver: 'EditarAlVolver',
    ElementosSeleccionados: 'elementos_seleccionados',
    SeguirRetrocediendo: 'SeguirRetrocediendo'
};

const ltrParametrosUrl = {
    origenDePeticion: 'origen',
    id: 'id',
    restrictores: ltrClaveDeEstado.restrictoresUrl,
    filtros: ltrClaveDeEstado.filtrosUrl,
    idSociedad: 'idSociedad',
    idPersona: 'IdPersona',
    idInterlocutor: 'IdInterlocutor',
    idEstado: 'transiciondtm',
    idAgenda: 'idAgenda',
    valorGuid: '0',
    fecha: 'fecha',
    guid: 'guid',
    SisDoc: {
        IdArchivador: 'idarchivador'
    },
    Juridico: {
        IdPlanificador: 'idplanificador',
        IdContrato: 'idcontrato'
    },
    Administracion: {
        IdParteTr: 'idparteTr',
        idExpediente: 'idexpediente',
    },
    Venta: {
        IdTarea: 'idtarea',
        IdParteTr: 'idpartetr',
        IdFactura: 'idfacturaemt',
        IdPresupuesto: 'idpresupuesto',
        IdPlfDeVenta: 'idplfdeventa',
        FiltroConOSinContrato: 'filtroporconosincontrato',
        AsociadaAUnContrato: 'asociadaauncontrato',
        IndicePorContrato: '1',
    },
    Gasto: {
        IdFactura: 'idfacturarec'
    }
};

const enumOrigenDeNavegacion = {
    menu: 'menu',
    dependencia: 'dependencia',
    relacion: 'relacion',
    noDefinido: 'undefined'
};

const GestorDeEventos = {
    deSeleccionDeFiltro: 'Crud.EventosModalDeSeleccion',
    deCrearRelaciones: 'Crud.EventosModalDeCrearRelaciones',
    paraImputar: 'Crud.EventosModalParaImputar',
    deConsultaDeRelaciones: 'Crud.EventosModalDeConsultaDeRelaciones',
    delMantenimiento: 'Crud.EventosDelMantenimiento',
    deListaDinamica: 'Crud.EventosDeListaDinamica',
    paraSeleccionarElementos: 'Crud.EventosModalParaSeleccionar',
    paraTransitar: 'Crud.EventosModalDeTransitar',
    paraImprimir: 'Crud.EventosModalDeImprimir'
};

const ltrNegocioSe = {
    Nombre: {
        Sociedades: 'Sociedades',
        Carpetas: 'Carpetas',
        Archivadores: 'Archivadores',
        Registros: 'Registros',
        Juridico: {
            Contrato: 'Contratos'
        },
        Venta: {
            Presupuesto: 'Presupuestos'
        }
    },
    Enumerado: {
        NoDefinido: 'No_Definido',
        Juridico: {
            Contrato: 'Contrato'
        },
        Venta: {
            Presupuesto: 'Presupuesto'
        }
    }
};


const ltrMenus = {
    menu: {
        individual: 'menu.individual',
        edicion: 'menu.edicion',
        creacion: 'menu.creacion',
        historial: 'menu.historial',
        contextual: 'menu.contextual',
        relacion: 'menu.de.relaciones',
        filtro: 'menu.de.filtro',
        formulario: 'menu.formulario'
    },
    PanelDeControl: {
        UltimosMenu: 'ultimos-accesos-menu-1'
    },
    Texto: {
        borrarPlantilla: 'Remplazar o borrar plantilla'
    },
    aQuienAfectaLaOpicionDeMenu: 'origen',
    accion: 'accion-menu',
    opcion: 'opcion',
    esContextual: 'escontextual',
    enumOrigen: {
        crud: 'crud',
        edicion: 'edicion',
        creacion: 'creacion',
        formulario: 'formulario'
    },
    eventosDeMf: {
        AbrirEnviarCorreo: 'abrir-correo',
        AbrirTransitar: 'abrir-transitar',
        ModalDeExportar: 'abrir-exportar',
        ModalDeCrearObservacion: 'crear-observacion',
        ModalDeCrearArchivador: 'crear-archivador',
        ModalDeCrearDirecciones: 'crear-direccion',
        alta: ltrEventos.ModalEdicion.DarDeAlta,
        baja: ltrEventos.ModalEdicion.DarDeBaja,
        Comun: {
            PermisosDeElemento: 'permiso-de-elemento',
            Imprimir: 'abrir-imprimir',
            GuardarEstadosDeExpansores: 'guardar-estados-de-expansores',
            GuardarDisposicionDeArchivos: 'guardar-disposicion-de-archivos',
            LeerDisposicionDeArchivos: 'leer-disposicion-de-archivos',
            CantidadALeeer: 'cantidad-a-leer',
            TamanoDelVisor: ltrPropiedades.Entorno.Vista.Indicadores.TamanoDelVisor,
            TamanoDeGraficos: ltrPropiedades.Entorno.Vista.Indicadores.TamanoDeGraficos,
            MostrarVisorAlIniciar: ltrPropiedades.Entorno.Vista.Indicadores.MostrarVisorAlIniciar,
            DisposicionDelEncolumnado: ltrPropiedades.Negocio.PlantillaDeFiltrado.DisposicionDelEncolumnado,
            OrdenacionDelResultado: ltrPropiedades.Negocio.PlantillaDeFiltrado.OrdenacionDelResultado,
            TamanoDelEncolumnado: ltrPropiedades.Negocio.PlantillaDeFiltrado.TamanoDelEncolumnado,
            GuardarColumnasDelGrid: ltrPropiedades.Negocio.PlantillaDeFiltrado.GuardarColumnasDelGrid,
            EliminarColumnasDelGrid: 'eliminar-columnas-del-grid',
            GuardarDatosCreacion: 'guardar-datos-creacion',
            Totalizador_Mostrar: 'mostrar-totales'
        },
        Parametrizacion: {
            Negocio: {
                GuardarPlantillaCreacion: 'guardar-plantillas-creacion',
                EliminarPlantillaCreacion: 'eliminar-plantillas-creacion',
                Comun_GuardarPlantillaFiltrado: 'guardar-plantillas-filtrado',
                Comun_EliminarPlantillaFiltrado: 'eliminar-plantillas-filtrado',
                Comun_OcultarColumnas: 'ocultar-columnas',
                Comun_LeerDatosParaExportacion: 'leer-datos-para-exportacion'
            },
            Estados: {
                Transiciones: 'transiciones'
            },
        },
        Entorno: {
            Trabajo: {
                ejecutar: 'ejecutar',
                bloquear: 'bloquear',
                desbloquear: 'desbloquear',
                resometer: 'resometer'
            },
            Correo: {
                EnviarCorreo: 'Correo_Enviar'
            },
            Agenda: {
                AbrirAgenda: 'abrir-agenda',
                ModalDeCrearEvento: 'crear-evento'
            },
            Usuario: {
                MiCertificado: 'mi-certificado',
                MiCorreo: 'mi-correo'
            },
            MiCorreo: {
                Archivar: 'micorreo_creararchivador',
                Tarea: 'micorreo_creartarea',
                RegistroEs: 'micorreo_crearregistroes',
                FacturaRec: 'micorreo_crearfacturarec',
                Expediente: 'micorreo_crearexpediente',
                ComoArchivar: 'micorreo_comoarchivar',
                ComoVincular: 'micorreo_comovincular'

            }
        },
        Maestros: {
            Terceros: {
                Interlocutores: 'interlocutores',
                Proveedores: 'ir-a-proveedor',
                Clientes: 'ir-a-cliente',
                Trabajadores: 'ir-a-trabajador',
                Procuradores: 'procuradores',
                Abogados: 'abogados',
                CentrosGestores: 'cgs',
                CuentasBancarias: 'cta-bancarias',
                TarjetasBancarias: 'tar-bancarias',
                Facturador: 'facturador-sociedad',
                Buzones: 'buzones',
                AsociarCertificado: 'asociar-certificado',
                CrearLote: 'lote-terceros',
                ActivarVerifactu: 'activar-verifactu',
                RecomponerBlockChain: 'recomponer-blockchain',
                CatalogosJudiciales: 'catalogos-judiciales'
            },
            Clientes: {
                CentroAdministrativo: 'nuevo-centro-adm',
                PuestoDeTrabajo: 'nuevo-puesto-trabajo',
                NuevoClienteWeb: 'nuevo-cliente-web',
                AsociarClienteWeb: 'vincular-cliente-web'
            }
        },
        SisDoc: {
            Archivadores: {
                IrACarpetas: 'ir-a-carpetas',
                Arc_ImportarZip: 'importar-zip',
                Arc_ProcesarFarConIa: 'procesar-far-con-ia',
                Arc_Descontabilizar: 'descontabilizar-spr',
                Arc_Exportar: 'exportar-archivador',
                Arc_Copiar: 'copiar-archivador'
            }
        },
        Administracion: {
            RegistroEs: {
                CrearTareas: 'crear-tarea',
            },
            Tareas: {
                IrAPartesTr: 'ir-a-partes-trabajo',
                CopiarTarea: 'copiar-tarea',
            },
            Expedientes: {
                VincularRegistroEntrada: 'vincular-re',
                IrATareas: 'ir-a-tareas',
                ImputarFacturas: 'imputar-facturas',
                IrAFacturasRec: 'ir-a-facturas-recibidas',
            }
        },
        Contabilidad: {
            Preasientos: {
                CrearLoteContable: 'crear-lote',
                AnularLoteContable: 'anular-lote',
                RegenerarLoteContable: 'regenerar-lote',
                AnularEstimacionDirecta: 'anular-estimacion',
                LoteTerceros: 'lote-terceros',
                ContabilizarPreasiento: 'contabilizar-preasiento',
                RegenerarPreasiento: 'regenerar-preasiento'
            }
        },
        Guarderias: {
            Infante: {
                AsociarCurso: 'asociar-curso'
            }
        },
        Juridico: {
            Contratos: {
                VincularRegistroEntrada: 'vincular-re',
                IrAPlfDeVentas: 'ir-a-planificaciones-venta',
                IrAPartesTr: 'ir-a-partes-trabajo',
                IrAFacturasEmt: 'ir-a-facturas-emitidas',
                IrAFacturasRec: 'ir-a-facturas-recibidas',
                GenerarLosPlanificadores: 'generar-planificadores',
                PrepararPartesDeTrabajo: 'preparar-partes-de-trabajo',
                EmitirPrefacturasPorParteTr: 'emitir-prefacturas-por-partetr',
                EmitirPrefacturasPorContrato: 'emitir-prefacturas-por-contrato',
                ImputarFacturas: 'imputar-facturas',
            },
            Planificador: {
                IrAPlfDeVentas: 'ir-a-planificaciones-venta',
                GenerarPlanificaciones: 'generar-planificaciones-venta'
            },
            Pleitos: {
                VincularRegistroEntrada: 'vincular-re',
            }
        },
        Gasto: {
            FacturasRec: {
                IrAContratos: 'ir-a-contratos',
                IrAExpedientes: 'ir-a-expedientes',
                IrAPagos: 'ir-a-pagos',
                IrAFacturasRec: 'ir-a-facturasRec',
                QuitarContrato: 'quitar-contrato',
                QuitarExpediente: 'quitar-expediente',
                Far_ImportarFarXml: 'importar-efactura',
                Far_ImportarPrvXml: 'importar-proveedor',
                Far_CrearFarConIa: 'crear-far-ia',
                CopiarFar: 'copiar-far',
                RectificarFar: 'rectificar-far',
                Renombrar: 'renombrar-far',
                CambiarProveedor: 'cambiar-proveedor',
                GenerarPreasieto: 'generar-preasiento',
                CancelarPreasientos: 'cancelar-preasientos'
            },
            RemesasPag: {
                Imprimir: 'abrir-imprimir',
                DarPorPagado: 'pagar-remesa',
                RetrocederPago: 'retroceder-pago'
            },
            Pagos: {
                GenerarPreasieto: 'generar-preasiento',
                CancelarPreasientos: 'cancelar-preasientos'
            }

        },
        Venta: {
            FacturasEmt: {
                IrAPartesTr: 'ir-a-partes-trabajo',
                IrAPpts: 'ir-a-ppts',
                IrAContratos: 'ir-a-contratos',
                IrAFacturasEmt: 'ir-a-facturasEmt',
                ImprimirPrefacturas: 'abrir-imprimir',
                CambiarVencimiento: 'cambiar-vencimiento',
                CopiarFae: 'copiar-fae',
                CopiarLa: 'copiar-la',
                SincronizarConLaAeat: 'sicronizar-con-la-aeat',
                FacturarTareas: 'facturar-tareas',
                Rectificativa: 'rectificativa',
                GenerarPreasieto: 'generar-preasiento',
                GenerarUbl: 'generar-ubl',
                CambiarDatos: 'cambiar-datos-fae'
            },
            RemesasFae: {
                Imprimir: 'abrir-imprimir',
                Cargar: 'cargar-remesa',
                AnularCargo: 'anular-cargo-remesa'
            },
            Partes: {
                IrAPlanificaciones: 'ir-a-planificaciones-venta',
                IrAPpts: 'ir-a-ppts',
                IrAContratos: 'ir-a-contratos',
                IrAFacturasEmt: 'ir-a-facturas-emitidas',
                DarPorRealizadasSegunPlan: 'dar-por-realizada-plan',
                DarPorRealizadasHoy: 'dar-por-realizada-hoy',
                SolicitarFechaDeEjecucion: "solicitar-fechas-ejecucion"
            },
            Presupuestos: {
                AsociarUnExpediente: 'vincular-expediente',
                Renombrar: 'renombrar-ppt',
                CopiarPpt: 'copiar-ppt',
                IrAPartesTr: 'ir-a-partes-trabajo',
                IrATareas: 'ir-a-tareas',
                IrAFacturasEmt: 'ir-a-facturas-emitidas',
                IrAContratos: 'ir-a-contratos'
            },
            Planificaciones: {
                IrAPartesTr: 'ir-a-partes-trabajo',
                IrAFacturasEmt: 'ir-a-facturas-emitidas'
            }
        },
    },
    IdsDeOpcinesMf: {
        idAlta: 'menu.edicion.dar-de-alta',
        idBaja: 'menu.edicion.dar-de-baja',
        idObservacion: 'menu.edicion.observaciones',
        idImprimir: 'menu.edicion.imprimir',
        idPermisos: 'menu.edicion.permisos',
        idArchivador: 'menu.edicion.archivadores',
        idTransitar: 'menu.edicion.transitar',
    },
    OpcinesMf: {
        enviarMail: 'abrir-correo'
    },
    Formulario: {
        alta: 'menu.opcion.alta',
        baja: 'menu.opcion.baja'
    },
    BarraDeMenu: {
        Nuevo: 'Nuevo',
        Editar: 'Editar',
        Consultar: 'Consultar',
        Borrar: 'Borrar',
        Eliminar: 'Eliminar',
        Copiar: 'Nuevo',
        Reactivar: 'Reactivar',
        Crear: 'Crear',
        Ventas: {
            RemesasFae: {
                Facturas: 'Facturas'
            },
            FacturasDeUnaRemesa: {
                IncluirFacturas: 'Añadir'
            }
        },
        Gastos: {
            RemesasPag: {
                Pagos: 'Pagos'
            },
            PagosDeUnaRemesa: {
                IncluirPagos: 'Añadir'
            }
        },
        Guarderia: {
            InfantesDeUnCurso: {
                IncluirInfantes: 'Añadir'
            }
        },
        Terceros: {
            Interlocutor: {
                CrearSociedad: 'cre-soc',
                CrearPersona: 'cre-per',
                CrearContacto: 'cre-ctc'
            },
            Cliente: {
                CrearSociedad: 'cre-soc',
                CrearPersona: 'cre-per'
            }
        }
    },
    MenuDeFiltrado: {
        NumeroDeOpciones: '3'
    }
};

const ltrCrud = {
    Acciones: {
        TrasModificar: "TrasModificar"
    },
    Enumerados: {
        Edicion: {
            Carga: {
                Principal: "DatosDto",
                Ampliaciones: "Ampliaciones",
                Detalles: "Detalles"
            }
        }
    }
}


const ltrHtml = {
    Divs: {
        PreguntasIa: `
            <div class='${ltrCss.crud.modal.ContenedorDeHistorialIa}' id="seccion-historial-ia">
                <p>Consultas recientes:</p>
                <div id='contenedor-historial-ia'>
                    [PreguntasIa]
                </div>
            </div>`
    },
    Modales: {
        PreguntasIa: `
                 <div style="min-width: 500px; max-width: 600px;" id="modal-ia-container">
                     <h2 style="margin-top: 0;">Asistente de Filtrado</h2>
                     
                     <div id="vista-pregunta-ia">
                         <div class="${ltrCss.crud.modal.cuerpo}">
                             [htmlHistorial]
                             <p>¿Qué estás buscando? Cuéntamelo y generaré los filtros por ti:</p>
                             <textarea id="input-pregunta-ia" placeholder="Indícame nombre, tipo...">[pregunta]</textarea>
                             <label style="display: flex; align-items: center; gap: 6px; margin-top: 8px; cursor: pointer;">
                             <input type="checkbox" id="chk-nueva-pregunta" [nuevaconservacion] />
                                  Nueva conversación
                             </label>
                         </div>
                         <div class="${ltrCss.crud.modal.pie}" style="display: flex; gap: 10px; margin-top: 15px;">
                             <button class="${ltrCss.crud.modal.boton}" id="btn-ejecutar-pregunta" style="background-color: #28a745; color: white;">Preguntar</button>
                             <button class="${ltrCss.crud.modal.boton}" id="btn-cerrar-dinamico">Cancelar</button>
                             <button class="${ltrCss.crud.modal.boton}" id="btn-ver-respuesta-json" style="margin-left: auto; background-color: #007bff; color: white;">Respuesta</button>
                         </div>
                     </div>
                
                     <div id="vista-edicion-json" style="display: none;">
                         <div class="${ltrCss.crud.modal.cuerpo}">
                             <p>Configuración técnica del filtro (JSON):</p>
                             <textarea id="textarea-json-ia" style="width: 100%; height: 250px; padding: 10px; border-radius: 4px; border: 1px solid #ccc; font-family: monospace; font-size: 12px; resize: vertical;"></textarea>
                         </div>
                         <div class="${ltrCss.crud.modal.pie}" style="display: flex; gap: 10px; margin-top: 15px;">
                             <button class="${ltrCss.crud.modal.boton}" id="btn-grabar-json" style="background-color: #28a745; color: white;">Grabar</button>
                             <button class="${ltrCss.crud.modal.boton}" id="btn-volver-pregunta">Cerrar</button>
                         </div>
                     </div>
                 </div>`
    }
};


