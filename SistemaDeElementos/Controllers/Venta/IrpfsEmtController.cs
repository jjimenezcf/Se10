using ServicioDeDatos;
using Gestor.Errores;
using GestoresDeNegocio.Ventas;
using ServicioDeDatos.Ventas;
using ModeloDeDto.Ventas;

namespace MVCSistemaDeElementos.Controllers
{
    public class IrpfsEmtController : EntidadController<ContextoSe, IrpfEmtDtm, IrpfEmtDto>
    {
        public IrpfsEmtController(GestorDeIrpfsEmt gestorDeIrpfsEmt, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeIrpfsEmt,
           gestorDeErrores
         )
        {
        }

    }
}
