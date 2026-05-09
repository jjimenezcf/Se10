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
    public class PermisosDeUnRolController : RelacionController<ContextoSe, PermisosDeUnRolDtm, PermisosDeUnRolDto>
    {
         public PermisosDeUnRolController(GestorDePermisosDeUnRol gestor, GestorDeErrores errores)
         : base
         (
           gestor,
           errores
         )
        {
        }

        [HttpPost]
        public IActionResult CrudPermisosDeUnRol()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext); 
            return ViewCrud(new DescriptorDePermisosDeUnRol(Contexto, ModoDescriptor.Mantenimiento));
        }
    }
}
