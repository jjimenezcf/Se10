using System;
using ServicioDeDatos;
using Utilidades;
using static ServicioDeDatos.Elemento.Enumerados;

namespace ModeloDeDto.Entorno
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, MostrarExpresion = "Nombre", OpcionDeCrear = false)]
    public class EventoDeAgendaDto : ElmentoAuditadoDto
    {

        ////------------------------------------------------------------------------
        //[IUPropiedad(Etiqueta = "Id de la agenda", Visible = false)]
        //public int IdNegocio { get; set; }

        //------------------------------------------------------------------------
        [IUPropiedad(Etiqueta = "Id del elemento", Visible = false)]
        public int IdElemento { get; set; }

        //------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = nameof(Agenda),
            Ayuda = "Seleccione la agenda",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(AgendaDto),
            GuardarEn = nameof(IdAgenda),
            Controlador = nameof(enumControladoresEntorno.Agendas),
            VistaDondeNavegar = enumVistasEntorno.CrudAgendas,
            CriterioDeBusqueda = enumCriteriosDeFiltrado.contiene,
            LongitudMinimaParaBuscar = 1,
            Fila = 0,
            Columna = 0,
            Obligatorio = true,
            Ordenar = true,
            EditableAlEditar = false,
            PorAnchoMnt = 30,
            PosicionEnGrid = 4,
            VisibleEnGrid = false
            )
        ]
        public string Agenda { get; set; }

        [IUPropiedad(Etiqueta = "Id de la agenda", Visible = false)]
        public int IdAgenda { get; set; }



        //------------------------------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Inicio",
           Ayuda = "Fecha de inicio",
           TipoDeControl = enumTipoControl.SelectorDeFechaHora,
           Formato = enumFormato.FechaTiempo,
           SelectorHasta = nameof(Fin) + ":0:1",
           Fila = 1,
           Columna = 0,
           Ordenar = true,
           VisibleEnGrid = true,
           EditableAlCrear = true,
           EditableAlEditar = true,
           Obligatorio = true,
           PosicionEnGrid = 0
           )
        ]
        public DateTime Inicio { get; set; }

        //----------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Referencia",
            VisibleEnGrid = true,
            Obligatorio = false,
            VisibleAlCrear = false,
            VisibleAlEditar = false,
            PosicionEnGrid = 1,
            TipoDeControl = enumTipoControl.Referencia,
            AccionRef = "javascript: " + nameof(enumNameSpaceTs.Entorno) + "." + nameof(enumFunctionTs.MiCalendario_IrAlElemento) + "(numeroDeFila)"
          )
        ]
        public string Referencia { get; set; }


        //------------------------------------------------------------------------
        [IUPropiedad(
          Etiqueta = "Evento",
          Ayuda = "Indique el nombre de la evento",
          Tipo = typeof(string),
          Fila = 0,
          Columna = 1,
          PorAnchoMnt = 30,
          ColSpan = 2,
          PosicionEnGrid = 2
          )
        ]
        public string Nombre { get; set; }

        //------------------------------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Fin",
           Ayuda = "Fecha de fin",
           TipoDeControl = enumTipoControl.SelectorDeFechaHora,
           Formato = enumFormato.FechaTiempo,
           Fila = 1,
           Columna = 1,
           Ordenar = true,
           VisibleEnGrid = true,
           EditableAlCrear = true,
           EditableAlEditar = true,
           Obligatorio = true,
           PosicionEnGrid = 3
           )
        ]
        public DateTime Fin { get; set; }

        //----------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Evento de día",
            Ayuda = "Indicar si es un evento diario, o ocupa una franja horaria del calendario",
            Fila = 2,
            Columna = 0,
            Posicion = 0,
            TipoDeControl = enumTipoControl.Check,
            css = enumCssControles.ControlApilado,
            ValorPorDefecto = false,
            VisibleEnGrid = false,
            OnChange = "javascript:" + nameof(enumNameSpaceTs.ApiDeAgenda) + "." + nameof(enumFunctionTs.Agenda_AlCambiar_EventoDeDia) + "(this)"
            )
        ]
        public bool EventoDeDia { get; set; }

        //----------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Evento del sistema",
            Ayuda = "Indicar si el evento es creado por el sistema",
            Fila = 2,
            Columna = 0,
            Posicion = 1,
            TipoDeControl = enumTipoControl.Check,
            css = enumCssControles.ControlApilado,
            VisibleAlCrear = false,
            EditableAlEditar = false
            )
        ]
        public bool EsDelSistema { get; set; }

        //--------------------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Avisar antes de ",
           TipoDeControl = enumTipoControl.Editor,
           Tipo = typeof(int),
           Fila = 2,
           Columna = 1,
           Posicion = 0,
           AutoSpan = false,
           AnchoMaximo = "200px",
           Obligatorio = false
           )
        ]
        public int? AvisarAntesDe { get; set; }

        //------------------------------------------------
        [IUPropiedad(
            Etiqueta = "medido en",
            Ayuda = "Indique como se mide el periodo de duración",
            TipoDeControl = enumTipoControl.Enumerado,
            Tipo = typeof(enumDurabilidad),
            GuardarEn = nameof(MedidoEn),
            Fila = 2,
            Columna = 1,
            Posicion = 1,
            AutoSpan = false,
            AnchoMaximo = "300px",
            Obligatorio = false
          )
        ]
        public enumDurabilidad? MedidoEn { get; set; }


        //----------------------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Descripción",
           Ayuda = "Descripción del evento",
           TipoDeControl = enumTipoControl.AreaDeTexto,
           VisibleEnGrid = false,
           Obligatorio = false,
           NumeroDeFilas = 3,
           Fila = 3,
           Columna = 0,
           AutoSpan = true
          )
        ]
        public string Descripcion { get; set; }

        //----------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Navegar al elemento",
            Ayuda = "navega al elemento asociado al evento",
            VisibleEnGrid = false,
            Obligatorio = false,
            Fila = 4,
            Columna = 0,
            EnConsultaOcultar = false,
            TipoDeControl = enumTipoControl.Referencia,
            css = enumCssControles.ControlApilado,
            VisibleAlCrear = false            
          )
        ]
        public Uri Url { get; set; }

        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(
           TamanoFijo = "10em"
         , Etiqueta = "Acción"
         , VisibleEnEdicion = false
         , TipoDeControl = enumTipoControl.Referencia
         , AccionRef = "javascript: " + nameof(enumNameSpaceTs.ApiDeAgenda) + "." + nameof(enumFunctionTs.Agenda_EjecutarAccionAsociada) + "(numeroDeFila)"
         , Alineada = enumAliniacion.derecha)]
        public string Accion { get; set; }

        //----------------------------------------------------------------
        [IUPropiedad(
            VisibleEnGrid = false,
            Obligatorio = false,
            Fila = 5,
            Columna = 0,
            VisibleAlCrear = false,
            EditableAlEditar = false,
            TipoDeControl = enumTipoControl.Editor,
            AutoSpan = true
          )
        ]
        public string Elemento { get; set; }
    }
}
