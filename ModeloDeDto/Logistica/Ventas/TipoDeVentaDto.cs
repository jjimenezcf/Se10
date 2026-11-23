using Utilidades;
using ModeloDeDto.Negocio;
using ServicioDeDatos;

namespace ModeloDeDto.Logistica
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class TipoDeVentaDto : TipoDeElementoDto, IPermisoDeInterventorDto
    {

        //-------------------------------------------------------------------------------------------------------
        [IUPropiedad(Etiqueta = "Id del estado inicial", Visible = false)]
        public int IdEstado { get; set; }
        [IUPropiedad(
            Etiqueta = "Estado inicial",
            Ayuda = "Seleccione estado en el que se inicia el documento de venta",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(EstadoDto),
            GuardarEn = nameof(IdEstado),
            Controlador = nameof(enumControladoresNegocio.Estados),
            VistaDondeNavegar = enumVistasNegocio.CrudDeEstados,
            LongitudMinimaParaBuscar = 1,
            Negocio = enumNegocio.Venta,
            CriterioDeBusqueda = enumCriteriosDeFiltrado.comienza,
            Fila = 1,
            Columna = 1,
            Obligatorio = true,
            Ordenar = true,
            AutoSpan = true
            )
        ]
        public string Estado { get; set; }

        //-------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "P. de Interventor",
            Ayuda = "Permiso de intervención",
            EditableAlEditar = false,
            Tipo = typeof(int),
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(PermisoDeInterventor),
            Controlador = nameof(enumControladoresSeguridad.Permisos),
            VistaDondeNavegar = enumVistasSeguridad.CrudPermiso,
            Fila = 3,
            Columna = 0,
            Obligatorio = false,
            VisibleEnEdicion = true,
            AutoSpan = true
            )
        ]
        public int IdPermisoDeInterventor { get; set; }
        [IUPropiedad(Visible = false)]
        public string PermisoDeInterventor { get; set; }

        //-------------------------------------------------------------------------------------------------------
        // tipo de la prefactura que se crea al facturar un documento de venta de este tipo
        [IUPropiedad(
              Etiqueta = "Id del tipo de factura emitida",
              Visible = false
              )]
        public int? IdTipoFacturaEmt { get; set; }

        [IUPropiedad(
            Etiqueta = "Tipo de prefactura",
            Ayuda = "Tipo de la prefactura que se crea al facturar el documento de venta",
            TipoDeControl = enumTipoControl.ListaDinamica,
            Negocio = enumNegocio.FacturaEmitida,
            GuardarEn = nameof(IdTipoFacturaEmt),
            Controlador = nameof(enumControladoresNegocio.TiposDeElemento),
            VistaDondeNavegar = enumVistasVentas.TiposDeFacturaEmt,
            MostrarExpresion = nameof(TipoDeElementoDto.Nombre),
            SoloEnAlta = true,
            LongitudMinimaParaBuscar = 1,
            Obligatorio = false,
            Fila = 4,
            Columna = 0,
            VisibleEnGrid = false,
            EditableAlCrear = true,
            EditableAlEditar = true,
            AutoSpan = true)]
        public string TipoFacturaEmt { get; set; }

        //-------------------------------------------------------------------------------------------------------
        // tipo de la devolución de venta que se crea al devolver líneas de un documento de venta de este tipo
        [IUPropiedad(
              Etiqueta = "Id del tipo de devolución de venta",
              Visible = false
              )]
        public int? IdTipoDevolucion { get; set; }

        [IUPropiedad(
            Etiqueta = "Tipo de devolución",
            Ayuda = "Tipo de la devolución de venta que se crea al devolver líneas del documento de venta",
            TipoDeControl = enumTipoControl.ListaDinamica,
            Negocio = enumNegocio.DevolucionVenta,
            GuardarEn = nameof(IdTipoDevolucion),
            Controlador = nameof(enumControladoresNegocio.TiposDeElemento),
            VistaDondeNavegar = enumVistasLogisticas.TiposDeDevolucionDeVenta,
            MostrarExpresion = nameof(TipoDeElementoDto.Nombre),
            SoloEnAlta = true,
            LongitudMinimaParaBuscar = 1,
            Obligatorio = false,
            Fila = 5,
            Columna = 0,
            VisibleEnGrid = false,
            EditableAlCrear = true,
            EditableAlEditar = true,
            AutoSpan = true)]
        public string TipoDevolucion { get; set; }

    }
}
