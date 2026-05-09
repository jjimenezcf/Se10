using System;
using Utilidades;

namespace ModeloDeDto.Negocio
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class HitoDto : ElementoDto, IUsaNegocioDto
    {
        //--------------------------------------------
        [IUPropiedad(
            Etiqueta = "Negocio",
            Ayuda = "Negocio del elemento",
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
            Ayuda = "historia del elemento",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Elemento),
            Fila = 0,
            Columna = 1,
            EditableAlCrear = false,
            EditableAlEditar = false,
            VisibleEnGrid = false
            )
        ]
        public int IdElemento { get; set; }

        [IUPropiedad(Visible = false)]
        public string Elemento { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Estado",
           Ayuda = "Nombre del estado",
           TipoDeControl = enumTipoControl.RestrictorDeEdicion,
           AutoSpan = true,
           Ordenar = false,
           Fila = 1,
           Columna = 0)
        ]
        public int IdEstado { get; set; }

        [IUPropiedad(Visible = false)]
        public string Estado { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Fecha",
           Ayuda = "Fecha de toma del estado",
           TipoDeControl = enumTipoControl.SelectorDeFechaHora,
           Fila = 1,
           Columna = 1,
           VisibleEnGrid = true,
           EditableAlCrear = false,
           EditableAlEditar = false
           )
        ]
        public DateTime Fecha { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Usuario",
           Ayuda = "usuario que ha genera la historia",
           TipoDeControl = enumTipoControl.RestrictorDeEdicion,
           AutoSpan = true,
           Ordenar = false,
           Fila = 2,
           Columna = 0)
        ]
        public int IdUsuario { get; set; }

        [IUPropiedad(Visible = false)]
        public string Usuario { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Transición",
           Ayuda = "transición ejecutada",
           TipoDeControl = enumTipoControl.RestrictorDeEdicion,
           AutoSpan = true,
           Ordenar = false,
           Fila = 2,
           Columna = 1)
        ]
        public int IdTransicion { get; set; }

        [IUPropiedad(Visible = false)]
        public string Transicion { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Observacion",
           Ayuda = "Observación anotada",
           TipoDeControl = enumTipoControl.RestrictorDeEdicion,
           AutoSpan = true,
           Ordenar = false,
           Fila = 3,
           Columna = 0)
        ]
        public int IdObservacion { get; set; }

        [IUPropiedad(Visible = false)]
        public string Observacion { get; set; }

    }

}
