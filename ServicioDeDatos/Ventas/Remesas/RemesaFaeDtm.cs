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

namespace ServicioDeDatos.Ventas
{
    public enum enumClaseDeRemesaFae
    {
        [Description("Facturas emitidas")]
        Emitidas,
        [Description("Facturas devueltas")]
        Devueltas
    }


    [Table(Tablas.REMESA_FAE, Schema = Esquemas.VENTA)]
    public class RemesaFaeDtm : ElementoDeProcesoDtm, IUsaArchivo
    {
        public enumClaseDeRemesaFae Clase { get; set; }

        public string Acreedor { get; set; }
        public string NifDelAcreedor { get; set; }
        public string SufijoAcreedor { get; set; }

        public string Presentador { get; set; }
        public string NifDelPresentador { get; set; }
        public string SufijoPresentador { get; set; }

        public new TipoDeRemesaFaeDtm Tipo { get; set; }
        public new EstadoDeUnaRemesaFaeDtm Estado { get; set; }
        public string Entidad { get; set; }
        public string Oficina { get; set; }

        public int IdCuentaDeAbono { get; set; }        
        public CuentaDeMiSociedadDtm CuentaDeAbono { get; set; }

        public DateTime? GeneradaEl { get; set; }
        public DateTime? PresentadaEl { get; set; }
        public DateTime? CargarEl { get; set; }
        public DateTime? CargadaEl { get; set; }

        public int? IdArchivo { get; set; }
        public ArchivoDtm Archivo { get; set ; }                
    }


    [Table(Tablas.REMESA_FAE + "_" + nameof(Sufijo.AUDITORIA), Schema = Esquemas.VENTA)]
    public class AuditoriaDeUnaRemesaFaeDtm : AuditoriaDtm
    {
    }

    [Table(Tablas.REMESA_FAE + "_" + nameof(Sufijo.ARCHIVO), Schema = Esquemas.VENTA)]
    public class ArchivosDeUnaRemesaFaeDtm : VinculoDtm
    {
        public RemesaFaeDtm RemesaFae { get; set; }
        public ArchivoDtm Archivo { get; set; }
    }

    [Table(Tablas.REMESA_FAE + "_" + nameof(Sufijo.AGENDA_EVENTO), Schema = Esquemas.VENTA)]
    public class AgendaDeUnaRemesaFaeDtm : VinculoDtm
    {
        public RemesaFaeDtm RemesaFae { get; set; }
        public EventoDeAgendaDtm Evento { get; set; }
    }

    [Table(Tablas.REMESA_FAE + "_" + nameof(Sufijo.OBSERVACION), Schema = Esquemas.VENTA)]
    public class ObservacionesDeUnaRemesaFaeDtm : ObservacionDtm
    {
        public override enumNegocio Negocio => enumNegocio.RemesaFae;
    }

    [Table(Tablas.REMESA_FAE + "_" + nameof(Sufijo.PERMISO), Schema = Esquemas.VENTA)]
    public class PermisoDeLaRemesaFaeDtm : PermisosDelElementoDtm
    {
    }

    [Table(Tablas.REMESA_FAE + "_" + nameof(Sufijo.TRAZA), Schema = Esquemas.VENTA)]
    public class TrazasDeUnaRemesaFaeDtm : TrazaDtm
    {
        public override enumNegocio Negocio => enumNegocio.RemesaFae;
    }

    [Table(Tablas.REMESA_FAE + "_" + nameof(Sufijo.HISTORIA), Schema = Esquemas.VENTA)]
    public class HitosDeUnaRemesaFaeDtm : HitoDtm
    {

    }

    [Table(Tablas.REMESA_FAE + "_" + nameof(Sufijo.ARCHIVADOR), Schema = Esquemas.VENTA)]
    public class ArchivadoresDeUnaRemesaFaeDtm : VinculoDtm
    {
        public RemesaFaeDtm RemesaFae { get; set; }
        public ArchivadorDtm Archivador { get; set; }
    }


    public static partial class ModeloDeRemesaFae
    {

        public static void RemesaFae(ModelBuilder modelBuilder)
        {
            ApiDeElementoDtm.DefinirCamposDelElementoDtm<RemesaFaeDtm>(modelBuilder);
            ApiDeElementoDtm.DefinirCampoCg<RemesaFaeDtm>(modelBuilder, nameof(RemesaFaeDtm.Cg));
            ApiDeElementoDtm.DefinirCampoTipo<RemesaFaeDtm>(modelBuilder, nameof(RemesaFaeDtm.Tipo));
            ApiDeElementoDtm.DefinirCampoEstado<RemesaFaeDtm>(modelBuilder, nameof(RemesaFaeDtm.Estado));

            modelBuilder.Entity<RemesaFaeDtm>().Property(nameof(RemesaFaeDtm.Clase)).HasColumnName(ICampos.CLASE).HasColumnType(IDominio.VARCHAR_30).HasDefaultValue(enumClaseDeRemesaFae.Emitidas).IsRequired(true);

            modelBuilder.Entity<RemesaFaeDtm>().Property(p => p.Acreedor).HasColumnName(ICampos.ACREEDOR).HasColumnType(IDominio.VARCHAR_255).IsRequired();
            modelBuilder.Entity<RemesaFaeDtm>().Property(p => p.NifDelAcreedor).HasColumnName(ICampos.NIF_ACREEDOR).HasColumnType(IDominio.VARCHAR_15).IsRequired();
            modelBuilder.Entity<RemesaFaeDtm>().Property(p => p.SufijoAcreedor).HasColumnName(ICampos.SUF_ACREEDOR).HasColumnType(IDominio.VARCHAR_3).IsRequired();

            modelBuilder.Entity<RemesaFaeDtm>().Property(p => p.Presentador).HasColumnName(ICampos.PRESENTADOR).HasColumnType(IDominio.VARCHAR_255).IsRequired();
            modelBuilder.Entity<RemesaFaeDtm>().Property(p => p.NifDelPresentador).HasColumnName(ICampos.NIF_PRESENTADOR).HasColumnType(IDominio.VARCHAR_15).IsRequired();
            modelBuilder.Entity<RemesaFaeDtm>().Property(p => p.SufijoPresentador).HasColumnName(ICampos.SUF_PRESENTADOR).HasColumnType(IDominio.VARCHAR_3).IsRequired();

            ApiDeRegistroDtm.DefinirCampoFk<RemesaFaeDtm>(modelBuilder, nameof(RemesaFaeDtm.CuentaDeAbono), nameof(RemesaFaeDtm.IdCuentaDeAbono), ICampos.ID_CUENTA, requerida: true, unico: false);
            modelBuilder.Entity<RemesaFaeDtm>().Property(p => p.Entidad).HasColumnName(ICampos.ENTIDAD).HasColumnType(IDominio.VARCHAR_4).IsRequired();
            modelBuilder.Entity<RemesaFaeDtm>().Property(p => p.Oficina).HasColumnName(ICampos.OFICINA).HasColumnType(IDominio.VARCHAR_4).IsRequired();

            modelBuilder.Entity<RemesaFaeDtm>().Property(p => p.GeneradaEl).HasColumnName(ICampos.GENERADA_EL).HasColumnType(IDominio.DATETIME_2).IsRequired(false);
            modelBuilder.Entity<RemesaFaeDtm>().Property(p => p.PresentadaEl).HasColumnName(ICampos.PRESENTADA_EL).HasColumnType(IDominio.DATETIME_2).IsRequired(false);
            modelBuilder.Entity<RemesaFaeDtm>().Property(p => p.CargarEl).HasColumnName(ICampos.CARGAR_EL).HasColumnType(IDominio.DATETIME_2).IsRequired(false);
            modelBuilder.Entity<RemesaFaeDtm>().Property(p => p.CargadaEl).HasColumnName(ICampos.CARGADA_EL).HasColumnType(IDominio.DATETIME_2).IsRequired(false);

            ApiDeElementoDtm.DefinirCampoArchivo<RemesaFaeDtm>(modelBuilder);
        }

        internal static void Trazas(ModelBuilder modelBuilder)
        {
            ApiTraza.DefinirCampos<TrazasDeUnaRemesaFaeDtm, RemesaFaeDtm>(modelBuilder);
        }

        internal static void Auditoria(ModelBuilder modelBuilder)
        {
            ApiDeAuditoria.DefinirCamposDeAuditoriaDtm<AuditoriaDeUnaRemesaFaeDtm>(modelBuilder);
        }

        internal static void Archivos(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<ArchivosDeUnaRemesaFaeDtm>(modelBuilder, nameof(ArchivosDeUnaRemesaFaeDtm.RemesaFae), nameof(ArchivosDeUnaRemesaFaeDtm.Archivo));

        }
        internal static void Agenda(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<AgendaDeUnaRemesaFaeDtm>(modelBuilder, nameof(AgendaDeUnaRemesaFaeDtm.RemesaFae), nameof(AgendaDeUnaRemesaFaeDtm.Evento));
        }

        internal static void Observaciones(ModelBuilder modelBuilder)
        {
            ApiDeObservaciones.DefinirCampos<ObservacionesDeUnaRemesaFaeDtm, RemesaFaeDtm>(modelBuilder);
        }

        internal static void Permisos(ModelBuilder modelBuilder)
        {
            ApiPermisosDelElemento.DefinirCampos<PermisoDeLaRemesaFaeDtm, RemesaFaeDtm>(modelBuilder);
        }

        internal static void Historia(ModelBuilder modelBuilder)
        {
            ApiDeHitos.DefinirCampos<HitosDeUnaRemesaFaeDtm, RemesaFaeDtm, EstadoDeUnaRemesaFaeDtm, TransicionesDeUnaRemesaFaeDtm, ObservacionesDeUnaRemesaFaeDtm>(modelBuilder);
        }

        internal static void Archivadores(ModelBuilder modelBuilder)
        {
            ApiDeVinculos.DefinirCampos<ArchivadoresDeUnaRemesaFaeDtm>(modelBuilder, nameof(ArchivadoresDeUnaRemesaFaeDtm.RemesaFae), nameof(ArchivadoresDeUnaRemesaFaeDtm.Archivador));
        }

    }
}
