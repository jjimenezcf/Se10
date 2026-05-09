using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServicioDeDatos.Seguridad
{

    [Table("TIPO_PERMISO", Schema = "SEGURIDAD")]
    public class TipoPermisoDtm : RegistroConNombreDtm
    {
        public virtual ICollection<PermisoDtm> Permisos { get; set; }
    }

    public static class TablaPermisoTipo
    {
        public static void Definir(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TipoPermisoDtm>().Property(p => p.Nombre).HasColumnName("NOMBRE").HasColumnType("VARCHAR(30)").IsRequired();

            modelBuilder.Entity<TipoPermisoDtm>()
                        .HasIndex(tp => tp.Nombre)
                        .HasDatabaseName("I_TIPO_PERMISO_NOMBRE")
                        .IsUnique();

            modelBuilder.Entity<TipoPermisoDtm>()
                .HasMany(tp => tp.Permisos)
                .WithOne(p => p.Tipo)
                .HasForeignKey(p => p.IdTipo);

        }
    }
}