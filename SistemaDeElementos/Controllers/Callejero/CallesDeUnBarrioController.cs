using ServicioDeDatos;
using Gestor.Errores;
using Microsoft.AspNetCore.Mvc;
using MVCSistemaDeElementos.Descriptores;
using ServicioDeDatos.Callejero;
using ModeloDeDto.Callejero;
using GestoresDeNegocio.Callejero;
using Utilidades;

namespace MVCSistemaDeElementos.Controllers
{
    public class CallesDeUnBarrioController : RelacionController<ContextoSe, BarriosDeUnaCalleDtm, CallesDeUnBarrioDto>
    {

        public CallesDeUnBarrioController(GestorDeCallesDeUnBarrio gestor, GestorDeErrores errores)
         : base
         (
           gestor,
           errores
         )
        {
        }

        [HttpPost]
        public IActionResult CrudCallesDeUnBarrio()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext); 
            return ViewCrud(new DescriptorDeCallesDeUnBarrio(Contexto, ModoDescriptor.Mantenimiento));
        }
    }
}
