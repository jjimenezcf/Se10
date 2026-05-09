using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServicioDeDatos.Seguridad
{

    public interface ITienePermisoDeConsultor
    {
        public int IdConsultor { get; set; }
        public abstract PermisoDtm Consultor { get; }
    }
    public interface ITienePermisoDeGestor
    {
        public int IdGestor { get; set; }
        public abstract PermisoDtm Gestor { get; }
    }
    public interface ITienePermisoDeAdm
    {
        public int IdAdministrador { get; set; }
        public abstract PermisoDtm Administrador { get; }
    }

    public static class ltrDeUnPermisoDtm
    {
        public static string PermisosDeElemento = nameof(PermisosDeElemento);
        public static string PermisosDeNegocio = nameof(PermisosDeNegocio);
        public static string PermisosDeTipo = nameof(PermisosDeTipo);
        public static string PermisosDeEstado = nameof(PermisosDeEstado);
        public static string PermisosDeTransicion = nameof(PermisosDeTransicion);
        public static string PermisosDeCg = nameof(PermisosDeCg);
    }

    [Table(Tablas.PERMISO, Schema = Esquemas.SEGURIDAD)]
    public class PermisoDtm : RegistroConNombreDtm
    {
        public int IdClase { get; set; }
        public virtual ClasePermisoDtm Clase { get; set; }
        
        public int IdTipo { get; set; }

        public virtual TipoPermisoDtm Tipo { get; set; }

        public ICollection<PermisosDeUnRolDtm> Roles { get; set; }
        public ICollection<UsuariosDeUnPermisoDtm> Usuarios { get; set; }
        public ICollection<PermisosHeredadosDtm> Puestos { get; set; }
    }

    public static partial class ModeloDeSeguridad
    {
        internal static string TablaDePermiso => $"{ApiDeRegistroDtm.EsquemaTabla(typeof(PermisoDtm))}"; 

        public static void Permiso(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PermisoDtm>().Property(p => p.Nombre).HasColumnName(ICampos.NOMBRE).HasColumnType(IDominio.VARCHAR_250).IsRequired();
            modelBuilder.Entity<PermisoDtm>()
                        .HasIndex(p => p.Nombre)
                        .HasDatabaseName($"I_{Tablas.PERMISO}_{ICampos.NOMBRE}")
                        .IsUnique();

            modelBuilder.Entity<PermisoDtm>().Property(p => p.IdTipo).IsRequired();
            modelBuilder.Entity<PermisoDtm>().Property(p => p.IdClase).IsRequired();

            modelBuilder.Entity<PermisoDtm>().Property(p => p.IdTipo).HasColumnName(ICampos.IDTIPO);
            modelBuilder.Entity<PermisoDtm>().Property(p => p.IdClase).HasColumnName(ICampos.IDCLASE);

            modelBuilder.Entity<PermisoDtm>()
                        .HasIndex(p => new {p.IdClase, p.IdTipo})
                        .HasDatabaseName($"I_{Tablas.PERMISO}_{ICampos.IDCLASE}_{ICampos.IDTIPO}");

            modelBuilder.Entity<PermisoDtm>()
                        .HasOne(p => p.Clase)
                        .WithMany(cp => cp.Permisos)
                        .HasForeignKey(p => p.IdClase)
                        .HasConstraintName($"FK_{Tablas.PERMISO}_{ICampos.IDCLASE}")
                        .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PermisoDtm>()
                        .HasOne(p => p.Tipo)
                        .WithMany(tp => tp.Permisos)
                        .HasForeignKey(p => p.IdTipo)
                        .HasConstraintName($"FK_{Tablas.PERMISO}_{ICampos.IDTIPO}")
                        .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PermisoDtm>()
                .HasMany(p => p.Roles)
                .WithOne(p => p.Permiso)
                .HasForeignKey(p => p.IdRol);

            modelBuilder.Entity<PermisoDtm>()
                .HasMany(p => p.Usuarios)
                .WithOne(p => p.Permiso)
                .HasForeignKey(p => p.IdPermiso);
        }
    }
}
