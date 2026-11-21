using System;
using System.Collections.Generic;
using System.Linq;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Logistica;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Logistica;
using ServicioDeDatos.MaestrosTecnico;
using SistemaDeElementos.Inicializador.Datos;
using Utilidades;
using static ServicioDeDatos.Elemento.Enumerados;

namespace TestLogistica
{
    // Compras de un almacén nuevo con existencias 310000 (materias primas) y la variación 611000 deducida por el almacén
    // (ver ExtensorDePreasientos.PreasentarCompraDeAlmacen):
    //  - con variación de existencias, el almacén tiene compras 601000 y devoluciones 608100: cada entrega se preasienta al debe de la de
    //    compras y al haber de la de albaranes del proveedor (4009 + su código contable); cada devolución al revés, con la de devoluciones
    //  - sin variación de existencias, el almacén no tiene cuentas de compras ni de devoluciones: la entrega va al debe de la de existencias
    //    y la devolución al haber de la misma
    // Al cancelar una entrega su preasiento se cancela
    public class PruebaDeCompras
    {
        private readonly bool _conVariacion;
        public string Nombre => $"Compras {(_conVariacion ? "con" : "sin")} variación de existencias contra un almacén";

        private readonly ConfigDeLogistica _config;
        private ContextoSe _contexto;
        private Informe _informe;

        // fecha del pedido, de las entregas y de la devolución: hoy o, para cerrar después hasta hoy, ayer (ver PruebaDeCierreConVariacion)
        private readonly DateTime _fechaDeLaCompra;

        private AlmacenDtm _almacen;
        public AlmacenDtm Almacen => _almacen;
        private string _cuentaDeLasEntregas;
        private string _cuentaDeLasDevoluciones;
        private string _cuentaDeAlbaranes;
        private PedidoDtm _pedido;
        private readonly List<LineaDeUnPedidoDtm> _lineas = new();

        public PruebaDeCompras(ConfigDeLogistica config, bool conVariacion, DateTime? fechaDeLaCompra = null)
        {
            _config = config;
            _conVariacion = conVariacion;
            _fechaDeLaCompra = (fechaDeLaCompra ?? DateTime.Today).Date;
        }

        // recepciones vigentes y devoluciones de la compra, para cuadrar lo que ha variado el inventario
        public decimal ImporteNetoDeLaCompra()
        {
            var neto = 0m;
            foreach (var linea in _lineas)
                foreach (var recepcion in Recepciones(linea))
                {
                    neto += Math.Round(recepcion.Importe, 2);
                    foreach (var devolucion in _contexto.Set<DevolucionDeEntregaDtm>().Where(d => d.IdLineaDeRecepcion == recepcion.Id).ToList())
                        neto -= Math.Round(devolucion.Importe(recepcion), 2);
                }
            return neto;
        }

        public void Ejecutar(ContextoSe contexto, Informe informe)
        {
            _contexto = contexto;
            _informe = informe;

            ActualizarMaestroDeCuentas();
            DefinirElAlmacen();
            CargarElInventarioInicial();
            CrearElPedido();
            MeterMateriales();
            Solicitar();

            var preasientosDeLasPrimerasEntregas = Entregar("Hacer las entregas");
            QuitarLasEntregas(preasientosDeLasPrimerasEntregas);
            Entregar("Volver a hacer las entregas");
            Devolver();
            Imprimir();
            ValidarLosPreasientosDeLaCompra();
        }

        // preasientos vigentes de la compra: los de las entradas de las entregas y los de las salidas de las devoluciones
        public List<int> PreasientosDeLaCompra()
        {
            var preasientos = new List<int>();
            foreach (var linea in _lineas)
                foreach (var recepcion in Recepciones(linea))
                {
                    if (EntradaDeCompra(recepcion)?.IdPreasiento is int entrada)
                        preasientos.Add(entrada);
                    foreach (var devolucion in _contexto.Set<DevolucionDeEntregaDtm>().Where(d => d.IdLineaDeRecepcion == recepcion.Id).ToList())
                        if (_contexto.Set<MovimientoDeAlmacenDtm>().FirstOrDefault(m => m.IdLineaDevolucion == devolucion.Id)?.IdPreasiento is int salida)
                            preasientos.Add(salida);
                }
            return preasientos;
        }

        //------------------------------------------------------------------------------------------------------------ 1. maestro de cuentas
        private void ActualizarMaestroDeCuentas()
        {
            _informe.Paso("Actualizar el maestro de cuentas");
            var antes = _contexto.Set<CuentaDtm>().Count();
            InzContable.InicializarCuentasDeAlmacenesYObras(_contexto);
            _informe.Dato($"cuentas creadas: {_contexto.Set<CuentaDtm>().Count() - antes}");

            foreach (var codigo in new[] { ltrCuenta.MateriasPrimasCodigo, ltrCuenta.ComprasDeMateriasPrimasCodigo, ltrCuenta.DevolucionesDeMateriasPrimasCodigo, ltrCuenta.VariacionDeMateriasPrimasCodigo })
                _informe.Comprobar(Cuenta(codigo) is not null, $"existe la cuenta {codigo}");
        }

        //------------------------------------------------------------------------------------------------------------ 2. almacén
        // Un almacén con movimientos no puede cambiar de método (ver ExtensorDeAlmacenes.PrepararInformacionContable): se abre uno nuevo, con
        // o sin variación de existencias, en el centro gestor del almacén de referencia (config.json)
        private void DefinirElAlmacen()
        {
            var con = _conVariacion ? "con" : "sin";
            _informe.Paso($"Crear un almacén {con} variación de existencias en el centro gestor del almacén {_config.IdAlmacen} (sociedad {_config.IdSociedad})");
            var referencia = _contexto.SeleccionarPorId<AlmacenDtm>(_config.IdAlmacen, usarLaCache: false);
            _informe.Comprobar(referencia.Sociedad(_contexto).Id == _config.IdSociedad, $"el almacén '{referencia.Referencia}' es de la sociedad {_config.IdSociedad}");

            var nuevo = new AlmacenDtm
            {
                IdCg = referencia.IdCg,
                IdTipo = referencia.IdTipo,
                IdEstado = 0,
                Nombre = $"Prueba {con} variación de existencias {DateTime.Now:dd/MM/yyyy HH:mm:ss}",
                UsaVariacionDeExistencias = _conVariacion,
                IdCuentaDeExistencias = Cuenta(ltrCuenta.MateriasPrimasCodigo).Id,
                IdCuentaDeVariacion = null,
                IdCuentaDeCompras = _conVariacion ? Cuenta(ltrCuenta.ComprasDeMateriasPrimasCodigo).Id : null,
                IdCuentaDeDevoluciones = _conVariacion ? Cuenta(ltrCuenta.DevolucionesDeMateriasPrimasCodigo).Id : null
            }.Insertar(_contexto);

            _almacen = _contexto.SeleccionarPorId<AlmacenDtm>(nuevo.Id, usarLaCache: false);
            _informe.Comprobar(_almacen.UsaVariacionDeExistencias == _conVariacion, $"{con} variación de existencias");
            _informe.Comprobar(_almacen.CuentaDeExistencias(_contexto).Codigo == ltrCuenta.MateriasPrimasCodigo, $"cuenta de existencias {ltrCuenta.MateriasPrimasCodigo}");
            _informe.Comprobar(_almacen.CuentaDeVariacion(_contexto).Codigo == ltrCuenta.VariacionDeMateriasPrimasCodigo, $"cuenta de variación deducida {ltrCuenta.VariacionDeMateriasPrimasCodigo}",
                $"es {_almacen.CuentaDeVariacion(_contexto).Codigo}");

            if (_conVariacion)
            {
                _cuentaDeLasEntregas = _almacen.CuentaDeCompras(_contexto).Codigo;
                _cuentaDeLasDevoluciones = _almacen.CuentaDeDevoluciones(_contexto).Codigo;
                _informe.Comprobar(_cuentaDeLasEntregas == ltrCuenta.ComprasDeMateriasPrimasCodigo, $"cuenta de compras {ltrCuenta.ComprasDeMateriasPrimasCodigo}");
                _informe.Comprobar(_cuentaDeLasDevoluciones == ltrCuenta.DevolucionesDeMateriasPrimasCodigo, $"cuenta de devoluciones {ltrCuenta.DevolucionesDeMateriasPrimasCodigo}");
            }
            else
            {
                // las entregas y las devoluciones van a la cuenta de existencias
                _cuentaDeLasEntregas = _cuentaDeLasDevoluciones = _almacen.CuentaDeExistencias(_contexto).Codigo;
                _informe.Comprobar(_almacen.IdCuentaDeCompras is null && _almacen.IdCuentaDeDevoluciones is null, "sin cuentas de compras ni de devoluciones");
            }
        }

        //------------------------------------------------------------------------------------------------------------ inventario inicial
        // Regularización de clase Inicial con fecha de ayer (o la de la compra si es anterior) y los materiales de la compra, aplicada como lo
        // hace el trabajo "Aplicar recuento" (TrabajosParaRegularizaciones.AplicarRecuento): abierta -> recontando (el almacén pasa a en
        // inventario), líneas, movimientos de stock inicial y a aplicada (el almacén vuelve a activo)
        private void CargarElInventarioInicial()
        {
            var ayer = new[] { DateTime.Today.AddDays(-1), _fechaDeLaCompra }.Min();
            _informe.Paso($"Cargar el inventario inicial con una regularización de clase Inicial del {ayer:dd/MM/yyyy}");

            var tipo = _contexto.Set<TipoDeRegularizacionDtm>().First(t => t.ClaseDeRegularizacion == enumRegularizacionAlm.Inicial);
            var regularizacion = new RegularizacionDtm
            {
                IdCg = _almacen.IdCg,
                IdTipo = tipo.Id,
                IdEstado = 0,
                Nombre = $"Inventario inicial de la prueba {DateTime.Now:dd/MM/yyyy HH:mm:ss}",
                IdAlmacen = _almacen.Id
            }.Insertar(_contexto);

            regularizacion = regularizacion.TransitarALaEtapa(_contexto, enumEtapasDeRegularizacion.RAL_Recontando.EstadosDeLaEtapa(), delSistema: false);

            var orden = 0;
            foreach (var lineaDePrueba in _config.Lineas)
            {
                orden += 10;
                new LineasDeUnaRegularizacionDtm
                {
                    IdElemento = regularizacion.Id,
                    Orden = orden,
                    Fecha = ayer,
                    IdUnitario = lineaDePrueba.IdUnitario,
                    Cantidad = _config.CantidadInicial,
                    Precio = lineaDePrueba.Precio
                }.Insertar(_contexto);
            }

            regularizacion = _contexto.SeleccionarPorId<RegularizacionDtm>(regularizacion.Id, usarLaCache: false);
            regularizacion.ValidarQueSePuedeAplicarElRecuento(_contexto);
            var movimientos = regularizacion.AplicarElRecuento(_contexto);
            regularizacion.TransitarALaEtapa(_contexto, enumEtapasDeRegularizacion.RAL_Cerrado.EstadosDeLaEtapa(),
                new Dictionary<string, object> { { ltrDeUnaRegularizacion.AplicandoElRecuento, true } }, delSistema: true);

            _informe.Comprobar(movimientos == _config.Lineas.Count, "un movimiento de stock inicial por material", $"se han generado {movimientos}");
            _almacen = _contexto.SeleccionarPorId<AlmacenDtm>(_almacen.Id, usarLaCache: false);
            _informe.Comprobar(_almacen.EstaEnLaEtapa(enumEtapasDeAlmacen.ALM_Etapa_Activo), "el almacén vuelve a estar activo");
        }

        //------------------------------------------------------------------------------------------------------------ 3. pedido
        private void CrearElPedido()
        {
            _informe.Paso($"Crear un pedido al proveedor {_config.IdProveedor}");
            var proveedor = _contexto.SeleccionarPorId<ServicioDeDatos.Terceros.ProveedorDtm>(_config.IdProveedor, usarLaCache: false);
            _informe.Comprobar(proveedor.CodigoContable is not null, $"el proveedor '{proveedor.Nombre}' tiene código contable");
            _cuentaDeAlbaranes = VariablesDeCuentas.Albaranes + proveedor.CodigoContable.ToString().PadLeft(4, '0');

            var idTipo = _config.IdTipoDePedido > 0 ? _config.IdTipoDePedido : enumNegocio.Pedido.Tipos(_contexto).First().Id;
            _pedido = new PedidoDtm
            {
                IdCg = _almacen.IdCg,
                IdTipo = idTipo,
                IdEstado = 0,
                Nombre = $"Prueba de compras {(_conVariacion ? "con" : "sin")} variación de existencias {DateTime.Now:dd/MM/yyyy HH:mm}",
                IdProveedor = _config.IdProveedor,
                IdAlmacen = _almacen.Id
            }.Insertar(_contexto);

            _informe.Dato($"pedido '{_pedido.Referencia}' (id {_pedido.Id}), albaranes del proveedor en la {_cuentaDeAlbaranes}");
            _informe.Comprobar(_pedido.EstaEnLaEtapa(enumEtapasDePedido.PED_Etapa_De_Cumplimentacion), "el pedido está en cumplimentación");
        }

        //------------------------------------------------------------------------------------------------------------ 4. materiales
        private void MeterMateriales()
        {
            _informe.Paso("Meter materiales en el pedido");
            var orden = 0;
            foreach (var lineaDePrueba in _config.Lineas)
            {
                var unitario = _contexto.SeleccionarPorId<UnitarioDtm>(lineaDePrueba.IdUnitario, usarLaCache: false);
                orden += 10;
                var linea = new LineaDeUnPedidoDtm
                {
                    IdElemento = _pedido.Id,
                    Orden = orden,
                    TipoDeLinea = enumTipoDeLinea.Unitario,
                    IdUnitario = unitario.Id,
                    IdNaturaleza = unitario.IdNaturaleza,
                    IdUnidad = unitario.IdUnidad,
                    Cantidad = lineaDePrueba.Cantidad,
                    Precio = lineaDePrueba.Precio
                }.Insertar(_contexto);
                _lineas.Add(linea);
                _informe.Dato($"línea {linea.Orden}: {lineaDePrueba.Cantidad} x '{unitario.Nombre}' a {lineaDePrueba.Precio:N2}");
            }
            _informe.Comprobar(_lineas.Count == _config.Lineas.Count, $"{_lineas.Count} líneas en el pedido");
        }

        // La transición de solicitar genera el pdf del pedido y se lo envía por correo al proveedor: aquí se deja el pedido como lo dejaría la
        // transición, con sus hitos como en GestorDeHitos.Transitar (el último se cierra con la transición 'Solicitar' y se abre el de solicitado),
        // solicitado con la fecha de la compra y con su pdf emitido (GestorDePedidos.EmitirPdfPedido), pero sin enviárselo al proveedor.
        // El fichero del pdf queda en el servidor documental: el rollback sólo deshace su registro en la BD
        private void Solicitar()
        {
            _informe.Paso("Solicitar el pedido y emitir su pdf (sin enviárselo al proveedor)");
            var solicitado = enumEtapasDePedido.PED_Etapa_De_Solicitud.Lista().First();
            var transicion = enumNegocio.Pedido.Transiciones(_contexto).First(t => t.IdOrigen == _pedido.IdEstado && t.IdDestino == solicitado && t.Activo && !t.DelSistema);

            var flujo = GestoresDeNegocio.Negocio.GestorDeHitos.Gestor(_contexto, enumNegocio.Pedido);
            var ultimoHito = flujo.LeerRegistros(0, 1, new List<ClausulaDeFiltrado>
            {
                new ClausulaDeFiltrado(nameof(HitoDtm.IdElemento), enumCriteriosDeFiltrado.igual, _pedido.Id.ToString()),
                new ClausulaDeFiltrado(GestoresDeNegocio.Negocio.GestorDeHitos.ltrFlujos.ultimoHito, enumCriteriosDeFiltrado.esNulo, null)
            }, null).First();
            ultimoHito.IdTransicion = transicion.Id;
            var alCerrar = new ParametrosDeNegocio(enumTipoOperacion.Modificar);
            alCerrar.Parametros[nameof(HitoDtm.Fecha)] = default(DateTime);
            flujo.PersistirRegistro(ultimoHito, alCerrar);

            var alAbrir = new ParametrosDeNegocio(enumTipoOperacion.Insertar);
            alAbrir.Parametros[nameof(HitoDtm.Fecha)] = default(DateTime);
            flujo.PersistirRegistro(new HitoDtm { IdElemento = _pedido.Id, IdEstado = solicitado }, alAbrir);

            // una entrega no puede ser anterior a la creación del pedido: si la compra es de un día anterior el pedido se da por creado ese día
            _contexto.Database.ExecuteSqlInterpolated($@"UPDATE LOGISTICA.PEDIDO SET ID_ESTADO = {solicitado}, PEDIDO_EL = {_fechaDeLaCompra},
                FECCRE = CASE WHEN FECCRE > {_fechaDeLaCompra} THEN {_fechaDeLaCompra} ELSE FECCRE END WHERE ID = {_pedido.Id}");
            ServicioDeCaches.EliminarTodas();
            _pedido = _contexto.SeleccionarPorId<PedidoDtm>(_pedido.Id, usarLaCache: false);
            _informe.Comprobar(_pedido.EstaEnLaEtapa(enumEtapasDePedido.PED_Etapa_De_Solicitud), "el pedido está solicitado");

            GestorDePedidos.EmitirPdfPedido(_contexto, _pedido);
            _pedido = _contexto.SeleccionarPorId<PedidoDtm>(_pedido.Id, usarLaCache: false);
            _informe.Comprobar(_pedido.IdArchivo is not null, "el pedido tiene su pdf como archivo del pedido");
        }

        //------------------------------------------------------------------------------------------------------------ impresiones
        // Las impresiones del pedido y de sus devoluciones (botones de imprimir del pedido): cada una sube un pdf al servidor documental y lo
        // anexa a los archivos del pedido
        private void Imprimir()
        {
            _informe.Paso("Imprimir el pedido y sus devoluciones");
            var antes = _pedido.LeerAnexados(_contexto).Count();

            GestorDePedidos.ImprimirPedido(_contexto, _pedido);
            _informe.Dato(GestorDeDevolucionesDeUnPedido.ImprimirDevoluciones(_contexto, _pedido.Id));

            var anexados = _pedido.LeerAnexados(_contexto).ToList();
            _informe.Comprobar(anexados.Count == antes + 2, "el pdf del pedido y el de sus devoluciones están anexados al pedido", $"se han anexado {anexados.Count - antes}");
            var prefijo = $"Devolucion-{_pedido.Referencia}-".NormalizarFichero();
            _informe.Comprobar(anexados.Any(a => a.Nombre.StartsWith(prefijo)), $"hay un archivo '{prefijo}...' con las devoluciones");
        }

        //------------------------------------------------------------------------------------------------------------ 5 y 7. entregas
        // Entrega lo pedido de cada línea en el almacén; devuelve los preasientos de las entradas de compra
        private List<int> Entregar(string paso)
        {
            _informe.Paso(paso);
            var preasientos = new List<int>();
            foreach (var linea in _lineas)
            {
                var leida = _contexto.SeleccionarPorId<LineaDeUnPedidoDtm>(linea.Id, usarLaCache: false);
                var mensaje = GestorDeEntregasDeUnPedido.RecepcionarLinea(_contexto, leida.Id, Convert.ToBase64String(leida.RowVersion), _fechaDeLaCompra,
                    (decimal)leida.Cantidad, (decimal)leida.Precio, null, null, null);
                _informe.Dato(mensaje);

                var recepcion = Recepciones(linea).OrderByDescending(r => r.Id).First();
                var entrada = EntradaDeCompra(recepcion);
                if (!_informe.Comprobar(entrada is not null, $"la entrega de la línea {linea.Orden} tiene su entrada de compra en el almacén"))
                    continue;
                if (!_informe.Comprobar(entrada.IdPreasiento is not null, $"la entrada de compra de la línea {linea.Orden} está preasentada"))
                    continue;

                preasientos.Add((int)entrada.IdPreasiento);
                ValidarPreasiento((int)entrada.IdPreasiento, $"entrega de la línea {linea.Orden}", debe: _cuentaDeLasEntregas, haber: _cuentaDeAlbaranes, Math.Round(recepcion.Importe, 2));
            }

            _pedido = _contexto.SeleccionarPorId<PedidoDtm>(_pedido.Id, usarLaCache: false);
            _informe.Comprobar(_pedido.EstaEnLaEtapa(enumEtapasDePedido.PED_Etapa_De_Recepcion), "el pedido está en recepción");
            return preasientos;
        }

        //------------------------------------------------------------------------------------------------------------ 6. quitar las entregas
        private void QuitarLasEntregas(List<int> preasientosDeLasEntregas)
        {
            _informe.Paso("Quitar las entregas");
            foreach (var linea in _lineas)
                foreach (var recepcion in Recepciones(linea))
                    _informe.Dato(GestorDeEntregasDeUnPedido.CancelarEntrega(_contexto, recepcion.Id, Convert.ToBase64String(recepcion.RowVersion)));

            var idsDeLineas = _lineas.Select(l => l.Id).ToList();
            _informe.Comprobar(!_contexto.Set<RecepcionDeLineaDtm>().Any(r => idsDeLineas.Contains(r.IdLinea)), "el pedido no tiene entregas");

            foreach (var idPreasiento in preasientosDeLasEntregas)
            {
                var preasiento = _contexto.SeleccionarPorId<PreasientoDtm>(idPreasiento, usarLaCache: false);
                _informe.Comprobar(preasiento.Estado(_contexto).Cancelado, $"el preasiento {preasiento.Referencia} de la entrega cancelada está cancelado",
                    $"está en '{preasiento.Estado(_contexto).Nombre}'");
                _informe.Comprobar(!_contexto.Set<MovimientoDeAlmacenDtm>().Any(m => m.IdPreasiento == idPreasiento), $"no queda la entrada de compra del preasiento {preasiento.Referencia}");
            }
        }

        //------------------------------------------------------------------------------------------------------------ 8. devolución
        private void Devolver()
        {
            _informe.Paso($"Devolver {_config.CantidadADevolver} de la entrega de la línea {_lineas[0].Orden}");
            var recepcion = Recepciones(_lineas[0]).OrderByDescending(r => r.Id).First();
            _informe.Dato(GestorDeDevolucionesDeUnPedido.DevolverEntrega(_contexto, recepcion.Id, Convert.ToBase64String(recepcion.RowVersion),
                _config.CantidadADevolver, _fechaDeLaCompra, "Prueba de devolución"));

            var devolucion = _contexto.Set<DevolucionDeEntregaDtm>().Where(d => d.IdLineaDeRecepcion == recepcion.Id).OrderByDescending(d => d.Id).FirstOrDefault();
            _informe.Comprobar(devolucion is not null, "la devolución está registrada");
        }

        //------------------------------------------------------------------------------------------------------------ 9. preasientos
        // Las entregas vigentes y la devolución: una entrada de compra preasentada por entrega y una salida preasentada por devolución
        private void ValidarLosPreasientosDeLaCompra()
        {
            _informe.Paso("Validar los preasientos de la compra");
            decimal debe = 0, haber = 0;
            foreach (var linea in _lineas)
            {
                foreach (var recepcion in Recepciones(linea))
                {
                    var entrada = EntradaDeCompra(recepcion);
                    if (_informe.Comprobar(entrada?.IdPreasiento is not null, $"la entrega de la línea {linea.Orden} está preasentada"))
                    {
                        ValidarPreasiento((int)entrada.IdPreasiento, $"entrega de la línea {linea.Orden}", debe: _cuentaDeLasEntregas, haber: _cuentaDeAlbaranes, Math.Round(recepcion.Importe, 2));
                        debe += Math.Round(recepcion.Importe, 2);
                    }

                    foreach (var devolucion in _contexto.Set<DevolucionDeEntregaDtm>().Where(d => d.IdLineaDeRecepcion == recepcion.Id).ToList())
                    {
                        var salida = _contexto.Set<MovimientoDeAlmacenDtm>().FirstOrDefault(m => m.IdLineaDevolucion == devolucion.Id);
                        if (_informe.Comprobar(salida?.IdPreasiento is not null, $"la devolución de la línea {linea.Orden} tiene su salida del almacén preasentada"))
                        {
                            var importe = Math.Round(devolucion.Importe(recepcion), 2);
                            ValidarPreasiento((int)salida.IdPreasiento, $"devolución de la línea {linea.Orden}", debe: _cuentaDeAlbaranes, haber: _cuentaDeLasDevoluciones, importe);
                            haber += importe;
                        }
                    }
                }
            }
            _informe.Dato($"compras al debe de la {_cuentaDeLasEntregas}: {debe:N2}; devoluciones al haber de la {_cuentaDeLasDevoluciones}: {haber:N2}; " +
                          $"saldo de albaranes del proveedor en la {_cuentaDeAlbaranes}: {debe - haber:N2} (acreedor)");
        }

        //------------------------------------------------------------------------------------------------------------ apoyo
        // Preasiento pendiente con dos apuntes por el mismo importe: debe y haber en las cuentas indicadas
        private void ValidarPreasiento(int idPreasiento, string de, string debe, string haber, decimal importe)
        {
            var preasiento = _contexto.SeleccionarPorId<PreasientoDtm>(idPreasiento, usarLaCache: false);
            _informe.Comprobar(preasiento.EstaEnLaEtapa(enumEtapasDePreasiento.SPR_Etapa_Pendiente), $"preasiento {preasiento.Referencia} de la {de} pendiente",
                $"está en '{preasiento.Estado(_contexto).Nombre}'");

            var apuntes = _contexto.Set<ApunteDeUnPreasientoDtm>().Where(a => a.IdElemento == idPreasiento).ToList();
            if (!_informe.Comprobar(apuntes.Count == 2, $"preasiento {preasiento.Referencia} con 2 apuntes", $"tiene {apuntes.Count}"))
                return;

            var apunteDelDebe = apuntes.First(a => a.Posicion == enumPosicionContable.Debe);
            var apunteDelHaber = apuntes.First(a => a.Posicion == enumPosicionContable.Haber);
            _informe.Comprobar(apunteDelDebe.Cuenta == debe && apunteDelDebe.Importe == importe, $"   debe  {debe} {importe:N2}", $"es {apunteDelDebe.Cuenta} {apunteDelDebe.Importe:N2}");
            _informe.Comprobar(apunteDelHaber.Cuenta == haber && apunteDelHaber.Importe == importe, $"   haber {haber} {importe:N2}", $"es {apunteDelHaber.Cuenta} {apunteDelHaber.Importe:N2}");
        }

        private List<RecepcionDeLineaDtm> Recepciones(LineaDeUnPedidoDtm linea) =>
            _contexto.Set<RecepcionDeLineaDtm>().Where(r => r.IdLinea == linea.Id).ToList();

        private MovimientoDeAlmacenDtm EntradaDeCompra(RecepcionDeLineaDtm recepcion) =>
            _contexto.Set<MovimientoDeAlmacenDtm>().FirstOrDefault(m => m.IdLineaDeRecepcion == recepcion.Id && m.IdLineaDevolucion == null);

        private CuentaDtm Cuenta(string codigo) => _contexto.Set<CuentaDtm>().FirstOrDefault(c => c.Codigo == codigo);
    }
}
