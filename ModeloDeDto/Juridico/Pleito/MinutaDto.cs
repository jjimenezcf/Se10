using System;
using Utilidades;


namespace ModeloDeDto.Juridico
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, OpcionDeBorrar = false)]
    public class MinutaDto : EsUnDetalleDto
    {
        //-----------------------------------------------------
        [IUPropiedad(
          Etiqueta = "Concepto",
          Ayuda = "Concepto por el que se paga",
          Tipo = typeof(string),
          TipoDeControl = enumTipoControl.Editor,
          Fila = 1,
          Columna = 0,
          Obligatorio = true,
          AutoSpan = true
          )
        ]
        public string Concepto { get; set; }


        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Orden",
            Ayuda = "orden de la línea",
            Tipo = typeof(int),
            Fila = 2,
            Columna = 0
            )
        ]
        public string Orden { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Fecha de Creacion",
           Ayuda = "Fecha de creación",
           TipoDeControl = enumTipoControl.SelectorDeFecha,
           Formato = enumFormato.Fecha,
           Fila = 2,
           Columna = 1,
           VisibleEnGrid = false,
           EditableAlCrear = true,
           EditableAlEditar = true,
           Obligatorio = true
           )
        ]
        public DateTime CreadoEl { get; set; }


        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Valor",
           Tipo = typeof(decimal),
           Ayuda = "Valor del concepto",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           Fila = 2,
           Columna = 2)
        ]
        public decimal Valor { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Fecha de abono",
           Ayuda = "Fecha de abonado",
           TipoDeControl = enumTipoControl.SelectorDeFecha,
           Formato = enumFormato.Fecha,
           Fila = 3,
           Columna = 1,
           Obligatorio = false,
            MantenerHuecoDeLaIzquierda = true
           )
        ]
        public DateTime? AbonadoEl { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Abonado",
           Tipo = typeof(decimal),
           Ayuda = "Cantidad de abonada",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           Obligatorio = false,
           Fila = 3,
           Columna = 2)
        ]
        public decimal? Abonado { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Pendiente",
           Tipo = typeof(decimal),
           Ayuda = "Cantidad de pendiente de abonar",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           Fila = 4,
           Columna = 2,
           VisibleAlCrear = false,
           VisibleAlEditar = true,
           EditableAlEditar = false,
            MantenerHuecoDeLaIzquierda = true)
        ]
        public decimal? Pendiente { get; set; }
    }
}
