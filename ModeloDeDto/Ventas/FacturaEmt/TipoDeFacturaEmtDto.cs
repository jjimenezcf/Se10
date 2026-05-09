using Utilidades;
using ModeloDeDto.Negocio;
using ServicioDeDatos;
using ModeloDeDto.MaestrosTecnico;
using ServicioDeDatos.MaestrosTecnico;
using ModeloDeDto.Contabilidad;

namespace ModeloDeDto.Ventas
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class TipoDeFacturaEmtDto : TipoDeElementoDto, IPermisoDeInterventorDto
    {
        //-------------------------------------------------------------------------------------------------------
        [IUPropiedad(Etiqueta = "Id del estado inicial", Visible = false) ]
        public int IdEstado { get; set; }
        [IUPropiedad(
            Etiqueta = "Estado inicial",
            Ayuda = "Seleccione estado en el que se inicia una factura",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(EstadoDto),
            GuardarEn = nameof(IdEstado),
            Controlador = nameof(enumControladoresNegocio.Estados),
            VistaDondeNavegar = enumVistasNegocio.CrudDeEstados,
            LongitudMinimaParaBuscar = 1,
            Negocio = enumNegocio.FacturaEmitida,
            CriterioDeBusqueda = enumCriteriosDeFiltrado.comienza,
            Fila = 1,
            Columna = 2,
            Obligatorio = true,
            Ordenar = true,
            AutoSpan = true
            )
        ]
        public string Estado { get; set; }


        //------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Serie",
            Ayuda = "Serie de factura",
            Tipo = typeof(string),
            LongitudMaxima = 3,
            Fila = 1,
            Columna = 3,
            Obligatorio = true
          )
        ]
        public string Serie { get; set; }

        //--------------------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Vencimiento a",
           Ayuda = "Días en los que vence el cobro de la factura",
           TipoDeControl = enumTipoControl.Editor,
           Fila = 1,
           Columna = 3,
           VisibleEnGrid = false,
           EditableAlCrear = true,
           EditableAlEditar = true,
           AutoSpan = true,
           ValorPorDefecto = 30,
           Alineada = enumAliniacion.derecha
           )
        ]
        public int Vencimiento { get; set; }



        //----------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Clase de unitario",
            Ayuda = "Seleccione la clase del unitario",
            TipoDeControl = enumTipoControl.Enumerado,
            Tipo = typeof(enumClaseUnitario),
            GuardarEn = nameof(ClaseDefecto),
            Fila = 2,
            Columna = 0
            )
        ]
        public string ClaseDefecto { get; set; }

        //----------------------------------------------
        [IUPropiedad(Etiqueta = "Id de la naturaleza contable", Visible = false)]
        public int IdNaturalezaDefecto { get; set; }

        [IUPropiedad(
            Etiqueta = "Naturaleza",
            Ayuda = "Seleccione la naturaleza contable",
            TipoDeControl = enumTipoControl.ListaDeElemento,
            SeleccionarDe = typeof(NaturalezaDto),
            Controlador = nameof(enumControladoresMt.Naturalezas),
            GuardarEn = nameof(IdNaturalezaDefecto),
            Obligatorio = false,
            MostrarExpresion = nameof(NaturalezaDtm.Expresion),
            Fila = 2,
            Columna = 1
            )
        ]
        public string Naturaleza { get; set; }

        //----------------------------------------------
        [IUPropiedad(Etiqueta = "Id la unidad de medida", Visible = false)]
        public int IdUnidadDefecto { get; set; }

        [IUPropiedad(
            Etiqueta = "Unidad",
            Ayuda = "Seleccione la unidad de medida",
            TipoDeControl = enumTipoControl.ListaDeElemento,
            SeleccionarDe = typeof(UnidadDto),
            Controlador = nameof(enumControladoresMt.Unidades),
            GuardarEn = nameof(IdUnidadDefecto),
            Obligatorio = false,
            Fila = 2,
            Columna = 2
            )
        ]
        public string Unidad { get; set; }

        //----------------------------------------------
        [IUPropiedad(Etiqueta = "Id del iva repercutido", Visible = false)]
        public int IdIvaRDefecto { get; set; }

        [IUPropiedad(
            Etiqueta = "Iva",
            Ayuda = "Seleccione el tipo de iva",
            TipoDeControl = enumTipoControl.ListaDeElemento,
            SeleccionarDe = typeof(IvaRepercutidoDto),
            Controlador = nameof(enumControladoresContables.IvasRepercutido),
            GuardarEn = nameof(IdIvaRDefecto),
            Obligatorio = false,
            Fila = 2,
            Columna = 3
            )
        ]
        public string IvaRepercutido { get; set; }

        //-------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "P. de Interventor",
            Ayuda = "Permiso de intervención",
            EditableAlEditar = false,
            Tipo = typeof(int),
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(PermisoDeInterventor),
            Controlador = nameof(enumControladoresSeguridad.Permisos),
            VistaDondeNavegar = enumVistasSeguridad.CrudPermiso,
            Fila = 3,
            Columna = 0,
            Obligatorio = false,
            VisibleEnEdicion = true,
            AutoSpan = true
            )
        ]
        public int IdPermisoDeInterventor { get; set; }
        [IUPropiedad(Visible = false)]
        public string PermisoDeInterventor { get; set; }

        //-------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Es para exportación",
            Ayuda = "indica si el tipo seleccionado es para exportación, intra o extra comunitario",
            VisibleEnGrid = false,
            VisibleAlCrear = true,
            VisibleEnEdicion = true,
            Obligatorio = true,
            Fila = 12,
            Columna = 1,
            TipoDeControl = enumTipoControl.Check,
            ValorPorDefecto = false,
            css = enumCssControles.ControlApilado
            )
        ]
        public bool EsExportacion { get; set; }
        //-------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Necesita periodo",
            Ayuda = "indica si el tipo seleccionado debe incluir el periodo de facturación",
            VisibleEnGrid = false,
            VisibleAlCrear = true,
            VisibleEnEdicion = true,
            Obligatorio = true,
            Fila = 12,
            Columna = 2,
            TipoDeControl = enumTipoControl.Check,
            ValorPorDefecto = false,
            css = enumCssControles.ControlApilado
            )
        ]
        public bool ConPeriodoEmt { get; set; }

    }
}
