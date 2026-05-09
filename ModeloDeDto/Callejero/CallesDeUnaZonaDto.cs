using Utilidades;
using ServicioDeDatos.Callejero;
using ServicioDeDatos;

namespace ModeloDeDto.Callejero
{

    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class CallesDeUnaZonaDto : ElementoDto, IRelacionDto
    {
        public static string ExpresionElemento = $"{nameof(Calle)} ({nameof(Zona)})";

        //----------------------------------------------------------
        [IUPropiedad(Etiqueta = "Zona",
            Ayuda = "Calles de una Zona",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Zona),
            Fila = 0,
            Columna = 0,
            VisibleEnGrid = false
            )
        ]
        public int IdZona { get; set; }

        [IUPropiedad(
            Etiqueta = "Zona",
            Visible = false
            )
        ]
        public string Zona { get; set; }

        //----------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Calle",
            Ayuda = "Indique la calle",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(CalleDto),
            GuardarEn = nameof(idCalle),
            RestringidoPorControl = nameof(IdZona),
            Controlador = nameof(enumControladoresCallejero.Calles),
            PropiedadRestrictora = nameof(IdZona),
            BuscarPor = nameof(CalleDtm.Nombre),
            CriterioDeBusqueda = enumCriteriosDeFiltrado.contiene,
            LongitudMinimaParaBuscar = 1,
            Fila = 1,
            Columna = 0,
            Ordenar = true
            )
        ]
        public string Calle { get; set; }

        [IUPropiedad(
            Etiqueta = "Id de la calle",
            Visible = false
            )
        ]
        public int idCalle { get; set; }


    }
}
