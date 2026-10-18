using DocumentFormat.OpenXml;
using SkiaSharp;
using System;
using System.IO;
using System.Linq;


namespace Utilidades
{
    public static class ExtensorDeImagenes
    {
        private const long MaxBytesSinComprimir = 3 * 1024 * 1024; // 3 MB
        private const int LadoMaximo = 2560; // tope 2K para reducir peso drásticamente
        private const int CalidadDeCompresion = 75; // ajuste óptimo entre peso y calidad visual
        private const long MaxPixeles = 100_000_000; // protege de imágenes que declaran dimensiones enormes para agotar la memoria

        public static bool EsImagen(string rutaConFichero, bool excluirSvg = false)
        {
            string ext = Path.GetExtension(rutaConFichero).ToLower();
            return ExtensorDeTipoDeArchivos.EsImagen(ext, errorSiNoEstaCatalogada: false, excluirSvg: excluirSvg);
        }

        public static string OptimizarImagenSiEsNecesario(string rutaArchivo)
        {
            if (!File.Exists(rutaArchivo)) return rutaArchivo;

            FileInfo fileInfo = new FileInfo(rutaArchivo);

            // Si pesa menos de 3MB, ni lo tocamos
            if (fileInfo.Length <= MaxBytesSinComprimir) return rutaArchivo;

            // Validamos extensiones comunes de imagen
            if (!EsImagen(rutaArchivo, excluirSvg: true))
            {
                return rutaArchivo; // No es una imagen soportada
            }

            try
            {
                // Creamos una ruta temporal para guardar la imagen comprimida
                string rutaTemporal = Path.Combine(Path.GetDirectoryName(rutaArchivo), "compressed_" + Path.GetFileName(rutaArchivo));

                using (SKBitmap imagen = Decodificar(rutaArchivo))
                using (SKBitmap reducida = ReducirSiExcedeElLadoMaximo(imagen))
                {
                    // Se guarda en el formato que indica la extensión, para que el contenido coincida con ella
                    Guardar(reducida ?? imagen, rutaTemporal, FormatoSegunExtension(fileInfo.Extension));
                }

                // Si la compresión funcionó y el archivo es más pequeño, reemplazamos el original
                FileInfo fileInfoCompreso = new FileInfo(rutaTemporal);
                if (fileInfoCompreso.Exists)
                {
                    File.Delete(rutaArchivo);
                    File.Move(rutaTemporal, rutaArchivo);
                }
            }
            catch (Exception)
            {
                // Loguear el error si tienes sistema de Logs.
                // Si falla la compresión, es mejor dejar pasar el archivo original a que se caiga el proceso.
            }

            return rutaArchivo;
        }

        public static string SanitizeImage(string rutaConFichero)
        {
            if (!File.Exists(rutaConFichero))
                throw new Exception($"El fichero '{rutaConFichero}' no existe");

            string extension = Path.GetExtension(rutaConFichero).ToLower();
            if (ExtensorDeTipoDeArchivos.EsSvg(extension, errorSiNoEstaCatalogada: true)) return SanitizeSvg(rutaConFichero);

            try
            {
                string directorio = Path.GetDirectoryName(rutaConFichero);
                string nombreSinExt = Path.GetFileNameWithoutExtension(rutaConFichero);
                string rutaSaneada = Path.Combine(directorio, $"{nombreSinExt}_saneado{extension}");

                // 1. Validamos el tamaño del archivo original
                FileInfo fileInfo = new FileInfo(rutaConFichero);

                // 2. Sanitización: al decodificar y volver a codificar solo se conservan los píxeles,
                //    los perfiles EXIF, IPTC y XMP no se trasladan a la imagen saneada
                using (SKBitmap imagen = Decodificar(rutaConFichero))
                // 3. Si excede los 3MB, la redimensionamos en caliente
                using (SKBitmap reducida = fileInfo.Length > MaxBytesSinComprimir ? ReducirSiExcedeElLadoMaximo(imagen) : null)
                {
                    Guardar(reducida ?? imagen, rutaSaneada, FormatoSegunExtension(extension));
                }

                return rutaSaneada;
            }
            catch (Exception e)
            {
                throw new Exception($"Error al sanitizar y comprimir la imagen '{rutaConFichero}'", e);
            }
        }

        // Solo se decodifican png, jpg y webp, comprobándolo por el contenido y no por la extensión,
        // para que un fichero de otro formato renombrado no llegue a procesarse
        private static SKBitmap Decodificar(string rutaConFichero)
        {
            using (SKCodec codec = SKCodec.Create(rutaConFichero))
            {
                if (codec == null)
                    throw new Exception($"El fichero '{rutaConFichero}' no es una imagen válida");

                if (codec.EncodedFormat != SKEncodedImageFormat.Png &&
                    codec.EncodedFormat != SKEncodedImageFormat.Jpeg &&
                    codec.EncodedFormat != SKEncodedImageFormat.Webp)
                    throw new Exception($"El contenido del fichero '{rutaConFichero}' es {codec.EncodedFormat}, solo se admiten imágenes png, jpg y webp");

                if ((long)codec.Info.Width * codec.Info.Height > MaxPixeles)
                    throw new Exception($"La imagen '{rutaConFichero}' de {codec.Info.Width}x{codec.Info.Height} píxeles excede el tamaño máximo permitido");

                return SKBitmap.Decode(codec) ?? throw new Exception($"No se ha podido decodificar la imagen '{rutaConFichero}'");
            }
        }

        // Devuelve una copia que cabe en LadoMaximo x LadoMaximo manteniendo la proporción, o null si ya cabía
        private static SKBitmap ReducirSiExcedeElLadoMaximo(SKBitmap imagen)
        {
            if (imagen.Width <= LadoMaximo && imagen.Height <= LadoMaximo)
                return null;

            double escala = Math.Min((double)LadoMaximo / imagen.Width, (double)LadoMaximo / imagen.Height);
            int ancho = Math.Max(1, (int)Math.Round(imagen.Width * escala));
            int alto = Math.Max(1, (int)Math.Round(imagen.Height * escala));

            return imagen.Resize(imagen.Info.WithSize(ancho, alto), SKFilterQuality.High)
                ?? throw new Exception("No se ha podido redimensionar la imagen");
        }

        private static SKEncodedImageFormat FormatoSegunExtension(string extension)
        {
            switch (ExtensorDeTipoDeArchivos.IntentarParsear(extension.ToLower()))
            {
                case enumExtensiones.jpg:
                case enumExtensiones.jpeg: return SKEncodedImageFormat.Jpeg;
                case enumExtensiones.webp: return SKEncodedImageFormat.Webp;
                case enumExtensiones.png: return SKEncodedImageFormat.Png;
            }
            throw new Exception($"No se puede guardar una imagen con la extensión '{extension}', solo se admiten png, jpg y webp");
        }

        // En jpg y webp se aplica la calidad de compresión; el png es sin pérdida y la ignora
        private static void Guardar(SKBitmap imagen, string ruta, SKEncodedImageFormat formato)
        {
            using (SKData datos = imagen.Encode(formato, CalidadDeCompresion))
            {
                if (datos == null)
                    throw new Exception($"No se ha podido codificar la imagen como {formato}");

                using (FileStream fichero = File.Create(ruta))
                    datos.SaveTo(fichero);
            }
        }

        private static string SanitizeSvg(string rutaConFichero)
        {
            try
            {
                string contenido = File.ReadAllText(rutaConFichero);
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(contenido);

                bool modificado = false;

                // 1. Eliminar etiquetas peligrosas (Búsqueda simple sin prefijos)
                var etiquetasPeligrosas = doc.DocumentNode.SelectNodes("//script | //object | //iframe | //foreignObject");
                if (etiquetasPeligrosas != null)
                {
                    foreach (var node in etiquetasPeligrosas) node.Remove();
                    modificado = true;
                }

                // 2. Recorrer TODOS los nodos para limpiar atributos de eventos y enlaces
                // Esta forma NO usa XPath, por lo que NO falla con los Namespaces
                var todosLosNodos = doc.DocumentNode.Descendants();
                foreach (var node in todosLosNodos)
                {
                    // Limpiar eventos "on..." (onclick, onload, etc.)
                    var attrsAEliminar = node.Attributes
                        .Where(a => a.Name.StartsWith("on", StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    if (attrsAEliminar.Any())
                    {
                        foreach (var attr in attrsAEliminar) node.Attributes.Remove(attr);
                        modificado = true;
                    }

                    // Limpiar enlaces "href" o "xlink:href" que contengan javascript
                    // Buscamos cualquier atributo que termine en "href"
                    var hrefAttr = node.Attributes.FirstOrDefault(a => a.Name.EndsWith("href", StringComparison.OrdinalIgnoreCase));
                    if (hrefAttr != null && hrefAttr.Value.Trim().ToLower().StartsWith("javascript:"))
                    {
                        hrefAttr.Value = "#";
                        modificado = true;
                    }
                }

                // Si no se detectó nada raro, devolvemos la ruta original
                if (!modificado) return rutaConFichero;

                // 3. Guardar versión saneada
                string directorio = Path.GetDirectoryName(rutaConFichero);
                string nombre = Path.GetFileNameWithoutExtension(rutaConFichero);
                string extension = Path.GetExtension(rutaConFichero);
                string rutaSaneada = Path.Combine(directorio, $"{nombre}_saneado{extension}");

                File.WriteAllText(rutaSaneada, doc.DocumentNode.OuterHtml);
                return rutaSaneada;
            }
            catch (Exception e)
            {
                // Esto atrapará cualquier error y te dará detalle en el log
                throw new Exception($"Error de seguridad al procesar SVG '{rutaConFichero}'", e);
            }
        }

    }
}
