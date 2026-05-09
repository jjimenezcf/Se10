using Gestor.Errores;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Http;
using Google.Apis.Services;
using MailKit.Security;
using ModeloDeDto.Entorno;
using Newtonsoft.Json;
using System.Net;
using Utilidades;

namespace ServicioDeAcceso
{
    internal class LectorDeGmail : ILectorDeCorreo
    {

        async Task<List<MiCorreoDto>> ILectorDeCorreo.ReadEmailAsync(object credenciales, string nombreAplicacion)
        {
            List<MiCorreoDto> correos = new();
            var service = new GmailService(new BaseClientService.Initializer
            {
                HttpClientInitializer = (IConfigurableHttpClientInitializer)credenciales,
                ApplicationName = nombreAplicacion
            });

            string accessToken = await ((UserCredential)credenciales).GetAccessTokenForRequestAsync();
            var networkCredential = new NetworkCredential(accessToken, string.Empty);
            var saslMechanism = new SaslMechanismOAuth2(networkCredential);

            UsersResource.MessagesResource.ListRequest messageRequest = service.Users.Messages.List("me");
            messageRequest.LabelIds = "INBOX";
            messageRequest.MaxResults = 40; // Adjust this value to get more or fewer messages

            // Execute request and get messages
            IList<Message> messages = messageRequest.Execute().Messages;
            if (messages == null || messages.Count == 0)
                return correos;

            // Process messages and add them to the correos list
            foreach (var message in messages)
            {
                var email = service.Users.Messages.Get("me", message.Id).Execute();
                // Process the email and add it to the correos list
            }

            foreach (var message in messages)
            {
                var correo = new MiCorreoDto();
                UsersResource.MessagesResource.GetRequest messageGetRequest = service.Users.Messages.Get("me", message.Id);
                Message fullMessage = messageGetRequest.Execute();

                // Print the subject of the message
                var subjectHeader = fullMessage.Payload.Headers.FirstOrDefault(h => h.Name == "Subject");
                correo.Id = Encriptacion.GenerarEntero();
                correo.Asunto = subjectHeader?.Value ?? "(Sin Asunto}";
                correo.Cuerpo = fullMessage.Raw;
                correo.Fecha = Fecha(fullMessage);
                correo.IdMensaje = fullMessage.Id;
                var adjuntos = new List<Adjunto>();
                if (fullMessage.Payload.Parts != null && fullMessage.Payload.Parts.Any(x => x.Filename != null && !x.Filename.IsNullOrEmpty()))
                {
                    foreach (var part in fullMessage.Payload.Parts.Where(x => x.Filename != null && x.Filename.Trim() != ""))
                    {
                        var adjunto = new Adjunto();
                        adjunto.fichero = part.Filename;
                        adjunto.tipoMime = part.MimeType;
                        adjuntos.Add(adjunto);
                    }
                }
                correo.Adjuntos = JsonConvert.SerializeObject(adjuntos);
            }
            return correos;
        }

        private DateTime Fecha(Message message)
        {
            var dateHeader = message.Payload.Headers.FirstOrDefault(h => h.Name == "Date");

            if (dateHeader != null)
            {
                DateTime date;
                if (DateTime.TryParse(dateHeader.Value, out date))
                {
                    return date;
                }
                else
                {
                    GestorDeErrores.Emitir($"la fecha '{dateHeader.Value}' del mensaje pasado no es válida");
                }
            }
            return Fechas.MilisegundosToFecha(message.InternalDate);
        }

        //public DescargarAdjunto(string idMensaje, string nombre, string mimeType)
        //{
        //    var mensaje = BuscarAdjunto(idMensaje);

        //    var attachmentRequest = service.Users.Messages.Attachments.Get("me", fullMessage.Id, part.Body.AttachmentId);
        //    var attachment = attachmentRequest.Execute();
        //    string downloadPath = Path.Combine(@"c:\Users\jjimenezc\desarrollo\Google\Gmail", part.Filename);
        //    //if (part.MimeType.Contains("text"))
        //    //{
        //    //    byte[] bytes = Convert.FromBase64CharArray(attachment.Data.ToCharArray(), 0, attachment.Data.ToCharArray().Length);
        //    //    File.WriteAllBytes(downloadPath, bytes);
        //    //}
        //    //else
        //    //{

        //    //var a = WebEncoders.Base64UrlDecode(attachment.Data);
        //    string base64String = attachment.Data.Replace('-', '+').Replace('_', '/').PadRight(attachment.Data.Length + (4 - attachment.Data.Length % 4) % 4, '=');


        //    // Convert the URL-safe base64 string to a byte array
        //    byte[] decodedData = Convert.FromBase64String(base64String);

        //    //byte[] bytes = Encoding.Default.GetBytes(a);
        //    BinaryWriter writer = new BinaryWriter(new FileStream(downloadPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None));
        //    writer.Write(decodedData);
        //    writer.Close();
        //    //}

        //    Console.WriteLine($"Archivo '{part.Filename}' descargado en '{downloadPath}'.");
        //}

        private Message BuscarAdjunto(string idMensaje)
        {
            return new Message();
        }
    }
}


/*
 * 
 * 
 
string[] Scopes = { GmailService.Scope.GmailReadonly, GmailService.Scope.GmailModify, GmailService.Scope.MailGoogleCom };
string ApplicationName = "Gmail";

UserCredential credential;
// Load client secrets.
string credPath = "token.json";
using (var stream = new FileStream(@"c:\Users\jjimenezc\desarrollo\Google\Gmail\cliente_escritorio.json", FileMode.Open, FileAccess.Read))
{
    * The file token.json stores the user's access and refresh tokens, and is created  automatically when the authorization flow completes for the first time. *
    credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
        GoogleClientSecrets.FromStream(stream).Secrets,
        Scopes,
        "jjimenezcf",
        CancellationToken.None,
        new FileDataStore(credPath, true)).Result;
    Console.WriteLine("Credential file saved to: " + credPath);
}

// GoogleCredential credenciales = GoogleCredential.FromFile(credPath).CreateScoped(new[] { "https://mail.google.com/" });

// Create Gmail API service.
var service = new GmailService(new BaseClientService.Initializer
{
    HttpClientInitializer = credential,
    ApplicationName = ApplicationName
});

// Define parameters of request.
UsersResource.LabelsResource.ListRequest request = service.Users.Labels.List("me");

// List labels.
IList<Google.Apis.Gmail.v1.Data.Label> labels = request.Execute().Labels;
Console.WriteLine("Labels:");
if (labels == null || labels.Count == 0)
{
    Console.WriteLine("No labels found.");
    return;
}


// Crear el objeto SaslMechanism a partir de las credenciales de Google OAuth2
string accessToken = await credential.GetAccessTokenForRequestAsync();
var networkCredential = new NetworkCredential(accessToken, string.Empty);
var saslMechanism = new SaslMechanismOAuth2(networkCredential);


// Define parameters of request to get messages from the inbox
UsersResource.MessagesResource.ListRequest messageRequest = service.Users.Messages.List("me");
messageRequest.LabelIds = "INBOX";
messageRequest.MaxResults = 40; // Adjust this value to get more or fewer messages

// Execute request and get messages
IList<Message> messages = messageRequest.Execute().Messages;

// Print the subject of each message
Console.WriteLine("\nMessages in Inbox:");
if (messages == null || messages.Count == 0)
    return;

foreach (var message in messages)
{
    // Get the full message
    UsersResource.MessagesResource.GetRequest messageGetRequest = service.Users.Messages.Get("me", message.Id);
    Message fullMessage = messageGetRequest.Execute();

    // Print the subject of the message
    var subjectHeader = fullMessage.Payload.Headers.FirstOrDefault(h => h.Name == "Subject");
    Console.WriteLine($"Subject:{Environment.NewLine}{subjectHeader?.Value ?? "(No subject)"}{Environment.NewLine}");

    // Print the body of the message
    Console.WriteLine($"Body:{fullMessage.Raw}{Environment.NewLine}");



    // Print the attachments of the message
    if (fullMessage.Payload.Parts != null && fullMessage.Payload.Parts.Any(x => x.Filename != null && x.Filename.Trim() != ""))
    {
        foreach (var part in fullMessage.Payload.Parts.Where(x => x.Filename != null && x.Filename.Trim() != ""))
        {

            Console.WriteLine($"Attachment: {part.Filename}, Mime: {part.MimeType}");

            var attachmentRequest = service.Users.Messages.Attachments.Get("me", fullMessage.Id, part.Body.AttachmentId);
            var attachment = attachmentRequest.Execute();
            string downloadPath = Path.Combine(@"c:\Users\jjimenezc\desarrollo\Google\Gmail", part.Filename);
            //if (part.MimeType.Contains("text"))
            //{
            //    byte[] bytes = Convert.FromBase64CharArray(attachment.Data.ToCharArray(), 0, attachment.Data.ToCharArray().Length);
            //    File.WriteAllBytes(downloadPath, bytes);
            //}
            //else
            //{

            //var a = WebEncoders.Base64UrlDecode(attachment.Data);
            string base64String = attachment.Data.Replace('-', '+').Replace('_', '/').PadRight(attachment.Data.Length + (4 - attachment.Data.Length % 4) % 4, '=');


            // Convert the URL-safe base64 string to a byte array
            byte[] decodedData = Convert.FromBase64String(base64String);

            //byte[] bytes = Encoding.Default.GetBytes(a);
            BinaryWriter writer = new BinaryWriter(new FileStream(downloadPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None));
            writer.Write(decodedData);
            writer.Close();
            //}

            Console.WriteLine($"Archivo '{part.Filename}' descargado en '{downloadPath}'.");
        }
    }
    else
    {
        Console.WriteLine($"No attachments found.{Environment.NewLine}");
    }

}






*/