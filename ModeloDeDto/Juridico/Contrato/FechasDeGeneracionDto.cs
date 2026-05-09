using System;
using Utilidades;

namespace ModeloDeDto.Juridico
{
    [IUDto(MostrarExpresion = "Fechas de generación de planificadores")]
    public class FechasDeGeneracionDto : ISelectorDeFechasDto
    {        
        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Fecha de inicio de generación",
            Ayuda = "Indica la fecha inicial de los planificadores a seleccionar",
            TipoDeControl = enumTipoControl.SelectorDeFecha,
            SelectorHasta = nameof(FechaHasta) + ":365",
            Fila = 2,
            Columna = 0
           )
        ]
        public DateTime FechaDesde { get; set; }

        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Fecha final de generación",
            Ayuda = "Indica la fecha tope hasta la que se generan las planificaciones",
            TipoDeControl = enumTipoControl.SelectorDeFecha,
            Fila = 2,
            Columna = 1
           )
        ]
        public DateTime FechaHasta { get; set ; }
    }
}
