using System;
using Utilidades;

namespace ModeloDeDto.Juridico
{
    [IUDto(MostrarExpresion = "Fechas de prefacturación de contratos")]
    public class FechasDePrefacturacionDto : ISelectorDeFechasDto
    {        
        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Seleccionar partes realizados desde",
            Ayuda = "Indica la fecha desde la que se obtendrán los partes realizados",
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
            Ayuda = "Indica la fecha tope hasta la que se seleccionarán partes",
            TipoDeControl = enumTipoControl.SelectorDeFecha,
            Fila = 2,
            Columna = 1
           )
        ]
        public DateTime FechaHasta { get; set ; }
    }
}
