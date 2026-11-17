using GestorDeElementos;
using GestorDeElementos.Extensores;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
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
        // trabajo. Es informativa: si falla sólo se anota en la traza
        //-------------------------------------------------------------------------------------------------------------
        public static void AnotarElTrabajoEnElElemento<T>(this EntornoDeTrabajo entorno, ContextoSe contexto, T elemento, string nombreDeLaObservacion = null)
        where T : IUsaObservacion, IRegistro
        {
            // si no se indica el nombre de la observación se usa el del trabajo sometido
            var nombreDelTrabajo = nombreDeLaObservacion.IsNullOrEmpty() ? entorno.TrabajoSometido.Nombre : nombreDeLaObservacion;
            try
            {
                var negocio = elemento.GetType().NegocioDeUnDtm();
                var idElemento = elemento.Id;
                var lanzado = negocio.Observaciones(contexto)
                    .Count(o => o.IdElemento == idElemento && o.Nombre.StartsWith(nombreDelTrabajo)) + 1;
                var urlDelTrabajo = new UriBuilder(CacheDeVariable.Cfg_UrlBase)
                {
                    Path = $"/{enumControladoresEntorno.TrabajosDeUsuario}/{enumVistasEntorno.CrudDeTrabajosDeUsuario}",
                    Query = $"id={entorno.TrabajoDeUsuario.Id}"
                }.Uri.ToString();
                elemento.CrearObservacion(contexto, $"{nombreDelTrabajo} ({lanzado})", urlDelTrabajo,
                    new Dictionary<string, object> { { ltrDeObservaciones.CreadaPorAdminSe, true } });
            }
            catch (Exception e)
            {
                entorno.CrearTraza($"No se ha podido crear la observación '{nombreDelTrabajo}' en el elemento '{elemento.Id}': {e.Message}");
            }
        }
    }
}
