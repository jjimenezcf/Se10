using Utilidades;

namespace ModeloDeDto.Logistica
{
    // Modificación de lo servido en una línea de un documento de venta servido (aún sin prefacturar): se pide en la modal que abre la acción
    // Devolver de sus líneas en la etapa de servicio (ver Logistica.Dsa_DevolverLinea y VentasController.epModificarLineaServida). Se
    // propone lo servido y el precio de venta de la línea; al aceptar se modifican la línea y su salida del almacén (con su preasiento y el
    // barrido de los posteriores) y se reimprime la oferta (ver ExtensorDeVentas.ModificarLineaServida)
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, MostrarExpresion = "Modificar lo servido")]
    public class ModificarLineaVentaDto
    {
        [IUPropiedad(Etiqueta = "Id de la línea vendida", Visible = false)]
        public int IdLineaDeVenta { get; set; }

        [IUPropiedad(Etiqueta = "Descuento de la línea", Visible = false)]
        public decimal? Descuento { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Cantidad servida",
           Tipo = typeof(decimal),
           Ayuda = "cantidad que realmente se ha servido",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           OnBlur = "javascript:" + nameof(enumNameSpaceTs.Logistica) + "." + nameof(enumFunctionTs.Dsa_CalcularImporteServido) + "()",
           Obligatorio = true,
           Formato = enumFormato.Numero_4,
           Fila = 0,
           Columna = 0)
        ]
        public decimal? Cantidad { get; set; }

        [IUPropiedad(
           Etiqueta = "Precio de venta",
           Tipo = typeof(decimal),
           Ayuda = "precio unitario sin iva al que se vende",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           OnBlur = "javascript:" + nameof(enumNameSpaceTs.Logistica) + "." + nameof(enumFunctionTs.Dsa_CalcularImporteServido) + "()",
           Obligatorio = true,
           Formato = enumFormato.Moneda,
           Fila = 0,
           Columna = 1)
        ]
        public decimal? Precio { get; set; }

        [IUPropiedad(
           Etiqueta = "Precio total",
           Tipo = typeof(decimal),
           Ayuda = "importe de la línea sin iva, con el descuento de la línea",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Obligatorio = false,
           Formato = enumFormato.Moneda,
           Fila = 0,
           Columna = 2)
        ]
        public decimal? Importe { get; set; }
    }
}
