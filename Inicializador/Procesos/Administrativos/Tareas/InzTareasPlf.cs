using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Negocio;
using GestoresDeNegocio.SistemaDocumental;
using GestoresDeNegocio.Tarea;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Tarea;
using Utilidades;

namespace Inicializador.Procesos
{
    public static class InzTareasPlf
    {
        private static readonly string n_plf = "PLF";

        public static void ModeloDeTareasPlf(ContextoSe contexto)
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

        public static readonly string n_estado_plf_pendiente = $"{n_plf}: Pendiente";
        public static readonly string n_estado_plf_cancelada = $"{n_plf}: Cancelada";
        public static readonly string n_estado_plf_asignada = $"{n_plf}: Asignada";
        public static readonly string n_estado_plf_iniciada = $"{n_plf}: Iniciada";
        public static readonly string n_estado_plf_parada = $"{n_plf}: Parada";
        public static readonly string n_estado_plf_finalizada = $"{n_plf}: Resuelta";
        public static readonly string n_estado_plf_terminada = $"{n_plf}: Terminada";
        private static void Estados(ContextoSe contexto)
        {
            contexto.IniciarTraza("Estados de las tareas de PLF");
            try
            {
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Tarea, n_estado_plf_pendiente, inicial: true, terminado: false, cancelado: false, 10);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Tarea, n_estado_plf_asignada, inicial: false,terminado: false, cancelado: false, 15);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Tarea, n_estado_plf_iniciada, inicial: false,terminado: false, cancelado: false, 20);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Tarea, n_estado_plf_parada, inicial: false, terminado: false, cancelado: false, 25);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Tarea, n_estado_plf_finalizada, inicial: false,terminado: false, cancelado: false,30);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Tarea, n_estado_plf_terminada, inicial: false,terminado: true, cancelado: false, 40);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Tarea, n_estado_plf_cancelada, inicial: false,terminado: false, cancelado: true,50);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        public static readonly string n_tran_plf_asignar = $"{n_plf}: Asignar";
        public static readonly string n_tran_plf_iniciar = $"{n_plf}: Iniciar";
        public static readonly string n_tran_plf_finalizar = $"{n_plf}: Finalizar";
        public static readonly string n_tran_plf_terminar = $"{n_plf}: Terminar";
        public static readonly string n_tran_plf_cancelar = $"{n_plf}: Cancelar";

        public static readonly string n_tran_plf_desasignar = $"{n_plf}: Desasignar";
        public static readonly string n_tran_plf_devolver_asignada = $"{n_plf}: Devolver a asignada";
        public static readonly string n_tran_plf_devolver_iniciada = $"{n_plf}: Devolver a iniciada";

        public static readonly string n_tran_plf_parar = $"{n_plf}: Parar";
        public static readonly string n_tran_plf_reiniciar = $"{n_plf}: Reiniciar";


        private static void Transiciones(ContextoSe contexto)
        {
            contexto.IniciarTraza("Transiciones de las tareas de PLF");
            try
            {
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Tarea, n_tran_plf_asignar, n_estado_plf_pendiente, n_estado_plf_asignada);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Tarea, n_tran_plf_iniciar, n_estado_plf_asignada, n_estado_plf_iniciada);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Tarea, n_tran_plf_finalizar, n_estado_plf_iniciada, n_estado_plf_finalizada);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Tarea, n_tran_plf_terminar, n_estado_plf_finalizada, n_estado_plf_terminada);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Tarea, n_tran_plf_cancelar, n_estado_plf_pendiente, n_estado_plf_cancelada, asunto: "Motivo de cancelación");

                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Tarea, n_tran_plf_desasignar, n_estado_plf_asignada, n_estado_plf_pendiente, asunto: "Motivo de desasignar");
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Tarea, n_tran_plf_devolver_asignada, n_estado_plf_iniciada, n_estado_plf_asignada, asunto: "Motivo de devolución a asignada");
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Tarea, n_tran_plf_devolver_iniciada, n_estado_plf_finalizada, n_estado_plf_iniciada, asunto: "Motivo de devolución a iniciada");

                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Tarea, n_tran_plf_parar, n_estado_plf_iniciada, n_estado_plf_parada, asunto: "Motivo de parar la tarea");
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Tarea, n_tran_plf_reiniciar, n_estado_plf_parada, n_estado_plf_iniciada);

            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        public static readonly string n_tipo_plf = $"{n_plf}: Tarea planificada";
        private static void Tipos(ContextoSe contexto)
        {
            contexto.IniciarTraza("Tipos de las trareas de PLF");
            try
            {
                var tipoAri = GestorDeTiposDeArchivadores.PersistirTipo(contexto, $"{n_plf}: Interno", enumClaseDeLibro.POR_CG_TIPO, n_plf, visible: false, delSistema: false);

                var estadoInicial = enumNegocio.Tarea.Estado(contexto, n_estado_plf_pendiente);
                GestorDeTiposDeTarea.PersistirTipo(contexto, enumClaseDeTarea.Control, n_tipo_plf, estadoInicial.Id, enumClaseDeLibro.POR_CG_TIPO, n_plf
                    , tipoAri
                    , usaPlanificacion: true);
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

            var asignado = contexto.SeleccionarEstado<EstadoDeUnaTareaDtm>(n_estado_plf_asignada).Id;
            enumNegocio.Tarea.IncluirEnParametro(contexto, enumEtapasDeTareas.TAR_Etapa_Asignada , asignado.ToString());

            var enresolucion = $"{contexto.SeleccionarEstado<EstadoDeUnaTareaDtm>(n_estado_plf_iniciada).Id}";
            enumNegocio.Tarea.IncluirEnParametro(contexto, enumEtapasDeTareas.TAR_Etapa_En_Resolucion, enresolucion);

            var enespera = contexto.SeleccionarEstado<EstadoDeUnaTareaDtm>(n_estado_plf_parada).Id;
            enumNegocio.Tarea.IncluirEnParametro(contexto, enumEtapasDeTareas.TAR_Etapa_En_Espera, enespera.ToString());

            var envalidacion = contexto.SeleccionarEstado<EstadoDeUnaTareaDtm>(n_estado_plf_finalizada).Id;
            enumNegocio.Tarea.IncluirEnParametro(contexto, enumEtapasDeTareas.TAR_Etapa_Validacion, envalidacion.ToString());
        }


    }
}
