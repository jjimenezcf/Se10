using ServicioDeDatos;
using Gestor.Errores;
using GestoresDeNegocio.SistemaDocumental;
using ServicioDeDatos.SistemaDocumental;
using ModeloDeDto.SistemaDocumental;

namespace MVCSistemaDeElementos.Controllers
{
    public class DatosDeActividadesFormativasController : EntidadController<ContextoSe, DatosDeActividadFormativaDtm, DatosDeActividadFormativaDto>
    {
        public DatosDeActividadesFormativasController(GestorDeDatosDeActividadesFormativas gestor, GestorDeErrores gestorDeErrores)
         : base
         (
           gestor,
           gestorDeErrores
         )
        {
        }

    }
}
