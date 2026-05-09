using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace ServicioDeCorreos;

public class GraphTokenService : IGraphTokenService
{
    private readonly ILogger<GraphTokenService> _logger;
    private readonly HttpClient _client;

    private Dictionary<string, (GraphToken token, DateTime expires)> _tokens = new();

    public GraphTokenService(ILogger<GraphTokenService> logger, HttpClient client)
    {
        _logger = logger;
        _client = client;
    }

    public async Task<GraphToken> GetToken(string tenantId, string grantType, string clientId, string clientSecret, string scope)
    {
        var currentTenantToken = _tokens.TryGetValue($"{tenantId}-{scope}-{grantType}", out var currentToken);
        if (!currentTenantToken || currentToken.expires < DateTime.UtcNow)
        {
            try
            {
                var parameters = new Dictionary<string, string>
                {
                    { "grant_type", grantType },
                    { "client_id", clientId },
                    { "scope", scope },
                    { "client_secret", clientSecret }
                };
                var tokenEndpoint = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";
                var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint)
                {
                    Content = new FormUrlEncodedContent(parameters)
                };

                using var response = await _client.SendAsync(request).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    _logger?.LogWarning("Getting graph token is not successfull. {Response}", JsonSerializer.Serialize(responseContent));
                    return null;
                }
                var returnJson = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                var token = await JsonSerializer.DeserializeAsync<GraphToken>(returnJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }).ConfigureAwait(false);
                var expires = token is not null ? DateTime.UtcNow.AddSeconds(token.ExpiresIn) : DateTime.UtcNow;
                if (!currentTenantToken)
                {
                    _tokens.Add($"{tenantId}-{scope}-{grantType}", (token ?? throw new InvalidDataException("Token is null"), expires));
                }
                else
                {
                    currentToken.expires = expires;
                    currentToken.token = token ?? throw new InvalidDataException("Token is null");
                }
                return token;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting graph token");
                throw;
            }
        }
        else
        {
            return currentToken.token;
        }
    }
}
