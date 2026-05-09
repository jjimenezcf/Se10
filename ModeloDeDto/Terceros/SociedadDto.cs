using ServicioDeDatos;
using Utilidades;

namespace ModeloDeDto.Terceros
{


    [IUDto(AnchoEtiqueta = 20
         , AnchoSeparador = 5
         , MostrarExpresion = nameof(Expresion))]
    public class SociedadDto : ElmentoAuditadoDto, IUsaNombreDto, IUsaBajaDto, IUsaArchivoDto
    {
        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Sociedad",
            Ayuda = "Indique el nombre del Sociedad",
            Tipo = typeof(string),
            Fila = 0,
            Columna = 0,
            Ordenar = true,
            OrdenarGridPor = nameof(Nombre),
            PorAnchoMnt = 50,
            Obligatorio = true,
            LongitudMaxima = 250,
            AutoSpan = true,
            OnBlur = "javascript: " + nameof(enumNameSpaceTs.Terceros) + "." + nameof(enumFunctionTs.Sociedad_CopiarEnRazonSocial) + "()"
          )
        ]
        public string Nombre { get; set; }

        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = "Razon social",
            Ayuda = "indique la razón social",
            LongitudMaxima = 255,
            Fila = 0,
            Columna = 1
          )
        ]
        public string RazonSocial { get; set; }
        //----------------------------------------------
        [IUPropiedad(
           Etiqueta = "NIF",
           Ayuda = "indique el NIF/CIF",
           TipoDeControl = enumTipoControl.Editor,
           LongitudMaxima = 25,
           Fila = 1,
           Columna = 0
          )
        ]
        public string Nif { get; set; }
        //----------------------------------------------
        [IUPropiedad(
           Etiqueta = "Codigo Fiscal",
           Ayuda = "indique anagrama o código de la sociedad gestionada por su sistema contable",
           TipoDeControl = enumTipoControl.Editor,
           LongitudMaxima = 4,
           VisibleEnGrid = false,
           Fila = 1,
           Columna = 1,
           Obligatorio = false
          )
        ]
        public string CodigoFiscal { get; set; }

        //----------------------------------------------
        [IUPropiedad(
           Etiqueta = "eMail",
           Ayuda = "indique el mail",
           TipoDeControl = enumTipoControl.Editor,
           LongitudMaxima = 50,
           Fila = 2,
           Columna = 1
          )
        ]
        public string eMail { get; set; }

        //----------------------------------------------
        [IUPropiedad(
           Etiqueta = "Teléfono",
           Ayuda = "indique el teléfono de la sociedad",
           TipoDeControl = enumTipoControl.Editor,
           LongitudMaxima = 15,
           VisibleEnGrid = false,
           Obligatorio = false,
           Fila = 2,
           Columna = 1,
           Posicion = 1
          )
        ]
        public string Telefono { get; set; }

        //----------------------------------------------
        [IUPropiedad(
           Etiqueta = "Certificado",
           Ayuda = "Certificado de la sociedad gestionada",
           TipoDeControl = enumTipoControl.Editor,
           VisibleEnGrid = false,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Obligatorio = false,
           Fila = 2,
           Columna = 2,
           Posicion = 1
          )
        ]
        public string Certificado { get; set; }

        //----------------------------------------------------------
        [IUPropiedad(Etiqueta = "Proveedor",
            Ayuda = "Proveedor asociado",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Proveedor),
            CriterioDeBusqueda = enumCriteriosDeFiltrado.igual,
            Negocio = enumNegocio.Proveedor,
            Fila = 3,
            Columna = 0,
            VisibleEnGrid = false,
            VisibleAlCrear = false,
            VisibleAlEditar = true,
            EditableAlEditar = false,
            Controlador = nameof(enumControladoresTerceros.Proveedores),
            VistaDondeNavegar = enumVistasTerceros.CrudProveedores,
            Obligatorio = false
            )
        ]
        public int? IdProveedor { get; set; }

        [IUPropiedad(Etiqueta = "texto proveedor",Visible = false)]
        public string Proveedor { get; set; }

        //----------------------------------------------------------
        [IUPropiedad(Etiqueta = "Cliente",
            Ayuda = "Cliente asociado",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Cliente),
            CriterioDeBusqueda = enumCriteriosDeFiltrado.igual,
            Negocio = enumNegocio.Cliente,
            Fila = 3,
            Columna = 1,
            VisibleEnGrid = false,
            VisibleAlCrear = false,
            VisibleAlEditar = true,
            EditableAlEditar = false,
            Controlador = nameof(enumControladoresTerceros.Clientes),
            VistaDondeNavegar = enumVistasTerceros.CrudClientes,
            Obligatorio = false
            )
        ]
        public int? IdCliente { get; set; }

        [IUPropiedad(Etiqueta = "texto cliente", Visible = false)]
        public string Cliente { get; set; }

        //----------------------------------------------------------
        [IUPropiedad(Etiqueta = "Interlocutor",
            Ayuda = "interlocutor asociado",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Interlocutor),
            CriterioDeBusqueda = enumCriteriosDeFiltrado.igual,
            Negocio = enumNegocio.Interlocutor,
            Fila = 3,
            Columna = 2,
            VisibleEnGrid = false,
            VisibleAlCrear = false,
            VisibleAlEditar = true,
            EditableAlEditar = false,
            Controlador = nameof(enumControladoresTerceros.Interlocutores),
            VistaDondeNavegar = enumVistasTerceros.CrudInterlocutores,
            Obligatorio = false
            )
        ]
        public int? IdInterlocutor { get; set; }

        [IUPropiedad(Etiqueta = "texto interlocutor", Visible = false)]
        public string Interlocutor { get; set; }

        //------------------------------------------------------------------------
        [IUPropiedad(
            VisibleEnGrid = false,
            VisibleEnEdicion = true,
            Etiqueta = "Logo",
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

        //----------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Editar centros gestores",
            Ayuda = "navega a la gestión de centros de gestión de la sociedad",
            VisibleEnGrid = false,
            Obligatorio = false,
            Fila = 4,
            Columna = 1,
            EnConsultaOcultar = false,
            TipoDeControl = enumTipoControl.Referencia,
            css = enumCssControles.ControlApilado,
            VisibleAlCrear = false,
            AccionRef = "/" + nameof(enumControladoresTerceros.CentrosGestores) + "/" + enumVistasTerceros.CrudCentrosGestores + "?" + nameof(CentroGestorDto.IdSociedad) + "=[id]",
            Posicion = 0            
            )
        ]
        public string Cgs { get; }


        //----------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Agenda de sociedad",
            Ayuda = "navega a la agenda de la sociedad",
            VisibleEnGrid = false,
            Obligatorio = false,
            Fila = 4,
            Columna = 1,
            EnConsultaOcultar = false,
            TipoDeControl = enumTipoControl.Referencia,
            css = enumCssControles.ControlApilado,
            VisibleAlCrear = false,
            AccionRef = "javascript:" + nameof(enumNameSpaceTs.Terceros) + "." + nameof(enumFunctionTs.Sociedad_AbrirAgenda) +"([" + nameof(IdAgenda) + "])",
            Posicion = 1
            )
        ]
        public string Agenda { get; }
        
        [IUPropiedad(Visible = false)]
        public int? IdAgenda { get; set; }

        //----------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Crear interlocutor",
            Ayuda = "Indica si al crear la sociedad se crea el interlocutor",
            VisibleEnGrid = false,
            Obligatorio = false,
            Fila = 5,
            Columna = 0,
            TipoDeControl = enumTipoControl.Check,
            css = enumCssControles.ControlApilado,
            ValorPorDefecto = false,
            EditableAlCrear = true,
            VisibleAlEditar = false,
            OnChange = "javascript:" + nameof(enumNameSpaceTs.Terceros) + "." + nameof(enumFunctionTs.Sociedad_CrearInterlocutor_Change) + "(this)"
            )
        ]
        public bool CrearInterlocutor { get; set; }       
        
        //----------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Crear proveedor",
            Ayuda = "indica si al crear la sociedad se crea como proveedor",
            Fila = 6,
            Columna = 0,
            TipoDeControl = enumTipoControl.Check,
            css = enumCssControles.ControlApilado,
            ValorPorDefecto = false,
            VisibleEnGrid = false,
            VisibleAlCrear = true,
            VisibleAlEditar = false,
            EditableAlCrear = true,
            OnChange = "javascript:" + nameof(enumNameSpaceTs.Terceros) + "." + nameof(enumFunctionTs.Sociedad_CrearProveedor_Change) +"(this)"
            )
        ]
        public bool CrearProveedor { get; set; }


        //----------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Crear cliente",
            Ayuda = "indica si al crear la sociedad se crea como cliente",
            Fila = 7,
            Columna = 0,
            TipoDeControl = enumTipoControl.Check,
            css = enumCssControles.ControlApilado,
            ValorPorDefecto = false,
            VisibleEnGrid = false,
            VisibleAlCrear = true,
            VisibleAlEditar = false,
            EditableAlCrear = true,
            OnChange = "javascript:" + nameof(enumNameSpaceTs.Terceros) + "." + nameof(enumFunctionTs.Sociedad_CrearCliente_Change) + "(this)"
            )
        ]
        public bool CrearCliente { get; set; }

        //----------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Crear procurador",
            Ayuda = "indica si al crear la sociedad se crea como procurador",
            Fila = 8,
            Columna = 0,
            TipoDeControl = enumTipoControl.Check,
            css = enumCssControles.ControlApilado,
            ValorPorDefecto = false,
            VisibleEnGrid = false,
            VisibleAlCrear = true,
            VisibleAlEditar = false,
            EditableAlCrear = true,
            OnChange = "javascript:" + nameof(enumNameSpaceTs.Terceros) + "." + nameof(enumFunctionTs.Sociedad_CrearProcurador_Change) + "(this)"
            )
        ]
        public bool CrearProcurador { get; set; }
        //----------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Crear abogado",
            Ayuda = "indica si al crear la sociedad se crea como abogado",
            Fila = 9,
            Columna = 0,
            TipoDeControl = enumTipoControl.Check,
            css = enumCssControles.ControlApilado,
            ValorPorDefecto = false,
            VisibleEnGrid = false,
            VisibleAlCrear = true,
            VisibleAlEditar = false,
            EditableAlCrear = true,
            OnChange = "javascript:" + nameof(enumNameSpaceTs.Terceros) + "." + nameof(enumFunctionTs.Sociedad_CrearAbogado_Change) +"(this)"
            )
        ]
        public bool CrearAbogado { get; set; }

        //----------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Es interlocutor",
            Ayuda = "indica si la sociedad es un interlocutor",
            Fila = 5,
            Columna = 0,
            TipoDeControl = enumTipoControl.Check,
            css = enumCssControles.ControlApilado,
            ValorPorDefecto = false,
            VisibleEnGrid = false,
            VisibleAlCrear = false,
            VisibleAlEditar = true,
            EditableAlEditar = false
            )
        ]
        public bool EsInterlocutor { get; set; }
        //----------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Baja",
            Ayuda = "indica si la Sociedad está de baja",
            VisibleEnGrid = false,
            Obligatorio = false,
            Fila = 10,
            Columna = 0,
            TipoDeControl = enumTipoControl.Check,
            css = enumCssControles.ControlApilado,
            ValorPorDefecto = false,
            VisibleAlCrear = true,
            EditableAlCrear = false,
            EditableAlEditar = false
            )
        ]
        public bool Baja { get; set; }

        //----------------------------------------------------------------
        [IUPropiedad(Visible = false)]
        public string DireccionFiscal { get; set; }

        //----------------------------------------------------------------
        [IUPropiedad(Visible = false)]
        public bool EsUnaDeMisSociedades { get; set; }

        //----------------------------------------------------------------
        [IUPropiedad(Visible = false)]
        public bool UsaVerifactu { get; set; }

        //----------------------------------------------------------------
        [IUPropiedad(Visible = false)]
        public bool VerifactuEnProductivo { get; set; }

        //----------------------------------------------------------------
        [IUPropiedad(Visible = false)] public int? IdAbogado { get; set; }
        [IUPropiedad(Visible = false)] public int? IdProcurador { get; set; }
        
        //----------------------------------------------------------------
        [IUPropiedad(Visible = false)]
        public string TipoDeTercero { get; set; }

    }
}
