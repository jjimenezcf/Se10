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
    public class PermisosPorElementoController : EntidadController<ContextoSe, PermisosPorElementoDtm, PermisosPorElementoDto>
    {
        public PermisosPorElementoController(GestorDePermisosPorElemento gestorDePermisos, GestorDeErrores gestorDeErrores)
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
            return BorrarIds(new List<int> { id }, parametros);
        }

        public override JsonResult epCrearRelacion(int idNegocio, string elementoJson)
        {
            return ApiController.PersistirElemento(new GestorDePermisosPorElemento(Contexto, Contexto.Mapeador), elementoJson, HttpContext, AntesDeEjecutar);
        }

        private ParametrosDeNegocio AntesDeEjecutar(PermisosPorElementoDto permisoPorElemento)
        {
            var permiso = GestorDePermisos.LeerRegistroPorId(Contexto, permisoPorElemento.IdPermiso);
            permisoPorElemento.IdNegocio = NegociosDeSe.IdNegocio(ApiDeEnsamblados.ToEnumerado<enumNegocio>(permiso.Nombre.Split(".")[0]));
            permisoPorElemento.IdElemento = permiso.Nombre.Split(":")[1].Trim().Entero();
            permisoPorElemento.Calculado = false;
            return new ParametrosDeNegocio(enumTipoOperacion.Insertar);
        }

        protected override IEnumerable<PermisosPorElementoDto> LeerElementos(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden, Dictionary<string, object> opcionesDeMapeo)
        {
            //filtros.Add(new ClausulaDeFiltrado { Clausula = nameof(PermisosPorElementoDto.Calculado), Criterio = enumCriteriosDeFiltrado.igual, Valor = false.ToString() });
            return base.LeerElementos(posicion, cantidad, filtros, orden, opcionesDeMapeo);
        }

    }

}
