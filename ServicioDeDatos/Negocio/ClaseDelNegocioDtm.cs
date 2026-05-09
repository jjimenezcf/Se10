using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;

namespace ServicioDeDatos.Negocio
{

    [Table(Tablas.CLASE, Schema = Esquemas.NEGOCIO)]
    public class ClaseDelNegocioDtm : RegistroConNombreDtm, IRegistroDeParametrizacion, ITieneCampoNegocio
    {
        public string Referencia { get; set; }
        public int IdNegocio { get; set; }
        public NegocioDtm Negocio { get; set; }
        public bool Activa { get; set; }

        public override string Expresion => $"({Referencia}) {base.Expresion}";
    }

    public static partial class ModeloDeNegocio
    {
        public static void ClasesDelDeNegocio(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ClaseDelNegocioDtm>().Property(p => p.Nombre).HasColumnName(ICampos.NOMBRE).HasColumnType(IDominio.VARCHAR_250).IsRequired();
            modelBuilder.Entity<ClaseDelNegocioDtm>().Property(p => p.Referencia).HasColumnName(ICampos.REFERENCIA).HasColumnType(IDominio.VARCHAR_15).IsRequired();
            DefinirCampoNegocio<ClaseDelNegocioDtm>(modelBuilder, unico: false);
            modelBuilder.Entity<ClaseDelNegocioDtm>()
               .HasIndex(p => new { p.IdNegocio, p.Nombre })
               .IsUnique(true)
               .HasDatabaseName($"I_{Tablas.CLASE}_{ICampos.ID_NEGOCIO}_{ICampos.NOMBRE}");
            modelBuilder.Entity<ClaseDelNegocioDtm>()
               .HasIndex(p => new { p.IdNegocio, p.Referencia })
               .IsUnique(true)
               .HasDatabaseName($"I_{Tablas.CLASE}_{ICampos.ID_NEGOCIO}_{ICampos.REFERENCIA}");
            modelBuilder.Entity<ClaseDelNegocioDtm>().Property(x => x.Activa).HasColumnName(ICampos.ACTIVA).HasColumnType(IDominio.BIT).IsRequired(true).HasDefaultValue(true);

        }
    }

}
