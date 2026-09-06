using ServicioDeDatos;
using Utilidades;
using ModeloDeDto.MaestrosTecnico;
using ServicioDeDatos.MaestrosTecnico;

namespace ModeloDeDto.Logistica
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, OpcionDeBorrar = false)]
    public class LineasDeUnaRegularizacionDto : EsUnDetalleDto
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
            trasSeleccionar = "javascript:" + nameof(enumNameSpaceTs.Logistica) + "." + nameof(enumFunctionTs.Ral_Tras_Seleccionar_Unitario) + "([" + nameof(enumParamTs.idLista) + "])",
            trasBlanquear = "javascript:" + nameof(enumNameSpaceTs.Logistica) + "." + nameof(enumFunctionTs.Ral_Tras_Blanquear_Unitario) + "()",
            OtrosParametrosDeFiltrado = "javascript: " + nameof(enumNameSpaceTs.Logistica) + "." + nameof(enumFunctionTs.Ral_FiltrosPorClaseDeUnitario) + "(this)"
            )
        ]
        public string Unitario { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Cantidad",
           Tipo = typeof(decimal),
           Ayuda = "cantidad de la regularización",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           OnBlur = "javascript:" + nameof(enumNameSpaceTs.Logistica) + "." + nameof(enumFunctionTs.Ral_CalcularImportesDeLinea) + "()",
           Obligatorio = true,
           Formato = enumFormato.Numero_6,
           Fila = 2,
           Columna = 0)
        ]
        public decimal Cantidad { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Precio",
           Tipo = typeof(decimal),
           Ayuda = "precio del material",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           OnBlur = "javascript:" + nameof(enumNameSpaceTs.Logistica) + "." + nameof(enumFunctionTs.Ral_CalcularImportesDeLinea) + "()",
           Obligatorio = true,
           Formato = enumFormato.Numero_6,
           Fila = 2,
           Columna = 1)
        ]
        public decimal Precio { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Total",
           Tipo = typeof(decimal),
           Ayuda = "importe de la línea (cantidad x precio)",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           MantenerHuecoDeLaIzquierda = true,
           Obligatorio = false,
           Formato = enumFormato.Moneda,
           Fila = 2,
           Columna = 3)
        ]
        public decimal Total { get; set; }

    }
}
