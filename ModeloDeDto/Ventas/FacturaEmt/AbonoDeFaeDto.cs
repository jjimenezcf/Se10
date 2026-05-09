using Utilidades;
using ModeloDeDto.Terceros;
using System;

namespace ModeloDeDto.Ventas
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, OpcionDeBorrar = false)]
    public class AbonoDeFaeDto : EsUnDetalleDto
    {
        //----------------------------------------------------------
        [IUPropiedad(Etiqueta = "Cliente",
            Ayuda = "Cliente de la factura",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Cliente),
            Fila = 0,
            Columna = 1,
            VisibleEnGrid = false,
            EditableAlCrear = false,
            EditableAlEditar = false,
            Obligatorio = true,
            AutoSpan = true,
            Controlador = nameof(enumControladoresTerceros.Clientes),
            VistaDondeNavegar = enumVistasTerceros.CrudClientes
            )
        ]
        public int IdCliente { get; set; }

        [IUPropiedad(Visible = false, PorAnchoMnt = 20, Etiqueta = "Cliente al que abonar", Obligatorio = false)]
        public string Cliente { get; set; }

        //-------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Clase de abono",
            Ayuda = "indique la clase de abono",
            TipoDeControl = enumTipoControl.Enumerado,
            Tipo = typeof(enumClaseDePago),
            GuardarEn = nameof(Clase),
            OnChange = "javascript:" + nameof(enumNameSpaceTs.Venta) + "." + nameof(enumFunctionTs.Fae_Tras_Cambiar_Clase_De_Abono) + "()",
            AutoSpan = true,
            Fila = 0,
            Columna = 2,
            EditableAlEditar = false
          )
        ]
        public enumClaseDePago Clase { get; set; }


        //-------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Id de la cuenta bancaria",
            Visible = false
            )
        ]
        public int? IdCuentaDeCargo { get; set; }

        [IUPropiedad(
            Etiqueta = "Cuenta de cargo",
            Ayuda = "seleccione la cuenta de cargo",
            TipoDeControl = enumTipoControl.ListaDeElemento,
            AutoPosicionamiento = true,
            SeleccionarDe = typeof(CuentaDeMiSociedadDto),
            Controlador = nameof(enumControladoresTerceros.CuentasDeMiSociedad),
            GuardarEn = nameof(IdCuentaDeCargo),
            Obligatorio = false,
            RestrictorFijo = nameof(ltrParametrosEp.negocio) + ";" + nameof(enumNegocio.FacturaEmitida),
            RestringidoPorControl = nameof(IdElemento),
            MostrarExpresion = "([" + nameof(CuentaDeMiSociedadDto.Alias) + "]) " + "[" + nameof(CuentaDeMiSociedadDto.Cuenta) + "]",
            EditableAlCrear = true,
            EditableAlEditar = false,
            AutoSpan = true,
            Fila = 1,
            Columna = 0,
            Posicion = 0
            )
        ]
        public string CuentaDeCargo { get; set; }

        //-------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Id de la cuenta bancaria de abono",
            Visible = false
            )
        ]
        public int? IdCuentaDeAbono { get; set; }

        [IUPropiedad(
            Etiqueta = "Cuenta de abono",
            Ayuda = "seleccione la cuenta en la que se ingresa el abono",
            TipoDeControl = enumTipoControl.ListaDeElemento,
            AutoPosicionamiento = true,
            SeleccionarDe = typeof(CuentaDeClienteDto),
            Controlador = nameof(enumControladoresTerceros.CuentasDeCliente),
            GuardarEn = nameof(IdCuentaDeAbono),
            Obligatorio = false,
            RestrictorFijo = nameof(ltrParametrosEp.negocio) + ";" + nameof(enumNegocio.Cliente),
            RestringidoPorControl = nameof(IdCliente),
            MostrarExpresion = "([" + nameof(CuentaDeClienteDto.Alias) + "]) " + "[" + nameof(CuentaDeClienteDto.Cuenta) + "]",
            EditableAlCrear = true,
            EditableAlEditar = false,
            AutoSpan = true,
            Fila =1,
            Columna = 1,
            Posicion =0
            )
        ]
        public string CuentaDeAbono { get; set; }

        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Abonado el",
            EtiquetaGrid = "Fecha de abono",
            PorAnchoSel = 20,
            Ayuda = "Fecha del abono",
            TipoDeControl = enumTipoControl.SelectorDeFechaHora,
            Fila = 2,
            Columna = 0,
            Obligatorio = true
           )
        ]
        public DateTime? AbonadoEl { get; set; }

        //--------------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Pendiente",
           Tipo = typeof(decimal),
           Ayuda = "importe pendiente",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           Obligatorio = false,
           MantenerHuecoDeLaIzquierda = true,
           Fila = 2,
           Columna = 1,
           Formato = enumFormato.Moneda,
           EditableAlCrear = false,
           EditableAlEditar = false)
        ]
        public decimal Pendiente { get; set; }

        //--------------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Importe",
           Tipo = typeof(decimal),
           Ayuda = "importe abonado",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           Obligatorio = true,
           Formato = enumFormato.Moneda,
           Fila =2,
           Columna = 2)
        ]
        public decimal Importe { get; set; }

        //--------------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Estado",
           Tipo = typeof(string),
           Ayuda = "estado del pago",
           TipoDeControl = enumTipoControl.Editor,
           VisibleEnEdicion = false)
        ]
        public string Estado { get; set; }

        //------------------------------------------------------------------------
        [IUPropiedad(
            VisibleEnGrid = false,
            VisibleAlCrear =true,
            VisibleAlEditar = false,
            VisibleAlConsultar =false,
            Etiqueta = "Justificante",
            Ayuda = "Seleccione o pegue el justificante",
            Tipo = typeof(int),
            TipoDeControl = enumTipoControl.SelectorDeUnArchivo,
            ExtensionesValidas = ExtensorDeTipoDeArchivos.NoEditables,
            Fila = 3,
            Columna = 0,
            AutoSpan = true)]
        public int? IdJustificante { get; set; }

        //----------------------------------------------------------
        [IUPropiedad(Etiqueta = "Abono",
            Ayuda = "Abono realizado",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Abono),
            VisibleEnGrid = false,
            VisibleAlCrear = false,
            VisibleAlEditar = true,
            VisibleAlConsultar = true,
            EditableAlCrear = false,
            EditableAlEditar = false,
            Obligatorio = true,
            AutoSpan = true,
            Controlador = nameof(enumControladoresGastos.Pagos),
            VistaDondeNavegar = enumVistasGastos.CrudPagos,
            Fila = 3,
            Columna = 0
            )
        ]
        public int IdAbono { get; set; }

        [IUPropiedad(Visible = false, PorAnchoMnt = 20, Etiqueta = "Pago del abono", Obligatorio = false)]
        public string Abono { get; set; }


        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Anotación",
           Ayuda = "Anotación sobre el abono ",
           TipoDeControl = enumTipoControl.AreaDeTexto,
           Fila = 4,
           Columna = 0,
           VisibleEnGrid = false,
           VisibleAlCrear = true,
           VisibleAlEditar = false,
           VisibleAlConsultar = false,
           EditableAlCrear = true,
           EditableAlEditar = false,
           Obligatorio = false,
           NumeroDeFilas = 3,
           AutoSpan = true
           )
        ]
        public string Anotacion { get; set; }


        //----------------------------------------------------------
        [IUPropiedad(Visible = false)]
        public string Referencia { get; set; }
    }
}
