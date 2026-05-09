using Utilidades;

namespace ModeloDeDto.Callejero
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, MostrarExpresion = "[Codigo]")]
    public class CodigoPostalDto : ElementoDto
    {
        [IUPropiedad(
            Etiqueta = "CP",
            Ayuda = "Código postal",
            Tipo = typeof(string),
            Fila = 0,
            Columna = 0,
            Ordenar = true,
            Obligatorio = true,
            LongitudMaxima = 5,
            Alineada = enumAliniacion.izquierda
          )
        ]
        public string Codigo { get; set; }
        //----------------------------------------------------------------

        [IUPropiedad(Etiqueta = "Provincia"
            , VisibleAlCrear = false
            , EditableAlEditar = false
            , Fila = 0
            , Columna = 1
            , Obligatorio = false)]
        public string Provincia { get; set; }
        //----------------------------------------------------------------

        [IUPropiedad(Etiqueta = "Municipios"
            , TipoDeControl = enumTipoControl.AreaDeTexto
            , VisibleAlCrear = false
            , EditableAlEditar = false
            , Fila = 1
            , Columna = 0
            , AutoSpan = true
            , Obligatorio = false)]
        public string Municipios { get; set; }
    }
}
