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
    public class PuestosDeUnUsuarioController :  RelacionController<ContextoSe, PuestosDeUnUsuarioDtm, PuestosDeUnUsuarioDto>
    {

        public PuestosDeUnUsuarioController(GestorDePuestosDeUnUsuario gestor, GestorDeErrores errores)
         : base
         (
           gestor,
           errores
         )
        {
        }

        [HttpPost]
        public IActionResult CrudPuestosDeUnUsuario()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext); 
            return ViewCrud(new DescriptorDePuestosDeUnUsuario(Contexto, ModoDescriptor.Mantenimiento));
        }
    }
}
