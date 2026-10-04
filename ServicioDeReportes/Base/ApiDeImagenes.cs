using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

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
                using var imagen = Image.Load<Rgba32>(rutaDeLaImagen);
                int izquierda = imagen.Width, derecha = -1, arriba = imagen.Height, abajo = -1;

                imagen.ProcessPixelRows(filas =>
                {
                    for (var y = 0; y < filas.Height; y++)
                    {
                        var fila = filas.GetRowSpan(y);
                        for (var x = 0; x < fila.Length; x++)
                        {
                            if (EsFondo(fila[x])) continue;
                            if (x < izquierda) izquierda = x;
                            if (x > derecha) derecha = x;
                            if (y < arriba) arriba = y;
                            if (y > abajo) abajo = y;
                        }
                    }
                });

                if (derecha >= izquierda && abajo >= arriba)
                    imagen.Mutate(x => x.Crop(new Rectangle(izquierda, arriba, derecha - izquierda + 1, abajo - arriba + 1)));

                using var salida = new MemoryStream();
                imagen.SaveAsPng(salida);
                return (salida.ToArray(), (float)imagen.Width / imagen.Height);
            }
            catch
            {
                return null;
            }
        }

        private static bool EsFondo(Rgba32 pixel) => pixel.A < 16 || (pixel.R > 240 && pixel.G > 240 && pixel.B > 240);
    }
}
