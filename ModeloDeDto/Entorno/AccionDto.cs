using ServicioDeDatos.Entorno;
using Utilidades;

namespace ModeloDeDto.Entorno
{
    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, OpcionDeCrear = false, OpcionDeBorrar = false)]
    public class AccionDto : ElementoDto
    {
        [IUPropiedad(
          Etiqueta = "Acción",
          Ayuda = "Indique el nombre de la acción",
          Tipo = typeof(string),
          Fila = 0,
          Columna = 1,
          Ordenar = true,
          PorAnchoMnt = 30,
          ColSpan =2
          )
        ]
        public string Nombre { get; set; }

        //------------------------------------------------------------------------------------------------
        [IUPropiedad(
          Etiqueta = "Descripción",
          Ayuda = "Describa el uso de la acción",
          Tipo = typeof(string),
          Fila = 1,
          Columna = 0,
          VisibleEnGrid = false,
          TipoDeControl = enumTipoControl.AreaDeTexto,
          AutoSpan = true
          )
        ]
        public string Descripcion { get; set; }

        //------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Clase de acción",
            Ayuda = "Indique la clase de acción",
            TipoDeControl = enumTipoControl.Enumerado,
            Tipo = typeof(enumClaseDeAccion),
            GuardarEn = nameof(ClaseDeAccion),
            Fila = 0,
            Columna = 0,
            Obligatorio = false,
            VisibleEnEdicion = true,
            TamanoFijo = "10em"
          )
        ]
        public string ClaseDeAccion { get; set; }

        
        //------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Dll",
            Ayuda = "indica el assembly",
            TipoDeControl = enumTipoControl.Editor,
            Tipo = typeof(string),
            Fila = 2,
            Columna = 0,
            Obligatorio = false,
            VisibleEnGrid = false
            )
        ]
        public string Dll { get; set; }
        
        //------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Clase",
            Ayuda = "indica espacio de nombre y clase",
            TipoDeControl = enumTipoControl.Editor,
            Tipo = typeof(string),
            Fila = 2,
            Columna = 1,
            Obligatorio = false,
            VisibleEnGrid = false
            )
        ]
        public string Clase { get; set; }
        
        //------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Método",
            Ayuda = "indica el método",
            TipoDeControl = enumTipoControl.Editor,
            Tipo = typeof(string),
            Fila = 2,
            Columna = 2,
            Obligatorio = false,
            VisibleEnGrid = false
            )
        ]
        public string Metodo { get; set; }

        //------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Esquema",
            Ayuda = "indica el esquema",
            TipoDeControl = enumTipoControl.Editor,
            Tipo = typeof(string),
            Fila = 3,
            Columna = 0,
            Obligatorio = false,
            VisibleEnGrid = false
            )
        ]
        public string Esquema { get; set; }
        
        //------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Procedimiento almacenado",
            Ayuda = "indica procedimiento almacenado",
            TipoDeControl = enumTipoControl.Editor,
            Tipo = typeof(string),
            Fila = 3,
            Columna = 1,
            Obligatorio = false,
            VisibleEnGrid = false,
            AutoSpan = true
            )
        ]
        public string Pa { get; set; }

        //------------------------------------------------------------------------------------------------
        [IUPropiedad(
          Etiqueta = "Bloque Sql",
          Ayuda = "Escriba el bloque Sql",
          Tipo = typeof(string),
          TipoDeControl = enumTipoControl.AreaDeTexto,
          Fila = 4,
          Columna = 0,
          Obligatorio = false,
          VisibleEnGrid = false,
          AutoSpan = true
          )
        ]
        public string Sql { get; set; }

        //------------------------------------------------------------------------------------------------
        [IUPropiedad(
          Etiqueta = "Programa",
          PorAnchoMnt = 30,
          Tipo = typeof(string),
          TipoDeControl = enumTipoControl.Editor,
          Obligatorio = false,
          VisibleEnGrid = true,
          VisibleEnEdicion = false
          )
        ]
        public string Programa => ClaseDeAccion == enumClaseDeAccion.DLL.ToString() ? $"{Clase}.{Metodo}" : $"{(Sql.IsNullOrEmpty() ? $"{Esquema}.{Pa}" : Sql)}";

    }
}
