using ServicioDeDatos;
using Gestor.Errores;
using Microsoft.AspNetCore.Mvc;
using GestorDeElementos;
using System.Collections.Generic;
using ServicioDeDatos.Seguridad;
using Utilidades;
using ServicioDeDatos.Gastos;
using ModeloDeDto.Gastos;
using GestoresDeNegocio.Gastos;

namespace MVCSistemaDeElementos.Controllers
{
    public class LineasDeUnaFarController : EntidadController<ContextoSe, LineaDeUnaFarDtm, LineaDeUnaFarDto>
    {
        public LineasDeUnaFarController(GestorDeLineasDeUnaFar gestorDeLineasDeUnaFar, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeLineasDeUnaFar,
           gestorDeErrores
         )
        {
        }

        public override JsonResult epCrearRelacion(int idNegocio, string elementoJson) =>
        ApiController.PersistirElemento(new GestorDeLineasDeUnaFar(Contexto, Contexto.Mapeador), elementoJson, HttpContext, AntesDeEjecutar_CrearLineaDeUnaFar);

        private ParametrosDeNegocio AntesDeEjecutar_CrearLineaDeUnaFar(LineaDeUnaFarDto elemento)
        {
            return new ParametrosDeNegocio(enumTipoOperacion.Insertar);
        }

        public JsonResult epBorrarRelacionPorId(int id, string parametrosJson) =>
        ApiController.BorrarPorId(new GestorDeLineasDeUnaFar(Contexto, Contexto.Mapeador), id, parametrosJson, HttpContext, AntesDeEjecutar_BorrarPorId);


        protected override IEnumerable<LineaDeUnaFarDto> LeerElementos(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden, Dictionary<string, object> opcionesDeMapeo)
        {
            var restrictor = ApiController.ObtenerNegocioYelemento(filtros);
            var idTipo = Contexto.SeleccionarPorId<FacturaRecDtm>(restrictor.idElemento).IdTipo;
            ApiDeDetalles.ValidarUsaDetalleDe(enumNegocio.FacturaRecibida, Contexto, idTipo, typeof(LineaDeUnaFarDtm));

            var modoAcceso = ApiDePermisos.LeerModoDeAcceso(Contexto, enumNegocio.FacturaRecibida, restrictor.idElemento);
            if (modoAcceso == enumModoDeAccesoDeDatos.SinPermiso)
                GestorDeErrores.Emitir($"El usuario {Contexto.DatosDeConexion.Login} no tiene acceso al elemento del negocio: {enumNegocio.FacturaRecibida.Singular()}");

            var gestor = GestorDeLineasDeUnaFar.Gestor(Contexto, Contexto.Mapeador);

            return gestor.LeerElementos(posicion, cantidad, filtros, orden, opcionesDeMapeo);
        }

        protected override LineaDeUnaFarDto LeerPorId(int id, Dictionary<string, object> parametros)
        {
            var gestor = GestorDeLineasDeUnaFar.Gestor(Contexto, Contexto.Mapeador);
            return gestor.LeerElementoPorId(id, parametros);
        }

    }
}
