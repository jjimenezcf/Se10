using ModeloDeDto.Entorno;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using Utilidades;

namespace ModeloDeDto.Negocio
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class AccionesDeTrnDto : ElementoDto, IRelacionDto, IUsaNegocioDto
    {
        public static string ExpresionElemento = nameof(Accion);

        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Negocio",
            Ayuda = "acciones de transiciones del negocio",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Negocio),
            Fila = 0,
            Columna = 0,
            EditableAlEditar = false,
            VisibleEnGrid = false
            )
        ]
        public int IdNegocio { get; set; }

        [IUPropiedad(Visible = false)]
        public string Negocio { get; set; }

        //----------------------------------------------------------
        [IUPropiedad(Etiqueta = "Transición",
            Ayuda = "acción de una transición",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Transicion),
            Fila = 0,
            Columna = 1,
            VisibleEnGrid = false,
            AutoSpan = true
            )
        ]
        public int IdTransicion { get; set; }

        [IUPropiedad(
            Etiqueta = "Transición",
            Visible = false
            )
        ]
        public string Transicion { get; set; }

        //----------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Id del acción",
            Visible = false
            )
        ]
        public int idAccion { get; set; }


        [IUPropiedad(
            Etiqueta = "Accion",
            Ayuda = "Indique la acción",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(AccionDto),
            GuardarEn = nameof(idAccion),
            Controlador = nameof(enumControladoresEntorno.Acciones),
            VistaDondeNavegar = enumVistasEntorno.CrudDeAcciones,
            RestringidoPorControl = nameof(IdNegocio),
            PropiedadRestrictora = nameof(IdTransicion),
            BuscarPor = nameof(AccionDtm.Nombre),
            CriterioDeBusqueda = enumCriteriosDeFiltrado.contiene,
            LongitudMinimaParaBuscar = 1,
            Fila = 2,
            Columna = 2,
            Ordenar = true,
            AutoSpan = true
            )
        ]
        public string Accion { get; set; }

        //------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Momento",
            Ayuda = "indique el momento de ejecución",
            TipoDeControl = enumTipoControl.Enumerado,
            Tipo = typeof(enumMomentoDeEjecucion),
            GuardarEn = nameof(Momento),
            Fila = 2,
            Columna = 0,
            Obligatorio = false,
            VisibleEnEdicion = true
          )
        ]
        public string Momento { get; set; }
        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Orden",
           Ayuda = "Orden de ejecución",
           TipoDeControl = enumTipoControl.Editor,
           Alineada = enumAliniacion.derecha,
           PorAnchoMnt = 10,
           Ordenar = true,
           Fila = 2,
           Columna = 1)
        ]
        public int Orden { get; set; }

        //------------------------------------------------------------------------------------------------
        [IUPropiedad(
          Etiqueta = "Descripción",
          Ayuda = "Describa porque de la acción",
          Tipo = typeof(string),
          TipoDeControl = enumTipoControl.AreaDeTexto,
          Fila = 1,
          Columna = 0,
          VisibleEnGrid = false,
          AutoSpan = true,
          NumeroDeFilas =2
          )
        ]
        public string Descripcion { get; set; }


        //------------------------------------------------------------------------------------------------
        [IUPropiedad(
          Etiqueta = "Parámetros",
          Ayuda = "Parámetros de ejecución",
          Tipo = typeof(string),
          TipoDeControl = enumTipoControl.AreaDeTexto,
          Fila = 3,
          Columna = 0,
          VisibleEnGrid = false,
          Obligatorio = false,
          AutoSpan = true
          )
        ]
        public string Parametros { get; set; }

        //-----------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Está activa",
            Ayuda = "indica si la acción está activa",
            VisibleEnGrid = true,
            Fila = 4,
            Columna = 0,
            TipoDeControl = enumTipoControl.Check,
            ValorPorDefecto = true,
            css = enumCssControles.ControlApilado
            )
        ]
        public bool Activo { get; set; }

    }
}
