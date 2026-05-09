using ModeloDeDto.Entorno;
using Utilidades;

namespace ModeloDeDto.Negocio
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, OpcionDeCrear = false, OpcionDeBorrar = false)]
    public class ParametroDeUsuarioDto : ElementoDto
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
            VisibleEnGrid = false
            )
        ]
        public int IdNegocio { get; set; }

        //--------------------------------------------
        [IUPropiedad(Visible = false)]
        public string Negocio { get; set; }

        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Id del usuario",
            Visible = false
            )
        ]
        public int IdUsuario { get; set; }

        [IUPropiedad(
            Etiqueta = "Usuario",
            Ayuda = "Usuario",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(UsuarioDto),
            GuardarEn = nameof(IdUsuario),
            MostrarExpresion = UsuarioDto.ExpresionElemento,
            Controlador = nameof(enumControladoresEntorno.Usuarios),
            VistaDondeNavegar = enumVistasEntorno.CrudUsuario,
            Fila = 0,
            Columna = 1,
            VisibleEnGrid = false,
            EditableAlEditar = true,
            Obligatorio = false,
            LongitudMinimaParaBuscar = 1,
            AutoSpan = false
            )
        ]
        public string Responsable { get; set; }
        //--------------------------------------------
        [IUPropiedad(
          Etiqueta = "Parametro",
          Ayuda = "Indique el nombre del parámetro",
          Tipo = typeof(string),
          Fila = 0,
          Columna = 2,
          Ordenar = true,
          EditableAlEditar = false
          )
        ]
        public string Nombre { get; set; }

        //--------------------------------------------
        [IUPropiedad(
          Etiqueta = "Valor",
          Ayuda = "Asigne un valor al parámetro",
          TipoDeControl = enumTipoControl.AreaDeTexto,
          Tipo = typeof(string),
          Fila = 2,
          Columna = 0,
          NumeroDeFilas = 5,
          LongitudMaxima = 2000,
          AutoSpan = true
          )
        ]
        public string Valor { get; set; }
    }
}
