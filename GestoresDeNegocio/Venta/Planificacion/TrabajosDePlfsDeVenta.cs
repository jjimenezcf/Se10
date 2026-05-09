using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.TrabajosSometidos;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Ventas;
using ServicioDeDatos.TrabajosSometidos;
using Utilidades;
using static ServicioDeDatos.Ventas.VariablesDePlfsDeVenta;

namespace GestoresDeNegocio.Ventas
{
    public enum enumTrabajosDePlfsDeVenta
    {
        [Description("Generar planificaciones de venta")]
        GenerarPlanificacionesDeVenta
    }

    public class TrabajosDePlfsDeVenta
    {
        public static TrabajoDeUsuarioDtm SometerGenerarPlanificacionesDeVenta(ContextoSe contexto)
        {
            var dll = Assembly.GetExecutingAssembly().GetName().Name;
            var clase = typeof(TrabajosDePlfsDeVenta).FullName;
            var ts = GestorDeTrabajosSometido.CrearObtener(contexto,enumTrabajosDePlfsDeVenta.GenerarPlanificacionesDeVenta.Descripcion(), dll, clase, nameof(enumTrabajosDePlfsDeVenta.GenerarPlanificacionesDeVenta), comunicarFin: false            );
            var datosDeCreacion = new Dictionary<string, object>
            {
                { nameof(TrabajoDeUsuarioDtm.Planificado), DateTime.Now.AddDays(1) },
                { nameof(TrabajoDeUsuarioDtm.Periodicidad), 86400}
            };
            return GestorDeTrabajosDeUsuario.CrearSiNoEstaPendiente(contexto, ts, datosDeCreacion);
        }

        public static void GenerarPlanificacionesDeVenta(EntornoDeTrabajo entorno)
        {
            var contexto = entorno.contextoDelProceso;
            contexto.IniciarTraza(nameof(GenerarPlanificacionesDeVenta));
            try
            {
                entorno.CrearTraza("Inicio del proceso");
                var trazaInfDtm = entorno.CrearTraza($"Traza informativa del proceso");

                //seleccinar las planficaciones vencidas 
                var filtros = new List<ClausulaDeFiltrado>
                {
                    new ClausulaDeFiltrado(nameof(PlanificacionDeVentaDtm.EjecutarEl), enumCriteriosDeFiltrado.menor, DateTime.Now.ToString()),
                    new ClausulaDeFiltrado(nameof(PlanificacionDeVentaDtm.IdEstado), enumCriteriosDeFiltrado.esAlgunoDe, VariablesDePlfsDeVenta.Estados(enumEtapasDePlfsDeVenta.PLF_Etapa_Pendiente))
                };
                var plfs = enumNegocio.PlanificacionDeVenta.SeleccionarPorFiltro<PlanificacionDeVentaDtm>(contexto, filtros, parametros: new Dictionary<string, object> { { ltrParametrosNeg.ValidarPermisosDeConsulta, false } });

                //para cada planificación
                foreach (var plf in plfs)
                {
                    var tran = contexto.IniciarTransaccion();
                    try
                    {
                        entorno.ActualizarTraza(trazaInfDtm, $"transitando la {plf.Referencia}");
                        TransicionDtm transicion = plf.TransicionPosible(contexto, enumEtapasDePlfsDeVenta.PLF_Etapa_Generada, enumEtapasDePlfsDeVenta.PLF_Etapa_Generada.Lista());
                        plf.Transitar(contexto, transicion.Id);
                        entorno.CrearTraza($"Planificación {plf.Referencia} generada");
                        contexto.Commit(tran);
                    }
                    catch (Exception ex)
                    {
                        entorno.AnotarError($"No se ha podido generar la planificación {plf.Referencia}", ex);
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
