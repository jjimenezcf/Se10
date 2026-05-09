using Utilidades;

namespace ModeloDeDto.Negocio
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, OpcionDeBorrar = false)]
    public class ParametroDeNegocioDto : ElementoDto
    {
        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Negocio",
            Ayuda = "negocio del parámetro",
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

        //--------------------------------------------
        [IUPropiedad(Visible = false)]
        public string Negocio { get; set; }

        //--------------------------------------------
        [IUPropiedad(
          Etiqueta = "Parámetro",
          Ayuda = "Indique el nombre del parámetro",
          Tipo = typeof(string),
          Fila = 0,
          Columna = 0,
          Ordenar = true,
          TamanoFijo = "15em",
          EditableAlEditar = false
          )
        ]
        public string Nombre { get; set; }

        //--------------------------------------------
        [IUPropiedad(
          Etiqueta = "Descripción",
          Tipo = typeof(string),
          Fila = 1,
          Columna = 0,
          EditableAlCrear = false,
          EditableAlEditar = false,
          Obligatorio = false
          )
        ]
        public string Descripcion { get; set; }

        //--------------------------------------------
        [IUPropiedad(
          Etiqueta = "Valor",
          Ayuda = "Asigne un valor al parámetro",
          TipoDeControl = enumTipoControl.AreaDeTexto,
          Tipo = typeof(string),
          Fila = 2,
          Columna = 0,
          NumeroDeFilas = 5,
          LongitudMaxima = 2000,
          AutoSpan = true
          )
        ]
        public string Valor { get; set; }

    }
}
