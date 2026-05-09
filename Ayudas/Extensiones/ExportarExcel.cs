using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using OfficeOpenXml.Table;

namespace Utilidades
{
    public static class ltrExcelExportador
    {
        public static readonly string Estandar = nameof(Estandar);
        public static readonly string Registros = nameof(Registros);
    }

    public static class ltrExcelEstilos
    {
        public const string Titulos = nameof(Titulos);
        public const string Informe = nameof(Informe);
        public const string Encolumnado = nameof(Encolumnado);
        public const string Totales = nameof(Totales);
        public const string ValoresDeTitulos = nameof(ValoresDeTitulos);
    }

    public class HojaExcel<T>
    {
        public string Nombre { get; }
        public List<T> Registros { get; }

        public int Columnas => typeof(T).GetProperties().Count();
        public List<string> Cabecera { get; } = new List<string>();

        public bool HayCabecera => Cabecera.Count > 0;

        public HojaExcel(string nombre)
        {
            Nombre = nombre;
        }

        public HojaExcel(List<string> cabecera, List<T> registros)
        : this(registros)
        {
            Cabecera = cabecera;
        }

        public HojaExcel(List<T> registros)
        : this(typeof(T).Name)
        {
            Registros = registros;
        }
    }

    public class LibroExcel<T>
    {
        public string Nombre { get; }

        public List<HojaExcel<T>> hojas = new List<HojaExcel<T>>();
        public LibroExcel(string nombre)
        {
            Nombre = nombre;
        }
    }

    public class ObjetoParaExportar
    {
        public string FicheroConRuta { get; }
        public string Plantilla { get; set; } = ltrExcelExportador.Estandar;
        public Dictionary<string, object> Datos { get; } = new Dictionary<string, object>();

        public ObjetoParaExportar(string ruta, string fichero, Dictionary<string, object> datos)
        : this(Path.Combine(ruta, fichero), datos)
        {
        }

        public ObjetoParaExportar(string rutaConFichero, Dictionary<string, object> datos)
        {
            if (!rutaConFichero.EndsWith(".xlsx"))
                rutaConFichero = rutaConFichero + ".xlsx";

            FicheroConRuta = rutaConFichero.Replace(" ", "_");
            Datos = datos;
        }
    }

    public interface IExportadorExcel
    {
        void Inicializar(ObjetoParaExportar objeto);
        public string Exportar();
    }

    public class ColumnaDelExcel
    {
        public PropertyInfo Propiedad { get; set; }
        public string Etiqueta { get; set; }
        public bool Totalizar { get; set; }
        public enumFormato Formato { get; set; } = enumFormato.Numero_2;
    }

    public class ExportarExcel<T> : IExportadorExcel
    {
        private string Fichero { get; set; }
        LibroExcel<T> Libro { get; set; }
        List<ColumnaDelExcel> Encabezado { get; set; }

        public string PatronUrl { get; set; }

        public ExportarExcel(ObjetoParaExportar objeto, List<ColumnaDelExcel> encabezado, string patron)
        {
            Inicializar(objeto, encabezado,patron);
        }


        public void Inicializar(ObjetoParaExportar objeto, List<ColumnaDelExcel> encabezado, string patron)
        {
            Encabezado = encabezado;
            Inicializar(objeto);
            PatronUrl = patron;
        }

        public void Inicializar(ObjetoParaExportar objeto)
        {
            Fichero = objeto.FicheroConRuta;
            var hoja = new HojaExcel<T>((List<T>)objeto.Datos[ltrExcelExportador.Registros]);
            Libro = new LibroExcel<T>(Path.GetFileNameWithoutExtension(objeto.FicheroConRuta));
            Libro.hojas.Add(hoja);
        }

        public string Exportar()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var libroExcel = new ExcelPackage())
            {
                foreach (var hoja in Libro.hojas)
                {
                    var hojaExcel = libroExcel.Workbook.Worksheets.Add(hoja.Nombre);
                    var propiedades = typeof(T).GetProperties();
                    var titulos = string.Join("|", Encabezado.Select(c => c.Etiqueta));

                    var filas = Filas(hoja?.Registros, Encabezado, PatronUrl);
                    hojaExcel
                    .Encolumnado("A", 1, titulos)
                    .Tabla("A", 2, filas, totalizarColumnas: ColumnasDeTotales(Encabezado), encabezado: Encabezado)
                    .Cells.AutoFitColumns();
                    if (filas.Count > 0)
                    {
                        var tabla = hojaExcel.Tables.Add(new ExcelAddressBase(fromRow: 1, fromCol: 1, toRow: filas.Count + 1, toColumn: titulos.Split(Simbolos.separadorDeValores).Length), hoja.Nombre);
                        tabla.ShowHeader = true;
                        tabla.TableStyle = TableStyles.Light6;
                        tabla.ShowTotal = true;
                    }
                }

                libroExcel.SaveAs(new FileInfo(Fichero));
            }
            return Fichero;
        }

        private static List<int> ColumnasDeTotales(List<ColumnaDelExcel> encabezado)
        {
            var columnasDeTotales = new List<int>();
            var columnaDeTotal = 0;
            foreach (var columna in encabezado)
            {
                if (columna.Totalizar) columnasDeTotales.Add(columnaDeTotal);
                columnaDeTotal++;
            }
            return columnasDeTotales;
        }

        //private static List<List<ValorDeCelda>> Filas(List<T> elementos, List<ColumnaDelExcel> encabezado)
        //{
        //    var valores = new List<List<ValorDeCelda>>();
        //    if (elementos is null)
        //        return valores;
        //    foreach (T elemento in elementos)
        //    {
        //        var filaDeValores = new List<ValorDeCelda>();
        //        foreach (var columna in encabezado)
        //        {
        //            filaDeValores.Add(new ValorDeCelda
        //            {
        //                Valor = columna.Propiedad?.GetValue(elemento) ?? null
        //            });
        //        }
        //        valores.Add(filaDeValores);
        //    }
        //    return valores;
        //}

        private static List<List<ValorDeCelda>> Filas(List<T> elementos, List<ColumnaDelExcel> encabezado, string patronUrl)
        {
            var valores = new List<List<ValorDeCelda>>();
            if (elementos is null) return valores;

            // Obtener propiedades de T usando reflexión
            var tipo = typeof(T);
            var propiedadId = tipo.GetProperty("Id");
            var propiedadReferencia = tipo?.GetProperty("Referencia") ?? null;

            foreach (T elemento in elementos)
            {
                var filaDeValores = new List<ValorDeCelda>();
                foreach (var columna in encabezado)
                {
                    if (columna.Propiedad.Name == "Referencia"  && propiedadReferencia != null)
                    {
                        // Obtener valores de Id y Referencia
                        var id = (int) propiedadId.GetValue(elemento);
                        var referencia = (string)propiedadReferencia.GetValue(elemento);

                        // Reemplazar en el patrón de URL
                        var valor = patronUrl
                            .Replace("[IdElemento]", id.ToString())
                            .Replace("[Referencia]", referencia);

                        filaDeValores.Add(new ValorDeCelda { Valor = valor, Formato = enumFormato.LinkHtml });
                    }
                    else
                    {
                        filaDeValores.Add(new ValorDeCelda
                        {
                            Valor = columna.Propiedad?.GetValue(elemento) ?? null
                        });
                    }
                }
                valores.Add(filaDeValores);
            }
            return valores;
        }


        //CrearCabecera(hojaExcel, properties);

        //var filaExcel = 2;
        //for (var i = 0; i < hoja.Filas.Count; i++)
        //{
        //    CrearFila(hojaExcel, hoja.Filas[i], properties, filaExcel);
        //    filaExcel++;
        //}

        //foreach (var columna in hojaExcel.Columns)
        //{
        //    columna.AutoFit(200);
        //}

        private static void CrearCabecera(ExcelWorksheet hojaExcel, PropertyInfo[] properties)
        {
            var celdas = new List<object>();
            // Recorrer las propiedades y excluir las que comienzan con "Id" y son de tipo entero
            foreach (var property in properties)
            {
                if (property.Name.StartsWith("Id") && property.PropertyType == typeof(int))
                {
                    continue;
                }
                // Agregar el valor de la propiedad a la lista de celdas
                celdas.Add(property.Name);
            }
            // Cargar la cabecera
            hojaExcel.Cells[$"A{1}"].LoadFromArrays(new[] { celdas.ToArray() });
        }

        private static void CrearFila(ExcelWorksheet hojaExcel, T elemento, PropertyInfo[] properties, int filaExcel)
        {
            var celdas = new List<object>();

            // Recorrer las propiedades y excluir las que comienzan con "Id" y son de tipo entero
            foreach (var property in properties)
            {
                if (property.Name.StartsWith("Id") && property.PropertyType == typeof(int))
                {
                    continue;
                }

                // Agregar el valor de la propiedad a la lista de celdas
                celdas.Add(property.GetValue(elemento));
            }

            // Cargar la fila actual en la hoja de Excel
            hojaExcel.Cells[$"A{filaExcel}"].LoadFromArrays(new[] { celdas.ToArray() });
        }

    }

    public class ValorDeCelda
    {
        public object Valor { get; set; }
        public enumFormato Formato { get; set; } = enumFormato.Sin_Formato;
        public bool Negrita { get; set; } = false;
    }

    public static class ExtensorExcel
    {

        public static ExcelWorksheet Informe(this ExcelWorksheet hoja, string rango, string titulo, string estilo = ltrExcelEstilos.Informe)
        {
            hoja.Cells[rango].Merge = true;
            hoja.Titulo(rango, titulo, estilo);
            return hoja;
        }

        public static ExcelWorksheet Titulos(this ExcelWorksheet hoja, string columna, int fila, string titulos, string estilo = ltrExcelEstilos.Titulos)
        {
            var filas = titulos.Split(Simbolos.separadorDeValores);
            var celdas = $"{columna}{fila}";
            for (int i = 1; i < filas.Length; i++)
            {
                celdas = $"{celdas}|{columna}{i + fila}";
            }
            return hoja.Titulos(celdas, titulos, estilo);
        }

        public static ExcelWorksheet Valores(this ExcelWorksheet hoja, string columna, int fila, List<ValorDeCelda> valores, string estilo = null)
        {
            var celdas = $"{columna}{fila}";
            for (int i = 1; i < valores.Count; i++)
            {
                celdas = $"{celdas}{Simbolos.separadorDeValores}{columna}{i + fila}";
            }
            return hoja.Valores(celdas, valores, estilo);
        }

        public static ExcelWorksheet Encolumnado(this ExcelWorksheet hoja, string columna, int fila, string encolumnado, string estilo = ltrExcelEstilos.Encolumnado)
        {
            var columnas = encolumnado.Split(Simbolos.separadorDeValores);
            var celdas = $"{columna}{fila}";
            for (int i = 1; i < columnas.Length; i++)
            {
                columna = ColumnaSiguiente(columna);
                celdas = $"{celdas}{Simbolos.separadorDeValores}{columna}{fila}";
            }
            return hoja.Encolumnado(celdas, encolumnado, estilo);
        }

        public static ExcelWorksheet Tabla(this ExcelWorksheet hoja, string columnaInicial, int filaDesde, List<List<ValorDeCelda>> filasParaTotalizar, List<int> totalizarColumnas = null, string estilo = null, List<ColumnaDelExcel> encabezado = null)
        {
            var columnas = 0;
            for (int i = 0; i < filasParaTotalizar.Count; i++)
            {
                var datos = filasParaTotalizar[i];
                columnas = filasParaTotalizar[i].Count;
                var columna = columnaInicial;
                for (int j = 0; j < datos.Count; j++)
                {
                    hoja.Valor($"{columna}{filaDesde + i}", datos[j], estilo);
                    columna = ColumnaSiguiente(columna);
                }
            }

            if (filasParaTotalizar.Count > 0 && totalizarColumnas != null && totalizarColumnas.Count > 0)
            {
                for (int z = 0; z < totalizarColumnas.Count; z++)
                {
                    var letraColumna = DesplazarColumna(columnaInicial, totalizarColumnas[z]);
                    var celdaDeTotal = $"{letraColumna}{filaDesde + filasParaTotalizar.Count}";
                    hoja.Cells[celdaDeTotal].Formula = $"=SUM({letraColumna}{filaDesde}:{letraColumna}{filaDesde + filasParaTotalizar.Count - 1})";
                    if (hoja.Workbook.Styles.NamedStyles.Any(s => s.Name == ltrExcelEstilos.Totales))
                    {
                        hoja.Cells[celdaDeTotal].StyleName = ltrExcelEstilos.Totales;
                    }
                    else
                    {
                        string formato = "#,##0.00";
                        if (encabezado != null)
                        {
                            var descriptor = encabezado.FirstOrDefault(d => d.Etiqueta == (string)hoja.Cells[$"{letraColumna}{filaDesde - 1}"].Value);
                            if (descriptor != null)
                            {
                                switch (descriptor.Formato)
                                {
                                    case enumFormato.Sin_Formato: formato = "0.00"; break;
                                    case enumFormato.Porcentaje: formato = "0.00%"; break;
                                    case enumFormato.Numero_2: formato = "#,##0.00"; break;
                                    case enumFormato.Numero_6: formato = "#,##0.000000"; break;
                                    case enumFormato.Moneda: formato = "€0.00"; break;
                                }
                            }
                        }
                        hoja.Cells[celdaDeTotal].Style.Numberformat.Format = formato;
                        hoja.Cells[celdaDeTotal].Style.Font.Bold = true;
                    }
                }
            }

            var columnaFit = columnaInicial;
            for (int i = 0; i < columnas; i++)
            {
                var filafinal = filaDesde + filasParaTotalizar.Count + (totalizarColumnas != null && totalizarColumnas.Count > 0 ? 1 : 0);
                var rango = $"{columnaFit}{filaDesde}:{columnaFit}{filafinal}";
                hoja.Cells[rango].AutoFitColumns();
                columnaFit = ColumnaSiguiente(columnaFit);
            }

            return hoja;
        }


        public static ExcelWorksheet Titulos(this ExcelWorksheet hoja, string rango, string titulos, string estilo = null)
        {
            var celdas = rango.Split(Simbolos.separadorDeValores);
            var valores = titulos.Split(Simbolos.separadorDeValores);
            var i = 0;
            foreach (var celda in celdas)
            {
                hoja.Titulo(celda, valores[i], estilo);
                i++;
            }
            return hoja;
        }


        public static ExcelWorksheet Encolumnado(this ExcelWorksheet hoja, string rango, string encolumnado, string estilo = null)
        {
            var celdas = rango.Split(Simbolos.separadorDeValores);
            var valores = encolumnado.Split(Simbolos.separadorDeValores);
            var i = 0;
            foreach (var celda in celdas)
            {
                hoja.Titulo(celda, valores[i], estilo);
                i++;
            }
            return hoja;
        }

        public static ExcelWorksheet Valores(this ExcelWorksheet hoja, string rango, List<ValorDeCelda> valores, string estilo = null)
        {
            var celdas = rango.Split(Simbolos.separadorDeValores);
            var i = 0;
            foreach (var celda in celdas)
            {
                hoja.Valor(celda, valores[i], estilo);
                i++;
            }
            return hoja;
        }

        public static ExcelWorksheet Valor(this ExcelWorksheet hoja, string rango, ValorDeCelda valor, string estilo = null)
        {
            if (!estilo.IsNullOrEmpty() && hoja.Workbook.Styles.NamedStyles.Any(s => s.Name == estilo))
            {
                hoja.Cells[rango].StyleName = estilo;
            }

            if (valor.Valor is not null)
            {
                if (typeof(decimal).IsAssignableFrom(valor.Valor.GetType()))
                    return hoja.Importe(rango, (decimal)valor.Valor, valor.Formato, valor.Negrita);

                if (typeof(DateTime).IsAssignableFrom(valor.Valor.GetType()))
                    return hoja.Fecha(rango, (DateTime)valor.Valor, valor.Formato, valor.Negrita);

                if (valor.Formato == enumFormato.LinkHtml)
                    return hoja.HiperVinculo(rango,(string)valor.Valor);
            }

            return hoja.Valor(rango, valor.Valor, valor.Negrita);
        }


        public static ExcelWorksheet Fecha(this ExcelWorksheet hoja, string rango, DateTime fecha, enumFormato formato = enumFormato.Fecha, bool negrita = false)
        {
            hoja.Cells[rango].Fecha(fecha, formato, negrita);
            return hoja;
        }

        public static ExcelWorksheet Importe(this ExcelWorksheet hoja, string rango, decimal importe, enumFormato formato = enumFormato.Numero_2, bool negrita = false)
        {
            hoja.Cells[rango].Importe(importe, formato, negrita);
            return hoja;
        }
        public static ExcelWorksheet HiperVinculo(this ExcelWorksheet hoja, string rango, string link)
        {
            hoja.Cells[rango].HiperVinculo(link);
            return hoja;
        }

        private static ExcelWorksheet Titulo(this ExcelWorksheet hoja, string rango, string valor, string estilo)
        {
            hoja.Cells[rango].Value = valor;
            if (!estilo.IsNullOrEmpty() && hoja.Workbook.Styles.NamedStyles.Any(s => s.Name == estilo))
            {
                hoja.Cells[rango].StyleName = estilo;
            }
            else
            {
                hoja.Cells[rango].Negrita();
            }
            return hoja;
        }

        public static ExcelWorksheet Valor(this ExcelWorksheet hoja, string rango, object valor, bool negrita = false)
        {
            hoja.Cells[rango].Value = valor;
            if (negrita) hoja.Cells[rango].Negrita();
            return hoja;
        }

        public static ExcelRange HiperVinculo(this ExcelRange celdas, string htmlLink)
        {
            if (string.IsNullOrEmpty(htmlLink))
            {
                celdas.Value = null;
                return celdas;
            }

            try
            {
                string pattern = @"<a.*?href=(['""])(?<url>.*?)\1.*?>(?<text>.*?)</a>";
                Match match = Regex.Match(htmlLink, pattern, RegexOptions.IgnoreCase);

                if (match.Success)
                {
                    string url = match.Groups["url"].Value;
                    string text = match.Groups["text"].Value;

                    // Set the cell value to the display text
                    celdas.Value = text;

                    // Create the hyperlink
                    celdas.Hyperlink = new Uri(url);

                    // Style the cell to look like a hyperlink (optional)
                    celdas.Style.Font.Color.SetColor(Color.Blue);
                }
                else
                {
                    // If the HTML doesn't match the expected format, just set the HTML as the cell value
                    celdas.Value = htmlLink;
                }
            }
            catch (UriFormatException)
            {
                celdas.Value = htmlLink;
            }

            return celdas;
        }

        public static ExcelRange Importe(this ExcelRange celdas, decimal importe, enumFormato formato = enumFormato.Numero_2, bool negrita = false)
        {
            celdas.Value = importe;
            if (negrita) celdas.Negrita();
            switch (formato)
            {
                case enumFormato.Sin_Formato:
                    celdas.Style.Numberformat.Format = "0.00";
                    return celdas;
                case enumFormato.Porcentaje:
                    celdas.Style.Numberformat.Format = "0.00%";
                    return celdas;
                case enumFormato.Moneda:
                    celdas.Style.Numberformat.Format = "€0.00";
                    return celdas;
                case enumFormato.Numero_2:
                    celdas.Style.Numberformat.Format = "0.00";
                    return celdas;
                case enumFormato.Numero_6:
                    celdas.Style.Numberformat.Format = "0.000000";
                    return celdas;
            }
            throw new Exception($"No se ha indicado como formatear en Excel el formato '{formato}'");
        }


        public static ExcelRange Fecha(this ExcelRange celdas, DateTime fecha, enumFormato formato = enumFormato.Fecha, bool negrita = false)
        {
            celdas.Value = fecha;
            if (negrita) celdas.Negrita();
            if (formato != enumFormato.Sin_Formato)
                celdas.Style.Numberformat.Format = formato.Descripcion();
            else
                celdas.Style.Numberformat.Format = enumFormato.Fecha.Descripcion();
            return celdas;
        }

        private static ExcelRange Negrita(this ExcelRange celdas)
        {
            celdas.Style.Font.Bold = true;
            return celdas;
        }

        public static void DefinirEstilos(this ExcelPackage libro)
        {
            var estiloColumnaTitulo = libro.Workbook.Styles.CreateNamedStyle(ltrExcelEstilos.Titulos);
            estiloColumnaTitulo.Style.Font.Bold = true;
            estiloColumnaTitulo.Style.Font.Color.SetColor(Color.White);
            estiloColumnaTitulo.Style.Fill.PatternType = ExcelFillStyle.Solid;
            estiloColumnaTitulo.Style.Fill.BackgroundColor.SetColor(Color.DarkBlue);

            var estiloTituloInforme = libro.Workbook.Styles.CreateNamedStyle(ltrExcelEstilos.Informe);
            estiloTituloInforme.Style.Font.Bold = true;
            estiloTituloInforme.Style.Font.Size = 16;
            estiloTituloInforme.Style.Font.Color.SetColor(Color.DarkBlue);
            estiloTituloInforme.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            estiloTituloInforme.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            estiloTituloInforme.Style.WrapText = true;

            var estiloEncolumnado = libro.Workbook.Styles.CreateNamedStyle(ltrExcelEstilos.Encolumnado);
            estiloEncolumnado.Style.Font.Bold = true;
            estiloEncolumnado.Style.Fill.PatternType = ExcelFillStyle.Solid;
            estiloEncolumnado.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
            estiloEncolumnado.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            estiloEncolumnado.Style.Border.Top.Color.SetColor(Color.Black);
            estiloEncolumnado.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            estiloEncolumnado.Style.Border.Left.Color.SetColor(Color.Black);
            estiloEncolumnado.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            estiloEncolumnado.Style.Border.Right.Color.SetColor(Color.Black);
            estiloEncolumnado.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            estiloEncolumnado.Style.Border.Bottom.Color.SetColor(Color.Black);

            var estiloCeldaTotales = libro.Workbook.Styles.CreateNamedStyle(ltrExcelEstilos.Totales);
            estiloCeldaTotales.Style.Font.Bold = true;
            estiloCeldaTotales.Style.Font.Color.SetColor(Color.Black);
            estiloCeldaTotales.Style.Fill.PatternType = ExcelFillStyle.None;
            estiloCeldaTotales.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            estiloCeldaTotales.Style.Border.Top.Color.SetColor(Color.Black);
            estiloCeldaTotales.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            estiloCeldaTotales.Style.Border.Left.Color.SetColor(Color.Black);
            estiloCeldaTotales.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            estiloCeldaTotales.Style.Border.Right.Color.SetColor(Color.Black);
            estiloCeldaTotales.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            estiloCeldaTotales.Style.Border.Bottom.Color.SetColor(Color.Black);
            //estiloCeldaTotales.Style.Border.Diagonal.Style = ExcelBorderStyle.Thin;
            //estiloCeldaTotales.Style.Border.Diagonal.Color.SetColor(Color.Black);
            //estiloCeldaTotales.Style.Border.DiagonalUp = true;



            var estiloCeldaValoresDeTitulo = libro.Workbook.Styles.CreateNamedStyle(ltrExcelEstilos.ValoresDeTitulos);
            estiloCeldaValoresDeTitulo.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
        }

        private static string DesplazarColumna(string columna, int cantidad)
        {
            for (int i = 0; i < cantidad; i++)
            {
                columna = ColumnaSiguiente(columna);
            }
            return columna;
        }

        public static string ColumnaSiguiente(string columnaActual)
        {
            int numeroDeColumna = ColumnaToNumero(columnaActual);
            string siguienteColumna = NumeroToColumna(numeroDeColumna + 1);

            return siguienteColumna;
        }

        private static int ColumnaToNumero(string column)
        {
            int number = 0;
            int pow = 1;

            for (int i = column.Length - 1; i >= 0; i--)
            {
                char c = column[i];
                if (c >= 'A' && c <= 'Z')
                {
                    number += (c - 'A' + 1) * pow;
                    pow *= 26;
                }
            }

            return number;
        }

        private static string NumeroToColumna(int number)
        {
            string column = "";

            while (number > 0)
            {
                number--;
                int remainder = number % 26;
                column = ((char)(remainder + 'A')).ToString() + column;
                number /= 26;
            }

            return column;
        }

    }

}
