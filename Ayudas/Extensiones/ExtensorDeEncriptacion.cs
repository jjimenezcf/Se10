using System;
using System.Security.Cryptography;
using System.Text;


namespace Utilidades
{
    public static class ExtensorDeEncriptacion
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
}
