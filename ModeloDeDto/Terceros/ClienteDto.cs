using ModeloDeDto.Contabilidad;
using ServicioDeDatos.Terceros;
using Utilidades;

namespace ModeloDeDto.Terceros
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, MostrarExpresion = nameof(Expresion), OpcionDeCrear = false, OpcionDeBorrar = false)]
    public class ClienteDto : ElmentoAuditadoDto, IUsaNombreDto, IUsaBajaDto
    {
        //----------------------------------------------
        [IUPropiedad(Etiqueta = "Id del interlocutor", Visible = false)]
        public int IdInterlocutor { get; set; }

        [IUPropiedad(
            Etiqueta = nameof(ltrCliente.Cliente),
            EditableAlEditar = false,
            Tipo = typeof(string),
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            PropiedadRestrictora = nameof(ClienteDto.IdInterlocutor),
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
        [IUPropiedad(
           Etiqueta = "eMail",
           Ayuda = "eMail del cliente",
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
           Ayuda = "teléfono del cliente",
           Fila = 1,
           Columna = 1,
           LongitudMaxima = 15
          )
        ]
        public string Telefono { get; set; }


        //----------------------------------------------
        [IUPropiedad(Etiqueta = "Id la cuenta contable", Visible = false)]
        public int IdCuenta { get; set; }

        [IUPropiedad(
            Etiqueta = "Cuenta",
            Ayuda = "Seleccione la cuenta contable",
            TipoDeControl = enumTipoControl.ListaDeElemento,
            SeleccionarDe = typeof(CuentaDto),
            Controlador = nameof(enumControladoresContables.Cuentas),
            GuardarEn = nameof(IdCuenta),
            VisibleEnGrid = false,
            Fila = 1,
            Columna = 2
            )
        ]
        public string Cuenta { get; set; }


        //----------------------------------------------
        [IUPropiedad(
           Etiqueta = "Ext.Cta",
           Ayuda = "extensión contable",
           Fila = 1,
           Columna = 2,
           Posicion = 1,
           Ordenar = true,
           VisibleAlCrear = false,
           EditableAlEditar = false,
           VisibleEnGrid = false,
           LongitudMaxima = 4
          )
        ]
        public string CodigoContable { get; set; }

        //----------------------------------------------
        [IUPropiedad(
           Etiqueta = "VAT",
           Ayuda = "VAT Number",
           Fila = 1,
           Columna = 3,
           LongitudMaxima = 25,
           Obligatorio = false
          )
        ]
        public string VAT { get; set; }

        //----------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Baja",
            Ayuda = "indica si el cliente está de baja",
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

        //----------------------------------------------------------------
        [IUPropiedad(Visible = false)]
        public string DireccionFiscal { get; set; }
        //----------------------------------------------------------------
        [IUPropiedad(Visible = false)]
        public string NIF { get; set; }
        //----------------------------------------------------------------
        [IUPropiedad(Visible = false)]
        public string RazonSocial { get; set; }
        //----------------------------------------------------------------
        [IUPropiedad(Visible = false)]
        public string TipoDeTercero { get; set; }
        //----------------------------------------------------------------
        [IUPropiedad(Visible = false)]
        public bool EsIntraComunitario { get; set; }
        //----------------------------------------------------------------
        [IUPropiedad(Visible = false)]
        public bool EsExtraComunitario { get; set; }
    }
}
