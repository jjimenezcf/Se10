using DocumentFormat.OpenXml.Packaging;
using iText.Signatures;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;


namespace Utilidades
{
    internal class ColumnaFecha
    {
        internal DocumentFormat.OpenXml.Spreadsheet.Worksheet Hoja { get; set; }
        internal int Indice { get; set; }
        internal bool EsFecha { get; set; }
    }

    public static class ExtensorDeXlsx
    {

        private const uint PapelA3 = 8; // valor de paperSize de Excel para A3 (297 x 420 mm)

        // Deja todas las hojas del libro para imprimirse en A3 y apaisado; modifica el fichero indicado.
        // Sólo toca la configuración de impresión de cada hoja, no sus datos.
        public static void ConfigurarPaginaApaisadaA3(string rutaXlsx)
        {
            // Orden de los hijos de una hoja según el esquema: lo que va después de pageSetup
            var posteriores = new[] { "headerFooter", "rowBreaks", "colBreaks", "customProperties", "cellWatches", "ignoredErrors", "smartTags", "drawing", "legacyDrawing", "legacyDrawingHF", "drawingHF", "picture", "oleObjects", "controls", "webPublishItems", "tableParts", "extLst" };

            using (SpreadsheetDocument documento = SpreadsheetDocument.Open(rutaXlsx, true))
            {
                foreach (var parteDeHoja in documento.WorkbookPart.WorksheetParts)
                {
                    var hoja = parteDeHoja.Worksheet;

                    var margenes = hoja.GetFirstChild<DocumentFormat.OpenXml.Spreadsheet.PageMargins>();
                    if (margenes == null)
                    {
                        margenes = new DocumentFormat.OpenXml.Spreadsheet.PageMargins { Left = 0.7, Right = 0.7, Top = 0.75, Bottom = 0.75, Header = 0.3, Footer = 0.3 };
                        var siguiente = hoja.ChildElements.FirstOrDefault(e => posteriores.Contains(e.LocalName));
                        if (siguiente != null) hoja.InsertBefore(margenes, siguiente);
                        else hoja.AppendChild(margenes);
                    }

                    var configuracion = hoja.GetFirstChild<DocumentFormat.OpenXml.Spreadsheet.PageSetup>();
                    if (configuracion == null)
                    {
                        configuracion = new DocumentFormat.OpenXml.Spreadsheet.PageSetup();
                        hoja.InsertAfter(configuracion, margenes);
                    }

                    configuracion.PaperSize = PapelA3;
                    configuracion.Orientation = DocumentFormat.OpenXml.Spreadsheet.OrientationValues.Landscape;

                    hoja.Save();
                }
            }
        }

        private static List<ColumnaFecha> _ColumnasFechas = null;
        public static string ToHtml(string filePath)
        {
            StringBuilder html = new StringBuilder();
            _ColumnasFechas = new List<ColumnaFecha>();

            using (SpreadsheetDocument document = SpreadsheetDocument.Open(filePath, false))
            {
                WorkbookPart workbookPart = document.WorkbookPart;
                IEnumerable<DocumentFormat.OpenXml.Spreadsheet.Sheet> sheets = workbookPart.Workbook.Descendants<DocumentFormat.OpenXml.Spreadsheet.Sheet>();

                foreach (DocumentFormat.OpenXml.Spreadsheet.Sheet sheet in sheets)
                {
                    WorksheetPart worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id);
                    DocumentFormat.OpenXml.Spreadsheet.SheetData sheetData = worksheetPart.Worksheet.Elements<DocumentFormat.OpenXml.Spreadsheet.SheetData>().First();

                    // Añadir el título de la hoja en negrita
                    html.Append($"<h2 style='font-weight: bold;'>{sheet.Name}</h2>");

                    html.Append("<table style='border-collapse: collapse; width: 100%; margin-bottom: 20px;'>");
                    foreach (DocumentFormat.OpenXml.Spreadsheet.Row row in sheetData.Elements<DocumentFormat.OpenXml.Spreadsheet.Row>())
                    {
                        html.Append("<tr style='white-space: nowrap;'>");
                        int currentColumn = 0;
                        foreach (DocumentFormat.OpenXml.Spreadsheet.Cell cell in row.Elements<DocumentFormat.OpenXml.Spreadsheet.Cell>())
                        {
                            string cellReference = cell.CellReference.Value;
                            int columnIndex = GetColumnIndexFromReference(cellReference);

                            // Añadir celdas vacías si es necesario
                            while (currentColumn < columnIndex)
                            {
                                html.Append("<td style='border: 1px solid #ddd; padding: 8px; overflow: hidden; text-overflow: ellipsis;'></td>");
                                currentColumn++;
                            }

                            string value = GetCellValue(cell, workbookPart);
                            html.Append($"<td style='border: 1px solid #ddd; padding: 8px; overflow: hidden; text-overflow: ellipsis;'>{value}</td>");
                            currentColumn++;
                        }
                        html.Append("</tr>");
                    }


                    html.Append("</table>");
                }
            }

            return html.ToString();
        }
        private static int GetColumnIndexFromReference(string cellReference)
        {
            string columnReference = new string(cellReference.TakeWhile(c => !char.IsDigit(c)).ToArray());
            int columnIndex = 0;
            for (int i = 0; i < columnReference.Length; i++)
            {
                columnIndex *= 26;
                columnIndex += (columnReference[i] - 'A' + 1);
            }
            return columnIndex;
        }

        private static string GetCellValueOld(DocumentFormat.OpenXml.Spreadsheet.Cell cell, WorkbookPart workbookPart)
        {
            if (cell == null) return string.Empty;

            string value = cell.InnerText;

            if (cell.DataType?.Value == DocumentFormat.OpenXml.Spreadsheet.CellValues.SharedString)
            {
                int id = int.Parse(value);
                value = workbookPart.SharedStringTablePart.SharedStringTable
                    .Elements<DocumentFormat.OpenXml.Spreadsheet.SharedStringItem>().ElementAt(id).InnerText;
            }
            else if (cell.StyleIndex != null)
            {
                var stylesheet = workbookPart.WorkbookStylesPart.Stylesheet;
                var cellFormat = stylesheet.CellFormats.Elements<DocumentFormat.OpenXml.Spreadsheet.CellFormat>()
                    .ElementAt((int)cell.StyleIndex.Value);

                if (cellFormat.NumberFormatId != null)
                {
                    int numberFormatId = (int)cellFormat.NumberFormatId.Value;

                    // Caso especial para formato 164
                    if (numberFormatId == 164)
                    {
                        // Intentar parsear con formatos específicos de fecha
                        if (IsDateTimeFormat(numberFormatId, cell, workbookPart))
                        {
                            if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out double excelDate))
                            {
                                DateTime dateValue = DateTime.FromOADate(excelDate);
                                return dateValue.ToString("yyyy-MM-dd HH:mm");
                            }
                            else
                            {
                                int columnIndex = GetColumnIndexFromReference(cell.CellReference);
                                DocumentFormat.OpenXml.Spreadsheet.Worksheet worksheet = cell.Ancestors<DocumentFormat.OpenXml.Spreadsheet.Worksheet>().FirstOrDefault();
                                var columna = _ColumnasFechas.FirstOrDefault(c => c.Hoja == worksheet && c.Indice == columnIndex);
                                if (columna is not null) columna.EsFecha = false;
                                return value; // Si falla la conversión a fecha, devolver el valor original
                            }
                        }
                        else
                        {
                            // Si no es una fecha, intentar como número
                            if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out double numericValue))
                            {
                                return FormatNumeric(numericValue);
                            }
                            else
                            {
                                // Si no es ni fecha ni número, devolver el valor original
                                return value;
                            }
                        }
                    }
                    else
                    {
                        // Lógica para otros formatos
                        if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out double numericValue) && !IsDateTimeFormat(numberFormatId, cell, workbookPart))
                        {
                            return FormatNumeric(numericValue);
                        }

                        if (IsDateTimeFormat(numberFormatId, cell, workbookPart) && double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out double excelDate))
                        {
                            try
                            {
                                DateTime dateValue = DateTime.FromOADate(excelDate);
                                return dateValue.ToString("yyyy-MM-dd HH:mm");
                            }
                            catch { } // Ignorar errores
                        }
                    }
                }
            }

            return value;
        }

        private static string GetCellValue(DocumentFormat.OpenXml.Spreadsheet.Cell cell, WorkbookPart workbookPart)
        {
            if (cell == null) return string.Empty;

            string value = cell.InnerText;

            if (cell.DataType != null && cell.DataType.Value == DocumentFormat.OpenXml.Spreadsheet.CellValues.SharedString)
            {
                int id = int.Parse(value);
                value = workbookPart.SharedStringTablePart.SharedStringTable.Elements<DocumentFormat.OpenXml.Spreadsheet.SharedStringItem>().ElementAt(id).InnerText;
            }
            else if (cell.StyleIndex != null)
            {
                var cellFormat = workbookPart.WorkbookStylesPart.Stylesheet.CellFormats.Elements<DocumentFormat.OpenXml.Spreadsheet.CellFormat>().ElementAt((int)cell.StyleIndex.Value);
                if (cellFormat.NumberFormatId != null)
                {
                    var numberFormatId = cellFormat.NumberFormatId.Value;

                    var estilos = workbookPart.WorkbookStylesPart.Stylesheet;

                    bool esFormatoFecha =
                        (numberFormatId >= 14 && numberFormatId <= 22) ||
                        (numberFormatId >= 45 && numberFormatId <= 47);

                    // Verificar si hay un formato personalizado que parezca fecha
                    if (!esFormatoFecha && estilos.NumberingFormats != null)
                    {
                        foreach (var nf in estilos.NumberingFormats.Elements<DocumentFormat.OpenXml.Spreadsheet.NumberingFormat>())
                        {
                            if (nf.NumberFormatId != null && nf.FormatCode != null)
                            {
                                if (nf.NumberFormatId.Value == numberFormatId &&
                                    nf.FormatCode.Value.ToLower().Contains("yy"))
                                {
                                    esFormatoFecha = true;
                                    break;
                                }
                            }
                        }
                    }

                    if (esFormatoFecha)
                    {
                        if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out double excelDate))
                        {
                            DateTime dateValue = DateTime.FromOADate(excelDate);
                            return dateValue.TimeOfDay == TimeSpan.Zero
                                ? dateValue.ToString("yyyy-MM-dd")
                                : dateValue.ToString("yyyy-MM-dd HH:mm");
                        }
                    }

                    // Manejo de números
                    if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out double numericValue))
                    {
                        if (Math.Abs(numericValue - Math.Round(numericValue)) < double.Epsilon)
                        {
                            // Es un número entero
                            return ((long)numericValue).ToString();
                        }
                        else
                        {
                            // Es un número decimal
                            return numericValue.ToString("F2", CultureInfo.InvariantCulture);
                        }
                    }
                }
            }

            return value;
        }


        private static bool IsDateTimeFormat(int numberFormatId, DocumentFormat.OpenXml.Spreadsheet.Cell cell, WorkbookPart workbookPart)
        {
            if (numberFormatId == 164)
            {
                // Obtener el índice de la columna
                int columnIndex = GetColumnIndexFromReference(cell.CellReference);
                // Obtener el número de fila de la celda actual
                int rowIndex = GetRowIndexFromReference(cell.CellReference);

                // Obtener el Worksheet
                DocumentFormat.OpenXml.Spreadsheet.Worksheet worksheet = cell.Ancestors<DocumentFormat.OpenXml.Spreadsheet.Worksheet>().FirstOrDefault();

                if (worksheet == null)
                {
                    return false; // No se pudo obtener el Worksheet, se asume que no es una fecha
                }
                var columna = _ColumnasFechas.FirstOrDefault(c => c.Hoja == worksheet && c.Indice == columnIndex);
                if (columna is not null)
                    return columna.EsFecha;

                // Obtener el SheetData
                DocumentFormat.OpenXml.Spreadsheet.SheetData sheetData = worksheet.Elements<DocumentFormat.OpenXml.Spreadsheet.SheetData>().First();

                // Buscar en las celdas anteriores de la misma columna si hay alguna que contenga la palabra "fecha"
                //for (int i = rowIndex - 1; i >= 1; i--)
                //{
                int i = rowIndex - 1;
                if (i >= 1)
                {
                    // Construir la referencia de la celda anterior
                    string cellReference = GetColumnLetter(columnIndex) + i.ToString();
                    // Obtener la celda anterior
                    var previousCell = sheetData.Descendants<DocumentFormat.OpenXml.Spreadsheet.Cell>().FirstOrDefault(c => c.CellReference == cellReference);

                    if (previousCell != null)
                    {
                        string previousCellValue = GetCellValue(previousCell, workbookPart).ToLower();
                        if (TieneTituloDeImporte(previousCellValue))
                        {
                            ColumnaFecha nueva = new ColumnaFecha();
                            nueva.Hoja = worksheet;
                            nueva.Indice = columnIndex;
                            nueva.EsFecha = false;
                            _ColumnasFechas.Add(nueva);
                            return false; // Si se encuentra "fecha" en una celda anterior, se asume que es una columna de fecha
                        }
                        if (previousCellValue.Contains("fecha") || previousCellValue.ToLower().EndsWith(" el"))
                        {
                            ColumnaFecha nueva = new ColumnaFecha();
                            nueva.Hoja = worksheet;
                            nueva.Indice = columnIndex;
                            nueva.EsFecha = true;
                            _ColumnasFechas.Add(nueva);
                            return true; // Si se encuentra "fecha" en una celda anterior, se asume que es una columna de fecha
                        }
                        //}
                    }
                }
                return false; // Si no se encuentra "fecha" en celdas anteriores, se asume que no es una columna de fecha
            }
            return (numberFormatId >= 14 && numberFormatId <= 22) ||
                   (numberFormatId >= 45 && numberFormatId <= 47) ||
                   (numberFormatId >= 165 && numberFormatId <= 180);
        }

        private static bool TieneTituloDeImporte(string previousCellValue)
        {
            return previousCellValue.Contains("total") || previousCellValue.Contains("valor") || previousCellValue.Contains("importe") || previousCellValue.Contains("suma") || previousCellValue.Contains("iva")
                || previousCellValue.Contains("numero") || previousCellValue.Contains("cantidad") || previousCellValue.Contains("irpf")
                || previousCellValue.Contains("impuesto") || previousCellValue.Contains("coste");
        }

        private static string FormatNumeric(double value)
        {
            return value % 1 == 0 ?
                ((long)value).ToString() :
                value.ToString("0.########", CultureInfo.InvariantCulture);
        }
        private static int GetRowIndexFromReference(string cellReference)
        {
            // Encuentra la posición del primer dígito en la referencia de la celda
            int digitIndex = cellReference.IndexOfAny("0123456789".ToCharArray());
            // Extrae la parte numérica de la referencia de la celda y conviértela en un entero
            if (digitIndex >= 0 && int.TryParse(cellReference.Substring(digitIndex), out int rowIndex))
            {
                return rowIndex;
            }
            return -1; // En caso de error, devuelve -1
        }
        private static string GetColumnLetter(int columnIndex)
        {
            string columnLetter = string.Empty;
            while (columnIndex > 0)
            {
                int modulo = (columnIndex - 1) % 26;
                columnLetter = Convert.ToChar('A' + modulo).ToString() + columnLetter;
                columnIndex = (columnIndex - modulo) / 26;
            }
            return columnLetter;
        }
    }
}
