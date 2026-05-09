using Utilidades;

namespace ModeloDeDto.Entorno
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class AgendaDto : ElementoDto
    {
        [IUPropiedad(
          Etiqueta = "Agenda",
          Ayuda = "Indique el nombre de la agenda",
          Tipo = typeof(string),
          Fila = 0,
          Columna = 0,
          Ordenar = true,
          ColSpan =2
          )
        ]
        public string Nombre { get; set; }

        //-------------------------------------------------------------------------------------------------------
        [IUPropiedad(
          Etiqueta = "Url",
          Ayuda = "Url para poderse subcribir",
          Tipo = typeof(string),
          Fila = 1,
          Columna = 0,
          VisibleEnGrid = false,
          VisibleAlCrear =false,
          EditableAlEditar =false,
          ColSpan = 2,
          Obligatorio = false
          )
        ]
        public string Uri { get; set; }

        //-------------------------------------------------------------------------------------------------------
        [IUPropiedad(
          Etiqueta = "Ruta",
          Ayuda = "Ruta para poderse subcribir",
          Tipo = typeof(string),
          Fila = 2,
          Columna = 0,
          VisibleAlCrear = false,
          VisibleAlEditar = true,
          EditableAlEditar = false,
          PorAnchoMnt = 75,
          ColSpan = 2,
          Obligatorio = false
          )
        ]
        public string UrlDeAgenda { get; set; }

        //-------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "P. de Intervención",
            Ayuda = "Permiso usado por procesos del sistema el sistema ",
            EditableAlEditar = false,
            Tipo = typeof(int),
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Interventor),
            Controlador = nameof(enumControladoresSeguridad.Permisos),
            VistaDondeNavegar = enumVistasSeguridad.CrudPermiso,
            Fila = 3,
            Columna = 0,
            Obligatorio = false,
            VisibleEnEdicion = true,
            AutoSpan = true,
            VisibleEnGrid = false
            )
        ]
        public int IdInterventor { get; set; }
        [IUPropiedad(Visible = false)]
        public string Interventor { get; set; }


        //-------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "P. de Gestor",
            Ayuda = "Permiso de gestor (permite incluir eventos en la agenda)",
            EditableAlEditar = false,
            Tipo = typeof(int),
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Gestor),
            Controlador = nameof(enumControladoresSeguridad.Permisos),
            VistaDondeNavegar = enumVistasSeguridad.CrudPermiso,
            Fila = 3,
            Columna = 1,
            Obligatorio = false,
            VisibleEnEdicion = true,
            AutoSpan = true,
            VisibleEnGrid = false
            )
        ]
        public int IdGestor { get; set; }
        [IUPropiedad(Visible = false)]
        public string Gestor { get; set; }


        //-------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "P. de Consultor",
            Ayuda = "Permiso de consultor (permite consultar eventos en la agenda)",
            EditableAlEditar = false,
            Tipo = typeof(int),
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Consultor),
            Controlador = nameof(enumControladoresSeguridad.Permisos),
            VistaDondeNavegar = enumVistasSeguridad.CrudPermiso,
            Fila = 3,
            Columna = 2,
            Obligatorio = false,
            VisibleEnEdicion = true,
            AutoSpan = true,
            VisibleEnGrid = false
            )
        ]
        public int IdConsultor { get; set; }
        
        [IUPropiedad(Visible = false)]
        public string Consultor { get; set; }

        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(
           TamanoFijo = "6em"
         , Etiqueta = "Acción"
         , EtiquetaGrid = ""
         , VisibleEnEdicion = false
         , TipoDeControl = enumTipoControl.Referencia
         , AccionRef = "javascript: " + nameof(enumNameSpaceTs.Entorno) + "." + nameof(enumFunctionTs.Agenda_AbrirAgendaSeleccionada) + "(numeroDeFila)"
         , Alineada = enumAliniacion.derecha)]
        public string Accion { get; set; } = "Consultar";

    }
}
