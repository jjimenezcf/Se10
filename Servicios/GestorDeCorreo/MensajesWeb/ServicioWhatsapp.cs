using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Utilidades;

namespace ServicioDeCorreos;

// WhatsApp: en modo CLIENTE abre https://wa.me/ y en modo SERVIDOR usa la Cloud API de Meta (WhatsApp Business Platform).
// Configuración (ServidorDeMensajes:WHATSAPP): PhoneNumberId y Token (token de acceso de un usuario de sistema) y, opcionales, Version de la api,
// Plantilla e Idioma. Con texto libre sólo se puede escribir a quien haya escrito al número en las últimas 24 horas, fuera de esa ventana Meta
// obliga a usar una plantilla aprobada: si se indica Plantilla, el mensaje va como único parámetro {{1}} de su cuerpo
public class ServicioWhatsapp : ServicioDeMensajesWebBase
{
    private static readonly Regex _saltos = new Regex(@"\s*[\r\n\t]+\s*|\s{4,}", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public ServicioWhatsapp(HttpClient cliente, IConfiguration configuracion) : base(cliente, configuracion) { }

    public override enumCanalDeMensajeria Canal => enumCanalDeMensajeria.WHATSAPP;

    public override string UrlParaElCliente(string telefono, string mensaje) => $"https://wa.me/{telefono}?text={Uri.EscapeDataString(mensaje)}";

    public override async Task EnviarAsync(string telefono, string mensaje)
    {
        var version = Valor(ltrAppSetting.ServidorDeMensajes.Whatsapp.Version, obligatorio: false, porDefecto: "v21.0");
        var idDelNumero = Valor(ltrAppSetting.ServidorDeMensajes.Whatsapp.PhoneNumberId);
        var token = Valor(ltrAppSetting.ServidorDeMensajes.Whatsapp.Token);
        var plantilla = Valor(ltrAppSetting.ServidorDeMensajes.Whatsapp.Plantilla, obligatorio: false);

        object cuerpo = plantilla.IsNullOrEmpty()
            ? new { messaging_product = "whatsapp", to = telefono, type = "text", text = new { body = mensaje } }
            : new
            {
                messaging_product = "whatsapp",
                to = telefono,
                type = "template",
                template = new
                {
                    name = plantilla,
                    language = new { code = Valor(ltrAppSetting.ServidorDeMensajes.Whatsapp.Idioma, obligatorio: false, porDefecto: "es") },
                    components = new[] { new { type = "body", parameters = new[] { new { type = "text", text = ParaPlantilla(mensaje) } } } }
                }
            };

        var peticion = new HttpRequestMessage(HttpMethod.Post, $"https://graph.facebook.com/{version}/{idDelNumero}/messages")
        {
            Content = new StringContent(JsonSerializer.Serialize(cuerpo), Encoding.UTF8, MimeTypeMap.ApplicationJson)
        };
        peticion.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        await Enviar(peticion).ConfigureAwait(false);
    }

    // Meta rechaza como parámetro de una plantilla los saltos de línea, tabuladores y más de 3 espacios seguidos
    private static string ParaPlantilla(string mensaje) => _saltos.Replace(mensaje, " ").Trim();
}
