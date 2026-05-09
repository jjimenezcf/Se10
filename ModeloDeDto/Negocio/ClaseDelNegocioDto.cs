using Utilidades;

namespace ModeloDeDto.Negocio
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class ClaseDelNegocioDto : ElementoDto
    {
        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Negocio",
            Ayuda = "negocio de la clase",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Negocio),
            Fila = 0,
            Columna = 0,
            EditableAlCrear = false,
            EditableAlEditar = false,
            VisibleEnGrid = false
            )
        ]
        public int IdNegocio { get; set; }

        [IUPropiedad(Visible = false)]
        public string Negocio { get; set; }

        //--------------------------------------------
        [IUPropiedad(
          Etiqueta = "Referencia",
          Ayuda = "Indique la referencia de la clase",
          Tipo = typeof(string),
          Fila = 0,
          Columna = 1
          )
        ]
        public string Referencia { get; set; }

        //--------------------------------------------
        [IUPropiedad(
          Etiqueta = "Clase",
          Ayuda = "Indique el nombre de la clase",
          Tipo = typeof(string),
          Fila = 1,
          Columna = 0,
          AutoSpan = true
          )
        ]
        public string Nombre { get; set; }

        //------------------------------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Activa",
           Ayuda = "indica si la la clase es usable",
           VisibleEnGrid = false,
           Obligatorio = false,
           Fila = 2,
           Columna = 0,
           TipoDeControl = enumTipoControl.Check,
           css = enumCssControles.ControlApilado,
           ValorPorDefecto = true,
           VisibleAlCrear = false,
           EditableAlEditar = true
           )]
        public bool Activa { get; set; }

        //------------------------------------------------------------------------
        [IUPropiedad(Visible = false)]
        public string Expresion { get; set; }

    }
}
