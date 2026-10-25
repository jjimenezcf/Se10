using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Terceros;

namespace ServicioDeDatos.Contabilidad
{
    // Registro de los lotes contables generados para una sociedad, lo mantienen los trabajos de crear y anular lote contable
    [Table(Tablas.CIERRE_CONTABLE, Schema = Esquemas.CONTABILIDAD)]
    public class CierreContableDtm : ElementoDtm
    {
        public int IdSociedad { get; set; }
        public SociedadDtm Sociedad { get; set; }
        public int IdLote { get; set; }
        public CircuitoDocDtm Lote { get; set; }
        public DateTime CerradaEl { get; set; }
    }

    [Table(Tablas.CIERRE_CONTABLE + "_" + nameof(Sufijo.AUDITORIA), Schema = Esquemas.CONTABILIDAD)]
    public class AuditoriaDeUnCierreContableDtm : AuditoriaDtm
    {
    }

    public static partial class ModeloContable
    {
        public static void AuditoriaDeCierresContables(ModelBuilder modelBuilder)
        {
            ApiDeAuditoria.DefinirCamposDeAuditoriaDtm<AuditoriaDeUnCierreContableDtm>(modelBuilder);
        }

        public static void CierresContables(ModelBuilder modelBuilder)
        {
            ApiDeElementoDtm.DefinirCamposDelElementoDtm<CierreContableDtm>(modelBuilder);

            ApiDeRegistroDtm.DefinirCampoFk<CierreContableDtm>(modelBuilder, nameof(CierreContableDtm.Sociedad), nameof(CierreContableDtm.IdSociedad), ICampos.ID_SOCIEDAD, requerida: true, unico: false);
            ApiDeRegistroDtm.DefinirCampoFk<CierreContableDtm>(modelBuilder, nameof(CierreContableDtm.Lote), nameof(CierreContableDtm.IdLote), ICampos.ID_LOTE, requerida: true, unico: true);
            modelBuilder.Entity<CierreContableDtm>().Property(p => p.CerradaEl).HasColumnName(ICampos.CERRADA_EL).HasColumnType(IDominio.DATETIME_2).IsRequired(true);
        }
    }
}
