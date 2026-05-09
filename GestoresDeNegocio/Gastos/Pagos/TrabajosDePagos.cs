using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.TrabajosSometidos;
using ServicioDeDatos;
using ServicioDeDatos.Gastos;
using ServicioDeDatos.TrabajosSometidos;
using Utilidades;

namespace GestoresDeNegocio.Gastos
{
    public enum enumTrabajosDePagos
    {
        [Description("Cerrar los pagos")]
        ProcesosDePagos
    }

    public class TrabajosDePagos
    {
        public static TrabajoDeUsuarioDtm SometerProcesosDePagos(ContextoSe contexto)
        {
            var dll = Assembly.GetExecutingAssembly().GetName().Name;
            var clase = typeof(TrabajosDePagos).FullName;
            var ts = GestorDeTrabajosSometido.CrearObtener(contexto, enumTrabajosDePagos.ProcesosDePagos.Descripcion(), dll, clase, nameof(enumTrabajosDePagos.ProcesosDePagos), comunicarFin: true);
            var datosDeCreacion = new Dictionary<string, object>
            {
                { nameof(TrabajoDeUsuarioDtm.Planificado), DateTime.Now.AddDays(1) },
                { nameof(TrabajoDeUsuarioDtm.Periodicidad), 86400}
            };
            return GestorDeTrabajosDeUsuario.CrearSiNoEstaPendiente(contexto, ts, datosDeCreacion);
        }

        public static void ProcesosDePagos(EntornoDeTrabajo entorno)
        {
            var contexto = entorno.contextoDelProceso;
            contexto.IniciarTraza(nameof(ProcesosDePagos));
            try
            {
                entorno.CrearTraza("Inicio del proceso");
                var trazaInfDtm = entorno.CrearTraza($"Traza informativa del proceso");

                var estadosPdt = VariableDePagos.Estados(enumEtapasDePagos.PAG_Etapa_Pendiente);

                var filtros = new List<ClausulaDeFiltrado>
                {
                    new ClausulaDeFiltrado(nameof(PagoDtm.IdEstado), enumCriteriosDeFiltrado.esAlgunoDe, estadosPdt)
                };
                var pagos = enumNegocio.Pago.SeleccionarPorFiltro<PagoDtm>(contexto, filtros, parametros: new Dictionary<string, object> { { ltrParametrosNeg.ValidarPermisosDeConsulta, false } });

                foreach (var pago in pagos)
                {
                    var tran = contexto.IniciarTransaccion();
                    try
                    {
                        entorno.ActualizarTraza(trazaInfDtm, $"procesando el {pago.Referencia}");
                        if (pago.PagadoEl is null) continue;

                        if (pago.PagadoEl.Fecha() <= DateTime.Now.Date)
                        {
                            if (pago.PagarEl is null) pago.PagarEl = pago.PagadoEl;
                            pago.TransitarALaEtapa(contexto, enumEtapasDePagos.PAG_Etapa_Pagado.EstadosDeLaEtapa(), new Dictionary<string, object> { { ltrParametrosNeg.EstaEjecutandoUnaAccion, true } }, delSistema: false);
                        }
                        entorno.CrearTraza($"pago {pago.Referencia} procesado");
                        contexto.Commit(tran);
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message.Contains("debe tener un justificante de pago, asocie el archivo"))
                            entorno.CrearTraza(string.Format(ltrDeUnPago.Mensaje_FaltaDePago, pago.Referencia));
                        else
                            entorno.AnotarError($"No se ha podido procesar el pago '{pago.Referencia}'", ex);
                        contexto.Rollback(tran);
                    }
                }
            }
            finally
            {
                entorno.CrearTraza($"Fin del proceso realizado");
                contexto.CerrarTraza();
            }
        }

    }
}
