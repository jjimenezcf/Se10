using System;
using Utilidades;

namespace ModeloDeDto.SistemaDocumental
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class FirmadoDto : ElementoDto
    {
        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Original",
            TipoDeControl = enumTipoControl.Editor,
            EditableAlEditar =false,
            Fila = 0,
            Columna = 0
            )
        ]
        public string Original { get; set; }

        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Firmado",
            TipoDeControl = enumTipoControl.Editor,
            EditableAlEditar = false,
            Fila = 0,
            Columna = 1
            )
        ]
        public string Firmado { get; set; }

        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Usuario",
            TipoDeControl = enumTipoControl.Editor,
            EditableAlEditar = false,
            Fila = 1,
            Columna = 0
            )
        ]
        public string Usuario { get; set; }

        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Certificado",
            TipoDeControl = enumTipoControl.Editor,
            EditableAlEditar = false,
            Fila = 1,
            Columna = 1
            )
        ]
        public string Certificado { get; set; }

        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Motivo",
            TipoDeControl = enumTipoControl.Editor,
            EditableAlEditar = false,
            Fila = 2,
            Columna = 0,
            AutoSpan = true
            )
        ]
        public string Motivo { get; set; }
        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Firmado el",
            TipoDeControl = enumTipoControl.SelectorDeFechaHora,
            EditableAlEditar = false,
            Fila = 3,
            Columna = 0
            )
        ]
        public DateTime FirmadoEl { get; set; }

    }
}
