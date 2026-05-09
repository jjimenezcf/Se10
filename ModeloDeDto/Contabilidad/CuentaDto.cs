namespace ModeloDeDto.Contabilidad
{

    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, MostrarExpresion = nameof(Expresion))]
    public class CuentaDto : ElementoDto
    {
        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Código",
            Ayuda = "Indique el código de cuenta",
            Tipo = typeof(string),
            Fila = 0,
            Columna = 0,
            Ordenar = true,
            Obligatorio = true,
            LongitudMaxima = 10
          )
        ]
        public string Codigo { get; set; }
        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Cuenta contable",
            Ayuda = "descripción de la cuenta contable",
            Tipo = typeof(string),
            Fila = 0,
            Columna = 1,
            Ordenar = true,
            Obligatorio = true,
            LongitudMaxima = 250
          )
        ]
        public string Nombre { get; set; }


        [IUPropiedad(Visible = false)]
        public string Expresion { get; set; }

    }
}
