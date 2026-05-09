using System.Threading.Tasks;

namespace ServicioDeCorreos;

public interface IGraphTokenService
{
    Task<GraphToken> GetToken(string tenantId, string grantType, string clientId, string clientSecret, string scope);
}
