using ServicioDeDatos;
using Gestor.Errores;
using ServicioDeDatos.Callejero;
using ModeloDeDto.Callejero;
using GestoresDeNegocio.Callejero;

namespace MVCSistemaDeElementos.Controllers
{
    public class BarriosDeUnaCalleController : RelacionController<ContextoSe, BarriosDeUnaCalleDtm, BarriosDeUnaCalleDto>
    {

        public BarriosDeUnaCalleController(GestorDeBarriosDeUnaCalle gestor, GestorDeErrores errores)
         : base
         (
           gestor,
           errores
         )
        {
        }

    }
}
