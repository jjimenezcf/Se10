using ModeloDeDto.Entorno;
using ModeloDeDto.Terceros;
using ServicioDeDatos.Seguridad;
using Utilidades;

namespace ModeloDeDto.Expediente
{
    public class IndExpediente
    {
        public const string IdTipoActividad = nameof(IdTipoActividad);
        public const string TipoActividad = nameof(TipoActividad);
    }


    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, MostrarExpresion = nameof(IUsaNombreDto.Nombre), EditarTrasCrear = true)]
    public class ExpedienteDto : ElementoDeUnProcesoDto, IUsaSolicitanteDto, IPuedeUsarResponsableDto
    {

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
         Etiqueta = "Id del solicitante del expediente",
         Visible = false
         )]
        public int IdSolicitante { get; set; }

        [IUPropiedad(
            Etiqueta = "Solicitante",
            Ayuda = "Quién solicita el expediente",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdSolicitante),
            Controlador = nameof(enumControladoresTerceros.Interlocutores),
            BuscarPor = ltrInterlocutor.ParaExpediente,
            SeleccionarDe = typeof(InterlocutorDto),
            VistaDondeNavegar = enumVistasTerceros.CrudInterlocutores,
            RestringidoPorControl = nameof(Tipo),
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
            Ayuda = "Usuario responsable del expediente",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(UsuarioDto),
            GuardarEn = nameof(IdResponsable),
            MostrarExpresion = UsuarioDto.ExpresionElemento,
            Controlador = nameof(enumControladoresEntorno.Usuarios),
            VistaDondeNavegar = enumVistasEntorno.CrudUsuario,
            Fila = 4,
            Columna = 0,
            VisibleEnGrid = true,
            EditableAlEditar = false,
            Obligatorio = false,
            LongitudMinimaParaBuscar = 1,
            AutoSpan = true
            )
        ]
        public string Responsable { get; set; }


        //--------------------------------------------
        [IUPropiedad(Visible = false, PorAnchoMnt = 10, Etiqueta = "Valorado en", Formato = enumFormato.Moneda)]
        public decimal ValoradoEn { get; set; }


        //--------------------------------------------
        [IUPropiedad(VisibleEnGrid = true, VisibleEnEdicion = false, PorAnchoMnt = 10, Etiqueta = "Gastos", Formato = enumFormato.Moneda)]
        public decimal Gastos { get; set; }

        //--------------------------------------------
        [IUPropiedad(VisibleEnGrid = true, VisibleEnEdicion = false, PorAnchoMnt = 10, Etiqueta = "Ingresos", Formato = enumFormato.Moneda)]
        public decimal Ingresos { get; set; }

        //--------------------------------------------
        [IUPropiedad(VisibleEnGrid = true, VisibleEnEdicion = false, PorAnchoMnt = 10, Etiqueta = "Pagado", Formato = enumFormato.Moneda)]
        public decimal Pagado { get; set; }

        //--------------------------------------------
        [IUPropiedad(VisibleEnGrid = true, VisibleEnEdicion = false, PorAnchoMnt = 10, Etiqueta = "Cobrado", Formato = enumFormato.Moneda)]
        public decimal Cobrado { get; set; }

        //--------------------------------------------
        [IUPropiedad(VisibleEnGrid = true, VisibleEnEdicion = false, PorAnchoMnt = 10, Etiqueta = "Margen", Ayuda = "Margen de beneficio", Formato = enumFormato.Porcentaje)]
        public decimal Margen { get; set; }
        //--------------------------------------------
        [IUPropiedad(VisibleEnGrid = true, VisibleEnEdicion = false, PorAnchoMnt = 10, Etiqueta = "Rentabilidad", Ayuda = "Rentabilidad sobre el coste", Formato = enumFormato.Porcentaje)]
        public decimal Rentabilidad { get; set; }

        //--------------------------------------------
        [IUPropiedad(Visible = false, PorAnchoMnt = 10, EtiquetaGrid = "Horas")]
        public decimal? Horas { get; set; }
        //--------------------------------------------
        [IUPropiedad(Visible = false, PorAnchoMnt = 15, EtiquetaGrid = "Planificación")]
        public string Planificacion { get; set; }
        //--------------------------------------------
        [IUPropiedad(Visible = false, PorAnchoMnt = 15, EtiquetaGrid = "Ejecución")]
        public string Ejecucion { get; set; }

        //-------------------------------------------------
        [IUPropiedad(Etiqueta = "Usa tareas", Visible = false)]
        public bool UsaTareas { get; set; }

        //-------------------------------------------------
        [IUPropiedad(Etiqueta = "Usa presupuestos", Visible = false)]
        public bool UsaPpts { get; set; }

        //-------------------------------------------------
        [IUPropiedad(Etiqueta = "Usa datos jurídicos", Visible = false)]
        public bool UsaDatosJuridicos { get; set; }        

        //-------------------------------------------------
        [IUPropiedad(Etiqueta = "SC de venta", Visible = false)]
        public bool ScDeVenta { get; set; }
        //-------------------------------------------------
        [IUPropiedad(Etiqueta = "SC de compra", Visible = false)]
        public bool ScDeCompra { get; set; }

    }


}
