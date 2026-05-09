using Utilidades;
using ModeloDeDto.Terceros;

namespace ModeloDeDto.Seguridad
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, MostrarExpresion = ExpresionPt)]
    public class PuestoDto : ElementoDto
    {
        public const string ExpresionPt = "Expresion";

        //----------------------------------------------
        [IUPropiedad(
         Etiqueta = "",
         Oculto = true,
         Obligatorio = false,
         VisibleEnGrid = false,
         Fila = 0,
         Columna = 0,
         Posicion = 0
         )]
        public int? IdSociedadDelCg { get; set; }

        [IUPropiedad(
         Etiqueta = "Id del Cg",
         Visible = false
         )]
        public int IdCg { get; set; }

        [IUPropiedad(
            Etiqueta = "CG",
            Ayuda = "Centro gestor",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdCg),
            MostrarExpresion = nameof(CentroGestorDto.Expresion),
            Controlador = nameof(enumControladoresTerceros.CentrosGestores),
            VistaDondeNavegar = enumVistasTerceros.CrudCentrosGestores,
            SeleccionarDe = typeof(CentroGestorDto),
            Negocio = enumNegocio.CentroGestor,
            LongitudMinimaParaBuscar = 1,
            Tipo = typeof(string),
            Fila = 0,
            Columna = 0,
            Obligatorio = true,
            Ordenar = true,
            VisibleEnEdicion = true,
            EditableAlCrear = true,
            EditableAlEditar = false)]
        public string Cg { get; set; }

        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Puesto",
            Ayuda = "Nombre al puesto de trabajo",
            Tipo = typeof(string),
            Fila = 0,
            Columna = 1,
            Ordenar = true
            )
        ]
        public string Nombre { get; set; }


        [IUPropiedad(
            Etiqueta = "Descripción",
            TipoDeControl = enumTipoControl.AreaDeTexto,
            Ayuda = "Descripción del puesto de trabajo",
            Visible = false,
            VisibleEnEdicion = true,
            Tipo = typeof(string),
            Fila = 1,
            Columna = 0,
            AutoSpan = true
            )
        ]
        public string Descripcion { get; set; }

        [IUPropiedad(Etiqueta = "Roles del puesto",Visible = false, VisibleEnGrid = false)]
        public string RolesDeUnPuesto { get; set; }

        //---------------------------------------------
        [IUPropiedad(
            Etiqueta = "Id del puesto",
            Visible = false
            )
        ]
        public int? IdRolesDeUnPuesto { get; set; }

        [IUPropiedad(
            Etiqueta = "Copiar roles de",
            Ayuda = "Indique el puesto",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(PuestoDto),
            GuardarEn = nameof(IdRolesDeUnPuesto),
            RestringidoPorControl = "",
            MostrarExpresion = nameof(Expresion),
            Controlador = nameof(enumControladoresSeguridad.PuestoDeTrabajo),
            VistaDondeNavegar = enumVistasSeguridad.CrudPuestoDeTrabajo,
            VisibleAlCrear = true,
            VisibleAlEditar = true,
            VisibleEnGrid = false,
            VisibleAlConsultar = false,
            AplicarJoin = true,
            Fila = 2,
            Columna = 0,
            Obligatorio = false
            )
        ]
        public string RolesDe { get; set; }

        //---------------------------------------------
        [IUPropiedad(
            Etiqueta = "Id del puesto",
            Visible = false
            )
        ]
        public int? IdPermisosDirectos { get; set; }

        [IUPropiedad(
            Etiqueta = "Copiar permisos directos",
            Ayuda = "Indique el puesto del que se quieren copiar los permisos directos",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(PuestoDto),
            GuardarEn = nameof(IdPermisosDirectos),
            RestringidoPorControl = "",
            MostrarExpresion = nameof(Expresion),
            Controlador = nameof(enumControladoresSeguridad.PuestoDeTrabajo),
            VistaDondeNavegar = enumVistasSeguridad.CrudPuestoDeTrabajo,
            VisibleAlCrear = true,
            VisibleAlEditar = true,
            VisibleEnGrid = false,
            VisibleAlConsultar = false,
            AplicarJoin = true,
            Fila = 2,
            Columna = 1,
            Obligatorio = false
            )
        ]
        public string PermisosDirectosDe { get; set; }

        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Expresión del puesto",
            Tipo = typeof(string),
            Visible = false
          )
        ]
        public string Expresion { get; set; }
    }

}
