using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;


namespace Utilidades
{
    public static class ExtensorDeCadenas
    {
        public enum enumCadenas { Si, No };

        public static bool IsNullOrEmpty(this string str, bool quitarBlancos = true)
        {
            if (str == null)
                return true;

            return string.IsNullOrEmpty(quitarBlancos ? str.Trim() : str);
        }

        private static readonly HashSet<string> _particulasEnMinusculas = new() { "a", "de", "del", "en", "y" };

        // Equivalente a Entorno.CapitalizarFrase: separa por espacios (colapsando los repetidos), deja
        // en minúsculas las partículas a/de/del/en/y y pone el resto con la inicial en mayúscula.
        public static string Capitalizar(this string frase)
        {
            if (frase.IsNullOrEmpty())
                return frase;

            var palabras = frase
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(palabra => _particulasEnMinusculas.Contains(palabra.ToLowerInvariant())
                    ? palabra.ToLowerInvariant()
                    : char.ToUpperInvariant(palabra[0]) + palabra.Substring(1).ToLowerInvariant());

            return string.Join(" ", palabras);
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

        public enum enumAlineacion { Izquierda, Centrado, Derecha }

        private static string AlinearIzquierda(string s, int ancho) =>
            s.Length >= ancho ? s : s.PadRight(ancho);

        private static string AlinearDerecha(string s, int ancho) =>
            s.Length >= ancho ? s : s.PadLeft(ancho);

        private static string Centrar(string s, int ancho)
        {
            if (s.Length >= ancho) return s;
            var pad = ancho - s.Length;
            var left = pad / 2;
            var right = pad - left;
            return new string(' ', left) + s + new string(' ', right);
        }

        public static string Alinear(string s, int ancho, enumAlineacion? alineacion = null)
        {
            if (alineacion != null)
                return alineacion switch
                {
                    enumAlineacion.Izquierda => AlinearIzquierda(s, ancho),
                    enumAlineacion.Derecha   => AlinearDerecha(s, ancho),
                    _                        => Centrar(s, ancho)
                };

            // Detección automática: número o fecha → derecha; texto → izquierda
            var trimmed = s.Trim();
            if (decimal.TryParse(trimmed, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out _) ||
                decimal.TryParse(trimmed, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.CurrentCulture, out _) ||
                DateTime.TryParse(trimmed, out _))
                return AlinearDerecha(s, ancho);

            return AlinearIzquierda(s, ancho);
        }

    }
}
