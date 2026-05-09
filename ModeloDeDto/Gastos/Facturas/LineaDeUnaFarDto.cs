using Utilidades;
using ModeloDeDto.MaestrosTecnico;
using ServicioDeDatos.MaestrosTecnico;
using ModeloDeDto.Contabilidad;
using static ServicioDeDatos.Elemento.Enumerados;

namespace ModeloDeDto.Gastos
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, OpcionDeBorrar = false)]
    public class LineaDeUnaFarDto : EsUnDetalleDto
    {

        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Orden",
            Ayuda = "orden de la línea",
            Tipo = typeof(int),
            Fila = 1,
            Columna = 0
            )
        ]
        public int Orden { get; set; }


        //-------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Clase de línea",
            Ayuda = "indique la clase de línea",
            TipoDeControl = enumTipoControl.Enumerado,
            Tipo = typeof(enumClaseDeLineaFar),
            GuardarEn = nameof(Clase),
            OnChange = "javascript:" + nameof(enumNameSpaceTs.Gasto) + "." + nameof(enumFunctionTs.Far_Tras_Cambiar_ClaseDeLinea) + "()",
            Fila = 1,
            Columna = 1
          )
        ]
        public enumClaseDeLineaFar Clase { get; set; }

        [IUPropiedad(Visible = false)]
        public string DescripcionDeClase { get; set; }  

        //----------------------------------------------
        [IUPropiedad(Etiqueta = "Id de la naturaleza contable", Visible = false)]
        public int? IdNaturaleza { get; set; }

        [IUPropiedad(
            Etiqueta = "Naturaleza",
            Ayuda = "Seleccione la naturaleza contable",
            TipoDeControl = enumTipoControl.ListaDeElemento,
            SeleccionarDe = typeof(NaturalezaDto),
            Controlador = nameof(enumControladoresMt.Naturalezas),
            GuardarEn = nameof(IdNaturaleza),
            Obligatorio = false,
            MostrarExpresion = nameof(NaturalezaDtm.Expresion),
            //TrasCargar = "javascript:" + nameof(enumNameSpaceTs.Gasto) + "." + nameof(enumFunctionTs.Far_TrasCargarNaturalezas) + "()",
            Fila = 1,
            Columna = 2
            )
        ]
        public string Naturaleza { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "B.I",
           Tipo = typeof(decimal),
           Ayuda = "base imponible",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           OnBlur = "javascript:" + nameof(enumNameSpaceTs.Gasto) + "." + nameof(enumFunctionTs.Far_CalcularImpuesto) + "()",
           Obligatorio = false,
           MantenerHuecoDeLaIzquierda = true,
           Formato = enumFormato.Numero_6,
           Fila = 1,
           Columna = 3)
        ]
        public decimal? BaseImponible { get; set; }

        //-----------------------------------------------------
        [IUPropiedad(
          Etiqueta = "Concepto",
          Ayuda = "Concepto facturado",
          Tipo = typeof(string),
          TipoDeControl = enumTipoControl.Editor,
          Fila = 2,
          Columna = 0,
          Obligatorio = true,
          AutoSpan = true
          )
        ]
        public string Concepto { get; set; }
        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Cantidad",
           Tipo = typeof(decimal),
           Ayuda = "cantidad facturada (No aplica a la B.I)",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           Formato = enumFormato.Numero_6,
           Fila = 2,
           Columna = 2)
        ]
        public decimal? Cantidad { get; set; }

        //----------------------------------------------
        [IUPropiedad(Etiqueta = "Id la unidad de medida", Visible = false)]
        public int? IdUnidad { get; set; }

        [IUPropiedad(
            Etiqueta = "Unidad",
            Ayuda = "Seleccione la unidad de medida",
            TipoDeControl = enumTipoControl.ListaDeElemento,
            SeleccionarDe = typeof(UnidadDto),
            Controlador = nameof(enumControladoresMt.Unidades),
            GuardarEn = nameof(IdUnidad),
            Obligatorio = false,
            Fila = 2,
            Columna = 3
            )
        ]
        public string Unidad { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Anotación",
           Ayuda = "Anotación opcional de la línea ",
           TipoDeControl = enumTipoControl.AreaDeTexto,
           Fila = 3,
           Columna = 0,
           VisibleEnGrid = false,
           Obligatorio = false,
           NumeroDeFilas = 3,
           AutoSpan = true
           )
        ]
        public string Anotacion { get; set; }


        //-----------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Iva",
            Ayuda = "Seleccione el tipo de iva",
            TipoDeControl = enumTipoControl.ListaDeElemento,
            SeleccionarDe = typeof(IvaSoportadoDto),
            Controlador = nameof(enumControladoresContables.IvasSoportado),
            GuardarEn = nameof(IdIvaS),
            OnChange = "javascript:" + nameof(enumNameSpaceTs.Gasto) + "." + nameof(enumFunctionTs.Far_IvaSoportadoCambiado) + "()",
            Obligatorio = false,
            VisibleEnGrid = false,
            Fila = 4,
            Columna = 0
            )
        ]
        public string IvaSoportado { get; set; }

        [IUPropiedad(Etiqueta = "Id del iva soportado", Visible = false)]
        public int? IdIvaS { get; set; }

        [IUPropiedad(
           Etiqueta = "Iva(%)",
           Tipo = typeof(decimal),
           Ayuda = "porcentaje de iva",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           Obligatorio = false,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Formato = enumFormato.Porcentaje,
           Fila = 4,
           Columna = 1)
        ]
        public decimal? PorcentajeIva { get; set; }

        //-----------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Irpf",
            Ayuda = "Seleccione el tipo de irpf",
            TipoDeControl = enumTipoControl.ListaDeElemento,
            SeleccionarDe = typeof(IrpfDto),
            Controlador = nameof(enumControladoresContables.Irpfs),
            GuardarEn = nameof(IdIrpf),
            OnChange = "javascript:" + nameof(enumNameSpaceTs.Gasto) + "." + nameof(enumFunctionTs.Far_IrpfCambiado) + "()",
            Obligatorio = false,
            VisibleEnGrid = false,
            Fila = 5,
            Columna = 0
            )
        ]
        public string Irpf { get; set; }

        [IUPropiedad(Etiqueta = "Id del irpf", Visible = false)]
        public int? IdIrpf { get; set; }

        [IUPropiedad(
           Etiqueta = "Irpf(%)",
           Tipo = typeof(decimal),
           Ayuda = "porcentaje de irpf",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           Obligatorio = false,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Formato = enumFormato.Porcentaje,
           Fila = 5,
           Columna = 1)
        ]
        public decimal? PorcentajeIrpf { get; set; }


        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Imp. de Iva",
           Tipo = typeof(decimal),
           Ayuda = "importe del iva",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           Obligatorio = false,
           MantenerHuecoDeLaIzquierda = true,
           Formato = enumFormato.Numero_6,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Fila = 4,
           Columna = 3)
        ]
        public decimal? ImporteDeIva { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Imp. de Irpf",
           Tipo = typeof(decimal),
           Ayuda = "importe del irpf",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           Obligatorio = false,
           MantenerHuecoDeLaIzquierda = true,
           Formato = enumFormato.Numero_6,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Fila = 5,
           Columna = 3)
        ]
        public decimal? ImporteDeIrpf { get; set; }

        //--------------------------------------------
        [IUPropiedad(Visible = false)]
        public decimal? importeLinea {get; set;}


        //--------------------------------------------
        [IUPropiedad(Visible = false)]
        public string Sigla { get; set; }

    }
}
