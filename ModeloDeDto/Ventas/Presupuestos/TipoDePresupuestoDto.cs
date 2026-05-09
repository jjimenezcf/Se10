using Utilidades;
using ModeloDeDto.Negocio;
using ServicioDeDatos;
using ServicioDeDatos.Presupuesto;

namespace ModeloDeDto.Presupuesto
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class TipoDePresupuestoDto : TipoDeElementoDto, IPermisoDeInterventorDto
    {

        //-------------------------------------------------------------------------------------------------------
        [IUPropiedad(Etiqueta = "Id del estado inicial", Visible = false) ]
        public int IdEstado { get; set; }
        [IUPropiedad(
            Etiqueta = "Estado inicial",
            Ayuda = "Seleccione estado en el que se inicia el presupuesto",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(EstadoDto),
            GuardarEn = nameof(IdEstado),
            Controlador = nameof(enumControladoresNegocio.Estados),
            VistaDondeNavegar = enumVistasNegocio.CrudDeEstados,
            LongitudMinimaParaBuscar = 1,
            Negocio = enumNegocio.Presupuesto,
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
            Etiqueta = "Clase del presupuesto",
            Ayuda = "indique la clase del presupuesto",
            TipoDeControl = enumTipoControl.Enumerado,
            Tipo = typeof(enumClaseDePresupuesto),
            GuardarEn = nameof(ClaseDePresupuesto),
            Fila = 2,
            Columna = 0,
            Obligatorio = false,
            VisibleEnEdicion = true,
            OnChange = "javascript:" + nameof(enumNameSpaceTs.Venta) + "." + nameof(enumFunctionTs.Ppt_TrasSeleccionarClaseDePpt) + "(this)"
          )
        ]
        public string ClaseDePresupuesto { get; set; }
 
        //-------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "P. de Interventor",
            Ayuda = "Permiso de intervención (permite modificar la fecha de caducidad o recalcular un ppt)",
            EditableAlEditar = false,
            Tipo = typeof(int),
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(PermisoDeInterventor),
            Controlador = nameof(enumControladoresSeguridad.Permisos),
            VistaDondeNavegar = enumVistasSeguridad.CrudPermiso,
            Fila = 7,
            Columna = 2,
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
              Etiqueta = "Id del tipo de parte de trabajo",
              Visible = false
              )]
        public int? IdTipoParteTr { get; set; }

        [IUPropiedad(
            Etiqueta = "Tipo de parte de trabajo a asociar",
            Ayuda = "Tipo del parte de trabajo por defecto que se asocia",
            TipoDeControl = enumTipoControl.ListaDinamica,
            Negocio = enumNegocio.ParteDeTrabajo,
            GuardarEn = nameof(IdTipoParteTr),
            Controlador = nameof(enumControladoresNegocio.TiposDeElemento),
            VistaDondeNavegar = enumVistasVentas.TiposDeParteTr,
            MostrarExpresion = nameof(TipoDeElementoDto.Nombre),
            SoloEnAlta = true,
            LongitudMinimaParaBuscar = 1,
            Obligatorio = false,
            Fila = 7,
            Columna = 0,
            VisibleEnGrid = false,
            EditableAlCrear = true,
            EditableAlEditar = true,
            AutoSpan = false)]
        public string TipoParteTr { get; set; }

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
