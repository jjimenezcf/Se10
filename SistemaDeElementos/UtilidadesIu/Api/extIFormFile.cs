using Microsoft.AspNetCore.Http;
using Utilidades;

namespace SistemaDeElementos.UtilidadesIu
{
    public static class extIFormFile
    {
        public static bool EsCsv(this IFormFile fichero)
        {
            return fichero.ContentType.Contains("csv")
                || fichero.ContentType == "application/vnd.ms-excel";
        }

        public static bool EsDocx(this IFormFile fichero)
        {
            return fichero.ContentType.Contains("openxmlformats-officedocument");
        }

        public static bool EsXml(this IFormFile fichero)
        {
            return fichero.ContentType.Contains("text/xml");
        }

        public static bool EsZip(this IFormFile fichero)
        {
            return fichero.ContentType.Contains("application/x-zip-compressed");
        }

        public static bool EsPdf(this IFormFile fichero)
        {
            return fichero.ContentType.Contains("pdf");
        }

        public static bool EsCertificado(this IFormFile fichero)
        {
            return fichero.ContentType.Contains("application/x-pkcs12");
        }

        public static bool EsImagen(this IFormFile fichero)
        {
            return fichero.ContentType == "image/jpeg"
                || fichero.ContentType == "image/png"
                || fichero.ContentType == "image/gif"
                || fichero.ContentType == "image/jpg"
                || fichero.ContentType == "image/vnd.Microsoft.icon"
                || fichero.ContentType == "image/x-icon"
                || fichero.ContentType == "image/vnd.djvu"
                || fichero.ContentType == "image/svg+xml";
        }

        public static bool EsTexto(this IFormFile fichero)
        {
            return fichero.ContentType == MimeTypeMap.TextPlain
                || fichero.ContentType == MimeTypeMap.ApplicationJson;
        }

    }
}
