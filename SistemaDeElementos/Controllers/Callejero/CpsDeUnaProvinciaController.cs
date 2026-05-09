using ServicioDeDatos;
using Gestor.Errores;
using Microsoft.AspNetCore.Mvc;
using MVCSistemaDeElementos.Descriptores;
using ServicioDeDatos.Callejero;
using ModeloDeDto.Callejero;
using GestoresDeNegocio.Callejero;
using GestorDeElementos;
using System.Collections.Generic;
using Utilidades;

namespace MVCSistemaDeElementos.Controllers
{
    public class CpsDeUnaProvinciaController :  RelacionController<ContextoSe, CpsDeUnaProvinciaDtm, CpsDeUnaProvinciaDto>
    {

        public CpsDeUnaProvinciaController(GestorDeCpsDeUnaProvincia gestor, GestorDeErrores errores)
         : base
         (
           gestor,
           errores
         )
        {
        }

        [HttpPost]
        public IActionResult CrudCpsDeUnaProvincia()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext); 
            return ViewCrud(new DescriptorDeCpsDeUnaProvincia(Contexto, ModoDescriptor.Mantenimiento));
        }


        protected override dynamic CargaDinamica(string claseElemento, int posicion, int cantidad, List<ClausulaDeFiltrado> filtros)
        {
            if (claseElemento == nameof(CodigoPostalDto))
                return GestorDeCodigosPostales.Gestor(_GestorDeElementos.Contexto, _GestorDeElementos.Mapeador).LeerCodigosPostales(posicion, cantidad, filtros);

            return base.CargaDinamica(claseElemento, posicion, cantidad, filtros);
        }

    }
}
