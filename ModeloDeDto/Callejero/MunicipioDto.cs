using ServicioDeDatos;
using Utilidades;

namespace ModeloDeDto.Callejero
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class MunicipioDto : ElmentoAuditadoDto
    {
        [IUPropiedad(
            Etiqueta = nameof(Pais),
            Ayuda = "Seleccione el país",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(PaisDto),
            GuardarEn = nameof(IdPais),
            Controlador = nameof(enumControladoresCallejero.Paises),
            VistaDondeNavegar = enumVistasCallejero.CrudPaises,
            AlSeleccionarBlanquearControl = nameof(Provincia),
            CriterioDeBusqueda = enumCriteriosDeFiltrado.comienza,
            LongitudMinimaParaBuscar = 1,
            Fila = 0,
            Columna = 0,
            Obligatorio = true,
            Ordenar = true
            )
         ]
        public string Pais { get; set; }

        [IUPropiedad(Etiqueta = "Id del pais", Visible = false)]
        public int IdPais { get; set; }

        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = nameof(Provincia),
            Ayuda = "Seleccione la provincia",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(ProvinciaDto),
            GuardarEn = nameof(IdProvincia),
            Controlador = nameof(enumControladoresCallejero.Provincias),
            VistaDondeNavegar = enumVistasCallejero.CrudProvincias,
            LongitudMinimaParaBuscar = 1,
            RestringidoPorControl = nameof(Pais),
            PropiedadRestrictora = nameof(IdPais),
            CriterioDeBusqueda = enumCriteriosDeFiltrado.comienza,
            Fila = 0,
            Columna = 1,
            Obligatorio = true,
            Ordenar = true
            )
        ]
        public string Provincia { get; set; }

        [IUPropiedad(Etiqueta = "Id de la provincia", Visible = false)]
        public int IdProvincia { get; set; }
        //----------------------------------------------

        [IUPropiedad(
            Etiqueta = "Municipio",
            Ayuda = "Indique el nombre del municipio",
            Tipo = typeof(string),
            Fila = 1,
            Columna = 0,
            Ordenar = true,
            PorAnchoMnt = 50,
            Obligatorio = true,
            LongitudMaxima = 250
          )
        ]
        public string Nombre { get; set; }

        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "DC",
            Ayuda = "Dígito postal",
            Tipo = typeof(string),
            Fila = 1,
            Columna = 1,
            Obligatorio = true,
            LongitudMaxima = 5,
            Alineada = enumAliniacion.derecha
          )
        ]
        public string DC { get; set; }
    }
}
