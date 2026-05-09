using ServicioDeDatos;
using ServicioDeDatos.TrabajosSometidos;
using System.Linq;

namespace GestorDeElementos.Extensores
{
    public static class ExtensorDeTrabajos
    {
        public static TrabajoDeUsuarioDtm UltimaEjecucion(this TrabajoSometidoDtm trabajo, ContextoSe contexto)
        {
            var ejecucion = contexto.Set<TrabajoDeUsuarioDtm>().Where(tu => tu.IdTrabajo == trabajo.Id 
                   && tu.Estado == enumEstadosDeUnTrabajo.Terminado.ToDtm()).OrderByDescending(tu => tu.Id).Take(1).ToList();

            return ejecucion.Count == 1 ? ejecucion[0] : null;
        }
    }
}
