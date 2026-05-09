using ServicioDeDatos.Ventas;
using System;
using Utilidades;

namespace ModeloDeDto.Ventas
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, MostrarExpresion = nameof(PeticionDeFacturaEmtDto.NumeroFactura), SoloGrid = true, OpcionDeEnviar = false, OpcionDeTransitar = false)]
    public class PeticionDeFacturaEmtDto : ElementoDto
    {
        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
              PorAnchoMnt = 15
            , Etiqueta = "Solicitada el"
            , Ordenar = true
            , OrdenarGridPor = nameof(SolicitadaEl)
            , TipoDeControl = enumTipoControl.SelectorDeFechaHora
            , VisibleEnGrid = true
            , Alineada = enumAliniacion.centrada)]
        public DateTime SolicitadaEl { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
              Etiqueta = "Peticion",
              TipoDeControl = enumTipoControl.Enumerado,
              Tipo = typeof(enumOperacionFacturador)
            , VisibleEnGrid = true)]
        public enumOperacionFacturador Peticion { get; set; }

        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Facturador",
            TipoDeControl = enumTipoControl.Editor,
            VisibleEnGrid = true
            )
        ]
        public string Facturador { get; set; }

        //----------------------------------------------
        [IUPropiedad(Visible = false)]
        public int? IdFactura { get; set; }

        [IUPropiedad(Etiqueta = "Nº de factura",
            Ayuda = "número de factura",
            VisibleEnGrid = true,
            TipoDeControlEnGrid = enumTipoControl.Referencia,
            AccionRef = "javascript: " + nameof(enumNameSpaceTs.Venta) + "." + nameof(enumFunctionTs.Fae_IrALaFacturaDelTercero) + "(numeroDeFila)",
            Obligatorio = false)]
        public string NumeroFactura { get; set; }

        //----------------------------------------------
        [IUPropiedad(Visible = false)]
        public string Mensaje { get; set; }
    }
}
