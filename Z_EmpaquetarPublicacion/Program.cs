using System.IO.Compression;

class Program
{
    static void Main()
    {
        string directorioOrigen = @"C:\prepublicacion";
        string directorioDestino = $@"{directorioOrigen}\prepublicando";
        string archivoZip = $@"{directorioOrigen}\publicacion.zip";

        try
        {
            // 1. Limpiar directorios de datos que no deben ir a producción
            LimpiarDirectorio(directorioOrigen, "Agendas");
            LimpiarDirectorio(directorioOrigen, "Archivos");
            LimpiarDirectorio(directorioOrigen, "Plantillas");

            // 2. Eliminar fichero token.json si existe
            EliminarFichero($@"{directorioOrigen}\wwwroot\token.json");

            // 3. Limpiar directorio de publicación
            LimpiarDirectorio(directorioOrigen, "Publicacion", esSubdirectorioDeWwwroot: false);

            // 4. Eliminar ficheros específicos que no deben ir a producción
            Console.WriteLine($"Eliminando ficheros específicos de {directorioOrigen}...");
            string[] patronesAEliminar = { "*.pdb", "appsettings.Development.json", "appsettings.json", "createdump" };
            foreach (string patron in patronesAEliminar)
            {
                foreach (string archivo in Directory.GetFiles(directorioOrigen, patron))
                    EliminarFichero(archivo);
            }

            // 5. Mover todo el contenido al directorio temporal prepublicando
            Console.WriteLine($"Moviendo contenido a {directorioDestino}...");
            if (Directory.Exists(directorioDestino))
                Directory.Delete(directorioDestino, true);
            Directory.CreateDirectory(directorioDestino);

            foreach (string origenPath in Directory.GetFileSystemEntries(directorioOrigen, "*", SearchOption.AllDirectories))
            {
                if (origenPath.StartsWith(directorioDestino))
                    continue;

                string destinoPath = origenPath.Replace(directorioOrigen, directorioDestino);
                if (Directory.Exists(origenPath))
                    Directory.CreateDirectory(destinoPath);
                else
                    File.Move(origenPath, destinoPath);
            }

            // 6. Eliminar todos los directorios del origen excepto prepublicando
            foreach (string dir in Directory.GetDirectories(directorioOrigen))
            {
                if (dir != directorioDestino)
                {
                    Console.WriteLine($"Eliminando directorio: {dir}");
                    Directory.Delete(dir, true);
                }
            }

            // 7. Copiar solo las DLLs propias y ficheros que cambian frecuentemente
            // Las DLLs de terceros (NuGet) no se incluyen porque no cambian entre publicaciones
            Console.WriteLine("Copiando ficheros propios al directorio raíz...");
            string[] archivosACopiar =
               {
                   // Ficheros de configuración del runtime
                   "SistemaDeElementos.deps.json",
                   "SistemaDeElementos.runtimeconfig.json",
                   "SistemaDeElementos.exe",
                   "web.config",
               
                   // DLLs propias
                   "SistemaDeElementos.dll",
                   "GestoresDeNegocio.dll",
                   "GestorDeElementos.dll",
                   "Utilidades.dll",
                   "Inicializador.dll",
                   "ModeloXml.dll",
                   "ModeloDeDto.dll",
                   "ServicioXml.dll",
                   "ServiciosLexnet.dll",
                   "ServicioDeDatos.dll",
                   "ServicioDeReportes.dll",
                   "ServicioDeCorreos.dll",
                   "GestorDeErrores.dll",
                   "Migraciones.dll",
                   "ProcesadorOcr.dll",
                   "VeriFactu.dll",
               
                   // iText 9 — sustituye a itextsharp.dll
                   "itext.barcodes.dll",
                   "itext.bouncy-castle-adapter.dll",
                   "itext.bouncy-castle-connector.dll",
                   "itext.commons.dll",
                   "itext.forms.dll",
                   "itext.io.dll",
                   "itext.kernel.dll",
                   "itext.layout.dll",
                   "itext.pdfa.dll",
                   "itext.pdfua.dll",
                   "itext.sign.dll",
                   "itext.styledxmlparser.dll",
                   "itext.svg.dll",
               
                   // BouncyCastle
                   "BouncyCastle.Cryptography.dll",
               
                   // ORM y datos
                   "Dapper.dll",
                   "Z.EntityFramework.Extensions.EFCore.dll",
                   "System.Linq.Dynamic.Core.dll",
                   "DocumentFormat.OpenXml.dll",
                   "DocumentFormat.OpenXml.Framework.dll",
               
                   // Validación y utilidades
                   "IbanNet.dll",
               
                   // Imágenes
                   "Magick.NET-Q16-AnyCPU.dll",
                   "Magick.NET.Core.dll",
                   "SkiaSharp.dll",
                   "libSkiaSharp.dll",
                   "libHarfBuzzSharp.dll",
                   "SixLabors.ImageSharp.dll",
               
                   // Reporting
                   "QuestPDF.dll",
                   "EPPlus.dll",
                   "EPPlus.Interfaces.dll",
                   "EPPlus.System.Drawing.dll",
                   "QRCoder.dll",
               
                   // Correo
                   "MailKit.dll",
                   "MimeKit.dll",
                   "Mailjet.Client.dll",
                   "Google.Apis.Gmail.v1.dll",
                   "Google.Apis.Auth.dll",
                   "Google.Apis.dll",
                   "Google.Apis.Core.dll",
               
                   // Calendario
                   "Ical.Net.dll",
                   "NodaTime.dll",
               
                   // Web y utilidades
                   "HtmlAgilityPack.dll",
                   "WebOptimizer.Core.dll",
                   "NUglify.dll",
                   "Microsoft.Bcl.Cryptography.dll",
                   "RtfPipe.dll",
                   "Microsoft.IO.RecyclableMemoryStream.dll",
               
                   // OCR
                   "Tesseract.dll",
                   "TesseractOCR.dll",
               
                   // Compresión
                   "SevenZipExtractor.dll",
               };

            foreach (string archivo in archivosACopiar)
            {
                string origenArchivo = Path.Combine(directorioDestino, archivo);
                string destinoArchivo = Path.Combine(directorioOrigen, archivo);
                if (File.Exists(origenArchivo))
                {
                    try
                    {
                        File.Copy(origenArchivo, destinoArchivo, overwrite: true);
                        Console.WriteLine($"Copiado: {archivo}");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"ERROR copiando '{archivo}': {e.Message}");
                    }
                }
                else
                {
                    Console.WriteLine($"AVISO: '{archivo}' no existe en prepublicando, se omite.");
                }
            }

            // 8. Copiar directorios necesarios al origen
            Console.WriteLine("Copiando directorios al origen...");
            string[] directoriosAMover = { "movil", "wwwroot", "es" };
            foreach (string directorio in directoriosAMover)
            {
                string origen = Path.Combine(directorioDestino, directorio);
                string destino = Path.Combine(directorioOrigen, directorio);
                if (Directory.Exists(origen))
                    CopiarDirectorioContenido(origen, destino);
                else
                    Console.WriteLine($"AVISO: El directorio '{directorio}' no existe en prepublicando, se omite.");
            }

            // 9. Crear el zip con todo el contenido del origen excepto prepublicando y el propio zip
            Console.WriteLine($"Creando zip en {archivoZip}...");
            if (File.Exists(archivoZip))
                File.Delete(archivoZip);

            using (FileStream zipToOpen = new FileStream(archivoZip, FileMode.Create))
            using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Create))
            {
                foreach (string archivo in Directory.GetFiles(directorioOrigen, "*", SearchOption.AllDirectories))
                {
                    if (!archivo.StartsWith(directorioDestino) && archivo != archivoZip)
                    {
                        string entryName = archivo.Replace(directorioOrigen + "\\", "");
                        try
                        {
                            archive.CreateEntryFromFile(archivo, entryName);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"ERROR añadiendo al zip '{entryName}': {e.Message}");
                        }
                    }
                }
            }

            Console.WriteLine("Proceso completado correctamente.");
            Console.WriteLine($"Zip generado en: {archivoZip}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"ERROR GENERAL: {e.Message}");
            Console.WriteLine(e.StackTrace);
        }
        finally
        {
            Console.WriteLine("Presiona una tecla para salir...");
            Console.ReadKey();
        }
    }

    static void LimpiarDirectorio(string directorioOrigen, string nombre, bool esSubdirectorioDeWwwroot = true)
    {
        string ruta = esSubdirectorioDeWwwroot
            ? $@"{directorioOrigen}\wwwroot\{nombre}"
            : $@"{directorioOrigen}\{nombre}";

        if (!Directory.Exists(ruta))
        {
            Console.WriteLine($"AVISO: El directorio '{nombre}' no existe, se omite.");
            return;
        }

        Console.WriteLine($"Limpiando directorio: {ruta}");
        try
        {
            Array.ForEach(Directory.GetFiles(ruta), File.Delete);
        }
        catch (Exception e)
        {
            Console.WriteLine($"ERROR limpiando '{nombre}': {e.Message}");
        }
    }

    static void EliminarFichero(string ruta)
    {
        if (!File.Exists(ruta))
            return;

        Console.WriteLine($"Eliminando fichero: {ruta}");
        try
        {
            File.Delete(ruta);
        }
        catch (Exception e)
        {
            Console.WriteLine($"ERROR eliminando '{ruta}': {e.Message}");
        }
    }

    static void CopiarDirectorioContenido(string origen, string destino)
    {
        if (!Directory.Exists(destino))
            Directory.CreateDirectory(destino);

        Console.WriteLine($"Copiando {origen} → {destino}");

        foreach (string archivo in Directory.GetFiles(origen))
        {
            try
            {
                File.Copy(archivo, Path.Combine(destino, Path.GetFileName(archivo)), overwrite: true);
            }
            catch (Exception e)
            {
                Console.WriteLine($"ERROR copiando '{Path.GetFileName(archivo)}': {e.Message}");
            }
        }

        foreach (string subdirectorio in Directory.GetDirectories(origen))
        {
            CopiarDirectorioContenido(
                subdirectorio,
                Path.Combine(destino, Path.GetFileName(subdirectorio))
            );
        }
    }
}