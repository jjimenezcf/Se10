using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Utilidades;

namespace ServicioDeCorreos;

// SMS: en modo CLIENTE abre la aplicación de mensajes del móvil con sms: y en modo SERVIDOR usa la api REST de un proveedor, de momento Twilio.
// Configuración (ServidorDeMensajes:SMS): AccountSid, AuthToken y Remitente (un número de Twilio o un remitente alfanumérico) y, opcional,
// Proveedor (TWILIO por defecto)
public class ServicioSMS : ServicioDeMensajesWebBase
{
    private const string Twilio = "TWILIO";

    public ServicioSMS(HttpClient cliente, IConfiguration configuracion) : base(cliente, configuracion) { }

    public override enumCanalDeMensajeria Canal => enumCanalDeMensajeria.SMS;

    public override string UrlParaElCliente(string telefono, string mensaje) => $"sms:+{telefono}?body={Uri.EscapeDataString(mensaje)}";

    public override async Task EnviarAsync(string telefono, string mensaje)
    {
        var proveedor = Valor(ltrAppSetting.ServidorDeMensajes.Sms.Proveedor, obligatorio: false, porDefecto: Twilio).ToUpper();
        if (proveedor != Twilio)
            throw new Exception($"El proveedor de SMS '{proveedor}' indicado en '{Configuracion.Path}:{ltrAppSetting.ServidorDeMensajes.Sms.Proveedor}' no está soportado, sólo {Twilio}");

        var sid = Valor(ltrAppSetting.ServidorDeMensajes.Sms.AccountSid);
        var token = Valor(ltrAppSetting.ServidorDeMensajes.Sms.AuthToken);
        var remitente = Valor(ltrAppSetting.ServidorDeMensajes.Sms.Remitente);

        var peticion = new HttpRequestMessage(HttpMethod.Post, $"https://api.twilio.com/2010-04-01/Accounts/{sid}/Messages.json")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string> { { "To", $"+{telefono}" }, { "From", remitente }, { "Body", mensaje } })
        };
        peticion.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{sid}:{token}")));
        await Enviar(peticion).ConfigureAwait(false);
    }
}
