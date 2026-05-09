using Utilidades;
using ModeloDeDto.Negocio;
using ServicioDeDatos;
using ServicioDeDatos.Juridico;
using ModeloDeDto.SistemaDocumental;
using ServicioDeDatos.Seguridad;

namespace ModeloDeDto.Juridico
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class TipoDeContratoDto : TipoDeElementoDto, IPermisoDeInterventorDto
    {
        //-------------------------------------------------------------------------------------------------------
        [IUPropiedad(Etiqueta = "Id del estado inicial", Visible = false) ]
        public int IdEstado { get; set; }
        [IUPropiedad(
            Etiqueta = "Estado inicial",
            Ayuda = "Seleccione estado en el que se inicia el contrato",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(EstadoDto),
            GuardarEn = nameof(IdEstado),
            Controlador = nameof(enumControladoresNegocio.Estados),
            VistaDondeNavegar = enumVistasNegocio.CrudDeEstados,
            LongitudMinimaParaBuscar = 1,
            Negocio = enumNegocio.Contrato,
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
            Etiqueta = "Clase del contrato",
            Ayuda = "indique la clase del contrato",
            TipoDeControl = enumTipoControl.Enumerado,
            Tipo = typeof(enumClaseDeContrato),
            GuardarEn = nameof(ClaseDeContrato),
            Fila = 2,
            Columna = 0,
            Obligatorio = false,
            VisibleEnEdicion = true,
            OnChange = "javascript:" + nameof(enumNameSpaceTs.Juridico) + "." + nameof(enumFunctionTs.Ctr_TrasSeleccionarClaseDeCtr) + "(this)"
          )
        ]
        public string ClaseDeContrato { get; set; }

        //-------------------------------------------------------------------------------------------------------
        [IUPropiedad(Etiqueta = "Id del tipo de archivador para almacenar la documentación del contrato", Visible = false)]
        public int IdTipoArchivador { get; set; }
        [IUPropiedad(
            Etiqueta = "Tipo de archivador donde almacenar el contrato",
            Ayuda = "Tipo de archivador a crear para almacenar la documentación del contrato",
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
            Fila = 7,
            Columna = 1,
            VisibleEnGrid = false,
            EditableAlCrear = true,
            EditableAlEditar = true,
            AutoSpan = false)]
        public string TipoFacturaEmt { get; set; }
    }
}
