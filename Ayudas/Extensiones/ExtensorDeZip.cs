using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;


namespace Utilidades
{
    public static class ExtensorDeZip
    {
        public static string ToHtml(string zipFilePath)
        {
            StringBuilder html = new StringBuilder();
            html.Append("<table style='border-collapse: collapse; width: 100%;'>");
            //html.Append("<tr style='background-color: #f2f2f2;'><th style='border: 1px solid #ddd; padding: 8px; text-align: left;'>Nombre</th><th style='border: 1px solid #ddd; padding: 8px; text-align: left;'>Tipo</th></tr>");
            html.Append("<tr style='background-color: #f2f2f2;'><th style='border: 1px solid #ddd; padding: 8px; text-align: left;'>Nombre</th></tr>");

            using (ZipArchive archive = ZipFile.OpenRead(zipFilePath))
            {
                var entries = archive.Entries.OrderBy(e => e.FullName).ToList();
                string currentDir = "";

                foreach (var entry in entries)
                {
                    string directory = Path.GetDirectoryName(entry.FullName);
                    if (directory != currentDir)
                    {
                        if (!string.IsNullOrEmpty(currentDir))
                        {
                            html.Append("</table></td></tr>");
                        }
                        currentDir = directory;
                        html.Append($"<tr><td colspan='2' style='border: 1px solid #ddd; padding: 8px;'><strong>{directory}</strong></td></tr>");
                        html.Append("<tr><td colspan='2' style='border: 1px solid #ddd; padding: 8px;'><table style='width: 100%;'>");
                    }

                    string name = Path.GetFileName(entry.FullName);
                    string type = entry.Length == 0 ? "Directorio" : "Archivo";
                    if (type == "Directorio")
                        continue;
                    //html.Append($"<tr><td style='border: 1px solid #ddd; padding: 8px;'>{name}</td><td style='border: 1px solid #ddd; padding: 8px;'>{type}</td></tr>");
                    html.Append($"<tr><td style='border: 1px solid #ddd; padding: 8px;'>{name}</td></tr>");
                }

                if (!string.IsNullOrEmpty(currentDir))
                {
                    html.Append("</table></td></tr>");
                }
            }

            html.Append("</table>");
            return html.ToString();
        }
    }
}
