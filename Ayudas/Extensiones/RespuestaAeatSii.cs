using HtmlAgilityPack;

namespace Utilidades
{
    public class RespuestaAETSii
    {
        public bool IsError { get; internal set; }
        public string ErrorCode { get; internal set; }
        public string ErrorMessage { get; internal set; }
               

        public RespuestaAETSii Parsear(string htmlContent)
        {
            var response = new RespuestaAETSii();
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);

            // Buscar el código de error
            var errorCodeNode = htmlDoc.DocumentNode.SelectSingleNode("//h1[@class='text-danger']");
            if (errorCodeNode != null)
            {
                response.IsError = true;
                response.ErrorCode = errorCodeNode.InnerText.Trim();
            }

            // Buscar el mensaje de error
            var errorMessageNode = htmlDoc.DocumentNode.SelectSingleNode("//p[following-sibling::ul]");
            if (errorMessageNode != null)
            {
                response.ErrorMessage = errorMessageNode.InnerText.Trim();
            }

            return response;
        }
    }
}
