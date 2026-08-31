using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Logistica;
using System.Collections.Generic;
using System.Linq;
using Utilidades;
using static Gestor.Errores.GestorDeErrores;

namespace GestorDeElementos.Extensores
{
    public static class ExtensorDeAlmacenes
    {
        public static void AntesDeCancelar(this AlmacenDtm almacen, ContextoSe contexto) => almacen.ValidarSinRegularizacionViva(contexto, "cancelar");

        public static void AntesDeCerrar(this AlmacenDtm almacen, ContextoSe contexto) => almacen.ValidarSinRegularizacionViva(contexto, "cerrar");

        public static void AntesDeRecontar(this AlmacenDtm almacen, ContextoSe contexto) => almacen.ValidarSinRegularizacionViva(contexto, "inventariar");

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

        public static void QuitarPreasiento(this MovimientoDeAlmacenDtm movimiento, ContextoSe contexto)
        {
            if (movimiento.IdPreasiento is null)
                return;

            var preasiento = movimiento.Preasiento ?? contexto.SeleccionarPorId<PreasientoDtm>((int)movimiento.IdPreasiento);
            if (preasiento.EstaEnLaEtapa(enumEtapasDePreasiento.SPR_Etapa_Contabilizado))
                Emitir($"No se puede modificar el movimiento de almacén Nº '{movimiento.Id}' ya que el preasiento '{preasiento.Referencia}' asociado está contabilizado");

            preasiento.CancelarPreasiento(contexto);
            movimiento.IdPreasiento = null;
            movimiento.Preasiento = null;
        }

        public static MovimientoDeAlmacenDtm ObtenerMovimientoAnterior(this MovimientoDeAlmacenDtm movimiento, ContextoSe contexto)
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

        public static List<MovimientoDeAlmacenDtm> ObtenerMovimientosPosteriores(this MovimientoDeAlmacenDtm movimiento, ContextoSe contexto)
        {
            return contexto.Set<MovimientoDeAlmacenDtm>()
                .Where(x => x.IdAlmacen == movimiento.IdAlmacen
                         && x.IdUnitario == movimiento.IdUnitario
                         && x.Id != movimiento.Id
                         && (x.RealizadoEl > movimiento.RealizadoEl || (x.RealizadoEl == movimiento.RealizadoEl && x.Id > movimiento.Id)))
                .OrderBy(x => x.RealizadoEl).ThenBy(x => x.Id)
                .ToList();
        }

        // Orquesta el recálculo completo de un movimiento posterior durante un barrido: quita su preasiento
        // anterior, recalcula su inventario y lo vuelve a preasentar. Tras llamarlo, el movimiento se
        // persiste con EstoyBarriendo=true para que AntesDePersistir/DespuesDePersistir no repitan el trabajo.
        public static void Barrer(this MovimientoDeAlmacenDtm movimiento, ContextoSe contexto)
        {
            movimiento.QuitarPreasiento(contexto);
            movimiento.CalcularInventarioDelMovimiento(contexto);
            movimiento.Preasentar(contexto);
        }

        public static void CalcularInventarioDelMovimiento(this MovimientoDeAlmacenDtm movimiento, ContextoSe contexto)
        {
            var clase = movimiento.TipoMovimiento?.ClaseMovimiento ?? contexto.SeleccionarPorId<TipoMovimientoDtm>(movimiento.IdTipoMovimiento).ClaseMovimiento;

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

        public static void CalcularInventarioEntrada(this MovimientoDeAlmacenDtm movimiento, ContextoSe contexto)
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
                movimiento.Valor = (anterior.Valor + (movimiento.Precio * movimiento.Cantidad)) / (anterior.Cantidad + movimiento.Cantidad);
            }
        }

        public static void CalcularInventarioSalida(this MovimientoDeAlmacenDtm movimiento, ContextoSe contexto)
        {
            var anterior = movimiento.ObtenerMovimientoAnterior(contexto);
            if (anterior is null)
                Emitir($"No se puede dar salida en el movimiento de almacén Nº '{movimiento.Id}' ya que no hay stock anterior para el almacén y el unitario");

            movimiento.Stock = anterior.Stock - movimiento.Cantidad;
            movimiento.Valor = (anterior.Valor - (movimiento.Precio * movimiento.Cantidad)) / (anterior.Cantidad - movimiento.Cantidad);
        }

        public static void CalcularInventarioInicial(this MovimientoDeAlmacenDtm movimiento, ContextoSe contexto)
        {
            var anterior = movimiento.ObtenerMovimientoAnterior(contexto);
            if (anterior is not null)
                Emitir($"El movimiento de almacén Nº '{movimiento.Id}' no puede ser un stock inicial ya que ya hay movimientos anteriores para el almacén y el unitario, use una regularización o un ajuste de precio");

            movimiento.Stock = movimiento.Cantidad;
            movimiento.Valor = movimiento.Precio;
        }

        public static void CalcularInventarioAjuste(this MovimientoDeAlmacenDtm movimiento, ContextoSe contexto)
        {
            var anterior = movimiento.ObtenerMovimientoAnterior(contexto);
            if (anterior is null)
                Emitir($"El movimiento de almacén Nº '{movimiento.Id}' no puede ser un ajuste de precio ya que no hay movimientos anteriores para el almacén y el unitario, use un stock inicial");

            movimiento.Cantidad = anterior.Cantidad;
            movimiento.Stock = anterior.Cantidad;
            movimiento.Valor = movimiento.Cantidad * movimiento.Precio;
        }

        public static void CalcularInventarioRegularizacion(this MovimientoDeAlmacenDtm movimiento, ContextoSe contexto)
        {
            var anterior = movimiento.ObtenerMovimientoAnterior(contexto);
            if (anterior is null)
                Emitir($"El movimiento de almacén Nº '{movimiento.Id}' no puede ser una regularización ya que no hay movimientos anteriores para el almacén y el unitario, use un stock inicial");

            movimiento.Precio = anterior.Precio;
            movimiento.Stock = movimiento.Cantidad;
            movimiento.Valor = movimiento.Cantidad * movimiento.Precio;
        }
    }
}
