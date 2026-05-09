using Utilidades;

namespace ModeloDeDto.Terceros
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, MostrarExpresion = nameof(Expresion))]
    public class ContactoDto : ElementoDto, IUsaNombreDto, IUsaBajaDto
    {
        //----------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Sociedad",
            Ayuda = "Sociedad del contacto",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Sociedad),
            Fila = 0,
            Columna = 0,
            EditableAlCrear = false,
            EditableAlEditar = false,
            VisibleEnGrid = false
            )
        ]
        public int IdElemento { get; set; }

        [IUPropiedad(Visible = false)]
        public string Sociedad { get; set; }

        //----------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Contacto",
            Ayuda = "nombre del del contacto",
            Tipo = typeof(string),
            Fila = 0,
            Columna = 0,
            Obligatorio = true,
            LongitudMaxima = 250,
            AutoSpan = true
          )
        ]
        public string Nombre { get; set; }

        //------------------------------------------------------------
        [IUPropiedad(
           Etiqueta = "eMail",
           Ayuda = "indique el mail",
           TipoDeControl = enumTipoControl.Editor,
           LongitudMaxima = 50,
           Fila = 1,
           Columna = 0
          )
        ]
        public string eMail { get; set; }

        //---------------------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Teléfono",
           Ayuda = "indique el teléfono",
           TipoDeControl = enumTipoControl.Editor,
           LongitudMaxima = 15,
           VisibleEnGrid = false,
           Obligatorio = false,
           Fila = 1,
           Columna = 1
          )
        ]
        public string Telefono { get; set; }

        //--------------------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Detalle",
           Ayuda = "Detalle",
           TipoDeControl = enumTipoControl.AreaDeTexto,
           Fila = 2,
           Columna = 0,
           AutoSpan = true
           )
        ]
        public string Descripcion { get; set; }

        //----------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Crear interlocutor",
            Ayuda = "Indica si al crear el contacto se crea el interlocutor",
            VisibleEnGrid = false,
            Obligatorio = false,
            Fila = 3,
            Columna = 0,
            TipoDeControl = enumTipoControl.Check,
            css = enumCssControles.ControlApilado,
            ValorPorDefecto = false,
            EditableAlCrear = true,
            EditableAlEditar = true
            )
        ]

        public bool CrearInterlocutor { get; set; }


        //----------------------------------------------------------------
        [IUPropiedad(Visible = false)]
        public int ? IdInterlocutor { get; set; }


        //----------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Es interlocutor",
            Ayuda = "indica si el contacto es un interlocutor",
            Fila = 3,
            Columna = 0,
            TipoDeControl = enumTipoControl.Check,
            css = enumCssControles.ControlApilado,
            ValorPorDefecto = false,
            VisibleEnGrid = false,
            VisibleAlCrear = false,
            VisibleAlEditar = true,
            EditableAlEditar = false
            )
        ]
        public bool EsInterlocutor { get { return IdInterlocutor != null && IdInterlocutor > 0; }}

        //----------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Baja",
            Ayuda = "indica si el contacto está de baja",
            VisibleEnGrid = false,
            Obligatorio = false,
            Fila = 4,
            Columna = 0,
            TipoDeControl = enumTipoControl.Check,
            css = enumCssControles.ControlApilado,
            ValorPorDefecto = false,
            VisibleAlCrear = false,
            EditableAlCrear = false,
            EditableAlEditar = false
            )
        ]
        public bool Baja { get; set; }


        //----------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Está de Baja",
            css = enumCssControles.ControlApilado,
            VisibleAlEditar  = false,
            VisibleAlCrear  = false,
            VisibleEnGrid = true
            )
        ]
        public string EstaDeBaja => Baja ? nameof(extCadenas.enumCadenas.Si) : nameof(extCadenas.enumCadenas.No);

        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Expresión de la sociedad",
            Tipo = typeof(string),
            Visible = false
          )
        ]
        public string Expresion { get; set; }
    }
}
