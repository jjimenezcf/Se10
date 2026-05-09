using System.Threading.Tasks;

namespace ServicioDeCorreos;

public interface IGraphClient
{
    Task<string> SendMessageAsync(string token, string content, string from);
}