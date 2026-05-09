using ServicioDeDatos;
using Gestor.Errores;
using GestoresDeNegocio.Juridico;
using ModeloDeDto.Juridico;
using ServicioDeDatos.Juridico;

namespace MVCSistemaDeElementos.Controllers
{
    public class AvancesController : EntidadController<ContextoSe, AvanceDtm, AvanceDto>
    {
        public AvancesController(GestorDeAvances gestorDeAvances, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeAvances,
           gestorDeErrores
         )
        {
        }

    }
}
