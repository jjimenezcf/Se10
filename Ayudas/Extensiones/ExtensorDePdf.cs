using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Signatures;
using System;
using System.IO;
using System.Text;
using ConvertApiDotNet;
using System.Linq;

namespace Utilidades
{
    public static class ExtensorDePdf
    {

        private const int MinutosMaximosDeConversion = 2;
        private static readonly object _bloqueoLibreOffice = new object();

        // Formatos que se pueden convertir a pdf (con LibreOffice y con ConvertApi)
        // Los html se guardan con extensión txt (ver SubirArchivoInterno), por eso un txt puede ser un html: se distingue por su contenido
        public static readonly string[] ExtensionesConvertiblesAPdf = { ".docx", ".xlsx", ".txt", ".html", ".htm" };

        public static bool EsConvertibleAPdf(string rutaOnombre)
        => ExtensionesConvertiblesAPdf.Contains(Path.GetExtension(rutaOnombre).ToLower());

        // Comprueba, sin convertir nada, que hay alguno de los dos motores: LibreOffice (se encuentra soffice.exe) o ConvertApi
        // (hay una clave definida). Devuelve null si se puede convertir; si no, el motivo por el que no se puede, para mostrárselo al usuario.
        // De ConvertApi sólo se comprueba que haya clave, no que sea válida (eso sólo se sabría al llamar al servicio).
        public static string MotivoPorElQueNoSePuedeConvertir(string rutaLibreOffice, Func<string> obtenerClaveConvertApi)
        {
            if (LocalizarSoffice(rutaLibreOffice) != null)
                return null;

            var sinLibreOffice = string.IsNullOrWhiteSpace(rutaLibreOffice)
                ? "no se ha indicado dónde está instalado"
                : $"no se encuentra soffice.exe en '{rutaLibreOffice.Trim().Trim('"')}'";

            string sinConvertApi;
            try
            {
                sinConvertApi = string.IsNullOrWhiteSpace(obtenerClaveConvertApi?.Invoke())
                    ? "no hay una clave de ConvertApi definida"
                    : null;
            }
            catch (Exception e)
            {
                sinConvertApi = $"no se ha podido leer la clave de ConvertApi: {e.Message}";
            }

            if (sinConvertApi == null)
                return null;

            return $"No se puede convertir a pdf: no hay acceso a LibreOffice ({sinLibreOffice}) ni licencia de ConvertApi ({sinConvertApi}).";
        }

        // Convierte a pdf (junto al fichero y con su nombre) con LibreOffice si se encuentra soffice.exe; si no, con ConvertApi.
        // La clave de ConvertApi se pide solo si hace falta, porque puede no estar definida.
        // opcionesDePagina: tamaño y orientación de la página; sólo se aplican a una hoja de cálculo (xlsx). Sin ellas, el xlsx sale con la
        // página que traiga el libro. Un docx que no declare página (sale en Letter) se deja en A4 vertical; si la declara, se respeta.
        // El resto de formatos no tienen página que configurar: txt y html salen en A4 vertical.
        public static string ConvertirAPdf(string fichero, string rutaLibreOffice, Func<string> obtenerClaveConvertApi, OpcionesDePaginaPdf opcionesDePagina = null)
        {
            // Quien llama debería haberlo comprobado antes (para avisar al usuario sin haber hecho nada más); aquí se asegura
            var motivo = MotivoPorElQueNoSePuedeConvertir(rutaLibreOffice, obtenerClaveConvertApi);
            if (motivo != null)
                throw new InvalidOperationException(motivo);

            var extension = Path.GetExtension(fichero).ToLower();

            if (extension == $".{enumExtensiones.xlsx}" && opcionesDePagina != null)
                return ConvertirAjustandoUnaCopia(fichero, rutaLibreOffice, obtenerClaveConvertApi, copia => ExtensorDeXlsx.ConfigurarPagina(copia, opcionesDePagina));

            if (extension == $".{enumExtensiones.docx}")
                return ConvertirAjustandoUnaCopia(fichero, rutaLibreOffice, obtenerClaveConvertApi, copia => ExtensorDeDocx.PonerPaginaSiNoLaDefine(copia, new OpcionesDePaginaPdf()));

            // Un html se sanea antes de dárselo al motor y se convierte como documento, no como texto
            if (EsHtmlParaConvertir(fichero))
                return ConvertirHtmlAPdf(fichero, rutaLibreOffice, obtenerClaveConvertApi);

            return ConvertirAPdfConElMotor(fichero, rutaLibreOffice, obtenerClaveConvertApi);
        }

        private static bool EsHtmlParaConvertir(string fichero)
        {
            var extension = Path.GetExtension(fichero).ToLower();
            if (extension == ".html" || extension == ".htm")
                return true;

            return extension == ".txt" && ExtensorDeHtml.EsHtml(File.ReadAllText(fichero));
        }

        // El html se sanea en una copia (el original no se toca) y se convierte con el filtro de LibreOffice para documentos web;
        // el pdf se deja junto al original, como en el resto de formatos
        private static string ConvertirHtmlAPdf(string fichero, string rutaLibreOffice, Func<string> obtenerClaveConvertApi)
        {
            var directorioTemporal = Path.Combine(Path.GetTempPath(), "Se_Conversion", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directorioTemporal);
            try
            {
                var copia = Path.Combine(directorioTemporal, $"{Path.GetFileNameWithoutExtension(fichero)}.html");
                File.WriteAllText(copia, ExtensorDeHtml.SanitizeHtmlParaPdf(File.ReadAllText(fichero)), new System.Text.UTF8Encoding(false));

                var pdfTemporal = ConvertirAPdfConElMotor(copia, rutaLibreOffice, obtenerClaveConvertApi, filtroDeLibreOffice: "pdf:writer_web_pdf_Export");
                var pdf = Path.Combine(Path.GetDirectoryName(fichero), Path.GetFileName(pdfTemporal));
                File.Move(pdfTemporal, pdf, overwrite: true);
                return pdf;
            }
            finally
            {
                try { Directory.Delete(directorioTemporal, recursive: true); } catch { }
            }
        }

        // Se ajusta el fichero (su página) en una copia, para no modificar el original; el pdf se deja junto al original, como en el resto de formatos
        private static string ConvertirAjustandoUnaCopia(string fichero, string rutaLibreOffice, Func<string> obtenerClaveConvertApi, Action<string> ajustarLaCopia)
        {
            var directorioTemporal = Path.Combine(Path.GetTempPath(), "Se_Conversion", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directorioTemporal);
            try
            {
                var copia = Path.Combine(directorioTemporal, Path.GetFileName(fichero));
                File.Copy(fichero, copia, overwrite: true);

                try
                {
                    ajustarLaCopia(copia);
                }
                catch (Exception)
                {
                    // si no se puede ajustar la página se convierte tal cual: es mejor un pdf con la página por defecto que ninguno
                    File.Copy(fichero, copia, overwrite: true);
                }

                var pdfTemporal = ConvertirAPdfConElMotor(copia, rutaLibreOffice, obtenerClaveConvertApi);
                var pdf = Path.Combine(Path.GetDirectoryName(fichero), Path.GetFileName(pdfTemporal));
                File.Move(pdfTemporal, pdf, overwrite: true);
                return pdf;
            }
            finally
            {
                try { Directory.Delete(directorioTemporal, recursive: true); } catch { }
            }
        }

        private static string ConvertirAPdfConElMotor(string fichero, string rutaLibreOffice, Func<string> obtenerClaveConvertApi, string filtroDeLibreOffice = "pdf")
        {
            var soffice = LocalizarSoffice(rutaLibreOffice);

            if (soffice == null)
                return ConvertirAPdfConConvertApi(fichero, obtenerClaveConvertApi());

            return ConvertirAPdfConLibreOffice(fichero, soffice, filtroDeLibreOffice);
        }

        // Admite la ruta de soffice.exe o la de la carpeta donde se descomprimió LibreOffice
        private static string LocalizarSoffice(string ruta)
        {
            if (string.IsNullOrWhiteSpace(ruta)) return null;

            ruta = ruta.Trim().Trim('"');

            if (File.Exists(ruta)) return ruta;

            foreach (var candidata in new[] { Path.Combine(ruta, "program", "soffice.exe"), Path.Combine(ruta, "soffice.exe") })
                if (File.Exists(candidata)) return candidata;

            return null;
        }

        private static string ConvertirAPdfConLibreOffice(string fichero, string soffice, string filtroDeExportacion)
        {
            var directorioDeSalida = Path.GetDirectoryName(fichero);
            var ficheroPdf = Path.Combine(directorioDeSalida, $"{Path.GetFileNameWithoutExtension(fichero)}.{enumExtensiones.pdf}");

            // Una sola conversión a la vez por proceso: dos instancias con el mismo perfil se estorban.
            // El perfil es propio de cada proceso para no chocar con otros sitios ni con un LibreOffice abierto.
            lock (_bloqueoLibreOffice)
            {
                var perfil = new Uri(Path.Combine(Path.GetTempPath(), "Se_LibreOffice", $"perfil_{Environment.ProcessId}")).AbsoluteUri;

                var inicio = new System.Diagnostics.ProcessStartInfo(soffice)
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                inicio.ArgumentList.Add($"-env:UserInstallation={perfil}");
                inicio.ArgumentList.Add("--headless");
                inicio.ArgumentList.Add("--norestore");
                inicio.ArgumentList.Add("--convert-to");
                inicio.ArgumentList.Add(filtroDeExportacion);
                inicio.ArgumentList.Add("--outdir");
                inicio.ArgumentList.Add(directorioDeSalida);
                inicio.ArgumentList.Add(fichero);

                // Si hubiera un pdf anterior, que no se confunda con el resultado de esta conversión
                if (File.Exists(ficheroPdf)) File.Delete(ficheroPdf);

                using (var proceso = System.Diagnostics.Process.Start(inicio))
                {
                    var salida = proceso.StandardOutput.ReadToEndAsync();
                    var errores = proceso.StandardError.ReadToEndAsync();

                    if (!proceso.WaitForExit(MinutosMaximosDeConversion * 60 * 1000))
                    {
                        proceso.Kill(entireProcessTree: true);
                        throw new Exception($"LibreOffice no terminó de convertir '{Path.GetFileName(fichero)}' en {MinutosMaximosDeConversion} minutos.");
                    }

                    proceso.WaitForExit();

                    if (proceso.ExitCode != 0 || !File.Exists(ficheroPdf) || new FileInfo(ficheroPdf).Length == 0)
                        throw new Exception($"LibreOffice no ha podido convertir '{Path.GetFileName(fichero)}' a pdf (código {proceso.ExitCode}). {errores.Result} {salida.Result}".Trim());
                }
            }

            return ficheroPdf;
        }

        // El formato de origen es la extensión del fichero (docx, xlsx, txt)
        private static string ConvertirAPdfConConvertApi(string fichero, string claveConverApi)
        {
            Exception excepcionEnTaskRun = null;
            System.Threading.Tasks.Task.Run(async () =>
            {
                try
                {
                    var covertapi = new ConvertApi(claveConverApi);
                    var formatoOrigen = Path.GetExtension(fichero).TrimStart('.').ToLower();
                    var resutadoConversion = await covertapi.ConvertAsync(formatoOrigen, enumExtensiones.pdf.ToString(), new ConvertApiFileParam("File", fichero));
                    if (resutadoConversion.Files.Count() == 1)
                    {
                        foreach (var file in resutadoConversion.Files)
                        {
                            var directorioDeSalida = Path.GetDirectoryName(fichero);
                            await resutadoConversion.SaveFilesAsync(directorioDeSalida);
                        }
                    }
                    else
                    {
                        throw new Exception("La conversión no produjo archivos. Revisa el resultado.");
                    }
                }
                catch (Exception ex)
                {
                    excepcionEnTaskRun = ex;
                }
            }).Wait();

            if (excepcionEnTaskRun != null)
                throw excepcionEnTaskRun;

            return Path.Combine(Path.GetDirectoryName(fichero), $"{Path.GetFileNameWithoutExtension(fichero)}.{enumExtensiones.pdf}");
        }

        public static string SanitizePdf(string rutaConFichero)
        {
            string extension = Path.GetExtension(rutaConFichero).ToLower();
            if (extension != ".pdf") return rutaConFichero;

            using (PdfReader reader = new PdfReader(rutaConFichero))
            {
                // SI EL PDF YA TIENE FIRMAS, NO LO TOCAMOS O ROMPEREMOS LA INTEGRIDAD
                SignatureUtil signUtil = new SignatureUtil(new PdfDocument(reader));
                if (signUtil.GetSignatureNames().Count > 0)
                {
                    return rutaConFichero; // Retornar el original sin sanear para no romper la firma
                }
            }

            string nombreArchivo = Path.GetFileNameWithoutExtension(rutaConFichero);
            string directorio = Path.GetDirectoryName(rutaConFichero);
            string rutaArchivoSanitizado = Path.Combine(directorio, nombreArchivo + "_saneado.pdf");

            try
            {
                using (PdfReader reader = new PdfReader(rutaConFichero))
                using (PdfWriter writer = new PdfWriter(rutaArchivoSanitizado))
                using (PdfDocument pdfDoc = new PdfDocument(reader, writer))
                {
                    PdfDictionary catalog = pdfDoc.GetCatalog().GetPdfObject();

                    // Eliminar acciones automáticas de apertura
                    catalog.Remove(PdfName.OpenAction);
                    catalog.Remove(PdfName.AA); // Additional Actions

                    // Limpiar el diccionario de nombres (donde suele vivir el JS de documento)
                    PdfDictionary names = catalog.GetAsDictionary(PdfName.Names);
                    if (names != null)
                    {
                        names.Remove(PdfName.JavaScript);
                    }

                    // Limpiar JavaScript en cada página (opcional pero seguro)
                    for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
                    {
                        PdfPage page = pdfDoc.GetPage(i);
                        page.GetPdfObject().Remove(PdfName.AA); // Acciones de la página
                        page.GetPdfObject().Remove(PdfName.JS);
                    }
                }
                return rutaArchivoSanitizado;
            }
            catch (Exception)
            {
                return rutaConFichero;
            }
        }

        public static bool EsPdf(string rutaConFichero)
        {
            if (!File.Exists(rutaConFichero))
                throw new Exception($"El fichero '{rutaConFichero}'no existe");

            byte[] buffer = new byte[5];
            using (FileStream fs = new FileStream(rutaConFichero, FileMode.Open, FileAccess.Read))
            {
                fs.ReadExactly(buffer, 0, 5);
            }

            // Convertimos los bytes a string para comparar la firma
            string firma = System.Text.Encoding.ASCII.GetString(buffer);
            return firma == "%PDF-";
        }


        public static string ToTexto(string filePath)
        {
            using (PdfReader reader = new PdfReader(filePath))
            using (PdfDocument pdfDoc = new PdfDocument(reader))
            {
                StringWriter output = new StringWriter();
                for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
                {
                    output.WriteLine(PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(i)));
                }
                return output.ToString();
            }
        }

        public static bool IsPdfA(string filePath)
        {
            try
            {
                using (PdfReader reader = new PdfReader(filePath))
                using (PdfDocument pdfDoc = new PdfDocument(reader))
                {
                    byte[] metadata = pdfDoc.GetXmpMetadataBytes();
                    if (metadata == null || metadata.Length == 0) return false;

                    string xmlMetadata = Encoding.UTF8.GetString(metadata);
                    return xmlMetadata.Contains("pdfaid:part");
                }
            }
            catch
            {
                return false;
            }
        }

        public static bool HasOCR(string filePath)
        {
            try
            {
                using (PdfReader reader = new PdfReader(filePath))
                using (PdfDocument pdfDoc = new PdfDocument(reader))
                {
                    StringBuilder text = new StringBuilder();

                    int totalPages = pdfDoc.GetNumberOfPages();
                    int pagesToScan = Math.Min(totalPages, 3);

                    for (int i = 1; i <= pagesToScan; i++)
                    {
                        ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                        string pageText = PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(i), strategy);
                        text.Append(pageText);
                    }

                    return text.ToString().Trim().Length > 10;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}