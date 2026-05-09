using Utilidades;

namespace ModeloDeDto.Gastos
{
    [IUDto()]
    public class TotalesDePago: TotalesDto
    {
        //-----------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Total a Pagar",
           Tipo = typeof(decimal),
           Ayuda = "Importe de lo que hay que pagary pagado",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Fila = 0,
           Columna = 0,
           Formato = enumFormato.Moneda
            )
        ]
        public decimal TotalPagos { get; set; }

        //-----------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Total por pagar",
           Tipo = typeof(decimal),
           Ayuda = "Importe de lo que hay que pagar",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Fila = 0,
           Columna = 1,
           Formato = enumFormato.Moneda
            )
        ]
        public decimal TotalPendiente { get; set; }

        //-----------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Total pagado",
           Tipo = typeof(decimal),
           Ayuda = "Importe de lo ya pagado",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Fila = 0,
           Columna = 2,
           Formato = enumFormato.Moneda
            )
        ]
        public decimal TotalPagado { get; set; }

        //-----------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Pagos al contado",
           Tipo = typeof(decimal),
           Ayuda = "Importe de lo pagado al contado",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Fila = 1,
           Columna = 0,
           Formato = enumFormato.Moneda
            )
        ]
        public decimal PagosContado { get; set; }

        //-----------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Pagos domiciliados",
           Tipo = typeof(decimal),
           Ayuda = "Importe de lo pagado por domiciliación",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Fila = 1,
           Columna = 1,
           Formato = enumFormato.Moneda
            )
        ]
        public decimal PagosDomiciliados { get; set; }

        //-----------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Pagos con tarjeta",
           Tipo = typeof(decimal),
           Ayuda = "Importe de lo pagado usando tarjetas",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Fila = 1,
           Columna = 2,
           Formato = enumFormato.Moneda
            )
        ]
        public decimal PagosTarjeta { get; set; }

        //-----------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Pagos con transferencias",
           Tipo = typeof(decimal),
           Ayuda = "Importe de lo transferido",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           MantenerHuecoDeLaIzquierda = true,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Fila = 2,
           Columna = 1,
           Formato = enumFormato.Moneda
            )
        ]
        public decimal PagosTransferidos { get; set; }

        //-----------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Pagos remesados",
           Tipo = typeof(decimal),
           Ayuda = "Importe de lo remesado",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Fila = 2,
           Columna = 2,
           Formato = enumFormato.Moneda
            )
        ]
        public decimal PagosRemesados { get; set; }
    }
}
