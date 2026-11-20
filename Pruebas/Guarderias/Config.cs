using System;
using System.IO;
using System.Text.Json;

namespace PruebasDeIntegracion
{
    // config.json junto al ejecutable: se puede cambiar sin recompilar
    public class Config
    {
        // sólo se deja ejecutar contra esta BD (la de la cadena de conexión de appsettings.json)
        public string BaseDeDatosPermitida { get; set; } = "SE_TEST";

        private static string RutaFichero => Path.Combine(AppContext.BaseDirectory, "config.json");
        private static readonly JsonSerializerOptions Opciones = new() { WriteIndented = true, PropertyNameCaseInsensitive = true };

        public static Config CargarOCrear()
        {
            if (File.Exists(RutaFichero))
            {
                var config = JsonSerializer.Deserialize<Config>(File.ReadAllText(RutaFichero), Opciones);
                if (config != null)
                    return config;
            }

            var nuevo = new Config();
            File.WriteAllText(RutaFichero, JsonSerializer.Serialize(nuevo, Opciones));
            return nuevo;
        }
    }
}
