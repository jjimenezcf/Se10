using ServicioDeDatos.Entorno;
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

        public static int IncrementarOrdenEn(ContextoSe contexto)
        {
            var incremento = enumNegocio.Pedido.LeerCrearParametro(contexto, enumParametrosDePedidos.PED_IncrementarOrdenEn, "10");
            if (incremento is null || incremento.Valor.Entero() == 0)
                Emitir($"Ha de definir el parámetro '{enumParametrosDePedidos.PED_IncrementarOrdenEn}' del negocio de '{enumNegocio.Pedido.Singular()}' con un valor mayor de 0");
            return incremento.Valor.Entero();
        }

        public static ContratoDtm Contrato(this PedidoDtm pedido, ContextoSe contexto, bool aplicarJoin = false, bool errorSiNoHay = true)
        {
            if (pedido.Contrato != null) return pedido.Contrato;

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

        public static void IncrementarLoPlanificado(this PedidoDtm pedido, ContextoSe contexto, ContratoDtm contrato)
        {
            var avance = contrato.Ampliacion<AvanceDtm>(contexto);
            avance.Planificado = avance.Planificado + pedido.Importe(contexto);
            avance.Modificar(contexto);
        }

        public static void DecrementarLoPlanificado(this PedidoDtm pedido, ContextoSe contexto, ContratoDtm contrato)
        {
            var avance = contrato.Ampliacion<AvanceDtm>(contexto);
            avance.Planificado = avance.Planificado - pedido.Importe(contexto);
            avance.Modificar(contexto);
        }

        public static void ModificarSaldosDelContrato(this PedidoDtm pedido, ContextoSe contexto, int idContrato)
        {
            var contrato = contexto.SeleccionarPorId<ContratoDtm>(idContrato);
            var avance = contrato.Ampliacion<AvanceDtm>(contexto);

            if (pedido.IdContrato is not null)
            {
                avance.Planificado = avance.Planificado + pedido.Importe(contexto);
                pedido.CrearTraza(contexto, "pedido imputado al contrato", $"El usuario {contexto.DatosDeConexion.Login} ha imputado el pedido '{pedido.Referencia}' al contrato {contrato.Referencia}");
            }
            else
            {
                avance.Planificado = avance.Planificado + pedido.Importe(contexto);
                pedido.CrearTraza(contexto, "pedido excluido del contrato", $"El usuario {contexto.DatosDeConexion.Login} ha excluido el pedido '{pedido.Referencia}' del contrato {contrato.Referencia}");
            }
            avance.Modificar(contexto, nameof(ModificarSaldosDelContrato));
        }

        public static decimal Importe(this PedidoDtm pedido, ContextoSe contexto)
        =>
        pedido.Detalles<LineaDeUnPedidoDtm>(contexto).Sum(linea => linea.ImporteDeLinea);

        // Importe de lo recibido del pedido, sumado en la BD: RecepcionDeLineaDtm.Importe es calculado (no está en la tabla) y EF no
        // lo puede traducir, se repite aquí su fórmula; si cambia una ha de cambiar la otra
        public static decimal Recibido(this PedidoDtm pedido, ContextoSe contexto)
            =>
            contexto.Set<RecepcionDeLineaDtm>()
                .Where(r => contexto.Set<LineaDeUnPedidoDtm>().Any(l => l.Id == r.IdLinea && l.IdElemento == pedido.Id))
                .Sum(r => r.Cantidad * r.Precio * (1 - (r.Descuento ?? 0) / 100));

        public static void Validar(this PedidoDtm pedido, ContextoSe contexto, ParametrosDeNegocio parametros)
        {
            pedido.ValidarFechas(parametros);
            pedido.ValidarExpediente(contexto);
            pedido.ValidarContrato(contexto);
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

        public static void ValidarContrato(this PedidoDtm pedido, ContextoSe contexto)
        {
            if (pedido.IdContrato is null || pedido.PedidoEl is null)
                return;

            var contrato = pedido.Contrato(contexto);
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
        // así que antes se han de cancelar todas las entregas de sus líneas (ver ExtensorDeRecepciones.LineaNoEntregada)
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

        private static void AnotarEventoDeEntrega(this PedidoDtm pedido, ContextoSe contexto)
        {
            var sociedad = pedido.Sociedad(contexto);
            var agenda = sociedad.IdAgenda == null ? sociedad.CrearAgenda(contexto) : contexto.SeleccionarPorId<AgendaDtm>((int)sociedad.IdAgenda);

            var evento = new EventoDeAgendaDtm();
            evento.IdAgenda = agenda.Id;
            evento.IdElemento = pedido.Id;
            evento.IdNegocio = enumNegocio.Pedido.IdNegocio();
            evento.EsDelSistema = true;
            evento.Inicio = ((DateTime)pedido.EntregarEl).Date;
            evento.Nombre = ltrDeUnPedido.EventoDeEntrega.Replace($"[{nameof(PedidoDtm.Referencia)}]", pedido.Referencia);
            evento.Descripcion = $"Entrega del pedido: {pedido.Referencia}";
            evento.Fin = evento.Inicio;
            GestorDeVinculos.Vincular(contexto, pedido, evento, new Dictionary<string, object> { { ltrParametrosNeg.EstaEjecutandoUnaAccion, true } });
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
