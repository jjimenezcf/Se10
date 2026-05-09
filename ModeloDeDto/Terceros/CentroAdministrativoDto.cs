using Utilidades;
using ServicioDeDatos.Terceros;

namespace ModeloDeDto.Terceros
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class CentroAdministrativoDto : EsUnDetalleDto
    {

        //-------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Rol del contacto",
            Ayuda = "Destinatorio de la eFactura",
            TipoDeControl = enumTipoControl.Enumerado,
            Tipo = typeof(enumRolCentroAdministrativo),
            GuardarEn = nameof(Rol),
            Fila = 0,
            Columna = 1,
            EditableAlEditar = false
          )
        ]
        public enumRolCentroAdministrativo Rol { get; set; }

        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Codigo Dir3",
            Ayuda = "código administrativo al que facturar",
            TipoDeControl = enumTipoControl.Editor,
            EditableAlEditar = false,
            Obligatorio = true,
            LongitudMaxima = 10,
            Fila = 0,
            Columna = 2,
            VisibleEnGrid = false
          )
        ]
        public string CodigoDir3 { get; set; }

        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Organo Gestor",
            Ayuda = "Nombre del órgano gestor",
            TipoDeControl = enumTipoControl.Editor,
            EditableAlEditar = false,
            Obligatorio = true,
            LongitudMaxima = 250,
            Fila = 1,
            Columna = 0,
            VisibleEnGrid = false
          )
        ]
        public string OrganoGestor { get; set; }

        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Unidad Tramitadora",
            Ayuda = "Nombre de la unidad de tramitación",
            TipoDeControl = enumTipoControl.Editor,
            EditableAlEditar = false,
            Obligatorio = false,
            LongitudMaxima = 250,
            Fila = 1,
            Columna = 1,
            VisibleEnGrid = false
          )
        ]
        public string UnidadTramitadora { get; set; }

        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Oficina contable",
            Ayuda = "Nombre de la oficina contable",
            TipoDeControl = enumTipoControl.Editor,
            EditableAlEditar = false,
            Obligatorio = false,
            LongitudMaxima = 250,
            Fila = 1,
            Columna = 2,
            VisibleEnGrid = false
          )
        ]
        public string OficinaContable { get; set; }

        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Alias",
            Ayuda = "Información descriptiva del C.A.",
            TipoDeControl = enumTipoControl.Editor,
            Obligatorio = false,
            LongitudMaxima = 250,
            Fila = 2,
            Columna = 0,
            AutoSpan = true,
            VisibleEnGrid = false
          )
        ]
        public string Alias { get; set; }

        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(Etiqueta = "Id del contacto", Visible = false)]
        public int IdContacto { get; set; }

        [IUPropiedad(
            Etiqueta = "Contacto",
            Ayuda = "seleccione el contacto",
            TipoDeControl = enumTipoControl.ListaDeElemento,
            SeleccionarDe = typeof(InterlocutorDtm),
            Controlador = nameof(enumControladoresTerceros.Interlocutores),
            RestrictorFijo = ltrInterlocutor.BuscarPorContactoCliente + Simbolos.PuntoComa + nameof(enumNegocio.Cliente),
            RestringidoPorControl = nameof(IdElemento),
            GuardarEn = nameof(IdContacto),
            MostrarExpresion = nameof(ContactoDto.Expresion),
            VisibleAlCrear = true,
            VisibleAlEditar = true,
            AutoSpan = true,
            Obligatorio = true,
            Fila = 3,
            Columna = 0,
            VisibleEnGrid = false
          )
        ]
        public string Contacto { get; set; }

        //------------------------------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Activo",
           Ayuda = "indica si el centro administrativo está activo está activo",
           VisibleEnGrid = false,
           Obligatorio = false,
           Fila = 5,
           Columna = 0,
           TipoDeControl = enumTipoControl.Check,
           css = enumCssControles.ControlApilado,
           ValorPorDefecto = true,
           VisibleAlCrear = false,
           EditableAlEditar = true
           )]
        public bool Activa { get; set; }

        [IUPropiedad(Visible = false)]
        public string Expresion { get; set; }

    }
}
