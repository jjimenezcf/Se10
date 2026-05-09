using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;

namespace ServicioDeDatos.Callejero
{

    public enum enumManoDeUnaCalle { Ambos, Par, Impar}

    public static class ParseosDeManosDeUnaCalle
    {
        public const string Ambos = "A";

        public static string ToBd(this enumManoDeUnaCalle mano)
        {
            switch (mano)
            {
                case enumManoDeUnaCalle.Ambos: return Ambos;
                case enumManoDeUnaCalle.Par: return "P";
                case enumManoDeUnaCalle.Impar: return "I";
            }
            throw new Exception($"El parseo a BD asociado a {mano} no se ha definido");
        }
    }

    [Table(Tablas.CALLE + "_" + Tablas.CP, Schema = Esquemas.CALLEJERO)]
    public class CpsDeUnaCalleDtm : RelacionDtm
    {
        public int IdCalle { get; set; }
        public CalleDtm Calle { get; set; }
        public int IdCp { get; set; }
        public CodigoPostalDtm Cp { get; set; }
        public int Desde { get; set; }
        public int Hasta { get; set; }
        public string Mano { get; set; }

        public CpsDeUnaCalleDtm()
        {
            PropiedadDelIdElemento1 = nameof(IdCalle);
            PropiedadDelIdElemento2 = nameof(IdCp);
        }
    }
    
    public static partial class ModeloDeCallejero
    {
        public static void CalleCp(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CpsDeUnaCalleDtm>().Property(v => v.IdCalle)
                .HasColumnName("ID_CALLE")
                .HasColumnType("INT")
                .IsRequired(true);

            modelBuilder.Entity<CpsDeUnaCalleDtm>()
                        .HasIndex(p => p.IdCalle)
                        .HasDatabaseName($"I_CALLE_CP_ID_CALLE");

            modelBuilder.Entity<CpsDeUnaCalleDtm>()
            .HasOne(p => p.Calle)
            .WithMany(c => c.Cps)
            .HasForeignKey(p => p.IdCalle)
            .HasConstraintName($"FK_CALLE_CP_ID_CALLE")
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CpsDeUnaCalleDtm>().Property(v => v.IdCp)
            .HasColumnName("ID_CP")
            .HasColumnType("INT")
            .IsRequired(true);

            modelBuilder.Entity<CpsDeUnaCalleDtm>()
                        .HasIndex(p => p.IdCp)
                        .HasDatabaseName($"I_CALLE_CP_ID_CP");

            modelBuilder.Entity<CpsDeUnaCalleDtm>()
            .HasOne(p => p.Cp)
            .WithMany(p => p.Calles)
            .HasForeignKey(p => p.IdCp)
            .HasConstraintName($"FK_CALLE_CP_ID_CP")
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CpsDeUnaCalleDtm>().HasAlternateKey(p => new {p.IdCp, p.IdCalle}).HasName("AK_CALLE_CP_ID_CP_ID_CALLE");

            modelBuilder.Entity<CpsDeUnaCalleDtm>().Property(v => v.Mano).IsRequired().HasColumnName("MANO").HasColumnType("CHAR(1)").HasDefaultValue("A");
            modelBuilder.Entity<CpsDeUnaCalleDtm>().Property(v => v.Desde).IsRequired().HasColumnName("DESDE").HasColumnType("DECIMAL(5)").HasDefaultValue(0);
            modelBuilder.Entity<CpsDeUnaCalleDtm>().Property(v => v.Hasta).IsRequired().HasColumnName("HASTA").HasColumnType("DECIMAL(5)").HasDefaultValue(99999);

        }
    }

}
