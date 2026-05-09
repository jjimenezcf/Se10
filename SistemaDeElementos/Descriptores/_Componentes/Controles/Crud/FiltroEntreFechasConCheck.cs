using ModeloDeDto;
using System.Collections.Generic;
using Utilidades;

namespace MVCSistemaDeElementos.Descriptores
{

    public class FiltroEntreFechasConCheck<TElemento> : ControlFiltroHtml where TElemento : ElementoDto
    {

        FiltroEntreFechas<TElemento> Fechas { get; set; }

        CheckDeMostrarColumna<TElemento> check { get; set; }

        BloqueDeFitro<TElemento> Bloque => (Padre is BloqueDeFitro<TElemento>) ? (Padre as BloqueDeFitro<TElemento>) : null;

        private bool MostrarEtiqueta { get; }

        public FiltroEntreFechasConCheck(IControlHtml Padre, FiltroEntreFechas<TElemento> fechas, string propiedadDto, string tituloCheck, Posicion posicion = null, List<string> columnas = null)
        : base(padre: Padre
              , id: $"{Padre.Id}_{enumTipoControl.Check.Render()}_{propiedadDto}"
              , ""
              , ""
              , ""
              , posicion == null ? new Posicion(1, 1) : posicion
              )
        {
            fechas.Padre = this;
            IniciarClase(fechas, propiedadDto, tituloCheck, posicion, columnas);
        }

        private void IniciarClase(FiltroEntreFechas<TElemento> fechas, string propiedadDto, string tituloCheck, Posicion posicion, List<string> columnas)
        {
            Tipo = enumTipoControl.FiltroEntreFechasMostrarColumna;

            check = new CheckDeMostrarColumna<TElemento>((IControlHtml)this, tituloCheck, "Mostrar la columna", false, propiedadDto, columnas: columnas);

            Fechas = fechas;
            Etiqueta = fechas.Etiqueta;

            if (Bloque != null)
            {
                Bloque.Tabla.Dimension.CambiarDimension(posicion);
                Bloque.AnadirControlEn(this);
            }
        }

        public override string RenderControl()
        {
            return RenderFiltroEntreFechasConCheck();
        }

        public string RenderFiltroEntreFechasConCheck()
        {
            var a = AtributosHtml.AtributosComunes($"div_{IdHtml}", IdHtml, PropiedadHtml, Tipo, Ayuda);
            var valores = a.MapearComunes();

            valores["CssContenedor"] = enumCssFiltro.ContenedorEntreFechasMostrarModal.Render();
            if (Bloque != null)
                valores["CssContenedor"] = $"{enumCssImportant.SinMarginTop.Render()} {valores["CssContenedor"]}";

            valores["Ayuda"] = "filtros a aplicar y check para indicar si se muestra la información";

            var control = PlantillasHtml.Render(PlantillasHtml.EntreFechasConCheckDeMostrar, valores);

            control = control.Replace("[CssContenedorEf]",
                $"{(Bloque != null ? enumCssImportant.SinMarginTop.Render() : "")} {enumCssControles.EntreFechasMostrar.Render()}");

            return control
                .Replace("[EtiquetaEntreFechas]", $"<div class='{enumCssFiltro.EtiquetaEntreFechas.Render()}'>{Etiqueta}</div>")
                .Replace("[EntreFechas]", Fechas.RenderControl())
                .Replace("[CheckDeMostrar]", check.RenderCheck());
        }
    }
}
