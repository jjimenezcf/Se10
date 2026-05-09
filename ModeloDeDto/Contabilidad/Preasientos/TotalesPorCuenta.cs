using Utilidades;

namespace ModeloDeDto.Contabilidad
{
    [IUDto()]
    public class TotalesPorCuenta: TotalesDto
    {
        //-----------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Saldos de cuenta contables",
           Ayuda = "muestra la información de los totales calculados",
           TipoDeControl = enumTipoControl.AreaDeTexto,
           EditableAlCrear = false,
           EditableAlEditar = false,
           CssDelArea = enumCssControles.MonoSpaceText,
           NumeroDeFilas = 10,
           Fila = 0,
           Columna = 0
            )
        ]
        public string TotalesPorCuentas { get; set; }

        //-----------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Totales por grupos contables",
           Ayuda = "muestra la información de los totales calculados",
           TipoDeControl = enumTipoControl.AreaDeTexto,
           EditableAlCrear = false,
           EditableAlEditar = false,
           CssDelArea = enumCssControles.MonoSpaceText,
           NumeroDeFilas = 10,
           Fila = 1,
           Columna = 0
            )
        ]
        public string TotalesPorGrupo { get; set; }
    }
}
