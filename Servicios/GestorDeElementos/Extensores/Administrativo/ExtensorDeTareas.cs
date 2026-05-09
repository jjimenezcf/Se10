using Microsoft.Win32;
using ModeloDeDto.Gastos;
using ModeloDeDto.Tarea;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.Presupuesto;
using ServicioDeDatos.RegistroEs;
using ServicioDeDatos.Tarea;
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

    public class ltrDeUnaTarea
    {
        public const string VinculosATareas = nameof(VinculosATareas);
        public const string IdTarea = nameof(IdTarea);
        public const string EventoDePlanificacion = $"planificación de la tarea: [{nameof(PlanificacionDeVentaDtm.Referencia)}]";

        public const string DependeDeParteTr = nameof(DependeDeParteTr);

        public const string IdPresupuesto = nameof(IdPresupuesto);
        public const string RelacionadaConPpt = nameof(RelacionadaConPpt);

        public const string IdExpediente = nameof(IdExpediente);
        public const string RelacionadaConExpediente = nameof(RelacionadaConExpediente);
        public const string VincularAlExpediente = nameof(VincularAlExpediente);

        public const string IdRegistroEs = nameof(IdRegistroEs);

        public const string IdSolicitante = nameof(IdSolicitante);
        public const string IdResponsable = nameof(IdResponsable);
        public const string Asignacion = nameof(Asignacion);

        public const string SolicitantesDeTarea = nameof(SolicitantesDeTarea);
        public const string ResponsablesDeTarea = nameof(ResponsablesDeTarea);
        public const string FacturasDeTareas = nameof(FacturasDeTareas);
        public const string ExpedientesDeTareas = nameof(ExpedientesDeTareas);
        public const string PresupuestosDeTareas = nameof(PresupuestosDeTareas);
        public const string FiltroPorEtapa = nameof(FiltroPorEtapa);       


        public const string IdFacturaEmt = nameof(IdFacturaEmt);
        public const string Facturada = nameof(Facturada);

        public const string Accion_ExluirDeLaFactura = nameof(Accion_ExluirDeLaFactura);
        public const string Accion_IncluirEnLaFactura = nameof(Accion_IncluirEnLaFactura);


        public const string TrazaDeCopiaDeTarea = nameof(TrazaDeCopiaDeTarea);
    }

    public static class ExtensorDeTareas
    {
        public static IQueryable<VinculoDtm> Tareas(this enumNegocio negocio, ContextoSe contexto)
        {
            switch (negocio)
            {
                case enumNegocio.Registro:
                    return contexto.Set<TareasDeUnRegistroDtm>();
                case enumNegocio.Expediente:
                    return contexto.Set<TareasDeUnExpedienteDtm>();
                case enumNegocio.Pleito:
                    return contexto.Set<TareasDeUnPleitoDtm>();
                case enumNegocio.Contrato:
                    return contexto.Set<TareasDeUnContratoDtm>();
                case enumNegocio.Presupuesto:
                    return contexto.Set<TareasDeUnPresupuestoDtm>();
                case enumNegocio.ParteDeTrabajo:
                    return contexto.Set<TareasDeUnParteTrDtm>();
            }

            throw new Exception($"Se debe indicar como obtener las tareas vinculadas al negocio: {negocio}");
        }

        public static IQueryable<T> FiltrosPorTareas<T>(this IQueryable<T> consulta, ContextoSe Contexto, List<ClausulaDeFiltrado> filtros)
        where T : RegistroDtm
        {
            if (!ApiDeInterfaceDtm.ImplementaUsaFlujo(typeof(T)))
                return consulta;

            var negocio = typeof(T).NegocioDeUnDtm();
            if (negocio.UsaTareas())
            {
                ClausulaDeFiltrado filtro = filtros.FirstOrDefault(f => f.Clausula.ToLower() == ltrDeUnaTarea.IdTarea.ToLower());
                if (filtro != null)
                {
                    consulta = consulta.Where(x => negocio.Tareas(Contexto).Any(y => y.idElemento1 == x.Id && y.idElemento2 == filtro.Valor.Entero()));
                    filtro.Aplicado = true;

                    var filtroQueMostrar = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrParametrosNeg.QueMostrar.ToLower() && x.Criterio == enumCriteriosDeFiltrado.diferente && !x.Aplicado);
                    if (filtroQueMostrar != null)
                        filtroQueMostrar.Aplicado = true;

                    return consulta;
                }
                
                filtro = filtros.FirstOrDefault(f => f.Clausula.ToLower() == ltrDeUnaTarea.VinculosATareas.ToLower());
                if (filtro != null)
                {
                    if (filtro.Valor.Entero() == ltrParametrosNeg.ConRelacion)
                        consulta = consulta.Where(x => negocio.Tareas(Contexto).Any(y => y.idElemento1 == x.Id));
                    if (filtro.Valor.Entero() == ltrParametrosNeg.SinRelacion)
                        consulta = consulta.Where(x => !negocio.Tareas(Contexto).Any(y => y.idElemento1 == x.Id));
                    filtro.Aplicado = true;
                }
            }
            return consulta;
        }

        public static bool UsaLaAmpliacionDe(ContextoSe contexto, int idTipo, Type tipoAmpliacion)
        {
            var tipoDtm = (TipoDeTareaDtm)enumNegocio.Tarea.CrearGestorDeTipo(contexto).LeerRegistroPorId(idTipo, aplicarJoin: false);
            if (tipoDtm.UsaPlanificacion && tipoAmpliacion == typeof(PlfDeTareaDtm))
                return true;

            return false;
        }

        public static void AntesDeAsignar(this TareaDtm tarea, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            var planificacion = tarea.Planificacion(contexto);
            if (planificacion == null) return;

            if (tarea.IdResponsable == default)
                Emitir($"la tarea '{tarea.Referencia}' ha de tener responsable indicado");

            if (planificacion.PlfDeInicio == default || planificacion.PlfDeFin == default)
                Emitir($"la tarea '{tarea.Referencia}' ha de tener fecha de planificación inicial y de final");
        }

        public static void TrasAsignar(this TareaDtm tarea, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            var planificacion = tarea.Planificacion(contexto);
            if (planificacion == null) return;

            if (contexto.DatosDeConexion.IdUsuario != (int)tarea.IdResponsable)
            {
                contexto.CrearCorreo(new List<string> { contexto.SeleccionarPorId<UsuarioDtm>((int)tarea.IdResponsable).eMail },
                    $"Se le ha asignado la tarea {tarea.Referencia}",
                    $"Ud. es el encargado de realizar la tarea adjunta.",
                    elementos: new List<ModeloDeDto.TipoDtoElmento> {
                    new ModeloDeDto.TipoDtoElmento {
                       TipoDto  = typeof(TareaDto).FullName,
                       IdElemento = tarea.Id,
                       Referencia = tarea.Expresion}
                        },
                    archivos: null,
                    parametros: new Dictionary<string, object> { { ltrParametrosNeg.EstaEjecutandoUnaAccion, true } });
            }

            tarea.CrearEventoDePlanificacion(contexto);
        }

        public static void AntesDeDesasignar(this TareaDtm tarea, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            var planificacion = tarea.Planificacion(contexto, errorSiNoHay: false);
            if (planificacion == null) return;

            if (planificacion.Iniciada != default || planificacion.Finalizada != default || planificacion.Duracion != default || planificacion.MedidoEn != default)
            {
                Emitir($"la tarea '{tarea.Referencia}' no se puede devolver a inicial con información de inico, fin o durabilida o en que se mide, blanquee los campos previamente");
            }
        }

        public static void TrasDesasignar(this TareaDtm tarea, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            if (tarea.IdResponsable == default)
                return;

            var planificacion = tarea.Planificacion(contexto, errorSiNoHay: false);
            if (planificacion == null) return;

            if (contexto.DatosDeConexion.IdUsuario != (int)tarea.IdResponsable)
            {
                contexto.CrearCorreo(new List<string> { contexto.SeleccionarPorId<UsuarioDtm>((int)tarea.IdResponsable).eMail },
                $"Se le ha desasignado la tarea {tarea.Referencia}",
                $"ya no tiene que realizar la tarea adjunta.",
                elementos: new List<ModeloDeDto.TipoDtoElmento> {
                    new ModeloDeDto.TipoDtoElmento {
                       TipoDto  = typeof(TareaDto).FullName,
                       IdElemento = tarea.Id,
                       Referencia = tarea.Expresion}
                    },
                archivos: null,
                parametros: new Dictionary<string, object> { { ltrParametrosNeg.EstaEjecutandoUnaAccion, true } });
            }

            var responsable = contexto.SeleccionarPorId<UsuarioDtm>((int)tarea.IdResponsable);
            var eventos = contexto.SeleccionarEventos(responsable.IdAgenda, tarea.Id, ltrDeUnaTarea.EventoDePlanificacion.Replace($"[{nameof(PlanificacionDeVentaDtm.Referencia)}]", tarea.Referencia));
            var p = new Dictionary<string, object> { { ltrParametrosNeg.EstaEjecutandoUnaAccion, true } };
            foreach (var evento in eventos) if (evento.EsDelSistema)
                    tarea.Desvincular(contexto, evento, p);
        }

        public static void AntesDeComenzar(this TareaDtm tarea, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            var planificacion = tarea.Planificacion(contexto);
            if (planificacion == null) return;

            if (planificacion.Iniciada == default) planificacion.Iniciada = DateTime.Now;
            planificacion.Modificar(contexto, parametros);
        }

        public static void AntesDeTerminarTarea(this TareaDtm tarea, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            if (tarea.IdClaseDeElemento is null && enumNegocio.Tarea.HayClasesDelTipo(contexto, tarea.IdTipo))
                Emitir($"No puede finalizar la tarea '{tarea.Referencia}' ya que no le ha indicado una clase");
        }


        public static void AntesDeAnularLaEjecucion(this TareaDtm tarea, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            var planificacion = tarea.Planificacion(contexto);
            if (planificacion == null) return;

            if (planificacion.Iniciada != default) planificacion.Iniciada = default;
            planificacion.Modificar(contexto, parametros);
        }

        public static void AntesDeFinalizar(this TareaDtm tarea, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            var planificacion = tarea.Planificacion(contexto);
            if (planificacion == null) return;

            if (planificacion.Finalizada != default && (DateTime)planificacion.Finalizada > DateTime.Now)
                Emitir($"La tarea {tarea.Referencia} no puede finalizarse con fecha posterior a la del día");

            if (planificacion.Finalizada != default && (DateTime)planificacion.Finalizada < (DateTime)planificacion.Iniciada)
                Emitir($"La tarea {tarea.Referencia} no puede finalizarse con fecha anterior a su inicio");

            if (planificacion.Finalizada != default && planificacion.Duracion != default && planificacion.MedidoEn != default)
                return;

            planificacion.ValidarDatosDeDurabilidad(tarea);
        }

        public static void TrasFinalizar(this TareaDtm tarea, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            var planificacion = tarea.Planificacion(contexto);
            if (planificacion == null) return;

            if (planificacion.Finalizada == default) planificacion.Finalizada = DateTime.Now;

            if (planificacion.Duracion == default)
            {
                if (!enumNegocio.Tarea.LeerCrearParametro(contexto, enumParametrosDeTareas.TAR_Proponer_Valoracion_Al_Finalizar, "N").Valor.EsTrue())
                {
                    Emitir($"Debe indicar la duración de la tarea '{tarea.Referencia}' o actualizar el parémetro '{enumParametrosDeTareas.TAR_Proponer_Valoracion_Al_Finalizar}' a valor 'S'");
                }
                var duracion = tarea.CalcularDuracion(contexto);
                planificacion.Duracion = duracion.duracion;
                planificacion.MedidoEn = duracion.medidoEn;
            }

            planificacion.Modificar(contexto, parametros, esUnaAccion: true);
        }

        public static void AntesDeNoAceptarLaFinalizacion(this TareaDtm tarea, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            var planificacion = tarea.Planificacion(contexto);
            if (planificacion == null) return;

            if (enumNegocio.Tarea.LeerCrearParametro(contexto, enumParametrosDeTareas.TAR_Eliminar_Valoracio_Tras_Rechazar, "N").Valor.EsTrue())
            {
                planificacion.Duracion = default;
                planificacion.MedidoEn = default;
                tarea.CrearTraza(contexto,
                    "Datos de ejecución blanqueados",
                    $"Al cancelar la finalización se han suprimido los datos de duración de la tarea, que eran:{Environment.NewLine}" +
                    $"Finalizada el: {(planificacion.Finalizada == default ? "--" : (DateTime)planificacion.Finalizada)}{Environment.NewLine}" +
                    $"Duración: {(planificacion.Duracion == default ? "--" : (decimal)planificacion.Duracion)}{Environment.NewLine}" +
                    $"Medido: {(planificacion.MedidoEn == default ? "--" : ((enumDurabilidad)planificacion.MedidoEn).Descripcion())}"
                );
            }
            planificacion.Finalizada = default;
            planificacion.Modificar(contexto, parametros);
        }

        public static void PersistirEvento(this PlfDeTareaDtm plfDeTarea, ContextoSe contexto, ParametrosDeNegocio parametros)
        {
            if (!parametros.Modificando)
                return;

            var tarea = plfDeTarea.AmpliacionDe<TareaDtm>(contexto);
            if (!tarea.EstaEnLaEtapa(enumEtapasDeTareas.TAR_Etapa_Asignada))
                return;

            var nombreEvento = ltrDeUnaTarea.EventoDePlanificacion.Replace($"[{nameof(TareaDtm.Referencia)}]", tarea.Referencia);
            var eventos = contexto.SeleccionarEventos(tarea.Id, nombreEvento);

            if (eventos.Count > 1)
            {
                var p = new Dictionary<string, object> { { ltrParametrosNeg.EstaEjecutandoUnaAccion, true } };
                for (var i = eventos.Count() - 1; i > 0; i--)
                    tarea.Desvincular(contexto, eventos[i], p);
            }

            if (eventos.Count > 0)
                tarea.ModificarEvento(eventos[0], contexto);
        }

        public static void ValidarTareasVinculadasEstanFinalizadas(this ElementoConCgDtm elemento, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            var tareas = elemento.Vinculados<TareaDtm>(contexto, parametros,
                new Dictionary<string, object> {
                    {
                        ltrDeVinculos.EstadosDelVinculadoDiferentes,
                        enumEtapasDeTareas.TAR_Etapa_Terminada.Estados() + "," +  enumEtapasDeTareas.TAR_Etapa_Cancelado.Estados()
                    }
                });

            foreach (var tarea in tareas.Where(tarea => !tarea.EstaEnLaEtapa(enumEtapasDeTareas.TAR_Etapa_Terminada) &&
                                                        !tarea.EstaEnLaEtapa(enumEtapasDeTareas.TAR_Etapa_Cancelado)))
                Emitir($"El {NegociosDeSe.NegocioDeUnDtm(elemento.GetType()).Singular(true)} '{elemento.Referencia}' tiene la {enumNegocio.Tarea.Singular(true)} {tarea.Referencia} sin terminar");
        }

        private static void CrearEventoDePlanificacion(this TareaDtm tarea, ContextoSe contexto)
        {
            var responsable = contexto.SeleccionarPorId<UsuarioDtm>((int)tarea.IdResponsable);
            var evento = new EventoDeAgendaDtm();
            evento.IdAgenda = responsable.IdAgenda;
            evento.IdElemento = tarea.Id;
            evento.IdNegocio = enumNegocio.Tarea.IdNegocio();
            evento.Inicio = (DateTime)tarea.Ampliacion<PlfDeTareaDtm>(contexto).PlfDeInicio;
            evento.Fin = (DateTime)tarea.Ampliacion<PlfDeTareaDtm>(contexto).PlfDeFin;
            evento.Nombre = ltrDeUnaTarea.EventoDePlanificacion.Replace($"[{nameof(TareaDtm.Referencia)}]", tarea.Referencia);
            evento.Descripcion = tarea.DescripcioEvento();
            evento.EsDelSistema = true;
            GestorDeVinculos.Vincular(contexto, tarea, evento, new Dictionary<string, object> { { ltrParametrosNeg.EstaEjecutandoUnaAccion, true } });
        }

        private static void ModificarEvento(this TareaDtm tarea, EventoDeAgendaDtm evento, ContextoSe contexto)
        {
            var plf = tarea.Ampliacion<PlfDeTareaDtm>(contexto);

            if (plf.PlfDeInicio is null)
            {
                var p = new Dictionary<string, object> { { ltrParametrosNeg.EstaEjecutandoUnaAccion, true } };
                tarea.Desvincular(contexto, evento,p );
                return;
            }

            if (evento.Inicio == plf.PlfDeInicio && evento.Fin == plf.PlfDeFin && evento.Descripcion == tarea.DescripcioEvento())
                return;

            evento.Inicio = (DateTime)plf.PlfDeInicio;
            evento.Fin = (DateTime)plf.PlfDeFin;
            evento.Descripcion = tarea.DescripcioEvento();
            evento.EsDelSistema = true;
            evento.Modificar(contexto, esUnaAccion: true);
        }

        public static void ValidarPlanificacion(this PlfDeTareaDtm planificacion, ContextoSe contexto, ParametrosDeNegocio parametros)
        {
            var tarea = planificacion.AmpliacionDe<TareaDtm>(contexto);

            if (parametros.Modificando)
            {
                var plfAnterior = (PlfDeTareaDtm)parametros.registroEnBd;
                if (plfAnterior.PlfDeInicio != planificacion.PlfDeInicio || plfAnterior.PlfDeFin != planificacion.PlfDeFin)
                {
                    if (!tarea.EstaEnLaEtapa(enumEtapasDeTareas.TAR_Etapa_Inicial) && !tarea.EstaEnLaEtapa(enumEtapasDeTareas.TAR_Etapa_Asignada))
                        Emitir($"No se puede modificar las fechas de la planificación de la tarea {tarea.Referencia} por estar en la etapa {tarea.Etapa()}");
                }
            }

            if (planificacion.PlfDeFin != default && planificacion.PlfDeInicio != default && (DateTime)planificacion.PlfDeInicio > (DateTime)planificacion.PlfDeFin)
                Emitir("La fecha de planificación final de la tarea a de ser mayor que la planificación inicial");

            if (planificacion.Finalizada != default && planificacion.Iniciada != default && (DateTime)planificacion.Iniciada > (DateTime)planificacion.Finalizada)
                Emitir("La fecha final de la tarea a de ser mayor que la inicial");

            //if (planificacion.PlfDeInicio != default && tarea.FechaCreacion > (DateTime)planificacion.PlfDeInicio)
            //    Emitir($"La fecha de planificación inicial '{(DateTime)planificacion.PlfDeInicio}' no puede ser anterior a la fecha de creación de la tarea '{tarea.FechaCreacion}'");

            if (planificacion.Iniciada != default && tarea.FechaCreacion > (DateTime)planificacion.Iniciada)
                Emitir($"La fecha de inicio '{(DateTime)planificacion.Iniciada}' no puede ser anterior a la fecha de creación de la tarea '{tarea.FechaCreacion}'");

            planificacion.ValidarDatosDeDurabilidad(tarea);
        }

        public static void ValidarDatosPorEtapas(this PlfDeTareaDtm planificacion, ContextoSe contexto, PlfDeTareaDtm anterior, Dictionary<string,object> parametros)
        {
            var tarea = planificacion.AmpliacionDe<TareaDtm>(contexto);

            if ((tarea.EstaEnLaEtapa(enumEtapasDeTareas.TAR_Etapa_En_Espera) || tarea.EstaEnLaEtapa(enumEtapasDeTareas.TAR_Etapa_Validacion)) && !planificacion.Igual(anterior))
                Emitir($"Los datos de la planificación de la tarea {tarea.Referencia} no son modificables");

            if (tarea.EstaEnLaEtapa(enumEtapasDeTareas.TAR_Etapa_En_Resolucion))
            {
                if (!planificacion.Iniciada.Igual(anterior.Iniciada))
                    Emitir($"La fecha de inicio de resolución de la tarea {tarea.Referencia} no es modificable una vez iniciada");

                if (!planificacion.PlfDeFin.Igual(anterior.PlfDeFin))
                    Emitir($"La fecha de planificación de fin de la tarea {tarea.Referencia} no es modificable una vez iniciada");

                if (!planificacion.PlfDeInicio.Igual(anterior.PlfDeInicio))
                    Emitir($"La fecha planificación de inicio de la tarea {tarea.Referencia} no es modificable una vez iniciada");
            }

            if (tarea.EstaEnLaEtapa(enumEtapasDeTareas.TAR_Etapa_Asignada))
            {
                if (planificacion.Finalizada != default || planificacion.Duracion != default || planificacion.MedidoEn != default)
                {
                    var transicion = parametros.LeerValor<TransicionDtm>(ltrParametrosNeg.TransionPendienteDeEjecucion, null);
                    if (transicion != null && enumNegocio.Tarea.Estado(contexto, transicion.IdDestino).Inicial)
                    {
                        planificacion.Finalizada = default;
                        planificacion.Duracion = default;
                        planificacion.MedidoEn = default;
                    }
                    else 
                        Emitir($"La fecha de fin, duración o medida en, de la tarea {tarea.Referencia}, no deben tener valor hasta que no se inicie");
                }
            }

        }

        private static void ValidarDatosDeDurabilidad(this PlfDeTareaDtm planificacion, TareaDtm tarea)
        {
            if (planificacion.Duracion != default && planificacion.MedidoEn == default)
                Emitir($"en la tarea '{tarea.Referencia}' si se indica duración, ha de indicar cómo está medida");

            if (planificacion.Duracion == default && planificacion.MedidoEn != default)
                Emitir($"en la tarea '{tarea.Referencia}' si no se indica duración, no ha de indicar cómo está medida");
        }

        public static FacturaEmtDtm FacturaEmt(this TareaDtm tarea, ContextoSe contexto)
        {
            if (tarea.IdFacturaEmt is null)
                return null;
            if (tarea.FacturaEmt is not null)
                return tarea.FacturaEmt;
            return contexto.SeleccionarPorId<FacturaEmtDtm>((int)tarea.IdFacturaEmt);
        }

        public static PlfDeTareaDtm Planificacion(this TareaDtm tarea, ContextoSe contexto, bool errorSiNoHay = true)
        {
            if (tarea.Planificacion is not null)
                return tarea.Planificacion;

            var tipo = tarea.Tipo<TipoDeTareaDtm>(contexto);
            if (!tipo.UsaPlanificacion)
                return null;

            var planificacion = tarea.Ampliacion<PlfDeTareaDtm>(contexto, errorSiNoHay: false);
            if (planificacion is null && errorSiNoHay)
                Emitir($"Debe indicar una planificación a la tarea '{tarea.Referencia}'");

            return tarea.Planificacion = planificacion;
        }

        public static bool HayPlanificacion(this TareaDtm tarea, ContextoSe contexto)
        {
            var tipo = contexto.SeleccionarPorId<TipoDeTareaDtm>(tarea.IdTipo);
            if (!tipo.UsaPlanificacion)
                return false;
            var planificacion = tarea.Ampliacion<PlfDeTareaDtm>(contexto, errorSiNoHay: false);
            return planificacion is not null;
        }

        public static PlfDeTareaDtm Planificar(this TareaDtm tarea, ContextoSe contexto, DateTime inicio, DateTime fin)
        {
            if (!UsaLaAmpliacionDe(contexto, tarea.IdTipo, typeof(PlfDeTareaDtm)))
                Emitir($"la {enumNegocio.Tarea} del tipo {contexto.SeleccionarPorId<TipoDeTareaDtm>(tarea.IdTipo).Nombre} no usa datos de planificación");

            var planificacion = new PlfDeTareaDtm();
            planificacion.IdElemento = tarea.Id;
            planificacion.PlfDeInicio = inicio;
            planificacion.PlfDeFin = fin;
            return planificacion.Insertar(contexto);
        }

        private static (decimal? duracion, enumDurabilidad? medidoEn) CalcularDuracion(this TareaDtm tarea, ContextoSe contexto)
        {
            var planificacion = tarea.Planificacion(contexto);
            if (planificacion == null) return (null, null);
            long tiempo = 0;
            var hitos = tarea.HitosDeUnaEtapaPosteriorA(contexto, enumEtapasDeTareas.TAR_Etapa_Asignada.Lista(), enumEtapasDeTareas.TAR_Etapa_En_Resolucion.Lista());
            foreach (var hito in hitos)
            {
                if (hito.Tiempo == default) continue;
                tiempo = tiempo + (long)hito.Tiempo;
            }

            TimeSpan duracion = new TimeSpan(tiempo);
            if (duracion.TotalDays < 1)
                return ((decimal)duracion.TotalHours, enumDurabilidad.Horas);

            return ((decimal)duracion.TotalDays, enumDurabilidad.Dias);
        }

        public static TareaDtm Copiar(this TareaDtm origen, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            var idExpediente = (int?)parametros.LeerValor<long?>(nameof(CopiarTareaDto.idExpediente), null);
            var idArchivo = (int?)parametros.LeerValor<long?>(nameof(CopiarTareaDto.IdArchivoAlCopiar), null);
            var enlazarArchivos = parametros.LeerValor<bool>(nameof(CopiarTareaDto.EnlazarArchivos));


            var transaccion = contexto.IniciarTransaccion();
            try
            {
                var nueva = new TareaDtm
                {
                    IdCg = contexto.SeleccionarPorId<CentroGestorDtm>((int)(long)parametros[nameof(CopiarTareaDto.IdCg)]).Id,
                    IdSolicitante = contexto.SeleccionarPorId<InterlocutorDtm>((int)(long)parametros[nameof(CopiarTareaDto.IdSolicitante)]).Id,
                    IdTipo = contexto.SeleccionarPorId<TipoDeTareaDtm>((int)(long)parametros[nameof(CopiarTareaDto.IdTipo)]).Id,
                    Nombre = parametros.LeerValor<string>(nameof(CopiarTareaDto.Nombre)),
                    Descripcion = parametros.LeerValor<string>(nameof(CopiarTareaDto.Descripcion))
                }.Insertar(contexto, new Dictionary<string, object> {
                                 { ltrDeUnaTarea.TrazaDeCopiaDeTarea, $"Copia de la tarea {origen.Expresion}" },
                                 { nameof(TareaDto.IdArchivoAlCrear), idArchivo},
                                 { ltrDeUnaTarea.VincularAlExpediente,idExpediente }
                              });


                if (enlazarArchivos)
                {
                    var archivos = origen.Archivos(contexto);
                    foreach (var archivo in archivos)
                    {
                        GestorDeVinculos.Vincular(contexto, enumNegocio.Tarea, enumNegocio.Archivos, nueva.Id, archivo.Id);
                        archivo.AuditarEnlazar(contexto, origen.Referencia);
                    }
                }

                contexto.Commit(transaccion);
                return nueva;
            }
            catch
            {
                contexto.Rollback(transaccion);
                throw;
            }
        }
        private static string DescripcioEvento(this TareaDtm tarea) => $"Se ha de realizar la tarea: {tarea.Expresion}";

    }
}
