using Utilidades;

namespace ModeloDeDto.Negocio
{
    // Modal que se abre desde el botón del teléfono de la edición: CorreosController.epEnviarWhatsApp valida el teléfono, deja una traza
    // en el elemento y devuelve el enlace https://wa.me/ con el que la interfaz abre WhatsApp (web o escritorio) con el mensaje ya escrito;
    // el envío lo confirma el usuario en WhatsApp, desde su propia cuenta
    [IUDto(MostrarExpresion = nameof(Para))]
    public class EnviarWhatsAppDto
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
           Ayuda = "Mensaje que se escribe en WhatsApp, se envía al pulsar enviar en WhatsApp",
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
