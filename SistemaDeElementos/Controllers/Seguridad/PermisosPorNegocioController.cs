using ServicioDeDatos;
using Gestor.Errores;
using ServicioDeDatos.Seguridad;
using ModeloDeDto.Seguridad;
using GestoresDeNegocio.Seguridad;
using Microsoft.AspNetCore.Mvc;
using Utilidades;
using System.Collections.Generic;
using GestorDeElementos;

namespace MVCSistemaDeElementos.Controllers
{
    public class PermisosPorNegocioController : EntidadController<ContextoSe, PermisosPorNegocioDtm, PermisosPorNegocioDto>
    {
        public PermisosPorNegocioController(GestorDePermisosPorNegocio gestorDePermisos, GestorDeErrores gestorDeErrores)
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
            return ApiController.PersistirElemento(new GestorDePermisosPorNegocio(Contexto, Contexto.Mapeador), elementoJson, HttpContext, AntesDeEjecutar);
        }

        private ParametrosDeNegocio AntesDeEjecutar(PermisosPorNegocioDto permisoPorNegocio)
        {
            var gestor = GestoresDeNegocio.Negocio.GestorDeNegocios.Gestor(Contexto, Contexto.Mapeador);
            var filtro = new ClausulaDeFiltrado(nameof(ltrDeUnPermisoDtm.PermisosDeNegocio), enumCriteriosDeFiltrado.igual, permisoPorNegocio.IdPermiso.ToString());

            var negocios = gestor.LeerRegistros(0, -1, new List<ClausulaDeFiltrado> { filtro }, false);
            if (negocios.Count != 1) GestorDeErrores.Emitir($"El permiso {GestorDePermisos.LeerRegistroPorId(Contexto,permisoPorNegocio.IdPermiso).Nombre} sólo se ha de relacionar con un negocio");

            permisoPorNegocio.IdNegocio = negocios[0].Id;
            permisoPorNegocio.Calculado = false;
            return new ParametrosDeNegocio(enumTipoOperacion.Insertar);
        }

        protected override IEnumerable<PermisosPorNegocioDto> LeerElementos(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden, Dictionary<string, object> opcionesDeMapeo)
        {
            //filtros.Add(new ClausulaDeFiltrado { Clausula = nameof(PermisosPorNegocioDto.Calculado), Criterio = enumCriteriosDeFiltrado.igual, Valor = false.ToString() });
            return base.LeerElementos(posicion, cantidad, filtros, orden, opcionesDeMapeo);
        }

    }

}
