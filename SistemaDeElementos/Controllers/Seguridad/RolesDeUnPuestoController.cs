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
    public class RolesDeUnPuestoController :  RelacionController<ContextoSe, RolesDeUnPuestoDtm, RolesDeUnPuestoDto>
    {

        public RolesDeUnPuestoController(GestorDeRolesDeUnPuesto gestor, GestorDeErrores errores)
         : base
         (
           gestor,
           errores
         )
        {
        }

        [HttpPost]
        public IActionResult CrudRolesDeUnPuesto()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext); 
            return ViewCrud(new DescriptorDeRolesDeUnPuesto(Contexto, ModoDescriptor.Mantenimiento));
        }

    }
}
