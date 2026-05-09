using Utilidades;
using ServicioDeDatos.Callejero;
using ServicioDeDatos;

namespace ModeloDeDto.Callejero
{

    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class CallesDeUnBarrioDto : ElementoDto, IRelacionDto
    {
        public static string ExpresionElemento = $"{nameof(Calle)} ({nameof(Barrio)})";

        //----------------------------------------------------------
        [IUPropiedad(Etiqueta = "Barrio",
            Ayuda = "Calles de un barrio",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Barrio),
            Fila = 0,
            Columna = 0,
            VisibleEnGrid = false
            )
        ]
        public int IdBarrio { get; set; }

        [IUPropiedad(
            Etiqueta = "Barrio",
            Visible = false
            )
        ]
        public string Barrio { get; set; }

        //----------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Id de la calle",
            Visible = false
            )
        ]
        public int idCalle { get; set; }


        [IUPropiedad(
            Etiqueta = "Calle",
            Ayuda = "Indique la calle",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(CalleDto),
            GuardarEn = nameof(idCalle),
            RestringidoPorControl = nameof(IdBarrio),
            Controlador = nameof(enumControladoresCallejero.Calles),
            PropiedadRestrictora = nameof(IdBarrio),
            BuscarPor = nameof(CalleDtm.Nombre),
            CriterioDeBusqueda = enumCriteriosDeFiltrado.contiene,
            LongitudMinimaParaBuscar = 1,
            Fila = 1,
            Columna = 0,
            Ordenar = true
            )
        ]
        public string Calle { get; set; }

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
