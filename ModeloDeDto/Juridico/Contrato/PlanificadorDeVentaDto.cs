using System;
using ModeloDeDto.Negocio;
using ModeloDeDto.Terceros;
using ServicioDeDatos.Juridico;
using Utilidades;
using static ServicioDeDatos.Elemento.Enumerados;

namespace ModeloDeDto.Juridico
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, MostrarExpresion = "Nombre", EditarTrasCrear = true)]
    public class PlanificadorDeVentaDto : ElmentoAuditadoDto
    {
        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = nameof(Contrato),
            Ayuda = "Planificador de ventas de un contrato",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Contrato),
            Fila = 0,
            Columna = 0,
            VisibleEnGrid = false,
            Controlador = nameof(enumControladoresJuridicos.Contratos),
            VistaDondeNavegar = enumVistasJuridicos.CrudContratos,
            ParametrosParaNavegar = nameof(ltrParametrosEp.Clase) + "=" + nameof(enumClaseDeContrato.Venta)
            )
        ]
        public int IdContrato { get; set; }

        [IUPropiedad(Etiqueta = "Contrato", Visible = false)]
        public string Contrato { get; set; }


        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Planificador",
            Ayuda = "Indique el nombre del planificador",
            Tipo = typeof(string),
            Fila = 0,
            Columna = 1,
            Ordenar = true,
            PorAnchoMnt = 50,
            Obligatorio = true,
            LongitudMaxima = 250
          )
        ]
        public string Nombre { get; set; }


        //-----------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "",
            Oculto = true,
            Obligatorio = false,
            VisibleEnGrid = false,
            Fila = 4,
            Columna = 0,
            Posicion = 0
            )]
        public int? IdSociedadDelCg { get; set; }

        [IUPropiedad(
         Etiqueta = "Id del Cg",
         Visible = false
         )]
        public int IdCgDeLaPlanificacion { get; set; }

        [IUPropiedad(
            Etiqueta = "CG de la Planificacion",
            Ayuda = "Centro gestor al que se le genera la planificación",
            TipoDeControl = enumTipoControl.ListaDinamica,
            Negocio = enumNegocio.CentroGestor,
            GuardarEn = nameof(IdCgDeLaPlanificacion),
            Controlador = nameof(enumControladoresTerceros.CentrosGestores),
            VistaDondeNavegar = enumVistasTerceros.CrudCentrosGestores,
            MostrarExpresion = nameof(CentroGestorDto.Expresion),
            RestringidoPorControl = nameof(IdSociedadDelCg),
            PropiedadRestrictora = nameof(IdSociedadDelCg),
            SoloEnAlta = true,
            LongitudMinimaParaBuscar = 1,
            VisibleEnGrid =false,
            Fila = 4,
            Columna = 0,
            EditableAlCrear = true,
            EditableAlEditar = true,
            Posicion = 1)]
        public string CgDeLaPlanificacion { get; set; }

        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(
              Etiqueta = "Id del tipo de la planificación",
              Visible = false
              )]
        public int IdTipoDePlanificacion { get; set; }

        [IUPropiedad(
            Etiqueta = "Tipo de planificación",
            Ayuda = "Tipo de la planificación a generar",
            TipoDeControl = enumTipoControl.ListaDinamica,
            Negocio = enumNegocio.PlanificacionDeVenta,
            GuardarEn = nameof(IdTipoDePlanificacion),
            Controlador = nameof(enumControladoresNegocio.TiposDeElemento),
            VistaDondeNavegar = enumVistasVentas.TiposDePlanificacionDeVenta,
            MostrarExpresion = nameof(TipoDeElementoDto.Nombre),
            SoloEnAlta = true,
            LongitudMinimaParaBuscar = 1,
            Fila = 4,
            Columna = 1,
            VisibleEnGrid = false,
            EditableAlCrear = true,
            EditableAlEditar = true,
            AutoSpan = true)]
        public string TipoDePlanificacion { get; set; }

        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(
              Etiqueta = "Id del tipo de factura emitida",
              Visible = false
              )]
        public int? IdTipoDeFactura { get; set; }

        [IUPropiedad(
            Etiqueta = "Tipo de factura emitir",
            Ayuda = "Tipo de la factura que se ha de emitir",
            TipoDeControl = enumTipoControl.ListaDinamica,
            Negocio = enumNegocio.FacturaEmitida,
            GuardarEn = nameof(IdTipoDeFactura),
            Controlador = nameof(enumControladoresNegocio.TiposDeElemento),
            VistaDondeNavegar = enumVistasVentas.TiposDeFacturaEmt,
            MostrarExpresion = nameof(TipoDeElementoDto.Nombre),
            SoloEnAlta = true,
            LongitudMinimaParaBuscar = 1,
            Obligatorio = false,
            Fila = 5,
            Columna = 1,
            VisibleEnGrid = false,
            EditableAlCrear = true,
            EditableAlEditar = true,
            trasSeleccionar = "javascript:" + nameof(enumNameSpaceTs.Juridico) + "." + nameof(enumFunctionTs.Plv_Tras_Seleccionar_TipoDeFactura) + "([" + nameof(enumParamTs.idLista) + "])",
            trasBlanquear = "javascript:" + nameof(enumNameSpaceTs.Juridico) + "." + nameof(enumFunctionTs.Plv_Tras_Blanquear_TipoDeFactura) + "([" + nameof(enumParamTs.idLista) + "])",
            AutoSpan = true)]
        public string TipoDeFactura { get; set; }

        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(
              Etiqueta = "Id del tipo del parte de trabajo",
              Visible = false
              )]
        public int? IdTipoDeParte { get; set; }

        [IUPropiedad(
            Etiqueta = "Tipo del P.T. a generar",
            Ayuda = "Tipo del parte de trabajo a generar",
            TipoDeControl = enumTipoControl.ListaDinamica,
            Negocio = enumNegocio.ParteDeTrabajo,
            GuardarEn = nameof(IdTipoDeParte),
            //SeleccionarDe = typeof(TipoDeParteTrDto),
            Controlador = nameof(enumControladoresNegocio.TiposDeElemento),
            VistaDondeNavegar = enumVistasVentas.TiposDeParteTr,
            MostrarExpresion = nameof(TipoDeElementoDto.Nombre),
            SoloEnAlta = true,
            LongitudMinimaParaBuscar = 1,
            Obligatorio = false,
            Fila = 5,
            Columna = 0,
            VisibleEnGrid = false,
            EditableAlCrear = true,
            EditableAlEditar = true,
            trasSeleccionar = "javascript:" + nameof(enumNameSpaceTs.Juridico) + "." + nameof(enumFunctionTs.Plv_Tras_Seleccionar_TipoDeParte) + "([" + nameof(enumParamTs.idLista) + "])",
            trasBlanquear = "javascript:" + nameof(enumNameSpaceTs.Juridico) + "." + nameof(enumFunctionTs.Plv_Tras_Blanquear_TipoDeParte) + "([" + nameof(enumParamTs.idLista) + "])",
            AutoSpan = true)]
        public string TipoDeParte { get; set; }


        //----------------------------------------------
        [IUPropiedad(Etiqueta = "Id del lote del contrato", Visible = false)]
        public int? IdLote { get; set; }

        [IUPropiedad(
            Etiqueta = "Lote",
            Ayuda = "Si el contrato usa lotes, seleccione uno",
            TipoDeControl = enumTipoControl.ListaDeElemento,
            SeleccionarDe = typeof(LoteDeUnContratoDto),
            Controlador = nameof(enumControladoresJuridicos.LotesDeUnContrato),
            GuardarEn = nameof(IdLote),
            Obligatorio = false,
            RestringidoPorControl = nameof(IdContrato),
            MostrarExpresion = nameof(LoteDeUnContratoDtm.Expresion),
            EditableAlCrear = true,
            EditableAlEditar = true,
            Fila = 6,
            Columna = 0,
            Posicion =0,
            OnChange = "javascript:" + nameof(enumNameSpaceTs.Juridico) + "." + nameof(enumFunctionTs.Plv_Tras_Seleccionar_Lote) + "()"
            )
        ]
        public string Lote { get; set; }

        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Fecha de comienzo",
            Ayuda = "Inicio de comienzo del planificador del contrato",
            TipoDeControl = enumTipoControl.SelectorDeFecha,
            Fila = 6,
            Columna = 1,
            Posicion =0
           )
        ]
        public DateTime Inicio { get; set; }

        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Fecha de fin",
            Ayuda = "fin de terminación del planificador del contrato",
            TipoDeControl = enumTipoControl.SelectorDeFecha,
            Fila = 6,
            Columna = 1,
            Posicion = 0
           )
        ]
        public DateTime Hasta { get; set; }

        //--------------------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Repetir cada",
           Ayuda = "Valor numérico de cada cuanto se ha de generar un planificación",
           TipoDeControl = enumTipoControl.Editor,
           Fila = 6,
           Columna = 0,
           Posicion = 1,
           VisibleEnGrid = false,
           EditableAlCrear = true,
           EditableAlEditar = true,
           ValorPorDefecto = 1
           )
        ]
        public int RepetirCada { get; set; }

        //------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Periodicidad",
            Ayuda = "Indique como medir el número indicado en las repeticiones",
            TipoDeControl = enumTipoControl.Enumerado,
            Tipo = typeof(enumPeriodicidad),
            GuardarEn = nameof(Periodicidad),
            Fila =6,
            Columna = 0,
            Posicion = 2,
            VisibleEnGrid = false,
            EditableAlCrear = true,
            EditableAlEditar = true
          )
        ]
        public enumPeriodicidad Periodicidad { get; set; }


        //----------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Generado",
            Ayuda = "indica si se el planificador está generado",
            Fila = 8,
            Columna = 0,
            TipoDeControl = enumTipoControl.Check,
            CssDelContenedor = enumCssControles.ContenedorCheck,
            css = enumCssControles.ControlApilado,
            Alineada = enumAliniacion.derecha,
            MantenerHuecoDeLaIzquierda = true,
            VisibleAlCrear = false,
            EditableAlEditar = false
            )
        ]
        public bool Generado { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "B.I.",
           Tipo = typeof(decimal),
           Ayuda = "base imponible",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           VisibleAlCrear = false,
           EditableAlEditar = false,
           Obligatorio = false,
           Formato = enumFormato.Moneda,
           Fila = 8,
           Columna = 1,
           Posicion = 0,
           MantenerHuecoDeLaIzquierda = true
            )
        ]
        public decimal TotalSinIva { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Total",
           Tipo = typeof(decimal),
           Ayuda = "importe de lo palnificado con iva",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           VisibleAlCrear = false,
           EditableAlEditar = false,
           Obligatorio = false,
           Formato = enumFormato.Moneda,
           Fila = 8,
           Columna = 1,
           Posicion = 1
            )
        ]
        public decimal TotalConIva { get; set; }

    }
}
