namespace ModeloDeDto.Callejero
{
    [IUDto(AnchoEtiqueta = 20
         , AnchoSeparador = 5
         , MostrarExpresion = "[Nombre]")]
    public class TipoDeViaDto : ElementoDto
    {

        [IUPropiedad(
            Etiqueta = "Sigla",
            Ayuda = "Sigla del tipo de la vía",
            Tipo = typeof(string),
            Fila = 0,
            Columna = 0,
            Ordenar = true,
            Obligatorio = true,
            LongitudMaxima = 10,
            PorAnchoMnt = 5,
            Alineada = enumAliniacion.izquierda
          )
        ]
        public string Sigla { get; set; }

        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Tipo de vía",
            Ayuda = "Indique el nombre",
            Tipo = typeof(string),
            Fila = 0,
            Columna = 1,
            Ordenar = true,
            Obligatorio = true,
            LongitudMaxima = 250
          )
        ]
        public string Nombre { get; set; }

    }
}
