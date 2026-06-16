using System.Threading.Tasks;

namespace ServicioDeCorreos;

public interface IGraphClient
{
    Task<string> SendMessageAsync(string token, string content, string from);

    /// <summary>
    /// Envía un mensaje en formato MIME RFC 2822 (Content-Type: text/plain).
    /// Necesario para incluir partes text/calendar inline que activan el widget
    /// de invitación de calendario en Gmail y Outlook.
    /// </summary>
    Task SendMimeMessageAsync(string token, string mimeContent, string from);
}