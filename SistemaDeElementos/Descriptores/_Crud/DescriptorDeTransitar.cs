using System.Collections.Generic;
using Utilidades;
using ModeloDeDto;
using ModeloDeDto.Entorno;
using MVCSistemaDeElementos.Descriptores;
using ServicioDeDatos.Seguridad;
using UtilidadesParaIu;
using ServicioDeDatos.Elemento;
using ModeloDeDto.Negocio;
using ServicioDeDatos;
using MVCSistemaDeElementos.Controllers;
using GestorDeElementos;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeTransitar<TElemento> : ControlHtml where TElemento : ElementoDto
    {
        public DescriptorDeCrud<TElemento> Crud => (DescriptorDeCrud<TElemento>)Padre;

        public RestrictorDeFiltro<TElemento> Origen { get; }
        public ListaDeElemento<TElemento> Transicion { get; }
        public ListasDinamicas<TElemento> Usuario { get; }

        public ControlDeArchivo SelectorDeUnArchivo { get; }

        public DescriptorDeTransitar(DescriptorDeCrud<TElemento> crud)
        : base(
          padre: crud,
          id: $"{crud.Id}_{enumTipoControl.pnlTransitar.Render()}",
          etiqueta: "Transitar",
          propiedad: null,
          ayuda: null,
          posicion: null
        )
        {
            Origen = new RestrictorDeFiltro<TElemento>(this, "Estado", nameof(TransicionDtm.Origen), "Estado origen", new Posicion(0, 0))
            {
                Controlador = nameof(enumControladoresNegocio.Estados),
                VistaDondeNavegar = enumVistasNegocio.CrudDeEstados,
                Negocio = NegociosDeSe.NegocioDeUnDto(typeof(TElemento))
            };
            Transicion = new ListaDeElemento<TElemento>(this
                , "Seleccionar transición"
                , "Seleccione la transición a ejecutar"
                , seleccionarDe: nameof(TransicionDto)
                , filtraPor: nameof(ltrTransiciones.transiciones)
                , mostrarExpresion: nameof(TransicionDto.Nombre)
                , posicion: new Posicion(0, 1)
                , nameof(TransicionesController))
            {
                OnChange = $"javascript: Crud.{enumGestorDeEventos.EventosModalDeTransitar}('{eventosDeTransitar.Seleccionar}','{IdHtml}')",
                AutoPosicionamiento = true
            };


            Usuario = new ListasDinamicas<TElemento>(this,
                                             etiqueta: "Usuario",
                                             filtrarPor: nameof(ltrTransiciones.Usuarios),
                                             ayuda: "Notificar de la transición a ...",
                                             seleccionarDe: nameof(UsuarioDto),
                                             buscarPor: nameof(UsuarioDto.NombreCompleto),
                                             mostrarExpresion: $"[{nameof(UsuarioDto.NombreCompleto)}]",
                                             criterioDeBusqueda: enumCriteriosDeFiltrado.contiene,
                                             posicion: new Posicion(0, 0),
                                             controlador: nameof(UsuariosController),
                                             navegarA: nameof(UsuariosController.CrudUsuario),
                                             restringirPor: "",
                                             alSeleccionarBlanquearControl: "")
            { LongitudMinimaParaBuscar = 1 };

            SelectorDeUnArchivo = new ControlDeArchivo(this, Id + "_selector_archivo", "Adjuntar un archivo a la transición",
                propiedad: nameof(IUsaArchivo.IdArchivo), 
                "Seleccione un archivo para adjuntar a la transición", 
                new Posicion(0, 2))
            {
            };
        }

        public string RenderDeTransitar()
        {
            return RenderControl();
        }

        public override string RenderControl()
        {
            var htmlModal = RenderizarModal(enumTipoDeModal.ModalDeTransitar,
                idHtml: IdHtml
                , controlador: Crud.Controlador
                , tituloH2: Etiqueta
                , cuerpo: cuerpoDeTransitar()
                , idOpcion: $"{IdHtml}-transitar"
                , opcion: Crud.NegocioActivo ? "Transitar" : ""
                , accion: Crud.NegocioActivo ? $"Crud.{enumGestorDeEventos.EventosModalDeTransitar}('{eventosDeTransitar.Transitar}','{IdHtml}')" : ""
                , cerrar: $"Crud.{enumGestorDeEventos.EventosModalDeTransitar}('{eventosDeTransitar.Cerrar}','{IdHtml}')"
                , navegador: ""
                , claseBoton: enumCssOpcionMenu.DeElemento
                , permisosNecesarios: enumModoDeAccesoDeDatos.Consultor);

            return htmlModal;
        }

        private string cuerpoDeTransitar()
        {
            var htmlArchivo = SelectorDeUnArchivo.RenderArchivo(estiloContenedor: "style='padding-right: 10px;'");


            var htmlCuerpo = $@"<div id=¨{IdHtml}_cuerpo_transitar¨ class=¨{enumCssDeTransitar.Cuerpo.Render()}¨>
                                        {Origen.RenderControl()}
                                        {Transicion.RenderControl()}
                                        {RenderEditorAsunto()}
                                        {htmlArchivo}
                                         <br/>
                                        {Usuario.RenderControl()}
                                </div>
                                ";
            return htmlCuerpo;
        }

        private string RenderEditorAsunto()
        {
            var otrosAtributosEditor = new Dictionary<string, string>();
            otrosAtributosEditor["LongitudMaxima"] = "maxlength='255'"; ;
            otrosAtributosEditor["Obligatorio"] = "obligatorio='N'";

            var otrosAtributosEtiqueta = new Dictionary<string, string>();
            otrosAtributosEtiqueta["estilo"] = "style='padding :0.5em;'";

            var html = $@"<div class=¨{Css.Render(enumCssDeTransitar.ContenedorAnotacion)}¨>
                         {RenderEditorConEtiquetaEncima($"{IdHtml}_asunto", "Anotación", "asunto", "para dejar constancia del motivo de la transición", otrosAtributosEditor, otrosAtributosEtiqueta)}
                         {RenderTextArea($"{IdHtml}_detalle", "cuerpo del asunto", "detalleAsunto", "indique el detalle de la anotación")}
                      </div>";
            return html;
        }
    }
}
/*
 *                                      <div id=¨{IdHtml}_cuerpo_enviar_correo¨ class=¨{enumCssEnviarCorreo.cabecera.Render()}¨>
                                        PuestosDeTrabajoReceptores
                                        UsuariosReceptores
                                     </div>
                                     <div id=¨{IdHtml}_cuerpo_sometido¨ class=¨{enumCssEnviarCorreo.cuerpo.Render()}¨>
                                        {RenderEditorAsunto()}
                                        {RenderTextArea($"{IdHtml}_mensaje", "mensaje", "cuerpoMensaje", "indique el mensaje")}
                                     </div>
                                     <div id=¨{IdHtml}_cuerpo_enviar¨ class=¨{enumCssEnviarCorreo.adjuntos.Render()}¨>                                        
                                        {RenderElementos()}
                                     </div>
 * */