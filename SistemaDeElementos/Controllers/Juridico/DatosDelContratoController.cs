using ServicioDeDatos;
using Gestor.Errores;
using GestoresDeNegocio.Juridico;
using ModeloDeDto.Juridico;
using ServicioDeDatos.Juridico;

namespace MVCSistemaDeElementos.Controllers
{
    public class CtrVentasController : EntidadController<ContextoSe, DatosDelContratoDtm, DatosDelContratoDto>
    {
        public CtrVentasController(GestorDeDatosDelContrato gestorDeDatosJuridicos, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeDatosJuridicos,
           gestorDeErrores
         )
        {
        }

    }
}
