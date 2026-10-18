using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using Microsoft.Extensions.DependencyInjection;
using ModeloDeDto.TrabajosSometidos;
using ServicioDeCorreos;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using System;
using System.Linq;
using Utilidades;

namespace GestoresDeNegocio.TrabajosSometidos
{
    // Mensajes a un teléfono por WhatsApp, Telegram o SMS (ver IServicioDeMensajesWeb): canal y modo los fijan las variables del sistema
    // CFG_Mensajeria_Canal y CFG_Mensajeria_Modo, y las credenciales de cada canal están en la sección ServidorDeMensajes del appsettings
    public static class ExtensorDeMensajesWeb
    {
        // Lee de las variables quién envía los mensajes a un teléfono y por qué canal
        public static ConfiguracionDeMensajesWebDto LeerConfiguracionDeMensajesWeb()
        {
            var (modo, canal) = Configuracion();
            return new ConfiguracionDeMensajesWebDto { Canal = canal.ToString(), Modo = modo.ToString() };
        }

        private static (enumModoDeMensajeria Modo, enumCanalDeMensajeria Canal) Configuracion() => (
            LeerEnumeradoDeVariable<enumModoDeMensajeria>(Variable.CFG_Mensajeria_Modo, CacheDeVariable.Cfg_Mensajeria_Modo),
            LeerEnumeradoDeVariable<enumCanalDeMensajeria>(Variable.CFG_Mensajeria_Canal, CacheDeVariable.Cfg_Mensajeria_Canal));

        private static T LeerEnumeradoDeVariable<T>(string variable, string valor) where T : struct
        {
            if (!Enum.TryParse<T>(valor?.Trim(), ignoreCase: true, out var enumerado) || !Enum.IsDefined(typeof(T), enumerado))
                GestorDeErrores.Emitir($"La variable '{variable}' está mal definida, el valor '{valor}' no es válido, los admitidos son: {string.Join(", ", Enum.GetNames(typeof(T)))}");
            return enumerado;
        }

        // Valida el teléfono, envía el mensaje por el canal de la variable CFG_Mensajeria_Canal y deja una traza en el elemento.
        // Si CFG_Mensajeria_Modo es SERVIDOR lo envía el servidor y devuelve null; si es CLIENTE no envía nada: devuelve la url que abre la
        // aplicación del canal con el mensaje ya escrito, lo envía el usuario, así que la traza indica que se abrió, no que se envió
        public static string EnviarMensajeWeb(this ContextoSe contexto, enumNegocio negocio, int idElemento, string para, string mensaje)
        {
            if (para.IsNullOrEmpty()) GestorDeErrores.Emitir("Debe indicar el teléfono al que enviar");
            var numero = ExtensorDeTelefonos.ANumeroInternacional(para);
            if (numero is null) GestorDeErrores.Emitir(ExtensorDeTelefonos.MensajeDeTelefonoNoValido(para));
            if (mensaje.IsNullOrEmpty()) GestorDeErrores.Emitir("Debe indicar el mensaje");

            var (modo, canal) = Configuracion();
            var servicio = contexto.ServiceProvider.GetServices<IServicioDeMensajesWeb>().FirstOrDefault(s => s.Canal == canal);
            if (servicio is null) GestorDeErrores.Emitir($"No hay ningún servicio para enviar mensajes por '{canal}'");

            string url = null;
            if (modo == enumModoDeMensajeria.CLIENTE)
            {
                url = servicio.UrlParaElCliente(numero, mensaje);
                if (url.IsNullOrEmpty())
                    GestorDeErrores.Emitir($"El canal '{canal}' no permite que el envío lo haga el cliente, cambie la variable '{Variable.CFG_Mensajeria_Modo}' a '{enumModoDeMensajeria.SERVIDOR}'");
            }
            else
            {
                try
                {
                    servicio.EnviarAsync(numero, mensaje).GetAwaiter().GetResult();
                }
                catch (Exception e)
                {
                    GestorDeErrores.Emitir($"No se ha podido enviar el mensaje por {canal}: {e.Message.Left(300)}", e);
                }
            }

            if (negocio.UsaTrazas())
            {
                var accion = modo == enumModoDeMensajeria.CLIENTE ? $"Se ha abierto {canal} para enviar" : $"Se ha enviado por {canal}";
                ((IElementoDtm)negocio.LeerRegistro(contexto, idElemento)).CrearTraza(contexto, enumTraza.envioDeMensajeWeb.Descripcion()
                    , $"{accion} al teléfono '+{numero}' el mensaje:{Environment.NewLine}{mensaje}");
            }

            return url;
        }
    }
}
