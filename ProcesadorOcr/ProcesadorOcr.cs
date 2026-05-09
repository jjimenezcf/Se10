using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Data;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Tesseract;
using Utilidades;
using SkiaSharp;

namespace ProcesadorOcr
{
    public class ProcesadorOcr
    {
        private static readonly string TESSDATA_PATH = "./tessdata";
        private static readonly string LANGUAGE = "spa";

        public async Task<string> ProcesarFichero(int idArchivo, string filePath, List<string>? keyWords = null, bool returnJson = false)
        {
            string extractedText = "";
            string fileExtension = System.IO.Path.GetExtension(filePath).ToLower();

            switch (fileExtension)
            {
                case ".docx":
                    extractedText = await Task.Run(() => extDocx.ToTexto(filePath));
                    break;
                case ".rtf":
                    extractedText = await Task.Run(() => extRtf.ToTexto(filePath));
                    break;
                case ".pdf":
                    extractedText = await PdfToTexto(filePath, keyWords ?? new List<string>());
                    break;
                case ".jpg":
                case ".jpeg":
                case ".png":
                case ".bmp":
                    extractedText = await Task.Run(() => RealizarOcr(filePath));
                    break;
                default:
                    if (extTexto.EsTexto(filePath))
                        return File.ReadAllText(filePath);
                    throw new NotSupportedException("Formato de archivo no soportado.");
            }

            return returnJson ? JsonConvert.SerializeObject(new { text = extractedText }) : extractedText;
        }

        private static string RealizarOcr(string imagePath)
        {
            using (var img = new Bitmap(imagePath))
            {
                Bitmap imagenProcesada = img.PreprocesarImagen();
                using (var engine = new TesseractEngine(TESSDATA_PATH, LANGUAGE, Tesseract.EngineMode.Default))
                {
                    engine.SetVariable("tessedit_pageseg_mode", "6");
                    using (var pixImage = imagenProcesada.BitmapToPix())
                    {
                        using (var page = engine.Process(pixImage))
                        {
                            return page.GetText();
                        }
                    }
                }
            }
        }

        public static async Task<string> PdfToTexto(string rutaArchivo, List<string> palabrasClaves)
        {
            StringBuilder fullText = new StringBuilder();
            Dictionary<string, decimal?> valoresExtraidos = new Dictionary<string, decimal?>();

            using (PdfReader lector = new PdfReader(rutaArchivo))
            using (PdfDocument pdfDoc = new PdfDocument(lector))
            {
                int totalPaginas = pdfDoc.GetNumberOfPages();

                for (int pagina = 1; pagina <= totalPaginas; pagina++)
                {
                    PdfPage pdfPage = pdfDoc.GetPage(pagina);

                    var margenes = new ProcesadorOcrDeLosMargenes();
                    var pie = new ProcesadorOcrDelPie();
                    var cabecera = new ProcesadorOcrDeLaCabecera(pdfPage.GetPageSize().GetHeight());

                    PdfCanvasProcessor procesadorMargenes = new PdfCanvasProcessor(margenes);
                    procesadorMargenes.ProcessPageContent(pdfPage);
                    string textoMargen = margenes.GetResultantText();

                    PdfCanvasProcessor procesadorPie = new PdfCanvasProcessor(pie);
                    procesadorPie.ProcessPageContent(pdfPage);
                    string textoPie = pie.GetResultantText();

                    PdfCanvasProcessor procesadorCabecera = new PdfCanvasProcessor(cabecera);
                    procesadorCabecera.ProcessPageContent(pdfPage);
                    string textoCabecera = cabecera.GetResultantText();

                    // Texto normal con estrategia simple
                    var estrategiaSimple = new SimpleTextExtractionStrategy();
                    PdfCanvasProcessor procesadorSimple = new PdfCanvasProcessor(estrategiaSimple);
                    procesadorSimple.ProcessPageContent(pdfPage);
                    string pageText = estrategiaSimple.GetResultantText();

                    string imageText = ImagenesToTexto(pdfPage);

                    string combinedText = textoCabecera + "\n" + pageText + "\n" + imageText + "\n" + textoMargen + "\n" + textoPie;
                    fullText.AppendLine(combinedText);

                    foreach (var keyword in palabrasClaves)
                    {
                        valoresExtraidos[keyword] = ExtraerValores(combinedText, keyword);
                    }
                }
            }

            var palabrasClavesNoEncontradas = valoresExtraidos.Where(kv => kv.Value == null).Select(kv => kv.Key).ToList();
            if (palabrasClavesNoEncontradas.Any())
            {
                string remainingText = fullText.ToString();
                foreach (var clave in palabrasClavesNoEncontradas)
                {
                    valoresExtraidos[clave] = BuscarElValorDecimalMasProximo(remainingText, clave);
                    if (valoresExtraidos[clave].HasValue)
                        fullText.AppendLine($"{clave} = {valoresExtraidos[clave]}");
                }
            }

            string cleanedText = string.Join("\n", fullText.ToString()
                .Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Where(line => !string.IsNullOrWhiteSpace(line)));

            return cleanedText;
        }

        private static decimal? ExtraerValores(string text, string keyword)
        {
            var regex = new Regex($@"{Regex.Escape(keyword)}[:\s]*(\d+(?:[.,]\d+)?)", RegexOptions.IgnoreCase);
            var match = regex.Match(text);
            if (match.Success)
            {
                if (decimal.TryParse(match.Groups[1].Value.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal amount))
                    return amount;
            }
            return null;
        }

        private static decimal? BuscarElValorDecimalMasProximo(string text, string keyword)
        {
            var words = text.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            int keywordIndex = Array.FindIndex(words, w => w.Equals(keyword, StringComparison.OrdinalIgnoreCase));
            if (keywordIndex == -1) return null;

            for (int i = 1; i < 10; i++)
            {
                if (keywordIndex + i < words.Length)
                {
                    if (decimal.TryParse(words[keywordIndex + i].Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal amount))
                        return amount;
                }
            }
            return null;
        }

        // En iText 9 la extracción de imágenes se hace mediante un listener propio
        private static string ImagenesToTexto(PdfPage pdfPage)
        {
            StringBuilder imageText = new StringBuilder();
            var extractor = new ImageTextExtractor(imageText, "./tessdata", "spa");
            PdfCanvasProcessor processor = new PdfCanvasProcessor(extractor);
            processor.ProcessPageContent(pdfPage);
            return imageText.ToString();
        }
    }

    // Listener auxiliar que captura imágenes de la página y les aplica OCR
    public class ImageTextExtractor : IEventListener
    {
        private readonly StringBuilder _result;
        private readonly string _tessdata;
        private readonly string _language;

        public ImageTextExtractor(StringBuilder result, string tessdata, string language)
        {
            _result = result;
            _tessdata = tessdata;
            _language = language;
        }

        public void EventOccurred(IEventData data, EventType type)
        {
            if (type == EventType.RENDER_IMAGE && data is ImageRenderInfo imageRenderInfo)
            {
                try
                {
                    byte[] imageBytes = imageRenderInfo.GetImage().GetImageBytes();
                    if (imageBytes != null)
                        ExtensorDeProcesadorOcr.ProcesarMemoria(imageBytes, _tessdata, _language, _result);
                }
                catch
                {
                    // Si la imagen no puede procesarse se ignora
                }
            }
        }

        public ICollection<EventType> GetSupportedEvents() =>
            new HashSet<EventType> { EventType.RENDER_IMAGE };
    }

    public class ProcesadorOcrDeLosMargenes : ITextExtractionStrategy
    {
        private readonly StringBuilder _result = new StringBuilder();

        public void EventOccurred(IEventData data, EventType type)
        {
            if (type == EventType.RENDER_TEXT && data is TextRenderInfo renderInfo)
                RenderText(renderInfo);
            else if (type == EventType.RENDER_IMAGE && data is ImageRenderInfo imageRenderInfo)
                RenderImage(imageRenderInfo);
        }

        public ICollection<EventType> GetSupportedEvents() =>
            new HashSet<EventType> { EventType.RENDER_TEXT, EventType.RENDER_IMAGE };

        private void RenderText(TextRenderInfo renderInfo)
        {
            var baseline = renderInfo.GetBaseline();
            var startPoint = baseline.GetStartPoint();
            float x = startPoint.Get(Vector.I1);
            float y = startPoint.Get(Vector.I2);

            if (x < 50 || y < 50)
                _result.Append(renderInfo.GetText());
        }

        private void RenderImage(ImageRenderInfo renderInfo)
        {
            try
            {
                var imageBytes = renderInfo.GetImage().GetImageBytes();
                if (imageBytes != null)
                    ExtensorDeProcesadorOcr.ProcesarMemoria(imageBytes, "./tessdata", "spa", _result);
            }
            catch { }
        }

        public string GetResultantText() => _result.ToString();
    }

    public class ProcesadorOcrDelPie : ITextExtractionStrategy
    {
        private readonly StringBuilder _result = new StringBuilder();
        private readonly float _footerHeight;

        public ProcesadorOcrDelPie(float footerHeight = 50)
        {
            _footerHeight = footerHeight;
        }

        public void EventOccurred(IEventData data, EventType type)
        {
            if (type == EventType.RENDER_TEXT && data is TextRenderInfo renderInfo)
                RenderText(renderInfo);
            else if (type == EventType.RENDER_IMAGE && data is ImageRenderInfo imageRenderInfo)
                RenderImage(imageRenderInfo);
        }

        public ICollection<EventType> GetSupportedEvents() =>
            new HashSet<EventType> { EventType.RENDER_TEXT, EventType.RENDER_IMAGE };

        private void RenderText(TextRenderInfo renderInfo)
        {
            var startPoint = renderInfo.GetBaseline().GetStartPoint();
            float y = startPoint.Get(Vector.I2);
            if (y <= _footerHeight)
                _result.Append(renderInfo.GetText());
        }

        private void RenderImage(ImageRenderInfo renderInfo)
        {
            try
            {
                var ctm = renderInfo.GetImageCtm();
                float translateY = ctm.Get(Matrix.I32);
                if (translateY <= _footerHeight)
                {
                    var imageBytes = renderInfo.GetImage().GetImageBytes();
                    if (imageBytes != null)
                        ExtensorDeProcesadorOcr.ProcesarMemoria(imageBytes, "./tessdata", "spa", _result);
                }
            }
            catch { }
        }

        public string GetResultantText() => _result.ToString();
    }

    public class ProcesadorOcrDeLaCabecera : ITextExtractionStrategy
    {
        private readonly StringBuilder _result = new StringBuilder();
        private readonly float _headerHeight;
        private readonly float _pageHeight;

        public ProcesadorOcrDeLaCabecera(float pageHeight, float headerHeight = 50)
        {
            _pageHeight = pageHeight;
            _headerHeight = headerHeight;
        }

        public void EventOccurred(IEventData data, EventType type)
        {
            if (type == EventType.RENDER_TEXT && data is TextRenderInfo renderInfo)
                RenderText(renderInfo);
            else if (type == EventType.RENDER_IMAGE && data is ImageRenderInfo imageRenderInfo)
                RenderImage(imageRenderInfo);
        }

        public ICollection<EventType> GetSupportedEvents() =>
            new HashSet<EventType> { EventType.RENDER_TEXT, EventType.RENDER_IMAGE };

        private void RenderText(TextRenderInfo renderInfo)
        {
            var startPoint = renderInfo.GetBaseline().GetStartPoint();
            float y = startPoint.Get(Vector.I2);
            if (y >= (_pageHeight - _headerHeight))
                _result.Append(renderInfo.GetText());
        }

        private void RenderImage(ImageRenderInfo renderInfo)
        {
            try
            {
                var ctm = renderInfo.GetImageCtm();
                float translateY = ctm.Get(Matrix.I32);
                if (translateY >= (_pageHeight - _headerHeight))
                {
                    var imageBytes = renderInfo.GetImage().GetImageBytes();
                    if (imageBytes != null)
                        ExtensorDeProcesadorOcr.ProcesarMemoria(imageBytes, "./tessdata", "spa", _result);
                }
            }
            catch { }
        }

        public string GetResultantText() => _result.ToString();
    }

    public static class ExtensorDeProcesadorOcr
    {
        public static void ProcesarMemoria(byte[] imageBytes, string tessData, string lengua, StringBuilder result)
        {
            using (MemoryStream ms = new MemoryStream(imageBytes))
            {
                using (Bitmap imagen = new Bitmap(ms))
                {
                    Bitmap processedImage = imagen.PreprocesarImagen();
                    string extractedText = RealizarOcr(processedImage, tessData, lengua);
                    result.AppendLine(extractedText);
                }
            }
        }

        private static string RealizarOcr(Bitmap imagen, string tessData, string lengua)
        {
            using (var engine = new TesseractEngine(tessData, lengua, Tesseract.EngineMode.Default))
            {
                using (var pix = imagen.BitmapToPix())
                {
                    engine.SetVariable("tessedit_pageseg_mode", "6");
                    using (var page = engine.Process(pix))
                        return page.GetText();
                }
            }
        }


        public static Bitmap PreprocesarImagen(this Bitmap image)
        {
            // Convertir Bitmap a SKBitmap
            using var ms = new MemoryStream();
            image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            ms.Seek(0, SeekOrigin.Begin);

            using var skBitmap = SKBitmap.Decode(ms);

            // Escalar x2
            var scaledWidth = skBitmap.Width * 2;
            var scaledHeight = skBitmap.Height * 2;
            using var scaledBitmap = skBitmap.Resize(new SKImageInfo(scaledWidth, scaledHeight), SKFilterQuality.High);

            // Convertir a escala de grises
            var grayBitmap = new SKBitmap(scaledWidth, scaledHeight);
            using (var canvas = new SKCanvas(grayBitmap))
            {
                var paint = new SKPaint
                {
                    ColorFilter = SKColorFilter.CreateColorMatrix(new float[]
                    {
                0.299f, 0.587f, 0.114f, 0, 0,
                0.299f, 0.587f, 0.114f, 0, 0,
                0.299f, 0.587f, 0.114f, 0, 0,
                0,      0,      0,      1, 0
                    })
                };
                canvas.DrawBitmap(scaledBitmap, 0, 0, paint);
            }

            // Convertir SKBitmap de vuelta a Bitmap
            using var resultImage = SKImage.FromBitmap(grayBitmap);
            using var resultData = resultImage.Encode(SKEncodedImageFormat.Png, 100);
            using var resultMs = new MemoryStream(resultData.ToArray());
            return new Bitmap(resultMs);
        }

        public static Pix BitmapToPix(this Bitmap bitmap)
        {
            // Convertir Bitmap a bytes PNG via SkiaSharp
            using var ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            ms.Seek(0, SeekOrigin.Begin);

            using var skBitmap = SKBitmap.Decode(ms);
            using var skImage = SKImage.FromBitmap(skBitmap);
            using var skData = skImage.Encode(SKEncodedImageFormat.Png, 100);

            return Pix.LoadFromMemory(skData.ToArray());
        }
    }
}