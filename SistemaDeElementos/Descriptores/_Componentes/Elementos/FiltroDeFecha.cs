using Utilidades;
using ModeloDeDto;
using ServicioDeDatos;

namespace MVCSistemaDeElementos.Descriptores
{
    // Filtro por una única fecha: es un editor de filtro con un input de tipo fecha, así el grid lo trata como un editor más y manda
    // su valor (yyyy-MM-dd) en la cláusula de filtrado con el criterio igual
    public class FiltroDeFecha<TElemento> : EditorFiltro<TElemento> where TElemento : ElementoDto
    {
        public FiltroDeFecha(IControlHtml padre, string etiqueta, string propiedad, string ayuda, Posicion posicion)
        : base(padre, etiqueta, propiedad, ayuda, posicion)
        {
            Criterio = enumCriteriosDeFiltrado.igual;
        }

        public override string RenderControl()
        {
            var otraCalse = Padre is ModalDeFiltrado<TElemento> ? enumCssFiltro.ContenedorEnModalDeFiltros.Render() : "";

            return $@"<div class=¨{enumCssFiltro.ContenedorEditor.Render()}{(otraCalse.IsNullOrEmpty() ? "" : $" {otraCalse}")}¨ title=¨{Ayuda}¨>
                         <input id=¨{IdHtml}¨ type = ¨date¨  class=¨{enumCssControles.EditorDeFiltro.Render()}¨ {base.RenderAtributos()}  placeholder=¨{Ayuda}¨>
                     </div>
                  ";
        }
    }

}
