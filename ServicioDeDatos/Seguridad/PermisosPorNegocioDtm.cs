using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Dapper;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Negocio;
using Utilidades;

namespace ServicioDeDatos.Seguridad
{
    [Table(Tablas.PERMISO_POR_NEGOCIO, Schema = Esquemas.SEGURIDAD)]
    public class PermisosPorNegocioDtm : RegistroDtm, IPermisoOtorgado
    {
        public int IdNegocio { get; set; }
        public int IdUsuario { get; set; }
        public int IdPermiso { get; set; }
        public bool Calculado { get; set; }

        public NegocioDtm Negocio { get; }
        public UsuarioDtm Usuario { get; }
        public PermisoDtm Permiso { get; }

    }

    public class PermisosPorNegocioSql
    {
        private static readonly string _LeerTabla = $@"
            select {ICampos.ID}
              , {ICampos.ID_NEGOCIO} as {nameof(PermisosPorNegocioDtm.IdNegocio)}
              , {ICampos.ID_USUARIO} as {nameof(PermisosPorNegocioDtm.IdUsuario)}
              , {ICampos.ID_PERMISO}  as {nameof(PermisosPorNegocioDtm.IdPermiso)}
              , {ICampos.CALCULADO}  as {nameof(PermisosPorNegocioDtm.Calculado)}
            from {ApiDeRegistroDtm.EsquemaTabla(typeof(PermisosPorNegocioDtm))} ";


        public static bool UsuarioConAlgunPermiso(ContextoSe contexto, List<int> permisos)
        {
            string _leer = $@"
            select top(1) 1
            from {ApiDeRegistroDtm.EsquemaTabla(typeof(PermisosPorNegocioDtm))} 
            where {ICampos.ID_USUARIO} = @{ICampos.ID_USUARIO}
              and {ICampos.ID_PERMISO} in ({permisos.ToString(Simbolos.Coma)})
            ";
            var parametrosSql = new Dictionary<string, object>
            {
                { $"@{ICampos.ID_USUARIO}", contexto.DatosDeConexion.IdUsuario }
            };
            var sentencia = new ConsultaSql<PermisosPorNegocioDtm>(contexto, _leer);
            return sentencia.LanzarConsulta(new DynamicParameters(parametrosSql)).Count() == 1;
        }


        public static List<PermisosPorNegocioDtm> PermisosDeUsuario(ContextoSe contexto, int idNegocio)
        {
            string _leer = $@"{_LeerTabla}
            where {ICampos.ID_NEGOCIO} = @{ICampos.ID_NEGOCIO}
              and {ICampos.ID_USUARIO} = @{ICampos.ID_USUARIO}
            ";

            var cache = ServicioDeCaches.Obtener(CacheDe.Permisos_PorNegocio);
            var indice = $"{idNegocio}.{contexto.DatosDeConexion.IdUsuario}";
            if (!cache.ContainsKey(indice))
            {
                var parametrosSql = new Dictionary<string, object>
                {
                    { $"@{ICampos.ID_NEGOCIO}", idNegocio },
                    { $"@{ICampos.ID_USUARIO}", contexto.DatosDeConexion.IdUsuario }
                };

                var sentencia = new ConsultaSql<PermisosPorNegocioDtm>(contexto, _leer);
                cache[indice] = sentencia.LanzarConsulta(new DynamicParameters(parametrosSql));
            }
            return (List<PermisosPorNegocioDtm>)cache[indice];
        }

        public static int Otorgar(ContextoSe contexto, int idNegocio, int idUsuario, int idPermiso, bool calculado)
        {
            string _crear = $@"
            insert into {ApiDeRegistroDtm.EsquemaTabla(typeof(PermisosPorNegocioDtm))} (
                 {ICampos.ID_NEGOCIO}
               , {ICampos.ID_USUARIO}
               , {ICampos.ID_PERMISO}
               , {ICampos.CALCULADO}) 
            select @{ICampos.ID_NEGOCIO}, @{ICampos.ID_USUARIO}, @{ICampos.ID_PERMISO},{(calculado ? 1 : 0)}
            where Not Exists (select 1 
                              from {ApiDeRegistroDtm.EsquemaTabla(typeof(PermisosPorNegocioDtm))} 
                              where {ICampos.ID_NEGOCIO} = @{ICampos.ID_NEGOCIO}
                                and {ICampos.ID_USUARIO} = @{ICampos.ID_USUARIO}
                                and {ICampos.ID_PERMISO} = @{ICampos.ID_PERMISO}
            )
            ";

            var parametrosSql = new Dictionary<string, object>
            {
                { $"@{ICampos.ID_NEGOCIO}", idNegocio },
                { $"@{ICampos.ID_USUARIO}", idUsuario },
                { $"@{ICampos.ID_PERMISO}", idPermiso }
            };

            var sentencia = new ConsultaSql<PermisosPorNegocioDtm>(contexto, _crear);
            return sentencia.EjecutarSentencia(new DynamicParameters(parametrosSql));
        }

        public static bool EstaElPermisoOtorgado(ContextoSe contexto, PermisosPorNegocioDtm registro)
        {
            string _leer = $@"select {ICampos.ID}
            from {ApiDeRegistroDtm.EsquemaTabla(typeof(PermisosPorNegocioDtm))} 
            where {ICampos.ID_PERMISO} = @{ICampos.ID_PERMISO}
              and {ICampos.ID_USUARIO} = @{ICampos.ID_USUARIO}
            ";

            var parametrosSql = new Dictionary<string, object>
            {
                { $"@{ICampos.ID_PERMISO}", registro.IdPermiso },
                { $"@{ICampos.ID_USUARIO}", registro.IdUsuario }
            };

            var sentencia = new ConsultaSql<RegistroDtm>(contexto, _leer);
            var r = sentencia.LanzarConsulta(new DynamicParameters(parametrosSql));

            return r.Count() > 0;
        }

        public static void QuitarPermisos(ContextoSe contexto, int idUsuario, bool? calculado)
        {
            var esCalculado = calculado == null ? "" : $" and {ICampos.CALCULADO} = {((bool)calculado ? 1 : 0)}";

            string _quitar = $@"
            delete 
            from {ApiDeRegistroDtm.EsquemaTabla(typeof(PermisosPorNegocioDtm))}
            where  {ICampos.ID_USUARIO} = @{ICampos.ID_USUARIO}{esCalculado}";
            var parametrosSql = new Dictionary<string, object>
            {
                { $"@{ICampos.ID_USUARIO}", idUsuario }
            };

            var sentencia = new ConsultaSql<PermisosPorNegocioDtm>(contexto, _quitar);
            sentencia.EjecutarSentencia(new DynamicParameters(parametrosSql));
            ServicioDeCaches.EliminarCache(CacheDe.Permisos_PorNegocio);
        }
        public static int EliminarTodos(ContextoSe contexto)
        {
            string _quitar = $@"
            delete 
            from {ApiDeRegistroDtm.EsquemaTabla(typeof(PermisosPorNegocioDtm))}
            where {ICampos.CALCULADO} = 1
            ";
            var parametrosSql = new Dictionary<string, object>();
            var sentencia = new ConsultaSql<PermisosPorNegocioDtm>(contexto, _quitar);
            ServicioDeCaches.EliminarCache(CacheDe.Permisos_PorNegocio);
            return sentencia.EjecutarSentencia(new DynamicParameters(parametrosSql));
        }
    }

    public static partial class ModeloDeSeguridad
    {
        internal static string TablaDePermisoPorNegocio => $"{ApiDeRegistroDtm.EsquemaTabla(typeof(PermisosPorNegocioDtm))}";

        public static void PermisoPorNegocio(ModelBuilder modelBuilder)
        {
            ApiDeRegistroDtm.DefinirCampoIdDtm<PermisosPorNegocioDtm>(modelBuilder);
            modelBuilder.Entity<PermisosPorNegocioDtm>().Property(p => p.IdNegocio).HasColumnName(ICampos.ID_NEGOCIO).HasColumnType("INT").IsRequired();
            modelBuilder.Entity<PermisosPorNegocioDtm>().Property(p => p.IdPermiso).HasColumnName(ICampos.ID_PERMISO).HasColumnType("INT").IsRequired();
            modelBuilder.Entity<PermisosPorNegocioDtm>().Property(p => p.IdUsuario).HasColumnName(ICampos.ID_USUARIO).HasColumnType("INT").IsRequired();
            modelBuilder.Entity<PermisosPorNegocioDtm>().Property(p => p.Calculado).HasColumnName(ICampos.CALCULADO).HasColumnType(IDominio.BIT).IsRequired();

            modelBuilder.Entity<PermisosPorNegocioDtm>()
                        .HasAlternateKey(x => new { x.IdNegocio, x.IdUsuario, x.IdPermiso })
                        .HasName($"AK_{ApiDeRegistroDtm.NombreDeTabla(typeof(PermisosPorNegocioDtm))}");


            ApiDeRegistroDtm.DefinirCampoFk<PermisosPorNegocioDtm>(modelBuilder
                , nameof(PermisosPorNegocioDtm.Negocio)
                , nameof(PermisosPorNegocioDtm.IdNegocio)
                , ICampos.ID_NEGOCIO
                , requerida: true
                , unico: false);

            ApiDeRegistroDtm.DefinirCampoFk<PermisosPorNegocioDtm>(modelBuilder
                , nameof(PermisosPorNegocioDtm.Usuario)
                , nameof(PermisosPorNegocioDtm.IdUsuario)
                , ICampos.ID_USUARIO
                , requerida: true
                , unico: false);

            ApiDeRegistroDtm.DefinirCampoFk<PermisosPorNegocioDtm>(modelBuilder
                , nameof(PermisosPorNegocioDtm.Permiso)
                , nameof(PermisosPorNegocioDtm.IdPermiso)
                , ICampos.ID_PERMISO
                , requerida: true
                , unico: false);

            modelBuilder.Entity<PermisosPorNegocioDtm>()
              .HasIndex(x => new { x.IdNegocio, x.IdUsuario })
              .IsUnique(false)
              .HasDatabaseName($"I_{ApiDeRegistroDtm.NombreDeTabla(typeof(PermisosPorNegocioDtm))}_{ICampos.ID_NEGOCIO}_{ICampos.ID_USUARIO}");
        }
    }
}
