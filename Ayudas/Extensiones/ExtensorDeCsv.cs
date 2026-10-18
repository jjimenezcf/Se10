using System.IO;
using System.Text;


namespace Utilidades
{
    public static class ExtensorDeCsv
    {
        public static string ToHtml(string filePath)
        {
            StringBuilder html = new StringBuilder();
            html.Append("<table style='border-collapse: collapse; width: 100%; margin-bottom: 20px;'>");

            string[] lines = File.ReadAllLines(filePath);

            foreach (string line in lines)
            {
                html.Append("<tr style='white-space: nowrap;'>");
                string[] cells = line.Split(Simbolos.separadorCss);

                foreach (string cell in cells)
                {
                    html.Append($"<td style='border: 1px solid #ddd; padding: 8px; overflow: hidden; text-overflow: ellipsis;'>{cell}</td>");
                }

                html.Append("</tr>");
            }

            html.Append("</table>");
            return html.ToString();
        }

    }
}
