using Utilidades;

namespace ModeloDeDto.TrabajosSometidos
{
    // Modal que se abre desde el botón del teléfono de la edición (WhatsApp, Telegram o SMS según la variable CFG_Mensajeria_Canal):
    // MensajesWebController.epEnviarMensajeWeb valida el teléfono, deja una traza en el elemento y, si CFG_Mensajeria_Modo es CLIENTE, devuelve la
    // url con la que la interfaz abre la aplicación del canal con el mensaje ya escrito (el envío lo confirma el usuario); si es SERVIDOR
    // lo envía el servidor
    [IUDto(MostrarExpresion = nameof(Para))]
    public class EnviarMensajeWebDto
    {
        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Teléfono",
            Ayuda = "Teléfono al que se envía, si no es español indíquelo con el prefijo del país (+351 ...)",
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
           Etiqueta = "Mensaje",
           Ayuda = "Mensaje que se envía al teléfono indicado",
           TipoDeControl = enumTipoControl.AreaDeTexto,
           Obligatorio = true,
           NumeroDeFilas = 5,
           Fila = 1,
           Columna = 0,
           LongitudMaxima = 1500,
           AutoSpan = true
          )
        ]
        public string Mensaje { get; set; }
    }
}
