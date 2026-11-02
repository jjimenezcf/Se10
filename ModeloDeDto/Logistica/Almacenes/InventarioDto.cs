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

        // fecha del movimiento del que salen el stock y el precio medio, no la fecha consultada
        [IUPropiedad(Etiqueta = "Realizado el", Ayuda = "fecha del movimiento del que se toman el stock y el precio medio", TipoDeControl = enumTipoControl.SelectorDeFechaHora, Alineada = enumAliniacion.derecha, VisibleEnGrid = true, Obligatorio = false)]
        public DateTime RealizadoEl { get; set; }

        [IUPropiedad(Etiqueta = "Stock", Ayuda = "stock del material a la fecha", TipoDeControl = enumTipoControl.Editor, Alineada = enumAliniacion.derecha, VisibleEnGrid = true, Obligatorio = false, Formato = enumFormato.Numero_6)]
        public decimal Stock { get; set; }

        [IUPropiedad(Etiqueta = "Precio medio", Ayuda = "precio medio ponderado del material en el almacén a la fecha", TipoDeControl = enumTipoControl.Editor, Alineada = enumAliniacion.derecha, VisibleEnGrid = true, Obligatorio = false, Formato = enumFormato.Moneda)]
        public decimal PrecioMedio { get; set; }

        [IUPropiedad(Etiqueta = "Valor", Ayuda = "valor del stock en el almacén (stock por precio medio)", TipoDeControl = enumTipoControl.Editor, Alineada = enumAliniacion.derecha, VisibleEnGrid = true, Obligatorio = false, Formato = enumFormato.Moneda)]
        public decimal Valor { get; set; }

        // precio de entrada de la última compra del material en el almacén hasta la fecha consultada: lo que cuesta comprarlo ahora
        [IUPropiedad(Etiqueta = "Última compra", Ayuda = "precio de la última entrada de compra en el almacén hasta la fecha", TipoDeControl = enumTipoControl.Editor, Alineada = enumAliniacion.derecha, VisibleEnGrid = true, Obligatorio = false, Formato = enumFormato.Moneda)]
        public decimal UltimaCompra { get; set; }

        // coste estimado definido en el maestro del material, no el real
        [IUPropiedad(Etiqueta = "Coste previsto", Ayuda = "coste estimado definido en el material", TipoDeControl = enumTipoControl.Editor, Alineada = enumAliniacion.derecha, VisibleEnGrid = true, Obligatorio = false, Formato = enumFormato.Moneda)]
        public decimal Coste { get; set; }

        [IUPropiedad(Etiqueta = "Precio de venta", Ayuda = "precio de venta definido en el material", TipoDeControl = enumTipoControl.Editor, Alineada = enumAliniacion.derecha, VisibleEnGrid = true, Obligatorio = false, Formato = enumFormato.Moneda)]
        public decimal Venta { get; set; }

        // (venta - precio medio) / venta, en porcentaje; 0 si el material no tiene precio de venta
        [IUPropiedad(Etiqueta = "%V", Ayuda = "(venta − valor de inventario) / venta", TipoDeControl = enumTipoControl.Editor, Alineada = enumAliniacion.derecha, VisibleEnGrid = true, Obligatorio = false, Formato = enumFormato.Numero_2)]
        public decimal MargenSobreValor { get; set; }

        // (venta - última compra) / venta, en porcentaje; 0 si no tiene precio de venta o no hay compras
        [IUPropiedad(Etiqueta = "%C", Ayuda = "(venta − última compra) / venta", TipoDeControl = enumTipoControl.Editor, Alineada = enumAliniacion.derecha, VisibleEnGrid = true, Obligatorio = false, Formato = enumFormato.Numero_2)]
        public decimal MargenSobreCompra { get; set; }
    }
}
