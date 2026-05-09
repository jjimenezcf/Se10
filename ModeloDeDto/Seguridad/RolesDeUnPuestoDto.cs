using ServicioDeDatos;
using Utilidades;

namespace ModeloDeDto.Seguridad
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class RolesDeUnPuestoDto : ElementoDto, IRelacionDto
    {

        public static string ExpresionElemento = nameof(Rol);
        //-------------------------------------------------------------------------
        [IUPropiedad(Etiqueta = "Puesto",
            Ayuda = "Roles de un puesto",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Puesto),
            CriterioDeBusqueda = enumCriteriosDeFiltrado.diferente,
            Fila = 0,
            Columna = 0,
            VisibleEnGrid = false
            )
        ]
        public int IdPuesto { get; set; }

        [IUPropiedad(
            Etiqueta = "Puesto de trabajo",
            Visible = false
            )
        ]
        public string Puesto { get; set; }


        //-------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Id del rol",
            Visible = false
            )
        ]
        public int IdRol { get; set; }


        [IUPropiedad(
            Etiqueta = "Rol",
            Ayuda = "Indique el rol",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(RolDto),
            GuardarEn = nameof(IdRol),
            Controlador = nameof(enumControladoresSeguridad.Rol),
            RestringidoPorControl = nameof(IdPuesto),
            VistaDondeNavegar = enumVistasSeguridad.CrudRol,
            Fila = 1,
            Columna = 0,
            Ordenar = true
            )
        ]
        public string Rol { get; set; }


    }
}
