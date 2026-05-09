using ModeloDeDto.Negocio;
using ModeloDeDto.Terceros;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.Seguridad;
using System;
using Utilidades;

namespace ModeloDeDto.Ventas
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, MostrarExpresion = nameof(IUsaNombreDto.Nombre), EditarTrasCrear = true)]
    public class PlanificacionDeVentaDto : ElementoDeUnProcesoDto
    {

        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "ejecutar el",
            Ayuda = "Fecha de ejecución de la planificación",
            TipoDeControl = enumTipoControl.SelectorDeFecha,
            Formato = enumFormato.Fecha,
            Fila = 1,
            Columna = 2,
            VisibleEnGrid = true,
            Ordenar = true
           )
        ]
        public DateTime EjecutarEl { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
         Etiqueta = "Id del cliente de la planificacion",
         Visible = false
         )]
        public int IdCliente { get; set; }

        [IUPropiedad(
            Etiqueta = "Cliente",
            Ayuda = "A quién se le facturará",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdCliente),
            Controlador = nameof(enumControladoresTerceros.Clientes),
            SeleccionarDe = typeof(ClienteDto),
            VistaDondeNavegar = enumVistasTerceros.CrudClientes,
            RestrictorFijo = ltrParametrosDto.Negocio + ";" + nameof(enumNegocio.Cliente) + ";" + nameof(enumModoDeAccesoDeDatos.Consultor),
            Negocio = enumNegocio.Cliente,
            LongitudMinimaParaBuscar = 1,
            Tipo = typeof(string),
            Fila = 2,
            Columna = 0,
            EditableAlCrear = true,
            EditableAlEditar = false,
            AutoSpan = true)]
        public string Cliente { get; set; }

        //-------------------------------------------------------------------------------------------------------------------
        [IUPropiedad(TipoDeControl = enumTipoControl.Editor,
            Fila = 3, Columna = 0
            , Etiqueta = "Contacto"
            , VisibleAlCrear = false
            , VisibleAlEditar = true
            , VisibleEnGrid = false)]
        public string Contacto { get; set; }

        //-------------------------------------------------------------------------------------------------------------------
        [IUPropiedad(TipoDeControl = enumTipoControl.Editor,
            Fila = 3, Columna = 1
            , Etiqueta = "Teléfono"
            , VisibleAlCrear = false
            , VisibleAlEditar = true
            , VisibleEnGrid = false)]
        public string Telefono { get; set; }

        //-------------------------------------------------------------------------------------------------------------------
        [IUPropiedad(TipoDeControl = enumTipoControl.Editor,
            Fila = 3, Columna = 2
            , Etiqueta = "eMail"
            , VisibleAlCrear = false
            , VisibleAlEditar = true
            , VisibleEnGrid = false)]
        public string eMail { get; set; }

        //----------------------------------------------------------
        [IUPropiedad(Etiqueta = "Planificador",
            Ayuda = "Planificador de la previsión",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Planificador),
            Fila = 4,
            Columna = 0,
            VisibleEnGrid = false,
            EditableAlCrear = false,
            EditableAlEditar = false,
            Obligatorio = false,
            Negocio = enumNegocio.PlanificadorDeVenta,
            Controlador = nameof(enumControladoresJuridicos.PlanificadorDeVentas),
            VistaDondeNavegar = enumVistasJuridicos.CrudPlanificadorDeVentas
            )
        ]
        public int? IdPlanificador { get; set; }

        [IUPropiedad(Visible = false,  Etiqueta = "Contrato", Obligatorio = false)]
        public string Planificador { get; set; }

        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(
              Etiqueta = "Id del tipo del parte de trabajo",
              Visible = false
              )]
        public int? IdTipoDeParte { get; set; }

        [IUPropiedad(
            Etiqueta = "Tipo del parte de trabajo a generar",
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
            Fila = 4,
            Columna = 1,
            VisibleEnGrid = false,
            EditableAlCrear = true,
            EditableAlEditar = true,
            trasSeleccionar = "javascript:" + nameof(enumNameSpaceTs.Venta) + "." + nameof(enumFunctionTs.Plv_Tras_Seleccionar_TipoDeParte) + "([" + nameof(enumParamTs.idLista) + "])",
            trasBlanquear = "javascript:" + nameof(enumNameSpaceTs.Venta) + "." + nameof(enumFunctionTs.Plv_Tras_Blanquear_TipoDeParte) + "([" + nameof(enumParamTs.idLista) + "])",
            AutoSpan = true)]
        public string TipoDeParte { get; set; }


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
            Fila = 4,
            Columna = 2,
            VisibleEnGrid = false,
            EditableAlCrear = true,
            EditableAlEditar = true,
            trasSeleccionar = "javascript:" + nameof(enumNameSpaceTs.Venta) + "." + nameof(enumFunctionTs.Plv_Tras_Seleccionar_TipoDeFactura) + "([" + nameof(enumParamTs.idLista) + "])",
            trasBlanquear = "javascript:" + nameof(enumNameSpaceTs.Venta) + "." + nameof(enumFunctionTs.Plv_Tras_Blanquear_TipoDeFactura) + "([" + nameof(enumParamTs.idLista) + "])",
            AutoSpan = true)]
        public string TipoDeFactura { get; set; }

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
           Fila = 13,
           Columna = 2,
           Posicion = 0,
           MantenerHuecoDeLaIzquierda = true
            )
        ]
        public decimal TotalSinIva { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Total",
           Tipo = typeof(decimal),
           Ayuda = "total a pagar",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           VisibleAlCrear = false,
           EditableAlEditar = false,
           Obligatorio = false,
           Formato = enumFormato.Moneda,
           Fila = 13,
           Columna = 2,
           Posicion = 1
            )
        ]
        public decimal TotalConIva { get; set; }

        //----------------------------------------------------------
        [IUPropiedad(Etiqueta = "Contrato",
            Ayuda = "Contrato asociado a la planificación",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Contrato),
            Fila = 12,
            Columna = 0,
            VisibleEnGrid = false,
            EditableAlCrear = false,
            EditableAlEditar = false,
            Obligatorio = false,
            AutoSpan = true,
            Negocio = enumNegocio.Contrato,
            Controlador = nameof(enumControladoresJuridicos.Contratos),
            VistaDondeNavegar = enumVistasJuridicos.CrudContratos,
            ParametrosParaNavegar = nameof(ltrParametrosEp.Clase) + "=" + nameof(enumClaseDeContrato.Venta)
            )
        ]
        public int? IdContrato { get; set; }

        [IUPropiedad(Visible = false, Etiqueta = "Contrato", Obligatorio = false)]
        public string Contrato { get; set; }


        //----------------------------------------------------------
        [IUPropiedad(Etiqueta = "Parte de trabajo",
            Ayuda = "Parte de trabajo generado",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(ParteDeTrabajo),
            Fila = 12,
            Columna = 1,
            VisibleEnGrid = false,
            EditableAlCrear = false,
            EditableAlEditar = false,
            Obligatorio = false,
            Negocio = enumNegocio.ParteDeTrabajo,
            Controlador = nameof(enumControladoresVentas.PartesTr),
            VistaDondeNavegar = enumVistasVentas.CrudPartesDeTrabajo
            )
        ]
        public int? IdParteTr { get; set; }

        [IUPropiedad(Visible = false, Etiqueta = "Parte de trabajo", Obligatorio = false)]
        public string ParteDeTrabajo { get; set; }

        //----------------------------------------------------------
        [IUPropiedad(Etiqueta = "Factura",
            Ayuda = "Factura emitida",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(FacturaEmt),
            Fila = 12,
            Columna = 2,
            VisibleEnGrid = false,
            EditableAlCrear = false,
            EditableAlEditar = false,
            Obligatorio = false,
            Negocio = enumNegocio.FacturaEmitida,
            Controlador = nameof(enumControladoresVentas.FacturasEmt),
            VistaDondeNavegar = enumVistasVentas.CrudFacturasEmt
            )
        ]
        public int? IdFacturaEmt { get; set; }

        [IUPropiedad(Visible = false, Etiqueta = "Factura emitida", Obligatorio = false)]
        public string FacturaEmt { get; set; }


    }


}
