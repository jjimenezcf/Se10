using ServicioDeDatos;
using Gestor.Errores;
using Microsoft.AspNetCore.Mvc;
using MVCSistemaDeElementos.Descriptores;
using ServicioDeDatos.Juridico;
using ModeloDeDto.Juridico;
using GestoresDeNegocio.Juridico;
using Utilidades;

namespace MVCSistemaDeElementos.Controllers
{
    public class UnitariosDeUnLoteController : RelacionController<ContextoSe, UnitariosDeUnLoteDtm, UnitariosDeUnLoteDto>
    {

        public UnitariosDeUnLoteController(GestorDeUnitariosDeUnLote gestor, GestorDeErrores errores)
         : base
         (
           gestor,
           errores
         )
        {
        }

        [HttpPost]
        public IActionResult CrudUnitariosDeUnLote()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext); 
            return ViewCrud(new DescriptorDeUnitariosDeUnLote(Contexto, ModoDescriptor.Mantenimiento));
        }
    }
}
