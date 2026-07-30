using GestorDeElementos.Extensores;
using Microsoft.Win32;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using Utilidades;

namespace GestorDeElementos
{
    public static class LinqExtensions
    {
        public static IOrderedQueryable<TSource> OrderBy<TSource, TKey>(this IQueryable<TSource> consulta, Expression<Func<TSource, TKey>> keySelector, bool ascending)
        {
            return ascending ? consulta.OrderBy(keySelector) : consulta.OrderByDescending(keySelector);
        }

        public static IOrderedQueryable<TSource> ThenBy<TSource, TKey>(this IOrderedQueryable<TSource> consultaOrdenada, Expression<Func<TSource, TKey>> keySelector, bool ascending)
        {
            return ascending ? consultaOrdenada.ThenBy(keySelector) : consultaOrdenada.ThenByDescending(keySelector);
        }
    }

    public static class Ordenar
    {
        public static IQueryable<TRegistro> AplicarOrdenesBasicos<TRegistro>(this IQueryable<TRegistro> consulta, ContextoSe contexto, List<ClausulaDeOrdenacion> ordenacion) where TRegistro : IRegistro
        {
            if (ordenacion.Count == 0)
                return consulta;

            IOrderedQueryable<TRegistro> consultaOrdenada = null;
            var ordenPorPrioridad = typeof(TRegistro).ImplementaPrioridad()
            ? ordenacion.FirstOrDefault(x => x.OrdenarPor.Equals(nameof(IPrioridad.Prioridad), StringComparison.InvariantCultureIgnoreCase))
            : null;

            // Prioridad se guarda como texto (Urgente/Alta/Media/Baja/NoDefinida), así que un
            // OrderBy directo la ordenaría alfabéticamente. Aquí se traduce a un rango numérico
            // (0=Urgente ... 4=NoDefinida) para que EF lo traduzca a un CASE WHEN en el ORDER BY.
            Expression<Func<TRegistro, int>> RangoDePrioridad = ordenPorPrioridad == null ? null : t =>
             ((IPrioridad)t).Prioridad == enumPrioridad.Urgente ? 0 :
             ((IPrioridad)t).Prioridad == enumPrioridad.Alta ? 1 :
             ((IPrioridad)t).Prioridad == enumPrioridad.Media ? 2 :
             ((IPrioridad)t).Prioridad == enumPrioridad.Baja ? 3 : 4;

            for (var i = 0; i < ordenacion.Count; i++)
            {
                var orden = ordenacion[i];
                if (ExcluirPropiedadOrdenPor(contexto, typeof(TRegistro), orden))
                {
                    EnviarMensajeDeMalOrdenacion(contexto, typeof(TRegistro), orden);
                    return consulta;
                }
                Expression<Func<TRegistro, object>> expresionOrderBy = ParsearExpresionLambda<TRegistro>(orden);

                if (i == 0)
                {
                    if (ordenPorPrioridad != null && orden.OrdenarPor == ordenPorPrioridad.OrdenarPor)
                    {
                        var ascendente = ordenPorPrioridad.Modo == ModoDeOrdenancion.ascendente;
                        consultaOrdenada = ascendente
                            ? consulta.OrderBy(RangoDePrioridad)
                            : consulta.OrderByDescending(RangoDePrioridad);
                    }
                    else
                        consultaOrdenada = consulta.OrderBy(expresionOrderBy, orden.Modo == ModoDeOrdenancion.ascendente);
                }
                else
                {
                    if (ordenPorPrioridad != null && orden.OrdenarPor == ordenPorPrioridad.OrdenarPor)
                    {
                        var ascendente = ordenPorPrioridad.Modo == ModoDeOrdenancion.ascendente;
                        consultaOrdenada = ascendente
                            ? consultaOrdenada.ThenBy(RangoDePrioridad)
                            : consultaOrdenada.ThenByDescending(RangoDePrioridad);
                    }
                    consultaOrdenada = consultaOrdenada.ThenBy(expresionOrderBy, orden.Modo == ModoDeOrdenancion.ascendente);
                }
            }

            return consultaOrdenada;
        }

        private static bool ExcluirPropiedadOrdenPor(ContextoSe contexto, Type tipoDelObjeto, ClausulaDeOrdenacion orden)
        {
            var listaDeOrdenes = orden.OrdenarPor.Split('.').ToList();
            if (listaDeOrdenes.Count() > 1)
            {
                var ordenarPor = listaDeOrdenes[0];
                if (!ApiDeEnsamblados.TienenLaPropiedad(tipoDelObjeto, ordenarPor)
                  || !ApiDeEnsamblados.HeredaDe(tipoDelObjeto, typeof(RegistroDtm)))
                    return true;

                tipoDelObjeto = ApiDeEnsamblados.TipoDeLaPropiedad(tipoDelObjeto, ordenarPor);
                var ordenPendiente = "";
                for (int i = 1; i < listaDeOrdenes.Count(); i++)
                {
                    ordenPendiente = $"{(ordenPendiente.IsNullOrEmpty() ? "" : $"{ordenPendiente}.")}" + listaDeOrdenes[i];
                }

                return ExcluirPropiedadOrdenPor(contexto, tipoDelObjeto, new ClausulaDeOrdenacion(ordenPendiente, orden.Modo));
            }

            var tipoDeLaPropiedad = ApiDeEnsamblados.TipoDeLaPropiedad(tipoDelObjeto, orden.OrdenarPor);
            if (tipoDeLaPropiedad == null)
                return true;

            if (ApiDeEnsamblados.HeredaDe(tipoDeLaPropiedad, typeof(RegistroDtm))) return true;

            return false;
        }

        private static void EnviarMensajeDeMalOrdenacion(ContextoSe contexto, Type tipoDelObjeto, ClausulaDeOrdenacion orden)
        {
            var asunto = "Orden mal definido";
            var cuerpo =
                $"Se ha definido mal la ordenación para la propiedad '{orden.OrdenarPor}' en la consulta de {tipoDelObjeto.Name}{Environment.NewLine}{Environment.NewLine}" +
                $"Pila de llamadas:{Environment.NewLine}" + new StackTrace().ToString();
            contexto.AnotarTraza(asunto, cuerpo);
            contexto.EnviarCorreoPorAdministrador(CacheDeVariable.Cfg_ServidorDeCorreo, CacheDeVariable.Cfg_CorreoDeSoporte.Split(';').ToList(), asunto, cuerpo);
        }

        private static Expression<Func<TRegistro, object>> ParsearExpresionLambda<TRegistro>(ClausulaDeOrdenacion orden) where TRegistro : IRegistro
        {
            var expresion = $"x => x.{orden.OrdenarPor}";
            var ConfiguracionDelParseo = new ParsingConfig();
            ConfiguracionDelParseo.UseParameterizedNamesInDynamicQuery = true;
            return DynamicExpressionParser.ParseLambda<TRegistro, object>(ConfiguracionDelParseo, false, expresion);
        }
    }
}
