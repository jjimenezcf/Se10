using System;
using Utilidades;
using ServicioDeDatos.Gastos;
using ServicioDeDatos;
using static Gestor.Errores.GestorDeErrores;
using System.Collections.Generic;
using System.Linq;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Ventas;

namespace GestorDeElementos.Extensores
{
    public static class ExtensorDeRemesasPag
    {
        public static bool UsaElDetalleDe(Type tipoDeDetalle)
        {
            if (tipoDeDetalle == typeof(PagoDeUnaRemesaDtm))
                return true;

            return false;
        }

        public static void AntesDeCancelar(this RemesaPagDtm remesa, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            if (remesa.Detalles<PagoDeUnaRemesaDtm>(contexto).Count > 0)
                Emitir($"La remesa '{remesa.Referencia}' tiene pagos asociadas, quítelos antes de cancelarla");
        }

        public static void AntesDeGenerar(this RemesaPagDtm remesa, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            if (remesa.Detalles<PagoDeUnaRemesaDtm>(contexto).Count == 0)
                Emitir($"La remesa '{remesa.Referencia}' no tiene pagos asociadas, añádalos antes de generarla");

            remesa.GeneradaEl = DateTime.Now;
            var parametro = enumNegocio.RemesaPag.LeerParametro(contexto, VariableDeRemesasPag.Parametro.REM_DiasDeTransferencia);
            remesa.PagarEl = remesa.PagarEl is null ? remesa.GeneradaEl.Fecha().Date.AddDays(parametro.Valor.Entero()) : remesa.PagarEl;
        }

        public static void AntesDePresentar(this RemesaPagDtm remesa, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            if (remesa.PagarEl is null)
                Emitir($"La remesa '{remesa.Referencia}' ha de indicar la fecha de pago");
            remesa.PresentadaEl = DateTime.Now;
            remesa.ValidarFechaDePago(contexto);
        }

        public static void AntesDeDarPorPagada(this RemesaPagDtm remesa, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            if (remesa.PagarEl.Fecha() > DateTime.Now && remesa.PagadaEl is null)
                Emitir($"La remesa '{remesa.Referencia}' no se puede cerrada ya que aun no se ha cumplido la fecha de pago '{remesa.PagarEl.Fecha().ToShortDateString()}'");

            if (remesa.PagadaEl is null)
                remesa.PagadaEl = DateTime.Now.Date;

            if (remesa.PagadaEl.Fecha() > DateTime.Now)
                Emitir($"La remesa '{remesa.Referencia}' no se puede dar por cerrada ya que hasta el día '{remesa.PagadaEl.Fecha().ToShortDateString()}' no estarán hechos los pagos");
        }

        private static void ValidarFechaDePago(this RemesaPagDtm remesa, ContextoSe contexto)
        {
            var parametro = enumNegocio.RemesaPag.LeerParametro(contexto, VariableDeRemesasPag.Parametro.REM_DiasDeTransferencia);
            if (remesa.PagarEl.Fecha().Date < remesa.PresentadaEl.Fecha().AddDays(parametro.Valor.Entero()).Date)
                Emitir($"La fecha de pago '{remesa.PagarEl.Fecha().ToShortDateString()}' de la remesa '{remesa.Referencia}' ha de ser al menos '{parametro.Valor}' día/s mayor a su presentación");
        }

        public static RemesaPagDtm AntesDeAnularGeneracion(this RemesaPagDtm remesa, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            parametros[ltrDeUnaRemesaPag.IdArchivoSepa] = contexto.SeleccionarPorId<RemesaPagDtm>(remesa.Id, usarLaCache: false).IdArchivo;
            remesa.IdArchivo = null;
            remesa.GeneradaEl = null;
            remesa.PagarEl = null;
            return remesa;
        }

        public static RemesaPagDtm AntesDeAnularPresentacion(this RemesaPagDtm remesa, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            if (HayPagosRealizados(remesa, contexto))
                Emitir($"La remesa '{remesa.Referencia}' no puede ser devuelta, por tener pagos o anulaciones.");

            remesa.PresentadaEl = null;
            remesa.PagadaEl = null;
            return remesa;
        }

        private static bool HayPagosRealizados(RemesaPagDtm remesa, ContextoSe contexto)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Rem_Pag_Hay_Pagos);
            if (!cache.ContainsKey(remesa.Id.ToString()))
            {
                var pagos = remesa.Detalles<PagoDeUnaRemesaDtm>(contexto);
                cache[$"{remesa.Id}"] = false;
                foreach (var pago in pagos)
                {
                    if (pago.PagadoEl is not null || pago.AnuladoEl is not null)
                    {
                        cache[$"{remesa.Id}"] = true;
                        break;
                    }
                }
            }
            return (bool)cache[$"{remesa.Id}"];
        }

        public static void DespuesDeAnularGeneracion(this RemesaPagDtm remesa, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            var idArchivo = parametros.LeerValor<int?>(ltrDeUnaRemesaPag.IdArchivoSepa);
            remesa.QuitarAnexado(contexto, idArchivo.Entero());
            remesa.CrearTraza(contexto, "Remesa retrocedida para corrección", $"Se ha retrocedido la remesa por el usuario '{contexto.DatosDeConexion.Login}'");
        }

        public static void DespuesDeAnularPresentacion(this RemesaPagDtm remesa, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            remesa.CrearTraza(contexto, "Presentación anulada", $"Se ha anulado la presentación por el usuario '{contexto.DatosDeConexion.Login}'");
        }

        public static void PersistirEventoDePago(this RemesaPagDtm remesa, ContextoSe contexto)
        {
            var sociedad = remesa.Sociedad(contexto);
            if (sociedad.IdAgenda is not null)
                remesa.EliminarEventoDePago(contexto);
            remesa.CrearEventoDePago(contexto);
        }

        public static void CrearEventoDePago(this RemesaPagDtm remesa, ContextoSe contexto)
        {
            var sociedad = remesa.Sociedad(contexto);
            var agenda = sociedad.IdAgenda == null ? sociedad.CrearAgenda(contexto) : contexto.SeleccionarPorId<AgendaDtm>((int)sociedad.IdAgenda);

            var evento = new EventoDeAgendaDtm();
            evento.IdAgenda = agenda.Id;
            evento.IdElemento = remesa.Id;
            evento.IdNegocio = enumNegocio.RemesaPag.IdNegocio();
            evento.EsDelSistema = true;
            evento.Inicio = ((DateTime)remesa.PagarEl).Date;
            evento.Nombre = ltrDeUnaRemesaPag.EventoDePago.Replace($"[{nameof(RemesaPagDtm.Referencia)}]", remesa.Referencia);
            evento.Descripcion = $"Pago de la remesa {remesa.Expresion} usando la cuenta {remesa.CuentaDePago(contexto).Cuenta(contexto).NumeroIban}";
            evento.Fin = evento.Inicio;
            GestorDeVinculos.Vincular(contexto, remesa, evento, new Dictionary<string, object> { { ltrParametrosNeg.EstaEjecutandoUnaAccion, true } });
        }

        public static void EliminarEventoDePago(this RemesaPagDtm remesa, ContextoSe contexto)
        {
            var nombreEvento = ltrDeUnaRemesaPag.EventoDePago.Replace($"[{nameof(RemesaPagDtm.Referencia)}]", remesa.Referencia);
            var eventos = contexto.SeleccionarEventos(remesa.Id, nombreEvento);
            var p = new Dictionary<string, object> { { ltrParametrosNeg.EstaEjecutandoUnaAccion, true } };
            foreach (var e in eventos.Where(e => e.EsDelSistema)) remesa.Desvincular(contexto, e, p);
        }

        public static CuentaDeMiSociedadDtm CuentaDePago(this RemesaPagDtm remesa, ContextoSe contexto)
        {
            return remesa.CuentaDePago == null
                ? contexto.SeleccionarPorId<CuentaDeMiSociedadDtm>(remesa.IdCuentaDePago, aplicarJoin: true)
                : remesa.CuentaDePago;
        }

        public static PagoDtm Pago(this PagoDeUnaRemesaDtm pagoRemesado, ContextoSe contexto)
        {
            return pagoRemesado.Pago == null
                ? contexto.SeleccionarPorId<PagoDtm>(pagoRemesado.IdPago, aplicarJoin: true)
                : pagoRemesado.Pago;
        }

        public static decimal Total(this RemesaPagDtm remesa, ContextoSe contexto)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Rem_Pag_Total);
            if (!cache.ContainsKey(remesa.Id.ToString()))
            {
                decimal total = 0;
                var pagosDeLaRemesa = remesa.Detalles<PagoDeUnaRemesaDtm>(contexto);
                foreach (var pagoDeLaRemesa in pagosDeLaRemesa)
                {
                    total = total + pagoDeLaRemesa.Pago(contexto).Importe;
                }
                cache[$"{remesa.Id}"] = total;
            }
            return (decimal)cache[$"{remesa.Id}"];
        }

        public static decimal Pagado(this RemesaPagDtm remesa, ContextoSe contexto)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Rem_Pag_Pagado);
            if (!cache.ContainsKey(remesa.Id.ToString()))
            {
                decimal total = 0;
                var pagosDeLaRemesa = remesa.Detalles<PagoDeUnaRemesaDtm>(contexto);
                foreach (var pagoDeLaRemesa in pagosDeLaRemesa)
                {
                    if (pagoDeLaRemesa.AnuladoEl is not null) continue;
                    if (pagoDeLaRemesa.PagadoEl is null) continue;
                    var pago = pagoDeLaRemesa.Pago(contexto);
                    if (pago.PagadoEl is null) continue;

                    total = total + pago.Importe;
                }
                cache[$"{remesa.Id}"] = total;
            }
            return (decimal)cache[$"{remesa.Id}"];
        }

        public static void Pagar(this RemesaPagDtm remesa, ContextoSe contexto, DateTime pagadaEl, VariableDePagos.enumMotivoTransicion motivo)
        {
            if (!remesa.EstaEnAlgunaDeLasEtapa(new List<enumEtapasDeRemesasPag> { enumEtapasDeRemesasPag.REM_Etapa_De_Presentacion, enumEtapasDeRemesasPag.REM_Etapa_De_Cierre }))
                Emitir($"No se puede pagar la remesa '{remesa.Referencia}' por no estar en la etapa de '{enumEtapasDeRemesasPag.REM_Etapa_De_Presentacion.Nombre(true)}'");

            var pagosDeLaRemesa = remesa.Detalles<PagoDeUnaRemesaDtm>(contexto);
            var contador = 0;
            foreach (var pr in pagosDeLaRemesa)
            {
                if (pr.PagadoEl.HasValue) return;
                if (pr.AnuladoEl.HasValue) continue;
                pr.PagadoEl = pagadaEl;
                pr.Modificar(contexto, parametros: new Dictionary<string, object> {
                    { ltrParametrosNeg.AccionQueSeEjecuta, ltrDeUnaRemesaPag.Accion_PagoDeRemesa },
                    { nameof(VariableDePagos.TransicionesPorMotivo),motivo}
                });
                contador++;
            }
            if (remesa.PagadaEl is null)
            {
                remesa.PagadaEl = pagadaEl;
                remesa.Modificar(contexto, ltrDeUnaRemesaPag.Accion_PagoDeRemesa);
            }
            remesa.CrearTraza(contexto, "Pago realizado", $"El usuario '{contexto.DatosDeConexion.Login}' ha realizado el pago de ${contador} pagos de la remesa '{remesa.Referencia}' con fecha '{pagadaEl.ToShortDateString()}'");
        }

        public static void AnularAnulacionDePago(this RemesaPagDtm remesa, ContextoSe contexto, PagoDtm pago, string motivo)
        {
            if (remesa.Etapa() != enumEtapasDeRemesasPag.REM_Etapa_De_Presentacion)
                Emitir($"No se puede anular la anulación del pago '{pago.Referencia}' en la remesa '{remesa.Referencia}' por no estar en la etapa de '{enumEtapasDeRemesasPag.REM_Etapa_De_Presentacion.Nombre(true)}'");

            var prs = remesa.Detalles<PagoDeUnaRemesaDtm>(contexto, filtros: new List<ClausulaDeFiltrado>
            {
                new ClausulaDeFiltrado(nameof(PagoDeUnaRemesaDtm.IdPago), enumCriteriosDeFiltrado.igual, pago.Id),
                new ClausulaDeFiltrado(nameof(PagoDeUnaRemesaDtm.IdElemento), enumCriteriosDeFiltrado.igual, remesa.Id)
            });
            var pr = prs[0];
            if (pr.AnuladoEl is null) Emitir($"El pago '{pago.Referencia}' de la remesa '{remesa.Referencia}' no está anulado");
            pr.AnuladoEl = null;
            pr.Motivo = motivo;
            pr.Modificar(contexto, parametros: new Dictionary<string, object> { { ltrParametrosNeg.AccionQueSeEjecuta, ltrDeUnaRemesaPag.Accion_AnularAnulacionDePago } });

            remesa.CrearTraza(contexto, "Anulación de anulación de pago", $"El usuario '{contexto.DatosDeConexion.Login}' ha anulado la anulación del pagos '{pago.Referencia}' de la remesa '{remesa.Referencia}'");
        }

        public static void RetrocederPago(this RemesaPagDtm remesa, ContextoSe contexto, DateTime pagadaEl)
        {
            if (remesa.Etapa() != enumEtapasDeRemesasPag.REM_Etapa_De_Presentacion)
                Emitir($"No se puede anular el pago de la remesa '{remesa.Referencia}' por no estar en la etapa de '{enumEtapasDeRemesasPag.REM_Etapa_De_Presentacion.Nombre(true)}'");

            var pagosDeLaRemesa = remesa.Detalles<PagoDeUnaRemesaDtm>(contexto);
            var contador = 0;
            foreach (var pr in pagosDeLaRemesa)
            {
                if (pr.AnuladoEl is not null) continue;
                if (pr.PagadoEl.Fecha().Date != pagadaEl.Date) continue;
                pr.PagadoEl = null;
                pr.Modificar(contexto, parametros: new Dictionary<string, object> {
                    { ltrParametrosNeg.AccionQueSeEjecuta, ltrDeUnaRemesaPag.Accion_PagoDeRemesa },
                    { nameof(VariableDePagos.TransicionesPorMotivo),VariableDePagos.enumMotivoTransicion.RetrocederRemesa}});
                contador++;
            }
            remesa.CrearTraza(contexto, "Pago anulado", $"El usuario '{contexto.DatosDeConexion.Login}' ha anulado el pago de ${contador} pagos de la remesa con fecha '{pagadaEl.ToShortDateString()}'");
        }


        public static void AnularPago(this RemesaPagDtm remesa, ContextoSe contexto, PagoDtm pago, DateTime anuladoEl, string motivo)
        {
            if (remesa.Etapa() != enumEtapasDeRemesasPag.REM_Etapa_De_Presentacion)
                Emitir($"No se puede delvolver el pago '{pago.Referencia}' de la remesa '{remesa.Referencia}' por no estar en la etapa de '{enumEtapasDeRemesasPag.REM_Etapa_De_Presentacion.Nombre(true)}'");

            var prs = remesa.Detalles<PagoDeUnaRemesaDtm>(contexto, filtros: new List<ClausulaDeFiltrado>
            {
                new ClausulaDeFiltrado(nameof(PagoDeUnaRemesaDtm.IdPago), enumCriteriosDeFiltrado.igual, pago.Id),
                new ClausulaDeFiltrado(nameof(PagoDeUnaRemesaDtm.IdElemento), enumCriteriosDeFiltrado.igual, remesa.Id)
            });
            var fr = prs[0];
            if (fr.AnuladoEl is not null) Emitir($"El pago '{pago.Referencia}' de la remesa '{remesa.Referencia}' ya está anulado con fecha '{fr.AnuladoEl.Fecha().ToShortDateString()}'");
            fr.PagadoEl = null;
            fr.AnuladoEl = anuladoEl;
            fr.Motivo = motivo;
            fr.Modificar(contexto, parametros: new Dictionary<string, object> { { ltrParametrosNeg.AccionQueSeEjecuta, ltrDeUnaRemesaPag.Accion_AnularPago } });

            remesa.CrearTraza(contexto, "Anulación de pago", $"El usuario '{contexto.DatosDeConexion.Login}' ha anulado el pago '{pago.Referencia}' de la remesa '{remesa.Referencia}' con fecha '{anuladoEl.ToShortDateString()}'");
        }

        public static void ActualizarPagos(this RemesaPagDtm remesa, ContextoSe contexto)
        {
            //if (remesa.Etapa() != enumEtapasDeRemesasPag.REM_Etapa_De_Cumplimentacion)
            //    Emitir($"No se puede modificar la fecha de pago de la remesa '{remesa.Referencia}' por estar en la etapa '{remesa.Etapa().Nombre(true)}'");

            var pagosDeLaRemesa = remesa.Detalles<PagoDeUnaRemesaDtm>(contexto);
            foreach (var pr in pagosDeLaRemesa)
            {
                var pago = pr.Pago(contexto);
                pago.PagarEl = remesa.PagarEl;
                pr.PagadoEl = null;
                pago.Modificar(contexto, parametros: new Dictionary<string, object> { { ltrParametrosNeg.AccionQueSeEjecuta, ltrDeUnaRemesaPag.Accion_CambiarFechaDePago } });
            }

        }


    }
}
