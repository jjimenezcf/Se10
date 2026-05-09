using Utilidades;
using ServicioDeDatos.Contabilidad;
using System;

namespace ModeloDeDto.Terceros
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class TarjetaDeMiSociedadDto : EsUnDetalleDto, IAuditadoDto
    {

        //-------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Clase de Tarjeta",
            Ayuda = "seleccione la clase de tarjeta",
            TipoDeControl = enumTipoControl.Enumerado,
            Tipo = typeof(enumClaseDeTarjeta),
            GuardarEn = nameof(Clase),
            Fila = 0,
            Columna = 1,
            EditableAlEditar = false
          )
        ]
        public string Clase { get; set; }

        //-------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Modo de Tarjeta",
            Ayuda = "seleccione el modo de pago de la tarjeta",
            TipoDeControl = enumTipoControl.Enumerado,
            Tipo = typeof(enumModoTarjeta),
            GuardarEn = nameof(Modo),
            Fila = 0,
            Columna = 2,
            EditableAlEditar = false
          )
        ]
        public string Modo { get; set; }

        //-------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Número",
            Ayuda = "Número de tarjeta",
            Fila = 0,
            Columna = 3,
            VisibleAlCrear = true,
            EditableAlEditar = false,
            AutoSpan = true,
            LongitudMaxima = 20,
            Obligatorio = true
          )
        ]
        public string Numero { get; set; }

        //----------------------------------------------
        [IUPropiedad(Etiqueta = "Id la cuenta de cargo", Visible = false)]
        public int IdCuentaDeCargo { get; set; }

        [IUPropiedad(
            Etiqueta = "Cargar en",
            Ayuda = "Seleccione la cuenta bancaria de cargo de la tarjeta",
            TipoDeControl = enumTipoControl.ListaDeElemento,
            SeleccionarDe = typeof(CuentaDeMiSociedadDto),
            Controlador = nameof(enumControladoresTerceros.CuentasDeMiSociedad),
            MostrarExpresion = nameof(CuentaDeMiSociedadDto.Expresion),
            GuardarEn = nameof(IdCuentaDeCargo),
            RestringidoPorControl = nameof(IdElemento),
            EditableAlEditar = false,
            CargarBajoDemanda = true,
            VisibleEnGrid = true,
            Fila = 1,
            Columna = 0,
            ColSpan = 3,
            Obligatorio = true
            )
        ]
        public string CuentaDeCargo { get; set; }

        //----------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Alias",
            Ayuda = "Alias de la tarjeta",
            Fila = 1,
            Columna =3,
            VisibleAlCrear = true,
            EditableAlEditar = true,
            AutoSpan = true,
            LongitudMaxima = 250,
            Obligatorio = true
            )
        ]
        public string Alias { get; set; }

        //------------------------------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Activa",
           Ayuda = "indica si la tarjeta de la sociedad está de activa",
           VisibleEnGrid = false,
           Obligatorio = false,
           Fila = 3,
           Columna = 0,
           TipoDeControl = enumTipoControl.Check,
           css = enumCssControles.ControlApilado,
           ValorPorDefecto = true,
           VisibleAlCrear = false,
           EditableAlEditar = true,
           OnChange = "javascript:" + nameof(enumNameSpaceTs.Terceros) + "." + nameof(enumFunctionTs.Sociedad_AlCambiar_TarjetaActiva) + "(this)"
           )]
        public bool Activa { get; set; }


        //----------------------------------------------
        [IUPropiedad(Visible = false, PorAnchoMnt = 20, Etiqueta = "Creado por")]
        public string Creador { get; set; }

        //----------------------------------------------
        [IUPropiedad(Visible = false, Etiqueta = "Id del creador")]
        public int IdCreador { get; set; }

        //----------------------------------------------
        [IUPropiedad(Visible = false, PorAnchoMnt = 20, Etiqueta = "Modificado por")]
        public string Modificador { get; set; }

        //----------------------------------------------
        [IUPropiedad(Visible = false, Etiqueta = "Id del modificador")]
        public int? IdModificador { get; set; }

        //----------------------------------------------
        [IUPropiedad(Visible = false, Etiqueta = "creada el")]
        public DateTime CreadoEl { get; set; }

        //----------------------------------------------
        [IUPropiedad(Visible = false, Etiqueta = "modificada el")]
        public DateTime? ModificadoEl { get; set; }

        //----------------------------------------------
        [IUPropiedad(Visible = false)]
        public string Expresion { get; set; }
    }
}
