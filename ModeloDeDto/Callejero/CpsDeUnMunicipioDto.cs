using Utilidades;
using ServicioDeDatos.Callejero;
using ServicioDeDatos;

namespace ModeloDeDto.Callejero
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class CpsDeUnMunicipioDto : ElementoDto, IRelacionDto
    {
        public static string ExpresionElemento = $"{nameof(CodigoPostal)}-{nameof(Municipio)}";

        //----------------------------------------------------------
        [IUPropiedad(Etiqueta = "Municipio",
            Ayuda = "códigos postales de un municipio",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Municipio),
            CriterioDeBusqueda = enumCriteriosDeFiltrado.diferente,
            Fila = 0,
            Columna = 0,
            VisibleEnGrid = false
            )
        ]
        public int IdMunicipio { get; set; }

        [IUPropiedad(
            Etiqueta = "Municipio",
            Visible = false
            )
        ]
        public string Municipio { get; set; }

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
            RestringidoPorControl = nameof(IdMunicipio),
            Controlador = nameof(enumControladoresCallejero.CodigosPostales),
            PropiedadRestrictora = nameof(IdMunicipio),
            BuscarPor = nameof(CodigoPostalDtm.Codigo),
            CriterioDeBusqueda = enumCriteriosDeFiltrado.contiene,
            LongitudMinimaParaBuscar = 1,
            Fila = 1,
            Columna = 0,
            Ordenar = true,
            BlanquearAlSalir = false,
            Obligatorio = false
            )
        ]
        public string CodigoPostal { get; set; }


    }
}
