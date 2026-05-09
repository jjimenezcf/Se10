using System;
using Utilidades;

namespace ModeloDeDto.Juridico
{
    [IUDto(MostrarExpresion = "Fechas de preparación de partes de trabajo")]
    public class FechasDePreparacionDto : ISelectorDeFechasDto
    {        
        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Seleccionar planificaciones a ejecutar desde",
            Ayuda = "Indica la fecha inicial desde donde se crearan los partes planificados",
            TipoDeControl = enumTipoControl.SelectorDeFecha,
            SelectorHasta = nameof(FechaHasta) + ":7",
            Fila = 2,
            Columna = 0
           )
        ]
        public DateTime FechaDesde { get; set; }

        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Hasta",
            Ayuda = "Indica la fecha final",
            TipoDeControl = enumTipoControl.SelectorDeFecha,
            Fila = 2,
            Columna = 1
           )
        ]
        public DateTime FechaHasta { get; set ; }
    }
}
