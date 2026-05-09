using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.TrabajosSometidos;
using iText.Commons.Actions.Contexts;
using ServicioDeDatos;
using ServicioDeDatos.Gastos;
using ServicioDeDatos.TrabajosSometidos;
using Utilidades;

namespace GestoresDeNegocio.Gastos
{
    public enum enumTrabajosDeRemesasPag
    {
        [Description("Pagar y cerrar las remesa de facturas")]
        ProcesosDeRemesasPag
    }

    public class TrabajosDeRemesasPag
    {
        public static TrabajoDeUsuarioDtm SometerProcesosDeRemesasPag(ContextoSe contexto)
        {
            var dll = Assembly.GetExecutingAssembly().GetName().Name;
            var clase = typeof(TrabajosDeRemesasPag).FullName;
            var ts = GestorDeTrabajosSometido.CrearObtener(contexto, enumTrabajosDeRemesasPag.ProcesosDeRemesasPag.Descripcion(), dll, clase, nameof(enumTrabajosDeRemesasPag.ProcesosDeRemesasPag), comunicarFin: true            );
            var datosDeCreacion = new Dictionary<string, object>
            {
                { nameof(TrabajoDeUsuarioDtm.Planificado), DateTime.Now.AddDays(1) },
                { nameof(TrabajoDeUsuarioDtm.Periodicidad), 86400}
            };
            return GestorDeTrabajosDeUsuario.CrearSiNoEstaPendiente(contexto, ts, datosDeCreacion);
        }

        public static void ProcesosDeRemesasPag(EntornoDeTrabajo entorno)
        {
            var contexto = entorno.contextoDelProceso;
            contexto.IniciarTraza(nameof(ProcesosDeRemesasPag), true);
            try
            {
                entorno.CrearTraza("Inicio del proceso");
                var trazaInfDtm = entorno.CrearTraza($"Traza informativa del proceso");

                //seleccinar las remesas pendientes de pagar y pendientes de cerrar
                var estadosPresentada = VariableDeRemesasPag.Estados(enumEtapasDeRemesasPag.REM_Etapa_De_Presentacion);

                var filtros = new List<ClausulaDeFiltrado>
                {
                    new ClausulaDeFiltrado(nameof(RemesaPagDtm.IdEstado), enumCriteriosDeFiltrado.esAlgunoDe, estadosPresentada)
                };
                var remesas = enumNegocio.RemesaPag.SeleccionarPorFiltro<RemesaPagDtm>(contexto, filtros, parametros: new Dictionary<string, object> { { ltrParametrosNeg.ValidarPermisosDeConsulta, false } });

                //para cada remesa
                foreach (var rem in remesas)
                {
                    var tran = contexto.IniciarTransaccion();
                    try
                    {
                        entorno.ActualizarTraza(trazaInfDtm, $"procesando la {rem.Referencia}");
                        if (rem.PagarEl.Fecha() <= DateTime.Now && rem.PagadaEl is null)
                            GestorDeRemesasPag.AdelantarPago(contexto, rem.Id, rem.PagarEl.Fecha());

                        if (rem.PagadaEl is null) continue;

                        //a los tres días de estar pagada la transito a cerrada si no lo está
                        if (rem.PagadaEl.Fecha().AddDays(3) < DateTime.Now)
                            rem.TransitarALaEtapa(contexto, enumEtapasDeRemesasPag.REM_Etapa_De_Cierre.EstadosDeLaEtapa(), new Dictionary<string, object> { { ltrParametrosNeg.EstaEjecutandoUnaAccion, true } }, delSistema: false);

                        entorno.CrearTraza($"remesa {rem.Referencia} procesada");
                        contexto.Commit(tran);
                    }
                    catch (Exception ex)
                    {
                        entorno.AnotarError($"No se ha podido procesar la remesa '{rem.Referencia}'", ex);
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
