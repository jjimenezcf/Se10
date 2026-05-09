using Microsoft.AspNetCore.Mvc;
using ServicioDeDatos;
using Gestor.Errores;
using MVCSistemaDeElementos.Descriptores;
using ServicioDeDatos.TrabajosSometidos;
using ModeloDeDto.TrabajosSometidos;
using GestoresDeNegocio.TrabajosSometidos;
using Utilidades;

namespace MVCSistemaDeElementos.Controllers
{
    public class TrabajosSometidoController : EntidadController<ContextoSe, TrabajoSometidoDtm, TrabajoSometidoDto>
    {

        public TrabajosSometidoController(GestorDeTrabajosSometido gestorDeTrabajos, GestorDeErrores gestorDeErrores)
        :base
        (
          gestorDeTrabajos, 
          gestorDeErrores
        )
        {

        }

        
        public IActionResult CrudDeTrabajosSometido()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext); 
            return ViewCrud(new DescriptorDeTrabajosSometido(Contexto, ModoDescriptor.Mantenimiento));
        }

    }

}
