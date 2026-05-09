using ModeloDeDto.MaestrosTecnico;
using ModeloDeDto.Terceros;
using ServicioDeDatos;
using ServicioDeDatos.MaestrosTecnico;
using ServicioDeDatos.Seguridad;
using System;
using Utilidades;

namespace ModeloDeDto.Gastos
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, MostrarExpresion = nameof(IUsaNombreDto.Nombre), EditarTrasCrear = true)]
    public class PagoDto : ElementoDeUnProcesoDto, IUsaCuentaBancariaDto
    {

        //-------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Clase de pago",
            Ayuda = "indique la clase de pago",
            TipoDeControl = enumTipoControl.Enumerado,
            EsAlmacenable = true,
            Tipo = typeof(enumClaseDePago),
            GuardarEn = nameof(Clase),
            OnChange = "javascript:" + nameof(enumNameSpaceTs.Gasto) + "." + nameof(enumFunctionTs.Pag_Tras_Cambiar_Clase_De_Pago) + "()",
            Fila = 0,
            Columna = 4,
            EditableAlEditar = false
          )
        ]
        public enumClaseDePago Clase { get; set; }


        //-------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Modo de pago",
            Ayuda = "indique el modo de pago",
            TipoDeControl = enumTipoControl.Enumerado,
            EsAlmacenable = true,
            Tipo = typeof(enumModoDePagoContado),
            GuardarEn = nameof(ModoDePago),
            OnChange = "javascript:" + nameof(enumNameSpaceTs.Gasto) + "." + nameof(enumFunctionTs.Pag_Tras_Cambiar_Modo_De_Pago) + "()",
            Fila = 0,
            Columna = 4,
            Posicion = 1,
            EditableAlEditar = false,
            VisibleEnGrid = false
          )
        ]
        public enumModoDePagoContado? ModoDePago { get; set; }


        //----------------------------------------------
        [IUPropiedad(Etiqueta = "Id la tarjeta de pago", Visible = false)]
        public int? IdTarjetaDePago { get; set; }

        [IUPropiedad(
            Etiqueta = "Tarjeta",
            Ayuda = "Seleccione la tarjeta bancaria",
            TipoDeControl = enumTipoControl.ListaDeElemento,
            SeleccionarDe = typeof(TarjetaDeMiSociedadDto),
            Controlador = nameof(enumControladoresTerceros.TarjetasDeMiSociedad),
            MostrarExpresion = nameof(TarjetaDeMiSociedadDto.Expresion),
            GuardarEn = nameof(IdTarjetaDePago),
            RestringidoPorControl = nameof(IdSociedadDelCg),
            CargarBajoDemanda = true,
            VisibleEnGrid = false,
            AutoSpan = true,
            Fila = 3,
            Columna = 0,
            Posicion = 0,
            Obligatorio = false,
            AutoPosicionamiento = true,
            EditableAlEditar = true,
            OnChange = "javascript:" + nameof(enumNameSpaceTs.Gasto) + "." + nameof(enumFunctionTs.Pag_Tras_Seleccionar_Tarjeta_Pago) + "()"
            )
        ]
        public string TarjetaDePago { get; set; }

        //----------------------------------------------
        [IUPropiedad(Etiqueta = "Id de la cuenta de pago de la sociedad", Visible = false)]
        public int? IdCuentaDePago { get; set; }

        [IUPropiedad(
            Etiqueta = "Cuenta de la sociedad",
            Ayuda = "Seleccione la cuenta bancaria de la sociedad",
            TipoDeControl = enumTipoControl.ListaDeElemento,
            SeleccionarDe = typeof(CuentaDeMiSociedadDto),
            Controlador = nameof(enumControladoresTerceros.CuentasDeMiSociedad),
            MostrarExpresion = nameof(CuentaDeMiSociedadDto.Cuenta),
            GuardarEn = nameof(IdCuentaDePago),
            RestringidoPorControl = nameof(IdSociedadDelCg),
            CargarBajoDemanda = true,
            VisibleEnGrid = false,
            AutoSpan = true,
            Fila = 3,
            Columna = 0,
            Posicion = 1,
            Obligatorio = false,
            AutoPosicionamiento = true,
            EditableAlEditar = false,
            OnChange = "javascript:" + nameof(enumNameSpaceTs.Gasto) + "." + nameof(enumFunctionTs.Pag_Tras_Seleccionar_Cuenta_Pago) + "()"
            )
        ]
        public string CuentaDePago { get; set; }

        //----------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Banco",
            Ayuda = "Banco asociado a la cuenta de pago",
            Fila = 3,
            Columna = 0,
            Posicion = 1,
            VisibleEnGrid = false,
            EditableAlCrear = false,
            EditableAlEditar = false,
            AnchoMaximo = "18em",
            Obligatorio = false
            )
        ]
        public string BancoDePago { get; set; }

        //----------------------------------------------
        [IUPropiedad(
           Etiqueta = "NIF",
           Ayuda = "indique el NIF/CIF",
           TipoDeControl = enumTipoControl.Editor,
           LongitudMaxima = 15,
           VisibleAlCrear = false,
           EditableAlEditar = false,
           TamanoFijo = "20em",
           Fila = 4,
           Columna = 0
          )
        ]
        public string Nif { get; set; }

        //-------------------------------------------------------------------------------------------------------------------
        [IUPropiedad(TipoDeControl = enumTipoControl.Editor,
              Fila = 4
            , Columna = 1
            , Etiqueta = "Contacto"
            , ColSpan = 2
            , VisibleAlCrear = false
            , VisibleAlEditar = true
            , VisibleEnGrid = false)]
        public string Contacto { get; set; }

        //-------------------------------------------------------------------------------------------------------------------
        [IUPropiedad(TipoDeControl = enumTipoControl.Editor,
            Fila = 4, Columna = 3
            , Etiqueta = "Teléfono"
            , VisibleAlCrear = false
            , VisibleAlEditar = true
            , VisibleEnGrid = false)]
        public string Telefono { get; set; }

        //-------------------------------------------------------------------------------------------------------------------
        [IUPropiedad(TipoDeControl = enumTipoControl.Editor,
            Fila = 4, Columna =4
            , Etiqueta = "eMail"
            , VisibleAlCrear = false
            , VisibleAlEditar = true
            , VisibleEnGrid = false)]
        public string eMail { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
         Etiqueta = "Id del acreedor",
         Visible = false,
         EsAlmacenable = true
         )]
        public int IdSolicitante { get; set; }

        [IUPropiedad(
            Etiqueta = "Acreedor",
            Ayuda = "Quién solicita el pago",
            TipoDeControl = enumTipoControl.ListaDinamica,
            EsAlmacenable = true,
            GuardarEn = nameof(IdSolicitante),
            Controlador = nameof(enumControladoresTerceros.Interlocutores),
            SeleccionarDe = typeof(InterlocutorDto),
            VistaDondeNavegar = enumVistasTerceros.CrudInterlocutores,
            RestrictorFijo = ltrParametrosDto.Negocio + ";" + nameof(enumNegocio.Interlocutor) + ";" + nameof(enumModoDeAccesoDeDatos.Consultor),
            Negocio = enumNegocio.Interlocutor,
            LongitudMinimaParaBuscar = 1,           
            Tipo = typeof(string),
            Fila = 5,
            Columna = 0,
            ColSpan = 2,
            VisibleEnGrid = false,
            EditableAlCrear = true,
            EditableAlEditar = false,
            Obligatorio = true,
            trasSeleccionar = "javascript:" + nameof(enumNameSpaceTs.Gasto) + "." + nameof(enumFunctionTs.Pag_Tras_Seleccionar_Acreedor) + "([" + nameof(enumParamTs.idLista) + "])",
            trasBlanquear = "javascript:" + nameof(enumNameSpaceTs.Gasto) + "." + nameof(enumFunctionTs.Pag_Tras_Blanquear_Acreedor) + "([" + nameof(enumParamTs.idLista) + "])",
            AutoSpan = true)]
        public string Solicitante { get; set; }
        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
         Etiqueta = "Id del proveedor de la nota de pago",
         Visible = false
         )]
        public int? IdProveedor { get; set; }

        [IUPropiedad(
            Etiqueta = "Proveedor",
            Ayuda = "Proveedor del pago",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdProveedor),
            Controlador = nameof(enumControladoresTerceros.Proveedores),
            SeleccionarDe = typeof(ProveedorDto),
            VistaDondeNavegar = enumVistasTerceros.CrudProveedores,
            RestrictorFijo = ltrParametrosDto.Negocio + ";" + nameof(enumNegocio.Proveedor) + ";" + nameof(enumModoDeAccesoDeDatos.Consultor),
            Negocio = enumNegocio.Proveedor,
            LongitudMinimaParaBuscar = 1,
            Tipo = typeof(string),
            Fila = 5,
            Columna = 2,
            Posicion = 0,
            VisibleEnGrid = false,
            EditableAlCrear = false,
            EditableAlEditar = false,
            Obligatorio = false,
            AutoSpan = true)]
        public string Proveedor { get; set; }

        //----------------------------------------------------------
        [IUPropiedad(Etiqueta = "Id del trabajador", Visible = false)]
        public int? IdTrabajador { get; set; }

        [IUPropiedad(
            Etiqueta = "Trabajador",
            Ayuda = "trabajador del pago",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(TrabajadorDto),
            GuardarEn = nameof(IdTrabajador),
            Controlador = nameof(enumControladoresTerceros.Trabajadores),
            VistaDondeNavegar = enumVistasTerceros.CrudTrabajadores,
            BuscarPor = nameof(TrabajadorDto.Expresion),
            CriterioDeBusqueda = enumCriteriosDeFiltrado.contiene,
            RestringidoPorControl = nameof(IdSociedadDelCg),
            PropiedadRestrictora = nameof(IdSociedadDelCg),
            LongitudMinimaParaBuscar = 3,
            Fila = 5,
            Columna = 2,
            Posicion = 1,
            VisibleEnGrid = false,
            EditableAlCrear = false,
            EditableAlEditar = false,
            Obligatorio = false,
            AutoSpan = true)
        ]
        public string Trabajador { get; set; }

        //----------------------------------------------------------
        [IUPropiedad(Visible = false)]
        public string CuentaDeAcreedor => $"{Iban}-{Entidad}-{Oficina}-{DcCcc}-{Numero}";

        //----------------------------------------------------------
        [IUPropiedad(Visible = false)]
        public int? IdCuentaDeAcreedor { get; set; }

        //----------------------------------------------------------
        [IUPropiedad(Etiqueta = "Cta. destino", Ayuda = "Iban (País-DC) o Pegue el nº de cuenta", Fila = 6, Columna = 1, Posicion=0, LongitudMaxima = 4, VisibleEnGrid = false, EditableAlEditar = false,
            OnPaste = "javascript:" + nameof(enumNameSpaceTs.Gasto) + "." + nameof(enumFunctionTs.Pag_AlPegar_Iban) + "(event)", Obligatorio = false)
        ]
        public string Iban { get; set; }
        [IUPropiedad(Etiqueta = Simbolos.Descarte, Ayuda = "Entidad", Fila = 6, Columna = 1, Posicion = 1, LongitudMaxima = 4, VisibleEnGrid = false, EditableAlEditar = false, Obligatorio = false)]
        public string Entidad { get; set; }
        [IUPropiedad(Etiqueta = Simbolos.Descarte, Ayuda = "Oficina", Fila = 6, Columna = 1, Posicion = 2, LongitudMaxima = 4, VisibleEnGrid = false, EditableAlEditar = false, Obligatorio = false)]
        public string Oficina { get; set; }
        [IUPropiedad(Etiqueta = Simbolos.Descarte, Ayuda = "DC", Fila = 6, Columna = 2, Posicion = 0, LongitudMaxima = 2, VisibleEnGrid = false, EditableAlEditar = false, Obligatorio = false)]
        public string DcCcc { get; set; }
        [IUPropiedad(Etiqueta = Simbolos.Descarte, Ayuda = "Número", Fila = 6, Columna = 2, Posicion = 1, LongitudMaxima = 10, VisibleEnGrid = false, EditableAlEditar = false, Obligatorio = false)]
        public string Numero { get; set; }

        //----------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Alias",
            Ayuda = "Alias de la cuenta",
            Fila = 6,
            Columna = 3,
            Posicion = 0,
            VisibleEnGrid = false,
            VisibleAlCrear = true,
            EditableAlEditar = false,
            LongitudMaxima = 250,
            Obligatorio = false,
            AutoSpan = true
            )
        ]
        public string Alias { get; set; }

        //----------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Banco",
            Ayuda = "Banco asociado a la cuenta",
            Fila = 6,
            Columna = 3,
            Posicion = 1,
            VisibleEnGrid = false,
            EditableAlCrear = false,
            EditableAlEditar = false,
            Obligatorio = false
            )
        ]
        public string BancoAcreedor { get; set; }

        //----------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Activa",
            Ayuda = "Está activa",
            TipoDeControl = enumTipoControl.Check,
            CssDelContenedor = enumCssControles.ContenedorCheckRight,
            Fila = 6,
            Columna = 3,
            Posicion = 2,
            VisibleEnGrid = false,
            VisibleAlCrear = true,
            EditableAlCrear = false,
            EditableAlEditar = false,
            AnchoMaximo ="6em",
            LongitudMaxima = 250,
            Obligatorio = false,
            AutoSpan = true
            )
        ]
        public bool Activa { get; set; }

        //----------------------------------------------------------
        [IUPropiedad(Etiqueta = "Factura",
            Ayuda = "Pago de la factura",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(FacturaRec),
            Fila = 8,
            Columna = 1,
            VisibleEnGrid = false,
            EditableAlCrear = false,
            EditableAlEditar = false,
            Obligatorio = false,
            AutoSpan = true,
            Controlador = nameof(enumControladoresGastos.FacturasRec),
            VistaDondeNavegar = enumVistasGastos.CrudFacturasRec
            )
        ]
        public int IdFacturaRec { get; set; }

        [IUPropiedad(Visible = false, PorAnchoMnt = 20, Etiqueta = "FacturaRec", Obligatorio = false)]
        public string FacturaRec { get; set; }

        //----------------------------------------------
        [IUPropiedad(Etiqueta = "Id de la naturaleza del gasto", Visible = false)]
        public int? IdNaturaleza { get; set; }

        [IUPropiedad(
            Etiqueta = "Naturaleza",
            Ayuda = "Seleccione la naturaleza contable del gasto",
            TipoDeControl = enumTipoControl.ListaDeElemento,
            SeleccionarDe = typeof(NaturalezaDto),
            Controlador = nameof(enumControladoresMt.Naturalezas),
            GuardarEn = nameof(IdNaturaleza),
            Obligatorio = false,
            MostrarExpresion = nameof(NaturalezaDtm.Expresion),
            Fila = 8,
            Columna = 1, 
            Posicion =1,
            VisibleEnGrid = false
            )
        ]
        public string Naturaleza { get; set; }

        //----------------------------------------------------------
        [IUPropiedad(Visible = false) ]
        public int? IdFacturaEmt { get; set; }

        //----------------------------------------------------------
        [IUPropiedad(Etiqueta = "Preasiento",
            Ayuda = "Preasiento contable",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Preasiento),
            CriterioDeBusqueda = enumCriteriosDeFiltrado.igual,
            Fila = 8,
            Columna = 2,
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

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
              PorAnchoMnt = 15
            , AlinearContenido = enumAliniacion.derecha
            , Etiqueta = "Pagar el"
            , Ordenar = true
            , OrdenarGridPor = nameof(PagoDto.PagarEl)
            , TipoDeControl = enumTipoControl.SelectorDeFecha
            , VisibleEnGrid = false
            , EditableAlCrear = true
            , EditableAlEditar = true
            , MantenerHuecoDeLaIzquierda = true
            , TamanoFijo = "15em"
            //, AnchoMaximo = "15em"
            , AutoSpan = true
            , Fila = 8
            , Columna = 3
            , Posicion = 0)]
        public DateTime? PagarEl { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
              PorAnchoMnt = 15
            , Etiqueta = "Pagado el"
            , Ordenar = true
            , OrdenarGridPor = nameof(PagoDto.PagadoEl)
            , TipoDeControl = enumTipoControl.SelectorDeFecha
            , TamanoFijo = "15em"
            , VisibleEnGrid = false
            , EditableAlCrear = true
            , EditableAlEditar = true
            //, AnchoMaximo = "15em"
            , Fila = 8
            , Columna = 3
            , Posicion = 1)]
        public DateTime? PagadoEl { get; set; }


        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Importe",
           Tipo = typeof(decimal),
            EsAlmacenable = true,
           EtiquetaGrid = "Importe",
           Ayuda = "importe a pagar",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           EditableAlCrear = true,
           EditableAlEditar = true,
           Obligatorio = true,
           Fila = 8,
           Columna = 4,
           AnchoMaximo = "8.7em",
           Formato = enumFormato.Moneda
            )
        ]
        public decimal Importe { get; set; }


        //------------------------------------------------------------------------
        [IUPropiedad(
            VisibleEnGrid = false,
            Etiqueta = "Archivo",
            Ayuda = "Seleccione un fichero",
            Tipo = typeof(int),
            LimiteEnByte = TipoControlExtension.BytePermitidosNormal,
            TipoDeControl = enumTipoControl.SelectorDeUnArchivo,
            ExtensionesValidas = "*",
            Fila = 9,
            Columna = 0,
            VisibleAlEditar = false,
            Obligatorio = false,
            AutoSpan = true)]
        public int? IdArchivoAlCrear { get; set; }

        [IUPropiedad(Visible = false)]
        public int? IdCliente { get; set; }
        
        [IUPropiedad(Visible = false)]
        public string Cliente { get; set; }

        [IUPropiedad(Visible = false)]
        public bool EsAbono { get; set; }
    }


}
