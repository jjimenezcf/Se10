using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Dapper;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;

namespace ServicioDeDatos.Seguridad
{

    [Table(Tablas.ROL_PUESTO, Schema = Esquemas.SEGURIDAD)]
    public class RolesDeUnPuestoDtm: RelacionDtm, INecesitaSerParametrizador
    {
        [Column(ICampos.IDROL, TypeName = IDominio.INT)]
        public int IdRol { get; set; }

        [Column(ICampos.IDPUESTO, TypeName = IDominio.INT)]
        public int IdPuesto { get; set; }

        public RolDtm Rol { get; set; }
        public PuestoDtm Puesto { get; set; }

        public RolesDeUnPuestoDtm()
        {
            PropiedadDelIdElemento1 = nameof(IdPuesto);
            PropiedadDelIdElemento2 = nameof(IdRol);
        }
    }

    public static class RolesDeUnPuestoSql
    {
        public static void Copiar(ContextoSe contexto, int idPtOrigen, int idPtDestino)
        {
            string _copiar = $@"
            insert into {ApiDeRegistroDtm.EsquemaTabla(typeof(RolesDeUnPuestoDtm))} (
                {ICampos.IDPUESTO}
              , {ICampos.IDROL}
            )
            select {idPtDestino}, t1.{ICampos.IDROL}
            from {ApiDeRegistroDtm.EsquemaTabla(typeof(RolesDeUnPuestoDtm))} t1
            where {ICampos.IDPUESTO} = @{ICampos.IDPUESTO}
              and Not Exists (select 1 
                  from {ApiDeRegistroDtm.EsquemaTabla(typeof(RolesDeUnPuestoDtm))} 
                  where {ICampos.IDPUESTO} = {idPtDestino}
                    and {ICampos.IDROL} = t1.{ICampos.IDROL}
            )
            ";

            var parametrosSql = new Dictionary<string, object>();
            parametrosSql.Add($"@{ICampos.IDPUESTO}", idPtOrigen);
            var sentencia = new ConsultaSql<PermisosPorTipoDtm>(contexto, _copiar);
            sentencia.EjecutarSentencia(new DynamicParameters(parametrosSql));
        }
    }

    public static partial class ModeloDeSeguridad
    {
        internal static string TablaRolesDeUnPt => $"{ApiDeRegistroDtm.EsquemaTabla(typeof(RolesDeUnPuestoDtm))}";

        public static void RolesDeUnPt(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RolesDeUnPuestoDtm>()
                .HasAlternateKey(p => new { p.IdRol, p.IdPuesto })
                .HasName($"AK_{ApiDeRegistroDtm.NombreDeTabla(typeof(RolesDeUnPuestoDtm))}");

            modelBuilder.Entity<RolesDeUnPuestoDtm>()
                .HasOne(x => x.Rol)
                .WithMany(r => r.Puestos)
                .HasForeignKey(x => x.IdRol)
                .HasConstraintName($"FK_{ApiDeRegistroDtm.NombreDeTabla(typeof(RolesDeUnPuestoDtm))}_{ICampos.IDROL}")
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RolesDeUnPuestoDtm>()
                .HasOne(x => x.Puesto)
                .WithMany(p => p.Roles)
                .HasForeignKey(x => x.IdPuesto)
                .HasConstraintName($"FK_{ApiDeRegistroDtm.NombreDeTabla(typeof(RolesDeUnPuestoDtm))}_{ICampos.IDPUESTO}")
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
