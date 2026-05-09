using ServicioDeDatos;
using Gestor.Errores;
using GestoresDeNegocio.Callejero;
using ServicioDeDatos.Ventas;
using ModeloDeDto.Ventas;
using Microsoft.AspNetCore.Mvc;
using MVCSistemaDeElementos.Descriptores;
using Utilidades;

namespace MVCSistemaDeElementos.Controllers
{
    public class FacturasEmtDeUnaRemesaController : RelacionController<ContextoSe, FacturaEmtDeUnaRemesaDtm, FacturaEmtDeUnaRemesaDto>
    {

        public FacturasEmtDeUnaRemesaController(GestorDeFacturasEmtDeUnaRemesa gestor, GestorDeErrores errores)
         : base
         (
           gestor,
           errores
         )
        {

        }

        [HttpPost]
        public IActionResult CrudFacturasEmtDeUnaRemesa()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);
            return ViewCrud(new DescriptorFacturasEmtDeUnaRemesa(Contexto, ModoDescriptor.Mantenimiento));
        }


    }
}
