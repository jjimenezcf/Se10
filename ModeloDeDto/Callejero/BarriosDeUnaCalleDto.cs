using Utilidades;
using ServicioDeDatos.Callejero;
using ServicioDeDatos;

namespace ModeloDeDto.Callejero
{

    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class BarriosDeUnaCalleDto : ElementoDto, IRelacionDto
    {
        public static string ExpresionElemento = nameof(Barrio);

        //----------------------------------------------------------
        [IUPropiedad(Etiqueta = "Calle",
            Ayuda = "barrios de una calle",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Calle),
            Fila = 0,
            Columna = 0,
            VisibleEnGrid = false
            )
        ]
        public int IdCalle { get; set; }

        [IUPropiedad(
            Etiqueta = "Calle",
            Visible = false
            )
        ]
        public string Calle { get; set; }

        //----------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Id del barrio",
            Visible = false
            )
        ]
        public int idBarrio { get; set; }


        [IUPropiedad(
            Etiqueta = "Barrio",
            Ayuda = "Indique el barrio",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(BarrioDto),
            GuardarEn = nameof(idBarrio),
            RestringidoPorControl = nameof(IdCalle),
            Controlador = nameof(enumControladoresCallejero.Barrios),
            VistaDondeNavegar = enumVistasCallejero.CrudBarrios,
            OnClick = "javascript:" + nameof(enumNameSpaceTs.Callejero) + "." + nameof(enumFunctionTs.Calle_IrABarriosDeUnaCalle) + "()",
            PropiedadRestrictora = ltrBarriosDeUnaCalleDtm.IdMunicipio,
            BuscarPor = nameof(BarrioDtm.Nombre),
            CriterioDeBusqueda = enumCriteriosDeFiltrado.contiene,
            LongitudMinimaParaBuscar = 1,
            Fila = 0,
            Columna = 1,
            Ordenar = true,
            AutoSpan = true,
            BlanquearAlSalir = false
            )
        ]
        public string Barrio { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Mano",
           Ayuda = "Lado de la calle",
           TipoDeControl = enumTipoControl.Editor,
           Fila = 2,
           Columna = 0,
            Obligatorio = false,
            ValorPorDefecto ='A',
           LongitudMaxima = 1)
        ]
        public string Mano { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Desde",
           Ayuda = "Nº de policía de inicio",
           TipoDeControl = enumTipoControl.Editor,
           Tipo = typeof(int),
           Fila = 2,
           Columna = 1,
            Obligatorio = false,
            ValorPorDefecto = 0,
           LongitudMaxima = 5)
        ]
        public int ? Desde { get; set; }
        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Hasta",
           Ayuda = "Nº de policía de fin",
           TipoDeControl = enumTipoControl.Editor,
           Tipo = typeof(int),
           Fila = 2,
           Columna = 2,
            Obligatorio = false,
           LongitudMaxima = 5)
        ]
        public int ? Hasta { get; set; }
    }
}
