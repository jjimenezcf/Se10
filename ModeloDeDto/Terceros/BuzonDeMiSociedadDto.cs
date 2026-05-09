using Utilidades;

namespace ModeloDeDto.Terceros
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class BuzonDeMiSociedadDto : EsUnDetalleDto
    {

        //-------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "eMail",
            Ayuda = "Cuenta de eMail",
            TipoDeControl = enumTipoControl.Editor,
            Fila = 1,
            Columna = 0
          )
        ]
        public string eMail { get; set; }

        //-------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Buzón",
            Ayuda = "nombre del buzón",
            TipoDeControl = enumTipoControl.Editor,
            Fila = 1,
            Columna =1
          )
        ]
        public string Buzon { get; set; }

        //-------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Orden",
            Ayuda = "Orden de presentación en la lista de pantalla",
            TipoDeControl = enumTipoControl.Editor,
            Fila = 1,
            Columna = 2
          )
        ]
        public int Orden { get; set; }

        //----------------------------------------------------------
        [IUPropiedad(Etiqueta = "Permiso",
            Ayuda = "Permiso del buzón",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Permiso),
            Fila = 2,
            Columna = 0,
            VisibleAlCrear = false,
            EditableAlEditar = true,
            AutoSpan = true
            )
        ]
        public int IdPermiso { get; set; }

        [IUPropiedad(Visible = false)]
        public string Permiso { get; set; }
    }
}
