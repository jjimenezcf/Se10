using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Negocio;

namespace ServicioDeDatos.Seguridad
{
    [Table(Tablas.PERMISO_POR_TRANSICION, Schema = Esquemas.SEGURIDAD)]
    public class PermisosPorTransicionDtm : RegistroDtm, IPermisoOtorgado
    {
        public int IdNegocio { get; set; }
        public int IdTransicion { get; set; }
        public int IdUsuario { get; set; }
        public int IdPermiso { get; set; }
        public bool Calculado { get; set; }

        public NegocioDtm Negocio { get; }
        public UsuarioDtm Usuario { get; }
        public PermisoDtm Permiso { get; }

    }

    public static partial class ModeloDeSeguridad
    {
        internal static string TablaDePermisoPorTransicion => $"{ApiDeRegistroDtm.EsquemaTabla(typeof(PermisosPorTransicionDtm))}";

        public static void PermisoPorTransicion(ModelBuilder modelBuilder)
        {
            ApiDeRegistroDtm.DefinirCampoIdDtm<PermisosPorTransicionDtm>(modelBuilder);
            modelBuilder.Entity<PermisosPorTransicionDtm>().Property(p => p.IdNegocio).HasColumnName(ICampos.ID_NEGOCIO).HasColumnType(IDominio.INT).IsRequired();
            modelBuilder.Entity<PermisosPorTransicionDtm>().Property(p => p.IdTransicion).HasColumnName(ICampos.ID_TRANSICION).HasColumnType(IDominio.INT).IsRequired();
            modelBuilder.Entity<PermisosPorTransicionDtm>().Property(p => p.IdPermiso).HasColumnName(ICampos.ID_PERMISO).HasColumnType(IDominio.INT).IsRequired();
            modelBuilder.Entity<PermisosPorTransicionDtm>().Property(p => p.IdUsuario).HasColumnName(ICampos.ID_USUARIO).HasColumnType(IDominio.INT).IsRequired();
            modelBuilder.Entity<PermisosPorTransicionDtm>().Property(p => p.Calculado).HasColumnName(ICampos.CALCULADO).HasColumnType(IDominio.BIT).IsRequired();

            modelBuilder.Entity<PermisosPorTransicionDtm>()
                        .HasAlternateKey(x => new { x.IdNegocio, x.IdTransicion, x.IdUsuario, x.IdPermiso })
                        .HasName($"AK_{ApiDeRegistroDtm.NombreDeTabla(typeof(PermisosPorTransicionDtm))}");


            ApiDeRegistroDtm.DefinirCampoFk<PermisosPorTransicionDtm>(modelBuilder
                , nameof(PermisosPorTransicionDtm.Negocio)
                , nameof(PermisosPorTransicionDtm.IdNegocio)
                , ICampos.ID_NEGOCIO
                , requerida: true
                , unico: false);

            ApiDeRegistroDtm.DefinirCampoFk<PermisosPorTransicionDtm>(modelBuilder
                , nameof(PermisosPorTransicionDtm.Usuario)
                , nameof(PermisosPorTransicionDtm.IdUsuario)
                , ICampos.ID_USUARIO
                , requerida: true
                , unico: false);

            ApiDeRegistroDtm.DefinirCampoFk<PermisosPorTransicionDtm>(modelBuilder
                , nameof(PermisosPorTransicionDtm.Permiso)
                , nameof(PermisosPorTransicionDtm.IdPermiso)
                , ICampos.ID_PERMISO
                , requerida: true
                , unico: false);


            modelBuilder.Entity<PermisosPorTransicionDtm>()
              .HasIndex(x => new { x.IdNegocio, x.IdTransicion })
              .IsUnique(false)
              .HasDatabaseName($"I_{ApiDeRegistroDtm.NombreDeTabla(typeof(PermisosPorTransicionDtm))}_{ICampos.ID_NEGOCIO}_{ICampos.ID_TRANSICION}");

            modelBuilder.Entity<PermisosPorTransicionDtm>()
              .HasIndex(x => new { x.IdNegocio, x.IdUsuario, x.IdPermiso })
              .IsUnique(false)
              .HasDatabaseName($"I_{ApiDeRegistroDtm.NombreDeTabla(typeof(PermisosPorTransicionDtm))}_{ICampos.ID_NEGOCIO}_{ICampos.ID_USUARIO}_{ICampos.ID_PERMISO}");
        }
    }
}
