using Utilidades;

namespace ModeloDeDto.Ventas
{
    [IUDto()]
    public class TotalesDePresupuestos: TotalesDto
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
           Etiqueta = "Presupuestado",
           Tipo = typeof(decimal),
           Ayuda = "total de los presupuestos seleccionados",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Fila = 0,
           Columna = 2,
           Formato = enumFormato.Moneda
            )
        ]
        public decimal Presupuestado { get; set; }

        //-----------------------------------------------------
        [IUPropiedad(
           Etiqueta = "En elaboración",
           Tipo = typeof(decimal),
           Ayuda = "Importe de los presupuestos no presentados al cliente",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Fila = 1,
           Columna = 0,
           Formato = enumFormato.Moneda
            )
        ]
        public decimal EnElaboracion { get; set; }

        //-----------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Pdt de aceptar",
           Tipo = typeof(decimal),
           Ayuda = "importe de los presupuestos presentados al cliente",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Fila = 1,
           Columna = 1,
           Formato = enumFormato.Moneda
            )
        ]
        public decimal PdtDelCliente { get; set; }

        //-----------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Facturable",
           Tipo = typeof(decimal),
           Ayuda = "importe de los presupuestos aceptados y no facturado",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Fila = 1,
           Columna = 1,
           Formato = enumFormato.Moneda
            )
        ]
        public decimal Facturable { get; set; }

        //-----------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Pdt sin facturas",
           Tipo = typeof(decimal),
           Ayuda = "importe de presupuestos sin ninguna facturar",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Fila = 2,
           Columna = 0,
           Formato = enumFormato.Moneda
            )
        ]
        public decimal PptSinFacturas { get; set; }

        //-----------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Facturado",
           Tipo = typeof(decimal),
           Ayuda = "importe de lo facturado de los presupuestos",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Fila = 2,
           Columna = 1,
           Formato = enumFormato.Moneda
            )
        ]
        public decimal Facturado { get; set; }

    }
}
