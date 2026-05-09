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
    public class RolesDeUnPermisoController : RelacionController<ContextoSe, PermisosDeUnRolDtm, RolesDeUnPermisoDto>
    {
        public RolesDeUnPermisoController(GestorDeRolesDeUnPermiso gestor, GestorDeErrores errores)
        : base
        (
          gestor,
          errores
        )
        {
        }

        [HttpPost]
        public IActionResult CrudRolesDeUnPermiso()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext); 
            return ViewCrud(new DescriptorDeRolesDeUnPermiso(Contexto, ModoDescriptor.Mantenimiento));
        }

    }
}

