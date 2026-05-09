using ModeloDeDto.Entorno;
using ModeloDeDto.Terceros;
using ServicioDeDatos.Seguridad;
using Utilidades;

namespace ModeloDeDto.Juridico
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, MostrarExpresion = nameof(IUsaNombreDto.Nombre), EditarTrasCrear = true)]
    public class PleitoDto : ElementoDeUnProcesoDto
    {

        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
         Etiqueta = "Id del solicitante del pleito",
         Visible = false
         )]
        public int IdSolicitante { get; set; }

        [IUPropiedad(
            Etiqueta = "Solicitante",
            Ayuda = "Quién solicita el pleito",
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
            EditableAlEditar = false,
            Obligatorio = false,
            LongitudMinimaParaBuscar = 1,
            AutoSpan = false
            )
        ]
        public string Responsable { get; set; }

        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Id del abogado de la otra parte",
            Visible = false
            )
        ]
        public int? IdAbogado { get; set; }

        [IUPropiedad(
            Etiqueta = "Abogado",
            Ayuda = "Abogado contrario",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(AbogadoDto),
            GuardarEn = nameof(IdAbogado),
            MostrarExpresion = nameof(AbogadoDto.Expresion),
            Controlador = nameof(enumControladoresTerceros.Abogados),
            VistaDondeNavegar = enumVistasTerceros.CrudAbogados,
            Fila = 10,
            Columna = 1,
            VisibleEnGrid = false,
            EditableAlEditar = false,
            Obligatorio = false,
            LongitudMinimaParaBuscar = 1,
            AutoSpan = true
            )
        ]
        public string Abogado { get; set; }


        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Id del procurador",
            Visible = false
            )
        ]
        public int? IdProcurador { get; set; }

        [IUPropiedad(
            Etiqueta = "Procurador",
            Ayuda = "Procurador contrario",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(ProcuradorDto),
            GuardarEn = nameof(IdProcurador),
            MostrarExpresion = nameof(ProcuradorDto.Expresion),
            Controlador = nameof(enumControladoresTerceros.Procuradores),
            VistaDondeNavegar = enumVistasTerceros.CrudProcuradores,
            Fila = 11,
            Columna = 0,
            VisibleEnGrid = false,
            EditableAlEditar = false,
            Obligatorio = false,
            LongitudMinimaParaBuscar = 1,
            AutoSpan = false
            )
        ]
        public string Procurador { get; set; }
        
        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Id del juzgado",
            Visible = false
            )
        ]
        public int? IdJuzgado { get; set; }

        [IUPropiedad(
            Etiqueta = "Juzgado",
            Ayuda = "Juzgado donde se celebra",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(JuzgadoDto),
            GuardarEn = nameof(IdProcurador),
            MostrarExpresion = nameof(JuzgadoDto.Nombre),
            Controlador = nameof(enumControladoresTerceros.Juzgados),
            VistaDondeNavegar = enumVistasTerceros.CrudJuzgados,
            Fila = 11,
            Columna = 1,
            VisibleEnGrid = true,
            EditableAlEditar = false,
            Obligatorio = false,
            LongitudMinimaParaBuscar = 1,
            AutoSpan = true
            )
        ]
        public string Juzgado { get; set; }

    }


}
