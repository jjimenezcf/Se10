using System.Threading.Tasks;

namespace ServicioDeCorreos;

// Canal por el que se manda un mensaje a un teléfono (variable CFG_Mensajeria_Canal); el nombre es también el de la sección del appsettings
public enum enumCanalDeMensajeria { WHATSAPP, TELEGRAM, SMS }

// Quién hace el envío (variable CFG_Mensajeria_Modo): CLIENTE devuelve una url que abre la aplicación del canal en el equipo del usuario
// con el mensaje ya escrito (lo envía el usuario), SERVIDOR lo envía el servidor llamando al servicio web del canal
public enum enumModoDeMensajeria { CLIENTE, SERVIDOR }

public interface IServicioDeMensajesWeb
{
    enumCanalDeMensajeria Canal { get; }

    // Los teléfonos llegan en formato internacional sólo con dígitos, sin '+' (ver extTelefonos.ANumeroInternacional)

    // Enlace que abre en el cliente la aplicación del canal con el mensaje ya escrito; null si el canal no lo permite
    string UrlParaElCliente(string telefono, string mensaje);

    // Envía el mensaje desde el servidor, lanza una excepción si el servicio web lo rechaza
    Task EnviarAsync(string telefono, string mensaje);
}
