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

namespace MVCSistemaDeElementos.Controllers
{
    public class PermisosPorEstadoController : EntidadController<ContextoSe, PermisosPorEstadoDtm, PermisosPorEstadoDto>
    {
        public PermisosPorEstadoController(GestorDePermisosPorEstado gestorDePermisos, GestorDeErrores gestorDeErrores)
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
            return ApiController.PersistirElemento(new GestorDePermisosPorEstado(Contexto, Contexto.Mapeador), elementoJson, HttpContext, AntesDeEjecutar);
        }

        private ParametrosDeNegocio AntesDeEjecutar(PermisosPorEstadoDto permisoPorEstado)
        {
            var permiso = GestorDePermisos.LeerRegistroPorId(Contexto, permisoPorEstado.IdPermiso);
            var negocio = ApiDeEnsamblados.ToEnumerado<enumNegocio>(permiso.Nombre.Split(".")[0]);
            permisoPorEstado.IdNegocio = NegociosDeSe.IdNegocio(negocio);
            var nombreEstado = permiso.Nombre.Substring(permiso.Nombre.IndexOf(':') + 1).Trim();
            var filtro = new ClausulaDeFiltrado(nameof(INombre.Nombre), enumCriteriosDeFiltrado.igual, nombreEstado);
            dynamic estados = NegociosDeSe.CrearGestorDeEstados(Contexto,negocio).LeerRegistros(0, -1, new List<ClausulaDeFiltrado> { filtro }, false);
            if (estados.Count != 1) GestorDeErrores.Emitir($"Debe existir un solo tipo para el permisos {permiso.Nombre}");
            permisoPorEstado.IdEstado = estados[0].Id;
            permisoPorEstado.Calculado = false;
            return new ParametrosDeNegocio(enumTipoOperacion.Insertar);
        }

        protected override IEnumerable<PermisosPorEstadoDto> LeerElementos(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden, Dictionary<string, object> opcionesDeMapeo)
        {
            //filtros.Add(new ClausulaDeFiltrado { Clausula = nameof(PermisosPorNegocioDto.Calculado), Criterio = enumCriteriosDeFiltrado.igual, Valor = false.ToString() });
            return base.LeerElementos(posicion, cantidad, filtros, orden, opcionesDeMapeo);
        }

    }

}
