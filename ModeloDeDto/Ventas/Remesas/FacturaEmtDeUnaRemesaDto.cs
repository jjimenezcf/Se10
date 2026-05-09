using ServicioDeDatos;
using ServicioDeDatos.Ventas;
using System;
using System.Collections.Generic;
using Utilidades;

namespace ModeloDeDto.Ventas
{

    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class FacturaEmtDeUnaRemesaDto : EsUnDetalleDto, IRelacionDto
    {
        public static string ExpresionElemento = nameof(Factura);
        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Elemento",
            Ayuda = "elemento",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Elemento),
            Controlador = nameof(enumControladoresVentas.RemesasFae),
            VistaDondeNavegar = enumVistasVentas.CrudRemesasFae,
            Negocio = enumNegocio.RemesaFae,
            Fila = 0,
            Columna = 0,
            EditableAlCrear = false,
            EditableAlEditar = false,
            VisibleEnGrid = false,
            AutoSpan = true,
            PorAnchoMnt = 30,
            CriterioDeBusqueda = enumCriteriosDeFiltrado.diferente
            )
        ]
        public override int IdElemento { get; set; }


        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Cliente",
           Ayuda = "Cliente de la factura",
           TipoDeControl = enumTipoControl.Editor,
           Fila = 0,
           Columna = 0,
           Posicion = 1,
           VisibleAlCrear = false,
           EditableAlEditar = false,
           Obligatorio = false)
        ]
        public string Cliente { get; set; }


        //----------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Id del factura",
            Visible = false
            )
        ]
        public int IdFactura { get; set; }


        [IUPropiedad(
            Etiqueta = "Factura",
            Ayuda = "seleccione la factura",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(FacturaEmtDto),
            GuardarEn = nameof(IdFactura),
            RestringidoPorControl = nameof(IdElemento),
            PropiedadRestrictora = ltrDeUnaFacturaEmt.ExcluirFacturasDeUnaRemesa,
            Controlador = nameof(enumControladoresVentas.FacturasEmt),
            VistaDondeNavegar = enumVistasVentas.CrudFacturasEmt,
            Negocio = enumNegocio.FacturaEmitida,
            //OnClick = "javascript:" + nameof(enumNameSpaceTs.Venta) + "." + nameof(enumFunctionTs.Rem_Ir_A_Facturas_Remesa) + "()",
            BuscarPor = nameof(FacturaEmtDto.Expresion),
            CriterioDeBusqueda = enumCriteriosDeFiltrado.contiene,
            LongitudMinimaParaBuscar = 1,
            MostrarExpresion = "[" + nameof(FacturaEmtDto.NumeroFactura)  + "] [" + nameof(FacturaEmtDto.Expresion) + "]",
            Fila = 1,
            Columna = 0,
            Ordenar = true,
            AutoSpan = true
            )
        ]
        public string Factura { get; set; }


        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Cargada el"
            , TipoDeControl = enumTipoControl.SelectorDeFecha
            , VisibleEnGrid = true
            , VisibleAlCrear = false
            , EditableAlEditar = true
            , Fila = 2
            , Columna = 0
            , Formato = enumFormato.Fecha
            , Alineada = enumAliniacion.centrada)]
        public DateTime? CargadaEl { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Devolucion permitida hasta"
            , TipoDeControl = enumTipoControl.SelectorDeFecha
            , VisibleEnGrid = true
            , VisibleAlCrear = false
            , EditableAlEditar = false
            , Fila = 2
            , Columna = 1
            , Formato = enumFormato.Fecha
            , Alineada = enumAliniacion.centrada)]
        public DateTime? FechaMaximaDeDevolucion { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Devuelto el"
            , TipoDeControl = enumTipoControl.SelectorDeFecha
            , VisibleEnGrid = true
            , VisibleAlCrear = false
            , EditableAlEditar = true
            , Fila = 2
            , Columna = 2
            , Formato = enumFormato.Fecha
            , Alineada = enumAliniacion.centrada)]
        public DateTime? DevueltoEl { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Motivo",
           Ayuda = "Indique el motivo de la devolución o anulación de devolución",
           TipoDeControl = enumTipoControl.Editor,
           Fila = 3,
           Columna = 0,
           VisibleEnGrid = false,
           VisibleAlCrear = false,
           EditableAlEditar = true,
           Obligatorio = false,
           AutoSpan = true,
           LongitudMaxima = 250)
        ]
        public string Motivo { get; set; }


        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Importe",
           Tipo = typeof(decimal),
           EtiquetaGrid = "Importe",
           Ayuda = "Importe de la factura",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           VisibleAlCrear = false,
           EditableAlEditar = false,
           Obligatorio = false,
           Fila = 4,
           Columna = 2,
            MantenerHuecoDeLaIzquierda = true,
           Formato = enumFormato.Moneda
            )
        ]
        public decimal? ImporteFactura { get; set; }

        //--------------------------------------------
        [IUPropiedad(Visible = false)]
        public List<string> Etapas { get; set; }

        //--------------------------------------------
        [IUPropiedad(Visible = false)]
        public bool EstaCargada { get; set; }


        //--------------------------------------------
        [IUPropiedad(Visible = false)]
        public bool EsInterventor { get; set; }

    }
}
