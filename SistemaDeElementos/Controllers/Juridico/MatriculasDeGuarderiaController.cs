using ServicioDeDatos;
using Gestor.Errores;
using GestoresDeNegocio.Juridico;
using ModeloDeDto.Juridico;
using ServicioDeDatos.Juridico;

namespace MVCSistemaDeElementos.Controllers
{
    public class MatriculasDeGuarderiaController : EntidadController<ContextoSe, MatriculaDeGuarderiaDtm, MatriculaDeGuarderiaDto>
    {
        public MatriculasDeGuarderiaController(GestorDeMatriculasDeGuarderia gestorDeMatriculasDeGuarderia, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeMatriculasDeGuarderia,
           gestorDeErrores
         )
        {
        }

    }
}
