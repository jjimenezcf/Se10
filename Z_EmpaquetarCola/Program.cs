using System;
using System.IO;
using System.Linq;

class Program
{
    static void Main()
    {
        try
        {
            string directorioCola = @"c:\Prepublicacion_SinTrim";
            string directorioSitioWeb = @"C:\Prepublicacion\prepublicando";

            Console.WriteLine("Eliminando ficheros que existen en el sitio web");
            foreach (string ruta in Directory.EnumerateFiles(directorioCola, "*", SearchOption.AllDirectories))
            {
                // Verificar si la ruta está presente en directorioSitioWeb
                string rutaRelativa = ruta.Replace($@"{directorioCola}\", "");
                string rutaEnSitioWeb = Path.Combine(directorioSitioWeb, rutaRelativa);
                if (File.Exists(rutaEnSitioWeb))
                {
                    // Si el archivo está en directorioSitioWeb, borrarlo de directorioCola
                    File.Delete(ruta);
                }
            }

            Console.WriteLine("Eliminando directorios que existen en el sitio web");
            foreach (string subdirectorio in Directory.EnumerateDirectories(directorioCola, "*", SearchOption.AllDirectories))
            {
                // Verificar si el subdirectorio está presente en directorioSitioWeb
                string subdirectorioRelativo = subdirectorio.Replace($@"{directorioCola}\", "");
                string subdirectorioEnSitioWeb = Path.Combine(directorioSitioWeb, subdirectorioRelativo);
                if (Directory.Exists(subdirectorioEnSitioWeb))
                {
                    // Si el subdirectorio está en directorioSitioWeb, borrarlo de directorioCola
                    Directory.Delete(subdirectorio, true);
                }
            }

            Console.WriteLine($"Eliminar archivos específicos del directorio {directorioCola}");
            string[] archivosAEliminar = { "*.pdb", "appsettings.Development.json", "appsettings.json", "createdump" };
            foreach (string patron in archivosAEliminar)
            {
                string[] archivosEliminar = Directory.GetFiles(directorioCola, patron);
                foreach (string archivo in archivosEliminar)
                {
                    Console.WriteLine($"eliminando: {archivo}");
                    File.Delete(archivo);
                }
            }
        }
        finally
        {
            Console.WriteLine($"Presiona una tecla para salir...");
            Console.ReadKey();
        }
    }
}