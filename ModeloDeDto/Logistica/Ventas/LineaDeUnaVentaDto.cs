using ServicioDeDatos;
using Utilidades;
using ModeloDeDto.MaestrosTecnico;
using ServicioDeDatos.MaestrosTecnico;

namespace ModeloDeDto.Logistica
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, OpcionDeBorrar = false)]
    public class LineaDeUnaVentaDto : EsUnDetalleDto
    {

        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Orden",
            Ayuda = "orden de la línea",
            Tipo = typeof(int),
            Fila = 1,
            Columna = 0,
            CssDeLaFila = enumCssGrid.fila200pxFr
            )
        ]
        public int Orden { get; set; }

        //----------------------------------------------------------
        [IUPropiedad(Etiqueta = "Id del unitario", Visible = false)]
        public int IdUnitario { get; set; }

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
            Columna = 1,
            Ordenar = true,
            Obligatorio = true,
            EditableAlEditar = false,
            AutoSpan = true,
            trasSeleccionar = "javascript:" + nameof(enumNameSpaceTs.Logistica) + "." + nameof(enumFunctionTs.Dsa_Tras_Seleccionar_Unitario) + "([" + nameof(enumParamTs.idLista) + "])",
            trasBlanquear = "javascript:" + nameof(enumNameSpaceTs.Logistica) + "." + nameof(enumFunctionTs.Dsa_Tras_Blanquear_Unitario) + "()"
            )
        ]
        public string Unitario { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Cantidad",
           Tipo = typeof(decimal),
           Ayuda = "cantidad vendida",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           OnBlur = "javascript:" + nameof(enumNameSpaceTs.Logistica) + "." + nameof(enumFunctionTs.Dsa_CalcularImportesDeLinea) + "()",
           Obligatorio = true,
           Formato = enumFormato.Numero_6,
           Fila = 2,
           Columna = 0)
        ]
        public decimal Cantidad { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Precio de venta",
           Tipo = typeof(decimal),
           Ayuda = "precio de venta del unitario",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           OnBlur = "javascript:" + nameof(enumNameSpaceTs.Logistica) + "." + nameof(enumFunctionTs.Dsa_CalcularImportesDeLinea) + "()",
           Obligatorio = true,
           Formato = enumFormato.Numero_6,
           Fila = 2,
           Columna = 1)
        ]
        public decimal Precio { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Descuento",
           Tipo = typeof(decimal),
           Ayuda = "porcentaje de descuento",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           OnBlur = "javascript:" + nameof(enumNameSpaceTs.Logistica) + "." + nameof(enumFunctionTs.Dsa_CalcularImportesDeLinea) + "()",
           Obligatorio = false,
           Formato = enumFormato.Porcentaje,
           Fila = 2,
           Columna = 2)
        ]
        public decimal? Descuento { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Importe",
           Tipo = typeof(decimal),
           Ayuda = "importe de la línea (cantidad x precio - descuento)",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Obligatorio = false,
           Formato = enumFormato.Moneda,
           Fila = 2,
           Columna = 3)
        ]
        public decimal Importe { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Precio almacén",
           Tipo = typeof(decimal),
           Ayuda = "valor del stock (precio medio ponderado) en el almacén a fecha de solicitud",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           VisibleAlCrear = false,
           EditableAlEditar = false,
           Obligatorio = false,
           Formato = enumFormato.Moneda,
           Fila = 3,
           Columna = 0)
        ]
        public decimal? PrecioAlmacen { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Margen",
           Tipo = typeof(decimal),
           Ayuda = "margen del precio de venta neto sobre el precio del almacén, si es negativo se vende por debajo del coste",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           VisibleAlCrear = false,
           EditableAlEditar = false,
           Obligatorio = false,
           Formato = enumFormato.Porcentaje,
           Fila = 3,
           Columna = 1)
        ]
        public decimal? Margen { get; set; }

    }
}
