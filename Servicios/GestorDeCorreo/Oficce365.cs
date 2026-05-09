using System;
using System.Collections.Generic;
using System.IO;
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
        var attachments = new List<object>();
        
        foreach (var rutaArchivo in archivos)
        {
            if (File.Exists(rutaArchivo))
            {
                var fileContent = File.ReadAllBytes(rutaArchivo);
                var base64Content = Convert.ToBase64String(fileContent);

                attachments.Add(new
                {
                    @odata_type = "#microsoft.graph.fileAttachment",
                    name = Path.GetFileName(rutaArchivo),
                    contentBytes = base64Content
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
                toRecipients = new[]
                    {
                        new { emailAddress = new { address = receptores } }
                    },
                from = new
                {
                    emailAddress = new { address = emisor }
                },
                attachments = attachments.ToArray(),
                hasAttachments = attachments.Count > 0
            },
            saveToSentItems = true
        };

        if (token is null)
            throw new System.Exception($"No se ha podido obtener un token para el TenantId {tenante.tenantId} y ClientId {tenante.clientId}");

        var response = await _graphClient.SendMessageAsync(token.AccessToken, JsonSerializer.Serialize(message,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }),
            emisor).ConfigureAwait(false);
    }
}
