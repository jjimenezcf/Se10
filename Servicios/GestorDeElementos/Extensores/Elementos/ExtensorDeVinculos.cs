using Microsoft.SqlServer.TransactSql.ScriptDom;
using ModeloDeDto;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Gastos;
using ServicioDeDatos.Ventas;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Utilidades;

namespace GestorDeElementos.Extensores
{

    public static class ExtencionDeVinculos
    {
        public static T Vincular<T>(this T elemento1, ContextoSe contexto, enumNegocio vinculado, int idElemento2, Dictionary<string, object> parametros = null)
        where T : RegistroDtm
        {
            var negocio = NegociosDeSe.NegocioDeUnDtm(elemento1.GetType());

            GestorDeVinculos.Gestor(contexto, negocio, vinculado).CrearVinculo(elemento1.Id, idElemento2, vincularSiNoLoEsta: true, parametros);
            return (T)NegociosDeSe.CrearGestor(contexto, negocio).LeerRegistroPorId(elemento1.Id, true, false);
        }

        public static T Vincular<T>(this T elemento1, ContextoSe contexto, IRegistro elemento2, Dictionary<string, object> parametros = null)
        where T : RegistroDtm
        {
            if (parametros == null)
                parametros = new Dictionary<string, object>();
            parametros[ltrParametrosNeg.ValidarPermisosDePersistencia] = false;

            var vinculado = NegociosDeSe.NegocioDeUnDtm(elemento2.GetType());

            return elemento1.Vincular(contexto, vinculado, elemento2.Id, parametros);
        }

        public static T Desvincular<T>(this T elemento1, ContextoSe contexto, IRegistro elemento2, Dictionary<string, object> parametros = null)
        where T : RegistroDtm
        {
            if (parametros.ContieneClave(ltrParametrosNeg.AccionQueSeEjecuta))
                parametros[ltrParametrosNeg.EstaEjecutandoUnaAccion] = true;
            var negocio = NegociosDeSe.NegocioDeUnDtm(elemento1.GetType());
            var vinculado = NegociosDeSe.NegocioDeUnDtm(elemento2.GetType());
            GestorDeVinculos.BorrarVinculo(contexto, negocio, vinculado, elemento1.Id, elemento2.Id, parametros);
            return (T)NegociosDeSe.CrearGestor(contexto, negocio).LeerRegistroPorId(elemento1.Id, true, false);
        }

        public static void DesvincularTodos<T>(this IRegistro elemento1, ContextoSe contexto, Dictionary<string, object> parametros = null)
        where T : RegistroDtm
        {
            if (parametros.ContieneClave(ltrParametrosNeg.AccionQueSeEjecuta))
                parametros[ltrParametrosNeg.AccionQueSeEjecuta] = true;
            var negocio = NegociosDeSe.NegocioDeUnDtm(elemento1.GetType());
            var vinculado = NegociosDeSe.NegocioDeUnDtm(typeof(T));
            var elementos = GestorDeVinculos.RegistrosVinculados<T>(contexto, negocio, vinculado, elemento1.Id);
            foreach (var elemento2 in elementos)
                GestorDeVinculos.BorrarVinculo(contexto, negocio, vinculado, elemento1.Id, elemento2.Id, parametros);
        }

        public static List<T> Vinculados<T>(this IRegistro elemento, ContextoSe contexto, Dictionary<string, object> parametros = null, Dictionary<string, object> filtros = null)
        where T : RegistroDtm
        {
            var negocio = NegociosDeSe.NegocioDeUnDtm(elemento.GetType());
            var vinculado = NegociosDeSe.NegocioDeUnDtm(typeof(T));
            return GestorDeVinculos.RegistrosVinculados<T>(contexto, negocio, vinculado, elemento.Id, parametros, filtros);
        }

        public static List<VinculoDtm> Vinculos<T>(this T elemento, ContextoSe contexto, enumNegocio vinculado)
        where T : IElementoDtm
        {
            return GestorDeVinculos.Vinculos(contexto, elemento, vinculado);
        }

        public static bool HayVinculos<T>(this T elemento, ContextoSe contexto, enumNegocio vinculado)
        where T : IElementoDtm
        {
            var negocio = typeof(T).NegocioDeUnDtm();
            return GestorDeVinculos.Existen(contexto, negocio, vinculado, elemento.Id);
        }

        public static List<T> Elementos<T>(this IRegistro elemento, ContextoSe contexto, Dictionary<string, object> parametros = null, Dictionary<string, object> filtros = null)
        where T : ElementoDto
        {
            var negocio = NegociosDeSe.NegocioDeUnDtm(elemento.GetType());
            var vinculado = NegociosDeSe.NegocioDeUnDtm(typeof(T));
            return GestorDeVinculos.ElementosVinculados<T>(contexto, negocio, vinculado, elemento.Id, parametros, filtros);
        }

        public static List<dynamic> Elementos(this IRegistro elemento, ContextoSe contexto, Type tipoDto, Dictionary<string, object> parametros = null, Dictionary<string, object> filtros = null)
        {
            var negocio = NegociosDeSe.NegocioDeUnDtm(elemento.GetType());
            var vinculado = NegociosDeSe.NegocioDeUnDto(tipoDto);
            MethodInfo method = typeof(GestorDeVinculos).GetMethod(nameof(GestorDeVinculos.ElementosVinculados));
            MethodInfo genericMethod = method.MakeGenericMethod(tipoDto);
            var elementos = ((IEnumerable)genericMethod.Invoke(null, new object[] { contexto, negocio, vinculado, elemento.Id, parametros, filtros })).Cast<dynamic>().ToList();
            return elementos;
        }

        public static List<VinculoDtm> CebarCacheDeVinculosCon<t1, t2, t3>(ContextoSe contexto, List<int> ids)
        where t1 : VinculoDtm
        where t2 : ElementoDtm
        where t3 : ElementoDtm
        {
            var cacheVinculos = ServicioDeCaches.Obtener(CacheDe.LeerVinculosCon);
            string tablaVinculo = ApiDeVinculos.Tabla(typeof(t2), typeof(t3));
            var todosLosVinculos = contexto.Set<t1>()
                            .Where(v => ids.Contains(v.idElemento1))
                            .ToList();
            foreach (var id in ids)
            {
                var vinculosDeUnId = todosLosVinculos.Where(v => v.idElemento1 == id).ToList();
                cacheVinculos[$"{tablaVinculo}-{id}"] = vinculosDeUnId.Cast<VinculoDtm>().ToList();
            }

            return todosLosVinculos.Cast<VinculoDtm>().ToList();
        }
    }
}
