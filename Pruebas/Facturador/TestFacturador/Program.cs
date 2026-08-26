using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using TestFacturador;

var jsonOpciones = new JsonSerializerOptions
{
    WriteIndented = true,
    PropertyNameCaseInsensitive = true,
    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
};

Console.WriteLine("=== TestFacturador ===");
Console.WriteLine();

var config = Config.CargarOCrear();

var urlBase = PedirUrlBase(config.UrlBase);
var uriBase = new Uri(NormalizarUrlBase(urlBase));

if (!await ComprobarUrlBaseAsync(uriBase))
{
    Console.WriteLine("Url Base no válida, pulsa cualquier tecla para salir.");
    Console.ReadKey();
    return;
}

var nif = Pedir("Nif del emisor", config.Nif);
var apiKey = Pedir("ApiKey", config.ApiKey);

config.UrlBase = urlBase;
config.Nif = nif;
config.ApiKey = apiKey;
config.Guardar();

using var handler = new HttpClientHandler
{
    // Solo para pruebas: admite el certificado de desarrollo de localhost.
    ServerCertificateCustomValidationCallback = (_, _, _, _) => true
};
using var http = new HttpClient(handler) { BaseAddress = uriBase };

var factura = FacturaDeEjemplo.Construir();
var cuerpo = JsonSerializer.Serialize(factura, jsonOpciones);
var rutaRelativa = $"Facturador/epCrearFactura?nif={Uri.EscapeDataString(nif)}&apiKey={Uri.EscapeDataString(apiKey)}";

Console.WriteLine();
Console.WriteLine("Se va a ejecutar la siguiente petición:");
Console.WriteLine();
Console.WriteLine($"POST {new Uri(http.BaseAddress!, rutaRelativa)}");
Console.WriteLine("Content-Type: application/json");
Console.WriteLine();
Console.WriteLine(cuerpo);
Console.WriteLine();

if (!Confirmar("¿Deseas ejecutarla?"))
{
    Console.WriteLine("Cancelado.");
    return;
}

Resultado? resultado;
try
{
    using var respuesta = await http.PostAsync(rutaRelativa, new StringContent(cuerpo, Encoding.UTF8, "application/json"));
    var contenido = await respuesta.Content.ReadAsStringAsync();
    MostrarRespuesta(contenido, jsonOpciones);
    resultado = JsonSerializer.Deserialize<Resultado>(contenido, jsonOpciones);
}
catch (Exception ex)
{
    Console.WriteLine($"Error al llamar al servicio: {DescribirError(ex, uriBase)}");
    return;
}

Console.WriteLine();
Console.WriteLine($"Estado: {resultado?.Estado}");
Console.WriteLine($"Mensaje: {resultado?.Mensaje ?? resultado?.Consola}");

if (resultado is null || !resultado.EsOk || resultado.Datos.ValueKind != JsonValueKind.Object)
{
    Console.WriteLine();
    Console.WriteLine("No se puede continuar: la factura no se ha creado correctamente.");
    return;
}

var facturaCreada = resultado.Datos.Deserialize<FacturaCreada>(jsonOpciones);
if (facturaCreada is null || string.IsNullOrEmpty(facturaCreada.NumeroFactura))
{
    Console.WriteLine("La respuesta no trae los datos esperados de la factura.");
    return;
}

Console.WriteLine();
Console.WriteLine($"Número de factura : {facturaCreada.NumeroFactura}");
Console.WriteLine($"GuidDeConsultaPdf  : {facturaCreada.GuidDeConsultaPdf}");
Console.WriteLine($"GuidDeConsultaXml  : {facturaCreada.GuidDeConsultaXml}");
Console.WriteLine($"UrlDeLaFactura     : {facturaCreada.UrlDeLaFactura}");

while (true)
{
    Console.WriteLine();
    Console.WriteLine("1. Descargar XML");
    Console.WriteLine("2. Descargar PDF");
    Console.WriteLine("3. Acceder a la factura");
    Console.WriteLine("4. Rectificar por datos erroneos");
    Console.WriteLine("0. Salir");
    Console.Write("Opción: ");
    var opcion = Console.ReadLine();

    switch (opcion)
    {
        case "1":
            if (facturaCreada.GuidDeConsultaXml is null)
                Console.WriteLine("Esta factura no tiene GuidDeConsultaXml.");
            else
                await DescargarDocumentoAsync(http, nif, apiKey, facturaCreada.NumeroFactura, facturaCreada.GuidDeConsultaXml.Value, "epSolicitarXml", jsonOpciones);
            break;

        case "2":
            if (facturaCreada.GuidDeConsultaPdf is null)
                Console.WriteLine("Esta factura no tiene GuidDeConsultaPdf.");
            else
                await DescargarDocumentoAsync(http, nif, apiKey, facturaCreada.NumeroFactura, facturaCreada.GuidDeConsultaPdf.Value, "epSolicitarPdf", jsonOpciones);
            break;

        case "3":
            AbrirEnNavegador(facturaCreada.UrlDeLaFactura);
            break;

        case "4":
            await RectificarPorDeAsync(http, apiKey, facturaCreada.NumeroFactura, jsonOpciones);
            break;

        case "0":
            return;

        default:
            Console.WriteLine("Opción no válida.");
            break;
    }
}

static async Task<bool> ComprobarUrlBaseAsync(Uri uriBase)
{
    try
    {
        await Dns.GetHostAddressesAsync(uriBase.Host);
        return true;
    }
    catch (Exception)
    {
        Console.WriteLine();
        Console.WriteLine($"La url '{uriBase}' no existe: no se puede resolver el host '{uriBase.Host}'.");
        return false;
    }
}

static string DescribirError(Exception ex, Uri uri)
{
    for (var actual = ex; actual != null; actual = actual.InnerException)
    {
        if (actual is SocketException socketEx)
        {
            if (socketEx.SocketErrorCode is SocketError.HostNotFound or SocketError.TryAgain or SocketError.NoData)
                return $"La url '{uri}' no existe: no se puede resolver el host '{uri.Host}'.";
            if (socketEx.SocketErrorCode == SocketError.ConnectionRefused)
                return $"No se puede conectar con '{uri}': conexión rechazada (¿está el servicio arrancado en ese puerto?).";
            if (socketEx.SocketErrorCode == SocketError.TimedOut)
                return $"No se puede conectar con '{uri}': ha caducado el tiempo de espera.";
        }
    }
    return ex.Message;
}

static string NormalizarUrlBase(string urlBase)
{
    var sinBarraFinal = urlBase.TrimEnd('/');
    return sinBarraFinal.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || sinBarraFinal.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
        ? sinBarraFinal + "/"
        : "https://" + sinBarraFinal + "/";
}

static string PedirUrlBase(string valorPorDefecto)
{
    Console.WriteLine("Introduce la url base (ej. localhost:44396, biwe.femdek.com, galileo.femdek.com)");
    return Pedir("Url base", valorPorDefecto);
}

static string Pedir(string etiqueta, string valorPorDefecto)
{
    Console.Write(string.IsNullOrEmpty(valorPorDefecto) ? $"{etiqueta}: " : $"{etiqueta} [{valorPorDefecto}]: ");
    var entrada = Console.ReadLine();
    return string.IsNullOrWhiteSpace(entrada) ? valorPorDefecto : entrada.Trim();
}

static void MostrarRespuesta(string contenido, JsonSerializerOptions jsonOpciones)
{
    Console.WriteLine();
    Console.WriteLine("Respuesta:");
    try
    {
        using var documento = JsonDocument.Parse(contenido);
        Console.WriteLine(JsonSerializer.Serialize(documento.RootElement, jsonOpciones));
    }
    catch (JsonException)
    {
        Console.WriteLine(contenido);
    }
}

static bool Confirmar(string pregunta)
{
    Console.Write($"{pregunta} (S/n): ");
    var entrada = Console.ReadLine()?.Trim().ToLowerInvariant();
    return string.IsNullOrEmpty(entrada) || entrada == "s" || entrada == "si" || entrada == "y" || entrada == "yes";
}

static async Task DescargarDocumentoAsync(HttpClient http, string nif, string apiKey, string numeroFactura, Guid guid, string accion, JsonSerializerOptions jsonOpciones)
{
    var rutaRelativa = $"Facturador/{accion}?nif={Uri.EscapeDataString(nif)}&apiKey={Uri.EscapeDataString(apiKey)}&numeroFactura={Uri.EscapeDataString(numeroFactura)}&guid={guid}";

    Resultado? resultado;
    try
    {
        var contenido = await http.GetStringAsync(rutaRelativa);
        MostrarRespuesta(contenido, jsonOpciones);
        resultado = JsonSerializer.Deserialize<Resultado>(contenido, jsonOpciones);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error al solicitar el documento: {DescribirError(ex, http.BaseAddress!)}");
        return;
    }

    if (resultado is null || !resultado.EsOk || resultado.Datos.ValueKind != JsonValueKind.String)
    {
        Console.WriteLine($"No se ha podido obtener la url de descarga: {resultado?.Mensaje ?? resultado?.Consola ?? "respuesta inesperada"}");
        return;
    }

    var urlDeDescarga = resultado.Datos.GetString()!;
    Console.WriteLine($"Url de descarga (válida 1 hora): {urlDeDescarga}");

    try
    {
        using var respuesta = await http.GetAsync(urlDeDescarga);
        if (!respuesta.IsSuccessStatusCode)
        {
            Console.WriteLine($"Error al descargar el fichero: {(int)respuesta.StatusCode} {respuesta.ReasonPhrase}");
            return;
        }

        var bytes = await respuesta.Content.ReadAsByteArrayAsync();
        var nombrePropuesto = respuesta.Content.Headers.ContentDisposition?.FileNameStar
                               ?? respuesta.Content.Headers.ContentDisposition?.FileName
                               ?? $"{numeroFactura.Replace('/', '-')}.{(accion == "epSolicitarPdf" ? "pdf" : "xml")}";
        nombrePropuesto = nombrePropuesto.Trim('"');

        var carpetaPorDefecto = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        Console.Write($"Carpeta donde guardarlo [{carpetaPorDefecto}]: ");
        var carpeta = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(carpeta))
            carpeta = carpetaPorDefecto;

        Directory.CreateDirectory(carpeta);
        var rutaCompleta = Path.Combine(carpeta, nombrePropuesto);
        await File.WriteAllBytesAsync(rutaCompleta, bytes);

        Console.WriteLine($"Guardado en: {rutaCompleta}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error al descargar el fichero: {DescribirError(ex, http.BaseAddress!)}");
    }
}

static async Task RectificarPorDeAsync(HttpClient http, string apiKey, string? numeroFacturaSugerido, JsonSerializerOptions jsonOpciones)
{
    var numeroFactura = Pedir("Número de factura a rectificar", numeroFacturaSugerido ?? "");
    var motivo = Pedir("Motivo de la rectificación", "Datos erróneos");

    var rutaRelativa = $"Facturador/epRectificarPorDe?apiKey={Uri.EscapeDataString(apiKey)}&numeroFactura={Uri.EscapeDataString(numeroFactura)}";

    Console.WriteLine();
    Console.WriteLine("Se va a ejecutar la siguiente petición:");
    Console.WriteLine();
    Console.WriteLine($"POST {new Uri(http.BaseAddress!, rutaRelativa)}");
    Console.WriteLine("Content-Type: text/plain");
    Console.WriteLine();
    Console.WriteLine(motivo);
    Console.WriteLine();

    if (!Confirmar("¿Deseas ejecutarla?"))
    {
        Console.WriteLine("Cancelado.");
        return;
    }

    Resultado? resultado;
    try
    {
        using var respuesta = await http.PostAsync(rutaRelativa, new StringContent(motivo, Encoding.UTF8, "text/plain"));
        var contenido = await respuesta.Content.ReadAsStringAsync();
        MostrarRespuesta(contenido, jsonOpciones);
        resultado = JsonSerializer.Deserialize<Resultado>(contenido, jsonOpciones);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error al llamar al servicio: {DescribirError(ex, http.BaseAddress!)}");
        return;
    }

    Console.WriteLine();
    Console.WriteLine($"Estado: {resultado?.Estado}");
    Console.WriteLine($"Mensaje: {resultado?.Mensaje ?? resultado?.Consola}");

    if (resultado is null || !resultado.EsOk || resultado.Datos.ValueKind != JsonValueKind.Object)
        return;

    var rectificativa = resultado.Datos.Deserialize<FacturaCreada>(jsonOpciones);
    if (rectificativa is null)
        return;

    Console.WriteLine();
    Console.WriteLine("Datos de la rectificativa creada:");
    Console.WriteLine($"Número de factura : {rectificativa.NumeroFactura}");
    Console.WriteLine($"GuidDeConsultaPdf  : {rectificativa.GuidDeConsultaPdf}");
    Console.WriteLine($"GuidDeConsultaXml  : {rectificativa.GuidDeConsultaXml}");
    Console.WriteLine($"UrlDeLaFactura     : {rectificativa.UrlDeLaFactura}");
}

static void AbrirEnNavegador(string? url)
{
    if (string.IsNullOrEmpty(url))
    {
        Console.WriteLine("No hay UrlDeLaFactura para esta petición.");
        return;
    }

    try
    {
        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"No se ha podido abrir el navegador: {ex.Message}");
        Console.WriteLine($"Url: {url}");
    }
}
