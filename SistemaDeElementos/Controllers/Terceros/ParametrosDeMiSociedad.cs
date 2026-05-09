using ServicioDeDatos;
using Gestor.Errores;
using GestoresDeNegocio.Terceros;
using ServicioDeDatos.Terceros;
using ModeloDeDto.Terceros;

namespace MVCSistemaDeElementos.Controllers
{
    public class ParametrosDeMiSociedadController : EntidadController<ContextoSe, ParametrosDeMiSociedadDtm, ParametrosDeMiSociedadDto>
    {
        public ParametrosDeMiSociedadController(GestorDeParametrosDeMiSociedad gestorDeParametrosDeMiSociedad, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeParametrosDeMiSociedad,
           gestorDeErrores
         )
        {
        }

    }
}
