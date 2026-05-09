using ServicioDeDatos;
using Gestor.Errores;
using Microsoft.AspNetCore.Mvc;
using GestorDeElementos;
using System.Collections.Generic;
using ServicioDeDatos.Seguridad;
using Utilidades;
using ServicioDeDatos.Terceros;
using ModeloDeDto.Terceros;
using GestoresDeNegocio.Terceros;

namespace MVCSistemaDeElementos.Controllers
{
    public class CuentasDeTrabajadorController : EntidadController<ContextoSe, CuentaDeTrabajadorDtm, CuentaDeTrabajadorDto>
    {
        public CuentasDeTrabajadorController(GestorDeCuentasDeTrabajador gestorDeCuentasDeTrabajador, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeCuentasDeTrabajador,
           gestorDeErrores
         )
        {
        }

        public override JsonResult epCrearRelacion(int idNegocio, string elementoJson) =>
        ApiController.PersistirElemento(new GestorDeCuentasDeTrabajador(Contexto, Contexto.Mapeador), elementoJson, HttpContext, AntesDeEjecutar_CrearCuentaDeTrabajador);

        private ParametrosDeNegocio AntesDeEjecutar_CrearCuentaDeTrabajador(CuentaDeTrabajadorDto elemento) => new ParametrosDeNegocio(enumTipoOperacion.Insertar);

        public JsonResult epBorrarRelacionPorId(int id, string parametrosJson) =>
        ApiController.BorrarPorId(new GestorDeCuentasDeTrabajador(Contexto, Contexto.Mapeador), id, parametrosJson, HttpContext, AntesDeEjecutar_BorrarPorId);

        protected override IEnumerable<CuentaDeTrabajadorDto> LeerElementos(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden, Dictionary<string, object> opcionesDeMapeo)
        {
            var restrictor = ApiController.ObtenerNegocioYelemento(filtros);
            ApiDeDetalles.ValidarUsaDetalleDe(enumNegocio.Trabajador, typeof(CuentaDeTrabajadorDtm));

            var modoAcceso = ApiDePermisos.LeerModoDeAcceso(Contexto, enumNegocio.Trabajador, restrictor.idElemento);
            if (modoAcceso == enumModoDeAccesoDeDatos.SinPermiso)
                GestorDeErrores.Emitir($"El usuario {Contexto.DatosDeConexion.Login} no tiene acceso al elemento del negocio: {enumNegocio.Trabajador.Singular()}");

            var gestor = GestorDeCuentasDeTrabajador.Gestor(Contexto, Contexto.Mapeador);

            return gestor.LeerElementos(posicion, cantidad, filtros, orden, opcionesDeMapeo);
        }

        protected override CuentaDeTrabajadorDto LeerPorId(int id, Dictionary<string, object> parametros)
        {
            var gestor = GestorDeCuentasDeTrabajador.Gestor(Contexto, Contexto.Mapeador);
            return gestor.LeerElementoPorId(id, parametros);
        }

    }
}
