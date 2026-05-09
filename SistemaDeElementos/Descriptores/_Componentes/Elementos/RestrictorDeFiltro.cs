using Utilidades;
using ModeloDeDto;
using ServicioDeDatos;

namespace MVCSistemaDeElementos.Descriptores
{
    public class RestrictorDeFiltro<TElemento> : ControlFiltroHtml where TElemento : ElementoDto
    {
        public int Restrictor { get; set; }
        public string TextoParaMostrar { get; set; }
        public string Controlador { get; set; }
        public string VistaDondeNavegar { get; set; }
        public enumNegocio Negocio { get; set; }
        public string OnClick { get; set; }
        public RestrictorDeFiltro(IControlHtml padre, string etiqueta, string propiedad, string ayuda, Posicion posicion)
        : base(padre: padre
              , id: $"{padre.Id}-{propiedad}"
              , etiqueta
              , propiedad
              , ayuda
              , posicion
              )
        {
            Tipo = enumTipoControl.RestrictorDeFiltro;
            Criterio = enumCriteriosDeFiltrado.igual;

            if (padre is BloqueDeFitro<TElemento>)
            {
                ((BloqueDeFitro<TElemento>)padre).Tabla.Dimension.CambiarDimension(posicion);
                ((BloqueDeFitro<TElemento>)padre).AnadirControlEn(this);
            }
        }

        public override string RenderControl()
        {
            return RenderEditor();
        }
        public override string RenderAtributos(string atributos = "")
        {
            atributos = base.RenderAtributos(atributos);
            if (Restrictor > 0)
                atributos = $"{atributos} value='{TextoParaMostrar}' restrictor={Restrictor}";
            return atributos;

        }
        public string RenderEditor()
        {
            var navegador = Controlador.IsNullOrEmpty() ? "" : RenderDto.DefinirNavegador(IdHtml, Controlador, VistaDondeNavegar, Negocio, OnClick);
            return $@"<div class=¨{Css.Render(enumCssFiltro.ContenedorEditor)}¨>
                         <input id=¨{IdHtml}¨ type = ¨text¨ class=¨{enumCssControles.RestrictorDeFiltro.Render()}¨ {RenderAtributos()} readonly placeholder=¨{Ayuda}¨>
                         {navegador}
                      </div>
                  ";
        }
    }
}
