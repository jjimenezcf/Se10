using Utilidades;

namespace ModeloDeDto.Contabilidad
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class ApunteDeUnPreasientoDto : EsUnDetalleDto
    {

        //-------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Posición",
            Ayuda = "posición del asiento",
            TipoDeControl = enumTipoControl.Enumerado,
            Tipo = typeof(enumPosicionContable),
            GuardarEn = nameof(Posicion),
            Fila = 1,Columna = 0,
            EditableAlEditar = false
          )
        ]
        public string Posicion { get; set; }

        //----------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Clase",
            Ayuda = "clase de la posición",
            TipoDeControl = enumTipoControl.Enumerado,
            Tipo = typeof(enumPosicionContable),
            GuardarEn = nameof(Clase),
            Fila = 1, Columna = 1,
            VisibleAlCrear = false,
            EditableAlEditar = false)]
        public enumClaseDeApunte Clase { get; set; }

        //----------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Tipo",
            Ayuda = "clase de la posición",
            TipoDeControl = enumTipoControl.Editor,
            Fila = 1, Columna = 2,
            VisibleAlCrear = false,
            EditableAlEditar = false)]
        public string Tipo { get; set; }

        //----------------------------------------------------------
        [IUPropiedad(Etiqueta = "Orden",
            Fila = 1, Columna = 3, 
            LongitudMaxima = 4,
            VisibleAlCrear = false,
            EditableAlEditar = false
            )
        ]
        public int Orden { get; set; }


        //----------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Cuenta",
            Ayuda = "cuenta contable",
            Fila = 2,
            Columna = 0,
            VisibleAlCrear = false,
            EditableAlEditar = false,
            AutoSpan = true
            )
        ]
        public string Cuenta { get; set; }

        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(
           TipoDeControl = enumTipoControl.Editor,
           Etiqueta = "Importe",
           Fila = 2, Columna = 1,
            VisibleAlCrear = false,
            EditableAlEditar = false,
           VisibleEnGrid = true)]
        public decimal Importe { get; set; }



        //----------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Concepto",
            Ayuda = "Concepto de la posición",
            Fila = 2,
            Columna = 3,
            VisibleAlCrear = false,
            EditableAlEditar = false,
            ColSpan = 2
            )
        ]
        public string Concepto { get; set; }

    }
}
