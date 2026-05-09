using ServicioDeDatos;
using Gestor.Errores;
using ServicioDeDatos.Tarea;
using ModeloDeDto.Tarea;
using GestoresDeNegocio.Tarea;

namespace MVCSistemaDeElementos.Controllers
{
    public class PlfDeTareasController : EntidadController<ContextoSe, PlfDeTareaDtm, PlfDeTareaDto>
    {
        public PlfDeTareasController(GestorDePlfDeTareas gestorDePlfDeTareas, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDePlfDeTareas,
           gestorDeErrores
         )
        {
        }

    }
}
