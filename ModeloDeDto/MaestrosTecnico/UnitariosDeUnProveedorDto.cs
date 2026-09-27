using Utilidades;
using ServicioDeDatos.MaestrosTecnico;

namespace ModeloDeDto.MaestrosTecnico
{
    /// <summary>
    /// Tarifa de un proveedor vista desde el proveedor: el restrictor es el proveedor y el seleccionable es el unitario
    /// </summary>
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class UnitariosDeUnProveedorDto : ElementoDto
    {
        //----------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Proveedor",
            Ayuda = "Proveedor de la tarifa",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Proveedor),
            Fila = 0,
            Columna = 0,
            EditableAlCrear = false,
            EditableAlEditar = false,
            VisibleEnGrid = false,
            AutoSpan = true
            )
        ]
        public int IdProveedor { get; set; }

        [IUPropiedad(Visible = false)]
        public string Proveedor { get; set; }

        //----------------------------------------------------------
        [IUPropiedad(Etiqueta = "Id del unitario", Visible = false)]
        public int IdElemento { get; set; }

        [IUPropiedad(
            Etiqueta = "Unitario",
            Ayuda = "Indique el unitario",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(UnitarioDto),
            GuardarEn = nameof(IdElemento),
            Controlador = nameof(enumControladoresMt.Unitarios),
            VistaDondeNavegar = enumVistasMts.CrudUnitarios,
            BuscarPor = nameof(UnitarioDtm.Nombre),
            CriterioDeBusqueda = enumCriteriosDeFiltrado.contiene,
            LongitudMinimaParaBuscar = 3,
            EditableAlCrear = true,
            EditableAlEditar = false,
            Fila = 1,
            Columna = 0,
            AutoSpan = true
            )
        ]
        public string Elemento { get; set; }

        //-----------------------------------------------------
        [IUPropiedad(
          Etiqueta = "Referencia",
          Ayuda = "Referencia del proveedor",
          Tipo = typeof(string),
          TipoDeControl = enumTipoControl.Editor,
          Fila = 2,
          Columna = 0,
          Obligatorio = true
          )
        ]
        public string Referencia { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Tarifa",
           Tipo = typeof(decimal),
           Ayuda = "Tarifa de compra",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           Fila = 2,
           Columna = 1)
        ]
        public decimal Tarifa { get; set; }
    }
}
