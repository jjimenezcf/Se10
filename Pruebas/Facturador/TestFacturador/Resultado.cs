using System.Text.Json;

namespace TestFacturador;

public class Resultado
{
    public string? Estado { get; set; }
    public string? Mensaje { get; set; }
    public string? Consola { get; set; }
    public int Total { get; set; }
    public JsonElement Datos { get; set; }
    public string? ModoDeAcceso { get; set; }
    public bool Logout { get; set; }

    public bool EsOk => string.Equals(Estado, "Ok", StringComparison.OrdinalIgnoreCase);
}

public class FacturaCreada
{
    public DateTime SolicitadaEl { get; set; }
    public string? Peticion { get; set; }
    public string? Facturador { get; set; }
    public string? NumeroFactura { get; set; }
    public string? Mensaje { get; set; }
    public Guid? GuidDeConsultaPdf { get; set; }
    public Guid? GuidDeConsultaXml { get; set; }
    public string? UrlDeLaFactura { get; set; }
}
