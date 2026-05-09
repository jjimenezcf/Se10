using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Negocio;

namespace ServicioDeDatos.Seguridad
{
    [Table(Tablas.PERMISO_POR_ESTADO, Schema = Esquemas.SEGURIDAD)]
    public class PermisosPorEstadoDtm : RegistroDtm, IPermisoOtorgado
    {
        public int IdNegocio { get; set; }
        public int IdEstado { get; set; }
        public int IdUsuario { get; set; }
        public int IdPermiso { get; set; }
        public bool Calculado { get; set; }

        public NegocioDtm Negocio { get; }
        public UsuarioDtm Usuario { get; }
        public PermisoDtm Permiso { get; }

    }

    public static partial class ModeloDeSeguridad
    {
        internal static string TablaDePermisoPorEstado => $"{ApiDeRegistroDtm.EsquemaTabla(typeof(PermisosPorEstadoDtm))}";

        public static void PermisoPorEstado(ModelBuilder modelBuilder)
        {
            ApiDeRegistroDtm.DefinirCampoIdDtm<PermisosPorEstadoDtm>(modelBuilder);
            modelBuilder.Entity<PermisosPorEstadoDtm>().Property(p => p.IdNegocio).HasColumnName(ICampos.ID_NEGOCIO).HasColumnType(IDominio.INT).IsRequired();
            modelBuilder.Entity<PermisosPorEstadoDtm>().Property(p => p.IdEstado).HasColumnName(ICampos.ID_ESTADO).HasColumnType(IDominio.INT).IsRequired();
            modelBuilder.Entity<PermisosPorEstadoDtm>().Property(p => p.IdPermiso).HasColumnName(ICampos.ID_PERMISO).HasColumnType(IDominio.INT).IsRequired();
            modelBuilder.Entity<PermisosPorEstadoDtm>().Property(p => p.IdUsuario).HasColumnName(ICampos.ID_USUARIO).HasColumnType(IDominio.INT).IsRequired();
            modelBuilder.Entity<PermisosPorEstadoDtm>().Property(p => p.Calculado).HasColumnName(ICampos.CALCULADO).HasColumnType(IDominio.BIT).IsRequired();

            modelBuilder.Entity<PermisosPorEstadoDtm>()
                        .HasAlternateKey(x => new { x.IdNegocio, x.IdEstado, x.IdUsuario, x.IdPermiso })
                        .HasName($"AK_{ApiDeRegistroDtm.NombreDeTabla(typeof(PermisosPorEstadoDtm))}");


            ApiDeRegistroDtm.DefinirCampoFk<PermisosPorEstadoDtm>(modelBuilder
                , nameof(PermisosPorEstadoDtm.Negocio)
                , nameof(PermisosPorEstadoDtm.IdNegocio)
                , ICampos.ID_NEGOCIO
                , requerida: true
                , unico: false);

            ApiDeRegistroDtm.DefinirCampoFk<PermisosPorEstadoDtm>(modelBuilder
                , nameof(PermisosPorEstadoDtm.Usuario)
                , nameof(PermisosPorEstadoDtm.IdUsuario)
                , ICampos.ID_USUARIO
                , requerida: true
                , unico: false);

            ApiDeRegistroDtm.DefinirCampoFk<PermisosPorEstadoDtm>(modelBuilder
                , nameof(PermisosPorEstadoDtm.Permiso)
                , nameof(PermisosPorEstadoDtm.IdPermiso)
                , ICampos.ID_PERMISO
                , requerida: true
                , unico: false);


            modelBuilder.Entity<PermisosPorEstadoDtm>()
              .HasIndex(x => new { x.IdNegocio, x.IdEstado })
              .IsUnique(false)
              .HasDatabaseName($"I_{ApiDeRegistroDtm.NombreDeTabla(typeof(PermisosPorEstadoDtm))}_{ICampos.ID_NEGOCIO}_{ICampos.ID_ESTADO}");

            modelBuilder.Entity<PermisosPorEstadoDtm>()
              .HasIndex(x => new { x.IdNegocio, x.IdUsuario, x.IdPermiso })
              .IsUnique(false)
              .HasDatabaseName($"I_{ApiDeRegistroDtm.NombreDeTabla(typeof(PermisosPorEstadoDtm))}_{ICampos.ID_NEGOCIO}_{ICampos.ID_USUARIO}_{ICampos.ID_PERMISO}");
        }
    }
}
