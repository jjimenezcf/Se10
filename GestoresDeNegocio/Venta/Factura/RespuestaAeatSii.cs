using Azure;
using HtmlAgilityPack;

namespace GestoresDeNegocio.Venta
{
    public class RespuestaAeatSii
    {
        public bool EsErronea { get; internal set; }
        public string CodigoDeError { get; internal set; }
        public string MensajeDeError { get; internal set; }


        public static RespuestaAeatSii Parsear(string htmlContent)
        {
            var respuesta = new RespuestaAeatSii();
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);

            // Buscar el código de error
            var nodoDelCodigoDeError = htmlDoc.DocumentNode.SelectSingleNode("//h1[contains(@class, 'text-danger')]");
            if (nodoDelCodigoDeError != null)
            {
                respuesta.EsErronea = true;
                respuesta.CodigoDeError = nodoDelCodigoDeError.InnerText.Trim();

                var nodoMain = htmlDoc.DocumentNode.SelectSingleNode("//main");
                if (nodoMain != null)
                {
                    var nodoDelMensajeErroneo = nodoMain.SelectSingleNode(".//p");
                    if (nodoDelMensajeErroneo != null)
                    {
                        respuesta.MensajeDeError = nodoDelMensajeErroneo.InnerText.Trim();
                    }
                }
            }

            return respuesta;
        }
    }
}


