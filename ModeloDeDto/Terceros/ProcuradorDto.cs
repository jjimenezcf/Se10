using Utilidades;

namespace ModeloDeDto.Terceros
{
    public static class ltrProcurador
    {
        public static readonly string Procurador = nameof(Procurador);
    }

    [IUDto(AnchoEtiqueta = 20
         , AnchoSeparador = 5
         , MostrarExpresion = nameof(Expresion)
         , OpcionDeCrear = false)]
    public class ProcuradorDto : ElmentoAuditadoDto, IUsaNombreDto, IUsaBajaDto
    {
        //----------------------------------------------
        [IUPropiedad(
            Etiqueta = nameof(ltrProcurador.Procurador),
            EditableAlEditar = false,
            Tipo = typeof(string),
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            PropiedadRestrictora = nameof(ProcuradorDto.IdInterlocutor),
            MostrarExpresion = nameof(Expresion),
            Controlador = nameof(enumControladoresTerceros.Interlocutores),
            VistaDondeNavegar = enumVistasTerceros.CrudInterlocutores,
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
        [IUPropiedad(Etiqueta = "Id del interlocutor", Visible = false)]
        public int IdInterlocutor { get; set; }

        //----------------------------------------------
        [IUPropiedad(Etiqueta = "Id de la persona", Visible = false)]
        public int IdPersona { get; set; }

        //----------------------------------------------
        [IUPropiedad(Etiqueta = "Id de la sociedad", Visible = false)]
        public int? IdSociedad { get; set; }

        //----------------------------------------------
        [IUPropiedad(Etiqueta = "Id del contacto", Visible = false)]
        public int? IdContacto { get; set; }

        //----------------------------------------------
        [IUPropiedad(
           Etiqueta = "eMail",
           Ayuda = "eMail del procurador",
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
           Ayuda = "teléfono del procurador",
           Fila = 1,
           Columna = 1,
           LongitudMaxima = 15
          )
        ]
        public string Telefono { get; set; }

        //----------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Baja",
            Ayuda = "indica si el procurador está de baja",
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
