using System;
using System.Collections.Generic;
using System.ComponentModel;
using Dapper;
using Gestor.Errores;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Entorno;
using Utilidades;

namespace ServicioDeDatos.Elemento
{
    public enum enumMomentoDeEjecucion
    {
        [Description("Antes de transitar")]
        A,
        [Description("Después de transitar")]
        D,
        [Description("Al finalizar de transitar")]
        T
    }

    public class AccionesDeTrnDtm : RelacionDtm, IAccionDeTrn, INecesitaSerParametrizador
    {
        public int IdTransicion { get; set; }
        public int IdAccion { get; set; }
        public string Parametros { get; set; }
        public string Descripcion { get; set; }
        public string Momento { get; set; }
        public string Accion { get; }
        public int Orden { get; set; }
        public string Transicion { get; }
        public bool Activo { get; set; }
        public AccionesDeTrnDtm()
        {
            PropiedadDelIdElemento1 = nameof(IdTransicion);
            PropiedadDelIdElemento2 = nameof(IdAccion);
        }

    }

    public static class ApiDeAccionDeTrn
    {
        public static string TablaDeAcciones(Type t) => $"{ApiDeRegistroDtm.EsquemaDeTabla(t)}.{ApiDeRegistroDtm.NombreDeTabla(t)}_{Sufijo.ACCION}";

        internal static void DefinirCampos<TEntity, TPadre>(ModelBuilder modelBuilder)
        where TEntity : AccionesDeTrnDtm
        where TPadre : TransicionDtm
        {
            var nombreDeTabla = ApiDeRegistroDtm.NombreDeTabla(typeof(TEntity));

            ApiDeRegistroDtm.DefinirCampoIdDtm<TEntity>(modelBuilder);

            modelBuilder.Entity<TEntity>().Property(x => x.IdTransicion).HasColumnType(IDominio.INT).HasColumnName(ICampos.ID_TRANSICION).IsRequired();
            modelBuilder.Entity<TEntity>().Property(x => x.IdAccion).HasColumnType(IDominio.INT).HasColumnName(ICampos.ID_ACCION).IsRequired();
            modelBuilder.Entity<TEntity>().Property(x => x.Parametros).HasColumnName(ICampos.PARAMETROS).HasColumnType(IDominio.VARCHAR_2000).IsRequired(false);
            modelBuilder.Entity<TEntity>().Property(p => p.Descripcion).HasColumnName(ICampos.DESCRIPCION).HasColumnType(IDominio.VARCHAR_2000).IsRequired(false);
            modelBuilder.Entity<TEntity>().Property(p => p.Momento).HasColumnName(ICampos.MOMENTO).HasColumnType(IDominio.VARCHAR_1).IsRequired();
            modelBuilder.Entity<TEntity>().Property(p => p.Orden).HasColumnType(IDominio.INT).HasColumnName(ICampos.ORDEN).IsRequired();
            modelBuilder.Entity<TEntity>().Property(p => p.Activo).HasColumnName(ICampos.ACTIVO).HasColumnType(IDominio.BIT).IsRequired();

            ApiDeRegistroDtm.DefinirFk<TEntity, TPadre>(modelBuilder, nameof(AccionesDeTrnDtm.IdTransicion), ICampos.ID_TRANSICION, unico: false);
            ApiDeRegistroDtm.DefinirFk<TEntity, AccionDtm>(modelBuilder, nameof(AccionesDeTrnDtm.IdAccion), ICampos.ID_ACCION, unico: false);

            modelBuilder.Entity<TEntity>().Ignore(p => p.Transicion);
            modelBuilder.Entity<TEntity>().Ignore(p => p.Accion);

            modelBuilder.Entity<TEntity>().HasIndex(x => new { x.IdTransicion, x.Momento, x.Orden }).HasDatabaseName($"I_{Tablas.ACCION}_POR_{ICampos.ORDEN}").IsUnique();

        }
    }

    public class AccionDeTrnSql
    {
        private static readonly string _Campos = $@"
                            t1.{ICampos.ID}              as {nameof(AccionesDeTrnDtm.Id)}
                          , t1.{ICampos.DESCRIPCION}     as {nameof(AccionesDeTrnDtm.Descripcion)}
                          , t1.{ICampos.ID_TRANSICION}   as {nameof(AccionesDeTrnDtm.IdTransicion)}
                          , t1.{ICampos.ID_ACCION}       as {nameof(AccionesDeTrnDtm.IdAccion)}
                          , t1.{ICampos.PARAMETROS}      as {nameof(AccionesDeTrnDtm.Parametros)}
                          , t1.{ICampos.MOMENTO}         as {nameof(AccionesDeTrnDtm.Momento)}
                          , t1.{ICampos.ACTIVO}          as {nameof(TransicionDtm.Activo)}
                          , t1.{ICampos.ORDEN}           as {nameof(AccionesDeTrnDtm.Orden)}
                          , t2.{ICampos.NOMBRE}          as {nameof(AccionesDeTrnDtm.Transicion)}
                          , t3.{ICampos.NOMBRE}          as {nameof(AccionesDeTrnDtm.Accion)}
                     ";

        public static AccionesDeTrnDtm LeerAccionPorId(ContextoSe contexto, string tablaDeAcciones, int id)
        {
            var tablaDeTransicion = tablaDeAcciones.Replace(Sufijo.ACCION, Sufijo.TRANSICION);
            var tablaDeAccion = ApiDeRegistroDtm.EsquemaTabla(typeof(AccionDtm));

            var leerPorId = $@"
                     select {_Campos}
                     from {tablaDeAcciones} T1
                     inner join {tablaDeTransicion} t2 on t2.{ICampos.ID} = t1.{ICampos.ID_TRANSICION}
                     inner join {tablaDeAccion} t3 on t3.{ICampos.ID} = t1.{ICampos.ID_ACCION}
                     where t1.{ICampos.ID} = @{ICampos.ID}
                     ";

            var cache = ServicioDeCaches.Obtener(nameof(LeerAccionPorId));
            var clave = $"{tablaDeAcciones}.{id}";
            if (!cache.ContainsKey(clave))
            {
                var parametrosSql = new Dictionary<string, object>();
                parametrosSql.Add($"@{ICampos.ID}", id);

                var consulta = new ConsultaSql<AccionesDeTrnDtm>(contexto, leerPorId);
                var registros = consulta.LanzarConsulta(new DynamicParameters(parametrosSql));

                if (registros.Count == 0)
                    GestorDeErrores.Emitir($"No se ha localizado la acción de la transición con Id: {id} en la tabla {tablaDeAcciones}");

                cache[clave] = registros[0];
            }

            return (AccionesDeTrnDtm)cache[clave];
        }

        public static List<AccionesDeTrnDtm> LeerAcciones(ContextoSe contexto, string tablaDeAcciones, int idTransicion, enumMomentoDeEjecucion momento)
        {
            var tablaDeTransicion = tablaDeAcciones.Replace(Sufijo.ACCION, Sufijo.TRANSICION);
            var tablaDeAccion = ApiDeRegistroDtm.EsquemaTabla(typeof(AccionDtm));

            var leerAcciones = $@"
                     select {_Campos}
                     from {tablaDeAcciones} T1 
                     inner join {tablaDeTransicion} t2 on t2.{ICampos.ID} = t1.{ICampos.ID_TRANSICION}
                     inner join {tablaDeAccion} t3 on t3.{ICampos.ID} = t1.{ICampos.ID_ACCION}
                     where t1.{ICampos.ID_TRANSICION} = @{ICampos.ID_TRANSICION}
                       and t1.{ICampos.MOMENTO} = @{ICampos.MOMENTO}
                     order by t1.{ICampos.ORDEN}
                     ";

            var parametrosSql = new Dictionary<string, object>
            {
                { $"@{ICampos.ID_TRANSICION}", idTransicion },
                { $"@{ICampos.MOMENTO}", $"{momento}" }
            };

            var consulta = new ConsultaSql<AccionesDeTrnDtm>(contexto, leerAcciones);
            var registros = consulta.LanzarConsulta(new DynamicParameters(parametrosSql));

            return registros;
        }


        public static List<AccionesDeTrnDtm> LeerAcciones(ContextoSe contexto, string tablaDeAcciones, int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden)
        {
            var tablaDeTransicion = tablaDeAcciones.Replace(Sufijo.ACCION, Sufijo.TRANSICION);
            var tablaDeAccion = ApiDeRegistroDtm.EsquemaTabla(typeof(AccionDtm));

            var leerAcciones = $@"
                     select {_Campos}
                     from {tablaDeAcciones} T1 
                     inner join {tablaDeTransicion} t2 on t2.{ICampos.ID} = t1.{ICampos.ID_TRANSICION}
                     inner join {tablaDeAccion} t3 on t3.{ICampos.ID} = t1.{ICampos.ID_ACCION}
                     [where]
                     [orden]
                     ";

            var parametrosSql = new Dictionary<string, object>();
            var clausulaWhere = DefinirClausulaWhere(filtros, parametrosSql);
            var clausulaOrder = DefinirClausulaOrden(orden);
            leerAcciones = leerAcciones.Replace("[where]", clausulaWhere);
            leerAcciones = leerAcciones.Replace("[orden]", clausulaOrder);

            if (cantidad > 0)
            {
                leerAcciones = $@"{leerAcciones}{Environment.NewLine}OFFSET @posicion ROWS FETCH NEXT @cantidad ROWS ONLY";
                parametrosSql.Add($"@posicion", posicion);
                parametrosSql.Add($"@cantidad", cantidad);
            }

            var consulta = new ConsultaSql<AccionesDeTrnDtm>(contexto, leerAcciones);
            var registros = consulta.LanzarConsulta(new DynamicParameters(parametrosSql));

            return registros;
        }


        public static int ContarRegistros(ContextoSe contexto, string tablaDeAcciones, List<ClausulaDeFiltrado> filtros)
        {
            var contarRegistros = $@"
               select Count(*) as cantidad
               from {tablaDeAcciones} T1 
               [where]
               ";
            var parametrosSql = new Dictionary<string, object>();
            var clausulaWhere = DefinirClausulaWhere(filtros, parametrosSql);
            contarRegistros = contarRegistros.Replace("[where]", clausulaWhere);

            var consulta = new ConsultaSql<RegistrosAfectados>(contexto, contarRegistros);
            var registros = consulta.LanzarConsulta(new DynamicParameters(parametrosSql));

            return registros[0].cantidad;
        }

        public static RegistroDtm Insertar(ContextoSe contexto, string tablaDeAcciones, AccionesDeTrnDtm accion)
        {
            var sentencia = $@"Insert into {tablaDeAcciones} ( 
                                         {ICampos.ID_TRANSICION} 
                                       , {ICampos.ID_ACCION}     
                                       , {ICampos.MOMENTO} 
                                       , {ICampos.ACTIVO}
                                       , {ICampos.ORDEN} 
                                       , {ICampos.PARAMETROS} 
                                       , {ICampos.DESCRIPCION} 
                                       )
                               values (        
                                         @{ICampos.ID_TRANSICION} 
                                       , @{ICampos.ID_ACCION}
                                       , @{ICampos.MOMENTO}
                                       , @{ICampos.ACTIVO}
                                       , @{ICampos.ORDEN} 
                                       , @{ICampos.PARAMETROS} 
                                       , @{ICampos.DESCRIPCION} 
                                      )
                               SELECT SCOPE_IDENTITY() as {nameof(RegistroDtm.Id)}
             ";

            var parametrosSql = new Dictionary<string, object>();
            AsignarParametros(accion, parametrosSql);

            var consulta = new ConsultaSql<RegistroDtm>(contexto, sentencia);
            var registros = consulta.LanzarConsulta(new DynamicParameters(parametrosSql))[0];
            return registros;
        }

        public static void Modificar(ContextoSe contexto, string tablaDeAcciones, AccionesDeTrnDtm accion)
        {
            var sentencia = $@"update {tablaDeAcciones} set 
                                         {ICampos.DESCRIPCION} = @{ICampos.DESCRIPCION} 
                                       , {ICampos.ID_TRANSICION} = @{ICampos.ID_TRANSICION}   
                                       , {ICampos.ID_ACCION} = @{ICampos.ID_ACCION}   
                                       , {ICampos.MOMENTO} = @{ICampos.MOMENTO}  
                                       , {ICampos.ACTIVO} = @{ICampos.ACTIVO}      
                                       , {ICampos.ORDEN}  = @{ICampos.ORDEN} 
                                       , {ICampos.PARAMETROS} = @{ICampos.PARAMETROS}
                               where {ICampos.ID} = @{ICampos.ID}";

            var parametrosSql = new Dictionary<string, object>();
            parametrosSql.Add($"@{ICampos.ID}", accion.Id);
            AsignarParametros(accion, parametrosSql);

            var consulta = new ConsultaSql<AccionesDeTrnDtm>(contexto, sentencia);
            consulta.EjecutarSentencia(new DynamicParameters(parametrosSql));
        }

        public static void Borrar(ContextoSe contexto, string tablaDeAcciones, AccionesDeTrnDtm accion)
        {
            var sentencia = $@"Delete 
                               from {tablaDeAcciones} 
                               where {ICampos.ID} = @{ICampos.ID}";

            var parametrosSql = new Dictionary<string, object>();
            parametrosSql.Add($"@{ICampos.ID}", accion.Id);

            var consulta = new ConsultaSql<AccionesDeTrnDtm>(contexto, sentencia);
            consulta.EjecutarSentencia(new DynamicParameters(parametrosSql));
        }

        private static void AsignarParametros(AccionesDeTrnDtm accion, Dictionary<string, object> parametrosSql)
        {
            parametrosSql.Add($"@{ICampos.ID_TRANSICION}", accion.IdTransicion);
            parametrosSql.Add($"@{ICampos.ID_ACCION}", accion.IdAccion);
            parametrosSql.Add($"@{ICampos.MOMENTO}", accion.Momento);
            parametrosSql.Add($"@{ICampos.ACTIVO}", accion.Activo);
            parametrosSql.Add($"@{ICampos.ORDEN}", accion.Orden);
            parametrosSql.Add($"@{ICampos.PARAMETROS}", accion.Parametros);
            parametrosSql.Add($"@{ICampos.DESCRIPCION}", accion.Descripcion);
            // parametrosSql.Add($"@{ICampos.ACTIVO}", Transicion.Activo);
        }
        private static string DefinirClausulaWhere(List<ClausulaDeFiltrado> filtros, Dictionary<string, object> parametrosSql)
        {
            var where = $"Where 1=1";
            foreach (var filtro in filtros)
            {
                if (filtro.Aplicado) continue;
                if (filtro.Clausula.Equals(nameof(AccionesDeTrnDtm.Transicion), StringComparison.CurrentCultureIgnoreCase) ||
                    filtro.Clausula.Equals(nameof(AccionesDeTrnDtm.IdTransicion), StringComparison.CurrentCultureIgnoreCase))
                    where = FiltroPorTransicion(where, parametrosSql, filtro);

                if (filtro.Clausula.Equals(nameof(AccionesDeTrnDtm.Accion), StringComparison.CurrentCultureIgnoreCase) ||
                    filtro.Clausula.Equals(nameof(AccionesDeTrnDtm.IdAccion), StringComparison.CurrentCultureIgnoreCase))
                    where = FiltroPorAccion(where, parametrosSql, filtro);

                if (filtro.Clausula.Equals(nameof(AccionesDeTrnDtm.Momento), StringComparison.CurrentCultureIgnoreCase))
                {
                    where = $"{where}{Environment.NewLine}and T1.{ICampos.MOMENTO} like @{ICampos.MOMENTO}";
                    parametrosSql.Add($"@{ICampos.MOMENTO}", filtro.Valor);
                    filtro.Aplicado = true;
                }

                if (filtro.Clausula.Equals(nameof(AccionesDeTrnDtm.Orden), StringComparison.CurrentCultureIgnoreCase))
                {
                    where = $"{where}{Environment.NewLine}and T1.{ICampos.ORDEN} like @{ICampos.ORDEN}";
                    parametrosSql.Add($"@{ICampos.ORDEN}", filtro.Valor);
                    filtro.Aplicado = true;
                }
            }
            return where;
        }

        public static string FiltroPorTransicion(string clausulaWhere, Dictionary<string, object> parametrosSql, ClausulaDeFiltrado filtro)
        {
            clausulaWhere = $"{clausulaWhere}{Environment.NewLine}and T1.{ICampos.ID_TRANSICION} = @{ICampos.ID_TRANSICION}";
            parametrosSql.Add($"@{ICampos.ID_TRANSICION}", filtro.Valor);
            filtro.Aplicado = true;
            return clausulaWhere;
        }
        public static string FiltroPorAccion(string clausulaWhere, Dictionary<string, object> parametrosSql, ClausulaDeFiltrado filtro)
        {
            clausulaWhere = $"{clausulaWhere}{Environment.NewLine}and T1.{ICampos.ID_ACCION} = @{ICampos.ID_ACCION}";
            parametrosSql.Add($"@{ICampos.ID_ACCION}", filtro.Valor);
            filtro.Aplicado = true;
            return clausulaWhere;
        }
        private static string DefinirClausulaOrden(List<ClausulaDeOrdenacion> orden)
        {
            return $"Order by T1.{ICampos.MOMENTO} ASC, T1.{ICampos.ORDEN} ASC";
        }

    }


}
