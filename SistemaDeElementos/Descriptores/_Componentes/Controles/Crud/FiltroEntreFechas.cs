using ModeloDeDto;
using ServicioDeDatos;
using Utilidades;

namespace MVCSistemaDeElementos.Descriptores
{

    public class FiltroEntreFechas<TElemento> : ControlFiltroHtml where TElemento : ElementoDto
    {
        BloqueDeFitro<TElemento> Bloque => (Padre is BloqueDeFitro <TElemento>) ? (Padre as BloqueDeFitro<TElemento>) : null;
        private bool MostrarEtiqueta { get; }
        public FiltroEntreFechas(IControlHtml padre, string etiqueta, string propiedad, string ayuda, Posicion posicion = null, bool renderEtiqueta = true)
        : base(padre, $"{padre.Id}-{propiedad}", etiqueta, propiedad, ayuda, posicion)
        {
            Tipo = enumTipoControl.FiltroEntreFechas;
            Criterio = enumCriteriosDeFiltrado.entreFechas;
            MostrarEtiqueta = renderEtiqueta;

            if (Bloque!=null)
            {
                Bloque.Tabla.Dimension.CambiarDimension(posicion);
                Bloque.AnadirControlEn(this);
            }
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

            var entreFechas = RenderFiltroEntreFechas(a);
            entreFechas = Padre is ModalDeFiltrado<TElemento> && MostrarEtiqueta
            ? entreFechas.Replace("[etiqueta]", $"<div class='{enumCssFiltro.EtiquetaEntreFechas.Render()}'>{Etiqueta}</div>")
            : entreFechas.Replace("[etiqueta]", "");

            return entreFechas;
        }

        private string RenderFiltroEntreFechas(AtributosHtml atributos)
        {
            var valores = atributos.MapearComunes();

            var otraCalse = Padre is ModalDeFiltrado<TElemento> ? enumCssFiltro.ContenedorEnModalDeFiltros.Render() : "";

            valores["CssContenedor"] = Padre is ModalDeFiltrado<TElemento> && MostrarEtiqueta
                ? $"{Css.Render(enumCssFiltro.ContenedorEntreFechasModal)} {otraCalse}"
                : Css.Render(enumCssFiltro.ContenedorEntreFechas);

            if (Bloque != null)
                valores["CssContenedor"] = $"{enumCssImportant.SinMarginTop.Render()} {valores["CssContenedor"]}";

            valores["Css"] = Css.Render(enumCssFiltro.Fecha);
            valores["CssHora"] = Css.Render(enumCssFiltro.Hora);
            valores["Placeholder"] = atributos.Ayuda;
            valores["Ayuda"] = atributos.Ayuda;

            var htmSelectorDeFecha = PlantillasHtml.Render(PlantillasHtml.filtroEntreFechas, valores);

            return htmSelectorDeFecha;
        }
    }
}
