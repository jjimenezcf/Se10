using Utilidades;

namespace ModeloDeDto.Tarea
{
    [IUDto()]
    public class TotalesDeTareas: TotalesDto
    {
        //-----------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Información de totales",
           Ayuda = "muestra la información de los totales calculados",
           TipoDeControl = enumTipoControl.AreaDeTexto,
           EditableAlCrear = false,
           EditableAlEditar = false,
           NumeroDeFilas = 7,
           Fila = 0,
           Columna = 0
            )
        ]
        public string Totales { get; set; }

        //-----------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Totales por ejecutor",
           Ayuda = "muestra el número de tareas, jornadas totales y media por ejecutor (solo tareas con planificación)",
           TipoDeControl = enumTipoControl.AreaDeTexto,
           EditableAlCrear = false,
           EditableAlEditar = false,
           NumeroDeFilas = 8,
           CssDelArea = enumCssControles.MonoSpaceText,
           Fila = 1,
           Columna = 0,
           AutoSpan = true
            )
        ]
        public string TotalesPorEjecutor { get; set; }

        //-----------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Totales por solicitante",
           Ayuda = "muestra el número de tareas, jornadas totales y media por solicitante (solo tareas con planificación)",
           TipoDeControl = enumTipoControl.AreaDeTexto,
           EditableAlCrear = false,
           EditableAlEditar = false,
           NumeroDeFilas = 8,
           CssDelArea = enumCssControles.MonoSpaceText,
           Fila = 2,
           Columna = 0,
           AutoSpan = true
            )
        ]
        public string TotalesPorSolicitante { get; set; }

        //-----------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Totales por expediente",
           Ayuda = "muestra el número de tareas, jornadas totales y media por expediente (solo tareas con planificación)",
           TipoDeControl = enumTipoControl.AreaDeTexto,
           EditableAlCrear = false,
           EditableAlEditar = false,
           NumeroDeFilas = 8,
           CssDelArea = enumCssControles.MonoSpaceText,
           Fila = 3,
           Columna = 0,
           AutoSpan = true
            )
        ]
        public string TotalesPorExpediente { get; set; }
    }
}
