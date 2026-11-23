using ServicioDeDatos;
using Utilidades;

namespace ModeloDeDto.Logistica
{
    // Línea de una devolución de venta: se selecciona el documento de venta (lupa) y una de sus líneas (lista); se propone la cantidad
    // vendida y el precio y descuento de la línea vendida, que no se pueden cambiar; el importe es lo que se abona, sin iva
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, OpcionDeBorrar = false)]
    public class LineaDeUnaDevolucionDeVentaDto : EsUnDetalleDto
    {

        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Orden",
            Ayuda = "orden de la línea",
            Tipo = typeof(int),
            Fila = 0,
            Columna = 0,
            CssDeLaFila = enumCssGrid.fila200pxFr
            )
        ]
        public int Orden { get; set; }

        //--------------------------------------------
        // documento de venta del que se devuelve: sólo para seleccionar la línea (no se guarda, la línea ya lo indica)
        [IUPropiedad(Etiqueta = "Id del documento de venta", Visible = false)]
        public int? IdVenta { get; set; }

        [IUPropiedad(
            Etiqueta = "Documento de venta",
            Ayuda = "Documento de venta (servido o facturado) del que se devuelve",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(VentaDto),
            GuardarEn = nameof(IdVenta),
            Controlador = nameof(enumControladoresLogistica.Ventas),
            VistaDondeNavegar = enumVistasLogisticas.CrudVentas,
            MostrarExpresion = nameof(VentaDto.Expresion),
            LongitudMinimaParaBuscar = 1,
            Fila = 0,
            Columna = 1,
            Obligatorio = false,
            EditableAlEditar = false,
            VisibleEnGrid = true,
            AutoSpan = true,
            trasSeleccionar = "javascript:" + nameof(enumNameSpaceTs.Logistica) + "." + nameof(enumFunctionTs.Ddv_Tras_Seleccionar_Venta) + "()",
            trasBlanquear = "javascript:" + nameof(enumNameSpaceTs.Logistica) + "." + nameof(enumFunctionTs.Ddv_Tras_Blanquear_Venta) + "()"
            )
        ]
        public string Venta { get; set; }

        //----------------------------------------------------------
        // línea vendida que se devuelve: la lista se carga con las líneas del documento de venta al seleccionarlo (ver
        // Logistica.Ddv_Tras_Seleccionar_Venta), por eso se carga bajo demanda
        [IUPropiedad(Etiqueta = "Id de la línea vendida", Visible = false)]
        public int IdLineaDeVenta { get; set; }

        [IUPropiedad(
            Etiqueta = "Línea vendida",
            Ayuda = "Línea del documento de venta que se devuelve",
            TipoDeControl = enumTipoControl.ListaDeElemento,
            SeleccionarDe = typeof(LineaDeUnaVentaDto),
            Controlador = nameof(enumControladoresLogistica.LineasDeUnaVenta),
            GuardarEn = nameof(IdLineaDeVenta),
            MostrarExpresion = nameof(LineaDeUnaVentaDto.Unitario),
            CargarBajoDemanda = true,
            Fila = 1,
            Columna = 0,
            Obligatorio = true,
            EditableAlEditar = false,
            VisibleEnGrid = true,
            AutoSpan = true,
            OnChange = "javascript:" + nameof(enumNameSpaceTs.Logistica) + "." + nameof(enumFunctionTs.Ddv_Tras_Seleccionar_LineaDeVenta) + "()"
            )
        ]
        public string LineaDeVenta { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Cantidad",
           Tipo = typeof(decimal),
           Ayuda = "cantidad devuelta, como mucho la vendida en la línea",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           OnBlur = "javascript:" + nameof(enumNameSpaceTs.Logistica) + "." + nameof(enumFunctionTs.Ddv_CalcularImporteDeLinea) + "()",
           Obligatorio = true,
           Formato = enumFormato.Numero_4,
           Fila = 2,
           Columna = 0)
        ]
        public decimal Cantidad { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Precio",
           Tipo = typeof(decimal),
           Ayuda = "precio unitario al que se vendió",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Obligatorio = false,
           Formato = enumFormato.Moneda,
           Fila = 2,
           Columna = 1)
        ]
        public decimal Precio { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Descuento",
           Tipo = typeof(decimal),
           Ayuda = "descuento con el que se vendió",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Obligatorio = false,
           Formato = enumFormato.Porcentaje,
           Fila = 2,
           Columna = 2)
        ]
        public decimal? Descuento { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Importe",
           Tipo = typeof(decimal),
           Ayuda = "importe devuelto sin iva (cantidad x precio - descuento)",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Obligatorio = false,
           Formato = enumFormato.Moneda,
           Fila = 2,
           Columna = 3)
        ]
        public decimal Importe { get; set; }

        //--------------------------------------------
        // datos de la devolución para el grid de devoluciones de la edición del documento de venta (ver DescriptorDeVentas): se calculan al
        // leer (ver GestorDeLineasDeUnaDevolucionDeVenta.DespuesDeMapearElElemento)
        [IUPropiedad(Etiqueta = "Devolución", Visible = false)]
        public string Devolucion { get; set; }

        [IUPropiedad(Etiqueta = "Estado de la devolución", Visible = false)]
        public string EstadoDeLaDevolucion { get; set; }

        [IUPropiedad(Etiqueta = "Devolución aceptada", Visible = false)]
        public bool Aceptada { get; set; }

        [IUPropiedad(Etiqueta = "Devolución cancelada", Visible = false)]
        public bool Cancelada { get; set; }

    }
}
