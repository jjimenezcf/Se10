using Utilidades;

namespace ModeloDeDto
{
    [IUDtoAttribute(MostrarExpresion = nameof(Suceso), OpcionDeCrear = false)]
    public class DetalleDeUnSucesoDto: ElementoDto
    {
        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Suceso",
            Ayuda = "nombre del suceso",
            Tipo = typeof(string),
            Fila = 0,
            Columna = 0,
            EditableAlEditar = false
          )
        ]
        public string Suceso { get; set; }

        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Detalle",
           Ayuda = "detalle del suceso",
           TipoDeControl = enumTipoControl.AreaDeTexto,
           Fila = 1,
           Columna = 0,
           EditableAlEditar = false
          )
        ]
        public string Detalle { get; set; }
    }
}
