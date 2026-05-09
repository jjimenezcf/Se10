using Utilidades;
using ModeloDeDto.Negocio;
using ServicioDeDatos;

namespace ModeloDeDto.Ventas
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class TipoDeParteTrDto : TipoDeElementoDto, IPermisoDeInterventorDto
    {
        //-------------------------------------------------------------------------------------------------------
        [IUPropiedad(Etiqueta = "Id del estado inicial", Visible = false) ]
        public int IdEstado { get; set; }
        [IUPropiedad(
            Etiqueta = "Estado inicial",
            Ayuda = "Seleccione estado en el que se inicia el parte de trabajo",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(EstadoDto),
            GuardarEn = nameof(IdEstado),
            Controlador = nameof(enumControladoresNegocio.Estados),
            VistaDondeNavegar = enumVistasNegocio.CrudDeEstados,
            LongitudMinimaParaBuscar = 1,
            Negocio = enumNegocio.ParteDeTrabajo,
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

        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(
              Etiqueta = "Id del tipo de factura emitida",
              Visible = false
              )]
        public int? IdTipoFacturaEmt { get; set; }

        [IUPropiedad(
            Etiqueta = "Tipo de factura a emitir",
            Ayuda = "Tipo de la factura que se ha de emitir",
            TipoDeControl = enumTipoControl.ListaDinamica,
            Negocio = enumNegocio.FacturaEmitida,
            GuardarEn = nameof(IdTipoFacturaEmt),
            Controlador = nameof(enumControladoresNegocio.TiposDeElemento),
            VistaDondeNavegar = enumVistasVentas.TiposDeFacturaEmt,
            MostrarExpresion = nameof(TipoDeElementoDto.Nombre),
            SoloEnAlta = true,
            LongitudMinimaParaBuscar = 1,
            Obligatorio = false,
            Fila = 3,
            Columna = 1,
            VisibleEnGrid = false,
            EditableAlCrear = true,
            EditableAlEditar = true,
            AutoSpan = true)]
        public string TipoFacturaEmt { get; set; }
    }
}
