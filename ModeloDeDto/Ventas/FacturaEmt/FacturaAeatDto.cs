using System;
using Utilidades;

namespace ModeloDeDto.Ventas
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, MostrarExpresion = nameof(FacturaAeatDto.NumeroFactura),SoloGrid = true,  OpcionDeEnviar = false, OpcionDeTransitar = false)]
    public class FacturaAeatDto : ElementoDto
    {
        [IUPropiedad(Visible = false)]
        public string Ano { get; set; }
        
        [IUPropiedad(Visible = false)]
        public string Serie { get; set; }
        
        [IUPropiedad(Visible = false)]
        public string Numero { get; set; }

        [IUPropiedad(Etiqueta = "Nº de factura", 
            Ayuda = "número de factura", 
            VisibleEnGrid = true,
            TipoDeControlEnGrid = enumTipoControl.Referencia,
            AccionRef = "javascript: " + nameof(enumNameSpaceTs.ApiDelCrud) + "." + nameof(enumFunctionTs.Negocio_IrALaFactura) + "(numeroDeFila)",
            Obligatorio = false) ]
        public string NumeroFactura => Ano + "-" + Serie + "-" + Numero;

        [IUPropiedad(Visible = false)]
        public string NifCliente { get; set; }

        [IUPropiedad(Visible = false)]
        public string NombreCliente { get; set; }

        [IUPropiedad(Etiqueta = "Cliente", Ayuda = "Cliente", VisibleEnGrid = true, Obligatorio = false, PorAnchoMnt = 20)]
        public string Cliente => "(" + NifCliente + ") " + NombreCliente;

        [IUPropiedad(Visible = false)]
        public DateTime FechaFactura { get; set; }

        [IUPropiedad(Etiqueta = "Facturada El", Ayuda = "fecha factura", VisibleEnGrid = true, Obligatorio = false)]
        public string FacturadaEl => FechaFactura.ToString("dd-MM-yyyy") ;

        [IUPropiedad(Etiqueta = "BI", Ayuda = "Base imponible", VisibleEnGrid = true, Obligatorio = false)]
        public string BI { get; set; }

        [IUPropiedad(Etiqueta = "Impuestos", Ayuda = "impuestos de la factura", VisibleEnGrid = true, Obligatorio = false)]
        public string Impuestos { get; set; }

        [IUPropiedad(Etiqueta = "Retenciones", Ayuda = "retención de la factura", VisibleEnGrid = false, Obligatorio = false, PosicionEnGrid = 12)]
        public string Retencion { get; set; }

        [IUPropiedad(Etiqueta = "Total a pagar", Ayuda = "total a pagar de la factura", VisibleEnGrid = true, Obligatorio = false)]
        public string Total { get; set; }


        [IUPropiedad(Visible = false)]
        public string TipoAeat { get; set; }

        [IUPropiedad(Visible = false)]
        public string TipoRectificativa { get; set; }

        [IUPropiedad(Etiqueta = "Huella", Ayuda = "huella", VisibleEnGrid = true, Obligatorio = false, PorAnchoMnt = 20)]
        public string Huella { get; set; }
    }
}
