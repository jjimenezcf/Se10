using System;
using Utilidades;

namespace ModeloDeDto.Negocio
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, OpcionDeBorrar = false, OpcionDeCrear = false)]
    public class TrazaDto : ElementoDto
    {
        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Negocio",
            Ayuda = "Trazas del negocio",
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

        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Elemento",
            Ayuda = "Traza del elemento",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Elemento),
            Fila = 0,
            Columna = 1,
            EditableAlEditar = false,
            VisibleEnGrid = false
            )
        ]
        public int IdElemento { get; set; }

        [IUPropiedad(Visible = false)]
        public string Elemento { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Traza",
           Ayuda = "indique la traza",
           EditableAlEditar = false,
           EditableAlCrear = false,
           TipoDeControl = enumTipoControl.Editor,
           AutoSpan = true,
           Fila = 1,
           Columna = 0)
        ]
        public string Nombre { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Detalle",
           Ayuda = "Detalle",
           TipoDeControl = enumTipoControl.AreaDeTexto,
            EditableAlEditar = false,
           Fila = 2,
           Columna = 0,
           AutoSpan = true
           )
        ]
        public string Descripcion { get; set; }

        //--------------------------------------------
        [IUPropiedad(Visible = false)]
        public int IdCreador { get; set; }

        [IUPropiedad(
           Etiqueta = "Creada por",
           Ayuda = "Creada por",
           Visible = false,
           VisibleEnGrid = true,
           VisibleAlEditar = true,
           EditableAlEditar = false,
           TipoDeControl = enumTipoControl.RestrictorDeEdicion,
           PropiedadRestrictora = nameof(IdCreador),
           MostrarExpresion = nameof(Creador),
           Fila = 3,
           Columna = 0)
        ]
        public string Creador { get; set; }


        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Creada el",
           Ayuda = "Fecha de creación",
           TipoDeControl = enumTipoControl.SelectorDeFechaHora,
           Formato = enumFormato.Fecha,
           Fila = 3,
           Columna = 1,
           VisibleEnGrid = true,
           VisibleAlCrear = false,
           EditableAlCrear = false,
           EditableAlEditar = false
           )
        ]
        public DateTime CreadaEl { get; set; }
    }

}
