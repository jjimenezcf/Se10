using Gestor.Errores;
using GestorDeElementos.Extensores;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Guarderias;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Ventas;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilidades;

namespace GestorDeElementos
{
    public static class ApiDeDetalles
    {
        public static readonly string MensajeDeNoUsaDetalle = "El tipo de elemento no implementa el detalle solicitado:";


        public static string NegocioNoUsaDetalle(enumNegocio negocio, Type tipo) => $"El '{negocio.Singular()}' no implementa el detalle '{tipo.Name}'";

        public static ElementoDtm DetalleDe(this IDetalle detalle, ContextoSe contexto, bool aplicarJoin = false, bool usarLaCache = true)
        =>
        (ElementoDtm)NegociosDeSe.CrearGestor(contexto, detalle.Negocio).LeerRegistroPorId(detalle.IdElemento, aplicarJoin, usarLaCache);

        public static T DetalleDe<T>(this IDetalle detalle, ContextoSe contexto, bool aplicarJoin = false, bool usarLaCache = true)
        where T : IElementoDtm
        =>
        (T)NegociosDeSe.CrearGestor(contexto, detalle.Negocio).LeerRegistroPorId(detalle.IdElemento, aplicarJoin, usarLaCache);

        public static List<T> Detalles<T>(this IElementoDtm registro, ContextoSe contexto, bool errorSiNoHay = false, bool aplicarJoin = false, bool usarLaCache = true, List<ClausulaDeFiltrado> filtros = null)
        where T : IDetalle
        {
            var detalles = Detalles(registro, contexto, typeof(T), errorSiNoHay, aplicarJoin, usarLaCache, filtros);
            var detallesCasteados = detalles.Cast<T>().ToList();
            return detallesCasteados;
        }

        public static List<IDetalle> Detalles(this IElementoDtm registro, ContextoSe contexto, Type tipoDetalle, bool errorSiNoHay = false, bool aplicarJoin = false, bool usarLaCache = true, List<ClausulaDeFiltrado> filtros = null)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Detalle);
            var indice = $"{tipoDetalle.Name}-{registro.Id}-{(aplicarJoin ? "S" : "N")}";

            if (filtros != null && filtros.Count > 0)
                usarLaCache = false;

            if (!usarLaCache || !cache.ContainsKey(indice))
            {
                var negocio = NegociosDeSe.NegocioDeUnDtm(registro.GetType());

                if (negocio.UsaTipo() && !UsaElDetalleDe(negocio, contexto, ((IUsaTipo)registro).IdTipo, tipoDetalle))
                    throw new Exception($"{MensajeDeNoUsaDetalle} '{tipoDetalle.Name}', indíquelo en el método: '{nameof(UsaElDetalleDe)}'");

                if (!negocio.UsaTipo() && !UsaElDetalleDe(negocio, tipoDetalle))
                    throw new Exception($"{MensajeDeNoUsaDetalle} '{tipoDetalle.Name}', indíquelo en el método: '{nameof(UsaElDetalleDe)}'");

                var detalles = contexto.SeleccionarDetalles(tipoDetalle, registro.Id, errorSiNoHay, aplicarJoin, filtros);
                if (detalles.Count > 0)
                    cache[indice] = detalles;

                //si no hay detalle no cacheo nada
                return detalles;
            }
            return (List<IDetalle>)cache[indice];
        }

        public static bool HayDetallesDe<T>(this IElementoDtm registro, ContextoSe contexto, bool errorSiNoHay = false, bool usarLaCache = true, List<ClausulaDeFiltrado> filtros = null)
        where T : RegistroDtm
        {
            if (!ApiDeInterfaceDtm.ImplementaDetalle(typeof(T)))
                GestorDeErrores.Emitir($"El tipo {typeof(T)} no se puede usar con la función {nameof(HayDetallesDe)}");

            var cache = ServicioDeCaches.Obtener(CacheDe.HayDetalle);
            var indice = $"{typeof(T).Name}-{registro.Id}";

            if (filtros != null && filtros.Count > 0)
                usarLaCache = false;

            if (!usarLaCache || !cache.ContainsKey(indice))
            {
                var negocio = NegociosDeSe.NegocioDeUnDtm(registro.GetType());

                if (negocio.UsaTipo() && !UsaElDetalleDe(negocio, contexto, ((IUsaTipo)registro).IdTipo, typeof(T)))
                    throw new Exception($"{MensajeDeNoUsaDetalle} '{typeof(T).Name}'");

                if (!negocio.UsaTipo() && !UsaElDetalleDe(negocio, typeof(T)))
                    throw new Exception($"{MensajeDeNoUsaDetalle} '{typeof(T).Name}'");

                if (filtros == null) filtros = new List<ClausulaDeFiltrado>();
                if (!filtros.Any(x => x.Clausula == nameof(IDetalle.IdElemento))) filtros.Add(new ClausulaDeFiltrado(nameof(IDetalle.IdElemento), enumCriteriosDeFiltrado.igual, registro.Id));

                if (usarLaCache)
                    cache[indice] = contexto.Existen<T>(nameof(IDetalle.IdElemento), registro.Id);
                else
                    return contexto.Existen<T>(filtros);

            }
            return (bool)cache[indice];
        }
        public static void ValidarUsaDetalleDe(this enumNegocio negocio, ContextoSe contexto, int idTipo, Type tipoDeDetalle)
        {
            if (!UsaElDetalleDe(negocio, contexto, idTipo, tipoDeDetalle))
                throw new Exception($"{MensajeDeNoUsaDetalle} '{tipoDeDetalle.Name}'");
        }

        public static void ValidarUsaDetalleDe(this enumNegocio negocio, Type tipoDeDetalle)
        {
            if (!UsaElDetalleDe(negocio, tipoDeDetalle))
                throw new Exception(NegocioNoUsaDetalle(negocio, tipoDeDetalle));
        }

        private static List<T> SeleccionarDetalles<T>(this ContextoSe contexto, int idElemento, bool errorSiNoHay = false, bool aplicarJoin = false, List<ClausulaDeFiltrado> filtros = null)
        where T : IDetalle
        {
            if (filtros == null) filtros = new List<ClausulaDeFiltrado>();

            if (filtros.Where(x => x.Clausula == nameof(IDetalle.IdElemento)).Count() == 0)
            {
                var filtro = new ClausulaDeFiltrado(nameof(IDetalle.IdElemento), enumCriteriosDeFiltrado.igual, idElemento);
                filtros.Add(filtro);
            }
            var gestor = typeof(T).CrearGestorDeUnDetalle(contexto);
            var detalles = (List<T>)gestor.LeerRegistros(0, -1, filtros, aplicarJoin);
            if (errorSiNoHay && detalles.Count() == 0)
                throw new Exception($"No hay detalles {typeof(T).Name} para el id del registro pasado: {idElemento}");

            return detalles;
        }

        private static List<IDetalle> SeleccionarDetalles(this ContextoSe contexto, Type tipo, int idElemento, bool errorSiNoHay = false, bool aplicarJoin = false, List<ClausulaDeFiltrado> filtros = null)
        {
            if (filtros == null) filtros = new List<ClausulaDeFiltrado>();

            if (filtros.Where(x => x.Clausula == nameof(IDetalle.IdElemento)).Count() == 0)
            {
                var filtro = new ClausulaDeFiltrado(nameof(IDetalle.IdElemento), enumCriteriosDeFiltrado.igual, idElemento);
                filtros.Add(filtro);
            }
            var gestor = tipo.CrearGestorDeUnDetalle(contexto);
            var detalles = ((IEnumerable<object>)gestor.LeerRegistros(0, -1, filtros, aplicarJoin)).Cast<IDetalle>().ToList();
            if (errorSiNoHay && detalles.Count() == 0)
                throw new Exception($"No hay detalles {tipo.Name} para el id del registro pasado: {idElemento}");

            return detalles;
        }

        public static bool UsaElDetalleDe(this enumNegocio negocio, ContextoSe contexto, int idTipo, Type tipoDetalle)
        {
            if (!negocio.UsaTipo())
                return false;

            switch (negocio)
            {
                case enumNegocio.Pleito: return ExtensorDePleitos.UsaElDetalleDe(contexto, idTipo, tipoDetalle);
                case enumNegocio.Presupuesto: return ExtensorDePresupuestos.UsaElDetalleDe(contexto, idTipo, tipoDetalle);
                case enumNegocio.Contrato: return ExtensorDeContratos.UsaElDetalleDe(contexto, idTipo, tipoDetalle);
                case enumNegocio.Expediente: return ExtensorDeExpedientes.UsaElDetalleDe(contexto, idTipo, tipoDetalle);
                case enumNegocio.CircuitoDoc: return ExtensoresDeCircuitosDoc.UsaElDetalleDe(contexto, idTipo, tipoDetalle);

            }
            return negocio.UsaElDetalleDe(tipoDetalle);
        }

        private static bool UsaElDetalleDe(this enumNegocio negocio, Type tipoDetalle)
        {
            switch (negocio)
            {
                case enumNegocio.PlanificadorDeVenta: return tipoDetalle == typeof(LineaDeUnPlfVentaDtm);
                case enumNegocio.PlanificacionDeVenta: return tipoDetalle == typeof(LineaDeUnaPlfVentaDtm);
                case enumNegocio.FacturaEmitida: return ExtensorDeFacturasEmt.UsaElDetalleDe(tipoDetalle);
                case enumNegocio.FacturaRecibida: return ExtensorDeFacturasRec.UsaElDetalleDe(tipoDetalle);
                case enumNegocio.Pedido: return ExtensorDePedidos.UsaElDetalleDe(tipoDetalle);
                case enumNegocio.Preasiento: return ExtensorDePreasientos.UsaElDetalleDe(tipoDetalle);
                case enumNegocio.RemesaFae: return ExtensorDeRemesasFae.UsaElDetalleDe(tipoDetalle);
                case enumNegocio.RemesaPag: return ExtensorDeRemesasPag.UsaElDetalleDe(tipoDetalle);
                case enumNegocio.ParteDeTrabajo: return ExtensorDePartesTr.UsaElDetalleDe(tipoDetalle);
                case enumNegocio.Cliente:
                    return tipoDetalle == typeof(CuentaDeClienteDtm) || tipoDetalle == typeof(UsuarioDeClienteDtm) || tipoDetalle == typeof(PuestoDeClienteDtm)
                        || tipoDetalle == typeof(CentroAdministrativoDtm);
                case enumNegocio.Proveedor: return tipoDetalle == typeof(CuentaDeProveedorDtm);
                case enumNegocio.Trabajador: return tipoDetalle == typeof(CuentaDeTrabajadorDtm);
                case enumNegocio.Interlocutor: return tipoDetalle == typeof(CuentaDeInterlocutorDtm);
                case enumNegocio.Sociedad: return tipoDetalle == typeof(CuentaDeMiSociedadDtm) || tipoDetalle == typeof(TarjetaDeMiSociedadDtm) || tipoDetalle == typeof(FacturadorDeSociedadDtm);
                case enumNegocio.CursoDeGuarderia: return tipoDetalle == typeof(ProfeDeCursoDeGuarderiaDtm) || tipoDetalle == typeof(InfanteDeUnCursoDtm);
            }
            return false;
        }

        public static void CebarCacheDeDetalles<T>(ContextoSe contexto, List<int> idsDeElemento,  bool leerElemento = false,  Func<IQueryable<T>, IQueryable<T>> incluyeExtras = null)
        where T : RegistroDtm, IDetalle
        {
            var cacheDeDetalle = ServicioDeCaches.Obtener(CacheDe.Detalle);
            bool aplicarJoin = incluyeExtras != null;
            var idsNuevos = idsDeElemento
                            .Where(id => !cacheDeDetalle.ContainsKey(IndiceCacheDeDetalle<T>(id, aplicarJoin)))
                            .Distinct()
                            .ToList();

            if (!idsNuevos.Any()) return;

            // 1. Empezamos con el Set básico
            IQueryable<T> query = contexto.Set<T>();

            // 2. Aplicamos el Include de Elemento si se solicita
            if (leerElemento)
                query = query.Include(p => p.Elemento);

            // 3. Aplicamos los Includes extras pasados por parámetro
            if (aplicarJoin)
                query = incluyeExtras(query);

            // 4. Ejecutamos la consulta con el filtro de IDs
            var todosLosDetalles = query
                      .Where(p => idsNuevos.Contains(p.IdElemento))
                      .ToList();

            // 5. Agrupación y poblado de caché (igual que antes)
            var grupos = todosLosDetalles.GroupBy(d => d.IdElemento).ToDictionary(g => g.Key, g => g.ToList());


            var cacheHayDetalle = ServicioDeCaches.Obtener(CacheDe.HayDetalle);


            foreach (var id in idsNuevos)
            {
                List<T> lineas = grupos.ContainsKey(id) ? grupos[id] : new List<T>();
                var listaParaCache = lineas.Cast<IDetalle>().ToList();
                cacheDeDetalle[IndiceCacheDeDetalle<T>(id, aplicarJoin)] = listaParaCache;
                cacheHayDetalle[$"{typeof(T).Name}-{id}"] = listaParaCache.Any();
            }
        }

        private static string IndiceCacheDeDetalle<T>(int idElemento, bool aplicarJoin)
            => $"{typeof(T).Name}-{idElemento}-{(aplicarJoin ? "S" : "N")}";
    }
}
