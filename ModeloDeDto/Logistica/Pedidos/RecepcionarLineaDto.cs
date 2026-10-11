using Utilidades;

namespace ModeloDeDto.Logistica
{
    // Datos de lo que se recibe de una línea de un pedido, se piden en la modal de recepcionar una línea (ver PedidosController.epRecibirLinea)
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, MostrarExpresion = "Recepcionar línea")]
    public class RecepcionarLineaDto
    {
        [IUPropiedad(Etiqueta = "Id de la recepción", Visible = false)]
        public int Id { get; set; }

        [IUPropiedad(Etiqueta = "Id de la línea del pedido", Visible = false)]
        public int IdLineaPedido { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Cantidad",
           Tipo = typeof(decimal),
           Ayuda = "cantidad entregada",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           OnBlur = "javascript:" + nameof(enumNameSpaceTs.Logistica) + "." + nameof(enumFunctionTs.Pedido_CalcularImporteEntregado) + "()",
           Obligatorio = true,
           Formato = enumFormato.Numero_6,
           Fila = 0,
           Columna = 0)
        ]
        public decimal? Cantidad { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Precio",
           Tipo = typeof(decimal),
           Ayuda = "precio de compra unitario sin iva",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           OnBlur = "javascript:" + nameof(enumNameSpaceTs.Logistica) + "." + nameof(enumFunctionTs.Pedido_CalcularImporteEntregado) + "()",
           Obligatorio = true,
           Formato = enumFormato.Numero_6,
           Fila = 0,
           Columna = 1)
        ]
        public decimal? Precio { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Descuento(%)",
           Tipo = typeof(decimal),
           Ayuda = "porcentaje de descuento aplicado",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           OnBlur = "javascript:" + nameof(enumNameSpaceTs.Logistica) + "." + nameof(enumFunctionTs.Pedido_CalcularImporteEntregado) + "()",
           Obligatorio = false,
           Formato = enumFormato.Porcentaje,
           Fila = 1,
           Columna = 0)
        ]
        public decimal? Descuento { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Importe entregado",
           Tipo = typeof(decimal),
           Ayuda = "importe de lo entregado con el descuento aplicado, sin iva",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Obligatorio = false,
           Formato = enumFormato.Moneda,
           Fila = 1,
           Columna = 1)
        ]
        public decimal? ImporteEntregado { get; set; }
    }
}
