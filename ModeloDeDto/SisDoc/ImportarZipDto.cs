using Utilidades;

namespace ModeloDeDto.SistemaDocumental
{
    [IUDto(MostrarExpresion = nameof(IUsaNombreDto.Nombre))]
    public class ImportarZipDto
    {
        //----------------------------------------------------------
        [IUPropiedad(Etiqueta = "Archivador",
            TipoDeControl = enumTipoControl.RestrictorDeEdicion,
            MostrarExpresion = nameof(Archivador),
            Fila = 0,
            Columna = 0,
            AutoSpan = false,
            Controlador = nameof(enumControladoresSistemaDocumental.Archivadores),
            VistaDondeNavegar = enumVistasSistemaDocumental.CrudArchivadores
            )
        ]
        public int IdArchivador { get; set; }

        [IUPropiedad(Visible = false)]
        public string Archivador { get; set; }

        //------------------------------------------------------------------------
        [IUPropiedad(
            VisibleEnGrid = false,
            Etiqueta = "Archivo Zip",
            Ayuda = "Seleccione el fichero zip a importar",
            Tipo = typeof(int),
            LimiteEnByte = TipoControlExtension.BytePermitidosEnZip,
            TipoDeControl = enumTipoControl.SelectorDeUnArchivo,
            ExtensionesValidas = ".Zip",
            Fila = 0,
            Columna = 1,
            AutoSpan = true)]
        public int IdArchivo { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Renombrar",
           Ayuda = "Renombra el archivo importado si existe con el mismo nombre",
           TipoDeControl = enumTipoControl.Check,
           Obligatorio = false,
           css = enumCssControles.CheckEnLinea,
           ValorPorDefecto = true,
           Fila = 1, Columna = 0, Posicion = 0)
        ]
        public bool Renombrar { get; set; }


        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Remplazar",
           Ayuda = "Si existe, remplaza el archivo existente con el importado",
           TipoDeControl = enumTipoControl.Check,
           Obligatorio = false,
           css = enumCssControles.CheckEnLinea,
           ValorPorDefecto = false,
           Fila = 1, Columna = 0, Posicion = 1)
        ]
        public bool Remplazar { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Eliminar archivo",
           Ayuda = "Elimina cualquier archivo existente que no esté en la carpeta del importado",
           TipoDeControl = enumTipoControl.Check,
           Obligatorio = false,
           css = enumCssControles.CheckEnLinea,
           ValorPorDefecto = false,
           Fila = 1, Columna = 1, Posicion = 0)
        ]
        public bool EliminarArchivo { get; set; }

        ////--------------------------------------------
        //[IUPropiedad(
        //   Etiqueta = "Eliminar carpeta",
        //   Tipo = typeof(decimal),
        //   Ayuda = "Elimina cualquier carpeta que no exita en el importado",
        //   TipoDeControl = enumTipoControl.Check,
        //   Obligatorio = false,
        //   css = enumCssControles.CheckEnLinea,
        //   ValorPorDefecto = false,
        //   Fila = 1, Columna = 1, Posicion = 1)
        //]
        //public bool EliminarCarpeta { get; set; }
    }
}
