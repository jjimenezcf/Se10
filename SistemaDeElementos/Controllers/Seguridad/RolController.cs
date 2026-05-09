using ServicioDeDatos;
using Gestor.Errores;
using Microsoft.AspNetCore.Mvc;
using MVCSistemaDeElementos.Descriptores;
using ServicioDeDatos.Seguridad;
using ModeloDeDto.Seguridad;
using GestoresDeNegocio.Seguridad;
using Utilidades;

namespace MVCSistemaDeElementos.Controllers
{
    public class RolController : EntidadController<ContextoSe, RolDtm, RolDto>
    {

        public RolController(GestorDeRoles gestor, GestorDeErrores gestorDeErrores)
         : base
         (
           gestor,
           gestorDeErrores
         )
        {
        }


        public IActionResult CrudRol()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext); 
            return ViewCrud(new DescriptorDeRol(Contexto, ModoDescriptor.Mantenimiento));
        }

    }
}
