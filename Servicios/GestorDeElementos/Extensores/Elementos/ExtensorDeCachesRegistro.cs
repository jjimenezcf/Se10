using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using System;
using Utilidades;

namespace GestorDeElementos.Extensores.Elementos
{
    public static class ExtensorDeCachesRegistro
    {
        public static void VaciarCacheDeRegistro<TRegistro>(this TRegistro registro, ContextoSe contexto, enumTipoOperacion operacion, string nombre)
        where TRegistro : RegistroDtm
        {
            if (operacion != enumTipoOperacion.Insertar && typeof(TRegistro).ImplementaNombre())
                VaciarCacheDeRegistroPorNombre<TRegistro>(nombre);

            VaciarCacheDeRegistroPorId<TRegistro>(registro.Id);

            ServicioDeCaches.EliminarCache(ServicioDeCaches.PorAk(typeof(TRegistro).FullName));
            ServicioDeCaches.EliminarCache(ServicioDeCaches.PorPropiedad(typeof(TRegistro).FullName));
            ServicioDeCaches.EliminarElemento(CacheDe.Dtm_HayRegistros, typeof(TRegistro).FullName);

            if (registro.ImplemtaUnaAmpliacion())
            {
                var elemento = ((IAmpliacion)registro).AmpliacionDe(contexto);
                var clave = $"{typeof(TRegistro).Name}-{((IAmpliacion)registro).IdElemento}";
                if (operacion != enumTipoOperacion.Insertar)
                {
                    ServicioDeCaches.EliminarElemento(CacheDe.AmpliacionesSinJoin, clave);
                    ServicioDeCaches.EliminarElemento(CacheDe.AmpliacionesConJoin, clave);
                }
                VaciarCacheDeRegistroPorId<TRegistro>(elemento.GetType(), ((IAmpliacion)registro).IdElemento);
                VaciarCacheDeRegistroPorNombre<TRegistro>(elemento.Nombre);
                ServicioDeCaches.EliminarCache(ServicioDeCaches.PorAk(elemento.GetType().FullName));
                ServicioDeCaches.EliminarCache(ServicioDeCaches.PorPropiedad(elemento.GetType().FullName));
            }

            if (registro.ImplemtaUnDetalle())
            {
                var elemento = ((IDetalle)registro).DetalleDe(contexto);
                VaciarCacheDeDetalle(typeof(TRegistro), ((IDetalle)registro).IdElemento);
                VaciarCacheDeRegistroPorId<TRegistro>(elemento.GetType(), ((IDetalle)registro).IdElemento);
                VaciarCacheDeRegistroPorNombre<TRegistro>(elemento.Nombre);
                ServicioDeCaches.EliminarCache(ServicioDeCaches.PorAk(elemento.GetType().FullName));
                ServicioDeCaches.EliminarCache(ServicioDeCaches.PorPropiedad(elemento.GetType().FullName));
                ApiDeEnsamblados.EscribirPropiedad(registro, nameof(IDetalle.Elemento), null);
            }

            var tiposQueReferencian = ApiDeRegistroDtm.EncontrarTiposQueReferencian<TRegistro>();
            foreach (var tipo in tiposQueReferencian)
            {
                ServicioDeCaches.EliminarCachesDelTipo(tipo);
            }
        }

        public static void VaciarCacheDeRegistroPorNombre<TRegistro>(string nombre)
        where TRegistro : RegistroDtm
        {
            var clavePorNombre = $"{nameof(INombre.Nombre)}-{nombre}";
            ServicioDeCaches.EliminarElemento(ServicioDeCaches.PorNombre(typeof(TRegistro).FullName), $"{clavePorNombre}-{ServicioDeCaches.enumConJoin.si}");
            ServicioDeCaches.EliminarElemento(ServicioDeCaches.PorNombre(typeof(TRegistro).FullName), $"{clavePorNombre}-{ServicioDeCaches.enumConJoin.no}");
        }


        private static void VaciarCacheDeRegistroPorId<TRegistro>(int id) where TRegistro: RegistroDtm 
        => 
        VaciarCacheDeRegistroPorId<TRegistro>(typeof(TRegistro), id);

        public static void VaciarCacheDeRegistroPorId<TRegistro>(Type tipo, int id) where TRegistro : RegistroDtm
        {
            var clavePorId = $"{nameof(IRegistro.Id)}-{id}";
            ServicioDeCaches.EliminarElemento(ServicioDeCaches.PorId(tipo.FullName), $"{clavePorId}-{ServicioDeCaches.enumConJoin.si}");
            ServicioDeCaches.EliminarElemento(ServicioDeCaches.PorId(tipo.FullName), $"{clavePorId}-{ServicioDeCaches.enumConJoin.no}");
        }

        public static void VaciarCacheDeDetalle(Type tipo, int idElemento)
        {
            var clave = $"{tipo.Name}-{idElemento}";
            ServicioDeCaches.EliminarElemento(CacheDe.HayDetalle, clave);
            ServicioDeCaches.EliminarElemento(CacheDe.Detalle, clave + "-S");
            ServicioDeCaches.EliminarElemento(CacheDe.Detalle, clave + "-N");
        }


        public static void VaciarCacheDeDetalle(Type tipo)
        {
            var patron = $"{tipo.Name}";
            ServicioDeCaches.EliminarElemento(CacheDe.HayDetalle, patron);
            ServicioDeCaches.EliminarElementos(CacheDe.Detalle, patron);
        }

        public static void VaciarCacheDeRegistroPorId(Type tipo, int id)
        {
            var clavePorId = $"{nameof(IRegistro.Id)}-{id}";
            ServicioDeCaches.EliminarElemento(ServicioDeCaches.PorId(tipo.FullName), $"{clavePorId}-{ServicioDeCaches.enumConJoin.si}");
            ServicioDeCaches.EliminarElemento(ServicioDeCaches.PorId(tipo.FullName), $"{clavePorId}-{ServicioDeCaches.enumConJoin.no}");
        }
    }
}
