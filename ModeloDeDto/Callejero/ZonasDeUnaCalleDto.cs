using Utilidades;
using ServicioDeDatos.Callejero;
using ServicioDeDatos;

namespace ModeloDeDto.Callejero
{

    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class ZonasDeUnaCalleDto : ElementoDto, IRelacionDto
    {
        public static string ExpresionElemento = nameof(Zona);

        //----------------------------------------------------------
        [IUPropiedad(Etiqueta = "Calle",
            Ayuda = "zonas de una calle",
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
            Etiqueta = "Id del zona",
            Visible = false
            )
        ]
        public int idZona { get; set; }


        [IUPropiedad(
            Etiqueta = "Zona",
            Ayuda = "Indique la zona",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(ZonaDto),
            GuardarEn = nameof(idZona),
            RestringidoPorControl = nameof(IdCalle),
            Controlador = nameof(enumControladoresCallejero.Zonas),
            VistaDondeNavegar = enumVistasCallejero.CrudZonas,
            OnClick = "javascript:" + nameof(enumNameSpaceTs.Callejero) + "." + nameof(enumFunctionTs.Calle_IrAZonasDeUnaCalle) + "()",
            PropiedadRestrictora = ltrZonasDeUnaCalleDtm.IdMunicipio,
            BuscarPor = nameof(ZonaDtm.Nombre),
            CriterioDeBusqueda = enumCriteriosDeFiltrado.contiene,
            LongitudMinimaParaBuscar = 1,
            Fila = 0,
            Columna = 1,
            Ordenar = true,
            AutoSpan = true,
            BlanquearAlSalir =false
            )
        ]
        public string Zona { get; set; }

    }
}
