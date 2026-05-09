using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using ServicioDeDatos.Elemento;
using Utilidades;

namespace ServicioDeDatos.Seguridad
{

    public class PermisosPorTransicionSql
    {
        private static readonly string _LeerTabla = $@"
            select {ICampos.ID}
              , {ICampos.ID_NEGOCIO} as {nameof(PermisosPorTransicionDtm.IdNegocio)}
              , {ICampos.ID_TRANSICION} as {nameof(PermisosPorTransicionDtm.IdTransicion)}
              , {ICampos.ID_USUARIO} as {nameof(PermisosPorTransicionDtm.IdUsuario)}
              , {ICampos.ID_PERMISO}  as {nameof(PermisosPorTransicionDtm.IdPermiso)}
              , {ICampos.CALCULADO}  as {nameof(PermisosPorTransicionDtm.Calculado)}
            from {ApiDeRegistroDtm.EsquemaTabla(typeof(PermisosPorTransicionDtm))} ";

        public static List<PermisosPorTransicionDtm> PermisosDeUsuario(ContextoSe contexto, int idNegocio, int IdTransicion)
        {
            string _leer = $@"{_LeerTabla}
            where {ICampos.ID_NEGOCIO} = @{ICampos.ID_NEGOCIO}
              and {ICampos.ID_USUARIO} = @{ICampos.ID_USUARIO}
            ";

            if (IdTransicion > 0) _leer = $"{_leer}{Environment.NewLine}and {ICampos.ID_TRANSICION} = @{ICampos.ID_TRANSICION}";

            var cache = ServicioDeCaches.Obtener(CacheDe.Permisos_PorTransicion);
            var indice = $"{idNegocio}.{contexto.DatosDeConexion.IdUsuario}.{IdTransicion}";
            if (!cache.ContainsKey(indice))
            {
                var parametrosSql = new Dictionary<string, object>
                {
                    { $"@{ICampos.ID_NEGOCIO}", idNegocio },
                    { $"@{ICampos.ID_USUARIO}", contexto.DatosDeConexion.IdUsuario },
                    { $"@{ICampos.ID_TRANSICION}", IdTransicion }
                };

                var sentencia = new ConsultaSql<PermisosPorTransicionDtm>(contexto, _leer);
                cache[indice] = sentencia.LanzarConsulta(new DynamicParameters(parametrosSql));
            }
            return (List<PermisosPorTransicionDtm>)cache[indice];
        }

        public static List<PermisosPorTransicionDtm> PermisosDeUsuario(ContextoSe contexto, int idNegocio)
        {
            string _leer = $@"{_LeerTabla}
            where {ICampos.ID_NEGOCIO} = @{ICampos.ID_NEGOCIO}
              and {ICampos.ID_USUARIO} = @{ICampos.ID_USUARIO}
            ";

            var cache = ServicioDeCaches.Obtener(CacheDe.Permisos_PorTransicion);
            var indice = $"{idNegocio}.{contexto.DatosDeConexion.IdUsuario}";
            if (!cache.ContainsKey(indice))
            {

                var parametrosSql = new Dictionary<string, object>
                {
                    { $"@{ICampos.ID_NEGOCIO}", idNegocio },
                    { $"@{ICampos.ID_USUARIO}", contexto.DatosDeConexion.IdUsuario }
                };

                var sentencia = new ConsultaSql<PermisosPorTransicionDtm>(contexto, _leer);
                cache[indice] = sentencia.LanzarConsulta(new DynamicParameters(parametrosSql));
            }
            return (List<PermisosPorTransicionDtm>)cache[indice];
        }

        public static bool EstaElPermisoOtorgado(ContextoSe contexto, PermisosPorTransicionDtm registro)
        {
            string _leer = $@"select {ICampos.ID}
            from {ApiDeRegistroDtm.EsquemaTabla(typeof(PermisosPorTransicionDtm))} 
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

        public static bool UsuarioConPermiso(ContextoSe contexto, int idPermiso)
        {
            string _leer = $@"
            select top(1) 1
            from {ApiDeRegistroDtm.EsquemaTabla(typeof(PermisosPorTransicionDtm))} 
            where {ICampos.ID_USUARIO} = @{ICampos.ID_USUARIO}
              and {ICampos.ID_PERMISO} = @{ICampos.ID_PERMISO}
            ";
            var parametrosSql = new Dictionary<string, object>
            {
                { $"@{ICampos.ID_USUARIO}", contexto.DatosDeConexion.IdUsuario },
                { $"@{ICampos.ID_PERMISO}", idPermiso }
            };
            var sentencia = new ConsultaSql<PermisosPorTransicionDtm>(contexto, _leer);
            return sentencia.LanzarConsulta(new DynamicParameters(parametrosSql)).Count() == 1;
        }

        public static void Otorgar(ContextoSe contexto, int idNegocio, int IdTransicion, int idUsuario, int idPermiso, bool calculado)
        {
            string _crear = $@"
            insert into {ApiDeRegistroDtm.EsquemaTabla(typeof(PermisosPorTransicionDtm))} (
                {ICampos.ID_NEGOCIO}
              , {ICampos.ID_TRANSICION}
              , {ICampos.ID_USUARIO}
              , {ICampos.ID_PERMISO}
              , {ICampos.CALCULADO}
            ) 
            select @{ICampos.ID_NEGOCIO}, @{ICampos.ID_TRANSICION}, @{ICampos.ID_USUARIO}, @{ICampos.ID_PERMISO}, {(calculado ? 1 : 0)}
            where Not Exists (select 1 
               from {ApiDeRegistroDtm.EsquemaTabla(typeof(PermisosPorTransicionDtm))} 
               where {ICampos.ID_NEGOCIO} = @{ICampos.ID_NEGOCIO}
                 and {ICampos.ID_TRANSICION} = @{ICampos.ID_TRANSICION}
                 and {ICampos.ID_USUARIO} = @{ICampos.ID_USUARIO}
                 and {ICampos.ID_PERMISO} = @{ICampos.ID_PERMISO}
            )
            ";

            var parametrosSql = new Dictionary<string, object>
            {
                { $"@{ICampos.ID_NEGOCIO}", idNegocio },
                { $"@{ICampos.ID_TRANSICION}", IdTransicion },
                { $"@{ICampos.ID_USUARIO}", idUsuario },
                { $"@{ICampos.ID_PERMISO}", idPermiso }
            };

            var sentencia = new ConsultaSql<PermisosPorTransicionDtm>(contexto, _crear);
            sentencia.EjecutarSentencia(new DynamicParameters(parametrosSql));
        }

        public static void QuitarPermisos(ContextoSe contexto, int idUsuario, bool? calculado)
        {
            var esCalculado = calculado == null ? "" : $" and {ICampos.CALCULADO} = {((bool)calculado ? 1 : 0)}";

            string _quitar = $@"
            delete 
            from {ApiDeRegistroDtm.EsquemaTabla(typeof(PermisosPorTransicionDtm))}
            where  {ICampos.ID_USUARIO} = @{ICampos.ID_USUARIO}{esCalculado}";
            var parametrosSql = new Dictionary<string, object> { { $"@{ICampos.ID_USUARIO}", idUsuario } };

            var sentencia = new ConsultaSql<PermisosPorTransicionDtm>(contexto, _quitar);
            sentencia.EjecutarSentencia(new DynamicParameters(parametrosSql));
            ServicioDeCaches.EliminarCache(CacheDe.Permisos_PorTransicion);
        }

        public static int EliminarTodos(ContextoSe contexto)
        {
            string _quitar = $@"
            delete 
            from {ApiDeRegistroDtm.EsquemaTabla(typeof(PermisosPorTransicionDtm))}
            where {ICampos.CALCULADO} = 1 
            ";
            var parametrosSql = new Dictionary<string, object>();

            var sentencia = new ConsultaSql<PermisosPorTransicionDtm>(contexto, _quitar);
            ServicioDeCaches.EliminarCache(CacheDe.Permisos_PorTransicion);
            return sentencia.EjecutarSentencia(new DynamicParameters(parametrosSql));
        }

    }


}
