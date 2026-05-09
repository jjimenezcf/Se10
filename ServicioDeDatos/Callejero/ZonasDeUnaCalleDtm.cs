using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;

namespace ServicioDeDatos.Callejero
{

    public class ltrZonasDeUnaCalleDtm
    {
        public const string IdMunicipio = nameof(ZonasDeUnaCalleDtm.Zona) + "." + nameof(ZonaDtm.IdMunicipio);
    }


    [Table(Tablas.CALLE + "_" + Tablas.ZONA, Schema = Esquemas.CALLEJERO)]
    public class ZonasDeUnaCalleDtm : RelacionDtm
    {
        public int IdCalle { get; set; }
        public CalleDtm Calle { get; set; }
        public int IdZona { get; set; }
        public ZonaDtm Zona { get; set; }

        public ZonasDeUnaCalleDtm()
        {
            PropiedadDelIdElemento1 = nameof(IdCalle);
            PropiedadDelIdElemento2 = nameof(IdZona);
        }
    }
    
    public static partial class ModeloDeCallejero
    {
        public static void CalleZona(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ZonasDeUnaCalleDtm>().Property(v => v.IdCalle)
                .HasColumnName(ICampos.ID_CALLE)
                .HasColumnType(IDominio.INT)
                .IsRequired(true);

            modelBuilder.Entity<ZonasDeUnaCalleDtm>()
                        .HasIndex(p => p.IdCalle)
                        .HasDatabaseName($"I_CALLE_ZONA_{ICampos.ID_CALLE}");

            modelBuilder.Entity<ZonasDeUnaCalleDtm>()
            .HasOne(p => p.Calle)
            .WithMany(c => c.Zonas)
            .HasForeignKey(p => p.IdCalle)
            .HasConstraintName($"FK_CALLE_ZONA_{ICampos.ID_CALLE}")
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ZonasDeUnaCalleDtm>().Property(v => v.IdZona)
            .HasColumnName(ICampos.ID_ZONA)
            .HasColumnType(IDominio.INT)
            .IsRequired(true);

            modelBuilder.Entity<ZonasDeUnaCalleDtm>()
                        .HasIndex(p => p.IdZona)
                        .HasDatabaseName($"I_CALLE_ZONA_{ICampos.ID_ZONA}");

            modelBuilder.Entity<ZonasDeUnaCalleDtm>()
            .HasOne(p => p.Zona)
            .WithMany(p => p.CallesDeUnaZona)
            .HasForeignKey(p => p.IdZona)
            .HasConstraintName($"FK_CALLE_ZONA_{ICampos.ID_ZONA}")
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ZonasDeUnaCalleDtm>().HasAlternateKey(p => new {p.IdZona, p.IdCalle}).HasName($"AK_CALLE_ZONA_{ICampos.ID_ZONA}_{ICampos.ID_CALLE}");
        }
    }

}
