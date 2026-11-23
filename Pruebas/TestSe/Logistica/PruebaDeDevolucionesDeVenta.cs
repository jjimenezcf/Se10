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
using ServicioDeDatos.Ventas;
using Utilidades;
using static ServicioDeDatos.Elemento.Enumerados;

namespace TestLogistica
{
    // Apoyo común a las pruebas de devoluciones de venta: devolver una cantidad de una línea vendida (como la acción Devolver de las
    // líneas del documento de venta, ExtensorDeDevolucionesDeVenta.DevolverLineaDeVenta), aceptar la devolución y validar la entrada en
    // el almacén, su preasiento y el documento anexado
    internal class DevolucionesDePrueba
    {
        private readonly ContextoSe _contexto;
        private readonly Informe _informe;

        public DevolucionesDePrueba(ContextoSe contexto, Informe informe)
        {
            _contexto = contexto;
            _informe = informe;
        }

        // El tipo del documento de venta ha de indicar el tipo de devolución con el que se crean sus devoluciones: si no lo tiene se le asigna
        // el primero activo que permite crear (se deshace con el rollback)
        public void DefinirElTipoDeDevolucion(VentaDtm venta)
        {
            var tipoDeVenta = _contexto.SeleccionarPorId<TipoDeVentaDtm>(venta.IdTipo, usarLaCache: false);
            if (tipoDeVenta.IdTipoDevolucion is null)
            {
                var tipoDeDevolucion = _contexto.Set<TipoDeDevolucionDeVentaDtm>().Where(t => t.Activo && t.PermiteCrear).OrderBy(t => t.Id).FirstOrDefault();
                if (!_informe.Comprobar(tipoDeDevolucion is not null, "hay un tipo de devolución de venta activo que permite crear"))
                    throw new Exception("No se puede devolver sin tipo de devolución de venta");
                _contexto.Database.ExecuteSqlInterpolated($"UPDATE LOGISTICA.VENTA_TIPO SET ID_TIPO_DEVOLUCION = {tipoDeDevolucion.Id} WHERE ID = {tipoDeVenta.Id}");
                ServicioDeCaches.EliminarTodas();
                _informe.Dato($"al tipo '{tipoDeVenta.Nombre}' se le asigna el tipo de devolución '{tipoDeDevolucion.Nombre}'");
            }
            tipoDeVenta = _contexto.SeleccionarPorId<TipoDeVentaDtm>(venta.IdTipo, usarLaCache: false);
            _informe.Comprobar(tipoDeVenta.IdTipoDevolucion is not null, $"el tipo '{tipoDeVenta.Nombre}' tiene tipo de devolución");
        }

        public DevolucionDeVentaDtm DevolverYAceptar(LineaDeUnaVentaDtm lineaDeVenta, decimal cantidad)
        {
            var venta = _contexto.SeleccionarPorId<VentaDtm>(lineaDeVenta.IdElemento, usarLaCache: false);
            var almacen = venta.Almacen(_contexto);
            var stockAntes = Stock(venta.IdAlmacen, lineaDeVenta.IdUnitario);
            DefinirElTipoDeDevolucion(venta);

            var devolucion = ExtensorDeDevolucionesDeVenta.DevolverLineaDeVenta(_contexto, lineaDeVenta.Id, cantidad, idDevolucion: 0);
            devolucion = Releer(devolucion);
            _informe.Dato($"devolución '{devolucion.Referencia}' (id {devolucion.Id})");
            _informe.Comprobar(devolucion.EstaEnLaEtapa(enumEtapasDeDevolucionDeVenta.DDV_Etapa_Tramitacion), $"la devolución '{devolucion.Referencia}' se crea en tramitación",
                $"está en '{devolucion.Estado(_contexto).Nombre}'");
            _informe.Comprobar(devolucion.IdCliente == venta.IdCliente, "del cliente del documento de venta");

            var lineas = devolucion.Lineas(_contexto);
            _informe.Comprobar(lineas.Count == 1 && lineas[0].IdLineaDeVenta == lineaDeVenta.Id && lineas[0].Cantidad == cantidad && lineas[0].Precio == lineaDeVenta.Precio,
                $"una línea que devuelve {cantidad:0.####} al precio vendido {lineaDeVenta.Precio:N2}", $"tiene {lineas.Count} línea(s)");

            devolucion = devolucion.TransitarALaEtapa(_contexto, enumEtapasDeDevolucionDeVenta.DDV_Etapa_Aceptada.EstadosDeLaEtapa(), delSistema: false);
            devolucion = Releer(devolucion);
            _informe.Comprobar(devolucion.EstaEnLaEtapa(enumEtapasDeDevolucionDeVenta.DDV_Etapa_Aceptada), $"la devolución '{devolucion.Referencia}' está aceptada",
                $"está en '{devolucion.Estado(_contexto).Nombre}'");
            _informe.Comprobar(devolucion.DevueltaEl?.Date == DateTime.Today, "devuelta hoy");

            // entrada en el almacén valorada al precio de la salida
            var entrada = _contexto.Set<MovimientoDeAlmacenDtm>().FirstOrDefault(m => m.IdLineaDeDevolucionDeVenta == lineas[0].Id);
            if (_informe.Comprobar(entrada is not null, "la línea devuelta tiene su entrada en el almacén"))
            {
                var salida = _contexto.Set<MovimientoDeAlmacenDtm>().First(m => m.IdLineaDeVenta == lineaDeVenta.Id);
                _informe.Comprobar(entrada.Cantidad == cantidad && entrada.Precio == salida.Precio, $"entran {cantidad:0.####} a {salida.Precio:N4}",
                    $"entran {entrada.Cantidad:0.####} a {entrada.Precio:N4}");
                _informe.Comprobar(Stock(venta.IdAlmacen, lineaDeVenta.IdUnitario) == stockAntes + cantidad, $"el stock sube a {stockAntes + cantidad:0.####}",
                    $"es {Stock(venta.IdAlmacen, lineaDeVenta.IdUnitario):0.####}");
                ValidarPreasiento(almacen, entrada);
            }

            // el documento de la devolución se anexa a sus archivos
            var nombre = $"Devolucion-{devolucion.Referencia}".NormalizarFichero();
            var anexos = enumNegocio.DevolucionVenta.Archivos(_contexto, devolucion.Id);
            _informe.Comprobar(anexos.Any(a => a.Nombre.StartsWith(nombre, StringComparison.CurrentCultureIgnoreCase)), $"el pdf '{nombre}.pdf' está anexado a la devolución",
                $"tiene {anexos.Count} archivo(s): {string.Join(", ", anexos.Select(a => a.Nombre))}");

            return devolucion;
        }

        // sin variación de existencias la entrada se preasienta al revés que la salida: existencias al debe y variación al haber
        private void ValidarPreasiento(AlmacenDtm almacen, MovimientoDeAlmacenDtm entrada)
        {
            if (almacen.UsaVariacionDeExistencias)
            {
                _informe.Comprobar(entrada.IdPreasiento is null, "con variación de existencias la entrada no se preasienta");
                return;
            }
            if (!_informe.Comprobar(entrada.IdPreasiento is not null, "sin variación de existencias la entrada está preasentada"))
                return;

            var importe = Math.Round(entrada.Cantidad * entrada.Precio, 2);
            var apuntes = _contexto.Set<ApunteDeUnPreasientoDtm>().Where(a => a.IdElemento == (int)entrada.IdPreasiento).ToList();
            if (!_informe.Comprobar(apuntes.Count == 2, "preasiento con 2 apuntes", $"tiene {apuntes.Count}"))
                return;
            var existencias = almacen.CuentaDeExistencias(_contexto).Codigo;
            var variacion = almacen.CuentaDeVariacion(_contexto).Codigo;
            var debe = apuntes.First(a => a.Posicion == enumPosicionContable.Debe);
            var haber = apuntes.First(a => a.Posicion == enumPosicionContable.Haber);
            _informe.Comprobar(debe.Cuenta == existencias && debe.Importe == importe, $"   debe  {existencias} {importe:N2}", $"es {debe.Cuenta} {debe.Importe:N2}");
            _informe.Comprobar(haber.Cuenta == variacion && haber.Importe == importe, $"   haber {variacion} {importe:N2}", $"es {haber.Cuenta} {haber.Importe:N2}");
        }

        public List<LineaDeUnaFaeDtm> LineasDe(FacturaEmtDtm factura) =>
            _contexto.Set<LineaDeUnaFaeDtm>().Where(l => l.IdElemento == factura.Id).OrderBy(l => l.Orden).ToList();

        // la rectificativa que rectifica a la factura (sin la caché de RectificadaPor)
        public FacturaEmtDtm RectificativaDe(FacturaEmtDtm factura)
        {
            var datos = _contexto.Set<RectificativaEmtDtm>().Where(r => r.IdRectificada == factura.Id).OrderByDescending(r => r.Id).FirstOrDefault();
            return datos is null ? null : _contexto.SeleccionarPorId<FacturaEmtDtm>(datos.IdElemento, usarLaCache: false);
        }

        public decimal Stock(int idAlmacen, int idUnitario) =>
            ExtensorDeAlmacenes.UltimoMovimientoHasta(_contexto, idAlmacen, idUnitario, DateTime.MaxValue)?.Stock ?? 0;

        public VentaDtm Releer(VentaDtm venta) => _contexto.SeleccionarPorId<VentaDtm>(venta.Id, usarLaCache: false);
        public FacturaEmtDtm Releer(FacturaEmtDtm factura) => _contexto.SeleccionarPorId<FacturaEmtDtm>(factura.Id, usarLaCache: false);
        private DevolucionDeVentaDtm Releer(DevolucionDeVentaDtm devolucion) => _contexto.SeleccionarPorId<DevolucionDeVentaDtm>(devolucion.Id, usarLaCache: false);
    }

    // Venta de material prefacturada (sin emitir) y devuelta en dos veces:
    //  - el usuario no puede cambiar la cantidad de la línea de la prefactura que factura la línea vendida
    //  - primero una parte y luego el resto: entra en el almacén y a la prefactura se le quita lo devuelto (la línea baja y, al devolver
    //    el resto, se borra); el documento de venta sigue facturado con ella y se puede emitir (su base es lo vendido menos lo devuelto)
    public class PruebaDeDevolucionesDeVenta
    {
        private readonly ConfigDeLogistica _config;
        private readonly bool _conVariacion;

        public string Nombre => $"Devolución de venta {(_conVariacion ? "con" : "sin")} variación de existencias: venta prefacturada, devolver una parte y luego el resto";

        public PruebaDeDevolucionesDeVenta(ConfigDeLogistica config, bool conVariacion)
        {
            _config = config;
            _conVariacion = conVariacion;
        }

        public void Ejecutar(ContextoSe contexto, Informe informe)
        {
            var compras = new PruebaDeCompras(_config, _conVariacion);
            compras.Ejecutar(contexto, informe);
            var almacen = compras.Almacen;
            var apoyo = new DevolucionesDePrueba(contexto, informe);

            PruebaDeVentas.DefinirElTipoDePrefactura(contexto, informe);

            var unitario = _config.Lineas[0].IdUnitario;
            var vendido = _config.CantidadAVender;
            informe.Paso($"Vender {vendido:0.####} a {_config.PrecioDeVenta:N2} al cliente {_config.IdCliente}, servido y prefacturado");
            var stockAntesDeVender = apoyo.Stock(almacen.Id, unitario);
            var venta = ExtensorDeVentas.CrearVentaDeMaterial(contexto, almacen.Id, unitario, _config.IdCliente, DateTime.Today, vendido, _config.PrecioDeVenta, servido: true, facturar: true);
            venta = apoyo.Releer(venta);
            if (!informe.Comprobar(venta.EstaEnLaEtapa(enumEtapasDeVenta.DSA_Etapa_Facturacion) && venta.IdFacturaEmt is not null, "el documento de venta está facturado con su prefactura"))
                return;
            var prefactura = contexto.SeleccionarPorId<FacturaEmtDtm>((int)venta.IdFacturaEmt, usarLaCache: false);
            var linea = contexto.Set<LineaDeUnaVentaDtm>().First(l => l.IdElemento == venta.Id);

            var parte = Math.Floor(vendido / 2) > 0 ? Math.Floor(vendido / 2) : vendido / 2;
            var resto = vendido - parte;

            informe.Paso("No se puede cambiar la cantidad de la línea de la prefactura: se devuelve desde el documento de venta");
            ValidarQueNoSePuedeCambiarLaLinea(contexto, informe, apoyo, prefactura, parte);

            informe.Paso($"Devolver {parte:0.####} de {vendido:0.####} y aceptar la devolución");
            apoyo.DevolverYAceptar(linea, parte);
            prefactura = apoyo.Releer(prefactura);
            venta = apoyo.Releer(venta);
            ValidarPrefacturaAjustada(contexto, informe, apoyo, prefactura, resto);
            informe.Comprobar(contexto.Set<RectificativaEmtDtm>().All(r => r.IdRectificada != prefactura.Id), "no se crea rectificativa de una prefactura");
            informe.Comprobar(venta.EstaEnLaEtapa(enumEtapasDeVenta.DSA_Etapa_Facturacion) && venta.IdFacturaEmt == prefactura.Id, "el documento de venta sigue facturado con la misma prefactura");
            informe.Comprobar(linea.CantidadPendienteDeDevolver(contexto) == resto, $"quedan {resto:0.####} por devolver", $"quedan {linea.CantidadPendienteDeDevolver(contexto):0.####}");
            ValidarQueSePuedeEmitir(informe, () => prefactura.ValidarImporteDeLasVentas(contexto), prefactura);

            informe.Paso($"Devolver el resto ({resto:0.####}) y aceptar la devolución");
            apoyo.DevolverYAceptar(linea, resto);
            prefactura = apoyo.Releer(prefactura);
            venta = apoyo.Releer(venta);
            ValidarPrefacturaAjustada(contexto, informe, apoyo, prefactura, 0);
            informe.Comprobar(venta.EstaEnLaEtapa(enumEtapasDeVenta.DSA_Etapa_Facturacion) && venta.IdFacturaEmt == prefactura.Id, "el documento de venta sigue facturado con la misma prefactura");
            informe.Comprobar(linea.CantidadPendienteDeDevolver(contexto) == 0, "no queda nada por devolver");
            informe.Comprobar(apoyo.Stock(almacen.Id, unitario) == stockAntesDeVender, $"el stock vuelve a {stockAntesDeVender:0.####}", $"es {apoyo.Stock(almacen.Id, unitario):0.####}");
            ValidarQueSePuedeEmitir(informe, () => prefactura.ValidarImporteDeLasVentas(contexto), prefactura);

            informe.Paso("No se puede devolver más de lo vendido");
            try
            {
                ExtensorDeDevolucionesDeVenta.DevolverLineaDeVenta(contexto, linea.Id, 1, idDevolucion: 0);
                informe.Comprobar(false, "devolver de un documento de venta ya devuelto da error");
            }
            catch (Exception e)
            {
                informe.Dato($"error esperado: {e.Message}");
            }
        }

        // la línea de la prefactura que factura la línea vendida no la cambia el usuario (ver GestorDeLineasDeUnaFae.AntesDePersistir)
        private static void ValidarQueNoSePuedeCambiarLaLinea(ContextoSe contexto, Informe informe, DevolucionesDePrueba apoyo, FacturaEmtDtm prefactura, decimal menos)
        {
            var linea = apoyo.LineasDe(prefactura).First(l => l.IdLineaDeVenta is not null);
            try
            {
                linea.Cantidad = linea.Cantidad - menos;
                linea.Modificar(contexto);
                informe.Comprobar(false, "cambiar la cantidad de la línea de la prefactura da error", "no da error");
            }
            catch (Exception e)
            {
                informe.Comprobar(e.Message.Contains("se devuelve desde"), "cambiar la cantidad de la línea de la prefactura da error", e.Message);
            }
        }

        // al aceptar la devolución a la prefactura se le quita lo devuelto: su línea se queda con lo que no se devuelve, o se borra
        internal static void ValidarPrefacturaAjustada(ContextoSe contexto, Informe informe, DevolucionesDePrueba apoyo, FacturaEmtDtm prefactura, decimal queda, int idLineaDeVenta = 0)
        {
            informe.Comprobar(prefactura.EstaEnLaEtapa(enumEtapasDeFacturasEmt.FAE_Etapa_Prefactura), $"'{prefactura.Referencia}' sigue siendo una prefactura", $"está en '{prefactura.Estado(contexto).Nombre}'");
            var lineas = apoyo.LineasDe(prefactura).Where(l => l.IdLineaDeVenta is not null && (idLineaDeVenta == 0 || l.IdLineaDeVenta == idLineaDeVenta)).ToList();
            if (queda == 0)
                informe.Comprobar(lineas.Count == 0, "se devuelve todo: la línea se borra de la prefactura", $"tiene {string.Join(", ", lineas.Select(l => $"{l.Cantidad:0.####}"))}");
            else
                informe.Comprobar(lineas.Count == 1 && lineas[0].Cantidad == queda, $"su línea se queda con {queda:0.####}", $"tiene {string.Join(", ", lineas.Select(l => $"{l.Cantidad:0.####}"))}");
        }

        // la validación previa a emitir: la base de la prefactura es lo vendido menos lo devuelto
        internal static void ValidarQueSePuedeEmitir(Informe informe, Action validar, FacturaEmtDtm prefactura)
        {
            try
            {
                validar();
                informe.Comprobar(true, $"'{prefactura.Referencia}' se puede emitir: su base es lo vendido menos lo devuelto");
            }
            catch (Exception e)
            {
                informe.Comprobar(false, $"'{prefactura.Referencia}' se puede emitir: su base es lo vendido menos lo devuelto", e.Message);
            }
        }
    }

    // Documento de venta de la BD cuya factura está emitida (sin rectificar), devuelto en dos veces:
    //  - primero una parte de una línea: entra en el almacén, la factura se rectifica por completo (prefactura rectificativa total, con
    //    sus líneas en negativo) y se crea una prefactura de sustitución sin lo devuelto, a la que pasa a apuntar el documento de venta
    //  - luego el resto de la línea: como la factura del documento de venta ya es la prefactura de sustitución, no se rectifica: se le
    //    quita lo devuelto y se puede emitir
    // Nunca se emite nada: las rectificativas y las sustituciones se quedan en prefactura y la transacción se deshace
    public class PruebaDeDevolucionDeFacturaEmitida
    {
        public static string Nombre => "Devolución de venta de un documento de venta de la BD con factura emitida: devolver una parte y luego el resto";

        public void Ejecutar(ContextoSe contexto, Informe informe)
        {
            var apoyo = new DevolucionesDePrueba(contexto, informe);

            informe.Paso("Buscar en la BD un documento de venta facturado con su factura emitida y sin rectificar");
            var (venta, factura, linea) = Buscar(contexto);
            if (!informe.Comprobar(venta is not null, "hay un documento de venta facturado, con salida del almacén y factura emitida sin rectificar"))
                return;
            Console.WriteLine($"   documento de venta '{venta.Referencia}' (id {venta.Id}), factura '{factura.NumeroDeFactura}' (id {factura.Id}), línea {linea.Orden}: {linea.Cantidad:0.####} a {linea.Precio:N2}");

            var lineasDeLaFactura = apoyo.LineasDe(factura);
            var pendiente = linea.CantidadPendienteDeDevolver(contexto);
            var parte = Math.Floor(pendiente / 2) > 0 ? Math.Floor(pendiente / 2) : pendiente / 2;
            var resto = pendiente - parte;
            var facturada = lineasDeLaFactura.FirstOrDefault(l => l.IdLineaDeVenta == linea.Id) ?? lineasDeLaFactura.First(l => l.Orden == linea.Orden);

            informe.Paso($"Devolver {parte:0.####} de {pendiente:0.####} y aceptar la devolución");
            apoyo.DevolverYAceptar(linea, parte);

            var rectificativa = apoyo.RectificativaDe(factura);
            if (informe.Comprobar(rectificativa is not null, $"la factura '{factura.NumeroDeFactura}' está rectificada"))
            {
                Console.WriteLine($"   rectificativa '{rectificativa.Referencia}' (id {rectificativa.Id})");
                informe.Comprobar(rectificativa.EstaEnLaEtapa(enumEtapasDeFacturasEmt.FAE_Etapa_Prefactura), "la rectificativa es una prefactura", $"está en '{rectificativa.Estado(contexto).Nombre}'");
                informe.Comprobar(rectificativa.ClaseRectificativa == enumClaseDeRectificativa.OR, "la rectificativa es total (por sustitución)", $"es {rectificativa.ClaseRectificativa}");
                var negativas = apoyo.LineasDe(rectificativa);
                informe.Comprobar(negativas.Count == lineasDeLaFactura.Count
                    && negativas.Zip(lineasDeLaFactura).All(p => p.First.Cantidad == -p.Second.Cantidad && p.First.Precio == p.Second.Precio),
                    "con todas las líneas de la factura en negativo", $"tiene {negativas.Count} línea(s), la factura {lineasDeLaFactura.Count}");
            }

            venta = apoyo.Releer(venta);
            var sustituta = venta.IdFacturaEmt is null || venta.IdFacturaEmt == factura.Id ? null : contexto.SeleccionarPorId<FacturaEmtDtm>((int)venta.IdFacturaEmt, usarLaCache: false);
            if (!informe.Comprobar(sustituta is not null, "el documento de venta apunta a la prefactura de sustitución", $"apunta a {venta.IdFacturaEmt}"))
                return;
            Console.WriteLine($"   prefactura de sustitución '{sustituta.Referencia}' (id {sustituta.Id})");
            informe.Comprobar(sustituta.EstaEnLaEtapa(enumEtapasDeFacturasEmt.FAE_Etapa_Prefactura) && !sustituta.EsRectificativa, "es una prefactura ordinaria");
            informe.Comprobar(venta.EstaEnLaEtapa(enumEtapasDeVenta.DSA_Etapa_Facturacion), "el documento de venta sigue facturado");
            var lineasSustituta = apoyo.LineasDe(sustituta);
            var lineaSustituta = lineasSustituta.FirstOrDefault(l => l.Orden == linea.Orden);
            informe.Comprobar(lineaSustituta?.Cantidad == facturada.Cantidad - parte, $"su línea {linea.Orden} tiene lo facturado menos lo devuelto ({facturada.Cantidad - parte:0.####})",
                $"tiene {lineaSustituta?.Cantidad:0.####}");
            informe.Comprobar(lineasSustituta.Count == lineasDeLaFactura.Count(l => l.Cantidad is null || l.Cantidad != 0), "con el resto de líneas de la factura",
                $"tiene {lineasSustituta.Count}, la factura {lineasDeLaFactura.Count}");

            if (resto <= 0)
                return;

            informe.Paso($"Devolver el resto de la línea ({resto:0.####}) y aceptar la devolución");
            apoyo.DevolverYAceptar(linea, resto);
            sustituta = apoyo.Releer(sustituta);
            venta = apoyo.Releer(venta);
            informe.Comprobar(contexto.Set<RectificativaEmtDtm>().Count(r => r.IdRectificada == factura.Id) == 1, "la factura emitida sólo se rectifica una vez");
            informe.Comprobar(contexto.Set<RectificativaEmtDtm>().All(r => r.IdRectificada != sustituta.Id), "la prefactura de sustitución no se rectifica");

            // la prefactura de sustitución es una prefactura: se le quita el resto devuelto (la línea baja o, si no queda nada, se borra)
            PruebaDeDevolucionesDeVenta.ValidarPrefacturaAjustada(contexto, informe, apoyo, sustituta, (decimal)facturada.Cantidad - parte - resto, linea.Id);
            informe.Comprobar(venta.EstaEnLaEtapa(enumEtapasDeVenta.DSA_Etapa_Facturacion) && venta.IdFacturaEmt == sustituta.Id, "el documento de venta sigue apuntando a la prefactura de sustitución");
            PruebaDeDevolucionesDeVenta.ValidarQueSePuedeEmitir(informe, () => sustituta.ValidarImporteDeLasVentas(contexto), sustituta);
            informe.Comprobar(linea.CantidadPendienteDeDevolver(contexto) == 0, "no queda nada por devolver de la línea");
        }

        // el último documento de venta facturado cuya factura está emitida, no es rectificativa ni está rectificada, y con una línea con
        // salida del almacén y algo por devolver
        private static (VentaDtm, FacturaEmtDtm, LineaDeUnaVentaDtm) Buscar(ContextoSe contexto)
        {
            var estados = enumEtapasDeVenta.DSA_Etapa_Facturacion.Lista();
            var ventas = contexto.Set<VentaDtm>().Where(v => v.IdFacturaEmt != null && estados.Contains(v.IdEstado)).OrderByDescending(v => v.Id).ToList();
            foreach (var venta in ventas)
            {
                var factura = contexto.SeleccionarPorId<FacturaEmtDtm>((int)venta.IdFacturaEmt, usarLaCache: false);
                if (factura.EstaEnLaEtapa(enumEtapasDeFacturasEmt.FAE_Etapa_Prefactura) || factura.FacturadaEl is null || factura.NumeroDeFactura.IsNullOrEmpty())
                    continue;
                if (factura.EsRectificativa || contexto.Set<RectificativaEmtDtm>().Any(r => r.IdRectificada == factura.Id))
                    continue;

                var linea = contexto.Set<LineaDeUnaVentaDtm>().Where(l => l.IdElemento == venta.Id).OrderBy(l => l.Orden).ToList()
                    .FirstOrDefault(l => l.CantidadPendienteDeDevolver(contexto) > 0 && contexto.Set<MovimientoDeAlmacenDtm>().Any(m => m.IdLineaDeVenta == l.Id));
                if (linea is not null)
                    return (venta, factura, linea);
            }
            return (null, null, null);
        }
    }
}
