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
    public class PuestosDeClienteController : EntidadController<ContextoSe, PuestoDeClienteDtm, PuestoDeClienteDto>
    {
        public PuestosDeClienteController(GestorDePuestosDeCliente gestorDePuestosDeCliente, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDePuestosDeCliente,
           gestorDeErrores
         )
        {
        }

        public override JsonResult epCrearRelacion(int idNegocio, string elementoJson) =>
        ApiController.PersistirElemento(new GestorDePuestosDeCliente(Contexto, Contexto.Mapeador), elementoJson, HttpContext, AntesDeEjecutar_CrearPuestoDeCliente);

        private ParametrosDeNegocio AntesDeEjecutar_CrearPuestoDeCliente(PuestoDeClienteDto elemento) => new ParametrosDeNegocio(enumTipoOperacion.Insertar);

        public JsonResult epBorrarRelacionPorId(int id, string parametrosJson) =>
        ApiController.BorrarPorId(new GestorDePuestosDeCliente(Contexto, Contexto.Mapeador), id, parametrosJson, HttpContext, AntesDeEjecutar_BorrarPorId);

        protected override IEnumerable<PuestoDeClienteDto> LeerElementos(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden, Dictionary<string, object> opcionesDeMapeo)
        {
            var restrictor = ApiController.ObtenerNegocioYelemento(filtros);
            ApiDeDetalles.ValidarUsaDetalleDe(enumNegocio.Cliente, typeof(PuestoDeClienteDtm));

            var modoAcceso = ApiDePermisos.LeerModoDeAcceso(Contexto, enumNegocio.Cliente, restrictor.idElemento);
            if (modoAcceso == enumModoDeAccesoDeDatos.SinPermiso)
                GestorDeErrores.Emitir($"El usuario {Contexto.DatosDeConexion.Login} no tiene acceso al elemento del negocio: {enumNegocio.Cliente.Singular()}");

            var gestor = GestorDePuestosDeCliente.Gestor(Contexto, Contexto.Mapeador);

            return gestor.LeerElementos(posicion, cantidad, filtros, orden, opcionesDeMapeo);
        }

        protected override PuestoDeClienteDto LeerPorId(int id, Dictionary<string, object> parametros)
        {
            var gestor = GestorDePuestosDeCliente.Gestor(Contexto, Contexto.Mapeador);
            return gestor.LeerElementoPorId(id, parametros);
        }

    }
}
