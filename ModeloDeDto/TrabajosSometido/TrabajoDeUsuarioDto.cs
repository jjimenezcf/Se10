using System;
using Utilidades;
using ModeloDeDto.Entorno;

namespace ModeloDeDto.TrabajosSometidos
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class TrabajoDeUsuarioDto : ElementoDto
    {
        public static string ExpresionElemento = nameof(Trabajo);

        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Sometido por",
            Ayuda = "Usuario sometedor",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Sometedor),
            Fila = 0,
            Columna = 0,
            EditableAlCrear = false,
            EditableAlEditar = false,
            VisibleEnGrid = false
            )
        ]
        public int IdSometedor { get; set; }

        [IUPropiedad(
            Etiqueta = "sometedor",
            Visible = false
            )
        ]
        public string Sometedor { get; set; }

        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Id del usuario ejecutor",
            Visible = false
            )
        ]
        public int? IdEjecutor { get; set; }

        [IUPropiedad(
            Etiqueta = "Ejecutado por",
            Ayuda = "Usuario ejecutor",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(UsuarioDto),
            GuardarEn = nameof(IdEjecutor),
            MostrarExpresion = UsuarioDto.ExpresionElemento,
            Controlador = nameof(enumControladoresEntorno.Usuarios),
            VistaDondeNavegar = enumVistasEntorno.CrudUsuario,
            Fila = 0,
            Columna = 1,
            VisibleEnGrid = true,
            EditableAlEditar = false,
            Obligatorio = false
            )
        ]
        public string Ejecutor { get; set; }

        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Id del trabajo",
            Visible = false
            )
        ]
        public int IdTrabajo { get; set; }

        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Trabajo",
            Ayuda = "trabajo sometido",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(TrabajoSometidoDto),
            MostrarExpresion = TrabajoSometidoDto.ExpresionElemento,
            Controlador = nameof(enumControladoresTrabajosSometidos.TrabajosSometido),
            GuardarEn = nameof(IdTrabajo),
            Fila = 1,
            Columna = 0,
            Ordenar = true,
            EditableAlEditar = false,
            AutoSpan = true
          )
        ]
        public string Trabajo { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Estado",
           Ayuda = "estado del trabajo",
           TipoDeControl = enumTipoControl.Editor,
           Fila = 2,
           Columna = 0,
           VisibleEnGrid = true,
           EditableAlEditar = false,
           VisibleAlCrear = false,
           Ordenar = true
           )
        ]
        public string Estado { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Entró",
           Ayuda = "Fecha de entrada",
           TipoDeControl = enumTipoControl.SelectorDeFechaHora,
           Formato = enumFormato.Fecha,
           Fila = 3,
           Columna = 0,
           VisibleEnGrid = true,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Obligatorio = true,
           Ordenar = true
           )
        ]
        public DateTime Encolado { get; set; }

        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Planificado",
            Ayuda = "Fecha planificada de ejecución",
            TipoDeControl = enumTipoControl.SelectorDeFechaHora,
            Formato = enumFormato.DiaHoraMinuto,
            Fila = 3,
            Columna = 1,
            VisibleEnGrid = true,
            Obligatorio = false,
            Ordenar = true
           )
        ]
        public DateTime? Planificado { get; set; }

        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Iniciado",
            Ayuda = "Fecha de inicio",
            TipoDeControl = enumTipoControl.SelectorDeFechaHora,
            Formato = enumFormato.DiaHoraMinuto,
            Fila = 4,
            Columna = 0,
            VisibleEnGrid = true,
            EditableAlCrear = false,
            EditableAlEditar = false,
            Obligatorio = false,
            Ordenar = true
           )
        ]
        public DateTime? Iniciado { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Terminado",
           Ayuda = "Fecha de fin",
           TipoDeControl = enumTipoControl.SelectorDeFechaHora,
           Formato = enumFormato.Tiempo,
           Fila = 4,
           Columna = 1,
           VisibleEnGrid = true,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Obligatorio = false
           )
        ]
        public DateTime? Terminado { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Parametros",
           Ayuda = "Json con los parámetros de ejecución",
           TipoDeControl = enumTipoControl.AreaDeTexto,
           Fila = 5,
           Columna = 0,
           VisibleEnGrid = false,
           EditableAlCrear = true,
           EditableAlEditar = true,
           Obligatorio = false,
           NumeroDeFilas = 5,
            AutoSpan = true
           )
        ]
        public string Parametros { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Periodicidad",
           Ayuda = "Tiempo en segundos de resometimiento",
           TipoDeControl = enumTipoControl.Editor,
           Fila = 6,
           Columna = 0,
           VisibleEnGrid = false,
           EditableAlCrear = true,
           EditableAlEditar = true,
           AutoSpan = false,
           AnchoMaximo = "200px",
           ValorPorDefecto = 0
           )
        ]
        public int Periodicidad { get; set; }
    }
}
