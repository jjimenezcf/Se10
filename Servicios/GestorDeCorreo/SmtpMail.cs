
using MimeKit;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Utilidades;
using System;
using MailKit.Security;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;

namespace ServicioDeCorreos
{


    public class SmtpMail : IDistribuidorSmtp
    {
        public class Tenante : ITenant
        {
            public string ServidorSmtp  { get; set; }
            public int PuertoSmtp { get; set; }

            public string ServidorImap { get; set; }
            public int PuertoImap { get; set; }

        }

        Tenante _tenant { get; set; }

        public SmtpMail(Tenante tenante, IConfiguration configuration)
        {
            var seccionDeServicioDeCorreo = configuration.GetSection(typeof(ltrAppSetting.ServidorDeCorreo).Name);
            if (seccionDeServicioDeCorreo is null) 
                throw new Exception($"Debe definir la sección de '{typeof(ltrAppSetting.ServidorDeCorreo).Name}' en el AppSetting");
        }

        public async Task EnviarAsyn(ITenant tenante, string emisor, string receptores, string asunto, string mensaje, List<string> archivos)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(((Tenante)tenante).clientId ?? "NoReply", emisor));
            message.To.Add(MailboxAddress.Parse(receptores));
            message.Subject = asunto;

            var builder = new BodyBuilder();
            builder.TextBody = mensaje;
            builder.HtmlBody = $"<html><body>{mensaje}</body></html>";

            foreach (var archivo in archivos)
            {
                if (File.Exists(archivo))
                {
                    builder.Attachments.Add(archivo);
                }
            }
            message.Body = builder.ToMessageBody();

            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                await client.ConnectAsync(((Tenante)tenante).ServidorSmtp, ((Tenante)tenante).PuertoSmtp, SecureSocketOptions.SslOnConnect);
                await client.AuthenticateAsync(((Tenante)tenante).clientId, ((Tenante)tenante).clientSecret);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }

            using (var imapClient = new ImapClient())
            {
                await imapClient.ConnectAsync(((Tenante)tenante).ServidorImap, ((Tenante)tenante).PuertoImap, SecureSocketOptions.SslOnConnect);
                await imapClient.AuthenticateAsync(((Tenante)tenante).clientId, ((Tenante)tenante).clientSecret);
                var sent = imapClient.GetFolder(SpecialFolder.Sent);
                await sent.OpenAsync(FolderAccess.ReadWrite);
                await sent.AppendAsync(message, MessageFlags.Seen);
                await imapClient.DisconnectAsync(true);
            }
        }
        public async Task<int> EliminarAsyn(ITenant tenante, int anterioresA)
        {
            int correosBorrados = 0;

            using (var imapClient = new ImapClient())
            {
                await imapClient.ConnectAsync(((Tenante)tenante).ServidorImap, ((Tenante)tenante).PuertoImap, SecureSocketOptions.SslOnConnect);
                await imapClient.AuthenticateAsync(((Tenante)tenante).clientId, ((Tenante)tenante).clientSecret);

                var sent = imapClient.GetFolder(SpecialFolder.Sent);
                await sent.OpenAsync(FolderAccess.ReadWrite);

                // Obtener todos los mensajes
                var uids = await sent.SearchAsync(SearchQuery.All);

                // Fecha límite
                var fechaLimite = DateTime.Now.AddDays(-1 * anterioresA);

                foreach (var uid in uids)
                {
                    var message = await sent.GetMessageAsync(uid);
                    if (message.Date.DateTime < fechaLimite)
                    {
                        // Marcar el mensaje para eliminación
                        await sent.AddFlagsAsync(uid, MessageFlags.Deleted, true);
                        correosBorrados++;
                    }
                }

                // Eliminar permanentemente los mensajes marcados
                await sent.ExpungeAsync();

                await imapClient.DisconnectAsync(true);
            }

            return correosBorrados;
        }


    }
}