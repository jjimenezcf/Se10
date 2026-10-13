using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Utilidades;

namespace ServicioDeCorreos;

// Base de los servicios de mensajes web: cada canal lee su configuración (credenciales, ...) de la sección ServidorDeMensajes:{CANAL}
// del appsettings, nunca de las variables del sistema, que están en base de datos y se ven en pantalla
public abstract class ServicioDeMensajesWebBase : IServicioDeMensajesWeb
{
    private readonly IConfiguration _configuracion;
    protected HttpClient Cliente { get; }

    protected ServicioDeMensajesWebBase(HttpClient cliente, IConfiguration configuracion)
    {
        Cliente = cliente;
        Cliente.Timeout = TimeSpan.FromSeconds(20);
        _configuracion = configuracion;
    }

    public abstract enumCanalDeMensajeria Canal { get; }

    public abstract string UrlParaElCliente(string telefono, string mensaje);

    public abstract Task EnviarAsync(string telefono, string mensaje);

    protected IConfigurationSection Configuracion => _configuracion.GetSection(ltrAppSetting.ServidorDeMensajes.Seccion).GetSection(Canal.ToString());

    // La configuración se lee al enviar, no al construir el servicio, para que un canal sin configurar no impida usar los demás
    protected string Valor(string clave, bool obligatorio = true, string porDefecto = null)
    {
        var valor = Configuracion[clave];
        if (!valor.IsNullOrEmpty())
            return valor.Trim();
        if (obligatorio)
            throw new Exception($"Falta por definir '{Configuracion.Path}:{clave}' en la configuración");
        return porDefecto;
    }

    protected async Task Enviar(HttpRequestMessage peticion)
    {
        using var respuesta = await Cliente.SendAsync(peticion).ConfigureAwait(false);
        if (respuesta.IsSuccessStatusCode)
            return;

        // no se incluye la url en el error: la de Telegram lleva el token del bot
        var cuerpo = await respuesta.Content.ReadAsStringAsync().ConfigureAwait(false);
        throw new Exception($"{Canal} respondió {(int)respuesta.StatusCode}: {cuerpo}");
    }
}
