using ServicioDeDatos;
using Gestor.Errores;
using GestoresDeNegocio.Juridico;
using ModeloDeDto.Juridico;
using ServicioDeDatos.Juridico;

namespace MVCSistemaDeElementos.Controllers
{
    public class SaldosDelContratoController : EntidadController<ContextoSe, SaldosDelContratoDtm, SaldosDelContratoDto>
    {
        public SaldosDelContratoController(GestorDeSaldosDelContrato gestorDeSaldosDelContrato, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeSaldosDelContrato,
           gestorDeErrores
         )
        {
        }

    }
}
