using ModeloDeDto.Terceros;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Ventas;
using Utilidades;

namespace ModeloDeDto.Ventas
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, MostrarExpresion = nameof(IUsaNombreDto.Nombre), EditarTrasCrear = true)]
    public class ParteTrDto : ElementoDeUnProcesoDto
    {

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
         Etiqueta = "Id del cliente del parte de trabajo",
         Visible = false
         )]
        public int IdCliente { get; set; }

        [IUPropiedad(
            Etiqueta = "Cliente",
            Ayuda = "A quién se le presenta el parte de trabajo",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdCliente),
            Controlador = nameof(enumControladoresTerceros.Clientes),
            VistaDondeNavegar = enumVistasTerceros.CrudClientes,
            SeleccionarDe = typeof(ClienteDto),
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

        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Presupuesto",
            Ayuda = "parte de trabajo sobre el presupuesto",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Presupuesto),
            Controlador = nameof(enumControladoresVentas.Presupuestos),
            VistaDondeNavegar = enumVistasVentas.CrudPresupuestos,
            Fila = 12,
            Columna = 0,
            EditableAlCrear = false,
            EditableAlEditar = false,
            VisibleEnGrid = false
            )
        ]
        public int? IdPresupuesto { get; set; }
        [IUPropiedad(Visible = false)]
        public string Presupuesto { get; set; }


        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "factura",
            Ayuda = "factura que incluye el parte de trabajo",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            Controlador = nameof(enumControladoresVentas.FacturasEmt),
            VistaDondeNavegar = enumVistasVentas.CrudFacturasEmt,
            MostrarExpresion = nameof(FacturaEmt),
            Fila = 12,
            Columna = 1,
            VisibleAlCrear = false,
            EditableAlEditar = false,
            VisibleEnGrid = false
            )
        ]
        public int? IdFacturaEmt { get; set; }

        [IUPropiedad(Visible = false)]
        public string FacturaEmt { get; set; }


        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Contrato",
            Ayuda = "contrato asociado",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Contrato),
            Controlador = nameof(enumControladoresJuridicos.Contratos),
            VistaDondeNavegar = enumVistasJuridicos.CrudContratos,
            ParametrosParaNavegar = nameof(ltrParametrosEp.Clase) + "=" + nameof(enumClaseDeContrato.Venta),
            Fila = 12,
            Columna = 2,
            EditableAlCrear = false,
            EditableAlEditar = false,
            VisibleEnGrid = false
            )
        ]
        public int? IdContrato { get; set; }

        [IUPropiedad(Visible = false)]
        public string Contrato { get; set; }

        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Planificacion",
            Ayuda = "Planificación de venta",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(PlfDeVenta),
            Controlador = nameof(enumControladoresVentas.PlanificacionesDeVenta),
            VistaDondeNavegar = enumVistasVentas.CrudPlanificacionesDeVenta,
            Fila = 13,
            Columna = 0,
            VisibleAlCrear = false,
            EditableAlEditar = false,
            VisibleEnGrid = false
            )
        ]
        public int? IdPlfDeVenta { get; set; }

        [IUPropiedad(Visible = false)]
        public string PlfDeVenta { get; set; }

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
        public decimal? TotalSinIva { get; set; }

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
        public decimal? TotalConIva { get; set; }


        //--------------------------------------------
        [IUPropiedad(Visible = false)]
        public enumEtapasDePartesTr Etapa { get; set; }

    }


}
