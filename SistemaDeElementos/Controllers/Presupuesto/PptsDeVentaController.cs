using ServicioDeDatos;
using Gestor.Errores;
using GestoresDeNegocio.Presupuesto;
using ModeloDeDto.Presupuesto;
using ServicioDeDatos.Presupuesto;

namespace MVCSistemaDeElementos.Controllers
{
    public class PptsDeVentaController : EntidadController<ContextoSe, PptDeVentaDtm, PptDeVentaDto>
    {
        public PptsDeVentaController(GestorDePptsDeVenta gestorDePptsDeVenta, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDePptsDeVenta,
           gestorDeErrores
         )
        {
        }

    }
}
