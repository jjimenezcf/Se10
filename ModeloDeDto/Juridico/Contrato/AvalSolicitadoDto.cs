using Utilidades;


namespace ModeloDeDto.Juridico
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, OpcionDeBorrar = false)]
    public class AvalSolicitadoDto : EsUnaAmpliacionDto
    {
        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Importe del aval",
           Tipo = typeof(decimal),
           Ayuda = "Indique el importe del aval solicitado",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           Formato = enumFormato.Moneda,
           Fila = 0,
           Columna = 0)
        ]
        public decimal ImporteAval { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Avisar antes de los x meses de su devolución",
           Tipo = typeof(int),
           Ayuda = "cuando se debe avisar que hay que devolver el importe",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           Obligatorio = false,
           Fila = 0,
           Columna = 1)
        ]
        public int? MesesDeAval { get; set; }

        //----------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Aviso enviado",
            Ayuda = "indica si se ha enviado el aviso",
            Fila = 1,
            Columna = 1,
            TipoDeControl = enumTipoControl.Check,
            CssDelContenedor = enumCssControles.ContenedorCheckRight,
            css = enumCssControles.ControlApilado,
            Alineada = enumAliniacion.derecha,
            MantenerHuecoDeLaIzquierda = true,
            ValorPorDefecto = false,
            EditableAlEditar = false
            )
        ]
        public bool? AvisoEnviado { get; set; }
    }
}
