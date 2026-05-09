using Utilidades;
using ModeloDeDto.Negocio;
using ServicioDeDatos;
using ServicioDeDatos.RegistroEs;
using ModeloDeDto.SistemaDocumental;
using ServicioDeDatos.Seguridad;

namespace ModeloDeDto.RegistroEs
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class TipoDeRegistroEsDto : TipoDeElementoDto, IPermisoDeInterventorDto
    {

        //-------------------------------------------------------------------------------------------------------
        [IUPropiedad(Etiqueta = "Id del estado inicial", Visible = false) ]
        public int IdEstado { get; set; }
        [IUPropiedad(
            Etiqueta = "Estado inicial",
            Ayuda = "Seleccione estado en el que se inicia el proceso",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(EstadoDto),
            GuardarEn = nameof(IdEstado),
            Controlador = nameof(enumControladoresNegocio.Estados),
            VistaDondeNavegar = enumVistasNegocio.CrudDeEstados,
            LongitudMinimaParaBuscar = 1,
            Negocio = enumNegocio.Registro,
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
            Etiqueta = "Clase de registro",
            Ayuda = "indique el momento de ejecución",
            TipoDeControl = enumTipoControl.Enumerado,
            Tipo = typeof(enumClaseDeRegistroEs),
            GuardarEn = nameof(ClaseDeRegistro),
            Fila = 2,
            Columna = 0,
            Obligatorio = false,
            VisibleEnEdicion = true
          )
        ]
        public string ClaseDeRegistro { get; set; }
        //-------------------------------------------------------------------------------------------------------
        [IUPropiedad(Etiqueta = "Id del tipo de archivador para almacenar la documentación de entrada", Visible = false)]
        public int? IdTipoArchivadorDeEntrada { get; set; }
        [IUPropiedad(
            Etiqueta = "Tipo de archivador de la entrada",
            Ayuda = "Tipo de archivador a crear para almacenar la documentación de entrada",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(TipoDeArchivadorDto),
            Controlador = nameof(enumControladoresNegocio.TiposDeElemento),
            VistaDondeNavegar = enumVistasSistemaDocumental.TiposDeArchivador,
            RestrictorFijo = nameof(ltrParametrosEp.negocio) + ";" + nameof(enumNegocio.Archivador) + ";" + nameof(enumModoDeAccesoDeDatos.Gestor),
            SoloEnAlta = true,
            Negocio = enumNegocio.Archivador,
            GuardarEn = nameof(IdTipoArchivadorDeEntrada),
            LongitudMinimaParaBuscar = 1,
            CriterioDeBusqueda = enumCriteriosDeFiltrado.comienza,
            Fila = 1,
            Columna = 1,
            Obligatorio = false,
            VisibleEnEdicion = true,
            AutoSpan = true
            )
        ]
        public string TipoArchivadorDeEntrada { get; set; }

        //-------------------------------------------------------------------------------------------------------
        [IUPropiedad(Etiqueta = "Id del tipo de archivador para almacenar las respuesta", Visible = false)]
        public int? IdTipoArchivadorDeSalida { get; set; }

        [IUPropiedad(
            Etiqueta = "Tipo de archivador de salida",
            Ayuda = "Tipo de archivador a crear para almacenar documentación de respuesta",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(TipoDeArchivadorDto),
            Controlador = nameof(enumControladoresNegocio.TiposDeElemento),
            VistaDondeNavegar = enumVistasSistemaDocumental.TiposDeArchivador,
            RestrictorFijo = nameof(ltrParametrosEp.negocio) + ";" + nameof(enumNegocio.Archivador) + ";" + nameof(enumModoDeAccesoDeDatos.Gestor),
            SoloEnAlta = true,
            Negocio = enumNegocio.Archivador,
            GuardarEn = nameof(IdTipoArchivadorDeSalida),
            LongitudMinimaParaBuscar = 1,
            CriterioDeBusqueda = enumCriteriosDeFiltrado.comienza,
            Fila = 1,
            Columna = 1,
            Obligatorio = false,
            VisibleEnEdicion = true,
            AutoSpan = true
            )
        ]
        public string TipoArchivadorDeSalida { get; set; }

        //-------------------------------------------------------------------------------------------------------
        [IUPropiedad(Etiqueta = "Id del tipo de archivador para almacenar la documentación del proceso", Visible = false)]
        public int? IdTipoArchivadorInterno { get; set; }
        [IUPropiedad(
            Etiqueta = "Tipo de archivador interno",
            Ayuda = "Tipo de archivador donde almacenar la documentación interna del proceso",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(TipoDeArchivadorDto),
            Controlador = nameof(enumControladoresNegocio.TiposDeElemento),
            VistaDondeNavegar = enumVistasSistemaDocumental.TiposDeArchivador,
            RestrictorFijo = nameof(ltrParametrosEp.negocio) + ";" + nameof(enumNegocio.Archivador) + ";" + nameof(enumModoDeAccesoDeDatos.Gestor),
            SoloEnAlta = true,
            Negocio = enumNegocio.Archivador,
            GuardarEn = nameof(IdTipoArchivadorInterno),
            LongitudMinimaParaBuscar = 1,
            CriterioDeBusqueda = enumCriteriosDeFiltrado.comienza,
            VisibleEnEdicion = true,
            Fila = 1,
            Columna = 1,
            Obligatorio = true,
            Ordenar = true,
            AutoSpan = true
            )
        ]
        public string TipoArchivadorInterno { get; set; }

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
    }
}
