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
using System;
using GestorDeElementos.Extensores;

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

        // Entrega parcial de una línea (ver ExtensorDeRecepciones.RecepcionarLinea): aquí sólo se leen y validan los datos de la modal
        public JsonResult epRecibirLinea(string parametrosJson)
        {
            var r = new Resultado();
            Contexto.IniciarTraza(GetType().Name + "_" + nameof(epRecibirLinea));
            var tran = Contexto.IniciarTransaccion();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                var parametros = parametrosJson.ToDiccionarioDeParametros();

                var idLinea = LeerIdDeLaLinea(parametros);
                var rowVersion = parametros.LeerValor<string>(nameof(LineaDeUnPedidoDtm.RowVersion), null);
                var cantidad = parametros.LeerValor<decimal?>(nameof(RecepcionarLineaDto.Cantidad), null);
                var precio = parametros.LeerValor<decimal?>(nameof(RecepcionarLineaDto.Precio), null);
                var descuento = parametros.LeerValor<decimal?>(nameof(RecepcionarLineaDto.Descuento), null);
                var recepcionadaEl = parametros.LeerValor<DateTime?>(nameof(RecepcionarLineaDto.RecepcionadaEl), null);
                var idAlmacen = (int)parametros.LeerValor<long>(nameof(RecepcionarLineaDto.IdAlmacen), 0);
                var idExpediente = (int)parametros.LeerValor<long>(nameof(RecepcionarLineaDto.IdExpediente), 0);

                if (cantidad == null || cantidad <= 0)
                    GestorDeErrores.Emitir("Ha de indicar la cantidad entregada, si no se ha entregado nada marque la línea como no entregada");
                if (precio == null || precio < 0)
                    GestorDeErrores.Emitir("Ha de indicar el precio de compra unitario sin iva");
                if (descuento != null && (descuento < 0 || descuento >= 100))
                    GestorDeErrores.Emitir("El descuento ha de ser mayor o igual que 0 y menor que 100");
                if (recepcionadaEl == null)
                    GestorDeErrores.Emitir("Ha de indicar la fecha de recepción");

                r.Consola = ExtensorDeRecepciones.RecepcionarLinea(Contexto, idLinea, rowVersion, recepcionadaEl.Value, cantidad.Value, precio.Value,
                    descuento == 0 ? null : descuento, idAlmacen == 0 ? null : idAlmacen, idExpediente == 0 ? null : idExpediente);
                r.Estado = enumEstadoPeticion.Ok;
                Contexto.Commit(tran);
            }
            catch (Exception e)
            {
                Contexto.Rollback(tran);
                ApiController.PrepararError(e, r, "Error al recibir la línea del pedido.");
            }
            finally
            {
                Contexto.CerrarTraza();
            }
            return new JsonResult(r);
        }

        // Aspa: cancela la entrega de la línea o la marca como no entregada (ver ExtensorDeRecepciones.LineaNoEntregada)
        public JsonResult epLineaNoEntregada(string parametrosJson)
        {
            var r = new Resultado();
            Contexto.IniciarTraza(GetType().Name + "_" + nameof(epLineaNoEntregada));
            var tran = Contexto.IniciarTransaccion();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                var parametros = parametrosJson.ToDiccionarioDeParametros();

                var idLinea = LeerIdDeLaLinea(parametros);
                var rowVersion = parametros.LeerValor<string>(nameof(LineaDeUnPedidoDtm.RowVersion), null);

                r.Consola = ExtensorDeRecepciones.LineaNoEntregada(Contexto, idLinea, rowVersion);
                r.Estado = enumEstadoPeticion.Ok;
                Contexto.Commit(tran);
            }
            catch (Exception e)
            {
                Contexto.Rollback(tran);
                ApiController.PrepararError(e, r, "Error al marcar la línea del pedido como no entregada.");
            }
            finally
            {
                Contexto.CerrarTraza();
            }
            return new JsonResult(r);
        }

        private static int LeerIdDeLaLinea(Dictionary<string, object> parametros)
        {
            var idLinea = (int)parametros.LeerValor<long>(nameof(RecepcionarLineaDto.IdLineaPedido), 0);
            if (idLinea == 0)
                GestorDeErrores.Emitir("Ha de indicar la línea del pedido");
            return idLinea;
        }

    }
}
