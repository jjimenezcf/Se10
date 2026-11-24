// Nombres de los eventos que se pasan a los gestores de eventos, y nombres de esos gestores.
// Depende de literales_Atributos.
// Orden de carga: ListaDeScripts.cs. Al definirse, un literal solo puede leer los de ficheros anteriores de esa lista.

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
        AplicarFiltro: 'aplicar-filtro',
        LimpiarFiltro: 'limpiar-filtro'
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
        Cerrar: 'cerrar-enviar-correo'
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
        OrdenarPor: atControl.ordenarPor
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
        AlternarVistaFichas: 'alternar-vista-de-fichas',
        OrdenarPor: atControl.ordenarPor,
        FilaPulsada: 'fila-pulsada',
        CambiarSelector: 'cambiar-selector',
        OcultarMostrarFiltro: 'ocultar-mostrar-filtro',
        OcultarMostrarBloque: 'ocultar-mostrar-bloque',
        OcultarMostrarAmpliacion: 'ocultar-mostrar-ampliacion',
        OcultarMostrarDetalle: 'ocultar-mostrar-detalle',
        MostrarOcultarVisorDeDetalle: 'mostrar-ocultar-visor-detalle',
        MostrarModalConDireccion: 'mostrar-modal-con-direccion',
        TeclaPulsada: 'tecla-pulsada',
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
        PlegarTodo: 'plegar-todo',
        DesplegarTodo: 'desplegar-todo',
        MostrarOcultarVisor: 'mostrar-ocultar-visor',
        SiguienteArchivo: 'siguiente-archivo',
        AnteriorArchivo: 'anterior-archivo',
        Retroceder: 'retroceder',
        Avanzar: 'avanzar',
        DescargarArchivo: 'descargar-archivo',
        CompartirConWhatsApp: 'compartir-con-whatsapp',
        ConvertirAPdf: ltrCss.crud.panelDeEdicion.ConvertirAPdf,
        EditarTexto: ltrCss.crud.panelDeEdicion.EditarTexto,
        CompartirConGuid: 'compartir-con-guid',
        CompartirElemento: 'compartir-elemento',
        ConsultarConGuid: 'consultar-con-guid',
        ResumirArchivo: 'resumir-archivo',
        PasarOcr: ltrCss.crud.panelDeEdicion.PasarOcr,
        FacturasRec: {
            Analizar: 'analizar-factura',
        }
    },
    Historial: {
        Cerrar: 'cerrar-historial',
        OcultarMostrarFiltro: 'ocultar-mostrar-filtro',
        CargarHistorial: 'cargar-historial',
        AplicarOrdenInicial: 'aplicar-orden-inicial',
        AnularOrdenacion: 'anular-ordenacion'
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
        }
    }
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
