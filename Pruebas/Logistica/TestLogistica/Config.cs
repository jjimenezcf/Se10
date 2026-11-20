using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace TestLogistica
{
    // Datos de las pruebas, en config.json junto al ejecutable: se pueden cambiar sin recompilar
    public class Config
    {
        // sólo se deja ejecutar contra esta BD (la de la cadena de conexión de appsettings.json)
        public string BaseDeDatosPermitida { get; set; } = "SE_TEST";

        public int IdSociedad { get; set; } = 1153;
        // almacén de referencia: si no usa variación de existencias, la prueba abre otro en su centro gestor que sí la usa
        public int IdAlmacen { get; set; } = 2;
        public int IdProveedor { get; set; } = 123;
        // 0: el primer tipo de pedido
        public int IdTipoDePedido { get; set; } = 0;

        // materiales del pedido: unitario, cantidad pedida y precio
        public List<LineaDePrueba> Lineas { get; set; } = new()
        {
            new LineaDePrueba { IdUnitario = 1199, Cantidad = 10, Precio = 12.50m },
            new LineaDePrueba { IdUnitario = 1212, Cantidad = 4, Precio = 30m }
        };

        // stock de cada material en el inventario inicial (regularización de clase Inicial de ayer), al precio de su línea
        public decimal CantidadInicial { get; set; } = 5;

        // cuánto se devuelve de la entrega de la primera línea
        public decimal CantidadADevolver { get; set; } = 3;

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
            nuevo.Guardar();
            return nuevo;
        }

        public void Guardar() => File.WriteAllText(RutaFichero, JsonSerializer.Serialize(this, Opciones));
    }

    public class LineaDePrueba
    {
        public int IdUnitario { get; set; }
        public decimal Cantidad { get; set; }
        public decimal Precio { get; set; }
    }
}
