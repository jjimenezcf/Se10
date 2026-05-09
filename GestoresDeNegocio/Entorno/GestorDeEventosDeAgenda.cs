using AutoMapper;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using Microsoft.EntityFrameworkCore;
using ModeloDeDto.Entorno;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.Seguridad;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilidades;
using static Gestor.Errores.GestorDeErrores;

namespace GestoresDeNegocio.Entorno
{
    public class GestorDeEventosDeAgenda : GestorDeElementos<ContextoSe, EventoDeAgendaDtm, EventoDeAgendaDto>
    {
        public override enumNegocio Negocio => enumNegocio.EventoDeAgenda;

        public class ltrEventosDelCalendario
        {
            public const string MostrarAgenda = nameof(MostrarAgenda);
            public const string DarPorNotificado = nameof(DarPorNotificado);
        }

        public class MapearEventosDelCalendario : Profile
        {
            public MapearEventosDelCalendario()
            {
                CreateMap<EventoDeAgendaDtm, EventoDeAgendaDto>()
                .ForMember(x => x.Agenda, y => y.MapFrom(y => y.Agenda.Nombre))
                .ForMember(x => x.Url, y => y.Ignore())
                .ForMember(x => x.Elemento, y => y.Ignore())
                .ForMember(x => x.Referencia, y => y.Ignore());
                CreateMap<EventoDeAgendaDto, EventoDeAgendaDtm>()
                .ForMember(x => x.Agenda, y => y.Ignore());
            }
        }

        public GestorDeEventosDeAgenda(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        public static GestorDeEventosDeAgenda Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeEventosDeAgenda(contexto, mapeador);
        }

        protected override IQueryable<EventoDeAgendaDtm> AplicarJoins(IQueryable<EventoDeAgendaDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(x => x.Agenda);
            return consulta;
        }

        protected override IQueryable<EventoDeAgendaDtm> AplicarSeguridad(IQueryable<EventoDeAgendaDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarSeguridad(consulta, filtros, parametros);

            if (Contexto.EsConsultaPorGuid == false)
            {
                var permisosDeElUsuario = Contexto.Set<UsuariosDeUnPermisoDtm>().Where(permisos => permisos.IdUsuario == Contexto.DatosDeConexion.IdUsuario);
                var agendasDeUsuario = Contexto.Set<AgendaDtm>().Where(x => permisosDeElUsuario.Any(p => p.IdPermiso == x.IdInterventor || p.IdPermiso == x.IdGestor || p.IdPermiso == x.IdConsultor));
                consulta = consulta.Where(eventos => agendasDeUsuario.Any(agendas => agendas.Id == eventos.IdAgenda));
            }
            return consulta;
        }

        protected override void AntesDePersistir(EventoDeAgendaDtm evento, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(evento, parametros);


            if (parametros.Modificando) evento.EsDelSistema = ((EventoDeAgendaDtm)parametros.registroEnBd).EsDelSistema;
            if (parametros.Modificando) evento.Notificado = ((EventoDeAgendaDtm)parametros.registroEnBd).Notificado;

            if (parametros.Eliminando && evento.Notificado.EsTrue())
                Emitir($"Un evento notificado no puede ser eliminado");

            if (parametros.EsUnaPeticion && parametros.Peticion == enumPeticion.epBorrarVinculo && evento.EsDelSistema)
                Emitir($"Un evento del sistema no puede ser eliminado por el usuario");

            if (parametros.Modificando && parametros.EsUnaPeticion && parametros.Peticion == enumPeticion.PersistirElemento && evento.EsDelSistema)
                Emitir($"Un evento del sistema no puede ser modificado por el usuario");

            if (!parametros.Eliminando)
            {
                if (evento.Fin < evento.Inicio)
                    Emitir($"La fecha de inicio del evento, '{evento.Inicio}' ha de ser anterior a la de su finalización, '{evento.Inicio}'");
                else
                    if (evento.Fin.Date == evento.Inicio.Date && evento.Fin.TimeOfDay.Ticks - evento.Inicio.TimeOfDay.Ticks < 0)
                        Emitir($"La fecha de inicio del evento, '{evento.Inicio}' ha de ser anterior a la de su finalización, '{evento.Inicio}'");

                if (parametros.EsUnaPeticion && evento.AvisarAntesDe is not null && evento.MedidoEn is not null)
                {
                    if (
                    parametros.Insertando ||
                    evento.SeHaModificadoElCampo<int>(x => x.Name == nameof(EventoDeAgendaDtm.AvisarAntesDe), parametros) ||
                    evento.SeHaModificadoElCampo<Enumerados.enumDurabilidad>(x => x.Name == nameof(EventoDeAgendaDtm.MedidoEn), parametros))
                    {
                        var ticks = evento.MedidoEn == Enumerados.enumDurabilidad.Dias
                            ? TimeSpan.TicksPerDay * evento.AvisarAntesDe.Entero()
                            : evento.MedidoEn == Enumerados.enumDurabilidad.Horas
                            ? TimeSpan.TicksPerHour * evento.AvisarAntesDe.Entero()
                            : TimeSpan.TicksPerMinute * evento.AvisarAntesDe.Entero();
                        if (evento.Inicio.Ticks - ticks < DateTime.Now.Ticks)
                            Emitir($"No se puede avisar en la fecha solicitada '{new DateTime(evento.Inicio.Ticks - ticks)}' por que es anterior al momento actual");
                    }
                }

                if (evento.AvisarAntesDe is not null && evento.MedidoEn is null)
                    Emitir($"Debe indicar si se quiere que se le avise antes de {evento.AvisarAntesDe.Entero()} días, horas o minutos");

                if (evento.AvisarAntesDe is null && evento.MedidoEn is not null)
                    Emitir($"Debe indicar el número de  {evento.MedidoEn.Descripcion()} antes de la fecha '{evento.Inicio}'  a avisar");

                if (parametros.Insertando) evento.Notificado = evento.AvisarAntesDe is not null ? false : null;
                if (parametros.Modificando && parametros.Parametros.LeerValor(ltrEventosDelCalendario.DarPorNotificado, false))
                    evento.Notificado = true;

            }

            if (parametros.Insertando) evento.EsDelSistema = !parametros.EsUnaPeticion;

            if (parametros.Insertando && parametros.InsertandoParaVincular)
            {
                if (evento.IdNegocio == 0) evento.IdNegocio = parametros.Parametros.LeerValor<int>(ltrParametrosNeg.IdNegocio);
                if (evento.IdElemento == 0) evento.IdElemento = parametros.Parametros.LeerValor<int>(ltrParametrosNeg.IdElemento);
            }

            if (parametros.Modificando || parametros.Eliminando)
            {
                evento.IdNegocio = ((EventoDeAgendaDtm)parametros.registroEnBd).IdNegocio;
                evento.IdElemento = ((EventoDeAgendaDtm)parametros.registroEnBd).IdElemento;
            }
        }

        protected override void DespuesDePersistir(EventoDeAgendaDtm evento, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(evento, parametros);
            evento.Agenda(Contexto).GenerarAgendaIcs(Contexto);
        }

        protected override void ObtenerModoDeAccesoAlElementoQueSeDevuelve(EventoDeAgendaDtm evento, EventoDeAgendaDto elemento, ParametrosDeNegocio parametros)
        {
            base.ObtenerModoDeAccesoAlElementoQueSeDevuelve(evento, elemento, parametros);

            if (ApiDePermisos.EsGestorDeLaAgenda(Contexto, evento.Agenda == null ? Contexto.SeleccionarPorId<AgendaDtm>(evento.IdAgenda) : evento.Agenda))
                elemento.ModoDeAcceso = enumModoDeAccesoDeDatos.Gestor;
            else
                elemento.ModoDeAcceso = ApiDePermisos.EsConsultorDeLaAgenda(Contexto, evento.Agenda == null ? Contexto.SeleccionarPorId<AgendaDtm>(evento.IdAgenda) : evento.Agenda)
                ? enumModoDeAccesoDeDatos.Consultor
                : enumModoDeAccesoDeDatos.SinPermiso;

            if (evento.EsDelSistema && elemento.ModoDeAcceso == enumModoDeAccesoDeDatos.Gestor)
                elemento.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor;

            if (parametros.Operacion == enumTipoOperacion.Modificar || parametros.Operacion == enumTipoOperacion.Eliminar)
                ServicioDeCaches.EliminarElementos(CacheDe.EventosMapeado, $"-{evento.Id}");
        }

        protected override void DespuesDeMapearElElemento(EventoDeAgendaDtm eventoDtm, EventoDeAgendaDto eventoDto, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(eventoDtm, eventoDto, parametros);

            if (eventoDtm.EsDelSistema || eventoDtm.Notificado.EsTrue())
            {
                eventoDto.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor;
                eventoDto.informacion = "Un evento enviado o del sistema no es modificable";
            }

            if (parametros.Peticion == enumPeticion.epLeerPorId)
            {
                var elemento = NegociosDeSe.ToEnumerado(eventoDtm.IdNegocio).SeleccionarPorId(Contexto, eventoDtm.IdElemento);
                eventoDto.Elemento = ((INombre)elemento).Expresion;
            }
            if (parametros.Peticion == enumPeticion.epLeerDatosParaElGrid)
            {
                var elemento = NegociosDeSe.ToEnumerado(eventoDtm.IdNegocio).SeleccionarPorId(Contexto, eventoDtm.IdElemento);
                eventoDto.Referencia = ((ITieneReferencia)elemento).Referencia;
                eventoDto.Accion = "Abrir agenda";
            }
        }

        public List<EventoDeAgendaDto> LeerEventos(int idAgenda, DateTime desde, DateTime hasta)
        {
            var fechaDesde = desde.AddDays(-7);
            var fechaHasta = hasta;
            var filtros = new List<ClausulaDeFiltrado>
            {
                new ClausulaDeFiltrado(nameof(EventoDeAgendaDtm.IdAgenda), enumCriteriosDeFiltrado.igual, idAgenda),
                new ClausulaDeFiltrado(nameof(EventoDeAgendaDtm.Inicio), enumCriteriosDeFiltrado.mayorIgual, fechaDesde.ToString()),
                new ClausulaDeFiltrado(nameof(EventoDeAgendaDtm.Fin), enumCriteriosDeFiltrado.menorIgual, fechaHasta.ToString())
            };

            var ordenacion = new List<ClausulaDeOrdenacion>();
            var opciones = new Dictionary<string, object>();
            opciones[nameof(ltrParametrosNeg.Peticion)] = enumPeticion.LeerEventos;
            var eventosDto = Gestor(Contexto, Contexto.Mapeador).LeerElementos(0, -1, filtros, ordenacion, opciones);
            return eventosDto.ToList();
        }

        public List<EventoDeAgendaDto> LeerEventosCercanos(enumNegocio negocio, int idElemento)
        {
            var diasDesde = negocio.Parametro(enumParametroAgenda.Agenda_Leer_Desde, emitirError: false, crearParametro: true, -1).Valor.Entero();
            var diasHasta = negocio.Parametro(enumParametroAgenda.Agenda_Leer_Hasta, emitirError: false, crearParametro: true, 7).Valor.Entero();
            return LeerEventos(negocio, idElemento, agendas: null, DateTime.Now.Date.AddDays(diasDesde), DateTime.Now.Date.AddDays(diasHasta));
        }

        public List<EventoDeAgendaDto> LeerEventosCercanos(List<int> agendas, enumNegocio negocio)
        {
            var diasDesde = negocio.Parametro(enumParametroAgenda.Agenda_Leer_Desde, emitirError: false, crearParametro: true, -1).Valor.Entero();
            var diasHasta = negocio.Parametro(enumParametroAgenda.Agenda_Leer_Hasta, emitirError: false, crearParametro: true, 7).Valor.Entero();
            return LeerEventos(negocio, idElemento: null, agendas, DateTime.Now.Date.AddDays(diasDesde), DateTime.Now.Date.AddDays(diasHasta));
        }

        private List<EventoDeAgendaDto> LeerEventos(enumNegocio negocio, int? idElemento, List<int> agendas, DateTime desde, DateTime hasta)
        {
            var filtros = new List<ClausulaDeFiltrado>();
            filtros.Add(new ClausulaDeFiltrado(nameof(EventoDeAgendaDtm.Inicio), enumCriteriosDeFiltrado.mayorIgual, desde.ToString()));
            filtros.Add(new ClausulaDeFiltrado(nameof(EventoDeAgendaDtm.Fin), enumCriteriosDeFiltrado.menorIgual, hasta.ToString()));

            if (agendas != null && agendas.Any()) filtros.Add(new ClausulaDeFiltrado(nameof(EventoDeAgendaDtm.IdAgenda), enumCriteriosDeFiltrado.esAlgunoDe, string.Join(',', agendas)));
            else
            {
                filtros.Add(new ClausulaDeFiltrado(nameof(EventoDeAgendaDtm.IdNegocio), enumCriteriosDeFiltrado.igual, negocio.IdNegocio()));
                filtros.Add(new ClausulaDeFiltrado(nameof(EventoDeAgendaDtm.IdElemento), enumCriteriosDeFiltrado.igual, idElemento.Entero()));
            }

            var ordenacion = new List<ClausulaDeOrdenacion>();
            var opciones = new Dictionary<string, object>();
            opciones[nameof(ltrParametrosNeg.Peticion)] = enumPeticion.LeerEventos;
            var eventosDto = Gestor(Contexto, Contexto.Mapeador).LeerElementos(0, -1, filtros, ordenacion, opciones);
            return eventosDto.ToList();
        }
    }
}

