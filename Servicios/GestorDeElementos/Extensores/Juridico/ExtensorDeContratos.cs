using System;
using System.Collections.Generic;
using System.Net.Mime;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Gastos;
using ServicioDeDatos.Guarderias;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.Logistica;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Ventas;
using Utilidades;
using static Gestor.Errores.GestorDeErrores;

namespace GestorDeElementos.Extensores
{
    public static class ExtensorDeContratos
    {
        public static bool UsaLaAmpliacionDe(this ContratoDtm contrato, ContextoSe contexto, Type tipoAmpliacion)
        =>
        UsaLaAmpliacionDe(contexto, contrato.IdTipo, tipoAmpliacion);

        public static bool UsaLaAmpliacionDe(ContextoSe contexto, int idTipo, Type tipoAmpliacion)
        {
            var tipoDtm = (TipoDeContratoDtm)enumNegocio.Contrato.CrearGestorDeTipo(contexto).LeerRegistroPorId(idTipo, aplicarJoin: false);
            if (tipoDtm.ClaseDeContrato == enumClaseDeContrato.Venta && (
                     tipoAmpliacion == typeof(AvanceDtm) || tipoAmpliacion == typeof(SaldosDelContratoDtm) ||
                     tipoAmpliacion == typeof(ProrrogaDtm) || tipoAmpliacion == typeof(DatosDelContratoDtm) ||
                     tipoAmpliacion == typeof(AvalSolicitadoDtm)))
                return true;

            if (tipoDtm.ClaseDeContrato == enumClaseDeContrato.Compra && (
                     tipoAmpliacion == typeof(AvanceDtm) || tipoAmpliacion == typeof(SaldosDelContratoDtm) ||
                     tipoAmpliacion == typeof(ProrrogaDtm) || tipoAmpliacion == typeof(DatosDelContratoDtm) ||
                     tipoAmpliacion == typeof(AvalSolicitadoDtm)))
                return true;

            if (tipoDtm.ClaseDeContrato == enumClaseDeContrato.MatriculaDeGuarderia && tipoAmpliacion == typeof(MatriculaDeGuarderiaDtm))
                return true;

            return false;
        }

        public static bool UsaElDetalleDe(ContextoSe contexto, int idTipo, Type tipoDeDetalle)
        {
            var tipoDtm = (TipoDeContratoDtm)enumNegocio.Contrato.CrearGestorDeTipo(contexto).LeerRegistroPorId(idTipo, aplicarJoin: false);

            if (tipoDtm.ClaseDeContrato == enumClaseDeContrato.Venta &&
                (tipoDeDetalle == typeof(NaturalezasDeUnContratoDtm) || tipoDeDetalle == typeof(ServiciosDeUnContratoDtm)))
                return true;

            return false;
        }

        public static void ValidarPorcentageDeBloqueo(this SaldosDelContratoDtm saldos, ContextoSe contexto)
        {
            var contrato = saldos.AmpliacionDe<ContratoDtm>(contexto);
            ValidarBloqueoInterno(contexto, contrato, saldos, avance: null);
        }

        public static void ValidarPorcentageDeBloqueo(this AvanceDtm avance, ContextoSe contexto)
        {
            var contrato = avance.AmpliacionDe<ContratoDtm>(contexto);
            ValidarBloqueoInterno(contexto, contrato, saldos: null, avance);
        }

        private static void ValidarBloqueoInterno(ContextoSe contexto, ContratoDtm contrato, SaldosDelContratoDtm saldos, AvanceDtm avance)
        {
            saldos = saldos == null ? contrato.Ampliacion<SaldosDelContratoDtm>(contexto, errorSiNoHay: false) : saldos;
            avance = avance == null ? contrato.Ampliacion<AvanceDtm>(contexto, errorSiNoHay: false) : avance;

            //si se está creando un contrato, los saldos y el avance aun no están constituidos
            if (avance == null || saldos == null)
                return;

            var importeDeAvance = avance.Cobrado + avance.Facturado + avance.Realizado + avance.Planificado;
            if (importeDeAvance == 0)
                return;
            var cuantoMeQueda = (saldos.Importe + saldos.Adendado) - importeDeAvance;
            var cuantoMePuedoPasar = (saldos.Importe + saldos.Adendado) - (saldos.Importe + saldos.Adendado) * 100 / saldos.Bloqueo;

            if (cuantoMeQueda > cuantoMePuedoPasar) return;

            Emitir($"Se ha sobrepasado el porcentage de bloqueo para el contrato '{contrato.Expresion}', importe del contrato {(saldos.Importe + saldos.Adendado).ToString("0,0.00")}, importe imputado {importeDeAvance.ToString("0,0.00")}");
        }

        public static bool SobrepasaPorcentajeDeNotificacion(this ContratoDtm contrato, ContextoSe contexto, SaldosDelContratoDtm saldos)
        {
            if (contrato.ClaseDeContrato != enumClaseDeContrato.Venta || contrato.ClaseDeContrato != enumClaseDeContrato.Compra)
                return false;

            var avance = contrato.Ampliacion<AvanceDtm>(contexto);
            var importeDeAvance = avance.Cobrado + avance.Facturado + avance.Realizado + avance.Planificado;
            if (importeDeAvance == 0)
                return false;

            var cuantoMeQueda = (saldos.Importe + saldos.Adendado) - importeDeAvance;
            var cuantoMePuedoPasar = (saldos.Importe + saldos.Adendado) - (saldos.Importe + saldos.Adendado) * 100 / saldos.Aviso;

            return cuantoMeQueda < cuantoMePuedoPasar;
        }

        public static bool EstaEnLaEtapa(this ContratoDtm contrato, string etapa) => etapa.ToLista<int>(Simbolos.Coma).Contains(contrato.IdEstado);

        public static bool EstaVigente(this ContratoDtm contrato) => contrato.EstaEnLaEtapa(VariablesDeContratos.etapaVigente);
        public static bool EstaCancelado(this ContratoDtm contrato) => contrato.EstaEnLaEtapa(VariablesDeContratos.etapaCancelado);
        public static bool EstaTerminado(this ContratoDtm contrato) => contrato.EstaEnLaEtapa(VariablesDeContratos.etapaFinalizacion);

        public static bool EstaEnElaboracion(this ContratoDtm contrato) => contrato.EstaEnLaEtapa(VariablesDeContratos.etapaDeElaboracion);

        public static bool EstaPdtDeProroga(this ContratoDtm contrato) => contrato.EstaEnLaEtapa(VariablesDeContratos.etapaPdtProrroga);

        public static void InicializarEtapas(ContextoSe contexto)
        {
            var estados = contexto.Estados<EstadoDeUnContratoDtm>(nameof(EstadoDtm.Inicial), true);
            var iniciales = "";
            var vigentes = "";
            foreach (EstadoDtm estado in estados)
            {
                iniciales = $"{(iniciales.IsNullOrEmpty() ? estado.Id.ToString() : $"{iniciales},{estado.Id}")}";
                var siguientes = estado.Siguientes(contexto, enumNegocio.Contrato);

                foreach (EstadoDtm siguiente in siguientes)
                {
                    if (siguiente.Cancelado || siguiente.Terminado)
                        continue;

                    vigentes = $"{(vigentes.IsNullOrEmpty() ? siguiente.Id.ToString() : $"{vigentes},{siguiente.Id}")}";
                }
            }

            var estadosCancelados = "";
            var cancelados = contexto.Estados<EstadoDeUnContratoDtm>(nameof(EstadoDtm.Cancelado), true);
            foreach (EstadoDtm cancelado in cancelados)
            {
                estadosCancelados = $"{(estadosCancelados.IsNullOrEmpty() ? cancelado.Id.ToString() : $"{estadosCancelados},{cancelado.Id}")}";
            }

            var estadosTerminados = "";
            var terminados = contexto.Estados<EstadoDeUnContratoDtm>(nameof(EstadoDtm.Terminado), true);
            var estadosDerogados = VariablesDeContratos.etapaDerogado;
            foreach (EstadoDtm terminado in terminados)
            {
                if (estadosDerogados.ToLista<int>(Simbolos.Coma).Contains(terminado.Id))
                    continue;
                if (estadosCancelados.ToLista<int>(Simbolos.Coma).Contains(terminado.Id))
                    continue;
                estadosTerminados = $"{(estadosTerminados.IsNullOrEmpty() ? terminado.Id.ToString() : $"{estadosTerminados},{terminado.Id}")}";
            }


            var pdtDeProrrogar = "";
            var todos = contexto.Estados<EstadoDeUnContratoDtm>();
            foreach (EstadoDtm estado in todos)
            {
                if (iniciales.IndexOf(estado.Id.ToString()) == -1 &&
                    vigentes.IndexOf(estado.Id.ToString()) == -1 &&
                    !estadosCancelados.ToLista<int>(Simbolos.Coma).Contains(estado.Id) &&
                    !estadosTerminados.ToLista<int>(Simbolos.Coma).Contains(estado.Id) &&
                    !estadosDerogados.ToLista<int>(Simbolos.Coma).Contains(estado.Id)
                    )
                    pdtDeProrrogar = $"{(pdtDeProrrogar.IsNullOrEmpty() ? estado.Id.ToString() : $"{pdtDeProrrogar},{estado.Id}")}";
            }
            enumNegocio.Contrato.IncluirEnParametro(contexto, enumEtapasDeContratos.CTR_Etapa_En_Elaboracion, iniciales);
            enumNegocio.Contrato.IncluirEnParametro(contexto, enumEtapasDeContratos.CTR_Etapa_Vigente, vigentes);
            enumNegocio.Contrato.IncluirEnParametro(contexto, enumEtapasDeContratos.CTR_Etapa_Pdt_Prorroga, pdtDeProrrogar);
            enumNegocio.Contrato.IncluirEnParametro(contexto, enumEtapasDeContratos.CTR_Etapa_Finalizacion, estadosTerminados);
            enumNegocio.Contrato.IncluirEnParametro(contexto, enumEtapasDeContratos.CTR_Etapa_Cancelado, estadosCancelados);
        }

        public static void EventoNotificado(this ContratoDtm contrato, ContextoSe contexto, EventoDeAgendaDtm evento)
        {
            var datos = contrato.Ampliacion<DatosDelContratoDtm>(contexto);

            if (datos.AvisarAntesDe.Entero() > 0)
            {
                if (ltrDatosDelContrato.AvisoPrevioDeFinDeContrato.Equals(evento.Nombre) && datos.FinContrato != default && datos.AvisarAntesDe != default)
                {
                    var fechaDeAvisoPrevio = ((DateTime)datos.FinContrato).AddMonths(-(int)datos.AvisarAntesDe);
                    if (fechaDeAvisoPrevio.Date == evento.Inicio)
                    {
                        datos.RecordatorioEnviado = true;
                        datos.Modificar(contexto, new Dictionary<string, object> { { ltrParametrosNeg.EsUnaPeticion, false } });
                    }
                }
            }
        }

        public static void SiHayAvalHayFechaDeFinDeContrato(this ContratoDtm contrato, ContextoSe contexto)
        {
            var datos = contrato.Ampliacion<DatosDelContratoDtm>(contexto, false);
            var aval = contrato.Ampliacion<AvalSolicitadoDtm>(contexto, false);
            if (aval != null && datos != null && (aval.MesesDeAval.Entero() > 0 || aval.ImporteAval > 0) && datos.FinContrato == null)
                Emitir($"No se puede definir la solicitud de un aval sin fecha de fin de contrato");
        }

        public static enumEtapasDeContratos Etapa(this ContratoDtm contrato)
        {
            if (contrato.EstaEnLaEtapa(VariablesDeContratos.etapaDeElaboracion)) return enumEtapasDeContratos.CTR_Etapa_En_Elaboracion;
            else
            if (contrato.EstaEnLaEtapa(VariablesDeContratos.etapaVigente)) return enumEtapasDeContratos.CTR_Etapa_Vigente;
            else
            if (contrato.EstaEnLaEtapa(VariablesDeContratos.etapaCancelado)) return enumEtapasDeContratos.CTR_Etapa_Cancelado;
            else
            if (contrato.EstaEnLaEtapa(VariablesDeContratos.etapaDerogado)) return enumEtapasDeContratos.CTR_Etapa_Derogado;
            else
            if (contrato.EstaEnLaEtapa(VariablesDeContratos.etapaPdtProrroga)) return enumEtapasDeContratos.CTR_Etapa_Pdt_Prorroga;
            else
            if (contrato.EstaEnLaEtapa(VariablesDeContratos.etapaFinalizacion)) return enumEtapasDeContratos.CTR_Etapa_Finalizacion;

            throw new Exception($"No se ha definido la etapa del contrato, " +
                $"cuando éste está en el estado {contrato.Propiedad<EstadoDtm>(typeof(EstadoDeUnContratoDtm)).Nombre}");
        }

        public static void AntesDeDerogarUnContrato(this ContratoDtm contrato, ContextoSe contexto)
        {
            var planificaciones = contexto.SeleccionarTodos<PlanificacionDeVentaDtm>(nameof(PlanificacionDeVentaDtm.IdContrato), contrato.Id, parametros: new Dictionary<string, object> {
                {ltrParametrosNeg.ExcluirCancelados, true },
                {ltrParametrosNeg.ExcluirTerminados, true }});
            foreach (var planificacion in planificaciones)
                planificacion.Cancelar(contexto);

            var partesTr = contexto.SeleccionarTodos<ParteTrDtm>(nameof(ParteTrDtm.IdContrato), contrato.Id, parametros: new Dictionary<string, object> {
                {ltrParametrosNeg.ExcluirCancelados, true },
                {ltrParametrosNeg.ExcluirTerminados, true }});
            foreach (var parte in partesTr)
                parte.Cancelar(contexto);

            var eventos = contexto.SeleccionarEventos(contrato.IdAgenda, contrato.Id, DateTime.Now);
            EliminarEventos(contexto, contrato, eventos);
        }

        public static void AntesDeCancelarUnContrato(this ContratoDtm contrato, ContextoSe contexto)
        {
            if (contexto.Existen<PlanificadorDeVentaDtm>(nameof(PlanificadorDeVentaDtm.IdContrato), contrato.Id))
                Emitir("No se puede cancelar un contrato con planificaciones, suprímalas");

            if (contexto.Existen<FacturaRecDtm>(nameof(FacturaRecDtm.IdContrato), contrato.Id))
                Emitir("No se puede cancelar un contrato con facturas imputadas, quítelas");

            if (contexto.Existen<PedidoDtm>(nameof(PedidoDtm.IdContrato), contrato.Id))
                Emitir("No se puede cancelar un contrato con pedidos imputados, quítelas");

            var parametros = new Dictionary<string, object> { { ltrParametrosNeg.EstaEjecutandoUnaAccion, true } };
            contrato.DesvincularTodos<EventoDeAgendaDtm>(contexto, parametros);
        }

        public static void AntesDeIniciarUnContrato(this ContratoDtm contrato, ContextoSe contexto)
        {
            if (contrato.IdResponsable == default)
            {
                Emitir($"El contrato {contrato.Referencia} ha de tener un responsable antes de pasarlo a vigente");
            }

            if (contrato.ClaseDeContrato == enumClaseDeContrato.Venta && contrato.Ampliacion<DatosDelContratoDtm>(contexto).IdCliente == default)
                Emitir($"El contrato {contrato.Referencia} ha de tener un cliente antes de pasarlo a vigente");

            if (contrato.ClaseDeContrato == enumClaseDeContrato.MatriculaDeGuarderia && contrato.Ampliacion<MatriculaDeGuarderiaDtm>(contexto).IdCliente == default)
                Emitir($"El contrato {contrato.Referencia} ha de tener un cliente antes de pasarlo a vigente");

            if (contrato.ClaseDeContrato == enumClaseDeContrato.Compra && contrato.Ampliacion<DatosDelContratoDtm>(contexto).IdProveedor == default)
                Emitir($"El contrato {contrato.Referencia} ha de tener un proveedor antes de pasarlo a vigente");

            if (contrato.ClaseDeContrato == enumClaseDeContrato.Venta &&
                contrato.Ampliacion<SaldosDelContratoDtm>(contexto).Importe == default &&
                contrato.Ampliacion<SaldosDelContratoDtm>(contexto).Importe <= 0)
                Emitir($"El contrato {contrato.Referencia} ha de tener un importe mayor de cero");

            if (contrato.ClaseDeContrato != enumClaseDeContrato.MatriculaDeGuarderia)
            {
                contrato.SiNoHayFechaFinNoPuedeSerProrrogable(contexto);
                contrato.SiEsProrrogableFechaProrrogaMayorFechaFinMasMeses(contexto);
                contrato.AnotarEventosDelContrato(contexto);
            }
        }

        public static void AntesDeDevolverAEnElaboracion(this ContratoDtm contrato, ContextoSe contexto)
        {
            if (contexto.Existen<FacturaRecDtm>(nameof(FacturaRecDtm.IdContrato), contrato.Id))
                Emitir("No se puede devolver un contrato con facturas imputadas, quítelas");

            if (contexto.Existen<PedidoDtm>(nameof(PedidoDtm.IdContrato), contrato.Id))
                Emitir("No se puede devolver un contrato con pedidos imputados, quítelos");
        }

        public static void TrasPasarAProrrogar(this ContratoDtm contrato, ContextoSe contexto)
        {
            var datos = contrato.Ampliacion<DatosDelContratoDtm>(contexto);
            var prorroga = contrato.Ampliacion<ProrrogaDtm>(contexto);
            var fechaAnterior = datos.FinContrato.Fecha();
            datos.FinContrato = datos.FinContrato.Fecha().AddMonths(prorroga.Meses.Entero());
            datos.Modificar(contexto, new Dictionary<string, object> { { ltrDatosDelContrato.EstoyProrrogando, true } });
            contrato.CrearTraza(contexto, "Contrato prorrogado", $"El usuario {contexto.DatosDeConexion.Login} ha prorrogado el contrato con fecha de fin {fechaAnterior} a {datos.FinContrato}");
        }

        public static void TrasPasarAElaboracion(this ContratoDtm contrato, ContextoSe contexto)
        {
            EliminarEventos(contexto, contrato, ltrDatosDelContrato.AvisoDeFinDeContrato);
            EliminarEventos(contexto, contrato, ltrDatosDelContrato.ContratoSinFechaDeFin);
            EliminarEventos(contexto, contrato, ltrDatosDelContrato.AvisoPrevioDeFinDeContrato);
        }

        private static void AnotarEventosDelContrato(this ContratoDtm contrato, ContextoSe contexto)
        {
            var datos = contrato.Ampliacion<DatosDelContratoDtm>(contexto);
            if (datos.FinContrato == default)
                PersistirEventosDeSinFinDeContrato(contexto, contrato, datos);
            else
                PersistirEventoDeFinDeContrato(contexto, contrato, datos);

            PersistirEventoDeAvisoPrevio(contrato, contexto, datos);
        }

        public static void PersistirEventoDeAvisoPrevio(this ContratoDtm contrato, ContextoSe contexto, DatosDelContratoDtm datos)
        {
            var eventos = contexto.SeleccionarEventos(contrato.IdAgenda, datos.IdElemento, ltrDatosDelContrato.AvisoPrevioDeFinDeContrato);
            if (datos.FechaDeAvisoPrevio != default && datos.FechaDeAvisoPrevio > DateTime.Now.Date)
            {
                if (eventos.Count > 0)
                    ModificarEventoDeAvisoPrevioFinDeContrato(contexto, contrato, eventos, (DateTime)datos.FechaDeAvisoPrevio);

                if (eventos.Count == 0)
                    InsertarEventoDeAvisoPrevioDeFinDeContrato(contexto, contrato, (DateTime)datos.FechaDeAvisoPrevio);
            }
            if (datos.FechaDeAvisoPrevio == default)
                EliminarEventos(contexto, contrato, eventos);
        }

        public static void RecalcularAvance(this ContratoDtm contrato, ContextoSe contexto, enumAvaceOperacion operacio, decimal incremento, decimal decremento)
        {
            if (!contrato.UsaLaAmpliacionDe(contexto, typeof(AvanceDtm)))
                return;

            var avance = contrato.Ampliacion<AvanceDtm>(contexto);
            switch (operacio)
            {
                case enumAvaceOperacion.PlanificadoVariado: avance.Planificado = avance.Planificado + incremento; break;
                case enumAvaceOperacion.EliminarPlanificacion: avance.Planificado = avance.Planificado - decremento; break;
                case enumAvaceOperacion.RealizarPartePlanificado:
                    avance.Realizado = avance.Realizado + incremento;
                    avance.Planificado = avance.Planificado - decremento;
                    break;
                case enumAvaceOperacion.RealizarParteDeContrato:
                    avance.Realizado = avance.Realizado + incremento;
                    break;
                case enumAvaceOperacion.AnularRealizacionDeParte:
                    avance.Planificado = avance.Planificado + incremento;
                    avance.Realizado = avance.Realizado - decremento;
                    break;
                case enumAvaceOperacion.AnularRealizacionDeContrato:
                    avance.Realizado = avance.Realizado - decremento;
                    break;
                case enumAvaceOperacion.FacturarPlanificado:
                    avance.Facturado = avance.Facturado + incremento;
                    avance.Planificado = avance.Planificado - decremento;
                    break;
                case enumAvaceOperacion.FacturarRealizado:
                    avance.Facturado = avance.Facturado + incremento;
                    avance.Realizado = avance.Realizado - decremento;
                    break;
                case enumAvaceOperacion.FacturarContrato:
                    avance.Facturado = avance.Facturado + incremento;
                    break;
                case enumAvaceOperacion.AnularCobro:
                    avance.Facturado = avance.Facturado + decremento;
                    avance.Cobrado = avance.Cobrado - decremento;
                    break;
                case enumAvaceOperacion.CobrarFactura:
                    avance.Facturado = avance.Facturado - incremento;
                    avance.Cobrado = avance.Cobrado + incremento;
                    break;

                default: Emitir($"No se ha indicado como afecta en el avance del contrato la operación '{operacio}'"); break;
            }
            avance.Modificar(contexto, esUnaAccion: true);
        }

        public static void ValidarExistePlanificadoresPendientes(ContextoSe contexto)
        {
            throw new NotImplementedException();
        }

        public static List<PlanificadorDeVentaDtm> PlanificadoresDeVenta(this ContratoDtm contrato, ContextoSe contexto)
        =>
        contexto.SeleccionarTodos<PlanificadorDeVentaDtm>(new List<ClausulaDeFiltrado>
        {
            new ClausulaDeFiltrado { Clausula = nameof(PlanificadorDeVentaDtm.IdContrato), Criterio = enumCriteriosDeFiltrado.igual, Valor = contrato.Id.ToString()},
            new ClausulaDeFiltrado { Clausula = ltrParametrosNeg.ExcluirCancelados, Criterio = enumCriteriosDeFiltrado.igual, Valor = true.ToString() }
        });

        public static ExpedienteDtm Expediente(this ContratoDtm contrato, ContextoSe contexto, bool aplicarJoin = false, bool errorSiNoHay = true)
        {
            if (contrato.Expediente != null) return contrato.Expediente;

            if (contrato.IdExpediente is null && errorSiNoHay)
                Emitir($"El contrato '{contrato.Referencia}' no tiene expediente asociado");

            if (contrato.IdExpediente is null) return null;

            return contrato.Expediente = contexto.SeleccionarPorId<ExpedienteDtm>((int)contrato.IdExpediente, aplicarJoin);
        }

        public static ClienteDtm Cliente(this ContratoDtm contrato, ContextoSe contexto, bool errorSiNoHay = false)
        {
            ClienteDtm cliente = null;
            if (contrato.ClaseDeContrato == enumClaseDeContrato.Venta)
            {
                contrato.Datos = enumNegocio.Contrato.Ampliacion<DatosDelContratoDtm>(contexto, contrato.Id, aplicarJoin: true);
                cliente = contrato.Datos.Cliente;
            }
            else if (contrato.ClaseDeContrato == enumClaseDeContrato.MatriculaDeGuarderia)
            {
                contrato.MatriculaDeGuarderia = enumNegocio.Contrato.Ampliacion<MatriculaDeGuarderiaDtm>(contexto, contrato.Id, aplicarJoin: true);
                cliente = contrato.MatriculaDeGuarderia.Cliente;
            }

            if (cliente == null && errorSiNoHay)
                Emitir($"Ha de indicar el cliente {contrato.DeLaClaseDeContrato} '{contrato.Referencia}'");

            return cliente;
        }

        public static DatosDelContratoDtm Datos(this ContratoDtm contrato, ContextoSe contexto, bool errorSiNoHay =false)
        =>
        contrato.Datos ??= enumNegocio.Contrato.Ampliacion<DatosDelContratoDtm>(contexto, contrato.Id, aplicarJoin: true,errorSiNoHay: errorSiNoHay);

        public static MatriculaDeGuarderiaDtm MatriculaDeGuarderia(this ContratoDtm contrato, ContextoSe contexto, bool errorSiNoHay = false)
        =>
        contrato.MatriculaDeGuarderia ??= enumNegocio.Contrato.Ampliacion<MatriculaDeGuarderiaDtm>(contexto, contrato.Id, aplicarJoin: true, errorSiNoHay: errorSiNoHay);

        public static ClienteDtm Cliente(this MatriculaDeGuarderiaDtm matricula, ContextoSe contexto)
        =>
        matricula.Cliente ??= matricula.IdCliente == null ? null : contexto.SeleccionarPorId<ClienteDtm>((int)matricula.IdCliente, aplicarJoin: true);

        public static InfanteDtm Infante(this MatriculaDeGuarderiaDtm matricula, ContextoSe contexto)
        =>
        matricula.Infante ??= matricula.IdInfante == null ? null : contexto.SeleccionarPorId<InfanteDtm>((int)matricula.IdInfante, aplicarJoin: true);

        public static CursoDeGuarderiaDtm Curso(this MatriculaDeGuarderiaDtm matricula, ContextoSe contexto)
        =>
        matricula.Curso ??= matricula.IdCurso == null ? null : contexto.SeleccionarPorId<CursoDeGuarderiaDtm>((int)matricula.IdCurso, aplicarJoin: true);

        public static (DateTime Inicio, DateTime? Fin) FechasDelContrato(this ContratoDtm contrato, ContextoSe contexto)
        {
            DateTime? inicio = null;
            DateTime? fin = null;
            if (contrato.ClaseDeContrato == enumClaseDeContrato.Venta || contrato.ClaseDeContrato == enumClaseDeContrato.Compra)
            {
                var datos = enumNegocio.Contrato.Ampliacion<DatosDelContratoDtm>(contexto, contrato.Id, errorSiNoHay: false, aplicarJoin: true);
                if (datos == null) Emitir($"Defina los datos del contrato '{contrato.Referencia}'");
                inicio = datos.InicioContrato;
                fin = datos.FinContrato;
            }
            else if (contrato.ClaseDeContrato == enumClaseDeContrato.MatriculaDeGuarderia)
            {
                var matricula = enumNegocio.Contrato.Ampliacion<MatriculaDeGuarderiaDtm>(contexto, contrato.Id, errorSiNoHay: false, aplicarJoin: true);
                if (matricula == null) Emitir($"Defina los datos de la matrícula '{contrato.Referencia}'");
                inicio = matricula == null ? null : matricula.Curso.Inicio;
                fin = matricula == null ? null : matricula.Curso.Fin;
            }
            else
            {
                Emitir($"No se puede obtener las fechas de inicio y fin del contrato '{contrato.Referencia}' de la clase {contrato.ClaseDeContrato.Descripcion()}");
            }

            return (inicio.Fecha().Date, fin?.Date);
        }

        private static void ModificarEventoDeAvisoPrevioFinDeContrato(ContextoSe contexto, ContratoDtm contrato, List<EventoDeAgendaDtm> eventos, DateTime fechaDeAvisoPrevio)
        {
            foreach (var evento in eventos)
            {
                if (evento.Inicio.Date < DateTime.Now.Date)
                    continue;

                if (evento.Inicio.Date != fechaDeAvisoPrevio)
                {
                    evento.Inicio = fechaDeAvisoPrevio.Date;
                    evento.Fin = fechaDeAvisoPrevio.Date;
                    evento.Modificar(contexto);
                    contrato.CrearTraza(contexto, "Modificado el aviso previo de fin de contrato", $"El usuario {contexto.DatosDeConexion.Login} ha modificado el aviso previo de fin de contrato con fecha {evento.Inicio.Date}");
                    return;
                }
            }
        }

        private static void InsertarEventoDeAvisoPrevioDeFinDeContrato(ContextoSe contexto, ContratoDtm contrato, DateTime fechaDeAvisoPrevio)
        {
            var evento = new EventoDeAgendaDtm
            {
                Inicio = fechaDeAvisoPrevio.Date,
                Fin = fechaDeAvisoPrevio.Date,
                Nombre = ltrDatosDelContrato.AvisoPrevioDeFinDeContrato,
                Descripcion = $"Le recordamos que el contrato {contrato.Referencia} finalizará próximamente",
                EsDelSistema = true
            };
            evento.IdAgenda = contrato.IdAgenda;
            evento.IdElemento = contrato.Id;
            evento.IdNegocio = enumNegocio.Contrato.IdNegocio();
            var parametros = new Dictionary<string, object> { { ltrParametrosNeg.EstaEjecutandoUnaAccion, true } };
            GestorDeVinculos.Vincular(contexto, contrato, evento, parametros);
            contrato.CrearTraza(contexto, "Creado el aviso previo de fin de contrato", $"El usuario {contexto.DatosDeConexion.Login} ha creado el aviso previo de fin de contrato con fecha {evento.Inicio.Date}");

        }

        private static void PersistirEventosDeSinFinDeContrato(ContextoSe contexto, ContratoDtm contrato, DatosDelContratoDtm datos)
        {
            EliminarEventos(contexto, contrato, ltrDatosDelContrato.AvisoDeFinDeContrato);
            var eventos = contexto.SeleccionarEventos(contrato.IdAgenda, datos.IdElemento, ltrDatosDelContrato.ContratoSinFechaDeFin);
            var modificado = false;
            foreach (var evento in eventos)
            {
                if (evento.Inicio.Date == datos.InicioContrato.Date.AddYears(1))
                    modificado = ModificarEventoFinDeContrato(contexto, contrato, evento, datos);
            }
            if (!modificado) InsertarEventoDeFinDeContrato(contexto, contrato, datos);
        }

        private static void PersistirEventoDeFinDeContrato(ContextoSe contexto, ContratoDtm contrato, DatosDelContratoDtm datos)
        {
            EliminarEventos(contexto, contrato, ltrDatosDelContrato.ContratoSinFechaDeFin);
            var eventos = contexto.SeleccionarEventos(contrato.IdAgenda, datos.IdElemento, ltrDatosDelContrato.AvisoDeFinDeContrato);
            var modificado = false;
            foreach (var evento in eventos)
            {
                if (evento.Fin.Date == ((DateTime)datos.FinContrato).Date)
                    modificado = ModificarEventoFinDeContrato(contexto, contrato, evento, datos);
            }
            if (!modificado) InsertarEventoDeFinDeContrato(contexto, contrato, datos);
        }

        private static void EliminarEventos(ContextoSe contexto, ContratoDtm contrato, string asunto)
        {
            var eventos = asunto.IsNullOrEmpty()
            ? contexto.SeleccionarEventos(contrato.IdAgenda, contrato.Id)
            : contexto.SeleccionarEventos(contrato.IdAgenda, contrato.Id, asunto);
            EliminarEventos(contexto, contrato, eventos);
        }

        private static void EliminarEventos(ContextoSe contexto, ContratoDtm contrato, List<EventoDeAgendaDtm> eventos)
        {
            var parametros = new Dictionary<string, object> { { ltrParametrosNeg.EstaEjecutandoUnaAccion, true } };
            foreach (var evento in eventos)
                contrato.Desvincular(contexto, evento, parametros);
        }

        private static bool ModificarEventoFinDeContrato(ContextoSe contexto, ContratoDtm contrato, EventoDeAgendaDtm evento, DatosDelContratoDtm datos)
        {
            DefinirDatosDelEvento(contrato, datos, evento);
            evento.Modificar(contexto);
            contrato.CrearTraza(contexto, "Fecha fin de contrato",
                datos.FinContrato != default ?
                $"El usuario {contexto.DatosDeConexion.Login} ha indicado una nueva fecha de fin de contrato {((DateTime)datos.FinContrato).Date}" :
                $"El usuario {contexto.DatosDeConexion.Login} ha indicado que el contrato no tienen fecha de fin");

            return true;
        }

        private static void InsertarEventoDeFinDeContrato(ContextoSe contexto, ContratoDtm contrato, DatosDelContratoDtm datos)
        {
            var evento = new EventoDeAgendaDtm();
            evento.IdAgenda = contrato.IdAgenda;
            evento.IdElemento = datos.IdElemento;
            evento.IdNegocio = enumNegocio.Contrato.IdNegocio();
            evento.EsDelSistema = true;
            DefinirDatosDelEvento(contrato, datos, evento);
            GestorDeVinculos.Vincular(contexto, contrato, evento, new Dictionary<string, object> { { ltrParametrosNeg.EstaEjecutandoUnaAccion, true } });
            contrato.CrearTraza(contexto, "Fecha fin de contrato",
                datos.FinContrato != default ?
                $"El usuario {contexto.DatosDeConexion.Login} ha indicado una nueva fecha de fin de contrato {((DateTime)datos.FinContrato).Date}" :
                $"El usuario {contexto.DatosDeConexion.Login} ha indicado que el contrato no tienen fecha de fin");
        }

        private static void DefinirDatosDelEvento(ContratoDtm contrato, DatosDelContratoDtm datos, EventoDeAgendaDtm evento)
        {
            if (datos.FinContrato != default)
            {
                evento.Inicio = ((DateTime)datos.FinContrato).Date;
                evento.Nombre = ltrDatosDelContrato.AvisoDeFinDeContrato;
                evento.Descripcion = $"Hoy vence el contrato {contrato.Referencia}";
            }
            else
            {
                evento.Inicio = datos.InicioContrato.Date.AddYears(1);
                evento.Nombre = ltrDatosDelContrato.ContratoSinFechaDeFin;
                evento.Descripcion = $"Le recordamos que el contrato {contrato.Referencia} no tiene fecha de fin";
            }
            evento.Fin = evento.Inicio;
        }

        private static void SiNoHayFechaFinNoPuedeSerProrrogable(this ContratoDtm contrato, ContextoSe contexto)
        {
            var prorroga = contrato.Ampliacion<ProrrogaDtm>(contexto, errorSiNoHay: false);
            var datos = contrato.Ampliacion<DatosDelContratoDtm>(contexto, errorSiNoHay: false);
            if (prorroga != null && datos != null)
            {
                if (datos.FinContrato == default && prorroga.ClaseDeProrroga != enumClaseDeProrroga.noProrrogable)
                    Emitir($"Un contrato sin fecha de fin de contrato no es prorrogable");
            }
        }

        private static void SiEsProrrogableFechaProrrogaMayorFechaFinMasMeses(this ContratoDtm contrato, ContextoSe contexto)
        {
            var prorroga = contrato.Ampliacion<ProrrogaDtm>(contexto, errorSiNoHay: false);
            var datos = contrato.Ampliacion<DatosDelContratoDtm>(contexto, errorSiNoHay: false);
            if (prorroga != null && datos != null)
            {
                if (datos.FinContrato != default &&
                   prorroga.ClaseDeProrroga != enumClaseDeProrroga.noProrrogable &&
                   prorroga.FechaUltimaProrroga != default &&
                   (DateTime)prorroga.FechaUltimaProrroga < ((DateTime)datos.FinContrato).AddMonths(prorroga.Meses.Entero()))
                    Emitir($"La fecha de fin de contrato, {(DateTime)datos.FinContrato}, más los meses de prorrogación, {prorroga.Meses.Entero()} no puede ser mayor que la fecha de última prórroga {(DateTime)prorroga.FechaUltimaProrroga}");
            }
        }
    }
}
