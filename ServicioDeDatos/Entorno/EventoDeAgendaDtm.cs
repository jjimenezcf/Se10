using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Negocio;
using static ServicioDeDatos.Elemento.Enumerados;

namespace ServicioDeDatos.Entorno
{
    [Table(Tablas.AGENDA_EVENTO, Schema = Esquemas.ENTORNO)]
    public class EventoDeAgendaDtm : ElementoDtm, IUsaDescripcion
    {
        public int IdAgenda { get; set; }
        public DateTime Inicio { get; set; }
        public DateTime Fin { get; set; }
        public AgendaDtm Agenda { get; set; }
        public bool EsDelSistema { get; set; }

        public bool EventoDeDia => Inicio.TimeOfDay.TotalMilliseconds - Fin.TimeOfDay.TotalMilliseconds == 0;

        public int IdNegocio { get; set; }
        public int IdElemento {get; set; }
        public string Descripcion { get; set; }
        public int? AvisarAntesDe { get; set; }
        public enumDurabilidad? MedidoEn { get; set; }
        public bool? Notificado { get; set; }
    }

    public static partial class ModeloDeEntorno
    {
        internal static string TablaEventoDeCalendario => $"{ApiDeRegistroDtm.EsquemaDeTabla(typeof(EventoDeAgendaDtm))}.{ApiDeRegistroDtm.NombreDeTabla(typeof(EventoDeAgendaDtm))}";

        public static void EventosDelCalendario(ModelBuilder modelBuilder)
        {
            ApiDeElementoDtm.DefinirCamposDelElementoDtm<EventoDeAgendaDtm>(modelBuilder);
            ApiDeRegistroDtm.DefinirCampoFk<EventoDeAgendaDtm>(modelBuilder, nameof(EventoDeAgendaDtm.Agenda), nameof(EventoDeAgendaDtm.IdAgenda), ICampos.ID_AGENDA, requerida: true, unico: false);

            modelBuilder.Entity<EventoDeAgendaDtm>().Property(p => p.Inicio).HasColumnName(ICampos.INICIO).HasColumnType(IDominio.DATETIME_2).IsRequired();
            modelBuilder.Entity<EventoDeAgendaDtm>().Property(p => p.Fin).HasColumnName(ICampos.FIN).HasColumnType(IDominio.DATETIME_2).IsRequired();
            modelBuilder.Entity<EventoDeAgendaDtm>().Property(p => p.IdElemento).HasColumnName(ICampos.ID_ELEMENTO).HasColumnType(IDominio.INT).IsRequired();
            modelBuilder.Entity<EventoDeAgendaDtm>().Property(p => p.IdNegocio).HasColumnName(ICampos.ID_NEGOCIO).HasColumnType(IDominio.INT).IsRequired();
            modelBuilder.Entity<EventoDeAgendaDtm>().Property(p => p.Descripcion).HasColumnName(ICampos.DESCRIPCION).HasColumnType(IDominio.VARCHAR_2000).IsRequired(false);

            modelBuilder.Entity<EventoDeAgendaDtm>().Property(p => p.EsDelSistema).HasColumnName(ICampos.DEL_SISTEMA).HasColumnType(IDominio.BIT).HasDefaultValue(false).IsRequired(true);


            modelBuilder.Entity<EventoDeAgendaDtm>().Property(p => p.AvisarAntesDe).HasColumnName(ICampos.AVISO).HasColumnType(IDominio.INT).IsRequired(false);
            modelBuilder.Entity<EventoDeAgendaDtm>().Property(p => p.MedidoEn).HasColumnName(ICampos.MEDIDO_EN).HasColumnType(IDominio.VARCHAR_30).IsRequired(false);


            modelBuilder.Entity<EventoDeAgendaDtm>().Property(nameof(EventoDeAgendaDtm.Notificado))
                .HasColumnName(ICampos.NOTIFICADO)
                .HasColumnType(IDominio.BIT).IsRequired(false);


            ApiDeRegistroDtm.DefinirFk<EventoDeAgendaDtm,NegocioDtm>(modelBuilder, nameof(EventoDeAgendaDtm.IdNegocio), ICampos.ID_NEGOCIO, unico: false);

        }
    }
}
