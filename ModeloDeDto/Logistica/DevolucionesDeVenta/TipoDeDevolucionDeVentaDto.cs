using Utilidades;
using ModeloDeDto.Negocio;
using ServicioDeDatos;

namespace ModeloDeDto.Logistica
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class TipoDeDevolucionDeVentaDto : TipoDeElementoDto, IPermisoDeInterventorDto
    {

        //-------------------------------------------------------------------------------------------------------
        [IUPropiedad(Etiqueta = "Id del estado inicial", Visible = false)]
        public int IdEstado { get; set; }
        [IUPropiedad(
            Etiqueta = "Estado inicial",
            Ayuda = "Seleccione estado en el que se inicia la devolución de venta",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(EstadoDto),
            GuardarEn = nameof(IdEstado),
            Controlador = nameof(enumControladoresNegocio.Estados),
            VistaDondeNavegar = enumVistasNegocio.CrudDeEstados,
            LongitudMinimaParaBuscar = 1,
            Negocio = enumNegocio.DevolucionVenta,
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

    }
}
