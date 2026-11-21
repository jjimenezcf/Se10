using System;
using System.IO;
using System.Text.Json;
using TestLogistica;

namespace PruebasDeIntegracion
{
    // config.json junto al ejecutable: se puede cambiar sin recompilar. Lo común arriba y una sección por ámbito que necesite datos propios
    public class Config
    {
        // sólo se deja ejecutar contra esta BD (la de la cadena de conexión de appsettings.json)
        public string BaseDeDatosPermitida { get; set; } = "SE_TEST";

        public ConfigDeLogistica Logistica { get; set; } = new();

        private static string RutaFichero => Path.Combine(AppContext.BaseDirectory, "config.json");
        private static readonly JsonSerializerOptions Opciones = new() { WriteIndented = true, PropertyNameCaseInsensitive = true };

        public static Config CargarOCrear()
        {
            if (File.Exists(RutaFichero))
            {
                var config = JsonSerializer.Deserialize<Config>(File.ReadAllText(RutaFichero), Opciones);
                if (config != null)
                {
                    // un config.json de antes de añadir una sección: se completa con sus valores por defecto
                    if (config.Logistica is null)
                    {
                        config.Logistica = new ConfigDeLogistica();
                        config.Guardar();
                    }
                    return config;
                }
            }

            var nuevo = new Config();
            nuevo.Guardar();
            return nuevo;
        }

        public void Guardar() => File.WriteAllText(RutaFichero, JsonSerializer.Serialize(this, Opciones));
    }
}
