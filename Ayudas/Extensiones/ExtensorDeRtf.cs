using RtfPipe;
using System;
using System.IO;


namespace Utilidades
{
    public static class ExtensorDeRtf
    {
        public static string ToHtml(string rutaAlFichero)
        {
            string rtfContent = File.ReadAllText(rutaAlFichero);
            return Rtf.ToHtml(rtfContent);
        }

        public static string ToTexto(string rutaAlFichero)
        {
            try
            {
                // Convertir RTF a HTML
                string html = ExtensorDeRtf.ToHtml(rutaAlFichero);

                // Eliminar las etiquetas HTML para obtener texto plano
                return ExtensorDeHtml.ToTexto(html);
            }
            catch (Exception ex)
            {
                return "Error al leer el archivo RTF: " + ex.Message;
            }
        }
    }
}
