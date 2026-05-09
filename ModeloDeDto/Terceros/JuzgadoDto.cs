using ModeloDeDto.Callejero;
using ServicioDeDatos;
using Utilidades;

namespace ModeloDeDto.Terceros
{
    public static class ltrJuzgado
    {
        public static readonly string Juzgados = nameof(Juzgados);
        public static readonly string Juzgado = nameof(Juzgado);
    }

    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class JuzgadoDto : ElementoDto
    {
        [IUPropiedad(Etiqueta = "Id de la clase de juzgado", Visible = false)]
        public int IdClase { get; set; }

        [IUPropiedad(
            Etiqueta = "Clase",
            Ayuda = "Indique clase de juzgado",
            TipoDeControl = enumTipoControl.ListaDeElemento,
            SeleccionarDe = typeof(ClaseDeJuzgadoDto),
            Controlador = nameof(enumControladoresTerceros.ClasesDeJuzgado),
            GuardarEn = nameof(IdClase),
            MostrarExpresion = ClaseDeJuzgadoDto.MostrarExpresion,
            Fila = 0,
            Columna = 0,
            VisibleEnGrid = false,
            Obligatorio = true,
            OnBlur = "javascript:" + nameof(enumNameSpaceTs.Terceros) + "." + nameof(enumFunctionTs.Juzgado_Clase_OnBlur) + "(this)"
            )
        ]
        public string Clase { get; set; }

        //----------------------------------------------

        [IUPropiedad(
            Etiqueta = nameof(Municipio),
            Ayuda = "Seleccione el municipio",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(MunicipioDto),
            GuardarEn = nameof(IdMunicipio),
            Controlador = nameof(enumControladoresCallejero.Municipios),
            VistaDondeNavegar = enumVistasCallejero.CrudMunicipios,
            AlSeleccionarBlanquearControl = "",
            LongitudMinimaParaBuscar = 1,
            RestringidoPorControl = "",
            PropiedadRestrictora = "",
            CriterioDeBusqueda = enumCriteriosDeFiltrado.comienza,
            Fila = 0,
            Columna = 1,
            Obligatorio = true,
            VisibleEnGrid = false,
            trasSeleccionar = "javascript:" + nameof(enumNameSpaceTs.Terceros) + "." + nameof(enumFunctionTs.Juzgado_Municipio_OnSelect) + "(this)"
            )
        ]
        public string Municipio { get; set; }

        [IUPropiedad(Etiqueta = "Id del municipio", Visible = false)]
        public int IdMunicipio { get; set; }

        //---------------------------------------------------------------------

        [IUPropiedad(
            Etiqueta = "Calificador",
            Ayuda = "Calificador del juzgado",
            Fila = 0,
            Columna = 2,
            Obligatorio = true,
            VisibleEnGrid = false,
            AutoSpan = false,
            OnChange = "javascript:" + nameof(enumNameSpaceTs.Terceros) + "." + nameof(enumFunctionTs.Juzgado_Calificador_Change) + "(this)"
          )
        ]
        public string Calificador { get; set; }


        //--------------------------------------------------------------------

        [IUPropiedad(
               Etiqueta = "Juzgado",
               Ayuda = "Nombre del juzgado",
               Tipo = typeof(string),
               EditableAlCrear = false,
               EditableAlEditar = false,
               Fila = 1,
               Columna = 0,
               Ordenar = true,
               PorAnchoMnt = 50,
               AutoSpan = true
               )
        ]
        public string Nombre { get; set; }

    }
}
