using ModeloDeDto;
using System;
using System.Collections.Generic;
using Utilidades;

namespace UtilidadesParaIu
{

    public static class HtmlRender
    {
        public static string Render(this string cadena)
        {
            while (cadena.IndexOf("< ") >= 0)
                cadena = cadena.Replace("< ", "<");


            while (cadena.IndexOf("  ") >= 0)
                cadena = cadena.Replace("  ", " ");

            return cadena.Replace("¨", "\"");
        }

        public static string Render(this enumAliniacion alineacion)
        {
            switch (alineacion)
            {
                case enumAliniacion.izquierda:
                    return "left";
                case enumAliniacion.derecha:
                    return "right";
                case enumAliniacion.centrada:
                    return "center";
                case enumAliniacion.justificada:
                    return "justify";
                default:
                    return "left";
            }
        }

        public static string RenderOptions(this Dictionary<string, string> opciones)
        {
            var ocionesHtml = "";
            foreach (var opcion in opciones)
            {
                var opcionHtml = $"<option value='{opcion.Key.Trim().ToLower()}'>{opcion.Value}</option>";
                ocionesHtml = $"{(opcionHtml.IsNullOrEmpty() ? opcionHtml : ocionesHtml + Environment.NewLine + opcionHtml)}";
            }
            return ocionesHtml;
        }
    }
}
