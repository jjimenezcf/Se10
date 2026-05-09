using ServicioDeDatos;
using Utilidades;

namespace ModeloDeDto.Negocio
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class TransicionDto : ElementoDto, IUsaNegocioDto
    {
        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Negocio",
            Ayuda = "Transiciones del negocio",
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
            Etiqueta = nameof(Origen),
            Ayuda = "Seleccione estado origen",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(EstadoDto),
            GuardarEn = nameof(IdOrigen),
            Controlador = nameof(enumControladoresNegocio.Estados),
            VistaDondeNavegar = enumVistasNegocio.CrudDeEstados,
            LongitudMinimaParaBuscar = 1,
            RestringidoPorControl = nameof(IdNegocio),
            PropiedadRestrictora = nameof(IdNegocio),
            CriterioDeBusqueda = enumCriteriosDeFiltrado.comienza,
            Fila = 1,
            Columna = 0,
            Obligatorio = true,
            Ordenar = true,
            AutoSpan = true
            )
        ]
        public string Origen { get; set; }

        [IUPropiedad(Etiqueta = "Id del estado origen", Visible = false)]
        public int IdOrigen { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Transición",
           Ayuda = "Nombre de la transición",
           TipoDeControl = enumTipoControl.Editor,
           AutoSpan = true,
           Ordenar = true,
           Fila = 0,
           Columna = 1)
        ]
        public string Nombre { get; set; }

        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = nameof(Destino),
            Ayuda = "Seleccione estado destino",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(EstadoDto),
            GuardarEn = nameof(IdDestino),
            Controlador = nameof(enumControladoresNegocio.Estados),
            VistaDondeNavegar = enumVistasNegocio.CrudDeEstados,
            LongitudMinimaParaBuscar = 1,
            RestringidoPorControl = nameof(IdNegocio),
            PropiedadRestrictora = nameof(IdNegocio),
            CriterioDeBusqueda = enumCriteriosDeFiltrado.comienza,
            Fila = 1,
            Columna = 1,
            Obligatorio = true,
            Ordenar = true,
            AutoSpan = true
            )
        ]
        public string Destino { get; set; }

        [IUPropiedad(Etiqueta = "Id del estado destino", Visible = false)]
        public int IdDestino { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Permiso",
           Ayuda = "permiso asociado a la transición",
            Tipo = typeof(int),
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Permiso),
            Controlador = nameof(enumControladoresSeguridad.Permisos),
            VistaDondeNavegar = enumVistasSeguridad.CrudPermiso,
            Fila = 1,
            Columna = 2,
            Obligatorio = false,
            VisibleAlEditar = true,
            EditableAlEditar = false,
            VisibleAlCrear = false,
            VisibleEnGrid = false
            )
        ]
        public int? IdPermiso { get; set; }

        [IUPropiedad(Visible = false)]
        public string Permiso { get; set; }



        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Asunto",
           Ayuda = "Asunto de la observación",
           TipoDeControl = enumTipoControl.Editor,
           AutoSpan = true,
           VisibleEnGrid = false,
           Obligatorio = false,
           Fila = 2,
           Columna = 0,
            ColSpan = 2)
        ]
        public string Asunto { get; set; }

        //-------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Con observación",
            Ayuda = "indica si es obligatorio crear una observación de transición",
            Fila = 3,
            Columna = 0,
            TipoDeControl = enumTipoControl.Check,
            ValorPorDefecto = false,
            VisibleEnGrid = false,
            css = enumCssControles.ControlApilado
            )
        ]
        public bool ConObservacion { get; set; }

        //-------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Es del sistema",
            Ayuda = "indica si la transición es del sistema",
            VisibleEnGrid = false,
            Fila = 3,
            Columna = 1,
            TipoDeControl = enumTipoControl.Check,
            ValorPorDefecto = false,
            css = enumCssControles.ControlApilado,
            OnChange = "javascript:" + nameof(enumNameSpaceTs.Negocio) + "." + nameof(enumFunctionTs.EsDelSistema_Change) + "(this)"
            )
        ]
        public new bool DelSistema { get; set; }

        //-------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Por defecto",
            Ayuda = "indica si la transición es la defecto",
            VisibleEnGrid = false,
            Fila = 3,
            Columna = 2,
            TipoDeControl = enumTipoControl.Check,
            ValorPorDefecto = false,
            css = enumCssControles.ControlApilado
            )
        ]
        public bool PorDefecto { get; set; }
        //-------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Inicia el proceso",
            Ayuda = "indica si la transición reinicia el proceso",
            Fila = 4,
            Columna = 0,
            TipoDeControl = enumTipoControl.Check,
            ValorPorDefecto = false,
            css = enumCssControles.ControlApilado,
            Obligatorio = false,
            VisibleAlEditar = true,
            EditableAlEditar = false,
            VisibleAlCrear = false,
            VisibleEnGrid = false
            )
        ]
        public bool EsInicial { get; set; }

        //-------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Termina",
            Ayuda = "indica si la transición termina el proceso",
            Fila = 4,
            Columna = 1,
            TipoDeControl = enumTipoControl.Check,
            ValorPorDefecto = false,
            css = enumCssControles.ControlApilado,
            Obligatorio = false,
            VisibleAlEditar = true,
            EditableAlEditar = false,
            VisibleAlCrear = false,
            VisibleEnGrid = false
            )
        ]
        public bool EsTerminado { get; set; }

        //-------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Es Cancelado",
            Ayuda = "indica si la transición cancela el proceso",
            Fila = 4,
            Columna = 2,
            TipoDeControl = enumTipoControl.Check,
            ValorPorDefecto = false,
            css = enumCssControles.ControlApilado,
            Obligatorio = false,
            VisibleAlEditar = true,
            EditableAlEditar = false,
            VisibleAlCrear = false,
            VisibleEnGrid = false
            )
        ]
        public bool EsCancelado { get; set; }

        //-------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Está activa",
            Ayuda = "indica si la transición está activa",
            VisibleEnGrid = true,
            Fila = 5,
            Columna = 0,
            TipoDeControl = enumTipoControl.Check,
            ValorPorDefecto = true,
            css = enumCssControles.ControlApilado,
            PorAnchoMnt = 10
            )
        ]
        public bool Activo { get; set; }
    }

}
