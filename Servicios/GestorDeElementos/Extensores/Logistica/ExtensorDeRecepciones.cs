using Gestor.Errores;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Logistica;
using ServicioDeDatos.MaestrosTecnico;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Terceros;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilidades;
using static Gestor.Errores.GestorDeErrores;
using static ServicioDeDatos.Elemento.Enumerados;

namespace GestorDeElementos.Extensores
{
    // Recepciones de las líneas de los pedidos (RecepcionDeLineaDtm): reglas para entregar y cancelar entregas, destino de las entregas
    // y cachés de recepciones. Entregar es insertar una recepción y cancelarla es eliminarla (ver GestorDeEntregasDeUnPedido)
    public static class ExtensorDeRecepciones
    {
        // Entregas registradas de una línea del pedido, por fecha; una línea se puede entregar en varias veces sin superar lo pedido.
        // Se cachean por línea (también la lista vacía) y se olvidan al entregar o cancelar una entrega (ver GestorDeEntregasDeUnPedido.EliminarCaches)
        public static List<RecepcionDeLineaDtm> Recepciones(this LineaDeUnPedidoDtm linea, ContextoSe contexto)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Pedido_Recepcion);
            if (!cache.ContainsKey(linea.Id.ToString()))
                cache[linea.Id.ToString()] = contexto.Set<RecepcionDeLineaDtm>()
                    .Where(recepcion => recepcion.IdLinea == linea.Id)
                    .OrderBy(recepcion => recepcion.RecepcionadaEl).ThenBy(recepcion => recepcion.Id)
                    .ToList();
            return (List<RecepcionDeLineaDtm>)cache[linea.Id.ToString()];
        }

        public static decimal CantidadRecibida(this LineaDeUnPedidoDtm linea, ContextoSe contexto)
            => linea.Recepciones(contexto).Sum(recepcion => recepcion.Cantidad);

        public static decimal CantidadDevuelta(this LineaDeUnPedidoDtm linea, ContextoSe contexto)
            => linea.Recepciones(contexto).Sum(recepcion => recepcion.CantidadDevuelta(contexto));

        // Lo que queda por entregar: lo pedido menos lo recibido que no se ha devuelto (lo devuelto el proveedor lo puede volver a entregar)
        public static decimal CantidadPendienteDeEntregar(this LineaDeUnPedidoDtm linea, ContextoSe contexto)
            => (linea.Cantidad ?? 0) - (linea.CantidadRecibida(contexto) - linea.CantidadDevuelta(contexto));

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

        public static AlmacenDtm Almacen(this RecepcionDeLineaDtm recepcion, ContextoSe contexto, bool erroSiNoVaAlAlmacen = true)
        {
            if (recepcion.IdAlmacen is null && erroSiNoVaAlAlmacen)
                Emitir("La recepción indicada no es de almacén");

            if (recepcion.IdAlmacen is null) return null;


            if (recepcion.Almacen != null && recepcion.Almacen.Id == (int)recepcion.IdAlmacen)
                return recepcion.Almacen;

            recepcion.Almacen = contexto.Set<AlmacenDtm>().First(a => a.Id == recepcion.IdAlmacen);
            return recepcion.Almacen;
        }
               
        public static LineaDeUnPedidoDtm LineaDePedido(this RecepcionDeLineaDtm recepcion, ContextoSe contexto)
        {
            return LineaDePedido(contexto, recepcion.Id);
        }

        public static LineaDeUnPedidoDtm LineaDePedido(ContextoSe contexto, int idDeRecepcion)
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

        // "Dar por recibido" (transición de solicitado a cerrado): la fecha de recepción es la de entrega si ya ha pasado y si no la de hoy
        public static DateTime FechaDeRecepcionAlDarPorRecibido(this PedidoDtm pedido)
            => pedido.EntregarEl is null || pedido.EntregarEl.Value.Date > DateTime.Today ? DateTime.Today : pedido.EntregarEl.Value.Date;

        public static ProveedorDtm Proveedor(this RecepcionDeLineaDtm recepcion, ContextoSe contexto)
        {
            var linea = recepcion.LineaDePedido(contexto);
            return linea.Elemento.Proveedor(contexto);
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
        public static void AdelantarLaFechaDeRecepcion(this PedidoDtm pedido, ContextoSe contexto, DateTime recepcionadaEl)
        {
            if (pedido.RecibidoEl is not null && pedido.RecibidoEl.Value.Date <= recepcionadaEl.Date)
                return;
            pedido.RecibidoEl = recepcionadaEl.Date;
            pedido.Modificar(contexto, nameof(AdelantarLaFechaDeRecepcion));
        }

        // Validaciones de una entrega, sin mirar la etapa del pedido: el gestor exige que esté en recepción salvo al darlo por recibido (de
        // solicitado a cerrado), que se registra tras cerrarlo (ver GestorDeEntregasDeUnPedido.AntesDePersistir). La entrega va al almacén
        // o a la obra del expediente, nunca a ambos (ver CK_PEDIDO_LINEA_RECEPCION_DESTINO); la fecha no puede ser posterior a hoy ni
        // anterior a la creación del pedido ni a la fecha en la que se pidió, y nunca se entrega más de lo pedido
        public static void ValidarEntrega(this RecepcionDeLineaDtm recepcion, ContextoSe contexto, LineaDeUnPedidoDtm linea, PedidoDtm pedido)
        {
            var recepcionadaEl = recepcion.RecepcionadaEl;
            if (recepcion.IdLinea != linea.Id || pedido.Id != linea.IdElemento)
                Emitir($"La línea {linea.Orden} no es del pedido '{pedido.Referencia}'");
            if (recepcionadaEl.Date > DateTime.Today)
                Emitir($"La fecha de recepción de la línea {linea.Orden} no puede ser posterior a hoy");
            if (recepcionadaEl.Date < pedido.FechaCreacion.Date)
                Emitir($"La fecha de recepción de la línea {linea.Orden} no puede ser anterior a la creación del pedido '{pedido.Referencia}' ({pedido.FechaCreacion:dd/MM/yyyy})");
            // la cabecera no admite una fecha de recepción anterior a la de pedido (ver ExtensorDePedidos.ValidarFechas)
            if (pedido.PedidoEl is not null && recepcionadaEl.Date < pedido.PedidoEl.Value.Date)
                Emitir($"La fecha de recepción de la línea {linea.Orden} no puede ser anterior a la fecha en la que se pidió el pedido '{pedido.Referencia}' ({pedido.PedidoEl:dd/MM/yyyy})");
            if ((recepcion.IdAlmacen == null) == (recepcion.IdExpediente == null))
                Emitir($"La entrega de la línea {linea.Orden} ha de ir a un almacén o a la obra de un expediente, y sólo a uno de ellos");
            if (recepcion.Cantidad <= 0)
                Emitir($"Ha de indicar la cantidad entregada de la línea {linea.Orden}");
            // nunca se entrega más de lo pedido: lo pendiente es lo pedido menos lo recibido que no se ha devuelto
            var pendiente = linea.CantidadPendienteDeEntregar(contexto);
            if (recepcion.Cantidad > pendiente)
                Emitir($"No se pueden entregar {recepcion.Cantidad} de la línea {linea.Orden}: se pidieron {linea.Cantidad ?? 0} y quedan por entregar {pendiente}");
            linea.ValidarDestinoDeLaEntrega(contexto, recepcion.IdAlmacen, recepcion.IdExpediente);
            pedido.ValidarSociedadDelDestino(contexto, recepcion.IdAlmacen, recepcion.IdExpediente);
        }

        // Al cancelar una entrega en un almacén se borra su movimiento de compra, si lo tiene. Sólo se puede si el almacén está activo
        // y su preasiento, si lo tiene, está pendiente; el preasiento se anula. Desde el movimiento anterior se barren los posteriores
        public static void AnularEntradaDeCompra(this RecepcionDeLineaDtm recepcion, ContextoSe contexto)
        {
            var linea = recepcion.LineaDePedido(contexto);
            var pedido = linea.Elemento;
            if (!pedido.EstaEnRecepcion())
                Emitir($"No se puede cancelar la entrega de la línea {linea.Orden}: el pedido '{pedido.Referencia}' ha de estar {enumEtapasDePedido.PED_Etapa_De_Recepcion.Nombre()}");

            if (!recepcion.VaAlAlmacen)
                return;

            var almacen = recepcion.Almacen(contexto);
            if (!almacen.EstaActivo())
                Emitir($"No se puede cancelar la entrega de la línea {linea.Orden}: el almacén '{almacen.Expresion}' no está activo y no se puede borrar su entrada de compra");

            var movimiento = recepcion.MovimientoDeCompra(contexto);
            if (movimiento is null)
                return;

            var contabilizado = movimiento.PreasientoContabilizado(contexto);
            if (contabilizado is not null)
                Emitir($"No se puede cancelar la entrega de la línea {linea.Orden}: el preasiento '{contabilizado.Referencia}' de su entrada de compra está {enumEtapasDePreasiento.SPR_Etapa_Contabilizado.Nombre()} (está en un lote contable)");

            movimiento.EliminarComoAdministrador(contexto, accionEjecutada: ltrDeUnMovimientoDeAlmacen.Accion_AnularEntradaDeCompra);
        }

        // Movimiento de compra que generó la entrega en un almacén, null si la entrega va a la obra de un expediente
        private static MovimientoDeAlmacenDtm MovimientoDeCompra(this RecepcionDeLineaDtm recepcion, ContextoSe contexto)
        {
            if (!recepcion.VaAlAlmacen)
                return null;

            var idTipoMovimiento = enumTiposDeMovimiento.Compra.LeerTipoDeMovimiento(contexto).Id;
            return contexto.Set<MovimientoDeAlmacenDtm>().FirstOrDefault(x => x.IdLineaDeRecepcion == recepcion.Id && x.IdTipoMovimiento == idTipoMovimiento);
        }

        // Una entrega cuyo preasiento está contabilizado (al generar el lote contable se transita a esa etapa) no se puede cancelar,
        // ya que para el sistema el asiento está contabilizado (ver GestorDeLineasDeUnPedido.DespuesDeMapearElElemento)
        public static bool EntregaContabilizada(this RecepcionDeLineaDtm recepcion, ContextoSe contexto)
            => recepcion.MovimientoDeCompra(contexto)?.PreasientoContabilizado(contexto) is not null;

        // Se anota quién cancela, porque la traza se guarda como administrador (ver ExtensorDeTrazasDtm.CrearTraza)
        public static string DescripcionParaLaTraza(this RecepcionDeLineaDtm recepcion, ContextoSe contexto, LineaDeUnPedidoDtm linea)
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

        // Las entregas las registra o cancela quien gestiona el pedido
        public static void ValidarQueEsGestor(this PedidoDtm pedido, ContextoSe contexto)
        {
            if (!ApiDePermisos.LeerModoDeAcceso(contexto, enumNegocio.Pedido, pedido).HayPermisosDe(enumModoDeAccesoDeDatos.Gestor))
                Emitir($"Ha de ser gestor del pedido '{pedido.Referencia}' para registrar o cancelar sus entregas");
        }

        // Lee la línea sin cache, valida que es la versión que el usuario vio en el grid, que quien entrega gestiona el pedido, que éste
        // admite recepciones (solicitado se permite, la entrega lo pasa a recepción) y que a la línea le queda algo por entregar
        public static (LineaDeUnPedidoDtm linea, PedidoDtm pedido) LeerLineaParaRecepcionar(ContextoSe contexto, int idLinea, string rowVersion, bool esUnaEntrega)
        {
            var linea = contexto.SeleccionarPorId<LineaDeUnPedidoDtm>(idLinea, usarLaCache: false);
            linea.ValidarRowId(contexto, rowVersion, releer: false);

            var pedido = contexto.SeleccionarPorId<PedidoDtm>(linea.IdElemento, usarLaCache: false);
            pedido.ValidarQueEsGestor(contexto);
            if (!pedido.EstaEnAlgunaDeLasEtapa(new List<enumEtapasDePedido> { enumEtapasDePedido.PED_Etapa_De_Solicitud, enumEtapasDePedido.PED_Etapa_De_Recepcion }))
                Emitir($"Las líneas del pedido '{pedido.Referencia}' solo se pueden recepcionar con el pedido {enumEtapasDePedido.PED_Etapa_De_Solicitud.Nombre()} o {enumEtapasDePedido.PED_Etapa_De_Recepcion.Nombre()}");

            if (esUnaEntrega && linea.CantidadPendienteDeEntregar(contexto) <= 0)
                Emitir($"La línea {linea.Orden} del pedido '{pedido.Referencia}' ya se ha entregado por completo");

            return (linea, pedido);
        }

        // Una entrega se cancela con el pedido en recepción (solicitado no puede haber nada entregado), por quien lo gestiona y si no tiene
        // devoluciones (lo devuelto salió del almacén y está preasentado); devuelve el pedido leído sin caché
        public static PedidoDtm ValidarEntregaParaCancelar(this RecepcionDeLineaDtm recepcion, ContextoSe contexto, LineaDeUnPedidoDtm linea)
        {
            var pedido = contexto.SeleccionarPorId<PedidoDtm>(linea.IdElemento, usarLaCache: false);
            pedido.ValidarQueEsGestor(contexto);
            if (!pedido.EstaEnLaEtapa(enumEtapasDePedido.PED_Etapa_De_Recepcion))
                Emitir($"Solo se puede cancelar la entrega de la línea {linea.Orden} con el pedido '{pedido.Referencia}' {enumEtapasDePedido.PED_Etapa_De_Recepcion.Nombre()}");
            if (recepcion.TieneDevoluciones(contexto))
                Emitir($"No se puede cancelar la entrega de la línea {linea.Orden} del pedido '{pedido.Referencia}' porque tiene devoluciones, cancélelas antes");
            return pedido;
        }

        // Destino por defecto de las entregas de un pedido: el almacén de la cabecera y, si no lo tiene, la obra de su expediente
        public static (int? idAlmacen, int? idExpediente) DestinoDeLasEntregas(this PedidoDtm pedido)
            => pedido.IdAlmacen != null ? (pedido.IdAlmacen, null) : (null, pedido.IdExpediente);
    }
}
