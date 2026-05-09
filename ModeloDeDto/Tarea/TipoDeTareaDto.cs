using Utilidades;
using ModeloDeDto.Negocio;
using ServicioDeDatos;
using ServicioDeDatos.Tarea;
using ModeloDeDto.SistemaDocumental;
using ServicioDeDatos.Seguridad;

namespace ModeloDeDto.Tarea
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class TipoDeTareaDto : TipoDeElementoDto, IPermisoDeInterventorDto
    {

        //-------------------------------------------------------------------------------------------------------
        [IUPropiedad(Etiqueta = "Id del estado inicial", Visible = false) ]
        public int IdEstado { get; set; }
        [IUPropiedad(
            Etiqueta = "Estado inicial",
            Ayuda = "Seleccione estado en el que se inicia la tarea",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(EstadoDto),
            GuardarEn = nameof(IdEstado),
            Controlador = nameof(enumControladoresNegocio.Estados),
            VistaDondeNavegar = enumVistasNegocio.CrudDeEstados,
            LongitudMinimaParaBuscar = 1,
            Negocio = enumNegocio.Tarea,
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
            Etiqueta = "Clase de tarea",
            Ayuda = "indique la clase de tarea",
            TipoDeControl = enumTipoControl.Enumerado,
            Tipo = typeof(enumClaseDeTarea),
            GuardarEn = nameof(ClaseDeTarea),
            Fila = 2,
            Columna = 0,
            Obligatorio = false,
            VisibleEnEdicion = true
          )
        ]
        public string ClaseDeTarea { get; set; }
        //-------------------------------------------------------------------------------------------------------
        [IUPropiedad(Etiqueta = "Id del tipo de archivador para almacenar la documentación de la tarea", Visible = false)]
        public int? IdTipoArchivador { get; set; }

        [IUPropiedad(
            Etiqueta = "Tipo de archivador",
            Ayuda = "Tipo de archivador a crear para almacenar la documentación resultante de ejecución de la tarea",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(TipoDeArchivadorDto),
            Controlador = nameof(enumControladoresNegocio.TiposDeElemento),
            VistaDondeNavegar = enumVistasSistemaDocumental.TiposDeArchivador,
            RestrictorFijo = nameof(ltrParametrosEp.negocio) + ";" + nameof(enumNegocio.Archivador) + ";" + nameof(enumModoDeAccesoDeDatos.Gestor),
            SoloEnAlta = true,
            Negocio = enumNegocio.Archivador,
            GuardarEn = nameof(IdTipoArchivador),
            LongitudMinimaParaBuscar = 1,
            CriterioDeBusqueda = enumCriteriosDeFiltrado.comienza,
            Fila = 1,
            Columna = 1,
            Obligatorio = false,
            VisibleEnEdicion = true,
            AutoSpan = true
            )
        ]
        public string TipoArchivador { get; set; }

        //-------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "P. de Interventor",
            Ayuda = "Permiso de intervención (permite cambiar la fecha de entrega)",
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

        //-------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Usa planificación",
            Ayuda = "indica si la tarea se planifica",
            VisibleEnGrid = false,
            VisibleAlCrear = true,
            VisibleEnEdicion = true,
            Obligatorio = true,
            Fila = 12,
            Columna = 1,
            TipoDeControl = enumTipoControl.Check,
            ValorPorDefecto = false,
            css = enumCssControles.ControlApilado
            )
        ]
        public bool UsaPlanificacion { get; set; }

        //-------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Es facturable",
            Ayuda = "indica si la tarea se puede facturar",
            VisibleEnGrid = false,
            VisibleAlCrear = true,
            VisibleEnEdicion = true,
            Obligatorio = true,
            Fila = 12,
            Columna = 2,
            TipoDeControl = enumTipoControl.Check,
            ValorPorDefecto = false,
            css = enumCssControles.ControlApilado
            )
        ]
        public bool EsFacturable { get; set; }

        //-------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Copiar dirección",
            Ayuda = "indica si se ha de copiar la dirección del solicitante a la tarea",
            VisibleEnGrid = false,
            VisibleAlCrear = true,
            VisibleEnEdicion = true,
            Obligatorio = true,
            Fila = 12,
            Columna = 3,
            TipoDeControl = enumTipoControl.Check,
            ValorPorDefecto = false,
            css = enumCssControles.ControlApilado
            )
        ]
        public bool CopiarDireccionDelSolicitante { get; set; }

    }
}
