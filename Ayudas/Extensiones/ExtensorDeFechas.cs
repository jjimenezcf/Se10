using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;


namespace Utilidades
{
    public static class ExtensorDeFechas
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
}
