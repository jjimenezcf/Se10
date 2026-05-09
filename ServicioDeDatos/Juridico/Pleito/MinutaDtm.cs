using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using Utilidades;

namespace ServicioDeDatos.Juridico
{
    [Table(Tablas.PLEITO + "_" + nameof(Sufijo.MINUTA), Schema = Esquemas.JURIDICO)]
    public class MinutaDtm: RegistroDtm, IDetalle
    {
        public int IdElemento { get; set; }
        public PleitoDtm Elemento { get; set; }
        IElementoDtm IDetalle.Elemento => Elemento;

        public int Orden { get; set; }
        public DateTime CreadoEl { get; set; }
        public string Concepto { get; set; }
        public decimal Valor { get; set; }
        public DateTime? AbonadoEl { get; set; }
        public decimal?  Abonado { get; set; }

        public decimal Pendiente => Valor - (Abonado == null ? 0 : (decimal)Abonado);
        public enumNegocio Negocio => enumNegocio.Pleito;
    }

    public static partial class ModeloDePleito
    {
        internal static void DatosDeMinuta(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MinutaDtm>().Ignore(x => x.Elemento);
            modelBuilder.Entity<MinutaDtm>().Ignore(x => x.Negocio);
            modelBuilder.Entity<MinutaDtm>().Ignore(x => x.Pendiente);
            modelBuilder.Entity<MinutaDtm>().Property(nameof(MinutaDtm.IdElemento)).HasColumnName(ICampos.ID_ELEMENTO).HasColumnType(IDominio.INT).IsRequired();
            ApiDeRegistroDtm.DefinirFk<MinutaDtm, PleitoDtm>(modelBuilder, nameof(MinutaDtm.IdElemento), ICampos.ID_ELEMENTO, unico: false);

            modelBuilder.Entity<MinutaDtm>().Property(nameof(MinutaDtm.Orden)).HasColumnName(ICampos.ORDEN).HasColumnType(IDominio.INT).IsRequired(true);
            modelBuilder.Entity<MinutaDtm>().Property(nameof(MinutaDtm.Valor)).HasColumnName(ICampos.VALOR).HasColumnType(IDominio.DECIMAL).IsRequired(true);
            modelBuilder.Entity<MinutaDtm>().Property(nameof(MinutaDtm.Abonado)).HasColumnName(ICampos.ABONADO).HasColumnType(IDominio.DECIMAL).IsRequired(false);
            modelBuilder.Entity<MinutaDtm>().Property(nameof(MinutaDtm.Concepto)).HasColumnName(ICampos.CONCEPTO).HasColumnType(IDominio.VARCHAR_2000).IsRequired(true);

            modelBuilder.Entity<MinutaDtm>().Property(nameof(MinutaDtm.CreadoEl)).HasColumnName(ICampos.CREADO_EL).HasColumnType(IDominio.DATETIME_2).IsRequired(true);
            modelBuilder.Entity<MinutaDtm>().Property(nameof(MinutaDtm.AbonadoEl)).HasColumnName(ICampos.ABONADO_EL).HasColumnType(IDominio.DATETIME_2).IsRequired(false);
        }
    }
}
