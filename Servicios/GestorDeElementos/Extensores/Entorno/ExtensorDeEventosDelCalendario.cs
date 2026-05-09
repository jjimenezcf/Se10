using ModeloDeDto.Entorno;
using ServicioDeDatos.Entorno;
using ServicioDeDatos;
using System;
using System.Collections.Generic;
using static Gestor.Errores.GestorDeErrores;
using ServicioDeDatos.Elemento;
using Utilidades;

namespace GestorDeElementos.Extensores
{
    public static class ExtensorDeEventosDelCalendario
    {
        public static List<EventoDeAgendaDtm> SeleccionarEventos(this ContextoSe contexto, int idAgenda, int idElemento, string nombre, DateTime? desde = default, DateTime? hasta = default)
        {
            var filtro = new Dictionary<string, object>();
            filtro[nameof(EventoDeAgendaDtm.IdAgenda)] = idAgenda;
            filtro[nameof(EventoDeAgendaDtm.IdElemento)] = idElemento;
            filtro[nameof(EventoDeAgendaDtm.Nombre)] = nombre;

            if (desde != default || hasta != default) return contexto.SeleccionarEventos(desde, hasta, filtro);

            return contexto.SeleccionarTodos<EventoDeAgendaDtm>(filtro);
        }

        public static List<EventoDeAgendaDtm> SeleccionarEventos(this ContextoSe contexto, int idElemento, string nombre, DateTime? desde = default, DateTime? hasta = default)
        {
            var filtro = new Dictionary<string, object>();
            filtro[nameof(EventoDeAgendaDtm.IdElemento)] = idElemento;
            filtro[nameof(EventoDeAgendaDtm.Nombre)] = nombre;

            if (desde != default || hasta != default) return contexto.SeleccionarEventos(desde, hasta, filtro);

            return contexto.SeleccionarTodos<EventoDeAgendaDtm>(filtro, parametros: new Dictionary<string, object> { { nameof(ltrParametrosNeg.ValidarPermisosDeConsulta) , false} });
        }



        public static List<EventoDeAgendaDtm> SeleccionarEventos(this ContextoSe contexto, int idAgenda, int idElemento, DateTime? desde = default, DateTime? hasta = default)
        {
            var filtro = new Dictionary<string, object>();
            filtro[nameof(EventoDeAgendaDtm.IdAgenda)] = idAgenda;
            filtro[nameof(EventoDeAgendaDtm.IdElemento)] = idElemento;

            if (desde != default || hasta != default) return contexto.SeleccionarEventos(desde, hasta, filtro);

            return contexto.SeleccionarTodos<EventoDeAgendaDtm>(filtro, parametros: new Dictionary<string, object> { { ltrParametrosNeg.ValidarPermisosDeConsulta, false } });
        }

        private static List<EventoDeAgendaDtm> SeleccionarEventos(this ContextoSe contexto, DateTime? desde = default, DateTime? hasta = default, Dictionary<string, object> filtros = null)
        {
            if (desde == default && hasta == default)
                Emitir($"Para invocar al método {nameof(SeleccionarEventos)} filtrando por fechas, debe indicar almenos una fecha");

            var clausulas = filtros == null ? new List<ClausulaDeFiltrado>() : filtros.ToFiltros();

            if (desde != default)
                clausulas.Add(new ClausulaDeFiltrado(nameof(EventoDeAgendaDtm.Inicio), enumCriteriosDeFiltrado.mayorIgual, desde.ToString()));
            if (hasta != default)
                clausulas.Add(new ClausulaDeFiltrado(nameof(EventoDeAgendaDtm.Inicio), enumCriteriosDeFiltrado.menor, hasta.ToString()));

            return contexto.Registros<EventoDeAgendaDto, EventoDeAgendaDtm>(clausulas, parametros: new Dictionary<string, object> { { ltrParametrosNeg.ValidarPermisosDeConsulta, false} });
        }

        public static AgendaDtm Agenda(this EventoDeAgendaDtm evento, ContextoSe contexto) =>  evento?.Agenda ?? contexto.SeleccionarPorId<AgendaDtm>(evento.IdAgenda);
    }
}
