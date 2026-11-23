using Gestor.Errores;
using ModeloDeDto.Logistica;
using ModeloDeDto.Ventas;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Logistica;
using ServicioDeDatos.MaestrosTecnico;
using ServicioDeDatos.Ventas;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilidades;
using static ServicioDeDatos.Elemento.Enumerados;

namespace GestorDeElementos.Extensores
{
    // Devoluciones de venta: lo que el cliente devuelve de las líneas de los documentos de venta. Mientras se tramitan se añaden líneas;
    // al aceptarlas se mete en el almacén lo devuelto (con su preasiento y el barrido de los posteriores) y se regularizan las facturas de
    // las ventas afectadas: a una prefactura se le quita lo devuelto; una factura emitida se rectifica por completo y se sustituye
    // por una prefactura nueva sin lo devuelto (ver RegularizarFacturas). Nunca se emite ninguna factura
    public static class ExtensorDeDevolucionesDeVenta
    {
        public static int IncrementarOrdenEn(ContextoSe contexto)
        {
            var incremento = enumNegocio.DevolucionVenta.LeerCrearParametro(contexto, enumParametrosDeDevolucionesDeVenta.DDV_IncrementarOrdenEn, "10");
            if (incremento is null || incremento.Valor.Entero() == 0)
                GestorDeErrores.Emitir($"Ha de definir el parámetro '{enumParametrosDeDevolucionesDeVenta.DDV_IncrementarOrdenEn}' del negocio de '{enumNegocio.DevolucionVenta.Singular()}' con un valor mayor de 0");
            return incremento.Valor.Entero();
        }

        // Orden de la siguiente línea de la devolución: el último más el incremento del parámetro
        public static int SiguienteOrden(this DevolucionDeVentaDtm devolucion, ContextoSe contexto)
        {
            var ultimo = devolucion.Detalles<LineaDeUnaDevolucionDeVentaDtm>(contexto).Max(l => (int?)l.Orden) ?? 0;
            return ultimo + IncrementarOrdenEn(contexto);
        }

        // Importe devuelto sin iva: suma de los importes de sus líneas (el importe no se guarda, se calcula en memoria)
        public static decimal TotalSinIva(this DevolucionDeVentaDtm devolucion, ContextoSe contexto)
            => Math.Round(devolucion.Detalles<LineaDeUnaDevolucionDeVentaDtm>(contexto).Sum(l => l.Importe), 2);

        public static LineaDeUnaVentaDtm LineaDeVenta(this LineaDeUnaDevolucionDeVentaDtm linea, ContextoSe contexto)
        {
            if (linea.LineaDeVenta is not null && linea.IdLineaDeVenta == linea.LineaDeVenta.Id)
                return linea.LineaDeVenta;

            return linea.LineaDeVenta = contexto.SeleccionarPorId<LineaDeUnaVentaDtm>(linea.IdLineaDeVenta);
        }

        public static List<LineaDeUnaDevolucionDeVentaDtm> Lineas(this DevolucionDeVentaDtm devolucion, ContextoSe contexto)
            => devolucion.Detalles<LineaDeUnaDevolucionDeVentaDtm>(contexto).OrderBy(l => l.Orden).ToList();

        //---------------------------------------------------------------------------------------------------------------------------
        // Lo devuelto de una línea vendida: las líneas de las devoluciones, se estén tramitando o estén aceptadas (no las canceladas)
        //---------------------------------------------------------------------------------------------------------------------------

        public static decimal CantidadDevuelta(this LineaDeUnaVentaDtm lineaDeVenta, ContextoSe contexto, int idLineaExcluida = 0)
        {
            var canceladas = enumEtapasDeDevolucionDeVenta.DDV_Etapa_Cancelada.Lista();
            return contexto.Set<LineaDeUnaDevolucionDeVentaDtm>()
                .Where(l => l.IdLineaDeVenta == lineaDeVenta.Id && l.Id != idLineaExcluida
                         && !contexto.Set<DevolucionDeVentaDtm>().Any(d => d.Id == l.IdElemento && canceladas.Contains(d.IdEstado)))
                .Sum(l => (decimal?)l.Cantidad) ?? 0m;
        }

        // Lo devuelto de una línea vendida en devoluciones ya aceptadas (lo que se descuenta al facturar, ver ExtensorDeVentas.CrearPrefactura)
        public static decimal CantidadDevueltaAceptada(this LineaDeUnaVentaDtm lineaDeVenta, ContextoSe contexto)
        {
            var aceptadas = enumEtapasDeDevolucionDeVenta.DDV_Etapa_Aceptada.Lista();
            return contexto.Set<LineaDeUnaDevolucionDeVentaDtm>()
                .Where(l => l.IdLineaDeVenta == lineaDeVenta.Id && contexto.Set<DevolucionDeVentaDtm>().Any(d => d.Id == l.IdElemento && aceptadas.Contains(d.IdEstado)))
                .Sum(l => (decimal?)l.Cantidad) ?? 0m;
        }

        // Lo que aún se puede devolver de una línea vendida: lo vendido menos lo ya devuelto
        public static decimal CantidadPendienteDeDevolver(this LineaDeUnaVentaDtm lineaDeVenta, ContextoSe contexto, int idLineaExcluida = 0)
            => lineaDeVenta.Cantidad - lineaDeVenta.CantidadDevuelta(contexto, idLineaExcluida);

        // Sólo se devuelve de documentos de venta servidos o facturados
        public static bool SePuedeDevolver(this VentaDtm venta)
            => venta.EstaEnLaEtapa(enumEtapasDeVenta.DSA_Etapa_Servicio) || venta.EstaEnLaEtapa(enumEtapasDeVenta.DSA_Etapa_Facturacion);

        // Al guardar una línea (ver GestorDeLineasDeUnaDevolucionDeVenta.AntesDePersistir): la línea vendida es de un documento de venta servido
        // o facturado del cliente de la devolución, el precio y el descuento son los de la venta, y nunca se devuelve más de lo que queda
        // por devolver
        public static void ValidarLinea(this LineaDeUnaDevolucionDeVentaDtm linea, ContextoSe contexto, DevolucionDeVentaDtm devolucion)
        {
            if (linea.IdLineaDeVenta <= 0)
                GestorDeErrores.Emitir($"Ha de indicar la línea vendida que se devuelve");

            var lineaDeVenta = contexto.SeleccionarPorId<LineaDeUnaVentaDtm>(linea.IdLineaDeVenta, usarLaCache: false);
            var venta = contexto.SeleccionarPorId<VentaDtm>(lineaDeVenta.IdElemento, usarLaCache: false);
            if (!venta.SePuedeDevolver())
                GestorDeErrores.Emitir($"El {enumNegocio.Venta.Singular(true)} '{venta.Referencia}' no está {enumEtapasDeVenta.DSA_Etapa_Servicio.Nombre()} ni {enumEtapasDeVenta.DSA_Etapa_Facturacion.Nombre()}, no se puede devolver");
            if (venta.IdCliente != devolucion.IdCliente)
                GestorDeErrores.Emitir($"El {enumNegocio.Venta.Singular(true)} '{venta.Referencia}' es de otro cliente, no se puede devolver en la {enumNegocio.DevolucionVenta.Singular(true)} '{devolucion.Referencia}'");

            linea.Precio = lineaDeVenta.Precio;
            linea.Descuento = lineaDeVenta.Descuento;

            if (linea.Cantidad <= 0)
                GestorDeErrores.Emitir($"Ha de indicar la cantidad devuelta de la línea {linea.Orden}");

            var pendiente = lineaDeVenta.CantidadPendienteDeDevolver(contexto, linea.Id);
            if (pendiente <= 0)
                GestorDeErrores.Emitir($"La línea {lineaDeVenta.Orden} del {enumNegocio.Venta.Singular(true)} '{venta.Referencia}' ya se ha devuelto por completo");
            if (linea.Cantidad > pendiente)
                GestorDeErrores.Emitir($"No se pueden devolver {linea.Cantidad:0.####} de la línea {lineaDeVenta.Orden} del {enumNegocio.Venta.Singular(true)} '{venta.Referencia}': se vendieron {lineaDeVenta.Cantidad:0.####} y ya se han devuelto {lineaDeVenta.CantidadDevuelta(contexto, linea.Id):0.####}, como mucho se pueden devolver {pendiente:0.####}");
        }

        //---------------------------------------------------------------------------------------------------------------------------
        // DDV: En tramitación --> DDV: Aceptar --> DDV: Aceptada
        //---------------------------------------------------------------------------------------------------------------------------

        // Antes de transitar: tiene líneas y siguen siendo válidas, la fecha (la de DevueltaEl o la de hoy, se guarda con la transición) no
        // es posterior a hoy ni anterior a cuando se sirvieron las ventas, y las facturas de las ventas afectadas se pueden regularizar (ver
        // RegularizarFacturas). Si la fecha cae en un periodo ya cerrado (en o antes del último cierre contable de la sociedad) la devolución
        // se hace el día siguiente al cierre: sus entradas en el almacén y sus preasientos van al periodo abierto
        public static void AntesDeAceptar(this DevolucionDeVentaDtm devolucion, ContextoSe contexto)
        {
            var lineas = devolucion.Lineas(contexto);
            if (lineas.Count == 0)
                GestorDeErrores.Emitir($"La {enumNegocio.DevolucionVenta.Singular(true)} '{devolucion.Referencia}' no tiene líneas, no se puede aceptar");

            var fecha = (devolucion.DevueltaEl ?? DateTime.Today).Date;
            if (fecha > DateTime.Today)
                GestorDeErrores.Emitir($"La fecha de la devolución ({fecha:dd/MM/yyyy}) no puede ser posterior a hoy");

            foreach (var linea in lineas)
            {
                var venta = linea.LineaDeVenta(contexto).Venta(contexto);
                fecha = FechaTrasElCierre(contexto, venta, fecha);
            }

            foreach (var linea in lineas)
            {
                linea.ValidarLinea(contexto, devolucion);
                var venta = linea.LineaDeVenta(contexto).Venta(contexto);
                if (venta.ServidoEl is not null && fecha < ((DateTime)venta.ServidoEl).Date)
                    GestorDeErrores.Emitir($"La fecha de la devolución ({fecha:dd/MM/yyyy}) no puede ser anterior a la del servicio del {enumNegocio.Venta.Singular(true)} '{venta.Referencia}' ({venta.ServidoEl:dd/MM/yyyy})");
            }

            foreach (var (factura, devueltas) in PorFactura(contexto, lineas))
                ValidarFactura(contexto, factura, devueltas);

            devolucion.DevueltaEl = fecha;
        }

        // La fecha, o el día siguiente al último cierre contable de la sociedad del almacén de la venta si cae en un periodo cerrado
        public static DateTime FechaTrasElCierre(ContextoSe contexto, VentaDtm venta, DateTime fecha)
        {
            var cierre = ExtensorDeSociedades.UltimoCierreContable(contexto, venta.Almacen(contexto).Sociedad(contexto).Id);
            return cierre is not null && fecha.Date <= cierre.CerradaEl.Date ? cierre.CerradaEl.Date.AddDays(1) : fecha.Date;
        }

        // Después de transitar: entradas en el almacén y regularización de las facturas (la impresión la hace GestorDeDevolucionesDeVenta)
        public static List<FacturaEmtDtm> AlAceptar(this DevolucionDeVentaDtm devolucion, ContextoSe contexto)
        {
            devolucion.CrearEntradasEnElAlmacen(contexto);
            return devolucion.RegularizarFacturas(contexto);
        }

        // Una entrada en el almacén de la venta por cada línea, a la fecha de la devolución y valorada al precio al que salió (el de la salida
        // de la venta). El gestor de movimientos calcula el inventario, la preasienta (al revés que la salida, ver
        // ExtensorDePreasientos.PreasentarEntradaDeAlmacen) y barre los movimientos posteriores
        public static void CrearEntradasEnElAlmacen(this DevolucionDeVentaDtm devolucion, ContextoSe contexto)
        {
            var fecha = (devolucion.DevueltaEl ?? DateTime.Today).Date;
            var idTipoDeMovimiento = enumTiposDeMovimiento.DevVenta.LeerTipoDeMovimiento(contexto).Id;
            var entradas = new List<string>();
            foreach (var linea in devolucion.Lineas(contexto))
            {
                if (contexto.Set<MovimientoDeAlmacenDtm>().Any(m => m.IdLineaDeDevolucionDeVenta == linea.Id))
                    continue;

                var lineaDeVenta = linea.LineaDeVenta(contexto);
                var venta = lineaDeVenta.Venta(contexto);
                var salida = contexto.Set<MovimientoDeAlmacenDtm>().FirstOrDefault(m => m.IdLineaDeVenta == lineaDeVenta.Id);
                if (salida is null)
                    GestorDeErrores.Emitir($"La línea {lineaDeVenta.Orden} del {enumNegocio.Venta.Singular(true)} '{venta.Referencia}' no tiene salida del almacén, no se puede devolver al almacén");

                var movimiento = new MovimientoDeAlmacenDtm
                {
                    IdAlmacen = venta.IdAlmacen,
                    IdUnitario = lineaDeVenta.IdUnitario,
                    IdTipoMovimiento = idTipoDeMovimiento,
                    Cantidad = linea.Cantidad,
                    Precio = salida.Precio,
                    RealizadoEl = fecha,
                    IdLineaDeDevolucionDeVenta = linea.Id
                }.InsertarComoAdministrador(contexto);
                entradas.Add($"Línea {linea.Orden}: {movimiento.Referencia(contexto)} en el almacén '{venta.Almacen(contexto).Expresion}', cantidad {linea.Cantidad:0.####}, valorada a {movimiento.Precio:N4}");
            }

            if (entradas.Count > 0)
                devolucion.CrearTraza(contexto, "Entradas en el almacén", $"Al aceptar, {contexto.Usuario.Expresion} ha dado entrada el {fecha:dd/MM/yyyy}:{Environment.NewLine}{string.Join(Environment.NewLine, entradas)}");
        }

        //---------------------------------------------------------------------------------------------------------------------------
        // Regularización de las facturas de las ventas devueltas. Nunca se emite nada: sólo se crean prefacturas
        //  - si la factura aún es una prefactura se le quita lo devuelto (ver AjustarPrefactura): el usuario no puede cambiar las líneas que
        //    facturan líneas vendidas; al emitirla se sigue validando que su base imponible sea lo vendido menos lo devuelto (ver
        //    ExtensorDeVentas.ValidarImporteDeLasVentas)
        //  - si está emitida se rectifica por completo (rectificativa total, por sustitución) y se crea una prefactura nueva con lo que no se
        //    devuelve, a la que pasan a apuntar los documentos de venta: así una devolución posterior regulariza la nueva, que no está
        //    rectificada. No se hacen rectificativas por diferencias
        //---------------------------------------------------------------------------------------------------------------------------

        // Cantidad que queda facturada en una línea de la factura tras la devolución
        private class LineaRegularizada
        {
            public LineaDeUnaFaeDtm Linea { get; set; }
            public decimal Cantidad { get; set; }
            public bool Devuelta { get; set; }
            // la línea de la devolución que la regulariza (la última, si son varias)
            public int? IdLineaDevuelta { get; set; }
        }

        // Las líneas devueltas agrupadas por la factura a la que apunta su documento de venta (las de ventas sin facturar no se regularizan)
        private static List<(FacturaEmtDtm Factura, List<LineaDeUnaDevolucionDeVentaDtm> Lineas)> PorFactura(ContextoSe contexto, List<LineaDeUnaDevolucionDeVentaDtm> lineas)
            => lineas.GroupBy(l => l.LineaDeVenta(contexto).Venta(contexto).IdFacturaEmt)
                .Where(g => g.Key is not null)
                .Select(g => (contexto.SeleccionarPorId<FacturaEmtDtm>((int)g.Key, usarLaCache: false), g.OrderBy(l => l.Orden).ToList()))
                .ToList();

        // Lo que queda en cada línea de la factura: lo facturado menos lo devuelto de la línea vendida que factura (IdLineaDeVenta, que
        // rellena ExtensorDeVentas.CrearPrefactura y se copia al rectificar y sustituir). Si se van a modificar o copiar las líneas se leen
        // sin la caché, para no tocar los objetos cacheados
        private static List<LineaRegularizada> LineasTrasDevolver(ContextoSe contexto, FacturaEmtDtm factura, List<LineaDeUnaDevolucionDeVentaDtm> lineasDevueltas, bool paraModificar = false)
        {
            var lineas = factura.Detalles<LineaDeUnaFaeDtm>(contexto, usarLaCache: !paraModificar)
                .OrderBy(l => l.Orden)
                .Select(l => new LineaRegularizada { Linea = l, Cantidad = l.Cantidad ?? 0m })
                .ToList();

            foreach (var devuelta in lineasDevueltas)
            {
                var lineaDeVenta = devuelta.LineaDeVenta(contexto);
                var facturada = lineas.FirstOrDefault(l => l.Linea.IdLineaDeVenta == lineaDeVenta.Id);
                if (facturada is null)
                    GestorDeErrores.Emitir($"La factura '{factura.Referencia}' no tiene la línea {lineaDeVenta.Orden} del {enumNegocio.Venta.Singular(true)} '{lineaDeVenta.Venta(contexto).Referencia}', no se puede regularizar la devolución de su línea {devuelta.Orden}");

                facturada.Cantidad -= devuelta.Cantidad;
                facturada.Devuelta = true;
                facturada.IdLineaDevuelta = devuelta.Id;
                if (facturada.Cantidad < 0)
                    GestorDeErrores.Emitir($"En la factura '{factura.Referencia}' la línea {facturada.Linea.Orden} tiene {facturada.Linea.Cantidad:0.####}, no se pueden devolver {facturada.Linea.Cantidad - facturada.Cantidad:0.####}");
            }

            return lineas;
        }

        private static bool SeDevuelveTodo(List<LineaRegularizada> lineas)
            => lineas.Where(l => l.Linea.Cantidad is not null).All(l => l.Cantidad == 0);

        // Una prefactura ha de tener lo que se devuelve (se ajusta al aceptar). Una factura ha de estar emitida sin rectificar, no puede ser una
        // rectificativa y ha de tener lo que se devuelve
        private static void ValidarFactura(ContextoSe contexto, FacturaEmtDtm factura, List<LineaDeUnaDevolucionDeVentaDtm> lineasDevueltas)
        {
            if (factura.EstaEnLaEtapa(enumEtapasDeFacturasEmt.FAE_Etapa_Prefactura))
            {
                // se ajustará al aceptar: ha de tener lo que se devuelve
                LineasTrasDevolver(contexto, factura, lineasDevueltas);
                return;
            }

            if (factura.EsRectificativa)
                GestorDeErrores.Emitir($"La factura '{factura.Referencia}' es rectificativa, no se puede regularizar con la devolución");
            if (factura.FacturadaEl is null || factura.NumeroDeFactura.IsNullOrEmpty())
                GestorDeErrores.Emitir($"La factura '{factura.Referencia}' no es una prefactura ni está emitida ('{factura.Estado(contexto).Nombre}'), no se puede regularizar con la devolución");
            if (factura.EstaRectificada(contexto))
                GestorDeErrores.Emitir($"La factura '{factura.NumeroDeFactura}' ya está rectificada por '{factura.RectificadaPor(contexto).Referencia}', no se puede volver a rectificar");

            LineasTrasDevolver(contexto, factura, lineasDevueltas);
        }

        // Devuelve las prefacturas creadas (rectificativas y de sustitución)
        public static List<FacturaEmtDtm> RegularizarFacturas(this DevolucionDeVentaDtm devolucion, ContextoSe contexto)
        {
            var creadas = new List<FacturaEmtDtm>();
            var hecho = new List<string>();
            var lineas = devolucion.Lineas(contexto);
            foreach (var (factura, devueltas) in PorFactura(contexto, lineas))
            {
                if (factura.EstaEnLaEtapa(enumEtapasDeFacturasEmt.FAE_Etapa_Prefactura))
                    hecho.Add(devolucion.AjustarPrefactura(contexto, factura, devueltas));
                else
                    hecho.Add(devolucion.SustituirFactura(contexto, factura, devueltas, creadas));
            }

            if (hecho.Count > 0)
                devolucion.CrearTraza(contexto, "Facturas regularizadas", $"Al aceptar, {contexto.Usuario.Expresion} ha regularizado:{Environment.NewLine}{string.Join(Environment.NewLine, hecho)}");

            var sinFacturar = lineas.Where(l => l.LineaDeVenta(contexto).Venta(contexto).IdFacturaEmt is null).ToList();
            if (sinFacturar.Count > 0)
                devolucion.CrearTraza(contexto, "Líneas sin factura", $"Las líneas {string.Join(", ", sinFacturar.Select(l => l.Orden))} son de documentos de venta sin facturar, no hay factura que regularizar");

            return creadas;
        }

        // La factura es una prefactura (aún no se ha comunicado a Hacienda): se le quita lo devuelto. Cada línea que factura una línea devuelta
        // se queda con lo que no se devuelve, o se borra si se devuelve todo, y anota la línea de la devolución que la ajusta. Lo hace el
        // sistema (es una acción): el usuario no puede cambiar estas líneas (ver GestorDeLineasDeUnaFae.AntesDePersistir)
        private static string AjustarPrefactura(this DevolucionDeVentaDtm devolucion, ContextoSe contexto, FacturaEmtDtm prefactura, List<LineaDeUnaDevolucionDeVentaDtm> devueltas)
        {
            var ajustes = new List<string>();
            foreach (var linea in LineasTrasDevolver(contexto, prefactura, devueltas, paraModificar: true).Where(l => l.Devuelta))
            {
                var facturada = linea.Linea;
                var antes = facturada.Cantidad ?? 0m;
                if (linea.Cantidad == 0)
                {
                    facturada.EliminarComoAdministrador(contexto, accionEjecutada: ltrDeUnaDevolucionDeVenta.Accion_AjustarPrefactura);
                    ajustes.Add($"línea {facturada.Orden} borrada (se devuelve todo: {antes:0.####})");
                    continue;
                }

                facturada.Cantidad = linea.Cantidad;
                facturada.IdLineaDeDevolucionDeVenta = linea.IdLineaDevuelta;
                facturada.ModificarComoAdministrador(contexto, accionQueSeEjecuta: ltrDeUnaDevolucionDeVenta.Accion_AjustarPrefactura);
                ajustes.Add($"línea {facturada.Orden}: {antes:0.####} --> {linea.Cantidad:0.####}");
            }

            var resultado = $"Prefactura '{prefactura.Referencia}' ajustada: {string.Join(", ", ajustes)}";
            prefactura.CrearTraza(contexto, "Prefactura ajustada", $"Al aceptar la {enumNegocio.DevolucionVenta.Singular(true)} '{devolucion.Referencia}', {contexto.Usuario.Expresion} ha quitado lo devuelto: {string.Join(", ", ajustes)}");
            return resultado;
        }

        // La factura está emitida: rectificativa total (ver ExtensorDeFacturasEmt.CrearRectificativa) y, si no se devuelve todo, una
        // prefactura nueva con lo que queda, a la que pasan a apuntar los documentos de venta de la factura
        private static string SustituirFactura(this DevolucionDeVentaDtm devolucion, ContextoSe contexto, FacturaEmtDtm factura, List<LineaDeUnaDevolucionDeVentaDtm> devueltas, List<FacturaEmtDtm> creadas)
        {
            var lineas = LineasTrasDevolver(contexto, factura, devueltas, paraModificar: true);

            var rectificativa = factura.CrearRectificativa(contexto, new Dictionary<string, object>
            {
                { nameof(HacerRectificativaDto.ClaseRectificativa), enumClaseDeRectificativa.OR.ToString() },
                { nameof(HacerRectificativaDto.Motivo), enumMotivoDeRectificacion.PorImportes.ToString() },
                { nameof(HacerRectificativaDto.MotivoDeRectificacion), $"{enumNegocio.DevolucionVenta.Singular(true)} '{devolucion.Referencia}'" }
            });
            creadas.Add(rectificativa);
            rectificativa.CrearTraza(contexto, "Prefactura rectificativa", $"{contexto.Usuario.Expresion} ha aceptado la {enumNegocio.DevolucionVenta.Singular(true)} '{devolucion.Referencia}', que rectifica por completo la factura '{factura.NumeroDeFactura}'");
            var resultado = $"Factura '{factura.NumeroDeFactura}': prefactura rectificativa total '{rectificativa.Referencia}'";

            if (SeDevuelveTodo(lineas))
                return $"{resultado}; se ha devuelto todo lo facturado, no hay prefactura de sustitución";

            var sustituta = new FacturaEmtDtm
            {
                IdCg = factura.IdCg,
                IdTipo = factura.IdTipo,
                Nombre = $"Sustituye a la factura {factura.NumeroDeFactura} por la {enumNegocio.DevolucionVenta.Singular(true)} '{devolucion.Referencia}'".Left(IDominio.Longitud(IDominio.VARCHAR_250) - 1),
                Descripcion = factura.Descripcion,
                IdCliente = factura.IdCliente,
                Contacto = factura.Contacto,
                Telefono = factura.Telefono,
                eMail = factura.eMail,
                IdPresupuesto = factura.IdPresupuesto,
                IdContrato = factura.IdContrato,
                IdParteTr = null,
                Ano = null,
                Serie = null,
                ClaseRectificativa = null,
                MotivoDeRectificacion = null
            }.Insertar(contexto);
            creadas.Add(sustituta);

            foreach (var linea in lineas)
            {
                // los comentarios (sin cantidad) se copian; lo devuelto por completo no
                if (linea.Linea.Cantidad is not null && linea.Cantidad == 0)
                    continue;
                var copia = linea.Linea;
                copia.Id = 0;
                copia.IdElemento = sustituta.Id;
                if (copia.Cantidad is not null)
                    copia.Cantidad = linea.Cantidad;
                // conserva la línea vendida (IdLineaDeVenta) y anota la devuelta que la reduce
                if (linea.Devuelta)
                    copia.IdLineaDeDevolucionDeVenta = linea.IdLineaDevuelta;
                copia.Insertar(contexto);
            }

            var aviso = factura.TienenIrpf(contexto) ? " La factura tenía irpf: revíselo en la prefactura de sustitución." : "";
            sustituta.CrearTraza(contexto, "Prefactura de sustitución", $"{contexto.Usuario.Expresion} ha aceptado la {enumNegocio.DevolucionVenta.Singular(true)} '{devolucion.Referencia}': sustituye a la factura '{factura.NumeroDeFactura}', rectificada por completo por '{rectificativa.Referencia}', sin lo devuelto.{aviso}");

            foreach (var venta in contexto.Set<VentaDtm>().Where(v => v.IdFacturaEmt == factura.Id).ToList())
            {
                venta.IdFacturaEmt = sustituta.Id;
                venta.FacturaEmt = null;
                venta.Modificar(contexto, esUnaAccion: true);
                venta.CrearTraza(contexto, "Prefactura de sustitución", $"Por la {enumNegocio.DevolucionVenta.Singular(true)} '{devolucion.Referencia}' la factura '{factura.NumeroDeFactura}' se ha rectificado por completo ('{rectificativa.Referencia}') y el {enumNegocio.Venta.Singular(true)} pasa a la prefactura '{sustituta.Referencia}'");
            }

            return $"{resultado} y prefactura de sustitución '{sustituta.Referencia}' con lo no devuelto.{aviso}";
        }

        //---------------------------------------------------------------------------------------------------------------------------
        // Devolver una línea vendida desde la edición del documento de venta (ver DevolucionesDeVentaController.epDevolverLineaDeVenta)
        //---------------------------------------------------------------------------------------------------------------------------

        // Tipo con el que se crean las devoluciones de un documento de venta: el indicado en su tipo de documento de venta (IdTipoDevolucion)
        private static TipoDeDevolucionDeVentaDtm TipoParaDevolver(ContextoSe contexto, VentaDtm venta)
        {
            var tipoDeVenta = venta.TipoDeVenta(contexto);
            var tipo = tipoDeVenta.IdTipoDevolucion is null
                ? null
                : contexto.Set<TipoDeDevolucionDeVentaDtm>().FirstOrDefault(t => t.Id == tipoDeVenta.IdTipoDevolucion);
            if (tipo is null)
                GestorDeErrores.Emitir($"El tipo de {enumNegocio.Venta.Singular(true)} '{tipoDeVenta.Nombre}' no tiene indicado el tipo de {enumNegocio.DevolucionVenta.Singular(true)} a usar: " +
                    $"parametrice en el tipo de {enumNegocio.Venta.Singular(true)} el tipo de {enumNegocio.DevolucionVenta.Singular(true)} con el que se han de crear sus devoluciones");
            return tipo;
        }

        // Añade la línea vendida a la devolución indicada, que ha de estar en tramitación; si no se indica, crea una devolución nueva para el
        // cliente de la venta. La venta ha de estar facturada (acción Devolver) o servida (al modificar lo servido con la salida en un periodo
        // cerrado, ver ExtensorDeVentas.ModificarLineaServida). Devuelve la devolución
        public static DevolucionDeVentaDtm DevolverLineaDeVenta(ContextoSe contexto, int idLineaDeVenta, decimal cantidad, int idDevolucion)
        {
            var lineaDeVenta = contexto.SeleccionarPorId<LineaDeUnaVentaDtm>(idLineaDeVenta, usarLaCache: false);
            var venta = contexto.SeleccionarPorId<VentaDtm>(lineaDeVenta.IdElemento, usarLaCache: false);
            if (!venta.SePuedeDevolver())
                GestorDeErrores.Emitir($"Sólo se pueden devolver las líneas de {enumNegocio.Venta.Plural(true)} servidos o facturados");

            DevolucionDeVentaDtm devolucion;
            if (idDevolucion > 0)
            {
                devolucion = contexto.SeleccionarPorId<DevolucionDeVentaDtm>(idDevolucion, usarLaCache: false);
                if (!devolucion.EstaEnLaEtapa(enumEtapasDeDevolucionDeVenta.DDV_Etapa_Tramitacion))
                    GestorDeErrores.Emitir($"La {enumNegocio.DevolucionVenta.Singular(true)} '{devolucion.Referencia}' no está {enumEtapasDeDevolucionDeVenta.DDV_Etapa_Tramitacion.Nombre()}, no se le pueden añadir líneas");
            }
            else
            {
                devolucion = new DevolucionDeVentaDtm
                {
                    IdCg = venta.IdCg,
                    IdTipo = TipoParaDevolver(contexto, venta).Id,
                    IdEstado = 0,
                    Nombre = $"Devolución del {enumNegocio.Venta.Singular(true)} '{venta.Referencia}'".Left(IDominio.Longitud(IDominio.VARCHAR_250) - 1),
                    IdCliente = venta.IdCliente,
                    Contacto = venta.Contacto,
                    Telefono = venta.Telefono,
                    eMail = venta.eMail
                }.Insertar(contexto);
            }

            var linea = new LineaDeUnaDevolucionDeVentaDtm
            {
                IdElemento = devolucion.Id,
                Orden = devolucion.SiguienteOrden(contexto),
                IdLineaDeVenta = lineaDeVenta.Id,
                Cantidad = cantidad,
                Precio = lineaDeVenta.Precio,
                Descuento = lineaDeVenta.Descuento
            }.Insertar(contexto);

            devolucion.CrearTraza(contexto, "Línea devuelta", $"{contexto.Usuario.Expresion} ha añadido la línea {linea.Orden}: {cantidad:0.####} de la línea {lineaDeVenta.Orden} del {enumNegocio.Venta.Singular(true)} '{venta.Referencia}'");
            return devolucion;
        }
    }
}
