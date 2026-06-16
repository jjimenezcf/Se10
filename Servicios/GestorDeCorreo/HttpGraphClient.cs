using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Utilidades;

namespace ServicioDeCorreos;

public class HttpGraphClient : IGraphClient
{
    private readonly HttpClient _client;

    public HttpGraphClient(HttpClient client)
    {
        _client = client;
    }

    public async Task<string> SendMessageAsync(string token, string content, string from)
    {
        var sendMailEndpoint = $"https://graph.microsoft.com/v1.0/users/{from}/sendMail";
        var contentData = content; //.Replace("oDataType", "@odata.type", StringComparison.InvariantCultureIgnoreCase);
        var request = new HttpRequestMessage(HttpMethod.Post, sendMailEndpoint)
        {
            Content = new StringContent(contentData, Encoding.UTF8, MimeTypeMap.ApplicationJson)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await _client.SendAsync(request).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            var errorResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            throw new Exception(errorResponse);
        }
        return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Envía el mensaje en formato MIME RFC 2822 con Content-Type: text/plain.
    /// Graph API acepta este formato y preserva las partes MIME (text/calendar, etc.)
    /// tal cual, lo que activa el widget de invitación en Gmail y Outlook.
    /// </summary>
    public async Task SendMimeMessageAsync(string token, string mimeContent, string from)
    {
        var sendMailEndpoint = $"https://graph.microsoft.com/v1.0/users/{from}/sendMail";

        // Graph API requiere el mensaje MIME codificado en base64
        var base64Mime = Convert.ToBase64String(Encoding.UTF8.GetBytes(mimeContent));

        var request = new HttpRequestMessage(HttpMethod.Post, sendMailEndpoint)
        {
            Content = new StringContent(base64Mime, Encoding.UTF8, "text/plain")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await _client.SendAsync(request).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            var errorResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            throw new Exception(errorResponse);
        }
    }
}
