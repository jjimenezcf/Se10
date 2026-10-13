using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Utilidades;

namespace ServicioDeCorreos;

// Telegram: en modo CLIENTE abre el chat con https://t.me/+teléfono (Telegram no permite precargar el texto) y en modo SERVIDOR usa la Bot API.
// Un bot no puede escribir a un teléfono, sólo a un chat que haya empezado con él (/start): ServidorDeMensajes:TELEGRAM:ChatIds relaciona
// cada teléfono (formato internacional sólo dígitos) con su chat_id. Además necesita BotToken, el que da @BotFather
public class ServicioTelegram : ServicioDeMensajesWebBase
{
    public ServicioTelegram(HttpClient cliente, IConfiguration configuracion) : base(cliente, configuracion) { }

    public override enumCanalDeMensajeria Canal => enumCanalDeMensajeria.TELEGRAM;

    public override string UrlParaElCliente(string telefono, string mensaje) => $"https://t.me/+{telefono}";

    public override async Task EnviarAsync(string telefono, string mensaje)
    {
        var token = Valor(ltrAppSetting.ServidorDeMensajes.Telegram.BotToken);
        var chatId = Configuracion.GetSection(ltrAppSetting.ServidorDeMensajes.Telegram.ChatIds)[telefono];
        if (chatId.IsNullOrEmpty())
            throw new Exception($"El teléfono '+{telefono}' no tiene chat de Telegram, debe escribir /start al bot y relacionarse en '{Configuracion.Path}:{ltrAppSetting.ServidorDeMensajes.Telegram.ChatIds}'");

        var peticion = new HttpRequestMessage(HttpMethod.Post, $"https://api.telegram.org/bot{token}/sendMessage")
        {
            Content = new StringContent(JsonSerializer.Serialize(new { chat_id = chatId, text = mensaje }), Encoding.UTF8, MimeTypeMap.ApplicationJson)
        };
        await Enviar(peticion).ConfigureAwait(false);
    }
}
