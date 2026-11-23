using Utilidades;

namespace ModeloDeDto.Logistica
{
    // Devolución de una línea de un documento de venta facturado, se pide en la modal que abre la acción Devolver de sus líneas (ver
    // Logistica.Dsa_DevolverLinea y DevolucionesDeVentaController.epDevolverLineaDeVenta). Se propone lo que queda por devolver (lo vendido
    // menos lo ya devuelto) al precio y descuento al que se vendió; se elige la devolución en tramitación del cliente a la que se añade
    // o, si no se elige ninguna, se crea una nueva
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, MostrarExpresion = "Devolver línea")]
    public class DevolverVentaDto
    {
        [IUPropiedad(Etiqueta = "Id de la línea vendida", Visible = false)]
        public int IdLineaDeVenta { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Cantidad",
           Tipo = typeof(decimal),
           Ayuda = "cantidad a devolver, como mucho lo vendido menos lo ya devuelto",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           OnBlur = "javascript:" + nameof(enumNameSpaceTs.Logistica) + "." + nameof(enumFunctionTs.Dsa_CalcularImporteADevolver) + "()",
           Obligatorio = true,
           Formato = enumFormato.Numero_4,
           Fila = 0,
           Columna = 0)
        ]
        public decimal? Cantidad { get; set; }

        [IUPropiedad(
           Etiqueta = "Precio",
           Tipo = typeof(decimal),
           Ayuda = "precio unitario sin iva al que se vendió",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Obligatorio = false,
           Formato = enumFormato.Moneda,
           Fila = 0,
           Columna = 1)
        ]
        public decimal? Precio { get; set; }

        [IUPropiedad(
           Etiqueta = "Descuento(%)",
           Tipo = typeof(decimal),
           Ayuda = "descuento con el que se vendió",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Obligatorio = false,
           Formato = enumFormato.Porcentaje,
           Fila = 0,
           Columna = 2)
        ]
        public decimal? Descuento { get; set; }

        [IUPropiedad(
           Etiqueta = "Importe",
           Tipo = typeof(decimal),
           Ayuda = "importe devuelto sin iva",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Obligatorio = false,
           Formato = enumFormato.Moneda,
           Fila = 0,
           Columna = 3)
        ]
        public decimal? Importe { get; set; }

        //--------------------------------------------
        // devoluciones en tramitación del cliente: se cargan al abrir la modal (ver CrudDeVentas.AbrirModalDeDevolverLinea)
        [IUPropiedad(Etiqueta = "Id de la devolución", Visible = false)]
        public int? IdDevolucion { get; set; }

        [IUPropiedad(
            Etiqueta = "Devolución",
            Ayuda = "Devolución en tramitación a la que se añade la línea; si no se elige ninguna se crea una nueva",
            TipoDeControl = enumTipoControl.ListaDeElemento,
            SeleccionarDe = typeof(DevolucionDeVentaDto),
            Controlador = nameof(enumControladoresLogistica.DevolucionesDeVenta),
            GuardarEn = nameof(IdDevolucion),
            MostrarExpresion = nameof(DevolucionDeVentaDto.Expresion),
            CargarBajoDemanda = true,
            Fila = 1,
            Columna = 0,
            Obligatorio = false,
            AutoSpan = true
            )
        ]
        public string Devolucion { get; set; }
    }
}
