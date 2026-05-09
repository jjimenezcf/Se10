using ServicioDeDatos.Contabilidad;
using Utilidades;

namespace ModeloDeDto.Contabilidad
{
    public static class ltrIvaRep
    {
        public static readonly string IvasRepercutido = nameof(IvasRepercutido);
        public static readonly string IvaRepercutido = nameof(IvaRepercutido);
    }

    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, MostrarExpresion = nameof(Expresion))]
    public class IvaRepercutidoDto : ElementoDto
    {

        //-------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Clase",
            Ayuda = "Indique la clase del iva",
            TipoDeControl = enumTipoControl.Enumerado,
            Tipo = typeof(enumClasesDeIvaRep),
            GuardarEn = nameof(Clase),
            VisibleEnGrid = false,
            Fila = 0,
            Columna = 0,
            Ordenar = true,
            Obligatorio = true,
            EditableAlEditar = false
          )]
        public string Clase { get; set; }

        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Iva repercutido",
            Ayuda = "tipo de iva",
            VisibleEnGrid = false,
            Fila = 0,
            Columna = 1,
            Ordenar = true,
            Obligatorio = true,
            LongitudMaxima = 250
          )
        ]
        public string Nombre { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Iva exento",
           Tipo = typeof(decimal),
           Ayuda = "Iva exento",
           TipoDeControl = enumTipoControl.Check,
           VisibleEnGrid = false,
           Obligatorio = false,
           css = enumCssControles.CheckEnLinea,
           ValorPorDefecto = false,
           VisibleAlCrear = true,
           EditableAlEditar = true,
           OnChange = "javascript:" + nameof(enumNameSpaceTs.Contabilidad) + "." + nameof(enumFunctionTs.IvaRep_AlCambiar_Exento) + "(this)",
           Fila = 1,
           Columna = 0)
        ]
        public bool Exento { get; set; }

        //--------------------------------------------
        [IUPropiedad(VisibleEnGrid = true, Etiqueta = "Iva", VisibleEnEdicion = false)]
        public string Expresion { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Iva(%)",
           Tipo = typeof(decimal),
           Ayuda = "porcentaje de iva",
           TamanoFijo = "10em",
           TipoDeControl = enumTipoControl.Editor,
           Obligatorio = true,
           Formato = enumFormato.Porcentaje,
           Fila = 1,
           Columna = 0)
        ]
        public decimal Porcentaje { get; set; }


        //----------------------------------------------
        [IUPropiedad(Etiqueta = "Id la cuenta contable", Visible = false)]
        public int IdCuenta { get; set; }

        [IUPropiedad(
            Etiqueta = "Cuenta",
            Ayuda = "Seleccione la cuenta contable",
            TipoDeControl = enumTipoControl.ListaDeElemento,
            SeleccionarDe = typeof(CuentaDto),
            Controlador = nameof(enumControladoresContables.Cuentas),
            GuardarEn = nameof(IdCuenta),
            VisibleEnGrid = true,
            AutoSpan = true,
            Fila = 1,
            Columna = 1
            )
        ]
        public string Cuenta { get; set; }

        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Motivo de exención",
            Ayuda = "Motivo de exención fiscal",
            VisibleEnGrid = false,
            Fila = 2,
            Columna = 0,
            Obligatorio = false,
            AutoSpan = true,
            LongitudMaxima = 250
          )
        ]
        public string DescripcionFiscal { get; set; }

    }
}
