using System;
using System.Collections.Generic;
using System.Text;

namespace Utilidades
{
    public static class ExtensorDeCss
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
                case enumTipoDeModal.ModalMiUsuario: return "modal-mi-usuario";
                case enumTipoDeModal.ModalDeFiltrado: return "modal-de-filtrado";
                case enumTipoDeModal.ModalDeTransitar: return "modal-de-transitar";
                case enumTipoDeModal.ModalDeImprimir: return "modal-de-imprimir";
                case enumTipoDeModal.ModalDeMensaje: return "modal-de-mensaje";

            }
            throw new Exception($"No se ha definido como renderizar la modal {tipoModal}");
        }

        public const string SemaforoEnGrid = "semaforo-grid";

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
                case enumCssControles.ConvertirAPdf: return "convertir-a-pdf";
                case enumCssControles.CompartirConGuid: return "compartir-con-guid";
                case enumCssControles.AlternarVistaFichas: return "alternar-vista-fichas";
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
                case enumCssFiltro.OpcionesMenuDeCreacionMovil: return "opciones-menu-creacion-movil";
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
                case enumCssMnt.MenuDeDetalleInvisible: return "menu-de-detalle-invisible";
                case enumCssMnt.MenuDeMostrarMapa: return "menu-de-mostrar-mapa";
                case enumCssMnt.MenuDeMostrarDireccion: return "menu-de-mostrar-direccion";
                case enumCssMnt.MenuDeMostrarDireccionInvisible: return "menu-de-mostrar-direccion-invisible";



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
                case enumCssModal.ContenidoModalConCabecera: return "contenido-modal-cabecera modal-con-grid";
                case enumCssModal.EstiloModalConCabecera: return "style= ¨grid-template-rows: 2.5em auto 2.5em; max-height: fit-content;¨";
                case enumCssModal.ContenidoCabecera: return "contenido-cabecera";
                case enumCssModal.CabeceraRelacionarElementos: return "cabecera-relacionar-elementos";
                case enumCssModal.CabeceraParaImputar: return "cabecera-para-imputar";
                case enumCssModal.ContenidoCuerpo: return "contenido-cuerpo";
                case enumCssModal.ContenidoCuerpoConGrid: return "contenido-cuerpo-con-grid";
                case enumCssModal.ContenedorCuerpoConGrid: return "contenedor-cuerpo-con-grid";

                case enumCssModal.EstiloContenidoCuerpo: return "style = ¨height: auto; overflow: hidden;¨";
                case enumCssModal.ContenidoPie: return "contenido-pie";
                case enumCssModal.ContenidoPieSoloBotones: return $"{enumCssModal.ContenidoPie.Render()} pie-solo-botones";
                case enumCssModal.DivDeBotonesPie: return "contenido-modal-pie-menu";
                case enumCssModal.PieDeTotales: return "pie-de-totales";
                case enumCssModal.BotonPrincipal: return $"boton-modal btn btn-primary {ExtensorDeCss.Render(enumCssOpcionMenu.BotonPorDefecto)}";
                case enumCssModal.BotonSecundario: return "boton-modal btn btn-secondary";

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
                case enumCssGrid.filaAutoFr: return "fila-auto-fr";
                case enumCssGrid.fila200pxFr: return "fila-200px-fr";
                case enumCssGrid.fila_2fr_0px_1fr_1fr_1fr: return "fila-2fr-0px-1fr-1fr-1fr";
                case enumCssGrid.fila_2fr_0px_1fr_1fr: return "fila-2fr-0px-1fr-1fr";
                case enumCssGrid.fila_2fr_0px_2fr_0px: return "fila-2fr-0px-2fr-0px";
                case enumCssGrid.fila_2fr_0px_1fr_1fr_0px_0px: return "fila-2fr-0px-1fr-1fr-0px-0px";
                case enumCssGrid.fila_100px_1fr_2fr_0px_0px: return "fila-100px-1fr-2fr-0px-0px";
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
                case enumCssCuerpo.CuerpoDatosError: return "cuerpo-datos-error";
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
                case enumCssDiv.ColumnaConBlockNoFlex: return "columna-con-block-no-flex";

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


}
