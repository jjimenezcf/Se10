using ServicioDeDatos;
using Gestor.Errores;
using Microsoft.AspNetCore.Mvc;
using MVCSistemaDeElementos.Descriptores;
using ServicioDeDatos.Seguridad;
using GestoresDeNegocio.Seguridad;
using ModeloDeDto.Seguridad;
using Utilidades;

namespace MVCSistemaDeElementos.Controllers
{
    public class UsuariosDeUnPuestoController :  RelacionController<ContextoSe, PuestosDeUnUsuarioDtm, UsuariosDeUnPuestoDto>
    {

        public UsuariosDeUnPuestoController(GestorDeUsuariosDeUnPuesto gestor, GestorDeErrores errores)
         : base
         (
           gestor,
           errores
         )
        {
        }

        [HttpPost]
        public IActionResult CrudUsuariosDeUnPuesto()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext); 
            return ViewCrud(new DescriptorDeUsuariosDeUnPuesto(Contexto, ModoDescriptor.Mantenimiento));
        }

    }
}
