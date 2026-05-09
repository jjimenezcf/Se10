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
    public class PermisosHeredadosController : EntidadController<ContextoSe, PermisosHeredadosDtm, PermisosDeUnPuestoDto>
    {
         public PermisosHeredadosController(GestorDePermisosHeredados gestor, GestorDeErrores errores)
         : base
         (
           gestor,
           errores
         )
        {
        }

        [HttpPost]
        public IActionResult CrudPermisosHeredados()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext); 
            return ViewCrud(new DescriptorDePermisosHeredados(Contexto, ModoDescriptor.Mantenimiento));
        }

    }
}
