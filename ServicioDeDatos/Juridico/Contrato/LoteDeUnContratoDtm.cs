using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Negocio;

namespace ServicioDeDatos.Juridico
{
    public class ltrLotesDeUnContrato
    {
        public static readonly string JoinConContratos = nameof(JoinConContratos);
        public static readonly string ConLotes = nameof(ConLotes);
        public static readonly string SinLotes = nameof(SinLotes);
    }

    [Table(Tablas.LOTE, Schema = Esquemas.JURIDICO)]
    public class LoteDeUnContratoDtm : ElementoDtm
    {
        public int IdContrato { get; set; }
        public ContratoDtm Contrato { get; set; }
        public DateTime? VigenteDesde { get; set; }
        public DateTime? VigenteHasta { get; set; }

        public bool Activo =>
            (VigenteHasta.HasValue && ((DateTime)VigenteHasta).Date <= DateTime.Now.Date && VigenteDesde.HasValue && ((DateTime)VigenteDesde).Date >= DateTime.Now.Date) ||
            (!VigenteHasta.HasValue && VigenteDesde.HasValue && ((DateTime)VigenteDesde).Date <= DateTime.Now.Date) ||
            (!VigenteDesde.HasValue && VigenteHasta.HasValue && ((DateTime)VigenteHasta).Date <= DateTime.Now.Date);


    }

    [Table(Tablas.LOTE + "_" + nameof(Sufijo.AUDITORIA), Schema = Esquemas.JURIDICO)]
    public class AuditoriaDeUnLoteDeUnContratoDtm : AuditoriaDtm
    {
    }
    public static partial class ModeloDeContrato
    {
        public static void LoteDeUnContrato(ModelBuilder modelBuilder)
        {
            ApiDeElementoDtm.DefinirCamposDelElementoDtm<LoteDeUnContratoDtm>(modelBuilder);
            ApiDeRegistroDtm.DefinirDependenciaConIndPorNombre<LoteDeUnContratoDtm, ContratoDtm>(modelBuilder, apuntadoPor: nameof(PlanificadorDeVentaDtm.IdContrato), idCampo: ICampos.ID_CONTRATO);
            modelBuilder.Entity<LoteDeUnContratoDtm>().Property(p => p.VigenteDesde).HasColumnName(ICampos.INICIO).HasColumnType(IDominio.DATETIME_2).IsRequired(false);
            modelBuilder.Entity<LoteDeUnContratoDtm>().Property(p => p.VigenteHasta).HasColumnName(ICampos.FIN).HasColumnType(IDominio.DATETIME_2).IsRequired(false);
        }

        public static void LoteDeUnContratoAudt(ModelBuilder modelBuilder)
        {
            ApiDeAuditoria.DefinirCamposDeAuditoriaDtm<AuditoriaDeUnLoteDeUnContratoDtm>(modelBuilder);
        }
    }
}
