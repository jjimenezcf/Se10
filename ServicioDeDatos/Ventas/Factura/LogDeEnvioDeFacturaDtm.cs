using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServicioDeDatos.Ventas
{
    public static class ltrDeLogDeEnvioDeFactura
    {
        public const string lote = nameof(lote);
        public const string IdSociedad = nameof(IdSociedad);
        public const string IdSemaforo = nameof(IdSemaforo);


        public const string SometerEnvio = nameof(SometerEnvio);
    }

    [Table(Tablas.FACTURA_EMT + "_" + nameof(Sufijo.LOG_AEAT), Schema = Esquemas.VENTA)]
    public class LogDeEnvioDeFacturaDtm : RegistroDtm
    {
        public DateTime GeneradaEl { get; set; }
        public DateTime? EnviadaEl { get; set; }

        public int IdFactura { get; set; }
        public FacturaEmtDtm Factura { get; set; }

    }


    public static partial class ModeloDeFacturaEmt
    {

        internal static void LogDeEnvioDeFactura(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LogDeEnvioDeFacturaDtm>().Property(p => p.GeneradaEl).HasColumnName(ICampos.GENERADA_EL).HasColumnType(IDominio.DATETIME_2).IsRequired(true);
            modelBuilder.Entity<LogDeEnvioDeFacturaDtm>().Property(p => p.EnviadaEl).HasColumnName(ICampos.ENVIADA_EL).HasColumnType(IDominio.DATETIME_2).IsRequired(false);
            ApiDeRegistroDtm.DefinirCampoFk<LogDeEnvioDeFacturaDtm>(modelBuilder, nameof(LogDeEnvioDeFacturaDtm.Factura), nameof(LogDeEnvioDeFacturaDtm.IdFactura), ICampos.ID_FACTURA_EMT, requerida: true, unico: true);
        }
    }
}
