using ServicioDeDatos;
using Gestor.Errores;
using GestoresDeNegocio.Expediente;
using ServicioDeDatos.Expediente;
using ModeloDeDto.Expediente;

namespace MVCSistemaDeElementos.Controllers
{
    public class DatosJuridicosController : EntidadController<ContextoSe, DatosJuridicosDtm, DatosJuridicosDto>
    {
        public DatosJuridicosController(GestorDeDatosJuridicos gestor, GestorDeErrores gestorDeErrores)
         : base
         (
           gestor,
           gestorDeErrores
         )
        {
        }

    }
}
