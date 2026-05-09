using System;
using System.Collections.Generic;
using System.Linq;
using Gestor.Errores;
using GestorDeElementos.Extensores;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using Utilidades;

namespace GestorDeElementos
{
    public static class ApiDeAmpliaciones
    {
        public static readonly string MensajeDeNoUsaAmpliacion = "El tipo: '[tipo]', no implementa la ampliación: '[tipoAmpliacion]'";

        public static void ValidarUsaAmpliacionDe(this enumNegocio negocio, ContextoSe contexto, int idTipo, Type tipoAmpliacion)
        {
            if (!UsaLaAmpliacionDe(negocio, contexto, idTipo, tipoAmpliacion))
            {
                var tipo = (ITipoDtm)negocio.CrearGestorDeTipo(contexto).LeerRegistroPorId(idTipo, aplicarJoin: false);
                throw new Exception(MensajeDeNoUsaAmpliacion.Replace("[tipo]", tipo.Nombre).Replace("[tipoAmpliacion]", tipoAmpliacion.Name));
            }
        }

        public static bool UsaLaAmpliacionDe(this enumNegocio negocio, ContextoSe contexto, int idTipo, Type tipoAmpliacion)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Negocio_UsaLaAmpliacion);
            var indice = $"{negocio}.{idTipo}.{tipoAmpliacion.Name}";
           
            if (!cache.ContainsKey(indice))
                cache[indice] = UsaLaAmpliacion(negocio, contexto, idTipo, tipoAmpliacion);

            return (bool)cache[indice];
        }

        private static bool UsaLaAmpliacion(enumNegocio negocio, ContextoSe contexto, int idTipo, Type tipoAmpliacion)
        {
            if (!negocio.UsaTipo())
            {
                switch (negocio)
                {
                    case enumNegocio.Sociedad: return true;
                }
                return false;
            }

            switch (negocio)
            {
                case enumNegocio.Pleito: return ExtensorDePleitos.UsaLaAmpliacionDe(contexto, idTipo, tipoAmpliacion);
                case enumNegocio.Presupuesto: return ExtensorDePresupuestos.UsaLaAmpliacionDe(contexto, idTipo, tipoAmpliacion);
                case enumNegocio.Contrato: return ExtensorDeContratos.UsaLaAmpliacionDe(contexto, idTipo, tipoAmpliacion);
                case enumNegocio.Tarea: return ExtensorDeTareas.UsaLaAmpliacionDe(contexto, idTipo, tipoAmpliacion);
                case enumNegocio.FacturaEmitida: return ExtensorDeFacturasEmt.UsaLaAmpliacionDe(contexto, idTipo, tipoAmpliacion);
                case enumNegocio.Expediente: return ExtensorDeExpedientes.UsaLaAmpliacionDe(contexto, idTipo, tipoAmpliacion);
                case enumNegocio.CircuitoDoc: return ExtensoresDeCircuitosDoc.UsaLaAmpliacionDe(contexto, idTipo, tipoAmpliacion);
            }
            return false;
        }

        public static T Ampliacion<T>(this IElementoDeProcesoDtm registro, ContextoSe contexto, bool errorSiNoHay = true, bool aplicarJoin = false, bool usarLaCache = true)
        where T : IAmpliacion
        {
            var cache = aplicarJoin ? ServicioDeCaches.Obtener(CacheDe.AmpliacionesConJoin) : ServicioDeCaches.Obtener(CacheDe.AmpliacionesSinJoin);
            var indice = $"{typeof(T).Name}-{registro.Id}";
            if (!usarLaCache || !cache.ContainsKey(indice))
            {
                var negocio = NegociosDeSe.NegocioDeUnDtm(registro.GetType());
                if (!UsaLaAmpliacionDe(negocio, contexto, registro.IdTipo, typeof(T)))
                {
                    var tipo = (ITipoDtm)negocio.CrearGestorDeTipo(contexto).LeerRegistroPorId(registro.IdTipo, aplicarJoin: false);
                    throw new Exception(MensajeDeNoUsaAmpliacion.Replace("[tipo]", tipo.Nombre).Replace("[tipoAmpliacion]", typeof(T).Name));
                }
                var ampliacion = contexto.SeleccionarAmpliacion<T>(registro.Id, errorSiNoHay, aplicarJoin);
                if (ampliacion != null)
                    cache[indice] = ampliacion;

                //si es nulo, no lo cacheo
                return ampliacion;
            }
            return (T)cache[indice];
        }


        public static T Ampliacion<T>(this enumNegocio negocio, ContextoSe contexto, int idPadre, bool errorSiNoHay = true, bool aplicarJoin = false, bool usarLaCache = true)
        where T : IAmpliacion
        {
            var cache = aplicarJoin ? ServicioDeCaches.Obtener(CacheDe.AmpliacionesConJoin) : ServicioDeCaches.Obtener(CacheDe.AmpliacionesSinJoin);
            var indice = $"{typeof(T).Name}-{idPadre}";
            if (!usarLaCache || !cache.ContainsKey(indice))
            {
                var registro = (IElementoDeProcesoDtm)negocio.CrearGestor(contexto).LeerRegistroPorId(idPadre, aplicarJoin: false, usarLaCache: true);
                if (!UsaLaAmpliacionDe(negocio, contexto, registro.IdTipo, typeof(T)))
                {
                    var tipo = (ITipoDtm)negocio.CrearGestorDeTipo(contexto).LeerRegistroPorId(registro.IdTipo, aplicarJoin: true);
                    throw new Exception(MensajeDeNoUsaAmpliacion.Replace("[tipo]", tipo.Nombre).Replace("[tipoAmpliacion]", typeof(T).Name));
                }
                var ampliacion = contexto.SeleccionarAmpliacion<T>(registro.Id, errorSiNoHay, aplicarJoin);
                if (ampliacion != null)
                    cache[indice] = ampliacion;

                //si es nulo, no lo cacheo
                return ampliacion;
            }
            return (T)cache[indice];
        }

        public static T AmpliacionDe<T>(this IAmpliacion ampliacion, ContextoSe contexto)
        where T : IElementoDtm
        {
            return ampliacion.Elemento<T>(contexto, false);

            // (T)NegociosDeSe.CrearGestor(contexto, ampliacion.Negocio).LeerRegistroPorId(ampliacion.IdElemento, false);
        }

        public static T Elemento<T>(this IAmpliacion ampliacion, ContextoSe contexto, bool aplicarJoin = false)
        where T : IElementoDtm
        {
            return (T) AmpliacionDe(ampliacion, contexto, aplicarJoin, usarLaCache: true);

            // => (T)ampliacion.Negocio.ElementoPorId(contexto, ampliacion.IdElemento, aplicarJoin);
        }


        public static IElementoDtm AmpliacionDe(this IAmpliacion ampliacion, ContextoSe contexto, bool aplicarJoin = false, bool usarLaCache = true)
        {
            if (ampliacion.Elemento != null)
                return (IElementoDtm) ampliacion.Elemento; 
            
            var registro = (IElementoDtm)NegociosDeSe.CrearGestor(contexto, ampliacion.Negocio)
                                            .LeerRegistroPorId(ampliacion.IdElemento, aplicarJoin, usarLaCache);

            ampliacion.CargarElemento(registro);
            return registro;
        }
        



        private static T SeleccionarAmpliacion<T>(this ContextoSe contexto, int idElemento, bool errorSiNoHay = true, bool aplicarJoin = false)
        where T : IAmpliacion
        {
            var filtros = new List<ClausulaDeFiltrado>();
            var filtro = new ClausulaDeFiltrado(nameof(IAmpliacion.IdElemento), enumCriteriosDeFiltrado.igual, idElemento);
            filtros.Add(filtro);
            var gestor = typeof(T).CrearGestorDeUnaAmpliacion(contexto);
            var ampliaciones = (List<T>)gestor.LeerRegistros(0, 2, filtros, aplicarJoin);
            if (errorSiNoHay && ampliaciones.Count() == 0)
                throw new Exception($"No hay amplición {typeof(T).Name} para el id del registro pasado: {idElemento}");

            return ampliaciones.Count() == 0 ? default(T) : ampliaciones[0];
        }

        public static IAmpliacion SeleccionarAmpliacion(this ElementoDtm registro, ContextoSe contexto, Type tipo, bool errorSiNoHay = true, bool aplicarJoin = false)
        {
            var filtros = new List<ClausulaDeFiltrado>();
            var filtro = new ClausulaDeFiltrado(nameof(IAmpliacion.IdElemento), enumCriteriosDeFiltrado.igual, registro.Id);
            filtros.Add(filtro);
            var gestor = tipo.CrearGestorDeUnaAmpliacion(contexto);
            var ampliaciones = ((IEnumerable<IAmpliacion>)gestor.LeerRegistros(0, 2, filtros, aplicarJoin)).ToList();
            if (errorSiNoHay && ampliaciones.Count() == 0)
                throw new Exception($"No hay amplición '{tipo.Name}' para el id del registro pasado: {registro.Id}");

            return ampliaciones.Count() == 0 ? null : ampliaciones[0];
        }


    }

}
