using SevenZipExtractor;
using System.IO;
using System.Linq;
using System.Text;


namespace Utilidades
{
    public static class ExtensorDe7z
    {
        public static string ToHtml(string sevenZipFilePath)
        {
            StringBuilder html = new StringBuilder();
            html.Append("<table style='border-collapse: collapse; width: 100%;'>");
            html.Append("<tr style='background-color: #f2f2f2;'><th style='border: 1px solid #ddd; padding: 8px; text-align: left;'>Nombre</th></tr>");

            using (ArchiveFile archiveFile = new ArchiveFile(sevenZipFilePath))
            {
                var entries = archiveFile.Entries.OrderBy(e => e.FileName).ToList();
                string currentDir = "";

                foreach (var entry in entries)
                {
                    string directory = Path.GetDirectoryName(entry.FileName);
                    if (directory != currentDir)
                    {
                        if (!string.IsNullOrEmpty(currentDir))
                        {
                            html.Append("</table></td></tr>");
                        }
                        currentDir = directory;
                        html.Append($"<tr><td style='border: 1px solid #ddd; padding: 8px;'><strong>{directory}</strong></td></tr>");
                        html.Append("<tr><td style='border: 1px solid #ddd; padding: 8px;'><table style='width: 100%;'>");
                    }

                    string name = Path.GetFileName(entry.FileName);
                    if (entry.IsFolder)
                        continue;

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
