using Utilidades;

namespace ModeloDeDto.Ventas
{
    [IUDto()]
    public class TotalesDeFacturasEmt: TotalesDto
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
           Etiqueta = "Irpf",
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
           Etiqueta = "BI con Iva y sin Irpf",
           Tipo = typeof(decimal),
           Ayuda = "total de las facturas seleccionadas con el Iva y sin el Irpf",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Fila = 0,
           Columna = 4,
           Formato = enumFormato.Moneda
            )
        ]
        public decimal APagar { get; set; }

        //-----------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Pdt de cobro",
           Tipo = typeof(decimal),
           Ayuda = "Importe pendiente de cobro",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Fila = 1,
           Columna = 0,
           Formato = enumFormato.Moneda
            )
        ]
        public decimal PendienteDeCobro { get; set; }

        //-----------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Cobros parciales",
           Tipo = typeof(decimal),
           Ayuda = "importe cobrado parcialmente",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Fila = 1,
           Columna = 1,
           Formato = enumFormato.Moneda
            )
        ]
        public decimal CobrosParciales { get; set; }


        //-----------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Cobros totales",
           Tipo = typeof(decimal),
           Ayuda = "importe cobrado parcialmente",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Fila = 1,
           Columna = 2,
           Formato = enumFormato.Moneda
            )
        ]
        public decimal CobrosTotales { get; set; }

        //-----------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Total cobrado",
           Tipo = typeof(decimal),
           Ayuda = "total que está cobrado",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Fila = 1,
           Columna = 3,
           MantenerHuecoDeLaIzquierda = true,
           Formato = enumFormato.Moneda
            )
        ]
        public decimal Cobrado { get; set; }


        //-----------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Por abonar",
           Tipo = typeof(decimal),
           Ayuda = "importe pendiente de abona",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Fila = 2,
           Columna = 2,
           MantenerHuecoDeLaIzquierda = true,
           Formato = enumFormato.Moneda
            )
        ]
        public decimal PorAbonar { get; set; }

        //-----------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Total abonado",
           Tipo = typeof(decimal),
           Ayuda = "total que está abonado",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Fila = 2,
           Columna = 3,
           MantenerHuecoDeLaIzquierda = true,
           Formato = enumFormato.Moneda
            )
        ]
        public decimal Abonado { get; set; }
    }
}
