using DocumentFormat.OpenXml.Drawing.Diagrams;
using System.ComponentModel;

namespace MVCSistemaDeElementos.UtilidadesIu
{
    public enum enumAccionVisorArchivo
    {
        [Description("pasar-ocr")]
        PasarOcr,
        [Description("resumir-archivo")]
        Resumir,
        [Description("analizar-factura")]
        AnalizarFactura,
        [Description("siguiente-archivo")]
        Siguiente,
        [Description("anterior-archivo")]
        Anterior,
        [Description("decargar-archivo")]
        Descargar,
        [Description("zoom-mas")]
        ZoomMas,
        [Description("zoom-menos")]
        ZoomMenos
    }

    // Mismo robot verde que ApiPreguntasIa.SvgRobot (ts), para los botones que se pintan desde el servidor
    public static class IconosIa
    {
        public const string Robot = @"<svg width=""14"" height=""14"" viewBox=""0 0 16 16"" fill=""none"" stroke=""currentColor"" stroke-width=""1.3"" stroke-linecap=""round"" stroke-linejoin=""round"">
<line x1=""5"" y1=""1.8"" x2=""6.2"" y2=""4""/><line x1=""11"" y1=""1.8"" x2=""9.8"" y2=""4""/>
<circle cx=""5"" cy=""1.6"" r=""0.7"" fill=""currentColor"" stroke=""none""/><circle cx=""11"" cy=""1.6"" r=""0.7"" fill=""currentColor"" stroke=""none""/>
<rect x=""2.5"" y=""4"" width=""11"" height=""9.5"" rx=""2.2""/>
<circle cx=""6"" cy=""8"" r=""1.1"" fill=""currentColor"" stroke=""none""/><circle cx=""10"" cy=""8"" r=""1.1"" fill=""currentColor"" stroke=""none""/>
<line x1=""6"" y1=""11.2"" x2=""10"" y2=""11.2""/></svg>";
    }

    public static class parametrosMvc
    {
        /*
        public static List<ClausulaDeOrdenacion> ParsearOrdenacion(this string orden)
        {
            var ordenParseado = new List<ClausulaDeOrdenacion>();

            if (!orden.IsNullOrEmpty())
            {
                var ordenes = orden.Split(';');
                var i = 0;
                while (i < ordenes.Length)
                {
                    if (ordenes[i].IsNullOrEmpty())
                        break;
                    else
                    {
                        var clausula = new ClausulaDeOrdenacion();
                        clausula.Criterio = ordenes[i];
                        clausula.Modo = ModoDeOrdenancion.ascendente;

                        if (i + 1 < ordenes.Length && ordenes[i + 1] == ModoDeOrdenancion.descendente.ToString())
                            clausula.Modo = ModoDeOrdenancion.descendente;

                        ordenParseado.Add(clausula);
                        i = i + 2;
                    }

                }
            }

            return ordenParseado;
        }
        */
    }
}
