using ServicioDeDatos;
using Gestor.Errores;
using GestoresDeNegocio.Negocio;
using ModeloDeDto.Negocio;
using ServicioDeDatos.Negocio;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace MVCSistemaDeElementos.Controllers
{
    public class ClasesDelNegocioController : EntidadController<ContextoSe, ClaseDelNegocioDtm, ClaseDelNegocioDto>
    {

        public ClasesDelNegocioController(GestorDeClasesDelNegocio gestorDeClasesDelNegocio, GestorDeErrores gestorDeErrores)
        :base
        (
          gestorDeClasesDelNegocio, 
          gestorDeErrores
        )
        {

        }

        protected override IEnumerable<ClaseDelNegocioDto> LeerElementos(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden, Dictionary<string, object> parametros)
        {
            return base.LeerElementos(posicion, cantidad, filtros, orden, parametros);
        }

        public override JsonResult epCrearRelacion(int idNegocio, string elementoJson) =>
        ApiController.PersistirElemento(new GestorDeClasesDelNegocio(Contexto, Contexto.Mapeador), elementoJson, HttpContext, AntesDeEjecutar_CrearElemento);

        public JsonResult epBorrarRelacionPorId(int id, string parametrosJson) =>
        ApiController.BorrarPorId(new GestorDeClasesDelNegocio(Contexto, Contexto.Mapeador), id, parametrosJson, HttpContext, AntesDeEjecutar_BorrarPorId);

    }

}
