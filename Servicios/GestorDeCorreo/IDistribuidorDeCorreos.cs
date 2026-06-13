using MimeKit;
using System.Collections.Generic;
using System.IO;
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


public static class ExtensorDistribuidorDeCorreos
{
    /// <summary>
    /// Devuelve true si el archivo es un fichero ICS de calendario.
    /// </summary>
    public static bool EsIcs(string rutaArchivo)
        => Path.GetExtension(rutaArchivo).Equals(".ics", System.StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Devuelve "text/calendar" para ficheros ICS y "application/octet-stream" para el resto.
    /// Usado por Office365 y MailJet para fijar el ContentType del adjunto.
    /// </summary>
    public static string ContentTypeParaAdjunto(string rutaArchivo)
        => EsIcs(rutaArchivo) ? "text/calendar" : "application/octet-stream";

    /// <summary>
    /// Construye el cuerpo MIME del mensaje. Si hay ficheros ICS entre los adjuntos,
    /// añade una parte text/calendar inline (activa el widget Sí/No/Quizás en Gmail)
    /// además del adjunto físico para clientes que no interpreten el inline.
    /// </summary>
    public static MimeEntity ConstruirCuerpoMime(string mensaje, List<string> archivos)
    {
        var icsArchivos = archivos.FindAll(a => EsIcs(a) && File.Exists(a));

        if (icsArchivos.Count > 0)
        {
            var alternative = new MultipartAlternative();
            alternative.Add(new TextPart(MimeKit.Text.TextFormat.Plain) { Text = mensaje });
            alternative.Add(new TextPart(MimeKit.Text.TextFormat.Html) { Text = $"<html><body>{mensaje}</body></html>" });

            foreach (var icsPath in icsArchivos)
            {
                var calPart = new TextPart("calendar") { Text = File.ReadAllText(icsPath) };
                calPart.ContentType.Parameters.Add("method", "REQUEST");
                alternative.Add(calPart);
            }

            var mixed = new Multipart("mixed");
            mixed.Add(alternative);

            foreach (var archivo in archivos)
            {
                if (File.Exists(archivo))
                {
                    mixed.Add(new MimePart(MimeTypes.GetMimeType(archivo))
                    {
                        Content = new MimeContent(File.OpenRead(archivo)),
                        ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                        ContentTransferEncoding = ContentEncoding.Base64,
                        FileName = Path.GetFileName(archivo)
                    });
                }
            }

            return mixed;
        }

        var builder = new BodyBuilder();
        builder.TextBody = mensaje;
        builder.HtmlBody = $"<html><body>{mensaje}</body></html>";
        foreach (var archivo in archivos)
            if (File.Exists(archivo))
                builder.Attachments.Add(archivo);
        return builder.ToMessageBody();
    }
}

