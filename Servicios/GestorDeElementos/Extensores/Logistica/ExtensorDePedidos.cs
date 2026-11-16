using ServicioDeDatos.Entorno;
using ServicioDeDatos.Elemento;
using ServicioDeDatos;
using ServicioDeDatos.Logistica;
using System;
using System.Collections.Generic;
using Utilidades;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Juridico;
using static Gestor.Errores.GestorDeErrores;
using System.Linq;
using ServicioDeDatos.Gastos;
using ServicioDeDatos.Ventas;
using ServicioDeDatos.SistemaDocumental;
using System.IO;

namespace GestorDeElementos.Extensores
{
    public static class ExtensorDePedidos
    {
        public static bool EstaEnRecepcion(this PedidoDtm pedido) => pedido.EstaEnLaEtapa(enumEtapasDePedido.PED_Etapa_De_Recepcion);

        public static bool UsaElDetalleDe(Type tipoDeDetalle)
        {
            if (tipoDeDetalle == typeof(LineaDeUnPedidoDtm))
                return true;

            return false;
        }

        public static bool EstaEnLaEtapa(this PedidoDtm pedido, string etapa) => etapa.ToLista<int>(Simbolos.Coma).Contains(pedido.IdEstado);

        // Copia la cabecera, la dirección de entrega (si la tiene) y las líneas del pedido en un pedido nuevo, que queda en el estado inicial
        // (cumplimentación). Las fechas de pedir el y entregar el solo se copian si son posteriores a hoy; no se copian las de recepción
        // y cierre, el archivo del pedido, las entregas ni las devoluciones
        public static PedidoDtm Copiar(this PedidoDtm pedidoOrigen, ContextoSe contexto)
        {
            var transaccion = contexto.IniciarTransaccion();
            try
            {
                var pedidoNuevo = new PedidoDtm
                {
                    IdCg = pedidoOrigen.IdCg,
                    IdTipo = pedidoOrigen.IdTipo,
                    IdEstado = 0,
                    Nombre = pedidoOrigen.Nombre,
                    Descripcion = pedidoOrigen.Descripcion,
                    IdProveedor = pedidoOrigen.IdProveedor,
                    Contacto = pedidoOrigen.Contacto,
                    Telefono = pedidoOrigen.Telefono,
                    eMail = pedidoOrigen.eMail,
                    IdContrato = pedidoOrigen.IdContrato,
                    IdExpediente = pedidoOrigen.IdExpediente,
                    IdAlmacen = pedidoOrigen.IdAlmacen,
                    PedidoEl = FechaPosteriorAHoy(pedidoOrigen.PedidoEl),
                    EntregarEl = FechaPosteriorAHoy(pedidoOrigen.EntregarEl),
                    RecibidoEl = null,
                    CerradoEl = null,
                    IdArchivo = null
                }.Insertar(contexto, new Dictionary<string, object> { { ltrParametrosNeg.Copiando, true } });

                var direccionDeEntrega = pedidoOrigen.Direccion(contexto, enumCalificadorDireccion.entrega);
                if (direccionDeEntrega is not null)
                    GestorDeDirecciones.AsociarDireccion(contexto, enumNegocio.Pedido, pedidoNuevo.Id, direccionDeEntrega.Copiar());

                var lineas = contexto.SeleccionarTodos<LineaDeUnPedidoDtm>(nameof(LineaDeUnPedidoDtm.IdElemento), pedidoOrigen.Id);
                foreach (var linea in lineas.OrderBy(l => l.Orden))
                {
                    new LineaDeUnPedidoDtm
                    {
                        IdElemento = pedidoNuevo.Id,
                        Orden = linea.Orden,
                        TipoDeLinea = linea.TipoDeLinea,
                        IdUnitario = linea.IdUnitario,
                        Concepto = linea.Concepto,
                        IdNaturaleza = linea.IdNaturaleza,
                        Precio = linea.Precio,
                        Cantidad = linea.Cantidad,
                        Descuento = linea.Descuento,
                        IdUnidad = linea.IdUnidad,
                        Anotacion = linea.Anotacion
                    }.Insertar(contexto);
                }

                pedidoNuevo.CrearTraza(contexto, "Pedido copiado", $"Copia del pedido '{pedidoOrigen.Referencia}' con {lineas.Count} líneas, realizada por {contexto.Usuario.Expresion}");
                contexto.Commit(transaccion);
                return pedidoNuevo;
            }
            catch
            {
                contexto.Rollback(transaccion);
                throw;
            }
        }

        private static DateTime? FechaPosteriorAHoy(DateTime? fecha) => fecha is not null && fecha.Value.Date > DateTime.Today ? fecha : null;

        public static int IncrementarOrdenEn(ContextoSe contexto)
        {
            var incremento = enumNegocio.Pedido.LeerCrearParametro(contexto, enumParametrosDePedidos.PED_IncrementarOrdenEn, "10");
            if (incremento is null || incremento.Valor.Entero() == 0)
                Emitir($"Ha de definir el parámetro '{enumParametrosDePedidos.PED_IncrementarOrdenEn}' del negocio de '{enumNegocio.Pedido.Singular()}' con un valor mayor de 0");
            return incremento.Valor.Entero();
        }

        public static ContratoDtm Contrato(this PedidoDtm pedido, ContextoSe contexto, bool aplicarJoin = false, bool errorSiNoHay = true)
        {
            if (pedido.Contrato != null && pedido.IdContrato is not null && (int)pedido.IdContrato == pedido.Contrato.Id) return pedido.Contrato;

            if (pedido.IdContrato is null && errorSiNoHay)
                Emitir($"El pedido '{pedido.Referencia}' no tiene contrato");

            if (pedido.IdContrato is null) return null;

            return pedido.Contrato = contexto.SeleccionarPorId<ContratoDtm>((int)pedido.IdContrato, aplicarJoin);
        }

        public static ExpedienteDtm Expediente(this PedidoDtm pedido, ContextoSe contexto, bool aplicarJoin = false, bool errorSiNoHay = true)
        {
            if (pedido.Expediente != null && pedido.IdExpediente is not null && (int)pedido.IdExpediente == pedido.Expediente.Id) return pedido.Expediente;

            if (pedido.IdExpediente is null && errorSiNoHay)
                Emitir($"El pedido '{pedido.Referencia}' no tiene expediente asociado");

            if (pedido.IdExpediente is null) return null;

            return pedido.Expediente = contexto.SeleccionarPorId<ExpedienteDtm>((int)pedido.IdExpediente, aplicarJoin);
        }

        public static AlmacenDtm Almacen(this PedidoDtm pedido, ContextoSe contexto, bool aplicarJoin = false, bool errorSiNoHay = true)
        {
            if (pedido.Almacen != null && pedido.IdAlmacen is not null && (int)pedido.IdAlmacen == pedido.Almacen.Id)
                return pedido.Almacen;

            if (pedido.IdAlmacen is null && errorSiNoHay)
                Emitir($"El pedido '{pedido.Referencia}' no tiene almacén asociado");

            if (pedido.IdAlmacen is null) return null;

            return pedido.Almacen = contexto.SeleccionarPorId<AlmacenDtm>((int)pedido.IdAlmacen, aplicarJoin);
        }

        // Avance del contrato de compra al que se imputa el pedido (importes sin IVA). Lo que aporta un pedido abierto es:
        //   planificado = importe de sus líneas - lo recibido (entregado menos devuelto)
        //   realizado   = lo recibido
        // Al cerrarlo o cancelarlo lo que queda sin recibir deja de estar planificado, y si se reabre se vuelve a planificar
        public static bool EstaFinalizado(this PedidoDtm pedido)
        => pedido.EstaEnAlgunaDeLasEtapa(new List<enumEtapasDePedido> { enumEtapasDePedido.PED_Etapa_Cerrado, enumEtapasDePedido.PED_Etapa_Cancelado });

        public static decimal PendienteDeRecibir(this PedidoDtm pedido, ContextoSe contexto) => pedido.Importe(contexto) - pedido.Recibido(contexto);

        // Al añadir, modificar o borrar una línea (sólo mientras se cumplimenta) varía lo planificado en la diferencia de importes
        public static void VariarLoPlanificado(this PedidoDtm pedido, ContextoSe contexto, decimal variacion)
        {
            if (variacion != 0)
                pedido.RecalcularAvance(contexto, enumAvaceOperacion.PlanificadoVariado, variacion, 0);
        }

        // Al entregar una línea, lo entregado pasa de planificado a realizado
        public static void RecibirEnElContrato(this PedidoDtm pedido, ContextoSe contexto, decimal importe)
        => pedido.RecalcularAvance(contexto, enumAvaceOperacion.RecibirPedido, importe, importe);

        // Al cancelar una entrega, lo entregado vuelve de realizado a planificado
        public static void AnularRecepcionEnElContrato(this PedidoDtm pedido, ContextoSe contexto, decimal importe)
        => pedido.RecalcularAvance(contexto, enumAvaceOperacion.AnularRecepcionDePedido, importe, importe);

        // Devolver es como cancelar la entrega de lo devuelto; si el pedido ya está cerrado lo devuelto no se espera, sólo sale de lo realizado
        public static void DevolverEnElContrato(this PedidoDtm pedido, ContextoSe contexto, decimal importe)
        => pedido.RecalcularAvance(contexto, enumAvaceOperacion.AnularRecepcionDePedido, pedido.EstaFinalizado() ? 0 : importe, importe);

        // Cancelar una devolución es como volver a entregar lo devuelto
        public static void CancelarDevolucionEnElContrato(this PedidoDtm pedido, ContextoSe contexto, decimal importe)
        => pedido.RecalcularAvance(contexto, enumAvaceOperacion.RecibirPedido, importe, pedido.EstaFinalizado() ? 0 : importe);

        // Al cerrar o cancelar el pedido lo que no se ha recibido deja de estar planificado
        public static void LiberarLoPendienteDelContrato(this PedidoDtm pedido, ContextoSe contexto)
        {
            var pendiente = pedido.PendienteDeRecibir(contexto);
            if (pendiente != 0)
                pedido.RecalcularAvance(contexto, enumAvaceOperacion.EliminarPlanificacion, 0, pendiente);
        }

        // Al reabrir un pedido cerrado o cancelado lo que no se ha recibido se vuelve a planificar
        public static void PlanificarLoPendienteEnElContrato(this PedidoDtm pedido, ContextoSe contexto)
        => pedido.VariarLoPlanificado(contexto, pedido.PendienteDeRecibir(contexto));

        // Al imputar el pedido a un contrato se le suma lo que aporta, y al quitárselo se le resta
        public static void ImputarAlAvanceDelContrato(this PedidoDtm pedido, ContextoSe contexto, ContratoDtm contrato)
        {
            if (!pedido.EstaFinalizado())
                contrato.RecalcularAvance(contexto, enumAvaceOperacion.PlanificadoVariado, pedido.PendienteDeRecibir(contexto), 0);
            contrato.RecalcularAvance(contexto, enumAvaceOperacion.RecibirPedido, pedido.Recibido(contexto), 0);
            pedido.CrearTraza(contexto, "Pedido imputado al contrato", $"El usuario {contexto.DatosDeConexion.Login} ha imputado el pedido '{pedido.Referencia}' al contrato {contrato.Referencia}");
        }

        public static void QuitarDelAvanceDelContrato(this PedidoDtm pedido, ContextoSe contexto, ContratoDtm contrato)
        {
            if (!pedido.EstaFinalizado())
                contrato.RecalcularAvance(contexto, enumAvaceOperacion.EliminarPlanificacion, 0, pedido.PendienteDeRecibir(contexto));
            contrato.RecalcularAvance(contexto, enumAvaceOperacion.AnularRecepcionDePedido, 0, pedido.Recibido(contexto));
            pedido.CrearTraza(contexto, "Pedido excluido del contrato", $"El usuario {contexto.DatosDeConexion.Login} ha excluido el pedido '{pedido.Referencia}' del contrato {contrato.Referencia}");
        }

        private static void RecalcularAvance(this PedidoDtm pedido, ContextoSe contexto, enumAvaceOperacion operacion, decimal incremento, decimal decremento)
        {
            if (pedido.IdContrato is null)
                return;
            pedido.Contrato(contexto).RecalcularAvance(contexto, operacion, incremento, decremento);
        }

        public static decimal Importe(this PedidoDtm pedido, ContextoSe contexto)
        =>
        pedido.Detalles<LineaDeUnPedidoDtm>(contexto).Sum(linea => linea.ImporteDeLinea);

        // Importe de lo recibido del pedido, sumado en la BD: RecepcionDeLineaDtm.Importe es calculado (no está en la tabla) y EF no
        // lo puede traducir, se repite aquí su fórmula; si cambia una ha de cambiar la otra
        // Lo entregado menos lo devuelto; las fórmulas repiten RecepcionDeLineaDtm.Importe y ExtensorDeDevolucionesDePedido.Importe para
        // sumarlas en la BD, si cambian éstas han de cambiar aquí
        public static decimal Recibido(this PedidoDtm pedido, ContextoSe contexto)
        {
            var entregado = contexto.Set<RecepcionDeLineaDtm>()
                .Where(r => contexto.Set<LineaDeUnPedidoDtm>().Any(l => l.Id == r.IdLinea && l.IdElemento == pedido.Id))
                .Sum(r => r.Cantidad * r.Precio * (1 - (r.Descuento ?? 0) / 100));
            var devuelto = contexto.Set<DevolucionDeEntregaDtm>()
                .Where(d => d.LineaDeRecepcion.Linea.IdElemento == pedido.Id)
                .Sum(d => (decimal?)(d.Cantidad * d.LineaDeRecepcion.Precio * (1 - (d.LineaDeRecepcion.Descuento ?? 0) / 100))) ?? 0m;
            return entregado - devuelto;
        }

        public static void Validar(this PedidoDtm pedido, ContextoSe contexto, ParametrosDeNegocio parametros)
        {
            pedido.ValidarFechas(parametros);
            pedido.ValidarExpediente(contexto);
            pedido.ValidarContrato(contexto, parametros);
            pedido.ValidarSociedadDelDestino(contexto, pedido.IdAlmacen, pedido.IdExpediente);
        }

        // El almacén y el expediente (de la cabecera o de una entrega) han de ser de la misma sociedad que el pedido
        public static void ValidarSociedadDelDestino(this PedidoDtm pedido, ContextoSe contexto, int? idAlmacen, int? idExpediente)
        {
            var idSociedad = pedido.Cg(contexto).IdSociedad;
            if (idAlmacen is not null)
            {
                var almacen = contexto.SeleccionarPorId<AlmacenDtm>((int)idAlmacen);
                if (almacen.Cg(contexto).IdSociedad != idSociedad)
                    Emitir($"El almacén '{almacen.Expresion}' no es de la misma sociedad que el pedido '{pedido.Referencia}'");
            }
            if (idExpediente is not null)
            {
                var expediente = contexto.SeleccionarPorId<ExpedienteDtm>((int)idExpediente);
                if (expediente.Cg(contexto).IdSociedad != idSociedad)
                    Emitir($"El expediente '{expediente.Expresion}' no es de la misma sociedad que el pedido '{pedido.Referencia}'");
            }
        }

        public static void ValidarFechas(this PedidoDtm pedido, ParametrosDeNegocio parametros)
        {
            var seAcabaDePedirOSeHaCambiadoLaFechaDePedido = parametros.Insertando || pedido.PropiedadCambiada<DateTime?>(nameof(PedidoDtm.PedidoEl), parametros);
            if (seAcabaDePedirOSeHaCambiadoLaFechaDePedido && pedido.PedidoEl is not null && pedido.PedidoEl.Fecha().Date < DateTime.Today)
            {
                Emitir($"El pedido '{pedido.Referencia}' no se puede pedir el '{pedido.PedidoEl.Fecha().ToString("dd-MM-yyyy")}' por ser anterior al día de hoy");
            }
            if (pedido.PedidoEl is not null && pedido.EntregarEl is not null && pedido.PedidoEl > pedido.EntregarEl)
            {
                Emitir($"El pedido '{pedido.Referencia}' no se puede entregar el '{pedido.EntregarEl.Fecha().ToString("dd-MM-yyyy")}' ya que se ha pedido el '{pedido.PedidoEl.Fecha().ToString("dd-MM-yyyy")}' ");
            }
            if (pedido.RecibidoEl is not null && pedido.CerradoEl is not null && pedido.RecibidoEl.Fecha().Date > pedido.CerradoEl.Fecha().Date)
            {
                Emitir($"El pedido '{pedido.Referencia}' no se puede cerrar el '{pedido.CerradoEl.Fecha().ToString("dd-MM-yyyy")}' por haber sido recibido el '{pedido.RecibidoEl.Fecha().ToString("dd-MM-yyyy")}' ");
            }

            if (pedido.EstaEnAlgunaDeLasEtapa(new List<enumEtapasDePedido> { enumEtapasDePedido.PED_Etapa_De_Recepcion, enumEtapasDePedido.PED_Etapa_Cerrado }))
            {
                if (pedido.RecibidoEl is null || pedido.PedidoEl is null || pedido.RecibidoEl.Fecha().Date < pedido.PedidoEl.Fecha().Date)
                    Emitir($"El pedido '{pedido.Referencia}' ha de indicar una fecha de recepción igual o posterior a la fecha de pedido '{pedido.PedidoEl.Fecha().ToString("dd-MM-yyyy")}'");
            }
            else if (pedido.RecibidoEl is not null)
            {
                Emitir($"El pedido '{pedido.Referencia}' no puede tener fecha de recepción si no está en la etapa de recepción o cerrado");
            }
        }

        public static void ValidarContrato(this PedidoDtm pedido, ContextoSe contexto, ParametrosDeNegocio parametros)
        {
            if (pedido.IdContrato is null)
                return;

            // un pedido es una compra, sólo se imputa a contratos de compra
            var contrato = pedido.Contrato(contexto);
            var seImputa = parametros.Insertando || pedido.PropiedadCambiada<int?>(nameof(PedidoDtm.IdContrato), parametros);
            if (seImputa && contrato.ClaseDeContrato != enumClaseDeContrato.Compra)
                Emitir($"El pedido '{pedido.Referencia}' sólo se puede imputar a un contrato de {enumClaseDeContrato.Compra.Descripcion()}, y '{contrato.Referencia}' es de {contrato.ClaseDeContrato.Descripcion()}");

            if (pedido.PedidoEl is null)
                return;

            var hitos = contrato.Hitos(contexto, enumEtapasDeContratos.CTR_Etapa_Vigente.Lista());
            var vigente = hitos.Any(hito => hito.Fecha.Date <= pedido.PedidoEl && (hito.Fin == null || ((DateTime)hito.Fin).Date.AddDays(1) > pedido.PedidoEl));
            if (!vigente)
            {
                Emitir($"El contrato '{contrato.Referencia}' no está o estuvo vigente para la fecha '{pedido.PedidoEl.Fecha().ToString("dd-MM-yyyy")}'");
            }
        }

        public static void ValidarExpediente(this PedidoDtm pedido, ContextoSe contexto)
        {
            if (pedido.IdExpediente is null)
                return;

            var expediente = pedido.Expediente(contexto);
            if (expediente.Estado(contexto).Cancelado)
            {
                Emitir($"El expediente '{expediente.Referencia}' está cancelado, no puede imputarle el pedido '{pedido.Referencia}");
            }
        }

        // PedidoEl, mientras el pedido se cumplimenta o se aprueba, es la fecha planificada para solicitarlo (un trabajo del sistema lo
        // transitará a solicitado al llegar dicha fecha); una vez solicitado es la fecha en la que se solicitó
        public static void AntesDeSolicitar(this PedidoDtm pedido, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            pedido.ValidarDestinoDeLasEntregas();
            pedido.ValidarLineasParaElAlmacen(contexto);
            if (pedido.Proveedor(contexto).Baja)
            {
                pedido.Proveedor(contexto).IndicarQueEstaDeBaja(contexto);
            }
            pedido.PedidoEl = DateTime.Today;
        }

        // Al volver de solicitado a cumplimentación o aprobación el pedido deja de estar solicitado. Si su archivo es el pdf que generó el sistema
        // al solicitarlo deja de ser el archivo del pedido y se renombra como antiguo, quedando anexado como constancia de que se emitió;
        // si lo subió el usuario mientras lo cumplimentaba o aprobaba, lo sigue siendo
        public static void AntesDeCancelarSolicitud(this PedidoDtm pedido, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            pedido.PedidoEl = null;

            if (pedido.IdArchivo is null || !pedido.ArchivoGeneradoAlSolicitar(contexto))
                return;

            pedido.RenombrarComoAntiguo(contexto, (int)pedido.IdArchivo);
            var firmado = contexto.Set<FirmadoDtm>().FirstOrDefault(x => x.IdOriginal == (int)pedido.IdArchivo);
            if (firmado is not null)
                pedido.RenombrarComoAntiguo(contexto, firmado.IdFirmado);

            pedido.IdArchivo = null;
        }

        // Al pasar de solicitado a la etapa de entrega (o directamente a cerrado) se recibe hoy; si se recibió antes se corrige con "Modificar cabecera"
        // Al recepcionar una línea o el pedido completo se indica la fecha de la entrega (ver ExtensorDeRecepciones.PasarARecepcion), si no es hoy
        public static void AntesDeRecibir(this PedidoDtm pedido, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            pedido.RecibidoEl = parametros.LeerValor<DateTime?>(nameof(PedidoDtm.RecibidoEl), null) ?? DateTime.Today;
        }

        // Al cerrar el pedido (desde la etapa de entrega o, con "Dar por recibido", desde solicitado) se cierra hoy; se corrige con "Modificar cabecera"
        public static void AntesDeCerrar(this PedidoDtm pedido, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            pedido.CerradoEl = DateTime.Today;
        }

        // Al reabrir el pedido cerrado deja de estar cerrado
        public static void AntesDeReabrir(this PedidoDtm pedido, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            pedido.CerradoEl = null;
        }

        // Al devolver el pedido de la etapa de entrega a solicitado deja de estar recibido; solicitado no puede haber nada entregado,
        // así que antes se han de cancelar todas las entregas de sus líneas (ver GestorDeEntregasDeUnPedido.CancelarEntrega)
        public static void AntesDeAnularRecepcion(this PedidoDtm pedido, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            if (pedido.TieneLineasRecepcionadas(contexto))
                Emitir($"El pedido '{pedido.Referencia}' tiene líneas recepcionadas, anule todas las recepciones antes de devolverlo a {enumEtapasDePedido.PED_Etapa_De_Solicitud.Nombre()}");
            pedido.RecibidoEl = null;
        }

        // El archivo que sube el usuario se crea mientras el pedido se cumplimenta o se aprueba; el que genera el sistema, al entrar en la etapa de solicitud
        public static bool ArchivoGeneradoAlSolicitar(this PedidoDtm pedido, ContextoSe contexto)
        {
            var entradaEnSolicitud = pedido.EntradaEnLaEtapaDeSolicitud(contexto);
            if (entradaEnSolicitud is null)
                return false;

            return contexto.SeleccionarPorId<ArchivoDtm>((int)pedido.IdArchivo).FechaCreacion >= entradaEnSolicitud;
        }

        // Evento de entrega en la agenda de la sociedad, como el de vencimiento de las facturas: se anota al solicitar el pedido en la fecha de
        // entrega; si se recibe antes se mueve a la fecha de recepción y si se anula la recepción vuelve a la de entrega. Se elimina al cancelar
        // la solicitud o el pedido. Sin fecha de entrega ni de recepción no hay evento
        public static void ModificarEventoDeEntrega(this PedidoDtm pedido, ContextoSe contexto)
        {
            var sociedad = pedido.Sociedad(contexto);
            if (sociedad.IdAgenda is not null)
                pedido.EliminarEventoDeEntrega(contexto);
            pedido.AnotarEventoDeEntrega(contexto);
        }

        public static void EliminarEventoDeEntrega(this PedidoDtm pedido, ContextoSe contexto)
        {
            var eventos = contexto.SeleccionarEventos(pedido.Id, pedido.NombreDelEventoDeEntrega());
            var p = new Dictionary<string, object> { { ltrParametrosNeg.EstaEjecutandoUnaAccion, true } };
            foreach (var e in eventos.Where(e => e.EsDelSistema && e.IdNegocio == enumNegocio.Pedido.IdNegocio())) pedido.Desvincular(contexto, e, p);
        }

        private static void AnotarEventoDeEntrega(this PedidoDtm pedido, ContextoSe contexto)
        {
            var fecha = pedido.FechaDelEventoDeEntrega();
            if (fecha is null)
                return;

            var sociedad = pedido.Sociedad(contexto);
            var agenda = sociedad.IdAgenda == null ? sociedad.CrearAgenda(contexto) : contexto.SeleccionarPorId<AgendaDtm>((int)sociedad.IdAgenda);

            var evento = new EventoDeAgendaDtm();
            evento.IdAgenda = agenda.Id;
            evento.IdElemento = pedido.Id;
            evento.IdNegocio = enumNegocio.Pedido.IdNegocio();
            evento.EsDelSistema = true;
            evento.Inicio = (DateTime)fecha;
            evento.Nombre = pedido.NombreDelEventoDeEntrega();
            evento.Descripcion = $"Entrega del pedido: {pedido.Referencia}";
            evento.Fin = evento.Inicio;
            GestorDeVinculos.Vincular(contexto, pedido, evento, new Dictionary<string, object> { { ltrParametrosNeg.EstaEjecutandoUnaAccion, true } });
        }

        private static string NombreDelEventoDeEntrega(this PedidoDtm pedido)
        => ltrDeUnPedido.EventoDeEntrega.Replace($"[{nameof(PedidoDtm.Referencia)}]", pedido.Referencia);

        // La de recepción si se ha recibido antes de la fecha de entrega, si no la de entrega
        private static DateTime? FechaDelEventoDeEntrega(this PedidoDtm pedido)
        {
            if (pedido.RecibidoEl is not null && (pedido.EntregarEl is null || pedido.RecibidoEl.Value.Date < pedido.EntregarEl.Value.Date))
                return pedido.RecibidoEl.Value.Date;
            return pedido.EntregarEl?.Date;
        }

        // Fecha del primer hito de la racha actual de estados de la etapa de solicitud (los hitos vienen ordenados del más reciente al más antiguo)
        private static DateTime? EntradaEnLaEtapaDeSolicitud(this PedidoDtm pedido, ContextoSe contexto)
        {
            var estadosDeSolicitud = enumEtapasDePedido.PED_Etapa_De_Solicitud.Lista();
            DateTime? entrada = null;
            foreach (var hito in pedido.Hitos(contexto))
            {
                if (!estadosDeSolicitud.Contains(hito.IdEstado))
                    break;
                entrada = hito.Fecha;
            }
            return entrada;
        }

        private static void RenombrarComoAntiguo(this PedidoDtm pedido, ContextoSe contexto, int idArchivo)
        {
            var archivo = contexto.SeleccionarPorId<ArchivoDtm>(idArchivo);
            archivo.Nombre = Path.GetFileNameWithoutExtension(archivo.Nombre) + "-old" + Path.GetExtension(archivo.Nombre);
            archivo.Nombre = pedido.ProponerNombreDeArchivo(contexto, archivo.Nombre);
            archivo.Modificar(contexto, ltrDeUnArchivo.Accion_Permitir_Modificar_Nombre);
        }
    }
}
