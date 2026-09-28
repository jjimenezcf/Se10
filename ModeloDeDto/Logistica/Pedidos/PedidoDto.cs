using ModeloDeDto.Expediente;
using ModeloDeDto.Juridico;
using ModeloDeDto.MaestrosTecnico;
using ModeloDeDto.Terceros;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.MaestrosTecnico;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Terceros;
using System;
using Utilidades;

namespace ModeloDeDto.Logistica
{


    public class IndPedido
    {
        public const string UnidadDeMedida = nameof(UnidadDeMedida);
        public const string Naturaleza = nameof(Naturaleza);
        public const string TipoDeLinea = nameof(TipoDeLinea);
    }


    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, MostrarExpresion = nameof(IUsaNombreDto.Nombre), EditarTrasCrear = true)]
    public class PedidoDto : ElementoDeUnProcesoDto
    {

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
         Etiqueta = "Id del proveedor del pedido",
         Visible = false
         )]
        public int IdProveedor { get; set; }

        [IUPropiedad(
            Etiqueta = "Proveedor",
            Ayuda = "Quién me provee",
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
            PorAnchoMnt = 25,
            EsAlmacenable = true,
            Ordenar = true,
            OrdenarGridPor = nameof(Proveedor) + "." + nameof(ProveedorDtm.Nombre),
            EditableAlCrear = true,
            EditableAlEditar = false,
            trasBlanquear = "javascript:" + nameof(enumNameSpaceTs.Logistica) + "." + nameof(enumFunctionTs.Ped_Tras_Blanquear_Proveedor) + "([" + nameof(enumParamTs.idLista) + "])",
            trasSeleccionar = "javascript:" + nameof(enumNameSpaceTs.Logistica) + "." + nameof(enumFunctionTs.Ped_Tras_Seleccionar_Proveedor) + "([" + nameof(enumParamTs.idLista) + "])"
            )]
        public string Proveedor { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        // Propuestos por el proveedor del pedido para inicializar una línea de tipo Alzada (ver Ped_Tras_Cambiar_TipoDeLinea)
        [IUPropiedad(Etiqueta = "Unidad propuesta por el proveedor", Visible = false)]
        public int? IdUnidadDelProveedor { get; set; }

        [IUPropiedad(Etiqueta = "Concepto propuesto por el proveedor", Visible = false)]
        public string ConceptoDelProveedor { get; set; }

        [IUPropiedad(Etiqueta = "Base imponible propuesta por el proveedor", Visible = false)]
        public decimal? BiPropuestoDelProveedor { get; set; }

        // Indica si el proveedor del pedido tiene tarifas definidas: si las tiene, al crear una línea se propone
        // tipo Unitario (elegir del maestro) en vez de Alzada (ver Ped_InicializarModalParaCrearLineas)
        [IUPropiedad(Etiqueta = "El proveedor usa tarifa", Visible = false)]
        public bool UsaTarifaDelProveedor { get; set; }

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
            RestrictorFijo = ltrDeUnContrato.SelectorParaUnPedido + Simbolos.PuntoComa + nameof(enumEtapasDeContratos.CTR_Etapa_Vigente) + Simbolos.separadorDeEtapas + nameof(enumEtapasDeContratos.CTR_Etapa_Finalizacion),
            RestringidoPorControl = nameof(Proveedor),
            Fila = 3,
            Columna = 3,
            Posicion = 0,
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
            Etiqueta = "Expediente del pedido",
            EtiquetaGrid = "Expediente",
            Ayuda = "expediente a asociarle la pedido",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(ExpedienteDto),
            GuardarEn = nameof(IdExpediente),
            MostrarExpresion = nameof(ExpedienteDtm.Expresion),
            Controlador = nameof(enumControladoresAdministrativos.Expedientes),
            VistaDondeNavegar = enumVistasAdministrativo.CrudExpedientes,
            RestrictorFijo = ltrDeUnExpediente.SelectorParaUnPedido + Simbolos.PuntoComa + nameof(enumEtapasDeExpedientes.EXP_Etapa_Ejecucion) + Simbolos.separadorDeEtapas + nameof(enumEtapasDeExpedientes.EXP_Etapa_Terminada),
            Fila = 3,
            Columna = 4,
            Posicion = 0,
            EditableAlCrear = true,
            EditableAlEditar = true,
            VisibleEnGrid = false,
            LongitudMinimaParaBuscar = 1,
            AutoSpan = false
            )
        ]
        public string Expediente { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
              PorAnchoMnt = 15
            , Etiqueta = "Pedir el"
            , Ordenar = true
            , OrdenarGridPor = nameof(PedidoDto.PedidoEl)
            , TipoDeControl = enumTipoControl.SelectorDeFecha
            , VisibleEnGrid = false
            , VisibleAlCrear = true
            , EditableAlEditar = true
            , Formato = enumFormato.Fecha
            , ValorPorDefecto = ValoresPorDefecto.Hoy
            , SelectorHasta = nameof(EntregarEl) + ":7"
            , Fila = 0
            , Columna = 4
            , Posicion = 1)]
        public DateTime? PedidoEl { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
              PorAnchoMnt = 15
            , Etiqueta = "Entregar el"
            , Ordenar = true
            , OrdenarGridPor = nameof(PedidoDto.EntregarEl)
            , TipoDeControl = enumTipoControl.SelectorDeFecha
            , VisibleEnGrid = false
            , EditableAlCrear = true
            , EditableAlEditar = true
            , Fila = 0
            , Columna = 4
            , Posicion = 1)]
        public DateTime? EntregarEl { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
              PorAnchoMnt = 15
            , Etiqueta = "Recibido el"
            , Ordenar = true
            , OrdenarGridPor = nameof(PedidoDto.RecibidoEl)
            , TipoDeControl = enumTipoControl.SelectorDeFecha
            , VisibleEnGrid = false
            , EditableAlCrear = false
            , EditableAlEditar = true
            , Fila = 2
            , Columna = 4
            , Posicion = 0)]
        public DateTime? RecibidoEl { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
              PorAnchoMnt = 15
            , Etiqueta = "Cerrado el"
            , Ordenar = true
            , OrdenarGridPor = nameof(PedidoDto.CerradoEl)
            , TipoDeControl = enumTipoControl.SelectorDeFecha
            , VisibleEnGrid = false
            , EditableAlCrear = false
            , EditableAlEditar = true
            , Fila = 2
            , Columna = 4
            , Posicion = 1)]
        public DateTime? CerradoEl { get; set; }


        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Importe",
           Tipo = typeof(decimal),
           Ayuda = "importe del pedido",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Fila = 12,
           Columna = 4,
           Posicion = 0,
           AutoSpan = true,
           MantenerHuecoDeLaIzquierda = true,
           Formato = enumFormato.Numero_6
            )
        ]
        public decimal? Importe { get; set; }


        //------------------------------------------------------------------------
        [IUPropiedad(
            VisibleEnGrid = false,
            EtiquetaGrid = "",
            Etiqueta = "Añadir fichero como pedido",
            Ayuda = "Fichero que se enviará al proveedor que sustituirá al generado por el sistema",
            Tipo = typeof(int),
            TipoDeControl = enumTipoControl.SelectorDeUnArchivo,
            ExtensionesValidas = ExtensorDeTipoDeArchivos.NoEditables,
            Fila = 13,
            Columna = 0,
            AutoSpan = true)]
        public int? IdArchivoPedido { get; set; }


        //----------------------------------------------
        [IUPropiedad(Etiqueta = "Id de la naturaleza contable", Visible = false)]
        public int? IdNaturaleza { get; set; }

        [IUPropiedad(
            Etiqueta = "Naturaleza",
            Ayuda = "Naturaleza propuesta para las líneas del pedido: la del proveedor si la tiene definida, si no la del negocio. No se guarda en el pedido, es solo informativa",
            TipoDeControl = enumTipoControl.ListaDeElemento,
            SeleccionarDe = typeof(NaturalezaDto),
            Controlador = nameof(enumControladoresMt.Naturalezas),
            GuardarEn = nameof(IdNaturaleza),
            Obligatorio = false,
            MostrarExpresion = nameof(NaturalezaDtm.Expresion),
            EditableAlCrear = false,
            EsAlmacenable = true,
            VisibleAlEditar = false,
            VisibleEnGrid = false,
            Fila = 12,
            Columna = 0,
            OnBlur = "javascript:" + nameof(enumNameSpaceTs.Logistica) + "." + nameof(enumFunctionTs.Ped_Tras_Cambiar_Naturaleza_Del_Detalle) + "()"
            )
        ]
        public string Naturaleza { get; set; }

        //----------------------------------------------------------------

        [IUPropiedad(
            Etiqueta = "Clase",
            Ayuda = "Clase de unitario correspondiente a la naturaleza del pedido indicada",
            TipoDeControl = enumTipoControl.Enumerado,
            Tipo = typeof(enumClaseUnitario),
            GuardarEn = nameof(ClaseDeLinea),
            VisibleAlEditar = false,
            EditableAlCrear = false,
            VisibleEnGrid = false,
            EsAlmacenable = true,
            Obligatorio = false,
            Fila = 12,
            Columna = 1
            )
        ]
        public enumClaseUnitario? ClaseDeLinea { get; set; }

    }


}
