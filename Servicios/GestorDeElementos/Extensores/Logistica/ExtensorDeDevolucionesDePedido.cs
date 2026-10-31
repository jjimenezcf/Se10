using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Logistica;
using ServicioDeDatos.Seguridad;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilidades;
using static Gestor.Errores.GestorDeErrores;
using static ServicioDeDatos.Elemento.Enumerados;

namespace GestorDeElementos.Extensores
{
    // Devoluciones al proveedor de lo entregado en las líneas de los pedidos (DevolucionDeEntregaDtm): una entrega puede tener varias
    // devoluciones y entre todas no pueden superar lo entregado. Si la entrega fue a un almacén, la devolución saca lo devuelto con un
    // movimiento de devolución de compra, que se preasienta al revés que la entrada (ver ExtensorDePreasientos.PreasentarSalidaDelmacen)
    public static class ExtensorDeDevolucionesDePedido
    {
        // Cantidad devuelta de una entrega: se cachea por entrega y se olvida al registrar o cancelar una devolución
        // (ver GestorDeDevolucionesDeUnPedido.EliminarCaches)
        public static decimal CantidadDevuelta(this RecepcionDeLineaDtm recepcion, ContextoSe contexto)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Pedido_Devuelto);
            var clave = recepcion.Id.ToString();
            if (!cache.ContainsKey(clave))
                cache[clave] = contexto.Set<DevolucionDeEntregaDtm>().Where(x => x.IdLineaDeRecepcion == recepcion.Id).Sum(x => (decimal?)x.Cantidad) ?? 0m;
            return (decimal)cache[clave];
        }

        // Lo que aún se puede devolver de una entrega: lo entregado menos lo ya devuelto
        public static decimal CantidadPendienteDeDevolver(this RecepcionDeLineaDtm recepcion, ContextoSe contexto)
            => recepcion.Cantidad - recepcion.CantidadDevuelta(contexto);

        public static bool TieneDevoluciones(this RecepcionDeLineaDtm recepcion, ContextoSe contexto)
            => recepcion.CantidadDevuelta(contexto) > 0;

        // Lo devuelto se valora al precio y descuento de la entrega, con la misma fórmula que RecepcionDeLineaDtm.Importe
        public static decimal Importe(this DevolucionDeEntregaDtm devolucion, RecepcionDeLineaDtm recepcion)
            => devolucion.Cantidad * recepcion.Precio * (1 - (recepcion.Descuento ?? 0) / 100);

        public static RecepcionDeLineaDtm Recepcion(this DevolucionDeEntregaDtm devolucion, ContextoSe contexto)
            => devolucion.LineaDeRecepcion is not null && devolucion.LineaDeRecepcion.Id == devolucion.IdLineaDeRecepcion
            ? devolucion.LineaDeRecepcion
            : devolucion.LineaDeRecepcion = contexto.SeleccionarPorId<RecepcionDeLineaDtm>(devolucion.IdLineaDeRecepcion);

        //---------------------------------------------------------------------------------------------------------------------------
        // Reglas de las devoluciones, las aplica GestorDeDevolucionesDeUnPedido al insertar (devolver una entrega) y al eliminar (cancelar
        // la devolución)
        //---------------------------------------------------------------------------------------------------------------------------

        // Lee la entrega sin caché y valida que es la versión que el usuario vio en el grid de entregas
        public static RecepcionDeLineaDtm LeerEntregaParaDevolver(ContextoSe contexto, int idRecepcion, string rowVersion)
        {
            var recepcion = contexto.SeleccionarPorId<RecepcionDeLineaDtm>(idRecepcion, usarLaCache: false);
            recepcion.ValidarRowId(contexto, rowVersion, releer: false);
            return recepcion;
        }

        // Las devoluciones las registra o cancela quien gestiona el pedido, con el pedido en recepción o cerrado
        public static (LineaDeUnPedidoDtm linea, PedidoDtm pedido) LeerPedidoParaDevolver(this RecepcionDeLineaDtm recepcion, ContextoSe contexto)
        {
            var linea = contexto.SeleccionarPorId<LineaDeUnPedidoDtm>(recepcion.IdLinea, usarLaCache: false);
            var pedido = contexto.SeleccionarPorId<PedidoDtm>(linea.IdElemento, usarLaCache: false);
            pedido.ValidarQueEsGestor(contexto);
            if (!pedido.EstaEnAlgunaDeLasEtapa(new List<enumEtapasDePedido> { enumEtapasDePedido.PED_Etapa_De_Recepcion, enumEtapasDePedido.PED_Etapa_Cerrado }))
                Emitir($"Las devoluciones del pedido '{pedido.Referencia}' solo se pueden registrar o cancelar con el pedido {enumEtapasDePedido.PED_Etapa_De_Recepcion.Nombre()} o {enumEtapasDePedido.PED_Etapa_Cerrado.Nombre()}");
            return (linea, pedido);
        }

        // Para cancelar una devolución cuya entrega fue a un almacén hay que borrar su salida (devolución de compra): el almacén ha de
        // estar activo y el preasiento de la salida sin contabilizar. Devuelve la salida a borrar, null si no la tiene
        public static MovimientoDeAlmacenDtm ValidarCancelacion(this DevolucionDeEntregaDtm devolucion, ContextoSe contexto, RecepcionDeLineaDtm recepcion, LineaDeUnPedidoDtm linea)
        {
            if (!recepcion.VaAlAlmacen)
                return null;

            var almacen = recepcion.Almacen(contexto);
            if (!almacen.EstaActivo())
                Emitir($"No se puede cancelar la devolución de la línea {linea.Orden}: el almacén '{almacen.Expresion}' no está activo y no se puede borrar su salida");

            var movimiento = devolucion.MovimientoDeDevolucion(contexto);
            var contabilizado = movimiento?.PreasientoContabilizado(contexto);
            if (contabilizado is not null)
                Emitir($"No se puede cancelar la devolución de la línea {linea.Orden}: el preasiento '{contabilizado.Referencia}' de su salida del almacén está {enumEtapasDePreasiento.SPR_Etapa_Contabilizado.Nombre()} (está en un lote contable)");
            return movimiento;
        }

        // Borra la salida del almacén de la devolución; el gestor de movimientos barre los posteriores
        public static void AnularSalidaPorDevolucion(this MovimientoDeAlmacenDtm movimiento, ContextoSe contexto)
            => movimiento.EliminarComoAdministrador(contexto, accionEjecutada: ltrDeUnMovimientoDeAlmacen.Accion_AnularDevolucionDeCompra);

        // La devolución se fecha entre max(fecha de la entrega, fecha del último cierre contable de la sociedad del pedido) y hoy, ambos incluidos
        private static void ValidarFechaDeDevolucion(ContextoSe contexto, RecepcionDeLineaDtm recepcion, PedidoDtm pedido, LineaDeUnPedidoDtm linea, DateTime devueltaEl)
        {
            if (devueltaEl.Date > DateTime.Today)
                Emitir($"La fecha de devolución de la línea {linea.Orden} ({devueltaEl:dd/MM/yyyy}) no puede ser posterior a hoy");

            var cierre = ExtensorDeSociedades.UltimoCierreContable(contexto, pedido.Cg(contexto).IdSociedad);
            var hayQueRespetarElCierre = cierre is not null && cierre.CerradaEl.Date > recepcion.RecepcionadaEl.Date;
            var desde = hayQueRespetarElCierre ? cierre.CerradaEl.Date : recepcion.RecepcionadaEl.Date;
            if (devueltaEl.Date < desde)
                Emitir(hayQueRespetarElCierre
                    ? $"La fecha de devolución de la línea {linea.Orden} ({devueltaEl:dd/MM/yyyy}) no puede ser anterior al último cierre contable de la sociedad ({desde:dd/MM/yyyy})"
                    : $"La fecha de devolución de la línea {linea.Orden} ({devueltaEl:dd/MM/yyyy}) no puede ser anterior a la de su entrega ({desde:dd/MM/yyyy})");
        }

        // Salida del almacén (devolución de compra) que generó la devolución, null si la entrega fue a la obra de un expediente
        private static MovimientoDeAlmacenDtm MovimientoDeDevolucion(this DevolucionDeEntregaDtm devolucion, ContextoSe contexto)
            => contexto.Set<MovimientoDeAlmacenDtm>().FirstOrDefault(x => x.IdLineaDevolucion == devolucion.Id);

        // Una devolución cuyo preasiento está contabilizado no se puede cancelar (ver GestorDeDevolucionesDeUnPedido.DespuesDeMapearElElemento)
        public static bool DevolucionContabilizada(this DevolucionDeEntregaDtm devolucion, ContextoSe contexto)
            => devolucion.MovimientoDeDevolucion(contexto)?.PreasientoContabilizado(contexto) is not null;

        // Nunca se devuelve más de lo que queda por devolver; la fecha, que la indica el usuario en la modal, ha de estar entre la mayor de
        // la fecha de la entrega y la del último cierre contable de la sociedad del pedido, y hoy; el motivo es obligatorio. Si la entrega
        // fue a un almacén, éste ha de estar activo para sacar lo devuelto
        public static void ValidarDevolucion(this RecepcionDeLineaDtm recepcion, ContextoSe contexto, PedidoDtm pedido, LineaDeUnPedidoDtm linea, decimal cantidad, DateTime devueltaEl, string motivo)
        {
            ValidarFechaDeDevolucion(contexto, recepcion, pedido, linea, devueltaEl);

            if (cantidad <= 0)
                Emitir($"Ha de indicar la cantidad a devolver de la línea {linea.Orden}");

            var pendiente = recepcion.CantidadPendienteDeDevolver(contexto);
            if (pendiente <= 0)
                Emitir($"La entrega de la línea {linea.Orden} ya se ha devuelto por completo");
            if (cantidad > pendiente)
                Emitir($"No se pueden devolver {cantidad} de la línea {linea.Orden}: se entregaron {recepcion.Cantidad} y ya se han devuelto {recepcion.CantidadDevuelta(contexto)}, como mucho se pueden devolver {pendiente}");

            if (motivo.IsNullOrEmpty() || motivo.Trim().Length == 0)
                Emitir($"Ha de indicar el motivo de la devolución de la línea {linea.Orden}");

            if (recepcion.VaAlAlmacen)
            {
                var almacen = recepcion.Almacen(contexto);
                if (!almacen.EstaActivo())
                    Emitir($"No se puede devolver la entrega de la línea {linea.Orden}: el almacén '{almacen.Expresion}' no está activo y no se puede sacar lo devuelto");
            }
        }

        // Saca del almacén lo devuelto con un movimiento de devolución de compra, valorado al precio neto de la entrega. Lleva la entrega
        // (IdLineaDeRecepcion) y la devolución (IdLineaDevolucion). El gestor de movimientos calcula su inventario, lo preasienta y barre los posteriores
        public static MovimientoDeAlmacenDtm SalidaPorDevolucion(this DevolucionDeEntregaDtm devolucion, ContextoSe contexto, RecepcionDeLineaDtm recepcion, LineaDeUnPedidoDtm linea)
        {
            if (recepcion.IdAlmacen is null || linea.IdUnitario is null)
                Emitir($"La devolución de la línea {linea.Orden} no se puede registrar como salida de almacén: falta el almacén o el material");

            var movimiento = new MovimientoDeAlmacenDtm
            {
                IdAlmacen = (int)recepcion.IdAlmacen,
                IdUnitario = (int)linea.IdUnitario,
                IdTipoMovimiento = enumTiposDeMovimiento.DevCompra.LeerTipoDeMovimiento(contexto).Id,
                Cantidad = devolucion.Cantidad,
                Precio = recepcion.Cantidad == 0 ? recepcion.Precio : recepcion.Importe / recepcion.Cantidad,
                RealizadoEl = devolucion.DevueltaEl,
                IdLineaDeRecepcion = recepcion.Id,
                IdLineaDevolucion = devolucion.Id
            };

            return movimiento.InsertarComoAdministrador(contexto);
        }

        // Se anota quién devuelve, porque la traza se guarda como administrador (ver ExtensorDeTrazasDtm.CrearTraza)
        public static string DescripcionParaLaTraza(this DevolucionDeEntregaDtm devolucion, ContextoSe contexto, RecepcionDeLineaDtm recepcion, LineaDeUnPedidoDtm linea, decimal importe)
        {
            var destino = recepcion.VaAlAlmacen
                ? $"almacén {recepcion.Almacen(contexto).Expresion}"
                : $"obra del expediente {contexto.SeleccionarPorId<ServicioDeDatos.Expediente.ExpedienteDtm>((int)recepcion.IdExpediente).Expresion}";

            return $"Devuelta por {contexto.Usuario.Expresion} el {DateTime.Now:dd/MM/yyyy HH:mm}{Environment.NewLine}" +
                   $"Línea {linea.Orden}: {linea.Concepto}{Environment.NewLine}" +
                   $"Entrega del {recepcion.RecepcionadaEl:dd/MM/yyyy} en el {destino}: cantidad {recepcion.Cantidad:N2}, precio {recepcion.Precio:N2}, descuento {(recepcion.Descuento ?? 0):N2} %{Environment.NewLine}" +
                   $"Devuelto el {devolucion.DevueltaEl:dd/MM/yyyy}: cantidad {devolucion.Cantidad:N2}, importe {importe:N2}{Environment.NewLine}" +
                   $"Motivo: {devolucion.Motivo}";
        }
    }
}
