// Atributos html de los controles (at*).
// Depende de literales_Basicos.
// Orden de carga: ListaDeScripts.cs. Al definirse, un literal solo puede leer los de ficheros anteriores de esa lista.

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
    MostraEnMovil: 'mostrar-en-movil',
    EtiquetaEnMovil: 'etiqueta-en-movil',
    formato: 'formato',
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
    vista: literal.vista,
    vistaMvc: 'vistamvc',
    obligatorio: 'obligatorio',
    SoloParaTs: 'solo-para-ts',
    modoOrdenacion: 'modo-ordenacion',
    ordenInicial: 'orden-inicial',
    expresionElemento: 'expresion-elemento',
    tipo: 'tipo',
    tipoColumna: 'tipo-control',
    tipoModal: 'tipomodal',
    imagenVinculada: 'imagen-vinculada',
    valorInput: 'value',
    valorPorDefecto: 'valor-de-defecto',
    parametrosParaNavegador: 'parametros-para-navegar',
    idDelElemento: 'idDelElemento',
    restrictor: 'restrictor',
    editable: 'editable',
    ContenidoEn: 'contenido-en',
    AplicarJoin: 'aplicar-join',
    cantidadPorLeer: 'cantidad-a-leer',
    conNavegador: 'con-navegador',
    tablaDeControles: 'tabla-de-controles',
    MenuFlotante: 'menu-flotante',
    NoMapeablesAlDto: 'propiedades-no-mapeables',
    PermisosNecesarios: 'permisos-necesarios',
    negocio: literal.negocio,
    transiciones: 'transiciones',
    esAmpliacion: 'es-ampliacion',
    // ampliación calculada que sólo se muestra (ver AmpliacionDeEdicion.SoloConsulta): no se persiste
    ampliacionSoloConsulta: 'solo-consulta',
    esDetalle: 'es-detalle',
    objeto: 'datos-del-objeto',
    TrasMapear: 'tras-mapear',
    soloConsulta: 'solo-consulta',
    esAlmacenable: 'es-almacenable',
    title: 'title',
    NombreDeAccion: 'NombreDeAccion',
    informativa: 'Informativa',
    eventoJs: {
        onclick: 'onclick'
    }
};

const atNombre = {
    contenedorControl: 'contenedor-control',
    tdPropiedad: 'td-propiedad'
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

const atMantenimiento = {
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

};

const atGrid = {
    id: atMantenimiento.gridDelMnt,
    EsFecha: atControl.EsFecha,
    formato: atControl.formato,
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
        pagina: 'pagina',
        leidos: 'leidos',
        posicion: 'posicion',
        total: 'total-en-bd'
    },
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
    idCtrlCantidad: 'nav_2_reg',
    idInfo: 'info',
    idMensaje: 'mensaje',
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
    imagen: atControl.imagenVinculada,
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
    archivosSeleccionados: 'archivos-seleccionados',
    AplicarOperacion: 'aplicar',
    Operacion: {
        Copiar: 'copiar',
        Mover: 'mover',
        Enlazar: 'enlazar',
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
    propiedadBuscar: 'propiedadBuscar',
    criterioBuscar: 'criterioBuscar',
    idEditorMostrar: 'ideditormostrar',
    mostrarPropiedad: atControl.mostrarPropiedad,
    idModal: 'id-modal',
    ListaDeSeleccionados: 'ids-seleccionados',
    selector: 'selector',
    propiedadParaFiltrar: 'propiedadFiltrar'
};

const atSelectorDeElementos = {
    ModalPadre: 'modal-padre',
    EditorAsociado: 'idEditor',
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
    valorFechaHasta: 'valorFechaHasta'
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
    trasSeleccionar: atArchivo.trasSeleccionar,
    trasBlanquear: 'tras-blanquear',
    antesDeBuscar: 'antes-de-buscar',
    RestrictorFijo: 'restrictor-fijo',
    ReadOnly: "readonly"
};

const atListasDeElemento = {
    mostrarExpresion: atListas.mostrarExpresion,
    yaCargado: atListas.yaCargado,
    cargarBajoDemanda: 'cargar-bajo-demanda',
    Cargando: atGrid.cargando,
    expresionPorDefecto: atListas.expresionPorDefecto,
    idNegocio: literal.idNegocio,
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
    // 'S' si la lista de usuarios tiene junto a ella el botón de enviar el elemento por correo (IUPropiedad.PermiteEnviarCorreo)
    enviarCorreoInterno: 'enviar-correo-interno',
    claseElemento: atListas.claseElemento,
    buscarPor: 'como-buscar',
    criterio: atControl.criterio,
    mostrarExpresion: atListas.mostrarExpresion,
    longitudNecesaria: 'longitud',
    idSeleccionado: 'idseleccionado',
    idSelAlEntrar: 'idAlEntrar',
    cargando: atGrid.cargando,
    ultimaCadenaBuscada: 'ultima-busqueda',
    cantidad: atControl.cantidadPorLeer,
    RestringidoPor: atListas.RestringidoPor,
    RestrictorFijo: atListas.RestrictorFijo,
    OtrosClausulasDeFiltrado: 'otros-clausulas-de-filtrado',
    OtrosParametrosDeFiltrado: 'otros-parametros-de-filtrado',
    SoloEnAlta: 'solo-en-alta',
    PropiedadRestrictora: atControl.propiedadRestrictora,
    ContenidoEn: atListas.ContenidoEn,
    BlanquearControlAsociado: 'blanquear-controles-dependientes',
    BlanquearAlSalir: 'blanquear-al-salir',
    AplicarJoin: atControl.AplicarJoin
};

const atListasDinamicasDto = {
    guardarEn: atListas.guardarEn
};

const atRestrictor = {
    mostrarPropiedad: atControl.mostrarPropiedad,
    noRestringido: literal.menos1,
    propiedadRestrictora: atControl.propiedadRestrictora,
    idRestrictor: atControl.restrictor
};

const atGridDeDetalle = {
    controlador: atControl.controlador,
    accionDeConsulta: 'accion-de-consulta',
    conCapa: 'con-capa',
    accionDeLeerPorId: 'accion-de-leer-por-id',
    EsAccion: 'columna-accion',
    Accion: literal.accion,
    restrictor: atControl.restrictor,
    restrictorFijo: atListas.RestrictorFijo,
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
    soloConsulta: atControl.soloConsulta,
    trasModificar: 'tras-modificar',
    NombreNegocio: literal.nombreNegocio,
    referenciaDeFiltrado: 'referencia-de-filtrado',
    filtrosActivos: 'filtros-activos',
    controlador: literal.controlador,
    accion: 'accion-controlador',
};

const atModalParaImputar = {
    faltaRestrictor: 'falta-restrictor',
    filtrarPor: atControl.filtrarPor,
    criterio: 'criterio-por',
    idNegocio: literal.idNegocio
}

const atCriterio = {
    contiene: 'contiene',
    comienza: 'comienza',
    igual: literal.filtro.criterio.igual,
    noEstaRelacionado: 'noEstaRelacionado',
    esAlgunoDe: 'esAlgunoDe',
    sonTodos: 'sonTodos',
    diferente: literal.filtro.criterio.diferente,
};

const atNavegar = {
    navegarAlCrud: 'navegar-al-crud',
    idRestrictor: atControl.restrictor,
    orden: 'orden',
    soloMapearEnElFiltro: 'solo-mapear-en-el-filtro',
    paraqueNavegar: 'paraque-navegar'
};
