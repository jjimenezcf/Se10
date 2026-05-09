using ServicioDeDatos;
using Gestor.Errores;
using ServicioDeDatos.Seguridad;
using ModeloDeDto.Seguridad;
using GestoresDeNegocio.Seguridad;
using Microsoft.AspNetCore.Mvc;
using Utilidades;
using System.Collections.Generic;
using GestorDeElementos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Terceros;
using System.Linq;

namespace MVCSistemaDeElementos.Controllers
{
    public class PermisosPorCgController : EntidadController<ContextoSe, PermisosPorCgDtm, PermisosPorCgDto>
    {
        public PermisosPorCgController(GestorDePermisosPorCg gestorDePermisos, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDePermisos,
           gestorDeErrores
         )
        {
        }

        public JsonResult epBorrarRelacionPorId(int id, string parametrosJson)
        {
            var parametros = Utilidades.extJson.ToDiccionarioDeParametros(parametrosJson);
            return BorrarIds(new List<int> {id}, parametros);
        }

        public override JsonResult epCrearRelacion(int idNegocio, string elementoJson)
        {
            return ApiController.PersistirElemento(new GestorDePermisosPorCg(Contexto, Contexto.Mapeador), elementoJson, HttpContext, AntesDeEjecutar);
        }

        private ParametrosDeNegocio AntesDeEjecutar(PermisosPorCgDto permisoPorCg)
        {
            var negociosDeUnCg = Contexto.Set<NegociosDeUnCgDtm>().Where(x => x.IdConsultor == permisoPorCg.IdPermiso || x.IdGestor == permisoPorCg.IdPermiso).ToList();
            permisoPorCg.IdNegocio = negociosDeUnCg[0].IdNegocio;
            permisoPorCg.IdCg = negociosDeUnCg[0].IdCg;
            permisoPorCg.Calculado = false;
            return new ParametrosDeNegocio(enumTipoOperacion.Insertar);
        }

        protected override IEnumerable<PermisosPorCgDto> LeerElementos(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden, Dictionary<string, object> opcionesDeMapeo)
        {
            //filtros.Add(new ClausulaDeFiltrado { Clausula = nameof(PermisosPorNegocioDto.Calculado), Criterio = enumCriteriosDeFiltrado.igual, Valor = false.ToString() });
            return base.LeerElementos(posicion, cantidad, filtros, orden, opcionesDeMapeo);
        }

    }

}
