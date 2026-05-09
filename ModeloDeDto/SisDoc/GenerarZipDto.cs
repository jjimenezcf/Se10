using Utilidades;

namespace ModeloDeDto.SistemaDocumental
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class GenerarZipDto : ElementoDto
    {

        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Nombre archivador",
            TipoDeControl = enumTipoControl.Editor,
            EditableAlEditar = true,
            Fila = 2,
            Columna = 0,
            AutoSpan = true
            )
        ]
        public string Nombre { get; set; }


    }
}
