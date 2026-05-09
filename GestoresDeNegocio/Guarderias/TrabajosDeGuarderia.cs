using GestoresDeNegocio.TrabajosSometidos;
using ServicioDeDatos.TrabajosSometidos;
using ServicioDeDatos;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using GestorDeElementos;
using ServicioDeDatos.Guarderias;
using GestorDeElementos.Extensores;
using Utilidades;

namespace GestoresDeNegocio.Guarderias
{
    public enum enumTrabajosDeGuarderias
    {
        [Description("Elimina los permisos de agenda de los puestos de trabajo")]
        AnularPermisosDeAgendas
    }

    public class TrabajosDeGuarderia
    {
        public static TrabajoDeUsuarioDtm SometerAnularPermisosDeAgendas(ContextoSe contexto)
        {
            var dll = Assembly.GetExecutingAssembly().GetName().Name;
            var clase = typeof(TrabajosDeGuarderia).FullName;
            var ts = GestorDeTrabajosSometido.CrearObtener(contexto, enumTrabajosDeGuarderias.AnularPermisosDeAgendas.Descripcion(), dll,clase, nameof(enumTrabajosDeGuarderias.AnularPermisosDeAgendas), comunicarFin: false);
            var datosDeCreacion = new Dictionary<string, object>
            {
                { nameof(TrabajoDeUsuarioDtm.Planificado), DateTime.Now.AddDays(1) },
                { nameof(TrabajoDeUsuarioDtm.Periodicidad), 86400}
            };
            return GestorDeTrabajosDeUsuario.CrearSiNoEstaPendiente(contexto, ts, datosDeCreacion);
        }

        public static void AnularPermisosDeAgendas(EntornoDeTrabajo entorno)
        {
            var contexto = entorno.contextoDelProceso;
            contexto.IniciarTraza(nameof(AnularPermisosDeAgendas));
            try
            {
                if (!ExtensorDeGuarderias.ModuloActivo(contexto))
                {
                    entorno.CrearTraza($"Modulo de guarderia no activo");
                    return;
                }
                var ultimaEjecucion = entorno.TrabajoSometido.UltimaEjecucion(contexto);
                var filtros = new List<ClausulaDeFiltrado>
                {
                    new ClausulaDeFiltrado(nameof(CursoDeGuarderiaDtm.Fin), enumCriteriosDeFiltrado.menor, DateTime.Now.Date)
                };
                if (ultimaEjecucion != null) filtros.Add(new ClausulaDeFiltrado(nameof(CursoDeGuarderiaDtm.Fin), enumCriteriosDeFiltrado.mayorIgual, ultimaEjecucion.Terminado));
                var cursosTerminados = contexto.SeleccionarTodos<CursoDeGuarderiaDtm>(filtros);
                if (cursosTerminados.Count > 0)
                {
                    foreach (var curso in cursosTerminados)
                    {
                        var tran = contexto.IniciarTransaccion();
                        try
                        {
                            var matriculados = curso.Detalles<InfanteDeUnCursoDtm>(contexto, aplicarJoin: true);
                            foreach (var matriculado in matriculados)
                            {
                                matriculado.Infante(contexto).Agenda(contexto).DesasignarPermisoAlPuesto(contexto, curso.IdGestor, ServicioDeDatos.Seguridad.enumModoDeAccesoDeDatos.Consultor);
                                matriculado.Infante(contexto).Agenda(contexto).DesasignarPermisoAlPuesto(contexto, curso.IdConsultor, ServicioDeDatos.Seguridad.enumModoDeAccesoDeDatos.Gestor);
                            }
                            contexto.Commit(tran);
                            entorno.CrearTraza($"Acceso a las agendas del curso '{curso.Expresion}' quitadas");
                        }
                        catch(Exception ex)
                        {
                            entorno.AnotarError($"No se ha podido anular el permiso a la agenda del curso '{curso.Expresion}'", ex);
                            contexto.Rollback(tran);
                        }
                    }
                }
                entorno.CrearTraza($"Fin del proceso realizado");
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }
    }
}
