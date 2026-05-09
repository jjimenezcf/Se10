using ServicioDeDatos;
using Gestor.Errores;
using Microsoft.AspNetCore.Mvc;
using MVCSistemaDeElementos.Descriptores;
using ServicioDeDatos.Negocio;
using ModeloDeDto.Negocio;
using GestoresDeNegocio.Negocio;
using Utilidades;

namespace MVCSistemaDeElementos.Controllers
{
    public class AccionesDeRelacionController : EntidadController<ContextoSe, AccionesDeRelacionDtm, AccionesDeRelacionDto>
    {

        public AccionesDeRelacionController(GestorDeAccionesDeRelacion gestorDeAcciones, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeAcciones,
           gestorDeErrores
         )
        {
        }


        public IActionResult CrudDeAccionesDeRelacion()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext); 
            return ViewCrud(new DescriptorDeAccionesDeRelacion(Contexto, ModoDescriptor.Mantenimiento));
        }
    }
}
