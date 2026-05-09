using Utilidades;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DivEnBlanco : ControlHtml
    {
        public DivEnBlanco(ControlHtml padre, string postId) 
        : base(padre: padre, $"{padre.Id}-blanco-{postId}", "", "", "", null)
        {
            Tipo = enumTipoControl.Bloque;
        }

        public override string RenderControl()
        {

            var a = new AtributosHtml(
                idHtml: IdHtml,
                propiedad: "",
                tipoDeControl: Tipo,
                visible: true,
                editable: false,
                obligatorio: false,
                ayuda: null,
                valorPorDefecto: null);

            return RenderDivEnBlanco(a);
        }

        public static string RenderDivEnBlanco(AtributosHtml atributos)
        {
            var valores = atributos.MapearComunes();
            valores["Css"] = enumCssControles.DivEnBlanco.Render();

            var htmlDivEnBlanco = PlantillasHtml.Render(PlantillasHtml.DivEnBlanco, valores);

            return htmlDivEnBlanco;
        }
    }
}
