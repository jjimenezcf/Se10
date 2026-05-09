using System;
using Utilidades;

namespace ModeloDeDto.Juridico
{
    [IUDto(MostrarExpresion = "Fechas de prefacturación de contratos por cliente")]
    public class FacturarPorContratoDto : ISelectorDeFechasDto
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
            Ayuda = "Indica la fecha máxima hasta la que se seleccionaran partes realizados",
            TipoDeControl = enumTipoControl.SelectorDeFecha,
            Fila = 2,
            Columna = 1
           )
        ]
        public DateTime FechaHasta { get; set ; }
    }
}
