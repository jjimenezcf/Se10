using ModeloDeDto.Contabilidad;
using ModeloDeDto.Entorno;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Terceros;
using Utilidades;

namespace ModeloDeDto.Terceros
{
    public static class ltrTrabajador
    {
        public static readonly string Trabajadores = enumNegocio.Trabajador.Plural();
        public static readonly string Trabajador = enumNegocio.Trabajador.Singular();
        public static readonly string IdPersona = nameof(IdPersona);
        public static readonly string IdSociedad = nameof(IdSociedad);
    }

    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, MostrarExpresion = nameof(Expresion), EditarTrasCrear = true)]
    public class TrabajadorDto : ElmentoAuditadoDto, IUsaNombreDto, IUsaBajaDto
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
            RestrictorFijo = nameof(NegociosDeUnCgDtm.Negocio) + ";" + nameof(enumNegocio.Trabajador) + ";" + nameof(enumModoDeAccesoDeDatos.Gestor),
            RestringidoPorControl = nameof(IdSociedadDelCg),
            PropiedadRestrictora = nameof(IdSociedadDelCg),
            SoloEnAlta = true,
            LongitudMinimaParaBuscar = 1,
            Tipo = typeof(string),
            Fila = 0,
            Columna = 0,
            Ordenar = true,
            EditableAlCrear = true,
            EditableAlEditar = false,
            trasSeleccionar = "javascript:" + nameof(enumNameSpaceTs.Terceros) + "." + nameof(enumFunctionTs.Trb_ProponerDatosDelCg) + "([" + nameof(enumParamTs.idLista) + "])",
            Posicion = 1)]
        public string Cg { get; set; }

        //----------------------------------------------
        [IUPropiedad(Etiqueta = "Id del interlocutor", Visible = false)]
        public int IdInterlocutor { get; set; }

        [IUPropiedad(
            Etiqueta = nameof(ltrTrabajador.Trabajador),
            AyudaDeCriteriosDeBusqueda = "seleccione por nombre del interlocutor, dni, correo o teléfono de la persona",
            GuardarEn = nameof(IdInterlocutor),
            BuscarPor = ltrInterlocutor.BuscarPorPersona,
            MostrarExpresion = nameof(InterlocutorDto.Expresion),
            Controlador = nameof(enumControladoresTerceros.Interlocutores),
            VistaDondeNavegar = enumVistasTerceros.CrudInterlocutores,
            SeleccionarDe = typeof(InterlocutorDto),
            Negocio = enumNegocio.Interlocutor,
            SoloEnAlta = true,
            EditableAlCrear = true,
            EditableAlEditar = false,
            TipoDeControl = enumTipoControl.ListaDinamica,
            Fila = 0,
            Columna = 1,
            Ordenar = true,
            PorAnchoMnt = 50,
            AutoSpan = true
          )
        ]
        public new string Expresion { get; set; }

        [IUPropiedad(Visible = false)]
        public string Nombre { get; set; }

        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Id del usuario asociado al trabajador",
            Visible = false
            )
        ]
        public int? IdUsuario { get; set; }

        [IUPropiedad(
            Etiqueta = "Usuario asociado",
            Ayuda = "Usuario asociado al trabajador",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(UsuarioDto),
            GuardarEn = nameof(IdUsuario),
            MostrarExpresion = UsuarioDto.ExpresionElemento,
            Controlador = nameof(enumControladoresEntorno.Usuarios),
            VistaDondeNavegar = enumVistasEntorno.CrudUsuario,
            Fila = 1,
            Columna = 0,
            VisibleEnGrid = false,
            EditableAlEditar = true,
            EditableAlCrear = true,
            Obligatorio = false,
            LongitudMinimaParaBuscar = 1,
            AutoSpan = true,
            trasSeleccionar = "javascript:" + nameof(enumNameSpaceTs.Terceros) + "." + nameof(enumFunctionTs.Trb_ProponerDatosDelUsuarioSeleccionado) + "([" + nameof(enumParamTs.idLista) + "])"
            )
        ]
        public string Usuario { get; set; }

        //----------------------------------------------
        [IUPropiedad(
           Etiqueta = "eMail",
           Ayuda = "eMail del trabajador",
           Fila = 1,
           Columna = 1,
           Ordenar = true,
           LongitudMaxima = 50
          )
        ]
        public string eMail { get; set; }

        //----------------------------------------------
        [IUPropiedad(
           Etiqueta = "Teléfono",
           Ayuda = "teléfono del trabajador",
           Fila = 1,
           Columna = 2,
           LongitudMaxima = 15
          )
        ]
        public string Telefono { get; set; }


        //----------------------------------------------
        [IUPropiedad(Etiqueta = "Id la cuenta contable", Visible = false, Obligatorio = false)]
        public int? IdCuenta { get; set; }

        [IUPropiedad(
            Etiqueta = "Cuenta",
            Ayuda = "Seleccione la cuenta contable",
            TipoDeControl = enumTipoControl.ListaDeElemento,
            SeleccionarDe = typeof(CuentaDto),
            Controlador = nameof(enumControladoresContables.Cuentas),
            GuardarEn = nameof(IdCuenta),
            VisibleEnGrid = false,
            TrasCargar = "javascript:" + nameof(enumNameSpaceTs.Terceros) + "." + nameof(enumFunctionTs.Trb_TrasCargarCuentasContables) + "()",
            Fila = 2,
            Columna = 1,
            Obligatorio = false
            )
        ]
        public string Cuenta { get; set; }

        //----------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Baja",
            Ayuda = "indica si el trabajador está de baja",
            Fila = 3,
            Columna = 0,
            TipoDeControl = enumTipoControl.Check,
            css = enumCssControles.ControlApilado,
            ValorPorDefecto = false,
            EditableAlEditar = true,
            VisibleEnGrid = false
            )
        ]
        public bool Baja { get; set; }
    }
}
