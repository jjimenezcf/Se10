namespace ModeloDeDto.MaestrosTecnico
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, MostrarExpresion = "[Nombre]")]
    public class UnidadDto : ElementoDto
    {
        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Sigla",
            Ayuda = "Indique las siglas de la unidad de medida",
            Tipo = typeof(string),
            Fila = 0,
            Columna = 0,
            Ordenar = true,
            Obligatorio = true,
            LongitudMaxima = 5
          )
        ]
        public string Sigla { get; set; }
        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Unidad de medida",
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
