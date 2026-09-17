using ModeloDeDto.Contabilidad;
using ServicioDeDatos.MaestrosTecnico;
using Utilidades;

namespace ModeloDeDto.MaestrosTecnico
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5  , MostrarExpresion = "[Nombre]")]
    public class NaturalezaDto : ElementoDto
    {
        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Clase",
            Ayuda = "Seleccione ...",
            TipoDeControl = enumTipoControl.Enumerado,
            Tipo = typeof(enumClaseUnitario),
            GuardarEn = nameof(Clase),
            Obligatorio = false,
            Fila = 0,
            Columna = 0
          )
        ]
        public enumClaseUnitario? Clase { get; set; }

        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Sigla",
            Ayuda = "Indique las siglas de la naturaleza contable",
            Tipo = typeof(string),
            Fila = 0,
            Columna = 1,
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
            Columna = 2,
            Ordenar = true,
            Obligatorio = true,
            LongitudMaxima = 250
          )
        ]
        public string Nombre { get; set; }

        //----------------------------------------------
        [IUPropiedad(Etiqueta = "Id la unidad de medida", Visible = false)]
        public int? IdUnidad { get; set; }

        [IUPropiedad(
            Etiqueta = "Unidad",
            Ayuda = "Seleccione la unidad de medida que propone esta naturaleza",
            TipoDeControl = enumTipoControl.ListaDeElemento,
            SeleccionarDe = typeof(UnidadDto),
            Controlador = nameof(enumControladoresMt.Unidades),
            GuardarEn = nameof(IdUnidad),
            Obligatorio = false,
            Fila = 1,
            Columna = 0
          )
        ]
        public string Unidad { get; set; }

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
            Columna = 2
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
            Columna = 3
            )
        ]
        public string CuentaDeIngreso { get; set; }

        //----------------------------------------------
        [IUPropiedad(Visible = false)]
        public string Expresion => $"({Sigla}) {Nombre}";

    }
}
