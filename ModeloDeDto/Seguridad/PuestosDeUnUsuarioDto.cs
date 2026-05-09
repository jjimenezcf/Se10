using ServicioDeDatos;
using Utilidades;

namespace ModeloDeDto.Seguridad
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class PuestosDeUnUsuarioDto: ElementoDto, IRelacionDto
    {
        public static string ExpresionElemento = nameof(Puesto);
        //----------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Usuario",
            Ayuda = "Puestos del usuario",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Usuario),
            CriterioDeBusqueda = enumCriteriosDeFiltrado.diferente,
            Fila = 0,
            Columna = 0,
            VisibleEnGrid = false
            )
        ]
        public int IdUsuario { get; set; }


        [IUPropiedad(
            Etiqueta = "usuario",
            Visible = false
            )
        ]
        public string Usuario { get; set; }

        //----------------------------------------------------------
        [IUPropiedad(Etiqueta = "Centro getor", VisibleEnGrid = true, Ordenar = true, VisibleAlCrear = false, VisibleAlEditar = true, EditableAlEditar = false, Fila = 1, Columna = 0)]
        public string CgDelPuesto { get; set; }

        [IUPropiedad(
            Etiqueta = "Id del puesto de trabajo",
            Visible = false
            )
        ]
        public int IdPuesto { get; set; }

        [IUPropiedad(
            Etiqueta = "Puesto",
            Ayuda = "Indique el puesto de trabajo",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(PuestoDto),
            GuardarEn = nameof(IdPuesto),
            MostrarExpresion = nameof(PuestoDto.Expresion),
            Controlador = nameof(enumControladoresSeguridad.PuestoDeTrabajo),
            VistaDondeNavegar = enumVistasSeguridad.CrudPuestoDeTrabajo,
            RestringidoPorControl = nameof(IdUsuario),
            AplicarJoin = true,
            Fila = 1,
            Columna = 0,
            Ordenar = true
            )
        ]
        public string Puesto { get; set; }

        //----------------------------------------------------------
        [IUPropiedad(
            Etiqueta ="Roles del puesto",
            Visible = false,
            VisibleEnGrid = false)]
        public string RolesDeUnPuesto { get; set; }

    }
}
