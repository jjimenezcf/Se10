using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Utilidades
{

    public static class ExtensorDeDocx
    {
        // Pone el tamaño y la orientación de la página SÓLO si el documento no declara ninguno (Word y LibreOffice usan entonces Letter, 612 x 792 pt).
        // Si el documento ya trae su página (apaisado, A3...) no se toca. Modifica el fichero indicado; devuelve si lo ha modificado.
        public static bool PonerPaginaSiNoLaDefine(string rutaDocx, OpcionesDePaginaPdf opciones)
        {
            using (WordprocessingDocument documento = WordprocessingDocument.Open(rutaDocx, true))
            {
                var cuerpo = documento.MainDocumentPart.Document.Body;
                if (cuerpo.Descendants<PageSize>().Any())
                    return false;

                // La sección del final del documento (la que define su última página) puede no existir: se crea
                var secciones = cuerpo.Descendants<SectionProperties>().ToList();
                if (!cuerpo.Elements<SectionProperties>().Any())
                {
                    var final = new SectionProperties();
                    cuerpo.AppendChild(final);
                    secciones.Add(final);
                }

                var (ancho, alto) = opciones.MedidasEnTwips;
                foreach (var seccion in secciones)
                {
                    // Orden del esquema dentro de una sección: referencias, notas, tipo, y luego el tamaño y los márgenes
                    var anterior = seccion.ChildElements.LastOrDefault(e => e is HeaderReference || e is FooterReference || e is FootnoteProperties || e is EndnoteProperties || e is SectionType);
                    var tamano = new PageSize
                    {
                        Width = opciones.EsHorizontal ? alto : ancho,
                        Height = opciones.EsHorizontal ? ancho : alto,
                        Orient = opciones.EsHorizontal ? PageOrientationValues.Landscape : PageOrientationValues.Portrait
                    };
                    if (anterior != null) seccion.InsertAfter(tamano, anterior);
                    else seccion.PrependChild(tamano);

                    if (!seccion.Elements<PageMargin>().Any())
                        seccion.InsertAfter(new PageMargin { Top = 1134, Right = 1134u, Bottom = 1134, Left = 1134u, Header = 709u, Footer = 709u, Gutter = 0u }, tamano);
                }

                documento.MainDocumentPart.Document.Save();
                return true;
            }
        }

        public static string SanitizeDocx(string rutaConFichero)
        {
            if (!File.Exists(rutaConFichero))
                throw new Exception($"El fichero '{rutaConFichero}'no existe");
            try
            {
                string directorio = Path.GetDirectoryName(rutaConFichero);
                string nombreSinExt = Path.GetFileNameWithoutExtension(rutaConFichero);
                string extension = Path.GetExtension(rutaConFichero);
                string rutaSaneada = Path.Combine(directorio, $"{nombreSinExt}_saneado{extension}");

                File.Copy(rutaConFichero, rutaSaneada, true);

                bool seHaModificado = false;

                // El 'using' se encarga de Guardar y Cerrar (Save & Close) automáticamente
                using (WordprocessingDocument doc = WordprocessingDocument.Open(rutaSaneada, true))
                {
                    // 1. ELIMINAR MACROS (VBA)
                    if (doc.MainDocumentPart.VbaProjectPart != null)
                    {
                        doc.MainDocumentPart.DeletePart(doc.MainDocumentPart.VbaProjectPart);
                        seHaModificado = true;
                    }

                    // 2. ELIMINAR OBJETOS OLE
                    var oleParts = doc.MainDocumentPart.EmbeddedObjectParts.ToList();
                    foreach (var ole in oleParts)
                    {
                        doc.MainDocumentPart.DeletePart(ole);
                        seHaModificado = true;
                    }
                    // limpiar las Propiedades del Documento
                    doc.PackageProperties.Creator = "Sistema de elementos";
                    doc.PackageProperties.Description = "Archivo Sanitizado";
                    doc.PackageProperties.LastModifiedBy = "SanitizerBot";
                    // Si hay cambios, se guardan al salir del bloque using
                }

                // Si no hubo cambios, borramos la copia y devolvemos la original
                if (!seHaModificado)
                {
                    if (File.Exists(rutaSaneada)) File.Delete(rutaSaneada);
                    return rutaConFichero;
                }

                return rutaSaneada;
            }
            catch (Exception e)
            {
                throw new Exception($"Error al sanitizar el documento '{rutaConFichero}'", e);
            }
        }

        public static bool EsDoc(string rutaConFichero)
        {
            if (!File.Exists(rutaConFichero))
                throw new Exception($"El fichero '{rutaConFichero}'no existe");

            try
            {
                byte[] buffer = new byte[8];
                using (FileStream fs = new FileStream(rutaConFichero, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    if (fs.Length < 8) return false;
                    fs.ReadExactly(buffer, 0, 8);
                }

                // Firma OLE2 (Legacy Doc): D0 CF 11 E0 A1 B1 1A E1
                return buffer[0] == 0xD0 && buffer[1] == 0xCF && buffer[2] == 0x11 && buffer[3] == 0xE0 &&
                       buffer[4] == 0xA1 && buffer[5] == 0xB1 && buffer[6] == 0x1A && buffer[7] == 0xE1;
            }
            catch
            {
                return false;
            }
        }

        public static bool EsDocx(string rutaConFichero)
        {
            if (!File.Exists(rutaConFichero))
                throw new Exception($"El fichero '{rutaConFichero}'no existe");

            try
            {
                byte[] buffer = new byte[4];
                using (FileStream fs = new FileStream(rutaConFichero, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    if (fs.Length < 4) return false;
                    fs.ReadExactly(buffer, 0, 4);
                }

                // Firma ZIP (Office Open XML): 50 4B 03 04 (PK..)
                // Nota: Esto confirma que es un ZIP. Para asegurar que es DOCX, 
                // validamos también la extensión.
                bool esZip = buffer[0] == 0x50 && buffer[1] == 0x4B && buffer[2] == 0x03 && buffer[3] == 0x04;
                string ext = Path.GetExtension(rutaConFichero).ToLower();

                return esZip && (ext == ".docx" || ext == ".docm" || ext == ".dotx");
            }
            catch
            {
                return false;
            }
        }

        public static string ToHtml(string docxPath)
        {
            using (WordprocessingDocument doc = WordprocessingDocument.Open(docxPath, false))
            {
                StringBuilder html = new StringBuilder("<html><body>");

                var body = doc.MainDocumentPart.Document.Body;

                foreach (var element in body.ChildElements)
                {
                    if (element is Paragraph para)
                    {
                        html.Append("<p>");
                        ProcessParagraph(para, html, doc);
                        html.Append("</p>");
                    }
                    else if (element is Table table)
                    {
                        ProcessTable(table, html, doc);
                    }
                }

                html.Append("</body></html>");
                return html.ToString();
            }
        }

        private static void ProcessParagraph(Paragraph para, StringBuilder html, WordprocessingDocument doc, bool isInTableCell = false)
        {
            foreach (var run in para.Elements<Run>())
            {
                foreach (var childElement in run.ChildElements)
                {
                    if (childElement is Text textElement)
                    {
                        string text = XmlEscape(textElement.Text);
                        ApplyFormatting(run, ref text);
                        html.Append(text);
                    }
                    else if (childElement is Drawing drawing)
                    {
                        ProcessImage(drawing, html, doc, isInTableCell);
                    }
                }
            }
        }


        private static void ProcessTable(Table table, StringBuilder html, WordprocessingDocument doc)
        {
            html.Append("<table border='1'>");
            foreach (var row in table.Elements<TableRow>())
            {
                html.Append("<tr>");
                foreach (var cell in row.Elements<TableCell>())
                {
                    html.Append("<td>");
                    foreach (var para in cell.Elements<Paragraph>())
                    {
                        html.Append("<p>");
                        ProcessParagraph(para, html, doc, true);
                        html.Append("</p>");
                    }
                    html.Append("</td>");
                }
                html.Append("</tr>");
            }
            html.Append("</table>");
        }

        private static void ApplyFormatting(Run run, ref string text)
        {
            var runProperties = run.RunProperties;
            if (runProperties != null)
            {
                if (runProperties.Bold != null)
                    text = $"<strong>{text}</strong>";
                if (runProperties.Italic != null)
                    text = $"<em>{text}</em>";
                if (runProperties.Underline != null && runProperties.Underline.Val != UnderlineValues.None)
                    text = $"<u>{text}</u>";
            }
        }

        private static void ProcessImage(Drawing drawing, StringBuilder html, WordprocessingDocument doc, bool isInTableCell = false)
        {
            var imageId = drawing.Descendants<DocumentFormat.OpenXml.Drawing.Blip>().FirstOrDefault()?.Embed?.Value;
            if (imageId != null)
            {
                var imagePart = (ImagePart)doc.MainDocumentPart.GetPartById(imageId);
                var imageBytes = GetImageBytes(imagePart);
                var base64Image = Convert.ToBase64String(imageBytes);
                var mimeType = imagePart.ContentType;

                // Obtener el tamaño de la imagen
                var extent = drawing.Descendants<DocumentFormat.OpenXml.Drawing.Wordprocessing.Extent>().FirstOrDefault();
                long width = extent?.Cx ?? 0;
                long height = extent?.Cy ?? 0;

                // Convertir de EMUs a píxeles (1 cm = 360000 EMUs, 1 inch = 914400 EMUs)
                int widthInPixels = (int)(width / 9525);
                int heightInPixels = (int)(height / 9525);

                if (isInTableCell)
                {
                    // Limitar el tamaño máximo a 64x64 si está en una celda de tabla
                    widthInPixels = Math.Min(widthInPixels, 64);
                    heightInPixels = Math.Min(heightInPixels, 64);

                    html.Append("<div style=\"display: flex; justify-content: center; align-items: center; height: 100%;\">");
                    html.Append($"<img src=\"data:{mimeType};base64,{base64Image}\" style=\"width: {widthInPixels}px; height: {heightInPixels}px; object-fit: contain;\" />");
                    html.Append("</div>");
                }
                else
                {
                    html.Append($"<img src=\"data:{mimeType};base64,{base64Image}\" width=\"{widthInPixels}\" height=\"{heightInPixels}\" />");
                }
            }
        }



        private static byte[] GetImageBytes(ImagePart imagePart)
        {
            using (var stream = imagePart.GetStream())
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }

        private static string XmlEscape(string text)
        {
            return System.Security.SecurityElement.Escape(text);
        }

        public static string ToTexto(string rutaAlFichero)
        {
            StringBuilder textoPlano = new StringBuilder();

            using (WordprocessingDocument doc = WordprocessingDocument.Open(rutaAlFichero, false))
            {
                var body = doc.MainDocumentPart.Document.Body;

                foreach (var para in body.Elements<Paragraph>())
                {
                    foreach (var run in para.Elements<Run>())
                    {
                        foreach (var text in run.Elements<Text>())
                        {
                            textoPlano.Append(text.Text);
                        }
                    }
                    textoPlano.AppendLine();
                }
            }

            return textoPlano.ToString();
        }

        // La conversión a pdf es común a otros formatos: ver ExtensorDePdf.ConvertirAPdf
        public static string ToPdf(string ficheroDocx, string rutaLibreOffice, Func<string> obtenerClaveConvertApi)
        => ExtensorDePdf.ConvertirAPdf(ficheroDocx, rutaLibreOffice, obtenerClaveConvertApi);

    }

}
