using Utilidades;

namespace ModeloDeDto.Negocio
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class EstadoDto : ElementoDto, IUsaNegocioDto
    {
        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Negocio",
            Ayuda = "Estados del negocio",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Negocio),
            Fila = 0,
            Columna = 0,
            EditableAlEditar = false,
            VisibleEnGrid = false
            )
        ]
        public int IdNegocio { get; set; }

        [IUPropiedad(Visible = false)]
        public string Negocio { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Orden",
           Ayuda = "Orden del estado",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           TamanoFijo = "10em",
           Ordenar = true,
           Fila = 0,
           Columna = 1)
        ]
        public int Orden { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Estado",
           Ayuda = "Nombre del estado",
           TipoDeControl = enumTipoControl.Editor,
           AutoSpan = true,
           Ordenar = true,
           Fila = 0,
           Columna = 2)
        ]
        public string Nombre { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Permiso",
           Ayuda = "permiso de estado",
            EditableAlEditar = false,
            Tipo = typeof(int),
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Permiso),
            Controlador = nameof(enumControladoresSeguridad.Permisos),
            VistaDondeNavegar = enumVistasSeguridad.CrudPermiso,
            Fila = 1,
            Columna = 0,
            Obligatorio = false,
            VisibleAlEditar = true,
            VisibleAlCrear = false,
            VisibleEnGrid = false,
            AutoSpan = true
            )
        ]
        public int? IdPermiso { get; set; }

        [IUPropiedad(Visible=false)]
        public string Permiso { get; set; }

        //-------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Es inicial",
            Ayuda = "indica si el proceso se está iniciando",
            VisibleEnGrid = true,
            Fila = 2,
            Columna = 0,
            TipoDeControl = enumTipoControl.Check,
            ValorPorDefecto = false,
            css = enumCssControles.ControlApilado,
            PorAnchoMnt =10
            )
        ]
        public bool Inicial { get; set; }

        //-------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Es final",
            Ayuda = "indica si el proceso está terminado",
            VisibleEnGrid = true,
            Fila = 3,
            Columna = 0,
            TipoDeControl = enumTipoControl.Check,
            ValorPorDefecto = false,
            css = enumCssControles.ControlApilado,
            PorAnchoMnt = 10
            )
        ]
        public bool Terminado { get; set; }

        //-------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Es Cancelado",
            Ayuda = "indica si el proceso está cancelado",
            VisibleEnGrid = true,
            Fila = 4,
            Columna = 0,
            TipoDeControl = enumTipoControl.Check,
            ValorPorDefecto = false,
            css = enumCssControles.ControlApilado,
            PorAnchoMnt = 10
            )
        ]
        public bool Cancelado { get; set; }


    }

}
