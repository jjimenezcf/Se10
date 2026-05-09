using ServicioDeDatos.Ventas;
using Utilidades;

namespace ModeloDeDto.Ventas
{
    [IUDto(MostrarExpresion = nameof(IUsaNombreDto.Nombre))]
    public class SelectorFaeParaRectificarDto
    {
        //----------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Rectificativa",
            Ayuda = "factura rectificativa",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(FacturaEmtDto),
            Controlador = nameof(enumControladoresVentas.FacturasEmt),
            VistaDondeNavegar = enumVistasVentas.CrudFacturasEmt,
            Fila = 0,
            Columna = 1,
            EditableAlCrear = false,
            EditableAlEditar = false,
            VisibleEnGrid = false
            )
        ]
        public int? IdRectificativa { get; set; }
        [IUPropiedad(Visible = false)]
        public string Rectificativa { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
         Etiqueta = "Id de la factura a rectificar",
         Visible = false
         )]
        public int idFacturaEmt { get; set; }

        [IUPropiedad(
            Etiqueta = "A rectificar",
            Ayuda = "factura a rectificar",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(idFacturaEmt),
            BuscarPor = ltrDeFacturaRectificada.SeleccionarEmtParaRectificar,
            Controlador = nameof(enumControladoresVentas.FacturasEmt),
            SeleccionarDe = typeof(FacturaEmtDto),
            OrdenarListaDinamicaPor = nameof(FacturaEmtDtm.NumeroDeFactura),
            VistaDondeNavegar = enumVistasVentas.CrudFacturasEmt,
            LongitudMinimaParaBuscar = 1,
            MostrarExpresion = nameof(ParteTrDto.Expresion),
            Tipo = typeof(string),
            Fila = 2,
            Columna = 0,
            EditableAlCrear = true,
            EditableAlEditar = false,
            AutoSpan = true)]
        public string FacturaEmt { get; set; }
    }

}
