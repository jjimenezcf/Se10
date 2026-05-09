using ServicioDeDatos;
using Gestor.Errores;
using GestoresDeNegocio.Ventas;
using ServicioDeDatos.Ventas;
using ModeloDeDto.Ventas;

namespace MVCSistemaDeElementos.Controllers
{
    public class PeriodosEmtController : EntidadController<ContextoSe, PeriodoEmtDtm, PeriodoEmtDto>
    {
        public PeriodosEmtController(GestorDePeriodosEmt gestorDePeriodosEmt, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDePeriodosEmt,
           gestorDeErrores
         )
        {
        }

    }
}
