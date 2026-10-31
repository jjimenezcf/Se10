using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Logistica;
using Microsoft.AspNetCore.Mvc;
using ModeloDeDto.Logistica;
using ServicioDeDatos;
using ServicioDeDatos.Logistica;
using ServicioDeDatos.Seguridad;
using System;
using System.Collections.Generic;
using Utilidades;

namespace MVCSistemaDeElementos.Controllers
{
    // Grid de entregas de la edición del pedido: se leen las entregas del pedido y desde cada una se cancela; su devolución la registra
    // DevolucionesDeUnPedidoController
    public class EntregasDeUnPedidoController : EntidadController<ContextoSe, RecepcionDeLineaDtm, EntregaDeUnPedidoDto>
    {
        public EntregasDeUnPedidoController(GestorDeEntregasDeUnPedido gestorDeEntregasDeUnPedido, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeEntregasDeUnPedido,
           gestorDeErrores
         )
        {
        }

        protected override IEnumerable<EntregaDeUnPedidoDto> LeerElementos(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden, Dictionary<string, object> opcionesDeMapeo)
        {
            var restrictor = ApiController.ObtenerNegocioYelemento(filtros);
            var modoAcceso = ApiDePermisos.LeerModoDeAcceso(Contexto, enumNegocio.Pedido, restrictor.idElemento);
            if (modoAcceso == enumModoDeAccesoDeDatos.SinPermiso)
                GestorDeErrores.Emitir($"El usuario {Contexto.DatosDeConexion.Login} no tiene acceso al elemento del negocio: {enumNegocio.Pedido.Singular()}");

            return GestorDeEntregasDeUnPedido.Gestor(Contexto, Contexto.Mapeador).LeerElementos(posicion, cantidad, filtros, orden, opcionesDeMapeo);
        }

        // Cancela la entrega (ver GestorDeEntregasDeUnPedido.CancelarEntrega): aquí sólo se leen los parámetros
        public JsonResult epCancelarEntrega(string parametrosJson)
        {
            var r = new Resultado();
            Contexto.IniciarTraza(GetType().Name + "_" + nameof(epCancelarEntrega));
            var tran = Contexto.IniciarTransaccion();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                var parametros = parametrosJson.ToDiccionarioDeParametros();

                var idRecepcion = LeerIdDeLaEntrega(parametros);
                var rowVersion = parametros.LeerValor<string>(nameof(RecepcionDeLineaDtm.RowVersion), null);

                r.Consola = GestorDeEntregasDeUnPedido.CancelarEntrega(Contexto, idRecepcion, rowVersion);
                r.Estado = enumEstadoPeticion.Ok;
                Contexto.Commit(tran);
            }
            catch (Exception e)
            {
                Contexto.Rollback(tran);
                ApiController.PrepararError(e, r, "Error al cancelar la entrega.");
            }
            finally
            {
                Contexto.CerrarTraza();
            }
            return new JsonResult(r);
        }

        private static int LeerIdDeLaEntrega(Dictionary<string, object> parametros)
        {
            var idRecepcion = (int)parametros.LeerValor<long>(ltrParametrosEp.id, 0);
            if (idRecepcion == 0)
                GestorDeErrores.Emitir("Ha de indicar la entrega");
            return idRecepcion;
        }
    }
}
