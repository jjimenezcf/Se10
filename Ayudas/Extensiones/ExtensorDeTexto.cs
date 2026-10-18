using System.IO;
using System.Text.RegularExpressions;
using System.Web;


namespace Utilidades
{
    public static class ExtensorDeTexto
    {
        public static void FileToHtml(string ruta)
        {
            var contenido = File.ReadAllText(ruta);
            contenido = ToHtml(contenido, aplicarEncode: false);
            File.WriteAllText(ruta, contenido);
        }

        public static string ToHtml(string texto, bool aplicarEncode)
        {
            string contenido = texto.IsNullOrEmpty()
                    ? ""
                    : aplicarEncode ? HttpUtility.HtmlEncode(texto) : texto;

            contenido = contenido
                        .Replace("\r\n", "<br>")  // CRLF (Windows)
                        .Replace("\n", "<br>")    // LF (Unix)
                        .Replace("\r", "<br>");   // CR (Mac antiguo)


            return $@"<!DOCTYPE html>
            <html lang=""es"">
            <head>
                <meta charset=""UTF-8"">
                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                <title>Contenido del Correo</title>
            </head>
            <body class='visor-iframe-body'>
                <p>{contenido}</p>
            </body>
            </html>";
        }

        public static bool EsTexto(string contenido)
        {
            // Patrones para detectar etiquetas de marcado comunes
            string htmlPattern = @"<[^>]+>";
            string rtfPattern = @"{\\rtf[^}]+}";
            string pdfPattern = @"/Type\s*/Page|/Obj\s*\d+\s*0\s*obj"; // Búsqueda básica de estructura PDF
            string docxPattern = @"<w:document[^>]+>";
            string excelPattern = @"<(?i:worksheet)[^>]*>";

            // Comprobar si alguno de los patrones coincide
            bool hasHtmlTags = Regex.IsMatch(contenido, htmlPattern, RegexOptions.IgnoreCase);
            bool hasRtfTags = Regex.IsMatch(contenido, rtfPattern);
            bool hasPdfStructure = Regex.IsMatch(contenido, pdfPattern);
            bool hasDocxTags = Regex.IsMatch(contenido, docxPattern, RegexOptions.IgnoreCase);
            bool hasExcelTags = Regex.IsMatch(contenido, excelPattern, RegexOptions.IgnoreCase);

            // Si no se encuentran etiquetas de marcado, se considera texto plano
            return !(hasHtmlTags || hasRtfTags || hasPdfStructure || hasDocxTags || hasExcelTags);
        }

    }
}
