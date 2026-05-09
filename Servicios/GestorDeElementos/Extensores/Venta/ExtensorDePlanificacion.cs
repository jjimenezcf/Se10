using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.MaestrosTecnico;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Ventas;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilidades;
using static Gestor.Errores.GestorDeErrores;
using static ServicioDeDatos.Elemento.Enumerados;

namespace GestorDeElementos.Extensores
{
    public static class ExtensorDePlanificacion
    {
        public static void ValidarQueLaLineaDeLaPlanificacionEsModificable(this LineaDeUnaPlfVentaDtm lineaPlv, ContextoSe contexto)
        {
            var plf = lineaPlv.DetalleDe<PlanificacionDeVentaDtm>(contexto);
            if (!plf.EstaEnLaEtapa(VariablesDePlfsDeVenta.enumEtapasDePlfsDeVenta.PLF_Etapa_Pendiente))
                Emitir($"La planificación '{plf.Referencia}' ya ha sido generada");

            if (!ModoDeAcceso.HayPermisosDe(plf.ModoDeAccesoALaPlanificacion(contexto), enumModoDeAccesoDeDatos.Gestor))
                Emitir($"No se pueden crear, modificar o borrar líneas de la planificación '{plf.Referencia}' por no ser editable");
        }

        public static void ValidarQueLaPlanificacionEsModificable(this PlanificacionDeVentaDtm plv, ContextoSe contexto)
        {
            if (!ModoDeAcceso.HayPermisosDe(plv.ModoDeAccesoALaPlanificacion(contexto), enumModoDeAccesoDeDatos.Gestor))
                Emitir($"No se pueden crear, modificar o borrar datos de la planificación");
        }

        public static enumModoDeAccesoDeDatos ModoDeAccesoALaPlanificacion(this PlanificacionDeVentaDtm plv, ContextoSe contexto)
        {
            if (plv.IdFacturaEmt.Entero() > 0 || plv.IdParteTr.Entero() > 0)
                return enumModoDeAccesoDeDatos.Consultor;

            return ApiDePermisos.LeerModoDeAcceso(contexto, enumNegocio.PlanificacionDeVenta, plv.Id);
        }

        public static decimal Total(this PlanificacionDeVentaDtm elemento, ContextoSe contexto, bool conIva)
        {
            var cacheSinIva = ServicioDeCaches.Obtener(CacheDe.Plv_TotalSinIva);
            var cacheConIva = ServicioDeCaches.Obtener(CacheDe.Plv_TotalConIva);
            if (!cacheSinIva.ContainsKey(elemento.Id.ToString()))
            {
                decimal totalSinIva = 0;
                decimal totalConIva = 0;
                var lineas = elemento.Detalles<LineaDeUnaPlfVentaDtm>(contexto);
                foreach (var linea in lineas)
                {
                    if (linea.TipoDeLinea != enumTipoDeLinea.Unitario)
                        continue;

                    totalSinIva = totalSinIva + linea.ImporteConDto;
                    totalConIva = totalConIva + linea.ImporteDeLinea;
                }
                cacheSinIva[$"{elemento.Id}"] = totalSinIva;
                cacheConIva[$"{elemento.Id}"] = totalConIva;
            }
            return conIva ? (decimal)cacheConIva[$"{elemento.Id}"] : (decimal)cacheSinIva[$"{elemento.Id}"];
        }

        public static void CrearEventoDePlanificacionAlContrato(this PlanificacionDeVentaDtm plv, ContextoSe contexto)
        {
            if (!plv.IdContrato.HasValue) return;

            var contrato = contexto.SeleccionarPorId<ContratoDtm>((int)plv.IdContrato, errorSiNoHay: false);

            var evento = new EventoDeAgendaDtm();
            evento.IdAgenda = contrato.IdAgenda;
            evento.IdElemento = plv.Id;
            evento.IdNegocio = enumNegocio.PlanificacionDeVenta.IdNegocio();
            evento.Inicio = plv.EjecutarEl.Date;
            evento.Nombre = ltrDeUnaPlanificacionDeVenta.EventoDeEjecucion.Replace($"[{nameof(PlanificacionDeVentaDtm.Referencia)}]", plv.Referencia);
            evento.Descripcion = $"Se ejecuta la planificación: {plv.Expresion}";
            evento.Fin = evento.Inicio;
            evento.EsDelSistema = true;
            GestorDeVinculos.Vincular(contexto, plv, evento, new Dictionary<string, object> { { ltrParametrosNeg.EstaEjecutandoUnaAccion, true } });
        }
        public static void CrearEventoDePlanificacionALaSociedad(this PlanificacionDeVentaDtm plv, ContextoSe contexto)
        {
            var sociedad = plv.Sociedad(contexto);
            var agenda = sociedad.IdAgenda == null ? sociedad.CrearAgenda(contexto) : contexto.SeleccionarPorId<AgendaDtm>((int)sociedad.IdAgenda);

            var evento = new EventoDeAgendaDtm();
            evento.IdAgenda = agenda.Id;
            evento.IdElemento = plv.Id;
            evento.IdNegocio = enumNegocio.PlanificacionDeVenta.IdNegocio();
            evento.Inicio = plv.EjecutarEl.Date;
            evento.Nombre = ltrDeUnaPlanificacionDeVenta.EventoDeEjecucion.Replace($"[{nameof(PlanificacionDeVentaDtm.Referencia)}]", plv.Referencia);
            evento.Descripcion = $"Se ejecuta la planificación: {plv.Expresion}";
            evento.Fin = evento.Inicio;
            evento.EsDelSistema = true;
            GestorDeVinculos.Vincular(contexto, plv, evento, new Dictionary<string, object> { { ltrParametrosNeg.EstaEjecutandoUnaAccion, true } });
        }

        public static void ModificarEventosDePlanificacion(this PlanificacionDeVentaDtm plv, ContextoSe contexto)
        {
            EliminarEventoDeVencimiento(plv, contexto);
            plv.CrearEventoDePlanificacionAlContrato(contexto);
            plv.CrearEventoDePlanificacionALaSociedad(contexto);
        }

        public static UnitarioDtm Unitario(this LineaDeUnaPlfVentaDtm linea, ContextoSe contexto, bool errorSiNoHay = true, bool aplicarJoin = false)
        {
            if (linea.IdUnitario.Entero() == 0)
            {
                if (errorSiNoHay) Emitir($"La línea '{linea.Orden}' de la planificación '{linea.DetalleDe<PlanificacionDeVentaDtm>(contexto).Referencia(contexto)}' no tiene asociado ningún unitario");
                return null;
            }

            return linea.Unitario == null ? contexto.SeleccionarPorId<UnitarioDtm>((int)linea.IdUnitario, aplicarJoin) : linea.Unitario;
        }

        public static decimal PorcentageDeIva(this LineaDeUnaPlfVentaDtm linea, ContextoSe contexto, bool errorSiNoHay = true)
        {
            if (linea.IdIvaR.Entero() == 0)
            {
                if (errorSiNoHay) Emitir($"La línea '{linea.Orden}' de la planificación '{linea.DetalleDe<PlanificacionDeVentaDtm>(contexto).Referencia(contexto)}' no tiene iva indicado");
                return 0;
            }

            return contexto.SeleccionarPorId<IvaRepercutidoDtm>(linea.IdIvaR.Entero()).Porcentaje;
        }

        private static void EliminarEventoDeVencimiento(this PlanificacionDeVentaDtm plv, ContextoSe contexto)
        {
            var nombreEvento = ltrDeUnaPlanificacionDeVenta.EventoDeEjecucion.Replace($"[{nameof(PlanificacionDeVentaDtm.Referencia)}]", plv.Referencia);
            var eventos = contexto.SeleccionarEventos(plv.Id, nombreEvento);
            var p = new Dictionary<string, object> { { ltrParametrosNeg.EstaEjecutandoUnaAccion, true } };
            foreach (var e in eventos.Where(e => e.EsDelSistema)) plv.Desvincular(contexto, e, p);
        }

        public static void TrasAnular(this PlanificacionDeVentaDtm plv, ContextoSe contexto)
        {
            var eventos = plv.Vinculados<EventoDeAgendaDtm>(contexto);
            var parametro = new Dictionary<string, object> { { ltrParametrosNeg.EstaEjecutandoUnaAccion, true } };
            foreach (var evento in eventos)
            {
                plv.Desvincular(contexto, evento, parametro);
            }

            if (plv.IdContrato.Entero() > 0)
            {
                var contrato = contexto.SeleccionarPorId<ContratoDtm>(plv.IdContrato.Entero());
                contrato.RecalcularAvance(contexto, enumAvaceOperacion.EliminarPlanificacion, 0, plv.Total(contexto, conIva: false));
            }
        }

        public static PlanificacionDeVentaDtm AntesDeGenerar(this PlanificacionDeVentaDtm plv, ContextoSe contexto)
        {
            if (plv.IdTipoDeParte.Entero() > 0)
            {
                if (plv.IdParteTr.Entero() > 0)
                    Emitir($"La planificación {plv.Referencia} ya tiene el parte de trabajo generado y asociado, {contexto.SeleccionarPorId<ParteTrDtm>(plv.IdParteTr.Entero()).Referencia}");

                plv.IdParteTr = plv.CrearParteTr(contexto).Id;
            }
            else if (plv.IdTipoDeFactura.Entero() > 0)
            {
                if (plv.IdFacturaEmt.Entero() > 0)
                    Emitir($"La planificación {plv.Referencia} ya tiene la factura generada y asociado, {contexto.SeleccionarPorId<FacturaEmtDtm>(plv.IdFacturaEmt.Entero()).Referencia}");

                plv.IdFacturaEmt = plv.CrearPrefactura(contexto).Id;
            }
            else
                Emitir($"La planificación {plv.Referencia} debe indicar o un parte o una factura a generar");
            return plv;
        }

        public static PlanificacionDeVentaDtm CrearPlanificacion(this PlanificadorDeVentaDtm planificador, ContextoSe contexto, ClienteDtm cliente, DateTime fechaDePlanificacion, int numero)
        {
            var planificacion = new PlanificacionDeVentaDtm
            {
                IdCg = planificador.IdCgDeLaPlanificacion,
                IdTipo = planificador.IdTipoDePlanificacion,
                IdCliente = cliente.Id,
                IdPlanificador = planificador.Id,
                IdContrato = planificador.IdContrato,
                IdTipoDeFactura = planificador.IdTipoDeFactura,
                IdTipoDeParte = planificador.IdTipoDeParte,
                Nombre = $"Generada por: {planificador.Expresion}, Nº: {numero}",
                Descripcion = $"{planificador.Nombre}",
                EjecutarEl = fechaDePlanificacion
            }
            .Insertar(contexto);
            return planificacion;
        }

        public static void CopiarLineasDelPlanificador(this PlanificacionDeVentaDtm plv, ContextoSe contexto)
        =>
        plv.CopiarLineas(contexto, contexto.SeleccionarPorId<PlanificadorDeVentaDtm>((int)plv.IdPlanificador));

        public static ParteTrDtm ParteTr(this PlanificacionDeVentaDtm plv, ContextoSe contexto)
        {
            if (plv.IdParteTr is null)
                return null;
            if (plv.ParteTr is not null)
                return plv.ParteTr;
            return contexto.SeleccionarPorId<ParteTrDtm>((int)plv.IdParteTr);
        }

        private static void CopiarLineas(this PlanificacionDeVentaDtm planificacion, ContextoSe contexto, PlanificadorDeVentaDtm planificador)
        {
            var lineasDePlanificador = planificador.Detalles<LineaDeUnPlfVentaDtm>(contexto);
            foreach (var linea in lineasDePlanificador)
            {
                new LineaDeUnaPlfVentaDtm
                {
                    IdElemento = planificacion.Id,
                    Orden = linea.Orden,
                    TipoDeLinea = linea.TipoDeLinea,
                    IdUnitario = linea.IdUnitario,
                    Concepto = linea.Concepto,
                    Cantidad = linea.Cantidad,
                    Coste = linea.Coste,
                    Venta = linea.Venta,
                    Anotacion = linea.Anotacion,
                    Descuento = linea.Descuento,
                    IdIvaR = linea.IdIvaR,
                    Iva = linea.Iva,
                    Clase = linea.Clase,
                    IdUnidad = linea.IdUnidad,
                    IdNaturaleza = linea.IdNaturaleza
                }
                .Insertar(contexto);
            }
        }
    }

}
