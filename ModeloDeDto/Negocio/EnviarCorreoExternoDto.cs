using Utilidades;

namespace ModeloDeDto.Negocio
{
    // Modal que se abre desde el botón del correo de la edición: envía un correo con un enlace de descarga con guid por cada archivo del
    // elemento que se marque (la lista de archivos la añade la interfaz al abrir la modal con ArchivosController.epLeerAnexadosDirectos
    // y el envío lo hace CorreosController.epEnviarCorreoExterno)
    [IUDto(MostrarExpresion = nameof(Asunto))]
    public class EnviarCorreoExternoDto
    {
        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Para",
            Ayuda = "Correo al que se envía",
            Tipo = typeof(string),
            Fila = 0,
            Columna = 0,
            Obligatorio = true,
            AutoSpan = true
          )
        ]
        public string Para { get; set; }

        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Asunto",
            Ayuda = "Asunto del correo",
            Tipo = typeof(string),
            Fila = 1,
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
           Ayuda = "Cuerpo del mensaje, al final se añaden los enlaces de descarga de los archivos marcados",
           TipoDeControl = enumTipoControl.AreaDeTexto,
           Obligatorio = true,
           NumeroDeFilas = 5,
           Fila = 2,
           Columna = 0,
           LongitudMaxima = 1999,
           AutoSpan = true
          )
        ]
        public string Cuerpo { get; set; }
    }
}
