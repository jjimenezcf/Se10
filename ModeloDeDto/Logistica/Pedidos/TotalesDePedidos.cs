using Utilidades;

namespace ModeloDeDto.Logistica
{
    [IUDto()]
    public class TotalesDePedidos: TotalesDto
    {
        //-----------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Total pedido",
           Tipo = typeof(decimal),
           Ayuda = "total enviado al proveedor (solicitado, en recepción y cerrado)",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Fila = 0,
           Columna = 0,
           Formato = enumFormato.Moneda
            )
        ]
        public decimal TotalPedido { get; set; }

        //-----------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Pendiente de servir",
           Tipo = typeof(decimal),
           Ayuda = "total solicitado al proveedor y pendiente de recibir",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Fila = 0,
           Columna = 1,
           Formato = enumFormato.Moneda
            )
        ]
        public decimal Pendiente { get; set; }

        //-----------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Total recibido",
           Tipo = typeof(decimal),
           Ayuda = "total en recepción y cerrado",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Fila = 0,
           Columna = 2,
           Formato = enumFormato.Moneda
            )
        ]
        public decimal Recibido { get; set; }

        //-----------------------------------------------------
        [IUPropiedad(
           Etiqueta = "En cumplimentación",
           Tipo = typeof(decimal),
           Ayuda = "total de los pedidos en cumplimentación",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Fila = 1,
           Columna = 0,
           Formato = enumFormato.Moneda
            )
        ]
        public decimal EnCumplimentacion { get; set; }

        //-----------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Devuelto",
           Tipo = typeof(decimal),
           Ayuda = "total de los pedidos devueltos",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Fila = 1,
           Columna = 1,
           Formato = enumFormato.Moneda
            )
        ]
        public decimal Devuelto { get; set; }

        //-----------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Totales por proveedor",
           Ayuda = "muestra por proveedor lo que está por solicitar, lo solicitado, lo entregado y el total enviado (solicitado + entregado)",
           TipoDeControl = enumTipoControl.AreaDeTexto,
           EditableAlCrear = false,
           EditableAlEditar = false,
           NumeroDeFilas = 8,
           CssDelArea = enumCssControles.MonoSpaceText,
           Fila = 2,
           Columna = 0,
           AutoSpan = true
            )
        ]
        public string TotalesPorProveedor { get; set; }

        //-----------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Totales por naturaleza",
           Ayuda = "muestra por naturaleza de las líneas lo pedido, excluyendo los pedidos cancelados y devueltos",
           TipoDeControl = enumTipoControl.AreaDeTexto,
           EditableAlCrear = false,
           EditableAlEditar = false,
           NumeroDeFilas = 8,
           CssDelArea = enumCssControles.MonoSpaceText,
           Fila = 3,
           Columna = 0,
           AutoSpan = true
            )
        ]
        public string TotalesPorNaturaleza { get; set; }

    }
}
