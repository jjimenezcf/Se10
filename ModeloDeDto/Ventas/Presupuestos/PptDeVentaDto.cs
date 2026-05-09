using ModeloDeDto.Contabilidad;
using Utilidades;


namespace ModeloDeDto.Presupuesto
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, OpcionDeBorrar = false)]
    public class PptDeVentaDto : EsUnaAmpliacionDto
    {
        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Descuento (%)",
           Tipo = typeof(decimal),
           Ayuda = "porcentaje de descuento por línea",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           Obligatorio = false,
           Formato = enumFormato.Numero_2,
           Fila = 0,
           Columna = 0)
        ]
        public decimal? Descuento { get; set; }

        //----------------------------------------------
        [IUPropiedad(Etiqueta = "Id del iva repercutido", Visible = false)]
        public int ? IdIvaR { get; set; }

        [IUPropiedad(
            Etiqueta = "Iva",
            Ayuda = "Seleccione el tipo de iva",
            TipoDeControl = enumTipoControl.ListaDeElemento,
            SeleccionarDe = typeof(IvaRepercutidoDto),
            Controlador = nameof(enumControladoresContables.IvasRepercutido),
            GuardarEn = nameof(IdIvaR),
            OnChange = "javascript:" + nameof(enumNameSpaceTs.Presupuesto) + "." + nameof(enumFunctionTs.Ppt_IvaRepercutidoCambiado) + "()",
            Obligatorio = false,
            ColSpan = 2,
            VisibleEnGrid = false,
            Fila = 0,
            Columna = 1
            )
        ]
        public string IvaRepercutido { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Iva(%)",
           Tipo = typeof(decimal),
           Ayuda = "Porcentaje de iva por defecto",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Obligatorio = false,
           Formato = enumFormato.Numero_2,
           Fila = 0,
           Columna = 3)
        ]
        public decimal ? Iva { get; set; }

    }
}
