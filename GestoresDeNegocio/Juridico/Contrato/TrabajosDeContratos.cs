using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.TrabajosSometidos;
using ModeloDeDto;
using ModeloDeDto.Juridico;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.TrabajosSometidos;
using Utilidades;

namespace GestoresDeNegocio.Juridico
{
    public enum enumTrabajosDeContratos
    {
        [Description("Notificar que se ha sobrepasado el porcentaje de aviso")]
        NotificarPorcentajeDeAvisoSobrepasado,
        [Description("Motor de contrato")]
        MotorDeContratos,
        [Description("Generar planificadores de un contrato")]
        GenerarPlanificadoresDeUnContrato,
        [Description("Motor de planificador")]
        MotorDePlanificador,
        [Description("Activar matrículas de guarderías")]
        ActivarMatriculasDeGuarderia,
    }

    public class TrabajosDeContratos
    {
        public static TrabajoDeUsuarioDtm SometerNotificarPorcentajeDeAvisoSobrepasado(ContextoSe contexto)
        {
            var dll = Assembly.GetExecutingAssembly().GetName().Name;
            var clase = typeof(TrabajosDeContratos).FullName;
            var ts = GestorDeTrabajosSometido.CrearObtener(contexto, enumTrabajosDeContratos.NotificarPorcentajeDeAvisoSobrepasado.Descripcion(), dll, clase, nameof(enumTrabajosDeContratos.NotificarPorcentajeDeAvisoSobrepasado), comunicarFin: false            );
            var datosDeCreacion = new Dictionary<string, object>
            {
                { nameof(TrabajoDeUsuarioDtm.Planificado), DateTime.Now.AddDays(1) },
                { nameof(TrabajoDeUsuarioDtm.Periodicidad), 86400}
            };
            return GestorDeTrabajosDeUsuario.CrearSiNoEstaPendiente(contexto, ts, datosDeCreacion);
        }

        public static void NotificarPorcentajeDeAvisoSobrepasado(EntornoDeTrabajo entorno)
        {
            var contexto = entorno.contextoDelProceso;
            contexto.IniciarTraza(nameof(NotificarPorcentajeDeAvisoSobrepasado));
            try
            {
                entorno.CrearTraza("Inicio del proceso");
                var etapaDeVigencia = VariablesDeContratos.etapaVigente.ToLista<int>(Simbolos.Coma);

                var sinNotificar = contexto.Set<SaldosDelContratoDtm>().Where(y => y.Notificado == false || y.Notificado == null &&
                                   contexto.Set<ContratoDtm>().Where(x => x.Id == y.IdElemento && etapaDeVigencia.Contains(x.IdEstado))
                                   .Any());
                var tran = contexto.IniciarTransaccion();
                try
                {
                    foreach (var saldos in sinNotificar)
                    {
                        var contrato = saldos.Elemento<ContratoDtm>(contexto);

                        if (contrato.ClaseDeContrato != enumClaseDeContrato.Venta || contrato.ClaseDeContrato != enumClaseDeContrato.Compra)
                            continue;

                        if (!contrato.SobrepasaPorcentajeDeNotificacion(contexto, saldos))
                            continue;

                        GestorDeCorreos.CrearCorreoPara(contexto,
                            new List<string> { contexto.SeleccionarPorId<UsuarioDtm>(contrato.IdResponsable.Entero()).eMail },
                            "Se ha sobrepasado el porcentaje de aviso",
                            $"El contrato {contrato.Expresion} ha sobrepasado el porcentaje de aviso de importe ejecutado",
                            new List<TipoDtoElmento> { new TipoDtoElmento { TipoDto = typeof(ContratoDto).FullName, IdElemento = contrato.Id, Referencia = contrato.Expresion } },
                            new List<string>());

                        saldos.Notificado = true;
                        saldos.Modificar(contexto);
                    }
                    contexto.Commit(tran);
                }
                catch(Exception e)
                {
                    entorno.AnotarError($"Error al notificar pordentajes de aviso de contratos", e);
                    contexto.Rollback(tran);
                }
            }
            finally
            {
                entorno.CrearTraza($"Fin del proceso realizado");
                contexto.CerrarTraza();
            }
        }


        public static TrabajoDeUsuarioDtm SometerMotorDeContratos(ContextoSe contexto)
        {
            var dll = Assembly.GetExecutingAssembly().GetName().Name;
            var clase = typeof(TrabajosDeContratos).FullName;
            var ts = GestorDeTrabajosSometido.CrearObtener(contexto, enumTrabajosDeContratos.MotorDeContratos.Descripcion(), dll,clase, nameof(enumTrabajosDeContratos.MotorDeContratos), comunicarFin: false);
            var datosDeCreacion = new Dictionary<string, object>
            {
                { nameof(TrabajoDeUsuarioDtm.Planificado), DateTime.Now.AddDays(1) },
                { nameof(TrabajoDeUsuarioDtm.Periodicidad), 86400}
            };
            return GestorDeTrabajosDeUsuario.CrearSiNoEstaPendiente(contexto, ts, datosDeCreacion);
        }


        public static void MotorDeContratos(EntornoDeTrabajo entorno)
        {
            var contexto = entorno.contextoDelProceso;

            contexto.IniciarTraza(nameof(MotorDeContratos));
            try
            {
                entorno.CrearTraza("Inicio del proceso");
                var etapaDeElaboracion = VariablesDeContratos.etapaDeElaboracion.ToLista<int>(Simbolos.Coma);
                var contratosEnElaboracion = contexto.Set<ContratoDtm>().Where(x => etapaDeElaboracion.Contains(x.IdEstado) && x.ClaseDeContrato != enumClaseDeContrato.MatriculaDeGuarderia);
                var iniciados = AplicarTransicionesALosContratosSeleccionados(entorno, contratosEnElaboracion, VariablesDeContratos.enumMotivoTransicion.Iniciar, (y) => HayQueIniciarlo(contexto, y));

                var etapaDeVigencia = VariablesDeContratos.etapaVigente.ToLista<int>(Simbolos.Coma);
                var contratosVigentes = contexto.Set<ContratoDtm>().Where(x => etapaDeVigencia.Contains(x.IdEstado) && x.ClaseDeContrato != enumClaseDeContrato.MatriculaDeGuarderia);
                var finalizados = AplicarTransicionesALosContratosSeleccionados(entorno, contratosVigentes, VariablesDeContratos.enumMotivoTransicion.Finalizar, (y) => HayQueFinalizarlo(contexto, y));

                contratosVigentes = contexto.Set<ContratoDtm>().Where(x => etapaDeVigencia.Contains(x.IdEstado) && x.ClaseDeContrato != enumClaseDeContrato.MatriculaDeGuarderia);
                var pdtsDeProrrogar = AplicarTransicionesALosContratosSeleccionados(entorno, contratosVigentes, VariablesDeContratos.enumMotivoTransicion.PdtProrroga, (y) => HayQuePasarloAPdtProrrogar(contexto, y));

                var etapaDePdtProrrogar = VariablesDeContratos.etapaPdtProrroga.ToLista<int>(Simbolos.Coma);
                var contratosPdtDeProrrogar = contexto.Set<ContratoDtm>().Where(x => etapaDePdtProrrogar.Contains(x.IdEstado) && x.ClaseDeContrato != enumClaseDeContrato.MatriculaDeGuarderia);
                var prorrogados = AplicarTransicionesALosContratosSeleccionados(entorno, contratosPdtDeProrrogar, VariablesDeContratos.enumMotivoTransicion.Prorrogar, (y) => HayQueProrrogarlo(contexto, y));


                var responsables = new List<UsuarioDtm>();
                ListaDeResponsables(contexto, iniciados.transitados, responsables);
                ListaDeResponsables(contexto, finalizados.transitados, responsables);
                ListaDeResponsables(contexto, pdtsDeProrrogar.transitados, responsables);
                ListaDeResponsables(contexto, prorrogados.transitados, responsables);

                foreach (var responsable in responsables)
                {
                    GestorDeCorreos.CrearCorreoPara(contexto,
                        new List<string> { responsable.eMail },
                        "Contratos transitados por el sistema",
                        "Los contratos adjuntos han sido modificados por el sistema, repáselos.",
                        contratosAdjuntos(responsable, iniciados.transitados, finalizados.transitados, pdtsDeProrrogar.transitados, prorrogados.transitados),
                        archivos: null);
                }

                responsables.Clear();
                ListaDeResponsables(contexto, iniciados.noTransitados, responsables);
                ListaDeResponsables(contexto, finalizados.noTransitados, responsables);
                ListaDeResponsables(contexto, pdtsDeProrrogar.noTransitados, responsables);
                ListaDeResponsables(contexto, prorrogados.noTransitados, responsables);

                foreach (var responsable in responsables)
                {
                    GestorDeCorreos.CrearCorreoPara(contexto,
                        new List<string> { responsable.eMail },
                        "Contratos no transitados por tener algún problema",
                        "Los contratos adjuntos no se han podido transitar por el sistema, repáselos.",
                        contratosAdjuntos(responsable, iniciados.noTransitados, finalizados.noTransitados, pdtsDeProrrogar.noTransitados, prorrogados.noTransitados),
                        archivos: null);
                }

                var etapaDeFin = VariablesDeContratos.etapaFinalizacion.ToLista<int>(Simbolos.Coma);
                var contratosFinalizados = contexto.Set<ContratoDtm>().Where(x => etapaDeFin.Contains(x.IdEstado) && x.ClaseDeContrato != enumClaseDeContrato.MatriculaDeGuarderia).ToList();

                foreach (var contrato in contratosFinalizados)
                {
                    var aval = contrato.Ampliacion<AvalSolicitadoDtm>(contexto);
                    var datos = contrato.Ampliacion<DatosDelContratoDtm>(contexto);
                    if (!aval.AvisoEnviado.Cierto() &&
                        aval.MesesDeAval.Entero() > 0 &&
                        datos.FinContrato != null &&
                        DateTime.Now.Date > datos.FinContrato.Fecha().AddMonths((int)aval.MesesDeAval) &&
                        contrato.IdResponsable.Entero() > 0)
                        GestorDeCorreos.CrearCorreoPara(contexto,
                            new List<string> { contexto.SeleccionarPorId<UsuarioDtm>((int)contrato.IdResponsable).eMail },
                            "Pendiente la solicitud de aval",
                            $"Se ha de solicitar la solicitud del aval {contrato.Expresion} del contrato.",
                            new List<TipoDtoElmento> { new TipoDtoElmento {
                        TipoDto = typeof(ContratoDto).FullName,
                        IdElemento = contrato.Id,
                        Referencia = contrato.Expresion} },
                            archivos: null);
                }

                entorno.CrearTraza($"Fin del proceso realizado");
            }
            finally
            {
                contexto.CerrarTraza();
                SometerMotorDePlanificador(entorno.contextoDelProceso);
            }
        }

        private static List<TipoDtoElmento> contratosAdjuntos(UsuarioDtm responsable, List<ContratoDtm> iniciados, List<ContratoDtm> finalizados, List<ContratoDtm> pdtsDeProrrogar, List<ContratoDtm> prorrogados)
        {
            var adjuntos = new List<TipoDtoElmento>();
            adjuntarContratos(responsable, iniciados, adjuntos);
            adjuntarContratos(responsable, finalizados, adjuntos);
            adjuntarContratos(responsable, pdtsDeProrrogar, adjuntos);
            adjuntarContratos(responsable, prorrogados, adjuntos);
            return adjuntos;
        }
        private static void adjuntarContratos(UsuarioDtm responsable, List<ContratoDtm> transitados, List<TipoDtoElmento> adjuntos)
        {
            foreach (var transitado in transitados)
            {
                if (transitado.IdResponsable.Entero() == responsable.Id)
                    adjuntos.Add(new TipoDtoElmento
                    {
                        TipoDto = typeof(ContratoDto).FullName,
                        IdElemento = transitado.Id,
                        Referencia = transitado.Expresion
                    });
            }
        }

        private static void ListaDeResponsables(ContextoSe contexto, List<ContratoDtm> transitados, List<UsuarioDtm> responsables)
        {
            foreach (var contrato in transitados)
            {
                if (contrato.IdResponsable.Equals(default))
                    continue;

                var responsable = contexto.SeleccionarPorId<UsuarioDtm>((int)contrato.IdResponsable);
                if (!responsables.Contains(responsable)) responsables.Add(responsable);
            }
        }

        private static (List<ContratoDtm> transitados, List<ContratoDtm> noTransitados) AplicarTransicionesALosContratosSeleccionados(
            EntornoDeTrabajo entorno,
            IQueryable<ContratoDtm> contratos,
            VariablesDeContratos.enumMotivoTransicion motivo,
            Func<ContratoDtm, Dictionary<string, object>> Condicion)
        {
            var transitados = new List<ContratoDtm>();
            var notransitados = new List<ContratoDtm>();
            var contexto = entorno.contextoDelProceso;
            //var transicionesAplicables = JsonConvert.DeserializeObject<List<TransicionAplicable>>(valorDevariable);

            foreach (var contrato in contratos.QueryInChunksOf(10))
            {
                var parametros = Condicion(contrato);
                if (parametros != null)
                {
                    var tran = contexto.IniciarTransaccion();
                    try
                    {
                        contrato.TransitarPorMotivo(contexto, VariablesDeContratos.TransicionesPorMotivo, motivo);
                        contexto.Commit(tran);
                        GestorDeTrazasDeUnTrabajo.AnotarTraza(entorno.ContextoDelEntorno, entorno.TrabajoDeUsuario,
                            $"El contrato {contrato.Referencia} se ha transitado correctamente con el motivo '{motivo}'");
                        transitados.Add(contrato);

                    }
                    catch (Exception exc)
                    {
                        contexto.Rollback(tran);
                        entorno.ContextoDelEntorno.AnotarExcepcion(exc);
                    }
                }
            }
            return (transitados, notransitados);
        }

        private static Dictionary<string, object> HayQueProrrogarlo(ContextoSe contexto, ContratoDtm contrato)
        {
            var prorroga = contrato.Ampliacion<ProrrogaDtm>(contexto);
            return prorroga.ClaseDeProrroga == enumClaseDeProrroga.tacita
            ? new Dictionary<string, object>
              {
                  {nameof(ltrParametrosEp.asunto), "Prorrogado por el sistema" },
                  {nameof(ltrParametrosEp.detalleAsunto), "El sistema ha prorrogado tácitamente el contrato" }
              }
            : null;
        }

        private static Dictionary<string, object> HayQuePasarloAPdtProrrogar(ContextoSe contexto, ContratoDtm contrato)
        {
            var prorroga = contrato.Ampliacion<ProrrogaDtm>(contexto);
            var datos = contrato.Ampliacion<DatosDelContratoDtm>(contexto);
            return prorroga.ClaseDeProrroga != enumClaseDeProrroga.noProrrogable && datos.FinContrato != default && ((DateTime)datos.FinContrato) <= DateTime.Now.Date
            ? new Dictionary<string, object>
              {
                  {nameof(ltrParametrosEp.asunto), "Enviado a Pdt. de prorrogar por el sistema" },
                  {nameof(ltrParametrosEp.detalleAsunto), "El sistema ha modificado el estado del contrato para que se decida si se prorroga" }
              }
            : null;
        }

        private static Dictionary<string, object> HayQueFinalizarlo(ContextoSe contexto, ContratoDtm contrato)
        {
            var prorroga = contrato.Ampliacion<ProrrogaDtm>(contexto);
            var datos = contrato.Ampliacion<DatosDelContratoDtm>(contexto);
            return prorroga.ClaseDeProrroga == enumClaseDeProrroga.noProrrogable && datos.FinContrato != default && ((DateTime)datos.FinContrato) <= DateTime.Now.Date
            ? new Dictionary<string, object>
              {
                  {nameof(ltrParametrosEp.asunto), "Finalizado el contrato por el sistema" },
                  {nameof(ltrParametrosEp.detalleAsunto), "El sistema ha finalizado el contrato por no ser prorrogable" }
              }
            : null;
        }

        private static Dictionary<string, object> HayQueIniciarlo(ContextoSe contexto, ContratoDtm contrato)
        {
            var etapaDeElaboracion = VariablesDeContratos.etapaDeElaboracion.ToLista<int>(Simbolos.Coma);
            var datos = contrato.Ampliacion<DatosDelContratoDtm>(contexto);
            return datos.InicioContrato <= DateTime.Now.Date && etapaDeElaboracion.Contains(contrato.IdEstado)
            ? new Dictionary<string, object>
              {
                  {nameof(ltrParametrosEp.asunto), "Iniciado por el sistema" },
                  {nameof(ltrParametrosEp.detalleAsunto), "El sistema ha transitado el contrato por tener una fecha inicio mayor o igual que la del día" }
              }
            : null;
        }


        public static TrabajoDeUsuarioDtm SometerGenerarPlanificadoresDeUnContrato(ContextoSe contexto, int idContrato)
        {
            var dll = Assembly.GetExecutingAssembly().GetName().Name;
            var clase = typeof(TrabajosDeContratos).FullName;
            var ts = GestorDeTrabajosSometido.CrearObtener(contexto, enumTrabajosDeContratos.GenerarPlanificadoresDeUnContrato.Descripcion(), dll, clase, nameof(enumTrabajosDeContratos.GenerarPlanificadoresDeUnContrato), comunicarFin: false);
            var datosDeCreacion = new Dictionary<string, object>
            {
                { nameof(TrabajoDeUsuarioDtm.Planificado), DateTime.Now.AddMinutes(1) },
                { nameof(TrabajoDeUsuarioDtm.Parametros), new  List<Parametro> {new Parametro(nameof(IRegistro.Id), idContrato) }.ToJson() }
            };
            return GestorDeTrabajosDeUsuario.Crear(contexto, ts, datosDeCreacion);
        }

        public static void GenerarPlanificadoresDeUnContrato(EntornoDeTrabajo entorno)
        {
            var contexto = entorno.contextoDelProceso;
            Dictionary<string, object> parametros = entorno.TrabajoDeUsuario.Parametros.ToDiccionarioDeParametros();
            var idContrato = parametros.LeerValor<long>(nameof(IRegistro.Id));
            var contrato = contexto.SeleccionarPorId<ContratoDtm>((int)idContrato);
            var otorgado = entorno.Ejecutor.OtorgarAdministrador(entorno.contextoDelProceso);
            contexto.IniciarTraza(nameof(GenerarPlanificadoresDeUnContrato));
            var tran = contexto.IniciarTransaccion();
            try
            {
                GenerarPlanificaciones(entorno, contrato, trazarLog: true);
                entorno.CrearTraza($"Fin del proceso realizado");
                contexto.Commit(tran);
            }
            catch(Exception e)
            {
                contexto.Rollback(tran);
                entorno.AnotarError(e);
            }
            finally
            {
                if (otorgado) entorno.Ejecutor.AnularAdministrador(contexto, otorgado);
                contexto.CerrarTraza();
            }
        }

        public static TrabajoDeUsuarioDtm SometerMotorDePlanificador(ContextoSe contexto)
        {
            var dll = Assembly.GetExecutingAssembly().GetName().Name;
            var clase = typeof(TrabajosDeContratos).FullName;
            var ts = GestorDeTrabajosSometido.CrearObtener(contexto, enumTrabajosDeContratos.MotorDePlanificador.Descripcion(), dll, clase, nameof(enumTrabajosDeContratos.MotorDePlanificador), comunicarFin: false);
            var datosDeCreacion = new Dictionary<string, object> { { nameof(TrabajoDeUsuarioDtm.Planificado), DateTime.Now.AddMinutes(1) } };
            return GestorDeTrabajosDeUsuario.Crear(contexto, ts, datosDeCreacion);
        }

        public static void MotorDePlanificador(EntornoDeTrabajo entorno)
        {
            var contexto = entorno.contextoDelProceso;

            var etapaDeVigencia = VariablesDeContratos.etapaVigente.ToLista<int>(Simbolos.Coma);
            var contratosVigentes = contexto.Set<ContratoDtm>()
                .Where(x => etapaDeVigencia.Contains(x.IdEstado) &&
                (x.ClaseDeContrato == enumClaseDeContrato.Venta || x.ClaseDeContrato == enumClaseDeContrato.MatriculaDeGuarderia)).ToList();

            contexto.IniciarTraza(nameof(MotorDePlanificador));
            try
            {
                foreach (var contrato in contratosVigentes)
                {
                    var tran = contexto.IniciarTransaccion();
                    try
                    {
                        GenerarPlanificaciones(entorno, contrato, trazarLog: false);
                        contexto.Commit(tran);
                    }
                    catch(Exception exc)
                    {
                        contexto.Rollback(tran, exc);
                        entorno.AnotarError(exc, mostrarPila: true);
                    }
                }
            }
            finally
            {
                entorno.CrearTraza($"Fin del proceso realizado");
                contexto.CerrarTraza();
            }
        }


        private static void GenerarPlanificaciones(EntornoDeTrabajo entorno, ContratoDtm contrato, bool trazarLog)
        {
            var contexto = entorno.contextoDelProceso;
            var planificadores = contexto.SeleccionarTodos<PlanificadorDeVentaDtm>(nameof(PlanificadorDeVentaDtm.IdContrato), contrato.Id);
            foreach (var planificador in planificadores)
            {
                if (planificador.Generado == true)
                {
                    if (trazarLog) entorno.CrearTraza($"El planificador '{planificador.Nombre}' {contrato.DeLaClaseDeContrato} '{contrato.Referencia}' ya estaba generado");
                    continue;
                }
                try
                {
                    planificador.GenerarPlanificaciones(contexto);
                    entorno.CrearTraza($"El planificador '{planificador.Nombre}' {contrato.DeLaClaseDeContrato} '{contrato.Referencia}' ha sido generado");
                }
                catch (Exception ex)
                {
                    entorno.AnotarError(ex);
                }
            }
        }


        public static TrabajoDeUsuarioDtm SometerActivarMatriculasDeGuarderia(ContextoSe contexto)
        {
            var dll = Assembly.GetExecutingAssembly().GetName().Name;
            var clase = typeof(TrabajosDeContratos).FullName;
            var ts = GestorDeTrabajosSometido.CrearObtener(contexto, enumTrabajosDeContratos.ActivarMatriculasDeGuarderia.Descripcion(), dll, clase, nameof(enumTrabajosDeContratos.ActivarMatriculasDeGuarderia), comunicarFin: false);
            var datosDeCreacion = new Dictionary<string, object>
            {
                { nameof(TrabajoDeUsuarioDtm.Planificado), DateTime.Now.AddMinutes(1) },
                { nameof(TrabajoDeUsuarioDtm.Periodicidad), 86400}
            };
            return GestorDeTrabajosDeUsuario.CrearSiNoEstaPendiente(contexto, ts, datosDeCreacion);
        }

        public static void ActivarMatriculasDeGuarderia_old(EntornoDeTrabajo entorno)
        {
            var contexto = entorno.contextoDelProceso;

            var etapaDeVigencia = VariablesDeContratos.etapaDeElaboracion.ToLista<int>(Simbolos.Coma);
            var matriculasPendientes = contexto.Set<ContratoDtm>().Where(x => etapaDeVigencia.Contains(x.IdEstado) && (x.ClaseDeContrato == enumClaseDeContrato.MatriculaDeGuarderia));

            contexto.IniciarTraza(nameof(ActivarMatriculasDeGuarderia));
            try
            {
                foreach (var matricula in matriculasPendientes)
                {
                    if (matricula.FechasDelContrato(contexto).Inicio >= DateTime.Now.Date)
                        continue;

                    var tran = contexto.IniciarTransaccion();
                    try
                    {
                        matricula.TransitarALaEtapa(contexto, enumEtapasDeContratos.CTR_Etapa_Vigente.EstadosDeLaEtapa(), new Dictionary<string, object>());
                        contexto.Commit(tran);
                    }
                    catch(Exception ex) 
                    {
                        contexto.Rollback(tran);
                        entorno.AnotarError(ex);
                    }
                }
            }
            finally
            {
                entorno.CrearTraza($"Fin del proceso realizado");
                contexto.CerrarTraza();
            }
        }

        public static void ActivarMatriculasDeGuarderia(EntornoDeTrabajo entorno)
        {
            var contexto = entorno.contextoDelProceso;

            var etapaDeVigencia = VariablesDeContratos.etapaDeElaboracion.ToLista<int>(Simbolos.Coma);
            var matriculasPendientes = contexto.Set<ContratoDtm>().Where(x => etapaDeVigencia.Contains(x.IdEstado) && (x.ClaseDeContrato == enumClaseDeContrato.MatriculaDeGuarderia)).ToList();

            contexto.IniciarTraza(nameof(ActivarMatriculasDeGuarderia));
            try
            {
                foreach (var matricula in matriculasPendientes)
                {
                    entorno.LanzarProceso(() =>
                    {
                        if (matricula.FechasDelContrato(contexto).Inicio >= DateTime.Now.Date)
                            return;

                        var tran = contexto.IniciarTransaccion();
                        try
                        {
                            matricula.TransitarALaEtapa(contexto, enumEtapasDeContratos.CTR_Etapa_Vigente.EstadosDeLaEtapa(), new Dictionary<string, object>());
                            contexto.Commit(tran);
                        }
                        catch (Exception ex)
                        {
                            contexto.Rollback(tran);
                            entorno.AnotarError(ex);
                            throw; // Re-lanzar la excepción para que RetryOperation la maneje
                        }
                    }, maxAttempts: 3, delaySeconds: 60);
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

