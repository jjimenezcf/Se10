using Utilidades;

namespace ModeloDeDto.Gastos
{
    [IUDto()]
    public class TotalesDeFacturasRec: TotalesDto
    {
        //-----------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Base Imponible",
           Tipo = typeof(decimal),
           Ayuda = "total de las BI seleccionadas",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Fila = 0,
           Columna = 0,
           Formato = enumFormato.Moneda
            )
        ]
        public decimal Bi { get; set; }       
        
        //-----------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Iva",
           Tipo = typeof(decimal),
           Ayuda = "total del Iva",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Fila = 0,
           Columna = 1,
           Formato = enumFormato.Moneda
            )
        ]
        public decimal Iva { get; set; }
        
        //-----------------------------------------------------
        [IUPropiedad(
           Etiqueta = "IRPF",
           Tipo = typeof(decimal),
           Ayuda = "total del Irpf",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Fila = 0,
           Columna = 2,
           Formato = enumFormato.Moneda
            )
        ]
        public decimal Irpf { get; set; }

        //-----------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Total factura",
           Tipo = typeof(decimal),
           Ayuda = "total de las facturas seleccionadas (Bi + Iva + Irpf)",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Fila = 1,
           Columna = 0,
           Formato = enumFormato.Moneda
            )
        ]
        public decimal Total { get; set; }

        //-----------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Total a pagar",
           Tipo = typeof(decimal),
           Ayuda = "total a pagar de las facturas seleccionadas (Bi + Iva)",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Fila = 1,
           Columna = 1,
           Formato = enumFormato.Moneda
            )
        ]
        public decimal Pagar { get; set; }

        //-----------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Pendiente de pago",
           Tipo = typeof(decimal),
           Ayuda = "total a pendiente de pago de la selección",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Fila = 1,
           Columna = 2,
           Formato = enumFormato.Moneda
            )
        ]
        public decimal Pendiente { get; set; }

        //-----------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Total pagado",
           Tipo = typeof(decimal),
           Ayuda = "total que está pagado",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Fila = 3,
           Columna = 2,
           MantenerHuecoDeLaIzquierda = true,
           Formato = enumFormato.Moneda
            )
        ]
        public decimal Pagado { get; set; }

        //-----------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Totales de impuestos",
           Ayuda = "muestra la información de los totales por Iva calculados",
           TipoDeControl = enumTipoControl.AreaDeTexto,
           EditableAlCrear = false,
           EditableAlEditar = false,
           NumeroDeFilas = 5,
           CssDelArea = enumCssControles.MonoSpaceText,
           Fila = 2,
           Columna = 0,
           AutoSpan = true
            )
        ]
        public string TotalesPorImpuestos { get; set; }
    }
}
