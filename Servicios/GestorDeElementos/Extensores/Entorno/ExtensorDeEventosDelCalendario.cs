using DocumentFormat.OpenXml.Presentation;
using ImageMagick.Drawing;
using ModeloDeDto;
using ModeloDeDto.Entorno;
using ModeloDeDto.Tarea;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.TrabajosSometidos;
using ServicioDeCorreos;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilidades;
using static Gestor.Errores.GestorDeErrores;
using static Utilidades.Ampliaciones;

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

            return contexto.SeleccionarTodos<EventoDeAgendaDtm>(filtro, parametros: new Dictionary<string, object> { { nameof(ltrParametrosNeg.ValidarPermisosDeConsulta), false } });
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

            return contexto.Registros<EventoDeAgendaDto, EventoDeAgendaDtm>(clausulas, parametros: new Dictionary<string, object> { { ltrParametrosNeg.ValidarPermisosDeConsulta, false } });
        }

        public static AgendaDtm Agenda(this EventoDeAgendaDtm evento, ContextoSe contexto) => evento?.Agenda ?? contexto.SeleccionarPorId<AgendaDtm>(evento.IdAgenda);

        private static readonly TimeZoneInfo _zonaEspana = ObtenerZonaEspana();

        private static TimeZoneInfo ObtenerZonaEspana()
        {
            // Windows usa "Romance Standard Time", Linux/Mac usa "Europe/Madrid"
            try { return TimeZoneInfo.FindSystemTimeZoneById("Romance Standard Time"); }
            catch { return TimeZoneInfo.FindSystemTimeZoneById("Europe/Madrid"); }
        }

        private static string GenerarIcs(EventoDeAgendaDtm evento, string[] listaCorreos)
        {
            var organizador = new ServicioDeCorreo(CacheDeVariable.Cfg_ServidorDeCorreo).Emisor; // CacheDeVariable.Cfg_CorreoDeSoporte;

            var inicioEspana = TimeZoneInfo.ConvertTime(evento.Inicio, _zonaEspana);
            var finEspana    = TimeZoneInfo.ConvertTime(evento.Fin,    _zonaEspana);

            var contenido = new System.Text.StringBuilder();
            contenido.AppendLine("BEGIN:VCALENDAR");
            contenido.AppendLine("VERSION:2.0");
            contenido.AppendLine("PRODID:Calendarios.Se");
            contenido.AppendLine("METHOD:REQUEST");
            contenido.AppendLine("");
            contenido.AppendLine("BEGIN:VEVENT");
            contenido.AppendLine($"UID:SE{evento.Id}@sistemadeelemenos");

            if (evento.EventoDeDia)
            {
                // Eventos de día completo: solo fecha, sin zona horaria
                contenido.AppendLine($"DTSTART;VALUE=DATE:{inicioEspana:yyyyMMdd}");
                contenido.AppendLine($"DTEND;VALUE=DATE:{finEspana:yyyyMMdd}");
            }
            else
            {
                // Eventos con hora: TZID=Europe/Madrid, sin Z (Z indica UTC)
                contenido.AppendLine($"DTSTART;TZID=Europe/Madrid:{inicioEspana:yyyyMMddTHHmmss}");
                contenido.AppendLine($"DTEND;TZID=Europe/Madrid:{finEspana:yyyyMMddTHHmmss}");
            }

            contenido.AppendLine("SEQUENCE:0");
            contenido.AppendLine("TRANSP:OPAQUE");
            contenido.AppendLine($"SUMMARY:{evento.Nombre}");
            contenido.AppendLine($"ORGANIZER;CN=Sistema de Elementos:mailto:{organizador}");

            foreach (var correo in listaCorreos)
                contenido.AppendLine($"ATTENDEE;CUTYPE=INDIVIDUAL;ROLE=REQ-PARTICIPANT;PARTSTAT=NEEDS-ACTION;RSVP=TRUE;CN={correo}:mailto:{correo}");

            string descripcion = (evento.Descripcion ?? "")
                .Replace("\r\n", "\\n")
                .Replace("\n", "\\n");
            contenido.AppendLine($"DESCRIPTION:{descripcion}");
            contenido.AppendLine("CLASS:PUBLIC");
            contenido.AppendLine($"DTSTAMP:{evento.FechaCreacion.ToUniversalTime():yyyyMMddTHHmmssZ}");
            contenido.AppendLine($"CREATED:{DateTime.UtcNow:yyyyMMddTHHmmssZ}");
            contenido.AppendLine("END:VEVENT");
            contenido.AppendLine("");
            contenido.AppendLine("END:VCALENDAR");

            if (!System.IO.Directory.Exists(enumRutas.RutaDeAgendas))
                System.IO.Directory.CreateDirectory(enumRutas.RutaDeAgendas);

            var rutaIcs = System.IO.Path.Combine(enumRutas.RutaDeAgendas, $"evento_{evento.Id}_{Guid.NewGuid():N}.ics");
            System.IO.File.WriteAllText(rutaIcs, contenido.ToString());
            return rutaIcs;
        }

        public static void EnviarEventoIcs(ContextoSe contexto, EventoDeAgendaDtm evento, string[] listaCorreos)
        {
            var negocio = NegociosDeSe.ToEnumerado(evento.IdNegocio);
            var tipodto = negocio.TipoDto();
            var elemento = contexto.SeleccionarElementoPorId(negocio.TipoDtm(), evento.IdElemento);
            var infoElemento = new TipoDtoElmento
            {
                TipoDto = tipodto.FullName,
                IdElemento = evento.IdElemento,
                Referencia = elemento.Referencia(contexto)
            };
            // ComponerUrl() devuelve HTML (<a href=...>) que no es válido en ICS — usamos la URL plana
            var urlPlana = negocio.ComponerUrlPorId(contexto, evento.IdElemento).Uri.ToString();
            var urlConsulta = infoElemento.ConsultaUrlPlana(contexto);
            var detalle = evento.Descripcion;
            evento.Descripcion = $"{evento.Descripcion}{Environment.NewLine}Acceso al elemento: {urlPlana}{(urlConsulta != string.Empty ?$"{Environment.NewLine}Acceso en consulta: {urlConsulta}" :"")}";
            string pathIcs = GenerarIcs(evento, listaCorreos);
            var tran = contexto.IniciarTransaccion();
            try
            {
                ExtensorDeCorreos.CrearCorreo(contexto,
                    listaCorreos.ToList(),
                    $"{ltrDeCoreos.CorreoDeInvitacion}: {evento.Nombre}",
                    $"{detalle}{Environment.NewLine}{Environment.NewLine}Descargue e incorpore el evento adjunto.",
                    new List<TipoDtoElmento> { infoElemento },
                    archivos: new List<string> { pathIcs },
                    parametros: new Dictionary<string, object> { { ltrParametrosNeg.EstaEjecutandoUnaAccion, true }
                 });

                elemento.CrearTraza(contexto, "Envio de Ics", $"Envío de invitación al evento '{evento.Nombre}' a {string.Join(", ", listaCorreos)}");
                contexto.Commit(tran);
            }
            catch
            {
                contexto.Rollback(tran);
                throw;
            }
        }
    }
}
