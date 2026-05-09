using System;
using System.Collections.Generic;
using Dapper;
using Gestor.Errores;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Seguridad;
using Utilidades;

namespace ServicioDeDatos.Elemento
{

    public class EstadoAuditado
    {
        public int idEstado;
        public string fechas;
    }

    public class ltrEstados
    {
        public static readonly string IdEstadoAuditado = nameof(IdEstadoAuditado).ToLower();
        public static readonly string FechaEstadoAuditado = nameof(FechaEstadoAuditado).ToLower();
        public const string EstadoNulo = Literal.Cero;
    }


    public class EstadoDtm : RegistroConNombreDtm, IEstado
    {
        public int IdPermiso { get; set; }
        public bool Inicial { get; set; }
        public bool Terminado { get; set; }
        public bool Cancelado { get; set; }
        public string Permiso { get; }
        public int Orden { get; set; }
        public static enumNegocio Negocio { get; }
    }

    public static class ApiDeEstado
    {
        public static string TablaDeEstados(Type t) => $"{ApiDeRegistroDtm.EsquemaDeTabla(t)}.{ApiDeRegistroDtm.NombreDeTabla(t)}_{Sufijo.ESTADO}";

        internal static void DefinirCampos<TEntity>(ModelBuilder modelBuilder) where TEntity : EstadoDtm
        {
            var nombreDeTabla = ApiDeRegistroDtm.NombreDeTabla(typeof(TEntity));

            ApiDeRegistroDtm.DefinirCampoIdDtm<TEntity>(modelBuilder);
            ApiDeNombreDtm.DefinirCampoNombreDtm<TEntity>(modelBuilder, 250, "", true, true);

            modelBuilder.Entity<TEntity>().Property(p => p.Inicial).HasColumnName(ICampos.INICIAL).HasColumnType(IDominio.BIT).IsRequired();
            modelBuilder.Entity<TEntity>().Property(p => p.Terminado).HasColumnName(ICampos.TERMINADO).HasColumnType(IDominio.BIT).IsRequired();
            modelBuilder.Entity<TEntity>().Property(p => p.Cancelado).HasColumnName(ICampos.CANCELADO).HasColumnType(IDominio.BIT).IsRequired();
            modelBuilder.Entity<TEntity>().Property(p => p.Orden).HasColumnType(IDominio.INT).HasColumnName(ICampos.ORDEN).IsRequired();

            modelBuilder.Entity<TEntity>().Property(x => x.IdPermiso).HasColumnType(IDominio.INT).HasColumnName(ICampos.ID_PERMISO).IsRequired();
            ApiDeRegistroDtm.DefinirFk<TEntity, PermisoDtm>(modelBuilder, nameof(EstadoDtm.IdPermiso), ICampos.ID_PERMISO, true);
        }
    }

    public class EstadoSql
    {
        private static readonly string _Campos = $@"
                            t1.{ICampos.ID}            as {nameof(EstadoDtm.Id)}
                          , t1.{ICampos.ORDEN}         as {nameof(EstadoDtm.Orden)}
                          , t1.{ICampos.NOMBRE}        as {nameof(EstadoDtm.Nombre)}
                          , t1.{ICampos.ID_PERMISO}    as {nameof(EstadoDtm.IdPermiso)}
                          , t1.{ICampos.INICIAL}       as {nameof(EstadoDtm.Inicial)}
                          , t1.{ICampos.TERMINADO}     as {nameof(EstadoDtm.Terminado)}
                          , t1.{ICampos.CANCELADO}     as {nameof(EstadoDtm.Cancelado)}
                          , t4.{ICampos.NOMBRE}        as {nameof(TransicionDtm.Permiso)}
                     ";

        public static EstadoDtm LeerEstadoPorId(ContextoSe contexto, string tablaDeEstados, int id)
        {
            var leerPorId = $@"
                     select {_Campos}
                     from {tablaDeEstados} T1 
                     inner join {ApiDeRegistroDtm.EsquemaTabla(typeof(PermisoDtm))} t4 on t4.{ICampos.ID} = t1.{ICampos.ID_PERMISO}
                     where t1.{ICampos.ID} = @{ICampos.ID}
                     ";
            var cache = ServicioDeCaches.Obtener(nameof(LeerEstadoPorId));
            var clave = $"{tablaDeEstados}.{id}";
            if (!cache.ContainsKey(clave))
            {
                var parametrosSql = new Dictionary<string, object>();
                parametrosSql.Add($"@{ICampos.ID}", id);

                var consulta = new ConsultaSql<EstadoDtm>(contexto, leerPorId);
                var registros = consulta.LanzarConsulta(new DynamicParameters(parametrosSql));

                if (registros.Count == 0)
                    GestorDeErrores.Emitir($"No se ha localizado el estado con Id: {id} en la tabla {tablaDeEstados}");

                cache[clave] = registros[0];
            }

            return (EstadoDtm)cache[clave];
        }
        public static List<EstadoDtm> LeerEstadoPorNombre(ContextoSe contexto, string tablaDeEstados, string nombre)
        {
            var leerPorNombre = $@"
                     select {_Campos}
                     from {tablaDeEstados} T1 
                     inner join {ApiDeRegistroDtm.EsquemaTabla(typeof(PermisoDtm))} t4 on t4.{ICampos.ID} = t1.{ICampos.ID_PERMISO}
                     where t1.{ICampos.NOMBRE} LIKE @{ICampos.NOMBRE}
                     ";

            var parametrosSql = new Dictionary<string, object>();
            parametrosSql.Add($"@{ICampos.NOMBRE}", nombre);

            var consulta = new ConsultaSql<EstadoDtm>(contexto, leerPorNombre);
            var registros = consulta.LanzarConsulta(new DynamicParameters(parametrosSql));

            return registros;
        }

        public static List<RegistroDtm> LeerEstadosPorSituacion(ContextoSe contexto, string tabla, string situacion)
        {

            var leerIds = $@"
                     select {ICampos.ID}
                     from {tabla}
                     where 1=1
                     ";
            var filtros = situacion == nameof(EstadoDtm.Cancelado) ? $"and {ICampos.CANCELADO} = @{ICampos.CANCELADO} " : "";
            filtros = situacion == nameof(EstadoDtm.Terminado) ? $"{filtros}and {ICampos.TERMINADO} = @{ICampos.TERMINADO}" : filtros;
            filtros = situacion == nameof(EstadoDtm.Inicial) ? $"{filtros}and {ICampos.INICIAL} = @{ICampos.INICIAL}" : filtros;
            var parametrosSql = new Dictionary<string, object>();
            parametrosSql.Add($"@{ICampos.CANCELADO}", true);
            parametrosSql.Add($"@{ICampos.TERMINADO}", true);
            parametrosSql.Add($"@{ICampos.INICIAL}", true);
            leerIds = $"{leerIds} {filtros}";

            var consulta = new ConsultaSql<RegistroDtm>(contexto, leerIds);
            var registros = consulta.LanzarConsulta(new DynamicParameters(parametrosSql));

            return registros;
        }

        public static List<EstadoDtm> LeerEstados(ContextoSe contexto, string tablaDeEstados, int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden)
        {
            var leerEstados = $@"
                     select {_Campos}
                     from {tablaDeEstados} T1 
                     inner join {ApiDeRegistroDtm.EsquemaTabla(typeof(PermisoDtm))} t4 on t4.{ICampos.ID} = t1.{ICampos.ID_PERMISO}
                     [where]
                     [orden]
                     ";
            var parametrosSql = new Dictionary<string, object>();
            var clausulaWhere = DefinirClausulaWhere(filtros, parametrosSql);
            var clausulaOrder = DefinirClausulaOrden(orden, parametrosSql);
            leerEstados = leerEstados.Replace("[where]", clausulaWhere);
            leerEstados = leerEstados.Replace("[orden]", clausulaOrder);

            if (cantidad > 0)
            {
                leerEstados = $@"{leerEstados}OFFSET @posicion ROWS FETCH NEXT @cantidad ROWS ONLY";
                parametrosSql.Add($"@posicion", posicion);
                parametrosSql.Add($"@cantidad", cantidad);
            }

            var consulta = new ConsultaSql<EstadoDtm>(contexto, leerEstados);
            var registros = consulta.LanzarConsulta(new DynamicParameters(parametrosSql));

            return registros;
        }


        public static int ContarRegistros(ContextoSe contexto, string tablaDeEstados, List<ClausulaDeFiltrado> filtros)
        {
            var contarRegistros = $@"
               select Count(*) as cantidad
               from {tablaDeEstados} T1 
               [where]
               ";
            var parametrosSql = new Dictionary<string, object>();
            var clausulaWhere = DefinirClausulaWhere(filtros, parametrosSql);
            contarRegistros = contarRegistros.Replace("[where]", clausulaWhere);
            var consulta = new ConsultaSql<RegistrosAfectados>(contexto, contarRegistros);
            var registros = consulta.LanzarConsulta(new DynamicParameters(parametrosSql));

            return registros[0].cantidad;
        }

        public static RegistroDtm Insertar(ContextoSe contexto, string tablaDeEstados, EstadoDtm estado)
        {
            var sentencia = $@"Insert into {tablaDeEstados} (  
                                         {ICampos.NOMBRE} 
                                       , {ICampos.ORDEN} 
                                       , {ICampos.ID_PERMISO} 
                                       , {ICampos.INICIAL}     
                                       , {ICampos.TERMINADO}
                                       , {ICampos.CANCELADO} 
                                       )
                               values (        
                                         @{ICampos.NOMBRE} 
                                       , @{ICampos.ORDEN} 
                                       , @{ICampos.ID_PERMISO} 
                                       , @{ICampos.INICIAL}     
                                       , @{ICampos.TERMINADO}
                                       , @{ICampos.CANCELADO}
                                      )
                               SELECT SCOPE_IDENTITY() as {nameof(RegistroDtm.Id)}
             ";

            var parametrosSql = new Dictionary<string, object>();
            AsignarParametros(estado, parametrosSql);

            var consulta = new ConsultaSql<RegistroDtm>(contexto, sentencia);
            var registros = consulta.LanzarConsulta(new DynamicParameters(parametrosSql))[0];
            ServicioDeCaches.EliminarCache($"{nameof(EstadoDtm.Terminado)}");
            ServicioDeCaches.EliminarCache($"{nameof(EstadoDtm.Cancelado)}");
            ServicioDeCaches.EliminarCache($"{nameof(EstadoDtm.Inicial)}");
            ServicioDeCaches.EliminarCache(CacheDe.Estados);
            return registros;
        }

        public static void Modificar(ContextoSe contexto, string tablaDeEstados, EstadoDtm estado)
        {
            var sentencia = $@"update {tablaDeEstados} set 
                                         {ICampos.NOMBRE}     = @{ICampos.NOMBRE} 
                                       , {ICampos.ORDEN}      = @{ICampos.ORDEN} 
                                       , {ICampos.ID_PERMISO} = @{ICampos.ID_PERMISO}     
                                       , {ICampos.INICIAL}    = @{ICampos.INICIAL}
                                       , {ICampos.TERMINADO}  = @{ICampos.TERMINADO}
                                       , {ICampos.CANCELADO}  = @{ICampos.CANCELADO}  
                               where {ICampos.ID} = @{ICampos.ID}";

            var parametrosSql = new Dictionary<string, object>
            {
                { $"@{ICampos.ID}", estado.Id }
            };
            AsignarParametros(estado, parametrosSql);

            var consulta = new ConsultaSql<EstadoDtm>(contexto, sentencia);
            consulta.EjecutarSentencia(new DynamicParameters(parametrosSql));
            ServicioDeCaches.EliminarCache($"{nameof(EstadoDtm.Terminado)}");
            ServicioDeCaches.EliminarCache($"{nameof(EstadoDtm.Cancelado)}");
            ServicioDeCaches.EliminarCache($"{nameof(EstadoDtm.Inicial)}");
            ServicioDeCaches.EliminarCache(CacheDe.Estados);
            ServicioDeCaches.EliminarCache(CacheDe.EstadosSiguientes);
            ServicioDeCaches.EliminarCache(CacheDe.Estado);
        }

        public static void Borrar(ContextoSe contexto, string tablaDeEstados, EstadoDtm estado)
        {
            var sentencia = $@"Delete 
                               from {tablaDeEstados} 
                               where {ICampos.ID} = @{ICampos.ID}";

            var parametrosSql = new Dictionary<string, object>
            {
                { $"@{ICampos.ID}", estado.Id }
            };

            var consulta = new ConsultaSql<EstadoDtm>(contexto, sentencia);
            consulta.EjecutarSentencia(new DynamicParameters(parametrosSql));
            ServicioDeCaches.EliminarCache($"{nameof(EstadoDtm.Terminado)}");
            ServicioDeCaches.EliminarCache($"{nameof(EstadoDtm.Cancelado)}");
            ServicioDeCaches.EliminarCache($"{nameof(EstadoDtm.Inicial)}");
            ServicioDeCaches.EliminarCache(CacheDe.Estados);
            ServicioDeCaches.EliminarCache(CacheDe.EstadosSiguientes);
            ServicioDeCaches.EliminarCache(CacheDe.Estado);
        }

        private static string DefinirClausulaWhere(List<ClausulaDeFiltrado> filtros, Dictionary<string, object> parametrosSql)
        {
            var where = ConsultaSql<EstadoDtm>.DefinirClausulaWhere(filtros, parametrosSql);
            return where;
        }
        private static void AsignarParametros(EstadoDtm estado, Dictionary<string, object> parametrosSql)
        {
            parametrosSql.Add($"@{ICampos.NOMBRE}", estado.Nombre);
            parametrosSql.Add($"@{ICampos.ID_PERMISO}", estado.IdPermiso);
            parametrosSql.Add($"@{ICampos.INICIAL}", estado.Inicial);
            parametrosSql.Add($"@{ICampos.TERMINADO}", estado.Terminado);
            parametrosSql.Add($"@{ICampos.CANCELADO}", estado.Cancelado);
            parametrosSql.Add($"@{ICampos.ORDEN}", estado.Orden);
        }

        private static string DefinirClausulaOrden(List<ClausulaDeOrdenacion> orden, Dictionary<string, object> parametrosSql)
        {
            if (orden.Count == 0)
                return $"Order by T1.{ICampos.ORDEN} ASC, T1.{ICampos.NOMBRE} ASC";

            var clausula = ConsultaSql<EstadoDtm>.DefinirClausulaOrden(orden);

            if (orden.Count > 1 || clausula.IsNullOrEmpty()) foreach (var ordenarPor in orden)
                {
                    if (ordenarPor.OrdenarPor.Equals(nameof(EstadoDtm.Orden), StringComparison.CurrentCultureIgnoreCase))
                        clausula = $"{(clausula.IsNullOrEmpty() ? "" : $"{clausula},")} T1.{ICampos.ORDEN} {ordenarPor.ModoBd}";

                    if (ordenarPor.OrdenarPor.Equals(nameof(EstadoDtm.Nombre), StringComparison.CurrentCultureIgnoreCase))
                        clausula = $"{(clausula.IsNullOrEmpty() ? "" : $"{clausula},")} T1.{ICampos.NOMBRE} {ordenarPor.ModoBd}";
                }

            return $"Order by {clausula}";
        }

    }

}
