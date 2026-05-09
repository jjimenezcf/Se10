using ModeloDeDto.Terceros;
using ServicioDeDatos;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Ventas;
using System;
using Utilidades;

namespace ModeloDeDto.Ventas
{
    public class IndFacturaEmt
    {
        public const string UsaVerifactu = nameof(UsaVerifactu);
    }

    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, MostrarExpresion = nameof(IUsaNombreDto.Nombre), EditarTrasCrear = true)]
    public class FacturaEmtDto : ElementoDeUnProcesoDto
    {
        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
             Etiqueta = "Año",
             Tipo = typeof(int),
             Ayuda = "año",
             VisibleEnGrid = false,
             TipoDeControl = enumTipoControl.Editor,
             Alineada = enumAliniacion.derecha,
             EditableAlEditar = false,
              EditableAlCrear = false,
             Obligatorio = false,
             Fila = 0,
             Columna = 4,
             Posicion = 0
              )
          ]
        public int? Ano { get; set; }

        [IUPropiedad(
             Etiqueta = "Serie",
             Ayuda = "serie",
             VisibleEnGrid = false,
             TipoDeControl = enumTipoControl.Editor,
             Alineada = enumAliniacion.derecha,
             EditableAlEditar = false,
              EditableAlCrear = false,
             Obligatorio = false,
             Fila = 0,
             Columna = 4,
             Posicion = 1
              )
          ]
        public string Serie { get; set; }

        [IUPropiedad(
          Etiqueta = "Número",
          Tipo = typeof(int),
          Ayuda = "nº de factura",
          VisibleEnGrid = false,
          TipoDeControl = enumTipoControl.Editor,
          Alineada = enumAliniacion.derecha,
          EditableAlEditar = false,
           EditableAlCrear = false,
          Obligatorio = false,
          Fila = 0,
          Columna = 4,
          Posicion = 2
           )
       ]
        public int? Numero { get; set; }


        [IUPropiedad(
             Etiqueta = "Nº de factura",
             Ayuda = "número de factura",
             VisibleEnGrid = true,
             TipoDeControl = enumTipoControl.Editor,
             Alineada = enumAliniacion.derecha,
             VisibleEnEdicion = false,
             PosicionEnGrid = 3,
             Obligatorio = false,
             OrdenarGridPor = nameof(NumeroFactura)
              )
          ]
        public string NumeroFactura => Numero == default ? "no emitida" : Ano + "-" + Serie + "-" + Numero;

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
              PorAnchoMnt = 15
            , Etiqueta = "Facturar el"
            , Ordenar = true
            , OrdenarGridPor = nameof(FacturadaEl)
            , TipoDeControl = enumTipoControl.SelectorDeFecha
            , VisibleEnGrid = false
            , VisibleAlCrear = false
            , EditableAlEditar = true
            , Fila = 1
            , Columna = 4
            , Alineada = enumAliniacion.centrada)]
        public DateTime? FacturadaEl { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
              PorAnchoMnt = 15
            , Etiqueta = "Emitida el"
            , Ordenar = true
            , OrdenarGridPor = nameof(EmitidaEl)
            , TipoDeControl = enumTipoControl.SelectorDeFechaHora
            , VisibleEnGrid = false
            , VisibleAlCrear = false
            , EditableAlEditar = false
            , Fila = 1
            , Columna = 4
            , Posicion = 1
            , Alineada = enumAliniacion.centrada)]
        public DateTime? EmitidaEl { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(Etiqueta = "Id del cliente de la factura", Oculto = true, Fila = 2, Columna = 0, SoloParaTs = true)]
        public int IdCliente { get; set; }

        [IUPropiedad(
            Etiqueta = "Cliente",
            Ayuda = "A quién se le factura",
            TipoDeControl = enumTipoControl.ListaDinamica,
            TipoDeControlEnGrid = enumTipoControl.Referencia,
            AccionRef = "javascript: " + nameof(enumNameSpaceTs.ApiDelCrud) + "." + nameof(enumFunctionTs.Negocio_IrAlCliente) + "(numeroDeFila)",
            GuardarEn = nameof(IdCliente),
            Controlador = nameof(enumControladoresTerceros.Clientes),
            SeleccionarDe = typeof(ClienteDto),
            VistaDondeNavegar = enumVistasTerceros.CrudClientes,
            RestrictorFijo = ltrParametrosDto.Negocio + ";" + nameof(enumNegocio.Cliente) + ";" + nameof(enumModoDeAccesoDeDatos.Consultor),
            Negocio = enumNegocio.Cliente,
            LongitudMinimaParaBuscar = 1,
            Tipo = typeof(string),
            Fila = 2,
            Columna = 0,
            EditableAlCrear = true,
            EditableAlEditar = false)]
        public string Cliente { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
              Etiqueta = "Emitir como",
              TipoDeControl = enumTipoControl.Enumerado,
              Tipo = typeof(enumClaseDeEmision)
            , Ayuda = "Seleccione si es una eFactura o impresa"
            , Visible = false
            , VisibleEnGrid = false
            , VisibleAlCrear = false
            , VisibleAlEditar = true
            , EditableAlEditar = true
            , Fila = 2
            , Columna = 3)]
        public enumClaseDeEmision ClaseDeEmision { get; set; }


        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
              PorAnchoMnt = 15
            , Etiqueta = "Vence el"
            , Ordenar = true
            , OrdenarGridPor = nameof(FacturaEmtDto.VenceEl)
            , TipoDeControl = enumTipoControl.SelectorDeFecha
            , VisibleEnGrid = false
            , EditableAlCrear = false
            , EditableAlEditar = true
            , Fila = 2
            , Columna = 3)]
        public DateTime? VenceEl { get; set; }


        //-------------------------------------------------------------------------------------------------------------------
        [IUPropiedad(TipoDeControl = enumTipoControl.Editor,
            Fila = 3, Columna = 0
            , Etiqueta = "Facturar A"
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

        //-------------------------------------------------------------------------------------------------------------------
        [IUPropiedad(Etiqueta = "Id del centro administrativo", Visible = false)]
        public int? IdCentroAdministrativo { get; set; }

        [IUPropiedad(Etiqueta = "usa centro administrativo", Visible = false)]
        public bool UsaCentroAdministrativo { get; set; }

        [IUPropiedad(
            Etiqueta = "Centro administrativo",
            Ayuda = "seleccione el centro administrativo del cliente",
            TipoDeControl = enumTipoControl.ListaDeElemento,
            SeleccionarDe = typeof(CentroAdministrativoDtm),
            Controlador = nameof(enumControladoresTerceros.CentrosAdministrativos),
            RestrictorFijo = ltrInterlocutor.BuscarPorContactoCliente + Simbolos.PuntoComa + nameof(enumNegocio.Cliente) + Simbolos.Pipe +
                             ltrFiltros.enumNegocio + Simbolos.PuntoComa + nameof(enumNegocio.Cliente),
            RestringidoPorControl = nameof(IdCliente),
            GuardarEn = nameof(IdCentroAdministrativo),
            MostrarExpresion = nameof(CentroAdministrativoDto.Expresion),
            VisibleAlCrear = false,
            VisibleAlEditar = true,
            AutoSpan = true,
            Obligatorio = false,
            Fila = 3,
            Columna = 3,
            Posicion = 0,
            //CargarBajoDemanda = true,
            VisibleEnGrid = false
          )
        ]
        public string CentroAdministrativo { get; set; }

        //-------------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            Fila = 3, 
            Columna = 3
            , Posicion = 1
            , Etiqueta = "Rectificada por",
            Ayuda = "factura que la rectifica",
            MostrarExpresion = nameof(Rectificativa),
            Controlador = nameof(enumControladoresVentas.FacturasEmt),
            VistaDondeNavegar = enumVistasVentas.CrudFacturasEmt
            , EditableAlEditar = false
            , VisibleAlCrear = false
            , VisibleAlEditar = true
            , VisibleEnGrid = false)
            ]
        public int? IdRectificativa { get; set; }

        [IUPropiedad(Visible = false)]
        public string Rectificativa { get; set; }

        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(
           TamanoFijo = "10em"
         , EtiquetaGrid = "Rectificada Por"
         , VisibleEnEdicion = false
         , TipoDeControl = enumTipoControl.Referencia
         , AccionRef = "javascript: " + nameof(enumNameSpaceTs.Venta) + "." + nameof(enumFunctionTs.Fae_IrARectificadaPor) + "(numeroDeFila)"
         , Alineada = enumAliniacion.derecha)]
        public string IrARectificadaPor{ get; set; }


        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(
           TamanoFijo = "10em"
         , EtiquetaGrid = "Rectifico A"
         , VisibleEnEdicion = false
         , TipoDeControl = enumTipoControl.Referencia
         , AccionRef = "javascript: " + nameof(enumNameSpaceTs.Venta) + "." + nameof(enumFunctionTs.Fae_IrARectificoA) + "(numeroDeFila)"
         , Alineada = enumAliniacion.derecha)]
        public string IrARectificoA { get; set; }


        //-------------------------------------------------------------------------------------------------------------------
        [IUPropiedad(TipoDeControl = enumTipoControl.Editor,
            Fila = 3, Columna = 4
            , Etiqueta = "Clase de Rec."
            , MantenerHuecoDeLaIzquierda = true
            , Obligatorio = false
            , EditableAlEditar = false
            , VisibleAlCrear = false
            , VisibleAlEditar = true
            , VisibleEnGrid = false)]
        public string ClaseRectificativa { get; set; }
        
        [IUPropiedad(Visible = false)]
        public enumClaseDeRectificativa? enumClaseRectificativa { get; set; }


        //-------------------------------------------------------------------------------------------------------------------
        [IUPropiedad(TipoDeControl = enumTipoControl.Editor,
            Fila = 3, Columna = 4
            , Etiqueta = "Motivo"
            , MantenerHuecoDeLaIzquierda = true
            , Obligatorio = false
            , EditableAlEditar = false
            , VisibleAlCrear = false
            , VisibleAlEditar = true
            , VisibleEnGrid = false)]
        public string MotivoDeRectificacion { get; set; }


        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "B.I.",
           Tipo = typeof(decimal),
           EtiquetaGrid = "B.I",
           Ayuda = "base imponible",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Obligatorio = false,
           Fila = 12,
           Columna = 3,
           Posicion =0,
           Formato = enumFormato.Moneda
            )
        ]
        public decimal? TotalSinIva { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Total a pagar",
            EtiquetaGrid = "Total a pagar",
           Tipo = typeof(decimal),
           Ayuda = "importe de la factura con iva y sin irpf",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Obligatorio = false,
           Fila = 12,
           Columna = 3,
           Posicion = 1,
           Formato = enumFormato.Moneda
            )
        ]
        public decimal? APagar { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Iva de la factura",
           EtiquetaGrid = "Iva",
           Tipo = typeof(decimal),
           Ayuda = "importe del iva de la factura",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           VisibleEnEdicion = false,
           VisibleEnGrid = false,
           Obligatorio = false,
           Formato = enumFormato.Moneda
            )
        ]
        public decimal? TotalIva { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Irpf de la factura",
           EtiquetaGrid = "Irpf",
           Tipo = typeof(decimal),
           Ayuda = "importe del irpf de la factura",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           VisibleEnEdicion = false,
           VisibleEnGrid = false,
           Obligatorio = false,
           Formato = enumFormato.Moneda
            )
        ]
        public decimal? TotalIrpf { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Cobrado",
           Tipo = typeof(decimal),
           Ayuda = "importe cobrado",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Obligatorio = false,
           VisibleEnGrid = false, 
           PorAnchoMnt = 15,
           Fila = 12,
           Columna = 4,
           Posicion = 0,
           Formato = enumFormato.Moneda
            )
        ]
        public decimal? Cobrado { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Pendiente",
           Tipo = typeof(decimal),
           Ayuda = "importe pendiente de cobro",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Obligatorio = false,
           VisibleEnGrid = false,
           PorAnchoMnt = 15,
           Fila = 12,
           Columna = 4,
           Posicion = 1,
           Formato = enumFormato.Moneda
            )
        ]
        public decimal? Pendiente { get; set; }

        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Presupuesto",
            Ayuda = "presupuesto facturado",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Presupuesto),
            Controlador = nameof(enumControladoresVentas.Presupuestos),
            VistaDondeNavegar = enumVistasVentas.CrudPresupuestos,
            Fila = 12,
            Columna = 0,
            EditableAlCrear = false,
            EditableAlEditar = false,
            VisibleEnGrid = false
            )
        ]
        public int? IdPresupuesto { get; set; }
        [IUPropiedad(Visible = false)]
        public string Presupuesto { get; set; }


        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Parte de trabajo",
            Ayuda = "parte de trabajo facturado",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            Controlador = nameof(enumControladoresVentas.PartesTr),
            VistaDondeNavegar = enumVistasVentas.CrudPartesDeTrabajo,
            MostrarExpresion = nameof(ParteTr),
            Fila = 12,
            Columna = 1,
            EditableAlCrear = false,
            EditableAlEditar = false,
            VisibleEnGrid = false
            )
        ]
        public int? IdParteTr { get; set; }

        [IUPropiedad(Visible = false)]
        public string ParteTr { get; set; }


        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Contrato",
            Ayuda = "contrato asociado",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Contrato),
            Controlador = nameof(enumControladoresJuridicos.Contratos),
            VistaDondeNavegar = enumVistasJuridicos.CrudContratos,
            ParametrosParaNavegar = nameof(ltrParametrosEp.Clase) + "=" + nameof(enumClaseDeContrato.Venta),
            Fila = 12,
            Columna = 2,
            EditableAlCrear = false,
            EditableAlEditar = false,
            VisibleEnGrid = false
            )
        ]
        public int? IdContrato { get; set; }

        [IUPropiedad(Visible = false)]
        public string Contrato { get; set; }


        //----------------------------------------------------------
        [IUPropiedad(Etiqueta = "Preasiento",
            Ayuda = "Preasiento contable",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Preasiento),
            CriterioDeBusqueda = enumCriteriosDeFiltrado.igual,
            Fila = 12,
            Columna = 2,
            Posicion = 1,
            VisibleEnGrid = false,
            VisibleAlCrear = false,
            VisibleAlEditar = true,
            EditableAlEditar = false,
            Controlador = nameof(enumControladoresContables.Preasientos),
            VistaDondeNavegar = enumVistasContables.CrudPreasientos,
            Obligatorio = false
            )
        ]
        public int? IdPreasiento { get; set; }

        [IUPropiedad(
            Etiqueta = "Preasiento",
            Visible = false
            )
        ]
        public string Preasiento { get; set; }


        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(
           TamanoFijo = "10em"
         , Etiqueta = "Expediente"
         , EtiquetaGrid = "Expediente"
         , VisibleEnEdicion = false
         , TipoDeControl = enumTipoControl.Referencia
         , AccionRef = "javascript: " + nameof(enumNameSpaceTs.ApiDelCrud) + "." + nameof(enumFunctionTs.Negocio_IrAlExpediente) + "(numeroDeFila)"
         , Alineada = enumAliniacion.derecha)]
        public string IrAlExpediente { get; set; }

        //----------------------------------------------------
        [IUPropiedad(Visible = false)]
        public bool EsRectificativa { get; set; }

        //----------------------------------------------------
        [IUPropiedad(Visible = false)]
        public bool EsPeriodica { get; set; }

        //----------------------------------------------------
        [IUPropiedad(Visible = false)]
        public bool ConIrpf { get; set; }

        //----------------------------------------------------
        [IUPropiedad(Visible = false)]
        public bool EsExportacion { get; set; }

        //----------------------------------------------------
        [IUPropiedad(Visible = false)]
        public bool? EsIntraComunitaria { get; set; }

        //----------------------------------------------------
        [IUPropiedad(Visible = false)]
        public bool? EstaCobrada { get; set; }

        //----------------------------------------------------
        [IUPropiedad(Oculto = true)]
        public int? IdArchivo { get; set; }

        //----------------------------------------------------
        [IUPropiedad(Visible = false)]
        public bool EstaComunicandose { get; set; }


        //----------------------------------------------------
        [IUPropiedad(Visible = false)]
        public decimal? PorAbonar { get; set; }

        ////----------------------------------------------------
        //[IUPropiedad(Visible = false)]
        //public decimal? ImportePorCobrar { get; set; }


    }


}
