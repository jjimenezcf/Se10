using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Terceros;
using Utilidades;

namespace ServicioDeDatos.Gastos
{
    public enum enumClaseDeRemesaPag
    {
        [Description("Transferencias")]
        Transferencias,
    }


    [Table(Tablas.REMESA_PAG, Schema = Esquemas.GASTO)]
    public class RemesaPagDtm : ElementoDeProcesoDtm, IUsaArchivo
    {
        public enumClaseDeRemesaPag Clase { get; set; }

        public string Deudor { get; set; }
        public string NifDelDeudor { get; set; }
        public string SufijoDeudor { get; set; }

        public string Presentador { get; set; }
        public string NifDelPresentador { get; set; }
        public string SufijoPresentador { get; set; }

        public new TipoDeRemesaPagDtm Tipo { get; set; }
        public new EstadoDeUnaRemesaPagDtm Estado { get; set; }
        public string Entidad { get; set; }
        public string Oficina { get; set; }

        public int IdCuentaDePago { get; set; }        
        public CuentaDeMiSociedadDtm CuentaDePago { get; set; }

        public DateTime? GeneradaEl { get; set; }
        public DateTime? PresentadaEl { get; set; }
        public DateTime? PagarEl { get; set; }
        public DateTime? PagadaEl { get; set; }

        public int? IdArchivo { get; set; }
        public ArchivoDtm Archivo { get; set ; }                
    }


    [Table(Tablas.REMESA_PAG + "_" + nameof(Sufijo.AUDITORIA), Schema = Esquemas.GASTO)]
    public class AuditoriaDeUnaRemesaPagDtm : AuditoriaDtm
    {
    }

    [Table(Tablas.REMESA_PAG + "_" + nameof(Sufijo.ARCHIVO), Schema = Esquemas.GASTO)]
    public class ArchivosDeUnaRemesaPagDtm : VinculoDtm
    {
        public RemesaPagDtm RemesaPag { get; set; }
        public ArchivoDtm Archivo { get; set; }
    }

    [Table(Tablas.REMESA_PAG + "_" + nameof(Sufijo.AGENDA_EVENTO), Schema = Esquemas.GASTO)]
    public class AgendaDeUnaRemesaPagDtm : VinculoDtm
    {
        public RemesaPagDtm RemesaPag { get; set; }
        public EventoDeAgendaDtm Evento { get; set; }
    }

    [Table(Tablas.REMESA_PAG + "_" + nameof(Sufijo.OBSERVACION), Schema = Esquemas.GASTO)]
    public class ObservacionesDeUnaRemesaPagDtm : ObservacionDtm
    {
        public override enumNegocio Negocio => enumNegocio.RemesaPag;
    }

    [Table(Tablas.REMESA_PAG + "_" + nameof(Sufijo.PERMISO), Schema = Esquemas.GASTO)]
    public class PermisoDeLaRemesaPagDtm : PermisosDelElementoDtm
    {
    }

    [Table(Tablas.REMESA_PAG + "_" + nameof(Sufijo.TRAZA), Schema = Esquemas.GASTO)]
    public class TrazasDeUnaRemesaPagDtm : TrazaDtm
    {
        public override enumNegocio Negocio => enumNegocio.RemesaPag;
    }

    [Table(Tablas.REMESA_PAG + "_" + nameof(Sufijo.HISTORIA), Schema = Esquemas.GASTO)]
    public class HitosDeUnaRemesaPagDtm : HitoDtm
    {

    }

    [Table(Tablas.REMESA_PAG + "_" + nameof(Sufijo.ARCHIVADOR), Schema = Esquemas.GASTO)]
    public class ArchivadoresDeUnaRemesaPagDtm : VinculoDtm
    {
        public RemesaPagDtm RemesaPag { get; set; }
        public ArchivadorDtm Archivador { get; set; }
    }


    public static partial class ModeloDeRemesaPag
    {

        public static void RemesaPag(ModelBuilder modelBuilder)
        {
            ApiDeElementoDtm.DefinirCamposDelElementoDtm<RemesaPagDtm>(modelBuilder);
            ApiDeElementoDtm.DefinirCampoCg<RemesaPagDtm>(modelBuilder, nameof(RemesaPagDtm.Cg));
            ApiDeElementoDtm.DefinirCampoTipo<RemesaPagDtm>(modelBuilder, nameof(RemesaPagDtm.Tipo));
            ApiDeElementoDtm.DefinirCampoEstado<RemesaPagDtm>(modelBuilder, nameof(RemesaPagDtm.Estado));

            modelBuilder.Entity<RemesaPagDtm>().Property(nameof(RemesaPagDtm.Clase)).HasColumnName(ICampos.CLASE).HasColumnType(IDominio.VARCHAR_30).HasDefaultValue(enumClaseDeRemesaPag.Transferencias).IsRequired(true);

            modelBuilder.Entity<RemesaPagDtm>().Property(p => p.Deudor).HasColumnName(ICampos.DEUDOR).HasColumnType(IDominio.VARCHAR_255).IsRequired();
            modelBuilder.Entity<RemesaPagDtm>().Property(p => p.NifDelDeudor).HasColumnName(ICampos.NIF_DEUDOR).HasColumnType(IDominio.VARCHAR_15).IsRequired();
            modelBuilder.Entity<RemesaPagDtm>().Property(p => p.SufijoDeudor).HasColumnName(ICampos.SUF_DEUDOR).HasColumnType(IDominio.VARCHAR_3).IsRequired();

            modelBuilder.Entity<RemesaPagDtm>().Property(p => p.Presentador).HasColumnName(ICampos.PRESENTADOR).HasColumnType(IDominio.VARCHAR_255).IsRequired();
            modelBuilder.Entity<RemesaPagDtm>().Property(p => p.NifDelPresentador).HasColumnName(ICampos.NIF_PRESENTADOR).HasColumnType(IDominio.VARCHAR_15).IsRequired();
            modelBuilder.Entity<RemesaPagDtm>().Property(p => p.SufijoPresentador).HasColumnName(ICampos.SUF_PRESENTADOR).HasColumnType(IDominio.VARCHAR_3).IsRequired();

            ApiDeRegistroDtm.DefinirCampoFk<RemesaPagDtm>(modelBuilder, nameof(RemesaPagDtm.CuentaDePago), nameof(RemesaPagDtm.IdCuentaDePago), ICampos.ID_CUENTA, requerida: true, unico: false);
            modelBuilder.Entity<RemesaPagDtm>().Property(p => p.Entidad).HasColumnName(ICampos.ENTIDAD).HasColumnType(IDominio.VARCHAR_4).IsRequired();
            modelBuilder.Entity<RemesaPagDtm>().Property(p => p.Oficina).HasColumnName(ICampos.OFICINA).HasColumnType(IDominio.VARCHAR_4).IsRequired();

            modelBuilder.Entity<RemesaPagDtm>().Property(p => p.GeneradaEl).HasColumnName(ICampos.GENERADA_EL).HasColumnType(IDominio.DATETIME_2).IsRequired(false);
            modelBuilder.Entity<RemesaPagDtm>().Property(p => p.PresentadaEl).HasColumnName(ICampos.PRESENTADA_EL).HasColumnType(IDominio.DATETIME_2).IsRequired(false);
            modelBuilder.Entity<RemesaPagDtm>().Property(p => p.PagarEl).HasColumnName(ICampos.CARGAR_EL).HasColumnType(IDominio.DATETIME_2).IsRequired(false);
            modelBuilder.Entity<RemesaPagDtm>().Property(p => p.PagadaEl).HasColumnName(ICampos.CARGADA_EL).HasColumnType(IDominio.DATETIME_2).IsRequired(false);

            ApiDeElementoDtm.DefinirCampoArchivo<RemesaPagDtm>(modelBuilder);
        }

        internal static void Trazas(ModelBuilder modelBuilder)
        {
            ApiTraza.DefinirCampos<TrazasDeUnaRemesaPagDtm, RemesaPagDtm>(modelBuilder);
        }

        internal static void Auditoria(ModelBuilder modelBuilder)
        {
            ApiDeAuditoria.DefinirCamposDeAuditoriaDtm<AuditoriaDeUnaRemesaPagDtm>(modelBuilder);
        }

        internal static void Archivos(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<ArchivosDeUnaRemesaPagDtm>(modelBuilder, nameof(ArchivosDeUnaRemesaPagDtm.RemesaPag), nameof(ArchivosDeUnaRemesaPagDtm.Archivo));

        }
        internal static void Agenda(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<AgendaDeUnaRemesaPagDtm>(modelBuilder, nameof(AgendaDeUnaRemesaPagDtm.RemesaPag), nameof(AgendaDeUnaRemesaPagDtm.Evento));
        }

        internal static void Observaciones(ModelBuilder modelBuilder)
        {
            ApiDeObservaciones.DefinirCampos<ObservacionesDeUnaRemesaPagDtm, RemesaPagDtm>(modelBuilder);
        }

        internal static void Permisos(ModelBuilder modelBuilder)
        {
            ApiPermisosDelElemento.DefinirCampos<PermisoDeLaRemesaPagDtm, RemesaPagDtm>(modelBuilder);
        }

        internal static void Historia(ModelBuilder modelBuilder)
        {
            ApiDeHitos.DefinirCampos<HitosDeUnaRemesaPagDtm, RemesaPagDtm, EstadoDeUnaRemesaPagDtm, TransicionesDeUnaRemesaPagDtm, ObservacionesDeUnaRemesaPagDtm>(modelBuilder);
        }

        internal static void Archivadores(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<ArchivadoresDeUnaRemesaPagDtm>(modelBuilder, nameof(ArchivadoresDeUnaRemesaPagDtm.RemesaPag), nameof(ArchivadoresDeUnaRemesaPagDtm.Archivador));
        }

    }
}
