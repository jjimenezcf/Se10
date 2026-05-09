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
