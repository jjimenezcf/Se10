
namespace ModeloDeDto.Terceros
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class UsuarioDeClienteNew : EsUnDetalleDto
    {

        //------------------------------------------------------------------------
        [IUPropiedad(
            VisibleEnGrid = false,
            Etiqueta = "Usuario",
            Ayuda = "Usuario de conexión",
            Tipo = typeof(string),
            Fila = 0,
            Columna = 2
            )
        ]
        public string Login { get; set; }

        //------------------------------------------------------------------------
        [IUPropiedad(
            VisibleEnGrid = false,
            Etiqueta = "Apellidos",
            Ayuda = "Apellidos",
            Tipo = typeof(string),
            Fila = 1,
            Columna = 0
            )
        ]
        public string Apellido { get; set; }

        //------------------------------------------------------------------------
        [IUPropiedad(
            VisibleEnGrid = false,
            Etiqueta = "Nombre",
            Ayuda = "Nombre",
            Tipo = typeof(string),
            Fila = 1,
            Columna = 1
            )
        ]
        public string Nombre { get; set; }

        //------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "eMail",
            Ayuda = "eMail",
            Tipo = typeof(string),
            Fila = 1,
            Columna = 2
            )
        ]
        public string eMail { get; set; }

    }
}
