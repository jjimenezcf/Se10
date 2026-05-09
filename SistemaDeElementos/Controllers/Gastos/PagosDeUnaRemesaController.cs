using ServicioDeDatos;
using Gestor.Errores;
using Microsoft.AspNetCore.Mvc;
using MVCSistemaDeElementos.Descriptores;
using ServicioDeDatos.Gastos;
using ModeloDeDto.Gastos;
using GestoresDeNegocio.Gastos;
using Utilidades;

namespace MVCSistemaDeElementos.Controllers
{
    public class PagosDeUnaRemesaController : RelacionController<ContextoSe, PagoDeUnaRemesaDtm, PagoDeUnaRemesaDto>
    {

        public PagosDeUnaRemesaController(GestorDePagosDeUnaRemesa gestor, GestorDeErrores errores)
         : base
         (
           gestor,
           errores
         )
        {

        }

        [HttpPost]
        public IActionResult CrudPagosDeUnaRemesa()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);
            return ViewCrud(new DescriptorPagosDeUnaRemesa(Contexto, ModoDescriptor.Mantenimiento));
        }


    }
}
