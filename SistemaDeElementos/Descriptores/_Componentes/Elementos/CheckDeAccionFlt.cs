using Utilidades;
using ModeloDeDto;

namespace MVCSistemaDeElementos.Descriptores
{
    public class CheckDeAccionFlt<TElemento> : ControlFiltroHtml where TElemento : ElementoDto
    {
        public bool ValorInicial { get; private set; }

        public string Accion { get; private set; }

        BloqueDeFitro<TElemento> Bloque => (Padre is BloqueDeFitro<TElemento>) ? (Padre as BloqueDeFitro<TElemento>) : null;


        public CheckDeAccionFlt(IControlHtml padre, string id,  string etiqueta, string ayuda, bool valorInicial, Posicion posicion, string accion)
        : base(padre: padre
              , id: id
              , etiqueta
              , ""
              , ayuda
              , posicion
              )
        {
            Tipo = enumTipoControl.Check;
            ValorInicial = valorInicial;
            Accion = accion;
            if (Bloque != null)
            {
                Bloque.Tabla.Dimension.CambiarDimension(posicion);
                Bloque.AnadirControlEn(this);
            }
        }

        public override string RenderControl()
        {
            return RenderCheck();
        }

        public string RenderCheck()
        {
            var a = AtributosHtml.AtributosComunes($"div_{IdHtml}", IdHtml, PropiedadHtml, Tipo, Ayuda);
            var valores = a.MapearComunes();

            var otraCalse = Padre is ModalDeFiltrado<TElemento> ? enumCssFiltro.ContenedorEnModalDeFiltros.Render() : "";
            valores["CssContenedor"] = Css.Render(enumCssFiltro.ContenedorCheck) + " " + otraCalse;
            valores["Css"] = Css.Render(enumCssFiltro.Check);
            valores["Etiqueta"] = Etiqueta;
            valores["Checked"] = ValorInicial.ToString().ToLower();
            valores["Accion"] = Accion;

            return PlantillasHtml.Render(PlantillasHtml.checkFlt, valores);
        }

    }

}