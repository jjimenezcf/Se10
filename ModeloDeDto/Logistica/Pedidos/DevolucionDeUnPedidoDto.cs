using System;
using Utilidades;

namespace ModeloDeDto.Logistica
{
    // Devoluciones de las entregas de un pedido, para el grid de devoluciones de la edición del pedido (ver GestorDeDevolucionesDeUnPedido).
    // IdElemento es el pedido; desde cada devolución se consulta o se cancela
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, MostrarExpresion = nameof(Concepto))]
    public class DevolucionDeUnPedidoDto : EsUnDetalleDto
    {
        [IUPropiedad(Etiqueta = "Id de la entrega", Visible = false)]
        public int IdLineaDeRecepcion { get; set; }

        [IUPropiedad(Etiqueta = "Devuelto el", Formato = enumFormato.Fecha, VisibleEnGrid = true, Obligatorio = false)]
        public DateTime DevueltaEl { get; set; }

        [IUPropiedad(Etiqueta = "Concepto", VisibleEnGrid = true, Obligatorio = false)]
        public string Concepto { get; set; }

        [IUPropiedad(Etiqueta = "Cantidad devuelta", Formato = enumFormato.Numero_6, VisibleEnGrid = true, Obligatorio = false)]
        public decimal Cantidad { get; set; }

        [IUPropiedad(Etiqueta = "Precio de devolución", Formato = enumFormato.Moneda, VisibleEnGrid = true, Obligatorio = false)]
        public decimal Precio { get; set; }

        [IUPropiedad(Etiqueta = "Descuento aplicado", Formato = enumFormato.Porcentaje, VisibleEnGrid = true, Obligatorio = false)]
        public decimal? Descuento { get; set; }

        [IUPropiedad(Etiqueta = "Importe de devolución", Formato = enumFormato.Moneda, VisibleEnGrid = true, Obligatorio = false)]
        public decimal Importe { get; set; }

        // para consultarla en la modal de devolución
        [IUPropiedad(Etiqueta = "Motivo", Visible = false)]
        public string Motivo { get; set; }

        [IUPropiedad(Etiqueta = "Id del almacén", Visible = false)]
        public int? IdAlmacen { get; set; }

        [IUPropiedad(Etiqueta = "Almacén", Visible = false)]
        public string Almacen { get; set; }

        [IUPropiedad(Etiqueta = "Id del expediente", Visible = false)]
        public int? IdExpediente { get; set; }

        [IUPropiedad(Etiqueta = "Expediente", Visible = false)]
        public string Expediente { get; set; }

        // con el preasiento de su salida del almacén contabilizado no se puede cancelar
        [IUPropiedad(Etiqueta = "Contabilizada", Visible = false)]
        public bool Contabilizada { get; set; }
    }
}
