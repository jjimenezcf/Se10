using System.Text.RegularExpressions;


namespace Utilidades
{
    public static class ExtensorDeTelefonos
    {
        // Pasa un teléfono al formato internacional sólo con dígitos que espera https://wa.me/ (prefijo del país sin '+' ni '00'):
        // se quitan espacios, guiones, puntos y paréntesis; si no lleva prefijo y tiene 9 dígitos se considera español y se le antepone el 34.
        // Devuelve null si no es un teléfono (letras, varios números, longitud fuera de 8..15 dígitos, ...)
        /*
         600 12 34 56          34600123456
         +34 600-12-34-56      34600123456
         0034 (600) 123456     34600123456
         +351 912 345 678      351912345678
         600 12 34             null
         600123456 / 912345678 null
         600-ABC-123           null
         */
        public const string PrefijoPorDefecto = "34";

        private static readonly Regex _separadores = new Regex(@"[\s\-.()]", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static readonly Regex _soloDigitos = new Regex(@"^\d{8,15}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public static string MensajeDeTelefonoNoValido(string telefono) => $"El teléfono '{telefono}' no es válido, indíquelo con el prefijo del país (+34 600 123 456)";

        public static string ANumeroInternacional(string telefono)
        {
            if (telefono.IsNullOrEmpty())
                return null;

            var numero = _separadores.Replace(telefono, "");
            if (numero.StartsWith("+")) numero = numero.Substring(1);
            else if (numero.StartsWith("00")) numero = numero.Substring(2);
            else if (numero.Length == 9) numero = $"{PrefijoPorDefecto}{numero}";
            else return null;

            return _soloDigitos.IsMatch(numero) ? numero : null;
        }
    }
}
