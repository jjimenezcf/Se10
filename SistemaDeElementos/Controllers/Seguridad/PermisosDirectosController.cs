using ServicioDeDatos;
using Gestor.Errores;
using ServicioDeDatos.Seguridad;
using GestoresDeNegocio.Seguridad;
using ModeloDeDto.Seguridad;

namespace MVCSistemaDeElementos.Controllers
{
    public class PermisosDirectosController : RelacionController<ContextoSe, PermisosDirectosDtm, PermisosDeUnPuestoDto>
    {
         public PermisosDirectosController(GestorDePermisosDirectos gestor, GestorDeErrores errores)
         : base
         (
           gestor,
           errores
         )
        {
        }


    }
}
