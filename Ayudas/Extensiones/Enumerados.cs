
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
        InicializarModalMiCertificado,
        InicializarModalCambiarPassword,

        SeleccionarIa,
        MostarOcultarAbout,
        DescargarDeclaracionResponsable,
        Logout,

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
        MiCalendario_IrAlElemento,

        Historial_EjecutarAccionAsociada,
        Historial_ExcluirClaseDeObjeto,
        Historial_IncluirClaseDeObjeto,

        Neg_Tras_Seleccionar_Tipo,
        Neg_Tras_Blanquear_Tipo,
        Neg_Tras_Seleccionar_CG,
        Neg_Tras_Blanquear_CG,
        Neg_Tras_Pulsar_Bloquear,
        Neg_Tras_Pulsar_Mostrar_Colores,

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
        Exp_InicializarModalParaCrearApuntes,
        Exp_InicializarModalParaEditarApuntes,


        Ppt_CalcularImportesDeLinea,
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

        Utilidades_Totalizar_Editor,

        Ped_Tras_Blanquear_Proveedor,
        Ped_Tras_Seleccionar_Proveedor,
        Ped_Tras_Cambiar_TipoDeLinea,
        Ped_Tras_Blanquear_Unitario,
        Ped_Tras_Seleccionar_Unitario,
        Ped_CalcularImportesDeLinea,
        Ped_InicializarModalParaCrearLineas,
        Ped_InicializarModalParaEditarLineas,

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
        public const string Comun_Tamano_Del_Visor = "tamano-del-visor";
        public const string Comun_Mostrar_Visor_Al_iniciar = "MostrarVisorAlIniciar";        
        public const string Comun_EliminarPlantillaCreacion = "eliminar-plantillas-creacion";
        public const string Comun_EliminarPlantillaFiltrado = "eliminar-plantillas-filtrado";
        public const string Comun_LeerDatosParaExportacion = "leer-datos-para-exportacion";
        public const string Comun_EnviarElemento = "enviar_elemento";

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

        public const string Infantes_AsociarCurso = "asociar-curso";

        public const string Spr_CrearLote = "crear-lote";
        public const string Spr_AnularLote = "anular-lote";
        public const string Spr_RegenerarLote = "regenerar-lote";
        public const string Spr_LoteDeTerceros = "lote-terceros";
        public const string Spr_CrearLoteConUnPreasiento = "contabilizar-preasiento";
        public const string Spr_RegenerarPreasiento = "regenerar-preasiento";
        public const string Spr_AnularEstimacionDirecta = "anular-estimacion";
    }
    public static class enumParaQueNavegar
    {
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
        PieDeTotales
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
        CuerpoDatosTabla
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
        Splitter
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
        CompartirConGuid,
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

    public static class MetodosDeRenderizacion
    {
        public static string Render(this enumGridTemplateColumn modo)
        {
            switch (modo)
            {
                case enumGridTemplateColumn.GridTemplateColumn_2_100_80: return "grid-template-column-2-100-80";
            }

            throw new Exception($"No se ha definido como renderizar el modo {modo}");
        }

        public static string Render(this enumCssJerarquia modo)
        {
            switch (modo)
            {
                case enumCssJerarquia.Contenedor: return "contenedor-arbol";
            }

            throw new Exception($"No se ha definido como renderizar el modo {modo}");
        }
        public static string Render(this enumCssMenuFlotante modo)
        {
            switch (modo)
            {
                case enumCssMenuFlotante.menuFlotante: return "menu-flotante";
                case enumCssMenuFlotante.Negro: return "menu-flotante-black";
                case enumCssMenuFlotante.SombraNegra: return "menu-flotante-black-shadow";
                case enumCssMenuFlotante.Blanco: return "menu-flotante-white";
                case enumCssMenuFlotante.SombraBlanca: return "menu-flotante-white-shadow";
                case enumCssMenuFlotante.MenuConScroll: return "menu-flotante-con-scroll";
                case enumCssMenuFlotante.IaDisponible: return "ia-disponible";
                case enumCssMenuFlotante.IaSeleccionada: return "ia-seleccionada";


            }

            throw new Exception($"No se ha definido como renderizar el modo {modo}");
        }

        public static string Render(this enumCssMenuDelGrid modo)
        {
            switch (modo)
            {
                case enumCssMenuDelGrid.menuDelGrid: return "menu-del-grid";
                case enumCssMenuDelGrid.menuDelGridRaiz: return "menu-del-grid-raiz";
                case enumCssMenuDelGrid.menuDelGridOpcion: return "menu-del-grid-opcion";
            }
            throw new Exception($"No se ha definido como renderizar el modo {modo}");
        }

        public static string Render(this enumModoOrdenacion modo)
        {
            switch (modo)
            {
                case enumModoOrdenacion.ascendente: return "ascendente";
                case enumModoOrdenacion.descendente: return "descendente";
                case enumModoOrdenacion.sinOrden: return "sin-orden";
            }

            throw new Exception($"No se ha definido como renderizar el modo {modo}");
        }

        public static string Render(this enumTipoDeModal tipoModal)
        {

            switch (tipoModal)
            {
                case enumTipoDeModal.ModalDeSeleccion: return "modal-de-seleccion";
                case enumTipoDeModal.ModalDeRelacion: return "modal-de-relacion";
                case enumTipoDeModal.ModalParaImputar: return "modal-para-imputar";
                case enumTipoDeModal.ModalDeConsulta: return "modal-de-consulta";
                case enumTipoDeModal.ModalParaSeleccionar: return "modal-para-seleccionar";
                case enumTipoDeModal.ModalDeCrearRelacion: return "modal-de-crear-relacion";
                case enumTipoDeModal.ModalDeEditarRelacion: return "modal-de-editar-relacion";
                case enumTipoDeModal.ModalDeBorrado: return "modal-de-borrado";
                case enumTipoDeModal.ModalDeCreacion: return "modal-de-creacion";
                case enumTipoDeModal.ModalDeEdicion: return "modal-de-edicion";
                case enumTipoDeModal.ModalDeCorreo: return "modal-de-correo";
                case enumTipoDeModal.ModalDeExportacion: return "modal-de-exportacion";
                case enumTipoDeModal.ModalDeVisorDeArchivos: return "modal-de-visor-de-archivo";
                case enumTipoDeModal.ModalDeFirmaDeArchivos: return "modal-de-firma-de-archivo";
                case enumTipoDeModal.ModalDeDatosDeFirma: return "modal-de-datos-de-firma";
                case enumTipoDeModal.ModalDeDatosDeBloqueo: return "modal-de-datos-de-bloqueo";
                case enumTipoDeModal.ModalDeDatosDeDesBloqueo: return "modal-de-datos-de-desbloqueo";
                case enumTipoDeModal.ModalDeSeleccionarDestino: return "modal-de-seleccionar-destino";
                case enumTipoDeModal.ModalDeGenerarZip: return "modal-de-generar-zip";
                case enumTipoDeModal.ModalDeBloqueoMultiple: return "modal-de-bloqueo-multiple";
                case enumTipoDeModal.ModalDeDesbloqueoMultiple: return "modal-de-desbloqueo-multiple";
                case enumTipoDeModal.ModalDeCrearVinculo: return "modal-de-crear-vinculo";
                case enumTipoDeModal.ModalDeCrearDetalle: return "modal-de-crear-detalle";
                case enumTipoDeModal.ModalParaVincular: return "modal-para-vincular";
                case enumTipoDeModal.ModalParaPedirDatos: return "modal-para-pedir-datos";
                case enumTipoDeModal.ModalParaOcultarColumnas: return "modal-para-ocultar-columnas";
                case enumTipoDeModal.ModalDeTotales: return "modal-de-totales";
                case enumTipoDeModal.ModalMiCertificado: return "modal-mi-certificado";
                case enumTipoDeModal.ModalCambiarPassword: return "modal-cambiar-password";
                case enumTipoDeModal.ModalDeFiltrado: return "modal-de-filtrado";
                case enumTipoDeModal.ModalDeTransitar: return "modal-de-transitar";
                case enumTipoDeModal.ModalDeImprimir: return "modal-de-imprimir";
                case enumTipoDeModal.ModalDeMensaje: return "modal-de-mensaje";

            }
            throw new Exception($"No se ha definido como renderizar la modal {tipoModal}");
        }
    }

    public static class Css
    {
        public static string Render(this enumCssSelectorEnModal clase)
        {
            switch (clase)
            {
                case enumCssSelectorEnModal.Contenedor: return "selector-en-modal-contenedor";
                case enumCssSelectorEnModal.Editor: return "selector-en-modal-editor";
                case enumCssSelectorEnModal.botonSelector: return "selector-en-modal-boton-selector";
            }
            throw new Exception($"No se ha definido que renderizar para la clase {clase} para el selector en modal");

        }

        public static string Render(this enumCssDeTransitar clase)
        {
            switch (clase)
            {
                case enumCssDeTransitar.Cuerpo: return "transitar-cuerpo";
                case enumCssDeTransitar.ContenedorAnotacion: return "transitar-contenedor-anotacion";
            }
            throw new Exception($"No se ha definido que renderizar para la clase {clase} para un formulario de transitar");

        }

        public static string Render(this enumCssDeImprimir clase)
        {
            switch (clase)
            {
                case enumCssDeImprimir.Cuerpo: return "imprimir-cuerpo";
            }
            throw new Exception($"No se ha definido que renderizar para la clase {clase} para un formulario de impresion");

        }

        public static string Render(this enumCssEnviarCorreo clase)
        {
            switch (clase)
            {
                case enumCssEnviarCorreo.Contenedor: return "enviar-correo-contenedor";
                case enumCssEnviarCorreo.cabecera: return "enviar-correo-cabecera";
                case enumCssEnviarCorreo.cuerpo: return "enviar-correo-cuerpo";
                case enumCssEnviarCorreo.adjuntos: return "enviar-correo-adjuntos";
            }
            throw new Exception($"No se ha definido que renderizar para la clase {clase} para un formulario de envío de correo");

        }


        public static string Render(this enumCssExportacion clase)
        {
            switch (clase)
            {
                case enumCssExportacion.Contenedor: return "exportacion-contenedor";
                case enumCssExportacion.lista: return "exportacion-lista";
                case enumCssExportacion.sometido: return "exportacion-sometido";
                case enumCssExportacion.mostradas: return "exportacion-mostradas";
                case enumCssExportacion.enviar: return "exportacion-enviar";
                case enumCssExportacion.plantilla: return "exportacion-plantilla";
                case enumCssExportacion.destino: return "exportacion-destino";
            }
            throw new Exception($"No se ha definido que renderizar para la clase {clase} para un formulario de exportación");

        }


        public static string Render(this enumCssAgenda clase)
        {
            switch (clase)
            {
                case enumCssAgenda.EventoAjustadoAlContenedor: return "evento-ajustado-al-contenedor";
                case enumCssAgenda.AjustarTextoAlDiv: return "AjustarTextoAlDiv";
            }
            throw new Exception($"No se ha definido que renderizar para la clase {clase} del una agenda");
        }


        public static string Render(this enumCssControlesFormulario clase)
        {
            switch (clase)
            {
                case enumCssControlesFormulario.Editor: return "formulario-editor";
                case enumCssControlesFormulario.Lista: return "formulario-lista";
                case enumCssControlesFormulario.Check: return "formulario-check";
                case enumCssControlesFormulario.Archivo: return "formulario-archivo";
                case enumCssControlesFormulario.SelectorArchivo: return "formulario-selector-archivo";
                case enumCssControlesFormulario.InfoArchivo: return "formulario-visor-datos-archivo";
                case enumCssControlesFormulario.ContenedorOpcion: return "formulario-contenedor-opcion";
                case enumCssControlesFormulario.MenuCabecera: return "formulario-menu-cabecera";
                case enumCssControlesFormulario.MenuPie: return "formulario-menu-pie";
                case enumCssControlesFormulario.ContenedorBarra: return "formulario-contenedor-barra";
            }
            throw new Exception($"No se ha definido que renderizar para la clase {clase} para un formulario");
        }

        public static string Render(this enumCssFormulario clase)
        {
            switch (clase)
            {
                case enumCssFormulario.ContenedorDeBloquesApilados: return "formulario-contenedor-de-bloques-apilados";
                case enumCssFormulario.ContenedorDePrimerBloquesAnexados: return "formulario-contenedor-de-primer-bloque-anexados";
                case enumCssFormulario.ContenedorDeSiguientesBloquesAnexados: return "formulario-contenedor-de-siguientes-bloques-anexados";
                case enumCssFormulario.ContenedorEdicionDto: return "formulario-contenedor-edicion-dto";
                case enumCssFormulario.DatosDto: return "formulario-datos-dto";
                case enumCssFormulario.BloqueIzquierdo: return "formulario-bloque-izquierdo";
                case enumCssFormulario.BloqueDerecho: return "formulario-bloque-derecho";
                case enumCssFormulario.BloqueExpansor: return "formulario-contenedor-bloque-expansor";
                case enumCssFormulario.BloqueDatos: return "formulario-contenedor-bloque-datos";
                case enumCssFormulario.BloqueAnexado: return "formulario-contenedor-bloque-datos";
                case enumCssFormulario.Tabla: return "formulario-tabla";
                case enumCssFormulario.fila: return "formulario-fila";
                case enumCssFormulario.columnaControl: return "formulario-columna-control";
                case enumCssFormulario.columnaLabel: return "formulario-columna-label";
                case enumCssFormulario.referenciaExpansor: return "formulario-referencia";
                case enumCssFormulario.CabeceraFormularioMenu: return "formulario-cabecera-menu";
                case enumCssFormulario.CabeceraFormularioOpciones: return "formulario-cabecera-opciones";
                case enumCssFormulario.CabeceraFormularioFiltro: return "formulario-cabecera-filtro";
                case enumCssFormulario.ModalDeFiltroDeFormulario: return "formulario-modal-filtro";
            }
            throw new Exception($"No se ha definido que renderizar para la clase {clase} para un formulario");
        }


        public static string Render(this enumCssExpansor clase)
        {
            switch (clase)
            {
                case enumCssExpansor.Contenedor: return "grid-span";
                case enumCssExpansor.Pie: return "grid-span-pie";
                case enumCssExpansor.Cabecera: return "grid-span-cabecera";
                case enumCssExpansor.CuerpoSimple: return "grid-span-cuerpo-simple";
                case enumCssExpansor.CuerpoCompuesto: return "grid-span-cuerpo-compuesto";
                case enumCssExpansor.CuerpoDeDetalle: return "grid-span-cuerpo-detalle";
                case enumCssExpansor.CuerpoDeControles: return "grid-span-cuerpo-controles";
                case enumCssExpansor.ContenedorDeControl: return "grid-span-cuerpo-control";
                case enumCssExpansor.Expansor: return "span-expansor";
                case enumCssExpansor.GridDeRelacion: return "grid-de-relacion";
            }
            throw new Exception($"No se ha definido que renderizar para la clase {clase}");
        }

        public static string Render(this enumCssControles clase)
        {
            switch (clase)
            {
                case enumCssControles.ContenedorEditorConEtiquetaIzquierda: return "contenedor-editor-etiqueta-izquierda";
                case enumCssControles.ContenedorListaDeElementos: return "contenedor-listas";
                case enumCssControles.ContenedorListaDinamica: return "contenedor-listas";
                case enumCssControles.ContenedorEditor: return "contenedor-editor";
                case enumCssControles.ContenedorBotonSelector: return "contenedor-boton-selector";
                case enumCssControles.ContenedorEtiqueta: return "contenedor-etiqueta";
                case enumCssControles.ContenedorEtiquetaIzquierda: return "contenedor-etiqueta-izquierda";
                case enumCssControles.ContenedorArchivo: return "contenedor-de-un-archivo";
                case enumCssControles.ContenedorDeArchivos: return "contenedor-de-archivos";
                case enumCssControles.ContenedorReferencias: return "contenedor-referencias";
                case enumCssControles.ContenedorMenuDeReferencias: return "contenedor-menu-de-referencias";
                case enumCssControles.ContenedorCheck: return "contenedor-check";
                case enumCssControles.ContenedorCheckRight: return "contenedor-check-right";
                case enumCssControles.ContenedorFecha: return "contenedor-fecha";
                case enumCssControles.ContenedorFechaDerecha: return "contenedor-fecha-derecha";
                case enumCssControles.ContenedorFechaHora: return "contenedor-fecha-hora";
                case enumCssControles.ContenedorAreaDeTexto: return "contenedor-area-de-texto";
                case enumCssControles.ContenedorAreaDeTextoCentrado: return "contenedor-area-de-texto-centrada";
                case enumCssControles.SelectorDeFecha: return "fecha-dto";
                case enumCssControles.SelectorDeHora: return "hora-dto";
                case enumCssControles.CheckEnLinea: return "check-en-linea-dto";
                case enumCssControles.CheckApilado: return "check-apilado-dto";
                case enumCssControles.ControlApilado: return "control-apilado-dto";
                case enumCssControles.RefApilado: return "ref-apilado-dto";
                case enumCssControles.Selector: return "selector-dto";
                case enumCssControles.Editor: return "editor-dto";
                case enumCssControles.InfoDeTotales: return "info-de-totales";
                case enumCssControles.BotonSelector: return "boton-selector-dto";
                case enumCssControles.EditorRestrictor: return "form-control";
                case enumCssControles.RestrictorDeFiltro: return "form-control";
                case enumCssControles.EditorDeFiltro: return "form-control";
                case enumCssControles.Etiqueta: return "etiqueta-dto";
                case enumCssControles.EtiquetaDerecha: return "etiqueta-derecha-dto";
                case enumCssControles.ListaDinamica: return "lista-dinamica";
                case enumCssControles.AreaDeTexto: return "area-de-texto-dto";
                case enumCssControles.MonoSpaceText: return "monospace-text";
                case enumCssControles.ListaDeElementos: return "lista-de-elementos-dto";
                case enumCssControles.FormDeArchivo: return "form-archivo";
                case enumCssControles.BotonNavegacion: return "boton-de-navegacion";
                case enumCssControles.BotonCompartir: return "boton-de-compartir";
                case enumCssControles.TablaDeArchivo: return "tabla-archivo-subir";
                case enumCssControles.FilaDeArchivo: return "tr-archivo-subir";
                case enumCssControles.ColumnaDeArchivo: return "td-archivo-subir";
                case enumCssControles.SelectorDeImagen: return "selector-de-archivo";
                case enumCssControles.BarraAzulArchivo: return "barra-azul";
                case enumCssControles.ReferenciaAlineadaAlFinal: return "referencia-alineada-al-final";
                case enumCssControles.BotonComoReferencia: return "boton-como-referencia";
                case enumCssControles.ReferenciaDeMenu: return "referencia-de-menu";
                case enumCssControles.DivEnBlanco: return "div-en-blanco";
                case enumCssControles.DivVisible: return "div-visible";
                case enumCssControles.DivNoVisible: return "div-no-visible";
                case enumCssControles.ArchivosSeleccionados: return "archivos-seleccionados";
                case enumCssControles.SelectorDeArchivos: return "selector-de-archivos";
                case enumCssControles.SelectorDeUnArchivo: return "selector-de-un-archivo";
                case enumCssControles.ImagenEnBoton: return "imagen-en-boton";
                case enumCssControles.ContenedorDeOpcion: return "contenedor-de-opcion";
                case enumCssControles.ContenedorDeOpcionDerecha: return "contenedor-de-opcion-derecha";
                case enumCssControles.FiltroSelectorDeArchivos: return "filtro-selector-archivos";
                case enumCssControles.LinksOpcionesSelectorDeArchivos: return "links-opciones-selector-archivos";
                case enumCssControles.PosicionAbsoluta: return "posicion-absoluta";
                case enumCssControles.ReferenciaCentrada: return "referencia-centrada";
                case enumCssControles.ReferenciaCentradaEnTd: return "referencia-centrada-en-td";
                case enumCssControles.ReferenciaAbrirFiltro: return "referecia-abrir-filtro";
                case enumCssControles.Oculto: return "control-oculto";
                case enumCssControles.EntreImportesMostrar: return "entre-importes-mostrar";
                case enumCssControles.EntreFechasMostrar: return "entre-fechas-mostrar";
                case enumCssControles.ListaValoresMostrar: return "lista-valores-mostrar";
                case enumCssControles.DescargarArchivo: return "descargar-anexado";
                case enumCssControles.ProcesarConIa: return "procesar-con-ia";
                case enumCssControles.PasarOcr: return "pasar-ocr";
                case enumCssControles.CompartirConWhatsApp: return "compartir-con-whatsapp";
                case enumCssControles.CompartirConGuid: return "compartir-con-guid";
                case enumCssControles.ConsultarConGuid: return "consultar-con-guid";
                case enumCssControles.EnviarCorreo: return "enviar-correo";
                case enumCssControles.NavegacionImagenes: return "navegacion-imagenes";
                case enumCssControles.NavegacionZoom: return "navegacion-zomm";
            }
            throw new Exception($"No se ha definido que renderizar para la clase {clase}");
        }

        public static string Render(this enumCssFiltro clase)
        {
            switch (clase)
            {
                case enumCssFiltro.ColumnaFiltro: return "columna-filtro";
                case enumCssFiltro.ListaDinamica: return "lista-dinamica";
                case enumCssFiltro.ListaDeElementos: return "lista-de-elementos";
                case enumCssFiltro.ListaDeMenu: return "lista-de-menu";
                case enumCssFiltro.ContenedorListaDinamica: return "contenedor-listas-filtro";
                case enumCssFiltro.ContenedorDeReferenciasParaAbrirModales: return "contenedor-de-referencias-para-abrir-modales";
                case enumCssFiltro.ContenedorListaDeElementos: return "contenedor-listas-filtro";
                case enumCssFiltro.OpcionesMenuDeCreacion: return "opciones-menu-creacion";
                case enumCssFiltro.ContenedorCheck: return "contenedor-check";
                case enumCssFiltro.ContenedorEditor: return "contenedor-editor-filtro";
                case enumCssFiltro.ContenedorSelector: return "contenedor-selector-filtro";
                case enumCssFiltro.ContenedorEntreFechas: return "contenedor-entre-fechas-filtro";
                case enumCssFiltro.ContenedorEntreRangos: return "contenedor-entre-rangos-filtro";
                case enumCssFiltro.ContenedorEntreImportes: return "contenedor-entre-importes-filtro";
                case enumCssFiltro.ContenedorEntreFechasModal: return "contenedor-entre-fechas-filtro-modal";
                case enumCssFiltro.ContenedorEntreRangosModal: return "contenedor-entre-rangos-filtro-modal";
                case enumCssFiltro.ContenedorListaDinamicaConChek: return "contenedor-lista-dinamica-con-check";
                case enumCssFiltro.ContenedorEntreImportesMostrarModal: return "contenedor-entre-importes-mostrar-filtro-modal";
                case enumCssFiltro.ContenedorEntreFechasMostrarModal: return "contenedor-entre-fechas-mostrar-filtro-modal";
                case enumCssFiltro.ContenedorListaDeElementosMostrarModal: return "contenedor-lista-mostrar-filtro-modal";
                case enumCssFiltro.ContenedorDeDosControlesEnCelda: return "contenedor-de-dos-controles-en-una-celda";
                case enumCssFiltro.ContenedorEntreImportesModal: return "contenedor-entre-importes-filtro-modal";
                case enumCssFiltro.ContenedorDeRelacionModal: return "contenedor-de-relacion-filtro-modal";
                case enumCssFiltro.ContenedorDireccion: return "contenedor-direccion-filtro";
                case enumCssFiltro.ContenedorPreasientos: return "contenedor-datos-preasiento-filtro";
                case enumCssFiltro.ContenedorConDosListasModal: return "contenedor-con-listas-filtro-modal";
                case enumCssFiltro.ContenedorDelTipoRelacionadoModal: return "contenedor-del-tipo-relacionado-filtro-modal";
                case enumCssFiltro.ContenedorEnModalDeFiltros: return "contenedor-en-modal-de-filtros";
                case enumCssFiltro.EtiquetaEntreFechas: return "etiqueta-entre-fechas";
                case enumCssFiltro.EtiquetaEntreImportes: return "etiqueta-entre-importes";
                case enumCssFiltro.EtiquetaEntreRangos: return "etiqueta-entre-rangos";
                case enumCssFiltro.EtiquetaConEditor: return "etiqueta-con-editor";
                case enumCssFiltro.Check: return "check-flt";
                case enumCssFiltro.CheckOn: return "check-on-flt";
                case enumCssFiltro.CheckOff: return "check-off-flt";
                case enumCssFiltro.ControlApilado: return "control-apilado-dto";
                case enumCssFiltro.Hora: return "hora-flt";
                case enumCssFiltro.Fecha: return "fecha-flt";
                case enumCssFiltro.Rango: return "rango-flt";
                case enumCssFiltro.Numero: return "numero-flt";
                case enumCssFiltro.Importe: return "importe-flt";
                case enumCssFiltro.FilaFiltro: return "fila-filtro";
                case enumCssFiltro.FilaFiltroSinSpan: return "fila-filtro-sin-span";
            }
            throw new Exception($"No se ha definido que renderizar para la clase {clase}");
        }


        public static string Render(this enumCssOrdenacion clase)
        {
            switch (clase)
            {
                case enumCssOrdenacion.SinOrden: return "ordenada-sin-orden";
                case enumCssOrdenacion.Ascendente: return "ordenada-ascendente";
                case enumCssOrdenacion.Descendente: return "ordenada-desscendente";
            }
            throw new Exception($"No se ha definido que renderizar para la clase {clase}");
        }

        public static string Render(this enumCssImportant clase)
        {
            switch (clase)
            {
                case enumCssImportant.SinMarginTop: return "important-sin-magin-top";
            }
            throw new Exception($"No se ha definido que renderizar para la clase {clase}");
        }


        public static string Render(this enumCssHistorial clase)
        {
            switch (clase)
            {
                case enumCssHistorial.CuerpoHistorial: return "cuerpo-historial";
                case enumCssHistorial.ContenedorCabecera: return "contenedor-historial-cabecera";
                case enumCssHistorial.ContenedorCuerpo: return "contenedor-historial-cuerpo";
                case enumCssHistorial.ContenedorPie: return "contenedor-historial-pie";
                case enumCssHistorial.ContenedorMenu: return "contenedor-historial-menu";
                case enumCssHistorial.ZonaMenu: return "zona-historial-menu";
                case enumCssHistorial.Titulo: return "titulo-historial";
                case enumCssHistorial.Proceso: return "proceso-historial";
                case enumCssHistorial.FiltroExpansor: return "div-historial-filtro-expansor";

                case enumCssHistorial.MenuMfHistorial: return "menu-historial";
            }
            throw new Exception($"No se ha definido que renderizar para la clase {clase}");
        }

        public static string Render(this enumCssCreacion clase)
        {
            switch (clase)
            {
                case enumCssCreacion.TablaDeCreacion: return "tabla-creacion";
                case enumCssCreacion.CuerpoDeCrearcion: return "cuerpo-creacion";
                case enumCssCreacion.EditarTrasCrear: return "editar-tras-crear";
                case enumCssCreacion.ContenedorPieOpciones: return "contenedor-pie-opciones";
                case enumCssCreacion.ContenedorPieModalOpciones: return "contenedor-pie-modal-opciones";
                case enumCssCreacion.ContenedorDeCreacioCuerpo: return "contenedor-creacion-cuerpo-datos-visor";
                case enumCssCreacion.ContenedorDeCreacioDatos: return "contenedor-edicion-editor-datos";
                case enumCssCreacion.ContenedorDeCreacioVisor: return "contenedor-edicion-editor-visor";
                case enumCssCreacion.VisorDeCreacion: return "visor-de-edicion";
                case enumCssCreacion.NavegadorDeCreacion: return "navegador-edicion-editor-visor";
                case enumCssCreacion.VisorNombreAnexado: return "visor-nombre-archivo";
                case enumCssCreacion.VisorOculto: return "visor-oculto";
            }
            throw new Exception($"No se ha definido que renderizar para la clase {clase}");
        }

        public static string Render(this enumCssEdicion clase)
        {
            switch (clase)
            {
                case enumCssEdicion.TablaDeEdicion: return "tabla-edicion";
                case enumCssEdicion.CuerpoDeEdicion: return "cuerpo-edicion";
                case enumCssEdicion.CuerpoDeEdicionSoloConsulta: return "cuerpo-edicion-solo-consulta";
                case enumCssEdicion.ContenedorDeEdicionCabecera: return "contenedor-edicion-cabecera";
                case enumCssEdicion.ContenedorDeEdicionCabeceraSoloConsulta: return "contenedor-edicion-cabecera-solo-consulta";
                case enumCssEdicion.ContenedorDeEdicionCuerpo: return "contenedor-edicion-cuerpo";
                case enumCssEdicion.ContenedorDeEdicionEditor: return "contenedor-edicion-editor";
                case enumCssEdicion.VisorOculto: return "visor-oculto";
                case enumCssEdicion.ContenedorDeEdicionCuerpoDatos: return "contenedor-edicion-editor-datos";
                case enumCssEdicion.ContenedorDeEdicionCuerpoVisor: return "contenedor-edicion-editor-visor";
                case enumCssEdicion.ContenedorDeEdicionCuerpoHistorial: return "contenedor-edicion-editor-historial";
                case enumCssEdicion.ContenedorDelVisorDeArchivoConHistorial: return "contenedor-del-visor-de-archivo-con-historial";
                case enumCssEdicion.NavegadorDeEdicionCuerpoVisor: return "navegador-edicion-editor-visor";
                case enumCssEdicion.VisorNombreAnexado: return "visor-nombre-archivo";
                case enumCssEdicion.VisorDeEdicion: return "visor-de-edicion";
                case enumCssEdicion.VisorDeHistorial: return "visor-edicion-historial";
                case enumCssEdicion.MenuDeHistorial: return "menu-edicion-historial";
                case enumCssEdicion.ContenedorDeEdicionPie: return "contenedor-edicion-pie";
                case enumCssEdicion.ContenedorDeEdicionPieSoloLectura: return "contenedor-edicion-pie-solo-lectura";
                case enumCssEdicion.CuerpoDeTablaDeEdicion: return "cuerpo-tabla-edicion";
                case enumCssEdicion.ContenedorId: return "contenedor-id";
                case enumCssEdicion.Titulo: return "contenedor-titulo";
                case enumCssEdicion.MenuDelProceso: return "menu-del-proceso";
                case enumCssEdicion.AccionesDelPanelDeEdicion: return "acciones-del-panel-edicion";
                case enumCssEdicion.MenuIndividual: return "menu-individual";
                case enumCssEdicion.DivVacioDeLaDerecha: return "div-vacio-derecha";
                case enumCssEdicion.DivNulo: return "div-nulo";
                case enumCssEdicion.Ampliacion: return "ampliacion-dto";
                case enumCssEdicion.Detalle: return "detalle-dto";
                case enumCssEdicion.PieDeDetalle: return "pie-detalle-dto";
                case enumCssEdicion.ContenedorDeAmpliacion: return "contenedro-ampliacion-dto";
                case enumCssEdicion.AbrirImagenDeDetalle: return "img-detalle-dto-abrir";
                case enumCssEdicion.CerrarImagenDeDetalle: return "img-detalle-dto-cerrar";
                case enumCssEdicion.TituloDeDetalle: return "titulo-detalle-dto";
                case enumCssEdicion.AbrirImagenDeAmpliacion: return "img-ampliacion-dto-abrir";
                case enumCssEdicion.CerrarImagenDeAmpliacion: return "img-ampliacion-dto-cerrar";
                case enumCssEdicion.TituloDeAmpliacion: return "titulo-ampliacion-dto";
                case enumCssEdicion.DatosPrincipales: return "datos-principales-dto";
                case enumCssEdicion.ContenedorDatosPrincipales: return "contenedor-datos-principales";
                case enumCssEdicion.Tbody: return "dto-tbody";
                case enumCssEdicion.Tr: return "dto-tr";
                case enumCssEdicion.Td: return "dto-td";
            }
            throw new Exception($"No se ha definido que renderizar para la clase {clase}");
        }

        public static string Render(this enumCssMnt clase)
        {
            switch (clase)
            {
                case enumCssMnt.MntMenuContenedor: return "div-mnt-menu-contenedor";
                case enumCssMnt.MntMenuZona: return "div-mnt-menu-zona";
                case enumCssMnt.MntFiltroExpansor: return "div-mnt-filtro-expansor";
                case enumCssMnt.MntFiltroBloqueContenedor: return "div-mnt-bloque-contenedor";
                case enumCssMnt.MntFiltroBloqueVacio: return "div-mnt-bloque-vacio";
                case enumCssMnt.MntTablaDeFiltro: return "tabla-filtro";
                case enumCssMnt.MenuDelProceso: return "menu-del-proceso";
                case enumCssMnt.MenuIndividual: return "menu-individual";
                case enumCssMnt.MenuContextual: return "menu-contextual";
                case enumCssMnt.MenuDeRelaciones: return "menu-de-relaciones";
                case enumCssMnt.MenuDeFiltro: return "menu-de-filtro";
                case enumCssMnt.MenuDeDetalle: return "menu-de-detalle";
                case enumCssMnt.MenuDeDetalleOculto: return "menu-de-detalle-oculto";
                case enumCssMnt.MenuDeDetalleVisible: return "menu-de-detalle-visible";

                case enumCssMnt.MenuFormulario: return "menu-formulario";
                case enumCssMnt.DivVacioDeLaDerecha: return "div-vacio-derecha";
                case enumCssMnt.DivNulo: return "div-nulo";
            }
            throw new Exception($"No se ha definido que renderizar para la clase {clase}");
        }

        public static string Render(this enumCssModal clase)
        {
            switch (clase)
            {
                case enumCssModal.ContenidoModal: return "contenido-modal";
                case enumCssModal.ContenidoModalConCabecera: return "contenido-modal-cabecera";
                case enumCssModal.EstiloModalConCabecera: return "style= ¨grid-template-rows: 2.5em auto 2.5em; max-height: fit-content;¨";
                case enumCssModal.ContenidoCabecera: return "contenido-cabecera";
                case enumCssModal.CabeceraRelacionarElementos: return "cabecera-relacionar-elementos";
                case enumCssModal.CabeceraParaImputar: return "cabecera-para-imputar";
                case enumCssModal.ContenidoCuerpo: return "contenido-cuerpo";
                case enumCssModal.ContenidoCuerpoConGrid: return "contenido-cuerpo-con-grid";
                case enumCssModal.ContenedorCuerpoConGrid: return "contenedor-cuerpo-con-grid";

                case enumCssModal.EstiloContenidoCuerpo: return "style = ¨height: auto; overflow: hidden;¨";
                case enumCssModal.ContenidoPie: return "contenido-pie";
                case enumCssModal.PieDeTotales: return "pie-de-totales";

            }
            throw new Exception($"No se ha definido que renderizar para la clase {clase}");
        }

        public static string Render(enumCssNavegadorEnModal clase)
        {
            switch (clase)
            {
                case enumCssNavegadorEnModal.Contenedor: return "pie-grid";
                case enumCssNavegadorEnModal.Cantidad: return "navegador-cantidad-grid";
                case enumCssNavegadorEnModal.Opcion: return "pie-grid-opciones";
                case enumCssNavegadorEnModal.Mensaje: return "pie-grid-mensaje";
                case enumCssNavegadorEnModal.InfoGrid: return "pie-grid-info";
                case enumCssNavegadorEnModal.Navegador: return "pie-grid-navegador";
            }
            throw new Exception($"No se ha definido que renderizar para la clase {clase}");
        }


        public static string Render(enumCssNavegadorEnMnt clase)
        {
            switch (clase)
            {
                case enumCssNavegadorEnMnt.Cantidad: return "navegador-cantidad-grid";
                case enumCssNavegadorEnMnt.Opcion: return "cuerpo-pie-opciones";
                case enumCssNavegadorEnMnt.Mensaje: return "cuerpo-pie-mensaje";
                case enumCssNavegadorEnMnt.InfoGrid: return "cuerpo-pie-info";
                case enumCssNavegadorEnMnt.Navegador: return "cuerpo-pie-navegador";
            }
            throw new Exception($"No se ha definido que renderizar para la clase {clase}");
        }

        public static string Render(this enumCssGrid clase)
        {
            switch (clase)
            {
                case enumCssGrid.ColumnaCabecera: return "columna-cabecera";
                case enumCssGrid.ColumnaPosicionableIzquierda: return "desplazar-izquierda";
                case enumCssGrid.ColumnaPosicionableDerecha: return "desplazar-derecha";
                case enumCssGrid.ContenedorDesplazador: return "contenedor-desplazador";
                case enumCssGrid.ContenedorTamano: return "contenedor-tamano";
                case enumCssGrid.ContenedorVisualizadorColumna: return "contenedor-visualizador-columnas";
                case enumCssGrid.ColumnaOculta: return "columna-oculta";
                case enumCssGrid.ColumnaAccion: return "columna-accion";
                case enumCssGrid.ColumnaAlineadaDerecha: return "columna-alineacion-derecha";
                case enumCssGrid.ColumnaDiv: return "div-de-columna";
                case enumCssGrid.OcultarColumna: return "ocultar-columna";
                case enumCssGrid.OrdenarColumna: return "ordenar-columna";
                case enumCssGrid.TransitarElementos: return "transitar-elementos";
                case enumCssGrid.OrdenarColumnaNoPermitido: return "ordenar-no-permitido";
                case enumCssGrid.OcultarColumnaIzquierda: return "ocultar-columna-izquierda";
                case enumCssGrid.OcultarColumnaDerecha: return "ocultar-columna-derecha";
                case enumCssGrid.OcultarColumnaCentrado: return "ocultar-columna-centrado";
                case enumCssGrid.MostrarColumnas: return "mostrar-columnas";
                case enumCssGrid.AumentarTamanoColumna: return "modificar-tamano-columna";
                case enumCssGrid.ReducirTamanoColumnas: return "reducir-tamano-columnas";
                case enumCssGrid.Nulo: return "";
                case enumCssGrid.ContenedorDeLaTablaConGraficos: return "contenedor-tabla-con-graficos";
                case enumCssGrid.ContenedorDelGridConElDivDeLaTabla: return "div-grid-tabla";
                case enumCssGrid.ContenedorDelGridConElDivDeGraficos: return "div-graficos";
                case enumCssGrid.Splitter: return "splitter-tabla";
            }
            throw new Exception($"No se ha definido que renderizar para la clase {clase}");
        }

        public static string Render(this enumCssCuerpo clase)
        {
            switch (clase)
            {
                case enumCssCuerpo.Cuerpo: return "cuerpo";
                case enumCssCuerpo.CuerpoSoloConGrid: return "cuerpo-solo-con-grid";
                case enumCssCuerpo.CuerpoSoloConsulta: return "cuerpo-solo-consulta";
                case enumCssCuerpo.CuerpoCabecera: return "cuerpo-cabecera";
                case enumCssCuerpo.CuerpoDatos: return "cuerpo-datos";
                case enumCssCuerpo.CuerpoDatosFiltro: return "cuerpo-datos-filtro";
                case enumCssCuerpo.CuerpoDatosFiltroBloque: return "cuerpo-datos-filtro-bloque";
                case enumCssCuerpo.CuerpoDatosFiltroReferencias: return "cuerpo-datos-filtro-referencias";
                case enumCssCuerpo.CuerpoDatosGrid: return "cuerpo-datos-grid";
                case enumCssCuerpo.CuerpoDatosGridTboby: return "cuerpo-datos-tbody";
                case enumCssCuerpo.CuerpoDatosGridThead: return "cuerpo-datos-thead";
                case enumCssCuerpo.CuerpoPie: return "cuerpo-pie";
                case enumCssCuerpo.CuerpoDatosFormulario: return "cuerpo-datos-formulario";
                case enumCssCuerpo.CuerpoPieFormulario: return "cuerpo-pie-formulario";
                case enumCssCuerpo.CuerpoCabeceraFormulario: return "cuerpo-cabecera-formulario";
            }
            throw new Exception($"No se ha definido que renderizar para la clase {clase}");
        }

        public static string Render(this enumCssDiv clase)
        {
            switch (clase)
            {
                case enumCssDiv.DivVisible: return enumCssControles.DivVisible.Render();
                case enumCssDiv.DivOculto: return enumCssControles.DivNoVisible.Render();
                case enumCssDiv.DivConMasPropiedades: return "div-con-mas-propiedades";
                case enumCssDiv.DivConConteidoAlineadoALaDerecha: return "div-con-contenido-en-derecha";
                case enumCssDiv.Nulo: return "";
                case enumCssDiv.SeparadorTop10px: return "separador-top-10px";
                case enumCssDiv.SeparadorTop50Left30px: return "separador-top-50px-left-30px";
                case enumCssDiv.Tabla: return "div-tabla";
                case enumCssDiv.Thead: return "div-thead";
                case enumCssDiv.Tbody: return "div-tbody";
                case enumCssDiv.Th: return "div-th";
                case enumCssDiv.Tr: return "div-tr";
                case enumCssDiv.Td: return "div-td";
                case enumCssDiv.CuerpoDatosTabla: return "cuerpo-datos-tabla";

            }
            throw new Exception($"No se ha definido que renderizar para la clase {clase}");
        }

        public static string Render(this enumCssBootStrap clase)
        {
            switch (clase)
            {
                case enumCssBootStrap.table: return "table";
                case enumCssBootStrap.tableStriped: return "table-striped";

            }
            throw new Exception($"No se ha definido que renderizar para la clase {clase}");
        }

        public static string Render(this enumCssOpcionMenu clase)
        {
            switch (clase)
            {
                case enumCssOpcionMenu.DeElemento: return "opcion-menu-de-elemento";
                case enumCssOpcionMenu.DeVista: return "opcion-menu-de-vista";
                case enumCssOpcionMenu.Basico: return "opcion-menu-basica";
                case enumCssOpcionMenu.BotonPorDefecto: return "boton-por-defecto";
                case enumCssOpcionMenu.BotonesDeMenu: return "botones-de-menu";
            }
            throw new Exception($"No se ha definido que renderizar para la clase {clase}");
        }
    }

    public enum enumExtensiones { pdf, docx, xml, html, xlsx, png, jpg, jpeg, svg, se, txt, json, rtf, zip, csv, p12, pfx, cer, webp, fit }

    public static class ExtensorDeTipoDeArchivos
    {
        public const string Imagenes = ".png, .jpg, .jpeg, .svg .webp";

        public const string NoEditables = ".pdf, " + Imagenes;

        public const string NoEditablesMaJson = ".pdf, .json, " + Imagenes;

        public const string Certificados = ".cer, .pfx, .p12" ;

        public static bool EsImagen(string extensionstring, bool errorSiNoEstaCatalogada)
        {
            var extension = IntentarParsear(extensionstring);

            if (errorSiNoEstaCatalogada && extension == null)
            {
               throw new Exception("La extensión '" + extensionstring + "' no es una extensión de archivo válida");
            }

            return extension.HasValue && (extension == enumExtensiones.png ||
                extension == enumExtensiones.jpg || 
                extension == enumExtensiones.jpeg ||
                extension == enumExtensiones.svg || 
                extension == enumExtensiones.webp);
        }

        public static bool EsSvg(string extensionstring, bool errorSiNoEstaCatalogada)
        {
            var extension = IntentarParsear(extensionstring);
            if (errorSiNoEstaCatalogada && extension == null)
            {
                throw new Exception("La extensión '" + extensionstring + "' no es una extensión de archivo válida");
            }
            return extension.HasValue && extension == enumExtensiones.svg ;
        }
        public static string Render(this enumExtensiones extension) => $".{extension}";

        public static bool EsHtml(string extension)
        {
            return Parser(extension, defecto: enumExtensiones.txt) == enumExtensiones.html;
        }

        public static enumExtensiones Parser(string extension, enumExtensiones? defecto = null)
        {
            var enumExt = ApiDeEnsamblados.ToEnumerado<enumExtensiones>(extension.Replace(".", ""), errorSiNoEsValido: false);
            if (enumExt == null && defecto == null)
                throw new Exception("La extensión '" + extension + "' no es una extensión de archivo válida");
            if (enumExt == null)
                enumExt = defecto;
            return enumExt.Value;
        }


        public static enumExtensiones? IntentarParsear(string extension)
        {
            var enumExt = ApiDeEnsamblados.ToEnumerado<enumExtensiones>(extension.Replace(".", ""), errorSiNoEsValido: false);
            if (enumExt == null)
                return null;
            return enumExt.Value;
        }
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
}