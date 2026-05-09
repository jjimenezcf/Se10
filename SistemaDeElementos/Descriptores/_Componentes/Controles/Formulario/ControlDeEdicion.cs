using Utilidades;

namespace MVCSistemaDeElementos.Descriptores
{
    public class ControlDeEdicion : ControlDeFormulario
    {
        public ControlDeEdicion(BloqueApilado padre, string id, string etiqueta,string ayuda)
            : base(padre, id, enumTipoControl.Editor, etiqueta, enumCssControlesFormulario.Editor, ayuda)
        {
        }

        public string RenderEditor()
        {
            return $@"<input {RenderAtributos(Id, IdHtml, Tipo, ClaseCss, Ayuda, $"type = ¨text¨")}></input>";
        }
    }
}
