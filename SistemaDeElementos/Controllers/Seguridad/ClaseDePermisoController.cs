using Microsoft.AspNetCore.Mvc;
using Gestor.Errores;
using MVCSistemaDeElementos.Descriptores;
using GestoresDeNegocio.Seguridad;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos;
using ModeloDeDto.Seguridad;
using Utilidades;

namespace MVCSistemaDeElementos.Controllers
{
    public class ClaseDePermisoController : EntidadController<ContextoSe, ClasePermisoDtm, ClasePermisoDto>
    {
        public ClaseDePermisoController(GestorDeClaseDePermisos gestor, GestorDeErrores errores)
        : base 
        (
         gestor,
         errores
        )
        {
        }

        public IActionResult CrudClaseDePermiso()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext); 
            return ViewCrud(new DescriptorDeClaseDePermiso(Contexto, ModoDescriptor.Mantenimiento));
        }
    }
}
