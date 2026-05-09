using Utilidades;

namespace MVCSistemaDeElementos.Descriptores
{
    public interface IBloqueDeFormulario
    {
        CuerpoDeFormulario  Cuerpo { get; }
    }

    public class ControlDeFormulario
    {
        public string Id {get;}
        public string IdHtml => $@"{Id}".ToLower();
        public string Etiqueta { get;}
        public string Ayuda { get; }
        public enumTipoControl Tipo { get; set; }
        public enumCssControlesFormulario ClaseCss {get;}

        public IBloqueDeFormulario Padre { get; set; }

        public ControlDeFormulario(BloqueApilado padre, string id, enumTipoControl tipo,  string etiqueta, enumCssControlesFormulario claseCss, string ayuda)
        {
            Padre = padre;
            Id = id;
            Etiqueta = etiqueta;
            Ayuda = ayuda;
            ClaseCss = claseCss;
            Tipo = tipo;
        }


        public static string RenderAtributos(string propiedad, string idHtml, enumTipoControl tipo, enumCssControlesFormulario clase, string ayuda,  string otrosAtributos = "")
        {
            var atributos = ControlHtml.RenderAtributos(propiedad, idHtml, tipo, clase.Render(), ayuda, otrosAtributos);
            return atributos;
        }

    }

}