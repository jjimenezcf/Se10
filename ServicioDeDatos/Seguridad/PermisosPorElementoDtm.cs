using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Configuration;
using System.Linq;
using Dapper;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Negocio;
using Utilidades;

namespace ServicioDeDatos.Seguridad
{
    [Table(Tablas.PERMISO_POR_ELEMENTO, Schema = Esquemas.SEGURIDAD)]
    public class PermisosPorElementoDtm : RegistroDtm, IPermisoOtorgado
    {
        public int IdNegocio { get; set; }
        public int IdElemento { get; set; }
        public int IdUsuario { get; set; }
        public int IdPermiso { get; set; }
        public bool Calculado { get; set; }

        public NegocioDtm Negocio { get; }
        public UsuarioDtm Usuario { get; }
        public PermisoDtm Permiso { get; }

    }


    public class PermisosPorElementoSql
    {

        private static readonly string _LeerTabla = $@"
            select {ICampos.ID}
              , {ICampos.ID_NEGOCIO} as {nameof(PermisosPorElementoDtm.IdNegocio)}
              , {ICampos.ID_ELEMENTO} as {nameof(PermisosPorElementoDtm.IdElemento)}
              , {ICampos.ID_USUARIO} as {nameof(PermisosPorElementoDtm.IdUsuario)}
              , {ICampos.ID_PERMISO}  as {nameof(PermisosPorElementoDtm.IdPermiso)}
              , {ICampos.CALCULADO}  as {nameof(PermisosPorElementoDtm.Calculado)}
            from {ApiDeRegistroDtm.EsquemaTabla(typeof(PermisosPorElementoDtm))} ";

        public static void QuitarPermisos(ContextoSe contexto, int idUsuario, bool? calculado)
        {
            var esCalculado = calculado == null ? "" : $" and {ICampos.CALCULADO} = {((bool)calculado ? 1 : 0)}";
            string _quitar = $@"
               delete 
               from {ApiDeRegistroDtm.EsquemaTabla(typeof(PermisosPorElementoDtm))}
               where  {ICampos.ID_USUARIO} = @{ICampos.ID_USUARIO}{esCalculado}";
            var parametrosSql = new Dictionary<string, object>
            {
                { $"@{ICampos.ID_USUARIO}", idUsuario }
            };
            var sentencia = new ConsultaSql<PermisosPorElementoDtm>(contexto, _quitar);
            sentencia.EjecutarSentencia(new DynamicParameters(parametrosSql));

            ServicioDeCaches.EliminarCache(CacheDe.Permisos_PorElemento);
            ServicioDeCaches.EliminarCache(CacheDe.Permisos_AlgunElemento);
            ServicioDeCaches.EliminarCache(CacheDe.Permisos_PermisoOtorgado);            
        }

        public static int EliminarTodos(ContextoSe contexto)
        {
            string _quitar = $@"
               delete 
               from {ApiDeRegistroDtm.EsquemaTabla(typeof(PermisosPorElementoDtm))}
               where {ICampos.CALCULADO} = 1
            ";
            var parametrosSql = new Dictionary<string, object>();
            var sentencia = new ConsultaSql<PermisosPorElementoDtm>(contexto, _quitar);
            ServicioDeCaches.EliminarCache(CacheDe.Permisos_PorElemento);
            ServicioDeCaches.EliminarCache(CacheDe.Permisos_AlgunElemento);
            ServicioDeCaches.EliminarCache(CacheDe.Permisos_PermisoOtorgado);
            return sentencia.EjecutarSentencia(new DynamicParameters(parametrosSql));
        }

        public static bool AlgunElementoConAcceso(ContextoSe contexto, int idNegocio)
        {
            string _leer = $@"
            select top(1) 1
            from {ApiDeRegistroDtm.EsquemaTabla(typeof(PermisosPorElementoDtm))} 
            where {ICampos.ID_NEGOCIO} = @{ICampos.ID_NEGOCIO}
              and {ICampos.ID_USUARIO} = @{ICampos.ID_USUARIO}
            ";

            var cache = ServicioDeCaches.Obtener(CacheDe.Permisos_AlgunElemento);
            var indice = $"{idNegocio}.{contexto.DatosDeConexion.IdUsuario}";
            if (!cache.ContainsKey(indice))
            {
                var parametrosSql = new Dictionary<string, object>
                {
                    { $"@{ICampos.ID_NEGOCIO}", idNegocio },
                    { $"@{ICampos.ID_USUARIO}", contexto.DatosDeConexion.IdUsuario }
                };

                var sentencia = new ConsultaSql<PermisosPorElementoDtm>(contexto, _leer);
                cache[indice] = sentencia.LanzarConsulta(new DynamicParameters(parametrosSql)).Count() > 0;
            }
            return (bool)cache[indice];
        }

        public static List<PermisosPorElementoDtm> PermisosDeUsuario(ContextoSe contexto, int idNegocio, int idElemento)
        {
            string _leer = $@"{_LeerTabla}
            where {ICampos.ID_NEGOCIO} = @{ICampos.ID_NEGOCIO}
              and {ICampos.ID_ELEMENTO} = @{ICampos.ID_ELEMENTO}
              and {ICampos.ID_USUARIO} = @{ICampos.ID_USUARIO}
            ";

            var cache = ServicioDeCaches.Obtener(CacheDe.Permisos_PorElemento);
            var indice = $"{idNegocio}.{idElemento}.{contexto.DatosDeConexion.IdUsuario}";
            if (!cache.ContainsKey(indice))
            {
                var parametrosSql = new Dictionary<string, object>
                {
                    { $"@{ICampos.ID_NEGOCIO}", idNegocio },
                    { $"@{ICampos.ID_ELEMENTO}", idElemento },
                    { $"@{ICampos.ID_USUARIO}", contexto.DatosDeConexion.IdUsuario }
                };

                var sentencia = new ConsultaSql<PermisosPorElementoDtm>(contexto, _leer);
                cache[indice] = sentencia.LanzarConsulta(new DynamicParameters(parametrosSql));
            }
            return (List<PermisosPorElementoDtm>)cache[indice];
        }

        public static int Eliminar(ContextoSe contexto, int idNegocio, int idElemento, int idPermiso, int idUsuario, bool calculado)
        {
            string _quitar = $@"
               delete 
               from {ApiDeRegistroDtm.EsquemaTabla(typeof(PermisosPorElementoDtm))}
               where {ICampos.CALCULADO} = {(calculado ? 1 : 0)}
                 and {ICampos.ID_NEGOCIO} = @{ICampos.ID_NEGOCIO}
                 and {ICampos.ID_ELEMENTO} =  @{ICampos.ID_ELEMENTO} 
                 and {ICampos.ID_USUARIO} =  @{ICampos.ID_USUARIO} 
                 and {ICampos.ID_PERMISO} =  @{ICampos.ID_PERMISO}
            ";
            var parametrosSql = new Dictionary<string, object>
            {
                { $"@{ICampos.ID_NEGOCIO}", idNegocio },
                { $"@{ICampos.ID_ELEMENTO}", idElemento },
                { $"@{ICampos.ID_PERMISO}", idPermiso },
                { $"@{ICampos.ID_USUARIO}", contexto.DatosDeConexion.IdUsuario }
            };

            var sentencia = new ConsultaSql<PermisosPorElementoDtm>(contexto, _quitar);
            ServicioDeCaches.EliminarCache(CacheDe.Permisos_PorElemento);
            ServicioDeCaches.EliminarCache(CacheDe.Permisos_AlgunElemento);
            ServicioDeCaches.EliminarCache(CacheDe.Permisos_PermisoOtorgado);

            return sentencia.EjecutarSentencia(new DynamicParameters(parametrosSql));
        }


        public static void Insertar(ContextoSe contexto, int idNegocio, int idElemento, int idPermiso, int idUsuario, bool calculado)
        {
            string _crear = $@"
            insert into {ApiDeRegistroDtm.EsquemaTabla(typeof(PermisosPorElementoDtm))} (
                {ICampos.ID_NEGOCIO}
              , {ICampos.ID_ELEMENTO}
              , {ICampos.ID_USUARIO}
              , {ICampos.ID_PERMISO}
              , {ICampos.CALCULADO}) 
            select @{ICampos.ID_NEGOCIO}, @{ICampos.ID_ELEMENTO}, @{ICampos.ID_USUARIO}, @{ICampos.ID_PERMISO}, {(calculado ? 1 : 0)} 
            where Not Exists (select 1 
               from {ApiDeRegistroDtm.EsquemaTabla(typeof(PermisosPorElementoDtm))} 
               where {ICampos.ID_NEGOCIO} = @{ICampos.ID_NEGOCIO}
                 and {ICampos.ID_ELEMENTO} = @{ICampos.ID_ELEMENTO}
                 and {ICampos.ID_USUARIO} = @{ICampos.ID_USUARIO}
                 and {ICampos.ID_PERMISO} = @{ICampos.ID_PERMISO}
            )
            ";

            var parametrosSql = new Dictionary<string, object>
            {
                { $"@{ICampos.ID_NEGOCIO}", idNegocio },
                { $"@{ICampos.ID_ELEMENTO}", idElemento },
                { $"@{ICampos.ID_USUARIO}", idUsuario },
                { $"@{ICampos.ID_PERMISO}", idPermiso }
            };

            var sentencia = new ConsultaSql<PermisosPorElementoDtm>(contexto, _crear);
            sentencia.EjecutarSentencia(new DynamicParameters(parametrosSql));
            ServicioDeCaches.EliminarCache(CacheDe.Permisos_PorElemento);
            ServicioDeCaches.EliminarCache(CacheDe.Permisos_AlgunElemento);
        }

        public static bool EstaElPermisoOtorgado(ContextoSe contexto, PermisosPorElementoDtm registro)
        {
            string _leer = $@"select top(1) 1
            from {ApiDeRegistroDtm.EsquemaTabla(typeof(PermisosPorElementoDtm))} 
            where {ICampos.ID_PERMISO} = @{ICampos.ID_PERMISO}
              and {ICampos.ID_USUARIO} = @{ICampos.ID_USUARIO}
            ";

            var cache = ServicioDeCaches.Obtener(CacheDe.Permisos_PermisoOtorgado);
            var indice = $"{registro.IdPermiso}.{registro.IdUsuario}";
            if (!cache.ContainsKey(indice))
            {
                var parametrosSql = new Dictionary<string, object>
                {
                   { $"@{ICampos.ID_PERMISO}", registro.IdPermiso },
                   { $"@{ICampos.ID_USUARIO}", registro.IdUsuario }
                };

                var sentencia = new ConsultaSql<PermisosPorElementoDtm>(contexto, _leer);
                cache[indice] = sentencia.LanzarConsulta(new DynamicParameters(parametrosSql)).Count() > 0;
            }
            return (bool)cache[indice];
        }
    }

    public static partial class ModeloDeSeguridad
    {
        internal static string TablaDePermisoPorElemento => $"{ApiDeRegistroDtm.EsquemaTabla(typeof(PermisosPorElementoDtm))}";

        public static void PermisoPorElemento(ModelBuilder modelBuilder)
        {
            ApiDeRegistroDtm.DefinirCampoIdDtm<PermisosPorElementoDtm>(modelBuilder);
            modelBuilder.Entity<PermisosPorElementoDtm>().Property(p => p.IdNegocio).HasColumnName(ICampos.ID_NEGOCIO).HasColumnType(IDominio.INT).IsRequired();
            modelBuilder.Entity<PermisosPorElementoDtm>().Property(p => p.IdElemento).HasColumnName(ICampos.ID_ELEMENTO).HasColumnType(IDominio.INT).IsRequired();
            modelBuilder.Entity<PermisosPorElementoDtm>().Property(p => p.IdPermiso).HasColumnName(ICampos.ID_PERMISO).HasColumnType(IDominio.INT).IsRequired();
            modelBuilder.Entity<PermisosPorElementoDtm>().Property(p => p.IdUsuario).HasColumnName(ICampos.ID_USUARIO).HasColumnType(IDominio.INT).IsRequired();
            modelBuilder.Entity<PermisosPorElementoDtm>().Property(p => p.Calculado).HasColumnName(ICampos.CALCULADO).HasColumnType(IDominio.BIT).IsRequired();

            modelBuilder.Entity<PermisosPorElementoDtm>()
                        .HasAlternateKey(x => new { x.IdNegocio, x.IdElemento, x.IdUsuario, x.IdPermiso })
                        .HasName($"AK_{ApiDeRegistroDtm.NombreDeTabla(typeof(PermisosPorElementoDtm))}");

            ApiDeRegistroDtm.DefinirCampoFk<PermisosPorElementoDtm>(modelBuilder
                , nameof(PermisosPorElementoDtm.Negocio)
                , nameof(PermisosPorElementoDtm.IdNegocio)
                , ICampos.ID_NEGOCIO
                , requerida: true
                , unico: false);

            ApiDeRegistroDtm.DefinirCampoFk<PermisosPorElementoDtm>(modelBuilder
                , nameof(PermisosPorElementoDtm.Usuario)
                , nameof(PermisosPorElementoDtm.IdUsuario)
                , ICampos.ID_USUARIO
                , requerida: true
                , unico: false);

            ApiDeRegistroDtm.DefinirCampoFk<PermisosPorElementoDtm>(modelBuilder
                , nameof(PermisosPorElementoDtm.Permiso)
                , nameof(PermisosPorElementoDtm.IdPermiso)
                , ICampos.ID_PERMISO
                , requerida: true
                , unico: false);

            modelBuilder.Entity<PermisosPorElementoDtm>()
              .HasIndex(x => new { x.IdNegocio, x.IdElemento })
              .IsUnique(false)
              .HasDatabaseName($"I_{ApiDeRegistroDtm.NombreDeTabla(typeof(PermisosPorElementoDtm))}_{ICampos.ID_NEGOCIO}_{ICampos.ID_ELEMENTO}");

            modelBuilder.Entity<PermisosPorElementoDtm>()
              .HasIndex(x => new { x.IdNegocio, x.IdElemento, x.IdUsuario, x.IdPermiso })
              .IsUnique(false)
              .HasDatabaseName($"I_{ApiDeRegistroDtm.NombreDeTabla(typeof(PermisosPorElementoDtm))}_{ICampos.ID_NEGOCIO}_{ICampos.ID_ELEMENTO}_{ICampos.ID_PERMISO}");
        }
    }

}
