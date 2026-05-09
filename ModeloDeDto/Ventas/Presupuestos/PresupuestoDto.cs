using ModeloDeDto.Entorno;
using ModeloDeDto.Terceros;
using ServicioDeDatos.Seguridad;
using Utilidades;

namespace ModeloDeDto.Presupuesto
{
    public class IndPresupuesto
    {
        public const string UnidadDeMedida = nameof(UnidadDeMedida);
        public const string Naturaleza = nameof(Naturaleza);
        public const string TipoDeLinea = nameof(TipoDeLinea);
        public const string ClaseDeUnitario = nameof(ClaseDeUnitario);
    }

    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, MostrarExpresion = nameof(IUsaNombreDto.Nombre), EditarTrasCrear = true)]
    public class PresupuestoDto : ElementoDeUnProcesoDto, IUsaSolicitanteDto, IPuedeUsarResponsableDto, IUsaDirecciones
    {

        //------------------------------------------------------------------------------------------------------------
        [IUPropiedad(Oculto = true, Obligatorio = false)]
        public int? IdTipoFacturaPorDefecto { get; set; }

        [IUPropiedad(Oculto = true, Obligatorio = false)]
        public string TipoFacturaPorDefecto { get; set; }

        //------------------------------------------------------------------------------------------------------------
        [IUPropiedad(Oculto = true, Obligatorio = false)]
        public int? IdTipoPartePorDefecto { get; set; }

        [IUPropiedad(Oculto = true, Obligatorio = false)]
        public string TipoPartePorDefecto { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
         Etiqueta = "Id del solicitante del Presupuesto",
         Visible = false
         )]
        public int IdSolicitante { get; set; }

        [IUPropiedad(
            Etiqueta = "Solicitante",
            Ayuda = "Quién solicita el Presupuesto",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdSolicitante),
            Controlador = nameof(enumControladoresTerceros.Interlocutores),
            SeleccionarDe = typeof(InterlocutorDto),
            VistaDondeNavegar = enumVistasTerceros.CrudInterlocutores,
            RestrictorFijo = ltrParametrosDto.Negocio + ";" + nameof(enumNegocio.Interlocutor) + ";" + nameof(enumModoDeAccesoDeDatos.Consultor),
            Negocio = enumNegocio.Interlocutor,
            LongitudMinimaParaBuscar = 1,
            Tipo = typeof(string),
            Fila = 2,
            Columna = 0,
            EditableAlCrear = true,
            EditableAlEditar = false,
            AutoSpan = true)]
        public string Solicitante { get; set; }

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
            Etiqueta = "Id del usuario responsable",
            Visible = false
            )
        ]
        public int? IdResponsable { get; set; }

        [IUPropiedad(
            Etiqueta = "Responsable del",
            Ayuda = "Usuario ejecutor",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(UsuarioDto),
            GuardarEn = nameof(IdResponsable),
            MostrarExpresion = UsuarioDto.ExpresionElemento,
            Controlador = nameof(enumControladoresEntorno.Usuarios),
            VistaDondeNavegar = enumVistasEntorno.CrudUsuario,
            Fila = 10,
            Columna = 0,
            VisibleEnGrid = false,
            EditableAlEditar = true,
            Obligatorio = false,
            LongitudMinimaParaBuscar = 1,
            ColSpan = 1
            )
        ]
        public string Responsable { get; set; }

        //----------------------------------------------------------
        [IUPropiedad(Etiqueta = "Expediente",
            Ayuda = "Expediente al que pertenece el presupuesto",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Expediente),
            Fila = 10,
            Columna = 1,
            VisibleEnGrid = false,
            EditableAlCrear = false,
            EditableAlEditar = false,
            Obligatorio = false,
            AutoSpan = true,
            Controlador = nameof(enumControladoresAdministrativos.Expedientes),
            VistaDondeNavegar = enumVistasAdministrativo.CrudExpedientes
            )
        ]
        public int idExpediente { get; set; }

        [IUPropiedad(Visible = false, PorAnchoMnt = 20, Etiqueta = "Expediente", Obligatorio = false)]
        public string Expediente { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "B.I.",
           Tipo = typeof(decimal),
           Ayuda = "base imponible",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           VisibleAlCrear = false,
           EditableAlEditar = false,
           Obligatorio = false,
           Formato = enumFormato.Moneda,
           Fila = 11,
           Columna = 1,
           Posicion = 0,
           MantenerHuecoDeLaIzquierda = true
            )
        ]
        public decimal? TotalSinIva { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Total",
           Tipo = typeof(decimal),
           Ayuda = "importe del presupuesto con iva",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           VisibleAlCrear = false,
           EditableAlEditar = false,
           Obligatorio = false,
           Formato = enumFormato.Moneda,
           Fila = 11,
           Columna = 1,
           Posicion = 1
            )
        ]
        public decimal? TotalConIva { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Ejecutando",
           EtiquetaGrid = "Ejecutando",
           Tipo = typeof(decimal),
           Ayuda = "importe en ejecución",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           VisibleAlCrear = false,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Obligatorio = false,
           VisibleEnGrid = false,
           PorAnchoMnt = 15,
           Fila = 11,
           Columna = 2,
           Posicion = 0,
           Formato = enumFormato.Moneda
            )
        ]
        public decimal? Ejecutando { get; set; }


        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Ejecutado",
           EtiquetaGrid = "Ejecutado",
           Tipo = typeof(decimal),
           Ayuda = "importe ejecutado",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           VisibleAlCrear = false,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Obligatorio = false,
           VisibleEnGrid = false,
           PorAnchoMnt = 15,
           Fila = 11,
           Columna = 2,
           Posicion = 1,
           Formato = enumFormato.Moneda
            )
        ]
        public decimal? Ejecutado { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Prefacturado",
           EtiquetaGrid = "Prefacturado",
           Tipo = typeof(decimal),
           Ayuda = "importe prefacturado",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           VisibleAlCrear = false,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Obligatorio = false,
           VisibleEnGrid = false,
           PorAnchoMnt = 15,
           Fila = 11,
           Columna = 2,
           Posicion = 2,
           Formato = enumFormato.Moneda
            )
        ]
        public decimal? Prefacturado { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Facturado",
           EtiquetaGrid = "Facturado",
           Tipo = typeof(decimal),
           Ayuda = "importe facturado",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           VisibleAlCrear = false,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Obligatorio = false,
           VisibleEnGrid = false,
           PorAnchoMnt = 15,
           Fila = 11,
           Columna = 2,
           Posicion = 3,
           Formato = enumFormato.Moneda
            )
        ]
        public decimal? Facturado { get; set; }


        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Direcciones",
           EtiquetaGrid = "Direcciones",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           VisibleEnEdicion = false,
           Obligatorio = false,
           VisibleEnGrid = false,
           PorAnchoMnt = 15
            )
        ]
        public string Direcciones { get; set; }

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

    }


}
