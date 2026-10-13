namespace ModeloDeDto.TrabajosSometidos
{
    // Respuesta de MensajesWebController.epLeerConfiguracionDeMensajesWeb: canal (WHATSAPP, TELEGRAM o SMS) y modo (CLIENTE o SERVIDOR) de los
    // mensajes a un teléfono, según las variables CFG_Mensajeria_Canal y CFG_Mensajeria_Modo; la interfaz los lee para pintar el botón junto al
    // teléfono con el icono del canal y para saber si ha de abrir una ventana al enviar
    public class ConfiguracionDeMensajesWebDto
    {
        public string Canal { get; set; }

        public string Modo { get; set; }
    }
}
