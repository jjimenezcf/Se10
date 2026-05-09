using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Negocio;
using GestoresDeNegocio.Tarea;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Tarea;
using Utilidades;

namespace Inicializador.Procesos
{
    public static class InzTareasEvo
    {
        private static readonly string n_ssii = "SSII";
        private static readonly string n_evo = "EVO";


        public static void ModeloDeTareasEvo(ContextoSe contexto)
        {
            var tran = contexto.IniciarTransaccion();
            try
            {
                Estados(contexto);
                Transiciones(contexto);
                Tipos(contexto);
                DefinirEtapas(contexto);
                contexto.Commit(tran);
            }
            catch
            {
                contexto.Rollback(tran);
                throw;
            }
        }

        public static readonly string n_estado_ssii_sin_flujo = $"{n_ssii}: Sin Flujo";

        public static readonly string n_estado_evo_pendiente = $"{n_evo}: Inicial";
        public static readonly string n_estado_evo_cancelada = $"{n_evo}: Cancelada";
        public static readonly string n_estado_evo_asignada = $"{n_evo}: Asignada";
        public static readonly string n_estado_evo_iniciada = $"{n_evo}: En Resolución";
        public static readonly string n_estado_evo_parada = $"{n_evo}: Parada";
        public static readonly string n_estado_evo_finalizada = $"{n_evo}: En validación";
        public static readonly string n_estado_evo_terminada = $"{n_evo}: Terminada";
        private static void Estados(ContextoSe contexto)
        {
            contexto.IniciarTraza("Estados de las tareas de EVO");
            try
            {
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Tarea, n_estado_ssii_sin_flujo, inicial: true, terminado: true, cancelado: false, 10);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Tarea, n_estado_evo_pendiente, inicial: true, orden: 10);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Tarea, n_estado_evo_asignada, orden: 15);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Tarea, n_estado_evo_iniciada, orden: 20);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Tarea, n_estado_evo_parada, orden: 25);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Tarea, n_estado_evo_finalizada, orden: 30);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Tarea, n_estado_evo_terminada, terminado: true, orden: 40);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Tarea, n_estado_evo_cancelada, cancelado: true, orden: 50);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        public static readonly string n_tran_evo_asignar = $"{n_evo}: Asignar";
        public static readonly string n_tran_evo_iniciar = $"{n_evo}: Iniciar";
        public static readonly string n_tran_evo_finalizar = $"{n_evo}: Finalizar";
        public static readonly string n_tran_evo_terminar = $"{n_evo}: Terminar";
        public static readonly string n_tran_evo_cancelar = $"{n_evo}: Cancelar";

        public static readonly string n_tran_evo_desasignar = $"{n_evo}: Desasignar";
        public static readonly string n_tran_evo_devolver_asignada = $"{n_evo}: Devolver a asignada";
        public static readonly string n_tran_evo_devolver_iniciada = $"{n_evo}: Devolver a iniciada";

        public static readonly string n_tran_evo_parar = $"{n_evo}: Parar";
        public static readonly string n_tran_evo_reiniciar = $"{n_evo}: Reiniciar";


        private static void Transiciones(ContextoSe contexto)
        {
            contexto.IniciarTraza("Transiciones de las tareas de EVO");
            try
            {
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Tarea, n_tran_evo_asignar, n_estado_evo_pendiente, n_estado_evo_asignada);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Tarea, n_tran_evo_iniciar, n_estado_evo_asignada, n_estado_evo_iniciada);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Tarea, n_tran_evo_finalizar, n_estado_evo_iniciada, n_estado_evo_finalizada);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Tarea, n_tran_evo_terminar, n_estado_evo_finalizada, n_estado_evo_terminada);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Tarea, n_tran_evo_cancelar, n_estado_evo_pendiente, n_estado_evo_cancelada, asunto: "Motivo de cancelación");

                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Tarea, n_tran_evo_desasignar, n_estado_evo_asignada, n_estado_evo_pendiente, asunto: "Motivo de desasignar");
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Tarea, n_tran_evo_devolver_asignada, n_estado_evo_iniciada, n_estado_evo_asignada, asunto: "Motivo de devolución a asignada");
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Tarea, n_tran_evo_devolver_iniciada, n_estado_evo_finalizada, n_estado_evo_iniciada, asunto: "Motivo de devolución a iniciada");

                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Tarea, n_tran_evo_parar, n_estado_evo_iniciada, n_estado_evo_parada, asunto: "Motivo de parar la tarea");
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Tarea, n_tran_evo_reiniciar, n_estado_evo_parada, n_estado_evo_iniciada);

            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        public static readonly string n_tipo_ssii = $"{n_ssii}: Estatum";
        public static readonly string n_tipo_Evo = $"{n_ssii}: Evolutivo";
        private static void Tipos(ContextoSe contexto)
        {
            contexto.IniciarTraza("Tipos de las trareas de EVO");
            try
            {
              var padre = GestorDeTiposDeTarea.PersistirTipo(contexto, enumClaseDeTarea.Control, n_tipo_ssii, enumNegocio.Tarea.Estado(contexto, n_estado_ssii_sin_flujo).Id,
                  clsLibro: enumClaseDeLibro.POR_CG_TIPO,
                  sigla: n_ssii,
                  tipoAri: null,
                  usaPlanificacion: true);

                GestorDeTiposDeTarea.PersistirTipo(contexto, enumClaseDeTarea.Control, n_tipo_Evo, enumNegocio.Tarea.Estado(contexto, n_estado_evo_pendiente).Id, 
                    enumClaseDeLibro.POR_CG_TIPO, 
                    n_evo,
                    tipoAri: null,
                    usaPlanificacion: true,
                    padre);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }


        private static void DefinirEtapas(ContextoSe contexto)
        {
            var estados = contexto.Estados<EstadoDeUnaTareaDtm>(nameof(EstadoDtm.Inicial), true);
            var iniciales = "";
            foreach (EstadoDtm estado in estados)
                iniciales = $"{(iniciales.IsNullOrEmpty() ? estado.Id.ToString() : $"{iniciales},{estado.Id}")}";
            enumNegocio.Tarea.ResetearParametro(contexto, enumEtapasDeTareas.TAR_Etapa_Inicial, iniciales);

            var estadosCancelados = "";
            var cancelados = contexto.Estados<EstadoDeUnaTareaDtm>(nameof(EstadoDtm.Cancelado), true);
            foreach (EstadoDtm cancelado in cancelados)
                estadosCancelados = $"{(estadosCancelados.IsNullOrEmpty() ? cancelado.Id.ToString() : $"{estadosCancelados},{cancelado.Id}")}";
            enumNegocio.Tarea.ResetearParametro(contexto, enumEtapasDeTareas.TAR_Etapa_Cancelado, estadosCancelados);

            var estadosTerminados = "";
            var terminados = contexto.Estados<EstadoDeUnaTareaDtm>(nameof(EstadoDtm.Terminado), true);
            foreach (EstadoDtm terminado in terminados)
                estadosTerminados = $"{(estadosTerminados.IsNullOrEmpty() ? terminado.Id.ToString() : $"{estadosTerminados},{terminado.Id}")}";
            enumNegocio.Tarea.ResetearParametro(contexto, enumEtapasDeTareas.TAR_Etapa_Terminada, estadosTerminados);

            var asignado = contexto.SeleccionarEstado<EstadoDeUnaTareaDtm>(n_estado_evo_asignada).Id;
            enumNegocio.Tarea.ResetearParametro(contexto, enumEtapasDeTareas.TAR_Etapa_Asignada , asignado.ToString());

            var enresolucion = $"{contexto.SeleccionarEstado<EstadoDeUnaTareaDtm>(n_estado_evo_iniciada).Id}";
            enumNegocio.Tarea.ResetearParametro(contexto, enumEtapasDeTareas.TAR_Etapa_En_Resolucion, enresolucion);

            var enespera = contexto.SeleccionarEstado<EstadoDeUnaTareaDtm>(n_estado_evo_parada).Id;
            enumNegocio.Tarea.ResetearParametro(contexto, enumEtapasDeTareas.TAR_Etapa_En_Espera, enespera.ToString());

            var envalidacion = contexto.SeleccionarEstado<EstadoDeUnaTareaDtm>(n_estado_evo_finalizada).Id;
            enumNegocio.Tarea.ResetearParametro(contexto, enumEtapasDeTareas.TAR_Etapa_Validacion, envalidacion.ToString());
        }


    }
}
