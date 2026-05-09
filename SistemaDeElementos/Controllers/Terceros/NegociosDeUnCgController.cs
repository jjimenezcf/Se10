using Gestor.Errores;
using GestoresDeNegocio.Terceros;
using ModeloDeDto.Terceros;
using ServicioDeDatos;
using ServicioDeDatos.Terceros;

namespace MVCSistemaDeElementos.Controllers
{
    public class NegociosDeUnCgController : EntidadController<ContextoSe, NegociosDeUnCgDtm, NegociosDeUnCgDto>
    {
        public NegociosDeUnCgController(GestorDeNegociosDeUnCg gestorDeNegociosDeUnCg, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeNegociosDeUnCg,
           gestorDeErrores
         )
        {
        }
    }
}
