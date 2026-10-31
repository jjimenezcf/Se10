using System;
using Utilidades;

namespace ModeloDeDto.Logistica
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, MostrarExpresion = nameof(MovimientoDeAlmacenDto.TipoMovimiento), SoloGrid = true, OpcionDeEnviar = false, OpcionDeTransitar = false)]
    public class MovimientoDeAlmacenDto : ElmentoAuditadoDto
    {
        [IUPropiedad(Etiqueta = "Tipo de movimiento", Ayuda = "tipo de movimiento de almacén", VisibleEnGrid = true, Obligatorio = false)]
        public string TipoMovimiento { get; set; }

        [IUPropiedad(Etiqueta = "Material", Ayuda = "material del movimiento", VisibleEnGrid = true, Obligatorio = false, PorAnchoMnt = 20)]
        public string Unitario { get; set; }

        [IUPropiedad(Etiqueta = "Cantidad", Ayuda = "cantidad del movimiento", TipoDeControl = enumTipoControl.Editor, Alineada = enumAliniacion.derecha, VisibleEnGrid = true, Obligatorio = false, Formato = enumFormato.Numero_6)]
        public decimal Cantidad { get; set; }

        [IUPropiedad(Etiqueta = "Precio", Ayuda = "precio del movimiento", TipoDeControl = enumTipoControl.Editor, Alineada = enumAliniacion.derecha, VisibleEnGrid = true, Obligatorio = false, Formato = enumFormato.Moneda)]
        public decimal Precio { get; set; }

        [IUPropiedad(Etiqueta = "Stock", Ayuda = "stock resultante del movimiento", TipoDeControl = enumTipoControl.Editor, Alineada = enumAliniacion.derecha, VisibleEnGrid = true, Obligatorio = false, Formato = enumFormato.Numero_6)]
        public decimal Stock { get; set; }

        [IUPropiedad(Etiqueta = "Valor", Ayuda = "valor del movimiento", TipoDeControl = enumTipoControl.Editor, Alineada = enumAliniacion.derecha, VisibleEnGrid = true, Obligatorio = false, Formato = enumFormato.Moneda)]
        public decimal Valor { get; set; }

        // Documento (pedido, regularización...) que generó el movimiento, entrada o salida; al pulsarlo se navega a su edición
        // (ver GestorDeMovimientosDeAlmacen.DespuesDeMapearElElemento)
        [IUPropiedad(Etiqueta = "Origen", Ayuda = "operación que generó el movimiento", VisibleEnGrid = true, Obligatorio = false
            , TipoDeControl = enumTipoControl.Referencia
            , AccionRef = "javascript: " + nameof(enumNameSpaceTs.Logistica) + "." + nameof(enumFunctionTs.Mal_IrAlDocumento) + "(numeroDeFila)")]
        public string Origen { get; set; }

        // Proveedor del pedido si el movimiento es la entrada de una recepción, vacío en el resto (ver GestorDeMovimientosDeAlmacen.DespuesDeMapearElElemento)
        [IUPropiedad(Etiqueta = "Proveedor", Ayuda = "proveedor del pedido recepcionado", VisibleEnGrid = true, Obligatorio = false
            , TipoDeControl = enumTipoControl.Referencia
            , AccionRef = "javascript: " + nameof(enumNameSpaceTs.Logistica) + "." + nameof(enumFunctionTs.Mal_IrAlProveedor) + "(numeroDeFila)")]
        public string Proveedor { get; set; }

        // Referencia del preasiento contable del movimiento, vacía si no lo tiene (ver GestorDeMovimientosDeAlmacen.DespuesDeMapearElElemento)
        [IUPropiedad(Etiqueta = "Preasiento", Ayuda = "preasiento contable del movimiento", VisibleEnGrid = true, Obligatorio = false
            , TipoDeControl = enumTipoControl.Referencia
            , AccionRef = "javascript: " + nameof(enumNameSpaceTs.Logistica) + "." + nameof(enumFunctionTs.Mal_IrAlPreasiento) + "(numeroDeFila)")]
        public string Preasiento { get; set; }

        [IUPropiedad(Etiqueta = "Realizado el", Ayuda = "fecha en la que se realizó el movimiento", TipoDeControl = enumTipoControl.SelectorDeFechaHora, Alineada = enumAliniacion.derecha, VisibleEnGrid = true, Obligatorio = false)]
        public DateTime RealizadoEl { get; set; }
    }
}
