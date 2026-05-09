using ModeloDeDto.Contabilidad;
using Utilidades;

namespace ModeloDeDto.MaestrosTecnico
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5  , MostrarExpresion = "[Nombre]")]
    public class NaturalezaDto : ElementoDto
    {        
        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Sigla",
            Ayuda = "Indique las siglas de la naturaleza contable",
            Tipo = typeof(string),
            Fila = 0,
            Columna = 0,
            Ordenar = true,
            Obligatorio = true,
            LongitudMaxima = 5
          )
        ]
        public string Sigla { get; set; }

        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Naturaleza contable",
            Ayuda = "Indique el nombre",
            Tipo = typeof(string),
            Fila = 0,
            Columna = 1,
            Ordenar = true,
            Obligatorio = true,
            LongitudMaxima = 250
          )
        ]
        public string Nombre { get; set; }

        //----------------------------------------------
        [IUPropiedad(Etiqueta = "Id la cuenta contable del gasto", Visible = false)]
        public int? IdCuentaDeGasto { get; set; }

        [IUPropiedad(
            Etiqueta = "Cuenta de gasto",
            Ayuda = "Seleccione la cuenta contable",
            TipoDeControl = enumTipoControl.ListaDeElemento,
            SeleccionarDe = typeof(CuentaDto),
            Controlador = nameof(enumControladoresContables.Cuentas),
            GuardarEn = nameof(IdCuentaDeGasto),
            VisibleEnGrid = true,
            AutoSpan = true,
            Fila = 1,
            Columna = 0
            )
        ]
        public string CuentaDeGasto { get; set; }

        //----------------------------------------------
        [IUPropiedad(Etiqueta = "Id la cuenta contable del gasto", Visible = false)]
        public int? IdCuentaDeIngreso { get; set; }

        [IUPropiedad(
            Etiqueta = "Cuenta de ingreso",
            Ayuda = "Seleccione la cuenta contable",
            TipoDeControl = enumTipoControl.ListaDeElemento,
            SeleccionarDe = typeof(CuentaDto),
            Controlador = nameof(enumControladoresContables.Cuentas),
            GuardarEn = nameof(IdCuentaDeIngreso),
            VisibleEnGrid = true,
            AutoSpan = true,
            Fila = 1,
            Columna = 1
            )
        ]
        public string CuentaDeIngreso { get; set; }

        //----------------------------------------------
        [IUPropiedad(Visible = false)]
        public string Expresion => $"({Sigla}) {Nombre}";

    }
}
