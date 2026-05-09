using System;
using Utilidades;

namespace ModeloDeDto.TrabajosSometidos
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5,OpcionDeCrear = false, OpcionDeBorrar = false)]
    public class TrazaDeUnTrabajoDto : ElementoDto
    {
        public static string ExpresionElemento = nameof(TrabajoDeUsuario);

        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Trabajo del usuario",
            Ayuda = "Trabajo sometido de un usuario",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(TrabajoDeUsuario),
            Fila = 0,
            Columna = 0,
            EditableAlCrear = false,
            EditableAlEditar = false,
            VisibleEnGrid = false
            )
        ]
        public int IdTrabajoDeUsuario { get; set; }

        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "trabajo de usuario",
            Visible = false
            )
        ]
        public string TrabajoDeUsuario { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Fecha",
           Ayuda = "Fecha de la traza",
           TipoDeControl = enumTipoControl.SelectorDeFechaHora,
           Fila = 1,
           Columna = 0,
           VisibleEnGrid = true,
           EditableAlCrear = false,
           EditableAlEditar = false,
           Ordenar = true
           )
        ]
        public DateTime Fecha { get; set; }
        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Traza",
           Ayuda = "Traza del trabajo",
           TipoDeControl = enumTipoControl.AreaDeTexto,
           VisibleEnGrid = true,
           Obligatorio = false,
           NumeroDeFilas = 5,
           AutoSpan = true,
           Fila = 2,
           Columna = 0
           )
        ]
        public string Traza { get; set; }

    }
}
