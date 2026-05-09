using Utilidades;


namespace ModeloDeDto.Terceros
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, OpcionDeBorrar = false)]
    public class ParametrosDeMiSociedadDto : EsUnaAmpliacionDto
    {     
        //-------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Inscrito en",
            Ayuda = "indique los datos de la inscripción mercantil",
            TipoDeControl = enumTipoControl.Editor,
            Fila = 0,
            Columna = 0,
            Obligatorio = false,
            VisibleEnEdicion = true
          )
        ]
        public string  InscritoEn { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Pie de presupuesto",
           Ayuda = "Indique que mostrar en la impresión del pie de una factura",
           TipoDeControl = enumTipoControl.AreaDeTexto,
           Fila = 1,
           Columna = 0,
           Obligatorio = false)
        ]
        public string PieDePresupuesto { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Pie de factura",
           Ayuda = "Indique que mostrar en la impresión del pie de una factura",
           TipoDeControl = enumTipoControl.AreaDeTexto,
           Fila = 2,
           Columna = 0,
           Obligatorio = false)
        ]
        public string PieDeFactura { get; set; }
    }
}
