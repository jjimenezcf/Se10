using Utilidades;

namespace ModeloDeDto.SistemaDocumental
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class DescargaConGuidDto : ElementoDto
    {
        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Archivo",
            TipoDeControl = enumTipoControl.Editor,
            EditableAlEditar =false,
            Fila = 0,
            Columna = 0
            )
        ]
        public string Archivo { get; set; }

        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Url de descarga",
            Ayuda = "Copie esta url, habilítela y envíela a la persona que quiere que descargue el archivo",
            TipoDeControl = enumTipoControl.Editor,
            EditableAlEditar = false,
            Fila = 0,
            Columna = 1
            )
        ]
        public string Url { get; set; }

    }
}
