using System.Text.RegularExpressions;

namespace Utilidades
{
    public static class ValidadorEmail
    {
        // Expresión regular estándar y eficiente para validar emails (RFC 5322 simplificada)
        private static readonly Regex EmailRegex = new Regex(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// Valida si el formato de un correo electrónico es correcto.
        /// </summary>
        /// <param name="correo">Cadena con el correo a evaluar.</param>
        /// <returns>True si el formato es válido; de lo contrario, False.</returns>
        public static void ValidarMail(string correo)
        {
            if (string.IsNullOrWhiteSpace(correo))
                throw new System.Exception($"Debe indicar el correo a validar");

            // Eliminamos posibles espacios en blanco al inicio o final
            string correoLimpio = correo.Trim();

            var esValido = EmailRegex.IsMatch(correoLimpio);
             if (!esValido)
                throw new System.Exception($"El correo '{correo}' no tiene un formato válido");

        }
    }
}
