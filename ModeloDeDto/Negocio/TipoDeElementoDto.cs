using Utilidades;
using ServicioDeDatos.Elemento;

namespace ModeloDeDto.Negocio
{
    public interface IPermisoDeInterventorDto
    {
        public int IdPermisoDeInterventor { get; set; }
        public string PermisoDeInterventor { get; set; }
    }

    public interface ITipoDto
    {
        public string Negocio { get; set; }       
        public int IdNegocio { get; set; }
        public int? IdPadre { get; set; }
        public string Padre { get; set; }
        public string Nombre { get; set; }
        public int? IdPermisoDeAdministrador { get; set; }
        public string PermisoDeAdministrador { get; set; }
        public int? IdPermisoDeGestor { get; set; }
        public string PermisoDeGestor { get; set; }
        public int? IdPermisoDeConsultor { get; set; }
        public string PermisoDeConsultor { get; set; }
        public string Sigla { get; set; }
        public string ClaseDeLibro { get; set; }
        public bool Activo { get; set; }
        public string Mascara { get; set; }
        public string Marcador { get; set; }
        public bool NombreModificable { get; set; }
        public bool PermiteCrear { get; set; }
        public bool EditarTrasCrear { get; set; }
        public bool HayClases { get; set; }

    }

    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, OpcionDeCrear = false, OpcionDeBorrar = false)]
    public class TipoDeElementoDto : ElementoDto, ITipoDto
    {
        public static string ExpresionElemento = $"{nameof(Nombre)}";
        //-------------------------------------------------------------------------------------------------------
        [IUPropiedad(Visible = false)]
        public int IdNegocio { get; set; }

        //------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Negocio",
            Ayuda = "Negocio",
            Tipo = typeof(string),
            Fila = 0,
            Columna = 0,
            VisibleEnEdicion = true,
            EditableAlCrear = false,
            EditableAlEditar = false
          )
        ]
        public string Negocio { get; set; }

        //------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Id del tipo padre",
            Visible = false
            )
        ]
        public int? IdPadre { get; set; }

        [IUPropiedad(
            Etiqueta = "Padre",
            Ayuda = "Padre del tipo documental",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(TipoDeElementoDto),
            GuardarEn = nameof(IdPadre),
            Controlador = nameof(enumControladoresNegocio.TiposDeElemento),
            LongitudMinimaParaBuscar = 1,
            Tipo = typeof(string),
            Fila = 0,
            Columna = 1,
            Obligatorio = false,
            Ordenar = true,
            VisibleEnEdicion = true,
            EditableAlCrear = true,
            EditableAlEditar = true
            )
        ]
        public string Padre { get; set; }

        //------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Nombre",
            Ayuda = "Indique el tipo de proceso",
            Tipo = typeof(string),
            Fila = 1,
            Columna = 0,
            Posicion = 0,
            Obligatorio = true,
            VisibleEnEdicion = true,
            LongitudMaxima = 250,
            AutoSpan = true
          )
        ]
        public string Nombre { get; set; }

        //------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Máscara",
            Ayuda = "Indique la expresión regular que ha de cumplir el nombre del elemento",
            Tipo = typeof(string),
            Fila = 1,
            Columna = 0,
            Posicion = 1,
            Obligatorio = false,
            VisibleEnEdicion = true,
            LongitudMaxima = 250,
            AutoSpan = true
          )
        ]
        public string Mascara { get; set; }

        //------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Marcador",
            Ayuda = "Explica la expresión regular",
            Tipo = typeof(string),
            Fila = 1,
            Columna = 0,
            Posicion = 2,
            Obligatorio = false,
            VisibleEnEdicion = true,
            LongitudMaxima = 250,
            AutoSpan = true
          )
        ]
        public string Marcador { get; set; }

        //-------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Administrador",
            Ayuda = "Permiso de administrador",
            EditableAlEditar = false,
            Tipo = typeof(int),
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(PermisoDeAdministrador),
            Controlador = nameof(enumControladoresSeguridad.Permisos),
            VistaDondeNavegar = enumVistasSeguridad.CrudPermiso,
            Fila = 3,
            Columna = 0,
            Obligatorio = false,
            VisibleAlCrear = false,
            VisibleEnEdicion = true,
            AutoSpan = true
            )
        ]
        public int? IdPermisoDeAdministrador { get; set; }

        [IUPropiedad(Visible = false)]
        public string PermisoDeAdministrador { get; set; }

        //-------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Gestión",
            Ayuda = "Permiso de gestión",
            EditableAlEditar = false,
            Tipo = typeof(int),
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(PermisoDeGestor),
            Controlador = nameof(enumControladoresSeguridad.Permisos),
            VistaDondeNavegar = enumVistasSeguridad.CrudPermiso,
            Fila = 4,
            Columna = 0,
            Obligatorio = false,
            VisibleAlCrear = false,
            VisibleEnEdicion = true,
            AutoSpan = true
            )
        ]
        public int? IdPermisoDeGestor { get; set; }
        [IUPropiedad(Visible = false)]
        public string PermisoDeGestor { get; set; }

        //-------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Consulta",
            Ayuda = "Permiso de consulta",
            EditableAlEditar = false,
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(PermisoDeConsultor),
            Controlador = nameof(enumControladoresSeguridad.Permisos),
            VistaDondeNavegar = enumVistasSeguridad.CrudPermiso,
            Tipo = typeof(int),
            Fila = 5,
            Columna = 0,
            Obligatorio = false,
            VisibleAlCrear = false,
            VisibleEnEdicion = true,
            AutoSpan = true
            )
        ]
        public int? IdPermisoDeConsultor { get; set; }
        [IUPropiedad(Visible = false)]
        public string PermisoDeConsultor { get; set; }

        //------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Sigla",
            Ayuda = "Serie del libro de registro",
            Tipo = typeof(string),
            Fila = 2,
            Columna = 0,
            Obligatorio = false,
            VisibleEnEdicion = true,
            LongitudMaxima = 5
          )
        ]
        public string Sigla { get; set; }
        //------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Libro de registro",
            Ayuda = "Indique como se referenciaran los elementos del tipo",
            TipoDeControl = enumTipoControl.Enumerado,
            Tipo = typeof(enumClaseDeLibro),
            GuardarEn = nameof(ClaseDeLibro),
            Fila = 2,
            Columna = 1,
            Obligatorio = false,
            VisibleEnEdicion = true,
            LongitudMaxima = 250
          )
        ]
        public string ClaseDeLibro { get; set; }

        //-------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Tipo activo",
            Ayuda = "indica si el tipo está activo",
            VisibleEnGrid = false,
            VisibleAlCrear = false,
            EditableAlCrear = false,
            Fila = 7,
            Columna = 0,
            TipoDeControl = enumTipoControl.Check,
            ValorPorDefecto = true,
            VisibleEnEdicion = true,
            css = enumCssControles.ControlApilado
            )
        ]
        public bool Activo { get; set; }

        //-------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Nombre modificable",
            Ayuda = "indica se puede modificar el nombre del elemento tras crearlo ",
            VisibleEnGrid = false,
            VisibleAlCrear = true,
            VisibleEnEdicion = true,
            Obligatorio = true,
            Fila = 12,
            Columna = 0,
            TipoDeControl = enumTipoControl.Check,
            ValorPorDefecto = true,
            css = enumCssControles.ControlApilado
            )
        ]
        public new bool NombreModificable { get; set; }

        //-------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Permite crear",
            Ayuda = "indica se puede crear elementos de esta tipología ",
            VisibleEnGrid = false,
            VisibleAlCrear = true,
            VisibleEnEdicion = true,
            Obligatorio = true,
            Fila = 13,
            Columna = 0,
            Posicion = 0,
            TipoDeControl = enumTipoControl.Check,
            ValorPorDefecto = true,
            css = enumCssControles.ControlApilado
            )
        ]
        public bool PermiteCrear { get; set; }

        //-------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Editar tras crear",
            Ayuda = "indica si se ha de editar el elemento tras crearlo ",
            VisibleEnGrid = false,
            VisibleAlCrear = true,
            VisibleEnEdicion = true,
            Obligatorio = true,
            Fila = 14,
            Columna = 0,
            Posicion =0,
            TipoDeControl = enumTipoControl.Check,
            ValorPorDefecto = true,
            css = enumCssControles.ControlApilado
            )
        ]
        public bool EditarTrasCrear { get; set; }

        //------------------------------------------------------
        [IUPropiedad(Visible = false)]
        public bool HayClases { get ; set; }
    }

}
