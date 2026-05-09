using ServicioDeDatos;
using Gestor.Errores;
using GestoresDeNegocio.Logistica;
using ModeloDeDto.Logistica;
using ServicioDeDatos.Logistica;
using Microsoft.AspNetCore.Mvc;
using GestorDeElementos;
using System.Collections.Generic;
using ServicioDeDatos.Seguridad;
using Utilidades;

namespace MVCSistemaDeElementos.Controllers
{
    public class LineasDeUnPedidoController : EntidadController<ContextoSe, LineaDeUnPedidoDtm, LineaDeUnPedidoDto>
    {
        public LineasDeUnPedidoController(GestorDeLineasDeUnPedido gestorDeLineasDeUnPedido, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeLineasDeUnPedido,
           gestorDeErrores
         )
        {
        }

        public override JsonResult epCrearRelacion(int idNegocio, string elementoJson) =>
        ApiController.PersistirElemento(new GestorDeLineasDeUnPedido(Contexto, Contexto.Mapeador), elementoJson, HttpContext, AntesDeEjecutar_CrearLineaDeUnPedido);

        private ParametrosDeNegocio AntesDeEjecutar_CrearLineaDeUnPedido(LineaDeUnPedidoDto elemento)
        {
            return new ParametrosDeNegocio(enumTipoOperacion.Insertar);
        }

        public JsonResult epBorrarRelacionPorId(int id, string parametrosJson) =>
        ApiController.BorrarPorId(new GestorDeLineasDeUnPedido(Contexto, Contexto.Mapeador), id, parametrosJson, HttpContext, AntesDeEjecutar_BorrarPorId);

        protected override IEnumerable<LineaDeUnPedidoDto> LeerElementos(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden, Dictionary<string, object> opcionesDeMapeo)
        {
            var restrictor = ApiController.ObtenerNegocioYelemento(filtros);
            var idTipo = Contexto.SeleccionarPorId<PedidoDtm>(restrictor.idElemento).IdTipo;            
            ApiDeDetalles.ValidarUsaDetalleDe(enumNegocio.Pedido, Contexto, idTipo, typeof(LineaDeUnPedidoDtm));

            var modoAcceso = ApiDePermisos.LeerModoDeAcceso(Contexto, enumNegocio.Pedido, restrictor.idElemento);
            if (modoAcceso == enumModoDeAccesoDeDatos.SinPermiso)
                GestorDeErrores.Emitir($"El usuario {Contexto.DatosDeConexion.Login} no tiene acceso al elemento del negocio: {enumNegocio.Pedido.Singular()}");

            var gestor = GestorDeLineasDeUnPedido.Gestor(Contexto, Contexto.Mapeador);

            return gestor.LeerElementos(posicion, cantidad, filtros, orden, opcionesDeMapeo);
        }

        protected override LineaDeUnPedidoDto LeerPorId(int id, Dictionary<string, object> parametros)
        {
            var gestor = GestorDeLineasDeUnPedido.Gestor(Contexto, Contexto.Mapeador);
            return gestor.LeerElementoPorId(id, parametros);
        }

    }
}
