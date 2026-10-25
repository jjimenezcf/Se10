
using System;
using System.ComponentModel;
using System.IO;

namespace Utilidades
{
    public enum enumTipoDeModal
    {
        ModalDeSeleccion
            , ModalDeRelacion
            , ModalParaImputar
            , ModalDeConsulta
            , ModalParaSeleccionar
            , ModalDeCrearRelacion
            , ModalDeCrearVinculo
            , ModalDeCrearDetalle
            , ModalDeEditarRelacion
            , ModalDeCreacion
            , ModalDeEdicion
            , ModalDeBorrado
            , ModalDeCorreo
            , ModalDeExportacion
            , ModalDeVisorDeArchivos
            , ModalDeFirmaDeArchivos
            , ModalDeDatosDeFirma
            , ModalDeDatosDeBloqueo
            , ModalDeDatosDeDesBloqueo
            , ModalDeSeleccionarDestino
            , ModalDeDesbloqueoMultiple
            , ModalDeBloqueoMultiple
            , ModalDeGenerarZip
            , ModalParaVincular
            , ModalParaPedirDatos
            , ModalParaOcultarColumnas
            , ModalDeTotales
            , ModalMiCertificado
            , ModalCambiarPassword
            , ModalMiUsuario
            , ModalDeFiltrado
            , ModalDeTransitar
            , ModalDeImprimir
            , ModalDeMensaje
    }


    public enum enumFormato
    {
        [Description("")]
        Sin_Formato,
        [Description("porcentaje")]
        Porcentaje,
        [Description("moneda")]
        Moneda,
        [Description("numero.2")]
        Numero_2,
        [Description("numero.6")]
        Numero_6,
        [Description("yyyy-MM-dd")]
        Fecha,
        [Description("yyyy-MM-dd HH:mm:ss")]
        FechaTiempo,
        [Description("yyyy-MM-dd HH:mm")]
        FechaHoraMinutos,
        [Description("dd HH:mm")]
        DiaHoraMinuto,
        [Description("HH:mm:ss")]
        Tiempo,
        [Description("base64")]
        base64,
        [Description("linkHtml")]
        LinkHtml
    }

    public enum enumGestorDeEventos
    {
        EventosModalDeConsultaDeRelaciones
      , EventosModalDeCrearRelaciones
      , EventosModalParaImputar
      , EventosModalDeExportacion
      , EventosModalDeEnviarCorreo
      , EventosDelMantenimiento
      , EventosDeEdicion
      , EventosDelFormulario
      , EventosModalDeSeleccion
      , EventosModalParaSeleccionar
      , EventosDeListaDinamica
      , EventosDeSelectorDeElementosEnModal
      , EventosMenuDelGrid
      , EventosModalDeRelacion
      , EventosDeExpansores
      , EventosDeJerarquia
      , EventosModalDeMensaje
      , EventosModalDeEdicion
      , EventosModalDeConsultaDto
      , EventosModalDeTransitar
      , EventosModalDeImprimir
      , EventosDeSociedad
      , EventosDeAgenda
      , EventosModalDeFiltrado
      , EventosModalDePedirDatos
      , EventosModalDeOcultarColumnas
      , EventosModalDeTotales
      , EventosModalDeMostrarMensaje
      , EjecutarMenuHistorial
    }

    public enum enumFunctionTs
    {
        DefinirLink,
        ProponerFechaEn,
        ProponerHoraEn,
        EsDelSistema_Change,
        InicializarModalDeEdicion,
        InicializarModalDeCreacion,
        InicializarModalParaCrearMinuta,

        BlanquearPasswordDelCertificado,
        BlanquearPassword,
        FirmarArchivo,
        AnularFirma,
        HabilitarDescargaConGuid,
        SubirMiCertificado,
        CambiarPassword,
        CambiarDatosUsuario,
        InicializarModalMiCertificado,
        InicializarModalCambiarPassword,
        InicializarModalMiUsuario,

        SeleccionarIa,
        OpcionSeleccionada,
        AbrirVista,
        MostarOcultarAbout,
        DescargarDeclaracionResponsable,
        Logout,
        ResetearDashBoard,
        MostarMisFiltros,

        MiCorreo_ComoArchivar,
        MiCorreo_ComoVincular,
        MiCorreo_CrearArchivador,
        MiCorreo_CrearTarea,
        MiCorreo_CrearRegistroEs,
        MiCorreo_CrearFacturaRec,
        MiCorreo_CrearExpediente,
        MiCorreo_Eliminar,
        MiCorreo_Imprimir,
        MiCorreo_Tras_Blanquear_Archivador,
        MiCorreo_Tras_Seleccionar_Archivador,
        MiCorreo_Tras_Blanquear_Tarea,
        MiCorreo_Tras_Seleccionar_Tarea,
        MiCorreo_Tras_Blanquear_RegistroEs,
        MiCorreo_Tras_Seleccionar_RegistroEs,
        MiCorreo_Tras_Blanquear_FacturaRec,
        MiCorreo_Tras_Seleccionar_FacturaRec,
        MiCorreo_Tras_Blanquear_Expediente,
        MiCorreo_Tras_Seleccionar_Expediente,
        MiCorreo_Tras_Seleccionar_ArchivadorDestino,
        MiCorreo_Tras_Seleccionar_TareaDestino,

        Agenda_AbrirAgenda,
        Agenda_AbrirAgendaSeleccionada,
        Agenda_EjecutarAccionAsociada,
        Agenda_AlCambiar_EventoDeDia,
        Agenda_EnviarEventoIcs,
        MiCalendario_IrAlElemento,

        Historial_EjecutarAccionAsociada,
        Historial_ExcluirClaseDeObjeto,
        Historial_IncluirClaseDeObjeto,

        Neg_Tras_Seleccionar_Tipo,
        Neg_Tras_Blanquear_Tipo,
        Neg_Tras_Seleccionar_CG,
        Neg_Tras_Blanquear_CG,
        Neg_Tras_Pulsar_Bloquear,

        Negocio_TrasSeleccionarAccion,
        Negocio_TrasSeleccionarAccionDeRelacion,
        Negocio_TrasAbrirModalDePlantillasPorTipo,
        Negocio_TrasAbrirModalDeClasesPorTipo,
        Negocio_TrasAbrirModalDePlantillasDeNegocio,
        Negocio_DescargarPlantillaPorTipo,
        Negocio_DescargarPlantillaDeNegocio,
        Negocio_DescargarEtiquetasDeNegocio,
        Negocio_EnviarEnlaceDeAcceso,
        Negocio_IrAlExpediente,
        Negocio_IrAlProveedor,
        Negocio_IrAlCliente,
        Negocio_IrALaFactura,


        ApiDePeticiones_DescargarEtiquetasDeNegocio,

        BlanquearDatosDeCalle,

        Persona_CrearInterlocutor_Change,
        Persona_CrearAbogado_Change,
        Persona_CrearProcurador_Change,
        Persona_CrearProveedor_Change,
        Persona_CrearCliente_Change,
        Persona_CrearTrabajador_Change,

        Sociedad_CrearInterlocutor_Change,
        Sociedad_CrearAbogado_Change,
        Sociedad_CrearProcurador_Change,
        Sociedad_CrearProveedor_Change,
        Sociedad_CrearCliente_Change,
        Sociedad_CopiarEnRazonSocial,
        Sociedad_IrACertificados,
        Sociedad_AlPegar_Iban,
        Sociedad_AlCambiar_CuentaActiva,
        Sociedad_AlCambiar_TarjetaActiva,
        Sociedad_InicializarModalParaCrearCuentas,
        Sociedad_InicializarModalParaCrearTarjetas,
        Sociedad_InicializarModalParaFacturador,
        Sociedad_RecargarGridDeArchivos,
        Sociedad_AbrirAgenda,

        CG_Tras_Seleccionar_Responsable,
        CG_Tras_Blanquear_Responsable,

        IvaRep_AlCambiar_Exento,
        IvaSop_AlCambiar_Exento,

        Trb_ProponerDatosDelUsuarioSeleccionado,
        Trb_ProponerDatosDelCg,
        Trb_TrasCargarCuentasContables,
        Trb_AlPegar_Iban,
        Trb_AlCambiar_CuentaActiva,
        Trb_InicializarModalParaCrearCuentas,
        Trb_RecargarGridDeArchivos,

        Int_AlPegar_Iban,
        Int_AlCambiar_CuentaActiva,
        Int_InicializarModalParaCrearCuentas,
        Int_RecargarGridDeArchivos,
        Int_ProcesarOpcionDeMenuLista,

        Calle_Tras_Seleccionar_Provincia,
        Calle_Tras_Seleccionar_Municipio,

        Direccion_Tras_Seleccionar_Provincia,
        Direccion_Tras_Seleccionar_Municipio,
        Direccion_Tras_Seleccionar_Calle,
        Direccion_Tras_Blanquear_Calle,

        SisDoc_BloquearArchivo,
        SisDoc_AplicarOperacion,
        SisDoc_BloqueoMultiple,
        SisDoc_DesbloqueoMultiple,
        SisDoc_GenerarZip,
        SisDoc_CambiarEncolumnado,
        SisDoc_SeleccionarTodo,
        SisDoc_AnularSeleccion,
        SisDoc_MostrarModalParaSeleccionarDestino,
        SisDoc_MostrarModalDeDesbloqueoMultiple,
        SisDoc_MostrarModalDeBloqueoMultiple,
        SisDoc_Procesar,
        SisDoc_MostrarModalDeGenerarZip,
        SisDoc_Tras_Seleccionar_Archivador,
        SisDoc_Tras_Seleccionar_Destino,
        SisDoc_Tras_Blanquear_Destino,
        SisDoc_CarpetasDeUnArchivador,
        SisDoc_DescargarConGuid,
        SisDoc_FiltrarEnSelector,
        SisDoc_RecargarVisor,

        Arcdor_AbrirCarpetas,
        Arcdor_ProponerDatosDelArcSeleccionado,
        Arcdor_InicializarModalDeCopiado,
        Arcdor_Tras_Pulsar_Copiar_Archivos,
        Arcdor_Tras_Pulsar_Enlazar_Archivos,

        Preasiento_IrAlOrigenPorId,
        Preasiento_IrAlLoteContable,
        Preasiento_IrAlOrigenDeLaFila,

        CierreContable_IrALaSociedad,
        CierreContable_IrAlLote,
        CierreContable_TrasSeleccionarSociedad,
        CierreContable_TrasBlanquearSociedad,
        CierreContable_TrasCambiarContabilidad,

        Cli_InicializarModalParaCrearCuentas,
        Cli_RecargarGridDeArchivos,
        Cli_RecargarGridDeTrazas,
        Cli_AlPegar_Iban,
        Cli_AlCambiar_CuentaActiva,
        Cli_ProcesarOpcionDeMenuLista,

        Prv_InicializarModalParaCrearCuentas,
        Prv_RecargarGridDeArchivos,
        Prv_AlPegar_Iban,
        Prv_AlCambiar_CuentaActiva,
        Prv_Tras_Cambiar_Modo_De_Pago,
        Prv_Tras_Cambiar_Cg_Propuesto,

        Juzgado_Clase_OnBlur,
        Juzgado_Municipio_OnSelect,
        Juzgado_Calificador_Change,

        Unitario_ProponerReferencia,
        Unitario_DesbloquearReferencia,
        Unitario_InicializarModalParaCrearTarifa,

        Exp_TrasAbrirModalDeCrearValoracion,
        Exp_CalcularValoracion,
        Exp_Tras_Cambiar_ElPrecioIncluyeElIva,
        Exp_InicializarModalParaCrearApuntes,
        Exp_InicializarModalParaEditarApuntes,


        Ppt_CalcularImportesDeLinea,
        Ppt_Tras_Cambiar_ElPrecioIncluyeElIva,
        Ppt_TrasCargarIvas,
        Ppt_InicializarModalParaCrearLineas,
        Ppt_InicializarModalParaEditarLineas,
        Ppt_Tras_Cambiar_TipoDeLinea,
        Ppt_Tras_Blanquear_Unitario,
        Ppt_Tras_Seleccionar_Unitario,
        Ppt_IvaRepercutidoCambiado,
        Ppt_ProponerDatosDelPptSeleccionado,
        Ppt_InicializarModalDeCopiado,
        Ppt_TrasSeleccionarClaseDePpt,

        Calle_IrABarriosDeUnaCalle,
        Calle_IrAZonasDeUnaCalle,
        Calle_IrACpsDeUnaCalle,

        Ctr_Tras_Seleccionar_Cliente,
        Ctr_Tras_Seleccionar_Proveedor,
        Ctr_Tras_Blanquear_Cliente,
        Ctr_Tras_Blanquear_Proveedor,
        Ctr_AjustarControlesDeEdicionDelPlanificador,
        Ctr_TrasSeleccionarClaseDeCtr,
        Ctr_TrasAbrirModalDeCopiarPlfDeVenta,
        Ctr_Tras_Seleccionar_CopiarContratoDeVenta,
        Ctr_Tras_Blanquear_CopiarContratoDeVenta,

        Ctr_Tras_Seleccionar_ClienteDeGuarderia,
        Ctr_Tras_Blanquear_ClienteDeGuarderia,
        Ctr_Tras_Seleccionar_CopiarPlfDeVenta,

        Plv_AjustarControlesDeEdicion,
        Plv_Tras_Seleccionar_TipoDeFactura,
        Plv_Tras_Seleccionar_TipoDeParte,
        Plv_Tras_Blanquear_TipoDeFactura,
        Plv_Tras_Blanquear_TipoDeParte,
        Plv_Tras_Seleccionar_Lote,
        Plv_Tras_Cambiar_TipoDeLinea,

        Plv_InicializarModalParaCrearLineas,
        Plv_InicializarModalParaEditarLineas,
        Plv_Tras_Blanquear_Unitario,
        Plv_Tras_Seleccionar_Unitario,
        Plv_CalcularImportesDeLinea,
        Plv_IvaRepercutidoCambiado,
        Plv_Antes_De_Buscar_Unitarios,
        Plv_Tras_Mapear_Filtro_IdContrato,
        Plv_Tras_Mapear_Filtro_IdPlanificador,

        Lot_Tras_Blanquear_Unitario,
        Lot_Tras_Seleccionar_Unitario,

        Fae_Tras_Cambiar_TipoDeLinea,
        Fae_Tras_Seleccionar_Unitario,
        Fae_Tras_Blanquear_Unitario,
        Fae_CalcularImportesDeLinea,
        Fae_Tras_Cambiar_ElPrecioIncluyeElIva,
        Fae_IvaRepercutidoCambiado,
        Fae_TrasCargarIvas,
        Fae_InicializarModalParaCrearLineas,
        Fae_InicializarModalParaEditarLineas,
        Fae_Tras_Mapear_Filtro_IdContrato,
        Fae_Tras_Cambiar_Clase_De_Cobro,
        Fae_Tras_Cambiar_Clase_De_Abono,
        Fae_InicializarModalParaCrearCobros,
        Fae_InicializarModalParaEditarCobros,
        Fae_InicializarModalParaCrearAbonos,
        Fae_InicializarModalParaEditarAbonos,
        Fae_MapearFechaDeVencimiento,
        Fae_ProponerDatosDeLaFaeSeleccionada,
        Fae_InicializarModalDeCopiado,
        Fae_InicializarModalParaRectificar,
        Fae_Al_Entrar_BI_Sujeta,
        Fae_Tras_Cambiar_Tipo_Irpf,
        Fae_CalcularIrpf,
        Fae_ValidarEnAeat,
        Fae_IrARectificadaPor,
        Fae_IrARectificoA,
        Fae_Antes_De_Buscar_Ppt,
        Fae_Antes_De_Buscar_Ctt,
        Fae_IrALaFacturaDelTercero,

        Ptr_Tras_Cambiar_TipoDeLinea,
        Ptr_Tras_Seleccionar_Unitario,
        Ptr_Tras_Blanquear_Unitario,
        Ptr_CalcularImportesDeLinea,
        Ptr_IvaRepercutidoCambiado,
        Ptr_TrasCargarIvas,
        Ptr_InicializarModalParaCrearLineas,
        Ptr_InicializarModalParaEditarLineas,
        Ptr_Tras_Mapear_Filtro_IdContrato,
        Ptr_Tras_Mapear_Filtro_IdTarea,
        Ptr_CalculaDuracionDeAsignacionPtr,
        Ptr_CopiarFechasDeAsignacionPtr,

        AsiPtr_Tras_Mapear_Filtro_IdContrato,
        AsiPtr_Tras_Mapear_Filtro_IdPresupuesto,

        Rem_Tras_Seleccionar_Cuenta_Abono,
        Rem_Tras_Blanquear_Cuenta_Abono,
        Rem_Ir_A_Facturas_Remesa,
        Rem_MapearFechaDeCargo,

        Pag_Tras_Cambiar_Clase_De_Pago,
        Pag_Tras_Cambiar_Modo_De_Pago,
        Pag_Tras_Seleccionar_Acreedor,
        Pag_Tras_Blanquear_Acreedor,
        Pag_Tras_Seleccionar_Cuenta_Pago,
        Pag_Tras_Seleccionar_Tarjeta_Pago,
        Pag_AlPegar_Iban,

        Rem_Tras_Seleccionar_Cuenta_Pago,
        Rem_MapearFechaDePago,

        Far_Tras_Cambiar_ClaseDeLinea,
        Far_Tras_Indicar_Fecha_De_Emision,
        Far_IvaSoportadoCambiado,
        Far_IrpfCambiado,
        Far_CalcularImpuesto,
        Far_InicializarModalParaCrearLineas,
        Far_InicializarModalParaEditarLineas,
        //Far_TrasCargarNaturalezas,
        Far_Tras_Blanquear_Proveedor,
        Far_Tras_Seleccionar_Proveedor,
        Far_AlCambiar_Pagada,
        Far_Tras_Cambiar_Modo_De_Pago,
        Far_MapearDatosDeLaRectificada,
        Far_ProponerDatosDelaFarSeleccionada,
        Far_InicializarModalDeCopiado,
        Far_Al_Entrar_En_Total_A_Pagar,

        Tar_ProponerDatosDelaTareaSeleccionada,
        Tar_InicializarModalDeCopiado,
        Tar_AbrirTareaAnterior,
        Tar_AbrirTareaPosterior,
        Tar_IrALaFacturaEmt,
        Tar_FiltrosParaCuandoRealizar,

        Utilidades_Totalizar_Editor,

        Ped_Tras_Blanquear_Proveedor,
        Ped_Tras_Seleccionar_Proveedor,
        Ped_Tras_Cambiar_TipoDeLinea,
        Ped_Tras_Cambiar_Naturaleza_Del_Detalle,
        Ped_Tras_Blanquear_Unitario,
        Ped_Tras_Seleccionar_Unitario,
        Ped_CalcularImportesDeLinea,
        Ped_InicializarModalParaCrearLineas,
        Ped_InicializarModalParaEditarLineas,
        Ped_Parametros_Para_Seleccionar_Unitarios,
        Ped_FiltrosPorClaseDeUnitario,
        Pedido_Recepcionar,
        Pedido_RecibirLinea,
        Pedido_LineaNoEntregada,
        Pedido_CalcularImporteEntregado,
        Pedido_Recepcion_TrasCambiarAlmacen,
        Pedido_Recepcion_TrasSeleccionarExpediente,

        Ral_Tras_Seleccionar_Almacen,
        Ral_Tras_Blanquear_Almacen,
        Ral_Tras_Seleccionar_Unitario,
        Ral_Tras_Blanquear_Unitario,
        Ral_InicializarModalParaCrearLineas,
        Ral_CalcularImportesDeLinea,
        Ral_FiltrosPorClaseDeUnitario,

        Mal_IrAlDocumento,
        Mal_IrAlProveedor,
        Mal_IrAlPreasiento,

        Neg_ProcesarOpcionDeMenuLista,

        Expotacion_AlCambiar_Plantilla,

        Cursos_InicializarModalParaIncluirProfesore,
        Cursos_InicializarModalParaIncluirInfante
    }

    public enum enumParamTs
    {
        idLista,
        control,
        lista
    }

    public enum enumTipoDeLlamada { Post, Get }

    public static class enumRutas
    {
        public static readonly string DirectorioDeAgendas = "agendas";
        private static readonly string DirectorioDePlantillas = "plantillas";
        private static readonly string DirectorioDeJson = "json";
        private static readonly string DirectorioDeDescarga = "archivos";
        private static readonly string DirectorioDeImagenes = "images";
        private static readonly string DirectorioDeNails = "nails";
        private static readonly string DirectorioDeTokens = "token.json";

        public static readonly string RutaBase = Directory.GetCurrentDirectory();
        public static readonly string RutaDeDescarga = Path.Combine(RutaBase, "wwwroot", DirectorioDeDescarga).NormalizeRuta();
        public static readonly string RutaDeJson = Path.Combine(RutaBase, "wwwroot", DirectorioDeJson).NormalizeRuta();
        public static readonly string RutaDeAgendas = Path.Combine(RutaBase, "wwwroot", DirectorioDeAgendas).NormalizeRuta();
        public static readonly string RutaDePlantillas = Path.Combine(RutaBase, "wwwroot", DirectorioDePlantillas).NormalizeRuta();
        public static readonly string RutaDeImagenes = Path.Combine(RutaBase, "wwwroot", DirectorioDeImagenes).NormalizeRuta();
        public static readonly string RutaDeNails = Path.Combine(RutaBase, "wwwroot", DirectorioDeImagenes, DirectorioDeNails).NormalizeRuta();
        public static readonly string RutaDeToken = Path.Combine(RutaBase, "wwwroot", DirectorioDeTokens).NormalizeRuta();
    }

    public static class eventosDeListaDinamica
    {
        public const string cargar = "cargar-lista-dinamica";
        public const string perderFoco = "perder-foco-lista-dinamica";
        public const string obtenerFoco = "obtener-foco-lista-dinamica";
    }

    public static class eventosDeMnt
    {
        public const string CrearElemento = "crear-elemento";
        public const string EditarElemento = "editar-elemento";
        public const string HistorialDelProceso = "historial-elemento";
        public const string ExportarElemento = "exportar-elementos";
        public const string EnviarElementos = "enviar-elementos";
        public const string TransitarElementos = "transitar-elementos";
        public const string EliminarElemento = "eliminar-elemento";
        public const string RelacionarElementos = "relacionar-elementos";
        public const string GestionarDependencias = "gestionar-dependencias";
        public const string AbrirModalParaRelacionar = "abrir-modal-para-relacionar";
        public const string AbrirModalParaImputar = "abrir-modal-para-imputar";
        public const string AbrirModalParaConsultarRelaciones = "abrir-modal-para-consultar-relaciones";
        public const string OcultarMostrarFiltro = "ocultar-mostrar-filtro";
        public const string OcultarMostrarBloque = "ocultar-mostrar-bloque";
        public const string OcultarMostrarAmpliacion = "ocultar-mostrar-ampliacion";
        public const string OcultarMostrarDetalle = "ocultar-mostrar-detalle";
        public const string FilaPulsada = "fila-pulsada";
        public const string TeclaPulsada = "tecla-pulsada";
        public const string OcultarMostrarColumnas = "ocultar-mostrar-columnas";
        public const string OrdenarPor = "ordenar-por";
        public const string MostrarOcultarVisorDeDetalle = "mostrar-ocultar-visor-detalle";
        public const string MostrarModalConDireccion = "mostrar-modal-con-direccion";
    }

    public static class eventosDeInterlocutores
    {
        public const string CrearSociedad = "crear-sociedad";
        public const string CrearContacto = "crear-contacto";
        public const string CrearPersona = "crear-persona";
    }

    public static class eventosDeAccionDeGrid
    {
        public const string MostrarSoloSeleccionadas = "mostrar-solo-seleccionadas";
        public const string SeleccionarTodo = "seleccionar-todo";
        public const string AnularSeleccion = "anular-seleccion";
        public const string AnularOrden = "anular-ordenacion";
        public const string AplicarOrdenInicial = "aplicar-orden-inicial";
        public const string RecargarGrid = "recargar-grid";

    }

    public static class eventosDeFormulario
    {
        public const string Cerrar = "cerrar";
        public const string Aceptar = "aceptar";
        public const string OcultarMostrarBloque = "ocultar-mostrar-bloque";
        public const string CerrarFiltro = "cerrar-filtro";
        public const string AplicarFiltro = "aplicar-filtro";
        public const string AbrirFiltro = "abrir-filtro";
        public const string TeclaPulsada = "tecla-pulsada";
    }

    public static class eventosDeJerarquia
    {
        public const string CrearNodo = "crear-nodo";
        public const string ModificarNodo = "modificar-nodo";
        public const string EliminarNodo = "eliminar-nodo";
        public const string CancelarModificacion = "cancelar-modificacion";
        public const string MostarJerarquia = "mostrar-jerarquia";
        public const string Plegar = "plegar-jerarquia";
    }


    public static class eventosDeSociedad
    {
        public const string EditarContacto = "editar-contacto";
    }


    public static class eventosDeExpansor
    {
        public const string OcultarMostrarBloque = "ocultar-mostrar-bloque";
        public const string NavegarDesdeEdicion = "navegar-desde-edicion";
        public const string AbrirModalCrearRelacion = "abrir-modal-crear-relacion";
        public const string AbrirModalCrearDetalle = "abrir-modal-crear-detalle";
        public const string AbrirModalCrearVinculo = "abrir-modal-crear-y-vincular";
        public const string AbrirModalParaVincular = "abrir-modal-para-vincular";
        public const string EliminarRelacion = "eliminar-relacion";
        public const string DarDeAlta = "dar-de-alta";
        public const string AbrirModalEditarRelacion = "abrir-modal-editar-relacion";
        public const string MostrarAcciones = "mostrar-acciones";
        public const string TrasAbrirModal = "tras-abrir-modal";
        public const string NavegarAEditar = "navegar-a-editar";
        public const string AbrirAgenda = "abrir-agenda";
        public const string MostrarPropiedad = "mostrar-propiedad";
    }

    public static class EventosModal
    {
        public const string TrasAbrir = "accion-tras-abrir";
        public const string TrasAceptar = "accion-tras-aceptar";
        public const string TrasModificar = "tras-modificar";
        public const string AlAceptar = "al-aceptar";
        public const string AlCerrar = "al-cerrar";
        public const string ResetearVista = "resetear-vista";
        public const string TituloOpcionCerrar = "tituloOpcionCerrar";

    }

    public static class EventosModalDeRelacion
    {
        public const string CrearRelacion = "crear-relacion";
        public const string CrearVinculo = "crear-vinculo";
        public const string Vincular = "vincular";
        public const string ModificarRelacion = "modificar-relacion";
        public const string Cerrar = "cerrar-modal-relacion";
        public const string CrearDetalle = "crear-detalle";

    }

    public static class enumOpcionDeMenu
    {
        public const string Nuevo = nameof(Nuevo);
        public const string Editar = nameof(Editar);
        public const string Borrar = nameof(Borrar);
        public const string Historial = nameof(Historial);
        public const string Resetear = nameof(Resetear);
    }

    public static class enumOpcionDeMenuInterlocutor
    {
        public const string CrearPersona = "cre-per";
        public const string CrearSociedad = "cre-soc";
        public const string CrearContacto = "cre-ctc";
    }

    public static class enumOpcionDeMenuCliente
    {
        public const string CrearPersona = "cre-per";
        public const string CrearSociedad = "cre-soc";
    }

    public static class eventosDeHistorial
    {
        public const string CerrarHistorial = "cerrar-historial";
        public const string OcultarMostrarFiltro = "ocultar-mostrar-filtro";
    }

    public static class eventosDeCreacion
    {
        public const string NuevoElemento = "nuevo-elemento";
        public const string CerrarCreacion = "cerrar-creacion";
    }

    public static class eventosDeMensaje
    {
        public const string CerrarModal = "cerrar-modal";
        public const string TrasAbrirModal = "tras-abrir-modal";
    }

    public static class eventosDeEdicion
    {
        public const string ModificarElemento = "modificar-elemento";
        public const string FirmarElemento = "firmar-elemento";
        public const string CancelarEdicion = "cancelar-edicion";
        public const string CerrarModal = "cerrar-modal";
        public const string PlegarTodo = "plegar-todo";
        public const string DesplegarTodo = "desplegar-todo";
        public const string MostrarOcultarVisor = "mostrar-ocultar-visor";
        public const string DescargarConGuid = "descargar-con-guid";
        public const string Avanzar = "avanzar";
        public const string Retroceder = "retroceder";
    }
    public static class eventosDeRelacionar
    {
        public const string Relacionar = "nuevas-relaciones";
        public const string Cerrar = "cerrar-relacionar";
    }
    public static class eventosDeImputar
    {
        public const string Imputar = "imputar";
        public const string Cerrar = "cerrar";
    }
    public static class eventosDeConsulta
    {
        public const string Cerrar = "cerrar-consulta";
    }
    public static class eventosParaSeleccionar
    {
        public const string Cerrar = "cerrar-seleccionar";
    }
    public static class eventosDeExportar
    {
        public const string Exportar = "exportar";
        public const string Cerrar = "cerrar-exportacion";
        public const string PulsarSometer = "pulsar-someter";
        public const string SalirListaDeCorreos = "salir-lista-correos";
    }
    public static class eventosDeEnviarCorreo
    {
        public const string Enviar = "enviar-correo";
        public const string Cerrar = "cerrar-enviar-correo";
        public const string SalirListaDeCorreos = "salir-lista-correos";
        public const string SeleccionaUsuarios = "seleccionar-usuarios";
    }

    public static class eventosDeTransitar
    {
        public const string Transitar = "transitar";
        public const string Seleccionar = "seleccionar";
        public const string Cerrar = "cerrar";
    }

    public static class eventosDeImpresion
    {
        public const string Imprimir = "imprimir";
        public const string Cerrar = "cerrar";
    }

    public static class eventosDeSelectorModal
    {
        public const string PerderFoco = "perder-foco";
        public const string ObtenerFoco = "obtener-foco";
        public const string Blanquear = "blanquear-selector";
        public const string TrasSeleccionar = "tras-seleccionar";
        public const string OpcionSeleccionada = "opcion-seleccionar";
    }
    public static class ModoDePresentacionCrud
    {
        public const string Grid = "grid";
        public const string FichasPorEstado = "fichas-por-estado";
        public const string FichasPorTipo = "fichas-por-tipo";
        public const string FichasPorPrioridad = "fichas-por-prioridad";
        public const string FichasPorProveedor = "fichas-por-proveedor";
    }

    public static class eventosDeMf
    {

        private const string IrAContrato = "ir-a-contratos";
        private const string IrAExpediente = "ir-a-expedientes";
        private const string IrAPartesTr = "ir-a-partes-trabajo";
        private const string IrAPpts = "ir-a-ppts";
        private const string IrAFacturasEmt = "ir-a-facturas-emitidas";
        private const string IrAPlvDeVenta = "ir-a-planificaciones-venta";
        private const string IrAFacturasRec = "ir-a-facturas-recibidas";
        private const string IrATareas = "ir-a-tareas";
        private const string ModalDeImprimir = "abrir-imprimir";


        public const string QuitarExpediente = "quitar-expediente";
        public const string QuitarContrato = "quitar-contrato";

        public const string CrearObservacion = "crear-observacion";
        public const string ModalDeCrearEvento = "crear-evento";
        public const string CrearArchivador = "crear-archivador";
        public const string AbrirEnviarCorreo = "abrir-correo";
        public const string AbrirTransitar = "abrir-transitar";
        public const string ModalDeCrearDirecciones = "crear-direccion";
        public const string ModalDeExportar = "abrir-exportar";
        public const string SalvarDatosDelGrid = "salvar-grid";
        public const string DarDeAlta = "dar-de-alta";
        public const string DarDeBaja = "dar-de-baja";
        public const string Contactos = "contactos";
        public const string Interlocutores = "interlocutores";
        public const string Procuradores = "procuradores";
        public const string Abogados = "abogados";
        public const string Proveedor = "ir-a-proveedor";
        public const string Cliente = "ir-a-cliente";
        public const string Trabajador = "ir-a-trabajador";
        public const string CentrosGestores = "cgs";
        public const string CuentasBancarias = "cta-bancarias";
        public const string MiCertificado = "mi-certificado";
        public const string Transiciones = "transiciones";
        public const string Flujo = "flujo";
        public const string CrearTareas = "crear-tarea";
        public const string AbrirAgenda = "abrir-agenda";
        public const string GenerarLosPlanificadores = "generar-planificadores";
        public const string GenerarPlanificadoresDeUnContrato = "generar-planificadores-contrato";
        public const string PrepararPartesDeTrabajo = "preparar-partes-de-trabajo";
        public const string EmitirPrefacturasPorParteTr = "emitir-prefacturas-por-partetr";
        public const string EmitirPrefacturasPorContrato = "emitir-prefacturas-por-contrato";
        public const string AsociarCertificado = "asociar-certificado";


        public const string Comun_Imprimir = ModalDeImprimir;
        public const string Comun_PermisosDeElemento = "permiso-de-elemento";
        public const string Comun_GuardarDatosCreacion = "guardar-datos-creacion";
        public const string Comun_GuardarPlantillaCreacion = "guardar-plantillas-creacion";
        public const string Comun_GuardarPlantillaFiltrado = "guardar-plantillas-filtrado";
        public const string Comun_OcultarColumnas = "ocultar-columnas";
        public const string Comun_GuardarColumnasDelGrid = "guardar-columnas-del-grid";
        public const string Comun_EliminarColumnasDelGrid = "eliminar-columnas-del-grid";
        public const string Comun_GuardarDisposicionDeArchivos = "guardar-disposicion-de-archivos";
        public const string Comun_LeerDisposicionDeArchivos = "leer-disposicion-de-archivos";
        public const string Comun_GuardarEstadosDeExpansores = "guardar-estados-de-expansores";
        public const string Comun_Disposicion_Del_Encolumnado = "disposicion-del-encolumnado";
        public const string Comun_Ordenacion_Del_Resultado = "ordenacion-del-resultado";
        public const string Comun_Tamano_Del_Encolumnado = "tamano-del-encolumnado";
        public const string Comun_Cantidad_A_Leer = "cantidad-a-leer";
        public const string Comun_Modo_De_Presentacion = "modo-de-presentacion";
        public const string Comun_Tamano_Del_Visor = "tamano-del-visor";
        public const string Comun_Mostrar_Visor_Al_iniciar = "MostrarVisorAlIniciar";
        public const string Comun_EliminarPlantillaCreacion = "eliminar-plantillas-creacion";
        public const string Comun_EliminarPlantillaFiltrado = "eliminar-plantillas-filtrado";
        public const string Comun_BlanquearFiltros = "blanquear-filtros";
        public const string Comun_LeerDatosParaExportacion = "leer-datos-para-exportacion";
        public const string Comun_EnviarElemento = "enviar_elemento";
        public const string Comun_EnviarCorreoExterno = "enviar-correo-externo";
        public const string Comun_EnviarMensajeWeb = "enviar-mensaje-web";
        public const string Comun_GuardarDisposicionDashBoard = "guardar-disposicion-dashboard";
        public const string Comun_GuardarDisposicionEstados = "guardar-disposicion-estados";

        public const string Imputar_FacturasRec = "imputar-facturas";

        public const string Totalizador_Mostrar = "mostrar-totales";

        public const string MiCorreo_CrearArchivador = nameof(enumFunctionTs.MiCorreo_CrearArchivador);
        public const string MiCorreo_CrearTarea = nameof(enumFunctionTs.MiCorreo_CrearTarea);
        public const string MiCorreo_CrearRegistroEs = nameof(enumFunctionTs.MiCorreo_CrearRegistroEs);
        public const string MiCorreo_CrearFacturaRec = nameof(enumFunctionTs.MiCorreo_CrearFacturaRec);
        public const string MiCorreo_CrearExpediente = nameof(enumFunctionTs.MiCorreo_CrearExpediente);
        public const string MiCorreo_ComoArchivar = nameof(enumFunctionTs.MiCorreo_ComoArchivar);
        public const string MiCorreo_ComoVincular = nameof(enumFunctionTs.MiCorreo_ComoVincular);


        public const string Correo_Enviar = nameof(Correo_Enviar);


        //public const string SalvarDatosDelFiltro = "salvar-filtro";
        //public const string EliminarDatosDelFiltro = "eliminar-filtro";


        public const string Soc_CuentasBancarias = CuentasBancarias;
        public const string Soc_TarjetasBancarias = "tar-bancarias";
        public const string Soc_facturador = "facturador-sociedad";
        public const string Soc_InfoContable = "inf-contable";
        public const string Soc_ActivarVerifactu = "activar-verifactu";
        public const string Soc_RecomponerBlockChain = "recomponer-blockchain";
        public const string Soc_CatalogosJudiciales = "catalogos-judiciales";



        public const string Arc_IrACapetas = "ir-a-carpetas";
        public const string Arc_ImportarZip = "importar-zip";
        public const string Arc_ProcesarFarConIa = "procesar-far-con-ia";
        public const string Arc_Descontabilizar = "descontabilizar-spr";
        public const string Arc_ExportarArchivador = "exportar-archivador";
        public const string Arc_CopiarArc = "copiar-archivador";

        public const string Tar_IrAPartesTr = IrAPartesTr;
        public const string Tar_CopiarTarea = "copiar-tarea";
        public const string Tar_CuandoRealizar = "cuando-realizar";
        public const string Tar_EliminarCuandoRealizar = "eliminar-cuando-realizar";

        public const string Exp_VincularRegistroEntrada = "vincular-re";
        public const string Exp_IrATareas = IrATareas;
        public const string Exp_ImputarFacturas = Imputar_FacturasRec;
        public const string Exp_IrAFacturasRec = IrAFacturasRec;

        public const string Ppt_IrAFacturasEmt = IrAFacturasEmt;
        public const string Ppt_IrAPartesTr = IrAPartesTr;
        public const string Ppt_IrATareas = IrATareas;
        public const string Ppt_CopiarPpt = "copiar-ppt";
        public const string Ppt_AsociarUnExpedienteAUnPpt = "vincular-expediente";
        public const string Ppt_ModalDeImprimir = ModalDeImprimir;
        public const string Ppt_Renombrar = "renombrar-ppt";

        public const string Ctr_IrAFacturasEmt = IrAFacturasEmt;
        public const string Ctr_IrAPartesTr = IrAPartesTr;
        public const string Ctr_IrAPlvDeVenta = IrAPlvDeVenta;
        public const string Ctr_IrAFacturasRec = IrAFacturasRec;
        public const string Ctr_ImputarFacturas = Imputar_FacturasRec;
        public const string Ctr_VincularRegistroEntrada = "vincular-re";

        public const string Plt_VincularRegistroEntrada = "vincular-re";

        public const string Plf_IrAPlvDeVenta = IrAPlvDeVenta;
        public const string Plf_GenerarPlanificadorDeVenta = "generar-planificaciones-venta";

        public const string Plv_IrAFacturasEmt = IrAFacturasEmt;
        public const string Plv_IrAPartesTr = IrAPartesTr;

        public const string Cli_CentroAdministrativo = "nuevo-centro-adm";
        public const string Cli_PuestoDeTrabajo = "nuevo-puesto-trabajo";
        public const string Cli_NuevoClienteWeb = "nuevo-cliente-web";
        public const string Cli_AsociarClienteWeb = "vincular-cliente-web";
        public const string Cli_ValidarNif = "validar-nif-aeat";

        public const string Fae_CambiarVencimiento = "cambiar-vencimiento";
        public const string Fae_CopiarFae = "copiar-fae";
        public const string Fae_CambiarDatos = "cambiar-datos-fae";
        public const string Fae_Rectificativa = "rectificativa";
        public const string Fae_FacturarTareas = "facturar-tareas";
        public const string Fae_CopiarLa = "copiar-la";
        public const string Fae_IrAContrato = IrAContrato;
        public const string Fae_IrAPpts = IrAPpts;
        public const string Fae_IrAPartesTr = IrAPartesTr;
        public const string Fae_ModalDeImprimir = ModalDeImprimir;
        public const string Fae_GenerarPreasiento = "generar-preasiento";
        public const string Fae_GenerarUbl = "generar-ubl";
        public const string Fae_SincronizarConAeat = "sicronizar-con-la-aeat";
        public const string Fae_AnadirCobro = "anadir-cobro-mf";


        public const string Rem_Fae_Cargar = "cargar-remesa";
        public const string Rem_Fae_AnularCargo = "anular-cargo-remesa";
        public const string Rem_Fae_ModalDeImprimir = ModalDeImprimir;

        public const string Rem_Pag_Pagar = "pagar-remesa";
        public const string Rem_Pag_RetrodePago = "retroceder-pago";
        public const string Rem_Pag_ModalDeImprimir = ModalDeImprimir;

        public const string Pag_ModalDeImprimir = ModalDeImprimir;
        public const string Pag_GenerarPreasiento = "generar-preasiento";
        public const string Pag_CancelarPreasientos = "cancelar-preasientos";

        public const string Ptr_DarPorRealizadasSegunPlan = "dar-por-realizada-plan";
        public const string Ptr_DarPorRealizadasHoy = "dar-por-realizada-hoy";
        public const string Ptr_SolicitarFechaDeEjecucion = "solicitar-fechas-ejecucion";
        public const string Ptr_IrAContrato = IrAContrato;
        public const string Ptr_IrAPpts = IrAPpts;
        public const string Ptr_IrAFacturasEmt = IrAFacturasEmt;
        public const string Ptr_IrAPlvDeVenta = IrAPlvDeVenta;
        public const string Ptr_ModalDeImprimir = ModalDeImprimir;

        public const string Far_QuitarExpediente = QuitarExpediente;
        public const string Far_QuitarContrato = QuitarContrato;
        public const string Far_IrAPago = "ir-a-pagos";
        public const string Far_IrAContrato = IrAContrato;
        public const string Far_IrAExpediente = IrAExpediente;
        public const string Far_ModalDeImprimir = ModalDeImprimir;
        public const string Far_ImportarXml = "importar-efactura";
        public const string Far_ImportarPrv = "importar-proveedor";
        public const string Far_CrearFarConIa = "crear-far-ia";
        public const string Far_CopiarFar = "copiar-far";
        public const string Far_RectificarFar = "rectificar-far";
        public const string Far_Renombrar = "renombrar-far";
        public const string Far_CambiarProveedor = "cambiar-proveedor";
        public const string Far_GenerarPreasiento = "generar-preasiento";
        public const string Far_CancelarPreasientos = "cancelar-preasientos";

        public const string Ped_IrAContrato = IrAContrato;
        public const string Ped_IrAExpediente = IrAExpediente;
        public const string Ped_QuitarExpediente = QuitarExpediente;
        public const string Ped_QuitarContrato = QuitarContrato;
        public const string Ped_ModalDeImprimir = ModalDeImprimir;
        public const string Ped_ModificarCabecera = "modificar-cabecera-pedido";
        public const string Ped_GuardarCabecera = "guardar-cabecera-pedido";
        public const string Ped_RecepcionarLinea = "recepcionar-linea-pedido";
        public const string Ped_RecepcionarPedido = "recepcionar-pedido";

        public const string Infantes_AsociarCurso = "asociar-curso";

        public const string Spr_CrearLote = "crear-lote";
        public const string Spr_AnularLote = "anular-lote";
        public const string Spr_RegenerarLote = "regenerar-lote";
        public const string Spr_LoteDeTerceros = "lote-terceros";
        public const string Spr_CrearLoteConUnPreasiento = "contabilizar-preasiento";
        public const string Spr_RegenerarPreasiento = "regenerar-preasiento";
        public const string Spr_AnularEstimacionDirecta = "anular-estimacion";

        public const string Ccc_CrearCierre = "crear-cierre";
        public const string Ccc_EliminarCierre = "eliminar-cierre";

        public const string Uni_ImportarCatalogo = "importar-catalogo-unitarios";
        public const string Uni_ImportarTarifa = "importar-tarifa";
        public const string Prv_ImportarTarifa = "importar-tarifa-de-proveedor";

        public const string Juz_ImportarCatalogo = "importar-catalogo-juzgados";

        public const string Mun_ImportarCatalogo = "importar-catalogo-municipios";
    }
    public static class enumParaQueNavegar
    {
        public const string ParametroDeUrl = "paraque-navegar";
        public const string crear = "crear";
        public const string editar = "editar";
        public const string seleccionar = "seleccionar";
        public const string consultar = "consultar";
        public const string gestionarRelaciones = "gestionar-relaciones";
        public const string gestionarDependencias = "gestionar-dependencias";
    };

    public enum enumModoOrdenacion
    {
        ascendente,
        descendente,
        sinOrden,
    };

    public enum enumCssOrdenacion
    {
        SinOrden,
        Ascendente,
        Descendente
    }

    public enum enumCssImportant
    {
        SinMarginTop
    }

    public enum enumCssFiltro
    {
        ColumnaFiltro,
        ListaDinamica,
        ListaDeElementos,
        ListaDeMenu,
        ContenedorDeReferenciasParaAbrirModales,
        ContenedorListaDinamica,
        ContenedorListaDeElementos,
        OpcionesMenuDeCreacion,
        OpcionesMenuDeCreacionMovil,
        ContenedorCheck,
        ContenedorEditor,
        ContenedorSelector,
        ContenedorEntreFechas,
        ContenedorEntreRangos,
        ContenedorEntreFechasModal,
        ContenedorEntreRangosModal,
        ContenedorEntreImportes,
        ContenedorEntreImportesModal,
        ContenedorDeRelacionModal,
        ContenedorDireccion,
        ContenedorPreasientos,
        ContenedorConDosListasModal,
        ContenedorDelTipoRelacionadoModal,
        ContenedorListaDinamicaConChek,
        ContenedorEntreImportesMostrarModal,
        ContenedorEntreFechasMostrarModal,
        ContenedorListaDeElementosMostrarModal,
        ContenedorDeDosControlesEnCelda,
        Check,
        CheckOn,
        CheckOff,
        ControlApilado,
        Hora,
        Fecha,
        Rango,
        Numero,
        Importe,
        FilaFiltro,
        FilaFiltroSinSpan,
        ContenedorEnModalDeFiltros,
        EtiquetaEntreFechas,
        EtiquetaEntreRangos,
        EtiquetaConEditor,
        EtiquetaEntreImportes
    }

    public enum enumCssCuerpo
    {
        Cuerpo,
        CuerpoSoloConGrid,
        CuerpoSoloConsulta,
        CuerpoCabecera,
        CuerpoDatos,
        CuerpoDatosError,
        CuerpoDatosFiltro,
        CuerpoDatosGrid,
        CuerpoDatosFormulario,
        CuerpoCabeceraFormulario,
        CuerpoDatosGridThead,
        CuerpoDatosGridTboby,
        CuerpoPie,
        CuerpoPieFormulario,
        CuerpoDatosFiltroBloque,
        CuerpoDatosFiltroReferencias
    }

    public enum enumCssMnt
    {
        MntMenuContenedor,
        MntMenuZona,
        MntFiltroExpansor,
        MntFiltroBloqueContenedor,
        MntFiltroBloqueVacio,
        MntTablaDeFiltro,
        MenuDelProceso,
        MenuIndividual,
        MenuContextual,
        MenuDeFiltro,
        MenuDeDetalle,
        MenuDeDetalleOculto,
        MenuDeDetalleVisible,
        MenuDeDetalleInvisible,
        MenuDeMostrarDireccion,
        MenuDeMostrarDireccionInvisible,
        MenuDeMostrarMapa,
        MenuDeRelaciones,
        MenuFormulario,
        DivVacioDeLaDerecha,
        DivNulo
    }

    public enum enumCssModal
    {
        ContenidoModal,
        ContenidoModalConCabecera,
        EstiloModalConCabecera,
        ContenidoCabecera,
        CabeceraRelacionarElementos,
        CabeceraParaImputar,
        ContenidoCuerpo,
        ContenedorCuerpoConGrid,
        ContenidoCuerpoConGrid,
        EstiloContenidoCuerpo,
        ContenidoPie,
        ContenidoPieSoloBotones,
        DivDeBotonesPie,
        PieDeTotales,
        BotonPrincipal,
        BotonSecundario,
    }

    public enum enumCssNavegadorEnModal
    {
        InfoGrid,
        Mensaje,
        Cantidad,
        Opcion,
        Contenedor,
        Navegador
    }

    public enum enumCssNavegadorEnMnt
    {
        InfoGrid,
        Mensaje,
        Cantidad,
        Opcion,
        Navegador
    }

    public enum enumCssDiv
    {
        DivVisible,
        DivOculto,
        DivConMasPropiedades,
        DivConConteidoAlineadoALaDerecha,
        Nulo,
        SeparadorTop10px,
        SeparadorTop50Left30px,
        Tabla,
        Thead,
        Tbody,
        Th,
        Tr,
        Td,
        CuerpoDatosTabla,
        ColumnaConBlockNoFlex
    }


    public enum enumCssBootStrap
    {
        table,
        tableStriped
    }

    public enum enumCssDeTransitar
    {
        Cuerpo,
        ContenedorAnotacion
    }


    public enum enumCssDeImprimir
    {
        Cuerpo
    }


    public enum enumCssOpcionMenu
    {
        DeVista,
        DeElemento,
        Basico,
        BotonPorDefecto,
        BotonesDeMenu
    }

    public enum enumCssGrid
    {
        ColumnaOculta,
        ColumnaCabecera,
        ContenedorDesplazador,
        ContenedorTamano,
        ContenedorVisualizadorColumna,
        ColumnaPosicionableDerecha,
        ColumnaPosicionableIzquierda,
        ColumnaAccion,
        ColumnaAlineadaDerecha,
        ColumnaDiv,
        OcultarColumna,
        OrdenarColumna,
        TransitarElementos,
        OrdenarColumnaNoPermitido,
        OcultarColumnaIzquierda,
        OcultarColumnaDerecha,
        OcultarColumnaCentrado,
        MostrarColumnas,
        AumentarTamanoColumna,
        ReducirTamanoColumnas,
        Nulo,
        ContenedorDeLaTablaConGraficos,
        ContenedorDelGridConElDivDeLaTabla,
        ContenedorDelGridConElDivDeGraficos,
        Splitter,
        filaAutoFr,
        fila200pxFr,
        fila_2fr_0px_1fr_1fr_1fr,
        fila_2fr_0px_1fr_1fr,
        fila_2fr_0px_1fr_1fr_0px_0px,
        fila_100px_1fr_2fr_0px_0px,
        fila_2fr_0px_2fr_0px
    }

    public enum enumCssHistorial
    {
        CuerpoHistorial,
        ContenedorCabecera,
        ContenedorCuerpo,
        ContenedorPie,
        ContenedorMenu,
        ZonaMenu,
        Titulo,
        Proceso,
        FiltroExpansor,
        MenuMfHistorial,
    }

    public enum enumCssCreacion
    {
        TablaDeCreacion,
        CuerpoDeCrearcion,
        EditarTrasCrear,
        ContenedorPieOpciones,
        ContenedorDeCreacioCuerpo,
        ContenedorDeCreacioDatos,
        ContenedorDeCreacioVisor,
        VisorDeCreacion,
        NavegadorDeCreacion,
        VisorNombreAnexado,
        VisorOculto,
        ContenedorPieModalOpciones,
    }

    public enum enumCssEdicion
    {
        TablaDeEdicion,
        CuerpoDeTablaDeEdicion,
        CuerpoDeEdicion,
        CuerpoDeEdicionSoloConsulta,
        ContenedorDeEdicionCabecera,
        ContenedorDeEdicionCabeceraSoloConsulta,
        ContenedorDeEdicionCuerpo,
        ContenedorDeEdicionEditor,
        VisorOculto,
        ContenedorDeEdicionCuerpoDatos,
        ContenedorDeEdicionCuerpoVisor,
        ContenedorDeEdicionCuerpoHistorial,
        ContenedorDelVisorDeArchivoConHistorial,
        NavegadorDeEdicionCuerpoVisor,
        VisorNombreAnexado,
        VisorDeEdicion,
        VisorDeHistorial,
        MenuDeHistorial,
        ContenedorDeEdicionPie,
        ContenedorDeEdicionPieSoloLectura,
        ContenedorId,
        Titulo,
        MenuDelProceso,
        MenuIndividual,
        DivVacioDeLaDerecha,
        DivNulo,
        AbrirImagenDeDetalle,
        CerrarImagenDeDetalle,
        TituloDeDetalle,
        AbrirImagenDeAmpliacion,
        CerrarImagenDeAmpliacion,
        TituloDeAmpliacion,
        Ampliacion,
        Detalle,
        PieDeDetalle,
        ContenedorDeAmpliacion,
        DatosPrincipales,
        ContenedorDatosPrincipales,
        AccionesDelPanelDeEdicion,
        Tabla,
        Tbody,
        Tr,
        Td
    }
    public enum enumCssAccionesPanelEdicion
    {
        [Description("img-detalle-dto-plegar")]
        Plegar,
        [Description("img-detalle-dto-desplegar")]
        Desplegar,
        [Description("img-detalle-dto-visor")]
        Visor,
        [Description("img-detalle-dto-avanzar")]
        Avanzar,
        [Description("img-detalle-dto-retroceder")]
        Retroceder,
        [Description("ocultar-visor")]
        MostrarVisor,
        [Description("mostrar-visor")]
        OcultarVisor,
        [Description("sin-visor")]
        SinVisor
    }

    public enum enumCssControlesFormulario
    {
        Editor,
        Lista,
        Check,
        Archivo,
        SelectorArchivo,
        InfoArchivo,
        ContenedorOpcion,
        ContenedorBarra,
        MenuCabecera,
        MenuPie
    }


    public enum enumCssJerarquia
    {
        Contenedor
    }

    public enum enumCssMenuFlotante
    {
        menuFlotante,
        Negro,
        SombraNegra,
        Blanco,
        SombraBlanca,
        MenuConScroll,
        IaDisponible,
        IaSeleccionada
    }

    public enum enumCssMenuDelGrid
    {
        menuDelGrid,
        menuDelGridRaiz,
        menuDelGridOpcion
    }


    public enum enumCssExpansor
    {
        Contenedor,
        Cabecera,
        CuerpoSimple,
        CuerpoCompuesto,
        CuerpoDeDetalle,
        CuerpoDeControles,
        Pie,
        ContenedorDeControl,
        Expansor,
        GridDeRelacion
    }

    public enum enumCssAgenda
    {
        EventoAjustadoAlContenedor,
        AjustarTextoAlDiv
    }

    public enum enumCssControles
    {
        Nulo,
        FormDeArchivo,
        BotonNavegacion,
        BotonCompartir,
        ContenedorListaDeElementos,
        ContenedorEditorConEtiquetaIzquierda,
        ContenedorListaDinamica,
        ContenedorEtiqueta,
        ContenedorEtiquetaIzquierda,
        ContenedorEditor,
        ContenedorBotonSelector,
        ContenedorCheck,
        ContenedorCheckRight,
        ContenedorFecha,
        ContenedorFechaDerecha,
        ContenedorFechaHora,
        ContenedorAreaDeTexto,
        ContenedorAreaDeTextoCentrado,
        ContenedorArchivo,
        ContenedorReferencias,
        ContenedorMenuDeReferencias,
        TablaDeArchivo,
        FilaDeArchivo,
        ColumnaDeArchivo,
        SelectorDeFecha,
        SelectorDeHora,
        CheckEnLinea,
        CheckApilado,
        ControlApilado,
        RefApilado,
        PosicionAbsoluta,
        ReferenciaCentrada,
        ReferenciaCentradaEnTd,
        Selector,
        Editor,
        InfoDeTotales,
        BotonSelector,
        Etiqueta,
        EtiquetaDerecha,
        ListaDeElementos,
        ListaDinamica,
        AreaDeTexto,
        MonoSpaceText,
        SelectorDeImagen,
        BarraAzulArchivo,
        EditorRestrictor,
        EditorDeFiltro,
        RestrictorDeFiltro,
        ReferenciaAlineadaAlFinal,
        BotonComoReferencia,
        DivEnBlanco,
        DivNoVisible,
        DivVisible,
        ReferenciaDeMenu,
        ContenedorDeArchivos,
        ArchivosSeleccionados,
        SelectorDeArchivos,
        SelectorDeUnArchivo,
        ImagenEnBoton,
        ContenedorDeOpcion,
        ContenedorDeOpcionDerecha,
        FiltroSelectorDeArchivos,
        LinksOpcionesSelectorDeArchivos,
        ReferenciaAbrirFiltro,
        Oculto,
        EntreImportesMostrar,
        EntreFechasMostrar,
        ListaValoresMostrar,
        DescargarArchivo,
        ProcesarConIa,
        PasarOcr,
        CompartirConWhatsApp,
        ConvertirAPdf,
        CompartirConGuid,
        AlternarVistaFichas,
        ConsultarConGuid,
        EnviarCorreo,
        NavegacionImagenes,
        NavegacionZoom
    }

    public enum enumGridTemplateColumn
    {
        GridTemplateColumn_2_100_80
    }

    public enum enumCssFormulario
    {
        ContenedorDeBloquesApilados,
        ContenedorDePrimerBloquesAnexados,
        ContenedorDeSiguientesBloquesAnexados,
        ContenedorEdicionDto,
        DatosDto,
        BloqueExpansor,
        BloqueDatos,
        BloqueAnexado,
        BloqueIzquierdo,
        BloqueDerecho,
        Tabla,
        fila,
        columnaLabel,
        columnaControl,
        referenciaExpansor,
        CabeceraFormularioMenu,
        CabeceraFormularioOpciones,
        CabeceraFormularioFiltro,
        ModalDeFiltroDeFormulario
    }

    public enum enumCssExportacion
    {
        Contenedor,
        lista,
        sometido,
        enviar,
        mostradas,
        plantilla,
        destino
    }

    public enum enumCssEnviarCorreo
    {
        Contenedor,
        cabecera,
        cuerpo,
        adjuntos
    }

    public enum enumCssSelectorEnModal
    {
        Contenedor,
        Editor,
        botonSelector
    }



    public enum enumClaseDePago
    {
        [Description("Contado")]
        Contado,
        [Description("Transferencia")]
        Transferencia,
        [Description("Remesa bancaria")]
        Remesa
    }

    public enum enumModoDePagoContado
    {
        [Description("Contado")]
        Contado,
        [Description("Tarjeta")]
        Tarjeta,
        [Description("Domiciliación bancaria")]
        Domiciliacion
    }

    public enum enumModoDePagoDelGasto
    {
        [Description("Contado")]
        Contado,
        [Description("Tarjeta")]
        Tarjeta,
        [Description("Banco")]
        Banco
    }

    public enum enumPrioridad
    {
        [Css("semaforo-violeta")]
        [Description("Urgente")]
        Urgente,
        [Css("semaforo-rojo")]
        [Description("Alta")]
        Alta,
        [Css("semaforo-azul")]
        [Description("Media")]
        Media,
        [Css("semaforo-verde")]
        [Description("Baja")]
        Baja,
        [Css("semaforo-amarillo")]
        [Description("No definida")]
        NoDefinida
    }
}