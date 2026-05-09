
using ServicioDeDatos;
using Utilidades;

namespace ModeloDeDto.Seguridad
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class RolesDeUnPermisoDto : ElementoDto, IRelacionDto
    {
        public static string ExpresionElemento = nameof(Rol);

        //-------------------------------------------------------------------------
        [IUPropiedad(Etiqueta = "Permiso",
            Ayuda = "Roles de un permiso",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            CriterioDeBusqueda = enumCriteriosDeFiltrado.diferente,
            MostrarExpresion = nameof(Permiso),
            Fila = 0,
            Columna = 0,
            VisibleEnGrid = false
            )
        ]
        public int IdPermiso { get; set; }

        [IUPropiedad(
            Etiqueta = "permiso",
            Visible = false
            )
        ]
        public string Permiso { get; set; }

        //-------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Id del rol",
            Visible = false
                     )]
        public int IdRol { get; set; }

        [IUPropiedad(
            Etiqueta = "Rol",
            Ayuda = "Indique el rol",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(RolDto),
            GuardarEn = nameof(IdRol),
            Controlador = nameof(enumControladoresSeguridad.Rol),
            RestringidoPorControl = nameof(IdPermiso),
            VistaDondeNavegar = enumVistasSeguridad.CrudPermiso,
            Fila = 1,
            Columna = 0,
            Ordenar = true
            )
        ]
        public string Rol { get; set; }
    }


}
