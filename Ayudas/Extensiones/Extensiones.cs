using ConvertApiDotNet;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using HtmlAgilityPack;
using iText.Kernel.Pdf;
using iText.Signatures;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;
using RtfPipe;
using SevenZipExtractor;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;


namespace Utilidades
{
    public static class extCadenas
    {
        public enum enumCadenas { Si, No };

        public static bool IsNullOrEmpty(this string str, bool quitarBlancos = true)
        {
            if (str == null)
                return true;

            return string.IsNullOrEmpty(quitarBlancos ? str.Trim() : str);
        }

        public static bool MayorQueCero(this int? numero)
        {
            if (numero == null)
                return false;

            return numero > 0;
        }

        public static bool Evaluar(this string str, string cadena)
        {
            if (str.IsNullOrEmpty() || cadena.IsNullOrEmpty())
                return false;

            if (cadena.StartsWith("=") && cadena.Length > 1)
            {
                cadena = cadena.Substring(0, cadena.Length - 1);
                if (str.Length < cadena.Length)
                    return false;

                return str.Equals(cadena);
            }

            if (cadena.StartsWith(Simbolos.Pipe) && cadena.Length > 1)
            {
                cadena = cadena.Substring(1);
                if (str.Length < cadena.Length)
                    return false;

                return str.StartsWith(cadena);
            }

            if (cadena.EndsWith(Simbolos.Pipe) && cadena.Length > 1)
            {
                cadena = cadena.Substring(0, cadena.Length - 1);
                if (str.Length < cadena.Length)
                    return false;

                return str.EndsWith(cadena);
            }

            return str.Contains(cadena);
        }

        public static int Incluir(this List<int> lista, string cadena, string separador = Simbolos.PuntoComa, bool quitarCeros = true)
        {
            var elementos = lista.Count;
            if (cadena.IsNullOrEmpty())
                return 0;
            lista.AddRange(cadena.ToLista<int>(separador, quitarCeros));
            return lista.Count - elementos;
        }

        public static string ToString(this List<string> lista, string separador)
        {
            var retorno = "";
            foreach (var l in lista)
            {
                if (l.IsNullOrEmpty())
                    continue;
                retorno = $"{retorno}{separador}{l.Trim()}";
            }
            return retorno.IsNullOrEmpty() ? retorno : retorno.Substring(1);
        }

        public static string ToString(this List<int> lista, string separador)
        {
            var retorno = "";
            foreach (var l in lista)
                retorno = $"{retorno}{separador}{l}";

            return retorno.IsNullOrEmpty() ? retorno : retorno.Substring(1);
        }

        public static string ToString<T>(this List<T> lista, Func<T, string> propiedad, string separador)
        {
            StringBuilder cadena = new StringBuilder();

            foreach (var elemento in lista)
            {
                if (cadena.Length > 0)
                {
                    cadena.Append($"{separador} ");
                }

                cadena.Append(propiedad(elemento));
            }

            return cadena.ToString();
        }

        public static string IncrementarSufijo(this string input)
        {
            // Patrón para buscar un guion bajo seguido de uno o más dígitos al final de la cadena
            string pattern = @"_(\d+)$";

            // Comprobar si la cadena coincide con el patrón
            Match match = Regex.Match(input, pattern);

            if (match.Success)
            {
                // Extraer el número
                int number = int.Parse(match.Groups[1].Value);

                // Incrementar el número
                number++;

                // Reemplazar el sufijo antiguo con el nuevo
                return Regex.Replace(input, pattern, $"_{number}");
            }

            // Si no coincide con el patrón, devolver la cadena original
            return input + "_1";
        }

        public static List<T> ToLista<T>(this string str, string separador = Simbolos.PuntoComa, bool quitarNulos = true, bool quitarNegativos = false)
        {
            var l = new List<T>();
            if (str.IsNullOrEmpty())
                return l;

            var cadenas = !str.Contains(Simbolos.PuntoComa) && separador == Simbolos.PuntoComa && str.Contains(Simbolos.Coma) && typeof(T) == typeof(int)
            ? str.Split(Simbolos.Coma)
            : str.Split(separador);

            foreach (string c in cadenas)
            {
                if (typeof(T) == typeof(int))
                {
                    var i = c.Entero();
                    if (i == 0 && quitarNulos)
                        continue;

                    if (quitarNegativos && i < 0)
                        continue;

                    l.Add((T)(object)i);
                    continue;
                }

                if (typeof(T) == typeof(string))
                {
                    if (c.IsNullOrEmpty())
                    {
                        if (!quitarNulos)
                            l.Add((T)(object)"");
                        continue;
                    }
                    else l.Add((T)(object)c.Trim());
                    continue;
                }

                throw new Exception($"No se ha definido como se pasa a una lista el tipo {typeof(T)}");
            }
            return l;
        }

        public static List<T> UnirListas<T>(this List<List<T>> listas)
        {
            if (listas == null)
                return new List<T>();

            // Combina todos los elementos de las listas, elimina duplicados y convierte a string
            var elementosUnicos = listas
                .Where(l => l != null) // Por si alguna lista interna es null
                .SelectMany(l => l)
                .Distinct();

            return elementosUnicos.ToList();
        }

        public static string RemplazarCaracteres(this string str, string caracterDeRemplazo)
        {
            return str.RemplazarCaracteres(@"[^\w\.@-_]", caracterDeRemplazo);
        }

        public static string RemplazarCaracteres(this string cadena, string remplazar, string por)
        {
            while (cadena.IndexOf(remplazar) > -1)
                cadena = Regex.Replace(cadena, remplazar, por, RegexOptions.None);

            return cadena;
        }

        public static string QuitarSubcadenaInicial(this string cadena, string subcadena, string remplazarPor = "")
        {
            while (cadena.StartsWith(subcadena))
            {
                cadena = cadena.Remove(0, subcadena.Length).Insert(0, remplazarPor);
            }
            return cadena;
        }

        public static string QuitarDobleIntro(this string cadena)
        {
            do
            {
                cadena = cadena.Replace($"{Environment.NewLine} ", Environment.NewLine);
                cadena = cadena.Replace($" {Environment.NewLine}", Environment.NewLine);
                cadena = cadena.Replace($"{Environment.NewLine}{Environment.NewLine}", Environment.NewLine);
            }
            while (cadena.IndexOf($"{Environment.NewLine} ") > -1 || cadena.IndexOf($" {Environment.NewLine}") > -1 || cadena.IndexOf($"{Environment.NewLine}{Environment.NewLine}") > -1);
            return cadena;
        }

        public static string Left(this string str, int count, string terminacion = "")
        {
            if (string.IsNullOrEmpty(str))
            {
                str = string.Empty;
            }
            else if (str.Length > count)
            {
                str = str.Substring(0, count - terminacion.Length) + terminacion;
            }

            return str;
        }

        public static string Right(this string str, int count, bool ponerPuntosSuspensivos = false)
        {
            if (string.IsNullOrEmpty(str))
            {
                //Set valid empty string as string could be null
                str = string.Empty;
            }

            if (str.Length < count)
                ponerPuntosSuspensivos = false;

            if (ponerPuntosSuspensivos)
                count = count - 3;

            //Check if the value is valid
            if (str.Length > count)
            {
                //Make the string no longer than the max length
                str = str.Substring(str.Length - count, count);
            }

            if (ponerPuntosSuspensivos)
                str = str + "...";

            //Return the string
            return str;
        }

        public static byte[] HexStringToByteArray(this string cadena)
        {
            int length = cadena.Length;
            byte[] bytes = new byte[length / 2];
            for (int i = 0; i < length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(cadena.Substring(i, 2), 16);
            }
            return bytes;
        }

        public static string Siguientes(this string str, int posIni, string cadena)
        {
            if (str is null) return string.Empty;
            var posfin = str.IndexOf(cadena, posIni);
            if (posfin == -1) return string.Empty;
            if (posfin == 0) return cadena;
            return str.Siguientes(posIni, posfin - posIni);
        }

        public static string Siguientes(this string str, int pos, int cantidad)
        {
            if (str is null || str.Length <= pos) return string.Empty;

            var longitud = str.Length - pos;
            if (longitud < cantidad)
                cantidad = longitud;

            return str.Substring(pos, cantidad);
        }

        public static bool EsFalse(this string str)
        {
            if (str.IsNullOrEmpty()) return true;
            return str.Trim().Equals(false.ToString(), StringComparison.CurrentCultureIgnoreCase) ||
                 str.Trim().Equals("F", StringComparison.CurrentCultureIgnoreCase) ||
                 str.Trim().Equals("N", StringComparison.CurrentCultureIgnoreCase);
        }

        public static bool EsTrue(this string str)
        {
            if (str.IsNullOrEmpty()) return false;
            return str.Trim().Equals(true.ToString(), StringComparison.CurrentCultureIgnoreCase) ||
                 str.Trim().Equals("T", StringComparison.CurrentCultureIgnoreCase) ||
                 str.Trim().Equals("S", StringComparison.CurrentCultureIgnoreCase) ||
                 str.Trim().Equals(enumCadenas.Si.ToString(), StringComparison.CurrentCultureIgnoreCase);
        }

        public static bool EsTrue(this enumCadenas enumerado) => enumerado == enumCadenas.Si;

        public static bool EsTrue(this bool? valor)
        {
            if (valor is null) return false;
            return Convert.ToBoolean(valor);
        }

        public static bool EsFalse(this bool? valor)
        {
            if (valor is null) return false;
            return Convert.ToBoolean(valor) == false;
        }
        public static bool? TrueOrNull(this string str, bool nuloEsFalse = true)
        {
            if (str.IsNullOrEmpty()) return nuloEsFalse ? false : null;
            return str.Trim().Equals(true.ToString(), StringComparison.CurrentCultureIgnoreCase) ||
                 str.Trim().Equals("T", StringComparison.CurrentCultureIgnoreCase) ||
                 str.Trim().Equals("S", StringComparison.CurrentCultureIgnoreCase);
        }

        public static bool? FalseOrNull(this string str, bool nuloEsFalse = true)
        {
            if (str.IsNullOrEmpty()) return nuloEsFalse ? false : null;
            return str.Trim().Equals(false.ToString(), StringComparison.CurrentCultureIgnoreCase) ||
                 str.Trim().Equals("F", StringComparison.CurrentCultureIgnoreCase) ||
                 str.Trim().Equals("N", StringComparison.CurrentCultureIgnoreCase);
        }

        public static string NormalizarFichero(this string fileName)
        {
            string invalidChars = Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars()));
            string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

            fileName = Regex.Replace(fileName, invalidRegStr, "_");
            fileName = fileName.Replace(",", "_");
            return fileName.Replace(" ", "_").Left(200);
        }

        public static string NormalizeRuta(this string ruta)
        {
            string invalidChars = Regex.Escape(new string(Path.GetInvalidPathChars()));
            string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

            return Regex.Replace(ruta, invalidRegStr, "_");
        }

        public static string Base64UrlDecode(this string input)
        {
            if (input.IsNullOrEmpty()) return null;
            return input.Replace('-', '+').Replace('_', '/').PadRight(input.Length + (4 - input.Length % 4) % 4, '=');
        }


        public static string DecodeBase64String(string bodyBase64Url)
        {
            if (bodyBase64Url.IsNullOrEmpty())
                return "";

            var base64String = bodyBase64Url.Replace('-', '+').Replace('_', '/').PadRight(bodyBase64Url.Length + (4 - bodyBase64Url.Length % 4) % 4, '=');
            return Encoding.UTF8.GetString(Convert.FromBase64String(base64String));
        }

        public static string ConvertPlainTextToHtml(string plainText)
        {
            // Reemplazar los saltos de línea por etiquetas <br>
            string html = plainText.Replace(Environment.NewLine, Simbolos.br);
            html = html.Replace(Simbolos.retorno, Simbolos.br);

            // Reemplazar los tabuladores por indentación
            html = html.Replace(Simbolos.tabulador, Simbolos.tabuladorHtml);

            return html;
        }

        public static string ConvertHtmlToPlainText(string html)
        {
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html.Replace(Simbolos.br, Environment.NewLine));

            var text = htmlDocument.DocumentNode.InnerText;
            text = WebUtility.HtmlDecode(text);
            // text = text.Replace("&lt;","<").Replace("&gt;", ">").Replace("&quot;", "\"").Replace("&#39;", @"'");
            return text;
        }

    }

    public static class extFechas
    {
        public const string DiaHora = "dd-MM-yyyy HH:mm:ss";
        public const string Dia = "dd-MM-yyyy";

        static string[] formatos = {
               // ISO 8601 con zona horaria (offset)
               "yyyy-MM-dd'T'HH:mm:ssK",
               "yyyy-MM-dd'T'HH:mm:sszzz",
           
               // ISO 8601 anteriores
               "yyyy-MM-ddTHH:mm:ss.fffZ",
               "yyyy-MM-ddTHH:mm:ssZ",
               "yyyy-MM-ddTHH:mm:ss.fff",
               "yyyy-MM-ddTHH:mm:ss",
               "yyyy-MM-dd'T'HH:mm:ss.fff'Z'",
               "yyyy-MM-dd'T'HH:mm:ss'Z'",
           
               // Fecha sola
               "yyyy-MM-dd",
               "dd/MM/yyyy",
               "dd-MM-yyyy",
               "MM/dd/yyyy",
           
               // Fecha + hora
               "dd/MM/yyyy HH:mm:ss",
               "dd/MM/yyyy H:mm:ss",
               "MM/dd/yyyy HH:mm:ss",
               "MM/dd/yyyy H:mm:ss",
               "d/m/yyyy HH:mm:ss",
               "m/d/yyyy HH:mm:ss",
               "M/d/yyyy h:mm:ss tt",
           
               // Caso especial
               "yyyy-dd-MM'T'HH:mm:ss.fff'Z'"
           };

        static string[] formatosOld = { 
    // Formatos ISO 8601 (Con 'T' y con/sin 'Z' o milisegundos)
    "yyyy-MM-ddTHH:mm:ss.fffZ",
    "yyyy-MM-ddTHH:mm:ssZ",
    "yyyy-MM-ddTHH:mm:ss.fff",
    "yyyy-MM-ddTHH:mm:ss",       // <--- Este es el que te fallaba recién
    "yyyy-MM-dd'T'HH:mm:ss.fff'Z'",
    "yyyy-MM-dd'T'HH:mm:ss'Z'",

    // Formatos con guiones y barras (Fecha corta)
    "yyyy-MM-dd",
    "dd/MM/yyyy",
    "dd-MM-yyyy",
    "MM/dd/yyyy",

    // Formatos con hora y fecha combinados (Espacios)
    "dd/MM/yyyy HH:mm:ss",
    "dd/MM/yyyy H:mm:ss",
    "MM/dd/yyyy HH:mm:ss",
    "MM/dd/yyyy H:mm:ss",
    "d/m/yyyy HH:mm:ss",
    "m/d/yyyy HH:mm:ss",
    "M/d/yyyy h:mm:ss tt",

    // Casos especiales de auditoría
    "yyyy-dd-MM'T'HH:mm:ss.fff'Z'"
};
        public static DateTime? ParsearFecha(this string fecha)
        {
            if (fecha.IsNullOrEmpty()) return default;

            DateTime fechaParseada;
            return DateTime.TryParseExact(fecha, formatos, CultureInfo.InvariantCulture, DateTimeStyles.None, out fechaParseada)
            ? fechaParseada
            : default;
        }

        public static DateTime? Fecha(this string fecha, bool finDelDia = false)
        {
            var fechaParseada = ParsearFecha(fecha);
            if (fechaParseada == default)
                return null;

            DateTimeOffset fechaOffset = DateTimeOffset.ParseExact(fecha, formatos, CultureInfo.InvariantCulture, DateTimeStyles.None);
            DateTime fechaSinUtc = fechaOffset.DateTime;
            if (finDelDia && fechaSinUtc.Hour == 0 && fechaSinUtc.Minute == 0 && fechaSinUtc.Second == 0)
            {
                var fechaFinDelDia = fechaSinUtc.AddHours(23);
                fechaFinDelDia = fechaFinDelDia.AddMinutes(59);
                fechaFinDelDia = fechaFinDelDia.AddSeconds(59);
                return fechaFinDelDia;
            }
            return fechaSinUtc;
        }

        public static DateTime FinDelDia(this DateTime fecha)
        {
            fecha.AddHours(23 - fecha.Hour);
            fecha.AddMinutes(59 - fecha.Minute);
            fecha.AddSeconds(59 - fecha.Second);
            return fecha;
        }

        public static bool Igual(this DateTime? fecha1, DateTime? fecha2)
        {
            if (fecha1 == default && fecha2 != default) return false;
            if (fecha1 != default && fecha2 == default) return false;
            if ((fecha1.Fecha() - fecha2.Fecha()).TotalSeconds > 0) return false;

            return true;
        }

        public static string FechaFormatoIso8601(this DateTime? fecha)
        {
            var input = fecha ?? new DateTime(1, 1, 1);
            var local = new DateTime(input.Year, input.Month, input.Day,
                input.Hour, input.Minute, input.Second, DateTimeKind.Local);

            return local.ToString("yyyy-MM-ddTHH:mm:ssK");
        }

        public static string FechaCorta(this DateTime? fecha, string formato = "dd-MM-yyyy", bool erroSiNoHay = false)
        {
            if (fecha == null && erroSiNoHay) throw new Exception("La fecha indicada es nula");

            return (fecha ?? new DateTime(1, 1, 1)).ToString(formato);
        }

        public static bool FechaConHora(this DateTime fecha) => fecha.Hour != 0 || fecha.Minute != 0 || fecha.Second != 0;

        public static bool ConFecha(this DateTime laFecha)
        {
            if (laFecha.Year == 1) return false;
            return laFecha.Year != 1901 || laFecha.Month != 01 || laFecha.Day != 01 || laFecha.FechaConHora();
        }


        public static bool ConFecha(this DateTime? fecha)
        {
            if (fecha == null) return false;
            var laFecha = (DateTime)fecha;
            return laFecha.Year == 1901 && laFecha.Month == 01 && laFecha.Day == 01 && laFecha.Hour == 0 && laFecha.Minute == 0 && laFecha.Second == 0;
        }

        public static DateTime AsignarHora(this DateTime? fecha)
        {
            var fechaorig = fecha.Fecha();
            var ahora = DateTime.Now;
            return new DateTime(fechaorig.Year, fechaorig.Month, fechaorig.Day, ahora.Hour, ahora.Minute, ahora.Second);
        }

        public static string Mes(int mes)
        {
            if (mes == 1) return "enero";
            if (mes == 2) return "febrero";
            if (mes == 3) return "marzo";
            if (mes == 4) return "abril";
            if (mes == 5) return "mayo";
            if (mes == 6) return "junio";
            if (mes == 7) return "julio";
            if (mes == 8) return "agosto";
            if (mes == 9) return "septiembre";
            if (mes == 10) return "octubre";
            if (mes == 11) return "noviembre";
            if (mes == 12) return "diciembre";

            throw new Exception($"Mes '{mes}' no contemplado");
        }

        public static DateTime MilisegundosToFecha(long? milisegundos, DateTimeKind dtk = DateTimeKind.Utc)
        {
            long internalDate = milisegundos ?? 0;
            return new DateTime(1970, 1, 1, 0, 0, 0, dtk).AddMilliseconds(internalDate);
        }

        public static DateTime? RedondearAlMinuto(this DateTime? dateTime, bool siguiente)
        {
            if (!dateTime.HasValue) return null;
            var date = dateTime.Value;
            if (siguiente)
            {
                return date.Second > 0 || date.Millisecond > 0
                    ? new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, 0).AddMinutes(1)
                    : new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, 0);
            }

            return new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, 0);
        }

        public static DateTime Fecha(this DateTime? fecha) => fecha == default ? DateTime.MinValue : (DateTime)fecha;

        public static int ValidarMes(string mesStr, string tipo)
        {
            if (!int.TryParse(mesStr, out int mes) || mes < 1 || mes > 12)
                throw new Exception($"El mes {tipo} '{mesStr}' no es válido. Debe ser un número entre 1 y 12.");
            return mes;
        }

        public static int ValidarDia(string diaStr, int mes, int anio, string tipo)
        {

            if (!int.TryParse(diaStr, out int dia))
                throw new Exception($"El día {tipo} '{diaStr}' no es válido. Debe ser un número.");

            int ultimoDiaDelMes = DateTime.DaysInMonth(anio, mes);
            if (dia < 1 || dia > ultimoDiaDelMes)
                throw new Exception($"El día {tipo} '{dia}' no es válido para el mes {mes}. Debe estar entre 1 y {ultimoDiaDelMes}.");

            return dia;
        }

        public static async Task<DateTime> ObtenerFechaUtcDeApiAsync()
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetStringAsync("https://worldtimeapi.org/api/timezone/Etc/UTC").ConfigureAwait(false);
                dynamic json = Newtonsoft.Json.JsonConvert.DeserializeObject(response);
                string fechaUtcIso = json.utc_datetime; // Ejemplo: "2025-06-24T18:14:23.456789+00:00"
                return DateTime.Parse(fechaUtcIso, null, System.Globalization.DateTimeStyles.RoundtripKind);
            }
        }


    }



    public class TimeApiResponse
    {
        public string currentLocalTime { get; set; }
        // Puedes agregar más propiedades si lo necesitas
    }

    public static class WorldTimeApiHelper
    {
        public static async Task<DateTime> ObtenerFechaLondresDeApiAsync()
        {
            var url = "https://timeapi.io/api/timezone/zone?timeZone=Europe%2FMadrid";
            using (var client = new HttpClient())
            {
                try
                {
                    // La petición debe ser GET, no POST
                    var response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    var json = await response.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject<TimeApiResponse>(json);

                    if (data?.currentLocalTime == null)
                        throw new Exception($"No se pudo obtener la fecha/hora de Madrid usando la Url '{url}'.");

                    return DateTime.Parse(data.currentLocalTime, null, DateTimeStyles.RoundtripKind);
                }
                catch (HttpRequestException ex)
                {
                    throw new Exception("Error al conectar con la API de timeapi.io: " + ex.Message, ex);
                }
                catch (Exception ex)
                {
                    throw new Exception("Error al obtener o procesar la fecha/hora de Madrid: " + ex.Message, ex);
                }
            }
        }
    }

    public static class extDiccionarios
    {

        public static Dictionary<string, object> ToDiccionario(this string json)
        {
            var diccionario = new Dictionary<string, object>();

            var arrayDefiltros = JsonConvert.DeserializeObject<List<object>>(json);
            for (int i = 0; i < arrayDefiltros.Count; i++)
            {
                var clave = ((Newtonsoft.Json.Linq.JArray)arrayDefiltros[i])[0].ToString();

                var item = ((Newtonsoft.Json.Linq.JArray)arrayDefiltros[i]);
                var valor = extDiccionarios.Parsear(item[1]);

                diccionario.Add(clave, valor);
            }
            return diccionario;
        }

        private static object Parsear(Newtonsoft.Json.Linq.JToken valor)
        {
            switch (valor.Type.ToString())
            {
                case nameof(Boolean): return Convert.ToBoolean(valor);
                case "Integer": return Convert.ToInt32(valor);
                case "Date": return Convert.ToDateTime(valor);
                case nameof(String): return Convert.ToString(valor);
            }
            throw new Exception($"No se ha definido como convertir el tipo {valor.Type} asociado al valor {valor}");
        }

        public static string Clave<T>(this Dictionary<string, T> diccionario, string clave)
        {
            if (diccionario == null)
                return null;

            foreach (var key in diccionario.Keys)
                if (key.ToLower().Equals(clave.ToLower()))
                    return key;
            return null;
        }

        public static bool ContieneClave<T>(this Dictionary<string, T> diccionario, string clave)
        {
            if (diccionario == null)
                return false;

            foreach (var key in diccionario.Keys)
                if (key.ToLower().Equals(clave.ToLower()))
                    return true;
            return false;
        }

        public static string LeerCadena(this Dictionary<string, string> diccionario, string clave, string valorPorDefecto)
        {
            if (diccionario == null)
                return valorPorDefecto;

            foreach (var key in diccionario.Keys)
                if (key.Equals(clave, StringComparison.CurrentCultureIgnoreCase))
                {
                    return diccionario[key];
                }
            return valorPorDefecto;
        }


        public static T LeerValor<T>(this Dictionary<string, object> diccionario, string clave)
        {
            var elemento = diccionario.FirstOrDefault(x => x.Key.Equals(clave, StringComparison.CurrentCultureIgnoreCase));
            if (elemento.Key is not null)
            {
                if (elemento.Value is null)
                {
                    if (typeof(T).EsNulable())
                        return (T)default;
                    throw new Exception($"El valor asociado a la {clave} es nulo, y el tipo {typeof(T).Name} no es nulable");
                }

                if (elemento.Value.GetType() == typeof(string) && ((string)elemento.Value).IsNullOrEmpty() && typeof(T) != typeof(string))
                {
                    if (typeof(T).EsNulable())
                        return (T)default;
                    throw new Exception($"El valor asociado a la {clave} es '', y el tipo {typeof(T).Name} no es nulable");
                }

                if (elemento.Value.GetType() == typeof(Newtonsoft.Json.Linq.JArray))
                    return ((Newtonsoft.Json.Linq.JArray)elemento.Value).ToObject<T>();

                if (typeof(T).BaseType == typeof(Enum) && elemento.Value is string)
                    return (T)Enum.Parse(typeof(T), (string)elemento.Value, ignoreCase: true);

                if (elemento.Value.GetType() == typeof(long) && typeof(T) == typeof(int))
                {
                    object value = Convert.ToInt32(elemento.Value);
                    return (T)Convert.ChangeType(value, typeof(T));
                }

                if (elemento.Value.GetType() == typeof(string) && typeof(T) == typeof(int) && elemento.Value.ToString().EsEntero())
                {
                    object value = Convert.ToInt32(elemento.Value);
                    return (T)Convert.ChangeType(value, typeof(T));
                }

                if ((elemento.Value.GetType() == typeof(long?) || elemento.Value.GetType() == typeof(long)) && typeof(T) == typeof(int?))
                {
                    object value = Convert.ToInt32(elemento.Value);
                    return (T)value;
                }

                if (elemento.Value.GetType() == typeof(string) && (typeof(T).IsEnum || Nullable.GetUnderlyingType(typeof(T))?.IsEnum == true))
                {
                    var valor = ApiDeEnsamblados.ToEnumerado(typeof(T), (string)elemento.Value, errorSiNoHay: false);
                    return (T)valor;
                }

                if (elemento.Value.GetType() == typeof(double) && (typeof(T) == typeof(decimal) || typeof(T) == typeof(decimal?)))
                {
                    object value = Convert.ToDecimal(elemento.Value);
                    return (T)value;
                }

                if (elemento.Value.GetType() == typeof(long) && (typeof(T) == typeof(decimal) || typeof(T) == typeof(decimal?)))
                {
                    object value = Convert.ToDecimal(elemento.Value);
                    return (T)value;
                }

                if (elemento.Value.GetType() == typeof(string) && (typeof(T) == typeof(List<string>)))
                {
                    var cadena = elemento.Value.ToString().Trim('[', ']');
                    string[] elementos = cadena.Split(',');
                    List<string> lista = elementos.Select(e => e.Trim('"')).ToList();
                    return (T)(object)lista;
                }

                if (elemento.Value.GetType() == typeof(double) && (typeof(T) == typeof(float) || typeof(T) == typeof(float?)))
                {
                    if (float.TryParse(elemento.Value.ToString(), out float floatValue))
                    {
                        return (T)(object)floatValue;
                    }
                    else
                    {
                        // Maneja el caso en el que la conversión falla
                        throw new InvalidOperationException($"No se pudo convertir el valor '{elemento.Value}' a {typeof(T)}.");
                    }
                }
                return (T)elemento.Value;
            }

            throw new Exception($"No se encuentra la clave '{clave}' en el diccionario");
        }


        public static T LeerValor<T>(this Dictionary<string, object> diccionario, Enum clave, T valorPorDefecto)
        {
            return LeerValor<T>(diccionario, clave.ToString(), valorPorDefecto);
        }

        public static T LeerValor<T>(this Dictionary<string, object> diccionario, string clave, T valorPorDefecto)
        {
            if (diccionario == null || !diccionario.ContieneClave(clave))
                return valorPorDefecto;
            try
            {
                return diccionario.LeerValor<T>(clave);
            }
            catch
            {
                return valorPorDefecto;
            }
        }

        public static Dictionary<string, object> Concatenar(this Dictionary<string, object> destino, Dictionary<string, object> origen)
        {
            if (origen != null)
            {
                foreach (var kvp in origen)
                {
                    if (!destino.ContainsKey(kvp.Key))
                    {
                        destino.Add(kvp.Key, kvp.Value);
                    }
                }
            }
            return destino;
        }

    }

    public static class extNumeros
    {
        public static int Entero(this string str)
        {
            int numero = 0;
            if (str.IsNullOrEmpty())
                return numero;

            int.TryParse(str, out numero);
            return numero;
        }

        public static decimal? TryNumero(this string str)
        {
            decimal numero;
            if (str.IsNullOrEmpty())
                return default(decimal?);

            str = ParsearSimboloDecimal(str);

            return decimal.TryParse(str, out numero) ? numero : default(decimal?);
        }

        public static decimal Decimal(this decimal? valor) => valor is null ? 0 : (decimal)valor;

        private static string ParsearSimboloDecimal(string str)
        {
            if (str.Contains(Simbolos.separadorDeMiles) && str.Contains(Simbolos.SeparadorDecimal))
            {
                var posDeMiles = str.LastIndexOf(Simbolos.separadorDeMiles);
                var posDecimal = str.LastIndexOf(Simbolos.SeparadorDecimal);
                if (posDeMiles < posDecimal && str.Count(f => f == Convert.ToChar(Simbolos.SeparadorDecimal)) == 1)
                    return str.Replace(Simbolos.separadorDeMiles, "");

                if (str.Count(f => f == Convert.ToChar(Simbolos.separadorDeMiles)) == 1 && posDecimal - posDeMiles == 3)
                    str = str.Replace(Simbolos.SeparadorDecimal, "").Replace(Simbolos.separadorDeMiles, Simbolos.SeparadorDecimal);

                throw new Exception($"El nº {str} está mal definido");
            }

            if (str.Contains(Simbolos.separadorDeMiles) && str.Count(f => f == Convert.ToChar(Simbolos.separadorDeMiles)) > 1)
                return str.Replace(Simbolos.separadorDeMiles, "");

            if (str.Contains(Simbolos.SeparadorDecimal) && str.Count(f => f == Convert.ToChar(Simbolos.SeparadorDecimal)) > 1)
                return str.Replace(Simbolos.SeparadorDecimal, "");

            if (str.Contains(Simbolos.separadorDeMiles) && str.Count(f => f == Convert.ToChar(Simbolos.separadorDeMiles)) == 1)
                return str.Replace(Simbolos.separadorDeMiles, Simbolos.SeparadorDecimal);

            return str;
        }

        public static bool EsEntero(this string str)
        {
            bool result = int.TryParse(str, out _);
            return result;
        }
        public static bool EsNumero(this string s)
        {
            bool result = decimal.TryParse(s, out _);
            return result;
        }


        public static int SumarDigitos(int? numero)
        {
            if (!numero.HasValue) return 0;

            int n = numero.Value;
            int suma = 0;
            while (n != 0)
            {
                suma += n % 10;
                n /= 10;
            }
            return suma;
        }


        public static decimal Decimal(this string str)
        {
            if (str.IsNullOrEmpty()) return 0;

            CultureInfo cultura = CultureInfo.CurrentCulture;
            string separadorDecimales = cultura.NumberFormat.NumberDecimalSeparator;
            string separadorMillares = cultura.NumberFormat.NumberGroupSeparator;
            decimal result;

            var hayMillares = str.Split(separadorMillares).Length >= 2;
            var hayDecimales = str.Split(separadorDecimales).Length == 2;
            var bloquesDeMillares = str.Split(separadorMillares).Length;
            var bloquesDeDecimales = str.Split(separadorDecimales).Length;
            if (bloquesDeDecimales > 2)
            {
                str = str.Replace(separadorDecimales, "");
            }

            if (hayMillares && !hayDecimales && bloquesDeMillares == 2)
            {
                str = str.Replace(separadorMillares, separadorDecimales);
            }

            if (decimal.TryParse(str, out result))
                return result;
            return 0;
        }

        public static string Formatear(this decimal? numero, decimal? valorPorDefecto = null, string formato = "F", int longitud = 12, int decimales = 2, bool alineacion = true, CultureInfo cultura = null, string separadorDecimal = null)
        {
            if (cultura is null) cultura = new CultureInfo("es-ES");

            if (valorPorDefecto is null && numero is null)
                return "";

            decimal valor = (numero is null) ? ((decimal)valorPorDefecto) : ((decimal)numero);
            return valor.Formatear(formato, longitud, decimales, alineacion, cultura, separadorDecimal);
        }

        public static string Formatear(this decimal numero, string formato = "F", int longitud = 12, int decimales = 2, bool alineacion = true, CultureInfo cultura = null, string separadorDecimal = null)
        {
            if (cultura is null) cultura = new CultureInfo("es-ES");
            var resultado = numero.ToString($"{formato}{decimales}", cultura);
            resultado = alineacion ? resultado.PadLeft(longitud) : resultado;

            if (!separadorDecimal.IsNullOrEmpty() && cultura.NumberFormat.NumberDecimalSeparator != separadorDecimal)
                return resultado.Replace(cultura.NumberFormat.NumberDecimalSeparator, separadorDecimal);

            return resultado;
        }

        public static string Porcentaje(this decimal? numero, decimal? valorPorDefecto = null, string formato = "F", int longitud = 4, int decimales = 2, bool alineacion = true, CultureInfo cultura = null)
        {
            if (cultura is null) cultura = new CultureInfo("es-ES");

            if (valorPorDefecto is null && numero is null)
                return "";

            decimal valor = (numero is null) ? ((decimal)valorPorDefecto) : ((decimal)numero);
            return valor.Porcentaje(formato, longitud, decimales, alineacion, cultura);
        }

        public static string Porcentaje(this decimal numero, string formato = "F", int longitud = 4, int decimales = 2, bool alineacion = true, CultureInfo cultura = null)
        {
            if (cultura is null) cultura = new CultureInfo("es-ES");
            var resultado = numero.ToString($"{formato}{decimales}", cultura) + "%";
            return alineacion ? resultado.PadLeft(longitud) : resultado;
        }

        public static string Moneda(this decimal? numero, decimal? valorPorDefecto = null, string formato = "C", int longitud = 12, int decimales = 2, bool alineacion = true, CultureInfo cultura = null)
        {
            if (cultura is null) cultura = new CultureInfo("es-ES");

            if (valorPorDefecto is null && numero is null)
                return "";

            decimal valor = (numero is null) ? ((decimal)valorPorDefecto) : ((decimal)numero);
            return valor.Moneda(formato, longitud, decimales, alineacion, cultura);
        }

        public static string ToMoneda(this decimal numero, string formato = "C", int longitud = 12, int decimales = 2, bool mostrarUltimoDecimal = true)
        {
            return Moneda(numero, formato, longitud, decimales, alineacion: false, cultura: null, mostrarUltimoDecimal, cadenaVaciaSiEsCero: false);
        }

        public static string Moneda(this decimal numero, string formato = "C", int longitud = 12, int decimales = 2, bool alineacion = true, CultureInfo cultura = null, bool mostrarUltimoDecimal = true, bool cadenaVaciaSiEsCero = false)
        {
            if (cultura is null) cultura = new CultureInfo("es-ES");

            if (cadenaVaciaSiEsCero && numero == 0M)
                return "";
            var esNegativo = numero < 0;
            if (esNegativo) numero = numero * -1;
            string resultado;
            if (mostrarUltimoDecimal)
            {
                // Obtener la parte decimal como string
                string parteDecimal = (numero % 1).ToString(cultura).TrimStart('0', ',', '.');

                // Si hay más decimales significativos que los especificados
                if (parteDecimal.Length > decimales && !parteDecimal.Substring(decimales).All(c => c == '0'))
                {
                    // Eliminar ceros no significativos al final
                    parteDecimal = parteDecimal.TrimEnd('0');

                    // Crear un nuevo formato con el número correcto de decimales
                    string nuevoFormato = formato + parteDecimal.Replace("-", "").Replace("0,", "").Length;  // parteDecimal.Replace("-", "").Replace("0,", "").Length;
                    resultado = numero.ToString(nuevoFormato, cultura);
                }
                else
                {
                    // Usar el formato original si no hay más decimales significativos
                    resultado = numero.ToString($"{formato}{decimales}", cultura);
                }
            }
            else
            {
                // Comportamiento original si MostrarUltimoDecimal es false
                resultado = numero.ToString($"{formato}{decimales}", cultura);
            }

            resultado = resultado.Trim();
            return (alineacion ? ((esNegativo ? "-" : "") + resultado).PadLeft(longitud) : (esNegativo ? "-" : "") + resultado);
        }

        public static string Formatear(this int? numero, int? valorPorDefecto = null, string formato = "F", int longitud = 8, int decimales = 0, bool alineacion = true, CultureInfo cultura = null)
        {
            if (cultura is null) cultura = new CultureInfo("es-ES");

            if (valorPorDefecto is null && numero is null)
                return "";

            int valor = (numero is null) ? ((int)valorPorDefecto) : ((int)numero);
            return valor.Formatear(formato, longitud, decimales, alineacion, cultura);
        }

        public static string Formatear(this int numero, string formato = "F", int longitud = 8, int decimales = 0, bool alineacion = true, CultureInfo cultura = null)
        {
            if (cultura is null) cultura = new CultureInfo("es-ES");
            var resultado = numero.ToString($"{formato}{decimales}", cultura).Trim();
            return alineacion ? resultado.PadLeft(longitud) : resultado;
        }

        public static int Entero(this int? numero) => numero == default ? 0 : (int)numero;

        public static bool Cierto(this bool? propiedad) => propiedad != default && (bool)propiedad;

    }

    public static class extEncriptacion
    {
        public static int GenerarEntero(string cadena = null)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(cadena is null ? DateTime.Now.Ticks.ToString() : cadena);
                byte[] hashBytes = sha256.ComputeHash(inputBytes);

                int entero = BitConverter.ToInt32(hashBytes, 0);

                if (entero < 0)
                {
                    unchecked
                    {
                        entero += int.MaxValue + 1;
                    }
                }

                return entero;
            }
        }

        public static int? StringToInt(this string cadena)
        {
            if (string.IsNullOrEmpty(cadena))
            {
                return null;
            }

            if (cadena.Length > 10)
            {
                throw new ArgumentException($"la longitud de '{cadena}' no ha de ser mayor de 10.");
            }

            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(cadena);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convertir el hash a un entero
                int result = BitConverter.ToInt32(hashBytes, 0);

                return result;
            }
        }

        public static string IntToString(this int numero)
        {
            byte[] bytes = BitConverter.GetBytes(numero);
            string idMensaje = Encoding.UTF8.GetString(bytes);

            return idMensaje;
        }

    }

    public static class extTexto
    {
        public static void FileToHtml(string ruta)
        {
            var contenido = File.ReadAllText(ruta);
            contenido = ToHtml(contenido, aplicarEncode: false);
            File.WriteAllText(ruta, contenido);
        }

        public static string ToHtml(string texto, bool aplicarEncode)
        {
            string contenido = texto.IsNullOrEmpty()
                    ? ""
                    : aplicarEncode ? HttpUtility.HtmlEncode(texto) : texto;

            contenido = contenido
                        .Replace("\r\n", "<br>")  // CRLF (Windows)
                        .Replace("\n", "<br>")    // LF (Unix)
                        .Replace("\r", "<br>");   // CR (Mac antiguo)


            return $@"<!DOCTYPE html>
            <html lang=""es"">
            <head>
                <meta charset=""UTF-8"">
                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                <title>Contenido del Correo</title>
            </head>
            <body class='visor-iframe-body'>
                <p>{contenido}</p>
            </body>
            </html>";
        }

        public static bool EsTexto(string contenido)
        {
            // Patrones para detectar etiquetas de marcado comunes
            string htmlPattern = @"<[^>]+>";
            string rtfPattern = @"{\\rtf[^}]+}";
            string pdfPattern = @"/Type\s*/Page|/Obj\s*\d+\s*0\s*obj"; // Búsqueda básica de estructura PDF
            string docxPattern = @"<w:document[^>]+>";
            string excelPattern = @"<(?i:worksheet)[^>]*>";

            // Comprobar si alguno de los patrones coincide
            bool hasHtmlTags = Regex.IsMatch(contenido, htmlPattern, RegexOptions.IgnoreCase);
            bool hasRtfTags = Regex.IsMatch(contenido, rtfPattern);
            bool hasPdfStructure = Regex.IsMatch(contenido, pdfPattern);
            bool hasDocxTags = Regex.IsMatch(contenido, docxPattern, RegexOptions.IgnoreCase);
            bool hasExcelTags = Regex.IsMatch(contenido, excelPattern, RegexOptions.IgnoreCase);

            // Si no se encuentran etiquetas de marcado, se considera texto plano
            return !(hasHtmlTags || hasRtfTags || hasPdfStructure || hasDocxTags || hasExcelTags);
        }

    }

    public static class extJson
    {
        public static bool EsArchivoJsonValido(string rutaArchivo)
        {
            try
            {
                string contenido = File.ReadAllText(rutaArchivo);
                JsonDocument.Parse(contenido);
                return true;
            }
            catch (System.Text.Json.JsonException)
            {
                return false;
            }
        }

        public static bool EsJsonValido(FileStream fileStream)
        {
            try
            {
                using (JsonDocument.Parse(fileStream))
                {
                    return true;
                }
            }
            catch (System.Text.Json.JsonException)
            {
                return false;
            }
        }


        public static void ValidarJson(string json)
        {

            var cache = ServicioDeCaches.Obtener(CacheDe.ValidarJson);
            if (!cache.ContainsKey(json))
            {
                JSchemaGenerator generator = new JSchemaGenerator();
                try
                {
                    JSchema schema = generator.Generate(typeof(List<Parametro>));
                    JArray actualJson = JArray.Parse(json);
                    bool valid = actualJson.IsValid(schema, out IList<string> errorMessages);

                    if (!valid)
                    {
                        var mensaje = "";
                        foreach (var me in errorMessages)
                        {
                            mensaje = $"{mensaje}{Environment.NewLine}{me}";
                        }
                        throw new Exception($"Parámetros Json mal definido.{Environment.NewLine}{json}{Environment.NewLine}{mensaje}");
                    }
                    else cache[json] = true;
                }
                catch (Exception exc)
                {
                    if (!exc.Message.Contains("The free-quota limit"))
                        throw new Exception($"Json mal definido", exc);
                }
            }
        }

        public static string ToJson(this List<Parametro> p)
        {
            if (p == null)
                p = new List<Parametro>();

            return JsonConvert.SerializeObject(p);
        }

        public static List<Parametro> ToListaDeParametros(this string json)
        {
            // ValidarJson(json);
            return JsonConvert.DeserializeObject<List<Parametro>>(json);
        }

        public static List<T> JsonToLista<T>(this string json)
        {
            if (json.IsNullOrEmpty())
                return new List<T>();
            return JsonConvert.DeserializeObject<List<T>>(json);
        }

        public static Dictionary<string, object> ToDiccionarioDeParametros(this string parametrosJson)
        {
            var parametros = new Dictionary<string, object>();
            if (!parametrosJson.IsNullOrEmpty())
            {
                var listaJson = parametrosJson.ToListaDeParametros();
                foreach (var p in listaJson)
                {
                    if (parametros.ContainsKey(p.parametro))
                        throw new Exception($"El parámetro '{p.parametro}' ya existe");
                    parametros.Add(p.parametro, p.valor);
                }
            }
            return parametros;
        }

        public static string ToJson(this List<string> lista)
        {
            if (lista == null)
                return null;
            return JsonConvert.SerializeObject(lista);
        }

        public static string ToJson(this List<int> lista)
        {
            if (lista == null)
                return null;
            return JsonConvert.SerializeObject(lista);
        }
        public static string ToJson(this Dictionary<string, object> dic)
        {
            var parametros = new List<Parametro>();
            foreach (var clave in dic.Keys)
            {
                var p = new Parametro(clave, dic[clave]);
                parametros.Add(p);
            }

            return parametros.ToJson();
        }

        public static void QuitarPropiedades(this JToken token, List<string> propiedades)
        {
            if (token is JObject obj)
            {
                foreach (var propiedad in propiedades)
                {
                    if (obj[propiedad] != null)
                    {
                        obj.Remove(propiedad);
                    }
                }

                foreach (var propiedad in obj.Properties())
                {
                    propiedad.Value.QuitarPropiedades(propiedades);
                }
            }
            else if (token is JArray arr)
            {
                foreach (var item in arr)
                {
                    item.QuitarPropiedades(propiedades);
                }
            }
        }

        public static object ObtenerPropiedad(this JsonDocument jsonDocument, string ruta)
        {
            JsonElement element = jsonDocument.RootElement;
            string[] propiedades = ruta.Split('.');

            foreach (var propiedad in propiedades)
            {
                if (propiedad.Contains("["))
                {
                    var arrayProp = propiedad.Split('[');
                    var arrayIndex = int.Parse(arrayProp[1].TrimEnd(']'));
                    element = element.GetProperty(arrayProp[0])[arrayIndex];
                }
                else
                {
                    if (!element.TryGetProperty(propiedad, out JsonElement childElement))
                    {
                        return null; // O manejar el caso de propiedad faltante de otra manera
                    }
                    element = childElement;
                }
            }

            return element.ValueKind switch
            {
                JsonValueKind.String => element.GetString(),
                JsonValueKind.Number => element.GetRawText(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                JsonValueKind.Null => null,
                _ => element.GetRawText()
            };
        }

        public static bool JsonEquals(string json1, string json2)
        {
            // Una comprobación rápida de nulidad o strings vacíos
            if (string.IsNullOrWhiteSpace(json1) || string.IsNullOrWhiteSpace(json2))
                return false;

            try
            {
                var j1 = JToken.Parse(json1);
                var j2 = JToken.Parse(json2);
                return JToken.DeepEquals(j1, j2);
            }
            catch (JsonReaderException)
            {
                return false;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static JObject Deserializar(string jsonString)
        {
            try
            {
                return JObject.Parse(jsonString);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("La cadena proporcionada no es un JSON válido.", ex);
            }
        }
    }

    public static class extPdf
    {
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
    }

    public static class extHtml
    {
        public static string SanitizeFile(string rutaConFichero)
        {
            string contenido = File.ReadAllText(rutaConFichero);
            if (contenido.Trim().Contains("!DOCTYPE html") && contenido.Contains("</html>")) // Mejora esta verificación si es necesario
            {
                string nombreArchivo = Path.GetFileNameWithoutExtension(rutaConFichero);
                string extension = Path.GetExtension(rutaConFichero);
                string directorio = Path.GetDirectoryName(rutaConFichero);

                // Sanitizar el contenido
                contenido = SanitizeHtml(contenido);

                // Construir la nueva ruta para el archivo saneado
                string rutaArchivoSanitizado = Path.Combine(directorio, nombreArchivo + "_saneado" + (!extension.StartsWith(".") ? "." + extension : extension));

                // Escribir el contenido saneado en un nuevo archivo
                File.WriteAllText(rutaArchivoSanitizado, contenido);

                return rutaArchivoSanitizado;
            }

            return rutaConFichero; // Si no es HTML, devuelve la ruta original
        }

        public static string SanitizeContenido(string contenido)
        {
            if (contenido.Trim().Contains("!DOCTYPE html") && contenido.Contains("</html>")) // Mejora esta verificación si es necesario
            {
                return contenido = SanitizeHtml(contenido);
            }
            return contenido;
        }

        public static string SanitizeHtml(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // Eliminar todos los scripts
            var nodes = doc.DocumentNode.SelectNodes("//script");
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    node.Remove();
                }
            }

            // Eliminar atributos on* (onclick, onload, etc.)
            nodes = doc.DocumentNode.SelectNodes("//*[@*[starts-with(name(), 'on')]]");
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    foreach (var attribute in node.Attributes.ToList())
                    {
                        if (attribute.Name.StartsWith("on"))
                        {
                            node.Attributes.Remove(attribute);
                        }
                    }
                }
            }

            return doc.DocumentNode.OuterHtml;
        }

        public static bool EsHtml(string contenido)
        {
            return contenido.Contains("</html>") || contenido.Contains("</body>") || contenido.Contains("</div>");
        }

        public static bool HayCabeceraHtml(string ruta)
        {
            var contenido = File.ReadAllText(ruta);
            return contenido.Contains("</html>");
        }

        public static bool EsTextoPlano(string contenido)
        {
            return !contenido.Contains("<html") && !contenido.Contains("<body");
        }

        public static string TextoToHtml(string texto)
        {
            return $@"<!DOCTYPE html>
                    <html lang=""es"">
                    <head>
                        <meta charset=""UTF-8"">
                        <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                        <title>Contenido del Correo</title>
                    </head>
                    <body>
                        {SanitizeHtml("<pre>" + (texto.IsNullOrEmpty() ? "" : HttpUtility.HtmlEncode(texto)) + "</pre>")}
                    </body>
                    </html>";

        }

        public static string ToTexto(string htmlContent)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(htmlContent);

            // StringBuilder para construir el texto
            StringBuilder textBuilder = new StringBuilder();

            // Eliminar la etiqueta <div> envolvente y extraer el texto
            var divNode = doc.DocumentNode.SelectSingleNode("//div");
            if (divNode != null)
            {
                foreach (var child in divNode.ChildNodes)
                {
                    textBuilder.Append(ExtractText(child));
                }
                divNode.Remove();
            }
            else
            {
                textBuilder.Append(ExtractText(doc.DocumentNode));
            }

            // Extraer el texto
            string text = textBuilder.ToString();

            // Decodificar entidades HTML
            text = WebUtility.HtmlDecode(text);

            // Limpiar espacios en blanco extra y saltos de línea múltiples
            //text = Regex.Replace(text, @"\s+", " ");
            //text = Regex.Replace(text, $"{Environment.NewLine}{Environment.NewLine}+", $"{Environment.NewLine}{Environment.NewLine}");
            text = text.Trim();

            return text;
        }

        private static string ExtractText(HtmlNode node)
        {
            StringBuilder text = new StringBuilder();

            if (node is HtmlTextNode)
            {
                text.Append(((HtmlTextNode)node).Text);
            }
            else if (node.HasChildNodes)
            {
                foreach (var child in node.ChildNodes)
                {
                    text.Append(ExtractText(child));
                }
            }
            else if (node.Name == "br")
            {
                text.Append(Environment.NewLine);
            }
            else if (node.Name == "p")
            {
                text.Append(Environment.NewLine + Environment.NewLine);
            }
            else if (node.Name == "li")
            {
                text.Append(Environment.NewLine + node.InnerText + Environment.NewLine);
            }

            return text.ToString();
        }

        public static string ReemplazarImagenesEnHtml(string htmlContent, List<string> rutas)
        {
            try
            {
                if (string.IsNullOrEmpty(htmlContent) || !rutas.Any())
                {
                    return htmlContent; // Si no hay contenido o adjuntos, devolvemos el HTML original
                }

                // Usamos HtmlAgilityPack para analizar el contenido HTML
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(htmlContent);

                // Seleccionamos todas las etiquetas <img>
                var imagenes = htmlDoc.DocumentNode.SelectNodes("//img");

                if (imagenes == null)
                {
                    return htmlContent; // Si no hay imágenes, devolvemos el HTML original
                }

                foreach (var img in imagenes)
                {
                    var src = img.GetAttributeValue("src", null); // Obtenemos el atributo src

                    if (!string.IsNullOrEmpty(src) && src.StartsWith("cid:"))
                    {
                        var cid = src.Substring(4); // Extraemos el CID eliminando "cid:"

                        // Buscamos el adjunto correspondiente
                        var ruta = rutas.FirstOrDefault(x => x.Contains(cid));

                        if (ruta != null)
                        {

                            if (File.Exists(ruta))
                            {
                                // Leemos el contenido del archivo y lo codificamos en base64
                                var bytes = File.ReadAllBytes(ruta);
                                var base64 = Convert.ToBase64String(bytes);

                                // Determinamos el tipo MIME del archivo (asumiendo que son imágenes)
                                if (!ExtensorDeTipoDeArchivos.EsImagen(Path.GetExtension(ruta), errorSiNoEstaCatalogada: false))
                                    continue;

                                var mimeType = MimeTypeMap.GetMimeTypeFromFilename(ruta);

                                // Construimos el esquema data: para incrustar la imagen
                                var dataUri = $"data:{mimeType};base64,{base64}";

                                // Reemplazamos el atributo src con la URI de datos
                                img.SetAttributeValue("src", dataUri);
                            }
                        }
                    }

                    // Devolvemos el HTML modificado como una cadena
                }
                return htmlDoc.DocumentNode.OuterHtml;
            }
            catch
            {
                return htmlContent;
            }

        }
    }

    public static class extRtf
    {
        public static string ToHtml(string rutaAlFichero)
        {
            string rtfContent = File.ReadAllText(rutaAlFichero);
            return Rtf.ToHtml(rtfContent);
        }

        public static string ToTexto(string rutaAlFichero)
        {
            try
            {
                // Convertir RTF a HTML
                string html = extRtf.ToHtml(rutaAlFichero);

                // Eliminar las etiquetas HTML para obtener texto plano
                return extHtml.ToTexto(html);
            }
            catch (Exception ex)
            {
                return "Error al leer el archivo RTF: " + ex.Message;
            }
        }
    }

    public static class extImagenes
    {
        public static bool EsImagen(string rutaConFichero)
        {
            string ext = Path.GetExtension(rutaConFichero).ToLower();
            return ExtensorDeTipoDeArchivos.EsImagen(ext, errorSiNoEstaCatalogada: false);
        }

        public static string SanitizeImage(string rutaConFichero)
        {
            if (!File.Exists(rutaConFichero)) 
                throw new Exception($"El fichero '{rutaConFichero}'no existe");

            string extension = Path.GetExtension(rutaConFichero).ToLower();
            if (ExtensorDeTipoDeArchivos.EsSvg(extension, errorSiNoEstaCatalogada : true)) return SanitizeSvg(rutaConFichero);
            try
            {
                string directorio = Path.GetDirectoryName(rutaConFichero);
                string nombreSinExt = Path.GetFileNameWithoutExtension(rutaConFichero);
                string rutaSaneada = Path.Combine(directorio, $"{nombreSinExt}_saneado{extension}");

                // Cargamos la imagen (ImageSharp detecta el formato automáticamente)
                using (Image image = Image.Load(rutaConFichero))
                {
                    // Al mutar la imagen o simplemente guardarla con una configuración limpia,
                    // eliminamos los perfiles EXIF, IPTC y XMP por defecto.
                    image.Metadata.ExifProfile = null;
                    image.Metadata.IptcProfile = null;
                    image.Metadata.XmpProfile = null;

                    // Guardamos la imagen. ImageSharp usará el codificador correcto según la extensión.
                    image.Save(rutaSaneada);
                }

                return rutaSaneada;
            }
            catch (Exception e)
            {
               throw new Exception($"Error al sanitizar la imagen '{rutaConFichero}'", e);
            }
        }


        private static string SanitizeSvg(string rutaConFichero)
        {
            try
            {
                string contenido = File.ReadAllText(rutaConFichero);
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(contenido);

                bool modificado = false;

                // 1. Eliminar etiquetas peligrosas (Búsqueda simple sin prefijos)
                var etiquetasPeligrosas = doc.DocumentNode.SelectNodes("//script | //object | //iframe | //foreignObject");
                if (etiquetasPeligrosas != null)
                {
                    foreach (var node in etiquetasPeligrosas) node.Remove();
                    modificado = true;
                }

                // 2. Recorrer TODOS los nodos para limpiar atributos de eventos y enlaces
                // Esta forma NO usa XPath, por lo que NO falla con los Namespaces
                var todosLosNodos = doc.DocumentNode.Descendants();
                foreach (var node in todosLosNodos)
                {
                    // Limpiar eventos "on..." (onclick, onload, etc.)
                    var attrsAEliminar = node.Attributes
                        .Where(a => a.Name.StartsWith("on", StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    if (attrsAEliminar.Any())
                    {
                        foreach (var attr in attrsAEliminar) node.Attributes.Remove(attr);
                        modificado = true;
                    }

                    // Limpiar enlaces "href" o "xlink:href" que contengan javascript
                    // Buscamos cualquier atributo que termine en "href"
                    var hrefAttr = node.Attributes.FirstOrDefault(a => a.Name.EndsWith("href", StringComparison.OrdinalIgnoreCase));
                    if (hrefAttr != null && hrefAttr.Value.Trim().ToLower().StartsWith("javascript:"))
                    {
                        hrefAttr.Value = "#";
                        modificado = true;
                    }
                }

                // Si no se detectó nada raro, devolvemos la ruta original
                if (!modificado) return rutaConFichero;

                // 3. Guardar versión saneada
                string directorio = Path.GetDirectoryName(rutaConFichero);
                string nombre = Path.GetFileNameWithoutExtension(rutaConFichero);
                string extension = Path.GetExtension(rutaConFichero);
                string rutaSaneada = Path.Combine(directorio, $"{nombre}_saneado{extension}");

                File.WriteAllText(rutaSaneada, doc.DocumentNode.OuterHtml);
                return rutaSaneada;
            }
            catch (Exception e)
            {
                // Esto atrapará cualquier error y te dará detalle en el log
                throw new Exception($"Error de seguridad al procesar SVG '{rutaConFichero}'", e);
            }
        }

    }

    public static class extDocx
    {
        public static string SanitizeDocx(string rutaConFichero)
        {
            if (!File.Exists(rutaConFichero))
                throw new Exception($"El fichero '{rutaConFichero}'no existe");
            try
            {
                string directorio = Path.GetDirectoryName(rutaConFichero);
                string nombreSinExt = Path.GetFileNameWithoutExtension(rutaConFichero);
                string extension = Path.GetExtension(rutaConFichero);
                string rutaSaneada = Path.Combine(directorio, $"{nombreSinExt}_saneado{extension}");

                File.Copy(rutaConFichero, rutaSaneada, true);

                bool seHaModificado = false;

                // El 'using' se encarga de Guardar y Cerrar (Save & Close) automáticamente
                using (WordprocessingDocument doc = WordprocessingDocument.Open(rutaSaneada, true))
                {
                    // 1. ELIMINAR MACROS (VBA)
                    if (doc.MainDocumentPart.VbaProjectPart != null)
                    {
                        doc.MainDocumentPart.DeletePart(doc.MainDocumentPart.VbaProjectPart);
                        seHaModificado = true;
                    }

                    // 2. ELIMINAR OBJETOS OLE
                    var oleParts = doc.MainDocumentPart.EmbeddedObjectParts.ToList();
                    foreach (var ole in oleParts)
                    {
                        doc.MainDocumentPart.DeletePart(ole);
                        seHaModificado = true;
                    }
                    // limpiar las Propiedades del Documento
                    doc.PackageProperties.Creator = "Sistema de elementos";
                    doc.PackageProperties.Description = "Archivo Sanitizado";
                    doc.PackageProperties.LastModifiedBy = "SanitizerBot";
                    // Si hay cambios, se guardan al salir del bloque using
                }

                // Si no hubo cambios, borramos la copia y devolvemos la original
                if (!seHaModificado)
                {
                    if (File.Exists(rutaSaneada)) File.Delete(rutaSaneada);
                    return rutaConFichero;
                }

                return rutaSaneada;
            }
            catch (Exception e)
            {
                throw new Exception($"Error al sanitizar el documento '{rutaConFichero}'", e);
            }
        }

        public static bool EsDoc(string rutaConFichero)
        {
            if (!File.Exists(rutaConFichero))
                throw new Exception($"El fichero '{rutaConFichero}'no existe");

            try
            {
                byte[] buffer = new byte[8];
                using (FileStream fs = new FileStream(rutaConFichero, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    if (fs.Length < 8) return false;
                    fs.ReadExactly(buffer, 0, 8);
                }

                // Firma OLE2 (Legacy Doc): D0 CF 11 E0 A1 B1 1A E1
                return buffer[0] == 0xD0 && buffer[1] == 0xCF && buffer[2] == 0x11 && buffer[3] == 0xE0 &&
                       buffer[4] == 0xA1 && buffer[5] == 0xB1 && buffer[6] == 0x1A && buffer[7] == 0xE1;
            }
            catch
            {
                return false;
            }
        }

        public static bool EsDocx(string rutaConFichero)
        {
            if (!File.Exists(rutaConFichero))
                throw new Exception($"El fichero '{rutaConFichero}'no existe");

            try
            {
                byte[] buffer = new byte[4];
                using (FileStream fs = new FileStream(rutaConFichero, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    if (fs.Length < 4) return false;
                    fs.ReadExactly(buffer, 0, 4);
                }

                // Firma ZIP (Office Open XML): 50 4B 03 04 (PK..)
                // Nota: Esto confirma que es un ZIP. Para asegurar que es DOCX, 
                // validamos también la extensión.
                bool esZip = buffer[0] == 0x50 && buffer[1] == 0x4B && buffer[2] == 0x03 && buffer[3] == 0x04;
                string ext = Path.GetExtension(rutaConFichero).ToLower();

                return esZip && (ext == ".docx" || ext == ".docm" || ext == ".dotx");
            }
            catch
            {
                return false;
            }
        }

        public static string ToHtml(string docxPath)
        {
            using (WordprocessingDocument doc = WordprocessingDocument.Open(docxPath, false))
            {
                StringBuilder html = new StringBuilder("<html><body>");

                var body = doc.MainDocumentPart.Document.Body;

                foreach (var element in body.ChildElements)
                {
                    if (element is Paragraph para)
                    {
                        html.Append("<p>");
                        ProcessParagraph(para, html, doc);
                        html.Append("</p>");
                    }
                    else if (element is Table table)
                    {
                        ProcessTable(table, html, doc);
                    }
                }

                html.Append("</body></html>");
                return html.ToString();
            }
        }

        private static void ProcessParagraph(Paragraph para, StringBuilder html, WordprocessingDocument doc, bool isInTableCell = false)
        {
            foreach (var run in para.Elements<Run>())
            {
                foreach (var childElement in run.ChildElements)
                {
                    if (childElement is Text textElement)
                    {
                        string text = XmlEscape(textElement.Text);
                        ApplyFormatting(run, ref text);
                        html.Append(text);
                    }
                    else if (childElement is Drawing drawing)
                    {
                        ProcessImage(drawing, html, doc, isInTableCell);
                    }
                }
            }
        }


        private static void ProcessTable(Table table, StringBuilder html, WordprocessingDocument doc)
        {
            html.Append("<table border='1'>");
            foreach (var row in table.Elements<TableRow>())
            {
                html.Append("<tr>");
                foreach (var cell in row.Elements<TableCell>())
                {
                    html.Append("<td>");
                    foreach (var para in cell.Elements<Paragraph>())
                    {
                        html.Append("<p>");
                        ProcessParagraph(para, html, doc, true);
                        html.Append("</p>");
                    }
                    html.Append("</td>");
                }
                html.Append("</tr>");
            }
            html.Append("</table>");
        }

        private static void ApplyFormatting(Run run, ref string text)
        {
            var runProperties = run.RunProperties;
            if (runProperties != null)
            {
                if (runProperties.Bold != null)
                    text = $"<strong>{text}</strong>";
                if (runProperties.Italic != null)
                    text = $"<em>{text}</em>";
                if (runProperties.Underline != null && runProperties.Underline.Val != UnderlineValues.None)
                    text = $"<u>{text}</u>";
            }
        }

        private static void ProcessImage(Drawing drawing, StringBuilder html, WordprocessingDocument doc, bool isInTableCell = false)
        {
            var imageId = drawing.Descendants<DocumentFormat.OpenXml.Drawing.Blip>().FirstOrDefault()?.Embed?.Value;
            if (imageId != null)
            {
                var imagePart = (ImagePart)doc.MainDocumentPart.GetPartById(imageId);
                var imageBytes = GetImageBytes(imagePart);
                var base64Image = Convert.ToBase64String(imageBytes);
                var mimeType = imagePart.ContentType;

                // Obtener el tamaño de la imagen
                var extent = drawing.Descendants<DocumentFormat.OpenXml.Drawing.Wordprocessing.Extent>().FirstOrDefault();
                long width = extent?.Cx ?? 0;
                long height = extent?.Cy ?? 0;

                // Convertir de EMUs a píxeles (1 cm = 360000 EMUs, 1 inch = 914400 EMUs)
                int widthInPixels = (int)(width / 9525);
                int heightInPixels = (int)(height / 9525);

                if (isInTableCell)
                {
                    // Limitar el tamaño máximo a 64x64 si está en una celda de tabla
                    widthInPixels = Math.Min(widthInPixels, 64);
                    heightInPixels = Math.Min(heightInPixels, 64);

                    html.Append("<div style=\"display: flex; justify-content: center; align-items: center; height: 100%;\">");
                    html.Append($"<img src=\"data:{mimeType};base64,{base64Image}\" style=\"width: {widthInPixels}px; height: {heightInPixels}px; object-fit: contain;\" />");
                    html.Append("</div>");
                }
                else
                {
                    html.Append($"<img src=\"data:{mimeType};base64,{base64Image}\" width=\"{widthInPixels}\" height=\"{heightInPixels}\" />");
                }
            }
        }



        private static byte[] GetImageBytes(ImagePart imagePart)
        {
            using (var stream = imagePart.GetStream())
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }

        private static string XmlEscape(string text)
        {
            return System.Security.SecurityElement.Escape(text);
        }

        public static string ToTexto(string rutaAlFichero)
        {
            StringBuilder textoPlano = new StringBuilder();

            using (WordprocessingDocument doc = WordprocessingDocument.Open(rutaAlFichero, false))
            {
                var body = doc.MainDocumentPart.Document.Body;

                foreach (var para in body.Elements<Paragraph>())
                {
                    foreach (var run in para.Elements<Run>())
                    {
                        foreach (var text in run.Elements<Text>())
                        {
                            textoPlano.Append(text.Text);
                        }
                    }
                    textoPlano.AppendLine();
                }
            }

            return textoPlano.ToString();
        }

        public static string ToPdf(string ficheroDocx, string claveConverApi)
        {
            Exception excepcionEnTaskRun = null;
            Task.Run(async () =>
            {
                try
                {
                    var covertapi = new ConvertApi(claveConverApi); // "1CPToQIEOjozKPfq"; 
                    var resutadoConversion = await covertapi.ConvertAsync(enumExtensiones.docx.ToString(), enumExtensiones.pdf.ToString(), new ConvertApiFileParam("File", ficheroDocx));
                    if (resutadoConversion.Files.Count() == 1)
                    {
                        foreach (var file in resutadoConversion.Files)
                        {
                            var directorioDeSalida = System.IO.Path.GetDirectoryName(ficheroDocx);
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

            return System.IO.Path.Combine(System.IO.Path.GetDirectoryName(ficheroDocx), $"{System.IO.Path.GetFileNameWithoutExtension(ficheroDocx)}.{enumExtensiones.pdf}");
        }

    }

    public static class extCsv
    {
        public static string ToHtml(string filePath)
        {
            StringBuilder html = new StringBuilder();
            html.Append("<table style='border-collapse: collapse; width: 100%; margin-bottom: 20px;'>");

            string[] lines = File.ReadAllLines(filePath);

            foreach (string line in lines)
            {
                html.Append("<tr style='white-space: nowrap;'>");
                string[] cells = line.Split(Simbolos.separadorCss);

                foreach (string cell in cells)
                {
                    html.Append($"<td style='border: 1px solid #ddd; padding: 8px; overflow: hidden; text-overflow: ellipsis;'>{cell}</td>");
                }

                html.Append("</tr>");
            }

            html.Append("</table>");
            return html.ToString();
        }

    }

    internal class ColumnaFecha
    {
        internal DocumentFormat.OpenXml.Spreadsheet.Worksheet Hoja { get; set; }
        internal int Indice { get; set; }
        internal bool EsFecha { get; set; }
    }

    public static class extXlsx
    {

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
    public static class extZip
    {
        public static string ToHtml(string zipFilePath)
        {
            StringBuilder html = new StringBuilder();
            html.Append("<table style='border-collapse: collapse; width: 100%;'>");
            //html.Append("<tr style='background-color: #f2f2f2;'><th style='border: 1px solid #ddd; padding: 8px; text-align: left;'>Nombre</th><th style='border: 1px solid #ddd; padding: 8px; text-align: left;'>Tipo</th></tr>");
            html.Append("<tr style='background-color: #f2f2f2;'><th style='border: 1px solid #ddd; padding: 8px; text-align: left;'>Nombre</th></tr>");

            using (ZipArchive archive = ZipFile.OpenRead(zipFilePath))
            {
                var entries = archive.Entries.OrderBy(e => e.FullName).ToList();
                string currentDir = "";

                foreach (var entry in entries)
                {
                    string directory = Path.GetDirectoryName(entry.FullName);
                    if (directory != currentDir)
                    {
                        if (!string.IsNullOrEmpty(currentDir))
                        {
                            html.Append("</table></td></tr>");
                        }
                        currentDir = directory;
                        html.Append($"<tr><td colspan='2' style='border: 1px solid #ddd; padding: 8px;'><strong>{directory}</strong></td></tr>");
                        html.Append("<tr><td colspan='2' style='border: 1px solid #ddd; padding: 8px;'><table style='width: 100%;'>");
                    }

                    string name = Path.GetFileName(entry.FullName);
                    string type = entry.Length == 0 ? "Directorio" : "Archivo";
                    if (type == "Directorio")
                        continue;
                    //html.Append($"<tr><td style='border: 1px solid #ddd; padding: 8px;'>{name}</td><td style='border: 1px solid #ddd; padding: 8px;'>{type}</td></tr>");
                    html.Append($"<tr><td style='border: 1px solid #ddd; padding: 8px;'>{name}</td></tr>");
                }

                if (!string.IsNullOrEmpty(currentDir))
                {
                    html.Append("</table></td></tr>");
                }
            }

            html.Append("</table>");
            return html.ToString();
        }
    }


    public static class ext7z
    {
        public static string ToHtml(string sevenZipFilePath)
        {
            StringBuilder html = new StringBuilder();
            html.Append("<table style='border-collapse: collapse; width: 100%;'>");
            html.Append("<tr style='background-color: #f2f2f2;'><th style='border: 1px solid #ddd; padding: 8px; text-align: left;'>Nombre</th></tr>");

            using (ArchiveFile archiveFile = new ArchiveFile(sevenZipFilePath))
            {
                var entries = archiveFile.Entries.OrderBy(e => e.FileName).ToList();
                string currentDir = "";

                foreach (var entry in entries)
                {
                    string directory = Path.GetDirectoryName(entry.FileName);
                    if (directory != currentDir)
                    {
                        if (!string.IsNullOrEmpty(currentDir))
                        {
                            html.Append("</table></td></tr>");
                        }
                        currentDir = directory;
                        html.Append($"<tr><td style='border: 1px solid #ddd; padding: 8px;'><strong>{directory}</strong></td></tr>");
                        html.Append("<tr><td style='border: 1px solid #ddd; padding: 8px;'><table style='width: 100%;'>");
                    }

                    string name = Path.GetFileName(entry.FileName);
                    if (entry.IsFolder)
                        continue;

                    html.Append($"<tr><td style='border: 1px solid #ddd; padding: 8px;'>{name}</td></tr>");
                }

                if (!string.IsNullOrEmpty(currentDir))
                {
                    html.Append("</table></td></tr>");
                }
            }

            html.Append("</table>");
            return html.ToString();
        }
    }

    public static class extTipos
    {
        public static bool EsNulable(this Type type)
        {
            return Nullable.GetUnderlyingType(type) != null;
        }
    }

    public class ValorAttribute : Attribute
    {
        public string Value { get; }

        public ValorAttribute(string value)
        {
            Value = value;
        }
    }

    public static class ExtensorDeEnum
    {
        public static string Descripcion(this Enum valorEnumerado, bool enMinusculas = false)
        {
            var type = valorEnumerado.GetType();
            if (!type.IsEnum)
            {
                throw new Exception($"{nameof(valorEnumerado)} debe ser un valor de enumerado");
            }
            var memberInfo = type.GetMember(valorEnumerado.ToString());
            if (memberInfo.Length > 0)
            {
                var attrs = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (attrs.Length > 0)
                {
                    return ((DescriptionAttribute)attrs[0]).Description;
                }
            }
            return enMinusculas ? valorEnumerado.ToString().ToLower() : valorEnumerado.ToString();
        }

        public static string Valor(this Enum valorEnumerado)
        {
            var type = valorEnumerado.GetType();
            var memberInfo = type.GetMember(valorEnumerado.ToString());
            if (memberInfo.Length > 0)
            {
                var attrs = memberInfo[0].GetCustomAttributes(typeof(ValorAttribute), false);
                if (attrs.Length > 0)
                {
                    return ((ValorAttribute)attrs[0]).Value;
                }
            }
            return valorEnumerado.ToString();
        }

        public static string ObtenerLaDescripcioDeUnEnumerado(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute),
                false);

            if (attributes != null &&
                attributes.Length > 0)
                return attributes[0].Description;
            else
                return value.ToString();
        }

        public static bool Existe<T>(string descripcion)
        where T : Enum
        =>
        Enum.GetValues(typeof(T)).Cast<T>().Any(valor => valor.Descripcion() == descripcion);


        public static T Enumerado<T>(string descripcion)
        where T : Enum
        =>
        Enum.GetValues(typeof(T)).Cast<T>().First(valor => valor.Descripcion() == descripcion);

    }


}
