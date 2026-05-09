using Utilidades;

namespace ModeloDeDto.SistemaDocumental
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class BloqueoMultipleDto : ElementoDto
    {

        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Motivo",
            TipoDeControl = enumTipoControl.Editor,
            EditableAlEditar = true,
            Fila = 2,
            Columna = 0,
            AutoSpan = true
            )
        ]
        public string Motivo { get; set; }
    }
}
