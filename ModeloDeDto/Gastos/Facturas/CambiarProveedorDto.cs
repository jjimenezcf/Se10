using ModeloDeDto.Terceros;
using ServicioDeDatos.Seguridad;
using Utilidades;

namespace ModeloDeDto.Gastos
{
    [IUDto(MostrarExpresion = nameof(IUsaNombreDto.Nombre))]
    public class CambiarProveedorDto : IRenombrarDto
    {
        [IUPropiedad(
            Etiqueta = "Factura",
            Ayuda = "Cambiar el proveedor de",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Elemento),
            Fila = 0,
            Columna = 0,
            VisibleEnGrid = false,
            Controlador = nameof(enumControladoresGastos.FacturasRec),
            VistaDondeNavegar = enumVistasGastos.CrudFacturasRec
            )
        ]
        public int IdElemento { get; set; }

        [IUPropiedad(Etiqueta = "Factura", Visible = false)]
        public string Elemento { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
         Etiqueta = "Id del proveedor de la factura",
         Visible = false
         )]
        public int IdProveedor { get; set; }

        [IUPropiedad(
            Etiqueta = "Proveedor",
            Ayuda = "Quién me factura",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdProveedor),
            Controlador = nameof(enumControladoresTerceros.Proveedores),
            SeleccionarDe = typeof(ProveedorDto),
            VistaDondeNavegar = enumVistasTerceros.CrudProveedores,
            RestrictorFijo = ltrParametrosDto.Negocio + ";" + nameof(enumNegocio.Proveedor) + ";" + nameof(enumModoDeAccesoDeDatos.Consultor),
            Negocio = enumNegocio.Proveedor,
            LongitudMinimaParaBuscar = 1,
            Tipo = typeof(string),
            Fila = 2,
            Columna = 0
            )]
        public string Proveedor { get; set; }
    }
}
