using Utilidades;
using ModeloDeDto.Negocio;
using ServicioDeDatos;
using ServicioDeDatos.Expediente;

namespace ModeloDeDto.Expediente
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class TipoDeExpedienteDto : TipoDeElementoDto, IPermisoDeInterventorDto
    {

        //-------------------------------------------------------------------------------------------------------
        [IUPropiedad(Etiqueta = "Id del estado inicial", Visible = false) ]
        public int IdEstado { get; set; }
        [IUPropiedad(
            Etiqueta = "Estado inicial",
            Ayuda = "Seleccione estado en el que se inicia del expediente",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(EstadoDto),
            GuardarEn = nameof(IdEstado),
            Controlador = nameof(enumControladoresNegocio.Estados),
            VistaDondeNavegar = enumVistasNegocio.CrudDeEstados,
            LongitudMinimaParaBuscar = 1,
            Negocio = enumNegocio.Expediente,
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
            Etiqueta = "Clase del expediente",
            Ayuda = "indique la clase del expediente",
            TipoDeControl = enumTipoControl.Enumerado,
            Tipo = typeof(enumClaseDeExpediente),
            GuardarEn = nameof(ClaseDeExpediente),
            Fila = 2,
            Columna = 0,
            Obligatorio = false,
            VisibleEnEdicion = true
          )
        ]
        public string ClaseDeExpediente { get; set; }
 
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
            Etiqueta = "Usa tareas",
            Ayuda = "indica si el expediente usa tareas",
            VisibleEnGrid = false,
            VisibleAlCrear = true,
            VisibleEnEdicion = true,
            Obligatorio = true,
            Fila = 8,
            Columna = 0,
            TipoDeControl = enumTipoControl.Check,
            ValorPorDefecto = true,
            css = enumCssControles.ControlApilado
            )
        ]
        public bool UsaTareas { get; set; }

        //-------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Usa presupuestos",
            Ayuda = "indica si el expediente usa presupuestos",
            VisibleEnGrid = false,
            VisibleAlCrear = true,
            VisibleEnEdicion = true,
            Obligatorio = true,
            Fila = 8,
            Columna = 1,
            TipoDeControl = enumTipoControl.Check,
            ValorPorDefecto = false,
            css = enumCssControles.ControlApilado
            )
        ]
        public bool UsaPpts { get; set; }


        //-------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Contratos de venta",
            Ayuda = "Expediente de ventas",
            VisibleEnGrid = false,
            VisibleAlCrear = true,
            VisibleEnEdicion = true,
            Obligatorio = true,
            Fila = 8,
            Columna = 2,
            TipoDeControl = enumTipoControl.Check,
            ValorPorDefecto = false,
            css = enumCssControles.ControlApilado
            )
        ]
        public bool ScDeVenta { get; set; }

        //-------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Contratos de compra",
            Ayuda = "Expedientes de compras",
            VisibleEnGrid = false,
            VisibleAlCrear = true,
            VisibleEnEdicion = true,
            Obligatorio = true,
            Fila = 8,
            Columna = 3,
            TipoDeControl = enumTipoControl.Check,
            ValorPorDefecto = false,
            css = enumCssControles.ControlApilado
            )
        ]
        public bool ScDeCompra{ get; set; }
    }
}
