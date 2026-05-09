using ServicioDeDatos;
using Gestor.Errores;
using ServicioDeDatos.Callejero;
using ModeloDeDto.Callejero;
using GestoresDeNegocio.Callejero;

namespace MVCSistemaDeElementos.Controllers
{
    public class CpsDeUnaCalleController : RelacionController<ContextoSe, CpsDeUnaCalleDtm, CpsDeUnaCalleDto>
    {

        public CpsDeUnaCalleController(GestorDeCpsDeUnaCalle gestor, GestorDeErrores errores)
         : base
         (
           gestor,
           errores
         )
        {
        }

    }
}
