using System;
using System.Text.RegularExpressions;


namespace Utilidades
{
    public static class ExtensorDeCorreos
    {
        // Sólo descarta lo que claramente no es un correo: sin espacios, una única @ con algo delante y un dominio con al menos un punto,
        // sin puntos seguidos ni en los extremos, y una terminación de 2 o más caracteres (admite +, apóstrofos, ñ, ...)
        /*
         juan@empresa.es                        True
         juan.perez+pedidos@sub.empresa.com     True
         o'neil@dominio.ie                      True
         compras@peñalver.es                    True
         juan@empresa                           False
         juan@@empresa.es                       False
         juan empresa@x.es                      False
         @empresa.es                            False
         juan@empresa..es                       False
         juan@.empresa.es                       False
         juan@empresa.e                         False
         juan                                   False
         */

        private static readonly Regex _patronDeCorreo = new Regex(@"^[^@\s]+@([^@\s.]+\.)+[^@\s.]{2,}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public static string MensajeDeCorreoNoValido(string eMail) => $"El correo '{eMail}' no es válido";

        public static bool EsUnCorreo(string eMail) => !eMail.IsNullOrEmpty() && _patronDeCorreo.IsMatch(eMail.Trim());

        public static void ValidarMail(string eMail)
        {
            if (!EsUnCorreo(eMail))
                throw new Exception(MensajeDeCorreoNoValido(eMail));
        }
    }
}
