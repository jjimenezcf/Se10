using System;
using Utilidades;

namespace ModeloDeDto.SistemaDocumental
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class DatosBloqueoDto : ElementoDto
    {
        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Archivo",
            Ayuda = "Archivo a bloquear",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Archivo),
            Fila = 0,
            Columna = 0,
            VisibleEnGrid = false
            )
        ]
        public int IdArchivo { get; set; }

        [IUPropiedad(Etiqueta = "Archivo", Visible = false)]
        public string Archivo { get; set; }


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

        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Auditoría",
            Ayuda = "Muestra el historial de bloqueo y desbloqueo de un archivo",
            TipoDeControl = enumTipoControl.AreaDeTexto,
            EditableAlEditar = false,
            Fila = 3,
            Columna = 0
            )
        ]
        public DateTime Auditoria { get; set; }

    }
}
