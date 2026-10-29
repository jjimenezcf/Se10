using ServicioDeDatos;
using Utilidades;
using ModeloDeDto.MaestrosTecnico;
using ServicioDeDatos.MaestrosTecnico;
using static ServicioDeDatos.Elemento.Enumerados;

namespace ModeloDeDto.Logistica
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, OpcionDeBorrar = false)]
    public class LineaDeUnPedidoDto : EsUnDetalleDto
    {

        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Orden",
            Ayuda = "orden de la línea",
            Tipo = typeof(int),
            Fila = 1,
            Columna = 0,
            CssDeLaFila = enumCssGrid.fila_100px_1fr_2fr_0px_0px
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
            OnChange = "javascript:" + nameof(enumNameSpaceTs.Logistica) + "." + nameof(enumFunctionTs.Ped_Tras_Cambiar_TipoDeLinea) + "()",
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
            trasSeleccionar = "javascript:" + nameof(enumNameSpaceTs.Logistica) + "." + nameof(enumFunctionTs.Ped_Tras_Seleccionar_Unitario) + "(["+nameof(enumParamTs.idLista)+"])",
            trasBlanquear = "javascript:" + nameof(enumNameSpaceTs.Logistica) + "." + nameof(enumFunctionTs.Ped_Tras_Blanquear_Unitario) + "()",
            OtrosParametrosDeFiltrado = "javascript:" + nameof(enumNameSpaceTs.Logistica) + "." + nameof(enumFunctionTs.Ped_Parametros_Para_Seleccionar_Unitarios) + "()",
            OtrosClausulasDeFiltrado = "javascript: " + nameof(enumNameSpaceTs.Logistica) + "." + nameof(enumFunctionTs.Ped_FiltrosPorClaseDeUnitario) + "()",
            AutoSpan = true
            )
        ]
        public string Unitario { get; set; }

        //-----------------------------------------------------
        [IUPropiedad(
          Etiqueta = "Concepto",
          Ayuda = "Concepto pedido",
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

        [IUPropiedad(Visible = false) ]
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
            Columna = 0,
            ColSpan = 2,
            CssDeLaFila = enumCssGrid.fila_2fr_0px_1fr_1fr_1fr
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
            Columna = 2
            )
        ]
        public string Unidad { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Cantidad",
           Tipo = typeof(decimal),
           Ayuda = "cantidad pedida",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           OnBlur = "javascript:" + nameof(enumNameSpaceTs.Logistica) + "." + nameof(enumFunctionTs.Ped_CalcularImportesDeLinea) + "()",
           Obligatorio = false,
           MantenerHuecoDeLaIzquierda =true,
           Formato = enumFormato.Numero_6,
           Fila = 3,
           Columna = 3)
        ]
        public decimal ? Cantidad { get; set; }
        
        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Tarifa",
           Tipo = typeof(decimal),
           Ayuda = "tarifa unitaria del proveedor sin iva",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           OnBlur = "javascript:" + nameof(enumNameSpaceTs.Logistica) + "." + nameof(enumFunctionTs.Ped_CalcularImportesDeLinea) + "()",
           Obligatorio = false,
           MantenerHuecoDeLaIzquierda = true,
           Formato = enumFormato.Numero_6,
           Fila = 3,
           Columna =4)
        ]
        public decimal ? Precio { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Descuento(%)",
           Tipo = typeof(decimal),
           Ayuda = "porcentaje de descuento por línea",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           OnBlur = "javascript:" + nameof(enumNameSpaceTs.Logistica) + "." + nameof(enumFunctionTs.Ped_CalcularImportesDeLinea) + "()",
           Obligatorio = false,
           Formato = enumFormato.Porcentaje,
           Fila = 4,
           Columna = 0)
        ]
        public decimal? Descuento { get; set; }


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
           Fila = 4,
           Columna = 1)
        ]
        public decimal? ImporteDeDto { get; set; }
        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Total sin descuento",
           Tipo = typeof(decimal),
           Ayuda = "coste sin descuento",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Obligatorio = false,
           Formato = enumFormato.Moneda,
           MantenerHuecoDeLaIzquierda = true,
           Fila = 4,
           Columna = 3)
        ]
        public decimal? ImporteSinDto { get; set; }


        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Importe",
           Tipo = typeof(decimal),
           Ayuda = "importe de la línea con el descuento",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Obligatorio = false,
           Formato = enumFormato.Moneda,
           Fila = 4,
           Columna = 4)
        ]
        public decimal? ImporteDeLinea => ImporteSinDto - ImporteDeDto;

        //--------------------------------------------
        // Datos de la entrega de la línea (RecepcionDeLineaDtm), vacíos si aún no se ha entregado (ver GestorDeLineasDeUnPedido.DespuesDeMapearElElemento)
        [IUPropiedad(
           Etiqueta = "Cantidad recibida",
           Tipo = typeof(decimal),
           Ayuda = "cantidad entregada",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           VisibleAlCrear = false,
           EditableAlEditar = false,
           Obligatorio = false,
           Formato = enumFormato.Numero_6,
           Fila = 5,
           Columna = 0)
        ]
        public decimal? CantidadRecibida { get; set; }

        [IUPropiedad(
           Etiqueta = "Precio de recepción",
           Tipo = typeof(decimal),
           Ayuda = "precio de compra unitario sin iva de lo entregado",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           VisibleAlCrear = false,
           EditableAlEditar = false,
           Obligatorio = false,
           Formato = enumFormato.Numero_6,
           Fila = 5,
           Columna = 1)
        ]
        public decimal? PrecioDeRecepcion { get; set; }

        [IUPropiedad(
           Etiqueta = "Descuento aplicado(%)",
           Tipo = typeof(decimal),
           Ayuda = "porcentaje de descuento aplicado en la entrega",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           VisibleAlCrear = false,
           EditableAlEditar = false,
           Obligatorio = false,
           Formato = enumFormato.Porcentaje,
           Fila = 5,
           Columna = 2)
        ]
        public decimal? DescuentoAplicado { get; set; }

        // Fecha y destino de la entrega, para mostrarla en la modal de recepción
        [IUPropiedad(Etiqueta = "Recepcionada el", Visible = false)]
        public System.DateTime? RecepcionadaEl { get; set; }

        [IUPropiedad(Etiqueta = "Id del almacén de la entrega", Visible = false)]
        public int? IdAlmacenDeRecepcion { get; set; }

        [IUPropiedad(Etiqueta = "Almacén de la entrega", Visible = false)]
        public string AlmacenDeRecepcion { get; set; }

        [IUPropiedad(Etiqueta = "Id del expediente de la entrega", Visible = false)]
        public int? IdExpedienteDeRecepcion { get; set; }

        [IUPropiedad(Etiqueta = "Expediente de la entrega", Visible = false)]
        public string ExpedienteDeRecepcion { get; set; }

        // El preasiento de la entrada de compra está contabilizado (en un lote contable): la entrega no se puede cancelar
        [IUPropiedad(Etiqueta = "Entrega contabilizada", Visible = false)]
        public bool EntregaContabilizada { get; set; }


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
