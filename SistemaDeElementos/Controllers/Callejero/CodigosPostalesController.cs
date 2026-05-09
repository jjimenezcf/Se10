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
    public class CodigosPostalesController : EntidadController<ContextoSe, CodigoPostalDtm, CodigoPostalDto>
    {

        public CodigosPostalesController(GestorDeCodigosPostales gestorDeCodigosPostales, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeCodigosPostales,
           gestorDeErrores
         )
        {
        }

        public IActionResult CrudCodigosPostales()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext); 
            return ViewCrud(new DescriptorDeCodigosPostales(Contexto, ModoDescriptor.Mantenimiento));
        }

    }
}
