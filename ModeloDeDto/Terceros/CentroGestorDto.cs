using Utilidades;
using ModeloDeDto.Entorno;

namespace ModeloDeDto.Terceros
{
    [IUDto(MostrarExpresion = nameof(IUsaNombreDto.Nombre))]
    public class CentroGestorDto : ElmentoAuditadoDto, IUsaNombreDto, IUsaBajaDto, IUsaArchivoDto
    {
        //-------------------------------------------------------------------------
        [IUPropiedad(Etiqueta = "Sociedad",
         Ayuda = "Sociedad del CG",
         TipoDeControl = enumTipoControl.ListaDinamica,
         SeleccionarDe = typeof(SociedadDto),
         GuardarEn = nameof(IdSociedad),
         MostrarExpresion = nameof(SociedadDto.Expresion),
         Controlador = nameof(enumControladoresTerceros.Sociedades),
         VistaDondeNavegar = enumVistasTerceros.CrudSociedades,
         AlSeleccionarBlanquearControl = nameof(CgPadre),
         Fila = 0,
         Columna = 0,
         VisibleEnGrid = false
         )]
        public string Sociedad { get; set; }

        [IUPropiedad(
            Etiqueta = "Sociedad",
            Visible = false
            )
        ]
        public int IdSociedad { get; set; }

        //----------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Id del CG padre",
            Visible = false
            )
        ]
        public int? IdCgPadre { get; set; }

        [IUPropiedad(
            Etiqueta = "CG Padre",
            Ayuda = "Indique el CG padre",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(CentroGestorDto),
            GuardarEn = nameof(IdCgPadre),
            MostrarExpresion = nameof(Expresion),
            Controlador = nameof(enumControladoresTerceros.CentrosGestores),
            VistaDondeNavegar = enumVistasTerceros.CrudCentrosGestores,
            RestringidoPorControl = nameof(Sociedad),
            PropiedadRestrictora = nameof(IdSociedad),
            AplicarJoin = true,
            Fila = 0,
            Columna = 1,
            Ordenar = true,
            Obligatorio = false
            )
        ]
        public string CgPadre { get; set; }

        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Centro gestor",
            Ayuda = "Indique el nombre del centro gestor",
            Tipo = typeof(string),
            Fila = 1,
            Columna = 0,
            Ordenar = true,
            PorAnchoMnt = 50,
            Obligatorio = true,
            LongitudMaxima = 250
          )
        ]
        public string Nombre { get; set; }

        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Código",
            Ayuda = "indique el código",
            Fila = 1,
            Columna = 1
          )
        ]
        public string Codigo { get; set; }

        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Usuario responsable",
            Visible = false
            )
        ]
        public int IdResponsable { get; set; }

        [IUPropiedad(
            Etiqueta = "Responsable",
            Ayuda = "Usuario responsable",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(UsuarioDto),
            GuardarEn = nameof(IdResponsable),
            MostrarExpresion = UsuarioDto.ExpresionElemento,
            Controlador = nameof(enumControladoresEntorno.Usuarios),
            VistaDondeNavegar = enumVistasEntorno.CrudUsuario,
            Fila = 2,
            Columna = 0,
            VisibleEnGrid = false,
            trasSeleccionar = "javascript:" + nameof(enumNameSpaceTs.Terceros) + "." + nameof(enumFunctionTs.CG_Tras_Seleccionar_Responsable) + "()",
            trasBlanquear = "javascript:" + nameof(enumNameSpaceTs.Terceros) + "." + nameof(enumFunctionTs.CG_Tras_Blanquear_Responsable) + "()"
            )
        ]
        public string Responsable { get; set; }

        //----------------------------------------------
        [IUPropiedad(
           Etiqueta = "eMail",
           Ayuda = "indique el mail",
           TipoDeControl = enumTipoControl.Editor,
           Fila = 2,
           Columna = 1,
           Obligatorio = false
          )
        ]
        public string eMail { get; set; }
        
        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Sigla",
            Ayuda = "indique la sigla",
            Obligatorio = false,
            Fila = 3,
            Columna = 0
          )
        ]
        public string Sigla { get; set; }

        //----------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Baja",
            Ayuda = "indica si el CG está de baja",
            VisibleEnGrid = false,
            Obligatorio = false,
            Fila = 3,
            Columna = 1,
            TipoDeControl = enumTipoControl.Check,
            ValorPorDefecto = false,
            EditableAlCrear = false,
            EditableAlEditar = true,
            DisplaCssDelTd = "contents",
            css = enumCssControles.CheckEnLinea
            )
        ]
        public bool Baja { get; set; }
        //------------------------------------------------------------------------
        [IUPropiedad(
            VisibleEnGrid = false,
            VisibleEnEdicion = true,
            Etiqueta = "Logotipo",
            Ayuda = "Seleccione un fichero",
            Tipo = typeof(int),
            TipoDeControl = enumTipoControl.Archivo,
            ExtensionesValidas = ExtensorDeTipoDeArchivos.Imagenes,
            UrlDelArchivo = nameof(Archivo),
            Obligatorio = false,
            Fila = 4,
            Columna = 0)]
        public int? IdArchivo { get; set; }

        [IUPropiedad(TipoDeControl = enumTipoControl.ImagenDelCanvas)]
        public string Archivo { get; set; }
    }
}
