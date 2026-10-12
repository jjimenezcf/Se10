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

        public JsonResult epRecibirLinea(string parametrosJson)
        {
            var r = new Resultado();
            Contexto.IniciarTraza(GetType().Name + "_" + nameof(epRecibirLinea));
            Dictionary<string, object> parametros = null;
            var tran = Contexto.IniciarTransaccion();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                parametros = parametrosJson.ToDiccionarioDeParametros();
                var (linea, pedido) = LeerLineaParaRecepcionar(parametros, esUnaEntrega: true);
                var cantidad = parametros.LeerValor<decimal?>(nameof(RecepcionarLineaDto.Cantidad), null);
                var precio = parametros.LeerValor<decimal?>(nameof(RecepcionarLineaDto.Precio), null);
                var descuento = parametros.LeerValor<decimal?>(nameof(RecepcionarLineaDto.Descuento), null);
                if (cantidad == null || cantidad <= 0)
                    GestorDeErrores.Emitir("Ha de indicar la cantidad entregada, si no se ha entregado nada marque la línea como no entregada");
                if (precio == null || precio < 0)
                    GestorDeErrores.Emitir("Ha de indicar el precio de compra unitario sin iva");
                if (descuento != null && (descuento < 0 || descuento >= 100))
                    GestorDeErrores.Emitir("El descuento ha de ser mayor o igual que 0 y menor que 100");
                if (descuento == 0) descuento = null;

                // destino elegido en la modal de recepción; en la entrega completa no se indica y va al de la cabecera del pedido
                int? idAlmacen = (int)parametros.LeerValor<long>(nameof(RecepcionarLineaDto.IdAlmacen), 0);
                int? idExpediente = (int)parametros.LeerValor<long>(nameof(RecepcionarLineaDto.IdExpediente), 0);
                if (idAlmacen == 0) idAlmacen = null;
                if (idExpediente == 0) idExpediente = null;
                (int? idAlmacen, int? idExpediente) destino = idAlmacen is null && idExpediente is null
                    ? pedido.DestinoDeLasEntregas()
                    : (idAlmacen, idExpediente);

                var recepcion = linea.Recibir(Contexto, cantidad.Value, precio.Value, descuento, destino.idAlmacen, destino.idExpediente);
                r.Consola = $"Línea {linea.Orden} recibida {(recepcion.VaAlAlmacen ? "en el almacén" : "en la obra del expediente")}: cantidad {cantidad}, precio {precio}, descuento {descuento}, importe entregado {recepcion.Importe:N2}";
                r.Estado = enumEstadoPeticion.Ok;
                Contexto.Commit(tran);
            }
            catch (Exception e)
            {
                Contexto.Rollback(tran);
                if (parametros != null) ExtensorDePedidos.OlvidarRecepcion(IdLineaARecepcionar(parametros));
                ApiController.PrepararError(e, r, "Error al recibir la línea del pedido.");
            }
            finally
            {
                Contexto.CerrarTraza();
            }
            return new JsonResult(r);
        }

        public JsonResult epLineaNoEntregada(string parametrosJson)
        {
            var r = new Resultado();
            Contexto.IniciarTraza(GetType().Name + "_" + nameof(epLineaNoEntregada));
            Dictionary<string, object> parametros = null;
            var tran = Contexto.IniciarTransaccion();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                parametros = parametrosJson.ToDiccionarioDeParametros();
                var (linea, _) = LeerLineaParaRecepcionar(parametros, esUnaEntrega: false);
                var estabaEntregada = linea.Recepcion(Contexto) != null;
                linea.CancelarRecepcion(Contexto);
                r.Consola = estabaEntregada
                    ? $"Cancelada la entrega de la línea {linea.Orden}"
                    : $"Línea {linea.Orden} marcada como no entregada";
                r.Estado = enumEstadoPeticion.Ok;
                Contexto.Commit(tran);
            }
            catch (Exception e)
            {
                Contexto.Rollback(tran);
                if (parametros != null) ExtensorDePedidos.OlvidarRecepcion(IdLineaARecepcionar(parametros));
                ApiController.PrepararError(e, r, "Error al marcar la línea del pedido como no entregada.");
            }
            finally
            {
                Contexto.CerrarTraza();
            }
            return new JsonResult(r);
        }

        private static int IdLineaARecepcionar(Dictionary<string, object> parametros)
            => (int)parametros.LeerValor<long>(nameof(RecepcionarLineaDto.IdLineaPedido), 0);

        // Lee la línea sin cache, valida que es la versión que el usuario vio en el grid y que el pedido admite recepciones;
        // se ha de llamar antes de cualquier modificación de la operación
        // esUnaEntrega: entrega parcial o total, la línea no puede estar ya entregada;
        // si no, se cancela la entrega (sólo con el pedido en recepción) o se marca como no entregada si no la tiene
        private (LineaDeUnPedidoDtm linea, PedidoDtm pedido) LeerLineaParaRecepcionar(Dictionary<string, object> parametros, bool esUnaEntrega)
        {
            var linea = Contexto.SeleccionarPorId<LineaDeUnPedidoDtm>(IdLineaARecepcionar(parametros), usarLaCache: false);
            linea.ValidarRowId(Contexto, parametros.LeerValor<string>(nameof(LineaDeUnPedidoDtm.RowVersion), null), releer: false);

            var pedido = Contexto.SeleccionarPorId<PedidoDtm>(linea.IdElemento, usarLaCache: false);
            if (!pedido.EstaEnAlgunaDeLasEtapa(new List<enumEtapasDePedido> { enumEtapasDePedido.PED_Etapa_De_Solicitud, enumEtapasDePedido.PED_Etapa_De_Recepcion }))
                GestorDeErrores.Emitir($"Las líneas del pedido '{pedido.Referencia}' solo se pueden recepcionar con el pedido {enumEtapasDePedido.PED_Etapa_De_Solicitud.Nombre()} o {enumEtapasDePedido.PED_Etapa_De_Recepcion.Nombre()}");

            var entregada = linea.Recepcion(Contexto) != null;
            if (esUnaEntrega && entregada)
                GestorDeErrores.Emitir($"La línea {linea.Orden} del pedido '{pedido.Referencia}' ya se ha entregado, para volver a entregarla cancele antes la entrega");
            if (!esUnaEntrega && entregada && !pedido.EstaEnLaEtapa(enumEtapasDePedido.PED_Etapa_De_Recepcion))
                GestorDeErrores.Emitir($"Solo se puede cancelar la entrega de la línea {linea.Orden} con el pedido '{pedido.Referencia}' {enumEtapasDePedido.PED_Etapa_De_Recepcion.Nombre()}");

            return (linea, pedido);
        }

    }
}
