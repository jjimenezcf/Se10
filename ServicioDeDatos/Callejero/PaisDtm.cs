using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Negocio;

namespace ServicioDeDatos.Callejero
{

    //https://libretilla.com/codigos-paises-iso-3166-1/
    public static class ltrIsoPaises
    {
        public const string Spain = "ES";
        public const string Brasil = "BR";
        public const string EEUU = "EU";
        public const string Italia = "IT";
        public const string Francia = "FR";
        public const string GranBretana = "GB";
        public const string Suecia = "SE";
        public const string Luxenburgo = "LU";
        public const string Irlanda = "IE";        
    }


    [Table(Tablas.PAIS, Schema = Esquemas.CALLEJERO)]
    public class PaisDtm : ElementoDtm
    {
        public string NombreIngles { get; set; }
        public string Codigo { get; set; }
        public string ISO2 { get; set; }
        public string Prefijo {get; set;}
        public bool EsUE {get; set; }
        public bool IntraComunitario => EsUE && ISO2 != ltrIsoPaises.Spain;
        public bool ExtraComunitario => !EsUE && ISO2 != ltrIsoPaises.Spain;
        public override string Expresion => $"({Codigo}) {Nombre}";
    }

    [Table(Tablas.PAIS + "_"+ nameof(Sufijo.AUDITORIA), Schema = Esquemas.CALLEJERO)]
    public class AuditoriaDeUnPaisDtm : AuditoriaDtm
    {
    }

    public static partial class ModeloDeCallejero
    {
        public static void Pais(ModelBuilder modelBuilder)
        {
            ApiDeElementoDtm.DefinirCamposDelElementoDtm<PaisDtm>(modelBuilder);
            modelBuilder.Entity<PaisDtm>().Property(v => v.Codigo)
                .HasColumnName(ICampos.CODIGO)
                .HasColumnType(IDominio.VARCHAR_3)
                .IsRequired(true);

            modelBuilder.Entity<PaisDtm>().Property(v => v.NombreIngles)
                .HasColumnName(ICampos.NOMBRE_INGLES)
                .HasColumnType(IDominio.VARCHAR_250)
                .IsRequired(true);

            modelBuilder.Entity<PaisDtm>().Property(v => v.ISO2)
                .HasColumnName(ICampos.ISO2)
                .HasColumnType(IDominio.VARCHAR_2)
                .IsRequired(true);

            modelBuilder.Entity<PaisDtm>().Property(v => v.Prefijo)
                .HasColumnName(ICampos.PREFIJO)
                .HasColumnType(IDominio.VARCHAR_10)
                .IsRequired(true);

            modelBuilder.Entity<PaisDtm>().HasIndex(p => p.Codigo).HasDatabaseName($"I_{Tablas.PAIS}_{ICampos.CODIGO}").IsUnique(true);
            modelBuilder.Entity<PaisDtm>().HasIndex(p => p.ISO2).HasDatabaseName($"I_{Tablas.PAIS}_{ICampos.ISO2}").IsUnique(true);

            modelBuilder.Entity<PaisDtm>().HasIndex(p => p.NombreIngles).HasDatabaseName($"AK_{Tablas.PAIS}_NAME").IsUnique(true);
            modelBuilder.Entity<PaisDtm>().Property(nameof(PaisDtm.EsUE)).HasColumnName(ICampos.ES_UE).HasColumnType(IDominio.BIT).IsRequired(true);

        }

        public static void PaisAudt(ModelBuilder modelBuilder)
        {
            Negocio.ApiDeAuditoria.DefinirCamposDeAuditoriaDtm<AuditoriaDeUnPaisDtm>(modelBuilder);

            //modelBuilder.Entity<AuditoriaDeUnPaisDtm>()
            //.HasOne(p => p.Elemento)
            //.WithMany()
            //.HasForeignKey(p => p.IdElemento)
            //.HasConstraintName($"FK_PAIS_AUDITORIA_ID_ELEMENTO")
            //.OnDelete(DeleteBehavior.Restrict);
        }
    }

}
