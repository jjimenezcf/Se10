using Utilidades;

namespace MVCSistemaDeElementos.Descriptores
{
    public class OpcionHtml : ControlHtml
    {
        public string Accion { get; }

        public OpcionHtml(ControlHtml padre, string id, string etiqueta, string ayuda, string accion)
        : base(padre, $"{padre.Id}-{id}", etiqueta,"",ayuda, null)
        {
            Accion = accion;
        }

        public string  RenderOpcion()
        {
            return RenderControl();
        }

        public override string RenderControl()
        {
            return $@"<div id = ¨{IdHtml}¨ class=¨{Css.Render(enumCssControlesFormulario.ContenedorOpcion)}¨>
                        <input id=¨{IdHtml}¨ 
                               type=¨button¨ 
                               class=¨{Css.Render(enumCssOpcionMenu.Basico)}¨ 
                               value=¨{Etiqueta}¨ 
                               onClick=¨{Accion}¨
                               title=¨{Ayuda}¨/>
                      </div>
                     ";
        }
    }
}
