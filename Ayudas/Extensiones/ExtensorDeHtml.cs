using DocumentFormat.OpenXml;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Utilidades
{
    public static class ExtensorDeHtml
    {
        public static string SanitizeFile(string rutaConFichero)
        {
            string contenido = File.ReadAllText(rutaConFichero);
            if (contenido.Trim().Contains("!DOCTYPE html") && contenido.Contains("</html>")) // Mejora esta verificación si es necesario
            {
                string nombreArchivo = Path.GetFileNameWithoutExtension(rutaConFichero);
                string extension = Path.GetExtension(rutaConFichero);
                string directorio = Path.GetDirectoryName(rutaConFichero);

                // Sanitizar el contenido
                contenido = SanitizeHtml(contenido);

                // Construir la nueva ruta para el archivo saneado
                string rutaArchivoSanitizado = Path.Combine(directorio, nombreArchivo + "_saneado" + (!extension.StartsWith(".") ? "." + extension : extension));

                // Escribir el contenido saneado en un nuevo archivo
                File.WriteAllText(rutaArchivoSanitizado, contenido);

                return rutaArchivoSanitizado;
            }

            return rutaConFichero; // Si no es HTML, devuelve la ruta original
        }

        public static string SanitizeContenido(string contenido)
        {
            if (contenido.Trim().Contains("!DOCTYPE html") && contenido.Contains("</html>")) // Mejora esta verificación si es necesario
            {
                return contenido = SanitizeHtml(contenido);
            }
            return contenido;
        }

        public static string SanitizeHtml(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // Eliminar todos los scripts
            var nodes = doc.DocumentNode.SelectNodes("//script");
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    node.Remove();
                }
            }

            // Eliminar atributos on* (onclick, onload, etc.)
            nodes = doc.DocumentNode.SelectNodes("//*[@*[starts-with(name(), 'on')]]");
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    foreach (var attribute in node.Attributes.ToList())
                    {
                        if (attribute.Name.StartsWith("on"))
                        {
                            node.Attributes.Remove(attribute);
                        }
                    }
                }
            }

            return doc.DocumentNode.OuterHtml;
        }

        // Saneado para convertir un html a pdf en el servidor. Además de los scripts y los atributos on*, quita todo lo que haría
        // que el motor de conversión cargase algo de fuera (marcos, objetos, hojas de estilo, imágenes que no estén incrustadas...)
        // o ejecutase algo; lo que queda es el documento y sus imágenes incrustadas (data:image).
        // No sustituye a SanitizeHtml, que es el que se aplica a lo que se muestra en el visor.
        public static string SanitizeHtmlParaPdf(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var prohibidas = new[] { "script", "iframe", "frame", "frameset", "object", "embed", "applet", "link", "base", "svg", "math", "video", "audio", "source", "track", "portal", "template" };
            foreach (var etiqueta in prohibidas)
                QuitarNodos(doc, $"//{etiqueta}");
            QuitarNodos(doc, "//meta[@http-equiv]"); // refresh y similares
            QuitarNodos(doc, "//meta[@charset]");    // la codificación se fija más abajo: se escribe siempre en utf-8

            // Sólo se admiten imágenes incrustadas; las que apuntan a una ruta o a una dirección harían que el servidor la leyese o la pidiese
            var imagenes = doc.DocumentNode.SelectNodes("//img");
            if (imagenes != null)
                foreach (var imagen in imagenes.ToList())
                {
                    var origen = Regex.Replace(imagen.GetAttributeValue("src", ""), @"[\s\u0000-\u001f]", "");
                    var incrustada = origen.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase)
                                  && !origen.StartsWith("data:image/svg", StringComparison.OrdinalIgnoreCase);
                    if (!incrustada)
                        imagen.Remove();
                }

            var elementos = doc.DocumentNode.SelectNodes("//*");
            if (elementos != null)
                foreach (var elemento in elementos.ToList())
                {
                    foreach (var atributo in elemento.Attributes.ToList())
                    {
                        var nombre = atributo.Name.ToLower();
                        var valor = Regex.Replace(atributo.Value ?? "", @"[\s\u0000-\u001f]", "").ToLower();

                        var esEvento = nombre.StartsWith("on");
                        var esRecurso = nombre == "background" || nombre == "poster" || nombre == "srcset" || nombre == "ping" || nombre == "data";
                        var esSrcFueraDeUnaImagen = nombre == "src" && elemento.Name.ToLower() != "img";
                        var esCodigo = (nombre == "href" || nombre == "src" || nombre == "action" || nombre == "formaction" || nombre == "xlink:href")
                                    && (valor.StartsWith("javascript:") || valor.StartsWith("vbscript:") || valor.StartsWith("data:text"));

                        if (esEvento || esRecurso || esSrcFueraDeUnaImagen || esCodigo)
                            elemento.Attributes.Remove(atributo);
                        else if (nombre == "style")
                            atributo.Value = LimpiarCss(atributo.Value);
                    }

                    if (elemento.Name.ToLower() == "style")
                        elemento.InnerHtml = LimpiarCss(elemento.InnerText);
                }

            // Se escribe en utf-8: se indica así dentro del documento (si no tuviera cabecera se le crea)
            var meta = "<meta charset=\"utf-8\">";
            var cabecera = doc.DocumentNode.SelectSingleNode("//head");
            var raiz = doc.DocumentNode.SelectSingleNode("//html");
            if (cabecera != null)
                cabecera.PrependChild(HtmlNode.CreateNode(meta));
            else if (raiz != null)
                raiz.PrependChild(HtmlNode.CreateNode($"<head>{meta}</head>"));
            else
                return $"<html><head>{meta}</head><body>{doc.DocumentNode.OuterHtml}</body></html>";

            return doc.DocumentNode.OuterHtml;
        }

        private static void QuitarNodos(HtmlDocument doc, string xpath)
        {
            var nodos = doc.DocumentNode.SelectNodes(xpath);
            if (nodos == null)
                return;

            foreach (var nodo in nodos.ToList())
                nodo.Remove();
        }

        // Quita de un css lo que cargaría algo de fuera: @import y url() que no sean incrustados. Las barras se eliminan para
        // que no se pueda escribir "url(" con secuencias de escape.
        private static string LimpiarCss(string css)
        {
            css = (css ?? "").Replace("\\", "");
            css = Regex.Replace(css, @"@import[^;]*;?", "", RegexOptions.IgnoreCase);
            css = Regex.Replace(css, @"url\s*\(\s*(?!['""]?\s*data:image/(?!svg))[^)]*\)", "none", RegexOptions.IgnoreCase);
            css = Regex.Replace(css, @"expression\s*\([^)]*\)", "none", RegexOptions.IgnoreCase);
            return css;
        }

        public static bool EsHtml(string contenido)
        {
            return contenido.Contains("</html>") || contenido.Contains("</body>") || contenido.Contains("</div>");
        }

        public static bool HayCabeceraHtml(string ruta)
        {
            var contenido = File.ReadAllText(ruta);
            return contenido.Contains("</html>");
        }

        public static bool EsTextoPlano(string contenido)
        {
            return !contenido.Contains("<html") && !contenido.Contains("<body");
        }

        public static string TextoToHtml(string texto)
        {
            return $@"<!DOCTYPE html>
                    <html lang=""es"">
                    <head>
                        <meta charset=""UTF-8"">
                        <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                        <title>Contenido del Correo</title>
                    </head>
                    <body>
                        {SanitizeHtml("<pre>" + (texto.IsNullOrEmpty() ? "" : HttpUtility.HtmlEncode(texto)) + "</pre>")}
                    </body>
                    </html>";

        }

        public static string ToTexto(string htmlContent)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(htmlContent);

            // StringBuilder para construir el texto
            StringBuilder textBuilder = new StringBuilder();

            // Eliminar la etiqueta <div> envolvente y extraer el texto
            var divNode = doc.DocumentNode.SelectSingleNode("//div");
            if (divNode != null)
            {
                foreach (var child in divNode.ChildNodes)
                {
                    textBuilder.Append(ExtractText(child));
                }
                divNode.Remove();
            }
            else
            {
                textBuilder.Append(ExtractText(doc.DocumentNode));
            }

            // Extraer el texto
            string text = textBuilder.ToString();

            // Decodificar entidades HTML
            text = WebUtility.HtmlDecode(text);

            // Limpiar espacios en blanco extra y saltos de línea múltiples
            //text = Regex.Replace(text, @"\s+", " ");
            //text = Regex.Replace(text, $"{Environment.NewLine}{Environment.NewLine}+", $"{Environment.NewLine}{Environment.NewLine}");
            text = text.Trim();

            return text;
        }

        private static string ExtractText(HtmlNode node)
        {
            StringBuilder text = new StringBuilder();

            if (node is HtmlTextNode)
            {
                text.Append(((HtmlTextNode)node).Text);
            }
            else if (node.HasChildNodes)
            {
                foreach (var child in node.ChildNodes)
                {
                    text.Append(ExtractText(child));
                }
            }
            else if (node.Name == "br")
            {
                text.Append(Environment.NewLine);
            }
            else if (node.Name == "p")
            {
                text.Append(Environment.NewLine + Environment.NewLine);
            }
            else if (node.Name == "li")
            {
                text.Append(Environment.NewLine + node.InnerText + Environment.NewLine);
            }

            return text.ToString();
        }

        public static string ReemplazarImagenesEnHtml(string htmlContent, List<string> rutas)
        {
            try
            {
                if (string.IsNullOrEmpty(htmlContent) || !rutas.Any())
                {
                    return htmlContent; // Si no hay contenido o adjuntos, devolvemos el HTML original
                }

                // Usamos HtmlAgilityPack para analizar el contenido HTML
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(htmlContent);

                // Seleccionamos todas las etiquetas <img>
                var imagenes = htmlDoc.DocumentNode.SelectNodes("//img");

                if (imagenes == null)
                {
                    return htmlContent; // Si no hay imágenes, devolvemos el HTML original
                }

                foreach (var img in imagenes)
                {
                    var src = img.GetAttributeValue("src", null); // Obtenemos el atributo src

                    if (!string.IsNullOrEmpty(src) && src.StartsWith("cid:"))
                    {
                        var cid = src.Substring(4); // Extraemos el CID eliminando "cid:"

                        // Buscamos el adjunto correspondiente
                        var ruta = rutas.FirstOrDefault(x => x.Contains(cid));

                        if (ruta != null)
                        {

                            if (File.Exists(ruta))
                            {
                                // Leemos el contenido del archivo y lo codificamos en base64
                                var bytes = File.ReadAllBytes(ruta);
                                var base64 = Convert.ToBase64String(bytes);

                                // Determinamos el tipo MIME del archivo (asumiendo que son imágenes)
                                if (!ExtensorDeTipoDeArchivos.EsImagen(Path.GetExtension(ruta), errorSiNoEstaCatalogada: false))
                                    continue;

                                var mimeType = MimeTypeMap.GetMimeTypeFromFilename(ruta);

                                // Construimos el esquema data: para incrustar la imagen
                                var dataUri = $"data:{mimeType};base64,{base64}";

                                // Reemplazamos el atributo src con la URI de datos
                                img.SetAttributeValue("src", dataUri);
                            }
                        }
                    }

                    // Devolvemos el HTML modificado como una cadena
                }
                return htmlDoc.DocumentNode.OuterHtml;
            }
            catch
            {
                return htmlContent;
            }

        }
    }

}

