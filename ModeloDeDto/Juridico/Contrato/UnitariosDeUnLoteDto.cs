using Utilidades;
using ServicioDeDatos;
using ModeloDeDto.MaestrosTecnico;
using ServicioDeDatos.MaestrosTecnico;

namespace ModeloDeDto.Juridico
{

    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class UnitariosDeUnLoteDto : ElementoDto, IRelacionDto
    {
        public static string ExpresionElemento = $"{nameof(Unitario)} ({nameof(Lote)})";

        //----------------------------------------------------------
        [IUPropiedad(Etiqueta = "Lote",
            Ayuda = "Unitarios de un lote",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Lote),
            CriterioDeBusqueda = enumCriteriosDeFiltrado.noEstaRelacionado,
            Fila = 0,
            Columna = 0,
            VisibleEnGrid = false
            )
        ]
        public int IdLote { get; set; }

        [IUPropiedad(
            Etiqueta = "Lote",
            Visible = false
            )
        ]
        public string Lote { get; set; }

        //----------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Id de la unitario",
            Visible = false
            )
        ]
        public int IdUnitario { get; set; }


        [IUPropiedad(
            Etiqueta = "Unitario",
            Ayuda = "Indique el unitario",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(UnitarioDto),
            GuardarEn = nameof(IdUnitario),
            RestringidoPorControl = nameof(IdLote),
            Controlador = nameof(enumControladoresMt.Unitarios),
            VistaDondeNavegar = enumVistasMts.CrudUnitarios,
            PropiedadRestrictora = nameof(IdLote),
            BuscarPor = nameof(UnitarioDtm.Nombre),
            CriterioDeBusqueda = enumCriteriosDeFiltrado.contiene,
            LongitudMinimaParaBuscar = 1,
            EditableAlEditar = false,
            EditableAlCrear = true,
            trasSeleccionar = "javascript:" + nameof(enumNameSpaceTs.Juridico) + "." + nameof(enumFunctionTs.Lot_Tras_Seleccionar_Unitario) + "([" + nameof(enumParamTs.idLista) + "])",
            trasBlanquear = "javascript:" + nameof(enumNameSpaceTs.Juridico) + "." + nameof(enumFunctionTs.Lot_Tras_Blanquear_Unitario) + "()",
            AutoSpan = true,
            Fila = 0,
            Columna = 1,
            Ordenar = true
            )
        ]
        public string Unitario { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Coste",
           Ayuda = "Precio de coste",
           TipoDeControl = enumTipoControl.Editor,
           Tipo = typeof(decimal),
           Fila = 1,
           Columna = 0
           )
        ]
        public decimal Coste { get; set; }
        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Venta",
           Ayuda = "Precio de venta",
           TipoDeControl = enumTipoControl.Editor,
           Tipo = typeof(decimal),
           Fila = 1,
           Columna =1
           )
        ]
        public decimal Venta { get; set; }
    }
}
