using System;
using Utilidades;
using ServicioDeDatos.Ventas;
using ServicioDeDatos;
using static Gestor.Errores.GestorDeErrores;
using System.Collections.Generic;
using System.Linq;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Entorno;

namespace GestorDeElementos.Extensores
{
    public static class ExtensorDeRemesasFae
    {
        public static bool UsaElDetalleDe(Type tipoDeDetalle)
        {
            if (tipoDeDetalle == typeof(FacturaEmtDeUnaRemesaDtm))
                return true;

            return false;
        }

        public static void AntesDeCancelar(this RemesaFaeDtm remesa, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            if (remesa.Detalles<FacturaEmtDeUnaRemesaDtm>(contexto).Count() > 0)
                Emitir($"La remesa '{remesa.Referencia}' tiene facturas asociadas, quítelas antes de cancelarla");
        }

        public static void AntesDeGenerar(this RemesaFaeDtm remesa, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            if (remesa.Detalles<FacturaEmtDeUnaRemesaDtm>(contexto).Count() == 0)
                Emitir($"La remesa '{remesa.Referencia}' no tiene facturas asociadas, añádalas antes de generarla");

            remesa.GeneradaEl = DateTime.Now;
            var parametro = enumNegocio.RemesaFae.LeerParametro(contexto, enumParametrosDeRemesasFae.REM_DiasDeEsperaDeCargo);
            remesa.CargarEl = remesa.GeneradaEl.Fecha().Date.AddDays(parametro.Valor.Entero());
        }

        public static void AntesDePresentar(this RemesaFaeDtm remesa, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            if (remesa.CargarEl is null)
                Emitir($"La remesa '{remesa.Referencia}' ha de indicar la fecha de cargo");
            remesa.PresentadaEl = DateTime.Now;
            remesa.ValidarFechaDeCargo(contexto);
        }

        public static void AntesDeDarPorConciliada(this RemesaFaeDtm remesa, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            var fechaMinimaDeCierre = remesa.FechaMaximaDeDevolucion(contexto);
            if (fechaMinimaDeCierre > DateTime.Now)
                Emitir($"La remesa '{remesa.Referencia}' no se puede dar por cerrada ya que hasta el día '{fechaMinimaDeCierre.ToShortDateString()}' pueden devolver cargos");
        }

        private static void ValidarFechaDeCargo(this RemesaFaeDtm remesa, ContextoSe contexto)
        {
            var parametro = enumNegocio.RemesaFae.LeerParametro(contexto, enumParametrosDeRemesasFae.REM_DiasDeEsperaDeCargo);
            if (remesa.CargarEl.Fecha().Date < remesa.PresentadaEl.Fecha().AddDays(parametro.Valor.Entero()).Date)
                Emitir($"La fecha de cargo '{remesa.CargarEl.Fecha().ToShortDateString()}' de la remesa '{remesa.Referencia}' ha de ser al menos '{parametro.Valor}' días mayor a su presentación");
        }

        public static RemesaFaeDtm AntesDeAnularGeneracion(this RemesaFaeDtm remesa, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            parametros[ltrDeUnaRemesaFae.IdArchivoSepa] = remesa.IdArchivo;
            remesa.IdArchivo = null;
            remesa.GeneradaEl = null;
            remesa.CargarEl = null;
            return remesa;
        }

        public static RemesaFaeDtm AntesDeAnularPresentacion(this RemesaFaeDtm remesa, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            remesa.PresentadaEl = null;
            return remesa;
        }

        public static void DespuesDeAnularGeneracion(this RemesaFaeDtm remesa, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            var idArchivo = parametros.LeerValor<int>(ltrDeUnaRemesaFae.IdArchivoSepa);
            remesa.QuitarAnexado(contexto, idArchivo);
            remesa.CrearTraza(contexto, "Remesa retrocedida para corrección", $"Se ha retrocedido la remesa por el usuario '{contexto.DatosDeConexion.Login}'");
        }

        public static void DespuesDeAnularPresentacion(this RemesaFaeDtm remesa, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            remesa.CrearTraza(contexto, "Presentación anulada", $"Se ha anulado la presentación por el usuario '{contexto.DatosDeConexion.Login}'");
        }

        public static void PersistirEventoDeCargo(this RemesaFaeDtm remesa, ContextoSe contexto)
        {
            var sociedad = remesa.Sociedad(contexto);
            if (sociedad.IdAgenda is not null)
                remesa.EliminarEventoDeCargo(contexto);
            remesa.CrearEventoDeCargo(contexto);
        }

        public static void CrearEventoDeCargo(this RemesaFaeDtm remesa, ContextoSe contexto)
        {
            var sociedad = remesa.Sociedad(contexto);
            var agenda = sociedad.IdAgenda == null ? sociedad.CrearAgenda(contexto) : contexto.SeleccionarPorId<AgendaDtm>((int)sociedad.IdAgenda);

            var evento = new EventoDeAgendaDtm();
            evento.IdAgenda = agenda.Id;
            evento.IdElemento = remesa.Id;
            evento.IdNegocio = enumNegocio.RemesaFae.IdNegocio();
            evento.EsDelSistema = true;
            evento.Inicio = ((DateTime)remesa.CargarEl).Date;
            evento.Nombre = ltrDeUnaRemesaFae.EventoDeCargo.Replace($"[{nameof(RemesaFaeDtm.Referencia)}]", remesa.Referencia);
            evento.Descripcion = $"Cargo de la remesa {remesa.Expresion} en la cuenta {remesa.CuentaDeAbono(contexto).Cuenta(contexto).NumeroIban}";
            evento.Fin = evento.Inicio;
            GestorDeVinculos.Vincular(contexto, remesa, evento, new Dictionary<string, object> { { ltrParametrosNeg.EstaEjecutandoUnaAccion, true } });
        }

        public static void EliminarEventoDeCargo(this RemesaFaeDtm remesa, ContextoSe contexto)
        {
            var nombreEvento = ltrDeUnaRemesaFae.EventoDeCargo.Replace($"[{nameof(RemesaFaeDtm.Referencia)}]", remesa.Referencia);
            var eventos = contexto.SeleccionarEventos(remesa.Id, nombreEvento);
            var p = new Dictionary<string, object> { { ltrParametrosNeg.EstaEjecutandoUnaAccion, true } };
            foreach (var e in eventos.Where(e => e.EsDelSistema)) remesa.Desvincular(contexto, e, p);
        }

        public static CuentaDeMiSociedadDtm CuentaDeAbono(this RemesaFaeDtm remesa, ContextoSe contexto)
        {
            return remesa.CuentaDeAbono == null
                ? contexto.SeleccionarPorId<CuentaDeMiSociedadDtm>(remesa.IdCuentaDeAbono, aplicarJoin: true)
                : remesa.CuentaDeAbono;
        }

        public static FacturaEmtDtm Factura(this FacturaEmtDeUnaRemesaDtm facturaRemesada, ContextoSe contexto)
        {
            return facturaRemesada.Factura == null
                ? contexto.SeleccionarPorId<FacturaEmtDtm>(facturaRemesada.IdFactura, aplicarJoin: true)
                : facturaRemesada.Factura;
        }

        public static decimal Total(this RemesaFaeDtm remesa, ContextoSe contexto)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Rem_Fae_Total);
            if (!cache.ContainsKey(remesa.Id.ToString()))
            {
                decimal total = 0;
                var facturasDeLaRemesa = remesa.Detalles<FacturaEmtDeUnaRemesaDtm>(contexto);
                foreach (var facturaDeLaRemesa in facturasDeLaRemesa)
                {

                    total = total + facturaDeLaRemesa.Factura(contexto).APagar(contexto);
                }
                cache[$"{remesa.Id}"] = total;
            }
            return (decimal)cache[$"{remesa.Id}"];
        }

        public static decimal Cobrado(this RemesaFaeDtm remesa, ContextoSe contexto)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Rem_Fae_Cobrado);
            if (!cache.ContainsKey(remesa.Id.ToString()))
            {
                decimal cobrado = 0;
                var facturasDeLaRemesa = remesa.Detalles<FacturaEmtDeUnaRemesaDtm>(contexto);
                foreach (var facturaDeLaRemesa in facturasDeLaRemesa)
                {
                    if (facturaDeLaRemesa.CargadaEl is not null)
                        cobrado = cobrado + facturaDeLaRemesa.Factura(contexto).APagar(contexto);
                }
                cache[$"{remesa.Id}"] = cobrado;
            }
            return (decimal)cache[$"{remesa.Id}"];
        }

        public static void Cargar(this RemesaFaeDtm remesa, ContextoSe contexto, DateTime cargadaEl)
        {
            if (remesa.Etapa() != enumEtapasDeRemesasFae.REM_Etapa_De_Presentacion)
                Emitir($"No se puede cargar la remesa '{remesa.Referencia}' por no estar en la etapa de '{enumEtapasDeRemesasFae.REM_Etapa_De_Presentacion.Nombre(true)}'");

            var facturasDeLaRemesa = remesa.Detalles<FacturaEmtDeUnaRemesaDtm>(contexto);
            var contador = 0;
            foreach (var fr in facturasDeLaRemesa)
            {
                if (fr.DevueltoEl.HasValue) return;
                if (fr.CargadaEl.HasValue) return;
                fr.CargadaEl = cargadaEl;
                fr.FechaMaximaDeDevolucion = remesa.FechaMaximaDeDevolucion(contexto);
                fr.Modificar(contexto, parametros: new Dictionary<string, object> { { ltrParametrosNeg.AccionQueSeEjecuta, ltrDeUnaRemesaFae.Accion_CargoDeRemesa } });
                contador++;
            }
            remesa.CrearTraza(contexto, "Cargo realizado", $"El usuario '{contexto.DatosDeConexion.Login}' ha realizado el cargo de ${contador} facturas de la remesa con fecha '{cargadaEl.ToShortDateString()}'");
        }

        public static void AnularDevolucionDeFactura(this RemesaFaeDtm remesa, ContextoSe contexto, FacturaEmtDtm factura, string motivo)
        {
            if (remesa.Etapa() != enumEtapasDeRemesasFae.REM_Etapa_De_Presentacion)
                Emitir($"No se puede anular la devolución de la factura '{factura.Referencia}' en la remesa '{remesa.Referencia}' por no estar en la etapa de '{enumEtapasDeRemesasFae.REM_Etapa_De_Presentacion.Nombre(true)}'");

            var frs = remesa.Detalles<FacturaEmtDeUnaRemesaDtm>(contexto, filtros: new List<ClausulaDeFiltrado>
            {
                new ClausulaDeFiltrado(nameof(FacturaEmtDeUnaRemesaDtm.IdFactura), enumCriteriosDeFiltrado.igual, factura.Id),
                new ClausulaDeFiltrado(nameof(FacturaEmtDeUnaRemesaDtm.IdElemento), enumCriteriosDeFiltrado.igual, remesa.Id)
            });
            var fr = frs[0];
            if (fr.DevueltoEl is null) Emitir($"La factura '{factura.Referencia}' de la remesa '{remesa.Referencia}' no está devuelta");
            fr.DevueltoEl = null;
            fr.Motivo = motivo;
            fr.FechaMaximaDeDevolucion = remesa.FechaMaximaDeDevolucion(contexto);
            fr.Modificar(contexto, parametros: new Dictionary<string, object> { { ltrParametrosNeg.AccionQueSeEjecuta, ltrDeUnaRemesaFae.Accion_AnularDevolucionDeFactura } });

            remesa.CrearTraza(contexto, "Anulación de devolución de factura", $"El usuario '{contexto.DatosDeConexion.Login}' ha anulado la devolución de la facturas '{factura.Referencia}' de la remesa '{remesa.Referencia}'");
        }

        public static void AnularCargo(this RemesaFaeDtm remesa, ContextoSe contexto, DateTime cargadaEl)
        {
            if (remesa.Etapa() != enumEtapasDeRemesasFae.REM_Etapa_De_Presentacion)
                Emitir($"No se puede anular el cargo de la remesa '{remesa.Referencia}' por no estar en la etapa de '{enumEtapasDeRemesasFae.REM_Etapa_De_Presentacion.Nombre(true)}'");

            var facturasDeLaRemesa = remesa.Detalles<FacturaEmtDeUnaRemesaDtm>(contexto);
            var contador = 0;
            foreach (var fr in facturasDeLaRemesa)
            {
                if (fr.DevueltoEl is not null) continue;
                if (fr.CargadaEl.Fecha().Date != cargadaEl.Date) continue;
                fr.CargadaEl = null;
                fr.FechaMaximaDeDevolucion = null;
                fr.Modificar(contexto, parametros: new Dictionary<string, object> { { ltrParametrosNeg.AccionQueSeEjecuta, ltrDeUnaRemesaFae.Accion_AnularCargoDeRemesa } });
                contador++;
            }
            remesa.CrearTraza(contexto, "Cargo anulado", $"El usuario '{contexto.DatosDeConexion.Login}' ha anulado el cargo de ${contador} facturas de la remesa con fecha '{cargadaEl.ToShortDateString()}'");
        }

        public static void DevolverFactura(this RemesaFaeDtm remesa, ContextoSe contexto, FacturaEmtDtm factura, DateTime devueltoEl, string motivo)
        {
            if (remesa.Etapa() != enumEtapasDeRemesasFae.REM_Etapa_De_Presentacion)
                Emitir($"No se puede delvolver la factura '{factura.Referencia}' de la remesa '{remesa.Referencia}' por no estar en la etapa de '{enumEtapasDeRemesasFae.REM_Etapa_De_Presentacion.Nombre(true)}'");

            var frs = remesa.Detalles<FacturaEmtDeUnaRemesaDtm>(contexto, filtros: new List<ClausulaDeFiltrado>
            {
                new ClausulaDeFiltrado(nameof(FacturaEmtDeUnaRemesaDtm.IdFactura), enumCriteriosDeFiltrado.igual, factura.Id),
                new ClausulaDeFiltrado(nameof(FacturaEmtDeUnaRemesaDtm.IdElemento), enumCriteriosDeFiltrado.igual, remesa.Id)
            });
            var fr = frs[0];
            if (fr.DevueltoEl is not null) Emitir($"La factura '{factura.Referencia}' de la remesa '{remesa.Referencia}' ya está devuelta con fecha '{fr.DevueltoEl.Fecha().ToShortDateString()}'");
            fr.CargadaEl = null;
            fr.DevueltoEl = devueltoEl;
            fr.FechaMaximaDeDevolucion = null;
            fr.Motivo = motivo;
            fr.Modificar(contexto, parametros: new Dictionary<string, object> { { ltrParametrosNeg.AccionQueSeEjecuta, ltrDeUnaRemesaFae.Accion_DevolverFactura } });

            remesa.CrearTraza(contexto, "Devolución de cargo de factura", $"El usuario '{contexto.DatosDeConexion.Login}' ha anulado el cargo de la facturas '{factura.Referencia}' de la remesa '{remesa.Referencia}' con fecha '{devueltoEl.ToShortDateString()}'");
        }

        public static DateTime FechaMaximaDeDevolucion(this RemesaFaeDtm remesa, ContextoSe contexto)
        {
            if (!remesa.CargarEl.HasValue)
                Emitir($"La remesa '{remesa.Referencia}' ha de indicar una fecha de cargo para poder obtener la fecha tope de devolución de cobros de facturas por parte del cliente");

            var parametro = enumNegocio.RemesaFae.LeerParametro(contexto, enumParametrosDeRemesasFae.REM_MesesDeEsperaDeDevolucionDeCargos);
            return remesa.CargadaEl.HasValue ? remesa.CargadaEl.Fecha().AddMonths(parametro.Valor.Entero()) :
                   remesa.CargarEl.Fecha().AddMonths(parametro.Valor.Entero());
        }

    }
}
