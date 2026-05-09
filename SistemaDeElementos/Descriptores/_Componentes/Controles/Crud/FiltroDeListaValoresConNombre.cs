using ModeloDeDto;
using Utilidades;

namespace MVCSistemaDeElementos.Descriptores
{

    public class FiltroDeListaValoresConNombre<TElemento> : ControlFiltroHtml where TElemento : ElementoDto
    {
        ListaDeValores<TElemento> ListaDeValores { get; }
        FiltroConEditor<TElemento> Editor { get; }


        public FiltroDeListaValoresConNombre(IControlHtml padre, ListaDeValores<TElemento> lista, FiltroConEditor<TElemento> editor)
        : base(padre, $"{padre.Id}-{lista.Propiedad}-{editor.Propiedad}", "", lista.Propiedad + "-" + editor.Propiedad, "", new Posicion(1, 0))
        {
            Tipo = enumTipoControl.FiltroDeListaValoresConEditor;

            ListaDeValores = lista;
            Editor = editor;
        }

        public override string RenderControl()
        {
            return RenderFiltroDeListaValoresConNombre();
        }

        private string RenderFiltroDeListaValoresConNombre()
        {
            //var renderEtiqueta = $"<div class='{enumCssFiltro.EtiquetaEntreFechas.Render()}'>{Etiqueta}</div>";
            var renderListaDeValores = ListaDeValores.RenderControl();
            var renderEditor = Editor.RenderControl();

            /*
             *$@"
        <div id='[IdHtmlContenedor]' name='contenedor-control' class='input-group [CssContenedor]' title='[Ayuda]'>
            [etiqueta]
            [listaValores]
            [Editor]
         </div>
        ";
             * */

            return PlantillasHtml.filtroDeListaDeValoresConEditor.Replace("[etiqueta]", "")
                .Replace("[IdHtmlContenedor]", IdHtml + ".contenedor")
                .Replace("[Ayuda]", Ayuda)
                .Replace("[CssContenedor]", $"{enumCssFiltro.ContenedorDeRelacionModal.Render()} {enumCssFiltro.ContenedorEnModalDeFiltros.Render()}")
                .Replace("[listaValores]", renderListaDeValores)
                .Replace("[editor]", renderEditor);
        }
    }
}
