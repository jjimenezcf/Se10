using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Negocio;

namespace ServicioDeDatos.Callejero
{
    [Table(Tablas.BARRIO, Schema = Esquemas.CALLEJERO)]
    public class BarrioDtm : ElementoDtm
    {
        public int IdMunicipio { get; set; }
        public MunicipioDtm Municipio { get; set; }
        public List<BarriosDeUnaCalleDtm> CallesDeUnBarrio { get; set; }
    }

    [Table(Tablas.BARRIO + "_" + nameof(Sufijo.AUDITORIA), Schema = Esquemas.CALLEJERO)]
    public class AuditoriaDeUnBarrioDtm : AuditoriaDtm
    {
        //public new virtual BarrioDtm Elemento { get; set; }
    }
    public static partial class ModeloDeCallejero
    {
        public static void Barrio(ModelBuilder modelBuilder)
        {
            ApiDeElementoDtm.DefinirCamposDelElementoDtm<BarrioDtm>(modelBuilder);

            modelBuilder.Entity<BarrioDtm>().Property(v => v.IdMunicipio)
                .HasColumnName(ICampos.ID_MUNICIPIO)
                .HasColumnType(IDominio.INT)
                .IsRequired(true);

            modelBuilder.Entity<BarrioDtm>().HasIndex(p => new { p.IdMunicipio, p.Nombre }).HasDatabaseName($"I_{Tablas.BARRIO}_{ICampos.ID_MUNICIPIO}_{ICampos.NOMBRE}").IsUnique(true);

            modelBuilder.Entity<BarrioDtm>()
                        .HasIndex(p => p.IdMunicipio)
                        .HasDatabaseName($"I_{Tablas.BARRIO}_{ICampos.ID_MUNICIPIO}");

            modelBuilder.Entity<BarrioDtm>()
            .HasOne(p => p.Municipio)
            .WithMany()
            .HasForeignKey(p => p.IdMunicipio)
            .HasConstraintName($"FK_{Tablas.BARRIO}_{ICampos.ID_MUNICIPIO}")
            .OnDelete(DeleteBehavior.Restrict);
        }

        public static void BarrioAudt(ModelBuilder modelBuilder)
        {
            ApiDeAuditoria.DefinirCamposDeAuditoriaDtm<AuditoriaDeUnBarrioDtm>(modelBuilder);
        }
    }
}
