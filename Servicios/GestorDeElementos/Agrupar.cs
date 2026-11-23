using Gestor.Errores;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection;
using Utilidades;

namespace GestorDeElementos
{
    // Calcula un agregado decimal sobre una lista de valores ya extraídos
    internal static class AgregadosHelper
    {
        internal static object Calcular(List<decimal> valores, enumOperacionDeTotales operacion)
        {
            if (!valores.Any()) return null;
            return operacion switch
            {
                enumOperacionDeTotales.Suma  => (object)valores.Sum(),
                enumOperacionDeTotales.Media => valores.Average(),
                enumOperacionDeTotales.Max   => valores.Max(),
                enumOperacionDeTotales.Min   => valores.Min(),
                _                            => null
            };
        }
    }

    // Versión dinámica de ObtenerTotalesAgrupados que trabaja sobre el tipo concreto en tiempo de ejecución.
    // Permite propiedades de subtipos (BaseImponible, Importe, IdResponsable…) y delega campos "calculado:" al caller.
    public static class AgruparDinamico
    {
        public static List<BloqueDeTotales> ObtenerTotalesConCalculados(
            this IEnumerable<ElementoDeProcesoDtm> registros,
            List<AgrupacionDeTotales> agrupaciones,
            Func<ElementoDeProcesoDtm, MetricaDeTotales, decimal?> resolverCalculado = null,
            Func<string, ElementoDeProcesoDtm, string> resolverClaveVirtual = null,
            Func<string, ElementoDeProcesoDtm, IEnumerable<string>> resolverClavesMultiples = null)
        {
            var resultado = new List<BloqueDeTotales>();

            foreach (var agrupacion in agrupaciones)
            {
                var bloque = new BloqueDeTotales
                {
                    PropiedadesDeAgrupacion = agrupacion.PropiedadesDeAgrupacion,
                    Filas = new List<FilaDeTotales>()
                };

                // Cada registro aporta una o varias combinaciones de claves: una si todas las claves son de un valor (propiedad del tipo real
                // o clave virtual) y varias si alguna es multivaluada (resolverClavesMultiples no devuelve null; p. ej. los artículos de una
                // factura). Un registro sin ningún valor en una clave multivaluada no cae en ningún grupo
                var entradas = registros
                    .SelectMany(r =>
                    {
                        var (combinaciones, multivaluadas) = CombinacionesDeClaves(r, agrupacion.PropiedadesDeAgrupacion, resolverClaveVirtual, resolverClavesMultiples);
                        return combinaciones.Select(claves => (registro: r, claves, multivaluadas));
                    })
                    .ToList();

                var grupos = entradas
                    .GroupBy(e => string.Join("|", agrupacion.PropiedadesDeAgrupacion.Select(prop => e.claves[prop]?.ToString() ?? string.Empty)))
                    .ToList();

                foreach (var grupo in grupos)
                {
                    var fila   = new FilaDeTotales();
                    var items  = grupo.Select(e => e.registro).Distinct().ToList();
                    var primero = items.FirstOrDefault();
                    var tipoReal = primero?.GetType() ?? typeof(ElementoDeProcesoDtm);
                    var primeraEntrada = grupo.First();

                    // Claves del grupo
                    foreach (var prop in agrupacion.PropiedadesDeAgrupacion)
                        fila.Claves[prop] = primeraEntrada.claves[prop];

                    // valor de las claves multivaluadas del grupo, para que los campos calculados devuelvan la parte de cada registro
                    var clavesDelGrupo = primeraEntrada.multivaluadas.Count == 0
                        ? null
                        : primeraEntrada.multivaluadas.ToDictionary(p => p, p => primeraEntrada.claves[p]?.ToString(), StringComparer.OrdinalIgnoreCase);

                    // Métricas
                    foreach (var metricaDeLaAgrupacion in agrupacion.Metricas)
                    {
                        var metrica = clavesDelGrupo == null
                            ? metricaDeLaAgrupacion
                            : new MetricaDeTotales(metricaDeLaAgrupacion.Operacion, metricaDeLaAgrupacion.Campo, metricaDeLaAgrupacion.Alias) { ClavesDelGrupo = clavesDelGrupo };
                        if (metrica.Operacion == enumOperacionDeTotales.Cuenta)
                        {
                            if (metrica.Campo.StartsWith("calculado:", StringComparison.OrdinalIgnoreCase) && resolverCalculado != null)
                                fila.Totales[metrica.Alias] = items.Count(r => { var v = resolverCalculado(r, metrica); return v.HasValue && v.Value != 0m; });
                            else
                                fila.Totales[metrica.Alias] = items.Count;
                            continue;
                        }

                        if (metrica.Campo.StartsWith("calculado:", StringComparison.OrdinalIgnoreCase))
                        {
                            if (resolverCalculado != null)
                            {
                                var vals = items.Select(r => resolverCalculado(r, metrica)).Where(v => v.HasValue).Select(v => v.Value).ToList();
                                fila.Totales[metrica.Alias] = AgregadosHelper.Calcular(vals, metrica.Operacion);
                            }
                            continue;
                        }

                        var propInfo = tipoReal.GetProperty(metrica.Campo, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                        if (propInfo != null)
                        {
                            var vals = items
                                .Select(r => { var v = propInfo.GetValue(r); return v == null ? (decimal?)null : Convert.ToDecimal(v); })
                                .Where(v => v.HasValue).Select(v => v.Value).ToList();
                            fila.Totales[metrica.Alias] = AgregadosHelper.Calcular(vals, metrica.Operacion);
                        }
                    }

                    bloque.Filas.Add(fila);
                }

                resultado.Add(bloque);
            }

            return resultado;
        }

        // Combinaciones de valores de las claves de agrupación de un registro: el producto de los valores de cada clave. Una propiedad del
        // tipo real o una clave virtual aporta un valor; una clave multivaluada (resolverClavesMultiples distinto de null) aporta sus valores
        // distintos, y si no tiene ninguno el registro no aporta combinaciones. Devuelve también qué claves son multivaluadas
        private static (List<Dictionary<string, object>> combinaciones, List<string> multivaluadas) CombinacionesDeClaves(
            ElementoDeProcesoDtm registro,
            List<string> propiedades,
            Func<string, ElementoDeProcesoDtm, string> resolverClaveVirtual,
            Func<string, ElementoDeProcesoDtm, IEnumerable<string>> resolverClavesMultiples)
        {
            var combinaciones = new List<Dictionary<string, object>> { new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase) };
            var multivaluadas = new List<string>();

            foreach (var prop in propiedades)
            {
                var valores = new List<object>();
                var pi = registro.GetType().GetProperty(prop, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (pi != null)
                    valores.Add(pi.GetValue(registro));
                else
                {
                    var multiples = resolverClavesMultiples?.Invoke(prop, registro);
                    if (multiples != null)
                    {
                        multivaluadas.Add(prop);
                        valores.AddRange(multiples.Where(v => v != null).Distinct());
                    }
                    else
                        valores.Add(resolverClaveVirtual?.Invoke(prop, registro));
                }

                combinaciones = combinaciones
                    .SelectMany(c => valores.Select(v => new Dictionary<string, object>(c, StringComparer.OrdinalIgnoreCase) { [prop] = v }))
                    .ToList();
            }

            return (combinaciones, multivaluadas);
        }
    }


    public static class Agrupar
    {
        public static List<BloqueDeTotales> ObtenerTotalesAgrupados<T>(this IQueryable<T> consulta,  List<AgrupacionDeTotales> agrupaciones, List<ClausulaDeFiltrado> filtros)
        where T : IRegistro
        {
            var tipo = typeof(T);
            var resultado = new List<BloqueDeTotales>();

            var consultaFiltrada = filtros != null && filtros.Count > 0
                ? consulta.AplicarFiltroPorPropiedades(filtros)
                : consulta;

            foreach (var agrupacion in agrupaciones)
            {
                ValidarPropiedades(tipo, agrupacion);

                var bloque = new BloqueDeTotales
                {
                    PropiedadesDeAgrupacion = agrupacion.PropiedadesDeAgrupacion,
                    Filas = new List<FilaDeTotales>()
                };

                var groupByExpr = $"new({string.Join(", ", agrupacion.PropiedadesDeAgrupacion)})";
                var grupos = consultaFiltrada.GroupBy(groupByExpr).ToDynamicList();

                foreach (var grupo in grupos)
                {
                    var fila = new FilaDeTotales();

                    var items = ((IEnumerable)grupo).Cast<T>().ToList();

                    // Los valores de clave se extraen del primer elemento del grupo (todos comparten la misma clave)
                    var primero = items.FirstOrDefault();
                    foreach (var prop in agrupacion.PropiedadesDeAgrupacion)
                    {
                        var propInfo = tipo.GetProperty(prop, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                        fila.Claves[prop] = primero != null ? propInfo?.GetValue(primero) : null;
                    }

                    foreach (var metrica in agrupacion.Metricas)
                    {
                        fila.Totales[metrica.Alias] = metrica.Operacion == enumOperacionDeTotales.Cuenta
                            ? (object)items.Count
                            : CalcularAgregado(items, metrica, tipo);
                    }

                    bloque.Filas.Add(fila);
                }

                resultado.Add(bloque);
            }

            return resultado;
        }

        private static void ValidarPropiedades(Type tipo, AgrupacionDeTotales agrupacion)
        {
            foreach (var prop in agrupacion.PropiedadesDeAgrupacion)
                if (!tipo.TienenLaPropiedad(prop))
                    GestorDeErrores.Emitir($"La propiedad de agrupación '{prop}' no existe en '{tipo.Name}'");

            foreach (var metrica in agrupacion.Metricas)
                if (metrica.Operacion != enumOperacionDeTotales.Cuenta && !tipo.TienenLaPropiedad(metrica.Campo))
                    GestorDeErrores.Emitir($"La propiedad de métrica '{metrica.Campo}' no existe en '{tipo.Name}'");
        }

        private static object CalcularAgregado<T>(List<T> items, MetricaDeTotales metrica, Type tipo)
        {
            var propInfo = tipo.GetProperty(metrica.Campo, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (propInfo == null)
                GestorDeErrores.Emitir($"No se encontró la propiedad '{metrica.Campo}' en '{tipo.Name}'");

            var valores = items
                .Select(x => {
                    var v = propInfo.GetValue(x);
                    return v == null ? (decimal?)null : Convert.ToDecimal(v);
                })
                .Where(v => v.HasValue)
                .Select(v => v.Value)
                .ToList();

            switch (metrica.Operacion)
            {
                case enumOperacionDeTotales.Suma: return valores.Sum();
                case enumOperacionDeTotales.Media: return valores.Count > 0 ? (object)valores.Average() : null;
                case enumOperacionDeTotales.Max: return valores.Count > 0 ? (object)valores.Max() : null;
                case enumOperacionDeTotales.Min: return valores.Count > 0 ? (object)valores.Min() : null;
                default:
                    GestorDeErrores.Emitir($"Operación '{metrica.Operacion}' no implementada en CalcularAgregado");
                    return null;
            }
        }
    }

    public static class ContarPorMetadatos
    {
        // Cuenta elementos agrupados por una o varias propiedades directas del DTM (IdTipo, IdEstado, IdCg...).
        // Devuelve el bloque listo para formatear sin necesidad de definir AgrupacionDeTotales a mano.
        public static BloqueDeTotales ContarPorPropiedad<T>(this IQueryable<T> consulta, params string[] propiedadesDeAgrupacion)
            where T : IRegistro
        {
            var agrupacion = new AgrupacionDeTotales(
                propiedadesDeAgrupacion.ToList(),
                new List<MetricaDeTotales> { new MetricaDeTotales(enumOperacionDeTotales.Cuenta, "", "Cantidad") }
            );
            return consulta.ObtenerTotalesAgrupados(new List<AgrupacionDeTotales> { agrupacion }, null)[0];
        }
    }

    public static class FiltrarYAgruparPorRelacionada
    {
        // Mantiene solo los elementos que tienen una entidad relacionada no nula que cumple el predicado opcional.
        // Uso: tareas.SoloConRelacionada(t => t.Planificacion(ctx, false), plf => plf.EnJornadas() > 0)
        public static IQueryable<T> SoloConRelacionada<T, TRelacionada>(this IEnumerable<T> elementos, Func<T, TRelacionada> obtenerRelacionada, Func<TRelacionada, bool> predicado = null)
        where TRelacionada : class
        {
            return elementos.Where(t => {
                var rel = obtenerRelacionada(t);
                return rel != null && (predicado == null || predicado(rel));
            }).AsQueryable();
        }

        // Agrupa elementos por una clave derivada de una entidad relacionada (descarta los que no tienen relacionada).
        // Uso: tareas.AgruparPorRelacionada(t => t.Responsable(ctx), u => u.Login)
        public static IQueryable<IGrouping<TKey, T>> AgruparPorRelacionada<T, TRelacionada, TKey>(this IQueryable<T> elementos, Func<T, TRelacionada> obtenerRelacionada, Func<TRelacionada, TKey> selectorClave)
        where TRelacionada : class
        where TKey : notnull
        {
            return elementos
                .Where(t => obtenerRelacionada(t) != null)
                .GroupBy(t => selectorClave(obtenerRelacionada(t)));
        }

        // Agrupa elementos por vínculos muchos-a-muchos: cada elemento puede aparecer en varios grupos.
        // Uso: tareas.AgruparPorVinculos(t => t.Vinculados<ExpedienteDtm>(ctx), e => e.Referencia)
        public static IEnumerable<IGrouping<TKey, T>> AgruparPorVinculos<T, TVinculado, TKey>(
            this IEnumerable<T> elementos,
            Func<T, IEnumerable<TVinculado>> obtenerVinculados,
            Func<TVinculado, TKey> selectorClave)
        where TVinculado : class
        where TKey : notnull
        {
            return elementos
                .SelectMany(t => obtenerVinculados(t)
                    .Where(v => v != null)
                    .Select(v => (clave: selectorClave(v), elemento: t)))
                .GroupBy(par => par.clave, par => par.elemento);
        }
    }
}
