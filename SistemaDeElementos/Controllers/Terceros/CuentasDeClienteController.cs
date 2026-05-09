using Gestor.Errores;
using GestorDeElementos;
using GestoresDeNegocio.Terceros;
using Microsoft.AspNetCore.Mvc;
using ModeloDeDto.Terceros;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Terceros;
using System.Collections.Generic;
using System.Linq;
using Utilidades;

namespace MVCSistemaDeElementos.Controllers
{
    public class CuentasDeClienteController : EntidadController<ContextoSe, CuentaDeClienteDtm, CuentaDeClienteDto>
    {
        public CuentasDeClienteController(GestorDeCuentasDeCliente gestorDeCuentasDeCliente, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeCuentasDeCliente,
           gestorDeErrores
         )
        {
        }

        public override JsonResult epCrearRelacion(int idNegocio, string elementoJson) =>
        ApiController.PersistirElemento(new GestorDeCuentasDeCliente(Contexto, Contexto.Mapeador), elementoJson, HttpContext, AntesDeEjecutar_CrearCuentaDeCliente);

        private ParametrosDeNegocio AntesDeEjecutar_CrearCuentaDeCliente(CuentaDeClienteDto elemento) => new ParametrosDeNegocio(enumTipoOperacion.Insertar);

        public JsonResult epBorrarRelacionPorId(int id, string parametrosJson) =>
        ApiController.BorrarPorId(new GestorDeCuentasDeCliente(Contexto, Contexto.Mapeador), id, parametrosJson, HttpContext, AntesDeEjecutar_BorrarPorId);

        protected override IEnumerable<CuentaDeClienteDto> LeerElementos(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden, Dictionary<string, object> opcionesDeMapeo)
        {
            if (filtros.Count == 2 && filtros.Any(f => f.Clausula.ToLower() == nameof(IUsaCliente.IdCliente).ToLower())
                 && !filtros.Any(f => f.Clausula.ToLower() == nameof(IUsaElemento.IdElemento).ToLower())
                 && filtros.Any(f => f.Clausula.ToLower() == nameof(ltrParametrosEp.negocio).ToLower()))
            {
                var filtro = filtros.First(f => f.Clausula.ToLower() == nameof(IUsaCliente.IdCliente).ToLower());
                filtro.Clausula = nameof(IUsaElemento.IdElemento);
            }

            var restrictor = ApiController.ObtenerNegocioYelemento(filtros);
            ApiDeDetalles.ValidarUsaDetalleDe(enumNegocio.Cliente, typeof(CuentaDeClienteDtm));

            var modoAcceso = ApiDePermisos.LeerModoDeAcceso(Contexto, enumNegocio.Cliente, restrictor.idElemento);
            if (modoAcceso == enumModoDeAccesoDeDatos.SinPermiso)
                GestorDeErrores.Emitir($"El usuario {Contexto.DatosDeConexion.Login} no tiene acceso al elemento del negocio: {enumNegocio.Cliente.Singular()}");

            var gestor = GestorDeCuentasDeCliente.Gestor(Contexto, Contexto.Mapeador);

            return gestor.LeerElementos(posicion, cantidad, filtros, orden, opcionesDeMapeo);
        }

        protected override CuentaDeClienteDto LeerPorId(int id, Dictionary<string, object> parametros)
        {
            var gestor = GestorDeCuentasDeCliente.Gestor(Contexto, Contexto.Mapeador);
            return gestor.LeerElementoPorId(id, parametros);
        }

    }
}
