using System;
using Utilidades;
using static ServicioDeDatos.Elemento.Enumerados;

namespace ModeloDeDto.Tarea
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, OpcionDeBorrar = false)]
    public class PlfDeTareaDto : EsUnaAmpliacionDto
    {
        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Inicio planificado",
            Ayuda = "fecha de inicio planificada",
            TipoDeControl = enumTipoControl.SelectorDeFechaHora,
            SelectorHasta = nameof(PlfDeFin) + ":0:1",
            Fila = 2,
            Columna = 0,
            Obligatorio = false
           )
        ]
        public DateTime? PlfDeInicio { get; set; }

        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Cuando se terminará",
            Ayuda = "fecha planificada de terminación",
            TipoDeControl = enumTipoControl.SelectorDeFechaHora,
            Fila = 2,
            Columna = 1,
            Obligatorio = false
           )
        ]
        public DateTime? PlfDeFin { get; set; }

        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Fecha real de inicio",
            Ayuda = "Indica cuándo se inicio realmente la tarea",
            TipoDeControl = enumTipoControl.SelectorDeFechaHora,
            Fila = 2,
            Columna = 0,
            Obligatorio = false
           )
        ]
        public DateTime? Iniciada { get; set; }

        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Fecha real de finalización",
            Ayuda = "fecha real de cuando se finalaza la tarea",
            TipoDeControl = enumTipoControl.SelectorDeFechaHora,
            Fila = 2,
            Columna = 1,
            Obligatorio = false
           )
        ]
        public DateTime? Finalizada { get; set; }

        //--------------------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Duración",
           Ayuda = "duración de la tarea",
           TipoDeControl = enumTipoControl.Editor,
           Fila = 3,
           Columna = 0,
            Posicion =0,
           AutoSpan = false,
           AnchoMaximo = "200px",
            Obligatorio = false
           )
        ]
        public decimal? Duracion { get; set; }

        //------------------------------------------------
        [IUPropiedad(
            Etiqueta = "medido en",
            Ayuda = "Indique como se mide el periodo de duración",
            TipoDeControl = enumTipoControl.Enumerado,
            Tipo = typeof(enumDurabilidad),
            GuardarEn = nameof(MedidoEn),
            Fila = 3,
            Columna = 0,
            Posicion =1,
            AutoSpan = false,
            AnchoMaximo = "300px",
            Obligatorio = false
          )
        ]
        public enumDurabilidad? MedidoEn { get; set; }

    }
}
