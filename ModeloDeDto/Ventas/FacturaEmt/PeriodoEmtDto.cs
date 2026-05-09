using System;
using Utilidades;


namespace ModeloDeDto.Ventas
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, OpcionDeBorrar = false)]
    public class PeriodoEmtDto : EsUnaAmpliacionDto
    {

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Inicio",
           Ayuda = "Inicio del periodo de facturación",
           TipoDeControl = enumTipoControl.SelectorDeFecha,
           Formato = enumFormato.Fecha,
           Fila = 0,
           Columna = 0,
           VisibleEnGrid = false,
           EditableAlCrear = true,
           EditableAlEditar = true,
           Obligatorio = false
           )
        ]
        public DateTime? Inicio { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Fin",
           Ayuda = "Fin del periodo de facturación",
           TipoDeControl = enumTipoControl.SelectorDeFecha,
           Formato = enumFormato.Fecha,
           Fila = 0,
           Columna = 1,
           VisibleEnGrid = false,
           EditableAlCrear = true,
           EditableAlEditar = true,
           Obligatorio = false
           )
        ]
        public DateTime? Fin { get; set; }

        //------------------------------------------------------------------------------------------------
        [IUPropiedad(
          Etiqueta = "Notaciones",
          Ayuda = "Información adicional sobre el periodo facturado",
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
