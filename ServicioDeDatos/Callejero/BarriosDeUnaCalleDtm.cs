using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;

namespace ServicioDeDatos.Callejero
{

    public class ltrBarriosDeUnaCalleDtm
    {
        public const string IdMunicipio = nameof(BarriosDeUnaCalleDtm.Barrio) + "." + nameof(BarrioDtm.IdMunicipio);
    }


    [Table(Tablas.CALLE + "_" + Tablas.BARRIO, Schema = Esquemas.CALLEJERO)]
    public class BarriosDeUnaCalleDtm : RelacionDtm
    {
        public int IdCalle { get; set; }
        public CalleDtm Calle { get; set; }
        public int IdBarrio { get; set; }
        public BarrioDtm Barrio { get; set; }
        public int Desde { get; set; }
        public int Hasta { get; set; }
        public string Mano { get; set; }

        public BarriosDeUnaCalleDtm()
        {
            PropiedadDelIdElemento1 = nameof(IdCalle);
            PropiedadDelIdElemento2 = nameof(IdBarrio);
        }
    }
    
    public static partial class ModeloDeCallejero
    {
        public static void CalleBarrio(ModelBuilder modelBuilder)
        {
            var tabla = ApiDeRegistroDtm.NombreDeTabla(typeof(BarriosDeUnaCalleDtm));

            modelBuilder.Entity<BarriosDeUnaCalleDtm>().Property(v => v.IdCalle)
             .HasColumnName(ICampos.ID_CALLE)
             .HasColumnType(IDominio.INT)
             .IsRequired(true);

            modelBuilder.Entity<BarriosDeUnaCalleDtm>()
                        .HasIndex(p => p.IdCalle)
                        .HasDatabaseName($"I_{tabla}_{ICampos.ID_CALLE}");

            modelBuilder.Entity<BarriosDeUnaCalleDtm>()
            .HasOne(p => p.Calle)
            .WithMany(c => c.Barrios)
            .HasForeignKey(p => p.IdCalle)
            .HasConstraintName($"FK_{tabla}_{ICampos.ID_CALLE}")
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BarriosDeUnaCalleDtm>().Property(v => v.IdBarrio)
            .HasColumnName(ICampos.ID_BARRIO)
            .HasColumnType(IDominio.INT)
            .IsRequired(true);

            modelBuilder.Entity<BarriosDeUnaCalleDtm>()
                        .HasIndex(p => p.IdBarrio)
                        .HasDatabaseName($"I_{tabla}_{ICampos.ID_BARRIO}");

            modelBuilder.Entity<BarriosDeUnaCalleDtm>()
            .HasOne(p => p.Barrio)
            .WithMany(p => p.CallesDeUnBarrio)
            .HasForeignKey(p => p.IdBarrio)
            .HasConstraintName($"FK_{tabla}_{ICampos.ID_BARRIO}")
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BarriosDeUnaCalleDtm>().HasAlternateKey(p => new { p.IdBarrio, p.IdCalle }).HasName($"AK_{tabla}_{ICampos.ID_BARRIO}_{ICampos.ID_CALLE}");


            modelBuilder.Entity<BarriosDeUnaCalleDtm>().Property(v => v.Mano).IsRequired().HasColumnName(ICampos.MANO).HasColumnType(IDominio.CHAR_1).HasDefaultValue("A");
            modelBuilder.Entity<BarriosDeUnaCalleDtm>().Property(v => v.Desde).IsRequired().HasColumnName(ICampos.DESDE).HasColumnType(IDominio.DECIMAL_5).HasDefaultValue(0);
            modelBuilder.Entity<BarriosDeUnaCalleDtm>().Property(v => v.Hasta).IsRequired().HasColumnName(ICampos.HASTA).HasColumnType(IDominio.DECIMAL_5).HasDefaultValue(99999);

        }
    }

}
