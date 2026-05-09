using ModeloDeDto.Contabilidad;
using ModeloDeDto.Expediente;
using ModeloDeDto.Juridico;
using ModeloDeDto.MaestrosTecnico;
using ModeloDeDto.Terceros;
using ServicioDeDatos;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Gastos;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.MaestrosTecnico;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Terceros;
using System;
using Utilidades;

namespace ModeloDeDto.Gastos
{


    public class IndFacturaRec
    {
        public const string UnidadDeMedida = nameof(UnidadDeMedida);
        public const string Naturaleza = nameof(Naturaleza);
        public const string ComoTratarLaFechaDeRecepcion = nameof(ComoTratarLaFechaDeRecepcion);
    }


    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, MostrarExpresion = nameof(IUsaNombreDto.Nombre), EditarTrasCrear = true)]
    public class FacturaRecDto : ElementoDeUnProcesoDto
    {
        [IUPropiedad(
          Etiqueta = "Factura proveedor",
          Tipo = typeof(string),
          Ayuda = "nº de factura de proveedor",
          EtiquetaGrid = "NºFac",
          VisibleEnGrid = true,
          PosicionEnGrid = 5,
          TipoDeControl = enumTipoControl.Editor,
          //Alineada = enumAliniacion.centrada,
          TamanoFijo = "10em",
          EditableAlEditar = true,
          EditableAlCrear = true,
          Obligatorio = true,
          Fila = 0,
          Columna = 4,
          Posicion = 0,
          EsAlmacenable = true,
          LongitudMaxima = 25
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
            TipoDeControlEnGrid = enumTipoControl.Referencia,
            AccionRef = "javascript: " + nameof(enumNameSpaceTs.ApiDelCrud) + "." + nameof(enumFunctionTs.Negocio_IrAlProveedor) + "(numeroDeFila)",
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
            PorAnchoMnt = 25,
            EsAlmacenable = true,
            Ordenar = true,
            OrdenarGridPor = nameof(Proveedor) + "." + nameof(ProveedorDtm.Nombre),
            EditableAlCrear = true,
            EditableAlEditar = false,
            trasBlanquear = "javascript:" + nameof(enumNameSpaceTs.Gasto) + "." + nameof(enumFunctionTs.Far_Tras_Blanquear_Proveedor) + "([" + nameof(enumParamTs.idLista) + "])",
            trasSeleccionar = "javascript:" + nameof(enumNameSpaceTs.Gasto) + "." + nameof(enumFunctionTs.Far_Tras_Seleccionar_Proveedor) + "([" + nameof(enumParamTs.idLista) + "])"
            )]
        public string Proveedor { get; set; }

        [IUPropiedad(Visible = false)]
        public string Interlocutor { get; set; }

        [IUPropiedad(Visible = false)]
        public int IdInterlocutor { get; set; }

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


        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Id de la factura rectificada",
            Visible = false
            )
        ]
        public int? IdRectificada { get; set; }

        [IUPropiedad(
            Etiqueta = "factura rectificada",
            EtiquetaGrid = "Rectificada",
            Ayuda = "factura a la que se rectifica",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(FacturaRecDto),
            GuardarEn = nameof(IdRectificada),
            MostrarExpresion = nameof(FacturaRecDto.Expresion),
            Controlador = nameof(enumControladoresGastos.FacturasRec),
            VistaDondeNavegar = enumVistasGastos.CrudFacturasRec,
            BuscarPor = ltrDeUnaFacturaRec.SelectorDeFacturasNoRecificadas,
            RestringidoPorControl = nameof(Proveedor) + ";" + nameof(IdSociedadDelCg) + ";" + ltrFiltros.IdEditado,
            Fila = 4,
            Columna = 0,
            Posicion = 0,
            VisibleAlCrear = false,
            EditableAlEditar = false,
            VisibleEnGrid = false,
            LongitudMinimaParaBuscar = 1,
            AutoSpan = false
            )
        ]
        public string Rectificada { get; set; }

        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Id del contrato",
            Visible = false
            )
        ]
        public int? IdContrato { get; set; }

        [IUPropiedad(
            Etiqueta = "Contrato de compra",
            EtiquetaGrid = "Contrato",
            Ayuda = "Contrato asociable",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(ContratoDto),
            GuardarEn = nameof(IdContrato),
            MostrarExpresion = nameof(ContratoDtm.Expresion),
            Controlador = nameof(enumControladoresJuridicos.Contratos),
            VistaDondeNavegar = enumVistasJuridicos.CrudContratos,
            ParametrosParaNavegar = nameof(ltrParametrosEp.Clase) + Simbolos.Igual + nameof(enumClaseDeContrato.Compra),
            RestrictorFijo = ltrDeUnContrato.SelectorParaUnaFacturaRec + Simbolos.PuntoComa + nameof(enumEtapasDeContratos.CTR_Etapa_Vigente) + Simbolos.PuntoComa + nameof(enumEtapasDeContratos.CTR_Etapa_Finalizacion),
            RestringidoPorControl = nameof(Proveedor),
            Fila = 4,
            Columna = 1,
            Posicion = 1,
            EditableAlCrear = true,
            EditableAlEditar = true,
            VisibleEnGrid = false,
            LongitudMinimaParaBuscar = 1,
            AutoSpan = false
            )
        ]
        public string Contrato { get; set; }


        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Id del expediente",
            Visible = false
            )
        ]
        public int? IdExpediente { get; set; }

        [IUPropiedad(
            Etiqueta = "Expediente de la factura",
            EtiquetaGrid = "Expediente",
            Ayuda = "expediente a asociarle la factura",
            TipoDeControl = enumTipoControl.ListaDinamica,
            TipoDeControlEnGrid = enumTipoControl.Referencia,
            AccionRef = "javascript: " + nameof(enumNameSpaceTs.ApiDelCrud) + "." + nameof(enumFunctionTs.Negocio_IrAlExpediente) + "(numeroDeFila)",
            SeleccionarDe = typeof(ExpedienteDto),
            GuardarEn = nameof(IdExpediente),
            MostrarExpresion = nameof(ExpedienteDtm.Expresion),
            Controlador = nameof(enumControladoresAdministrativos.Expedientes),
            VistaDondeNavegar = enumVistasAdministrativo.CrudExpedientes,
            RestrictorFijo = ltrDeUnExpediente.SelectorParaUnaFacturaRec + Simbolos.PuntoComa + nameof(enumEtapasDeExpedientes.EXP_Etapa_Ejecucion) + Simbolos.PuntoComa + nameof(enumEtapasDeExpedientes.EXP_Etapa_Terminada),
            Fila = 4,
            Columna = 2,
            Posicion = 0,
            EditableAlCrear = true,
            EditableAlEditar = true,
            VisibleEnGrid = false,
            LongitudMinimaParaBuscar = 1,
            AutoSpan = false
            )
        ]
        public string Expediente { get; set; }

        //[IUPropiedad(
        //   TamanoFijo = "10em"
        // , Etiqueta = "Ver expediente"
        // , EtiquetaGrid = "Ver expediente"
        // , VisibleEnEdicion = false
        // , TipoDeControl = enumTipoControl.Referencia
        // , AccionRef = "javascript: " + nameof(enumNameSpaceTs.ApiDelCrud) + "." + nameof(enumFunctionTs.Negocio_IrAlExpediente) + "(numeroDeFila)"
        // , Alineada = enumAliniacion.derecha)]
        //public string IrAlExpediente { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
              PorAnchoMnt = 15
            , Etiqueta = "Facturada el"
            , Ordenar = true
            , OrdenarGridPor = nameof(FacturaRecDto.FacturadaEl)
            , TipoDeControl = enumTipoControl.SelectorDeFecha
            , VisibleEnGrid = false
            , VisibleAlCrear = true
            , EditableAlEditar = true
            , Formato = enumFormato.Fecha
            , OnBlur = "javascript:" + nameof(enumNameSpaceTs.Gasto) + "." + nameof(enumFunctionTs.Far_Tras_Indicar_Fecha_De_Emision) + "()"
            , Fila = 0
            , Columna = 4
            , Posicion = 1)]
        public DateTime? FacturadaEl { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
              PorAnchoMnt = 15
            , Etiqueta = "Recibida el"
            , Ordenar = true
            , OrdenarGridPor = nameof(FacturaRecDto.RecibidaEl)
            , TipoDeControl = enumTipoControl.SelectorDeFecha
            , VisibleEnGrid = false
            , EditableAlCrear = true
            , EditableAlEditar = true
            , ValorPorDefecto = ValoresPorDefecto.Hoy
            , Fila = 2
            , Columna = 4
            , Posicion = 0)]
        public DateTime? RecibidaEl { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
              PorAnchoMnt = 15
            , Etiqueta = "Vence el"
            , Ordenar = true
            , OrdenarGridPor = nameof(FacturaRecDto.VenceEl)
            , TipoDeControl = enumTipoControl.SelectorDeFecha
            , VisibleEnGrid = false
            , EditableAlCrear = true
            , EditableAlEditar = true
            , Fila = 2
            , Columna = 4
            , Posicion = 1)]
        public DateTime? VenceEl { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
              PorAnchoMnt = 15
            , Etiqueta = "Contabilizada el"
            , EtiquetaGrid = "Ctbl. el"
            , Ordenar = true
            , OrdenarGridPor = nameof(FacturaRecDto.ContabilizadaEl)
            , TipoDeControl = enumTipoControl.SelectorDeFecha
            , VisibleEnGrid = false
            , EditableAlCrear = false
            , EditableAlEditar = true
            , Fila = 2
            , Columna = 4
            , Posicion = 2)]
        public DateTime? ContabilizadaEl { get; set; }

        //----------------------------------------------------------
        [IUPropiedad(Etiqueta = "Preasiento",
            Ayuda = "Preasiento contable",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Preasiento),
            CriterioDeBusqueda = enumCriteriosDeFiltrado.igual,
            Fila = 12,
            Columna = 0,
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

        //----------------------------------------------
        [IUPropiedad(Etiqueta = "Id de la naturaleza contable", Visible = false)]
        public int? IdNaturaleza { get; set; }

        [IUPropiedad(
            Etiqueta = "Naturaleza",
            Ayuda = "Seleccione la naturaleza contable",
            TipoDeControl = enumTipoControl.ListaDeElemento,
            SeleccionarDe = typeof(NaturalezaDto),
            Controlador = nameof(enumControladoresMt.Naturalezas),
            GuardarEn = nameof(IdNaturaleza),
            Obligatorio = false,
            MostrarExpresion = nameof(NaturalezaDtm.Expresion),
            EditableAlCrear = true,
            EsAlmacenable = true,
            VisibleAlEditar = false,
            VisibleEnGrid = false,
            Fila = 12,
            AnchoMaximo = "142px",
            Columna = 0,
            Posicion = 0,
            AutoSpan = true
            )
        ]
        public string Naturaleza { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Cantidad",
           Tipo = typeof(decimal),
           Ayuda = "cantidad a indicar en la línea crear, 0 o blanco si no se desea crear una línea",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           Formato = enumFormato.Numero_6,
           ValorPorDefecto = 1,
           Obligatorio = false,
           EditableAlCrear = true,
           VisibleAlEditar = false,
           VisibleEnGrid = false,
           Fila = 12,
           Columna = 0,
           Posicion = 1)
        ]
        public decimal? Cantidad { get; set; }

        //----------------------------------------------
        [IUPropiedad(Etiqueta = "Id la unidad de medida", Visible = false)]
        public int? IdUnidad { get; set; }

        [IUPropiedad(
            Etiqueta = "Unidad",
            Ayuda = "Seleccione la unidad de medida de la línea a crear",
            TipoDeControl = enumTipoControl.ListaDeElemento,
            SeleccionarDe = typeof(UnidadDto),
            Controlador = nameof(enumControladoresMt.Unidades),
            GuardarEn = nameof(IdUnidad),
            Obligatorio = false,
            EditableAlCrear = true,
            EsAlmacenable = true,
            VisibleAlEditar = false,
            VisibleEnGrid = false,
            Fila = 12,
            Columna = 1,
            Posicion = 0
            )
        ]
        public string Unidad { get; set; }


        //-----------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Iva",
            Ayuda = "iva a aplicar a la BI para crear la línea de iva",
            TipoDeControl = enumTipoControl.ListaDeElemento,
            SeleccionarDe = typeof(IvaSoportadoDto),
            Controlador = nameof(enumControladoresContables.IvasSoportado),
            GuardarEn = nameof(IdIvaS),
            Obligatorio = false,
            VisibleEnGrid = false,
            EditableAlCrear = true,
            EsAlmacenable = true,
            VisibleAlEditar = false,
            Fila = 12,
            Columna = 1,
            Posicion = 1
            )
        ]
        public string IvaSoportado { get; set; }

        [IUPropiedad(Etiqueta = "Id del iva soportado", Visible = false)]
        public int? IdIvaS { get; set; }

        //-----------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Irpf",
            Ayuda = "irpf a aplicar a la BI para crear la línea de Irpf",
            TipoDeControl = enumTipoControl.ListaDeElemento,
            SeleccionarDe = typeof(IrpfDto),
            Controlador = nameof(enumControladoresContables.Irpfs),
            GuardarEn = nameof(IdIrpf),
            Obligatorio = false,
            VisibleEnGrid = false,
            EditableAlCrear = true,
            EsAlmacenable = true,
            VisibleAlEditar = false,
            Fila = 12,
            Columna = 2,
            Posicion = 0
            )
        ]
        public string IrpfAplicado { get; set; }

        [IUPropiedad(Etiqueta = "Id del irpf", Visible = false)]
        public int? IdIrpf { get; set; }


        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Base Imponible",
           Tipo = typeof(decimal),
           EtiquetaGrid = "B.I",
           Ayuda = "importe sin iva ni irpf de la factura",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = true,
           EditableAlEditar = true,
           Fila = 12,
           Columna = 2,
           Posicion = 1,
           AutoSpan = true,
           MantenerHuecoDeLaIzquierda = true,
           Formato = enumFormato.Numero_6,
           EsAlmacenable = true
            )
        ]
        public decimal BaseImponible { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Total a pagar",
           EtiquetaGrid = "A pagar",
           Tipo = typeof(decimal),
           Ayuda = "importe de la factura con iva y sin Irpf",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = true,
           EditableAlEditar = true,
           Fila = 12,
           Columna = 3,
           Posicion = 0,
           EsAlmacenable = true,
           Formato = enumFormato.Numero_6,
            OnFocus = "javascript:" + nameof(enumNameSpaceTs.Gasto) + "." + nameof(enumFunctionTs.Far_Al_Entrar_En_Total_A_Pagar) + "()"
            )
        ]
        public decimal TotalDelPago { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Total pagado",
           EtiquetaGrid = "Pagado",
           Tipo = typeof(decimal),
           Ayuda = "importe pagado",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           VisibleEnEdicion = false,
           Formato = enumFormato.Numero_6
            )
        ]
        public decimal TotalPagado { get; set; }

        //--------------------------------------------
        [IUPropiedad(Visible = false)]
        public decimal TotalPagosEnCurso { get; set; }

        //--------------------------------------------
        [IUPropiedad(Visible = false)]
        public decimal TotalRectificado { get; set; }

        //--------------------------------------------
        [IUPropiedad(Visible = false)]
        public decimal TotalDevuelto { get; set; }
        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Importe IVA",
           EtiquetaGrid = "IVA",
           Tipo = typeof(decimal),
           Ayuda = "importe de IVA",
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
        public decimal? Iva { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Importe IRPF",
           EtiquetaGrid = "IRPF",
           Tipo = typeof(decimal),
           Ayuda = "importe de IRPF",
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
        public decimal? Irpf { get; set; }


        //------------------------------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Pagada",
           Ayuda = "indica si la factura se ha pagado",
           VisibleEnGrid = false,
           Obligatorio = false,
           Fila = 13,
           Columna = 0,
           TipoDeControl = enumTipoControl.Check,
           css = enumCssControles.CheckEnLinea,
           ValorPorDefecto = false,
           VisibleAlCrear = true,
           VisibleAlEditar = false,
           AnchoMaximo = "100px",
           EsAlmacenable = true,
           OnChange = "javascript:" + nameof(enumNameSpaceTs.Gasto) + "." + nameof(enumFunctionTs.Far_AlCambiar_Pagada) + "(this)"
           )]
        public bool Pagada { get; set; }


        //-------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Modo de pago",
            Ayuda = "indique la clase de pago",
            TipoDeControl = enumTipoControl.Enumerado,
            EsAlmacenable = true,
            Tipo = typeof(enumModoDePagoContado),
            GuardarEn = nameof(ModoDePago),
            OnChange = "javascript:" + nameof(enumNameSpaceTs.Gasto) + "." + nameof(enumFunctionTs.Far_Tras_Cambiar_Modo_De_Pago) + "()",
            Fila = 13,
            Columna = 0,
            Posicion = 1,
            VisibleEnGrid = false,
            EditableAlCrear = true,
            VisibleAlEditar = false
          )
        ]
        public enumModoDePagoContado ModoDePago { get; set; }


        //----------------------------------------------
        [IUPropiedad(Etiqueta = "Id la cuenta de cargo", Visible = false)]
        public int IdTarjeta { get; set; }

        [IUPropiedad(
            Etiqueta = "Tarjeta bancaria",
            Ayuda = "Seleccione la tarjeta usada",
            TipoDeControl = enumTipoControl.ListaDeElemento,
            SeleccionarDe = typeof(TarjetaDeMiSociedadDto),
            Controlador = nameof(enumControladoresTerceros.TarjetasDeMiSociedad),
            MostrarExpresion = nameof(TarjetaDeMiSociedadDto.Expresion),
            GuardarEn = nameof(IdTarjeta),
            RestringidoPorControl = nameof(Cg),
            VisibleEnGrid = false,
            EditableAlCrear = true,
            VisibleAlEditar = false,
            CargarBajoDemanda = true,
            Fila = 13,
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
            RestringidoPorControl = nameof(IdSociedadDelCg),
            VisibleEnGrid = false,
            EditableAlCrear = true,
            VisibleAlEditar = false,
            CargarBajoDemanda = true,
            Fila = 13,
            Columna = 2,
            Obligatorio = false,
            EsAlmacenable = true
            )
        ]
        public string DomiciliadaEn { get; set; }


        //------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Impuestos",
            CssDeLaColumna = enumCssGrid.ColumnaOculta,
            VisibleEnGrid = true,
            VisibleAlEditar = false,
            VisibleAlCrear = false)]
        public string Impuestos { get; set; }

        //------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Naturalezas",
            CssDeLaColumna = enumCssGrid.ColumnaOculta,
            VisibleEnGrid = true,
            VisibleAlEditar = false,
            VisibleAlCrear = false)]
        public string Naturalezas { get; set; }

        //------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Formas de pago",
            CssDeLaColumna = enumCssGrid.ColumnaOculta,
            VisibleEnGrid = true,
            VisibleAlEditar = false,
            VisibleAlCrear = false)]
        public string FormasDePago { get; set; }


        //------------------------------------------------------------------------
        [IUPropiedad(
            VisibleEnGrid = false,
            EtiquetaGrid = "",
            Etiqueta = "Archivo factura",
            Ayuda = "Seleccione el fichero de la factura recibida",
            Tipo = typeof(int),
            TipoDeControl = enumTipoControl.SelectorDeUnArchivo,
            ExtensionesValidas = ExtensorDeTipoDeArchivos.NoEditablesMaJson,
            Fila = 15,
            Columna = 0,
            AutoSpan = true)]
        public int? IdArchivo { get; set; }

        //------------------------------------------------------------------------
        [IUPropiedad(
            VisibleEnGrid = false,
            EtiquetaGrid = "",
            Etiqueta = "justificante de pago",
            Ayuda = "Seleccione el justificante de pago",
            Tipo = typeof(int),
            TipoDeControl = enumTipoControl.SelectorDeUnArchivo,
            ExtensionesValidas = ExtensorDeTipoDeArchivos.NoEditablesMaJson,
            VisibleAlEditar = false,
            VisibleEnVisorAlCrear = false,
            Fila = 15,
            Columna = 1,
            AutoSpan = true)]
        //[IUPropiedad(Visible = false)]
        public int? IdJustificanteDePago { get; set; }


        [IUPropiedad(Visible = false)]
        public bool? EsIncorporada { get; set; }
    }


}
