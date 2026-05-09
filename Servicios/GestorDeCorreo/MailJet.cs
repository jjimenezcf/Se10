using Mailjet.Client;
using Mailjet.Client.Resources;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Utilidades;

namespace ServicioDeCorreos
{


    public class MailJet : IDistribuidorMailJet
    {
        public class Tenante : ITenant
        {
            public string Emisor { get; set; }
            public string eMailEmisor => tenantId;
            public string ApiKey => clientId;
            public string ApiSecret => clientSecret;

        }

        MailjetClient MailjetClient { get; set; }
        Tenante _tenant { get; set; }

        public MailJet(Tenante tenante, IConfiguration configuration)
        {
            var servicioDeCorreo = configuration.GetSection(typeof(ltrAppSetting.ServidorDeCorreo).Name).GetSection(typeof(ltrAppSetting.ServidorDeCorreo.MailJet).Name.ToUpper());
            _tenant = tenante;
            _tenant.clientId = servicioDeCorreo.GetSection(ltrAppSetting.ServidorDeCorreo.MailJet.ApiKey).Value;
            _tenant.clientSecret = servicioDeCorreo.GetSection(ltrAppSetting.ServidorDeCorreo.MailJet.ApiSecret).Value;
            _tenant.tenantId = servicioDeCorreo.GetSection(ltrAppSetting.ServidorDeCorreo.MailJet.eMailEmisor).Value;
        }

        public async Task EnviarAsyn(ITenant tenante, string emisor, string receptores, string asunto, string mensaje, List<string> archivos)
        {

            MailjetClient = new MailjetClient(((MailJet.Tenante) tenante).ApiKey, ((MailJet.Tenante)tenante).ApiSecret);

            var attachments = new JArray();
            foreach (var archivo in archivos)
            {
                if (File.Exists(archivo))
                {
                    byte[] fileBytes = File.ReadAllBytes(archivo);
                    string base64File = Convert.ToBase64String(fileBytes);
                    attachments.Add(new JObject {
                             {"ContentType", MimeTypeMap.ApplicationOctetStream},
                             {"Filename", Path.GetFileName(archivo)},
                             {"Base64Content", base64File} });
                }
            }

            MailjetRequest request = new MailjetRequest
            {
                Resource = Send.Resource,
            }
            .Property(Send.FromEmail, emisor)
            .Property(Send.FromName, ((Tenante)tenante).Emisor ?? "NoReply")
            .Property(Send.Subject, asunto)
            .Property(Send.TextPart, mensaje)
            .Property(Send.HtmlPart, $"<html><body>{mensaje}</body></html>")
            .Property(Send.Recipients, new JArray {
                new JObject {
                    {"Email", receptores}
                }
            })
            .Property(Send.Attachments, attachments);

            MailjetResponse response = await MailjetClient.PostAsync(request);
            if (response.StatusCode != 200)
            {
                throw new Exception(response.Content.ToString());
            }
        }
    }
}

//{ "HTMLPart", $"<h3>{cuerpo}<h3>" }
