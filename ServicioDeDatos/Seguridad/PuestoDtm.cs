using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Terceros;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServicioDeDatos.Seguridad
{
    public static class ltrDePuestoTrabajo
    {
        public static readonly string CopiarRoles = nameof(CopiarRoles);
        public static readonly string CopiarPermisosDirectos = nameof(CopiarPermisosDirectos);
        public static readonly string PtDeAdministrador = "ADM: Puesto de permisos de administradores";
    }


    [Table(Tablas.PUESTO, Schema = Esquemas.SEGURIDAD)]
    public class PuestoDtm : RegistroConNombreDtm, IUsaDescripcion, IUsaCg, INecesitaSerParametrizador
    {
        public int IdCg { get; set; }
        public string Descripcion { get; set; }
        public string RolesDeUnPuesto { get; }
        public virtual ICollection<RolesDeUnPuestoDtm> Roles { get; set; }
        public virtual ICollection<PuestosDeUnUsuarioDtm> Usuarios { get; set; }
        public virtual ICollection<PermisosHeredadosDtm> Permisos { get; set; }
        public CentroGestorDtm Cg { get; set; }

        public new string Expresion { get { return Cg != null ? $"{Cg.Expresion}: {Nombre}" : Nombre; } }
    }

    public static partial class ModeloDeSeguridad
    {
        internal static string TablaPts => $"{ApiDeRegistroDtm.EsquemaDeTabla(typeof(PuestoDtm))}.{ApiDeRegistroDtm.NombreDeTabla(typeof(PuestoDtm))}";

        public static void PuestoDeTrabajo(ModelBuilder modelBuilder)
        {
            ApiDeNombreDtm.DefinirCampoNombreDtm<PuestoDtm>(modelBuilder);
            ApiDeNombreDtm.DefinirCampoDescripcion<PuestoDtm>(modelBuilder);
            ApiDeElementoDtm.DefinirCampoCg<PuestoDtm>(modelBuilder, nameof(PuestoDtm.Cg));
            modelBuilder.Entity<PuestoDtm>().Property(p => p.RolesDeUnPuesto).HasColumnName(ICampos.ROLES).HasColumnType(IDominio.VARCHAR_MAX)
                .HasComputedColumnSql($"{Esquemas.SEGURIDAD}.{Funciones.OBTENER_ROLES_DE_UN_PUESTO}({ICampos.ID})");

            modelBuilder.Entity<PuestoDtm>()
                    .HasMany(p => p.Usuarios)
                    .WithOne(p => p.Puesto)
                    .HasForeignKey(p => p.IdUsuario);

            modelBuilder.Entity<PuestoDtm>()
                    .HasMany(p => p.Roles)
                    .WithOne(p => p.Puesto)
                    .HasForeignKey(p => p.IdRol);


            modelBuilder.Entity<PuestoDtm>()
                        .HasIndex(p => new { p.IdCg, p.Nombre })
                        .IsUnique(true)
                        .HasDatabaseName($"I_{ApiDeRegistroDtm.NombreDeTabla(typeof(PuestoDtm))}_{ICampos.ID_CG}_{ICampos.NOMBRE}");


        }
    }
}
