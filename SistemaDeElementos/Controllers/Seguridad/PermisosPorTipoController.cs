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
    public class PermisosPorTipoController : EntidadController<ContextoSe, PermisosPorTipoDtm, PermisosPorTipoDto>
    {
        public PermisosPorTipoController(GestorDePermisosPorTipo gestorDePermisos, GestorDeErrores gestorDeErrores)
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
            return ApiController.PersistirElemento(new GestorDePermisosPorTipo(Contexto, Contexto.Mapeador), elementoJson, HttpContext, AntesDeEjecutar);
        }

        private ParametrosDeNegocio AntesDeEjecutar(PermisosPorTipoDto permisosPorTipo)
        {
            //Nombre del permiso por tipo = ARCHIVADOR.TIPO (Gestor): General

            var permiso = GestorDePermisos.LeerRegistroPorId(Contexto, permisosPorTipo.IdPermiso);
            var negocio = ApiDeEnsamblados.ToEnumerado<enumNegocio>(permiso.Nombre.Split(".")[0]);
            permisosPorTipo.IdNegocio = NegociosDeSe.IdNegocio(negocio);
            var nombreTipo = permiso.Nombre.Substring(permiso.Nombre.IndexOf(':')+1).Trim();
            var filtro = new ClausulaDeFiltrado(nameof(INombre.Nombre), enumCriteriosDeFiltrado.igual, nombreTipo);
            dynamic tipos = NegociosDeSe.CrearGestor(Contexto, negocio).GestorDeTipos.LeerRegistros(0, -1, new List<ClausulaDeFiltrado> { filtro }, false);
            if (tipos.Count != 1) GestorDeErrores.Emitir($"Debe existir un solo tipo para el permisos {permiso.Nombre}");
            permisosPorTipo.IdTipo = tipos[0].Id ;
            permisosPorTipo.Calculado = false;
            return new ParametrosDeNegocio(enumTipoOperacion.Insertar);
        }

        protected override IEnumerable<PermisosPorTipoDto> LeerElementos(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden, Dictionary<string, object> opcionesDeMapeo)
        {
            //filtros.Add(new ClausulaDeFiltrado { Clausula = nameof(PermisosPorTipoDto.Calculado), Criterio = enumCriteriosDeFiltrado.igual, Valor = false.ToString() });
            return base.LeerElementos(posicion, cantidad, filtros, orden, opcionesDeMapeo);
        }

    }

}
