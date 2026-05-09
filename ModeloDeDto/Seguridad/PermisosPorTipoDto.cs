using ServicioDeDatos.Seguridad;
using Utilidades;

namespace ModeloDeDto.Seguridad
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, OpcionDeBorrar = false)]
    public class PermisosPorTipoDto : ElementoDto
    {
        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Negocio",
            Ayuda = "Negocio al que pertenece el tipo",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Negocio),
            Fila = 0,
            Columna = 0,
            VisibleAlCrear = false,
            EditableAlEditar = false,
            VisibleEnGrid = false
            )
        ]
        public int IdNegocio { get; set; }

        [IUPropiedad(Visible = false)]
        public string Negocio { get; set; }

        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Tipo",
            Ayuda = "Tipo del negocio tratado",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Tipo),
            Fila = 0,
            Columna = 1,
            VisibleAlCrear = false,
            EditableAlEditar = false,
            VisibleEnGrid = false
            )
        ]
        public int IdTipo { get; set; }

        [IUPropiedad(Visible = false)]
        public string Tipo { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Usuario",
           Ayuda = "usuario al que se aplica el permisos",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Usuario),
            PropiedadRestrictora = nameof(IdUsuario),
            EditableAlCrear = false,
            EditableAlEditar = false,
            VisibleEnGrid = false,
           Fila = 0,
           Columna = 0)
        ]
        public int IdUsuario { get; set; }

        [IUPropiedad(Visible = false)]
        public string Usuario { get; set; }

        //-------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Id del permiso",
            Visible = false
            )
        ]
        public int IdPermiso { get; set; }

        [IUPropiedad(
            Etiqueta = "Permiso del tipo",
            Ayuda = "Indique el permiso",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(PermisoDto),
            Controlador = nameof(enumControladoresSeguridad.Permisos),
            GuardarEn = nameof(IdPermiso),
            BuscarPor = nameof(ltrDeUnPermisoDtm.PermisosDeTipo),
            Fila = 1,
            Columna = 0
            )
        ]
        public string Permiso { get; set; }

        //----------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Calculado",
            Ayuda = "indica si el permiso es calculado",
            VisibleEnGrid = true,
            Fila = 2,
            Columna = 1,
            TipoDeControl = enumTipoControl.Check,
            ValorPorDefecto = false,
            VisibleAlCrear = false
            )
        ]
        public bool Calculado { get; set; }
    }

}
