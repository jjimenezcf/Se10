using ServicioDeDatos;
using Gestor.Errores;
using Microsoft.AspNetCore.Mvc;
using MVCSistemaDeElementos.Descriptores;
using ServicioDeDatos.Juridico;
using GestoresDeNegocio.Juridico;
using ModeloDeDto.Juridico;
using GestoresDeNegocio.MaestrosTecnico;
using Utilidades;

namespace MVCSistemaDeElementos.Controllers
{
    public class LotesDeUnContratoController : EntidadController<ContextoSe, LoteDeUnContratoDtm, LoteDeUnContratoDto>
    {
        public LotesDeUnContratoController(GestorDeLotesDeUnContrato gestorDeLotes, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeLotes,
           gestorDeErrores
         )
        {
        }

        public IActionResult CrudLotes()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext); 
            return ViewCrud(new DescriptorDeLotesDeUnContrato(Contexto, ModoDescriptor.Mantenimiento));
        }

        public JsonResult epBorrarRelacionPorId(int id, string parametrosJson) =>
        ApiController.BorrarPorId(new GestorDeLotesDeUnContrato(Contexto, Contexto.Mapeador), id, parametrosJson, HttpContext, AntesDeEjecutar_BorrarPorId);

    }
}
