

using Utilidades;
using ServicioDeDatos.Elemento;
using ServicioDeDatos;

namespace ModeloDeDto.Seguridad
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class PermisosDeUnRolDto : ElementoDto, IRelacionDto
    {
        public static string ExpresionElemento = nameof(Permiso); 

        [IUPropiedad(Etiqueta = "Rol",
            Ayuda = "permisos de un rol",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            CriterioDeBusqueda = enumCriteriosDeFiltrado.diferente,
            MostrarExpresion = nameof(Rol),
            Fila = 0,
            Columna = 0,
            VisibleEnGrid = false
            )
        ]
        public int IdRol { get; set; }

        [IUPropiedad(
            Etiqueta = "rol",
            Visible = false
            )
        ]
        public string Rol { get; set; }


        [IUPropiedad(
            Etiqueta = "Id del permiso",
            Visible = false
            )
        ]
        public int IdPermiso { get; set; }

        [IUPropiedad(
            Etiqueta = "Permiso",
            Ayuda = "Indique el permiso",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(PermisoDto),
            GuardarEn = nameof(IdPermiso),
            Controlador = nameof(enumControladoresSeguridad.Permisos),
            RestringidoPorControl = nameof(IdRol),
            VistaDondeNavegar = enumVistasSeguridad.CrudPermiso,
            LongitudMinimaParaBuscar = 1,
            Fila = 1,
            Columna = 0,
            Ordenar = true,
            OrdenarGridPor = nameof(INombre.Nombre)
            )
        ]
        public string Permiso { get; set; }
    }


}
