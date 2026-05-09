using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;

namespace ServicioDeDatos.Callejero
{

    public class ltrCpsDeUnMunicipioDtm
    {
        public static readonly string CodigoPostal = $"{nameof(CpsDeUnMunicipioDtm.Cp)}.{nameof(CodigoPostalDtm.Codigo)}";
    }


    [Table(Tablas.MUNICIPIO + "_" + Tablas.CP, Schema = Esquemas.CALLEJERO)]
    public class CpsDeUnMunicipioDtm : RelacionDtm
    {
        public int IdMunicipio { get; set; }
        public MunicipioDtm Municipio { get; set; }
        public int IdCp { get; set; }
        public CodigoPostalDtm Cp { get; set; }

        public CpsDeUnMunicipioDtm()
        {
            PropiedadDelIdElemento1 = nameof(IdMunicipio);
            PropiedadDelIdElemento2 = nameof(IdCp);
        }
    }


    public static partial class ModeloDeCallejero
    {
        public static void MunicipioCp(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CpsDeUnMunicipioDtm>().Property(v => v.IdMunicipio)
                .HasColumnName("ID_MUNICIPIO")
                .HasColumnType("INT")
                .IsRequired(true);

            modelBuilder.Entity<CpsDeUnMunicipioDtm>()
                        .HasIndex(p => p.IdMunicipio)
                        .HasDatabaseName($"I_MUNICIPIO_CP_ID_MUNICIPIO");

            modelBuilder.Entity<CpsDeUnMunicipioDtm>()
            .HasOne(p => p.Municipio)
            .WithMany(m => m.Cps)
            .HasForeignKey(p => p.IdMunicipio)
            .HasConstraintName($"FK_MUNICIPIO_CP_ID_MUNICIPIO")
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CpsDeUnMunicipioDtm>().Property(v => v.IdCp)
            .HasColumnName("ID_CP")
            .HasColumnType("INT")
            .IsRequired(true);

            modelBuilder.Entity<CpsDeUnMunicipioDtm>()
                        .HasIndex(p => p.IdCp)
                        .HasDatabaseName($"I_MUNICIPIO_CP_ID_CP");

            modelBuilder.Entity<CpsDeUnMunicipioDtm>()
            .HasOne(p => p.Cp)
            .WithMany(p => p.Municipios)
            .HasForeignKey(p => p.IdCp)
            .HasConstraintName($"FK_MUNICIPIO_CP_ID_CP")
            .OnDelete(DeleteBehavior.Restrict);

            //modelBuilder.Entity<CpsDeUnMunicipioDtm>().HasAlternateKey(p => new { p.IdCp, p.IdMunicipio }).HasName("AK_MUNICIPIO_CP");
        }
    }

}

