using ServicioDeDatos;
using Gestor.Errores;
using Microsoft.AspNetCore.Mvc;
using MVCSistemaDeElementos.Descriptores;
using ServicioDeDatos.Entorno;
using ModeloDeDto.Entorno;
using GestoresDeNegocio.Entorno;
using Utilidades;

namespace MVCSistemaDeElementos.Controllers
{
    public class PermisosDeUnUsuarioController : EntidadController<ContextoSe, UsuariosDeUnPermisoDtm, PermisosDeUnUsuarioDto>
    {
         public PermisosDeUnUsuarioController(GestorDePermisosDeUnUsuario gestor, GestorDeErrores errores)
         : base
         (
           gestor,
           errores
         )
        {
        }

        [HttpPost]
        public IActionResult CrudPermisosDeUnUsuario()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext); 
            return ViewCrud(new DescriptorDePermisosDeUnUsuario(Contexto, ModoDescriptor.Mantenimiento));
        }

    }
}
