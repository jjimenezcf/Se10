using GestoresDeNegocio.Negocio;
using ServicioDeDatos;
using ServicioDeDatos.RegistroEs;
using ServicioDeDatos.Tarea;
using Utilidades;
using System.Linq;
using GestorDeElementos.Extensores;
using GestorDeElementos;

namespace GestoresDeNegocio.Tarea
{
    public static class AccionesDeTareas
    {
        public const string N_AlCerraTodasLasTareaDelRegistroEs = "Al cerrar todas las tareas del registro";
        public const string N_AlCancelarTodasLasTareasDelRegistroEs = "Al cancelar todas las tareas del registro";
        public const string N_AlIniciarUnaTareaDelRegistroEs = "Al iniciar una tarea del registro";
        public const string N_ValidarQueUnaTareaEstaCancelada = "Validar que la tarea adjunta está cancelada";


        public static void AlCerraTodasLasTareaDelRegistroEs(EntornoDeUnaAccion entorno)
        {
            var tarea = (TareaDtm)entorno.Registro;
            var transiciones = entorno.Transicciones(enumNegocio.Registro);
            var registrosEs = tarea.Vinculados<RegistroEsDtm>(entorno.Contexto);
            if (registrosEs.Count == 0)
                return;

            foreach (RegistroEsDtm registroEs in registrosEs)
            {
                if (TodasSusTareasEstanCerradas(entorno, registroEs))
                {
                    foreach (var transicion in transiciones.Where(transicion => registroEs.IdEstado == transicion.IdOrigen))
                    {
                        registroEs.Transitar(entorno.Contexto, transicion.Id);
                        break;
                    }
                }
            }
        }

        public static void AlCancelarTodasLasTareasDelRegistroEs(EntornoDeUnaAccion entorno)
        {
            var tarea = (TareaDtm)entorno.Registro;
            var registrosEs = GestorDeVinculos.RegistrosVinculados<RegistroEsDtm>(entorno.Contexto, enumNegocio.Tarea, enumNegocio.Registro, tarea.Id);
            foreach (var registroEs in registrosEs)
            {
                if (TodasSusTareasEstanCanceladas(entorno, registroEs))
                    registroEs.Cancelar(entorno.Contexto);
            }
        }
        public static void AlIniciarUnaTareaDelRegistroEs(EntornoDeUnaAccion entorno)
        {
            var tarea = (TareaDtm)entorno.Registro;
            var transiciones = entorno.Transicciones(enumNegocio.Registro);

            var registrosEs = tarea.Vinculados<RegistroEsDtm>(entorno.Contexto);
            if (registrosEs.Count == 0)
                return;

            foreach (var registroEs in registrosEs)
                foreach (var transicion in transiciones.Where(transicion => registroEs.IdEstado == transicion.IdOrigen))
                {
                    registroEs.Transitar(entorno.Contexto, transicion.Id);
                    break;
                }
        }
        private static bool TodasSusTareasEstanCanceladas(EntornoDeUnaAccion entorno, RegistroEsDtm registroEs)
        {
            var tareas = registroEs.Vinculados<TareaDtm>(entorno.Contexto);
            foreach (var tarea in tareas)
            {
                if (tarea.Id == entorno.Registro.Id) continue;
                var estado = GestorDeEstados.Gestor(entorno.Contexto, enumNegocio.Tarea).LeerRegistroPorId(tarea.IdEstado);
                if (!estado.Cancelado)
                    return false;
            }
            return true;
        }
        private static bool TodasSusTareasEstanCerradas(EntornoDeUnaAccion entorno, RegistroEsDtm registroEs)
        {
            var gestor = GestorDeEstados.Gestor(entorno.Contexto, enumNegocio.Tarea);
            var tareas = registroEs.Vinculados<TareaDtm>(entorno.Contexto);
            foreach (var tarea in tareas)
            {
                if (tarea.Id == entorno.Registro.Id) continue;
                var estado = gestor.LeerRegistroPorId(tarea.IdEstado);
                if (!estado.Terminado && !estado.Cancelado)
                    return false;
            }
            return true;
        }


    }
}
