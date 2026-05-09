using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Terceros;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using Utilidades;

namespace ServicioDeDatos.Guarderias
{
    public static class ltrDeInfante
    {
        public static readonly string Aula = nameof(Aula);
        public const string SeleccionarParaUnCurso = nameof(SeleccionarParaUnCurso);

        public const string ExcluirInfactes = nameof(ExcluirInfactes);
    }

    [Table(Tablas.INFANTE, Schema = Esquemas.GUARDERIA)]
    public class InfanteDtm : ElementoDtm, IUsaTraza, IUsaArchivo, IUsaBaja, IUsaAgenda
    {
        public int IdContacto { get; set; }
        public int? IdMadre { get; set; }
        public int? IdPadre { get; set; }
        public InterlocutorDtm Contacto { get; set; }
        public PersonaDtm Padre { get; set; }
        public PersonaDtm Madre { get; set; }
        public DateTime NacidoEl { get; set; }
        public int? IdArchivo { get; set; }
        public virtual ArchivoDtm Archivo { get; set; }
        public bool Baja { get; set; }
        public int IdAgenda { get; set; }
        public AgendaDtm Agenda { get; set; }

        public int Anos
        {
            get
            {
                DateTime hoy = DateTime.Today;
                int anos = hoy.Year - NacidoEl.Year;
                if (NacidoEl.Date > hoy.AddYears(-anos)) anos--;
                return anos;
            }
        }

        public int Meses
        {
            get
            {
                DateTime hoy = DateTime.Today;
                int meses = hoy.Month - NacidoEl.Month;
                if (meses < 0) meses += 12;
                if (hoy.Day < NacidoEl.Day) meses--;
                if (meses < 0) meses = 11;
                return meses;
            }
        }

        public string EdadFormateada
        {
            get
            {
                return !NacidoEl.ConFecha()? " --- ": $"Años: {Anos} Meses: {Meses}";
            }
        }

        public string NombreDeAgenda => $"Agenda de: {Nombre}";
    }

    [Table(Tablas.INFANTE + "_" + nameof(Sufijo.AUDITORIA), Schema = Esquemas.GUARDERIA)]
    public class AuditoriaDeUnInfanteDtm : AuditoriaDtm
    {
    }


    [Table(Tablas.INFANTE + "_" + nameof(Sufijo.ARCHIVO), Schema = Esquemas.GUARDERIA)]
    public class ArchivosDeUnInfanteDtm : VinculoDtm
    {
        public InfanteDtm Infante { get; set; }
        public ArchivoDtm Archivo { get; set; }
    }

    [Table(Tablas.INFANTE + "_" + nameof(Sufijo.OBSERVACION), Schema = Esquemas.GUARDERIA)]
    public class ObservacionesDeUnInfanteDtm : ObservacionDtm
    {
        public override enumNegocio Negocio => enumNegocio.Infante;
    }

    [Table(Tablas.INFANTE + "_" + nameof(Sufijo.TRAZA), Schema = Esquemas.GUARDERIA)]
    public class TrazasDeUnInfanteDtm : TrazaDtm
    {
        public override enumNegocio Negocio => enumNegocio.Infante;
    }

    [Table(Tablas.INFANTE + "_" + nameof(Sufijo.ARCHIVADOR), Schema = Esquemas.GUARDERIA)]
    public class ArchivadoresDeUnInfanteDtm : VinculoDtm
    {
        public InfanteDtm Infante { get; set; }
        public ArchivadorDtm Archivador { get; set; }
    }

    [Table(Tablas.INFANTE + "_" + nameof(Sufijo.AGENDA_EVENTO), Schema = Esquemas.GUARDERIA)]
    public class AgendaDeUnInfanteDtm : VinculoDtm
    {
        public InfanteDtm Infante { get; set; }
        public EventoDeAgendaDtm Evento { get; set; }
    }

    public static partial class ModeloDeGuarderias
    {
        public static void Infante(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<InfanteDtm>().Ignore(x => x.EdadFormateada);
            modelBuilder.Entity<InfanteDtm>().Ignore(x => x.Anos);
            modelBuilder.Entity<InfanteDtm>().Ignore(x => x.Meses);
            modelBuilder.Entity<InfanteDtm>().Ignore(x => x.Contacto);

            ApiDeElementoDtm.DefinirCamposDelElementoDtm<InfanteDtm>(modelBuilder);
            ApiDeRegistroDtm.DefinirCampoFk<InfanteDtm>(modelBuilder, nameof(InfanteDtm.Contacto), nameof(InfanteDtm.IdContacto), ICampos.ID_CONTACTO, requerida: true, unico: false);
            ApiDeRegistroDtm.DefinirCampoFk<InfanteDtm>(modelBuilder, nameof(InfanteDtm.Padre), nameof(InfanteDtm.IdPadre), nameof(ICampos.ID_PADRE), requerida: false, unico: false);
            ApiDeRegistroDtm.DefinirCampoFk<InfanteDtm>(modelBuilder, nameof(InfanteDtm.Madre), nameof(InfanteDtm.IdMadre), nameof(ICampos.ID_MADRE), requerida: false, unico: false);


            modelBuilder.Entity<InfanteDtm>().Property(nameof(InfanteDtm.NacidoEl)).HasColumnName(ICampos.NACIDO_EL).HasColumnType(IDominio.DATE).IsRequired(true);

            ApiDeElementoDtm.DefinirCampoArchivo<InfanteDtm>(modelBuilder, obligatorio: false, unico: true);
            ApiDeElementoDtm.DefinirCampoAgenda<InfanteDtm>(modelBuilder, obligatorio: true, unico: true);
        }

        public static void AuditoriaDeUnInfante(ModelBuilder modelBuilder)
        {
            ApiDeAuditoria.DefinirCamposDeAuditoriaDtm<AuditoriaDeUnInfanteDtm>(modelBuilder);
        }

        internal static void ArchivosDeUnInfante(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<ArchivosDeUnInfanteDtm>(modelBuilder, nameof(ArchivosDeUnInfanteDtm.Infante), nameof(ArchivosDeUnInfanteDtm.Archivo));
        }

        internal static void ObservacionesDeUnInfante(ModelBuilder modelBuilder)
        {
            ApiDeObservaciones.DefinirCampos<ObservacionesDeUnInfanteDtm, InfanteDtm>(modelBuilder);
        }

        internal static void ArchivadoresDeUnInfante(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<ArchivadoresDeUnInfanteDtm>(modelBuilder, nameof(ArchivadoresDeUnInfanteDtm.Infante), nameof(ArchivadoresDeUnInfanteDtm.Archivador));
        }

        internal static void TrazasDeUnInfante(ModelBuilder modelBuilder)
        {
            ApiTraza.DefinirCampos<TrazasDeUnInfanteDtm, InfanteDtm>(modelBuilder);
        }
        internal static void AgendaDeUnInfante(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<AgendaDeUnInfanteDtm>(modelBuilder, nameof(AgendaDeUnInfanteDtm.Infante), nameof(AgendaDeUnInfanteDtm.Evento));
        }

    }
}
