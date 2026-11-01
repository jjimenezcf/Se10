using System;
using Utilidades;
using ModeloDeDto.Terceros;
using ServicioDeDatos;
using ServicioDeDatos.Logistica;
using ServicioDeDatos.Seguridad;

namespace ModeloDeDto.Logistica
{

    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, MostrarExpresion = nameof(IUsaNombreDto.Nombre), EditarTrasCrear = true)]
    public class VentaDto : ElementoDeUnProcesoDto, IDatosDeContactoDto
    {

        //--------------------------------------------
        [IUPropiedad(Etiqueta = "", Oculto = true, Obligatorio = false, VisibleEnGrid = false, Fila = 0, Columna = 0, Posicion = 0)]
        public new int IdCg { get; set; }

        [IUPropiedad(Etiqueta = "", Oculto = true, Obligatorio = false, VisibleEnGrid = false, Fila = 0, Columna = 0, Posicion = 0)]
        public new string Cg { get; set; }

        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Id del almacén",
            Visible = false
            )
        ]
        public int IdAlmacen { get; set; }

        [IUPropiedad(
            Etiqueta = "Almacén",
            Ayuda = "Almacén desde el que se sirve la venta",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(AlmacenDto),
            GuardarEn = nameof(IdAlmacen),
            MostrarExpresion = nameof(AlmacenDtm.Expresion),
            Controlador = nameof(enumControladoresLogistica.Almacenes),
            VistaDondeNavegar = enumVistasLogisticas.CrudAlmacenes,
            RestrictorFijo = nameof(ltrParametrosEp.negocio) + ";" + nameof(enumNegocio.Almacen) + ";" + nameof(enumModoDeAccesoDeDatos.Gestor),
            Negocio = enumNegocio.Almacen,
            LongitudMinimaParaBuscar = 1,
            Fila = 0,
            Columna = 0,
            Obligatorio = true,
            EditableAlCrear = true,
            EditableAlEditar = false,
            VisibleEnGrid = true,
            AutoSpan = true,
            trasSeleccionar = "javascript:" + nameof(enumNameSpaceTs.Logistica) + "." + nameof(enumFunctionTs.Dsa_Tras_Seleccionar_Almacen) + "()",
            trasBlanquear = "javascript:" + nameof(enumNameSpaceTs.Logistica) + "." + nameof(enumFunctionTs.Dsa_Tras_Blanquear_Almacen) + "()"
            )
        ]
        public string Almacen { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
         Etiqueta = "Id del cliente de la venta",
         Visible = false
         )]
        public int IdCliente { get; set; }

        [IUPropiedad(
            Etiqueta = "Cliente",
            Ayuda = "A quién se le vende",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdCliente),
            Controlador = nameof(enumControladoresTerceros.Clientes),
            VistaDondeNavegar = enumVistasTerceros.CrudClientes,
            SeleccionarDe = typeof(ClienteDto),
            RestrictorFijo = ltrParametrosDto.Negocio + ";" + nameof(enumNegocio.Cliente) + ";" + nameof(enumModoDeAccesoDeDatos.Consultor),
            Negocio = enumNegocio.Cliente,
            LongitudMinimaParaBuscar = 1,
            Tipo = typeof(string),
            Fila = 2,
            Columna = 0,
            Posicion = 0,
            Obligatorio = true,
            EditableAlCrear = true,
            EditableAlEditar = false,
            AutoSpan = true)]
        public string Cliente { get; set; }

        //-------------------------------------------------------------------------------------------------------------------
        [IUPropiedad(TipoDeControl = enumTipoControl.Editor,
            Fila = 3, Columna = 0
            , Etiqueta = "Contacto"
            , VisibleAlCrear = false
            , VisibleAlEditar = true
            , VisibleEnGrid = false)]
        public string Contacto { get; set; }

        //-------------------------------------------------------------------------------------------------------------------
        [IUPropiedad(TipoDeControl = enumTipoControl.Editor,
            Fila = 3, Columna = 1
            , Etiqueta = "Teléfono"
            , VisibleAlCrear = false
            , VisibleAlEditar = true
            , VisibleEnGrid = false)]
        public string Telefono { get; set; }

        //-------------------------------------------------------------------------------------------------------------------
        [IUPropiedad(TipoDeControl = enumTipoControl.Editor,
            Fila = 3, Columna = 2
            , Etiqueta = "eMail"
            , VisibleAlCrear = false
            , VisibleAlEditar = true
            , VisibleEnGrid = false)]
        public string eMail { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
              PorAnchoMnt = 15
            , Etiqueta = "Solicitado el"
            , Ayuda = "Fecha en la que el cliente solicita la venta"
            , Ordenar = true
            , OrdenarGridPor = nameof(VentaDto.SolicitadoEl)
            , TipoDeControl = enumTipoControl.SelectorDeFecha
            , VisibleEnGrid = true
            , EditableAlCrear = true
            , EditableAlEditar = true
            , Formato = enumFormato.Fecha
            , Fila = 2
            , Columna = 0
            , Posicion = 1
            , AnchoMaximo = "11em")]
        public DateTime? SolicitadoEl { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
              PorAnchoMnt = 15
            , Etiqueta = "Servido el"
            , Ayuda = "Fecha en la que se sirvió la venta"
            , Ordenar = true
            , OrdenarGridPor = nameof(VentaDto.ServidoEl)
            , TipoDeControl = enumTipoControl.SelectorDeFecha
            , VisibleEnGrid = true
            , VisibleAlCrear = false
            , EditableAlEditar = false
            , Formato = enumFormato.Fecha
            , Fila = 2
            , Columna = 0
            , Posicion = 2
            , AnchoMaximo = "11em")]
        public DateTime? ServidoEl { get; set; }

        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Factura",
            Ayuda = "factura emitida de la venta",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            Controlador = nameof(enumControladoresVentas.FacturasEmt),
            VistaDondeNavegar = enumVistasVentas.CrudFacturasEmt,
            MostrarExpresion = nameof(Factura),
            Fila = 12,
            Columna = 0,
            VisibleAlCrear = false,
            EditableAlEditar = false,
            VisibleEnGrid = false
            )
        ]
        public int? IdFactura { get; set; }

        [IUPropiedad(Visible = false)]
        public string Factura { get; set; }

    }


}
