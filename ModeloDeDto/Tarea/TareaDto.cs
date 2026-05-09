using ModeloDeDto.Entorno;
using ModeloDeDto.Terceros;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Tarea;
using Utilidades;
using static ServicioDeDatos.Elemento.Enumerados;

namespace ModeloDeDto.Tarea
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, MostrarExpresion = nameof(IUsaNombreDto.Nombre), EditarTrasCrear = true)]
    public class TareaDto : ElementoDeUnProcesoDto,IUsaSolicitanteDto, IPuedeUsarResponsableDto
    {

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
         Etiqueta = "Id del solicitante de la tarea",
         Visible = false
         )]
        public int IdSolicitante { get; set; }

        [IUPropiedad(
            Etiqueta = "Solicitante",
            Ayuda = "Quién solicita la tarea",
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
            AutoSpan = true,
            EsAlmacenable = true
            )]
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
            Etiqueta = "Asignada a",
            Ayuda = "Usuario ejecutor",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(UsuarioDto),
            GuardarEn = nameof(IdResponsable),
            MostrarExpresion = UsuarioDto.ExpresionElemento,
            Controlador = nameof(enumControladoresEntorno.Usuarios),
            VistaDondeNavegar = enumVistasEntorno.CrudUsuario,
            Fila = 4,
            Columna = 0,
            VisibleEnGrid = true,
            EditableAlEditar = true,
            Obligatorio = false,
            Ordenar = true,
            OrdenarGridPor = nameof(Responsable) + "." + nameof(UsuarioDtm.Login),
            LongitudMinimaParaBuscar = 1,
            AutoSpan = true,
            EsAlmacenable = true
            )
        ]
        public string Responsable { get; set; }

        //----------------------------------------------------------
        [IUPropiedad(Etiqueta = "Expediente",
            Ayuda = "Expediente agrupador de la tarea",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Expediente),
            Fila = 4,
            Columna = 1,
            VisibleEnGrid = false,
            EditableAlCrear = false,
            EditableAlEditar = false,
            AutoSpan = true,
            Controlador = nameof(enumControladoresAdministrativos.Expedientes),
            VistaDondeNavegar = enumVistasAdministrativo.CrudExpedientes,
            Obligatorio = false
            )
        ]
        public int? IdExpediente { get; set; }

        [IUPropiedad(Visible = false, PorAnchoMnt = 20, Etiqueta = "Expediente", Obligatorio = false)]
        public string Expediente { get; set; }

        //------------------------------------------------------------------------
        [IUPropiedad(
            VisibleEnGrid = false,
            Etiqueta = "Archivo",
            Ayuda = "Seleccione un fichero",
            Tipo = typeof(int),
            LimiteEnByte = TipoControlExtension.BytePermitidosNormal,
            TipoDeControl = enumTipoControl.SelectorDeUnArchivo,
            ExtensionesValidas = "*",
            Fila = 7,
            Columna = 0,
            VisibleAlEditar = false,
            Obligatorio = false,
            AutoSpan = true)]
        public int? IdArchivoAlCrear { get; set; }

        //----------------------------------------------------------
        [IUPropiedad(Etiqueta = "Factura",
            Ayuda = "Factura asociada",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(FacturaEmt),
            Fila = 4,
            Columna = 2,
            VisibleEnGrid = false,
            EditableAlCrear = false,
            EditableAlEditar = false,
            AutoSpan = true,
            Controlador = nameof(enumControladoresVentas.FacturasEmt),
            VistaDondeNavegar = enumVistasVentas.CrudFacturasEmt,
            Obligatorio = false
            )
        ]
        public int? IdFacturaEmt { get; set; }

        [IUPropiedad(Visible = false)]
        public string FacturaEmt { get; set; }

        //-------------------------------------------------
        [IUPropiedad(Visible = false)]
        public enumEtapasDeTareas Etapa { get; set; }

        //-------------------------------------------------
        [IUPropiedad(Etiqueta = "Usa planificacion", Visible = false)]
        public bool UsaPlanificacion { get; set; }


        //-------------------------------------------------
        [IUPropiedad(Visible = false
          , PorAnchoMnt = 20
          , EtiquetaGrid = "Planificada"
          , Ordenar = false
          , Obligatorio = true
          , VisibleEnEdicion = false
          , Alineada = enumAliniacion.centrada)]
        public string Planificada { get; set; }

        //----------------------------------------------
        [IUPropiedad(Visible = false
          , PorAnchoMnt = 20
          , EtiquetaGrid = "Ejecutada"
          , Ordenar = false
          , Obligatorio = true
          , VisibleEnEdicion = false
          , Alineada = enumAliniacion.centrada)]
        public string Ejecutada { get; set; }

        //----------------------------------------------
        [IUPropiedad(Visible = false
          , PorAnchoMnt = 15
          , EtiquetaGrid = "Durabilidad"
          , Ordenar = false
          , Obligatorio = true
          , VisibleEnEdicion = false
          , Alineada = enumAliniacion.centrada)]
        public string Durabilidad { get; set; }

        //-------------------------------------------------
        [IUPropiedad(Visible = false)]
        public bool EsFacturable { get; set; }
        
        [IUPropiedad(Visible = false)]
        public decimal? Facturado { get; set; }

        [IUPropiedad(Visible = false)] 
        public enumDurabilidad? Medido { get; set; }
    }


}
