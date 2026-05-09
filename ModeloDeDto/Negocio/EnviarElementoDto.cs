using ModeloDeDto.Entorno;
using Utilidades;

namespace ModeloDeDto.Negocio
{
    [IUDto(MostrarExpresion = nameof(IUsaNombreDto.Nombre))]
    public class EnviarElementoDto
    {
        //----------------------------------------------------------
        [IUPropiedad(Etiqueta = "",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Elemento),
            Fila = 0,
            Columna = 0,
            AutoSpan = false
            )
        ]
        public int IdElemento { get; set; }

        [IUPropiedad(Visible = false)]
        public string Elemento { get; set; }

        //----------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Id del usuario",
            Visible = false
            )
        ]
        public int IdUsuario { get; set; }

        [IUPropiedad(
            Etiqueta = "Usuario",
            Ayuda = "Indique el usuario",
            TipoDeControl = enumTipoControl.ListaDinamica,
            SeleccionarDe = typeof(UsuarioDto),
            GuardarEn = nameof(IdUsuario),
            Controlador = nameof(enumControladoresEntorno.Usuarios),
            VistaDondeNavegar = enumVistasEntorno.CrudUsuario,
            BuscarPor = UsuariosPor.NombreCompleto,
            Fila = 1,
            Columna = 0,
            Ordenar = true
            )
        ]
        public string Usuario { get; set; }

        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Asunto",
            Ayuda = "Asunto del correo",
            Tipo = typeof(string),
            Fila = 2,
            Columna = 0,
            Obligatorio = true,
            LongitudMaxima = 250,
            AutoSpan = true
          )
        ]
        public string Asunto { get; set; }

        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(
           Etiqueta = "Cuerpo",
           Ayuda = "Cuerpo del mensaje",
           TipoDeControl = enumTipoControl.AreaDeTexto,
           Obligatorio = true,
           NumeroDeFilas = 5,
           Fila = 3,
           Columna = 0,
           LongitudMaxima = 1999,
           AutoSpan = true
          )
        ]
        public string Cuerpo { get; set; }


        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Permitir modificar",
           Ayuda = "Indica si puede modificar ademas de consultar",
           TipoDeControl = enumTipoControl.Check,
           Obligatorio = false,
           css = enumCssControles.CheckEnLinea,
           ValorPorDefecto = true,
           Fila = 4, Columna = 0, Posicion = 0)
        ]
        public bool OtorgarGestor { get; set; }

    }
}
