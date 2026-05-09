using ModeloDeDto;
using ServicioDeDatos;
using Utilidades;

namespace MVCSistemaDeElementos.Descriptores
{

    public class FiltroConEditor<TElemento> : ControlFiltroHtml where TElemento : ElementoDto
    {
        BloqueDeFitro<TElemento> Bloque => (Padre is BloqueDeFitro <TElemento>) ? (Padre as BloqueDeFitro<TElemento>) : null;
        private bool MostrarEtiqueta { get; }
        public string ExpresioRegular { get; set; }
        public string PlaceHolder { get; set; }
        public string ValorPorDefecto { get; set; }

        public FiltroConEditor(IControlHtml padre, string etiqueta, string propiedad, string ayuda, Posicion posicion = null,
            bool renderEtiqueta = true, string valorPorDefecto = "")
        : base(padre, $"{padre.Id}-{propiedad}", etiqueta, propiedad, ayuda, posicion)
        {
            Tipo = enumTipoControl.FiltroConEditor;
            Criterio = enumCriteriosDeFiltrado.contiene;
            BuscarPor = propiedad;
            MostrarEtiqueta = renderEtiqueta;

            if (Bloque!=null)
            {
                Bloque.Tabla.Dimension.CambiarDimension(posicion);
                Bloque.AnadirControlEn(this);
            }
            ValorPorDefecto = valorPorDefecto;
        }

        public override string RenderControl()
        {
            var a = new AtributosHtml(
                idHtml: IdHtml,
                propiedad: PropiedadHtml,
                tipoDeControl: Tipo,
                visible: Visible,
                editable: Editable,
                obligatorio: Obligatorio,
                ayuda: Ayuda,
                valorPorDefecto: null);

            var ConEditor = RenderFiltroConEditor(a); // EditorDeTexto.RenderEditorDeTextoParaFiltrar(a);
            ConEditor = Padre is ModalDeFiltrado<TElemento> && MostrarEtiqueta
            ? ConEditor.Replace("[etiqueta]", $"<div class='{enumCssFiltro.EtiquetaConEditor.Render()}'>{Etiqueta}</div>")
            : ConEditor.Replace("[etiqueta]", "");

            return ConEditor;
        }

        private string RenderFiltroConEditor(AtributosHtml atributos)
        {
            var valores = atributos.MapearComunes();
            var otraClase = Padre is ModalDeFiltrado<TElemento> ? enumCssFiltro.ContenedorEnModalDeFiltros.Render() : "";

            var ccsContenedor = Padre is ModalDeFiltrado<TElemento> && MostrarEtiqueta
                ? $"{Css.Render(enumCssFiltro.ContenedorEditor)} {otraClase}"
                : Css.Render(enumCssFiltro.ContenedorEditor);

            if (Bloque != null)
                ccsContenedor = $"{enumCssImportant.SinMarginTop.Render()} {ccsContenedor}";

            valores["Css"] = Css.Render(enumCssFiltro.Rango);
            valores["CssContenedor"] = ccsContenedor;
            valores["Placeholder"] = atributos.Ayuda;
            valores["Ayuda"] = atributos.Ayuda;
            valores["CriterioDeFiltrado"] = Criterio;
            valores["BuscarPor"] = BuscarPor;

            var htmlEditorTexto = PlantillasHtml.Render(PlantillasHtml.editorFlt, valores);

            htmlEditorTexto = htmlEditorTexto.Replace("[onBlur]", "");
            htmlEditorTexto = htmlEditorTexto.Replace($"style = '[Estilos]'", "");
            htmlEditorTexto = htmlEditorTexto.Replace($"valor-de-defecto='[ValorPorDefecto]'", ValorPorDefecto.IsNullOrEmpty() ? "" : $"valor-de-defecto='{ValorPorDefecto}'");
            htmlEditorTexto = htmlEditorTexto.Replace($"[{nameof(IUPropiedadAttribute.Formato)}]", "");
            htmlEditorTexto = htmlEditorTexto.Replace($"value='[ValorPorDefecto]'", ValorPorDefecto.IsNullOrEmpty() ? "": $"value='{ValorPorDefecto}'");
            htmlEditorTexto = htmlEditorTexto.Replace($"[LongitudMaxima]", "");

            return htmlEditorTexto;
        }
    }
}
