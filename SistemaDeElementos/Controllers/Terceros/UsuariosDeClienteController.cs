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
using ModeloDeDto;
using Newtonsoft.Json;

namespace MVCSistemaDeElementos.Controllers
{
    public class UsuariosDeClienteController : EntidadController<ContextoSe, UsuarioDeClienteDtm, UsuarioDeClienteDto>
    {
        public UsuariosDeClienteController(GestorDeUsuariosDeCliente gestorDeUsuariosDeCliente, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeUsuariosDeCliente,
           gestorDeErrores
         )
        {
        }

        public override JsonResult epCrearRelacion(int idNegocio, string elementoJson) =>
        ApiController.PersistirElemento(new GestorDeUsuariosDeCliente(Contexto, Contexto.Mapeador), elementoJson, HttpContext, AntesDeEjecutar_CrearUsuarioDeCliente);

        private ParametrosDeNegocio AntesDeEjecutar_CrearUsuarioDeCliente(UsuarioDeClienteDto elemento) => new ParametrosDeNegocio(enumTipoOperacion.Insertar);

        public JsonResult epBorrarRelacionPorId(int id, string parametrosJson) =>
        ApiController.BorrarPorId(new GestorDeUsuariosDeCliente(Contexto, Contexto.Mapeador), id, parametrosJson, HttpContext, AntesDeEjecutar_BorrarPorId);

        protected override IEnumerable<UsuarioDeClienteDto> LeerElementos(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden, Dictionary<string, object> opcionesDeMapeo)
        {
            var restrictor = ApiController.ObtenerNegocioYelemento(filtros);
            ApiDeDetalles.ValidarUsaDetalleDe(enumNegocio.Cliente, typeof(UsuarioDeClienteDtm));

            var modoAcceso = ApiDePermisos.LeerModoDeAcceso(Contexto, enumNegocio.Cliente, restrictor.idElemento);
            if (modoAcceso == enumModoDeAccesoDeDatos.SinPermiso)
                GestorDeErrores.Emitir($"El usuario {Contexto.DatosDeConexion.Login} no tiene acceso al elemento del negocio: {enumNegocio.Cliente.Singular()}");

            var gestor = GestorDeUsuariosDeCliente.Gestor(Contexto, Contexto.Mapeador);

            return gestor.LeerElementos(posicion, cantidad, filtros, orden, opcionesDeMapeo);
        }

        protected override UsuarioDeClienteDto LeerPorId(int id, Dictionary<string, object> parametros)
        {
            var gestor = GestorDeUsuariosDeCliente.Gestor(Contexto, Contexto.Mapeador);
            return gestor.LeerElementoPorId(id, parametros);
        }
        
        protected override IDetalleDto CrearDetalle(ContextoSe contexto, enumNegocio enumNegocio, string elementoJson, Dictionary<string, object> parametros)
        {
            var elemento = JsonConvert.DeserializeObject<UsuarioDeClienteNew>(elementoJson);
            return GestorDeUsuariosDeCliente.CrearUsuario(Contexto, elemento.IdElemento, elemento, parametros);
        }
    }
}
