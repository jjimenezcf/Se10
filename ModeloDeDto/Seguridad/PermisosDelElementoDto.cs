using Utilidades;

namespace ModeloDeDto.Seguridad
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, OpcionDeBorrar = false)]
    public class PermisosDelElementoDto : ElementoDto
    {
        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Negocio",
            Ayuda = "negocio del elemento",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Negocio),
            Fila = 0,
            Columna = 0,
            EditableAlCrear = false,
            EditableAlEditar = false,
            VisibleEnGrid = false
            )
        ]
        public int IdNegocio { get; set; }

        [IUPropiedad(Visible = false)]
        public string Negocio { get; set; }

        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Elemento",
            Ayuda = "permisos sobre el elemento",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Elemento),
            Fila = 0,
            Columna = 1,
            EditableAlCrear = false,
            EditableAlEditar = false,
            VisibleEnGrid = false,
            AutoSpan = true
            )
        ]
        public int IdElemento { get; set; }

        [IUPropiedad(Visible = false)]
        public string Elemento { get; set; }

        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "P. Administrador",
            Ayuda = "permisos de administración",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Administrador),
            Fila = 1,
            Columna = 0,
            EditableAlCrear = false,
            EditableAlEditar = false,
            VisibleEnGrid = false,
            Controlador = nameof(enumControladoresSeguridad.Permisos),
            VistaDondeNavegar = enumVistasSeguridad.CrudPermiso
            )
        ]
        public int IdAdministrador { get; set; }

        [IUPropiedad(Visible = false)]
        public string Administrador { get; set; }

        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "P. Gestor",
            Ayuda = "permisos de gesión",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Gestor),
            Fila = 1,
            Columna = 1,
            EditableAlCrear = false,
            EditableAlEditar = false,
            VisibleEnGrid = false,
            Controlador = nameof(enumControladoresSeguridad.Permisos),
            VistaDondeNavegar = enumVistasSeguridad.CrudPermiso
            )
        ]
        public int IdGestor { get; set; }

        [IUPropiedad(Visible = false)]
        public string Gestor { get; set; }

        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "P. Consultor",
            Ayuda = "permisos de consultor",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Consultor),
            Fila = 1,
            Columna = 2,
            EditableAlCrear = false,
            EditableAlEditar = false,
            VisibleEnGrid = false,
            Controlador = nameof(enumControladoresSeguridad.Permisos),
            VistaDondeNavegar = enumVistasSeguridad.CrudPermiso
            )
        ]
        public int IdConsultor { get; set; }

        [IUPropiedad(Visible = false)]
        public string Consultor { get; set; }

    }

}
