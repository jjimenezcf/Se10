using ServicioDeDatos.Elemento;
using ServicioDeDatos.Tarea;
using ServicioDeDatos;
using System;
using System.Linq;
using Utilidades;
using ServicioDeDatos.Negocio;
using iText.Commons.Actions.Contexts;

namespace GestorDeElementos.Extensores
{
    public static class ExtensorDeClases
    {
        public static IQueryable<ClaseDelTipoDtm> ClasesDelTipo(this enumNegocio negocio, ContextoSe contexto)
        {
            switch (negocio)
            {
                case enumNegocio.Tarea:
                    return contexto.Set<ClaseDelTipoTareaDtm>().Cast<ClaseDelTipoDtm>();
                default:
                    throw new Exception($"Se debe indicar como obtener el dbSet de la ClasesDelTipo del negocio: {negocio}");
            }

        }

        public static ClaseDelNegocioDtm Clase(this ClaseDelTipoDtm clase, ContextoSe contexto)
        {
            if (clase.Clase is null)
                clase.Clase = contexto.SeleccionarPorId<ClaseDelNegocioDtm>(clase.IdClase);
            return clase.Clase;
        }

        public static bool HayClasesDelNegocio(this enumNegocio negocio, ContextoSe contexto)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Negocio_HayClasesDelNegocio);
            var indice = negocio.IdNegocio().ToString();
            if (cache.ContainsKey(indice))
                return (bool)cache[indice];

            cache[indice] = contexto.Set<ClaseDelNegocioDtm>().Any(x => x.IdNegocio == negocio.IdNegocio());
            return (bool)cache[indice];
        }
        public static bool HayClasesDelTipo(this enumNegocio negocio, ContextoSe contexto, int idTipo)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Negocio_HayClasesDelTipo);
            var indice = negocio.IdNegocio().ToString() + "-" + idTipo.ToString() ;
            if (cache.ContainsKey(indice))
                return (bool)cache[indice];

            cache[indice] = negocio.ClasesDelTipo(contexto).Any(x => x.IdTipo == idTipo);
            return (bool)cache[indice];
        }
    }
}
