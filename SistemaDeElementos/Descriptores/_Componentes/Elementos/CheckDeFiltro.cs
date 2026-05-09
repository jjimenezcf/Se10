using Utilidades;
using ModeloDeDto;
using ServicioDeDatos;

namespace MVCSistemaDeElementos.Descriptores
{
    public class CheckFiltro<TElemento> : ControlFiltroHtml where TElemento : ElementoDto
    {
        public bool ValorInicial { get; private set; }
        public bool FiltrarPorFalse { get; private set; }

        public bool EsOnOff { get; set; }

        public string Accion { get; set; }
        BloqueDeFitro<TElemento> Bloque => (Padre is BloqueDeFitro<TElemento>) ? (Padre as BloqueDeFitro<TElemento>) : null;


        public CheckFiltro(IControlHtml padre, string etiqueta, string filtrarPor, string ayuda, bool valorInicial, bool filtrarPorFalse, Posicion posicion = null, string accion = null)
        : base(padre: padre
              , id: $"{padre.Id}_{enumTipoControl.Check.Render()}_{filtrarPor}"
              , etiqueta
              , filtrarPor
              , ayuda
              , posicion
              )
        {
            Tipo = enumTipoControl.Check;
            Criterio = enumCriteriosDeFiltrado.igual;
            ValorInicial = valorInicial;
            Accion = accion;
            FiltrarPorFalse = filtrarPorFalse;
            if (Bloque != null)
            {
                Bloque.Tabla.Dimension.CambiarDimension(posicion);
                Bloque.AnadirControlEn(this);
            }
        }

        public CheckFiltro(FiltroDelFormulario filtroDelFormulario, string etiqueta, string filtrarPor, string ayuda, bool valorInicial, bool filtrarPorFalse = false)
        : base(padre: filtroDelFormulario
              , id: $"{filtroDelFormulario.Id}-{filtrarPor}"
              , etiqueta
              , filtrarPor
              , ayuda
              , null
              )
        {
            Tipo = enumTipoControl.Check;
            Criterio = enumCriteriosDeFiltrado.igual;
            ValorInicial = valorInicial;
            FiltrarPorFalse = filtrarPorFalse;
        }

        public override string RenderControl()
        {
            return RenderCheckFlt();
        }

        public string RenderCheckFlt()
        {

            var otraCalse = Padre is ModalDeFiltrado<TElemento> ? enumCssFiltro.ContenedorEnModalDeFiltros.Render() : "";

            if (EsOnOff)
            {
                return RenderCheckFiltroOnOff(IdHtml, PropiedadHtml, ValorInicial, Etiqueta, Accion,otraCalse).Replace("[FiltrarPorFalse]", FiltrarPorFalse ? "S" : "N");
            }
                        
            var render = RenderCheck(IdHtml, PropiedadHtml, ValorInicial, Etiqueta, Accion, 
                   Bloque != null ?
                   enumCssFiltro.ControlApilado :
                   null, 
                otraCalse);
            return render.Replace("[FiltrarPorFalse]", FiltrarPorFalse ? "S" : "N");
        }

    }

}