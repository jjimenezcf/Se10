using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using GestoresDeNegocio.TrabajosSometidos;
using ServicioDeDatos;
using ServicioDeDatos.TrabajosSometidos;
using Utilidades;

namespace TestLogistica
{
    // Ejecuta un trabajo sometido como TrabajoDeUsuarioDtm.EjecutarTrabajo (GestorDeTrabajosDeUsuario.Iniciar), pero con el contexto de la prueba
    // como contexto del proceso. Iniciar crea el del proceso en otro scope (ContextoSe.Crear), es decir, en otra conexión fuera de la transacción
    // de la prueba: no ve lo que la prueba ha creado sin confirmar (SE_TEST no tiene READ_COMMITTED_SNAPSHOT, se quedaría esperando a sus
    // bloqueos) y lo que no haga dentro de su transacción no se deshace con el rollback. Aquí todo va en la transacción de la prueba: la
    // del trabajo no se abre por existir ya (IniciarTransaccion) y sus trazas y errores se anotan con el mismo contexto.
    // No se pone el semáforo del trabajo (Dapper, en otra conexión) ni se comunica el fin por correo
    public static class EjecutorDeTrabajos
    {
        public class Resultado
        {
            public List<string> Errores { get; init; }
            public List<string> Trazas { get; init; }
        }

        public static Resultado Ejecutar(ContextoSe contexto, TrabajoDeUsuarioDtm trabajo)
        {
            var entornoAnterior = contexto.Entorno;
            var trazarAnterior = contexto.TrazarEnElTrabajo;
            var nombreAnterior = contexto.NombreDelTrabajo;

            var entorno = new EntornoDeTrabajo(GestorDeTrabajosDeUsuario.GestorTu(contexto), trabajo);
            entorno.contextoDelProceso = contexto;
            contexto.Entorno = entorno;
            contexto.TrazarEnElTrabajo = entorno.CrearTraza;
            try
            {
                var metodo = ApiDeEnsamblados.ObtenerMetodoEstatico(entorno.TrabajoSometido.Dll, entorno.TrabajoSometido.Clase, entorno.TrabajoSometido.Metodo);
                try
                {
                    metodo.Invoke(null, new object[] { entorno });
                }
                catch (TargetInvocationException e) when (e.InnerException is not null)
                {
                    ExceptionDispatchInfo.Capture(e.InnerException).Throw();
                }
            }
            finally
            {
                contexto.Entorno = entornoAnterior;
                contexto.TrazarEnElTrabajo = trazarAnterior;
                contexto.NombreDelTrabajo = nombreAnterior;
                contexto.EsElContextosDeUnEntorno = false;
            }

            return new Resultado
            {
                Errores = contexto.Set<ErrorDeUnTrabajoDtm>().Where(e => e.IdTrabajoDeUsuario == trabajo.Id).OrderBy(e => e.Id).Select(e => e.Error).ToList(),
                Trazas = contexto.Set<TrazaDeUnTrabajoDtm>().Where(t => t.IdTrabajoDeUsuario == trabajo.Id).OrderBy(t => t.Id).Select(t => t.Traza).ToList()
            };
        }
    }
}
