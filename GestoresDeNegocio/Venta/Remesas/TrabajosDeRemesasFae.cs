using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.TrabajosSometidos;
using ServicioDeDatos;
using ServicioDeDatos.Ventas;
using ServicioDeDatos.TrabajosSometidos;
using Utilidades;
using ServicioDeDatos.Elemento;

namespace GestoresDeNegocio.Ventas
{
    public enum enumTrabajosDeRemesasFae
    {
        [Description("Cargar y cierra las remesa de facturas")]
        ProcesosDeRemesasFae
    }

    public class TrabajosDeRemesasFae
    {
        public static TrabajoDeUsuarioDtm SometerProcesosDeRemesasFae(ContextoSe contexto)
        {
            var dll = Assembly.GetExecutingAssembly().GetName().Name;
            var clase = typeof(TrabajosDeRemesasFae).FullName;
            var ts = GestorDeTrabajosSometido.CrearObtener(contexto, enumTrabajosDeRemesasFae.ProcesosDeRemesasFae.Descripcion(), dll, clase, nameof(enumTrabajosDeRemesasFae.ProcesosDeRemesasFae), comunicarFin: true            );
            var datosDeCreacion = new Dictionary<string, object>
            {
                { nameof(TrabajoDeUsuarioDtm.Planificado), DateTime.Now.AddDays(1) },
                { nameof(TrabajoDeUsuarioDtm.Periodicidad), 86400}
            };
            return GestorDeTrabajosDeUsuario.CrearSiNoEstaPendiente(contexto, ts, datosDeCreacion);
        }

        public static void ProcesosDeRemesasFae(EntornoDeTrabajo entorno)
        {
            var contexto = entorno.contextoDelProceso;
            contexto.IniciarTraza(nameof(ProcesosDeRemesasFae));
            try
            {
                entorno.CrearTraza("Inicio del proceso");
                var trazaInfDtm = entorno.CrearTraza($"Traza informativa del proceso");

                //seleccinar las remesas pendientes de cargar y pendientes de cerrar
                var estadosPresentada = VariableDeRemesasFae.Estados(enumEtapasDeRemesasFae.REM_Etapa_De_Presentacion);

                if (estadosPresentada == ltrEstados.EstadoNulo)
                {
                    entorno.CrearTraza($"la etapa {enumEtapasDeRemesasFae.REM_Etapa_De_Presentacion} está sin definir, no se procesarán las remesas");
                }
                else
                {
                    var filtros = new List<ClausulaDeFiltrado>
                    {
                      new ClausulaDeFiltrado(nameof(RemesaFaeDtm.IdEstado), enumCriteriosDeFiltrado.esAlgunoDe, estadosPresentada)
                    };
                    var remesas = enumNegocio.RemesaFae.SeleccionarPorFiltro<RemesaFaeDtm>(contexto, filtros, parametros: new Dictionary<string, object> { { ltrParametrosNeg.ValidarPermisosDeConsulta, false } });

                    //para cada remesa
                    foreach (var rem in remesas)
                    {
                        var tran = contexto.IniciarTransaccion();
                        try
                        {
                            entorno.ActualizarTraza(trazaInfDtm, $"procesando la {rem.Referencia}");
                            if (rem.CargarEl.Fecha() <= DateTime.Now && rem.CargarEl is null)
                                rem.Cargar(contexto, DateTime.Now.Date);

                            if (rem.FechaMaximaDeDevolucion(contexto) < DateTime.Now)
                                rem.TransitarALaEtapa(contexto, enumEtapasDeRemesasFae.REM_Etapa_De_Cierre.EstadosDeLaEtapa(), new Dictionary<string, object> { { ltrParametrosNeg.EstaEjecutandoUnaAccion, true } });

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
            }
            finally
            {
                entorno.CrearTraza($"Fin del proceso realizado");
                contexto.CerrarTraza();
            }
        }

    }
}
