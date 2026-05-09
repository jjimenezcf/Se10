using ModeloDeDto;
using System.Collections.Generic;
using Utilidades;

namespace MVCSistemaDeElementos.Descriptores
{

    public class FiltroEntreImportesConCheck<TElemento> : ControlFiltroHtml where TElemento : ElementoDto
    {

        FiltroEntreImportes<TElemento> Importes { get; set; }

        CheckDeMostrarColumna<TElemento> check { get; set; }

        BloqueDeFitro<TElemento> Bloque => (Padre is BloqueDeFitro<TElemento>) ? (Padre as BloqueDeFitro<TElemento>) : null;

        private bool MostrarEtiqueta { get; }

        public FiltroEntreImportesConCheck(IControlHtml Padre, FiltroEntreImportes<TElemento> importes, string columna, string tituloCheck, Posicion posicion = null, List<string> columnas = null)
        : base(padre: Padre
              , id: $"{Padre.Id}_{enumTipoControl.Check.Render()}_{columna}"
              , ""
              , ""
              , ""
              , posicion == null ? new Posicion(1, 1) : posicion
              )
        {
            importes.Padre = this;
            IniciarClase(importes, columna, tituloCheck, posicion, columnas);
        }

        private void IniciarClase(FiltroEntreImportes<TElemento> importes, string columna, string tituloCheck, Posicion posicion, List<string> columnas)
        {
            Tipo = enumTipoControl.FiltroEntreImportesMostrarColumna;

            check = new CheckDeMostrarColumna<TElemento>((IControlHtml)this, tituloCheck, "Mostrar la columna", false, columna, columnas: columnas);

            Importes = importes;
            Etiqueta = importes.Etiqueta;

            if (Bloque != null)
            {
                Bloque.Tabla.Dimension.CambiarDimension(posicion);
                Bloque.AnadirControlEn(this);
            }
        }

        public override string RenderControl()
        {
            return RenderFiltroEntreImportesConCheck();
        }

        public string RenderFiltroEntreImportesConCheck()
        {
            var a = AtributosHtml.AtributosComunes($"div_{IdHtml}", IdHtml, PropiedadHtml, Tipo, Ayuda);
            var valores = a.MapearComunes();

            valores["CssContenedor"] = enumCssFiltro.ContenedorEntreImportesMostrarModal.Render();
            if (Bloque != null)
                valores["CssContenedor"] = $"{enumCssImportant.SinMarginTop.Render()} {valores["CssContenedor"]}";

            valores["Ayuda"] = "filtros a aplicar y check para indicar si se muestra la información";

            var control = PlantillasHtml.Render(PlantillasHtml.EntreImportesConCheckDeMostrar, valores);

            control = control.Replace("[CssContenedorEi]",
                $"{(Bloque != null ? enumCssImportant.SinMarginTop.Render() : "")} {enumCssControles.EntreImportesMostrar.Render()}");

            return control
                .Replace("[EtiquetaEntreImportes]", $"<div class='{enumCssFiltro.EtiquetaEntreImportes.Render()}'>{Etiqueta}</div>")
                .Replace("[EntreImportes]", Importes.RenderControl())
                .Replace("[CheckDeMostrar]", check.RenderCheck());
        }
    }
}
