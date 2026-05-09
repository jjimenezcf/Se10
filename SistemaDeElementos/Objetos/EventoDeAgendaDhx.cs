using System;
using GestorDeElementos;
using Microsoft.AspNetCore.Http;
using ModeloDeDto.Entorno;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Juridico;
using Utilidades;

namespace SistemaDeElementos.Objetos
{
    public class EventoDeAgendaDhx
    {

        public DateTime start_date { get; set; }

        public DateTime end_date { get; set; }

        public string text { get; set; }

        public string extra_data { get; set; }

    }

    public static class ExtexionDeEventoDeAgendaDhx
    {
        public static EventoDeAgendaDhx CrearDhx(this EventoDeAgendaDto eventoDto, ContextoSe contexto,HttpContext httpContext)
        {
            var eventoDhx = new EventoDeAgendaDhx();
            eventoDhx.start_date = eventoDto.Inicio;
            eventoDhx.end_date = eventoDto.Fin;
            if (eventoDto.IdNegocio > 0)
            {
                var elemento = NegociosDeSe.CrearGestor(contexto, eventoDto.IdNegocio).LeerRegistroPorId(eventoDto.IdElemento, aplicarJoin: false);

                var referencia = elemento.GetType().ImplementaUsaReferencia() ? $"({((IUsaReferencia)elemento).Referencia}) " : "";

                eventoDhx.text = $"<a href='{UriEvento(contexto,eventoDto, httpContext)}' target='_blank' " +
                    $"class='{enumCssAgenda.AjustarTextoAlDiv.Render()}'>{referencia}{eventoDto.Nombre}</a>";
            }
            else
            {
                eventoDhx.text = eventoDto.Nombre;
            }         
            eventoDhx.text = $"<div class='{enumCssAgenda.EventoAjustadoAlContenedor.Render()} {NegociosDeSe.ToEnumerado(eventoDto.IdNegocio).ToString().ToLower()}'>{eventoDhx.text}<span style='display: none'>{(!eventoDto.Descripcion.IsNullOrEmpty() ?eventoDto.Descripcion: eventoDto.Nombre)}</span></div>";

            return eventoDhx;
        }

        public static string UriEvento_1(ContextoSe contexto, EventoDeAgendaDto evento, HttpContext httpContext)
        {
            var negocio = NegociosDeSe.LeerNegocioPorId(evento.IdNegocio);
            var vista = contexto.SeleccionarPorPropiedad<VistaMvcDtm>(nameof(VistaMvcDtm.ElementoDto), negocio.ElementoDto); 
            string parametros = OtrosParametros(contexto, evento.IdNegocio, evento.IdElemento);
            var urlBase = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";
            return $"{urlBase}/{vista.Controlador}/{vista.Accion}?id={evento.IdElemento}{(parametros.IsNullOrEmpty() ? "" : parametros)}";
        }

        public static string UriEvento(ContextoSe contexto, EventoDeAgendaDto evento, HttpContext httpContext)
        {
            var negocio = NegociosDeSe.LeerNegocioPorId(evento.IdNegocio);
            var vista = contexto.SeleccionarPorPropiedad<VistaMvcDtm>(nameof(VistaMvcDtm.ElementoDto), negocio.ElementoDto);
            string parametros = OtrosParametros(contexto, evento.IdNegocio, evento.IdElemento);
            var urlBase = httpContext.Request.Host.Port is null 
                ? new UriBuilder(httpContext.Request.Scheme, httpContext.Request.Host.Host)
                : new UriBuilder(httpContext.Request.Scheme, httpContext.Request.Host.Host, (int) httpContext.Request.Host.Port);
            var uri = new UriBuilder(urlBase.Uri)
            {
                Path = $"/{vista.Controlador}/{vista.Accion}",
                Query = $"id={evento.IdElemento}{(parametros.IsNullOrEmpty() ? "" : parametros)}"
            };
            return uri.ToString();
        }



        private static string OtrosParametros(ContextoSe contexto, int idNegocio, int idElemento)
        {
            if (idNegocio == enumNegocio.Contrato.IdNegocio())
            {
                var contrato = contexto.SeleccionarPorId<ContratoDtm>(idElemento);
                return $"&{ltrParametrosEp.Clase}={contrato.ClaseDeContrato}";
            }
            return "";
        }
    }

}
