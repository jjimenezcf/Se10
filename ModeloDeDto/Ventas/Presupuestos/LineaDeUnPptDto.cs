using ServicioDeDatos;
using Utilidades;
using ModeloDeDto.MaestrosTecnico;
using ServicioDeDatos.MaestrosTecnico;
using ModeloDeDto.Contabilidad;
using static ServicioDeDatos.Elemento.Enumerados;

namespace ModeloDeDto.Presupuesto
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, OpcionDeBorrar = false)]
    public class LineaDeUnPptDto : EsUnDetalleDto
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
            Etiqueta = "Tipo de línea",
            Ayuda = "indique el tipo de línea",
            TipoDeControl = enumTipoControl.Enumerado,
            Tipo = typeof(enumTipoDeLinea),
            GuardarEn = nameof(TipoDeLinea),
            OnChange = "javascript:" + nameof(enumNameSpaceTs.Presupuesto) + "." + nameof(enumFunctionTs.Ppt_Tras_Cambiar_TipoDeLinea) + "()",
            Fila = 1,
            Columna = 1,
            EditableAlEditar = false
          )
        ]
        public string TipoDeLinea { get; set; }


        //----------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Id del unitario",
            Visible = false
            )
        ]
        public int ? IdUnitario { get; set; }


        [IUPropiedad(
            Etiqueta = "Unitario",
            Ayuda = "Indique el unitario",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(UnitarioDto),
            GuardarEn = nameof(IdUnitario),
            Controlador = nameof(enumControladoresMt.Unitarios),
            VistaDondeNavegar = enumVistasMts.CrudUnitarios,
            BuscarPor = nameof(UnitarioDtm.Nombre),
            CriterioDeBusqueda = enumCriteriosDeFiltrado.contiene,
            Fila = 1,
            Columna = 2,
            Ordenar = true,
            Obligatorio = false,
            EditableAlEditar = false,
            trasSeleccionar = "javascript:" + nameof(enumNameSpaceTs.Presupuesto) + "." + nameof(enumFunctionTs.Ppt_Tras_Seleccionar_Unitario) + "(["+nameof(enumParamTs.idLista)+"])",
            trasBlanquear = "javascript:" + nameof(enumNameSpaceTs.Presupuesto) + "." + nameof(enumFunctionTs.Ppt_Tras_Blanquear_Unitario) + "()",
            AutoSpan = true
            )
        ]
        public string Unitario { get; set; }

        //-----------------------------------------------------
        [IUPropiedad(
          Etiqueta = "Concepto",
          Ayuda = "Concepto presupuestado",
          Tipo = typeof(string),
          TipoDeControl = enumTipoControl.Editor,
          Fila = 2,
          Columna = 0,
          Obligatorio =false,
          AutoSpan = true
          )
        ]
        public string Concepto { get; set; }


        //----------------------------------------------------------------

        [IUPropiedad(
            Etiqueta = "Clase",
            Ayuda = "Seleccione la clase del unitario",
            TipoDeControl = enumTipoControl.Enumerado,
            Tipo = typeof(enumClaseUnitario),
            GuardarEn = nameof(Clase),
            EditableAlEditar = false,
            EditableAlCrear = true,
            Fila = 3,
            Columna = 0
            )
        ]
        public string Clase { get; set; }

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
            Obligatorio = false,
            MostrarExpresion = nameof(NaturalezaDtm.Expresion),
            EditableAlCrear = true,
            EditableAlEditar = false,
            Fila = 3,
            Columna = 1,
            ColSpan = 2
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
            Obligatorio = false,
            EditableAlCrear = true,
            EditableAlEditar = false,
            Fila = 3,
            Columna = 3
            )
        ]
        public string Unidad { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Cantidad",
           Tipo = typeof(decimal),
           Ayuda = "cantidad presupuestada",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           OnBlur = "javascript:" + nameof(enumNameSpaceTs.Presupuesto) + "." + nameof(enumFunctionTs.Ppt_CalcularImportesDeLinea) + "()",
           Obligatorio = false,
           MantenerHuecoDeLaIzquierda =true,
           Formato = enumFormato.Numero_6,
           Fila = 4,
           Columna = 2)
        ]
        public decimal ? Cantidad { get; set; }
        
        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "P. de venta",
           Tipo = typeof(decimal),
           Ayuda = "precio de venta",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           OnBlur = "javascript:" + nameof(enumNameSpaceTs.Presupuesto) + "." + nameof(enumFunctionTs.Ppt_CalcularImportesDeLinea) + "()",
           Obligatorio = false,
           MantenerHuecoDeLaIzquierda = true,
           Formato = enumFormato.Numero_6,
           Fila = 4,
           Columna = 3)
        ]
        public decimal ? Precio { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Descuento(%)",
           Tipo = typeof(decimal),
           Ayuda = "porcentaje de descuento por línea",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           OnBlur = "javascript:" + nameof(enumNameSpaceTs.Presupuesto) + "." + nameof(enumFunctionTs.Ppt_CalcularImportesDeLinea) + "()",
           Obligatorio = false,
           Formato = enumFormato.Porcentaje,
           Fila = 5,
           Columna = 0)
        ]
        public decimal? Descuento { get; set; }

        //----------------------------------------------
        [IUPropiedad(Etiqueta = "Id del iva repercutido", Visible = false)]
        public int IdIvaR { get; set; }

        [IUPropiedad(
            Etiqueta = "Iva",
            Ayuda = "Seleccione el tipo de iva",
            TipoDeControl = enumTipoControl.ListaDeElemento,
            SeleccionarDe = typeof(IvaRepercutidoDto),
            Controlador = nameof(enumControladoresContables.IvasRepercutido),
            GuardarEn = nameof(IdIvaR),
            OnChange = "javascript:" + nameof(enumNameSpaceTs.Presupuesto) + "." + nameof(enumFunctionTs.Ppt_IvaRepercutidoCambiado) + "()",
            OnBlur = "javascript:" + nameof(enumNameSpaceTs.Presupuesto) + "." + nameof(enumFunctionTs.Ppt_CalcularImportesDeLinea) + "()",
            TrasCargar = "javascript:" + nameof(enumNameSpaceTs.Presupuesto) + "." + nameof(enumFunctionTs.Ppt_TrasCargarIvas) + "()",
            Obligatorio = false,
            ColSpan = 2,
            VisibleEnGrid = false,
            Fila = 5,
            Columna = 1
            )
        ]
        public string IvaRepercutido { get; set; }

        //--------------------------------------------
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
           Fila = 5,
           Columna = 3)
        ]
        public decimal? Iva { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Total sin descuento",
           Tipo = typeof(decimal),
           Ayuda = "Venta sin descuento",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Obligatorio = false,
           Formato = enumFormato.Moneda,
           Fila = 6,
           Columna = 0)
        ]
        public decimal? ImporteSinDto { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Importe del descuento.",
           Tipo = typeof(decimal),
           Ayuda = "importe del descuento",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Obligatorio = false,
           Formato = enumFormato.Moneda,
           Fila = 6,
           Columna = 1)
        ]
        public decimal? ImporteDeDto { get; set; }

        //--------------------------------------------
        [IUPropiedad(Visible = false)
        ]
        public decimal? BaseImponible => ImporteSinDto - ImporteDeDto;

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Importe del IVA.",
           Tipo = typeof(decimal),
           Ayuda = "importe del iva",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Obligatorio = false,
           Formato = enumFormato.Moneda,
           Fila = 6,
           Columna = 2)
        ]
        public decimal? ImporteDeIva { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Importe de Linea.",
           Tipo = typeof(decimal),
           Ayuda = "importe de la línea",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Obligatorio = false,
           Formato = enumFormato.Moneda,
           Fila = 6,
           Columna = 3)
        ]
        public decimal? ImporteDeLinea { get; set; }


        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Anotación",
           Ayuda = "Anotación opcional de la línea ",
           TipoDeControl = enumTipoControl.AreaDeTexto,
           Fila = 7,
           Columna = 0,
           VisibleEnGrid = false,
           Obligatorio = false,
           NumeroDeFilas = 3,
           AutoSpan = true
           )
        ]
        public string Anotacion { get; set; }


    }
}
