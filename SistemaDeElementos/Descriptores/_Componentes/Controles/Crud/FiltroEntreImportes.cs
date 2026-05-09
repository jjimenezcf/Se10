using ModeloDeDto;
using ServicioDeDatos;
using Utilidades;

namespace MVCSistemaDeElementos.Descriptores
{

    public class FiltroEntreImportes<TElemento> : ControlFiltroHtml where TElemento : ElementoDto
    {
        BloqueDeFitro<TElemento> Bloque => (Padre is BloqueDeFitro <TElemento>) ? (Padre as BloqueDeFitro<TElemento>) : null;
        private bool MostrarEtiqueta { get; }
        public FiltroEntreImportes(IControlHtml padre, string etiqueta, string propiedad, string ayuda, Posicion posicion = null, bool renderEtiqueta = true)
        : base(padre, $"{padre.Id}-{propiedad}", etiqueta, propiedad, ayuda, posicion)
        {
            Tipo = enumTipoControl.FiltroEntreImportes;
            Criterio = enumCriteriosDeFiltrado.entreImportes;
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

            var entreImportes = RenderFiltroEntreImportes(a);
            entreImportes = Padre is ModalDeFiltrado<TElemento> && MostrarEtiqueta
            ? entreImportes.Replace("[etiqueta]", $"<div class='{enumCssFiltro.EtiquetaEntreImportes.Render()}'>{Etiqueta}</div>")
            : entreImportes.Replace("[etiqueta]", "");

            return entreImportes;
        }

        internal string RenderFiltroEntreImportes(AtributosHtml atributos)
        {
            var valores = atributos.MapearComunes();

            var otraCalse = Padre is ModalDeFiltrado<TElemento> ? enumCssFiltro.ContenedorEnModalDeFiltros.Render() : "";

            valores["CssContenedor"] = Padre is ModalDeFiltrado<TElemento> && MostrarEtiqueta
                ? $"{Css.Render(enumCssFiltro.ContenedorEntreImportesModal)} {otraCalse}"
                : Css.Render(enumCssFiltro.ContenedorEntreImportes);
            valores["Css"] = Css.Render(enumCssFiltro.Importe);
            valores["Placeholder"] = atributos.Ayuda;
            valores["Ayuda"] = atributos.Ayuda;

            var htmSelectorDeFecha = PlantillasHtml.Render(PlantillasHtml.filtroEntreImportes, valores);

            return htmSelectorDeFecha;
        }
    }
}
