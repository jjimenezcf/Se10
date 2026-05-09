using ModeloDeDto.Entorno;
using ServicioDeDatos.Entorno;
using ServicioDeDatos;
using Utilidades;
using ServicioDeDatos.Negocio;

namespace ModeloDeDto.Negocio
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, OpcionDeCrear = false, OpcionDeBorrar = false, OpcionDeEditar = true)]
    public class AccionDeNegocioDto : ElementoDto
    {
        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Negocio",
            Ayuda = "negocio del parámetro",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Negocio),
            Fila = 0,
            Columna = 0,
            EditableAlCrear = false,
            EditableAlEditar = false,
            VisibleEnGrid = false,
            AutoSpan =true
            )
        ]
        public int IdNegocioAfectado { get; set; }

        [IUPropiedad(Visible = false)]
        public string Negocio { get; set; }

        //----------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Id del acción",
            Visible = false
            )
        ]
        public int IdAccion { get; set; }


        [IUPropiedad(
            Etiqueta = "Acción",
            Ayuda = "Indique el acción",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(AccionDto),
            GuardarEn = nameof(IdAccion),
            Controlador = nameof(enumControladoresEntorno.Acciones),
            BuscarPor = nameof(AccionDtm.Nombre),
            CriterioDeBusqueda = enumCriteriosDeFiltrado.contiene,
            LongitudMinimaParaBuscar = 1,
            VistaDondeNavegar = enumVistasEntorno.CrudDeAcciones,
            Fila = 1,
            Columna = 0,
            Ordenar = true,
            AutoSpan = true,
            EditableAlCrear = true,
            EditableAlEditar = false,
            PorAnchoMnt = 50
            )
        ]
        public string Accion { get; set; }

        //-------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Momento de ejecución",
            Ayuda = "indique cúando se ejecuta la acción",
            TipoDeControl = enumTipoControl.Enumerado,
            Tipo = typeof(enumMomentoDeAccion),
            GuardarEn = nameof(Momento),
            Fila = 1,
            Columna = 1,
            Posicion = 0,
            EditableAlCrear = true,
            EditableAlEditar = false
          )
        ]
        public string Momento { get; set; }

        //--------------------------------------------
        [IUPropiedad(
          Etiqueta = "Orden",
          Ayuda = "orden",
          Tipo = typeof(int),
            Fila = 1,
            Columna = 1,
            Posicion = 1
          )
        ]
        public int Orden { get; set; }

        //--------------------------------------------
        [IUPropiedad(
          Etiqueta = "Parametros",
          Ayuda = "Parámetros de ejecución",
          TipoDeControl = enumTipoControl.AreaDeTexto,
          Tipo = typeof(string),
          Fila = 2,
          Columna = 0,
          NumeroDeFilas = 5,
          LongitudMaxima = 2000,
          AutoSpan = true
          )
        ]
        public string Parametros { get; set; }

        //--------------------------------------------
        [IUPropiedad(
          Etiqueta = "Descripcion",
          Ayuda = "Descripción de la acción",
          TipoDeControl = enumTipoControl.AreaDeTexto,
          Tipo = typeof(string),
          Fila = 3,
          Columna = 0,
          NumeroDeFilas = 3,
          LongitudMaxima = 2000,
          AutoSpan = true
          )
        ]
        public string Descripcion { get; set; }

        //----------------------------------------------------------------

        [IUPropiedad(
            Etiqueta = "Activa",
            Ayuda = "indica si la acción está activa",
            VisibleEnGrid = false,
            Obligatorio = true,
            Fila = 4,
            Columna = 0,
            TipoDeControl = enumTipoControl.Check,
            css = enumCssControles.ControlApilado,
            VisibleAlCrear = false,
            VisibleAlEditar = true,
            EditableAlEditar = true,
            ValorPorDefecto = true
            )
        ]
        public bool Activo { get; set; }
    }
}
