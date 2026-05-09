using Utilidades;

namespace ModeloDeDto.Negocio
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, OpcionDeCrear = true, OpcionDeBorrar = true)]
    public class PlantillaDeExportacionDto : ElementoDto
    {
        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Negocio",
            Ayuda = "Estados del negocio",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Negocio),
            Fila = 0,
            Columna = 0,
            EditableAlEditar = false,
            VisibleEnGrid = false
            )
        ]
        public int IdNegocio { get; set; }

        [IUPropiedad(Visible = false)]
        public string Negocio { get; set; }

        //--------------------------------------------
        [IUPropiedad(
          Etiqueta = "Plantilla",
          Ayuda = "Indique el nombre de la plantilla de exportación",
          Tipo = typeof(string),
          Fila = 0,
          Columna = 1,
          Ordenar = true,
          EditableAlEditar = false,
          AutoSpan = true
          )
        ]
        public string Nombre { get; set; }


        //------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Dll",
            Ayuda = "indica el assembly",
            TipoDeControl = enumTipoControl.Editor,
            Tipo = typeof(string),
            Fila = 1,
            Columna = 0,
            VisibleEnGrid = false
            )
        ]
        public string Dll { get; set; }

        //------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Clase",
            Ayuda = "indica espacio de nombre y clase",
            TipoDeControl = enumTipoControl.Editor,
            Tipo = typeof(string),
            Fila = 1,
            Columna = 1,
            VisibleEnGrid = false
            )
        ]
        public string Clase { get; set; }

        //------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Método",
            Ayuda = "indica el método",
            TipoDeControl = enumTipoControl.Editor,
            Tipo = typeof(string),
            Fila = 1,
            Columna = 2,
            VisibleEnGrid = false
            )
        ]
        public string Metodo { get; set; }


        //------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Programa",
            TipoDeControl = enumTipoControl.Editor,
            Tipo = typeof(string),
            VisibleEnGrid = true,
            VisibleEnEdicion = false
            )
        ]
        public string Programa => Dll + Simbolos.Punto +  Clase + Simbolos.Punto + Metodo;


        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Permiso",
           Ayuda = "permiso de exportación",
            EditableAlEditar = false,
            Tipo = typeof(int),
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Permiso),
            Controlador = nameof(enumControladoresSeguridad.Permisos),
            VistaDondeNavegar = enumVistasSeguridad.CrudPermiso,
            Fila = 2,
            Columna = 0,
            Obligatorio = false,
            VisibleAlEditar = true,
            VisibleAlCrear = false,
            VisibleEnGrid = false,
            AutoSpan = true
            )
        ]
        public int? IdPermiso { get; set; }

        [IUPropiedad(Visible = false)]
        public string Permiso { get; set; }


    }
}
