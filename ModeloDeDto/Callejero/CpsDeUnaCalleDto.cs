using Utilidades;
using ServicioDeDatos.Callejero;
using ServicioDeDatos;

namespace ModeloDeDto.Callejero
{

    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class CpsDeUnaCalleDto : ElementoDto, IRelacionDto
    {
        public static string ExpresionElemento = nameof(CodigoPostal);

        //----------------------------------------------------------
        [IUPropiedad(Etiqueta = "Calle",
            Ayuda = "códigos postales de una calle",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Calle),
            Fila = 0,
            Columna = 0,
            VisibleEnGrid = false,
            AutoSpan = true
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
            Etiqueta = "Id del código postal",
            Visible = false
            )
        ]
        public int IdCp { get; set; }


        [IUPropiedad(
            Etiqueta = "Código Postal",
            Ayuda = "Indique el CP",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(CodigoPostalDto),
            GuardarEn = nameof(IdCp),
            Controlador = nameof(enumControladoresCallejero.CodigosPostales),
            VistaDondeNavegar = enumVistasCallejero.CrudCodigosPostales,
            RestringidoPorControl = nameof(IdCalle),
            PropiedadRestrictora = nameof(IdCalle),
            BuscarPor = nameof(CodigoPostalDtm.Codigo),
            CriterioDeBusqueda = enumCriteriosDeFiltrado.comienza,
            BlanquearAlSalir = false,
            LongitudMinimaParaBuscar = 1,
            Fila = 0,
            Columna =2,
            Ordenar = true,
            OrdenarListaDinamicaPor = nameof(CodigoPostalDto.Codigo)
            )
        ]
        public string CodigoPostal { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Mano",
           Ayuda = "Lado de la calle",
           TipoDeControl = enumTipoControl.Editor,
           Fila = 2,
           Columna = 0,
            Obligatorio = false,
            ValorPorDefecto = ParseosDeManosDeUnaCalle.Ambos,
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
