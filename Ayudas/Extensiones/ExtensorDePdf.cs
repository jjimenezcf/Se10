using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using System;
using System.IO;
using System.Text;

namespace Utilidades
{
    public static class ExtensorDePdf
    {
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