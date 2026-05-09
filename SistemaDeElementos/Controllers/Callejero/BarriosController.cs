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
    public class BarriosController : EntidadController<ContextoSe, BarrioDtm, BarrioDto>
    {
        public BarriosController(GestorDeBarrios gestorDeBarrios, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeBarrios,
           gestorDeErrores
         )
        {
        }

        public IActionResult CrudBarrios()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext); 
            return ViewCrud(new DescriptorDeBarrios(Contexto, ModoDescriptor.Mantenimiento));
        }

    }
}
