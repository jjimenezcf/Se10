using ModeloDeDto.Contabilidad;
using ModeloDeDto.Gastos;
using ModeloDeDto.MaestrosTecnico;
using ModeloDeDto.Negocio;
using ServicioDeDatos.MaestrosTecnico;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Terceros;
using System.Security;
using Utilidades;

namespace ModeloDeDto.Terceros
{
    public static class ltrProveedor
    {
        public static readonly string Proveedor = nameof(Proveedor);
        public static readonly string IdPersona = nameof(IdPersona);
        public static readonly string IdSociedad = nameof(IdSociedad);
        public static readonly string NIF = nameof(NIF);
        public static readonly string Mensaje_NoSePuedeCrearSiEsGestionada = "No puedo crear el proveedor '[NIF]' ya que es una sociedad gestionada";
    }

    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, MostrarExpresion = nameof(Expresion), OpcionDeBorrar = false)]
    public class ProveedorDto : ElmentoAuditadoDto, IUsaNombreDto, IUsaBajaDto
    {
        //----------------------------------------------
        [IUPropiedad(Etiqueta = "Id del interlocutor", Visible = false)]
        public int IdInterlocutor { get; set; }

        [IUPropiedad(
            Etiqueta = nameof(ltrProveedor.Proveedor),
            EditableAlEditar = false,
            Tipo = typeof(string),
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            PropiedadRestrictora = nameof(ProveedorDto.IdInterlocutor),
            MostrarExpresion = nameof(Expresion),
            Controlador = nameof(enumControladoresTerceros.Interlocutores),
            VistaDondeNavegar = enumVistasTerceros.CrudInterlocutores,
            Fila = 0,
            Columna = 0,
            Ordenar = true,
            PorAnchoMnt = 50,
            AutoSpan = true
          )
        ]
        public new string Expresion { get; set; }

        [IUPropiedad(Visible = false)]
        public string Nombre { get; set; }

        //----------------------------------------------
        [IUPropiedad(
           Etiqueta = "eMail",
           Ayuda = "eMail del proveedor",
           Fila = 1,
           Columna = 0,
           Ordenar = true,
           LongitudMaxima = 50
          )
        ]
        public string eMail { get; set; }

        //----------------------------------------------
        [IUPropiedad(
           Etiqueta = "Teléfono",
           Ayuda = "teléfono del proveedor",
           Fila = 1,
           Columna = 1,
           LongitudMaxima = 15
          )
        ]
        public string Telefono { get; set; }


        //----------------------------------------------
        [IUPropiedad(Etiqueta = "Id la cuenta contable", Visible = false)]
        public int IdCuenta { get; set; }

        [IUPropiedad(
            Etiqueta = "Cuenta",
            Ayuda = "Seleccione la cuenta contable",
            TipoDeControl = enumTipoControl.ListaDeElemento,
            SeleccionarDe = typeof(CuentaDto),
            Controlador = nameof(enumControladoresContables.Cuentas),
            GuardarEn = nameof(IdCuenta),
            VisibleEnGrid = false,
            Fila = 1,
            Columna = 2
            )
        ]
        public string Cuenta { get; set; }


        //----------------------------------------------
        [IUPropiedad(
           Etiqueta = "Ext.Cta",
           Ayuda = "extensión contable",
           Fila = 1,
           Columna = 2,
           Posicion = 1,
           Ordenar = true,
           VisibleAlCrear = false,
           EditableAlEditar = false,
           VisibleEnGrid = false,
           LongitudMaxima = 4
          )
        ]
        public string CodigoContable { get; set; }

        //----------------------------------------------
        [IUPropiedad(Etiqueta = "Id del Cg", Visible = false)]
        public int? IdCgPropuesto { get; set; }

        [IUPropiedad(
            Etiqueta = "CG propuesto",
            Ayuda = "Centro gestor",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdCgPropuesto),
            MostrarExpresion = nameof(CentroGestorDto.Expresion),
            Controlador = nameof(enumControladoresTerceros.CentrosGestores),
            VistaDondeNavegar = enumVistasTerceros.CrudCentrosGestores,
            SeleccionarDe = typeof(CentroGestorDto),
            Negocio = enumNegocio.CentroGestor,
            RestrictorFijo = nameof(NegociosDeUnCgDtm.Negocio) + ";" + nameof(enumNegocio.CentroGestor) + ";" + nameof(enumModoDeAccesoDeDatos.Gestor),
            SoloEnAlta = true,
            LongitudMinimaParaBuscar = 1,
            Tipo = typeof(string),
            Fila = 2,
            Columna = 0,
            AutoSpan = true,
            Posicion = 1,
            trasSeleccionar = "javascript:" + nameof(enumNameSpaceTs.Terceros) + "." + nameof(enumFunctionTs.Prv_Tras_Cambiar_Cg_Propuesto) + "()",
            trasBlanquear = "javascript:" + nameof(enumNameSpaceTs.Terceros) + "." + nameof(enumFunctionTs.Prv_Tras_Cambiar_Cg_Propuesto) + "()")]
        public string CgPropuesto { get; set; }

        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(Etiqueta = "Id del tipo de factura", Visible = false)]
        public int? IdTipoFarPropuesto { get; set; }

        [IUPropiedad(
            Etiqueta = "Tipo de factura",
            Ayuda = "Tipo de la factura proponer",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdTipoFarPropuesto),
            SeleccionarDe = typeof(TipoDeFacturaRecDto),
            RestrictorFijo = ltrParametrosDto.Negocio + ";" + nameof(enumNegocio.FacturaRecibida) + ";" + nameof(enumModoDeAccesoDeDatos.Gestor),
            Controlador = nameof(enumControladoresNegocio.TiposDeElemento),
            VistaDondeNavegar = enumVistasGastos.TiposDeFacturaRec,
            MostrarExpresion = nameof(TipoDeElementoDto.Nombre),
            SoloEnAlta = true,
            LongitudMinimaParaBuscar = 1,
            Negocio = enumNegocio.FacturaRecibida,
            Tipo = typeof(string),
            Fila = 2,
            Columna = 1,
            AutoSpan = true)]
        public string TipoFarPropuesto { get; set; }

        //----------------------------------------------
        [IUPropiedad(Etiqueta = "Id de la naturaleza contable por defecto", Visible = false)]
        public int? IdNaturaleza { get; set; }

        [IUPropiedad(
            Etiqueta = "Naturaleza",
            Ayuda = "Seleccione la naturaleza contable por defecto",
            TipoDeControl = enumTipoControl.ListaDeElemento,
            SeleccionarDe = typeof(NaturalezaDto),
            Controlador = nameof(enumControladoresMt.Naturalezas),
            GuardarEn = nameof(IdNaturaleza),
            Obligatorio = false,
            MostrarExpresion = nameof(NaturalezaDtm.Expresion),
            Fila = 2,
            Columna = 2
            )
        ]
        public string Naturaleza { get; set; }

        //----------------------------------------------
        [IUPropiedad(Etiqueta = "Id la unidad de medida por defecto", Visible = false)]
        public int? IdUnidad { get; set; }

        [IUPropiedad(
            Etiqueta = "Unidad",
            Ayuda = "Seleccione la unidad de medida por defecto",
            TipoDeControl = enumTipoControl.ListaDeElemento,
            SeleccionarDe = typeof(UnidadDto),
            Controlador = nameof(enumControladoresMt.Unidades),
            GuardarEn = nameof(IdUnidad),
            Obligatorio = false,
            Fila = 2,
            Columna = 3
            )
        ]
        public string Unidad { get; set; }

        //----------------------------------------------
        [IUPropiedad(
           Etiqueta = "Concepto",
           Ayuda = "Concepto facturado por defecto",
           Obligatorio = false,
           Fila = 3,
           Columna = 0,
           LongitudMaxima = 250
          )
        ]
        public string Concepto { get; set; }


        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Base Imponible propuesta",
           Tipo = typeof(decimal),
           Ayuda = "importe sin iva ni irpf a proponer tras seleccionar el proveedor al crear una factura",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           Obligatorio = false,
           VisibleEnGrid = false,
           EditableAlCrear = true,
           EsAlmacenable = true,
           VisibleAlEditar = true,
           Fila = 3,
           Columna = 1,
           Formato = enumFormato.Numero_6
            )
        ]
        public decimal BiPropuesto { get; set; }

        //-----------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Iva",
            Ayuda = "iva a aplicar a la BI por defecto",
            TipoDeControl = enumTipoControl.ListaDeElemento,
            SeleccionarDe = typeof(IvaSoportadoDto),
            Controlador = nameof(enumControladoresContables.IvasSoportado),
            GuardarEn = nameof(IdIvaS),
            Obligatorio = false,
            VisibleEnGrid = false,
            EditableAlCrear = true,
            EsAlmacenable = true,
            VisibleAlEditar = true,
            Fila = 3,
            Columna = 2
            )
        ]
        public string IvaSoportado { get; set; }

        [IUPropiedad(Etiqueta = "Id del iva soportado", Visible = false)]
        public int? IdIvaS { get; set; }

        //-----------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Irpf",
            Ayuda = "irpf a aplicar a la BI por defecto",
            TipoDeControl = enumTipoControl.ListaDeElemento,
            SeleccionarDe = typeof(IrpfDto),
            Controlador = nameof(enumControladoresContables.Irpfs),
            GuardarEn = nameof(IdIrpf),
            Obligatorio = false,
            VisibleEnGrid = false,
            EditableAlCrear = true,
            EsAlmacenable = true,
            VisibleAlEditar = true,
            Fila = 3,
            Columna = 3
            )
        ]
        public string Irpf { get; set; }

        [IUPropiedad(Etiqueta = "Id del irpf", Visible = false)]
        public int? IdIrpf { get; set; }

        //-------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Modo de pago",
            Ayuda = "indique la clase de pago a proponer al introducir una factura",
            TipoDeControl = enumTipoControl.Enumerado,
            EsAlmacenable = true,
            Tipo = typeof(enumModoDePagoContado),
            GuardarEn = nameof(ModoDePago),
            OnChange = "javascript:" + nameof(enumNameSpaceTs.Terceros) + "." + nameof(enumFunctionTs.Prv_Tras_Cambiar_Modo_De_Pago) + "()",
            OnReset = "javascript:" + nameof(enumNameSpaceTs.Terceros) + "." + nameof(enumFunctionTs.Prv_Tras_Cambiar_Modo_De_Pago) + "()",
            Fila = 4,
            Columna = 0,
            Posicion = 1,
            VisibleEnGrid = false,
            VisibleEnEdicion = true,
            Obligatorio = false
          )
        ]
        public enumModoDePagoContado? ModoDePago { get; set; }


        //----------------------------------------------
        [IUPropiedad(Etiqueta = "Id la cuenta de cargo", Visible = false)]
        public int IdTarjeta { get; set; }

        [IUPropiedad(
            Etiqueta = "Tarjeta bancaria",
            Ayuda = "Seleccione la tarjeta a proponer al introducir una factura",
            TipoDeControl = enumTipoControl.ListaDeElemento,
            SeleccionarDe = typeof(TarjetaDeMiSociedadDto),
            Controlador = nameof(enumControladoresTerceros.TarjetasDeMiSociedad),
            MostrarExpresion = nameof(TarjetaDeMiSociedadDto.Expresion),
            GuardarEn = nameof(IdTarjeta),
            RestringidoPorControl = nameof(CgPropuesto),
            VisibleEnGrid = false,
            VisibleEnEdicion = true,
            CargarBajoDemanda = true,
            Fila = 4,
            Columna = 1,
            AutoSpan = true,
            Obligatorio = false,
            EsAlmacenable = true
            )
        ]
        public string Tarjeta { get; set; }

        //----------------------------------------------
        [IUPropiedad(Etiqueta = "Id la cuenta de cargo", Visible = false)]
        public int IdDomiciliadaEn { get; set; }

        [IUPropiedad(
            Etiqueta = "Domiciliada en ",
            Ayuda = "Seleccione la cuenta bancaria de domiciliación",
            TipoDeControl = enumTipoControl.ListaDeElemento,
            SeleccionarDe = typeof(CuentaDeMiSociedadDto),
            Controlador = nameof(enumControladoresTerceros.CuentasDeMiSociedad),
            MostrarExpresion = nameof(CuentaDeMiSociedadDto.Expresion),
            GuardarEn = nameof(IdDomiciliadaEn),
            RestringidoPorControl = nameof(CgPropuesto),
            VisibleEnGrid = false,
            VisibleEnEdicion = true,
            CargarBajoDemanda = true,
            Fila = 4,
            Columna = 2,
            Obligatorio = false,
            EsAlmacenable = true
            )
        ]
        public string DomiciliadaEn { get; set; }

        //----------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Baja",
            Ayuda = "indica si el proveedor está de baja",
            Fila = 5,
            Columna = 0,
            TipoDeControl = enumTipoControl.Check,
            css = enumCssControles.ControlApilado,
            ValorPorDefecto = false,
            EditableAlEditar = true,
            VisibleEnGrid = false
            )
        ]
        public bool Baja { get; set; }

        //----------------------------------------------------------------
        [IUPropiedad(Visible = false)]
        public string NIF { get; set; }

        //----------------------------------------------------------------
        [IUPropiedad(Visible = false)]
        public string DireccionFiscal { get; set; }
        //----------------------------------------------------------------
        [IUPropiedad(Visible = false)]
        public string RazonSocial { get; set; }
        //----------------------------------------------------------------
        [IUPropiedad(Visible = false)]
        public bool EsIntraComunitario { get; set; }
        //----------------------------------------------------------------
        [IUPropiedad(Visible = false)]
        public bool EsExtraComunitario { get; set; }
    }
}
