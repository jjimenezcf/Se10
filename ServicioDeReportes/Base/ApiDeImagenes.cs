using SkiaSharp;

namespace ServicioDeReportes.Base
{
    public static class ApiDeImagenes
    {
        // Quita los márgenes transparentes o blancos de una imagen para que, al encajarla en el reporte, ocupe todo el hueco reservado.
        // Devuelve la imagen recortada en png y su proporción ancho/alto, o null si no se puede procesar
        public static (byte[] Imagen, float Proporcion)? RecortarMargenes(string rutaDeLaImagen)
        {
            try
            {
                using var codec = SKCodec.Create(rutaDeLaImagen);
                if (codec == null) return null;

                // Rgba sin premultiplicar para comparar los colores tal cual, con independencia de la transparencia
                var formato = new SKImageInfo(codec.Info.Width, codec.Info.Height, SKColorType.Rgba8888, SKAlphaType.Unpremul);
                using var imagen = SKBitmap.Decode(codec, formato);
                if (imagen == null) return null;

                int izquierda = imagen.Width, derecha = -1, arriba = imagen.Height, abajo = -1;

                var pixeles = imagen.GetPixelSpan();
                for (var y = 0; y < imagen.Height; y++)
                {
                    var fila = pixeles.Slice(y * imagen.RowBytes, imagen.Width * imagen.BytesPerPixel);
                    for (var x = 0; x < imagen.Width; x++)
                    {
                        if (EsFondo(fila.Slice(x * imagen.BytesPerPixel, imagen.BytesPerPixel))) continue;
                        if (x < izquierda) izquierda = x;
                        if (x > derecha) derecha = x;
                        if (y < arriba) arriba = y;
                        if (y > abajo) abajo = y;
                    }
                }

                var recorte = derecha >= izquierda && abajo >= arriba
                    ? new SKRectI(izquierda, arriba, derecha + 1, abajo + 1)
                    : new SKRectI(0, 0, imagen.Width, imagen.Height);

                using var completa = SKImage.FromBitmap(imagen);
                using var recortada = completa.Subset(recorte);
                using var salida = recortada.Encode(SKEncodedImageFormat.Png, 100);
                return (salida.ToArray(), (float)recortada.Width / recortada.Height);
            }
            catch
            {
                return null;
            }
        }

        // pixel en Rgba8888: [R, G, B, A]
        private static bool EsFondo(ReadOnlySpan<byte> pixel) => pixel[3] < 16 || (pixel[0] > 240 && pixel[1] > 240 && pixel[2] > 240);
    }
}
