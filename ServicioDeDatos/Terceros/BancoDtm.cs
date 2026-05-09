using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Callejero;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Negocio;

namespace ServicioDeDatos.Terceros
{
    [Table(Tablas.BANCO, Schema = Esquemas.TERCEROS)]
    public class BancoDtm : ElementoDtm
    {
        public int IdPais { get; set; }
        public PaisDtm Pais { get; set; }
        public string BicSwift { get; set; }
        public string Codigo { get; set; }
    }

    [Table(Tablas.BANCO + "_" + nameof(Sufijo.AUDITORIA), Schema = Esquemas.TERCEROS)]
    public class AuditoriaDeUnBancoDtm : AuditoriaDtm
    {
    }


    public static partial class ModeloDeTerceros
    {
        public static void Banco(ModelBuilder modelBuilder)
        {
            ApiDeElementoDtm.DefinirCamposDelElementoDtm<BancoDtm>(modelBuilder, indiceUnicoPorNombre: true);
            ApiDeRegistroDtm.DefinirDependencia<BancoDtm, PaisDtm>(modelBuilder, apuntadoPor: nameof(BancoDtm.IdPais), idCampo: ICampos.ID_PAIS, requerido: true);
            modelBuilder.Entity<BancoDtm>().Property(v => v.Codigo).HasColumnName(ICampos.CODIGO).HasColumnType(IDominio.VARCHAR_4).IsRequired(true);
            modelBuilder.Entity<BancoDtm>().Property(v => v.BicSwift).HasColumnName(ICampos.BIC_SWIFT).HasColumnType(IDominio.VARCHAR_11).IsRequired(true);
            modelBuilder.Entity<BancoDtm>().HasIndex(new string[] { nameof(BancoDtm.IdPais), nameof( BancoDtm.Codigo) }).HasDatabaseName($"I_{Tablas.BANCO}_{ICampos.ID_PAIS}_{ICampos.CODIGO}").IsUnique(true);
            modelBuilder.Entity<BancoDtm>().HasIndex(new string[] { nameof(BancoDtm.BicSwift) }).HasDatabaseName($"I_{Tablas.BANCO}_{ICampos.BIC_SWIFT}").IsUnique(true);
        }

        public static void BancoAudt(ModelBuilder modelBuilder)
        {
            ApiDeAuditoria.DefinirCamposDeAuditoriaDtm<AuditoriaDeUnBancoDtm>(modelBuilder);
        }

    }

}

