using ServicioDeDatos;
using ServicioDeDatos.Gastos;
using System;
using System.Collections.Generic;
using Utilidades;

namespace ModeloDeDto.Gastos
{

    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class PagoDeUnaRemesaDto : EsUnDetalleDto, IRelacionDto
    {
        public static string ExpresionElemento = nameof(Pago);
        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Elemento",
            Ayuda = "elemento",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Elemento),
            Controlador = nameof(enumControladoresGastos.RemesasPag),
            VistaDondeNavegar = enumVistasGastos.CrudRemesasPag,
            Negocio = enumNegocio.RemesaPag,
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
           Etiqueta = "Acreedor",
           Ayuda = "Acreedor del pago",
           TipoDeControl = enumTipoControl.Editor,
           Fila = 0,
           Columna = 0,
           Posicion = 1,
           VisibleAlCrear = false,
           EditableAlEditar = false,
           Obligatorio = false)
        ]
        public string Acreedor { get; set; }


        //----------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Id del pago",
            Visible = false
            )
        ]
        public int IdPago { get; set; }


        [IUPropiedad(
            Etiqueta = "Pago",
            Ayuda = "seleccione el pago",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(PagoDto),
            GuardarEn = nameof(IdPago),
            RestringidoPorControl = nameof(IdElemento),
            PropiedadRestrictora = ltrDeUnPago.ExcluirPagosDeUnaRemesa,
            Controlador = nameof(enumControladoresGastos.Pagos),
            VistaDondeNavegar = enumVistasGastos.CrudPagos,
            Negocio = enumNegocio.Pago,
            //OnClick = "javascript:" + nameof(enumNameSpaceTs.Venta) + "." + nameof(enumFunctionTs.Rem_Ir_A_Facturas_Remesa) + "()",
            BuscarPor = nameof(PagoDto.Expresion),
            CriterioDeBusqueda = enumCriteriosDeFiltrado.contiene,
            LongitudMinimaParaBuscar = 1,
            MostrarExpresion = nameof(PagoDto.Expresion),
            Fila = 1,
            Columna = 0,
            Ordenar = true,
            EditableAlEditar = false,
            AutoSpan = true
            )
        ]
        public string Pago { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Pagar el"
            , TipoDeControl = enumTipoControl.SelectorDeFecha
            , VisibleEnGrid = true
            , VisibleAlCrear = false
            , EditableAlEditar = false
            , Fila = 2
            , Columna = 0
            , Formato = enumFormato.Fecha
            , Alineada = enumAliniacion.centrada)]
        public DateTime? PagarEl { get; set; }


        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Pagada el"
            , TipoDeControl = enumTipoControl.SelectorDeFecha
            , VisibleEnGrid = true
            , VisibleAlCrear = false
            , EditableAlEditar = false
            , Fila = 2
            , Columna = 1
            , Formato = enumFormato.Fecha
            , Alineada = enumAliniacion.centrada)]
        public DateTime? PagadoEl { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Importe",
           Tipo = typeof(decimal),
           EtiquetaGrid = "Importe",
           Ayuda = "Importe del pago",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           VisibleAlCrear = false,
           EditableAlEditar = false,
           Obligatorio = false,
           Fila = 2,
           Columna = 2,
           MantenerHuecoDeLaIzquierda = true,
           Formato = enumFormato.Moneda
            )
        ]
        public decimal? ImportePago { get; set; }
        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Anulado el"
            , TipoDeControl = enumTipoControl.SelectorDeFecha
            , VisibleEnGrid = true
            , VisibleAlCrear = false
            , EditableAlEditar = true
            , Fila = 3
            , Columna = 0
            , Posicion = 0
            , AnchoMaximo = "18em"
            , Formato = enumFormato.Fecha
            , Alineada = enumAliniacion.centrada)]
        public DateTime? AnuladoEl { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Motivo",
           Ayuda = "Indique el motivo de la devolución o anulación de devolución",
           TipoDeControl = enumTipoControl.Editor,
           Fila = 3,
           Columna = 0,
           Posicion = 1,
           VisibleEnGrid = false,
           VisibleAlCrear = false,
           EditableAlEditar = true,
           Obligatorio = false,
           AutoSpan = true,
           LongitudMaxima = 250)
        ]
        public string Motivo { get; set; }

        //--------------------------------------------
        [IUPropiedad(Visible = false)]
        public List<string> Etapas { get; set; }
        //--------------------------------------------
        [IUPropiedad(Visible = false)]
        public bool EstaPagado { get; set; }
        //--------------------------------------------
        [IUPropiedad(Visible = false)]
        public bool EstaAnulado { get; set; }     
        //--------------------------------------------
        [IUPropiedad(Visible = false)]
        public bool EsInterventor { get; set; }

    }
}
