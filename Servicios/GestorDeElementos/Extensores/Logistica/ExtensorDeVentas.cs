using Gestor.Errores;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Logistica;
using ServicioDeDatos.MaestrosTecnico;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Ventas;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilidades;
using static ServicioDeDatos.Elemento.Enumerados;

namespace GestorDeElementos.Extensores
{
    public static class ExtensorDeVentas
    {
        public static int IncrementarOrdenEn(ContextoSe contexto)
        {
            var incremento = enumNegocio.Venta.LeerCrearParametro(contexto, enumParametrosDeVentas.DSA_IncrementarOrdenEn, "10");
            if (incremento is null || incremento.Valor.Entero() == 0)
                GestorDeErrores.Emitir($"Ha de definir el parámetro '{enumParametrosDeVentas.DSA_IncrementarOrdenEn}' del negocio de '{enumNegocio.Venta.Singular()}' con un valor mayor de 0");
            return incremento.Valor.Entero();
        }

        // Venta de material desde la consulta de inventario (ver VentasController.epCrearVenta): crea el documento de venta del primer tipo
        // activo que permite crear, con una línea del material, en la etapa de solicitud.
        // Si se indica servido, el documento pasa a la etapa de servicio, y al hacerlo se crea la salida del almacén de lo vendido a la fecha
        // indicada (ver CrearSalidasDelAlmacen); si además se indica facturar, pasa a la de facturación, que crea la prefactura al cliente
        // del tipo indicado en el tipo de documento de venta (ver CrearPrefactura). Facturar exige servido. Todo o nada
        public static VentaDtm CrearVentaDeMaterial(ContextoSe contexto, int idAlmacen, int idUnitario, int idCliente, DateTime fecha, decimal cantidad, decimal precio, bool servido = false, bool facturar = false)
        {
            fecha = fecha.Date;
            if (facturar && !servido)
                GestorDeErrores.Emitir("Para facturar la venta ha de marcarla como servida");
            ValidarVentaDeMaterial(contexto, idAlmacen, idUnitario, idCliente, fecha, cantidad, precio, servido);
            if (facturar && precio <= 0)
                GestorDeErrores.Emitir("Para facturar la venta el precio ha de ser mayor que 0");

            var almacen = contexto.SeleccionarPorId<AlmacenDtm>(idAlmacen);
            var unitario = contexto.SeleccionarPorId<UnitarioDtm>(idUnitario);
            var cliente = contexto.SeleccionarPorId<ClienteDtm>(idCliente);
            var tipo = TipoParaVenderMaterial(contexto);
            if (facturar)
                tipo.TipoDeFacturaEmt(contexto);

            var transaccion = contexto.IniciarTransaccion();
            try
            {
                var venta = new VentaDtm
                {
                    IdCg = almacen.IdCg,
                    IdTipo = tipo.Id,
                    IdEstado = 0,
                    Nombre = $"Venta de {unitario.Nombre}".Left(IDominio.Longitud(IDominio.VARCHAR_250) - 1),
                    IdCliente = idCliente,
                    IdAlmacen = idAlmacen,
                    SolicitadoEl = fecha,
                    ServidoEl = servido ? fecha : null
                }.Insertar(contexto);

                var linea = new LineaDeUnaVentaDtm
                {
                    IdElemento = venta.Id,
                    Orden = IncrementarOrdenEn(contexto),
                    IdUnitario = idUnitario,
                    Cantidad = cantidad,
                    Precio = precio
                }.Insertar(contexto);

                venta.CrearTraza(contexto, "Venta de material",
                    $"Venta realizada por {contexto.Usuario.Expresion} desde el inventario del almacén '{almacen.Expresion}'{Environment.NewLine}" +
                    $"Material: {unitario.Expresion}{Environment.NewLine}" +
                    $"Cliente: {cliente.Expresion}{Environment.NewLine}" +
                    $"Fecha: {fecha:dd/MM/yyyy}, cantidad: {cantidad:0.####}, precio: {precio:N2}, total: {cantidad * precio:N2}");

                // la salida del almacén se crea al pasar a servido, a la fecha de ServidoEl
                if (servido)
                    venta = venta.Servir(contexto);
                if (facturar)
                    venta = venta.Facturar(contexto);

                contexto.Commit(transaccion);
                return venta;
            }
            catch
            {
                contexto.Rollback(transaccion);
                throw;
            }
        }

        // El stock sólo se valida si se sirve (es cuando sale del almacén); lo vuelve a validar el gestor de movimientos al crear la salida
        private static void ValidarVentaDeMaterial(ContextoSe contexto, int idAlmacen, int idUnitario, int idCliente, DateTime fecha, decimal cantidad, decimal precio, bool servido)
        {
            if (idAlmacen <= 0) GestorDeErrores.Emitir("Ha de indicar el almacén desde el que se vende");
            if (idUnitario <= 0) GestorDeErrores.Emitir("Ha de indicar el material que se vende");
            if (idCliente <= 0) GestorDeErrores.Emitir("Ha de indicar el cliente al que se vende");
            if (cantidad <= 0) GestorDeErrores.Emitir("La cantidad a vender ha de ser mayor que 0");
            if (precio < 0) GestorDeErrores.Emitir("El precio de venta no puede ser negativo");
            if (fecha > DateTime.Today) GestorDeErrores.Emitir($"La fecha de la venta ({fecha:dd/MM/yyyy}) no puede ser posterior a hoy");

            var almacen = contexto.SeleccionarPorId<AlmacenDtm>(idAlmacen);
            if (!almacen.EstaActivo())
                GestorDeErrores.Emitir($"El almacén '{almacen.Expresion}' no está activo, no se puede vender material desde él");

            var cierre = ExtensorDeSociedades.UltimoCierreContable(contexto, almacen.Sociedad(contexto).Id);
            if (cierre is not null && fecha < cierre.CerradaEl.Date)
                GestorDeErrores.Emitir($"La fecha de la venta ({fecha:dd/MM/yyyy}) no puede ser anterior al último cierre contable de la sociedad ({cierre.CerradaEl:dd/MM/yyyy})");

            if (!servido)
                return;

            var anterior = ExtensorDeAlmacenes.UltimoMovimientoHasta(contexto, idAlmacen, idUnitario, fecha);
            var stock = anterior?.Stock ?? 0;
            if (stock < cantidad)
            {
                var formato = ExtensorDeAlmacenes.FormatoDeCantidad(stock);
                GestorDeErrores.Emitir($"No puede realizarse la venta ya que el {fecha:dd/MM/yyyy} sólo quedan {stock.ToString(formato)} y se solicitan {cantidad.ToString(formato)}");
            }
        }

        // Tipo con el que se crean las ventas de material: el primero activo que permite crear
        private static TipoDeVentaDtm TipoParaVenderMaterial(ContextoSe contexto)
        {
            var tipo = contexto.Set<TipoDeVentaDtm>()
                .Where(t => t.Activo && t.PermiteCrear)
                .OrderBy(t => t.Id)
                .FirstOrDefault();
            if (tipo is null)
                GestorDeErrores.Emitir($"No hay definido ningún tipo de {enumNegocio.Venta.Singular(true)} activo que permita crear, no se puede vender el material");
            return tipo;
        }

        public static VentaDtm Venta(this LineaDeUnaVentaDtm linea, ContextoSe contexto)
        {
            if (linea.Elemento is not null && linea.IdElemento == linea.Elemento.Id)
            {
                return linea.Elemento;
            }

            return linea.Elemento = contexto.SeleccionarPorId<VentaDtm>(linea.IdElemento);
        }

        /// <summary>
        /// Valor del stock (precio medio ponderado) del unitario de la línea en el almacén del documento de venta a fecha de solicitud:
        /// el Valor del último movimiento de almacén realizado hasta ese día. Si no se ha indicado la fecha de solicitud se toma la de hoy.
        /// Devuelve null si no hay movimientos anteriores.
        /// </summary>
        public static decimal? PrecioAlmacen(this LineaDeUnaVentaDtm linea, ContextoSe contexto)
        {
            var venta = linea.Venta(contexto);
            var hasta = (venta.SolicitadoEl ?? DateTime.Now).Date.AddDays(1);

            return contexto.Set<MovimientoDeAlmacenDtm>()
                .Where(x => x.IdAlmacen == venta.IdAlmacen
                         && x.IdUnitario == linea.IdUnitario
                         && x.TipoMovimiento.ClaseMovimiento != enumClaseDeMovimiento.Nulo
                         && x.RealizadoEl < hasta)
                .OrderByDescending(x => x.RealizadoEl).ThenByDescending(x => x.Id)
                .Select(x => (decimal?)x.Valor)
                .FirstOrDefault();
        }

        /// <summary>
        /// Margen (%) del precio de venta neto de descuento sobre el precio del almacén: (precio neto - precio almacén) / precio neto.
        /// Si es negativo se está vendiendo por debajo del coste.
        /// </summary>
        public static decimal? Margen(this LineaDeUnaVentaDtm linea, decimal? precioAlmacen)
        {
            var precioNeto = linea.Precio * (1 - (linea.Descuento ?? 0m) / 100);
            if (precioAlmacen is null || precioNeto == 0)
                return null;

            return Math.Round((precioNeto - (decimal)precioAlmacen) / precioNeto * 100, 2);
        }

        public static AlmacenDtm Almacen(this VentaDtm venta, ContextoSe contexto)
        {
            if (venta.Almacen is not null && venta.IdAlmacen == venta.Almacen.Id)
            {
                return venta.Almacen;
            }

            return venta.Almacen = contexto.SeleccionarPorId<AlmacenDtm>(venta.IdAlmacen);
        }

        // Importe de la venta sin iva: suma de los importes de sus líneas con su descuento (el importe no se guarda, se calcula en memoria)
        public static decimal TotalSinIva(this VentaDtm venta, ContextoSe contexto)
            => Math.Round(venta.Detalles<LineaDeUnaVentaDtm>(contexto).Sum(l => l.Importe), 2);

        public static TipoDeVentaDtm TipoDeVenta(this VentaDtm venta, ContextoSe contexto)
            => contexto.SeleccionarPorId<TipoDeVentaDtm>(venta.IdTipo);

        // Tipo de la prefactura de los documentos de venta de un tipo: el indicado en el tipo de documento de venta
        public static TipoDeFacturaEmtDtm TipoDeFacturaEmt(this TipoDeVentaDtm tipo, ContextoSe contexto)
        {
            if (tipo.IdTipoFacturaEmt is null)
                GestorDeErrores.Emitir($"El tipo de {enumNegocio.Venta.Singular(true)} '{tipo.Nombre}' no tiene indicado el tipo de prefactura que se ha de crear al facturar");
            return contexto.SeleccionarPorId<TipoDeFacturaEmtDtm>((int)tipo.IdTipoFacturaEmt);
        }

        /// <summary>
        /// DSA: Solicitado --> DSA: Servir --> DSA: Sirviendo (del sistema): al transitar se crean las salidas del almacén de sus líneas
        /// (ver AntesDeServir y CrearSalidasDelAlmacen). Devuelve la venta releída
        /// </summary>
        public static VentaDtm Servir(this VentaDtm venta, ContextoSe contexto)
        {
            return venta.TransitarALaEtapa(contexto, enumEtapasDeVenta.DSA_Etapa_Servicio.EstadosDeLaEtapa(), delSistema: true);
        }

        // Solicitado --> Sirviendo, antes de transitar: si no se ha indicado cuándo se sirve, se sirve hoy (la fecha, sin hora, de las salidas
        // del almacén); se guarda con la transición
        public static void AntesDeServir(this VentaDtm venta, ContextoSe contexto)
        {
            if (venta.Detalles<LineaDeUnaVentaDtm>(contexto).Count == 0)
                GestorDeErrores.Emitir($"El {enumNegocio.Venta.Singular(true)} '{venta.Referencia}' no tiene líneas, no se puede servir");
            venta.ServidoEl = (venta.ServidoEl ?? DateTime.Today).Date;
            venta.ValidarFechaDeServicio();
        }

        // "Servido el" (la fecha de las salidas del almacén, ver CrearSalidasDelAlmacen) se puede indicar mientras el documento de venta está
        // solicitado: no puede ser anterior a la fecha de solicitud ni posterior a hoy. Se valida al guardar (GestorDeVentas.AntesDePersistir)
        // y al servir (AntesDeServir)
        public static void ValidarFechaDeServicio(this VentaDtm venta)
        {
            if (venta.ServidoEl is null)
                return;

            var servidoEl = ((DateTime)venta.ServidoEl).Date;
            if (venta.SolicitadoEl is not null && servidoEl < ((DateTime)venta.SolicitadoEl).Date)
                GestorDeErrores.Emitir($"La fecha de servicio ({servidoEl:dd/MM/yyyy}) no puede ser anterior a la de solicitud del {enumNegocio.Venta.Singular(true)} ({venta.SolicitadoEl:dd/MM/yyyy})");
            if (servidoEl > DateTime.Today)
                GestorDeErrores.Emitir($"La fecha de servicio ({servidoEl:dd/MM/yyyy}) no puede ser posterior a hoy ({DateTime.Today:dd/MM/yyyy})");
        }

        // Solicitado --> Sirviendo, después de transitar: una salida del almacén de la venta por cada línea, a la fecha en la que se sirve
        // y valorada al precio medio vigente. El gestor de movimientos valida el stock, la preasienta (salida de existencias si el almacén no
        // usa la variación, ver ExtensorDePreasientos.PreasentarSalidaDelmacen) y barre los movimientos posteriores
        public static void CrearSalidasDelAlmacen(this VentaDtm venta, ContextoSe contexto)
        {
            var fecha = (venta.ServidoEl ?? DateTime.Today).Date;
            var idTipoDeMovimiento = enumTiposDeMovimiento.Venta.LeerTipoDeMovimiento(contexto).Id;
            var lineas = venta.Detalles<LineaDeUnaVentaDtm>(contexto);
            var salidas = new List<string>();
            foreach (var linea in lineas.OrderBy(l => l.Orden))
            {
                if (contexto.Set<MovimientoDeAlmacenDtm>().Any(m => m.IdLineaDeVenta == linea.Id))
                    continue;

                var movimiento = new MovimientoDeAlmacenDtm
                {
                    IdAlmacen = venta.IdAlmacen,
                    IdUnitario = linea.IdUnitario,
                    IdTipoMovimiento = idTipoDeMovimiento,
                    Cantidad = linea.Cantidad,
                    Precio = 0,
                    RealizadoEl = fecha,
                    IdLineaDeVenta = linea.Id
                }.InsertarComoAdministrador(contexto);
                salidas.Add($"Línea {linea.Orden}: {movimiento.Referencia(contexto)}, cantidad {linea.Cantidad:0.####}, valorada a {movimiento.Precio:N4}");
            }

            if (salidas.Count > 0)
                venta.CrearTraza(contexto, "Salidas del almacén", $"Al servir, {contexto.Usuario.Expresion} ha dado salida del almacén '{venta.Almacen(contexto).Expresion}' el {fecha:dd/MM/yyyy}:{Environment.NewLine}{string.Join(Environment.NewLine, salidas)}");
        }

        /// <summary>
        /// DSA: Sirviendo --> DSA: Facturar --> DSA: Facturado (transición de usuario): al transitar se crea la prefactura (ver
        /// GestorDeVentas.DespuesDeTransitar y CrearPrefactura). Devuelve la venta releída, con su IdFactura
        /// </summary>
        public static VentaDtm Facturar(this VentaDtm venta, ContextoSe contexto)
        {
            return venta.TransitarALaEtapa(contexto, enumEtapasDeVenta.DSA_Etapa_Facturacion.EstadosDeLaEtapa(), delSistema: false);
        }

        /// <summary>
        /// DSA: Sirviendo --> DSA: Devolver a solicitado --> DSA: Solicitado (del sistema)
        /// </summary>
        public static VentaDtm DevolverASolicitado(this VentaDtm venta, ContextoSe contexto)
        {
            return venta.TransitarALaEtapa(contexto, enumEtapasDeVenta.DSA_Etapa_Solicitud.EstadosDeLaEtapa(), delSistema: true);
        }

        // Sirviendo --> Solicitado, antes de transitar: se borran las salidas del almacén de sus líneas (no pueden tener el preasiento
        // contabilizado); el gestor de movimientos cancela su preasiento y barre los posteriores. Deja de estar servido
        public static void AntesDeDevolverASolicitado(this VentaDtm venta, ContextoSe contexto)
        {
            var salidas = venta.SalidasDelAlmacen(contexto);
            foreach (var salida in salidas)
            {
                var preasiento = salida.PreasientoContabilizado(contexto);
                if (preasiento is not null)
                    GestorDeErrores.Emitir($"No se puede devolver a solicitado el {enumNegocio.Venta.Singular(true)} '{venta.Referencia}': la salida del almacén '{salida.Referencia(contexto)}' tiene el preasiento '{preasiento.Referencia}' contabilizado");
            }

            var borradas = new List<string>();
            foreach (var salida in salidas.OrderByDescending(m => m.RealizadoEl).ThenByDescending(m => m.Id))
            {
                borradas.Add(salida.Referencia(contexto));
                salida.EliminarComoAdministrador(contexto, accionEjecutada: ltrDeUnMovimientoDeAlmacen.Accion_AnularSalidaDeVenta);
            }

            venta.ServidoEl = null;
            if (borradas.Count > 0)
                venta.CrearTraza(contexto, "Salidas del almacén anuladas", $"Al devolver a solicitado, {contexto.Usuario.Expresion} ha anulado las salidas del almacén:{Environment.NewLine}{string.Join(Environment.NewLine, borradas)}");
        }

        /// <summary>
        /// "Cambiar datos" del menú de edición (ver VentasController.epCambiarDatos): cambia el nombre, el cliente y la fecha de servicio del
        /// documento de venta. Sólo en la etapa de solicitud o en la de servicio, sin prefactura ni factura y sin devoluciones. En la de servicio
        /// se rehacen las salidas del almacén: se borran (el gestor de movimientos cancela su preasiento y barre los posteriores) y se crean de
        /// nuevo a la nueva fecha (con su preasiento y el barrido). No se puede si alguna salida tiene el preasiento contabilizado o si hay un
        /// cierre contable en o después de la fecha anterior o de la nueva. Devuelve la venta releída
        /// </summary>
        public static VentaDtm CambiarDatos(ContextoSe contexto, int idVenta, string nombre, int idCliente, DateTime? servidoEl)
        {
            var venta = contexto.SeleccionarPorId<VentaDtm>(idVenta, usarLaCache: false);
            var enSolicitud = venta.EstaEnLaEtapa(enumEtapasDeVenta.DSA_Etapa_Solicitud);
            var enServicio = venta.EstaEnLaEtapa(enumEtapasDeVenta.DSA_Etapa_Servicio);
            if (!enSolicitud && !enServicio)
                GestorDeErrores.Emitir($"Sólo se pueden cambiar los datos del {enumNegocio.Venta.Singular(true)} '{venta.Referencia}' en la etapa de {enumEtapasDeVenta.DSA_Etapa_Solicitud.Nombre(true)} o en la de {enumEtapasDeVenta.DSA_Etapa_Servicio.Nombre(true)}");
            if (venta.IdFacturaEmt is not null)
                GestorDeErrores.Emitir($"No se pueden cambiar los datos del {enumNegocio.Venta.Singular(true)} '{venta.Referencia}': tiene prefactura o factura");
            if (venta.Detalles<LineaDeUnaVentaDtm>(contexto, usarLaCache: false).Any(l => l.CantidadDevuelta(contexto) > 0))
                GestorDeErrores.Emitir($"No se pueden cambiar los datos del {enumNegocio.Venta.Singular(true)} '{venta.Referencia}': tiene {enumNegocio.DevolucionVenta.Plural(true)}");
            if (nombre.IsNullOrEmpty())
                GestorDeErrores.Emitir($"Ha de indicar el nombre del {enumNegocio.Venta.Singular(true)}");
            if (idCliente == 0)
                GestorDeErrores.Emitir($"Ha de indicar el cliente del {enumNegocio.Venta.Singular(true)}");
            if (enServicio && servidoEl is null)
                GestorDeErrores.Emitir($"El {enumNegocio.Venta.Singular(true)} '{venta.Referencia}' está servido: ha de indicar la fecha de servicio");

            var fechaAnterior = venta.ServidoEl?.Date;
            var fechaNueva = servidoEl?.Date;
            var cambios = new List<string>();
            if (venta.Nombre != nombre) cambios.Add($"nombre '{venta.Nombre}' --> '{nombre}'");
            if (venta.IdCliente != idCliente) cambios.Add($"cliente {venta.IdCliente} --> {idCliente}");
            if (fechaAnterior != fechaNueva) cambios.Add($"servido el {fechaAnterior:dd/MM/yyyy} --> {fechaNueva:dd/MM/yyyy}");

            var salidas = enServicio ? venta.SalidasDelAlmacen(contexto) : new List<MovimientoDeAlmacenDtm>();
            if (enServicio)
            {
                foreach (var salida in salidas)
                {
                    var preasiento = salida.PreasientoContabilizado(contexto);
                    if (preasiento is not null)
                        GestorDeErrores.Emitir($"No se pueden cambiar los datos del {enumNegocio.Venta.Singular(true)} '{venta.Referencia}': la salida del almacén '{salida.Referencia(contexto)}' tiene el preasiento '{preasiento.Referencia}' contabilizado");
                }

                var cierre = ExtensorDeSociedades.UltimoCierreContable(contexto, venta.Almacen(contexto).Sociedad(contexto).Id);
                if (cierre is not null)
                {
                    var cerradaEl = cierre.CerradaEl.Date;
                    if (fechaAnterior is not null && fechaAnterior <= cerradaEl)
                        GestorDeErrores.Emitir($"No se pueden cambiar los datos del {enumNegocio.Venta.Singular(true)} '{venta.Referencia}': se sirvió el {fechaAnterior:dd/MM/yyyy} y hay un cierre contable el {cerradaEl:dd/MM/yyyy}");
                    if (fechaNueva <= cerradaEl)
                        GestorDeErrores.Emitir($"No se pueden cambiar los datos del {enumNegocio.Venta.Singular(true)} '{venta.Referencia}': la nueva fecha de servicio ({fechaNueva:dd/MM/yyyy}) no es posterior al cierre contable del {cerradaEl:dd/MM/yyyy}");
                }
            }

            var cambiaElCliente = venta.IdCliente != idCliente;
            venta.Nombre = nombre;
            venta.IdCliente = idCliente;
            if (cambiaElCliente)
                venta.CambiarDatosCliente(contexto);
            venta.ServidoEl = fechaNueva;
            venta.ValidarFechaDeServicio();

            if (enServicio)
            {
                // primero se borran las salidas (de la más reciente a la más antigua), luego se guarda la venta y se crean las nuevas
                foreach (var salida in salidas.OrderByDescending(m => m.RealizadoEl).ThenByDescending(m => m.Id))
                    salida.EliminarComoAdministrador(contexto, accionEjecutada: ltrDeUnMovimientoDeAlmacen.Accion_AnularSalidaDeVenta);
            }

            venta.Modificar(contexto, esUnaAccion: true);
            venta = contexto.SeleccionarPorId<VentaDtm>(idVenta, usarLaCache: false);

            if (enServicio)
                venta.CrearSalidasDelAlmacen(contexto);

            venta.CrearTraza(contexto, "Datos cambiados", $"{contexto.Usuario.Expresion} ha cambiado los datos del {enumNegocio.Venta.Singular(true)}" +
                (cambios.Count == 0 ? " (sin cambios)" : $": {string.Join(", ", cambios)}") +
                (enServicio ? $"{Environment.NewLine}Se han rehecho las salidas del almacén a la fecha {fechaNueva:dd/MM/yyyy}" : ""));
            return venta;
        }

        // Salidas del almacén de las líneas del documento de venta
        public static List<MovimientoDeAlmacenDtm> SalidasDelAlmacen(this VentaDtm venta, ContextoSe contexto)
        {
            var idsDeLineas = venta.Detalles<LineaDeUnaVentaDtm>(contexto).Select(l => l.Id).ToList();
            return contexto.Set<MovimientoDeAlmacenDtm>()
                .Where(m => m.IdLineaDeVenta != null && idsDeLineas.Contains((int)m.IdLineaDeVenta))
                .ToList();
        }

        // Importe sin iva de lo devuelto del documento de venta en devoluciones ya aceptadas (las que están en tramitación aún no cuentan)
        public static decimal TotalDevueltoSinIva(this VentaDtm venta, ContextoSe contexto)
        {
            var idsDeLineas = venta.Detalles<LineaDeUnaVentaDtm>(contexto).Select(l => l.Id).ToList();
            var aceptadas = enumEtapasDeDevolucionDeVenta.DDV_Etapa_Aceptada.Lista();
            return Math.Round(contexto.Set<LineaDeUnaDevolucionDeVentaDtm>()
                .Where(l => idsDeLineas.Contains(l.IdLineaDeVenta)
                         && contexto.Set<DevolucionDeVentaDtm>().Any(d => d.Id == l.IdElemento && aceptadas.Contains(d.IdEstado)))
                .ToList()
                .Sum(l => l.Importe), 2);
        }

        /// <summary>
        /// Antes de emitir una factura (ver ExtensorDeFacturasEmt.AntesDeEmitirFactura): si hay documentos de venta que apuntan a ella, su base
        /// imponible ha de ser lo vendido en ellos menos lo devuelto en devoluciones aceptadas. Al aceptar una devolución de un documento de
        /// venta prefacturado la prefactura no se toca (ver ExtensorDeDevolucionesDeVenta.RegularizarFacturas): es aquí donde se pide corregirla
        /// </summary>
        public static void ValidarImporteDeLasVentas(this FacturaEmtDtm factura, ContextoSe contexto)
        {
            var ventas = contexto.Set<VentaDtm>().Where(v => v.IdFacturaEmt == factura.Id).ToList();
            if (ventas.Count == 0)
                return;

            var vendido = ventas.Sum(v => v.TotalSinIva(contexto));
            var devuelto = ventas.Sum(v => v.TotalDevueltoSinIva(contexto));
            var esperado = Math.Round(vendido - devuelto, 2);
            var lineasDeLaFactura = factura.Detalles<LineaDeUnaFaeDtm>(contexto, usarLaCache: false);
            var baseImponible = Math.Round(lineasDeLaFactura.Sum(l => l.ImporteConDto), 2);
            if (Math.Abs(baseImponible - esperado) < 0.01m)
                return;

            var documentos = string.Join(", ", ventas.Select(v => $"'{v.Referencia}'"));
            GestorDeErrores.Emitir($"No se puede emitir la factura '{factura.Referencia}': su base imponible ({baseImponible:N2}) no coincide con lo que realmente se vende " +
                $"en {(ventas.Count == 1 ? $"el {enumNegocio.Venta.Singular(true)}" : $"los {enumNegocio.Venta.Plural(true)}")} {documentos} ({esperado:N2}: " +
                $"vendido {vendido:N2}{(devuelto == 0 ? "" : $" menos {devuelto:N2} devuelto en {enumNegocio.DevolucionVenta.Plural(true)} aceptadas")}). " +
                $"Modifique las líneas de la factura para que su base imponible sea {esperado:N2}.{DiferenciasPorLinea(contexto, ventas, lineasDeLaFactura)}");
        }

        /// <summary>
        /// Antes de emitir una factura (ver ExtensorDeFacturasEmt.AntesDeEmitirFactura): ninguna de las líneas vendidas que factura puede tener
        /// una devolución en tramitación. Mientras no se acepta, lo devuelto no ha entrado en el almacén ni se ha quitado de la prefactura:
        /// emitirla facturaría material que el cliente ha devuelto y dejaría en el almacén menos existencias de las que hay. Hay que aceptar
        /// la devolución (o cancelarla) desde el documento de venta
        /// </summary>
        public static void ValidarDevolucionesPendientes(this FacturaEmtDtm factura, ContextoSe contexto)
        {
            var idsDeLineasVendidas = contexto.Set<LineaDeUnaFaeDtm>()
                .Where(l => l.IdElemento == factura.Id && l.IdLineaDeVenta != null)
                .Select(l => (int)l.IdLineaDeVenta)
                .ToList();
            if (idsDeLineasVendidas.Count == 0)
                return;

            var enTramitacion = enumEtapasDeDevolucionDeVenta.DDV_Etapa_Tramitacion.Lista();
            var pendientes = contexto.Set<LineaDeUnaDevolucionDeVentaDtm>()
                .Where(l => idsDeLineasVendidas.Contains(l.IdLineaDeVenta)
                         && contexto.Set<DevolucionDeVentaDtm>().Any(d => d.Id == l.IdElemento && enTramitacion.Contains(d.IdEstado)))
                .Select(l => l.IdElemento)
                .Distinct()
                .ToList();
            if (pendientes.Count == 0)
                return;

            var referencias = contexto.Set<DevolucionDeVentaDtm>().Where(d => pendientes.Contains(d.Id)).Select(d => d.Referencia).ToList();
            GestorDeErrores.Emitir($"No se puede emitir la factura '{factura.Referencia}': {(referencias.Count == 1 ? $"la {enumNegocio.DevolucionVenta.Singular(true)}" : $"las {enumNegocio.DevolucionVenta.Plural(true)}")} " +
                $"{string.Join(", ", referencias.Select(r => $"'{r}'"))} de sus líneas {(referencias.Count == 1 ? "está" : "están")} pendiente{(referencias.Count == 1 ? "" : "s")} de aceptar. " +
                $"Acéptela{(referencias.Count == 1 ? "" : "s")} o cancéle{(referencias.Count == 1 ? "la" : "las")} desde el {enumNegocio.Venta.Singular(true)}: si se emitiera, se facturaría material devuelto y el almacén quedaría con menos existencias de las que hay");
        }

        // Qué líneas no cuadran, por el origen de cada línea de la factura (IdLineaDeVenta): las vendidas cuya cantidad o importe facturado
        // no es lo vendido menos lo devuelto y las líneas de la factura que no vienen de ningún documento de venta
        private static string DiferenciasPorLinea(ContextoSe contexto, List<VentaDtm> ventas, List<LineaDeUnaFaeDtm> lineasDeLaFactura)
        {
            const int maximo = 10;
            var diferencias = new List<string>();
            foreach (var venta in ventas)
            {
                foreach (var vendida in venta.Detalles<LineaDeUnaVentaDtm>(contexto).OrderBy(l => l.Orden))
                {
                    var devuelta = vendida.CantidadDevueltaAceptada(contexto);
                    var cantidadEsperada = vendida.Cantidad - devuelta;
                    var importeEsperado = Math.Round(vendida.Importe * (vendida.Cantidad == 0 ? 0 : cantidadEsperada / vendida.Cantidad), 2);
                    var facturadas = lineasDeLaFactura.Where(l => l.IdLineaDeVenta == vendida.Id).ToList();
                    var cantidadFacturada = facturadas.Sum(l => l.Cantidad ?? 0);
                    var importeFacturado = Math.Round(facturadas.Sum(l => l.ImporteConDto), 2);
                    if (cantidadFacturada == cantidadEsperada && Math.Abs(importeFacturado - importeEsperado) < 0.01m)
                        continue;

                    var unitario = contexto.SeleccionarPorId<UnitarioDtm>(vendida.IdUnitario);
                    diferencias.Add($"'{venta.Referencia}' línea {vendida.Orden} {unitario.Expresion}: facturado {cantidadFacturada:0.####} por {importeFacturado:N2}, " +
                        $"ha de ser {cantidadEsperada:0.####} por {importeEsperado:N2} (vendido {vendida.Cantidad:0.####}{(devuelta == 0 ? "" : $", devuelto {devuelta:0.####}")})");
                }
            }

            foreach (var sinOrigen in lineasDeLaFactura.Where(l => l.IdLineaDeVenta is null && l.ImporteConDto != 0).OrderBy(l => l.Orden))
                diferencias.Add($"línea {sinOrigen.Orden} de la factura '{sinOrigen.Concepto}' ({sinOrigen.ImporteConDto:N2}) no corresponde a ningún {enumNegocio.Venta.Singular(true)}");

            if (diferencias.Count == 0)
                return "";
            var mas = diferencias.Count > maximo ? $"{Environment.NewLine}... y {diferencias.Count - maximo} más" : "";
            return $"{Environment.NewLine}Diferencias:{Environment.NewLine}- {string.Join($"{Environment.NewLine}- ", diferencias.Take(maximo))}{mas}";
        }

        // Lo que ha hecho ModificarLineaServida: si ha modificado la línea (para reimprimir la oferta) y la devolución creada, si la hay
        public class ResultadoDeModificarLoServido
        {
            public VentaDtm Venta { get; set; }
            public bool LineaModificada { get; set; }
            public DevolucionDeVentaDtm Devolucion { get; set; }
        }

        /// <summary>
        /// Acción Devolver de una línea con el documento de venta servido (aún sin prefacturar, ver VentasController.epModificarLineaServida):
        /// se corrige lo servido. La cantidad es lo realmente servido, sin lo ya devuelto.
        ///  - si la salida del almacén está en un periodo abierto se modifican la cantidad (lo servido más lo devuelto) y el precio de la línea
        ///    y la salida: al guardarla el gestor de movimientos cancela su preasiento, recalcula el inventario, la vuelve a preasentar y barre
        ///    los posteriores (ver GestorDeMovimientosDeAlmacen.AntesDePersistir y DespuesDePersistir)
        ///  - si está en un periodo cerrado (en o antes del último cierre contable, o con el preasiento contabilizado) la salida no se toca:
        ///    se modifica el precio de la línea y lo que se deja de servir se devuelve en una devolución de venta nueva, en tramitación, que se
        ///    acepta desde el grid de devoluciones del documento de venta (su entrada en el almacén va al día siguiente al cierre, ver
        ///    ExtensorDeDevolucionesDeVenta.AntesDeAceptar). Tras el cierre no se puede aumentar lo servido
        /// La reimpresión de la oferta la hace GestorDeVentas
        /// </summary>
        public static ResultadoDeModificarLoServido ModificarLineaServida(ContextoSe contexto, int idLineaDeVenta, decimal cantidad, decimal precio)
        {
            var linea = contexto.SeleccionarPorId<LineaDeUnaVentaDtm>(idLineaDeVenta, usarLaCache: false);
            var venta = contexto.SeleccionarPorId<VentaDtm>(linea.IdElemento, usarLaCache: false);
            if (!venta.EstaEnLaEtapa(enumEtapasDeVenta.DSA_Etapa_Servicio))
                GestorDeErrores.Emitir($"Sólo se puede modificar lo servido de un {enumNegocio.Venta.Singular(true)} en la etapa de {enumEtapasDeVenta.DSA_Etapa_Servicio.Nombre(true)}, el '{venta.Referencia}' está en '{venta.Estado(contexto).Nombre}'");
            if (cantidad <= 0)
                GestorDeErrores.Emitir($"La cantidad servida ha de ser mayor que 0; para quitar la línea devuelva el {enumNegocio.Venta.Singular(true)} a solicitado");
            if (precio < 0)
                GestorDeErrores.Emitir("El precio de venta no puede ser negativo");

            var devuelta = linea.CantidadDevuelta(contexto);
            var servida = linea.Cantidad - devuelta;
            if (cantidad == servida && precio == linea.Precio)
                GestorDeErrores.Emitir($"No ha cambiado ni la cantidad servida ni el precio de venta de la línea {linea.Orden}");

            var salida = contexto.Set<MovimientoDeAlmacenDtm>().FirstOrDefault(m => m.IdLineaDeVenta == linea.Id);
            if (salida is null)
                GestorDeErrores.Emitir($"La línea {linea.Orden} del {enumNegocio.Venta.Singular(true)} '{venta.Referencia}' no tiene salida del almacén");

            var contabilizado = salida.PreasientoContabilizado(contexto);
            var periodoCerrado = contabilizado is not null
                || ExtensorDeDevolucionesDeVenta.FechaTrasElCierre(contexto, venta, salida.RealizadoEl) != salida.RealizadoEl.Date;
            if (periodoCerrado && cantidad > servida)
                GestorDeErrores.Emitir($"La salida del almacén '{salida.Referencia(contexto)}' de la línea {linea.Orden} está en un periodo cerrado" +
                    (contabilizado is null ? "" : $" (su preasiento '{contabilizado.Referencia}' está contabilizado)") + ", no se puede aumentar lo servido");

            var resultado = new ResultadoDeModificarLoServido { Venta = venta };
            var antes = $"{servida:0.####} a {linea.Precio:N2}";
            var detalles = new List<string>();

            // la línea: en un periodo abierto la cantidad y el precio, en uno cerrado sólo el precio
            var cambiaLaSalida = !periodoCerrado && cantidad != servida;
            if (cambiaLaSalida || precio != linea.Precio)
            {
                if (cambiaLaSalida)
                    linea.Cantidad = cantidad + devuelta;
                linea.Precio = precio;
                linea.Modificar(contexto, esUnaAccion: true);
                resultado.LineaModificada = true;
            }

            if (cambiaLaSalida)
            {
                var preasientoAnterior = salida.IdPreasiento;
                salida.Cantidad = linea.Cantidad;
                salida.ModificarComoAdministrador(contexto);
                salida = contexto.SeleccionarPorId<MovimientoDeAlmacenDtm>(salida.Id, usarLaCache: false);
                detalles.Add($"Salida del almacén '{salida.Referencia(contexto)}': salen {salida.Cantidad:0.####} valoradas a {salida.Precio:N4}" +
                    (preasientoAnterior is null && salida.IdPreasiento is null ? "" : $", preasiento {(salida.IdPreasiento is null ? "cancelado" : "regenerado")}"));
            }

            // en un periodo cerrado lo que se deja de servir se devuelve (después de cambiar el precio: la devolución toma el de la línea)
            if (periodoCerrado && cantidad < servida)
            {
                resultado.Devolucion = ExtensorDeDevolucionesDeVenta.DevolverLineaDeVenta(contexto, linea.Id, servida - cantidad, idDevolucion: 0);
                detalles.Add($"La salida del almacén '{salida.Referencia(contexto)}' está en un periodo cerrado: se devuelven {servida - cantidad:0.####} en la {enumNegocio.DevolucionVenta.Singular(true)} '{resultado.Devolucion.Referencia}', pendiente de aceptar");
            }

            venta.CrearTraza(contexto, "Línea servida modificada", $"{contexto.Usuario.Expresion} ha modificado lo servido de la línea {linea.Orden}: de {antes} a {cantidad:0.####} a {precio:N2}" +
                (detalles.Count == 0 ? "" : $"{Environment.NewLine}{string.Join(Environment.NewLine, detalles)}"));
            return resultado;
        }

        /// <summary>
        /// DSA: Sirviendo --> DSA: Facturar --> DSA: Facturado
        /// </summary>
        public static void AntesDeFacturar(this VentaDtm venta, ContextoSe contexto)
        {
            if (venta.IdFacturaEmt is not null)
                GestorDeErrores.Emitir($"El {enumNegocio.Venta.Singular(true)} '{venta.Referencia}' ya tiene una factura asociada");
            if (venta.Detalles<LineaDeUnaVentaDtm>(contexto).Count == 0)
                GestorDeErrores.Emitir($"El {enumNegocio.Venta.Singular(true)} '{venta.Referencia}' no tiene líneas, no se puede facturar");
            venta.TipoDeVenta(contexto).TipoDeFacturaEmt(contexto);
        }

        /// <summary>
        /// DSA: Sirviendo --> DSA: Facturar --> DSA: Facturado: crea la prefactura al cliente del documento de venta con una línea de tipo
        /// unitario por cada línea de la venta (el material, su concepto, naturaleza y unidad, la cantidad, el precio y el descuento vendidos,
        /// y el iva por defecto del tipo de factura). Como la línea apunta a la vendida (IdLineaDeVenta), su precio es el de la venta y no el
        /// del maestro (ver GestorDeLineasDeUnaFae.ValidarDatosSegunTipoDeLinea). El tipo de la prefactura es el indicado en el tipo de
        /// documento de venta. Asigna IdFacturaEmt
        /// </summary>
        public static FacturaEmtDtm CrearPrefactura(this VentaDtm venta, ContextoSe contexto)
        {
            var tipo = venta.TipoDeVenta(contexto).TipoDeFacturaEmt(contexto);
            var lineas = venta.Detalles<LineaDeUnaVentaDtm>(contexto);
            if (lineas.Count == 0)
                GestorDeErrores.Emitir($"El {enumNegocio.Venta.Singular(true)} '{venta.Referencia}' no tiene líneas, no se puede crear la prefactura");

            var factura = new FacturaEmtDtm
            {
                IdCg = venta.IdCg,
                IdTipo = tipo.Id,
                Nombre = $"Factura correspondiente al {enumNegocio.Venta.Singular(true)} '{venta.Referencia}'".Left(IDominio.Longitud(IDominio.VARCHAR_250) - 1),
                Descripcion = venta.Descripcion,
                IdCliente = venta.IdCliente,
                Contacto = venta.Contacto,
                Telefono = venta.Telefono,
                eMail = venta.eMail,
                IdPresupuesto = null,
                IdContrato = null,
                IdParteTr = null,
                Ano = null,
                Serie = null,
                ClaseRectificativa = null,
                MotivoDeRectificacion = null
            }.Insertar(contexto);

            // se factura lo que realmente se vende: lo servido menos lo devuelto en devoluciones aceptadas (p.e. al modificar lo servido con la
            // salida en un periodo cerrado, ver ModificarLineaServida); lo devuelto por completo no se factura
            decimal? porcentajeDeIva = tipo.IdIvaRDefecto is null ? null : contexto.SeleccionarPorId<IvaRepercutidoDtm>((int)tipo.IdIvaRDefecto).Porcentaje;
            foreach (var linea in lineas.OrderBy(l => l.Orden))
            {
                var cantidad = linea.Cantidad - linea.CantidadDevueltaAceptada(contexto);
                if (cantidad <= 0)
                    continue;
                var unitario = contexto.SeleccionarPorId<UnitarioDtm>(linea.IdUnitario);
                new LineaDeUnaFaeDtm
                {
                    IdElemento = factura.Id,
                    Orden = linea.Orden,
                    TipoDeLinea = enumTipoDeLinea.Unitario,
                    IdUnitario = unitario.Id,
                    Concepto = unitario.Expresion,
                    Cantidad = cantidad,
                    Precio = linea.Precio,
                    Descuento = linea.Descuento,
                    IdIvaR = tipo.IdIvaRDefecto,
                    Iva = porcentajeDeIva,
                    IdUnidad = unitario.IdUnidad,
                    IdNaturaleza = unitario.IdNaturaleza,
                    IdLineaDeVenta = linea.Id
                }.Insertar(contexto);
            }

            venta.IdFacturaEmt = factura.Id;
            venta.FacturaEmt = null;
            venta.Modificar(contexto, esUnaAccion: true);
            venta.CrearTraza(contexto, "Prefactura creada", $"{contexto.Usuario.Expresion} ha creado la prefactura '{factura.Referencia}' del {enumNegocio.Venta.Singular(true)} con {lineas.Count} líneas");
            return factura;
        }

        /// <summary>
        /// DSA: Facturado --> DSA: Cancelar prefactura --> DSA: Sirviendo (del sistema)
        /// Se llama desde ExtensorDeFacturasEmt.AntesDeAnular al anular la prefactura a la que apunta el documento de venta (ver AlCancelarPrefactura)
        /// </summary>
        public static VentaDtm CancelarPrefactura(this VentaDtm venta, ContextoSe contexto)
        {
            return venta.TransitarALaEtapa(contexto, enumEtapasDeVenta.DSA_Etapa_Servicio.EstadosDeLaEtapa(), delSistema: true);
        }

        // Facturado --> Sirviendo, antes de transitar: se desvincula la prefactura (se guarda con la transición), se anota en la traza y la
        // transición queda anotada con una observación de asunto "Al cancelar prefactura {referencia}" y de cuerpo el enlace a la prefactura
        // (GestorDeHitos.Transitar la crea con el asunto y el detalle que se dejan en los parámetros de la transición)
        public static void AlCancelarPrefactura(this VentaDtm venta, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            if (venta.IdFacturaEmt is null)
                return;

            var prefactura = contexto.SeleccionarPorId<FacturaEmtDtm>((int)venta.IdFacturaEmt);
            venta.IdFacturaEmt = null;
            venta.FacturaEmt = null;
            venta.CrearTraza(contexto, "Prefactura cancelada", $"{contexto.Usuario.Expresion} ha cancelado la prefactura '{prefactura.Referencia}' del {enumNegocio.Venta.Singular(true)}, que vuelve a la etapa de servicio");

            parametros[ltrParametrosEp.asunto] = $"Al cancelar prefactura {prefactura.Referencia}";
            parametros[ltrParametrosEp.detalleAsunto] = enumNegocio.FacturaEmitida.ComponerUrlPorId(contexto, prefactura.Id).ToString();
        }

        /// <summary>
        /// DSA: Solicitado --> DSA: Cancelar --> DSA: Cancelado
        /// </summary>
        public static void AntesDeCancelar(this VentaDtm venta, ContextoSe contexto)
        {
            // TODO: validaciones previas a la cancelación
        }
    }
}
