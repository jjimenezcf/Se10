using ModeloDeDto.Entorno;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper;
using GestorDeElementos;
using ServicioDeDatos;
using ModeloDeDto;
using ServicioDeDatos.Entorno;
using Utilidades;

namespace GestoresDeNegocio.Entorno
{
    public class GestorDeAccesosRecientes : GestorDeElementos<ContextoSe, AccesoRecienteDtm, ElementoDto>
    {
        public class MapearAccesosRecientes : Profile
        {
            public MapearAccesosRecientes()
            {
                CreateMap<AccesoRecienteDtm, ElementoDto>();
            }
        }

        public GestorDeAccesosRecientes(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        public static void GuardarMenuAccedido(ContextoSe contexto, int idMenu, int idVista, string parametros, string opcionHtml, string urlAccedida)
        {
            var gestor = new GestorDeAccesosRecientes(contexto, contexto.Mapeador);
            var accesoRecientes = LeerAccesosRecientes(contexto, enumClaseDeAcceso.Menu);
            var accesoExistente = accesoRecientes.FirstOrDefault(a => a.IdMenu == idMenu && a.IdVista == idVista && a.OpcionHtml == opcionHtml && a.UrlAccedida == urlAccedida);
            if (accesoExistente is not null)
                gestor.PersistirRegistro(accesoExistente, new ParametrosDeNegocio(enumTipoOperacion.Eliminar));
            var accesoReciente = new AccesoRecienteDtm
            {
                IdUsuario = contexto.Usuario.Id,
                Nombre = contexto.SeleccionarPorId<MenuDtm>(idMenu).Nombre,
                ClaseDeAcceso = enumClaseDeAcceso.Menu,
                IdMenu = idMenu,
                IdVista = idVista,
                OpcionHtml = opcionHtml,
                UrlAccedida = urlAccedida,
                AccedioEl = DateTime.Now
            };
            gestor.PersistirRegistro(accesoReciente, new ParametrosDeNegocio(enumTipoOperacion.Insertar));
            accesoRecientes.Add(accesoReciente);
        }

        public static void GuardarRegistroAccedido(ContextoSe contexto, string nombre, int idVista, string parametros, string opcionHtml, string urlAccedida)
        {
            var gestor = new GestorDeAccesosRecientes(contexto, contexto.Mapeador);
            var accesoRecientes = LeerAccesosRecientes(contexto, enumClaseDeAcceso.Registros);
            var accesoExistente = accesoRecientes.FirstOrDefault(a => a.UrlAccedida == urlAccedida);
            if (accesoExistente is not null)
                gestor.PersistirRegistro(accesoExistente, new ParametrosDeNegocio(enumTipoOperacion.Eliminar));

            var accesoReciente = new AccesoRecienteDtm
            {
                IdUsuario = contexto.Usuario.Id,
                Nombre = nombre,
                ClaseDeAcceso = enumClaseDeAcceso.Registros,
                IdMenu = null,
                IdVista = idVista,
                OpcionHtml = opcionHtml,
                UrlAccedida = urlAccedida,
                AccedioEl = DateTime.Now
            };
            gestor.PersistirRegistro(accesoReciente, new ParametrosDeNegocio(enumTipoOperacion.Insertar));
            accesoRecientes.Add(accesoReciente);
        }

        protected override void EliminarCaches(AccesoRecienteDtm registro, ParametrosDeNegocio parametros)
        {
            base.EliminarCaches(registro, parametros);
            ServicioDeCaches.EliminarElemento(CacheDe.Ent_AccesosRecientes, $"{enumClaseDeAcceso.Menu}-{Contexto.Usuario.Id}");
            ServicioDeCaches.EliminarElemento(CacheDe.Ent_AccesosRecientes, $"{enumClaseDeAcceso.Registros}-{Contexto.Usuario.Id}");
        }

        public static List<AccesoRecienteDtm> LeerAccesosRecientes(ContextoSe contexto, enumClaseDeAcceso clase)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Ent_AccesosRecientes);
            var indice = $"{clase}-{contexto.Usuario.Id}";

            if (!cache.ContainsKey(indice.ToString()))
            {
                // Obtener todos los accesos del usuario y ordenarlos por fecha de acceso descendente.
                // No se agrupa aquí para no perder el orden global.
                var accesosRecientesOrdenados = contexto.Set<AccesoRecienteDtm>()
                    .Where(a => a.IdUsuario == contexto.Usuario.Id && a.ClaseDeAcceso.Equals(clase))
                    .OrderByDescending(a => a.AccedioEl)
                    .ToList();

                // Quitar los duplicados usando un método auxiliar
                var accesosRecientesSinDuplicados = quitarDuplicados(accesosRecientesOrdenados);

                // Tomamos los 10 primeros elementos de la lista ya sin duplicados y ordenada
                cache[indice.ToString()] = accesosRecientesSinDuplicados.Take(10).ToList();
            }

            // Devolvemos la lista desde la caché.
            var lista = (List<AccesoRecienteDtm>)cache[indice.ToString()];

            return lista;
        }

        /// <summary>
        /// Recorre una lista ordenada de accesos recientes y elimina los duplicados basándose en
        /// un conjunto de propiedades clave.
        /// </summary>
        /// <param name="accesosOrdenados">La lista de accesos recientes ya ordenada.</param>
        /// <returns>Una nueva lista con los accesos únicos.</returns>
        private static List<AccesoRecienteDtm> quitarDuplicados(List<AccesoRecienteDtm> accesosOrdenados)
        {
            var listaUnica = new List<AccesoRecienteDtm>();
            var clavesVistas = new HashSet<(int, enumClaseDeAcceso, int?, string)>();

            foreach (var acceso in accesosOrdenados)
            {
                // Se crea una clave única para cada acceso basada en las propiedades.
                var clave = (acceso.IdUsuario, acceso.ClaseDeAcceso, acceso.IdMenu, acceso.OpcionHtml);

                // Si la clave no ha sido vista, añadimos el acceso a la lista final y la clave al HashSet.
                if (clavesVistas.Add(clave))
                {
                    listaUnica.Add(acceso);
                }
            }

            return listaUnica;
        }

    }
}

