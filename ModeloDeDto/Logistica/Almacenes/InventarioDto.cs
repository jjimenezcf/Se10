using System;
using Utilidades;

namespace ModeloDeDto.Logistica
{
    // Inventario de un almacén a una fecha: cada fila es el último movimiento de un unitario en el almacén hasta esa fecha
    // (ver GestorDeInventario), con el stock y el precio medio que dejó y los datos del unitario
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, MostrarExpresion = nameof(InventarioDto.Unitario), SoloGrid = true, OpcionDeEnviar = false, OpcionDeTransitar = false)]
    public class InventarioDto : ElmentoAuditadoDto
    {
        // nombre del unitario; al pulsarlo se navega a su edición
        [IUPropiedad(Etiqueta = "Material", Ayuda = "material inventariado", VisibleEnGrid = true, Obligatorio = false, PorAnchoMnt = 20
            , TipoDeControl = enumTipoControl.Referencia
            , AccionRef = "javascript: " + nameof(enumNameSpaceTs.Logistica) + "." + nameof(enumFunctionTs.Inv_IrAlUnitario) + "(numeroDeFila)")]
        public string Unitario { get; set; }

        [IUPropiedad(Etiqueta = "Referencia", Ayuda = "referencia del material", VisibleEnGrid = true, Obligatorio = false)]
        public string Referencia { get; set; }

        [IUPropiedad(Etiqueta = "Naturaleza", Ayuda = "naturaleza contable del material", VisibleEnGrid = true, Obligatorio = false)]
        public string Naturaleza { get; set; }

        [IUPropiedad(Etiqueta = "Unidad", Ayuda = "unidad de medida del material", VisibleEnGrid = true, Obligatorio = false)]
        public string Unidad { get; set; }

        [IUPropiedad(Etiqueta = "Stock", Ayuda = "stock del material a la fecha", TipoDeControl = enumTipoControl.Editor, Alineada = enumAliniacion.derecha, VisibleEnGrid = true, Obligatorio = false, Formato = enumFormato.Numero_6)]
        public decimal Stock { get; set; }

        [IUPropiedad(Etiqueta = "Precio medio", Ayuda = "precio medio ponderado del material en el almacén a la fecha", TipoDeControl = enumTipoControl.Editor, Alineada = enumAliniacion.derecha, VisibleEnGrid = true, Obligatorio = false, Formato = enumFormato.Moneda)]
        public decimal PrecioMedio { get; set; }

        [IUPropiedad(Etiqueta = "Valor", Ayuda = "valor del stock en el almacén (stock por precio medio)", TipoDeControl = enumTipoControl.Editor, Alineada = enumAliniacion.derecha, VisibleEnGrid = true, Obligatorio = false, Formato = enumFormato.Moneda)]
        public decimal Valor { get; set; }

        [IUPropiedad(Etiqueta = "Coste", Ayuda = "coste definido en el material", TipoDeControl = enumTipoControl.Editor, Alineada = enumAliniacion.derecha, VisibleEnGrid = true, Obligatorio = false, Formato = enumFormato.Moneda)]
        public decimal Coste { get; set; }

        [IUPropiedad(Etiqueta = "Precio de venta", Ayuda = "precio de venta definido en el material", TipoDeControl = enumTipoControl.Editor, Alineada = enumAliniacion.derecha, VisibleEnGrid = true, Obligatorio = false, Formato = enumFormato.Moneda)]
        public decimal Venta { get; set; }

        // (venta - precio medio) / venta, en porcentaje; 0 si el material no tiene precio de venta
        [IUPropiedad(Etiqueta = "Margen %", Ayuda = "margen del precio medio sobre el precio de venta", TipoDeControl = enumTipoControl.Editor, Alineada = enumAliniacion.derecha, VisibleEnGrid = true, Obligatorio = false, Formato = enumFormato.Numero_2)]
        public decimal Margen { get; set; }

        [IUPropiedad(Etiqueta = "Último movimiento", Ayuda = "fecha del último movimiento del material hasta la fecha consultada", TipoDeControl = enumTipoControl.SelectorDeFechaHora, Alineada = enumAliniacion.derecha, VisibleEnGrid = true, Obligatorio = false)]
        public DateTime RealizadoEl { get; set; }
    }
}
