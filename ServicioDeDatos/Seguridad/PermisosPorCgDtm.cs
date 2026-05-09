using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.Terceros;
using Utilidades;

namespace ServicioDeDatos.Seguridad
{
    [Table(Tablas.PERMISO_POR_NEGOCIOS_CG, Schema = Esquemas.SEGURIDAD)]
    public class PermisosPorCgDtm : RegistroDtm, IPermisoOtorgado
    {
        public int IdNegocio { get; set; }
        public int IdCg { get; set; }
        public int IdUsuario { get; set; }
        public int IdPermiso { get; set; }
        public bool Calculado { get; set; }

        public NegocioDtm Negocio { get; }
        public CentroGestorDtm Cg { get; }
        public UsuarioDtm Usuario { get; }
        public PermisoDtm Permiso { get; }
    }

    public class PermisosPorCgSql
    {

        public static bool EstaElPermisoOtorgado(ContextoSe contexto, PermisosPorCgDtm permisoPorCg)
        {
            string _leer = $@"select {ICampos.ID}
            from {ApiDeRegistroDtm.EsquemaTabla(typeof(PermisosPorCgDtm))} 
            where {ICampos.ID_PERMISO} = @{ICampos.ID_PERMISO}
              and {ICampos.ID_USUARIO} = @{ICampos.ID_USUARIO}
            ";
            var cache = ServicioDeCaches.Obtener(CacheDe.Permisos_CgPorPermisos);
            var indice = $"U:{contexto.DatosDeConexion.IdUsuario}P:{permisoPorCg.IdPermiso}";
            if (cache.ContainsKey(indice))
            {
                var parametrosSql = new Dictionary<string, object>
                {
                    { $"@{ICampos.ID_PERMISO}", permisoPorCg.IdPermiso },
                    { $"@{ICampos.ID_USUARIO}", permisoPorCg.IdUsuario }
                };
                var sentencia = new ConsultaSql<RegistroDtm>(contexto, _leer);
                var r = sentencia.LanzarConsulta(new DynamicParameters(parametrosSql));
                cache[indice] = r.Count() > 0;
            }

            return (bool)cache[indice];
        }

        public static bool UsuarioConAlgunPermiso(ContextoSe contexto, List<int> permisos)
        {
            string _leer = $@"
            select top(1) 1
            from {ApiDeRegistroDtm.EsquemaTabla(typeof(PermisosPorCgDtm))} 
            where {ICampos.ID_USUARIO} = @{ICampos.ID_USUARIO}
              and {ICampos.ID_PERMISO} in ({permisos.ToString(Simbolos.Coma)})
            ";


            var cache = ServicioDeCaches.Obtener(CacheDe.Permisos_CgPorPermisos);
            var indice = $"U:{contexto.DatosDeConexion.IdUsuario}P:{permisos.ToString(Simbolos.Coma)}";
            if (!cache.ContainsKey(indice))
            {
                var parametrosSql = new Dictionary<string, object>  { { $"@{ICampos.ID_USUARIO}", contexto.DatosDeConexion.IdUsuario } };
                var sentencia = new ConsultaSql<PermisosPorCgDtm>(contexto, _leer);
                var r = sentencia.LanzarConsulta(new DynamicParameters(parametrosSql)).Count() == 1;
                cache[indice] = r;
            }

            return (bool)cache[indice];
        }

        public static void Otorgar(ContextoSe contexto, int idNegocio, int idCg, int idUsuario, int idPermiso, bool calculado)
        {
            string _crear = $@"
            insert into {ApiDeRegistroDtm.EsquemaTabla(typeof(PermisosPorCgDtm))} (
                 {ICampos.ID_NEGOCIO}
               , {ICampos.ID_CG}
               , {ICampos.ID_USUARIO}
               , {ICampos.ID_PERMISO}
               , {ICampos.CALCULADO}
            ) 
            select @{ICampos.ID_NEGOCIO}, @{ICampos.ID_CG}, @{ICampos.ID_USUARIO}, @{ICampos.ID_PERMISO}, @{ICampos.CALCULADO}
            where Not Exists (select 1 
                              from {ApiDeRegistroDtm.EsquemaTabla(typeof(PermisosPorCgDtm))} 
                              where {ICampos.ID_NEGOCIO} = @{ICampos.ID_NEGOCIO}
                                and {ICampos.ID_CG} = @{ICampos.ID_CG}
                                and {ICampos.ID_USUARIO} = @{ICampos.ID_USUARIO}
                                and {ICampos.ID_PERMISO} = @{ICampos.ID_PERMISO}
            )
            ";

            var parametrosSql = new Dictionary<string, object>
            {
                { $"@{ICampos.ID_NEGOCIO}", idNegocio },
                { $"@{ICampos.ID_CG}", idCg },
                { $"@{ICampos.ID_USUARIO}", idUsuario },
                { $"@{ICampos.ID_PERMISO}", idPermiso },
                { $"@{ICampos.CALCULADO}", calculado ? 1 : 0 }
            };

            var sentencia = new ConsultaSql<PermisosPorCgDtm>(contexto, _crear);
            sentencia.EjecutarSentencia(new DynamicParameters(parametrosSql));

            var dependientes = CentroGestorSql.CentrosGestoresDependientes(contexto, idCg);
            foreach (var hijo in dependientes)
            {
                parametrosSql[$"@{ICampos.ID_CG}"] = hijo.Id;
                sentencia.EjecutarSentencia(new DynamicParameters(parametrosSql));
            }
        }

        public static void QuitarPermisos(ContextoSe contexto, int idUsuario, bool? calculado)
        {
            var esCalculado = calculado == null ? "" : $" and {ICampos.CALCULADO} = {((bool)calculado ? 1 : 0)}";
            string _quitar = $@"
            delete 
            from {ApiDeRegistroDtm.EsquemaTabla(typeof(PermisosPorCgDtm))}
            where  {ICampos.ID_USUARIO} = @{ICampos.ID_USUARIO}{esCalculado}";
            var parametrosSql = new Dictionary<string, object>
            {
                { $"@{ICampos.ID_USUARIO}", idUsuario }
            };

            var sentencia = new ConsultaSql<PermisosPorCgDtm>(contexto, _quitar);
            sentencia.EjecutarSentencia(new DynamicParameters(parametrosSql));
            EliminarCachesDePermisosPorCg();
        }

        public static int EliminarTodos(ContextoSe contexto)
        {
            string _quitar = $@"
            delete 
            from {ApiDeRegistroDtm.EsquemaTabla(typeof(PermisosPorCgDtm))}
            where {ICampos.CALCULADO} = 1 
            ";
            var parametrosSql = new Dictionary<string, object>();
            var sentencia = new ConsultaSql<PermisosPorCgDtm>(contexto, _quitar);
            var resultado = sentencia.EjecutarSentencia(new DynamicParameters(parametrosSql));
            EliminarCachesDePermisosPorCg();
            return resultado;
        }

        public static void EliminarCachesDePermisosPorCg()
        {
            ServicioDeCaches.EliminarCache(CacheDe.Permisos_PorCg);
            ServicioDeCaches.EliminarCache(CacheDe.Permisos_CgsConGestion);
            ServicioDeCaches.EliminarCache(CacheDe.Permisos_CgPorNegocio);
            ServicioDeCaches.EliminarCache(CacheDe.Permisos_CgPorPermisos);
        }
    }

    public static partial class ModeloDeSeguridad
    {
        internal static string TablaDePermisoPorCg => $"{ApiDeRegistroDtm.EsquemaTabla(typeof(PermisosPorCgDtm))}";

        public static void PermisoPorCg(ModelBuilder modelBuilder)
        {
            ApiDeRegistroDtm.DefinirCampoIdDtm<PermisosPorCgDtm>(modelBuilder);
            modelBuilder.Entity<PermisosPorCgDtm>().Property(p => p.IdNegocio).HasColumnName(ICampos.ID_NEGOCIO).HasColumnType(IDominio.INT).IsRequired();
            modelBuilder.Entity<PermisosPorCgDtm>().Property(p => p.IdCg).HasColumnName(ICampos.ID_CG).HasColumnType(IDominio.INT).IsRequired();
            modelBuilder.Entity<PermisosPorCgDtm>().Property(p => p.IdPermiso).HasColumnName(ICampos.ID_PERMISO).HasColumnType(IDominio.INT).IsRequired();
            modelBuilder.Entity<PermisosPorCgDtm>().Property(p => p.IdUsuario).HasColumnName(ICampos.ID_USUARIO).HasColumnType(IDominio.INT).IsRequired();
            modelBuilder.Entity<PermisosPorCgDtm>().Property(p => p.Calculado).HasColumnName(ICampos.CALCULADO).HasColumnType(IDominio.BIT).IsRequired();

            modelBuilder.Entity<PermisosPorCgDtm>()
                        .HasAlternateKey(x => new { x.IdNegocio, x.IdCg, x.IdUsuario, x.IdPermiso })
                        .HasName($"AK_{ApiDeRegistroDtm.NombreDeTabla(typeof(PermisosPorCgDtm))}");


            ApiDeRegistroDtm.DefinirCampoFk<PermisosPorCgDtm>(modelBuilder
                , nameof(PermisosPorCgDtm.Negocio)
                , nameof(PermisosPorCgDtm.IdNegocio)
                , ICampos.ID_NEGOCIO
                , requerida: true
                , unico: false);

            ApiDeRegistroDtm.DefinirCampoFk<PermisosPorCgDtm>(modelBuilder
                , nameof(PermisosPorCgDtm.Cg)
                , nameof(PermisosPorCgDtm.IdCg)
                , ICampos.ID_CG
                , requerida: true
                , unico: false);

            ApiDeRegistroDtm.DefinirCampoFk<PermisosPorCgDtm>(modelBuilder
                , nameof(PermisosPorCgDtm.Usuario)
                , nameof(PermisosPorCgDtm.IdUsuario)
                , ICampos.ID_USUARIO
                , requerida: true
                , unico: false);

            ApiDeRegistroDtm.DefinirCampoFk<PermisosPorCgDtm>(modelBuilder
                , nameof(PermisosPorCgDtm.Permiso)
                , nameof(PermisosPorCgDtm.IdPermiso)
                , ICampos.ID_PERMISO
                , requerida: true
                , unico: false);


            modelBuilder.Entity<PermisosPorCgDtm>()
              .HasIndex(x => new { x.IdNegocio, x.IdCg })
              .IsUnique(false)
              .HasDatabaseName($"I_{ApiDeRegistroDtm.NombreDeTabla(typeof(PermisosPorCgDtm))}_{ICampos.ID_NEGOCIO}_{ICampos.ID_CG}");

            modelBuilder.Entity<PermisosPorCgDtm>()
              .HasIndex(x => new { x.IdNegocio, x.IdUsuario, x.IdPermiso })
              .IsUnique(false)
              .HasDatabaseName($"I_{ApiDeRegistroDtm.NombreDeTabla(typeof(PermisosPorCgDtm))}_{ICampos.ID_NEGOCIO}_{ICampos.ID_USUARIO}_{ICampos.ID_PERMISO}");
        }
    }
}
