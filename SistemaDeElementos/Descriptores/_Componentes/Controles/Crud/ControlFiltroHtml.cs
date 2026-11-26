using System.Collections.Generic;
using ServicioDeDatos;
using Utilidades;
using UtilidadesParaIu;

namespace MVCSistemaDeElementos.Descriptores
{

    public class ControlFiltroHtml : ControlHtml
    {
        public enumCriteriosDeFiltrado Criterio { get; set; }
        public string BuscarPor { get; set; }

        // ancho css de la columna del bloque de filtro que ocupa el control (p.e. "250px"); sin indicar, la columna se reparte (1fr) con las demás
        public string AnchoDeColumna { get; set; }

        // atributo que marca un editor de texto del filtro para que el CrudMnt le ponga a la derecha el botón con el que el usuario
        // elige el criterio (contiene, igual, distinto, no contiene, comienza, termina); ver ApiDeCriterioDeTexto.ts
        public const string AtributoSelectorDeCriterio = "selector-de-criterio";

        // sólo lo activan los editores de texto (EditorFiltro y FiltroConEditor); se desactiva en los filtros de texto que no siguen el
        // estándar de Filtrar.FiltrarPorTexto (p.e. el de estados)
        public bool ConSelectorDeCriterio { get; set; } = false;

        protected bool RenderizarSelectorDeCriterio =>
            ConSelectorDeCriterio && (Criterio == enumCriteriosDeFiltrado.contiene || Criterio == enumCriteriosDeFiltrado.porReferencia);

        protected string AtributoDelSelectorDeCriterio => RenderizarSelectorDeCriterio ? $"{AtributoSelectorDeCriterio}='S' " : "";

        public ControlFiltroHtml(IControlHtml padre, string id, string etiqueta, string propiedad, string ayuda, Posicion posicion)
        : base(padre, id, etiqueta, propiedad, ayuda, posicion)
        {
        }

        public override string RenderControl()
        {
            return "";
        }

        public override string RenderAtributos(string atributos = "")
        {
            atributos = base.RenderAtributos(atributos);
            atributos += $@"control-de-filtro='S'
                            criterio-de-filtro='{Criterio}' {AtributoDelSelectorDeCriterio}";
            return atributos;
        }

        public static string RenderControlInputDeFiltro(string idhtmlPadre, string id, string etiqueta = null, string propiedad = null, enumCriteriosDeFiltrado criterio = enumCriteriosDeFiltrado.contiene, string ayuda=null)
        {
            var idControl = $"{idhtmlPadre}_{id.ToLower()}";
            return $@"
                     <div class='{enumCssFiltro.ColumnaFiltro.Render()}'>
                        <div class='{enumCssControles.ContenedorEtiqueta.Render()}'>
                          <label for='{idControl}' class='{enumCssControles.Etiqueta.Render()}'>{(etiqueta is null ? id : etiqueta)}:</label>
                        </div>
                        <div class='{enumCssFiltro.ContenedorEditor.Render()}'>
                          <input type='text' id='{idControl}' name='{id}' class='{enumCssControles.EditorDeFiltro.Render()}' placeholder='{ayuda}' propiedad='{(propiedad is null ? id : propiedad)}' tipo='{enumTipoControl.FiltroConEditor.Render()}' control-de-filtro='S' criterio-de-filtro='{criterio}' >
                        </div>
                     </div>
                     ";
        }

        public static string RenderControlListaDeValoresFiltro(string idhtmlPadre, string id, Dictionary<string, string> opciones, string etiqueta = null, string propiedad = null, enumCriteriosDeFiltrado criterio = enumCriteriosDeFiltrado.igual, string ayuda = null, enumFunctionTs? alSeleccionar = null)
        {
            var idControl = $"{idhtmlPadre}_{id.ToLower()}";
            var html = $@"
                      <div class='{enumCssFiltro.ColumnaFiltro.Render()}'>
                        <div class='{enumCssControles.ContenedorEtiqueta.Render()}'>
                          <label for='{idControl}' class='{enumCssControles.Etiqueta.Render()}'>{(etiqueta is null ? id : etiqueta)}:</label>
                        </div>
                        <div class='{enumCssFiltro.ContenedorListaDeElementos.Render()}'>
                          <select id='{idControl}' name='{id}' 
                             propiedad='{(propiedad is null ? id : propiedad)}' 
                             class='{enumCssFiltro.ListaDeElementos.Render()}' 
                             tipo='{enumTipoControl.ListaDeValores.Render()}'  
                             placeholder='{ayuda}' 
                             control-de-filtro='S' 
                             criterio-de-filtro='{criterio}'
                             {(alSeleccionar is null? "": $"onChange = '{enumNameSpaceTs.Crud}.{alSeleccionar}()'")}>
                          {opciones.RenderOptions()}
                          </select>
                        </div>
                     </div>
                     ".Render();
            return html;
        }

    }
}
