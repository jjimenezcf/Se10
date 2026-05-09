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
    public static class InzTareasSpc
    {
        private static readonly string n_ssii = "SSII";
        private static readonly string n_spc = "SPC";


        public static void ModeloDeTareasSpc(ContextoSe contexto)
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

        public static readonly string n_estado_spc_pendiente = $"{n_spc}: Inicial";
        public static readonly string n_estado_spc_cancelada = $"{n_spc}: Cancelada";
        public static readonly string n_estado_spc_asignada = $"{n_spc}: Asignada";
        public static readonly string n_estado_spc_iniciada = $"{n_spc}: En Resolución";
        public static readonly string n_estado_spc_parada = $"{n_spc}: Parada";
        public static readonly string n_estado_spc_finalizada = $"{n_spc}: En validación";
        public static readonly string n_estado_spc_terminada = $"{n_spc}: Terminada";
        private static void Estados(ContextoSe contexto)
        {
            contexto.IniciarTraza("Estados de las tareas de SPC");
            try
            {
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Tarea, n_estado_ssii_sin_flujo, inicial: true, terminado: true, cancelado: false, 10);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Tarea, n_estado_spc_pendiente, inicial: true, orden: 10);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Tarea, n_estado_spc_asignada, orden: 15);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Tarea, n_estado_spc_iniciada, orden: 20);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Tarea, n_estado_spc_parada, orden: 25);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Tarea, n_estado_spc_finalizada, orden: 30);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Tarea, n_estado_spc_terminada, terminado: true, orden: 40);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Tarea, n_estado_spc_cancelada, cancelado: true, orden: 50);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        public static readonly string n_tran_spc_asignar = $"{n_spc}: Asignar";
        public static readonly string n_tran_spc_iniciar = $"{n_spc}: Iniciar";
        public static readonly string n_tran_spc_finalizar = $"{n_spc}: Finalizar";
        public static readonly string n_tran_spc_terminar = $"{n_spc}: Terminar";
        public static readonly string n_tran_spc_cancelar = $"{n_spc}: Cancelar";

        public static readonly string n_tran_spc_desasignar = $"{n_spc}: Desasignar";
        public static readonly string n_tran_spc_devolver_asignada = $"{n_spc}: Devolver a asignada";
        public static readonly string n_tran_spc_devolver_iniciada = $"{n_spc}: Devolver a iniciada";

        public static readonly string n_tran_spc_parar = $"{n_spc}: Parar";
        public static readonly string n_tran_spc_reiniciar = $"{n_spc}: Reiniciar";


        private static void Transiciones(ContextoSe contexto)
        {
            contexto.IniciarTraza("Transiciones de las tareas de SPC");
            try
            {
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Tarea, n_tran_spc_asignar, n_estado_spc_pendiente, n_estado_spc_asignada);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Tarea, n_tran_spc_iniciar, n_estado_spc_asignada, n_estado_spc_iniciada);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Tarea, n_tran_spc_finalizar, n_estado_spc_iniciada, n_estado_spc_finalizada);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Tarea, n_tran_spc_terminar, n_estado_spc_finalizada, n_estado_spc_terminada);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Tarea, n_tran_spc_cancelar, n_estado_spc_pendiente, n_estado_spc_cancelada, asunto: "Motivo de cancelación");

                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Tarea, n_tran_spc_desasignar, n_estado_spc_asignada, n_estado_spc_pendiente, asunto: "Motivo de desasignar");
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Tarea, n_tran_spc_devolver_asignada, n_estado_spc_iniciada, n_estado_spc_asignada, asunto: "Motivo de devolución a asignada");
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Tarea, n_tran_spc_devolver_iniciada, n_estado_spc_finalizada, n_estado_spc_iniciada, asunto: "Motivo de devolución a iniciada");

                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Tarea, n_tran_spc_parar, n_estado_spc_iniciada, n_estado_spc_parada, asunto: "Motivo de parar la tarea");
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Tarea, n_tran_spc_reiniciar, n_estado_spc_parada, n_estado_spc_iniciada);

            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        public static readonly string n_tipo_ssii = $"{n_ssii}: Estatum";
        public static readonly string n_tipo_Spc = $"{n_ssii}: Correctivo";
        private static void Tipos(ContextoSe contexto)
        {
            contexto.IniciarTraza("Tipos de las trareas de SPC");
            try
            {
                var padre = GestorDeTiposDeTarea.PersistirTipo(contexto, enumClaseDeTarea.Control, n_tipo_ssii, enumNegocio.Tarea.Estado(contexto, n_estado_ssii_sin_flujo).Id,
                    clsLibro: enumClaseDeLibro.POR_CG_TIPO,
                    sigla: n_ssii,
                    tipoAri: null,
                    usaPlanificacion: true);

                GestorDeTiposDeTarea.PersistirTipo(contexto, enumClaseDeTarea.Control, n_tipo_Spc, enumNegocio.Tarea.Estado(contexto, n_estado_spc_pendiente).Id,
                    enumClaseDeLibro.POR_CG_TIPO,
                    n_spc,
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

            var asignado = contexto.SeleccionarEstado<EstadoDeUnaTareaDtm>(n_estado_spc_asignada).Id;
            enumNegocio.Tarea.ResetearParametro(contexto, enumEtapasDeTareas.TAR_Etapa_Asignada, asignado.ToString());

            var enresolucion = $"{contexto.SeleccionarEstado<EstadoDeUnaTareaDtm>(n_estado_spc_iniciada).Id}";
            enumNegocio.Tarea.ResetearParametro(contexto, enumEtapasDeTareas.TAR_Etapa_En_Resolucion, enresolucion);

            var enespera = contexto.SeleccionarEstado<EstadoDeUnaTareaDtm>(n_estado_spc_parada).Id;
            enumNegocio.Tarea.ResetearParametro(contexto, enumEtapasDeTareas.TAR_Etapa_En_Espera, enespera.ToString());

            var envalidacion = contexto.SeleccionarEstado<EstadoDeUnaTareaDtm>(n_estado_spc_finalizada).Id;
            enumNegocio.Tarea.ResetearParametro(contexto, enumEtapasDeTareas.TAR_Etapa_Validacion, envalidacion.ToString());
        }


    }
}
