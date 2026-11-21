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
                GestorDeErrores.Emitir($"No hay stock suficiente el {fecha:dd/MM/yyyy}: quedan {stock:0.####} y se quieren vender {cantidad:0.####}");
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
            => Math.Round(contexto.Set<LineaDeUnaVentaDtm>().Where(l => l.IdElemento == venta.Id).ToList().Sum(l => l.Importe), 2);

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
            if (!contexto.Set<LineaDeUnaVentaDtm>().Any(l => l.IdElemento == venta.Id))
                GestorDeErrores.Emitir($"El {enumNegocio.Venta.Singular(true)} '{venta.Referencia}' no tiene líneas, no se puede servir");
            venta.ServidoEl = (venta.ServidoEl ?? DateTime.Today).Date;
        }

        // Solicitado --> Sirviendo, después de transitar: una salida del almacén de la venta por cada línea, a la fecha en la que se sirve
        // y valorada al precio medio vigente. El gestor de movimientos valida el stock, la preasienta (salida de existencias si el almacén no
        // usa la variación, ver ExtensorDePreasientos.PreasentarSalidaDelmacen) y barre los movimientos posteriores
        public static void CrearSalidasDelAlmacen(this VentaDtm venta, ContextoSe contexto)
        {
            var fecha = (venta.ServidoEl ?? DateTime.Today).Date;
            var idTipoDeMovimiento = enumTiposDeMovimiento.Venta.LeerTipoDeMovimiento(contexto).Id;
            var lineas = contexto.SeleccionarTodos<LineaDeUnaVentaDtm>(nameof(LineaDeUnaVentaDtm.IdElemento), venta.Id);
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

        // Salidas del almacén de las líneas del documento de venta
        public static List<MovimientoDeAlmacenDtm> SalidasDelAlmacen(this VentaDtm venta, ContextoSe contexto)
        {
            var idsDeLineas = contexto.Set<LineaDeUnaVentaDtm>().Where(l => l.IdElemento == venta.Id).Select(l => l.Id).ToList();
            return contexto.Set<MovimientoDeAlmacenDtm>()
                .Where(m => m.IdLineaDeVenta != null && idsDeLineas.Contains((int)m.IdLineaDeVenta))
                .ToList();
        }

        /// <summary>
        /// DSA: Sirviendo --> DSA: Facturar --> DSA: Facturado
        /// </summary>
        public static void AntesDeFacturar(this VentaDtm venta, ContextoSe contexto)
        {
            if (venta.IdFacturaEmt is not null)
                GestorDeErrores.Emitir($"El {enumNegocio.Venta.Singular(true)} '{venta.Referencia}' ya tiene una factura asociada");
            if (!contexto.Set<LineaDeUnaVentaDtm>().Any(l => l.IdElemento == venta.Id))
                GestorDeErrores.Emitir($"El {enumNegocio.Venta.Singular(true)} '{venta.Referencia}' no tiene líneas, no se puede facturar");
            venta.TipoDeVenta(contexto).TipoDeFacturaEmt(contexto);
        }

        /// <summary>
        /// DSA: Sirviendo --> DSA: Facturar --> DSA: Facturado: crea la prefactura al cliente del documento de venta con una línea alzada por
        /// cada línea de la venta (concepto, naturaleza y unidad del material, su cantidad, precio y descuento, y el iva por defecto del tipo
        /// de factura); no es de tipo unitario porque ésta tomaría el precio de venta del material en lugar del vendido. El tipo de la
        /// prefactura es el indicado en el tipo de documento de venta. Asigna IdFacturaEmt
        /// </summary>
        public static FacturaEmtDtm CrearPrefactura(this VentaDtm venta, ContextoSe contexto)
        {
            var tipo = venta.TipoDeVenta(contexto).TipoDeFacturaEmt(contexto);
            var lineas = contexto.SeleccionarTodos<LineaDeUnaVentaDtm>(nameof(LineaDeUnaVentaDtm.IdElemento), venta.Id);
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

            decimal? porcentajeDeIva = tipo.IdIvaRDefecto is null ? null : contexto.SeleccionarPorId<IvaRepercutidoDtm>((int)tipo.IdIvaRDefecto).Porcentaje;
            foreach (var linea in lineas.OrderBy(l => l.Orden))
            {
                var unitario = contexto.SeleccionarPorId<UnitarioDtm>(linea.IdUnitario);
                new LineaDeUnaFaeDtm
                {
                    IdElemento = factura.Id,
                    Orden = linea.Orden,
                    TipoDeLinea = enumTipoDeLinea.Alzada,
                    IdUnitario = null,
                    Concepto = unitario.Expresion,
                    Cantidad = linea.Cantidad,
                    Precio = linea.Precio,
                    Descuento = linea.Descuento,
                    IdIvaR = tipo.IdIvaRDefecto,
                    Iva = porcentajeDeIva,
                    IdUnidad = unitario.IdUnidad,
                    IdNaturaleza = unitario.IdNaturaleza
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
