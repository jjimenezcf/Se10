using System;
using Utilidades;

namespace ModeloDeDto.Ventas
{
    [IUDto(MostrarExpresion = "Fechas de ejecución inicial")]
    public class FechasDeEjecucionDto 
    {        
        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Iniciada",
            Ayuda = "Indica la fecha inicial de ejecución del parte",
            TipoDeControl = enumTipoControl.SelectorDeFechaHora,
            SelectorHasta = nameof(Terminada) + ":0:8",
            Fila = 2,
            Columna = 0
           )
        ]
        public DateTime Iniciada { get; set; }

        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Terminada",
            Ayuda = "Indica la fecha en la que se ha terminado",
            TipoDeControl = enumTipoControl.SelectorDeFechaHora,
            Obligatorio = false,
            Fila = 2,
            Columna = 1
           )
        ]
        public DateTime Terminada { get; set ; }
    }
}
