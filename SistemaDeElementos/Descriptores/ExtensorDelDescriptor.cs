using ModeloDeDto;
using MVCSistemaDeElementos.Descriptores;
using ServicioDeDatos;
using System;
using Utilidades;

namespace SistemaDeElementos.Descriptores
{
    public static class ExtensorDelDescriptor<E> where E : ElementoDto
    {
        public static DescriptorDeCrud<E> Crear(ContextoSe contexto, ModoDescriptor modo, string titulo, Func<ContextoSe, ModoDescriptor, string, DescriptorDeCrud<E>> descriptor)
        {
            var cache = ServicioDeCaches.Obtener("Descriptores");
            if (!cache.ContainsKey(contexto.DatosDeConexion.IdUsuario.ToString())) {
             cache[contexto.DatosDeConexion.IdUsuario.ToString()] = descriptor(contexto, modo, titulo);
            }
            return (DescriptorDeCrud<E>)cache[contexto.DatosDeConexion.IdUsuario.ToString()];
        }

    }
}
