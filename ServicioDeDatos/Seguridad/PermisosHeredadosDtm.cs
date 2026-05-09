using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;

namespace ServicioDeDatos.Seguridad
{
    public class PermisosHeredadosDtm : RelacionDtm, INecesitaSerParametrizador
    {
        public int IdPuesto { get; set; }

        public int IdPermiso { get; set; }

        public string Roles { get; set; }

        public virtual PuestoDtm Puesto { get; set; }

        public virtual PermisoDtm Permiso { get; set; }
        public PermisosHeredadosDtm()
        {
            PropiedadDelIdElemento1 = nameof(IdPuesto);
            PropiedadDelIdElemento2 = nameof(IdPermiso);
        }
    }


    public static partial class ModeloDeSeguridad
    {

        public static void CrearVistaDePermisosPorPuesto(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PermisosHeredadosDtm>().ToView(Vistas.PERMISOS_HEREDADOS, Esquemas.SEGURIDAD).HasKey(x => new { x.Id });

            modelBuilder.Entity<PermisosHeredadosDtm>().Property(p => p.Id).HasColumnName(ICampos.ID)
            .HasColumnType(IDominio.INT)
            .HasComputedColumnSql($"CAST(ROW_NUMBER() OVER(ORDER BY t2.{ICampos.IDPUESTO} ASC) as {IDominio.INT})");

            modelBuilder.Entity<PermisosHeredadosDtm>().Property(nameof(PermisosHeredadosDtm.IdPermiso)).HasColumnName(ICampos.IDPERMISO).HasColumnType(IDominio.INT).IsRequired(true);
            modelBuilder.Entity<PermisosHeredadosDtm>().Property(nameof(PermisosHeredadosDtm.IdPuesto)).HasColumnName(ICampos.IDPUESTO).HasColumnType(IDominio.INT).IsRequired(true);

            modelBuilder.Entity<PermisosHeredadosDtm>().Property(p => p.Roles)
            .HasColumnName(ICampos.ROLES).HasColumnType(IDominio.VARCHAR_MAX)
            .HasComputedColumnSql($"{Esquemas.SEGURIDAD}.{Funciones.OBTENER_ORIGEN_PUESTO_PERMISO}({ICampos.IDPUESTO}, {ICampos.IDPERMISO})");

            modelBuilder.Entity<PermisosHeredadosDtm>()
                .HasOne(x => x.Puesto)
                .WithMany(x => x.Permisos)
                .HasForeignKey(x => x.IdPuesto);

            modelBuilder.Entity<PermisosHeredadosDtm>()
                .HasOne(x => x.Permiso)
                .WithMany(x => x.Puestos)
                .HasForeignKey(x => x.IdPermiso);

        }
    }
}
