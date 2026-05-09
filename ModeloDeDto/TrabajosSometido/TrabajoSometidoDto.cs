using Utilidades;
using ModeloDeDto.Entorno;
using ModeloDeDto.Seguridad;

namespace ModeloDeDto.TrabajosSometidos
{
    [IUDto(AnchoEtiqueta = 20
          , AnchoSeparador = 5)]
    public class TrabajoSometidoDto : ElementoDto
    {
        internal const string ExpresionElemento = nameof(Nombre);

        [IUPropiedad(
          Etiqueta = "Trabajo",
          Ayuda = "Indique el nombre del trabajo sometido",
          Tipo = typeof(string),
          Fila = 0,
          Columna = 0,
          Ordenar = true,
          PorAnchoMnt = 50,
            AutoSpan = true
          )
        ]
        public string Nombre { get; set; }

        [IUPropiedad(
            Etiqueta = "Es una DLL",
            Ayuda = "Indique si es una ddl o PA",
            Tipo = typeof(bool),
            TipoDeControl = enumTipoControl.Check,
            Fila = 1,
            Columna = 0,
            ValorPorDefecto = true,
            VisibleEnGrid = false
          )
        ]
        public string EsDll { get; set; }

        [IUPropiedad(
            Etiqueta = "Programa",
            Tipo = typeof(string),
            Obligatorio = false,
            VisibleEnGrid = true,
            VisibleEnEdicion = false
            )
        ]
        public string Programa { get; set; }

        [IUPropiedad(
            Etiqueta = "Dll",
            Ayuda = "indica el assembly",
            TipoDeControl = enumTipoControl.Editor,
            Tipo = typeof(string),
            Fila = 2,
            Columna = 0,
            Obligatorio = false,
            VisibleEnGrid = false
            )
        ]
        public string Dll { get; set; }

        [IUPropiedad(
            Etiqueta = "Clase",
            Ayuda = "indica espacio de nombre y clase",
            TipoDeControl = enumTipoControl.Editor,
            Tipo = typeof(string),
            Fila = 2,
            Columna = 1,
            Obligatorio = false,
            VisibleEnGrid = false
            )
        ]
        public string Clase { get; set; }

        [IUPropiedad(
            Etiqueta = "Método",
            Ayuda = "indica el método",
            TipoDeControl = enumTipoControl.Editor,
            Tipo = typeof(string),
            Fila = 2,
            Columna = 2,
            Obligatorio = false,
            VisibleEnGrid = false
            )
        ]
        public string Metodo { get; set; }

        [IUPropiedad(
            Etiqueta = "Esquema",
            Ayuda = "indica el esquema",
            TipoDeControl = enumTipoControl.Editor,
            Tipo = typeof(string),
            Fila = 3,
            Columna = 0,
            Obligatorio = false,
            VisibleEnGrid = false
            )
        ]
        public string Esquema { get; set; }

        [IUPropiedad(
            Etiqueta = "PA",
            Ayuda = "indica procedimiento almacenado",
            TipoDeControl = enumTipoControl.Editor,
            Tipo = typeof(string),
            Fila = 3,
            Columna = 1,
            Obligatorio = false,
            VisibleEnGrid = false
            )
        ]
        public string PA { get; set; }

        [IUPropiedad(
            Etiqueta = "Comunicar fin",
            Ayuda = "Indique si se ha decomunicar el fin de ejecución",
            Tipo = typeof(bool),
            TipoDeControl = enumTipoControl.Check,
            Fila = 4,
            Columna = 0,
            ValorPorDefecto = false,
            VisibleEnGrid = false,
            css = Utilidades.enumCssControles.ControlApilado
          )
        ]
        public string ComunicarFin { get; set; }

        [IUPropiedad(
            Etiqueta = "Comunicar error",
            Ayuda = "Indique si se ha decomunicar si hay errores",
            Tipo = typeof(bool),
            TipoDeControl = enumTipoControl.Check,
            Fila = 5,
            Columna = 0,
            ValorPorDefecto = true,
            VisibleEnGrid = false,
            css = Utilidades.enumCssControles.ControlApilado
          )
        ]
        public string ComunicarError { get; set; }

        [IUPropiedad(
          Etiqueta = "Id del usuario ejecutor",
          Visible = false
          )
        ]
        public int? idEjecutor { get; set; }

        [IUPropiedad(
            Etiqueta = "Usuario ejecutor",
            Ayuda = "Indique quién ejecuta el trabajo, si es distinto al sometedor",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(UsuarioDto),
            MostrarExpresion = UsuarioDto.ExpresionElemento,
            GuardarEn = nameof(idEjecutor),
            Controlador = nameof(enumControladoresEntorno.Usuarios),
            VistaDondeNavegar = nameof(enumVistasEntorno.CrudUsuario),
            Fila = 6,
            Columna = 0,
            Ordenar = false,
            Obligatorio = false,
            VisibleEnGrid = false,
            AutoSpan = true
            )
        ]
        public string Ejecutor { get; set; }

        [IUPropiedad(
          Etiqueta = "Id del puesto de trabajo a informar",
          Visible = false
          )
        ]
        public int? idInformarA { get; set; }

        [IUPropiedad(
            Etiqueta = "Puesto de trabajo",
            Ayuda = "Indique el puesto de trabajo al que se ha de informa",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(PuestoDto),
            GuardarEn = nameof(idInformarA),
            MostrarExpresion = PuestoDto.ExpresionPt,
            Controlador = nameof(enumControladoresSeguridad.PuestoDeTrabajo),
            VistaDondeNavegar = nameof(enumVistasEntorno.CrudPuestoDeTrabajo),
            Fila = 7,
            Columna = 0,
            Ordenar = false,
            Obligatorio = false,
            VisibleEnGrid = false,
            AutoSpan = true
            )
        ]
        public string InformarA { get; set; }

    }
}
