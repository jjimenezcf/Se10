using System;
using System.Collections.Generic;
using Dapper;
using Gestor.Errores;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Entorno;
using Utilidades;

namespace ServicioDeDatos.Elemento
{
    public class HitoDtm : RegistroDtm, IHitoDtm
    {
        public int IdElemento { get; set; }
        public DateTime Fecha { get; set; }
        public int IdUsuario { get; set; }
        public int IdEstado { get; set; }
        public long? Tiempo { get; set; }
        public int? IdTransicion { get; set; }
        public int? IdObservacion { get; set; }
        public string Elemento { get; set; }
        public string Usuario { get; set; }
        public string Estado { get; set; }
        public string Transicion { get; set; }
        public string Observacion { get; set; }
        public DateTime? Fin => Tiempo is null ? null : Fecha.AddTicks((long)Tiempo);

    }


    public static class ApiDeHitos
    {

        public static string TablaDeHistorias(Type t) => $"{ApiDeRegistroDtm.EsquemaDeTabla(t)}.{ApiDeRegistroDtm.NombreDeTabla(t)}_{Sufijo.HISTORIA}";


        internal static void DefinirCampos<TEntity, TPadre, TEstado, TTransicion, TObservacion>(ModelBuilder modelBuilder)
        where TEntity : HitoDtm
        where TPadre : ElementoDtm
        where TEstado : EstadoDtm
        where TTransicion : TransicionDtm
        where TObservacion : ObservacionDtm
        {
            var nombreDeTabla = ApiDeRegistroDtm.NombreDeTabla(typeof(TEntity));

            ApiDeRegistroDtm.DefinirCampoIdDtm<TEntity>(modelBuilder);
            ApiDeElementoDtm.DefinirCampoIdElemento<TEntity>(modelBuilder);
            modelBuilder.Entity<TEntity>().Property(p => p.Fecha).HasColumnName(ICampos.FECHA).HasColumnType(IDominio.DATETIME_2).IsRequired();
            modelBuilder.Entity<TEntity>().Property(p => p.IdUsuario).HasColumnName(ICampos.ID_USUARIO).HasColumnType(IDominio.INT).IsRequired();
            modelBuilder.Entity<TEntity>().Property(p => p.IdEstado).HasColumnName(ICampos.ID_ESTADO).HasColumnType(IDominio.INT).IsRequired();
            modelBuilder.Entity<TEntity>().Property(p => p.Tiempo).HasColumnName(ICampos.TIEMPO).HasColumnType(IDominio.BIGINT).IsRequired(false);
            modelBuilder.Entity<TEntity>().Property(p => p.IdTransicion).HasColumnName(ICampos.ID_TRANSICION).HasColumnType(IDominio.INT).IsRequired(false);
            modelBuilder.Entity<TEntity>().Property(p => p.IdObservacion).HasColumnName(ICampos.ID_OBSERVACION).HasColumnType(IDominio.INT).IsRequired(false);

            ApiDeRegistroDtm.DefinirFk<TEntity, UsuarioDtm>(modelBuilder, nameof(HitoDtm.IdUsuario), ICampos.ID_USUARIO, false);
            ApiDeRegistroDtm.DefinirFk<TEntity, TEstado>(modelBuilder, nameof(HitoDtm.IdEstado), ICampos.ID_ESTADO, false);
            ApiDeRegistroDtm.DefinirFk<TEntity, TTransicion>(modelBuilder, nameof(HitoDtm.IdTransicion), ICampos.ID_TRANSICION, false);
            ApiDeRegistroDtm.DefinirFk<TEntity, TObservacion>(modelBuilder, nameof(HitoDtm.IdObservacion), ICampos.ID_OBSERVACION, false);
            ApiDeRegistroDtm.DefinirFk<TEntity, TPadre>(modelBuilder, nameof(HitoDtm.IdElemento), ICampos.ID_ELEMENTO, false);

            modelBuilder.Entity<TEntity>().Ignore(p => p.Usuario);
            modelBuilder.Entity<TEntity>().Ignore(p => p.Estado);
            modelBuilder.Entity<TEntity>().Ignore(p => p.Transicion);
            modelBuilder.Entity<TEntity>().Ignore(p => p.Observacion);
            modelBuilder.Entity<TEntity>().Ignore(p => p.Elemento);
        }
    }
    public class HitoSql
    {
        private static readonly string _campos = $@"
                            t1.{ICampos.ID}                 as {nameof(HitoDtm.Id)}
                          , t1.{ICampos.ID_ELEMENTO}        as {nameof(HitoDtm.IdElemento)}
                          , t1.{ICampos.ID_ESTADO}          as {nameof(HitoDtm.IdEstado)}
                          , t1.{ICampos.ID_USUARIO}         as {nameof(HitoDtm.IdUsuario)}
                          , t1.{ICampos.ID_TRANSICION}      as {nameof(HitoDtm.IdTransicion)}
                          , t1.{ICampos.ID_OBSERVACION}     as {nameof(HitoDtm.IdObservacion)}
                          , t1.{ICampos.TIEMPO}             as {nameof(HitoDtm.Tiempo)}
                          , t1.{ICampos.FECHA}              as {nameof(HitoDtm.Fecha)}
                     ";

        public static HitoDtm LeerHistoriaPorId(ContextoSe contexto, string esquemaTabla, int id)
        {
            var leerPorId = $@"
                     select {_campos}
                     from {esquemaTabla} T1 
                     where t1.{ICampos.ID} = @{ICampos.ID}
                     ";

            var cache = ServicioDeCaches.Obtener(nameof(LeerHistoriaPorId));
            var clave = $"{esquemaTabla}.{id}";
            if (!cache.ContainsKey(clave))
            {
                var parametrosSql = new Dictionary<string, object>
                {
                    { $"@{ICampos.ID}", id }
                };

                var consulta = new ConsultaSql<HitoDtm>(contexto, leerPorId);
                var registros = consulta.LanzarConsulta(new DynamicParameters(parametrosSql));

                if (registros.Count == 0)
                    GestorDeErrores.Emitir($"No se ha localizado la historia con Id: {id} en la tabla {esquemaTabla}");

                cache[clave] = registros[0];
            }

            return (HitoDtm)cache[clave];
        }


        public static HitoDtm LeerAnteriorHito(ContextoSe contexto, string esquemaTabla, int idElemento)
        {
            var historia = LeerHistoriaDelElemento(contexto, esquemaTabla, idElemento);
            if (historia.Count <= 1) return null;
            return historia[1];
        }

        public static List<HitoDtm> LeerHitosDeUnaEtapaPosteriorA(ContextoSe contexto, string esquemaTabla, int idElemento, List<int> etapaRestrictora, List<int> etapaDeFiltrado)
        {
            var filtroPorEtapa = "1=0";
            foreach (var id in etapaDeFiltrado) filtroPorEtapa = filtroPorEtapa + $" or T1.ID_ESTADO = {id}";

            var restrictorPorEtapa = "1=0";
            foreach (var id in etapaRestrictora) restrictorPorEtapa = restrictorPorEtapa + $" or T11.ID_ESTADO = {id}";

            var leerHitosPosteriores = $@"
                     select {_campos}
                     from {esquemaTabla} T1 
                     where t1.{ICampos.ID_ELEMENTO} = @{ICampos.ID_ELEMENTO}
                     AND ({filtroPorEtapa})
                     and t1.ID > (select top 1 id 
                                  from TAREA.TAREA_HISTORIA T11
			                      where t11.{ICampos.ID_ELEMENTO} = @{ICampos.ID_ELEMENTO}
                                  AND ({restrictorPorEtapa})
			                      order by T11.id desc)
			         order by T1.id ASC
                     ";
            var parametrosSql = new Dictionary<string, object>
            {
                { $"@{ICampos.ID_ELEMENTO}", idElemento }
            };

            var consulta = new ConsultaSql<HitoDtm>(contexto, leerHitosPosteriores);
            var registros = consulta.LanzarConsulta(new DynamicParameters(parametrosSql));

            return registros;
        }

        public static List<HitoDtm> LeerHistoriaDelElemento(ContextoSe contexto, string esquemaTabla, int idElemento)
        {
            var leerPorIdElemento = $@"
                     select {_campos}
                     , t2.{ICampos.LOGIN} as {nameof(HitoDtm.Usuario)}
                     , t3.{ICampos.NOMBRE} as {nameof(HitoDtm.Estado)}
                     , t4.{ICampos.NOMBRE} as {nameof(HitoDtm.Transicion)}
                     , t5.{ICampos.NOMBRE} as {nameof(HitoDtm.Observacion)}
                     from {esquemaTabla} T1 
                     inner join {ApiDeRegistroDtm.EsquemaTabla(typeof(UsuarioDtm))} t2 on T1.{ICampos.ID_USUARIO} = T2.{ICampos.ID}
                     inner join {esquemaTabla.Replace(Sufijo.HISTORIA, Sufijo.ESTADO)} t3 on T1.{ICampos.ID_ESTADO} = T3.{ICampos.ID}
                     left join {esquemaTabla.Replace(Sufijo.HISTORIA, Sufijo.TRANSICION)} t4 on T1.{ICampos.ID_TRANSICION} = T4.{ICampos.ID}
                     left join {esquemaTabla.Replace(Sufijo.HISTORIA, Sufijo.OBSERVACION)} t5 on T1.{ICampos.ID_OBSERVACION} = T5.{ICampos.ID}
                     where t1.{ICampos.ID_ELEMENTO} = @{ICampos.ID_ELEMENTO}
                     order by t1.id desc
                     ";

            var parametrosSql = new Dictionary<string, object>
            {
                { $"@{ICampos.ID_ELEMENTO}", idElemento }
            };

            var consulta = new ConsultaSql<HitoDtm>(contexto, leerPorIdElemento);
            var registros = consulta.LanzarConsulta(new DynamicParameters(parametrosSql));

            if (registros.Count == 0)
                GestorDeErrores.Emitir($"No se ha localizado la historia del elemento: {idElemento} en la tabla {esquemaTabla}");

            return registros;
        }

        public static List<HitoDtm> LeerUltimoHito(ContextoSe contexto, string esquemaTabla, int idElemento)
        {
            var leerPorIdElemento = $@"
                     select {_campos}
                     from {esquemaTabla} T1 
                     where t1.{ICampos.ID_ELEMENTO} = @{ICampos.ID_ELEMENTO}
                       and t1.{ICampos.ID_TRANSICION} is null
                     order by t1.id desc
                     ";

            var parametrosSql = new Dictionary<string, object>
            {
                { $"@{ICampos.ID_ELEMENTO}", idElemento }
            };

            var consulta = new ConsultaSql<HitoDtm>(contexto, leerPorIdElemento);
            var registros = consulta.LanzarConsulta(new DynamicParameters(parametrosSql));

            if (registros.Count == 0)
                GestorDeErrores.Emitir($"No se ha localizado la historia del elemento: {idElemento} en la tabla {esquemaTabla}");

            return registros;
        }

        public static HitoDtm LeerHitoAnteriorA(ContextoSe contexto, string esquemaTabla, HitoDtm hito, bool errorSiNoHay)
        {
            var leerAnteriorA = $@"
                     select top(1) {_campos}
                     , t3.{ICampos.NOMBRE} as {nameof(HitoDtm.Estado)}
                     from {esquemaTabla} T1 
                     inner join {esquemaTabla.Replace(Sufijo.HISTORIA, Sufijo.ESTADO)} t3 on T1.{ICampos.ID_ESTADO} = T3.{ICampos.ID}
                     where t1.{ICampos.ID_ELEMENTO} = @{ICampos.ID_ELEMENTO}
                       and t1.{ICampos.FECHA} < @{ICampos.FECHA}
                     order by t1.{ICampos.FECHA} desc
                     ";

            var parametrosSql = new Dictionary<string, object>
            {
                { $"@{ICampos.ID_ELEMENTO}", hito.IdElemento },
                { $"@{ICampos.FECHA}", hito.Fecha }
            };

            var consulta = new ConsultaSql<HitoDtm>(contexto, leerAnteriorA);
            var registros = consulta.LanzarConsulta(new DynamicParameters(parametrosSql));

            if (registros.Count == 0 && errorSiNoHay)
                GestorDeErrores.Emitir($"No hay hito anterior al '{hito.Elemento}' cuando este tenía el estado '{hito.Estado}'");

            return registros.Count == 0 ? null : registros[0];
        }

        public static int ContarRegistros(ContextoSe contexto, string esquemaTabla, int idElemento)
        {
            var contarRegistros = $@"
                     select count(*)
                     from {esquemaTabla} T1 
                     where t1.{ICampos.ID_ELEMENTO} = @{ICampos.ID_ELEMENTO}
                     ";
            var parametrosSql = new Dictionary<string, object>();
            parametrosSql.Add($"@{ICampos.ID_ELEMENTO}", idElemento);
            var consulta = new ConsultaSql<RegistrosAfectados>(contexto, contarRegistros);
            var registros = consulta.LanzarConsulta(new DynamicParameters(parametrosSql));

            return registros[0].cantidad;
        }

        public static RegistroDtm Insertar(ContextoSe contexto, string esquemaTabla, HitoDtm historia)
        {
            var sentencia = $@"Insert into {esquemaTabla} (  
                                        {ICampos.ID_ELEMENTO}   
                                       ,{ICampos.ID_ESTADO}     
                                       ,{ICampos.ID_USUARIO}    
                                       ,{ICampos.FECHA}      
                                       )   
                               values (        
                                         @{ICampos.ID_ELEMENTO}   
                                       , @{ICampos.ID_ESTADO}     
                                       , @{ICampos.ID_USUARIO}        
                                       , @{ICampos.FECHA}         
                                      )
                               SELECT SCOPE_IDENTITY() as {nameof(RegistroDtm.Id)}
             ";

            var parametrosSql = new Dictionary<string, object>();
            AsignarParametros(historia, parametrosSql);

            var consulta = new ConsultaSql<RegistroDtm>(contexto, sentencia);
            var registros = consulta.LanzarConsulta(new DynamicParameters(parametrosSql))[0];
            return registros;
        }

        public static void Modificar(ContextoSe contexto, string esquemaTabla, HitoDtm historia, DateTime fechafin)
        {
            var sentencia = $@"update {esquemaTabla} 
                               set {ICampos.TIEMPO} = @{ICampos.TIEMPO} 
                                  ,{ICampos.ID_TRANSICION} = @{ICampos.ID_TRANSICION} 
                                  ,{ICampos.ID_OBSERVACION} = @{ICampos.ID_OBSERVACION} 
                               where {ICampos.ID} = @{ICampos.ID}";
            if (fechafin == DateTime.MinValue)
                fechafin = DateTime.Now;
            //{ $"@{ICampos.TIEMPO}", DateTime.Now.Ticks - historia.Fecha.Ticks },
            var parametrosSql = new Dictionary<string, object>
            {
                { $"@{ICampos.ID}", historia.Id },
                { $"@{ICampos.TIEMPO}",fechafin.Ticks - historia.Fecha.Ticks },
                { $"@{ICampos.ID_TRANSICION}", historia.IdTransicion },
                { $"@{ICampos.ID_OBSERVACION}", historia.IdObservacion }
            };

            var consulta = new ConsultaSql<HitoDtm>(contexto, sentencia);
            consulta.EjecutarSentencia(new DynamicParameters(parametrosSql));
        }

        private static void AsignarParametros(HitoDtm historia, Dictionary<string, object> parametrosSql)
        {
            parametrosSql.Add($"@{ICampos.ID_ELEMENTO}", historia.IdElemento);
            parametrosSql.Add($"@{ICampos.ID_ESTADO}", historia.IdEstado);
            parametrosSql.Add($"@{ICampos.ID_USUARIO}", historia.IdUsuario);
            parametrosSql.Add($"@{ICampos.FECHA}", historia.Fecha == default ? DateTime.Now : historia.Fecha);
        }



        /*
        private static readonly string _todos = $@" 
                            {_campos}
                          , t2.{ICampos.NOMBRE}             as {nameof(HistoriaDtm.Transicion)}
                          , t3.{ICampos.NOMBRE}             as {nameof(HistoriaDtm.Observacion)}
                          , t4.{ICampos.NOMBRE}             as {nameof(HistoriaDtm.Estado)}
                          , t5.{ICampos.NOMBRE}             as {nameof(HistoriaDtm.Elemento)}
                          , t6.{ICampos.NOMBRE}             as {nameof(HistoriaDtm.Usuario)}
                     ";

        public static List<HistoriaDtm> LeerLaHistoria(ContextoSe contexto, string esquemaTabla, int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden)
        {
            var tablaDeEstado = esquemaTabla.Replace(Sufijo.HISTORIA, Sufijo.ESTADO);
            var tablaDeTransicion = esquemaTabla.Replace(Sufijo.TRANSICION, Sufijo.ESTADO);
            var tablaDeObservacion = esquemaTabla.Replace(Sufijo.O, Sufijo.ESTADO);
            var leerTransiciones = $@"
                     select {_todos}
                     from {esquemaTabla}  T1 
                     inner join {TablaDeEstado} t2 on t2.{ICampos.ID} = t1.{ICampos.ID_ORIGEN}
                     inner join {TablaDeEstado} t3 on t3.{ICampos.ID} = t1.{ICampos.ID_DESTINO}
                     inner join {ApiDeRegistroDtm.EsquemaTabla(typeof(PermisoDtm))} t4 on t4.{ICampos.ID} = t1.{ICampos.ID_PERMISO}
                     [where]
                     [orden]
                     ";

            var parametrosSql = new Dictionary<string, object>();
            var clausulaWhere = DefinirClausulaWhere(filtros, parametrosSql);
            var clausulaOrder = DefinirClausulaOrden(orden);
            leerTransiciones = leerTransiciones.Replace("[where]", clausulaWhere);
            leerTransiciones = leerTransiciones.Replace("[orden]", clausulaOrder);

            if (cantidad > 0)
            {
                leerTransiciones = $@"{leerTransiciones}OFFSET @posicion ROWS FETCH NEXT @cantidad ROWS ONLY";
                parametrosSql.Add($"@posicion", posicion);
                parametrosSql.Add($"@cantidad", cantidad);
            }

            var consulta = new ConsultaSql<HistoriaDtm>(contexto, leerTransiciones);
            var registros = consulta.LanzarConsulta(new DynamicParameters(parametrosSql));

            return registros;
        }

        public static int ContarRegistros(ContextoSe contexto, string esquemaTabla, List<ClausulaDeFiltrado> filtros)
        {
            var contarRegistros = $@"
               select Count(*) as cantidad
               from {esquemaTabla} T1 
               [where]
               ";
            var parametrosSql = new Dictionary<string, object>();
            var clausulaWhere = DefinirClausulaWhere(filtros, parametrosSql);
            contarRegistros = contarRegistros.Replace("[where]", clausulaWhere);
            var consulta = new ConsultaSql<RegistrosAfectados>(contexto, contarRegistros);
            var registros = consulta.LanzarConsulta(new DynamicParameters(parametrosSql));

            return registros[0].cantidad;
        }

        private static string DefinirClausulaOrden(List<ClausulaDeOrdenacion> orden)
        {
            if (orden.Count == 0)
                return $"Order by T2.{ICampos.ID} ASC, T1.{ICampos.NOMBRE} ASC, T3.{ICampos.ID} ASC";

            var clausula = ConsultaSql<EstadoDtm>.DefinirClausulaOrden(orden);

            if (orden.Count > 1 || clausula.IsNullOrEmpty()) foreach (var ordenarPor in orden)
                {

                    if (ordenarPor.OrdenarPor.Equals(nameof(HistoriaDtm.Origen), StringComparison.CurrentCultureIgnoreCase))
                        clausula = $"{(clausula.IsNullOrEmpty() ? "" : $"{clausula},")} T2.{ICampos.ORDEN} {ordenarPor.ModoBd}";

                    if (ordenarPor.OrdenarPor.Equals(nameof(HistoriaDtm.Destino), StringComparison.CurrentCultureIgnoreCase))
                        clausula = $"{(clausula.IsNullOrEmpty() ? "" : $"{clausula},")} T3.{ICampos.ORDEN} {ordenarPor.ModoBd}";

                    if (ordenarPor.OrdenarPor.Equals(nameof(HistoriaDtm.Nombre), StringComparison.CurrentCultureIgnoreCase))
                        clausula = $"{(clausula.IsNullOrEmpty() ? "" : $"{clausula},")} T1.{ICampos.NOMBRE} {ordenarPor.ModoBd}";
                }

            return $"Order by {clausula}";
        }

        private static string DefinirClausulaWhere(List<ClausulaDeFiltrado> filtros, Dictionary<string, object> parametrosSql)
        {
            var where = ConsultaSql<HistoriaDtm>.DefinirClausulaWhere(filtros, parametrosSql);
            foreach (var filtro in filtros)
            {
                if (filtro.Aplicado) continue;
                if (filtro.Clausula.Equals(nameof(HistoriaDtm.Origen), StringComparison.CurrentCultureIgnoreCase))
                    where = FiltroPorOrigen(where, parametrosSql, filtro);

                if (filtro.Clausula.Equals(nameof(HistoriaDtm.Destino), StringComparison.CurrentCultureIgnoreCase))
                    where = FiltroPorDestino(where, parametrosSql, filtro);

                if (filtro.Clausula.Equals($"{nameof(EstadoDtm)}", StringComparison.CurrentCultureIgnoreCase))
                    where = FiltroPorEstado(where, parametrosSql, filtro);

                if (filtro.Clausula.Equals($"{nameof(HistoriaDtm)}", StringComparison.CurrentCultureIgnoreCase))
                    where = FiltroPorTransicion(where, parametrosSql, filtro);
            }
            return where;
        }

        public static string FiltroPorOrigen(string clausulaWhere, Dictionary<string, object> parametrosSql, ClausulaDeFiltrado filtro)
        {
            clausulaWhere = $"{clausulaWhere}{Environment.NewLine}and T1.{ICampos.ID_ORIGEN} = @{ICampos.ID_ORIGEN}";
            parametrosSql.Add($"@{ICampos.ID_ORIGEN}", filtro.Valor);
            filtro.Aplicado = true;
            return clausulaWhere;
        }
        public static string FiltroPorDestino(string clausulaWhere, Dictionary<string, object> parametrosSql, ClausulaDeFiltrado filtro)
        {
            clausulaWhere = $"{clausulaWhere}{Environment.NewLine}and T1.{ICampos.ID_DESTINO} = @{ICampos.ID_DESTINO}";
            parametrosSql.Add($"@{ICampos.ID_DESTINO}", filtro.Valor);
            filtro.Aplicado = true;
            return clausulaWhere;
        }
        public static string FiltroPorEstado(string clausulaWhere, Dictionary<string, object> parametrosSql, ClausulaDeFiltrado filtro)
        {
            clausulaWhere = $"{clausulaWhere}{Environment.NewLine}and (T1.{ICampos.ID_DESTINO} = @{ICampos.ID_DESTINO} or T1.{ICampos.ID_ORIGEN} = @{ICampos.ID_ORIGEN})";
            parametrosSql.Add($"@{ICampos.ID_DESTINO}", filtro.Valor);
            parametrosSql.Add($"@{ICampos.ID_ORIGEN}", filtro.Valor);
            filtro.Aplicado = true;
            return clausulaWhere;
        }
        public static string FiltroPorTransicion(string clausulaWhere, Dictionary<string, object> parametrosSql, ClausulaDeFiltrado filtro)
        {
            var idEstados = filtro.Valor.Split(";");

            //if (idEstados.Length != 2)
            //    GestorDeErrores.Emitir($"El filtro en transiciones por {nameof(HistoriaDtm)} debe ir con los valores: Id_Origen;Id_Destino");

            clausulaWhere = $"{clausulaWhere}{Environment.NewLine}and (T1.{ICampos.ID_DESTINO} = @{ICampos.ID_DESTINO} or T1.{ICampos.ID_ORIGEN} = @{ICampos.ID_ORIGEN})";
            parametrosSql.Add($"@{ICampos.ID_DESTINO}", idEstados.Length == 2 ? idEstados[1] : idEstados[0]);
            parametrosSql.Add($"@{ICampos.ID_ORIGEN}", idEstados[0]);
            filtro.Aplicado = true;
            return clausulaWhere;
        }
        */

    }


}