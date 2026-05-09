using ServicioDeDatos.Contabilidad;
using Utilidades;

namespace ModeloDeDto.Contabilidad
{
    public static class ltrIvaSop
    {
        public static readonly string IvasSoportado = nameof(IvasSoportado);
        public static readonly string IvaSoportado = nameof(IvaSoportado);
    }


    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, MostrarExpresion = nameof(Detalle))]
    public class IvaSoportadoDto : ElementoDto
    {
        //-------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Código",
            Ayuda = "Indique el código del iva",
            TipoDeControl = enumTipoControl.Enumerado,
            Tipo = typeof(enumClasesDeIvaSop),
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
            Etiqueta = "Iva soportado",
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
           Etiqueta = "Iva(%)",
           Tipo = typeof(decimal),
           Ayuda = "porcentaje de iva",
           TamanoFijo = "10em",
           TipoDeControl = enumTipoControl.Editor,
           Obligatorio = true,
           Formato = enumFormato.Porcentaje,
           Fila = 0,
           Columna = 2)
        ]
        public decimal Porcentaje { get; set; }

        //--------------------------------------------
        [IUPropiedad(Visible = false)]
        public string Expresion { get; set; }

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
           OnChange = "javascript:" + nameof(enumNameSpaceTs.Contabilidad) + "." + nameof(enumFunctionTs.IvaSop_AlCambiar_Exento) + "(this)",
           Fila = 1,
           Columna = 0)
        ]
        public bool Exento { get; set; }

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
            AutoSpan = true,
            VisibleEnGrid = false,
            Fila = 1,
            Columna = 1
            )
        ]
        public string Cuenta { get; set; }

        //----------------------------------------------
        [IUPropiedad(Visible = true, Etiqueta = "Iva", VisibleEnEdicion = false)]
        public string Detalle { get; set; }

    }
}
