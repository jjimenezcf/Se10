using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServicioDeDatos.Seguridad
{
    [Table(Tablas.ROL_PERMISO, Schema = Esquemas.SEGURIDAD)]
    public class PermisosDeUnRolDtm : RelacionDtm, INecesitaSerParametrizador
    {
        public int IdRol { get; set; }

        public int IdPermiso { get; set; }

        public RolDtm Rol { get; set; }
        public PermisoDtm Permiso { get; set; }

        public PermisosDeUnRolDtm()
        {
            PropiedadDelIdElemento1 = nameof(IdRol);
            PropiedadDelIdElemento2 = nameof(IdPermiso);
        }

    }

    public static partial class ModeloDeSeguridad
    {
        internal static string TablaPermisosDeUnRol => $"{ApiDeRegistroDtm.EsquemaTabla(typeof(PermisosDeUnRolDtm))}";

        public static void PermisosDeUnRol(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PermisosDeUnRolDtm>()
            .HasAlternateKey(p => new { p.IdRol, p.IdPermiso })
            .HasName($"AK_{ApiDeRegistroDtm.NombreDeTabla(typeof(PermisosDeUnRolDtm))}");

            modelBuilder.Entity<PermisosDeUnRolDtm>().Property(nameof(PermisosDeUnRolDtm.IdRol)).HasColumnName(ICampos.IDROL).HasColumnType(IDominio.INT).IsRequired(true);
            ApiDeRegistroDtm.DefinirFkConMuchos<PermisosDeUnRolDtm>(modelBuilder, nameof(PermisosDeUnRolDtm.Rol), nameof(PermisosDeUnRolDtm.IdRol), nameof(RolDtm.Permisos), ICampos.IDROL, false);

            modelBuilder.Entity<PermisosDeUnRolDtm>().Property(nameof(PermisosDeUnRolDtm.IdPermiso)).HasColumnName(ICampos.IDPERMISO).HasColumnType(IDominio.INT).IsRequired(true);
            ApiDeRegistroDtm.DefinirFkConMuchos<PermisosDeUnRolDtm>(modelBuilder, nameof(PermisosDeUnRolDtm.Permiso), nameof(PermisosDeUnRolDtm.IdPermiso), nameof(PermisoDtm.Roles), ICampos.IDPERMISO, false);
        }
    }

}

