using ModeloDeDto;
using ServicioDeDatos;
using Utilidades;

namespace MVCSistemaDeElementos.Descriptores
{

    public class FiltroEntreRangos<TElemento> : ControlFiltroHtml where TElemento : ElementoDto
    {
        BloqueDeFitro<TElemento> Bloque => (Padre is BloqueDeFitro <TElemento>) ? (Padre as BloqueDeFitro<TElemento>) : null;
        private bool MostrarEtiqueta { get; }
        public string ExpresioRegular { get; set; }
        public string PlaceHolder { get; set; }

        public FiltroEntreRangos(IControlHtml padre, string etiqueta, string propiedad, string ayuda, Posicion posicion = null, bool renderEtiqueta = true)
        : base(padre, $"{padre.Id}-{propiedad}", etiqueta, propiedad, ayuda, posicion)
        {
            Tipo = enumTipoControl.FiltroEntreRangos;
            Criterio = enumCriteriosDeFiltrado.entreRangos;
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

            var entreRangos = RenderFiltroEntreRangos(a);
            entreRangos = Padre is ModalDeFiltrado<TElemento> && MostrarEtiqueta
            ? entreRangos.Replace("[etiqueta]", $"<div class='{enumCssFiltro.EtiquetaEntreRangos.Render()}'>{Etiqueta}</div>")
            : entreRangos.Replace("[etiqueta]", "");

            return entreRangos;
        }

        private string RenderFiltroEntreRangos(AtributosHtml atributos)
        {
            var valores = atributos.MapearComunes();
            var otraClase = Padre is ModalDeFiltrado<TElemento> ? enumCssFiltro.ContenedorEnModalDeFiltros.Render() : "";

            var ccsContenedor = Padre is ModalDeFiltrado<TElemento> && MostrarEtiqueta
                ? $"{Css.Render(enumCssFiltro.ContenedorEntreRangosModal)} {otraClase}"
                : Css.Render(enumCssFiltro.ContenedorEntreRangos);

            if (Bloque != null)
                ccsContenedor = $"{enumCssImportant.SinMarginTop.Render()} {ccsContenedor}";

            valores["Css"] = Css.Render(enumCssFiltro.Rango);
            valores["CssContenedor"] = ccsContenedor;
            valores["Placeholder"] = PlaceHolder;
            valores["Ayuda"] = atributos.Ayuda;

            var htmRangos = PlantillasHtml.Render(PlantillasHtml.filtroEntreRangos, valores);
            htmRangos = htmRangos.Replace($"[{nameof(IUPropiedadAttribute.ExpresionRegular)}]", !ExpresioRegular.IsNullOrEmpty() ? $"patterm='{ExpresioRegular}'" : "");

            return htmRangos;
        }
    }
}
