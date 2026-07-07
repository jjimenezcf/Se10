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
            Func<string, ElementoDeProcesoDtm, string> resolverClaveVirtual = null)
        {
            var resultado = new List<BloqueDeTotales>();

            foreach (var agrupacion in agrupaciones)
            {
                var bloque = new BloqueDeTotales
                {
                    PropiedadesDeAgrupacion = agrupacion.PropiedadesDeAgrupacion,
                    Filas = new List<FilaDeTotales>()
                };

                // Agrupar por clave compuesta usando el tipo real en tiempo de ejecución
                var grupos = registros
                    .GroupBy(r => string.Join("|", agrupacion.PropiedadesDeAgrupacion.Select(prop =>
                    {
                        var pi = r.GetType().GetProperty(prop, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                        if (pi != null) return pi.GetValue(r)?.ToString() ?? string.Empty;
                        return resolverClaveVirtual?.Invoke(prop, r) ?? string.Empty;
                    })))
                    .ToList();

                foreach (var grupo in grupos)
                {
                    var fila   = new FilaDeTotales();
                    var items  = grupo.ToList();
                    var primero = items.FirstOrDefault();
                    var tipoReal = primero?.GetType() ?? typeof(ElementoDeProcesoDtm);

                    // Claves del grupo
                    foreach (var prop in agrupacion.PropiedadesDeAgrupacion)
                    {
                        var pi = tipoReal.GetProperty(prop, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                        if (pi != null)
                            fila.Claves[prop] = primero != null ? pi.GetValue(primero) : null;
                        else if (resolverClaveVirtual != null && primero != null)
                            fila.Claves[prop] = resolverClaveVirtual(prop, primero);
                        else
                            fila.Claves[prop] = null;
                    }

                    // Métricas
                    foreach (var metrica in agrupacion.Metricas)
                    {
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
