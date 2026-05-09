using System;
using Utilidades;

namespace MVCSistemaDeElementos.Descriptores
{
    public class EditorDeTexto : ControlHtml
    {
        int LongitudMaxima { get; set; }

        public EditorDeTexto(ControlHtml padre, string etiqueta, string propiedad, string ayuda, int longitudMaxima = 0) :
        base(padre: padre, $"{padre.Id}-{propiedad}", etiqueta, propiedad, ayuda, null)
        {
            Tipo = enumTipoControl.Editor;
            LongitudMaxima = longitudMaxima;
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

            a.LongitudMaxima = LongitudMaxima;

            return RenderEditorDeTexto(a);
        }

        public static string RenderEditorDeTexto(AtributosHtml atributos)
        {
            var valores = atributos.MapearComunes();
            valores["CssContenedor"] = Css.Render(enumCssControles.ContenedorEditor);
            valores["Css"] = Css.Render(enumCssControles.Editor);
            valores["LongitudMaxima"] = atributos.LongitudMaxima > 0 ? $"{Environment.NewLine}maxlength=¨{atributos.LongitudMaxima}¨" : "";
            valores["Placeholder"] = atributos.Ayuda;
            valores["ValorPorDefecto"] = atributos.ValorPorDefecto;

            valores["ocultar_div"] = "";

            var htmlEditorTexto = PlantillasHtml.Render(PlantillasHtml.editorDto, valores);

            htmlEditorTexto = htmlEditorTexto.Replace("[onBlur]", "");
            htmlEditorTexto = htmlEditorTexto.Replace("[Navegador]", "");

            return htmlEditorTexto;
        }


        public static string RenderEditorDeTextoParaFiltrar(AtributosHtml atributos)
        {
            var valores = atributos.MapearComunes();
            valores["CssContenedor"] = Css.Render(enumCssControles.ContenedorEditor);
            valores["Css"] = Css.Render(enumCssControles.Editor);
            valores["LongitudMaxima"] = atributos.LongitudMaxima > 0 ? $"{Environment.NewLine}maxlength=¨{atributos.LongitudMaxima}¨" : "";
            valores["Placeholder"] = atributos.Ayuda;
            valores["ValorPorDefecto"] = atributos.ValorPorDefecto;

            valores["ocultar_div"] = "";

            var htmlEditorTexto = PlantillasHtml.Render(PlantillasHtml.editorFlt, valores);

            htmlEditorTexto = htmlEditorTexto.Replace("[onBlur]", "");
            htmlEditorTexto = htmlEditorTexto.Replace($"style = '[Estilos]'", "");
            htmlEditorTexto = htmlEditorTexto.Replace($"criterio-de-filtro='[CriterioDeFiltrado]'", "");
            htmlEditorTexto = htmlEditorTexto.Replace($"[Formato]", "");
            htmlEditorTexto = htmlEditorTexto.Replace($"valor-de-defecto=''", "");

            return htmlEditorTexto;
        }


    }
}
