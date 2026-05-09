using Utilidades;

namespace ModeloDeDto.Contabilidad
{
    public static class ltrIrpf
    {
        public static readonly string Irpfs = nameof(Irpfs);
        public static readonly string Irpf = nameof(Irpf);
    }


    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, MostrarExpresion = nameof(Expresion))]
    public class IrpfDto : ElementoDto
    {
        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Código",
            Ayuda = "Indique el código del irpf",
            VisibleEnGrid = false,
            Fila = 0,
            Columna = 0,
            Ordenar = true,
            Obligatorio = true,
            LongitudMaxima = 10
          )
        ]
        public string Codigo { get; set; }
        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Irpf",
            Ayuda = "tipo de irpf",
            VisibleEnGrid = false,
            Fila = 0,
            Columna = 1,
            Ordenar = true,
            Obligatorio = true,
            LongitudMaxima = 250
          )
        ]
        public string Nombre { get; set; }


        [IUPropiedad(
            Etiqueta = "Irpf",
            VisibleEnGrid = true, 
            VisibleEnEdicion = false)]
        public string Expresion { get; set; }


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
            Columna = 0
            )
        ]
        public string Cuenta { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Irpf(%)",
           Tipo = typeof(decimal),
           Ayuda = "porcentaje de irpf",
           TipoDeControl = enumTipoControl.Editor,
           Obligatorio = true,
           Formato = enumFormato.Porcentaje,
           Fila = 0,
           Columna = 2)
        ]
        public decimal Porcentaje { get; set; }

    }
}
