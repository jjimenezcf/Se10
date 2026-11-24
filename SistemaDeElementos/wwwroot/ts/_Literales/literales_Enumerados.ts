// Enumerados: valores cerrados que el ts compara o envía (negocios, tipos de modal, formatos, modos de trabajo...).
// No depende de ningún otro fichero de literales.
// Orden de carga: ListaDeScripts.cs. Al definirse, un literal solo puede leer los de ficheros anteriores de esa lista.

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
    Cobro = 'Cobro',
    PlanificacionDeVenta = 'PlanificacionDeVenta',
    PlanificacionDeCompra = 'PlanificacionDeCompra',
    RemesaFae = 'RemesaFae',
    Gasto = 'Gasto',
    Pago = 'Pago',
    RemesaPag = 'RemesaPag',
    FacturaRecibida = 'FacturaRecibida',
    Infante = 'Infante',
    CursoDeGuarderia = 'CursoDeGuarderia',
    Pedido = 'Pedido',
    Preasiento = 'Preasiento',
    Almacen = 'Almacen',
    Regularizacion = 'Regularizacion',
    CierreContable = 'CierreContable',
    Venta = 'Venta',
    DevolucionVenta = 'DevolucionVenta'
}

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
    Numero_4: 'numero.4',
    Porcentaje: 'porcentaje',
    FechaHora: 'yyyy-MM-dd HH:mm:ss',
    ddMMyyyyHHmm: 'dd-MM-yyyy HH:mm',
    base64: 'base64'
}

const enumTipoControl = {
    string: 'string',
    number: 'number'
}

const enumParaQueNavegar = {
    gestionar: 'gestionar',
    crear: 'crear',
    editar: 'editar',
    consultar: 'consultar',
    seleccionar: 'seleccionar',
    gestionarRelaciones: 'gestionar-relaciones',
    gestionarDependencias: 'gestionar-dependencias'
};

const enumCalificadorDireccion = {
    Correspondencia: 'correspondencia',
    Contacto: 'contacto',
    Fiscal: 'fiscal',
    Ejecucion: 'ejecucion',
    Entrega: 'entrega'
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

const enumModoDePresentacion = {
    Grid: 'grid',
    FichasPorEstado: 'fichas-por-estado',
    FichasPorTipo: 'fichas-por-tipo',
    FichasPorPrioridad: 'fichas-por-prioridad',
    FichasPorProveedor: 'fichas-por-proveedor',
    FichasPorCliente: 'fichas-por-cliente'
};

// propiedad por la que se agrupan las tarjetas dentro de la vista de fichas
const ltrModoPresentacion = {
    estado: 'estado',
    tipo: 'tipo',
    prioridad: 'prioridad',
    proveedor: 'proveedor',
    cliente: 'cliente',
};

// valores de enumPrioridad, de más a menos prioritario, tal como llegan en el json
const ltrOrdenDePrioridades = ['Urgente', 'Alta', 'Media', 'Baja', 'NoDefinida'];

enum enumIa {
    IaPerplexity = 'IaPerplexity',
    IaApyHub = 'IaApyhub'
}

enum enumTipoDeLinea {
    Unitario = 'Unitario',
    Alzada = 'Alzada',
    Comentario = 'Comentario'
}

const enumOrigenDeNavegacion = {
    menu: 'menu',
    dependencia: 'dependencia',
    relacion: 'relacion',
    noDefinido: literal.undefined
};
