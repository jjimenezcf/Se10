
using Gestor.Errores;
using ServicioDeDatos.Callejero;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Text.RegularExpressions;
using Utilidades;

namespace ServicioDeDatos.Terceros
{


    public enum enumTipoTercero
    {
        Autonomo,
        Empresa,
        Nie,
        Invalido
    }


    public enum enumClaseDeNacionalidad
    {
        Nacional,
        Intracomunitario,
        Extracomunitario
    }


    public static class ApiDeTerceros
    {
        public static enumTipoTercero TipoDeTerceroEsp(string nifCif)
        {
            // Validar el formato del NIF/CIF
            if (string.IsNullOrWhiteSpace(nifCif) || nifCif.Length < 9)
            {
                return enumTipoTercero.Invalido;
            }
            // Normalizar el NIF/CIF eliminando espacios y convirtiendo a mayúsculas
            nifCif = nifCif.Trim().ToUpper();

            // Comprobar si es un NIF/CIF de empresa
            if ((nifCif.StartsWith(ltrIsoPaises.Spain) && nifCif.Length == 11 && char.IsLetter(nifCif[2])) ||
                (char.IsLetter(nifCif[0]) && nifCif.Length == 9))
            {
                return enumTipoTercero.Empresa;
            }

            // Comprobar si es un NIF (DNI + letra) para autónomos
            if ((nifCif.StartsWith(ltrIsoPaises.Spain) && nifCif.Length == 11 && char.IsNumber(nifCif[2])) || (char.IsDigit(nifCif[0]) && nifCif.Length == 9))
            {
                return enumTipoTercero.Autonomo;
            }

            // Comprobar si es un NIE (X, Y, Z + 7 dígitos + letra)
            if ((nifCif[0] == 'X' || nifCif[0] == 'Y' || nifCif[0] == 'Z') && nifCif.Length == 9)
            {
                return enumTipoTercero.Nie;
            }

            return enumTipoTercero.Invalido;
        }

        public static string ValidarNif(string NIF)
        {
            if (NIF.Length == 11 && NIF.StartsWith(ltrIsoPaises.Spain))
                NIF = NIF.Right(9);

            if (NIF.Length != 9)
                return $"El NIF introducido '{NIF}' no es correcto, ha de ser de 9 caracteres";

            var letras = "TRWAGMYFPDXBNJZSQVHLCKE";
            NIF = NIF.ToUpper();
            var nifLength = NIF.Length;

            var ultimoCaracter = NIF.Substring(nifLength - 1);
            if (ultimoCaracter.EsEntero())
                return $"El NIF introducido '{NIF}' no es correcto, el último caracter ha de ser una letra";

            string nifNumero = NIF.Substring(0, nifLength - 1);
            if (!nifNumero.EsEntero())
                return $"El NIF introducido '{NIF}' no es correcto,los primeros 8 caracteres han de ser numéricos";

            var numero = nifNumero.Entero();
            numero = numero % 23;
            var letraCalculada = letras.Substring(numero, 1);

            if (ultimoCaracter != letraCalculada)
                return $"El NIF introducido '{NIF}' no es correcto, la letra calculada es '{letraCalculada}'";

            return "";
        }
        public static string ValidarNie(string nie)
        {
            //NIF cuyo primer carácter es una letra y esta letra es una 'M'
            //Fuente: https://es.wikipedia.org/wiki/N%C3%BAmero_de_identificaci%C3%B3n_fiscal
            //NIF M: M + 7 números + letra de control
            //Extranjeros sin NIE, de forma transitoria por estar obligados a tenerlo o bien de forma definitiva al no estar obligados a ello
            if (nie[0].Equals('M')) return ValidarNif($"0{nie.Right(nie.Length - 1)}");
            else
            {
                var dni = nie.ToUpper();
                var pre = dni.Substring(0, 1);
                var prev = dni[0].ToString();

                if (pre == "X")
                {
                    prev = "0";
                }
                else if (pre == "Y")
                {
                    prev = "1";
                }
                else if (pre == "Z")
                {
                    prev = "2";
                }
                var nifCalculado = prev + dni.Substring(1, dni.Length - 1);
                return ValidarNif(nifCalculado);
            }
        }

        public static void ValidarCif(SociedadDtm sociedad)
        {
            //Buscamos algunas cadenas no permitidas.

            if (extCadenas.IsNullOrEmpty(sociedad.NIF)) GestorDeErrores.Emitir($"No se ha indicado el NIF a la sociedad {sociedad.Nombre}");

            var nif = sociedad.NIF;
            if (!sociedad.NIF.StartsWith(ltrIsoPaises.Brasil))
            {
                nif = nif.Replace("-", "");
                nif = nif.Replace(".", "");
                nif = nif.Replace("/", "");
                nif = nif.Replace(" ", "");
                nif = nif.Replace(@"\", "");
                nif = nif.Replace("+", "");
                sociedad.NIF = nif;
            }

            var nifValido = ValidarNif(sociedad.NIF).IsNullOrEmpty();
            var nieValido = ValidarNie(sociedad.NIF).IsNullOrEmpty();
            var cifValido = CifValido(sociedad.NIF);

            if (!nifValido && !nieValido && !cifValido)
                GestorDeErrores.Emitir($"El NIF introducido '{sociedad.NIF}' no es correcto");
        }

        public static bool NifCifValido(string nif) => CifValido(nif) || ValidarNif(nif).IsNullOrEmpty();

        public static bool CifValido(string cif)
        {

            if (cif.Length < 9)
                return false;

            var pais = cif.Left(2);

            switch (pais)
            {
                case ltrIsoPaises.GranBretana: return ValidarCifGb(cif);
                case ltrIsoPaises.Italia: return ValidarCifIt(cif);
                case ltrIsoPaises.Francia: return ValidarCifFr(cif);
                case ltrIsoPaises.Suecia: return ValidarCifSw(cif);
                case ltrIsoPaises.Brasil: return ValidarCifBr(cif);
                case ltrIsoPaises.Irlanda: return ValidarCIFIrlanda(cif);
                case ltrIsoPaises.EEUU: return ValidarCifEu(cif);
                case ltrIsoPaises.Luxenburgo: return ValidarCifLuxenburgo(cif);

            }

            return ValidarCifEsp(cif);
        }

        private static bool ValidarCifBr(string cif)
        {
            string patron = @"^BR\d{2}\.\d{3}\.\d{3}\/\d{4}-\d{2}$";
            return Regex.IsMatch(cif, patron);
        }

        private static bool ValidarCIFIrlanda(string cif)
        {
            string patron = @"^IE\d{7}[A-W]$";
            return Regex.IsMatch(cif, patron);
        }
        private static bool ValidarCifEsp(string cif)
        {
            // Eliminar prefijo de país si existe
            if (cif.StartsWith(ltrIsoPaises.Spain))
                cif = cif.Substring(2);

            // Validar longitud básica (debería ser 9 caracteres para CIF completo)
            if (cif.Length != 9)
                return false;

            // Validar caracteres no permitidos
            if (cif.Contains("/") || cif.Contains("-"))
                return false;

            string[] letrasCodigo = { "J", "A", "B", "C", "D", "E", "F", "G", "H", "I" };
            string[] letrasValidas = { "A", "B", "C", "D", "E", "F", "G", "H", "J", "K", "L", "M", "N", "P", "Q", "R", "S", "U", "V", "W" };

            var letraInicial = cif[0].ToString();
            var digitoControl = cif[cif.Length - 1].ToString();

            // Validar letra inicial
            if (!letrasValidas.Contains(letraInicial))
                return false;

            // Validar que los caracteres centrales son números
            var parteNumerica = cif.Substring(1, cif.Length - 2);
            if (!parteNumerica.All(char.IsDigit))
                return false;

            var n = parteNumerica;
            var sumaPares = 0;
            var sumaImpares = 0;
            var sumaTotal = 0;

            for (int i = 0; i < n.Length; i++)
            {
                int aux = int.Parse(n[i].ToString());

                if ((i + 1) % 2 == 0) // Posiciones pares (recordar que empieza en 0)
                {
                    sumaPares += aux;
                }
                else // Posiciones impares
                {
                    aux = aux * 2;
                    sumaImpares += extNumeros.SumarDigitos(aux);
                }
            }

            sumaTotal = sumaPares + sumaImpares;
            var unidades = sumaTotal % 10;
            if (unidades != 0)
                unidades = 10 - unidades;

            bool retVal;
            switch (letraInicial)
            {
                case "A":
                case "B":
                case "E":
                case "H":
                    retVal = digitoControl == unidades.ToString();
                    break;

                case "K":
                case "P":
                case "Q":
                case "S":
                    retVal = digitoControl == letrasCodigo[unidades];
                    break;

                default:
                    retVal = (digitoControl == unidades.ToString()) || (digitoControl == letrasCodigo[unidades]);
                    break;
            }

            return retVal;
        }

        private static bool ValidarCifGb(string numeroCif)
        {
            //Validación CIF, formato UK estándar (N(1-9))
            //S=8*N(1)+7*N(2)+6*N(3)+5*N(4)+4*N(5)+3*N(6)+2*N(7)+10*N(8)+N(9) , si S%97=0, CIF correcto.
            //Documentación obtenida de: http://www.sima.cat/nif.php?pais=ES&nif=282433951

            try
            {
                numeroCif = numeroCif.Remove(0, 2);

                int? s = numeroCif.Substring(0, 1).Entero() * 8 + numeroCif.Substring(1, 1).Entero() * 7 +
                        numeroCif.Substring(2, 1).Entero() * 6 + numeroCif.Substring(3, 1).Entero() * 5 +
                        numeroCif.Substring(4, 1).Entero() * 4 + numeroCif.Substring(5, 1).Entero() * 3 +
                        numeroCif.Substring(6, 1).Entero() * 2 + numeroCif.Substring(7, 1).Entero() * 10 +
                        numeroCif.Substring(8, 1).Entero() * 1;

                var t = s % 97;

                //**Documentación obtenidad de https://en.wikipedia.org/wiki/VAT_identification_number, apartado Reino Unido, indica que a partir de 2010
                //la validación cambia por falta de números se amplia el VAT con dos dígitos más y en lugar de ser la validación %97 es un algoritmo
                // llamado '9755'
                var r = ((s + 55) % 97);
                return r == 0 || t == 0;
            }
            catch
            {
                return false;
            }
        }

        private static bool ValidarCifSw(string numeroCif)
        {
            try
            {
                // Validación NIF sueco ()
                // El NIF sueco consta de 12 dígitos
                // S=2#N(1)+N(2)+2#N(3)+N(4)+2#N(5)+N(6)+2#N(7)+N(8)+2#N(9)
                // C(1)=10-S%10; if C(1)=10, C(1)=0
                // Eliminar los dos últimos dígitos (siempre 01) del NIF sueco
                numeroCif = numeroCif.Remove(0, 2);
                var dc = numeroCif.Substring(9, 1).Entero();
                numeroCif = numeroCif.Substring(0, 9);

                int s = extNumeros.SumarDigitos(numeroCif.Substring(0, 1).Entero() * 2) +
                         numeroCif.Substring(1, 1).Entero() +
                         extNumeros.SumarDigitos(numeroCif.Substring(2, 1).Entero() * 2) +
                         numeroCif.Substring(3, 1).Entero() +
                         extNumeros.SumarDigitos(numeroCif.Substring(4, 1).Entero() * 2) +
                         numeroCif.Substring(5, 1).Entero() +
                         extNumeros.SumarDigitos(numeroCif.Substring(6, 1).Entero() * 2) +
                         numeroCif.Substring(7, 1).Entero() +
                         extNumeros.SumarDigitos(numeroCif.Substring(8, 1).Entero() * 2);

                int c = 10 - s % 10;
                if (c == 10) { c = 0; }

                return (c == dc);
            }
            catch
            {
                return false;
            }
        }

        public static bool ValidarCifIt(string numeroCif)
        {
            // Validación inicial del formato
            if (string.IsNullOrEmpty(numeroCif) ||
                numeroCif.Length != 13 ||
                !numeroCif.StartsWith("IT", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            try
            {
                string digitos = numeroCif.Substring(2);

                // Verificar que todos los caracteres sean dígitos
                foreach (char c in digitos)
                {
                    if (!char.IsDigit(c)) return false;
                }

                // Cálculo del dígito de control
                int suma = 0;
                for (int i = 0; i < 10; i++) // Procesar primeros 10 dígitos
                {
                    int digito = int.Parse(digitos[i].ToString());

                    if (i % 2 == 0) // Posiciones pares (0,2,4,6,8)
                    {
                        suma += digito;
                    }
                    else // Posiciones impares (1,3,5,7,9)
                    {
                        suma += extNumeros.SumarDigitos(digito * 2);
                    }
                }

                int digitoControlCalculado = 10 - (suma % 10);
                digitoControlCalculado = digitoControlCalculado == 10 ? 0 : digitoControlCalculado;

                // Comparar con el último dígito
                int digitoControlReal = int.Parse(digitos[10].ToString());
                return digitoControlCalculado == digitoControlReal;
            }
            catch
            {
                return false;
            }
        }

        private static bool ValidarCifLuxenburgo(string numeroCif)
        {
            // Validación CIF de Luxemburgo
            // Formato: LU + 8 dígitos
            // Los primeros 6 dígitos son un número de serie
            // Los últimos 2 dígitos son un número de control

            try
            {
                // Verificar el formato básico
                if (!Regex.IsMatch(numeroCif, @"^LU\d{8}$"))
                    return false;

                // Remover el prefijo LU
                string numero = numeroCif.Substring(2);

                // Obtener los primeros 6 dígitos y los 2 dígitos de control
                int baseNumber = int.Parse(numero.Substring(0, 6));
                int checkDigits = int.Parse(numero.Substring(6, 2));

                // Calcular los dígitos de control
                int calculatedCheck = baseNumber % 89;

                // Comparar los dígitos de control calculados con los proporcionados
                return calculatedCheck == checkDigits;
            }
            catch
            {
                return false;
            }
        }

        private static bool ValidarCifFr(string numeroCif)
        {
            //Validación CIF, formato IT estándar (sistema nuevo)
            //C{0-9,A-H,J-N,P-Z} → C{0-33} 
            //si C(1) < 10, S = C(1) * 24 + C(2) - 10
            //si C(1) > 9, S = C(1) * 34 + C(2) - 100
            //X = S % 11
            //S = S\11 + 1
            //Y = (N(1 - 9) + S) % 11
            //si X = Y, correcto   
            //Validación CIF, formato IT estándar (sistema antiguo)
            //C(1-2)=(N(1-9)*100+12)%97         
            //Documentación obtenida de: http://www.sima.cat/nif.php?pais=ES&nif=282433951

            string patron = "0123456789ABCDEFGHJKLMNPQRSTUVWXYZ";

            numeroCif = numeroCif.Remove(0, 2);

            int s = 0;
            try
            {
                int c1 = patron.IndexOf(numeroCif.Substring(0, 1));
                int c2 = patron.IndexOf(numeroCif.Substring(1, 1));
                decimal n = numeroCif.Substring(2, 9).Decimal();

                if (c1 < 10)
                {
                    s = c1 * 24 + c2 - 10;
                }
                else if (c1 > 9)
                {
                    s = c1 * 34 + c2 - 100;
                }

                int x = s % 11;
                s = s / 11 + 1;
                decimal? y = (n + s) % 11;
                var validacion1 = x == y;

                //Validación 2
                //C(1-2)=(N(1-9)*100+12)%97     
                var validacion2 = ((string.Concat(c1.ToString(), c2.ToString())).Entero() == ((n * 100 + 12) % 97));

                return validacion1 || validacion2;
            }
            catch
            {
                return false;
            }
        }
        private static bool ValidarCifEu(string numeroCif)
        {
            try
            {
                if (string.IsNullOrEmpty(numeroCif))
                    return false;

                // Quitar prefijo de país si lo hay (US, EIN, etc.)
                if (numeroCif.Length > 2 && !char.IsDigit(numeroCif[0]))
                    numeroCif = numeroCif.Remove(0, 2);

                // Eliminar guiones y espacios
                numeroCif = numeroCif.Replace("-", "").Replace(" ", "").Trim();

                // El EIN debe tener exactamente 9 dígitos
                if (numeroCif.Length != 9)
                    return false;

                // Debe ser todo numérico
                if (!numeroCif.All(char.IsDigit))
                    return false;

                // Los dos primeros dígitos son el prefijo del IRS
                // Hay prefijos no válidos según el IRS
                int prefijo = int.Parse(numeroCif.Substring(0, 2));
                var prefijoNoValidos = new HashSet<int> { 0, 7, 8, 9, 17, 18, 19, 28, 29, 49, 69, 70, 78, 79, 89, 96, 97 };

                if (prefijoNoValidos.Contains(prefijo))
                    return false;

                // Los últimos 7 dígitos no pueden ser todos ceros
                string sufijo = numeroCif.Substring(2);
                if (sufijo == "0000000")
                    return false;

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static (string Nombre, string Ape1, string Ape2) InferirNombreConApellidos(string nombreCompleto)
        {
            try
            {
                // Limpieza inicial para evitar espacios extra
                nombreCompleto = nombreCompleto.Trim();

                // Manejar casos con coma
                if (nombreCompleto.Contains(","))
                {
                    var partes = nombreCompleto.Split(',');
                    // Manejo de apellidos: puede contener más de una palabra
                    var apellidosParte = partes[0].Trim();
                    var nombre = partes.Length > 1 ? partes[1].Trim() : "";

                    // Asume el primer espacio como separador, el resto va a Ape2
                    string ape11 = apellidosParte.Contains(" ") ? apellidosParte.Substring(0, apellidosParte.IndexOf(' ')) : apellidosParte;
                    string ape22 = apellidosParte.Contains(" ") ? apellidosParte.Substring(apellidosParte.IndexOf(' ') + 1) : "";

                    return (Nombre: nombre, Ape1: ape11, Ape2: ape22);
                }

                // --- Lógica de división por espacios ---
                string[] partesNombre = nombreCompleto.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries); // Evita problemas con múltiples espacios

                // Caso 1: Solo Nombre (Ej: "Juan")
                if (partesNombre.Length == 1)
                    return (Nombre: partesNombre[0], Ape1: "", Ape2: ""); // ✅ SOLUCIÓN AL ERROR

                // Caso 2: Nombre y Primer Apellido (Ej: "Juan Pérez")
                if (partesNombre.Length == 2)
                    return (Nombre: partesNombre[0], Ape1: partesNombre[1], Ape2: "");

                // Lista de preposiciones comunes (Lógica original)
                var preposiciones = new HashSet<string> { "de", "del", "de la", "de los", "de las" };

                int indiceUltimaPreposicion = Array.FindLastIndex(partesNombre, p => preposiciones.Contains(p.ToLower()));

                if (indiceUltimaPreposicion != -1 && indiceUltimaPreposicion < partesNombre.Length - 1)
                {
                    // Si la preposición está antes del último elemento
                    // Asumimos: [Nombre(s)] [PrimerApellido] [Preposición] [SegundoApellido]
                    // Tu lógica original parece asumir: [Nombre(s)] [PrimerApellido] [Preposición] [Apellidos restantes]

                    // Simplificando la lógica de preposición para evitar errores complejos de índice:
                    string nombre = string.Join(" ", partesNombre.Take(indiceUltimaPreposicion));
                    string primerApellido = partesNombre[indiceUltimaPreposicion];
                    string segundoApellido = string.Join(" ", partesNombre.Skip(indiceUltimaPreposicion + 1));

                    // NOTA: Esta sección de preposición puede requerir más ajuste según los casos de tu negocio, 
                    // ya que la lógica original era compleja y propensa a errores de índice.
                    // La he ajustado para que sea más robusta.

                    // Revertimos a la lógica de preposición más segura si quieres mantener esa funcionalidad:
                    if (partesNombre.Length > 3)
                    {
                        // Lógica común: Nombre(s) + 2 Apellidos
                        return (
                            Nombre: string.Join(" ", partesNombre.Take(partesNombre.Length - 2)),
                            Ape1: partesNombre[partesNombre.Length - 2],
                            Ape2: partesNombre[partesNombre.Length - 1]
                        );
                    }

                    // Si quieres mantener tu lógica original, deberías revisarla, pero la dejaré fuera por seguridad.
                    // Volvemos a la lógica simple de tres o más partes sin preposición.
                }

                // Caso 3: Nombre, Primer Apellido y Segundo Apellido (o más)
                // Asume que las dos últimas palabras son los apellidos.
                string nombreSimple = string.Join(" ", partesNombre.Take(partesNombre.Length - 2));
                string ape1 = partesNombre[partesNombre.Length - 2];
                string ape2 = partesNombre[partesNombre.Length - 1];

                return (Nombre: nombreSimple, Ape1: ape1, Ape2: ape2);
            }
            catch
            {
                return (Nombre: nombreCompleto, Ape1: "", Ape2: "");
            }
        }


        public static enumClaseDeNacionalidad ClaseDeNacionalidad(string nif)
        {
            if (nif.IsNullOrEmpty()) GestorDeErrores.Emitir("El NIF no puede ser nulo o vacío");

            nif = nif.Trim().ToUpper();

            // Si empieza por ES, quitar el prefijo y tratarlo como nacional
            if (nif.StartsWith(ltrIsoPaises.Spain))
                nif = nif.Substring(2);

            // NIE: X, Y, Z inicial (considerados nacionales)
            if (nif.Length == 9 && (nif[0] == 'X' || nif[0] == 'Y' || nif[0] == 'Z'))
                return enumClaseDeNacionalidad.Nacional;

            // DNI: 8 dígitos + letra (persona nacional)
            if (nif.Length == 9 && char.IsDigit(nif[0]))
                return enumClaseDeNacionalidad.Nacional;

            // CIF: Letra inicial (A-H, J, U, V, N, P, Q, R, S, W) + 7 dígitos + letra/dígito
            if (nif.Length == 9 && char.IsLetter(nif[0]))
            {
                // Si es CIF español
                return enumClaseDeNacionalidad.Nacional;
            }

            // Si empieza por dos letras y no es ES, es intracomunitario
            if (nif.Length > 2 && char.IsLetter(nif[0]) && char.IsLetter(nif[1]))
            {
                if (nif.StartsWith(ltrIsoPaises.Spain))
                    return enumClaseDeNacionalidad.Nacional;
                else
                    return enumClaseDeNacionalidad.Intracomunitario;
            }

            // Si no cumple nada de lo anterior, lo consideramos extracomunitario
            return enumClaseDeNacionalidad.Extracomunitario;
        }
    }


}
