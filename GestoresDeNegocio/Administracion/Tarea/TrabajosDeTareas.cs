using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.TrabajosSometidos;
using ModeloDeDto;
using ModeloDeDto.Tarea;
using ServicioDeDatos;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Gastos;
using ServicioDeDatos.Tarea;
using ServicioDeDatos.TrabajosSometidos;
using Utilidades;

namespace GestoresDeNegocio.Tarea
{
    public enum enumTrabajosDeTareas
    {
        [Description("Recalcular prioridad")]
        RecalcularPrioridad,
    }

    public class TrabajosDeTareas
    {
        public static TrabajoDeUsuarioDtm SometerRecalcularPrioridad(ContextoSe contexto)
        {
            var dll = Assembly.GetExecutingAssembly().GetName().Name;
            var clase = typeof(TrabajosDeTareas).FullName;
            var ts = GestorDeTrabajosSometido.CrearObtener(contexto, enumTrabajosDeTareas.RecalcularPrioridad.Descripcion(), dll, clase, nameof(enumTrabajosDeTareas.RecalcularPrioridad), comunicarFin: true);

            var parametrosEntrada = new Dictionary<string, object> { };

            var datosDeCreacion = new Dictionary<string, object>
            {
                { nameof(TrabajoDeUsuarioDtm.Parametros), parametrosEntrada.ToJson() },
                { nameof(TrabajoDeUsuarioDtm.Planificado), DateTime.Now.AddMinutes(-1) },
            };
            return GestorDeTrabajosDeUsuario.CrearSiNoEstaPendiente(contexto, ts, datosDeCreacion);
        }

        public static void RecalcularPrioridad(EntornoDeTrabajo entorno)
        {
            var contexto = entorno.contextoDelProceso;
            var tareasVivas = contexto.SeleccionarTodos<TareaDtm>(new List<ClausulaDeFiltrado> {
                new ClausulaDeFiltrado {Clausula = ltrDeUnaTarea.ConPrioridad, Criterio = enumCriteriosDeFiltrado.igual, Valor = true.ToString()}
                }, negocio: enumNegocio.Tarea);

            var ahora = DateTime.Now;

            foreach (var tarea in tareasVivas)
            {
                var tran = contexto.IniciarTransaccion();
                try
                {
                    var prioridad = tarea.Prioridad.Value;

                    if (tarea.EstaEnLaEtapa(enumEtapasDeTareas.TAR_Etapa_Inicial))
                    {
                        if (prioridad == enumPrioridad.Baja && tarea.FechaCreacion < ahora.AddMonths(-1))
                            CambiarPrioridadPorAntiguedad(tarea, contexto, enumPrioridad.Media);
                        else if (prioridad == enumPrioridad.Media && tarea.FechaCreacion < ahora.AddMonths(-2))
                            CambiarPrioridadPorAntiguedad(tarea, contexto, enumPrioridad.Alta);
                        else if (prioridad == enumPrioridad.Alta && tarea.FechaCreacion < ahora.AddMonths(-3))
                            CambiarPrioridadPorAntiguedad(tarea, contexto, enumPrioridad.Urgente);
                        else if (prioridad == enumPrioridad.Urgente && tarea.FechaCreacion < ahora.AddMonths(-4))
                            CambiarPrioridadPorAntiguedad(tarea, contexto, null);
                    }
                    else if (tarea.EstaEnLaEtapa(enumEtapasDeTareas.TAR_Etapa_Asignada) || tarea.EstaEnLaEtapa(enumEtapasDeTareas.TAR_Etapa_En_Resolucion))
                    {
                        if (!tarea.HayPlanificacion(contexto))
                        {
                            if (prioridad == enumPrioridad.Baja && tarea.FechaCreacion < ahora.AddMonths(-1))
                                CambiarPrioridadPorAntiguedad(tarea, contexto, enumPrioridad.Media);
                            else if (prioridad == enumPrioridad.Media && tarea.FechaCreacion < ahora.AddMonths(-2))
                                CambiarPrioridadPorAntiguedad(tarea, contexto, enumPrioridad.Alta);
                            else if (prioridad == enumPrioridad.Alta && tarea.FechaCreacion < ahora.AddMonths(-3))
                                CambiarPrioridadPorAntiguedad(tarea, contexto, enumPrioridad.Urgente, avisarAlCreador: true);
                            else if (prioridad == enumPrioridad.Urgente && tarea.FechaCreacion < ahora.AddMonths(-4))
                                CambiarPrioridadPorAntiguedad(tarea, contexto, null);
                        }
                        else
                        {
                            var planificacion = tarea.Planificacion(contexto, errorSiNoHay: false);

                            if (planificacion.PlfDeFin == null || planificacion.PlfDeFin > ahora)
                            {
                                if (planificacion.PlfDeInicio != null && planificacion.PlfDeInicio < ahora.AddMonths(-1))
                                {
                                    if (prioridad == enumPrioridad.Baja)
                                        CambiarPrioridadPorAntiguedad(tarea, contexto, enumPrioridad.Media);
                                    else if (prioridad == enumPrioridad.Media)
                                        CambiarPrioridadPorAntiguedad(tarea, contexto, enumPrioridad.Alta);
                                    else if (prioridad == enumPrioridad.Alta)
                                        CambiarPrioridadPorAntiguedad(tarea, contexto, enumPrioridad.Urgente, avisarAlCreador: true);
                                }
                            }
                            else if (prioridad != enumPrioridad.Urgente)
                            {
                                CambiarPrioridadPorAntiguedad(tarea, contexto, enumPrioridad.Urgente, avisarAlCreador: true);
                            }
                        }
                    }

                    contexto.Commit(tran);
                }
                catch (Exception ex)
                {
                    contexto.Rollback(tran);
                    entorno.AnotarError($"No se ha podido recalcular la prioridad de la tarea '{tarea.Referencia}'", ex);
                }
            }
        }

        private static void CambiarPrioridadPorAntiguedad(TareaDtm tarea, ContextoSe contexto, enumPrioridad? nueva, bool avisarAlCreador = false)
        {
            var anterior = tarea.Prioridad.Value;
            tarea.Prioridad = nueva;
            tarea.Modificar(contexto);

            var descripcion = nueva == null
                ? $"El proceso '{enumTrabajosDeTareas.RecalcularPrioridad.Descripcion()}' ha anulado la prioridad '{anterior.Descripcion()}'"
                : $"El proceso '{enumTrabajosDeTareas.RecalcularPrioridad.Descripcion()}' ha pasado la prioridad '{anterior.Descripcion()}' a '{nueva.Value.Descripcion()}'";
            tarea.CrearTraza(contexto, "Recálculo de prioridad", descripcion);

            if (avisarAlCreador)
            {
                var creador = contexto.SeleccionarPorId<UsuarioDtm>(tarea.IdUsuaCrea);
                contexto.CrearCorreo(new List<string> { creador.eMail },
                    $"La tarea {tarea.Referencia} ha pasado a prioridad urgente",
                    $"La tarea adjunta ha pasado a prioridad urgente.",
                    elementos: new List<TipoDtoElmento> {
                        new TipoDtoElmento {
                            TipoDto = typeof(TareaDto).FullName,
                            IdElemento = tarea.Id,
                            Referencia = tarea.Expresion }
                    },
                    archivos: null,
                    parametros: new Dictionary<string, object> { { ltrParametrosNeg.EstaEjecutandoUnaAccion, true } });
            }
        }
    }
}
