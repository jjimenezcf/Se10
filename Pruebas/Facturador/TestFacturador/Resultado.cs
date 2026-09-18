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

public class ClienteJson
{
    public string? TipoDeCliente { get; set; }
    public string NIF { get; set; } = "";
    public string? Nombre { get; set; }
    public string? Apellidos { get; set; }
    public string? eMail { get; set; }
    public string? Telefono { get; set; }
    public string? Municipio { get; set; }
    public string? CodigoPostal { get; set; }
    public string? TipoDeVia { get; set; }
    public string? Calle { get; set; }
    public int Numero { get; set; }
    public bool ValidarEnLaAeat { get; set; }
    public bool SustituirDatosIdentificativos { get; set; }
    public bool SustituirDatosDeContacto { get; set; }
    public bool CrearCalleSiNoExiste { get; set; }
    public bool ValidarEnCatastro { get; set; }
}

public class ClienteCreado
{
    public int Id { get; set; }
    public string? NIF { get; set; }
    public string? Nombre { get; set; }
}
