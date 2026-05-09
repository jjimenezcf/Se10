using ServicioDeDatos;
using Utilidades;

namespace ModeloDeDto.Callejero
{

    public class ltrProvinciaDto
    {
        public static readonly string CodigoPostal = nameof(CodigoPostal);
    }

    [IUDto(AnchoEtiqueta = 20
          , AnchoSeparador = 5
          , MostrarExpresion = "([Codigo]) [Nombre]")]
    public class ProvinciaDto : ElmentoAuditadoDto
    {
        [IUPropiedad(
            Etiqueta = nameof(Pais),
            Ayuda = "Seleccione el pais",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(PaisDto),
            GuardarEn = nameof(IdPais),
            Controlador = nameof(enumControladoresCallejero.Paises),
            VistaDondeNavegar = enumVistasCallejero.CrudPaises,
            CriterioDeBusqueda = enumCriteriosDeFiltrado.comienza,
            LongitudMinimaParaBuscar = 1,
            Fila = 0,
            Columna = 0,
            Obligatorio = true,
            Ordenar = true
            )
        ]
        public string Pais { get; set; }

        [IUPropiedad(Etiqueta = "Id del pais",
            Visible = false)]
        public int IdPais { get; set; }

        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Provincia",
            Ayuda = "Indique el nombre de la provincia",
            Tipo = typeof(string),
            Fila = 0,
            Columna = 1,
            Ordenar = true,
            PorAnchoMnt = 50,
            Obligatorio = true,
            LongitudMaxima = 250
          )
        ]
        public string Nombre { get; set; }

        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Código",
            Ayuda = "Código de la provincia",
            Tipo = typeof(string),
            Fila = 0,
            Columna = 2,
            Ordenar = true,
            Obligatorio = true,
            LongitudMaxima = 2,
            Alineada = enumAliniacion.derecha
          )
        ]
        public string Codigo { get; set; }

        //----------------------------------------------


        [IUPropiedad(
            Etiqueta = "Sigla",
            Ayuda = "Sigla de la provincia",
            Tipo = typeof(string),
            Fila = 1,
            Columna = 0,
            Obligatorio = true,
            LongitudMaxima = 3,
            Alineada = enumAliniacion.derecha
          )
        ]
        public string Sigla { get; set; }

        //----------------------------------------------

        [IUPropiedad(
            Etiqueta = "Prefijo telefónico",
            Ayuda = "Asigne el prefijo telefónico de la provincia",
            Tipo = typeof(string),
            Fila = 1,
            Columna = 1,
            Obligatorio = true,
            LongitudMaxima = 10,
            Alineada = enumAliniacion.derecha
          )
        ]
        public string Prefijo { get; set; }

    }
}
