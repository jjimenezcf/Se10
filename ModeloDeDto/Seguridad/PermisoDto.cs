using Utilidades;

namespace ModeloDeDto.Seguridad
{
    public static class PermisoPor
    {
        public static string Nombre = ltrFiltros.Nombre;
        public static string PermisoDeUnRol = nameof(PermisoDeUnRol).ToLower();
    }

    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, OpcionDeCrear = false, OpcionDeBorrar = false)]
    public class PermisoDto : ElementoDto
    {
        [IUPropiedad(
            Etiqueta = "Permiso",
            Ayuda = "De un nombre al permiso",
            Tipo = typeof(string),
            EditableAlCrear = false,
            EditableAlEditar = false,
            Fila = 0,
            Columna = 0,
            Ordenar = true,
            PorAnchoMnt =50,
            AutoSpan = true
            )
        ]
        public string Nombre { get; set; }

        [IUPropiedad(
            Etiqueta = "Id de la clase de permiso",
            Visible = false
            )
        ]

        //Definimos como montar una lista con los valores de la clase de permisos en edición y filtrado
        public int IdClase { get; set; }

        [IUPropiedad(
            Etiqueta = "Clase",
            Ayuda = "Indique clase de permiso",
            TipoDeControl = enumTipoControl.ListaDeElemento,
            SeleccionarDe = typeof(ClasePermisoDto),
            Controlador = nameof(enumControladoresSeguridad.ClaseDePermiso),
            GuardarEn = nameof(IdClase),
            MostrarExpresion = ClasePermisoDto.MostrarExpresion,
            Fila = 1,
            Columna = 0,
            Ordenar = true,
            OrdenarGridPor = "Clase.Nombre",
            PorAnchoMnt = 15
            )
        ]
        public string Clase { get; set; }

        [IUPropiedad(
            Etiqueta = "Id del tipo de permiso",
            Visible = false
            )
        ]

        //montamos una lista para seleccionar el tipo en la edición y en el filtro
        public int IdTipo { get; set; }

        [IUPropiedad(
            Etiqueta = "Tipo",
            Ayuda = "Indique el tipo a aplicar",
            TipoDeControl = enumTipoControl.ListaDeElemento,
            SeleccionarDe = typeof(TipoPermisoDto),
            Controlador = nameof(enumControladoresSeguridad.TipoDePermiso),
            GuardarEn = nameof(IdTipo),
            MostrarExpresion = nameof(Nombre),
            Fila = 1,
            Columna = 1,
            Ordenar = true,
            OrdenarGridPor = "Tipo.Nombre",
            PorAnchoMnt = 15
            )
        ]
        public string Tipo { get; set; }

    }
}
