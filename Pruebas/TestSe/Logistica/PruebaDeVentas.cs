using System;
using System.Collections.Generic;
using System.Linq;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Logistica;
using ServicioDeDatos.MaestrosTecnico;
using ServicioDeDatos.Ventas;
using Utilidades;
using static ServicioDeDatos.Elemento.Enumerados;

namespace TestLogistica
{
    // Venta de material (documento de salida) desde un almacén con o sin variación de existencias, como desde la consulta de inventario
    // (ExtensorDeVentas.CrearVentaDeMaterial), servida y facturada; después se cancela su prefactura y se devuelve a solicitado:
    //  - al servir se crea la salida del almacén valorada al precio medio; sin variación de existencias se preasienta (variación 61x al debe y
    //    existencias 3xx al haber), con variación no
    //  - al facturar se crea la prefactura al cliente con el tipo de prefactura del tipo de venta, y la venta apunta a ella
    //  - al anular la prefactura la venta vuelve a servida, se desvincula de ella y la transición queda anotada con una observación de asunto
    //    "Al cancelar prefactura {referencia}" y de cuerpo el enlace a la prefactura
    //  - al devolver a solicitado se quita la salida del almacén (el stock vuelve a ser el de antes de la venta) y se cancela su preasiento
    // El almacén y su stock salen de la prueba de compras. Nunca se emite la factura: sólo se trabaja con la prefactura
    public class PruebaDeVentas
    {
        private readonly ConfigDeLogistica _config;
        private readonly bool _conVariacion;

        public string Nombre => $"Venta de material {(_conVariacion ? "con" : "sin")} variación de existencias: prefacturar, cancelar la prefactura y devolver a solicitado";

        private ContextoSe _contexto;
        private Informe _informe;
        private AlmacenDtm _almacen;
        private UnitarioDtm _unitario;
        private decimal _stockAntesDeVender;
        private VentaDtm _venta;
        private FacturaEmtDtm _prefactura;
        private MovimientoDeAlmacenDtm _salida;

        public PruebaDeVentas(ConfigDeLogistica config, bool conVariacion)
        {
            _config = config;
            _conVariacion = conVariacion;
        }

        public void Ejecutar(ContextoSe contexto, Informe informe)
        {
            _contexto = contexto;
            _informe = informe;

            var compras = new PruebaDeCompras(_config, _conVariacion);
            compras.Ejecutar(contexto, informe);
            _almacen = compras.Almacen;

            DefinirElTipoDePrefactura(_contexto, _informe);
            Vender();
            ValidarLaSalidaDelAlmacen();
            ValidarLaPrefactura();
            CancelarLaPrefactura();
            DevolverASolicitado();
        }

        //------------------------------------------------------------------------------------------------------------ tipo de prefactura
        // El tipo con el que se venden materiales (el primero activo que permite crear) ha de indicar el tipo de prefactura: si no lo tiene se
        // le asigna el primero de facturas emitidas (se deshace con el rollback)
        internal static void DefinirElTipoDePrefactura(ContextoSe _contexto, Informe _informe)
        {
            _informe.Paso("Comprobar el tipo de prefactura del tipo de venta");
            var tipo = _contexto.Set<TipoDeVentaDtm>().Where(t => t.Activo && t.PermiteCrear).OrderBy(t => t.Id).FirstOrDefault();
            if (!_informe.Comprobar(tipo is not null, "hay un tipo de venta activo que permite crear"))
                throw new Exception("No se puede vender sin tipo de venta");

            if (tipo.IdTipoFacturaEmt is null)
            {
                var tipoDeFactura = _contexto.Set<TipoDeFacturaEmtDtm>().OrderBy(t => t.Id).First();
                _contexto.Database.ExecuteSqlInterpolated($"UPDATE LOGISTICA.VENTA_TIPO SET ID_TIPO_FACTURA = {tipoDeFactura.Id} WHERE ID = {tipo.Id}");
                ServicioDeCaches.EliminarTodas();
                _informe.Dato($"al tipo '{tipo.Nombre}' se le asigna el tipo de prefactura '{tipoDeFactura.Nombre}'");
            }
            tipo = _contexto.SeleccionarPorId<TipoDeVentaDtm>(tipo.Id, usarLaCache: false);
            _informe.Comprobar(tipo.IdTipoFacturaEmt is not null, $"el tipo '{tipo.Nombre}' tiene tipo de prefactura");
        }

        //------------------------------------------------------------------------------------------------------------ vender
        private void Vender()
        {
            var lineaDePrueba = _config.Lineas[0];
            _unitario = _contexto.SeleccionarPorId<UnitarioDtm>(lineaDePrueba.IdUnitario, usarLaCache: false);
            _informe.Paso($"Vender {_config.CantidadAVender} de '{_unitario.Nombre}' a {_config.PrecioDeVenta:N2} al cliente {_config.IdCliente}, servido y facturado");

            _stockAntesDeVender = Stock();
            _informe.Dato($"stock antes de vender: {_stockAntesDeVender:0.####}");

            _venta = ExtensorDeVentas.CrearVentaDeMaterial(_contexto, _almacen.Id, _unitario.Id, _config.IdCliente, DateTime.Today,
                _config.CantidadAVender, _config.PrecioDeVenta, servido: true, facturar: true);
            _venta = Releer(_venta);

            _informe.Dato($"documento de venta '{_venta.Referencia}' (id {_venta.Id})");
            _informe.Comprobar(_venta.EstaEnLaEtapa(enumEtapasDeVenta.DSA_Etapa_Facturacion), "el documento de venta está facturado", $"está en '{_venta.Estado(_contexto).Nombre}'");
            _informe.Comprobar(_venta.ServidoEl?.Date == DateTime.Today, "servido hoy");
            _informe.Comprobar(_venta.IdFacturaEmt is not null, "el documento de venta apunta a su prefactura");

            var lineas = _contexto.Set<LineaDeUnaVentaDtm>().Where(l => l.IdElemento == _venta.Id).ToList();
            _informe.Comprobar(lineas.Count == 1 && lineas[0].IdUnitario == _unitario.Id && lineas[0].Cantidad == _config.CantidadAVender && lineas[0].Precio == _config.PrecioDeVenta,
                "una línea con el material, la cantidad y el precio vendidos", $"tiene {lineas.Count} línea(s)");
            _informe.Comprobar(_venta.TotalSinIva(_contexto) == Math.Round(_config.CantidadAVender * _config.PrecioDeVenta, 2),
                $"total sin iva {_config.CantidadAVender * _config.PrecioDeVenta:N2}", $"es {_venta.TotalSinIva(_contexto):N2}");
        }

        //------------------------------------------------------------------------------------------------------------ salida del almacén
        private void ValidarLaSalidaDelAlmacen()
        {
            _informe.Paso("Validar la salida del almacén y su preasiento");
            var salidas = _venta.SalidasDelAlmacen(_contexto);
            if (!_informe.Comprobar(salidas.Count == 1, "una salida del almacén por la línea vendida", $"hay {salidas.Count}"))
                return;

            _salida = salidas[0];
            _informe.Comprobar(_salida.Cantidad == _config.CantidadAVender, $"salen {_config.CantidadAVender}", $"salen {_salida.Cantidad}");
            _informe.Comprobar(_salida.RealizadoEl.Date == DateTime.Today, "con fecha de hoy");
            _informe.Comprobar(Stock() == _stockAntesDeVender - _config.CantidadAVender, $"el stock baja a {_stockAntesDeVender - _config.CantidadAVender:0.####}", $"es {Stock():0.####}");

            if (_conVariacion)
            {
                _informe.Comprobar(_salida.IdPreasiento is null, "con variación de existencias la salida no se preasienta");
                return;
            }

            if (!_informe.Comprobar(_salida.IdPreasiento is not null, "sin variación de existencias la salida está preasentada"))
                return;

            // la existencia sale por su precio medio: la variación al debe y las existencias al haber
            var importe = Math.Round(_salida.Cantidad * _salida.Precio, 2);
            var preasiento = _contexto.SeleccionarPorId<PreasientoDtm>((int)_salida.IdPreasiento, usarLaCache: false);
            _informe.Comprobar(preasiento.EstaEnLaEtapa(enumEtapasDePreasiento.SPR_Etapa_Pendiente), $"preasiento {preasiento.Referencia} pendiente",
                $"está en '{preasiento.Estado(_contexto).Nombre}'");
            var apuntes = _contexto.Set<ApunteDeUnPreasientoDtm>().Where(a => a.IdElemento == preasiento.Id).ToList();
            if (!_informe.Comprobar(apuntes.Count == 2, $"preasiento {preasiento.Referencia} con 2 apuntes", $"tiene {apuntes.Count}"))
                return;

            var variacion = _almacen.CuentaDeVariacion(_contexto).Codigo;
            var existencias = _almacen.CuentaDeExistencias(_contexto).Codigo;
            var debe = apuntes.First(a => a.Posicion == enumPosicionContable.Debe);
            var haber = apuntes.First(a => a.Posicion == enumPosicionContable.Haber);
            _informe.Comprobar(debe.Cuenta == variacion && debe.Importe == importe, $"   debe  {variacion} {importe:N2}", $"es {debe.Cuenta} {debe.Importe:N2}");
            _informe.Comprobar(haber.Cuenta == existencias && haber.Importe == importe, $"   haber {existencias} {importe:N2}", $"es {haber.Cuenta} {haber.Importe:N2}");
        }

        //------------------------------------------------------------------------------------------------------------ prefactura
        private void ValidarLaPrefactura()
        {
            _informe.Paso("Validar la prefactura");
            if (_venta.IdFacturaEmt is null)
                return;

            _prefactura = _contexto.SeleccionarPorId<FacturaEmtDtm>((int)_venta.IdFacturaEmt, usarLaCache: false);
            _informe.Dato($"prefactura '{_prefactura.Referencia}' (id {_prefactura.Id})");
            _informe.Comprobar(_prefactura.EstaEnLaEtapa(enumEtapasDeFacturasEmt.FAE_Etapa_Prefactura), $"'{_prefactura.Referencia}' es una prefactura",
                $"está en '{_prefactura.Estado(_contexto).Nombre}'");
            _informe.Comprobar(_prefactura.Numero is null, "sin número de factura");
            _informe.Comprobar(_prefactura.IdCliente == _config.IdCliente, $"del cliente {_config.IdCliente}", $"es del {_prefactura.IdCliente}");

            var lineas = _contexto.Set<LineaDeUnaFaeDtm>().Where(l => l.IdElemento == _prefactura.Id).ToList();
            if (!_informe.Comprobar(lineas.Count == 1, "una línea en la prefactura", $"tiene {lineas.Count}"))
                return;
            var linea = lineas[0];
            _informe.Comprobar(linea.TipoDeLinea == enumTipoDeLinea.Unitario && linea.IdUnitario == _unitario.Id, $"la línea es del unitario '{_unitario.Referencia}'", $"es {linea.TipoDeLinea} del unitario {linea.IdUnitario}");
            _informe.Comprobar(linea.IdLineaDeVenta is not null, "la línea apunta a la línea vendida");
            _informe.Comprobar(linea.Concepto?.Contains(_unitario.Referencia) == true, $"su concepto lleva la referencia '{_unitario.Referencia}'", $"es '{linea.Concepto}'");
            _informe.Comprobar(linea.Cantidad == _config.CantidadAVender && linea.Precio == _config.PrecioDeVenta,
                $"{_config.CantidadAVender} x {_config.PrecioDeVenta:N2}", $"es {linea.Cantidad} x {linea.Precio:N2}");
        }

        //------------------------------------------------------------------------------------------------------------ cancelar la prefactura
        // Se anula como desde la ficha de la factura (transición de usuario 'FAE: Anular', que pide el detalle del motivo)
        private void CancelarLaPrefactura()
        {
            _informe.Paso("Anular la prefactura");
            if (_prefactura is null)
                return;

            _prefactura = _prefactura.TransitarALaEtapa(_contexto, enumEtapasDeFacturasEmt.FAE_Etapa_Anulada.EstadosDeLaEtapa(),
                new Dictionary<string, object> { { ltrParametrosEp.detalleAsunto, "Prueba de anulación de la prefactura de una venta de material" } }, delSistema: false);
            _prefactura = _contexto.SeleccionarPorId<FacturaEmtDtm>(_prefactura.Id, usarLaCache: false);
            _informe.Comprobar(_prefactura.EstaEnLaEtapa(enumEtapasDeFacturasEmt.FAE_Etapa_Anulada), $"la prefactura '{_prefactura.Referencia}' está anulada",
                $"está en '{_prefactura.Estado(_contexto).Nombre}'");
            _informe.Comprobar(_prefactura.Numero is null, "sigue sin número de factura");

            _informe.Paso("Validar que el documento de venta vuelve a servido");
            _venta = Releer(_venta);
            _informe.Comprobar(_venta.EstaEnLaEtapa(enumEtapasDeVenta.DSA_Etapa_Servicio), "el documento de venta está servido", $"está en '{_venta.Estado(_contexto).Nombre}'");
            _informe.Comprobar(_venta.IdFacturaEmt is null, "ya no apunta a la prefactura");
            _informe.Comprobar(_venta.SalidasDelAlmacen(_contexto).Count == 1, "mantiene su salida del almacén");

            // la transición de cancelar la prefactura queda anotada con el enlace a la prefactura
            var asunto = $"Al cancelar prefactura {_prefactura.Referencia}";
            var observacion = enumNegocio.Venta.Observaciones(_contexto).FirstOrDefault(o => o.IdElemento == _venta.Id && o.Nombre == asunto);
            if (!_informe.Comprobar(observacion is not null, $"observación '{asunto}'"))
                return;
            _informe.Comprobar(observacion.Descripcion?.Contains($"id={_prefactura.Id}") == true, "su cuerpo es el enlace a la prefactura", $"es '{observacion.Descripcion}'");
            _informe.Comprobar(enumNegocio.Venta.Hitos(_contexto).Any(h => h.IdElemento == _venta.Id && h.IdObservacion == observacion.Id),
                "la observación es la de la transición de cancelar la prefactura");
        }

        //------------------------------------------------------------------------------------------------------------ devolver a solicitado
        // Transición del sistema: quita la salida del almacén (y con ella su preasiento) y deja de estar servido
        private void DevolverASolicitado()
        {
            _informe.Paso("Devolver el documento de venta a solicitado");
            _venta = _venta.DevolverASolicitado(_contexto);
            _venta = Releer(_venta);
            _informe.Comprobar(_venta.EstaEnLaEtapa(enumEtapasDeVenta.DSA_Etapa_Solicitud), "el documento de venta está solicitado", $"está en '{_venta.Estado(_contexto).Nombre}'");
            _informe.Comprobar(_venta.ServidoEl is null, "ya no está servido");
            _informe.Comprobar(_venta.SalidasDelAlmacen(_contexto).Count == 0, "no tiene salidas del almacén");
            _informe.Comprobar(Stock() == _stockAntesDeVender, $"el stock vuelve a {_stockAntesDeVender:0.####}", $"es {Stock():0.####}");

            if (_salida?.IdPreasiento is int idPreasiento)
            {
                var preasiento = _contexto.SeleccionarPorId<PreasientoDtm>(idPreasiento, usarLaCache: false);
                _informe.Comprobar(preasiento.Estado(_contexto).Cancelado, $"el preasiento {preasiento.Referencia} de la salida está cancelado",
                    $"está en '{preasiento.Estado(_contexto).Nombre}'");
            }
        }

        //------------------------------------------------------------------------------------------------------------ apoyo
        // stock vigente del material vendido en el almacén
        private decimal Stock() =>
            ExtensorDeAlmacenes.UltimoMovimientoHasta(_contexto, _almacen.Id, _unitario.Id, DateTime.MaxValue)?.Stock ?? 0;

        private VentaDtm Releer(VentaDtm venta) => _contexto.SeleccionarPorId<VentaDtm>(venta.Id, usarLaCache: false);
    }
}
