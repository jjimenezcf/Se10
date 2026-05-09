using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Negocio;

namespace ServicioDeDatos.Callejero
{
    [Table(Tablas.ZONA, Schema = Esquemas.CALLEJERO)]
    public class ZonaDtm : ElementoDtm
    {
        public int IdMunicipio { get; set; }
        public MunicipioDtm Municipio { get; set; }

        public List<ZonasDeUnaCalleDtm> CallesDeUnaZona { get; set; }
    }

    [Table(Tablas.ZONA + "_"+ nameof(Sufijo.AUDITORIA), Schema = Esquemas.CALLEJERO)]
    public class AuditoriaDeUnZonaDtm : AuditoriaDtm
    {
        //public new virtual ZonaDtm Elemento { get; set; }
    }
    public static partial class ModeloDeCallejero
    {
        public static void Zona(ModelBuilder modelBuilder)
        {
            ApiDeElementoDtm.DefinirCamposDelElementoDtm<ZonaDtm>(modelBuilder);

            modelBuilder.Entity<ZonaDtm>().Property(v => v.IdMunicipio)
                .HasColumnName("ID_MUNICIPIO")
                .HasColumnType("INT")
                .IsRequired(true);

            modelBuilder.Entity<ZonaDtm>().HasAlternateKey(p => new { p.IdMunicipio, p.Nombre }).HasName("AK_ZONA_ID_MUNICIPIO_NOMBRE");

            modelBuilder.Entity<ZonaDtm>()
                        .HasIndex(p => p.IdMunicipio)
                        .HasDatabaseName($"I_ZONA_ID_MUNICIPIO");

            modelBuilder.Entity<ZonaDtm>()
            .HasOne(p => p.Municipio)
            .WithMany()
            .HasForeignKey(p => p.IdMunicipio)
            .HasConstraintName($"FK_ZONA_ID_MUNICIPIO")
            .OnDelete(DeleteBehavior.Restrict);
        }

        public static void ZonaAudt(ModelBuilder modelBuilder)
        {
            Negocio.ApiDeAuditoria.DefinirCamposDeAuditoriaDtm<AuditoriaDeUnZonaDtm>(modelBuilder);

            //modelBuilder.Entity<AuditoriaDeUnZonaDtm>()
            //.HasOne(p => p.Elemento)
            //.WithMany()
            //.HasForeignKey(p => p.IdElemento)
            //.HasConstraintName($"FK_ZONA_AUDITORIA_ID_ELEMENTO")
            //.OnDelete(DeleteBehavior.Restrict);
        }
    }
}
