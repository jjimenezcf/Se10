using Utilidades;
using ModeloDeDto.Terceros;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.SistemaDocumental;

namespace ModeloDeDto.SistemaDocumental
{

    public class IndArchivador
    {
        public const string PermiteSincronizar = nameof(PermiteSincronizar);
        public const string IdTipoArchivadorDeFacturaRec = nameof(IdTipoArchivadorDeFacturaRec);
    }

    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, MostrarExpresion = nameof(IUsaNombreDto.Nombre))]
    public class ArchivadorDto : ElmentoAuditadoDto, IElementoConTipoConCgDto, IUsaBajaDto, IUsaBloqueoDto
    {

        //----------------------------------------------
        [IUPropiedad(
        Etiqueta = "",
        Oculto = true,
        Obligatorio = false,
        VisibleEnGrid = false,
        Fila = 0,
        Columna = 0,
        Posicion = 0
        )]
        public int? IdSociedadDelCg { get; set; }

        [IUPropiedad(
         Etiqueta = "Id del Cg",
         Visible = false
         )]
        public int IdCg { get; set; }

        [IUPropiedad(
            Etiqueta = "CG",
            Ayuda = "Centro gestor",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdCg),
            MostrarExpresion = nameof(CentroGestorDto.Expresion),
            Controlador = nameof(enumControladoresTerceros.CentrosGestores),
            VistaDondeNavegar = enumVistasTerceros.CrudCentrosGestores,
            SeleccionarDe = typeof(CentroGestorDto),
            Negocio = enumNegocio.CentroGestor,
            RestrictorFijo = nameof(NegociosDeUnCgDtm.Negocio) + ";" + nameof(enumNegocio.Archivador) + ";" + nameof(enumModoDeAccesoDeDatos.Gestor),
            SoloEnAlta = true,
            LongitudMinimaParaBuscar = 1,
            Tipo = typeof(string),
            Fila = 0,
            Columna = 0,
            Ordenar = true,
            OrdenarGridPor = nameof(Cg)+"."+ nameof(CentroGestorDtm.Codigo),
            EditableAlCrear = true,
            EditableAlEditar = false)]
        public string Cg { get; set; }


        //----------------------------------------------
        [IUPropiedad(
              Etiqueta = "Id del tipo de archivado",
              Visible = false
              )]
        public int IdTipo { get; set; }

        [IUPropiedad(
            Etiqueta = "Tipo",
            Ayuda = "Tipo de archivador",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdTipo),
            Controlador = nameof(enumControladoresNegocio.TiposDeElemento),
            SeleccionarDe = typeof(TipoDeArchivadorDto),
            VistaDondeNavegar = enumVistasSistemaDocumental.TiposDeArchivador,
            RestrictorFijo = ltrParametrosDto.Negocio + ";" + nameof(enumNegocio.Archivador) + ";" + nameof(enumModoDeAccesoDeDatos.Gestor),
            SoloEnAlta = true,
            Negocio = enumNegocio.Archivador,
            LongitudMinimaParaBuscar = 1,
            Tipo = typeof(string),
            Fila = 0,
            Columna = 1,
            Ordenar = true,
            OrdenarGridPor = nameof(Tipo) + "." + nameof(TipoDeArchivadorDtm.Nombre),
            EditableAlCrear = true,
            EditableAlEditar = false)]
        public string Tipo { get; set; }


        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Referencia",
            Ayuda = "Referencia del archivador",
            Tipo = typeof(string),
            Ordenar = true,
            Obligatorio = false,
            VisibleAlCrear = false,
            EditableAlEditar = false,
            EditableAlCrear = false,
            Fila = 1,
            Columna = 0
          )
        ]
        public string Referencia { get; set; }
        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Archivador",
            Ayuda = "Indique el nombre del archivador",
            Tipo = typeof(string),
            Fila = 1,
            Columna = 1,
            Ordenar = true,
            PorAnchoMnt = 50,
            Obligatorio = true,
            ColSpan = 2,
            LongitudMaxima = 250
          )
        ]
        public string Nombre { get; set; }


        //----------------------------------------------
        [IUPropiedad(
           Etiqueta = "Descripción",
           Ayuda = "describa el elemento",
           TipoDeControl = enumTipoControl.AreaDeTexto,
           VisibleEnGrid = false,
           Obligatorio = false,
           EditableAlEditar =true,
           NumeroDeFilas = 5,
           Fila = 2,
           Columna = 0,
           AutoSpan = true
          )
        ]
        public string Descripcion { get; set; }

        //----------------------------------------------------------------
        // antes tenía puesto que además de ser adm podía ser creador
        [IUPropiedad(
            Etiqueta = "Sincronizar con",
            Ayuda = "seleccione la carpeta con la que sincronizar",
            VisibleEnGrid = false,
            Obligatorio = false,
            Fila = 3,
            Columna = 0,
            TipoDeControl = enumTipoControl.Editor,
            PermisosNecesarios = nameof(enumModoDeAccesoDeDatos.Administrador), 
            VisibleEnEdicion = false
            )
        ]
        public string SincronizarCon { get; set; }
        //----------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Sincronizar",
            Ayuda = "Somete el trabajo de sincronización del documento",
            VisibleEnGrid = false,
            Obligatorio = false,
            Fila = 3,
            Columna = 1,
            TipoDeControl = enumTipoControl.Referencia,
            css = enumCssControles.ReferenciaCentrada,
            VisibleAlCrear = false,
            AccionRef = "javascript: " + nameof(enumNameSpaceTs.SistemaDocumental) + ".SometerSincronizacion([id])"
            )
        ]
        public string Sincronizar { get; }

        //----------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Carpetas del archivador",
            Ayuda = "navega a la gestión de carpetas del archivador",
            VisibleEnGrid = false,
            Obligatorio = false,
            Fila = 4,
            Columna = 0,
            EnConsultaOcultar = false,
            TipoDeControl = enumTipoControl.Referencia,
            css = enumCssControles.ControlApilado,
            VisibleAlCrear = false,
            AccionRef = "/" + nameof(enumControladoresSistemaDocumental.Carpetas) + "/" + enumVistasSistemaDocumental.CrudCarpetas +"?idarchivador=[id]"
            )
        ]
        public string Carpetas { get; }

        //------------------------------------------------------------------------
        [IUPropiedad(
            VisibleEnGrid = false,
            Etiqueta = "Archivo",
            Ayuda = "Seleccione un fichero",
            Tipo = typeof(int),
            LimiteEnByte = TipoControlExtension.BytePermitidosEnZip,
            TipoDeControl = enumTipoControl.SelectorDeUnArchivo,
            ExtensionesValidas = "*",
            Fila = 5,
            Columna = 0,
            VisibleAlEditar = false,
            Obligatorio = false,
            AutoSpan = true)]
        public int? IdArchivoAlCrear { get; set; }

        //----------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Bloqueado",
            Ayuda = "indica si el archivador está bloqueado",
            VisibleEnGrid = false,
            Obligatorio = false,
            Fila = 5,
            Columna = 0,
            TipoDeControl = enumTipoControl.Check,
            css = enumCssControles.ControlApilado,
            ValorPorDefecto = false,
            VisibleAlCrear = false,
            EditableAlCrear = false,
            EditableAlEditar =true,
            OnChange = "javascript:" + nameof(enumNameSpaceTs.Crud) + "." + nameof(enumFunctionTs.Neg_Tras_Pulsar_Bloquear) + "()"
            )
        ]
        public bool Bloqueado { get; set; }

        [IUPropiedad(Visible = false)]
        public string Bloqueador { get; set; }

        //----------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Baja",
            Ayuda = "indica si el archivador está de baja",
            VisibleEnGrid = false,
            Obligatorio = false,
            Fila = 6,
            Columna = 1,
            TipoDeControl = enumTipoControl.Check,
            css = enumCssControles.ControlApilado,
            ValorPorDefecto = false,
            VisibleAlCrear = false,
            EditableAlCrear = false,
            EditableAlEditar = false
            )
        ]
        public bool Baja { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Cantidad",
           Ayuda = "cantidad de ficheros anexados",
           TipoDeControl = enumTipoControl.Editor,
           Fila = 10,
           Columna = 0,
           Visible = false,
           VisibleEnGrid = false,
           VisibleEnEdicion = false,
           Obligatorio = false
           )
        ]
        public int Cantidad { get; set; }
        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Con carpetas",
           Ayuda = "indica si en el archivador hay carpetas",
           TipoDeControl = enumTipoControl.Editor,
           Fila = 10,
           Columna = 0,
            Visible = false,
           VisibleEnGrid = false,
           VisibleEnEdicion = false,
           Obligatorio = false
           )
        ]
        public bool ConCarpetas { get; set; }

        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(
           TamanoFijo = "6em"
         , Etiqueta = "Acción"
         , EtiquetaGrid = ""
         , VisibleEnEdicion = false
         , TipoDeControl = enumTipoControl.Referencia
         , AccionRef = "javascript: " + nameof(enumNameSpaceTs.SistemaDocumental) + "." + nameof(enumFunctionTs.Arcdor_AbrirCarpetas) + "(numeroDeFila)"
         , Alineada = enumAliniacion.derecha)]
        public string Accion { get; set; } = "Carpetas";

        //--------------------------------------------------
        [IUPropiedad(Visible = false)]
        public bool UsaClasePorTipo { get; set; }

        [IUPropiedad(Visible = false)]
        public bool EsInterventor { get; set; }

        [IUPropiedad(Visible = false)]
        public bool EsGestor { get; set; }

        [IUPropiedad(Visible = false)]
        public bool EsUnCorreo {  get; set; }
    }
}
