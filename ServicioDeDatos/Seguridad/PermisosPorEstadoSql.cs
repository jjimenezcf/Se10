using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using ServicioDeDatos.Elemento;
using Utilidades;

namespace ServicioDeDatos.Seguridad
{

    public class PermisosPorEstadoSql
    {
        private static readonly string _LeerTabla = $@"
            select {ICampos.ID}
              , {ICampos.ID_NEGOCIO} as {nameof(PermisosPorEstadoDtm.IdNegocio)}
              , {ICampos.ID_ESTADO} as {nameof(PermisosPorEstadoDtm.IdEstado)}
              , {ICampos.ID_USUARIO} as {nameof(PermisosPorEstadoDtm.IdUsuario)}
              , {ICampos.ID_PERMISO}  as {nameof(PermisosPorEstadoDtm.IdPermiso)}
              , {ICampos.CALCULADO}  as {nameof(PermisosPorEstadoDtm.Calculado)}
            from {ApiDeRegistroDtm.EsquemaTabla(typeof(PermisosPorEstadoDtm))} ";

        public static List<PermisosPorEstadoDtm> PermisosDeUsuario(ContextoSe contexto, int idNegocio, int IdEstado)
        {
            string _leer = $@"{_LeerTabla}
            where {ICampos.ID_NEGOCIO} = @{ICampos.ID_NEGOCIO}
              and {ICampos.ID_USUARIO} = @{ICampos.ID_USUARIO}
            ";

            if (IdEstado > 0) _leer = $"{_leer}{Environment.NewLine}and {ICampos.ID_ESTADO} = @{ICampos.ID_ESTADO}";

            var cache = ServicioDeCaches.Obtener(CacheDe.Permisos_PorEstado);
            var indice = $"{idNegocio}.{contexto.DatosDeConexion.IdUsuario}.{IdEstado}";
            if (!cache.ContainsKey(indice))
            {
                var parametrosSql = new Dictionary<string, object>
                {
                    { $"@{ICampos.ID_NEGOCIO}", idNegocio },
                    { $"@{ICampos.ID_USUARIO}", contexto.DatosDeConexion.IdUsuario },
                    { $"@{ICampos.ID_ESTADO}", IdEstado }
                };

                var sentencia = new ConsultaSql<PermisosPorEstadoDtm>(contexto, _leer);
                cache[indice] = sentencia.LanzarConsulta(new DynamicParameters(parametrosSql));
            }
            return (List<PermisosPorEstadoDtm>)cache[indice];
        }

        public static List<PermisosPorEstadoDtm> PermisosDeUsuario(ContextoSe contexto, int idNegocio)
        {
            string _leer = $@"{_LeerTabla}
            where {ICampos.ID_NEGOCIO} = @{ICampos.ID_NEGOCIO}
              and {ICampos.ID_USUARIO} = @{ICampos.ID_USUARIO}
            ";

            var cache = ServicioDeCaches.Obtener(CacheDe.Permisos_PorEstado);
            var indice = $"{idNegocio}.{contexto.DatosDeConexion.IdUsuario}";
            if (!cache.ContainsKey(indice))
            {

                var parametrosSql = new Dictionary<string, object>
                {
                    { $"@{ICampos.ID_NEGOCIO}", idNegocio },
                    { $"@{ICampos.ID_USUARIO}", contexto.DatosDeConexion.IdUsuario }
                };

                var sentencia = new ConsultaSql<PermisosPorEstadoDtm>(contexto, _leer);
                cache[indice] = sentencia.LanzarConsulta(new DynamicParameters(parametrosSql));
            }
            return (List<PermisosPorEstadoDtm>)cache[indice];
        }

        public static bool EstaElPermisoOtorgado(ContextoSe contexto, PermisosPorEstadoDtm registro)
        {
            string _leer = $@"select {ICampos.ID}
            from {ApiDeRegistroDtm.EsquemaTabla(typeof(PermisosPorEstadoDtm))} 
            where {ICampos.ID_PERMISO} = @{ICampos.ID_PERMISO}
              and {ICampos.ID_USUARIO} = @{ICampos.ID_USUARIO}
            ";

            var parametrosSql = new Dictionary<string, object>();

            parametrosSql.Add($"@{ICampos.ID_PERMISO}", registro.IdPermiso);
            parametrosSql.Add($"@{ICampos.ID_USUARIO}", registro.IdUsuario);

            var sentencia = new ConsultaSql<RegistroDtm>(contexto, _leer);
            var r = sentencia.LanzarConsulta(new DynamicParameters(parametrosSql));

            return r.Count() > 0;
        }

        public static bool UsuarioConPermiso(ContextoSe contexto, int idPermiso)
        {
            string _leer = $@"
            select top(1) 1
            from {ApiDeRegistroDtm.EsquemaTabla(typeof(PermisosPorEstadoDtm))} 
            where {ICampos.ID_USUARIO} = @{ICampos.ID_USUARIO}
              and {ICampos.ID_PERMISO} = @{ICampos.ID_PERMISO}
            ";
            var parametrosSql = new Dictionary<string, object>
            {
                { $"@{ICampos.ID_USUARIO}", contexto.DatosDeConexion.IdUsuario },
                { $"@{ICampos.ID_PERMISO}", idPermiso }
            };
            var sentencia = new ConsultaSql<PermisosPorEstadoDtm>(contexto, _leer);
            return sentencia.LanzarConsulta(new DynamicParameters(parametrosSql)).Count() == 1;
        }

        public static void Otorgar(ContextoSe contexto, int idNegocio, int IdEstado, int idUsuario, int idPermiso, bool calculado)
        {
            string _crear = $@"
            insert into {ApiDeRegistroDtm.EsquemaTabla(typeof(PermisosPorEstadoDtm))} (
                {ICampos.ID_NEGOCIO}
              , {ICampos.ID_ESTADO}
              , {ICampos.ID_USUARIO}
              , {ICampos.ID_PERMISO}
              , {ICampos.CALCULADO}
            ) 
            select @{ICampos.ID_NEGOCIO}, @{ICampos.ID_ESTADO}, @{ICampos.ID_USUARIO}, @{ICampos.ID_PERMISO}, {(calculado ? 1 : 0)}
            where Not Exists (select 1 
               from {ApiDeRegistroDtm.EsquemaTabla(typeof(PermisosPorEstadoDtm))} 
               where {ICampos.ID_NEGOCIO} = @{ICampos.ID_NEGOCIO}
                 and {ICampos.ID_ESTADO} = @{ICampos.ID_ESTADO}
                 and {ICampos.ID_USUARIO} = @{ICampos.ID_USUARIO}
                 and {ICampos.ID_PERMISO} = @{ICampos.ID_PERMISO}
            )
            ";

            var parametrosSql = new Dictionary<string, object>
            {
                { $"@{ICampos.ID_NEGOCIO}", idNegocio },
                { $"@{ICampos.ID_ESTADO}", IdEstado },
                { $"@{ICampos.ID_USUARIO}", idUsuario },
                { $"@{ICampos.ID_PERMISO}", idPermiso }
            };

            var sentencia = new ConsultaSql<PermisosPorEstadoDtm>(contexto, _crear);
            sentencia.EjecutarSentencia(new DynamicParameters(parametrosSql));
        }

        public static void QuitarPermisos(ContextoSe contexto, int idUsuario, bool? calculado)
        {
            var esCalculado = calculado == null ? "" : $" and {ICampos.CALCULADO} = {((bool)calculado ? 1 : 0)}";

            string _quitar = $@"
            delete 
            from {ApiDeRegistroDtm.EsquemaTabla(typeof(PermisosPorEstadoDtm))}
            where  {ICampos.ID_USUARIO} = @{ICampos.ID_USUARIO}{esCalculado}";
            var parametrosSql = new Dictionary<string, object> { { $"@{ICampos.ID_USUARIO}", idUsuario } };

            var sentencia = new ConsultaSql<PermisosPorEstadoDtm>(contexto, _quitar);
            sentencia.EjecutarSentencia(new DynamicParameters(parametrosSql));
            ServicioDeCaches.EliminarCache(CacheDe.Permisos_PorEstado);
        }

        public static int EliminarTodos(ContextoSe contexto)
        {
            string _quitar = $@"
            delete 
            from {ApiDeRegistroDtm.EsquemaTabla(typeof(PermisosPorEstadoDtm))}
            where {ICampos.CALCULADO} = 1 
            ";
            var parametrosSql = new Dictionary<string, object>();

            var sentencia = new ConsultaSql<PermisosPorEstadoDtm>(contexto, _quitar);
            ServicioDeCaches.EliminarCache(CacheDe.Permisos_PorEstado);
            return sentencia.EjecutarSentencia(new DynamicParameters(parametrosSql));
        }

    }


}
