using ServicioDeDatos;
using Gestor.Errores;
using Microsoft.AspNetCore.Mvc;
using MVCSistemaDeElementos.Descriptores;
using ServicioDeDatos.Callejero;
using GestoresDeNegocio.Callejero;
using ModeloDeDto.Callejero;
using Utilidades;

namespace MVCSistemaDeElementos.Controllers
{
    public class ZonasController : EntidadController<ContextoSe, ZonaDtm, ZonaDto>
    {
        public ZonasController(GestorDeZonas gestorDeZonas, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeZonas,
           gestorDeErrores
         )
        {
        }

        public IActionResult CrudZonas()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext); 
            return ViewCrud(new DescriptorDeZonas(Contexto, ModoDescriptor.Mantenimiento));
        }

    }
}
