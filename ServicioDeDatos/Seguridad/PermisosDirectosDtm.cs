using Dapper;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServicioDeDatos.Seguridad
{
    [Table(Tablas.PERMISO_DIRECTOS, Schema = Esquemas.SEGURIDAD)]
    public class PermisosDirectosDtm : RelacionDtm, INecesitaSerParametrizador
    {
        public int IdPuesto { get; set; }

        public int IdPermiso { get; set; }

        public PuestoDtm Puesto { get; set; }
        public PermisoDtm Permiso { get; set; }

        public PermisosDirectosDtm()
        {
            PropiedadDelIdElemento1 = nameof(IdPuesto);
            PropiedadDelIdElemento2 = nameof(IdPermiso);
        }

    }

    public static class PermisosDirectosSql
    {
        public static void Copiar(ContextoSe contexto, int idPtOrigen, int idPtDestino)
        {
            string _copiar = $@"
            insert into {ApiDeRegistroDtm.EsquemaTabla(typeof(PermisosDirectosDtm))} (
                {ICampos.IDPUESTO}
              , {ICampos.IDPERMISO}
            )
            select {idPtDestino}, t1.{ICampos.IDPERMISO}
            from {ApiDeRegistroDtm.EsquemaTabla(typeof(PermisosDirectosDtm))} t1
            where {ICampos.IDPUESTO} = @{ICampos.IDPUESTO}
              and Not Exists (select 1 
                  from {ApiDeRegistroDtm.EsquemaTabla(typeof(PermisosDirectosDtm))} 
                  where {ICampos.IDPUESTO} = {idPtDestino}
                    and {ICampos.IDPERMISO} = t1.{ICampos.IDPERMISO}
            )
            ";

            var parametrosSql = new Dictionary<string, object>
            {
                { $"@{ICampos.IDPUESTO}", idPtOrigen }
            };
            var sentencia = new ConsultaSql<PermisosPorTipoDtm>(contexto, _copiar);
            sentencia.EjecutarSentencia(new DynamicParameters(parametrosSql));
        }
    }

    public static partial class ModeloDeSeguridad
    {
        internal static string TablaPermisosDeUnPuesto => $"{ApiDeRegistroDtm.EsquemaTabla(typeof(PermisosDirectosDtm))}";

        public static void PermisosDeUnPuesto(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PermisosDirectosDtm>()
            .HasAlternateKey(p => new { p.IdPuesto, p.IdPermiso })
            .HasName($"AK_{ApiDeRegistroDtm.NombreDeTabla(typeof(PermisosDirectosDtm))}");

            modelBuilder.Entity<PermisosDirectosDtm>().Property(nameof(PermisosDirectosDtm.IdPuesto)).HasColumnName(ICampos.IDPUESTO).HasColumnType(IDominio.INT).IsRequired(true);
            ApiDeRegistroDtm.DefinirFk<PermisosDirectosDtm>(modelBuilder, nameof(PermisosDirectosDtm.Puesto), nameof(PermisosDirectosDtm.IdPuesto), ICampos.IDPUESTO, false);

            modelBuilder.Entity<PermisosDirectosDtm>().Property(nameof(PermisosDirectosDtm.IdPermiso)).HasColumnName(ICampos.IDPERMISO).HasColumnType(IDominio.INT).IsRequired(true);
            ApiDeRegistroDtm.DefinirFk<PermisosDirectosDtm>(modelBuilder, nameof(PermisosDirectosDtm.Permiso), nameof(PermisosDirectosDtm.IdPermiso), ICampos.IDPERMISO, false);
        }
    }
}
