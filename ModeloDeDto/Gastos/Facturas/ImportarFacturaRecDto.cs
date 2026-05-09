using ModeloDeDto.Terceros;
using ServicioDeDatos.Seguridad;
using Utilidades;

namespace ModeloDeDto.Gastos
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, MostrarExpresion = nameof(IUsaNombreDto.Nombre), EditarTrasCrear = true)]
    public class ImportarFacturaRecDto : ElementoDeUnProcesoDto
    {
        [IUPropiedad(
          Etiqueta = "Factura proveedor",
          Tipo = typeof(string),
          Ayuda = "nº de factura de proveedor",
          VisibleEnGrid = false,
          PosicionEnGrid = 5,
          TipoDeControl = enumTipoControl.Editor,
          Alineada = enumAliniacion.derecha,
          EditableAlEditar = true,
          EditableAlCrear = true,
          Obligatorio = true,
          Fila = 2,
          Columna = 1,
          Posicion = 0
           )
       ]
        public string Numero { get; set; }

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
            Columna = 0,
            EditableAlCrear = true,
            EditableAlEditar = false)]
        public string Proveedor { get; set; }

        [IUPropiedad(Visible = false)]
        public string Interlocutor { get; set; }

        [IUPropiedad(Visible = false)]
        public int IdInterlocutor { get; set; }


        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Base Imponible",
           Tipo = typeof(decimal),
           EtiquetaGrid = "B.I",
           Ayuda = "importe sin iva ni irpf",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = true,
           EditableAlEditar = true,
           Fila = 12,
           Columna = 2,
           MantenerHuecoDeLaIzquierda = true,
           Formato = enumFormato.Numero_6
            )
        ]
        public decimal BaseImponible { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Total a pagar",
           EtiquetaGrid = "Total",
           Tipo = typeof(decimal),
           Ayuda = "importe de la factura con iva y sin Irpf",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = true,
           EditableAlEditar = true,
           Fila = 12,
           Columna = 3,
           Formato = enumFormato.Numero_6
            )
        ]
        public decimal TotalDelPago { get; set; }



    }


}
