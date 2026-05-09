using System;
using Utilidades;


namespace ModeloDeDto.Juridico
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, OpcionDeBorrar = false)]
    public class RecobroDto : EsUnaAmpliacionDto
    {

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Fecha de deuda",
           Ayuda = "Fecha en la que se produjo la deuda",
           TipoDeControl = enumTipoControl.SelectorDeFecha,
           Formato = enumFormato.Fecha,
           Fila = 0,
           Columna = 0,
           VisibleEnGrid = false,
           EditableAlCrear = true,
           EditableAlEditar = true,
           Obligatorio = true
           )
        ]
        public DateTime FechaDeDeuda { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Principal",
           Tipo = typeof(decimal),
           Ayuda = "Principal de la deuda",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           Fila = 0,
           Columna = 1)
        ]
        public decimal Principal { get; set; }


        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Intereses",
           Tipo = typeof(decimal),
           Ayuda = "intereses generados",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           Fila = 0,
           Columna = 2)
        ]
        public decimal? Intereses { get; set; }


        //------------------------------------------------------------------------------------------------
        [IUPropiedad(
          Etiqueta = "Notaciones",
          Ayuda = "Información adicional sobre la deuda",
          Tipo = typeof(string),
          TipoDeControl = enumTipoControl.AreaDeTexto,
          Fila = 1,
          Columna = 0,
          Obligatorio = false,
          VisibleEnGrid = false,
          AutoSpan = true
          )
        ]
        public string Notacion { get; set; }
    }
}
