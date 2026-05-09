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
           Ayuda = "total pedido",
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
           Ayuda = "total pendiente de servir",
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
           Ayuda = "total recibido",
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

    }
}
