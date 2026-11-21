using System.Collections.Generic;

namespace TestLogistica
{
    // Datos de las pruebas de logística: la sección "Logistica" de config.json
    public class ConfigDeLogistica
    {
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
    }

    public class LineaDePrueba
    {
        public int IdUnitario { get; set; }
        public decimal Cantidad { get; set; }
        public decimal Precio { get; set; }
    }
}
