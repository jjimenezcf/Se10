using Utilidades;

namespace ModeloDeDto.Entorno
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, OpcionDeCrear = false, OpcionDeBorrar = false)]
    public class VariableDto : ElementoDto
    {
        [IUPropiedad(
          Etiqueta = "Variable",
          Ayuda = "Indique el nombre de la variable",
          Tipo = typeof(string),
          Fila = 0,
          Columna = 0,
          Ordenar = true,
          EditableAlEditar = false,
          PorAnchoMnt = 50
          )
        ]
        public string Nombre { get; set; }

        [IUPropiedad(
          Etiqueta = "Descripción",
          Ayuda = "Describa el uso de la variable",
          Tipo = typeof(string),
          Fila = 1,
          Columna = 0,
          VisibleEnGrid = false
          )
        ]
        public string Descripcion { get; set; }

        [IUPropiedad(
          Etiqueta = "Valor",
          Ayuda = "Asigne un valor a la variable",
           TipoDeControl = enumTipoControl.AreaDeTexto,
           Fila = 2,
           Columna = 0,
           VisibleEnGrid = true,
           EditableAlCrear = true,
           EditableAlEditar = true,
           Obligatorio = false,
           NumeroDeFilas = 5,
           AutoSpan = true
          )
        ]
        public string Valor { get; set; }
    }
}
