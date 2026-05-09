using Gestor.Errores;
using GestoresDeNegocio.Seguridad;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos;
using ModeloDeDto.Seguridad;

namespace MVCSistemaDeElementos.Controllers
{
    public class TipoDePermisoController : EntidadController<ContextoSe, TipoPermisoDtm, TipoPermisoDto>
    {
        public TipoDePermisoController(GestorDeTipoPermiso gestor, GestorDeErrores errores)
        : base
        (
         gestor,
         errores
        )
        {
        }
    }
}
