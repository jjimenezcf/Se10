using System;
using Utilidades;

namespace ModeloDeDto.Entorno
{

    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5)]
    public class MiUsuarioDto : ElementoDto, IUsaArchivoDto
    {
     

        //------------------------------------------------------------------------
        [IUPropiedad(
            VisibleEnGrid = false,
            Etiqueta = "Usuario",
            Ayuda = "Usuario de conexión",
            Tipo = typeof(string),
            Fila = 1,
            Columna = 0,
            EditableAlEditar = false
            )
        ]
        public string Login { get; set; }


        //------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "eMail",
            Ayuda = "eMail",
            Tipo = typeof(string),
            Fila = 1,
            Columna = 1
            )
        ]
        public string eMail { get; set; }

        //------------------------------------------------------------------------
        [IUPropiedad(
            VisibleEnGrid = false,
            VisibleEnEdicion = true,
            Etiqueta = "Fotografía",
            Ayuda = "Seleccione un fichero",
            Tipo = typeof(int),
            TipoDeControl = enumTipoControl.Archivo,
            ExtensionesValidas = ExtensorDeTipoDeArchivos.Imagenes,
            UrlDelArchivo = nameof(Archivo),
            Obligatorio = false,
            Fila = 2,
            Columna = 0)]
        public int? IdArchivo { get; set; }

        [IUPropiedad(TipoDeControl = enumTipoControl.ImagenDelCanvas)]
        public string Archivo { get; set; }

    }



}
