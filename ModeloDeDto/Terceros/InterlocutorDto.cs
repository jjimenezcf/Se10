using Utilidades;

namespace ModeloDeDto.Terceros
{
    public static class ltrInterlocutor
    {
        public static readonly string Interlocutor = nameof(Interlocutor);
        public const string BuscarPorPersona = nameof(BuscarPorPersona);
        public const string BuscarPorContacto = nameof(BuscarPorContacto);
        public const string BuscarPorContactoCliente = nameof(BuscarPorContactoCliente);
        public const string ParaExpediente = nameof(ParaExpediente);
        public const string ParaInfante = nameof(ParaInfante);
        public const string NombreModificado = nameof(NombreModificado);
    }

    public class IndInterlocutor
    {
        public const string TercerosJudiciales = nameof(TercerosJudiciales);
    }



    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, MostrarExpresion = nameof(Expresion), OpcionDeCrear = false, OpcionDeBorrar = false)]
    public class InterlocutorDto : ElmentoAuditadoDto, IUsaNombreDto, IUsaBajaDto
    {
        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = nameof(ltrInterlocutor.Interlocutor),
            EditableAlEditar = false,
            Tipo = typeof(string),
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            PropiedadRestrictora = nameof(Id),
            MostrarExpresion = nameof(Expresion),
            Controlador = nameof(enumControladoresTerceros.Interlocutores),
            VistaDondeNavegar = enumVistasTerceros.CrudTerceros,
            Fila = 0,
            Columna = 0,
            Ordenar = true,
            PorAnchoMnt = 50,
            AutoSpan = true
          )
        ]
        public new string Expresion { get; set; }

        [IUPropiedad(Visible = false)]
        public string Nombre { get; set; }

        //----------------------------------------------
        [IUPropiedad(Etiqueta = "Id de la persona", Oculto = true)]
        public int? IdPersona { get; set; }

        //----------------------------------------------
        [IUPropiedad(Etiqueta = "Id de la sociedad", Oculto = true)]
        public int? IdSociedad { get; set; }

        //----------------------------------------------
        [IUPropiedad(Etiqueta = "Id del contacto", Oculto = true)]
        public int? IdContacto { get; set; }

        //----------------------------------------------
        [IUPropiedad(
           Etiqueta = "eMail",
           Ayuda = "eMail del interlocutor",
           Fila = 1,
           Columna = 0,
           Ordenar = true,
           LongitudMaxima = 50
          )
        ]
        public string eMail { get; set; }

        //----------------------------------------------
        [IUPropiedad(
           Etiqueta = "Teléfono",
           Ayuda = "teléfono del interlocutor",
           Fila = 1,
           Columna = 1,
           LongitudMaxima = 15
          )
        ]
        public string Telefono { get; set; }

        //----------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Baja",
            Ayuda = "indica si el interlocutor está de baja",
            Fila = 3,
            Columna = 0,
            TipoDeControl = enumTipoControl.Check,
            css = enumCssControles.ControlApilado,
            ValorPorDefecto = false,
            EditableAlEditar = false,
            VisibleEnGrid = false
            )
        ]
        public bool Baja { get; set; }


        //----------------------------------------------------------------
        [IUPropiedad(Visible = false)]
        public string DireccionDeContacto { get; set; }
        //----------------------------------------------------------------
        [IUPropiedad(Visible = false)]
        public string NIF { get; set; }
    }
}
