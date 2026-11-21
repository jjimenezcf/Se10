using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Logistica;
using ServicioDeDatos.MaestrosTecnico;
using ServicioDeDatos.Terceros;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

        //------------------------------------------------------------------------------------------- información contable
        // Está definida si tiene cuenta de existencias (la de variación se deduce de ella al grabar, ver PrepararInformacionContable)
        public static bool TieneInformacionContable(this AlmacenDtm almacen) => almacen.IdCuentaDeExistencias is not null;

        // Para preasentar un movimiento el almacén ha de tener definida su información contable. El mensaje indica el método que usaba su
        // sociedad cuando se definía por sociedad, para que el almacén siga con el mismo
        public static void ValidarInformacionContable(this AlmacenDtm almacen, ContextoSe contexto)
        {
            if (almacen.TieneInformacionContable())
                return;

            var sociedad = almacen.Sociedad(contexto);
            var usaba = sociedad.UsabaVariacionDeExistencias();
            Emitir($"El almacén '{almacen.Referencia}' no tiene definida su información contable: indique en él la cuenta de existencias y si usa variación de existencias" +
                   (usaba is null ? "" : $" (su sociedad '{sociedad.Expresion}' {(usaba.Value ? "usaba" : "no usaba")} la variación de existencias)"));
        }

        public static CuentaDtm CuentaDeExistencias(this AlmacenDtm almacen, ContextoSe contexto)
        {
            almacen.ValidarInformacionContable(contexto);
            if (almacen.CuentaDeExistencias is null || almacen.CuentaDeExistencias.Id != almacen.IdCuentaDeExistencias)
                almacen.CuentaDeExistencias = ExtensorDeCuentas.LeerCuenta(contexto, almacen.IdCuentaDeExistencias);
            return almacen.CuentaDeExistencias;
        }

        public static CuentaDtm CuentaDeVariacion(this AlmacenDtm almacen, ContextoSe contexto)
        {
            almacen.ValidarInformacionContable(contexto);
            if (almacen.IdCuentaDeVariacion is null)
                Emitir($"El almacén '{almacen.Referencia}' no tiene cuenta de variación de existencias, grabe de nuevo su información contable para que se deduzca de la de existencias");
            if (almacen.CuentaDeVariacion is null || almacen.CuentaDeVariacion.Id != almacen.IdCuentaDeVariacion)
                almacen.CuentaDeVariacion = ExtensorDeCuentas.LeerCuenta(contexto, almacen.IdCuentaDeVariacion);
            return almacen.CuentaDeVariacion;
        }

        // Opcionales: si no se indican, las compras y las devoluciones con variación de existencias van a la cuenta de gasto de la naturaleza
        public static CuentaDtm CuentaDeCompras(this AlmacenDtm almacen, ContextoSe contexto)
        {
            if (almacen.IdCuentaDeCompras is null)
                return null;
            if (almacen.CuentaDeCompras is null || almacen.CuentaDeCompras.Id != almacen.IdCuentaDeCompras)
                almacen.CuentaDeCompras = ExtensorDeCuentas.LeerCuenta(contexto, almacen.IdCuentaDeCompras);
            return almacen.CuentaDeCompras;
        }

        public static CuentaDtm CuentaDeDevoluciones(this AlmacenDtm almacen, ContextoSe contexto)
        {
            if (almacen.IdCuentaDeDevoluciones is null)
                return null;
            if (almacen.CuentaDeDevoluciones is null || almacen.CuentaDeDevoluciones.Id != almacen.IdCuentaDeDevoluciones)
                almacen.CuentaDeDevoluciones = ExtensorDeCuentas.LeerCuenta(contexto, almacen.IdCuentaDeDevoluciones);
            return almacen.CuentaDeDevoluciones;
        }

        // Al grabar el almacén (ver GestorDeAlmacenes.AntesDePersistir):
        //  - sin cuenta de existencias la información está sin definir: no puede tener el resto de cuentas
        //  - la de existencias ha de ser del grupo 30, 31 o 32; la de variación, si no se indica, se deduce de ella (y se crea si no existe),
        //    y si se indica ha de ser la 61x de su grupo; las de compras (60x de su grupo) y devoluciones (608) son opcionales
        //  - una vez definida, si el almacén tiene movimientos no se puede cambiar el método ni la cuenta de existencias: se cierra y se abre
        //    otro con su inventario
        public static void PrepararInformacionContable(this AlmacenDtm almacen, ContextoSe contexto, AlmacenDtm enBd)
        {
            if (enBd is not null && enBd.TieneInformacionContable()
                && (enBd.UsaVariacionDeExistencias != almacen.UsaVariacionDeExistencias || enBd.IdCuentaDeExistencias != almacen.IdCuentaDeExistencias)
                && contexto.Set<MovimientoDeAlmacenDtm>().Any(m => m.IdAlmacen == almacen.Id))
                Emitir($"No se puede cambiar si usa variación de existencias ni la cuenta de existencias del almacén '{almacen.Referencia}' porque tiene movimientos: ciérrelo y abra otro con su inventario");

            if (!almacen.TieneInformacionContable())
            {
                if (almacen.IdCuentaDeVariacion is not null || almacen.IdCuentaDeCompras is not null || almacen.IdCuentaDeDevoluciones is not null)
                    Emitir($"Indique la cuenta de existencias del almacén '{almacen.Referencia}' antes que el resto de sus cuentas");
                return;
            }

            var existencias = ExtensorDeCuentas.LeerCuenta(contexto, almacen.IdCuentaDeExistencias);
            var grupo = existencias.GrupoDeExistencias();

            if (almacen.IdCuentaDeVariacion is null)
                almacen.IdCuentaDeVariacion = existencias.DeducirCuentaDeVariacion(contexto).Id;
            else
                ExtensorDeCuentas.LeerCuenta(contexto, almacen.IdCuentaDeVariacion).ValidarPrefijo(ExtensorDeCuentas.PrefijoDeVariacion(grupo), $"cuenta de variación de existencias de {ExtensorDeCuentas.DescripcionDelGrupo(grupo)}");

            ExtensorDeCuentas.LeerCuenta(contexto, almacen.IdCuentaDeCompras)?.ValidarPrefijo(ExtensorDeCuentas.PrefijoDeCompras(grupo), $"cuenta de compras de {ExtensorDeCuentas.DescripcionDelGrupo(grupo)}");
            ExtensorDeCuentas.LeerCuenta(contexto, almacen.IdCuentaDeDevoluciones)?.ValidarPrefijo(ExtensorDeCuentas.PrefijoDeDevoluciones, "cuenta de devoluciones de compras");

            // las navegaciones se leen de nuevo con los ids grabados (ver CuentaDeExistencias...)
            almacen.CuentaDeExistencias = almacen.CuentaDeVariacion = almacen.CuentaDeCompras = almacen.CuentaDeDevoluciones = null;
        }

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

        public static UnitarioDtm Unitario(this MovimientoDeAlmacenDtm movimiento, ContextoSe contexto, bool aplicarJoin = false)
        {
            if (movimiento.Unitario is null || movimiento.IdUnitario != movimiento.Unitario.Id)
                movimiento.Unitario = contexto.SeleccionarPorId<UnitarioDtm>(movimiento.IdUnitario, aplicarJoin: aplicarJoin);
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

            preasiento.CancelarPreasiento(contexto);
            movimiento.IdPreasiento = null;
            movimiento.Preasiento = null;
        }

        // Preasiento del movimiento si está en la etapa de contabilizado, o null
        public static PreasientoDtm PreasientoContabilizado(this MovimientoDeAlmacenDtm movimiento, ContextoSe contexto)
        {
            var preasiento = movimiento.Preasiento(contexto);
            return preasiento is not null && preasiento.EstaEnLaEtapa(enumEtapasDePreasiento.SPR_Etapa_Contabilizado) ? preasiento : null;
        }

        // Antes de borrar la entrada de compra de una recepción cancelada: su preasiento, si lo tiene, ha de estar pendiente y se cancela;
        // si está contabilizado está en un lote contable y no se puede cancelar (ver ExtensorDePreasientos.CancelarPreasiento)
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
                documento = (enumNegocio.Pedido, ExtensorDeRecepciones.LineaDePedido(contexto, (int)movimiento.IdLineaDeRecepcion).IdElemento);
            else if (movimiento.IdLineaInventario != null)
                documento = (enumNegocio.Regularizacion, contexto.SeleccionarPorId<LineasDeUnaRegularizacionDtm>((int)movimiento.IdLineaInventario).IdElemento);
            else if (movimiento.IdLineaDeVenta != null)
                documento = (enumNegocio.Venta, contexto.SeleccionarPorId<LineaDeUnaVentaDtm>((int)movimiento.IdLineaDeVenta).IdElemento);

            if (documento.negocio == enumNegocio.No_Definido)
                Emitir($"El movimiento de almacén '{movimiento.Referencia(contexto)}' no tiene un documento de origen o destino al que navegar");

            return documento.negocio.UrlDeEdicion(contexto, documento.id);
        }

        // Url de la edición del preasiento contable del movimiento
        public static string UrlDelPreasiento(this MovimientoDeAlmacenDtm movimiento, ContextoSe contexto)
        {
            var preasiento = movimiento.Preasiento(contexto);
            if (preasiento is null)
                Emitir($"El movimiento de almacén '{movimiento.Referencia(contexto)}' no tiene preasiento contable");
            return enumNegocio.Preasiento.UrlDeEdicion(contexto, preasiento.Id);
        }

        // Url de la edición del proveedor del pedido cuya recepción generó el movimiento
        public static string UrlDelProveedor(this MovimientoDeAlmacenDtm movimiento, ContextoSe contexto)
        {
            var proveedor = movimiento.Proveedor(contexto, emitirErrorSiNoEsRecepcion: true);
            return enumNegocio.Proveedor.UrlDeEdicion(contexto, proveedor.Id);
        }

        // Url de la edición del unitario del movimiento, para navegar desde la consulta de inventario. El unitario no tiene controlador
        // en enumNegocio.Controlador(), por eso se compone con su controlador y su vista de mantenimiento
        public static string UrlDelUnitario(this MovimientoDeAlmacenDtm movimiento)
            => $"{enumControladoresMt.Unitarios}/{enumVistasMts.CrudUnitarios}?Id={movimiento.IdUnitario}";

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
                case enumClaseDeMovimiento.Salida:
                    if (movimiento.IdLineaDevolucion is not null) movimiento.CalcularInventarioDevolucionDeCompra(contexto);
                    else movimiento.CalcularInventarioSalida(contexto);
                    break;
                case enumClaseDeMovimiento.Inicial: movimiento.CalcularInventarioInicial(contexto); break;
                case enumClaseDeMovimiento.Ajuste: movimiento.CalcularInventarioAjuste(contexto); break;
                case enumClaseDeMovimiento.Regularizacion: movimiento.CalcularInventarioRegularizacion(contexto); break;
                case enumClaseDeMovimiento.Nulo: break;
            }
        }

        public static RecepcionDeLineaDtm Recepcion(this MovimientoDeAlmacenDtm movimiento, ContextoSe contexto, bool emitirErrorSiNoEsRecepcion)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Alm_Movimiento_Recepcion);

            if(movimiento.Id == 0)
                return movimiento.LineaDeRecepcion(contexto, emitirErrorSiNoEsRecepcion);

            if (!cache.ContainsKey(movimiento.Id.ToString()))
            {
                cache[movimiento.Id.ToString()] = movimiento.LineaDeRecepcion(contexto, emitirErrorSiNoEsRecepcion);
            }

            if (emitirErrorSiNoEsRecepcion && cache[movimiento.Id.ToString()] is null)
                Emitir($"El movimiento de almacén '{movimiento.Referencia(contexto)}' no es de una entrega de pedido");

            return (RecepcionDeLineaDtm)cache[movimiento.Id.ToString()];
        }

        public static RecepcionDeLineaDtm LineaDeRecepcion(this MovimientoDeAlmacenDtm movimiento, ContextoSe contexto, bool emitirErrorSiNoEsRecepcion)
        {
            if (movimiento.LineaDeRecepcion is not null && movimiento.IdLineaDeRecepcion is not null && movimiento.IdLineaDeRecepcion == movimiento.LineaDeRecepcion.Id)
                return movimiento.LineaDeRecepcion;
            
            if (movimiento.IdLineaDeRecepcion is null)
            {
                if (emitirErrorSiNoEsRecepcion)
                    Emitir($"El movimiento de almacén '{movimiento.Referencia(contexto)}' no es de una entrega de pedido");
                return null;
            }

            return movimiento.LineaDeRecepcion = contexto.SeleccionarPorId<RecepcionDeLineaDtm>((int)movimiento.IdLineaDeRecepcion, aplicarJoin: true);
        }

        public static PedidoDtm Pedido(this MovimientoDeAlmacenDtm movimiento, ContextoSe contexto, bool emitirErrorSiNoEsRecepcion)
        {
            if (movimiento.IdLineaDeRecepcion is null)
            {
                if (emitirErrorSiNoEsRecepcion)
                    Emitir($"El movimiento de almacén '{movimiento.Referencia(contexto)}' no es de una entrega de pedido");
                return null;
            }

            var lineaPedido = ExtensorDeRecepciones.LineaDePedido(contexto, (int)movimiento.IdLineaDeRecepcion);
            return lineaPedido.DetalleDe<PedidoDtm>(contexto, usarLaCache: true);
        }

        public static ProveedorDtm Proveedor(this MovimientoDeAlmacenDtm movimiento, ContextoSe contexto, bool emitirErrorSiNoEsRecepcion)
        {
            var pedido = movimiento.Pedido(contexto, emitirErrorSiNoEsRecepcion);
            if (pedido is null)
                return null;
            return pedido.Proveedor(contexto);
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

        // Valor del inventario del almacén antes del instante indicado: por cada unitario su último movimiento anterior, que tiene el
        // stock y el precio medio (Valor) vigentes, como en la consulta de inventario (ver GestorDeInventario.AplicarFiltros)
        public static decimal ValorDelInventario(this AlmacenDtm almacen, ContextoSe contexto, System.DateTime antesDe)
        {
            var movimientos = contexto.Set<MovimientoDeAlmacenDtm>();
            var valor = movimientos
                .Where(m => m.IdAlmacen == almacen.Id
                         && m.RealizadoEl < antesDe
                         && m.TipoMovimiento.ClaseMovimiento != enumClaseDeMovimiento.Nulo
                         && !movimientos.Any(p => p.IdAlmacen == m.IdAlmacen
                                               && p.IdUnitario == m.IdUnitario
                                               && p.RealizadoEl < antesDe
                                               && p.TipoMovimiento.ClaseMovimiento != enumClaseDeMovimiento.Nulo
                                               && (p.RealizadoEl > m.RealizadoEl || (p.RealizadoEl == m.RealizadoEl && p.Id > m.Id))))
                .Select(m => m.Stock * m.Valor)
                .ToList()
                .Sum();
            return System.Math.Round(valor, 2);
        }

        // Valor de los inventarios de apertura del almacén realizados en [desde, hasta): los stocks iniciales que no vienen de un recuento (sin
        // línea de inventario o de una regularización de clase Inicial). No se preasientan porque son los saldos de apertura, que ya están en
        // la contabilidad (ver ExtensorDePreasientos.PreasentarStockInicial). Un stock inicial no puede tener movimientos anteriores del mismo
        // material (ver CalcularInventarioInicial), así que su valor es el que añade al inventario
        public static decimal ValorDeLaAperturaEntre(this AlmacenDtm almacen, ContextoSe contexto, System.DateTime desde, System.DateTime hasta)
        {
            var tiposDeApertura = contexto.Set<TipoDeRegularizacionDtm>().Where(t => t.ClaseDeRegularizacion == enumRegularizacionAlm.Inicial).Select(t => t.Id);
            var regularizacionesDeApertura = contexto.Set<RegularizacionDtm>().Where(r => tiposDeApertura.Contains(r.IdTipo)).Select(r => r.Id);
            var lineasDeApertura = contexto.Set<LineasDeUnaRegularizacionDtm>().Where(l => regularizacionesDeApertura.Contains(l.IdElemento)).Select(l => l.Id);

            var valor = contexto.Set<MovimientoDeAlmacenDtm>()
                .Where(m => m.IdAlmacen == almacen.Id
                         && m.RealizadoEl >= desde && m.RealizadoEl < hasta
                         && m.TipoMovimiento.ClaseMovimiento == enumClaseDeMovimiento.Inicial
                         && (m.IdLineaInventario == null || lineasDeApertura.Contains((int)m.IdLineaInventario)))
                .Select(m => m.Stock * m.Valor)
                .ToList()
                .Sum();
            return System.Math.Round(valor, 2);
        }

        // Último movimiento del material en el almacén hasta el instante indicado, incluido (el que tiene el stock y el valor vigentes en ese
        // momento); null si todavía no tenía movimientos. Los del mismo instante cuentan como anteriores porque un movimiento nuevo irá después
        // de ellos (ver ObtenerMovimientoAnterior)
        public static MovimientoDeAlmacenDtm UltimoMovimientoHasta(ContextoSe contexto, int idAlmacen, int idUnitario, System.DateTime instante)
        {
            return contexto.Set<MovimientoDeAlmacenDtm>()
                .Where(x => x.IdAlmacen == idAlmacen
                         && x.IdUnitario == idUnitario
                         && x.RealizadoEl <= instante
                         && x.TipoMovimiento.ClaseMovimiento != enumClaseDeMovimiento.Nulo)
                .OrderByDescending(x => x.RealizadoEl).ThenByDescending(x => x.Id)
                .FirstOrDefault();
        }

        public static MovimientoDeAlmacenDtm ObtenerMovimientoAnterior(this MovimientoDeAlmacenDtm movimiento, ContextoSe contexto)
        {
            // un movimiento que aún no se ha insertado (Id 0) irá después de los que ya existen en el mismo instante (p.e. el ajuste de precio
            // que sigue en el mismo día a la regularización de stock de una línea del recuento)
            var esNuevo = movimiento.Id == 0;
            return contexto.Set<MovimientoDeAlmacenDtm>()
                .Where(x => x.IdAlmacen == movimiento.IdAlmacen
                         && x.IdUnitario == movimiento.IdUnitario
                         && x.Id != movimiento.Id
                         && x.TipoMovimiento.ClaseMovimiento != enumClaseDeMovimiento.Nulo
                         && (x.RealizadoEl < movimiento.RealizadoEl || (x.RealizadoEl == movimiento.RealizadoEl && (esNuevo || x.Id < movimiento.Id))))
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

        // La devolución de compra saca lo devuelto a su precio de compra (el neto de la entrega, no el precio medio), deshaciendo lo que
        // la entrada aportó al valor: el precio medio de lo que queda es el que había sin lo devuelto
        private static void CalcularInventarioDevolucionDeCompra(this MovimientoDeAlmacenDtm movimiento, ContextoSe contexto)
        {
            var anterior = movimiento.ObtenerMovimientoAnterior(contexto);
            if (anterior is null)
                Emitir($"No se puede devolver en el movimiento de almacén '{movimiento.Referencia(contexto)}' ya que no hay stock anterior para el almacén y el material");

            movimiento.Stock = anterior.Stock - movimiento.Cantidad;
            if (movimiento.Stock < 0)
                Emitir($"No hay stock suficiente para la devolución del movimiento de almacén '{movimiento.Referencia(contexto)}': quedan {anterior.Stock} y se devuelven {movimiento.Cantidad}");

            var valorQueQueda = anterior.Stock * anterior.Valor - movimiento.Cantidad * movimiento.Precio;
            movimiento.Valor = movimiento.Stock == 0 || valorQueQueda < 0 ? anterior.Valor : valorQueQueda / movimiento.Stock;
        }

        private static void CalcularInventarioInicial(this MovimientoDeAlmacenDtm movimiento, ContextoSe contexto)
        {
            var anterior = movimiento.ObtenerMovimientoAnterior(contexto);
            if (anterior is not null)
                Emitir($"El material '{movimiento.Unitario(contexto).Referencia}' no puede tener un stock inicial el {movimiento.RealizadoEl:dd/MM/yyyy HH:mm} ya que tiene movimientos anteriores en el almacén, use una regularización o un ajuste de precio");

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
