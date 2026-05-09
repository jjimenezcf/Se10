using ServicioDeDatos;
using Gestor.Errores;
using GestoresDeNegocio.Juridico;
using ModeloDeDto.Juridico;
using ServicioDeDatos.Juridico;

namespace MVCSistemaDeElementos.Controllers
{
    public class RecobrosController : EntidadController<ContextoSe, RecobroDtm, RecobroDto>
    {
        public RecobrosController(GestorDeRecobros gestorDeRecobros, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeRecobros,
           gestorDeErrores
         )
        {
        }

    }
}
