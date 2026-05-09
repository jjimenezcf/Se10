using ServicioDeDatos.MaestrosTecnico;
using Utilidades;


namespace ModeloDeDto.MaestrosTecnico
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, MostrarExpresion = "Nombre")]
    public class UnitarioDto : ElmentoAuditadoDto
    {
        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Unitario",
            Ayuda = "Indique el nombre del unitario",
            Tipo = typeof(string),
            Fila = 0,
            Columna = 0,
            Ordenar = true,
            PorAnchoMnt = 50,
            LongitudMaxima = 250,
            AutoSpan = true
          )
        ]
        public string Nombre { get; set; }

        //----------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Clase",
            Ayuda = "Seleccione la clase del unitario",
            TipoDeControl = enumTipoControl.Enumerado,
            Tipo = typeof(enumClaseUnitario),
            GuardarEn = nameof(Clase),
            EditableAlEditar = false,
            EditableAlCrear = true,
            Fila = 1,
            Columna = 0,
            OnBlur = "javascript:" + nameof(enumNameSpaceTs.MaestrosTecnico) + "." + nameof(enumFunctionTs.Unitario_ProponerReferencia) + "()"
            )
        ]
        public enumClaseUnitario Clase { get; set; }

        //----------------------------------------------
        [IUPropiedad(Etiqueta = "Id de la naturaleza contable", Visible = false)]
        public int IdNaturaleza { get; set; }

        [IUPropiedad(
            Etiqueta = "Naturaleza",
            Ayuda = "Seleccione la naturaleza contable",
            TipoDeControl = enumTipoControl.ListaDeElemento,
            SeleccionarDe = typeof(NaturalezaDto),
            Controlador = nameof(enumControladoresMt.Naturalezas),
            GuardarEn = nameof(IdNaturaleza),
            MostrarExpresion = nameof(NaturalezaDtm.Expresion),
            EditableAlCrear = true,
            EditableAlEditar = false,
            Fila = 1,
            Columna = 1,
            OnBlur = "javascript:" + nameof(enumNameSpaceTs.MaestrosTecnico) + "." + nameof(enumFunctionTs.Unitario_ProponerReferencia) + "()"
            )
        ]
        public string Naturaleza { get; set; }

        //----------------------------------------------
        [IUPropiedad(Etiqueta = "Id la unidad de medida", Visible = false)]
        public int IdUnidad { get; set; }

        [IUPropiedad(
            Etiqueta = "Unidad",
            Ayuda = "Seleccione la unidad de medida",
            TipoDeControl = enumTipoControl.ListaDeElemento,
            SeleccionarDe = typeof(UnidadDto),
            Controlador = nameof(enumControladoresMt.Unidades),
            GuardarEn = nameof(IdUnidad),
            EditableAlCrear = true,
            EditableAlEditar = false,
            Fila = 1,
            Columna = 2
            )
        ]
        public string Unidad { get; set; }


        //----------------------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Descripción",
           Ayuda = "Descripción del unitario, se mostrará en la impresión del ppt o factura",
           TipoDeControl = enumTipoControl.AreaDeTexto,
           VisibleEnGrid = false,
           Obligatorio = false,
           NumeroDeFilas = 3,
           Fila = 2,
           Columna = 0,
           AutoSpan = true
          )
        ]
        public string Descripcion { get; set; }


        //----------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Referencia",
            Ayuda = "Referencia del unitario",
            Tipo = typeof(string),
            Ordenar = true,
            Fila = 3,
            Columna = 0,
            ColSpan = 1,
            EditableAlEditar = false
          )
        ]
        public string Referencia { get; set; }


        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Coste",
           Tipo = typeof(decimal),
           Ayuda = "Valor de coste",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           Fila = 3,
           Columna = 2)
        ]
        public decimal Coste { get; set; }


        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Tarifa",
           Tipo = typeof(decimal),
           Ayuda = "Base imponible",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           Fila = 3,
           Columna = 3)
        ]
        public decimal Venta { get; set; }


        //----------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "proponer referencia",
            Ayuda = "indica si la referenccia del unitario la da el sistema",
            VisibleEnGrid = false,
            Obligatorio = false,
            Fila = 4,
            Columna = 0,
            TipoDeControl = enumTipoControl.Check,
            css = enumCssControles.ControlApilado,
            ValorPorDefecto = true,
            VisibleAlCrear = true,
            VisibleAlEditar = false,
            EditableAlCrear = true,
            OnChange = "javascript:" + nameof(enumNameSpaceTs.MaestrosTecnico) + "." + nameof(enumFunctionTs.Unitario_DesbloquearReferencia) + "()"
            )
        ]
        public bool Proponer { get; set; }

        //----------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Baja",
            Ayuda = "indica si el unitario está de baja",
            VisibleEnGrid = false,
            Obligatorio = false,
            Fila = 5,
            Columna = 0,
            TipoDeControl = enumTipoControl.Check,
            css = enumCssControles.ControlApilado,
            ValorPorDefecto = false,
            VisibleAlCrear = true,
            EditableAlCrear = false,
            EditableAlEditar = true
            )
        ]
        public bool Baja { get; set; }


    }
}
