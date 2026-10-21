using Gestor.Errores;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Logistica;
using ServicioDeDatos.MaestrosTecnico;
using ServicioDeDatos.Seguridad;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilidades;
using static Gestor.Errores.GestorDeErrores;
using static ServicioDeDatos.Elemento.Enumerados;

namespace GestorDeElementos.Extensores
{
    // Recepciones de las líneas de los pedidos (RecepcionDeLineaDtm): entregas parciales, entrega completa del pedido,
    // cancelación de entregas, destino de las entregas y cachés de recepciones
    public static class ExtensorDeRecepciones
    {
        // Entrega registrada de una línea del pedido, null si aún no se ha entregado.
        // Se cachea también el null; se lee sin seguimiento porque la instancia cacheada se comparte entre peticiones
        public static RecepcionDeLineaDtm LeerRecepcion(this LineaDeUnPedidoDtm linea, ContextoSe contexto, bool erroSiNoHay)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Pedido_Recepcion);
            if (!cache.ContainsKey(linea.Id.ToString()))
            {
                var recepcion = contexto.Set<RecepcionDeLineaDtm>().FirstOrDefault(recepcion => recepcion.IdLinea == linea.Id);
                cache[linea.Id.ToString()] = recepcion;
            }

            if (cache[linea.Id.ToString()] is null && erroSiNoHay)
                Emitir($"La línea {linea.Orden} del pedido '{linea.Elemento.Referencia}' no tiene entrega registrada");

            return (RecepcionDeLineaDtm) cache[linea.Id.ToString()];
        }

        // Para enviar el pedido al proveedor ha de saberse dónde se entregará
        public static void ValidarDestinoDeLasEntregas(this PedidoDtm pedido)
        {
            if (pedido.IdAlmacen is null && pedido.IdExpediente is null)
                Emitir($"Para enviar el pedido '{pedido.Referencia}' al proveedor ha de indicar el almacén o el expediente al que se entregará");
        }

        // Si el pedido va a un almacén sólo puede llevar materiales del catálogo: todas sus líneas (salvo los comentarios) han de ser
        // unitarios de naturaleza material. Se valida al solicitarlo (ver ExtensorDePedidos.AntesDeSolicitar)
        public static void ValidarLineasParaElAlmacen(this PedidoDtm pedido, ContextoSe contexto)
        {
            if (pedido.IdAlmacen is null)
                return;

            var lineas = pedido.Detalles<LineaDeUnPedidoDtm>(contexto).Where(linea => linea.TipoDeLinea != enumTipoDeLinea.Comentario)
                .OrderBy(linea => linea.Orden)
                .ToList();
            var noValidas = lineas.Where(linea => linea.TipoDeLinea != enumTipoDeLinea.Unitario || linea.Clase(contexto) != enumClaseUnitario.Material)
                .Select(linea => linea.Orden.ToString())
                .ToList();
            if (noValidas.Count > 0)
                Emitir($"El pedido '{pedido.Referencia}' va a un almacén y sólo puede llevar materiales del catálogo; revise las líneas {string.Join(", ", noValidas)}");
        }

        // Al recibir o cancelar la entrega de una línea cambian su recepción y si el pedido tiene recepciones
        public static void OlvidarRecepcion(this LineaDeUnPedidoDtm linea) => OlvidarRecepcion(linea.Id, linea.IdElemento);

        public static void OlvidarRecepcion(int idLinea, int idPedido)
        {
            ServicioDeCaches.EliminarElemento(CacheDe.Pedido_Recepcion, idLinea.ToString());
            OlvidarRecepciones(idPedido);
        }

        public static void OlvidarRecepciones(int idPedido)
        {
            ServicioDeCaches.EliminarElemento(CacheDe.Pedido_HayRecepciones, idPedido.ToString());
        }

        // La entrega va al almacén o a la obra del expediente, nunca a ambos (ver CK_PEDIDO_LINEA_RECEPCION_DESTINO).
        // El pedido ha de estar en recepción (ver PasarARecepcion) y la fecha no puede ser posterior a hoy ni anterior a la creación del pedido
        // ni a la fecha en la que se pidió. Si es anterior a la fecha de recepción de la cabecera, ésta pasa a ser la de la entrega.
        // Sin gestor, la auditoría de creación se rellena aquí
        public static RecepcionDeLineaDtm Recibir(this LineaDeUnPedidoDtm linea, ContextoSe contexto, PedidoDtm pedido, DateTime recepcionadaEl, decimal cantidad, decimal precio, decimal? descuento, int? idAlmacen, int? idExpediente)
        {
            if (!pedido.EstaEnLaEtapa(enumEtapasDePedido.PED_Etapa_De_Recepcion))
                Emitir($"Para recepcionar la línea {linea.Orden} el pedido '{pedido.Referencia}' ha de estar {enumEtapasDePedido.PED_Etapa_De_Recepcion.Nombre()}");
            var recepcion = linea.RegistrarRecepcion(contexto, pedido, recepcionadaEl, cantidad, precio, descuento, idAlmacen, idExpediente);
            pedido.AdelantarLaFechaDeRecepcion(contexto, recepcion.RecepcionadaEl);
            return recepcion;
        }

        // Entrega en un almacén: inserta el movimiento de entrada (compra) con su stock y valor; el movimiento
        // se valora al precio neto de descuento. Queda enganchado a la entrega por IdLineaDeRecepcion (FK).
        // El gestor de movimientos calcula su inventario, lo preasienta y barre los posteriores del mismo almacén y material
        public static MovimientoDeAlmacenDtm EntradaDeCompra(this RecepcionDeLineaDtm recepcion, ContextoSe contexto, LineaDeUnPedidoDtm linea)
        {
            if (recepcion.IdAlmacen is null || linea.IdUnitario is null)
                Emitir($"La entrega de la línea {linea.Orden} no se puede registrar como entrada de almacén: falta el almacén o el material");

            var movimiento = new MovimientoDeAlmacenDtm
            {
                IdAlmacen = (int)recepcion.IdAlmacen,
                IdUnitario = (int)linea.IdUnitario,
                IdTipoMovimiento = enumTiposDeMovimiento.Compra.LeerTipoDeMovimiento(contexto).Id,
                Cantidad = recepcion.Cantidad,
                Precio = recepcion.Cantidad == 0 ? recepcion.Precio : recepcion.Importe / recepcion.Cantidad,
                RealizadoEl = recepcion.RecepcionadaEl,
                IdLineaDeRecepcion = recepcion.Id
            };

            // el gestor toma el semáforo, calcula el inventario, preasienta y barre los posteriores
            return movimiento.InsertarComoAdministrador(contexto);
        }

        // Las recepciones se registran con el pedido en recepción: si está solicitado se transita a recepción, anotando como fecha de
        // recepción de la cabecera la de la entrega (ver ExtensorDePedidos.AntesDeRecibir). Devuelve el pedido releído tras transitar
        public static PedidoDtm PasarARecepcion(this PedidoDtm pedido, ContextoSe contexto, DateTime recibidoEl)
        {
            if (!pedido.EstaEnLaEtapa(enumEtapasDePedido.PED_Etapa_De_Solicitud))
                return pedido;

            var parametros = new Dictionary<string, object> { { nameof(PedidoDtm.RecibidoEl), recibidoEl.Date } };
            var etapa = enumEtapasDePedido.PED_Etapa_De_Recepcion.EstadosDeLaEtapa();
            // se usa la transición del sistema y, si no la hay, la de usuario
            if (pedido.TransicionPosible(contexto, etapa.etapa, etapa.estados, delSistema: true, emitirErrorSiNoHay: false) != null)
                pedido.TransitarALaEtapa(contexto, etapa, parametros, delSistema: true);
            else
                pedido.TransitarALaEtapa(contexto, etapa, parametros, delSistema: false);

            return contexto.SeleccionarPorId<PedidoDtm>(pedido.Id, usarLaCache: false);
        }

        // Se cachea también el false; se olvida al recibir o cancelar la entrega de cualquiera de sus líneas (ver OlvidarRecepcion)
        public static bool TieneLineasRecepcionadas(this PedidoDtm pedido, ContextoSe contexto)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Pedido_HayRecepciones);
            if (cache.ContainsKey(pedido.Id.ToString()))
                return (bool)cache[pedido.Id.ToString()];
            // las líneas salen de la caché de detalles; una lista en memoria no se traduce a SQL dentro de la consulta, sus ids sí (IN)
            var idsDeLineas = pedido.Detalles<LineaDeUnPedidoDtm>(contexto).Select(linea => linea.Id).ToList();
            var hayRecepciones = idsDeLineas.Count > 0 && contexto.Set<RecepcionDeLineaDtm>().Any(recepcion => idsDeLineas.Contains(recepcion.IdLinea));
            cache[pedido.Id.ToString()] = hayRecepciones;
            return hayRecepciones;
        }

        public static LineaDeUnPedidoDtm LeerLineaDePedido(this RecepcionDeLineaDtm recepcion, ContextoSe contexto)
        {
            return LeerLineaDePedido(contexto, recepcion.Id);
        }

        public static AlmacenDtm LeerAlmacen(this RecepcionDeLineaDtm recepcion, ContextoSe contexto, bool erroSiNoVaAlAlmacen = true)
        {
            if (recepcion.IdAlmacen is null && erroSiNoVaAlAlmacen)
                Emitir("La recepción indicada no es de almacén");

            if (recepcion.IdAlmacen is null) return null;


            if (recepcion.Almacen != null && recepcion.Almacen.Id == (int)recepcion.IdAlmacen)
                return recepcion.Almacen;

            recepcion.Almacen = contexto.Set<AlmacenDtm>().First(a => a.Id == recepcion.IdAlmacen);
            return recepcion.Almacen;
        }

        public static LineaDeUnPedidoDtm LeerLineaDePedido(ContextoSe contexto, int idDeRecepcion)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Pedido_LineaPedidoPorRecepcion);
            if (cache.ContainsKey(idDeRecepcion.ToString()))
                return (LineaDeUnPedidoDtm)cache[idDeRecepcion.ToString()];

            var lineaDelPedido = contexto.Set<LineaDeUnPedidoDtm>().FirstOrDefault(lp => contexto.Set<RecepcionDeLineaDtm>().Any(r => r.Id == idDeRecepcion && r.IdLinea == lp.Id));
            if (lineaDelPedido is null)
                Emitir($"No se ha encontrado la línea del pedido de la entrega {idDeRecepcion}");

            lineaDelPedido.Elemento = lineaDelPedido.DetalleDe<PedidoDtm>(contexto);
            cache[idDeRecepcion.ToString()] = lineaDelPedido;
            return lineaDelPedido;
        }

        //---------------------------------------------------------------------------------------------------------------------------
        // Operaciones de recepción, se ejecutan dentro de la transacción del llamador. Al acabar, bien o mal, olvidan las cachés de
        // recepciones que hayan podido leer o cambiar, para que si la transacción se deshace no queden con datos que no se guardaron
        //---------------------------------------------------------------------------------------------------------------------------

        // Entrega parcial de una línea con lo indicado en la modal de recepción; si no se indica destino va al de la cabecera.
        // Si el pedido está solicitado pasa a recepción
        public static string RecepcionarLinea(ContextoSe contexto, int idLineaPedido, string rowVersion, DateTime recepcionadaEl, decimal cantidad, decimal precio, decimal? descuento, int? idAlmacen, int? idExpediente)
        {
            PedidoDtm pedido = null;
            try
            {
                LineaDeUnPedidoDtm lineaDePedido;
                (lineaDePedido, pedido) = LeerLineaParaRecepcionar(contexto, idLineaPedido, rowVersion, esUnaEntrega: true);

                var destino = idAlmacen is null && idExpediente is null ? pedido.DestinoDeLasEntregas() : (idAlmacen, idExpediente);
                var estabaSolicitado = pedido.EstaEnLaEtapa(enumEtapasDePedido.PED_Etapa_De_Solicitud);
                pedido = pedido.PasarARecepcion(contexto, recepcionadaEl);
                var recepcion = lineaDePedido.Recibir(contexto, pedido, recepcionadaEl, cantidad, precio, descuento, destino.idAlmacen, destino.idExpediente);

                return $"Línea {lineaDePedido.Orden} recibida el {recepcion.RecepcionadaEl:dd/MM/yyyy} {(recepcion.VaAlAlmacen ? "en el almacén" : "en la obra del expediente")}: cantidad {cantidad}, precio {precio}, descuento {descuento}, importe entregado {recepcion.Importe:N2}"
                     + (estabaSolicitado ? $"; el pedido '{pedido.Referencia}' pasa a {enumEtapasDePedido.PED_Etapa_De_Recepcion.Nombre()}" : "");
            }
            finally
            {
                OlvidarRecepcion(idLineaPedido, pedido?.Id ?? 0);
            }
        }

        // Aspa: cancela la entrega de la línea, sólo con el pedido en recepción; si la línea no tiene entrega es un error
        public static string LineaNoEntregada(ContextoSe contexto, int idLineaPedido, string rowVersion)
        {
            PedidoDtm pedido = null;
            try
            {
                LineaDeUnPedidoDtm linea;
                // valida que la línea tenga entrega y que el pedido esté en recepción
                (linea, pedido) = LeerLineaParaRecepcionar(contexto, idLineaPedido, rowVersion, esUnaEntrega: false);
                linea.CancelarRecepcion(contexto);
                return $"Cancelada la entrega de la línea {linea.Orden}";
            }
            finally
            {
                OlvidarRecepcion(idLineaPedido, pedido?.Id ?? 0);
            }
        }

        // Entrega completa: recepciona todas las líneas del pedido con lo pedido en cada una; si no se indica destino va al de la cabecera.
        // Si el pedido está solicitado pasa a recepción; si alguna línea ya se ha recepcionado no se permite, se han de hacer recepciones parciales
        // "Dar por entregado": recepciona todas las líneas del pedido con lo pedido en cada una; si no se indica destino va al de la cabecera.
        // Si el pedido está solicitado pasa a recepción; si alguna línea ya se ha recepcionado no se permite, se han de hacer recepciones parciales
        public static string RecepcionarPedido(ContextoSe contexto, int idPedido, string rowVersion, DateTime recepcionadoEl, int? idAlmacen, int? idExpediente)
        {
            var idsDeLineas = new List<int>();
            try
            {
                var pedido = contexto.SeleccionarPorId<PedidoDtm>(idPedido, usarLaCache: false);
                pedido.ValidarRowId(contexto, rowVersion, releer: false);
                pedido.ValidarQueEsGestor(contexto);

                var destino = idAlmacen is null && idExpediente is null ? pedido.DestinoDeLasEntregas() : (idAlmacen, idExpediente);

                if (!pedido.EstaEnAlgunaDeLasEtapa(new List<enumEtapasDePedido> { enumEtapasDePedido.PED_Etapa_De_Solicitud, enumEtapasDePedido.PED_Etapa_De_Recepcion }))
                    Emitir($"El pedido '{pedido.Referencia}' solo se puede recepcionar {enumEtapasDePedido.PED_Etapa_De_Solicitud.Nombre()} o {enumEtapasDePedido.PED_Etapa_De_Recepcion.Nombre()}");

                pedido = pedido.PasarARecepcion(contexto, recepcionadoEl);
                var (lineas, importe) = pedido.RecibirTodasLasLineas(contexto, recepcionadoEl, destino, idsDeLineas, estaEnRecepcion: true);

                return $"Pedido '{pedido.Referencia}' recepcionado el {recepcionadoEl:dd/MM/yyyy} {(destino.idAlmacen != null ? "en el almacén" : "en la obra del expediente")}: {lineas} líneas por un importe de {importe:N2}";
            }
            finally
            {
                foreach (var idLinea in idsDeLineas)
                    OlvidarRecepcion(idLinea, idPedido);
                OlvidarRecepciones(idPedido);
            }
        }

        // "Dar por recibido" (transición de solicitado a cerrado): la fecha de recepción es la de entrega si ya ha pasado y si no la de hoy
        public static DateTime FechaDeRecepcionAlDarPorRecibido(this PedidoDtm pedido)
            => pedido.EntregarEl is null || pedido.EntregarEl.Value.Date > DateTime.Today ? DateTime.Today : pedido.EntregarEl.Value.Date;

        // "Dar por recibido": una vez cerrado el pedido se recepcionan todas sus líneas con lo pedido, en la fecha de recepción de la
        // cabecera (ver GestorDePedidos.DespuesDeTransitar) y al destino de la cabecera. Un pedido sin líneas que recibir se cierra igualmente
        public static void RecepcionarAlDarPorRecibido(this PedidoDtm pedido, ContextoSe contexto)
        {
            var idsDeLineas = new List<int>();
            try
            {
                var recibidoEl = pedido.RecibidoEl ?? pedido.FechaDeRecepcionAlDarPorRecibido();
                pedido.RecibirTodasLasLineas(contexto, recibidoEl, pedido.DestinoDeLasEntregas(), idsDeLineas, estaEnRecepcion: false);
            }
            finally
            {
                foreach (var idLinea in idsDeLineas)
                    OlvidarRecepcion(idLinea, pedido.Id);
                OlvidarRecepciones(pedido.Id);
            }
        }

        // Al almacén sólo van materiales y ha de estar activo; a la obra de un expediente va cualquier clase, si no está terminado ni cancelado
        private static void ValidarDestinoDeLaEntrega(this LineaDeUnPedidoDtm linea, ContextoSe contexto, int? idAlmacen, int? idExpediente)
        {
            if (idAlmacen is not null)
            {
                // al almacén sólo entra lo que está en el catálogo: una partida alzada no, aunque su naturaleza sea de material
                if (linea.IdUnitario is null)
                    Emitir($"No se puede hacer la recepción de la línea {linea.Orden} en un almacén: sólo se reciben materiales del catálogo y es una partida alzada");

                var clase = linea.Clase(contexto);
                if (clase != enumClaseUnitario.Material)
                    Emitir($"No se puede hacer la recepción de la línea {linea.Orden} en un almacén: sólo se reciben materiales y es de {(clase?.ToString() ?? "una naturaleza sin clase")}");

                var almacen = contexto.SeleccionarPorId<AlmacenDtm>((int)idAlmacen);
                if (!almacen.EstaEnLaEtapa(enumEtapasDeAlmacen.ALM_Etapa_Activo))
                    Emitir($"No se puede hacer la recepción de la línea {linea.Orden} en el almacén '{almacen.Expresion}' por no estar activo");
            }

            if (idExpediente is not null)
            {
                var expediente = contexto.SeleccionarPorId<ExpedienteDtm>((int)idExpediente);
                if (expediente.EstaEnLaEtapa(enumEtapasDeExpedientes.EXP_Etapa_Terminada) || expediente.EstaEnLaEtapa(enumEtapasDeExpedientes.EXP_Etapa_Cancelada))
                    Emitir($"No se puede hacer la recepción de la línea {linea.Orden} en la obra del expediente '{expediente.Expresion}' por estar terminado o cancelado");
            }
        }

        // La fecha de recepción de la cabecera es la de la primera entrega
        private static void AdelantarLaFechaDeRecepcion(this PedidoDtm pedido, ContextoSe contexto, DateTime recepcionadaEl)
        {
            if (pedido.RecibidoEl is not null && pedido.RecibidoEl.Value.Date <= recepcionadaEl.Date)
                return;
            pedido.RecibidoEl = recepcionadaEl.Date;
            pedido.Modificar(contexto, nameof(AdelantarLaFechaDeRecepcion));
        }

        // Registra la entrega sin mirar la etapa del pedido: Recibir exige que esté en recepción, al darlo por recibido (de solicitado
        // a cerrado) se registra tras cerrarlo (ver RecepcionarAlDarPorRecibido)
        private static RecepcionDeLineaDtm RegistrarRecepcion(this LineaDeUnPedidoDtm linea, ContextoSe contexto, PedidoDtm pedido, DateTime recepcionadaEl, decimal cantidad, decimal precio, decimal? descuento, int? idAlmacen, int? idExpediente)
        {
            if (pedido.Id != linea.IdElemento)
                Emitir($"La línea {linea.Orden} no es del pedido '{pedido.Referencia}'");
            if (recepcionadaEl.Date > DateTime.Today)
                Emitir($"La fecha de recepción de la línea {linea.Orden} no puede ser posterior a hoy");
            if (recepcionadaEl.Date < pedido.FechaCreacion.Date)
                Emitir($"La fecha de recepción de la línea {linea.Orden} no puede ser anterior a la creación del pedido '{pedido.Referencia}' ({pedido.FechaCreacion:dd/MM/yyyy})");
            // la cabecera no admite una fecha de recepción anterior a la de pedido (ver ExtensorDePedidos.ValidarFechas)
            if (pedido.PedidoEl is not null && recepcionadaEl.Date < pedido.PedidoEl.Value.Date)
                Emitir($"La fecha de recepción de la línea {linea.Orden} no puede ser anterior a la fecha en la que se pidió el pedido '{pedido.Referencia}' ({pedido.PedidoEl:dd/MM/yyyy})");
            if ((idAlmacen == null) == (idExpediente == null))
                Emitir($"La entrega de la línea {linea.Orden} ha de ir a un almacén o a la obra de un expediente, y sólo a uno de ellos");
            linea.ValidarDestinoDeLaEntrega(contexto, idAlmacen, idExpediente);

            var recepcion = new RecepcionDeLineaDtm
            {
                IdLinea = linea.Id,
                Cantidad = cantidad,
                Precio = precio,
                Descuento = descuento,
                RecepcionadaEl = recepcionadaEl.Date,
                IdAlmacen = idAlmacen,
                IdExpediente = idExpediente,
                IdUsuaCrea = contexto.DatosDeConexion.IdUsuario,
                FechaCreacion = DateTime.Now
            };
            try
            {
                contexto.Add(recepcion);
                contexto.SaveChanges();
                if (recepcion.VaAlAlmacen)
                    recepcion.EntradaDeCompra(contexto, linea);
            }
            finally
            {
                linea.OlvidarRecepcion();
            }
            return recepcion;
        }

        // Al cancelar una entrega en un almacén se borra su movimiento de compra, si lo tiene. Sólo se puede si el almacén está activo
        // y su preasiento, si lo tiene, está pendiente; el preasiento se anula. Desde el movimiento anterior se barren los posteriores
        private static void AnularEntradaDeCompra(this RecepcionDeLineaDtm recepcion, ContextoSe contexto)
        {
            var linea = recepcion.LeerLineaDePedido(contexto);
            var pedido = linea.Elemento;
            if (!pedido.EstaEnRecepcion())
                Emitir($"No se puede cancelar la entrega de la línea {linea.Orden}: el pedido '{pedido.Referencia}' ha de estar {enumEtapasDePedido.PED_Etapa_De_Recepcion.Nombre()}");

            if (!recepcion.VaAlAlmacen)
                return;

            var almacen = recepcion.LeerAlmacen(contexto);
            if (!almacen.EstaActivo())
                Emitir($"No se puede cancelar la entrega de la línea {linea.Orden}: el almacén '{almacen.Expresion}' no está activo y no se puede borrar su entrada de compra");

            var idTipoMovimiento = enumTiposDeMovimiento.Compra.LeerTipoDeMovimiento(contexto).Id;
            var movimiento = contexto.Set<MovimientoDeAlmacenDtm>().FirstOrDefault(x => x.IdLineaDeRecepcion == recepcion.Id && x.IdTipoMovimiento == idTipoMovimiento);
            if (movimiento is null)
                return;

            movimiento.EliminarComoAdministrador(contexto, accionEjecutada: ltrDeUnMovimientoDeAlmacen.Accion_AnularEntradaDeCompra);
        }

        // Se borra la entrega leída de la base de datos, no la cacheada, para que su RowVersion sea el actual.
        // Queda una traza en el pedido con quién la ha cancelado y los datos que tenía la entrega
        private static void CancelarRecepcion(this LineaDeUnPedidoDtm linea, ContextoSe contexto)
        {
            var recepcion = linea.LeerRecepcion(contexto, erroSiNoHay: false);
            if (recepcion is null)
                return;
            try
            {
                recepcion.AnularEntradaDeCompra(contexto);
                var pedido = contexto.SeleccionarPorId<PedidoDtm>(linea.IdElemento);
                var descripcion = recepcion.DescripcionParaLaTraza(contexto, linea);
                contexto.Remove(recepcion);
                contexto.SaveChanges();
                pedido.CrearTraza(contexto, $"Entrega de la línea {linea.Orden} cancelada", descripcion);
            }
            finally
            {
                linea.OlvidarRecepcion();
                ServicioDeCaches.EliminarElemento(CacheDe.Pedido_LineaPedidoPorRecepcion, recepcion.Id.ToString());
            }
        }

        // Se anota quién cancela, porque la traza se guarda como administrador (ver ExtensorDeTrazasDtm.CrearTraza)
        private static string DescripcionParaLaTraza(this RecepcionDeLineaDtm recepcion, ContextoSe contexto, LineaDeUnPedidoDtm linea)
        {
            var destino = recepcion.VaAlAlmacen
                ? $"almacén {contexto.SeleccionarPorId<AlmacenDtm>((int)recepcion.IdAlmacen).Expresion}"
                : $"obra del expediente {contexto.SeleccionarPorId<ExpedienteDtm>((int)recepcion.IdExpediente).Expresion}";
            var registradaPor = contexto.SeleccionarPorId<UsuarioDtm>(recepcion.IdUsuaCrea).Expresion;

            return $"Cancelada por {contexto.Usuario.Expresion} el {DateTime.Now:dd/MM/yyyy HH:mm}{Environment.NewLine}" +
                   $"Línea {linea.Orden}: {linea.Concepto}{Environment.NewLine}" +
                   $"Recepcionada el {recepcion.RecepcionadaEl:dd/MM/yyyy} en el {destino}{Environment.NewLine}" +
                   $"Cantidad {recepcion.Cantidad:N2}, precio {recepcion.Precio:N2}, descuento {(recepcion.Descuento ?? 0):N2} %, importe {recepcion.Importe:N2}{Environment.NewLine}" +
                   $"Registrada por {registradaPor} el {recepcion.FechaCreacion:dd/MM/yyyy HH:mm}";
        }

        // Recibe todas las líneas con lo pedido en cada una; si alguna ya se ha recepcionado no se permite, se han de hacer recepciones parciales.
        // estaEnRecepcion: entrega completa del pedido en recepción; si no, se da por recibido al cerrarlo y no es error que no haya líneas
        private static (int lineas, decimal importe) RecibirTodasLasLineas(this PedidoDtm pedido, ContextoSe contexto, DateTime recepcionadoEl, (int? idAlmacen, int? idExpediente) destino, List<int> idsDeLineas, bool estaEnRecepcion)
        {
            if (pedido.TieneLineasRecepcionadas(contexto))
                Emitir($"El pedido '{pedido.Referencia}' tiene líneas recepcionadas, recepcione las que falten con recepciones parciales");

            // los comentarios no tienen cantidad ni precio, no se reciben
            var lineas = pedido.Detalles<LineaDeUnPedidoDtm>(contexto)
                .Where(linea => linea.TipoDeLinea != enumTipoDeLinea.Comentario && linea.Cantidad > 0 && linea.Precio != null)
                .OrderBy(linea => linea.Orden)
                .ToList();
            if (lineas.Count == 0 && estaEnRecepcion)
                Emitir($"El pedido '{pedido.Referencia}' no tiene líneas que recepcionar");

            idsDeLineas.AddRange(lineas.Select(linea => linea.Id));
            var importe = 0m;
            foreach (var linea in lineas)
            {
                var recepcion = estaEnRecepcion
                    ? linea.Recibir(contexto, pedido, recepcionadoEl, (decimal)linea.Cantidad, (decimal)linea.Precio, linea.Descuento, destino.idAlmacen, destino.idExpediente)
                    : linea.RegistrarRecepcion(contexto, pedido, recepcionadoEl, (decimal)linea.Cantidad, (decimal)linea.Precio, linea.Descuento, destino.idAlmacen, destino.idExpediente);
                importe += recepcion.Importe;
            }
            return (lineas.Count, importe);
        }

        // Las entregas las registra o cancela quien gestiona el pedido
        private static void ValidarQueEsGestor(this PedidoDtm pedido, ContextoSe contexto)
        {
            if (!ApiDePermisos.LeerModoDeAcceso(contexto, enumNegocio.Pedido, pedido).HayPermisosDe(enumModoDeAccesoDeDatos.Gestor))
                Emitir($"Ha de ser gestor del pedido '{pedido.Referencia}' para registrar o cancelar sus entregas");
        }

        // Lee la línea sin cache, valida que es la versión que el usuario vio en el grid y que el pedido admite recepciones.
        // esUnaEntrega: la línea no puede estar ya entregada (solicitado se permite, la entrega lo pasa a recepción); si no, se anula
        // la entrega, que la línea ha de tener, sólo con el pedido en recepción
        private static (LineaDeUnPedidoDtm linea, PedidoDtm pedido) LeerLineaParaRecepcionar(ContextoSe contexto, int idLinea, string rowVersion, bool esUnaEntrega)
        {
            var linea = contexto.SeleccionarPorId<LineaDeUnPedidoDtm>(idLinea, usarLaCache: false);
            linea.ValidarRowId(contexto, rowVersion, releer: false);

            var pedido = contexto.SeleccionarPorId<PedidoDtm>(linea.IdElemento, usarLaCache: false);
            pedido.ValidarQueEsGestor(contexto);
            if (!pedido.EstaEnAlgunaDeLasEtapa(new List<enumEtapasDePedido> { enumEtapasDePedido.PED_Etapa_De_Solicitud, enumEtapasDePedido.PED_Etapa_De_Recepcion }))
                Emitir($"Las líneas del pedido '{pedido.Referencia}' solo se pueden recepcionar con el pedido {enumEtapasDePedido.PED_Etapa_De_Solicitud.Nombre()} o {enumEtapasDePedido.PED_Etapa_De_Recepcion.Nombre()}");

            var entregada = linea.LeerRecepcion(contexto, erroSiNoHay: false) != null;
            if (esUnaEntrega && entregada)
                Emitir($"La línea {linea.Orden} del pedido '{pedido.Referencia}' ya se ha entregado, para volver a entregarla cancele antes la entrega");
            // el aspa anula la entrega: la línea ha de tenerla y, como solicitado no puede haber nada entregado, sólo en recepción
            if (!esUnaEntrega && !entregada)
                Emitir($"La línea {linea.Orden} del pedido '{pedido.Referencia}' no tiene entrega asociada");
            if (!esUnaEntrega && !pedido.EstaEnLaEtapa(enumEtapasDePedido.PED_Etapa_De_Recepcion))
                Emitir($"Solo se puede cancelar la entrega de la línea {linea.Orden} con el pedido '{pedido.Referencia}' {enumEtapasDePedido.PED_Etapa_De_Recepcion.Nombre()}");

            return (linea, pedido);
        }

        // Destino por defecto de las entregas de un pedido: el almacén de la cabecera y, si no lo tiene, la obra de su expediente
        private static (int? idAlmacen, int? idExpediente) DestinoDeLasEntregas(this PedidoDtm pedido)
            => pedido.IdAlmacen != null ? (pedido.IdAlmacen, null) : (null, pedido.IdExpediente);
    }
}
