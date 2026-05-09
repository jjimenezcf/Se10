using ModeloDeDto.Negocio;
using ServicioDeDatos.Negocio;
using Utilidades;

namespace ModeloDeDto
{

    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class ClaseDelTipoDto: ElementoDto
    {       
        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Negocio",
            Ayuda = "clases del tipo del negocio",
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

        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Tipo",
            Ayuda = "tipo de negocio",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Tipo),
            Fila = 0,
            Columna = 1,
            EditableAlCrear = false,
            EditableAlEditar = false,
            VisibleEnGrid = false
            )
        ]
        public int IdTipo { get; set; }

        [IUPropiedad(Visible = false)]
        public string Tipo { get; set; }

        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Clase",
            Ayuda = "Clase del tipo",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Clase),
            Fila = 1,
            Columna = 0,
            VisibleAlCrear = false,
            EditableAlEditar = false
            )
        ]
        public int IdClase { get; set; }

        [IUPropiedad(Visible = false)]
        public string Clase { get; set; }


        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Clase",
            Ayuda = "seleccionar clase",
            TipoDeControl = enumTipoControl.ListaDeElemento,
            SeleccionarDe = typeof(ClaseDelNegocioDtm),
            Controlador = nameof(enumControladoresNegocio.ClasesDelNegocio),
            RestrictorFijo =nameof(IdNegocio),
            RestringidoPorControl =nameof(IdTipo),
            GuardarEn = nameof(IdClase),
            MostrarExpresion = nameof(ClaseDelNegocioDto.Expresion),
            VisibleAlCrear = true,
            VisibleAlEditar = false,
            AutoSpan = true,
            Fila = 1,
            Columna = 0
          )
        ]
        public string Clases { get; set; }

        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(Etiqueta = "Activa", Visible = false)]
        public bool Activa { get; set; }

    }
}
