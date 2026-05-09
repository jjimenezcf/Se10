

using Utilidades;

namespace ModeloDeDto.Seguridad
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class PermisosDeUnPuestoDto : ElementoDto, IRelacionDto
    {
        public static string ExpresionElemento = nameof(Permiso);


        //-------------------------------------------------------

        [IUPropiedad(Etiqueta = "Centro getor", VisibleEnGrid = false, VisibleAlCrear = false, VisibleAlEditar = true, EditableAlEditar = false, Fila = 0, Columna = 0)]
        public string CgDelPuesto { get; set; }

        //-------------------------------------------------------------------------
        [IUPropiedad(Etiqueta = "Puesto",
            Ayuda = "permisos de un puesto",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Puesto),
            Fila = 0,
            Columna = 0,
            VisibleEnGrid = false,
            AutoSpan = true
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
            Controlador = nameof(enumControladoresSeguridad.Permisos),
            VistaDondeNavegar = enumVistasSeguridad.CrudPermiso,
            GuardarEn = nameof(IdPermiso),
            Fila = 1,
            Columna = 0,
            Ordenar = true
            )
        ]
        public string Permiso { get; set; }

        //-------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Roles",
            Ayuda = "Origen del permiso",
            VisibleAlCrear = false,
            VisibleAlEditar = true,
            EditableAlCrear = false,
            EditableAlEditar = false,
            VisibleEnGrid = false,
            TipoDeControl = enumTipoControl.Editor,
            Fila = 1,
            Columna = 1,
            PorAnchoMnt = 60
            )
        ]
        public string Roles { get; set; }


    }


}
