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
    // Grid de devoluciones de la edición del pedido: se leen las devoluciones de sus entregas y desde cada una se cancela. También registra
    // la devolución de una entrega desde el grid de entregas. Ambas operaciones las resuelve GestorDeDevolucionesDeUnPedido
    public class DevolucionesDeUnPedidoController : EntidadController<ContextoSe, DevolucionDeEntregaDtm, DevolucionDeUnPedidoDto>
    {
        public DevolucionesDeUnPedidoController(GestorDeDevolucionesDeUnPedido gestorDeDevolucionesDeUnPedido, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeDevolucionesDeUnPedido,
           gestorDeErrores
         )
        {
        }

        protected override IEnumerable<DevolucionDeUnPedidoDto> LeerElementos(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden, Dictionary<string, object> opcionesDeMapeo)
        {
            var restrictor = ApiController.ObtenerNegocioYelemento(filtros);
            var modoAcceso = ApiDePermisos.LeerModoDeAcceso(Contexto, enumNegocio.Pedido, restrictor.idElemento);
            if (modoAcceso == enumModoDeAccesoDeDatos.SinPermiso)
                GestorDeErrores.Emitir($"El usuario {Contexto.DatosDeConexion.Login} no tiene acceso al elemento del negocio: {enumNegocio.Pedido.Singular()}");

            return GestorDeDevolucionesDeUnPedido.Gestor(Contexto, Contexto.Mapeador).LeerElementos(posicion, cantidad, filtros, orden, opcionesDeMapeo);
        }

        // Devolución de lo entregado (ver GestorDeDevolucionesDeUnPedido.DevolverEntrega): aquí sólo se leen y validan los datos de la modal
        public JsonResult epDevolverEntrega(string parametrosJson)
        {
            var r = new Resultado();
            Contexto.IniciarTraza(GetType().Name + "_" + nameof(epDevolverEntrega));
            var tran = Contexto.IniciarTransaccion();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                var parametros = parametrosJson.ToDiccionarioDeParametros();

                var idRecepcion = (int)parametros.LeerValor<long>(ltrParametrosEp.id, 0);
                if (idRecepcion == 0)
                    GestorDeErrores.Emitir("Ha de indicar la entrega");
                var rowVersion = parametros.LeerValor<string>(nameof(RecepcionDeLineaDtm.RowVersion), null);
                var cantidad = parametros.LeerValor<decimal?>(nameof(DevolucionDeEntregaDto.Cantidad), null);
                var devueltaEl = parametros.LeerValor<DateTime?>(nameof(DevolucionDeEntregaDto.DevueltaEl), null);
                var motivo = parametros.LeerValor<string>(nameof(DevolucionDeEntregaDto.Motivo), null);

                if (cantidad == null || cantidad <= 0)
                    GestorDeErrores.Emitir("Ha de indicar la cantidad a devolver");
                if (devueltaEl == null)
                    GestorDeErrores.Emitir("Ha de indicar la fecha de devolución");
                if (motivo.IsNullOrEmpty())
                    GestorDeErrores.Emitir("Ha de indicar el motivo de la devolución");

                r.Consola = GestorDeDevolucionesDeUnPedido.DevolverEntrega(Contexto, idRecepcion, rowVersion, cantidad.Value, devueltaEl.Value, motivo);
                r.Estado = enumEstadoPeticion.Ok;
                Contexto.Commit(tran);
            }
            catch (Exception e)
            {
                Contexto.Rollback(tran);
                ApiController.PrepararError(e, r, "Error al devolver la entrega.");
            }
            finally
            {
                Contexto.CerrarTraza();
            }
            return new JsonResult(r);
        }

        // Imprime las devoluciones del pedido y anexa el pdf a sus archivos (ver GestorDeDevolucionesDeUnPedido.ImprimirDevoluciones)
        public JsonResult epImprimirDevoluciones(string parametrosJson)
        {
            var r = new Resultado();
            Contexto.IniciarTraza(GetType().Name + "_" + nameof(epImprimirDevoluciones));
            var tran = Contexto.IniciarTransaccion();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                var parametros = parametrosJson.ToDiccionarioDeParametros();

                var idPedido = (int)parametros.LeerValor<long>(ltrParametrosEp.id, 0);
                if (idPedido == 0)
                    GestorDeErrores.Emitir("Ha de indicar el pedido");

                r.Consola = GestorDeDevolucionesDeUnPedido.ImprimirDevoluciones(Contexto, idPedido);
                r.Estado = enumEstadoPeticion.Ok;
                Contexto.Commit(tran);
            }
            catch (Exception e)
            {
                Contexto.Rollback(tran);
                ApiController.PrepararError(e, r, "Error al imprimir las devoluciones.");
            }
            finally
            {
                Contexto.CerrarTraza();
            }
            return new JsonResult(r);
        }

        // Cancela la devolución (ver GestorDeDevolucionesDeUnPedido.CancelarDevolucion): aquí sólo se leen los parámetros
        public JsonResult epCancelarDevolucion(string parametrosJson)
        {
            var r = new Resultado();
            Contexto.IniciarTraza(GetType().Name + "_" + nameof(epCancelarDevolucion));
            var tran = Contexto.IniciarTransaccion();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                var parametros = parametrosJson.ToDiccionarioDeParametros();

                var idDevolucion = (int)parametros.LeerValor<long>(ltrParametrosEp.id, 0);
                if (idDevolucion == 0)
                    GestorDeErrores.Emitir("Ha de indicar la devolución");
                var rowVersion = parametros.LeerValor<string>(nameof(DevolucionDeEntregaDtm.RowVersion), null);

                r.Consola = GestorDeDevolucionesDeUnPedido.CancelarDevolucion(Contexto, idDevolucion, rowVersion);
                r.Estado = enumEstadoPeticion.Ok;
                Contexto.Commit(tran);
            }
            catch (Exception e)
            {
                Contexto.Rollback(tran);
                ApiController.PrepararError(e, r, "Error al cancelar la devolución.");
            }
            finally
            {
                Contexto.CerrarTraza();
            }
            return new JsonResult(r);
        }
    }
}
