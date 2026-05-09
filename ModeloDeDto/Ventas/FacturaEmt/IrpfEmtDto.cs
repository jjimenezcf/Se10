using ModeloDeDto.Contabilidad;
using Utilidades;


namespace ModeloDeDto.Ventas
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, OpcionDeBorrar = false)]
    public class IrpfEmtDto : EsUnaAmpliacionDto
    {

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "B.I sujeta",
           Tipo = typeof(decimal),
           Ayuda = "bi sujeta",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           OnFocus = "javascript:" + nameof(enumNameSpaceTs.Venta) + "." + nameof(enumFunctionTs.Fae_Al_Entrar_BI_Sujeta) + "()",
           OnBlur = "javascript:" + nameof(enumNameSpaceTs.Venta) + "." + nameof(enumFunctionTs.Fae_CalcularIrpf) + "()",
           Obligatorio = false,
           Formato = enumFormato.Numero_6,
           Fila = 0,
           Columna = 0)
        ]
        public decimal? BiSujeta { get; set; }

        //----------------------------------------------
        [IUPropiedad(Etiqueta = "Id del irpf", Visible = false)]
        public int? IdIrpf { get; set; }

        [IUPropiedad(
            Etiqueta = "Irpf",
            Ayuda = "tipo de irpf",
            TipoDeControl = enumTipoControl.ListaDeElemento,
            SeleccionarDe = typeof(IrpfDto),
            Controlador = nameof(enumControladoresContables.Irpfs),
            GuardarEn = nameof(IdIrpf),
            OnChange = "javascript:" + nameof(enumNameSpaceTs.Venta) + "." + nameof(enumFunctionTs.Fae_Tras_Cambiar_Tipo_Irpf) + "()",
            OnBlur = "javascript:" + nameof(enumNameSpaceTs.Venta) + "." + nameof(enumFunctionTs.Fae_CalcularIrpf) + "()",
            Obligatorio = false,
            VisibleEnGrid = false,
            Fila = 0,
            Columna = 1
            )
        ]
        public string TipoIrpf { get; set; }

        //--------------------------------------------
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
           Fila = 0,
           Columna = 2)
        ]
        public decimal? Irpf { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Importe Irpf",
           Tipo = typeof(decimal),
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Obligatorio = false,
           Formato = enumFormato.Moneda,
           Fila = 0,
           Columna = 3)
        ]
        public decimal? Importe { get; set; }
    }
}
