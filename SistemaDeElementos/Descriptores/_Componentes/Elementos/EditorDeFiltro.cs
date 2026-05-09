using Utilidades;
using ModeloDeDto;
using ServicioDeDatos;

namespace MVCSistemaDeElementos.Descriptores
{
    public class EditorFiltro<TElemento> : ControlFiltroHtml where TElemento : ElementoDto
    {
        public BloqueDeFitro<TElemento> Bloque => (BloqueDeFitro<TElemento>)Padre;

        public EditorFiltro(IControlHtml padre, string etiqueta, string propiedad, string ayuda, Posicion posicion)
        : base(padre: padre
              , id: $"{padre.Id}_{enumTipoControl.Editor.Render()}_{propiedad}"
              , etiqueta
              , propiedad
              , ayuda
              , posicion
              )
        {
            Tipo = enumTipoControl.Editor;
            Criterio = enumCriteriosDeFiltrado.contiene;

            if (padre is BloqueDeFitro<TElemento>)
            {
                Bloque.Tabla.Dimension.CambiarDimension(posicion);
                Bloque.AnadirControlEn(this);
            }
        }

        public override string RenderControl()
        {
            return RenderEditor();
        }

        public string RenderEditor()
        {

            var otraCalse = Padre is ModalDeFiltrado<TElemento> ? enumCssFiltro.ContenedorEnModalDeFiltros.Render() : "";

            return $@"<div class=¨{enumCssFiltro.ContenedorEditor.Render()}{(otraCalse.IsNullOrEmpty()?"": $" {otraCalse}")}¨ title=¨{Ayuda}¨>
                         <input id=¨{IdHtml}¨ type = ¨text¨  class=¨{enumCssControles.EditorDeFiltro.Render()}¨ {base.RenderAtributos()}  placeholder=¨{Ayuda}¨>
                     </div>
                  ";
        }
    }

}
