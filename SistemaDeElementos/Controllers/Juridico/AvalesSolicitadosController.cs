using ServicioDeDatos;
using Gestor.Errores;
using GestoresDeNegocio.Juridico;
using ModeloDeDto.Juridico;
using ServicioDeDatos.Juridico;

namespace MVCSistemaDeElementos.Controllers
{
    public class AvalesSolicitadosController : EntidadController<ContextoSe, AvalSolicitadoDtm, AvalSolicitadoDto>
    {
        public AvalesSolicitadosController(GestorDeAvalesSolicitados gestorDeAvalesSolicitados, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeAvalesSolicitados,
           gestorDeErrores
         )
        {
        }

    }
}
