using ModeloDeDto.Callejero;
using ServicioDeDatos;
using Utilidades;

namespace ModeloDeDto.Terceros
{
    public static class ltrBanco
    {
        public static readonly string Bancos = nameof(Bancos);
        public static readonly string BancoNoDefinido = "Banco no definido en la BD";
    }

    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, MostrarExpresion = "([Codigo]) [Nombre]")]
    public class BancoDto : ElmentoAuditadoDto
    {
        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = nameof(Pais),
            Ayuda = "Seleccione el país",
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
            Etiqueta = "Banco",
            Ayuda = "Indique el nombre del banco",
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
            Etiqueta = "Bic/Swift",
            Ayuda = "Bic del banco",
            Tipo = typeof(string),
            Fila = 1,
            Columna = 0,
            Obligatorio = true,
            LongitudMaxima = 11
          )
        ]
        public string BicSwift { get; set; }

        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Código de la entidad",
            EtiquetaGrid ="Entidad",
            Ayuda = "Código de la banco",
            Tipo = typeof(string),
            Fila = 1,
            Columna = 1,
            Ordenar = true,
            Obligatorio = true,
            LongitudMaxima = 4,
            Posicion = 0,
            Alineada = enumAliniacion.derecha
          )
        ]
        public string Codigo { get; set; }

        //----------------------------------------------

        [IUPropiedad(
            Etiqueta = "ISO",
            Ayuda = "ISO del país",
            Tipo = typeof(string),
            Fila = 1,
            Columna = 1,
            Obligatorio = true,
            LongitudMaxima = 2,
            Posicion = 1,
            Alineada = enumAliniacion.derecha,
            VisibleAlCrear = false,
            EditableAlEditar = false
          )
        ]
        public string Iso2 { get; set; }

    }
}
