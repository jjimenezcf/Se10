using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.Presupuesto;
using ServicioDeDatos.RegistroEs;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Tarea;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Ventas;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using Utilidades;

namespace GestorDeElementos.Extensores
{
    public enum enumParametroAgenda
    {
        [Description("Días a trás a partir del día de hoy desde el que se mostrará el calendario")]
        Agenda_Leer_Desde,
        [Description("Días hacia adelante a partir del día de hoy que se leerá el calendario")]
        Agenda_Leer_Hasta
    }

    public static class ExtensorDeAgendas
    {
        public static IQueryable<VinculoDtm> Agenda(this enumNegocio negocio, ContextoSe contexto)
        {
            switch (negocio)
            {
                case enumNegocio.Sociedad:
                    return contexto.Set<AgendaDeUnaSociedadDtm>();
                case enumNegocio.Expediente:
                    return contexto.Set<AgendaDeUnExpedienteDtm>();
                case enumNegocio.Contrato:
                    return contexto.Set<AgendaDeUnContratoDtm>();
                case enumNegocio.Pleito:
                    return contexto.Set<AgendaDeUnPleitoDtm>();
                case enumNegocio.Presupuesto:
                    return contexto.Set<AgendaDeUnPresupuestoDtm>();
                case enumNegocio.Registro:
                    return contexto.Set<AgendaDeUnRegistroEsDtm>();
                case enumNegocio.Tarea:
                    return contexto.Set<AgendaDeUnaTareaDtm>();
                case enumNegocio.FacturaEmitida:
                    return contexto.Set<AgendaDeUnaFacturaEmtDtm>();
                case enumNegocio.ParteDeTrabajo:
                    return contexto.Set<AgendaDeUnParteTrDtm>();
                case enumNegocio.PlanificacionDeVenta:
                    return contexto.Set<AgendaDeUnaPlanificacionDeVentaDtm>();
            }

            throw new Exception($"Se debe indicar como obtener los interlocutores vinculados al negocio: {negocio}");
        }

        public static void EliminarAgendaIcs(this AgendaDtm agenda) => File.Delete(agenda.RutaIcs);

        public static void GenerarAgendaIcs(this AgendaDtm agenda, ContextoSe contexto)
        {
            List<EventoDeAgendaDtm> eventos = agenda.ObtenerEventos(contexto, DateTime.Today.AddDays(-30), DateTime.Today.AddMonths(3));
            string icsAgenda = agenda.GenerarIcs(contexto, eventos);

            if (!Directory.Exists(enumRutas.RutaDeAgendas))
                Directory.CreateDirectory(enumRutas.RutaDeAgendas);

            using (StreamWriter writer = new StreamWriter(agenda.RutaIcs))
            {
                writer.Write(icsAgenda.ToString());
            }
        }

        public static Uri GenerarUri(this AgendaDtm agenda)
        {
            string ruta = $"/{enumControladoresEntorno.Agendas}/{enumVistasEntorno.Subcribirme}";

            UriBuilder builder = new UriBuilder(CacheDeVariable.Cfg_UrlBase.Replace("https", "webcal"))
            {
                Path = ruta,
                Query = $"id={agenda.Id}&guid={Guid.NewGuid()}"
            };

            return builder.Uri;
        }

        public static Uri UrlDeAgenda(this AgendaDtm agenda)
        {
            UriBuilder builder = new UriBuilder(CacheDeVariable.Cfg_UrlBase.Replace("https", "webcal"))
            {
                Path = Path.Combine(enumRutas.DirectorioDeAgendas, agenda.Ics + ".ics")
            };
            return builder.Uri;
        }

        public static bool EsDeUsuario(this AgendaDtm agenda, ContextoSe contexto) => contexto.Set<UsuarioDtm>().Any(u => u.IdAgenda == agenda.Id);

        public static bool EsDeSociedad(this AgendaDtm agenda, ContextoSe contexto) => contexto.Set<SociedadDtm>().Any(s => s.IdAgenda == agenda.Id);

        private static List<EventoDeAgendaDtm> ObtenerEventos(this AgendaDtm agenda, ContextoSe contexto, DateTime fechaDesde, DateTime fechaHasta)
        {
            var filtros = new List<ClausulaDeFiltrado>
            {
                new ClausulaDeFiltrado(nameof(EventoDeAgendaDtm.IdAgenda), enumCriteriosDeFiltrado.igual, agenda.Id),
                new ClausulaDeFiltrado(nameof(EventoDeAgendaDtm.Inicio), enumCriteriosDeFiltrado.mayorIgual, fechaDesde.ToString()),
                new ClausulaDeFiltrado(nameof(EventoDeAgendaDtm.Fin), enumCriteriosDeFiltrado.menorIgual, fechaHasta.ToString())
            };

            return contexto.SeleccionarTodos<EventoDeAgendaDtm>(filtros, parametros: new Dictionary<string, object> { { ltrParametrosNeg.ValidarPermisosDeConsulta, false } });
        }

        private static string GenerarIcs(this AgendaDtm agenda, ContextoSe contexto, List<EventoDeAgendaDtm> eventos)
        {
            var renderizarUrl = agenda.EsDeSociedad(contexto) || agenda.EsDeUsuario(contexto);

            var calendario = new StringBuilder();
            calendario.AppendLine("BEGIN:VCALENDAR");
            calendario.AppendLine("VERSION:2.0");
            calendario.AppendLine("PRODID:Calendarios.Se");
            calendario.AppendLine("METHOD:PUBLISH");
            calendario.AppendLine("");

            foreach (var evento in eventos)
            {
                calendario.AppendLine("BEGIN:VEVENT");
                calendario.AppendLine($"UID:SE{evento.Id}");

                if (evento.EventoDeDia)
                    calendario.AppendLine($"DTSTART;VALUE=DATE:{evento.Inicio.ToUniversalTime().ToString("yyyyMMdd")}");
                else
                    calendario.AppendLine($"DTSTART;VALUE=DATE-TIME:{evento.Inicio.ToUniversalTime().ToString("yyyyMMddTHHmmssZ")}");

                calendario.AppendLine("SEQUENCE:0");
                calendario.AppendLine("TRANSP:OPAQUE");

                if (evento.EventoDeDia)
                    calendario.AppendLine($"DTEND;VALUE=DATE:{evento.Inicio.ToUniversalTime().ToString("yyyyMMdd")}");
                else
                    calendario.AppendLine($"DTEND;VALUE=DATE-TIME:{evento.Inicio.ToUniversalTime().ToString("yyyyMMddTHHmmssZ")}");

                calendario.AppendLine($"SUMMARY:{evento.Nombre}");
                var urlDelEvento = renderizarUrl ? UriEvento(contexto, evento) : "";
                string descripcionConUrl = $"{evento.Descripcion}\n\n{urlDelEvento}";

                string descripcionFormateada = descripcionConUrl
                    .Replace("\r\n", "\\n")
                    .Replace("\n", "\\n");

                calendario.AppendLine($"DESCRIPTION:{descripcionFormateada}");

                calendario.AppendLine("CLASS:PUBLIC");
                calendario.AppendLine($"CATEGORIES:{NegociosDeSe.LeerNegocioPorId(evento.IdNegocio).Nombre}");
                calendario.AppendLine($"DTSTAMP:{evento.FechaCreacion.ToUniversalTime().ToString("yyyyMMddTHHmmssZ")}");
                calendario.AppendLine($"CREATED:{DateTime.Now.ToUniversalTime().ToString("yyyyMMddTHHmmssZ")}");
                calendario.AppendLine("END:VEVENT");
                calendario.AppendLine("");
            }
            calendario.AppendLine("END:VCALENDAR");

            return calendario.ToString();
        }

        public static string UriEvento(ContextoSe contexto, EventoDeAgendaDtm evento)
        {
            var negocio = NegociosDeSe.LeerNegocioPorId(evento.IdNegocio);
            var vista = contexto.SeleccionarPorPropiedad<VistaMvcDtm>(nameof(VistaMvcDtm.ElementoDto), negocio.ElementoDto);
            string parametros = OtrosParametros(contexto, evento.IdNegocio, evento.IdElemento);

            UriBuilder builder = new UriBuilder(CacheDeVariable.Cfg_UrlBase)
            {
                Path = $"{vista.Controlador}/{vista.Accion}",
                Query = $"id={evento.IdElemento}{(parametros.IsNullOrEmpty() ? "" : parametros)}"
            };
            // $"{CacheDeVariable.Cfg_UrlBase}/{vista.Controlador}/{vista.Accion}?id={evento.IdElemento}{(parametros.IsNullOrEmpty() ? "" : parametros)}";
            return builder.Uri.ToString();
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

        public static PermisosDirectosDtm AsignarPermisoAlPuesto(this AgendaDtm agenda, ContextoSe contexto, int idPuesto, enumModoDeAccesoDeDatos modo)
        {
           return new PermisosDirectosDtm
            {
                IdPuesto = idPuesto,
                IdPermiso = modo == enumModoDeAccesoDeDatos.Gestor ? agenda.IdGestor : agenda.IdConsultor
            }.InsertarComoAdministradorSiNoExiste(contexto, new List<string> { nameof(PermisosDirectosDtm.IdPuesto), nameof(PermisosDirectosDtm.IdPermiso) });
        }

        public static void DesasignarPermisoAlPuesto(this AgendaDtm agenda, ContextoSe contexto, int idPuesto, enumModoDeAccesoDeDatos modo)
        {
            var registro = contexto.SeleccionarTodos<PermisosDirectosDtm>(new Dictionary<string, object> {
                     { nameof(PermisosDirectosDtm.IdPuesto), idPuesto },
                     { nameof(PermisosDirectosDtm.IdPermiso), modo == enumModoDeAccesoDeDatos.Consultor ? agenda.IdConsultor: agenda.IdGestor }
            });
            if (registro.Count == 1)
            {
                registro[0].EliminarComoAdministrador(contexto);
            }
        }

        public static bool PuedeConsultarla(this AgendaDtm agenda, ContextoSe contexto)        
        {
            var permisosDeElUsuario = contexto.Set<UsuariosDeUnPermisoDtm>().Where(permisos => permisos.IdUsuario == contexto.DatosDeConexion.IdUsuario);
            return permisosDeElUsuario.Any(p => p.IdPermiso == agenda.IdGestor || p.IdPermiso == agenda.IdConsultor || p.IdPermiso == agenda.IdInterventor);
        }
    }
}
