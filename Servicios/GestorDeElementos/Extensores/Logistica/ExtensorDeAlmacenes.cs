using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Logistica;
using ServicioDeDatos.MaestrosTecnico;
using System.Collections.Generic;
using System.Linq;
using Utilidades;
using static Gestor.Errores.GestorDeErrores;

namespace GestorDeElementos.Extensores
{
    public static class ExtensorDeAlmacenes
    {
        public static bool EstaActivo(this AlmacenDtm almacen) => almacen.EstaEnLaEtapa(enumEtapasDeAlmacen.ALM_Etapa_Activo);

        public static void AntesDeCancelar(this AlmacenDtm almacen, ContextoSe contexto) => almacen.ValidarSinRegularizacionViva(contexto, "cancelar");

        public static void AntesDeCerrar(this AlmacenDtm almacen, ContextoSe contexto) => almacen.ValidarSinRegularizacionViva(contexto, "cerrar");

        public static void AntesDeRecontar(this AlmacenDtm almacen, ContextoSe contexto) => almacen.ValidarQueNoSSeEstaRecontando(contexto);

        // Los tipos de movimiento se leen todos de una vez y se cachean por nombre (la descripción del enumerado), sin seguimiento porque
        // la caché se comparte entre peticiones. Se olvidan al persistir un tipo de movimiento (ver GestorDeTiposDeMovimiento)
        public static TipoMovimientoDtm LeerTipoDeMovimiento(this enumTiposDeMovimiento tipo, ContextoSe contexto)
        {
            var tipos = TiposDeMovimiento(contexto);

            if (!tipos.Any(t => t.Nombre == tipo.Descripcion()))
                Emitir($"No está definido el tipo de movimiento de almacén '{tipo.Descripcion()}'");

            return tipos.First(x => x.Nombre == tipo.Descripcion());
        }

        public static TipoMovimientoDtm TipoDeMovimiento(this MovimientoDeAlmacenDtm movimiento, ContextoSe contexto)
        {
            return TiposDeMovimiento(contexto).First(x => x.Id == movimiento.IdTipoMovimiento);
        }

        public static UnitarioDtm Unitario(this MovimientoDeAlmacenDtm movimiento, ContextoSe contexto)
        {
            if (movimiento.Unitario is null || movimiento.IdUnitario != movimiento.Unitario.Id)
                movimiento.Unitario = contexto.SeleccionarPorId<UnitarioDtm>(movimiento.IdUnitario);
            return movimiento.Unitario;
        }

        public static AlmacenDtm Almacen(this MovimientoDeAlmacenDtm movimiento, ContextoSe contexto)
        {
            if (movimiento.Almacen is null || movimiento.IdAlmacen != movimiento.Almacen.Id)
                movimiento.Almacen = contexto.SeleccionarPorId<AlmacenDtm>(movimiento.IdAlmacen);
            return movimiento.Almacen;
        }

        public static PreasientoDtm Preasiento(this MovimientoDeAlmacenDtm movimiento, ContextoSe contexto)
        {
            if (movimiento.IdPreasiento is null)
                return null;

            if (movimiento.Preasiento is null || movimiento.IdPreasiento != movimiento.Preasiento.Id)
                movimiento.Preasiento = contexto.SeleccionarPorId<PreasientoDtm>((int)movimiento.IdPreasiento);
            return movimiento.Preasiento;
        }

        public static void QuitarPreasiento(this MovimientoDeAlmacenDtm movimiento, ContextoSe contexto)
        {
            if (movimiento.IdPreasiento is null)
                return;

            var preasiento = movimiento.Preasiento(contexto);
            if (preasiento.EstaEnLaEtapa(enumEtapasDePreasiento.SPR_Etapa_Contabilizado))
                Emitir($"No se puede modificar el movimiento de almacén Nº '{movimiento.Id}' ya que el preasiento '{preasiento.Referencia}' asociado está contabilizado");

            preasiento.CancelarPreasiento(contexto, permitirCancelarUnContabilizado: false);
            movimiento.IdPreasiento = null;
            movimiento.Preasiento = null;
        }

        // Antes de borrar la entrada de compra de una recepción cancelada: su preasiento, si lo tiene, ha de estar pendiente y se anula
        public static void CancelarPreasientoDeRecepcion(this MovimientoDeAlmacenDtm movimiento, ContextoSe contexto)
        {
            if (movimiento.IdLineaDeRecepcion is null)
                Emitir($"El movimiento de referencia '{movimiento.Referencia(contexto)}' no es una entrada de compra, no se puede anular");

            if (movimiento.IdPreasiento is null)
                return;

            movimiento.CancelarPreasiento(contexto);

        }

        // La referencia no se guarda (se ignora en el modelo), se calcula como la de los cobros (ver ExtensorDeCobrosDeFae.CalcularReferencia):
        // al mapear, al persistir y al leer (ver GestorDeMovimientosDeAlmacen). Al insertar aún no hay Id y acaba en "(0)", se recalcula al leerlo
        // Url de la edición del documento que generó el movimiento: el pedido de la recepción o la regularización del inventario.
        // Los movimientos de obra y las devoluciones aún no tienen negocio al que navegar
        public static string UrlDelDocumento(this MovimientoDeAlmacenDtm movimiento, ContextoSe contexto)
        {
            (enumNegocio negocio, int id) documento = (enumNegocio.No_Definido, 0);
            if (movimiento.IdLineaDeRecepcion != null)
                documento = (enumNegocio.Pedido, ExtensorDeRecepciones.LeerLineaDePedido(contexto, (int)movimiento.IdLineaDeRecepcion).IdElemento);
            else if (movimiento.IdLineaInventario != null)
                documento = (enumNegocio.Regularizacion, contexto.SeleccionarPorId<LineasDeUnaRegularizacionDtm>((int)movimiento.IdLineaInventario).IdElemento);

            if (documento.negocio == enumNegocio.No_Definido)
                Emitir($"El movimiento de almacén '{movimiento.Referencia(contexto)}' no tiene un documento de origen o destino al que navegar");

            return $"{documento.negocio.Controlador()}/{documento.negocio.VistaMvc(contexto).Accion}?Id={documento.id}";
        }

        public static string Referencia(this MovimientoDeAlmacenDtm movimiento, ContextoSe contexto)
        {
            if (movimiento.Referencia.IsNullOrEmpty() || movimiento.Referencia.EndsWith("(0)"))
            {
                movimiento.Referencia = movimiento.Almacen(contexto).Referencia + ": " +
                    TiposDeMovimiento(contexto).First(x => x.Id == movimiento.IdTipoMovimiento).ClaseMovimiento.Descripcion() + ", Fecha: " +
                    movimiento.RealizadoEl.ToString("dd/MM/yyyy") + $" ({movimiento.Id})";
            }
            return movimiento.Referencia;
        }

        // Recalcula y guarda, en orden, los movimientos posteriores del mismo almacén y unitario
        public static void Barrer(this MovimientoDeAlmacenDtm movimiento, ContextoSe contexto)
        {
            foreach (var posterior in movimiento.ObtenerMovimientosPosteriores(contexto))
            {
                posterior.CalcularTotalesDelMovimiento(contexto);
                posterior.ModificarComoAdministrador(contexto, parametros: new Dictionary<string, object> { { ltrDeUnMovimientoDeAlmacen.EstoyBarriendo, true } });
            }
        }

        // Valor es el precio medio ponderado (por unidad) tras el movimiento; Stock, las unidades que quedan
        public static void CalcularInventarioDelMovimiento(this MovimientoDeAlmacenDtm movimiento, ContextoSe contexto)
        {
            var clase = movimiento.TipoDeMovimiento(contexto).ClaseMovimiento;

            switch (clase)
            {
                case enumClaseDeMovimiento.Entrada: movimiento.CalcularInventarioEntrada(contexto); break;
                case enumClaseDeMovimiento.Salida: movimiento.CalcularInventarioSalida(contexto); break;
                case enumClaseDeMovimiento.Inicial: movimiento.CalcularInventarioInicial(contexto); break;
                case enumClaseDeMovimiento.Ajuste: movimiento.CalcularInventarioAjuste(contexto); break;
                case enumClaseDeMovimiento.Regularizacion: movimiento.CalcularInventarioRegularizacion(contexto); break;
                case enumClaseDeMovimiento.Nulo: break;
            }
        }

        public static PedidoDtm LeerPedido(this MovimientoDeAlmacenDtm movimiento, ContextoSe contexto, bool emitirErrorSiNoEsRecepcion)
        {
            if (movimiento.IdLineaDeRecepcion is null)
            {
                if (emitirErrorSiNoEsRecepcion)
                    Emitir($"El movimiento de almacén '{movimiento.Id}' no es de una entrega de pedido");
                return null;
            }

            var lineaPedido = ExtensorDeRecepciones.LeerLineaDePedido(contexto, (int)movimiento.IdLineaDeRecepcion);
            return lineaPedido.DetalleDe<PedidoDtm>(contexto, usarLaCache: true);
        }

        private static void ValidarSinRegularizacionViva(this AlmacenDtm almacen, ContextoSe contexto, string accion)
        {
            var estadosIniciada = enumEtapasDeRegularizacion.RAL_Inicial.Lista();
            var estadosRecontando = enumEtapasDeRegularizacion.RAL_Recontando.Lista();

            if (!estadosIniciada.Any())
                Emitir($"Debe definir el parámetro '{enumEtapasDeRegularizacion.RAL_Inicial}' del negocio de '{enumNegocio.Regularizacion}'");

            if (!estadosRecontando.Any())
                Emitir($"Debe definir el parámetro '{enumEtapasDeRegularizacion.RAL_Recontando}' del negocio de '{enumNegocio.Regularizacion}'");

            var estadosVivos = estadosIniciada.Union(estadosRecontando).ToList();
            var hayRegularizacionViva = contexto.Set<RegularizacionDtm>().Any(x => x.IdAlmacen == almacen.Id && estadosVivos.Contains(x.IdEstado));
            if (hayRegularizacionViva)
                Emitir($"No se puede {accion} el almacén '{almacen.Referencia}' porque tiene una regularización en curso");
        }

        private static void ValidarQueNoSSeEstaRecontando(this AlmacenDtm almacen, ContextoSe contexto)
        {
            if (!almacen.EstaRecontando(contexto))
                return;

            var estadosRecontando = enumEtapasDeRegularizacion.RAL_Recontando.Lista();
            var regularizacion = contexto.Set<RegularizacionDtm>().FirstOrDefault(x => x.IdAlmacen == almacen.Id && estadosRecontando.Contains(x.IdEstado));
            if (regularizacion != null)
                Emitir($"El almacén '{almacen.Referencia}' ya se está recontando en la regularización '{regularizacion.Referencia}'");
        }

        private static bool EstaRecontando(this AlmacenDtm almacen, ContextoSe contexto)
        {
            var estadosRecontando = enumEtapasDeRegularizacion.RAL_Recontando.Lista();

            if (!estadosRecontando.Any())
                Emitir($"Debe definir el parámetro '{enumEtapasDeRegularizacion.RAL_Recontando}' del negocio de '{enumNegocio.Regularizacion}'");

            return contexto.Set<RegularizacionDtm>().Any(x => x.IdAlmacen == almacen.Id && estadosRecontando.Contains(x.IdEstado));
        }

        private static List<TipoMovimientoDtm> TiposDeMovimiento(ContextoSe contexto)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Alm_TiposDeMovimiento);
            if (!cache.ContainsKey(nameof(TipoMovimientoDtm)))
            {
                var tipos = contexto.Set<TipoMovimientoDtm>().ToList();
                cache[nameof(TipoMovimientoDtm)] = tipos;
            }
            return cache[nameof(TipoMovimientoDtm)] as List<TipoMovimientoDtm>;
        }

        private static MovimientoDeAlmacenDtm ObtenerMovimientoAnterior(this MovimientoDeAlmacenDtm movimiento, ContextoSe contexto)
        {
            return contexto.Set<MovimientoDeAlmacenDtm>()
                .Where(x => x.IdAlmacen == movimiento.IdAlmacen
                         && x.IdUnitario == movimiento.IdUnitario
                         && x.Id != movimiento.Id
                         && x.TipoMovimiento.ClaseMovimiento != enumClaseDeMovimiento.Nulo
                         && (x.RealizadoEl < movimiento.RealizadoEl || (x.RealizadoEl == movimiento.RealizadoEl && x.Id < movimiento.Id)))
                .OrderByDescending(x => x.RealizadoEl).ThenByDescending(x => x.Id)
                .FirstOrDefault();
        }

        private static List<MovimientoDeAlmacenDtm> ObtenerMovimientosPosteriores(this MovimientoDeAlmacenDtm movimiento, ContextoSe contexto)
        {
            return contexto.Set<MovimientoDeAlmacenDtm>()
                .Where(x => x.IdAlmacen == movimiento.IdAlmacen
                         && x.IdUnitario == movimiento.IdUnitario
                         && x.Id != movimiento.Id
                         && (x.RealizadoEl > movimiento.RealizadoEl || (x.RealizadoEl == movimiento.RealizadoEl && x.Id > movimiento.Id)))
                .OrderBy(x => x.RealizadoEl).ThenBy(x => x.Id)
                .ToList();
        }

        // Recalcula un movimiento ya existente: cancela su preasiento (no puede estar contabilizado), recalcula su inventario y lo
        // vuelve a preasentar. Lo usan el barrido de los posteriores (que luego persisten con EstoyBarriendo=true para que
        // AntesDePersistir/DespuesDePersistir no repitan el trabajo) y la modificación de un movimiento (ver GestorDeMovimientosDeAlmacen)
        public static void CalcularTotalesDelMovimiento(this MovimientoDeAlmacenDtm movimiento, ContextoSe contexto)
        {
            if (movimiento.Id > 0)
            {
                movimiento.QuitarPreasiento(contexto);
            }
            movimiento.CalcularInventarioDelMovimiento(contexto);
            movimiento.Preasentar(contexto);
        }

        private static void CalcularInventarioEntrada(this MovimientoDeAlmacenDtm movimiento, ContextoSe contexto)
        {
            var anterior = movimiento.ObtenerMovimientoAnterior(contexto);
            if (anterior is null)
            {
                movimiento.Stock = movimiento.Cantidad;
                movimiento.Valor = movimiento.Precio;
            }
            else
            {
                movimiento.Stock = anterior.Stock + movimiento.Cantidad;
                movimiento.Valor = movimiento.Stock == 0
                    ? movimiento.Precio
                    : ((anterior.Stock * anterior.Valor) + (movimiento.Cantidad * movimiento.Precio)) / movimiento.Stock;
            }
        }

        private static void CalcularInventarioSalida(this MovimientoDeAlmacenDtm movimiento, ContextoSe contexto)
        {
            var anterior = movimiento.ObtenerMovimientoAnterior(contexto);
            if (anterior is null)
                Emitir($"No se puede dar salida en el movimiento de almacén Nº '{movimiento.Id}' ya que no hay stock anterior para el almacén y el unitario");

            movimiento.Stock = anterior.Stock - movimiento.Cantidad;
            if (movimiento.Stock < 0)
                Emitir($"No hay stock suficiente para la salida del movimiento de almacén Nº '{movimiento.Id}': quedan {anterior.Stock} y se piden {movimiento.Cantidad}");

            // la salida se valora al precio medio vigente, que no cambia por salir
            movimiento.Precio = anterior.Valor;
            movimiento.Valor = anterior.Valor;
        }

        private static void CalcularInventarioInicial(this MovimientoDeAlmacenDtm movimiento, ContextoSe contexto)
        {
            var anterior = movimiento.ObtenerMovimientoAnterior(contexto);
            if (anterior is not null)
                Emitir($"El movimiento de almacén Nº '{movimiento.Id}' no puede ser un stock inicial ya que ya hay movimientos anteriores para el almacén y el unitario, use una regularización o un ajuste de precio");

            movimiento.Stock = movimiento.Cantidad;
            movimiento.Valor = movimiento.Precio;
        }

        private static void CalcularInventarioAjuste(this MovimientoDeAlmacenDtm movimiento, ContextoSe contexto)
        {
            var anterior = movimiento.ObtenerMovimientoAnterior(contexto);
            if (anterior is null)
                Emitir($"El movimiento de almacén Nº '{movimiento.Id}' no puede ser un ajuste de precio ya que no hay movimientos anteriores para el almacén y el unitario, use un stock inicial");

            movimiento.Cantidad = anterior.Stock;
            movimiento.Stock = anterior.Stock;
            movimiento.Valor = movimiento.Precio;
        }

        private static void CalcularInventarioRegularizacion(this MovimientoDeAlmacenDtm movimiento, ContextoSe contexto)
        {
            var anterior = movimiento.ObtenerMovimientoAnterior(contexto);
            if (anterior is null)
                Emitir($"El movimiento de almacén Nº '{movimiento.Id}' no puede ser una regularización ya que no hay movimientos anteriores para el almacén y el unitario, use un stock inicial");

            movimiento.Precio = anterior.Valor;
            movimiento.Stock = movimiento.Cantidad;
            movimiento.Valor = anterior.Valor;
        }
    }

}
