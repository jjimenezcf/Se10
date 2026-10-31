using System;
using Utilidades;

namespace ModeloDeDto.Logistica
{
    // Entregas (recepciones) de las líneas de un pedido, para el grid de entregas de la edición del pedido (ver GestorDeEntregasDeUnPedido).
    // IdElemento es el pedido; desde cada entrega se consulta, se cancela o se devuelve
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, MostrarExpresion = nameof(Concepto))]
    public class EntregaDeUnPedidoDto : EsUnDetalleDto
    {
        [IUPropiedad(Etiqueta = "Id de la línea del pedido", Visible = false)]
        public int IdLinea { get; set; }

        [IUPropiedad(Etiqueta = "Recibido el", Formato = enumFormato.Fecha, VisibleEnGrid = true, Obligatorio = false)]
        public DateTime RecepcionadaEl { get; set; }

        [IUPropiedad(Etiqueta = "Concepto", VisibleEnGrid = true, Obligatorio = false)]
        public string Concepto { get; set; }

        [IUPropiedad(Etiqueta = "Cantidad recibida", Formato = enumFormato.Numero_6, VisibleEnGrid = true, Obligatorio = false)]
        public decimal Cantidad { get; set; }

        [IUPropiedad(Etiqueta = "Precio de recepción", Formato = enumFormato.Moneda, VisibleEnGrid = true, Obligatorio = false)]
        public decimal Precio { get; set; }

        [IUPropiedad(Etiqueta = "Descuento aplicado", Formato = enumFormato.Porcentaje, VisibleEnGrid = true, Obligatorio = false)]
        public decimal? Descuento { get; set; }

        [IUPropiedad(Etiqueta = "Importe de recepción", Formato = enumFormato.Moneda, VisibleEnGrid = true, Obligatorio = false)]
        public decimal Importe { get; set; }

        // destino de la entrega, para consultarla en la modal de recepción
        [IUPropiedad(Etiqueta = "Id del almacén", Visible = false)]
        public int? IdAlmacen { get; set; }

        [IUPropiedad(Etiqueta = "Almacén", Visible = false)]
        public string Almacen { get; set; }

        [IUPropiedad(Etiqueta = "Id del expediente", Visible = false)]
        public int? IdExpediente { get; set; }

        [IUPropiedad(Etiqueta = "Expediente", Visible = false)]
        public string Expediente { get; set; }

        // lo ya devuelto: mientras no se haya devuelto todo se puede devolver; con devoluciones no se puede cancelar
        [IUPropiedad(Etiqueta = "Cantidad devuelta", Visible = false)]
        public decimal CantidadDevuelta { get; set; }

        // con el preasiento de su entrada de compra contabilizado no se puede cancelar
        [IUPropiedad(Etiqueta = "Contabilizada", Visible = false)]
        public bool Contabilizada { get; set; }
    }
}
