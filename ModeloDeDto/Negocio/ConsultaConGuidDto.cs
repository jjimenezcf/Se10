using Utilidades;

namespace ModeloDeDto.Negocio
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class ConsultaConGuidDto : ElementoDto
    {
        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Url de consulta",
            Ayuda = "Copie esta url, habilítela y envíela a la persona que quiere consultar el elemento",
            TipoDeControl = enumTipoControl.Editor,
            EditableAlEditar = false,
            Fila = 0,
            Columna = 1
            )
        ]
        public string Url { get; set; }

    }
}
