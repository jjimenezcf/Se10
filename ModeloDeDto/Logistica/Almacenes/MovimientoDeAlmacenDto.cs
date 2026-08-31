using System;
using Utilidades;

namespace ModeloDeDto.Logistica
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, MostrarExpresion = nameof(MovimientoDeAlmacenDto.TipoMovimiento), SoloGrid = true, OpcionDeEnviar = false, OpcionDeTransitar = false)]
    public class MovimientoDeAlmacenDto : ElmentoAuditadoDto
    {
        [IUPropiedad(Etiqueta = "Tipo de movimiento", Ayuda = "tipo de movimiento de almacén", VisibleEnGrid = true, Obligatorio = false)]
        public string TipoMovimiento { get; set; }

        [IUPropiedad(Etiqueta = "Unitario", Ayuda = "unitario del movimiento", VisibleEnGrid = true, Obligatorio = false, PorAnchoMnt = 20)]
        public string Unitario { get; set; }

        [IUPropiedad(Etiqueta = "Cantidad", Ayuda = "cantidad del movimiento", TipoDeControl = enumTipoControl.Editor, Alineada = enumAliniacion.derecha, VisibleEnGrid = true, Obligatorio = false, Formato = enumFormato.Numero_6)]
        public decimal Cantidad { get; set; }

        [IUPropiedad(Etiqueta = "Precio", Ayuda = "precio del movimiento", TipoDeControl = enumTipoControl.Editor, Alineada = enumAliniacion.derecha, VisibleEnGrid = true, Obligatorio = false, Formato = enumFormato.Moneda)]
        public decimal Precio { get; set; }

        [IUPropiedad(Etiqueta = "Stock", Ayuda = "stock resultante del movimiento", TipoDeControl = enumTipoControl.Editor, Alineada = enumAliniacion.derecha, VisibleEnGrid = true, Obligatorio = false, Formato = enumFormato.Numero_6)]
        public decimal Stock { get; set; }

        [IUPropiedad(Etiqueta = "Valor", Ayuda = "valor del movimiento", TipoDeControl = enumTipoControl.Editor, Alineada = enumAliniacion.derecha, VisibleEnGrid = true, Obligatorio = false, Formato = enumFormato.Moneda)]
        public decimal Valor { get; set; }

        [IUPropiedad(Etiqueta = "Destino", Ayuda = "destino del movimiento", VisibleEnGrid = true, Obligatorio = false)]
        public string Destino { get; set; }

        [IUPropiedad(Etiqueta = "Origen", Ayuda = "origen del movimiento", VisibleEnGrid = true, Obligatorio = false)]
        public string Origen { get; set; }

        [IUPropiedad(Etiqueta = "Realizado el", Ayuda = "fecha en la que se realizó el movimiento", TipoDeControl = enumTipoControl.SelectorDeFechaHora, Alineada = enumAliniacion.derecha, VisibleEnGrid = true, Obligatorio = false)]
        public DateTime RealizadoEl { get; set; }
    }
}
