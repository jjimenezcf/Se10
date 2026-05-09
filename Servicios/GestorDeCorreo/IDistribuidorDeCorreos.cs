using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServicioDeCorreos;


public class ITenant
{
    public string tenantId { get; set; }
    public string grantType { get; set; }
    public string clientId { get; set; }
    public string clientSecret { get; set; }
    public string scope { get; set; }
}

public interface IDistribuidorDeCorreos
{
    Task EnviarAsyn(ITenant tenante, string emisor, string receptores, string asunto, string mensaje, List<string> archivos);
}


public interface IDistribuidorOfice365: IDistribuidorDeCorreos
{
}


public interface IDistribuidorMailJet : IDistribuidorDeCorreos
{
}


public interface IDistribuidorSmtp : IDistribuidorDeCorreos
{
    Task<int> EliminarAsyn(ITenant tenante, int anterioresA);
}

