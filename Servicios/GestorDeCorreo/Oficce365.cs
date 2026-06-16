using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Utilidades;

namespace ServicioDeCorreos;

public class Oficce365 : IDistribuidorOfice365
{
    private readonly IGraphClient _graphClient;
    private readonly IGraphTokenService _graphTokenService;

    public class Tenant: ITenant
    {
    }

    public Oficce365(IGraphClient graphClient, IGraphTokenService graphTokenService)
    {
        _graphClient = graphClient;
        _graphTokenService = graphTokenService;
    }

    public async Task EnviarAsyn(ITenant tenante, string emisor, string receptores, string asunto, string cuerpo, List<string> archivos)
    {
        var token = await _graphTokenService.GetToken(tenante.tenantId, tenante.grantType, tenante.clientId, tenante.clientSecret, tenante.scope).ConfigureAwait(false);

        if (token is null)
            throw new Exception($"No se ha podido obtener un token para el TenantId {tenante.tenantId} y ClientId {tenante.clientId}");

        var icsArchivos = archivos.FindAll(a => ExtensorDistribuidorDeCorreos.EsIcs(a) && File.Exists(a));

        if (icsArchivos.Count > 0)
        {
            // Ruta MIME: Graph acepta RFC 2822 con Content-Type: text/plain.
            // Esto preserva la parte text/calendar inline y activa el widget de invitación
            // en Gmail y Outlook, a diferencia del JSON que solo soporta adjuntos de fichero.
            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress("NoReply", emisor));
            mimeMessage.To.Add(MailboxAddress.Parse(receptores));
            mimeMessage.Subject = asunto;
            mimeMessage.Body = ExtensorDistribuidorDeCorreos.ConstruirCuerpoMime(cuerpo, archivos);

            using var stream = new MemoryStream();
            mimeMessage.WriteTo(stream);
            var mimeContent = Encoding.UTF8.GetString(stream.ToArray());

            await _graphClient.SendMimeMessageAsync(token.AccessToken, mimeContent, emisor).ConfigureAwait(false);
        }
        else
        {
            // Ruta JSON: sin ICS, se usa el endpoint JSON estándar de Graph
            var attachments = new List<object>();
            foreach (var rutaArchivo in archivos)
            {
                if (File.Exists(rutaArchivo))
                {
                    var fileContent = File.ReadAllBytes(rutaArchivo);
                    attachments.Add(new Dictionary<string, object>
                    {
                        ["@odata.type"]  = "#microsoft.graph.fileAttachment",
                        ["name"]         = Path.GetFileName(rutaArchivo),
                        ["contentBytes"] = Convert.ToBase64String(fileContent),
                        ["contentType"]  = ExtensorDistribuidorDeCorreos.ContentTypeParaAdjunto(rutaArchivo)
                    });
                }
            }

            var message = new
            {
                message = new
                {
                    subject = asunto,
                    body = new
                    {
                        contentType = "Html",
                        content = cuerpo.IsNullOrEmpty() ? "" : cuerpo
                    },
                    toRecipients = new[] { new { emailAddress = new { address = receptores } } },
                    from = new { emailAddress = new { address = emisor } },
                    attachments = attachments.ToArray(),
                    hasAttachments = attachments.Count > 0
                },
                saveToSentItems = true
            };

            await _graphClient.SendMessageAsync(token.AccessToken,
                JsonSerializer.Serialize(message, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }),
                emisor).ConfigureAwait(false);
        }
    }
}
