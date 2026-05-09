using Utilidades;
using ModeloDeDto.Negocio;

namespace ModeloDeDto.SistemaDocumental
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class TipoDeArchivadorDto: TipoDeElementoDto
    {
        //-------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Visible en extranet",
            Ayuda = "indica si el archivador es visible en la extranet",
            VisibleEnGrid = false,
            VisibleAlCrear = true,
            VisibleEnEdicion = true,
            Obligatorio = true,
            Fila = 8,
            Columna = 0,
            TipoDeControl = enumTipoControl.Check,
            ValorPorDefecto = true,
            css = enumCssControles.ControlApilado
            )
        ]
        public bool Visible { get; set; }

        //-------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Del sistema",
            Ayuda = "indica si el archivador lo puede crear un usuario, o modificar sus datos identificativos",
            VisibleEnGrid = false,
            VisibleAlCrear = true,
            VisibleEnEdicion = true,
            Obligatorio = true,
            Fila = 9,
            Columna = 0,
            TipoDeControl = enumTipoControl.Check,
            ValorPorDefecto = false,
            css = enumCssControles.ControlApilado
            )
        ]
        public new bool DelSistema { get; set; }

    }
}
