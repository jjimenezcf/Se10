using ServicioDeDatos;
using Gestor.Errores;
using Microsoft.AspNetCore.Mvc;
using MVCSistemaDeElementos.Descriptores;
using ServicioDeDatos.Entorno;
using GestoresDeNegocio.Entorno;
using ModeloDeDto.Entorno;
using Utilidades;

namespace MVCSistemaDeElementos.Controllers
{
    public class AccionesController : EntidadController<ContextoSe, AccionDtm, AccionDto>
    {

        public AccionesController(GestorDeAcciones gestorDeAcciones, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeAcciones,
           gestorDeErrores
         )
        {
        }


        public IActionResult CrudDeAcciones()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext); 
            return ViewCrud(new DescriptorDeAcciones(Contexto, ModoDescriptor.Mantenimiento));
        }
    }
}
