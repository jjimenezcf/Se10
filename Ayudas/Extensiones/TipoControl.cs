using System;

namespace Utilidades
{
    public enum enumInputType { text, file, check };

    public static class InputTypeExtension
    {
        public static string Render(this enumInputType tipo)
        {
            switch (tipo)
            {
                case enumInputType.file: return "file";
                case enumInputType.check: return "check";
                case enumInputType.text: return "text";
            }

            throw new Exception($"No se ha definido como renderizar el tipo de input {tipo}");
        }
    }

    public enum enumTipoControl
    {     SelectorDeFiltro
        , SelectorDeElemento
        , SelectorDeArchivos
        , SelectorDeUnArchivo
        , ListaDeElemento
        , ListaDeValores
        , ListaDeMenu
        , Enumerado
        , ListaDinamica
        , Navegador
        , BotonAccion
        , Editor
        , RestrictorDeFiltro
        , RestrictorDeEdicion
        , Archivo
        , Check
        , UrlDeArchivo
        , VisorDeArchivo
        , ImagenDelCanvas
        , DesplegableDeFiltro
        , GridModal
        , TablaBloque
        , Bloque
        , Ampliacion
        , ZonaDeMenu
        , ZonaDeDatos
        , ZonaDeFiltro
        , Menu
        , VistaCrud
        , DescriptorDeCrud
        , Opcion
        , Label
        , Referencia
        , ReferenciaPost
        , Lista
        , SelectorDeFecha
        , SelectorDeFechaHora
        , AreaDeTexto
        , Plantilla
        , Mantenimiento
        , Historial
        , pnlCreador
        , pnlEditor
        , pnlBorrado
        , ModalDeMensaje
        , ModalDeRelacion
        , ModalParaImputar
        , ModalDeConsulta
        , ModalDeSeleccion
        , FiltroEntreFechas
        , pnlExportacion
        , pnlEnviarCorreo
        , pnlTransitar
        , pnlImprimir
        , GridDeDetalle
        , ModalDeCreacionRelacion
        , ModalDeCrearVinculo
        , ModalDeCrearDetalle
        , ModalParaVincular
        , ModalParaPedirDatos
        , ModalParaOcultarColumnas
        , ModalDeTotales
        , ModalDeEditarRelacion
        , ModalDeConsultaDto
        , ModalDeFiltrado
        , Password
        , FiltroEntreImportes
        , FiltroEntreRangos
        , FiltroDeRelacion
        , FiltroDeTipos
        , ListaDinamicaParaMostrarColumna
        , FiltroEntreImportesMostrarColumna
        , FiltroEntreFechasMostrarColumna
        , FiltroConListasDinamicas
        , FiltroConEditor
        , FiltroDeListaValoresConEditor
        , CirculoEnCelda
        , AgrupadorDeListaDeValores
    }

    public static class TipoControlExtension
    {

        public const int BytePermitidosEnZip = 524288000;
        public const int BytePermitidosNormal = 104857600;
        public static string Render(this enumTipoControl tipo) {

            switch(tipo)
            {
                case enumTipoControl.Editor: return "editor";
                case enumTipoControl.Password: return "editor";
                case enumTipoControl.Check: return "check";
                case enumTipoControl.SelectorDeFiltro: return "selector";
                case enumTipoControl.SelectorDeElemento: return "selector-de-elemento";
                case enumTipoControl.SelectorDeArchivos: return "selector-de-archivos";
                case enumTipoControl.SelectorDeUnArchivo: return "selector-de-un-archivo";
                case enumTipoControl.ListaDeElemento: return "lista-de-elemento";
                case enumTipoControl.ListaDinamica: return "lista-dinamica";
                case enumTipoControl.Navegador: return "link";
                case enumTipoControl.BotonAccion: return "boton-accion";
                case enumTipoControl.ListaDeValores: return "lista-de-valores";
                case enumTipoControl.ListaDeMenu: return "lista-de-menu";
                case enumTipoControl.Enumerado: return "lista-de-valores";
                case enumTipoControl.RestrictorDeFiltro: return "restrictor-filtro";
                case enumTipoControl.RestrictorDeEdicion: return "restrictor-edicion";
                case enumTipoControl.Archivo: return "archivo";
                case enumTipoControl.UrlDeArchivo: return "url-archivo";
                case enumTipoControl.VisorDeArchivo: return "visor-archivo";
                case enumTipoControl.SelectorDeFecha: return "selector-de-fecha";
                case enumTipoControl.SelectorDeFechaHora: return "selector-de-fecha-hora";
                case enumTipoControl.FiltroEntreFechas: return "filtro-entre-fechas";
                case enumTipoControl.FiltroEntreImportes: return "filtro-entre-importes";
                case enumTipoControl.FiltroEntreRangos: return "filtro-entre-rangos";
                case enumTipoControl.FiltroDeRelacion: return "filtro-de-relacion";
                case enumTipoControl.FiltroDeTipos: return "filtro-de-tipos";
                case enumTipoControl.ListaDinamicaParaMostrarColumna: return "lista-para-mostrar";
                case enumTipoControl.AgrupadorDeListaDeValores: return "agrupador-de-lista-para-mostrar";
                case enumTipoControl.FiltroEntreImportesMostrarColumna: return "filtro-entre-importes-para-mostrar";
                case enumTipoControl.FiltroEntreFechasMostrarColumna: return "filtro-entre-fechas-para-mostrar";
                case enumTipoControl.FiltroConListasDinamicas: return "filtro-con-listasD";
                case enumTipoControl.FiltroConEditor: return "filtro-con-editor";
                case enumTipoControl.FiltroDeListaValoresConEditor: return "filtro-con-lv-editor";
                case enumTipoControl.CirculoEnCelda: return "circulo-en-celda";
                case enumTipoControl.AreaDeTexto: return "area-de-texto";
                case enumTipoControl.Opcion: return "opcion";
                case enumTipoControl.GridDeDetalle: return "grid-de-detalle";
                case enumTipoControl.ModalDeCreacionRelacion: return "modal-de-creacion-relacion";
                case enumTipoControl.ModalDeEditarRelacion: return "modal-de-editar-relacion";
                case enumTipoControl.DesplegableDeFiltro: return "desplegable-de-filtro";
                case enumTipoControl.GridModal: return "grid-modal";
                case enumTipoControl.TablaBloque: return "tabla-bloque";
                case enumTipoControl.Bloque: return "bloque";
                case enumTipoControl.ZonaDeMenu: return "zona-menu";
                case enumTipoControl.ZonaDeDatos: return "zona-de-datos";
                case enumTipoControl.ZonaDeFiltro: return "zona-de-filtro";
                case enumTipoControl.Menu: return "menu";
                case enumTipoControl.VistaCrud: return "vista-crud";
                case enumTipoControl.DescriptorDeCrud: return "descriptor-crud";
                case enumTipoControl.Label: return "label";
                case enumTipoControl.Referencia: return "referencia";
                case enumTipoControl.ReferenciaPost: return "referencia-post";
                case enumTipoControl.Lista: return "lista";
                case enumTipoControl.Plantilla: return "plantilla";
                case enumTipoControl.Mantenimiento: return "mantenimiento";
                case enumTipoControl.Historial: return "historial";
                case enumTipoControl.pnlCreador: return "panel-creador";
                case enumTipoControl.pnlEditor: return "panel-editor";
                case enumTipoControl.pnlExportacion: return "panel-exportacion";
                case enumTipoControl.pnlEnviarCorreo: return "panel-enviar-correo";
                case enumTipoControl.pnlTransitar: return "panel-transitar";
                case enumTipoControl.pnlImprimir: return "panel-imprimir";
                case enumTipoControl.pnlBorrado: return "panel-borrado";
                case enumTipoControl.ModalDeRelacion: return "modal-de-relacion";
                case enumTipoControl.ModalDeMensaje: return "modal-de-mensaje";
                case enumTipoControl.ModalParaImputar: return "modal-para-imputar";
                case enumTipoControl.ModalDeConsulta: return "modal-de-consulta";
                case enumTipoControl.ModalDeSeleccion: return "modal-de-seleccion";
                case enumTipoControl.ImagenDelCanvas: return "imagen-de-canva";
                case enumTipoControl.Ampliacion: return "ampliacion-de-elemento";
                case enumTipoControl.ModalDeFiltrado: return "modal-de-filtrado";
            }
            throw new Exception($"El tipo de control '{tipo}' no está definido como renderizarlo");
    }
}

}
