using ServicioDeDatos.SistemaDocumental;
using Utilidades;

namespace ModeloDeDto.Negocio
{

    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class PlantillaDeNegocioDto: ElementoDto
    {       
        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Negocio",
            Ayuda = "Plantillas del negocio",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Negocio),
            Fila = 0,
            Columna = 0,
            EditableAlCrear = false,
            EditableAlEditar = false,
            VisibleEnGrid = false,
            AutoSpan = true
            )
        ]
        public int IdNegocio { get; set; }

        [IUPropiedad(Visible = false)]
        public string Negocio { get; set; }

        
        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Nombre",
            Ayuda = "nombre de la plantilla",
            Tipo = typeof(string),
            Fila = 1,
            Columna = 0,
            Obligatorio = true,
            LongitudMaxima = 250,
            AutoSpan = true
          )
        ]
        public string Nombre { get; set; }

        //----------------------------------------------------------

        [IUPropiedad(
            Etiqueta = "Accion",
            Ayuda = "acción para obtener los datos de pla plantilla",
            EditableAlEditar = false,
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Accion),
            Controlador = nameof(enumControladoresEntorno.Acciones),
            VistaDondeNavegar = enumVistasEntorno.CrudDeAcciones,
            Tipo = typeof(int),
            Fila = 2,
            Columna = 0,
            Obligatorio = false,
            VisibleAlCrear = false,
            VisibleEnEdicion = true
            )
        ]
        public int idAccion { get; set; }

        [IUPropiedad(Visible = false)]
        public string Accion { get; set; }


        //-------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Permiso",
            Ayuda = "Permiso de uso de la plantilla",
            EditableAlEditar = false,
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Permiso),
            Controlador = nameof(enumControladoresSeguridad.Permisos),
            VistaDondeNavegar = enumVistasSeguridad.CrudPermiso,
            Tipo = typeof(int),
            Fila = 2,
            Columna = 1,
            Obligatorio = false,
            VisibleAlCrear = false,
            VisibleEnEdicion = true
            )
        ]
        public int? IdPermiso { get; set; }

        [IUPropiedad(Visible = false)]
        public string Permiso { get; set; }

        //----------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Plantilla actual",
            Ayuda = "Descarga la plantilla en una nueva pesataña",
            Obligatorio = false,
            Fila = 3,
            Columna = 0,
            Posicion = 0,
            TipoDeControl = enumTipoControl.Referencia,
            css = enumCssControles.ReferenciaCentradaEnTd,
            VisibleAlCrear = false,
            AccionRef = "javascript:" + nameof(enumNameSpaceTs.Negocio) + "." + nameof(enumFunctionTs.Negocio_DescargarPlantillaDeNegocio) + "(["+nameof(IdArchivo)+"])"
            )
        ]
        public string Plantilla { get; set; }

        //----------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Etiquetas disponibles",
            Ayuda = "Descarga las etiquetas disponibles para confeccionar una plantilla",
            Obligatorio = false,
            Fila = 3,
            Columna = 0,
            Posicion = 1,
            TipoDeControl = enumTipoControl.Referencia,
            css = enumCssControles.ReferenciaCentradaEnTd,
            VisibleAlCrear = false,
            AccionRef = "javascript:" + nameof(enumNameSpaceTs.Negocio) + "." + nameof(enumFunctionTs.Negocio_DescargarEtiquetasDeNegocio) + "([" + nameof(IdNegocio) + "])"
            )
        ]
        public string Etiquetas { get; set; }

        //------------------------------------------------------------------------
        [IUPropiedad(
            VisibleEnGrid = false,
            Etiqueta = "Archivo",
            Ayuda = "Subir plantilla",
            Tipo = typeof(int),
            TipoDeControl = enumTipoControl.SelectorDeUnArchivo,
            ExtensionesValidas = "." + nameof(enumExtensiones.docx),
            Fila = 3,
            Columna = 1,
            AutoSpan = true)]
        public int? IdArchivo { get; set; }



    }
}
