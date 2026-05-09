using ServicioDeDatos;
using Gestor.Errores;
using Microsoft.AspNetCore.Mvc;
using MVCSistemaDeElementos.Descriptores;
using ServicioDeDatos.Callejero;
using ModeloDeDto.Callejero;
using GestoresDeNegocio.Callejero;
using Utilidades;

namespace MVCSistemaDeElementos.Controllers
{
    public class CallesDeUnaZonaController : RelacionController<ContextoSe, ZonasDeUnaCalleDtm, CallesDeUnaZonaDto>
    {

        public CallesDeUnaZonaController(GestorDeCallesDeUnaZona gestor, GestorDeErrores errores)
         : base
         (
           gestor,
           errores
         )
        {
        }

        [HttpPost]
        public IActionResult CrudCallesDeUnaZona()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext); 
            return ViewCrud(new DescriptorDeCallesDeUnaZona(Contexto, ModoDescriptor.Mantenimiento));
        }
    }
}
