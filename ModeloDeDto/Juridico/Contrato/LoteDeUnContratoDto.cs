using System;
using Utilidades;


namespace ModeloDeDto.Juridico
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, MostrarExpresion = "Nombre", EditarTrasCrear = true)]
    public class LoteDeUnContratoDto : ElmentoAuditadoDto
    {
        //----------------------------------------------

        [IUPropiedad(
            Etiqueta = nameof(Contrato),
            Ayuda = "Lotes de un contrato",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Contrato),
            Fila = 0,
            Columna = 0,
            VisibleEnGrid = false
            )
        ]
        public int IdContrato { get; set; }

        [IUPropiedad(Etiqueta = "Contrato", Visible = false)]
        public string Contrato { get; set; }


        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Lote",
            Ayuda = "Indique el nombre del lote",
            Tipo = typeof(string),
            Fila = 0,
            Columna = 1,
            Ordenar = true,
            PorAnchoMnt = 50,
            Obligatorio = true,
            LongitudMaxima = 250
          )
        ]
        public string Nombre { get; set; }

        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Fecha de inicio",
            Ayuda = "Inicio de vigencia del lote del contrato",
            TipoDeControl = enumTipoControl.SelectorDeFecha,
            Fila = 2,
            Columna = 0,
            Obligatorio = false
           )
        ]
        public DateTime? VigenteDesde { get; set; }

        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Fecha de fin",
            Ayuda = "fin de vigencia del lote del contrato",
            TipoDeControl = enumTipoControl.SelectorDeFecha,
            Fila = 2,
            Columna = 1,
            Obligatorio = false
           )
        ]
        public DateTime? VigenteHasta { get; set; }


        //----------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Activo",
            Ayuda = "indica si se el lote está activo",
            Fila = 3,
            Columna = 1,
            TipoDeControl = enumTipoControl.Check,
            CssDelContenedor = enumCssControles.ContenedorCheckRight,
            css = enumCssControles.ControlApilado,
            Alineada = enumAliniacion.derecha,
            MantenerHuecoDeLaIzquierda = true,
            VisibleAlCrear = false,
            EditableAlEditar = false
            )
        ]
        public bool Activo { get; set; }

    }
}
