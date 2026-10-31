using System.Collections.Generic;
using System.Linq;

namespace ModeloDeDto.Logistica
{
    // Impreso de las devoluciones de un pedido: la misma cabecera y parámetros de impresión que el pedido (ver PedidoRpt), y en el
    // detalle lo devuelto en vez de las líneas pedidas (ver GeneradorDeDevolucionDePedidoRpt y ReporteDeDevolucionDePedido)
    public class DevolucionDePedidoRpt : PedidoRpt
    {
        public List<DevolucionDeUnPedidoDto> Devoluciones { get; set; } = new List<DevolucionDeUnPedidoDto>();
        public bool HayDescuentoEnLoDevuelto => Devoluciones.Any(devolucion => devolucion.Descuento is not null && devolucion.Descuento > 0);

        public DevolucionDePedidoRpt()
        {
            Lineas = new List<LineaDeUnPedidoDto>();
        }
    }
}
