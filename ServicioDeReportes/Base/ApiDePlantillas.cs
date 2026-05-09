using DocumentFormat.OpenXml.Wordprocessing;
using Table = DocumentFormat.OpenXml.Wordprocessing.Table;
using Text = DocumentFormat.OpenXml.Wordprocessing.Text;
using Run = DocumentFormat.OpenXml.Wordprocessing.Run;
using pltMapeosDeTabla = System.Collections.Generic.Dictionary<string, string>;
using pltFilaDeTabla = System.Collections.Generic.Dictionary<string, string>;
using pltDatosDePlantilla = System.Collections.Generic.Dictionary<string, string>;
using pltFormulasDePlantilla = System.Collections.Generic.Dictionary<string, string>;
using DocumentFormat.OpenXml;
using Paragraph = DocumentFormat.OpenXml.Wordprocessing.Paragraph;
using Utilidades;
using DocumentFormat.OpenXml.Packaging;
using ServicioDeDatos.Elemento;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Text;

namespace ServicioDeReportes.Base
{
    public static class ApiDePlantillas
    {

        public static string CrearLaPlantilla(string nombre)
        {
            if (!Path.Exists(enumRutas.RutaDePlantillas))
                Directory.CreateDirectory(enumRutas.RutaDePlantillas);
            string plantilla = Path.Combine(enumRutas.RutaDePlantillas, nombre);

            using (WordprocessingDocument wordDocument = WordprocessingDocument.Create(plantilla, WordprocessingDocumentType.Document))
            {
                MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
                mainPart.Document = new Document();
                mainPart.Document.Body = new Body();
            }
            return plantilla;
        }


        public static void EliminarLaPlantilla(this IPlantillaConAccion plantilla) => File.Delete(Path.Combine(enumRutas.RutaDePlantillas, plantilla.fichero));

        public static void ProcesarParte(OpenXmlCompositeElement parte, pltFormulasDePlantilla formulasDePlantilla, Dictionary<string, pltDatosDePlantilla> datosDePlantilla, Dictionary<string, Dictionary<string, object>> datosDelObjeto)
        {
            ProcesarEtiquetasDeUnPa(parte, datosDePlantilla);
            ProcesarEtiquetasDelDto(parte, datosDelObjeto);
            ProcesarFormulas(parte, formulasDePlantilla);
        }

        public static void ProcesarMapeosDeTablasDelPa(OpenXmlCompositeElement parte, Dictionary<string, pltMapeosDeTabla> descriptorDeMapeos, Dictionary<string, List<pltFilaDeTabla>> filasDeTablas)
        {
            foreach (var descriptor in descriptorDeMapeos)
            {
                var texto = parte.Descendants<Text>().FirstOrDefault(t => t.Text.ToLower().Equals(Simbolos.PltInicio + descriptor.Key + Simbolos.PltCierre));
                if (texto == null) continue;
                var elemento = texto.Parent;
                TableRow? filaMarcador = null;
                while (elemento != null)
                {
                    if (elemento == null) return;
                    if (elemento is TableRow) filaMarcador = (TableRow)elemento;
                    if (elemento is Table) break;
                    elemento = elemento.Parent;
                }
                if (filasDeTablas.ContainsKey(descriptor.Key))
                {
                    CrearLineasEnLaTabla(elemento as Table, descriptor.Value, filasDeTablas[descriptor.Key]);
                    filaMarcador?.Remove();
                }
            }
        }

        public static void ProcesarMapeosDeDetalles(OpenXmlCompositeElement parte, DetallesDelObjeto detalles)
        {
            foreach (var informacionDeDetalle in detalles.Detalles)
            {
                var textos = parte.Descendants<Text>().ToList();
                if (textos is null || textos.Count == 0) return;
                var encabezados = textos.Where(t => t.Text.ToLower() == $"{Simbolos.PltInicio}{informacionDeDetalle.Key.ToLower()}{Simbolos.PltCierre}").ToList();
                foreach (var item in encabezados)
                {
                    ProcesarItemSiEsTabla(item, informacionDeDetalle.Value);
                }
            }
        }

        public static void ProcesarMapeosDeExtensiones<T>(OpenXmlCompositeElement parte, List<T> detalles, enumEncabezadosDeTablas encabezado)
        {
            var textos = parte.Descendants<Text>().ToList();
            if (textos is null || textos.Count == 0) return;
            var encabezados = textos.Where(t => t.Text == $"{Simbolos.PltInicio}{encabezado}{Simbolos.PltCierre}").ToList();
            foreach (var item in encabezados)
            {
                ProcesarItemSiEsTabla(item, detalles);
            }
        }

        private static void ProcesarItemSiEsTabla<T>(Text item, List<T> detalles)
        {
            var elemento = item.Parent;
            TableRow? filaMarcador = null;
            while (elemento != null)
            {
                if (elemento == null) return;
                if (elemento is TableRow) filaMarcador = (TableRow)elemento;
                if (elemento is Table) break;
                elemento = elemento.Parent;
            }
            if (elemento != null)
            {
                CrearFilasEnLaTabla((Table)elemento, detalles);
                filaMarcador?.Remove();
            }

        }

        private static void ProcesarFormulas(OpenXmlCompositeElement documento, pltFormulasDePlantilla formulasDePlantilla)
        {
            var formulas = documento.Descendants<Text>().Where(t => t.Text.ToLower().Contains($"{Simbolos.PltInicio}formula.")).ToList();
            foreach (Text formula in formulas)
            {
                var clave = formula.Text.Split(".")[1].Replace(Simbolos.PltCierre, "").Trim();
                if (!formulasDePlantilla.ContainsKey(clave)) continue;
                if (formulasDePlantilla[clave] == "DateTime.Now")
                    formula.Text = formula.Text.Replace(Simbolos.PltInicio + "formula." + clave + Simbolos.PltCierre, DateTime.Now.ToString("dd-MM-yyyy"));
            }
        }

        private static void ProcesarEtiquetasDeUnPaXX(OpenXmlCompositeElement parte, Dictionary<string, pltDatosDePlantilla> datosDePlantilla)
        {
            foreach (var agrupacion in datosDePlantilla)
            {
                var parrafos = parte.Descendants<Text>().Where(t => t.InnerText.ToLower().Contains(Simbolos.PltInicio + agrupacion.Key.ToLower() + ".")).ToList();
                foreach (Text parrafo in parrafos)
                {
                    bool parrafoProcesado = false;
                    var posicion = 0;
                    while (!parrafoProcesado)
                    {
                        var posInicial = parrafo.Text.Length > posicion ? parrafo.Text.IndexOf(Simbolos.PltInicio, posicion) : -1;
                        if (posInicial >= 0)
                        {
                            var posFinal = parrafo.Text.IndexOf(Simbolos.PltCierre, posicion);
                            var etiqueta = parrafo.Text.Substring(posInicial + Simbolos.PltInicio.Length, posFinal - posInicial - Simbolos.PltInicio.Length);
                            var partes = etiqueta.Split(".");
                            if (partes.Length > 1)
                            {
                                var clave = partes[1].Trim();
                                if (agrupacion.Value.ContainsKey(clave))
                                {
                                    parrafo.Text = parrafo.Text.Replace(Simbolos.PltInicio + agrupacion.Key + "." + clave + Simbolos.PltCierre, agrupacion.Value[clave]);
                                }
                                posicion = posInicial + 3;
                            }
                            else posicion = posFinal;
                        }
                        else parrafoProcesado = true;
                    }
                }
            }

        }

        private static void ProcesarEtiquetasDeUnPaX(OpenXmlCompositeElement parte, Dictionary<string, pltDatosDePlantilla> datosDePlantilla)
        {
            var parrafos = parte.Descendants<Text>().ToList();
            foreach (Text parrafo in parrafos)
            {
                foreach (var agrupacion in datosDePlantilla)
                {
                    var inicio = 0;
                    while (inicio < parrafo.Text.Length - Simbolos.PltInicio.Length - Simbolos.PltCierre.Length)
                    {
                        var posicion = parrafo.Text.IndexOf(Simbolos.PltInicio + agrupacion.Key, inicio, StringComparison.CurrentCultureIgnoreCase);
                        if (posicion < 0) break;
                        var final = parrafo.Text.IndexOf(Simbolos.PltCierre, inicio);
                        if (final < 0) break;
                        inicio = final + Simbolos.PltCierre.Length;
                        var etiqueta = parrafo.Text.Substring(posicion + Simbolos.PltInicio.Length, final - posicion - Simbolos.PltCierre.Length);
                        if (etiqueta.IndexOf(".") < 0) break;
                        var clave = etiqueta.Split(".")[1];
                        var dic = datosDePlantilla[agrupacion.Key.ToString()];
                        if (dic.ContieneClave(clave))
                            parte.InnerXml = parte.InnerXml.ToLower().Replace($"{Simbolos.PltInicio}{agrupacion.Key}.{clave}{Simbolos.PltCierre}".ToLower(), dic[dic.Clave(clave)]);
                    }
                }
            }
        }


        private static void ProcesarEtiquetasDeUnPa(OpenXmlCompositeElement parte, Dictionary<string, pltDatosDePlantilla> datosDePlantilla)
        {
            string pattern = Simbolos.PltInicio + @"(" + string.Join("|", datosDePlantilla.Keys.Select(k => Regex.Escape(k.ToLower()))) + @")\.(\w+)" + Simbolos.PltCierre;
            foreach (var agrupacion in datosDePlantilla)
            {
                string lowerKey = agrupacion.Key.ToLower();
                var informacionParaRemplazar = datosDePlantilla[agrupacion.Key];
                parte.InnerXml = Regex.Replace(parte.InnerXml, pattern, m =>
                {
                    if (m.Groups[1].Value.ToLower() == lowerKey && informacionParaRemplazar.ContainsKey(m.Groups[2].Value))
                    {
                        return m.Groups[0].Value.ToLower().Replace($"{Simbolos.PltInicio}{agrupacion.Key}.{m.Groups[2].Value}{Simbolos.PltCierre}".ToLower(), informacionParaRemplazar[m.Groups[2].Value]);
                    }
                    return m.Groups[0].Value;
                });
            }
        }

        private static void ProcesarEtiquetasDelDto(OpenXmlCompositeElement parte, Dictionary<string, Dictionary<string, object>> datosDelObjeto)
        {
            var textos = parte.Descendants<Text>().ToList();
            foreach (var (entradaKey, entradaValue) in datosDelObjeto)
            {
                foreach (var (claveKey, claveValue) in entradaValue)
                {
                    var sustituir = $"{entradaKey}.{claveKey}".ToLowerInvariant().Trim();
                    ProcesarSustitucion(textos, sustituir, claveValue);
                }
            }
        }

        private static void ProcesarSustitucion(List<Text> textos, string sustituir, object valor)
        {
            for (int i = 0; i < textos.Count; i++)
            {
                if (!textos[i].Text.Contains(Simbolos.PltInicio)) continue;

                var (textoCompleto, finIndex) = ObtenerTextoCompleto(textos, i);
                if (string.IsNullOrEmpty(textoCompleto)) continue;

                var textoLower = textoCompleto.ToLowerInvariant().Replace(" ", "");
                if (!textoLower.Contains(sustituir)) continue;

                if (DebeRealizarSustitucion(textoLower, sustituir))
                {
                    RealizarSustitucion(textos, i, finIndex, valor);
                }

                i = finIndex; // Saltar al final del bloque procesado
            }
        }

        private static (string textoCompleto, int finIndex) ObtenerTextoCompleto(List<Text> textos, int inicio)
        {
            var sb = new StringBuilder();
            for (int j = inicio; j < textos.Count; j++)
            {
                sb.Append(textos[j].Text);
                if (textos[j].Text.Contains(Simbolos.PltCierre))
                {
                    return (sb.ToString(), j);
                }
            }
            return (string.Empty, inicio);
        }

        private static bool DebeRealizarSustitucion(string textoLower, string sustituir)
        {
            int posicion = textoLower.IndexOf(sustituir);
            int posicionSiguiente = posicion + sustituir.Length;
            return posicionSiguiente < textoLower.Length &&
                   (textoLower[posicionSiguiente] == '.' || textoLower[posicionSiguiente] == '}');
        }

        private static void RealizarSustitucion(List<Text> textos, int inicio, int fin, object valor)
        {
            var posDeCierre = textos[fin].Text.IndexOf(Simbolos.PltCierre);
            var cadenaFinal = textos[fin].Text.Substring(posDeCierre + Simbolos.PltCierre.Length);

            SustituirTexto(textos[inicio], valor, cadenaFinal);
            for (int j = inicio + 1; j <= fin; j++)
            {
                textos[j].Text = string.Empty;
            }
        }



        private static void ProcesarEtiquetasDelDtoXX(OpenXmlCompositeElement parte, Dictionary<string, Dictionary<string, object>> datosDelObjeto)
        {
            var p = parte.Descendants<Text>().ToList();
            foreach (var entrada in datosDelObjeto)
            {
                foreach (var clave in entrada.Value)
                {
                    var sustituir = (entrada.Key + "." + clave.Key).ToLower().Trim();
                    var itemInicio = -1;
                    var itemfin = -1;
                    for (int i = 0; i < p.Count; i++)
                    {
                        //item de inicio 
                        if (p[i].Text.Contains(Simbolos.PltInicio))
                            itemInicio = i;
                        if (itemInicio > -1 && p[i].Text.Contains(Simbolos.PltCierre))
                            itemfin = i;
                        if (itemfin > -1)
                        {
                            var texto = "";
                            for (int j = itemInicio; j <= itemfin; j++)
                                texto = texto + p[j].Text;

                            texto = texto.ToLower().Replace(" ", "");
                            if (texto.Contains(sustituir))
                            {
                                int posicion = texto.IndexOf(sustituir);
                                int posicionSiguiente = posicion + sustituir.Length;
                                if (posicionSiguiente < texto.Length)
                                {
                                    var caracter = texto[posicionSiguiente];
                                    if (caracter == '.' || caracter == '}')
                                    {
                                        var posDeCierre = p[itemfin].Text.IndexOf(Simbolos.PltCierre);
                                        var cadenaFinal = p[itemfin].Text.Substring(posDeCierre + Simbolos.PltCierre.Length);

                                        SustituirTexto(p[itemInicio], clave.Value, cadenaFinal);
                                        for (int j = itemInicio+1; j <= itemfin; j++)
                                            p[j].Text = "";
                                    }
                                }
                            }
                            itemfin = -1;
                            itemInicio = -1;
                        }

                    }
                }
            }
        }


        private static void ProcesarEtiquetasDelDtoX(OpenXmlCompositeElement parte, Dictionary<string, Dictionary<string, object>> datosDelObjeto)
        {
            foreach (var entrada in datosDelObjeto)
            {
                var parrafos = parte.Descendants<Text>().Where(t => t.InnerText.ToLower().Contains(Simbolos.PltInicio + entrada.Key.ToLower() + ".")).ToList();
                foreach (Text parrafo in parrafos)
                {
                    string pattern = @"{{{(.*?)\.(.*?)}}}";
                    Match match = Regex.Match(parrafo.Text, pattern);
                    while (match.Success)
                    {
                        var claveDeEntrada = match.Groups[1].Value.ToLower().Trim();
                        if (entrada.Key.ToLower() == claveDeEntrada)
                        {
                            var claveDelCampo = match.Groups[2].Value.Trim();
                            var campos = entrada.Value;
                            if (campos.Keys.Any(x => x.Contains(claveDelCampo, StringComparison.CurrentCultureIgnoreCase)))
                            {
                                var sustituir = Simbolos.PltInicio + entrada.Key + "." + claveDelCampo + Simbolos.PltCierre;
                                var formato = parrafo.Text.Siguientes(match.Index + match.Length, 3) == ".F(" ? parrafo.Text.Siguientes(match.Index + match.Length, ")") : "";
                                SustituirEtiqueta(parrafo, sustituir.ToLower(), campos.LeerValor(claveDelCampo, (object?)""));
                            }
                            else break;
                        }
                        match = Regex.Match(parrafo.Text, pattern);
                    }
                }
            }
        }


        private static void CrearFilasEnLaTabla<T>(Table tabla, List<T> detalles)
        {

            List<TableRow> filas = tabla.Descendants<TableRow>().ToList();
            var ultimaFila = filas.Last(x => !x.InnerText.IsNullOrEmpty());
            foreach (var detalle in detalles)
            {
                if (detalle is null) continue;
                TableRow nuevaFila = (TableRow)ultimaFila.CloneNode(true);
                var cells = nuevaFila.Elements<TableCell>().ToList();
                foreach (TableCell celda in cells)
                {
                    var texto = celda.GetFirstChild<Paragraph>()?.GetFirstChild<Run>()?.GetFirstChild<Text>();
                    if (texto == null) continue;
                    foreach (PropertyInfo propiedad in detalle.GetType().GetProperties())
                    {
                        var sustituir = $"{Simbolos.PltInicio}{propiedad.Name}{Simbolos.PltCierre}".ToLower();
                        if (celda.InnerText.ToLower() == sustituir)
                        {
                            var valor = propiedad.GetValue(detalle);
                            SustituirEtiqueta(texto, sustituir, valor is null ? "" : valor);
                            break;
                        }
                    }
                }
                tabla.InsertBefore(nuevaFila, ultimaFila);
            }


            while (tabla.Elements<TableRow>().LastOrDefault(x => x.InnerText.IsNullOrEmpty()) != null)
            {
                tabla.Elements<TableRow>()?.LastOrDefault()?.Remove();
            }
            var lastRow = tabla.Elements<TableRow>().LastOrDefault();
            if (lastRow != null) lastRow.Remove();
        }

        private static void CrearLineasEnLaTabla(Table? tabla, pltMapeosDeTabla descriptor, List<pltFilaDeTabla> datos)
        {
            if (tabla != null)
            {
                List<TableRow> filas = tabla.Descendants<TableRow>().ToList();
                var ultimaFila = filas.Last(x => !x.InnerText.IsNullOrEmpty());
                foreach (pltFilaDeTabla linea in datos)
                {
                    TableRow nuevaFila = (TableRow)ultimaFila.CloneNode(true);
                    var cells = nuevaFila.Elements<TableCell>().ToList();
                    foreach (TableCell cell in cells)
                    {
                        var mapeo = descriptor.FirstOrDefault(x => Simbolos.PltInicio + x.Key.ToLower() + Simbolos.PltCierre == cell.InnerText.ToLower());
                        if (mapeo.Key != null) RemplazarMarcador(cell, linea, mapeo);
                    }
                    tabla.InsertBefore(nuevaFila, ultimaFila);
                }
                while (tabla.Elements<TableRow>().LastOrDefault(x => x.InnerText.IsNullOrEmpty()) != null)
                {
                    tabla.Elements<TableRow>()?.LastOrDefault()?.Remove();
                }
                var lastRow = tabla.Elements<TableRow>().LastOrDefault();
                if (lastRow != null) lastRow.Remove();
            }
        }

        private static void RemplazarMarcador(TableCell celda, pltFilaDeTabla linea, KeyValuePair<string, string> mapeo)
        {
            if (!linea.ContainsKey(mapeo.Value)) return;

            var texto = celda.GetFirstChild<Paragraph>()?.GetFirstChild<Run>()?.GetFirstChild<Text>();
            if (texto != null) texto.Text = linea[mapeo.Value];
        }

        private static void SustituirTexto(Text texto, object? valor, string cadenafinal)
        {
            var posIni = texto.Text.IndexOf(Simbolos.PltInicio);
            var cadenaInicial = texto.Text.Substring(0, posIni);


            if (valor is null || valor.EsCadena())
                texto.Text = $"{valor}";
            else if (valor.EsEntero()) texto.Text = ((int)valor).ToString();
            else if (valor.EsDecimal()) texto.Text = ((decimal)valor).ToString();
            else if (valor.EsFecha())
            {
                texto.Text = ((DateTime)valor).ToString();
            }
            else if (valor.EsBooleano()) texto.Text =  (bool)valor ? "Si": "No";
            else if (valor.EsEnumerado()) texto.Text = ((Enum)valor).Descripcion();

            texto.Text = cadenaInicial + texto.Text + cadenafinal;
        }


        private static void SustituirEtiqueta(Text etiqueta, string sustituir, object? valor)
        {
            if (valor is null || valor.EsCadena())
                etiqueta.Text = AsignarCadena(sustituir, etiqueta.Text.ToLower(), $"{valor}");
            else if (valor.EsEntero()) etiqueta.Text = AsignarEntero(sustituir, etiqueta.Text.ToLower(), (int)valor);
            else if (valor.EsDecimal()) etiqueta.Text = AsignarDecimal(sustituir, etiqueta.Text.ToLower(), (decimal)valor);
            else if (valor.EsFecha())
            {

                etiqueta.Text = AsignarFecha(sustituir, etiqueta.Text.ToLower(), (DateTime)valor);
            }
            else if (valor.EsBooleano()) etiqueta.Text = AsignarBooleano(sustituir, etiqueta.Text.ToLower(), (bool)valor);
            else if (valor.EsEnumerado()) etiqueta.Text = AsignarEnumerado(sustituir, etiqueta.Text.ToLower(), (Enum)valor);
        }

        private static bool EsEntero(this object valor) => valor is null ? false : valor.GetType() == typeof(int) || valor.GetType() == typeof(int?);
        private static bool EsDecimal(this object valor) => valor is null ? false : valor.GetType() == typeof(decimal) || valor.GetType() == typeof(decimal?);
        private static bool EsCadena(this object valor) => valor is null ? true : valor.GetType() == typeof(string);
        private static bool EsFecha(this object valor) => valor is null ? false : valor.GetType() == typeof(DateTime) || valor.GetType() == typeof(DateTime?);
        private static bool EsBooleano(this object valor) => valor is null ? false : valor.GetType() == typeof(bool) || valor.GetType() == typeof(bool?);
        private static bool EsEnumerado(this object valor)
        {
            var tipo = valor.GetType();
            if (tipo.IsEnum) return true;

            return tipo.IsGenericType && tipo.GetGenericTypeDefinition() == typeof(Nullable<>) && tipo.GetGenericArguments()[0].IsEnum;
        }

        private static string AsignarEntero(string sustituir, string texto, int numero)
        {
            var cadena = "";
            string pattern = @"Formatear\(N([0-9]|1[0-9]|20)\.([0-6])\)$";
            if (Regex.IsMatch(texto, pattern))
            {
                cadena = Regex.Replace(texto, pattern, m => numero.ToString($"N{m.Groups[1].Value},{m.Groups[2].Value}"));
            }
            else cadena = numero.ToString();

            return texto.Replace(sustituir, cadena);
        }

        private static string AsignarDecimal(string sustituir, string texto, decimal numero)
        {
            var cadena = "";
            string pattern = @"Formatear\(N([0-9]|1[0-9]|20)\.([0-6])\)$";
            if (Regex.IsMatch(texto, pattern))
            {
                cadena = Regex.Replace(texto, pattern, m => numero.ToString($"N{m.Groups[1].Value},{m.Groups[2].Value}"));
            }
            else cadena = numero.ToString("N2");

            return texto.Replace(sustituir, cadena);
        }

        private static string AsignarCadena(string sustituir, string texto, string valor) => texto.Replace(sustituir, valor.IsNullOrEmpty() ? "" : valor);

        private static string AsignarFecha(string sustituir, string texto, DateTime fecha) => texto.Replace(sustituir, fecha.ToString("dd-MM-yyyy"));
        private static string AsignarBooleano(string sustituir, string texto, bool valor) => texto.Replace(sustituir, valor ? extCadenas.enumCadenas.Si.ToString() : extCadenas.enumCadenas.No.ToString());

        private static string AsignarEnumerado(string sustituir, string texto, Enum valor) => texto.Replace(sustituir, valor.Descripcion());

    }
}
