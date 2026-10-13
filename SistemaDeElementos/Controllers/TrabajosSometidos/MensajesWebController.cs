using AutoMapper;
using Gestor.Errores;
using GestorDeElementos;
using GestoresDeNegocio.TrabajosSometidos;
using Microsoft.AspNetCore.Mvc;
using ModeloDeDto.TrabajosSometidos;
using ServicioDeDatos;
using ServicioDeDatos.Seguridad;
using System;
using System.Collections.Generic;
using Utilidades;

namespace MVCSistemaDeElementos.Controllers
{
    // Endpoints de los mensajes a un teléfono por WhatsApp, Telegram o SMS, ver ExtensorDeMensajesWeb
    public class MensajesWebController : BaseController<MensajeWebDto>
    {
        public MensajesWebController(GestorDeErrores gestorDeErrores, ContextoSe contexto, IMapper mapeador)
        : base(gestorDeErrores, contexto, mapeador)
        {
        }

        // Canal y modo de los mensajes (variables CFG_Mensajeria_Canal y CFG_Mensajeria_Modo): la interfaz lo lee para pintar el botón junto
        // al teléfono con el icono del canal y para saber si ha de abrir una ventana al enviar
        public JsonResult epLeerConfiguracionDeMensajesWeb(string parametrosJson)
        {
            var r = new Resultado();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                r.Datos = ExtensorDeMensajesWeb.LeerConfiguracionDeMensajesWeb();
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "Error al leer la configuración de los mensajes.");
            }
            return new JsonResult(r);
        }

        // Envía el mensaje al teléfono indicado por el canal configurado; si el envío lo ha de hacer el cliente devuelve en Datos.Url el enlace
        // con el que la interfaz abre la aplicación del canal con el mensaje escrito, si lo hace el servidor Datos.Url es null
        public JsonResult epEnviarMensajeWeb(string parametrosJson)
        {
            var r = new Resultado();
            Dictionary<string, object> parametros = parametrosJson.ToDiccionarioDeParametros();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                var negocio = NegociosDeSe.ToEnumerado((int)parametros.LeerValor<long>(ltrParametrosEp.idNegocio));
                var idElemento = (int)parametros.LeerValor<long>(ltrParametrosEp.idElemento);

                var url = Contexto.EnviarMensajeWeb(negocio, idElemento
                    , parametros.LeerValor<string>(nameof(EnviarMensajeWebDto.Para), null)
                    , parametros.LeerValor<string>(nameof(EnviarMensajeWebDto.Mensaje), null));

                r.Datos = new { Url = url };
                r.Mensaje = url.IsNullOrEmpty() ? "Mensaje enviado" : "Mensaje preparado, pulse enviar en la aplicación para mandarlo";
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "Error al enviar el mensaje.");
            }
            return new JsonResult(r);
        }
    }
}
