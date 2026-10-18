using System;


namespace Utilidades
{
    public enum enumExtensiones { pdf, docx, xml, html, xlsx, png, jpg, jpeg, svg, se, txt, json, rtf, zip, csv, p12, pfx, cer, webp, fit }

    public static class ExtensorDeTipoDeArchivos
    {
        public const string Imagenes = ".png, .jpg, .jpeg, .svg .webp";

        public const string NoEditables = ".pdf, " + Imagenes;

        public const string NoEditablesMasJson = ".pdf, .json, " + Imagenes;

        public const string NoEditablesMasJsonMasXml = ".xml, " + NoEditablesMasJson;

        public const string Certificados = ".cer, .pfx, .p12";

        public static readonly string xml = "." + enumExtensiones.xml.ToString();

        public static bool EsImagen(string extensionstring, bool errorSiNoEstaCatalogada, bool excluirSvg = false)
        {
            var extension = IntentarParsear(extensionstring);

            if (errorSiNoEstaCatalogada && extension == null)
            {
                throw new Exception("La extensión '" + extensionstring + "' no es una extensión de archivo válida");
            }

            //extension != ".jpg" && extension != ".jpeg" && extension != ".png" && extension != ".webp"

            if (excluirSvg && extension.HasValue && extension == enumExtensiones.svg) return false;

            return extension.HasValue && (extension == enumExtensiones.png ||
                extension == enumExtensiones.jpg ||
                extension == enumExtensiones.jpeg ||
                extension == enumExtensiones.svg ||
                extension == enumExtensiones.webp);
        }

        public static bool EsSvg(string extensionstring, bool errorSiNoEstaCatalogada)
        {
            var extension = IntentarParsear(extensionstring);
            if (errorSiNoEstaCatalogada && extension == null)
            {
                throw new Exception("La extensión '" + extensionstring + "' no es una extensión de archivo válida");
            }
            return extension.HasValue && extension == enumExtensiones.svg;
        }
        public static string Render(this enumExtensiones extension) => $".{extension}";

        public static bool EsHtml(string extension)
        {
            return Parser(extension, defecto: enumExtensiones.txt) == enumExtensiones.html;
        }

        public static enumExtensiones Parser(string extension, enumExtensiones? defecto = null)
        {
            var enumExt = ApiDeEnsamblados.ToEnumerado<enumExtensiones>(extension.Replace(".", ""), errorSiNoEsValido: false);
            if (enumExt == null && defecto == null)
                throw new Exception("La extensión '" + extension + "' no es una extensión de archivo válida");
            if (enumExt == null)
                enumExt = defecto;
            return enumExt.Value;
        }


        public static enumExtensiones? IntentarParsear(string extension)
        {
            var enumExt = ApiDeEnsamblados.ToEnumerado<enumExtensiones>(extension.Replace(".", ""), errorSiNoEsValido: false);
            if (enumExt == null)
                return null;
            return enumExt.Value;
        }
    }

}
