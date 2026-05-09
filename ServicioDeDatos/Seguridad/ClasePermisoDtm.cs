using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServicioDeDatos.Seguridad
{

    [Table(Tablas.CLASE_PERMISO, Schema = Esquemas.SEGURIDAD)]
    public class ClasePermisoDtm : RegistroConNombreDtm
    {
        public virtual ICollection<PermisoDtm> Permisos { get; set; }
    }

    public static class TablaClasePermiso
    {
        public static void Definir(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ClasePermisoDtm>().Property(p => p.Nombre).HasColumnName(ICampos.NOMBRE).HasColumnType(IDominio.VARCHAR_30).IsRequired();
            modelBuilder.Entity<ClasePermisoDtm>()
                        .HasIndex(cp => cp.Nombre)
                        .HasDatabaseName($"I_{Tablas.CLASE_PERMISO}_{ICampos.NOMBRE}")
                        .IsUnique();

            modelBuilder.Entity<ClasePermisoDtm>()
                .HasMany(cp => cp.Permisos)
                .WithOne(p => p.Clase)
                .HasForeignKey(p=>p.IdClase);

        }
    }
}
