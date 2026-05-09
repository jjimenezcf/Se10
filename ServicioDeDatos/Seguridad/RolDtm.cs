using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServicioDeDatos.Seguridad
{

    [Table("ROL", Schema = "SEGURIDAD")]
    public class RolDtm : RegistroConNombreDtm, IUsaDescripcion
    {
        public string Descripcion { get; set; }

        public ICollection<PermisosDeUnRolDtm> Permisos { get; set; }
        public ICollection<RolesDeUnPuestoDtm> Puestos { get; set; }
    }

    public static partial class ModeloDeSeguridad
    {
        public static void Rol(ModelBuilder modelBuilder)
        {
            ApiDeNombreDtm.DefinirCampoNombreDtm<RolDtm>(modelBuilder, unico:true);
            ApiDeNombreDtm.DefinirCampoDescripcion<RolDtm>(modelBuilder);

            modelBuilder.Entity<RolDtm>()
                    .HasMany(p => p.Permisos)
                    .WithOne(p => p.Rol)
                    .HasForeignKey(p => p.IdPermiso);

            modelBuilder.Entity<RolDtm>()
                    .HasMany(p => p.Puestos)
                    .WithOne(p => p.Rol)
                    .HasForeignKey(p => p.IdRol);
        }
    }
}
