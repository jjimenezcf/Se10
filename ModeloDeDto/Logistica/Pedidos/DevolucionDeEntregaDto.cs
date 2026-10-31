using System;
using Utilidades;

namespace ModeloDeDto.Logistica
{
    // Devolución de lo entregado en una línea de un pedido, se pide en la modal de devolver la entrega (ver LineasDeUnPedidoController.epDevolverEntrega).
    // Se propone lo que queda por devolver, valorado al precio y descuento de la entrega, y la fecha de hoy; se editan la cantidad, la fecha y el motivo
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, MostrarExpresion = "Devolver entrega")]
    public class DevolucionDeEntregaDto
    {
        [IUPropiedad(Etiqueta = "Id de la devolución", Visible = false)]
        public int Id { get; set; }

        [IUPropiedad(Etiqueta = "Id de la línea del pedido", Visible = false)]
        public int IdLineaPedido { get; set; }

        //--------------------------------------------
        // Fila 0: lo devuelto y su valoración; fila 1: fecha y destino de la entrega; fila 3: el motivo
        [IUPropiedad(
           Etiqueta = "Cantidad",
           Tipo = typeof(decimal),
           Ayuda = "cantidad a devolver, como mucho lo entregado menos lo ya devuelto",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           OnBlur = "javascript:" + nameof(enumNameSpaceTs.Logistica) + "." + nameof(enumFunctionTs.Pedido_CalcularImporteDevuelto) + "()",
           Obligatorio = true,
           Formato = enumFormato.Numero_6,
           Fila = 0,
           Columna = 0)
        ]
        public decimal? Cantidad { get; set; }

        [IUPropiedad(
           Etiqueta = "Precio",
           Tipo = typeof(decimal),
           Ayuda = "precio de compra unitario sin iva de la entrega",
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
           Ayuda = "porcentaje de descuento aplicado en la entrega",
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
           Ayuda = "importe de lo devuelto con el descuento de la entrega, sin iva",
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
        [IUPropiedad(
              Etiqueta = "Devuelta El"
            , Ayuda = "Fecha de la devolución: no puede ser posterior a hoy ni anterior a la entrega o al último cierre contable"
            , TipoDeControl = enumTipoControl.SelectorDeFecha
            , Formato = enumFormato.Fecha
            , Obligatorio = true
            , Fila = 1
            , Columna = 0)]
        public DateTime? DevueltaEl { get; set; }

        // Destino de la entrega que se devuelve, sólo se muestra
        [IUPropiedad(
            Etiqueta = "Almacén",
            Ayuda = "Almacén al que fue la entrega",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Almacen),
            Fila = 1,
            Columna = 1,
            VisibleEnGrid = false,
            Obligatorio = false,
            Controlador = nameof(enumControladoresLogistica.Almacenes),
            VistaDondeNavegar = enumVistasLogisticas.CrudAlmacenes
            )
        ]
        public int? IdAlmacen { get; set; }

        [IUPropiedad(Etiqueta = "Almacén", Visible = false)]
        public string Almacen { get; set; }

        [IUPropiedad(
            Etiqueta = "Obra del expediente",
            Ayuda = "Expediente a cuya obra fue la entrega",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Expediente),
            Fila = 1,
            Columna = 2,
            VisibleEnGrid = false,
            Obligatorio = false,
            Controlador = nameof(enumControladoresAdministrativos.Expedientes),
            VistaDondeNavegar = enumVistasAdministrativo.CrudExpedientes
            )
        ]
        public int? IdExpediente { get; set; }

        [IUPropiedad(Etiqueta = "Expediente", Visible = false)]
        public string Expediente { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Motivo",
           Ayuda = "Motivo de la devolución",
           TipoDeControl = enumTipoControl.AreaDeTexto,
           Fila = 3,
           Columna = 0,
           Obligatorio = true,
           NumeroDeFilas = 3,
           AutoSpan = true
           )
        ]
        public string Motivo { get; set; }
    }
}
