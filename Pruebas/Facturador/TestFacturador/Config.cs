using System.Text.Json;

namespace TestFacturador;

public class Config
{
    public string Nif { get; set; } = "00811725D";
    public string NifFacturado { get; set; } = "V30048086";
    public string ApiKey { get; set; } = "";
    public string UrlBase { get; set; } = "localhost:44396";

    // últimos datos introducidos al crear un cliente, se proponen por defecto en la siguiente vez
    public ClienteJson? UltimoCliente { get; set; }

    private static string RutaFichero => Path.Combine(AppContext.BaseDirectory, "config.json");

    private static readonly JsonSerializerOptions Opciones = new() { WriteIndented = true, PropertyNameCaseInsensitive = true };

    public static Config CargarOCrear()
    {
        if (File.Exists(RutaFichero))
        {
            var json = File.ReadAllText(RutaFichero);
            var config = JsonSerializer.Deserialize<Config>(json, Opciones);
            if (config != null)
                return config;
        }

        var nuevo = new Config();
        nuevo.Guardar();
        return nuevo;
    }

    public void Guardar()
    {
        var json = JsonSerializer.Serialize(this, Opciones);
        File.WriteAllText(RutaFichero, json);
    }
}
