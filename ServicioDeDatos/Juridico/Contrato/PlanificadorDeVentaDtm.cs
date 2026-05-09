using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Ventas;
using static ServicioDeDatos.Elemento.Enumerados;

namespace ServicioDeDatos.Juridico
{

    [Table(Tablas.PLANIFICADOR_VENTA, Schema = Esquemas.JURIDICO)]
    public class PlanificadorDeVentaDtm : ElementoDtm
    {
        public int IdContrato { get; set; }
        public ContratoDtm Contrato { get; set; }
        public DateTime Inicio { get; set; }
        public DateTime Hasta { get; set; }

        public bool Generado { get; set; }

        public int IdCgDeLaPlanificacion { get; set; }
        public int IdTipoDePlanificacion { get; set; }
        public int? IdTipoDeFactura { get; set; }
        public int? IdTipoDeParte { get; set; }
        public int? IdLote { get; set; }
        public TipoDePlanificacionDeVentaDtm TipoDePlanificacion { get; set; }    
        public TipoDeFacturaEmtDtm TipoDeFactura { get; set; }
        public TipoDeParteTrDtm TipoDeParte { get; set; }
        public CentroGestorDtm CgDeLaPlanificacion { get; set; }
        public LoteDeUnContratoDtm Lote { get; set; }
        public int RepetirCada { get; set; }
        public enumPeriodicidad Periodicidad { get; set; }
    }

    [Table(Tablas.PLANIFICADOR_VENTA + "_" + nameof(Sufijo.AUDITORIA), Schema = Esquemas.JURIDICO)]
    public class AuditoriaDeUnPlanificadorDeVentaDtm : AuditoriaDtm
    {
    }
    public static partial class ModeloDeContrato
    {
        public static void PlanificadorDeVenta(ModelBuilder modelBuilder)
        {
            ApiDeElementoDtm.DefinirCamposDelElementoDtm<PlanificadorDeVentaDtm>(modelBuilder);
            modelBuilder.Entity<PlanificadorDeVentaDtm>().Property(p => p.Inicio).HasColumnName(ICampos.INICIO).HasColumnType(IDominio.DATETIME_2).IsRequired();
            modelBuilder.Entity<PlanificadorDeVentaDtm>().Property(p => p.Hasta).HasColumnName(ICampos.FIN).HasColumnType(IDominio.DATETIME_2).IsRequired();

            ApiDeRegistroDtm.DefinirDependenciaConIndPorNombre<PlanificadorDeVentaDtm, ContratoDtm>(modelBuilder, apuntadoPor: nameof(PlanificadorDeVentaDtm.IdContrato), idCampo: ICampos.ID_CONTRATO);
            ApiDeRegistroDtm.DefinirDependencia<PlanificadorDeVentaDtm, CentroGestorDtm>(modelBuilder, apuntadoPor: nameof(PlanificadorDeVentaDtm.IdCgDeLaPlanificacion), idCampo: ICampos.ID_CG);
            ApiDeRegistroDtm.DefinirDependencia<PlanificadorDeVentaDtm, TipoDePlanificacionDeVentaDtm>(modelBuilder, apuntadoPor: nameof(PlanificadorDeVentaDtm.IdTipoDePlanificacion), idCampo: ICampos.ID_TIPO_PLANIFICACION);
            ApiDeRegistroDtm.DefinirDependencia<PlanificadorDeVentaDtm, TipoDeFacturaEmtDtm>(modelBuilder, apuntadoPor: nameof(PlanificadorDeVentaDtm.IdTipoDeFactura), idCampo: ICampos.ID_TIPO_FACTURA, requerido:false);
            ApiDeRegistroDtm.DefinirDependencia<PlanificadorDeVentaDtm, TipoDeParteTrDtm>(modelBuilder, apuntadoPor: nameof(PlanificadorDeVentaDtm.IdTipoDeParte), idCampo: ICampos.ID_TIPO_PARTE, requerido: false);
            ApiDeRegistroDtm.DefinirDependencia<PlanificadorDeVentaDtm, LoteDeUnContratoDtm>(modelBuilder, apuntadoPor: nameof(PlanificadorDeVentaDtm.IdLote), idCampo: ICampos.ID_LOTE, requerido: false);

            modelBuilder.Entity<PlanificadorDeVentaDtm>().Property(p => p.Periodicidad).HasColumnName(ICampos.PERIODICIDAD).HasColumnType(IDominio.VARCHAR_30).IsRequired();
            modelBuilder.Entity<PlanificadorDeVentaDtm>().Property(p => p.RepetirCada).HasColumnName(ICampos.REPETIR_CADA).IsRequired(true).HasColumnType(IDominio.INT);
            modelBuilder.Entity<PlanificadorDeVentaDtm>().Property(p => p.Generado).HasColumnName(ICampos.GENERADO).HasColumnType(IDominio.BIT).IsRequired(true);
        }

        public static void PlanificadorDeVentaAudt(ModelBuilder modelBuilder)
        {
            ApiDeAuditoria.DefinirCamposDeAuditoriaDtm<AuditoriaDeUnPlanificadorDeVentaDtm>(modelBuilder);
        }
    }
}
