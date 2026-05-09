using System.Collections.Generic;
using Utilidades;

namespace ModeloDeDto.Entorno
{

    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, OpcionDeCrear = false, OpcionDeEditar =true, OpcionDeBorrar = false)]
    public class VistaMvcDto : ElementoDto
    {
        [IUPropiedad(
            Etiqueta = "Vista",
            Ayuda = "Nombre de la vista",
            Tipo = typeof(string),
            Fila = 1,
            Columna = 0,
            Ordenar = true,
            AutoSpan = true,
            EditableAlEditar = false
            )
        ]
        public string Nombre { get; set; }

        //--------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Controlador",
            Ayuda = "Nombre del controlador",
            Tipo = typeof(string),
            Fila = 2,
            Columna = 0,
            Ordenar = true,
            EditableAlEditar = false
            )
        ]
        public string Controlador { get; set; }

        //----------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Accion",
            Ayuda = "Nombre de la acción",
            Tipo = typeof(string),
            Fila = 2,
            Columna = 1,
            Ordenar = true,
            EditableAlEditar = false
            )
        ]
        public string Accion { get; set; }

        //-------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Parametros",
            Ayuda = "Lista de parámetros de entrada",
            Tipo = typeof(string),
            Fila = 3,
            Columna = 0,
            Ordenar = true,
            Obligatorio = false,
            VisibleEnGrid =false
            )
        ]
        public string Parametros { get; set; }

        //------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Mostrar en modal",
            Ayuda = "indica si se ha de mostrar en modal la creación o edición",
            VisibleEnEdicion = true,
            Obligatorio = true,
            TipoDeControl = enumTipoControl.Check,
            ValorPorDefecto = false,
            Tipo = typeof(bool),
            Fila = 3,
            Columna = 1
            )
        ]
        public string MostrarEnModal { get; set; }

        //------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Permiso",
            Ayuda = "Permiso de acceso",
            EditableAlEditar = false,
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Permiso),
            PropiedadRestrictora = nameof(IdPermiso),
            Controlador = nameof(enumControladoresSeguridad.Permisos),
            VistaDondeNavegar = enumVistasSeguridad.CrudPermiso,
            Tipo = typeof(string),
            Fila = 4,
            Columna = 0,
            Obligatorio = false,
            VisibleEnGrid = false,
            VisibleAlEditar = true,
            VisibleAlConsultar = true,
            VisibleAlCrear = false
            )
        ]
        public string Permiso { get; set; }
        [IUPropiedad(Etiqueta = "Permiso", Ayuda = "Permiso de acceso", Visible = false)]
        public string IdPermiso { get; set; }


        //----------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Elemento Dto",
            Ayuda = "Espacio de nombre y clase que se muestra",
            Tipo = typeof(string),
            Ordenar = true,
            Fila = 4,
            Columna = 1,
            VisibleEnGrid =true,
            EditableAlEditar = false
            )
        ]
        public string ElementoDto { get; set; }

        [IUPropiedad(Visible = false)]
        public List<MenuDto> Menus { get; set; }
    }
}
