using Utilidades;
using System;

namespace ModeloDeDto.Ventas
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, OpcionDeBorrar = false)]
    public class RectificativaEmtDto : EsUnDetalleDto
    {

        //----------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Rectificada",
            Ayuda = "factura rectificada",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Rectificada),
            Controlador = nameof(enumControladoresVentas.FacturasEmt),
            VistaDondeNavegar = enumVistasVentas.CrudFacturasEmt,
            Fila = 1,
            Columna = 0,
            EditableAlCrear = false,
            EditableAlEditar = false,
            VisibleEnGrid = false
            )
        ]
        public int? IdRectificada { get; set; }
        [IUPropiedad(Visible = false)]
        public string Rectificada { get; set; }

        //-----------------------------------------------------
        [IUPropiedad(
          Etiqueta = "Concepto",
          Ayuda = "Concepto rectificado",
          Tipo = typeof(string),
          TipoDeControl = enumTipoControl.Editor,
          Fila = 2,
          Columna = 0,
          Obligatorio = false,
          AutoSpan = true
          )
        ]
        public string Concepto { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Facturada el"
            , TipoDeControl = enumTipoControl.SelectorDeFechaHora
            , VisibleEnGrid = false
            , VisibleAlCrear = false
            , EditableAlEditar = false
            , Fila = 3
            , Columna = 0
            , Posicion = 0
            , Alineada = enumAliniacion.centrada)]
        public DateTime FacturadaEl { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Total",
           Tipo = typeof(decimal),
           Ayuda = "importe de la factura con iva y sin irpf",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Obligatorio = false,
           Fila = 3,
           Columna = 0,
           Posicion = 1,
           Formato = enumFormato.Moneda
            )
        ]
        public decimal TotalAPagar { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Cobrado",
           Tipo = typeof(decimal),
           Ayuda = "importe cobrado",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Obligatorio = false,
           Fila = 3,
           Columna = 1,
           Posicion = 0,
           Formato = enumFormato.Moneda
            )
        ]
        public decimal Cobrado { get; set; }


        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Pendiente",
           Tipo = typeof(decimal),
           Ayuda = "importe pendiente de conbro",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Obligatorio = false,
           Fila = 3,
           Columna = 1,
           Posicion = 1,
           Formato = enumFormato.Moneda
            )
        ]
        public decimal Pendiente { get; set; }

    }
}
