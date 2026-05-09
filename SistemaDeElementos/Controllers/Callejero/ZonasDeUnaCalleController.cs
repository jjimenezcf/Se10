using ServicioDeDatos;
using Gestor.Errores;
using ServicioDeDatos.Callejero;
using ModeloDeDto.Callejero;
using GestoresDeNegocio.Callejero;

namespace MVCSistemaDeElementos.Controllers
{
    public class ZonasDeUnaCalleController : RelacionController<ContextoSe, ZonasDeUnaCalleDtm, ZonasDeUnaCalleDto>
    {

        public ZonasDeUnaCalleController(GestorDeZonasDeUnaCalle gestor, GestorDeErrores errores)
         : base
         (
           gestor,
           errores
         )
        {
        }

    }
}
