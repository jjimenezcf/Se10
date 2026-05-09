using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Entorno;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ModeloDeDto.Entorno;
using MVCSistemaDeElementos.Descriptores;
using Newtonsoft.Json;
using ServicioDeDatos;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Guarderias;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Ventas;
using SistemaDeElementos.Objetos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Utilidades;
using static Gestor.Errores.GestorDeErrores;

namespace MVCSistemaDeElementos.Controllers
{
    public class VisorDeAgendaController : EntidadController<ContextoSe, EventoDeAgendaDtm, EventoDeAgendaDto>
    {
        public VisorDeAgendaController(GestorDeEventosDeAgenda gestor, GestorDeErrores gestorDeErrores)
         : base
         (
           gestor,
           gestorDeErrores
         )
        {
        }

        public IActionResult MiCalendario()
        {
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);
                return ViewCrud(new DescriptorDeMiCalendario(Contexto, ModoDescriptor.Mantenimiento));
            }
            catch (Exception e)
            {
                return RenderMensaje(e.Message);
            }
        }
        public IActionResult VisorDeAgenda()
        {
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);
                ViewBag.DatosDeConexion = DatosDeConexion;

                if (!HttpContext.Request.Query.ContainsKey(ltrParametrosEp.guid))
                    throw new Exception("Debe indicar el guid de operación");

                if (!HttpContext.Request.Query.ContainsKey(ltrParametrosEp.idAgenda))
                    throw new Exception("Debe indicar la agenda a tratar");

                var fecha = DateTime.Today;
                if (HttpContext.Request.Query.ContainsKey(ltrParametrosEp.fecha))
                {
                    fecha = DateTime.Parse(HttpContext.Request.Query[ltrParametrosEp.fecha].ToString());
                }

                var idAgenda = HttpContext.Request.Query[ltrParametrosEp.idAgenda].ToString().Entero();
                var restrictores = $"{idAgenda}|{fecha}";

                var agenda = Contexto.SeleccionarPorId<AgendaDtm>(idAgenda);
                if (!agenda.PuedeConsultarla(Contexto))
                    Emitir("No tiene permisos para acceder a la agenda, solicítelos");

                var agendas = ServicioDeCaches.Obtener(CacheDe.VisorDeAgenda);

                if (agendas.ContainsKey(HttpContext.Request.Query[ltrParametrosEp.guid]) && (string)agendas[HttpContext.Request.Query[ltrParametrosEp.guid]] != restrictores)
                    throw new Exception("No puede modificar los datos de la Url");

                if (agendas.ContainsKey(HttpContext.Request.Query[ltrParametrosEp.guid]))
                    return DevolverPanelDeControlConMensaje("No se puede recargar esta página, vuélvala abrir");

                agendas[HttpContext.Request.Query[ltrParametrosEp.guid]] = restrictores;

                return ViewCrud(new DescriptorDelVisorDeAgenda(Contexto, ModoDescriptor.Mantenimiento, fecha));
            }
            catch (Exception e)
            {
                if (Excepciones.EsElCodigoError(e, enumCodigoDeError.ElementoYaRenderizado))
                    return VisorDeAgenda();
                return RenderMensaje(e.Message);
            }
        }


        public JsonResult epIrAlElementoDelEvento(string parametrosJson)
        {
            var r = new Resultado();

            Dictionary<string, object> parametros = parametrosJson.ToDiccionarioDeParametros();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);

                var idEvento = (int)parametros.LeerValor<long>(nameof(EventoDeAgendaDtm.Id));
                var evento = Contexto.SeleccionarPorId<EventoDeAgendaDtm>(idEvento);
                var negocio = NegociosDeSe.ToEnumerado(evento.IdNegocio);
                var vista = negocio.VistaMvc(Contexto);
                r.Datos = $"{negocio.Controlador()}/{vista.Accion}?Id={evento.IdElemento}";
                r.Consola = $"Origen obtenido correctamente";
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "No se puede obtener el origen del evento.");
            }
            return new JsonResult(r);
        }

        public IEnumerable<EventoDeAgendaDhx> LeerEventos(int timeshift, DateTime from, DateTime to)
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);

            var urlOrigen = HttpContext.Request.Headers.Referer;

            var uri = new Uri(urlOrigen);
            var parametros = HttpUtility.ParseQueryString(uri.Query);

            if (!parametros.AllKeys.ToList().Contains(ltrParametrosEp.guid))
                GestorDeErrores.Emitir("Es necesario el código operacional para acceder a la agenda");

            var agendas = ServicioDeCaches.Obtener(CacheDe.VisorDeAgenda);

            var restrictores = agendas.ContainsKey(parametros[ltrParametrosEp.guid])
                ? ((string)agendas[parametros[ltrParametrosEp.guid]]).Split('|')
                : ((string)parametros[ltrParametrosEp.idAgenda]).Split('|');
            var idAgenda = restrictores[0];

            Contexto.IniciarTraza("Obtener eventos");
            try
            {
                var eventosDto = ((GestorDeEventosDeAgenda)_GestorDeElementos).LeerEventos(idAgenda.Entero(), from, to);
                List<EventoDeAgendaDhx> eventosDhx = new List<EventoDeAgendaDhx>();
                var eventosMapeados = ServicioDeCaches.Obtener(CacheDe.EventosMapeado);
                foreach (var evento in eventosDto)
                {
                    if (evento.Fin < from)
                        continue;
                    var indice = parametros[ltrParametrosEp.guid] + "-" + evento.Id.ToString();
                    if (eventosMapeados.ContainsKey(indice))
                        continue;

                    eventosDhx.Add(evento.CrearDhx(Contexto, HttpContext));
                    eventosMapeados[indice] = true;
                }
                return eventosDhx;
            }
            finally
            {
                Contexto.CerrarTraza();
            }
        }

        protected override ParametrosDeNegocio AntesDeEjecutar_ModificarRelacion(EventoDeAgendaDto evento)
        {
            var parametrosDeNegocio = base.AntesDeEjecutar_ModificarRelacion(evento);
            var gestor = GestorDeEventosDeAgenda.Gestor(Contexto, Contexto.Mapeador).LeerElementoPorId(evento.Id);

            return parametrosDeNegocio;
        }

        protected override EventoDeAgendaDto LeerPorId(int id, Dictionary<string, object> parametros)
        {
            if (Contexto.DatosDeConexion.EsAdministrador) parametros[ltrParametrosNeg.ValidarPermisosDeConsulta] = false;

            var evento = base.LeerPorId(id, parametros);
            evento.Url = new Uri(ExtexionDeEventoDeAgendaDhx.UriEvento(Contexto, evento, HttpContext));
            return evento;
        }

        public JsonResult epLeerLosEventosDel(string filtrosJson, string parametrosJson)
        {
            var r = new Resultado();
            Contexto.IniciarTraza(GetType().Name + "_" + nameof(epLeerLosEventosDel));
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                var parametros = parametrosJson.ToDiccionarioDeParametros();
                parametros[ltrParametrosNeg.EsUnaPeticion] = true;
                parametros[ltrParametrosNeg.Peticion] = enumPeticion.epLeerElementos;
                List<ClausulaDeFiltrado> filtros = JsonConvert.DeserializeObject<List<ClausulaDeFiltrado>>(filtrosJson);
                var negocio = NegociosDeSe.ToEnumerado(filtros.First(f => f.Clausula.ToLower() == nameof(EventoDeAgendaDto.IdNegocio).ToLower()).Valor.Entero());
                var idElemento = filtros.First(x => x.Clausula.ToLower() == nameof(EventoDeAgendaDto.IdElemento).ToLower()).Valor.Entero();
                var eventos = ((GestorDeEventosDeAgenda)_GestorDeElementos).LeerEventosCercanos(negocio, idElemento);
                r.Datos = eventos;
                r.Total = eventos.Count();
                r.Estado = enumEstadoPeticion.Ok;
                r.Consola = $"eventos leidos";
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "Error al leer.");
            }
            finally
            {
                Contexto.CerrarTraza();
            }
            return new JsonResult(r);
        }

        [AllowAnonymous]
        public JsonResult epLeerLosEventosPorGuid(string filtrosJson, string parametrosJson)
        {
            var r = new Resultado();
            Contexto.IniciarTraza(GetType().Name + "_" + nameof(epLeerLosEventosPorGuid));
            try
            {
                Contexto.AsignarUsuario(ExtensorDeUsuarios.Administrador(Contexto));
                var parametros = extJson.ToDiccionarioDeParametros(parametrosJson);
                var guid = parametros.LeerValor<string>(ltrParametrosEp.guid);
                var id = (int)parametros.LeerValor<long>(ltrParametrosEp.id);

                List<ClausulaDeFiltrado> filtros = JsonConvert.DeserializeObject<List<ClausulaDeFiltrado>>(filtrosJson);
                var negocio = NegociosDeSe.ToEnumerado(filtros.First(f => f.Clausula.ToLower() == nameof(EventoDeAgendaDto.IdNegocio).ToLower()).Valor.Entero());
                var idElemento = filtros.First(x => x.Clausula.ToLower() == nameof(EventoDeAgendaDto.IdElemento).ToLower()).Valor.Entero();

                if (idElemento != id)
                    Emitir("Criterios de consulta erroneos");

                ValidarConsultaPorGuid(negocio, id, guid);

                var agendas = new List<int>();
                if (negocio == enumNegocio.Infante)
                {
                    var infante = Contexto.SeleccionarPorId<InfanteDtm>(idElemento);
                    agendas.Add(infante.IdAgenda);
                    agendas.Add(infante.CursoEnElQueEsta(Contexto).IdAgenda);
                }

                var eventos = negocio != enumNegocio.Infante
                ? ((GestorDeEventosDeAgenda)_GestorDeElementos).LeerEventosCercanos(negocio, idElemento)
                : ((GestorDeEventosDeAgenda)_GestorDeElementos).LeerEventosCercanos(agendas, negocio);

                r.Datos = eventos;
                r.Total = eventos.Count();
                r.Estado = enumEstadoPeticion.Ok;
                r.Consola = $"eventos leidos";
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "Error al leer.");
            }
            finally
            {
                Contexto.CerrarTraza();
            }
            return new JsonResult(r);
        }


    }
}
