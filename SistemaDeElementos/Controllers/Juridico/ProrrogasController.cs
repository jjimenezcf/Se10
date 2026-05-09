using ServicioDeDatos;
using Gestor.Errores;
using GestoresDeNegocio.Juridico;
using ModeloDeDto.Juridico;
using ServicioDeDatos.Juridico;

namespace MVCSistemaDeElementos.Controllers
{
    public class ProrrogasController : EntidadController<ContextoSe, ProrrogaDtm, ProrrogaDto>
    {
        public ProrrogasController(GestorDeProrrogas gestorDeProrrogas, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeProrrogas,
           gestorDeErrores
         )
        {
        }

    }
}
