using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using AutoMapper;
using Gestor.Errores;
using ModeloDeDto;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using Utilidades;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace GestorDeElementos
{

    public class Gestores<TContexto, TRegistro, TElemento>
        where TRegistro : RegistroDtm
        where TElemento : ElementoDto
        where TContexto : ContextoSe
    {
        public static GestorDeElementos<TContexto, TRegistro, TElemento> Obtener(TContexto contexto, IMapper mapeador, string clase)
        {
            /*
             * No puedo cachear el objeto gestorDeElementos ya que el contexto es disposable y si un proceso coge el objeto y otro hace un dispose de la conexión mientras se usa
             * entonces fallaría el programa. Por tanto solo cacheo el objeto ConstructorInfo
             */

            var cache = ServicioDeCaches.Obtener(nameof(Gestor));
            clase = $"GestoresDeNegocio.{clase}";
            if (!cache.ContainsKey(clase))
            {
                //string ruta = Assembly.GetExecutingAssembly().GetName().CodeBase;
                //ruta = ruta.Substring(8); /* Quitamos FILE://// */
                //ruta = Path.GetDirectoryName(ruta);

                //var ensamblado = ApiDeEnsamblados.ObtenerDll(ApiDeEnsamblados.DllDelGestorDeNegocio); // Assembly.LoadFrom(Path.Combine(ruta, ApiDeEnsamblados.DllDelGestorDeNegocio));
                var type = ApiDeEnsamblados.ObtenerType(ApiDeEnsamblados.DllDelGestorDeNegocio, clase); // ensamblado.GetType(clase);

                List<Type> tipos = new List<Type> { typeof(TContexto), typeof(IMapper) };
                var constructorSinParametros = type.GetConstructor(tipos.ToArray());
                cache[clase] = constructorSinParametros;
            }
            return (GestorDeElementos<TContexto, TRegistro, TElemento>)((ConstructorInfo)cache[clase]).Invoke(new object[] { contexto, mapeador });
        }
    }

    public static class ApiContextoSe
    {
        public static TSource LeerCacheadoPorId<TSource>(this IQueryable<TSource> source, int id, bool errorSiNoHay = true) where TSource : RegistroDtm
        {
            var cache = ServicioDeCaches.Obtener(ServicioDeCaches.PorId(typeof(TSource).FullName));
            var indice = $"{nameof(RegistroDtm.Id)}-{id}-{ServicioDeCaches.enumConJoin.no}";
            if (!cache.ContainsKey(indice))
            {
                var registro = source.AsNoTracking().FirstOrDefault(x => x.Id == id);
                if (registro == null && errorSiNoHay)
                    GestorDeErrores.Emitir($"No se ha localizado el objeto con id {id} buscado en la entidad {typeof(TSource).Name}");

                cache[indice] = registro;
            }
            return (TSource)cache[indice];
        }

        public static TSource LeerCacheadoPorNombre<TSource>(this IQueryable<TSource> source, string nombre, bool errorSiNoHay = true, bool errorSiHayMasDeUno = true) where TSource: INombre
        {
            var cache = ServicioDeCaches.Obtener(ServicioDeCaches.PorNombre(typeof(TSource).FullName));
            var indice = $"{nameof(INombre.Nombre)}-{nombre}-{ServicioDeCaches.enumConJoin.no}";
            if (!cache.ContainsKey(indice))
            {
                var registros = source.Take(2).Where(x => x.Nombre.Equals(nombre)).Take(2);

                if (registros.Count() == 2 && errorSiNoHay)
                    GestorDeErrores.Emitir($"No se ha localizado el objeto con id {nombre} buscado en la entidad {typeof(TSource).Name}");

                if (registros.Count() == 2 && errorSiHayMasDeUno)
                    GestorDeErrores.Emitir($"No se ha localizado el objeto con id {nombre} buscado en la entidad {typeof(TSource).Name}");

                if (registros.Count() == 0)
                    cache[indice] = null;
                else
                    cache[indice] = registros.ToList()[0];
            }
            return (TSource)cache[indice];
        }

        public static TSource LeerCacheadoPorPropiedad<TSource>(this IQueryable<TSource> source, string nombrePropiedad, object valor, bool errorSiNoHay = true, bool errorSiHayMasDeUno = true) where TSource : IRegistro
        {
            var cache = ServicioDeCaches.Obtener(ServicioDeCaches.PorPropiedad(typeof(TSource).FullName));
            var indice = $"{nombrePropiedad}-{valor}-{ServicioDeCaches.enumConJoin.no}";
            if (!cache.ContainsKey(indice))
            {
                var filtro = new ClausulaDeFiltrado(clausula: nombrePropiedad, criterio: enumCriteriosDeFiltrado.igual, valor: valor.ToString());
                var registros = source.AplicarFiltroPorPropiedades(new List<ClausulaDeFiltrado> { filtro });
                                
                if (registros.Count() == 0 && errorSiNoHay)
                    GestorDeErrores.Emitir($"No se ha localizado el objeto con {nombrePropiedad} {valor} buscado en la entidad {typeof(TSource).Name}");

                if (registros.Count() == 2 && errorSiHayMasDeUno)
                    GestorDeErrores.Emitir($"Hay más de un objeto con {nombrePropiedad} {valor} buscado en la entidad {typeof(TSource).Name}");

                if (registros.Count() == 0)
                    cache[indice] = null;
                else
                    cache[indice] = registros.ToList()[0];
            }
            return (TSource)cache[indice];
        }
    }
}
