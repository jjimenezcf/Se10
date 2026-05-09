using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.TrabajosSometidos;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ModeloDeDto;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.TrabajosSometidos;
using Utilidades;
using static GestoresDeNegocio.Entorno.GestorDeEventosDeAgenda;
using static ServicioDeDatos.Elemento.Enumerados;

namespace GestoresDeNegocio.Entorno
{
    public enum enumTrabajosDeAgenda
    {
        [Description("Notificar eventos de agenda")]
        ComunicadosDeAgendasDelSistema
    }

    public class TrabajosDeAgenda
    {
        public static TrabajoDeUsuarioDtm SometerComunicadosDeAgendasDelSistema(ContextoSe contexto)
        {
            var dll = Assembly.GetExecutingAssembly().GetName().Name;
            var clase = typeof(TrabajosDeAgenda).FullName;
            var ts = GestorDeTrabajosSometido.CrearObtener(contexto,
                enumTrabajosDeAgenda.ComunicadosDeAgendasDelSistema.Descripcion(),
                dll,
                clase,
                nameof(enumTrabajosDeAgenda.ComunicadosDeAgendasDelSistema), comunicarFin: false
            );
            var datosDeCreacion = new Dictionary<string, object>
            {
                { nameof(TrabajoDeUsuarioDtm.Planificado), DateTime.Now.AddDays(1) },
                { nameof(TrabajoDeUsuarioDtm.Periodicidad), 300} 
            };
            return GestorDeTrabajosDeUsuario.CrearSiNoEstaPendiente(contexto, ts, datosDeCreacion);
        }

        public static void ComunicadosDeAgendasDelSistema(EntornoDeTrabajo entorno)
        {
            var contexto = entorno.contextoDelProceso;
            contexto.IniciarTraza(nameof(ComunicadosDeAgendasDelSistema));

            /* obtener las agendas del sistema o corporativas*/
            /* obtener los usuarios con permisos a ellas*/
            /* ver los eventos que vencen mañana*/
            /* montar el cuerpo */
            /* enviar recordatorio a usuarios con acceso */


            /* obtener las agendas de cada usuario*/
            /* obtener eventos que cumplen mañana y no han sido programados*/
            /* montar el cuerpo */
            /* enviar recordatorio a cada usuario */

            entorno.CrearTraza("Notificaciones de eventos que cumplen mañana y no han sido marcados para recordar");

            var desde = DateTime.Now.Date.AddDays(1);
            var hasta = DateTime.Now.Date.AddDays(2);
            var eventos = contexto.SeleccionarTodos<EventoDeAgendaDtm>(new Dictionary<string,object>());
            var tran = contexto.IniciarTransaccion();
            try
            {
                foreach (var evento in eventos)
                {
                    var negocio = NegociosDeSe.ToEnumerado(evento.IdNegocio);
                    var elemento = negocio.RegistroPorId(contexto, evento.IdElemento);
                    GestorDeCorreos.CrearCorreoPara(contexto,
                        new List<string> { contexto.SeleccionarPorId<UsuarioDtm>(evento.IdUsuaCrea).eMail },
                        "Notificación de eventos de agendas",
                        $"El '{evento.Nombre}' asociado a '{((INombre) elemento).Expresion}' cumplirá con fecha {evento.Fin}",
                        new List<TipoDtoElmento> { new TipoDtoElmento { 
                                                       TipoDto = negocio.TipoDto().FullName,
                                                       IdElemento = elemento.Id,
                                                       Referencia = elemento.Expresion 
                                                   } },
                        new List<string>());

                    if (negocio == enumNegocio.Contrato)  
                        contexto.SeleccionarPorId<ContratoDtm>(evento.IdElemento).EventoNotificado(contexto, evento);
                }
                contexto.Commit(tran);
            }
            catch
            {
                contexto.Rollback(tran);
                throw;
            }
            finally
            {
                entorno.CrearTraza($"Fin del proceso realizado");
                contexto.CerrarTraza();
            }
        }

        public static void NotificacionesDeAgenda(ContextoSe contexto)
        {
            contexto.IniciarTraza(nameof(NotificacionesDeAgenda), debugar: false);

            DateTime fechaActual = DateTime.Now;

            //var eventos2 = contexto.Set<EventoDeAgendaDtm>()
            //    .Where(e => e.Notificado == false)
            //    .ToList();
            //var eventos = eventos2 
            //    .Where(e => e.Inicio.Subtract(GetTimeSpan(e.AvisarAntesDe, e.MedidoEn)) < fechaActual)
            //    .ToList();


          var eventos = contexto.Set<EventoDeAgendaDtm>()
          .FromSqlRaw($@"
                SELECT * FROM ENTORNO.AGENDA_EVENTO 
                WHERE Notificado = 0 
                AND CAST(Inicio AS datetime) - DATEADD(MINUTE,  
                      CASE 
                	    WHEN MEDIDO_EN = 'Dias' THEN AVISO * 1440  
                        WHEN MEDIDO_EN = 'Horas' THEN AVISO * 60  
                        ELSE AVISO 
                	  END,
                	CAST('1900-01-01' AS DATETIME)) <  @fechaActual",
                new SqlParameter("@fechaActual", fechaActual))
               .ToList();

            var ejecutor = contexto.SeleccionarPorId<UsuarioDtm>(contexto.DatosDeConexion.IdUsuario);
            var tran = contexto.IniciarTransaccion();
            var otorgados = ejecutor.OtorgarAdministrador(contexto);
            try
            {
                foreach (var evento in eventos)
                {
                    var negocio = NegociosDeSe.ToEnumerado(evento.IdNegocio);
                    var elemento = negocio.RegistroPorId(contexto, evento.IdElemento);
                    GestorDeCorreos.CrearCorreoPara(contexto,
                        new List<string> { contexto.SeleccionarPorId<UsuarioDtm>(evento.IdUsuaCrea).eMail },
                        "Notificación de evento",
                        $"Le recuerdo que el próximo '{evento.Inicio}' ha anotado en el calendario de que se le avise de '{evento.Nombre}' asociado a '{((INombre)elemento).Expresion}'",
                        new List<TipoDtoElmento> { new TipoDtoElmento {
                                                       TipoDto = negocio.TipoDto().FullName,
                                                       IdElemento = elemento.Id,
                                                       Referencia = elemento.Expresion
                                                   } },
                        new List<string>());
                    evento.Modificar(contexto, new Dictionary<string, object> { { ltrEventosDelCalendario.DarPorNotificado, true } });
                }
                contexto.Commit(tran);
            }
            catch(Exception ex) 
            {
                contexto.Rollback(tran, ex);
            }
            finally
            {
                ejecutor.AnularAdministrador(contexto, otorgados);
            }
        }

        private static TimeSpan GetTimeSpan(int? valor, enumDurabilidad? durabilidad)
        {
            if (valor == null || durabilidad == null)
            {
                return TimeSpan.Zero;
            }

            switch (durabilidad)
            {
                case enumDurabilidad.Dias:
                    return TimeSpan.FromDays((double)valor);
                case enumDurabilidad.Horas:
                    return TimeSpan.FromHours((double)valor);
                case enumDurabilidad.Minutos:
                    return TimeSpan.FromMinutes((double)valor);
                default:
                    return TimeSpan.Zero;
            }
        }
    }
}
