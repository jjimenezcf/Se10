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
    public class PuestosDeUnRolController :  RelacionController<ContextoSe, RolesDeUnPuestoDtm, PuestosDeUnRolDto>
    {

        public PuestosDeUnRolController(GestorDePuestosDeUnRol gestor, GestorDeErrores errores)
         : base
         (
           gestor,
           errores
         )
        {
        }

        [HttpPost]
        public IActionResult CrudPuestosDeUnRol()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext); 
            return ViewCrud(new DescriptorDePuestosDeUnRol(Contexto, ModoDescriptor.Mantenimiento));
        }

    }
}
