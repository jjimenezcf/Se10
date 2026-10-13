using Utilidades;

namespace ModeloDeDto.TrabajosSometidos
{
    // Sin datos propios: MensajesWebController sólo tiene endpoints y BaseController<T> exige un dto (como MiCorreoDto en MiCorreoController)
    [IUDto(OpcionDeBorrar = false, OpcionDeEditar = false, OpcionDeCrear = false, ConMfs = false)]
    public class MensajeWebDto : ElementoDto
    {
    }
}
