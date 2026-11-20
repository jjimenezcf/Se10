using GestorDeElementos;
using GestorDeElementos.Extensores;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.TrabajosSometidos;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilidades;

namespace GestoresDeNegocio.TrabajosSometidos
{
    public static class ExtensorDeObservacionesDeTrabajos
    {
        //-------------------------------------------------------------------------------------------------------------
        // Observación de administrador en el elemento sobre el que ha trabajado el job: el asunto es el nombre del trabajo
        // seguido, entre paréntesis, de las veces que se ha ejecutado sobre el elemento (1, 2, 3...) y el cuerpo la url del
        // trabajo. Es informativa: si falla sólo se anota en la traza.
        // Si al someter el trabajo ya se anotó (ver AnotarElTrabajoSometidoEnElElemento) no se repite
        //-------------------------------------------------------------------------------------------------------------
        public static void AnotarElTrabajoEnElElemento<T>(this EntornoDeTrabajo entorno, ContextoSe contexto, T elemento, string nombreDeLaObservacion = null)
        where T : IUsaObservacion, IRegistro
        {
            // si no se indica el nombre de la observación se usa el del trabajo sometido
            var nombreDelTrabajo = nombreDeLaObservacion.IsNullOrEmpty() ? entorno.TrabajoSometido.Nombre : nombreDeLaObservacion;
            try
            {
                CrearLaObservacion(contexto, elemento, entorno.TrabajoDeUsuario.Id, nombreDelTrabajo);
            }
            catch (Exception e)
            {
                entorno.CrearTraza($"No se ha podido crear la observación '{nombreDelTrabajo}' en el elemento '{elemento.Id}': {e.Message}");
            }
        }

        //-------------------------------------------------------------------------------------------------------------
        // Lo mismo pero al someter el trabajo, nada más crearlo: así desde el elemento se puede navegar al trabajo para pararlo o
        // ejecutarlo interactivamente sin esperar a que lo inicie la cola, y el enlace sigue ahí aunque el trabajo falle.
        // Es informativa: si el usuario que somete no puede anotar en el elemento no se anota (lo hará el propio trabajo al empezar)
        // y no impide someterlo. Devuelve el trabajo recibido para poder encadenarla con la creación
        //-------------------------------------------------------------------------------------------------------------
        public static TrabajoDeUsuarioDtm AnotarElTrabajoSometidoEnElElemento<T>(this TrabajoDeUsuarioDtm trabajo, ContextoSe contexto, T elemento, string nombreDeLaObservacion)
        where T : IUsaObservacion, IRegistro
        {
            if (trabajo is null || trabajo.Id <= 0)
                return trabajo;

            try
            {
                CrearLaObservacion(contexto, elemento, trabajo.Id, nombreDeLaObservacion);
            }
            catch
            {
                // se anotará cuando se ejecute el trabajo
            }
            return trabajo;
        }

        private static void CrearLaObservacion<T>(ContextoSe contexto, T elemento, int idTrabajoDeUsuario, string nombreDelTrabajo)
        where T : IUsaObservacion, IRegistro
        {
            var negocio = elemento.GetType().NegocioDeUnDtm();
            var idElemento = elemento.Id;
            var urlDelTrabajo = new UriBuilder(CacheDeVariable.Cfg_UrlBase)
            {
                Path = $"/{enumControladoresEntorno.TrabajosDeUsuario}/{enumVistasEntorno.CrudDeTrabajosDeUsuario}",
                Query = $"id={idTrabajoDeUsuario}"
            }.Uri.ToString();

            var observaciones = negocio.Observaciones(contexto);
            if (observaciones.Any(o => o.IdElemento == idElemento && o.Descripcion == urlDelTrabajo))
                return;

            var lanzado = observaciones.Count(o => o.IdElemento == idElemento && o.Nombre.StartsWith(nombreDelTrabajo)) + 1;
            elemento.CrearObservacion(contexto, $"{nombreDelTrabajo} ({lanzado})", urlDelTrabajo,
                new Dictionary<string, object> { { ltrDeObservaciones.CreadaPorAdminSe, true } });
        }
    }
}
