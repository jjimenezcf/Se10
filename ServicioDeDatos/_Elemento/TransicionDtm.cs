using Dapper;
using Gestor.Errores;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ServicioDeDatos.Seguridad;
using System;
using System.Collections.Generic;
using Utilidades;

namespace ServicioDeDatos.Elemento
{
    public class TransicionAuditada
    {
        public int idTransicion;
        public string fechas;
    }

    public class TransicionAplicable
    {
        public int IdEstado { get; set; }
        public int IdTransicion { get; set; }
        public string Transicion { get; set; }
        public int IdEstadoDestino { get; set; }
        public bool PorDefecto { get; set; }

        public static List<TransicionAplicable> Transiciones(string transicionesPorMotivoJson, Enum motivo, bool errorSiNoHay)
        {
            var transicionesAplicables = new List<TransicionAplicable>();
            var transicionesPosibles = JsonConvert.DeserializeObject<List<TransicionPorMotivo>>(transicionesPorMotivoJson);
            foreach (var transicion in transicionesPosibles)
            {
                if (transicion.Motivo == motivo.ToString())
                {
                    transicionesAplicables.Add(new TransicionAplicable { IdEstado = transicion.IdEstado, IdTransicion = transicion.IdTransicion });
                }
            }

            if (transicionesAplicables.Count == 0 && errorSiNoHay)
            {
                GestorDeErrores.Emitir($"No hay transiciones aplicables para el motivo '{motivo}' definidas en el parámetro 'TransicionesPorMotivo'");
            }

            return transicionesAplicables;
        }
    }

    public class TransicionPorMotivo
    {
        public string Motivo { get; set; }
        public int IdEstado { get; set; }
        public int IdTransicion { get; set; }
    }

    public class ltrTransiciones
    {
        public static readonly string IdTransicionAuditada = nameof(IdTransicionAuditada).ToLower();
        public static readonly string FechaTransicionAuditado = nameof(FechaTransicionAuditado).ToLower();
        public const string transiciones = nameof(transiciones);
        public const string Usuarios = nameof(Usuarios);
        public const string filtroPorEstados = nameof(filtroPorEstados);
        public const string filtroOrPorIdDeEstado = nameof(filtroOrPorIdDeEstado);
        public const string aplicarPermisos = nameof(aplicarPermisos);
        public const string EtapasOrigen = nameof(EtapasOrigen);
        public const string EtapasDestino = nameof(EtapasDestino);
        public const string ErroSiMasDeUno = nameof(ErroSiMasDeUno);
    }


    public class TransicionDtm : RegistroConNombreDtm, ITransicion
    {
        public int IdOrigen { get; set; }
        public int IdDestino { get; set; }
        public bool DelSistema { get; set; }
        public bool ConObservacion { get; set; }
        public bool PorDefecto { get; set; }
        public string Asunto { get; set; }
        public int IdPermiso { get; set; }
        public bool Activo { get; set; }
        public string Origen { get; set; }
        public string Destino { get; set; }
        public string Permiso { get; set; }
        public bool EsInicial { get; }
        public bool EsTerminado { get; }
        public bool EsCancelado { get; }
    }

    public static class ApiDeTransicion
    {
        public static string TablaDeTransiciones(Type t) => $"{ApiDeRegistroDtm.EsquemaDeTabla(t)}.{ApiDeRegistroDtm.NombreDeTabla(t)}_{Sufijo.TRANSICION}";

        internal static void DefinirCampos<TEntity, TEstado>(ModelBuilder modelBuilder)
        where TEntity : TransicionDtm
        where TEstado : EstadoDtm
        {
            var nombreDeTabla = ApiDeRegistroDtm.NombreDeTabla(typeof(TEntity));

            ApiDeRegistroDtm.DefinirCampoIdDtm<TEntity>(modelBuilder);
            ApiDeNombreDtm.DefinirCampoNombreDtm<TEntity>(modelBuilder, 250, "", false, true);

            modelBuilder.Entity<TEntity>().Property(x => x.IdOrigen).HasColumnType(IDominio.INT).HasColumnName(ICampos.ID_ORIGEN).IsRequired();
            modelBuilder.Entity<TEntity>().Property(x => x.IdDestino).HasColumnType(IDominio.INT).HasColumnName(ICampos.ID_DESTINO).IsRequired();
            modelBuilder.Entity<TEntity>().Property(x => x.IdPermiso).HasColumnType(IDominio.INT).HasColumnName(ICampos.ID_PERMISO).IsRequired();

            modelBuilder.Entity<TEntity>().Property(p => p.DelSistema).HasColumnName(ICampos.DEL_SISTEMA).HasColumnType(IDominio.BIT).IsRequired();
            modelBuilder.Entity<TEntity>().Property(p => p.ConObservacion).HasColumnName(ICampos.CON_OBSERVACION).HasColumnType(IDominio.BIT).IsRequired();
            modelBuilder.Entity<TEntity>().Property(p => p.PorDefecto).HasColumnName(ICampos.POR_DEFECCTO).HasColumnType(IDominio.BIT).IsRequired().HasDefaultValue(false);
            modelBuilder.Entity<TEntity>().Property(p => p.Asunto).HasColumnName(ICampos.ASUNTO).HasColumnType(IDominio.VARCHAR_250).IsRequired(false);
            modelBuilder.Entity<TEntity>().Property(p => p.Activo).HasColumnName(ICampos.ACTIVO).HasColumnType(IDominio.BIT).IsRequired();


            ApiDeRegistroDtm.DefinirFk<TEntity, TEstado>(modelBuilder, nameof(TransicionDtm.IdOrigen), ICampos.ID_ORIGEN, unico: false);
            ApiDeRegistroDtm.DefinirFk<TEntity, TEstado>(modelBuilder, nameof(TransicionDtm.IdDestino), ICampos.ID_DESTINO, unico: false);
            ApiDeRegistroDtm.DefinirFk<TEntity, PermisoDtm>(modelBuilder, nameof(TransicionDtm.IdPermiso), ICampos.ID_PERMISO, true);


            modelBuilder.Entity<TEntity>().HasIndex(x => new { x.Nombre, x.IdOrigen, x.IdDestino }).HasDatabaseName($"I_{nombreDeTabla}_{ICampos.NOMBRE}_{ICampos.ID_ORIGEN}_{ICampos.ID_DESTINO}").IsUnique();


            modelBuilder.Entity<TEntity>().Ignore(p => p.Destino);
            modelBuilder.Entity<TEntity>().Ignore(p => p.Origen);
            modelBuilder.Entity<TEntity>().Ignore(p => p.Permiso);
        }

    }

    public class TransicionSql
    {
        private static readonly string _Campos = $@"
                            t1.{ICampos.ID}              as {nameof(TransicionDtm.Id)}
                          , t1.{ICampos.NOMBRE}          as {nameof(TransicionDtm.Nombre)}
                          , t1.{ICampos.ID_ORIGEN}       as {nameof(TransicionDtm.IdOrigen)}
                          , t1.{ICampos.ID_DESTINO}      as {nameof(TransicionDtm.IdDestino)}
                          , t1.{ICampos.ID_PERMISO}      as {nameof(TransicionDtm.IdPermiso)}
                          , t1.{ICampos.CON_OBSERVACION} as {nameof(TransicionDtm.ConObservacion)}
                          , t1.{ICampos.POR_DEFECCTO}    as {nameof(TransicionDtm.PorDefecto)}
                          , t1.{ICampos.DEL_SISTEMA}     as {nameof(TransicionDtm.DelSistema)}
                          , t1.{ICampos.ASUNTO}          as {nameof(TransicionDtm.Asunto)}
                          , t1.{ICampos.ACTIVO}          as {nameof(TransicionDtm.Activo)}
                          , t2.{ICampos.NOMBRE}          as {nameof(TransicionDtm.Origen)}
                          , t3.{ICampos.NOMBRE}          as {nameof(TransicionDtm.Destino)}
                          , t2.{ICampos.INICIAL}         as {nameof(TransicionDtm.EsInicial)}
                          , t3.{ICampos.TERMINADO}       as {nameof(TransicionDtm.EsTerminado)}
                          , t3.{ICampos.CANCELADO}       as {nameof(TransicionDtm.EsCancelado)}
                          , t4.{ICampos.NOMBRE}          as {nameof(TransicionDtm.Permiso)}
                     ";

        public static TransicionDtm LeerTransicionPorId(ContextoSe contexto, string tablaDeTransiciones, int id)
        {
            var TablaDeEstado = tablaDeTransiciones.Replace(Sufijo.TRANSICION, Sufijo.ESTADO);
            var leerPorId = $@"
                     select {_Campos}
                     from {tablaDeTransiciones} T1 
                     inner join {TablaDeEstado} t2 on t2.{ICampos.ID} = t1.{ICampos.ID_ORIGEN}
                     inner join {TablaDeEstado} t3 on t3.{ICampos.ID} = t1.{ICampos.ID_DESTINO}
                     inner join {ApiDeRegistroDtm.EsquemaTabla(typeof(PermisoDtm))} t4 on t4.{ICampos.ID} = t1.{ICampos.ID_PERMISO}
                     where t1.{ICampos.ID} = @{ICampos.ID}
                     ";

            leerPorId = leerPorId.Replace("[TablaEstado]", TablaDeEstado);

            var cache = ServicioDeCaches.Obtener(nameof(LeerTransicionPorId));
            var clave = $"{tablaDeTransiciones}.{id}";
            if (!cache.ContainsKey(clave))
            {
                var parametrosSql = new Dictionary<string, object>
                {
                    { $"@{ICampos.ID}", id }
                };

                var consulta = new ConsultaSql<TransicionDtm>(contexto, leerPorId);
                var registros = consulta.LanzarConsulta(new DynamicParameters(parametrosSql));

                if (registros.Count == 0)
                    GestorDeErrores.Emitir($"No se ha localizado la transición con Id: {id} en la tabla {tablaDeTransiciones}");

                cache[clave] = registros[0];
            }

            return (TransicionDtm)cache[clave];
        }

        public static List<TransicionDtm> LeerTransiciones(ContextoSe contexto, string tablaDeTransiciones, int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden)
        {
            var TablaDeEstado = tablaDeTransiciones.Replace(Sufijo.TRANSICION, Sufijo.ESTADO);
            var leerTransiciones = $@"
                     select {_Campos}
                     from {tablaDeTransiciones}  T1 
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

            var consulta = new ConsultaSql<TransicionDtm>(contexto, leerTransiciones);
            var registros = consulta.LanzarConsulta(new DynamicParameters(parametrosSql));

            return registros;
        }


        public static int ContarRegistros(ContextoSe contexto, string tablaDeTransiciones, List<ClausulaDeFiltrado> filtros)
        {
            var contarRegistros = $@"
               select Count(*) as cantidad
               from {tablaDeTransiciones} T1 
               [where]
               ";
            var parametrosSql = new Dictionary<string, object>();
            var clausulaWhere = DefinirClausulaWhere(filtros, parametrosSql);
            contarRegistros = contarRegistros.Replace("[where]", clausulaWhere);
            var consulta = new ConsultaSql<RegistrosAfectados>(contexto, contarRegistros);
            var registros = consulta.LanzarConsulta(new DynamicParameters(parametrosSql));

            return registros[0].cantidad;
        }

        public static RegistroDtm Insertar(ContextoSe contexto, string tablaDeTransiciones, TransicionDtm Transicion)
        {
            var sentencia = $@"Insert into {tablaDeTransiciones} (  
                                         {ICampos.NOMBRE} 
                                       , {ICampos.ID_ORIGEN} 
                                       , {ICampos.ID_DESTINO}     
                                       , {ICampos.ID_PERMISO}
                                       , {ICampos.DEL_SISTEMA} 
                                       , {ICampos.CON_OBSERVACION} 
                                       , {ICampos.POR_DEFECCTO} 
                                       , {ICampos.ASUNTO} 
                                       , {ICampos.ACTIVO} 
                                       )
                               values (        
                                         @{ICampos.NOMBRE} 
                                       , @{ICampos.ID_ORIGEN}
                                       , @{ICampos.ID_DESTINO}
                                       , @{ICampos.ID_PERMISO} 
                                       , @{ICampos.DEL_SISTEMA}     
                                       , @{ICampos.CON_OBSERVACION}
                                       , @{ICampos.POR_DEFECCTO} 
                                       , @{ICampos.ASUNTO}
                                       , @{ICampos.ACTIVO}
                                      )
                               SELECT SCOPE_IDENTITY() as {nameof(RegistroDtm.Id)}
             ";

            var parametrosSql = new Dictionary<string, object>();
            AsignarParametros(Transicion, parametrosSql);

            var consulta = new ConsultaSql<RegistroDtm>(contexto, sentencia);
            var registros = consulta.LanzarConsulta(new DynamicParameters(parametrosSql))[0];
            return registros;
        }

        public static void Modificar(ContextoSe contexto, string tablaDeTransiciones, TransicionDtm Transicion)
        {
            var sentencia = $@"update {tablaDeTransiciones} set 
                                         {ICampos.NOMBRE} = @{ICampos.NOMBRE} 
                                       , {ICampos.ID_ORIGEN} = @{ICampos.ID_ORIGEN}   
                                       , {ICampos.ID_DESTINO} = @{ICampos.ID_DESTINO}   
                                       , {ICampos.ID_PERMISO} = @{ICampos.ID_PERMISO}     
                                       , {ICampos.DEL_SISTEMA} = @{ICampos.DEL_SISTEMA}
                                       , {ICampos.CON_OBSERVACION} = @{ICampos.CON_OBSERVACION}
                                       , {ICampos.POR_DEFECCTO} = @{ICampos.POR_DEFECCTO}
                                       , {ICampos.ASUNTO} = @{ICampos.ASUNTO}  
                                       , {ICampos.ACTIVO} = @{ICampos.ACTIVO}  
                               where {ICampos.ID} = @{ICampos.ID}";

            var parametrosSql = new Dictionary<string, object>
            {
                { $"@{ICampos.ID}", Transicion.Id }
            };
            AsignarParametros(Transicion, parametrosSql);

            var consulta = new ConsultaSql<TransicionDtm>(contexto, sentencia);
            consulta.EjecutarSentencia(new DynamicParameters(parametrosSql));
        }


        public static void FijarPorDefecto(ContextoSe contexto, string tablaDeTransiciones, TransicionDtm Transicion)
        {
            var sentencia = $@"update {tablaDeTransiciones} set {ICampos.POR_DEFECCTO} = @{ICampos.POR_DEFECCTO}
                               where {ICampos.ID} <> @{ICampos.ID} and {ICampos.ID_ORIGEN} = @{ICampos.ID_ORIGEN} ";

            var parametrosSql = new Dictionary<string, object>
            {
                { $"@{ICampos.ID}", Transicion.Id },
                { $"@{ICampos.ID_ORIGEN}", Transicion.IdOrigen },
                { $"@{ICampos.POR_DEFECCTO}", false }
            };

            var consulta = new ConsultaSql<TransicionDtm>(contexto, sentencia);
            consulta.EjecutarSentencia(new DynamicParameters(parametrosSql));
        }


        public static void Borrar(ContextoSe contexto, string tablaDeTransiciones, TransicionDtm Transicion)
        {
            var sentencia = $@"Delete 
                               from {tablaDeTransiciones} 
                               where {ICampos.ID} = @{ICampos.ID}";

            var parametrosSql = new Dictionary<string, object>();
            parametrosSql.Add($"@{ICampos.ID}", Transicion.Id);

            var consulta = new ConsultaSql<TransicionDtm>(contexto, sentencia);
            consulta.EjecutarSentencia(new DynamicParameters(parametrosSql));
        }

        private static void AsignarParametros(TransicionDtm Transicion, Dictionary<string, object> parametrosSql)
        {
            parametrosSql.Add($"@{ICampos.NOMBRE}", Transicion.Nombre);
            parametrosSql.Add($"@{ICampos.ID_ORIGEN}", Transicion.IdOrigen);
            parametrosSql.Add($"@{ICampos.ID_DESTINO}", Transicion.IdDestino);
            parametrosSql.Add($"@{ICampos.ID_PERMISO}", Transicion.IdPermiso);
            parametrosSql.Add($"@{ICampos.DEL_SISTEMA}", Transicion.DelSistema);
            parametrosSql.Add($"@{ICampos.CON_OBSERVACION}", Transicion.ConObservacion);
            parametrosSql.Add($"@{ICampos.POR_DEFECCTO}", Transicion.PorDefecto);
            parametrosSql.Add($"@{ICampos.ASUNTO}", Transicion.Asunto);
            parametrosSql.Add($"@{ICampos.ACTIVO}", Transicion.Activo);
        }

        private static string DefinirClausulaOrden(List<ClausulaDeOrdenacion> orden)
        {
            if (orden.Count == 0)
                return $"Order by T2.{ICampos.ID} ASC, T1.{ICampos.NOMBRE} ASC, T3.{ICampos.ID} ASC";

            var clausula = ConsultaSql<EstadoDtm>.DefinirClausulaOrden(orden);

            if (orden.Count > 1 || clausula.IsNullOrEmpty()) foreach (var ordenarPor in orden)
                {

                    if (ordenarPor.OrdenarPor.Equals(nameof(TransicionDtm.Origen), StringComparison.CurrentCultureIgnoreCase))
                        clausula = $"{(clausula.IsNullOrEmpty() ? "" : $"{clausula},")} T2.{ICampos.ORDEN} {ordenarPor.ModoBd}";

                    if (ordenarPor.OrdenarPor.Equals(nameof(TransicionDtm.Destino), StringComparison.CurrentCultureIgnoreCase))
                        clausula = $"{(clausula.IsNullOrEmpty() ? "" : $"{clausula},")} T3.{ICampos.ORDEN} {ordenarPor.ModoBd}";

                    if (ordenarPor.OrdenarPor.Equals(nameof(TransicionDtm.Nombre), StringComparison.CurrentCultureIgnoreCase))
                        clausula = $"{(clausula.IsNullOrEmpty() ? "" : $"{clausula},")} T1.{ICampos.NOMBRE} {ordenarPor.ModoBd}";
                }

            return $"Order by {clausula}";
        }

        private static string DefinirClausulaWhere(List<ClausulaDeFiltrado> filtros, Dictionary<string, object> parametrosSql)
        {
            var where = ConsultaSql<TransicionDtm>.DefinirClausulaWhere(filtros, parametrosSql);
            foreach (var filtro in filtros)
            {
                if (filtro.Aplicado) continue;

                if (filtro.Clausula.Equals(nameof(TransicionDtm.Activo), StringComparison.CurrentCultureIgnoreCase))
                {
                    where = FiltroPorActivo(where, parametrosSql, filtro);
                    continue;
                }
                if (filtro.Clausula.Equals(nameof(TransicionDtm.DelSistema), StringComparison.CurrentCultureIgnoreCase))
                {
                    where = FiltroPorDelSistema(where, parametrosSql, filtro);
                    continue;
                }

                if (filtro.Clausula.Equals(nameof(TransicionDtm.ConObservacion), StringComparison.CurrentCultureIgnoreCase))
                {
                    where = FiltroPorConObservacion(where, parametrosSql, filtro);
                    continue;
                }
                if (filtro.Clausula.Equals(nameof(TransicionDtm.PorDefecto), StringComparison.CurrentCultureIgnoreCase))
                {
                    where = FiltroPorDefecto(where, parametrosSql, filtro);
                    continue;
                }
                if (filtro.Clausula.Equals(nameof(TransicionDtm.Origen), StringComparison.CurrentCultureIgnoreCase)  && !filtro.Valor.EsEntero())
                {
                    where = FiltroPorNombreDeEstadoOrigen(where, parametrosSql, filtro.Valor);
                    filtro.Aplicado= true;
                    continue;
                }

                if (filtro.Clausula.Equals(nameof(TransicionDtm.Destino), StringComparison.CurrentCultureIgnoreCase) && !filtro.Valor.EsEntero())
                {
                    where = FiltroPorNombreDeEstadoDestino(where, parametrosSql, filtro.Valor);
                    filtro.Aplicado = true;
                    continue;
                }

                if (filtro.Clausula.Equals(nameof(TransicionDtm.Origen), StringComparison.CurrentCultureIgnoreCase) || filtro.Clausula.Equals(nameof(TransicionDtm.IdOrigen), StringComparison.CurrentCultureIgnoreCase))
                {
                    where = FiltroPorIdOrigen(where, parametrosSql, filtro);
                    continue;
                }

                if (filtro.Clausula.Equals(nameof(TransicionDtm.Destino), StringComparison.CurrentCultureIgnoreCase) || filtro.Clausula.Equals(nameof(TransicionDtm.IdDestino), StringComparison.CurrentCultureIgnoreCase))
                {
                    where = FiltroPorIdDestino(where, parametrosSql, filtro);
                    continue;
                }

                if (filtro.Clausula.Equals($"{ltrTransiciones.filtroOrPorIdDeEstado}", StringComparison.CurrentCultureIgnoreCase))
                    where = FiltroPorIdDeEstado(where, parametrosSql, filtro);

                if (filtro.Clausula.Equals($"{ltrTransiciones.filtroPorEstados}", StringComparison.CurrentCultureIgnoreCase))
                    where = FiltroPorTransicion(where, parametrosSql, filtro);
            }
            return where;
        }


        private static string FiltroPorNombreDeEstadoDestino(string clausulaWhere, Dictionary<string, object> parametrosSql, string estadoDestino)
        {
            clausulaWhere = $"{clausulaWhere}{Environment.NewLine}and T3.{ICampos.NOMBRE} like '' + @{ICampos.DESTINO} + ''";
            parametrosSql.Add($"@{ICampos.DESTINO}", estadoDestino);
            return clausulaWhere;
        }

        private static string FiltroPorNombreDeEstadoOrigen(string clausulaWhere, Dictionary<string, object> parametrosSql, string estadoOrigen)
        {
            clausulaWhere = $"{clausulaWhere}{Environment.NewLine}and T2.{ICampos.NOMBRE} like '' + @{ICampos.ORIGEN} + ''";
            parametrosSql.Add($"@{ICampos.ORIGEN}", estadoOrigen);
            return clausulaWhere;
        }

        private static string FiltroPorActivo(string clausulaWhere, Dictionary<string, object> parametrosSql, ClausulaDeFiltrado filtro)
        {
            clausulaWhere = $"{clausulaWhere}{Environment.NewLine}and T1.{ICampos.ACTIVO} = @{ICampos.ACTIVO}";
            parametrosSql.Add($"@{ICampos.ACTIVO}", filtro.Valor);
            filtro.Aplicado = true;
            return clausulaWhere;
        }

        private static string FiltroPorDelSistema(string clausulaWhere, Dictionary<string, object> parametrosSql, ClausulaDeFiltrado filtro)
        {
            clausulaWhere = $"{clausulaWhere}{Environment.NewLine}and T1.{ICampos.DEL_SISTEMA} = @{ICampos.DEL_SISTEMA}";
            parametrosSql.Add($"@{ICampos.DEL_SISTEMA}", filtro.Valor);
            filtro.Aplicado = true;
            return clausulaWhere;
        }

        private static string FiltroPorConObservacion(string clausulaWhere, Dictionary<string, object> parametrosSql, ClausulaDeFiltrado filtro)
        {
            clausulaWhere = $"{clausulaWhere}{Environment.NewLine}and T1.{ICampos.CON_OBSERVACION} = @{ICampos.CON_OBSERVACION}";
            parametrosSql.Add($"@{ICampos.CON_OBSERVACION}", filtro.Valor);
            filtro.Aplicado = true;
            return clausulaWhere;
        }

        private static string FiltroPorDefecto(string clausulaWhere, Dictionary<string, object> parametrosSql, ClausulaDeFiltrado filtro)
        {
            clausulaWhere = $"{clausulaWhere}{Environment.NewLine}and T1.{ICampos.POR_DEFECCTO} = @{ICampos.POR_DEFECCTO}";
            parametrosSql.Add($"@{ICampos.POR_DEFECCTO}", filtro.Valor);
            filtro.Aplicado = true;
            return clausulaWhere;
        }


        private static string FiltroPorIdOrigen(string clausulaWhere, Dictionary<string, object> parametrosSql, ClausulaDeFiltrado filtro)
        {
            clausulaWhere = $"{clausulaWhere}{Environment.NewLine}and T1.{ICampos.ID_ORIGEN} = @{ICampos.ID_ORIGEN}";
            parametrosSql.Add($"@{ICampos.ID_ORIGEN}", filtro.Valor);
            filtro.Aplicado = true;
            return clausulaWhere;
        }

        private static string FiltroPorIdDestino(string clausulaWhere, Dictionary<string, object> parametrosSql, ClausulaDeFiltrado filtro)
        {
            clausulaWhere = $"{clausulaWhere}{Environment.NewLine}and T1.{ICampos.ID_DESTINO} = @{ICampos.ID_DESTINO}";
            parametrosSql.Add($"@{ICampos.ID_DESTINO}", filtro.Valor);
            filtro.Aplicado = true;
            return clausulaWhere;
        }

        private static string FiltroPorIdDeEstado(string clausulaWhere, Dictionary<string, object> parametrosSql, ClausulaDeFiltrado filtro)
        {
            clausulaWhere = $"{clausulaWhere}{Environment.NewLine}and (T1.{ICampos.ID_DESTINO} = @{ICampos.ID_DESTINO} or T1.{ICampos.ID_ORIGEN} = @{ICampos.ID_ORIGEN})";
            parametrosSql.Add($"@{ICampos.ID_DESTINO}", filtro.Valor);
            parametrosSql.Add($"@{ICampos.ID_ORIGEN}", filtro.Valor);
            filtro.Aplicado = true;
            return clausulaWhere;
        }

        private static string FiltroPorTransicion(string clausulaWhere, Dictionary<string, object> parametrosSql, ClausulaDeFiltrado filtro)
        {
            var estados = filtro.Valor.Split(Simbolos.PuntoComa);

            if (estados.Length != 2)
                GestorDeErrores.Emitir($"El filtro en transiciones por {nameof(TransicionDtm)} debe ir con los valores: Id_Origen;Id_Destino");

            filtro.Aplicado = true;
            if (estados[0].EsNumero() && estados[1].EsNumero())
            {
                clausulaWhere = $"{clausulaWhere}{Environment.NewLine}and (T1.{ICampos.ID_DESTINO} = @{ICampos.ID_DESTINO} and T1.{ICampos.ID_ORIGEN} = @{ICampos.ID_ORIGEN})";
                parametrosSql.Add($"@{ICampos.ID_DESTINO}", estados[1]);
                parametrosSql.Add($"@{ICampos.ID_ORIGEN}", estados[0]);
                return clausulaWhere;
            }

            clausulaWhere = FiltroPorNombreDeEstadoOrigen(clausulaWhere, parametrosSql, estados[0]);
            clausulaWhere = FiltroPorNombreDeEstadoDestino(clausulaWhere, parametrosSql, estados[1]);
            return clausulaWhere;
        }

    }


}
