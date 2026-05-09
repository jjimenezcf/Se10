using System;
using Utilidades;

namespace ModeloDeDto.Ventas
{
    public class LogDeEnvioDeFacturaDto : ElementoDto
    {
        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Factura",
            Ayuda = "Factura enviada o por enviar a la AEAT",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Factura),
            Fila = 0,
            Columna = 0,
            Controlador = nameof(enumControladoresVentas.FacturasEmt),
            VistaDondeNavegar = enumVistasVentas.CrudFacturasEmt,
            EditableAlCrear = false,
            EditableAlEditar = false,
            AutoSpan = true
            )
        ]
        public int IdFactura { get; set; }

        [IUPropiedad(Visible = false)]
        public string Factura { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
              PorAnchoMnt = 15
            , Etiqueta = "Generada el"
            , Ayuda = "Indica cuando se creo la entrada en el log de envío"
            , Ordenar = true
            , OrdenarGridPor = nameof(GeneradaEl)
            , TipoDeControl = enumTipoControl.SelectorDeFecha
            , VisibleEnGrid = true
            , EditableAlCrear = false
            , EditableAlEditar = false
            , Fila = 1
            , Columna = 0
            , Alineada = enumAliniacion.centrada)]
        public DateTime GeneradaEl { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
              PorAnchoMnt = 15
            , Etiqueta = "Enviada el"
            , Ayuda = "Indica cuando se registro el envío con respuesta en la AEAT"
            , Ordenar = true
            , OrdenarGridPor = nameof(EnviadaEl)
            , TipoDeControl = enumTipoControl.SelectorDeFecha
            , VisibleEnGrid = true
            , EditableAlCrear = false
            , EditableAlEditar = false
            , Fila = 1
            , Columna = 0
            , Alineada = enumAliniacion.centrada)]
        public DateTime? EnviadaEl { get; set; }

    }
}
