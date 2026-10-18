using DocumentFormat.OpenXml;
using System;
using System.Globalization;
using System.Linq;


namespace Utilidades
{
    public static class ExtensorDeNumeros
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

        public static string Moneda(this decimal? numero, decimal? valorPorDefecto = null, string formato = "C", int longitud = 12, int decimales = 2, bool alineacion = true, CultureInfo cultura = null, bool mostrarUltimoDecimal =false)
        {
            if (cultura is null) cultura = new CultureInfo("es-ES");

            if (valorPorDefecto is null && numero is null)
                return "";

            decimal valor = (numero is null) ? ((decimal)valorPorDefecto) : ((decimal)numero);
            return valor.Moneda(formato, longitud, decimales, alineacion, cultura, mostrarUltimoDecimal);
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
}
