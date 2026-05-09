using System;
using System.Collections.Generic;
using ServicioDeDatos.Elemento;
using Utilidades;
using static GestoresDeNegocio.Negocio.GestorDeTiposDeElemento<ServicioDeDatos.ContextoSe, ServicioDeDatos.Elemento.TipoDeElementoDtm, ModeloDeDto.Negocio.TipoDeElementoDto>;

namespace MVCSistemaDeElementos.Descriptores
{
    public class FiltroDelFormulario : ControlHtml
    {
        DescriptorDeFormulario Formulario => (DescriptorDeFormulario)Padre;
        public bool RenderizarFiltro { get; set; } = false;
     
        public List<ControlFiltroHtml> ControlesDeFiltrado = new List<ControlFiltroHtml>();

        public FiltroDelFormulario(DescriptorDeFormulario formulario, string titulo)
        : base(formulario, $"filtro-{formulario.Id}", titulo, "", "", null)
        {

        }
        public string RenderFiltro()
        {
            return RenderizarFiltro ? RenderControl() : "";
        }

        private string RenderControlesDeFiltrado()
        {
            var html = "";
            foreach(var control in ControlesDeFiltrado)
            {
                html = $"{html}{Environment.NewLine}{control.RenderControl()}";
            }
            return html;
        }

        public override string RenderControl()
        {

            var onkeypress = $"onkeypress = ¨{enumAccionDeFormulario.TeclaPulsada.Render()}¨";

            string _htmlMiModal = 
                $@"
                 <!--  ******************  Filtro del formulario {Formulario.IdHtml} ********************************* -->
                 <div id=¨{IdHtml}¨ class=¨contenedor-modal¨ formulario=¨{Formulario.IdHtml}¨>
                    <div id=¨{IdHtml}_contenido¨ class=¨{enumCssModal.ContenidoModal.Render()}¨ >
                 	   <div id=¨{IdHtml}_cabecera¨ class=¨{enumCssModal.ContenidoCabecera.Render()}¨>
                 		    	{Etiqueta}
                       </div>
                 	   <div id=¨{IdHtml}_cuerpo¨ class=¨{enumCssModal.ContenidoCuerpo.Render()} {enumCssFormulario.ModalDeFiltroDeFormulario.Render()}¨ {onkeypress}>
                 	      {RenderControlesDeFiltrado()}
                       </div>
                       <div id=¨{IdHtml}_pie¨ class=¨{enumCssModal.ContenidoPie.Render()}¨>
                          <input type=¨text¨ id=¨{IdHtml}_Aceptar¨ 
                              class=¨boton-modal¨ 
                              value=¨Filtrar¨ 
                              clase=¨{Css.Render(enumCssOpcionMenu.Basico)}¨ 
                              readonly 
                              onclick=¨{enumAccionDeFormulario.AplicarFiltro.Render()}¨/>
                          <input type=¨text¨ id=¨{IdHtml}_Cerrar¨  
                              class=¨boton-modal¨ 
                              value=¨Cerrar¨ 
                              clase=¨{Css.Render(enumCssOpcionMenu.Basico)}¨ 
                              readonly 
                              onclick=¨{enumAccionDeFormulario.CerrarFiltro.Render()}¨ />
                       </div>
                    </div>
                 </div>";

            return _htmlMiModal;
        }

        internal object RenderOpciones()
        {
            var idHtml = $"{IdHtml}-opciones";
            var idHtmlCheck = $"{idHtml}-arbol";
            var idHtmlNombre = $"{idHtml}-{nameof(INombre.Nombre).ToLower()}";
            var accion = $"onClick = ¨{enumAccionDeJerarquia.MostrarJerarquia.Render(Formulario.namespaceTs)}¨";
            var renderCheck = RenderCheck(idHtmlCheck, ltrDeUnTipoDeElemento.mostrarJerarquia, true, "Jeraquía", accion);

            var a = new AtributosHtml(
                idHtml: idHtmlNombre,
                propiedad: $"{nameof(INombre.Nombre).ToLower()}",
                tipoDeControl: enumTipoControl.Editor,
                visible: true,
                editable: true,
                obligatorio: false,
                ayuda: "filtro por nombre",
                valorPorDefecto: null);

            a.LongitudMaxima = 20;

            var renderEditor = EditorDeTexto.RenderEditorDeTextoParaFiltrar(a);

            return RenderizarFiltro
                ? $@"
                    {renderCheck}
                    {renderEditor}
                  "
                : "";
        }

        internal string RenderBotonAbrirModalDeFiltro()
        {
            return RenderizarFiltro 
                ? $@"
                     <input type=¨text¨ 
                      id=¨{IdHtml}-abrir¨ 
                      class=¨boton-de-seleccion¨ 
                      value=¨...¨ 
                      onclick=¨{enumAccionDeFormulario.AbrirFiltro.Render()}¨  
                      readonly />
                  " 
                : "";
        }
    }
}
