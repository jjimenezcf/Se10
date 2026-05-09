using Utilidades;

namespace ModeloDeDto.Seguridad
{
    [IUDto(AnchoEtiqueta = 20
      , AnchoSeparador = 5)]
    public class RolDto : ElementoDto
    {
        [IUPropiedad(
            Etiqueta = "Rol",
            Ayuda = "Nombre del rol",
            Tipo = typeof(string),
            Fila = 0,
            Columna = 0,
            Ordenar = true
            )
        ]
        public string Nombre { get; set; }


        [IUPropiedad(
            Etiqueta = "Descripción",
            Ayuda = "Descripción del rol",
            TipoDeControl = enumTipoControl.AreaDeTexto,
            Tipo = typeof(string),
            Fila = 1,
            Columna =0,
            Ordenar = true
            )
        ]
        public string Descripcion { get; set; }       
        
        //----------------------------------------------------------
        [IUPropiedad(Etiqueta = "Permiso",
            Ayuda = "Permisos a incluir",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Permiso),
            Fila = 4,
            Columna = 0,
            VisibleEnGrid = false,
            EditableAlCrear = false,
            VisibleAlEditar = false,
            VisibleAlConsultar = false,
            AutoSpan = true,
            Controlador = nameof(enumControladoresSeguridad.Permisos),
            VistaDondeNavegar = enumVistasSeguridad.CrudPermiso,
            Obligatorio = false
            )
        ]
        public int? idPermiso { get; set; }

        [IUPropiedad(Visible = false, Obligatorio = false)]
        public string Permiso { get; set; }
    }
}
