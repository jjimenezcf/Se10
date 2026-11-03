using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Negocio;
using GestoresDeNegocio.Tarea;
using GestoresDeNegocio.TrabajosSometidos;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Tarea;
using Utilidades;

namespace Inicializador.Procesos
{
    /// <summary>
    /// Tareas de soporte: tipo 'SSII: Soporte' (sigla SPC), hijo de 'SSII: Estatum', con planificación y facturable.
    /// Comparte la sigla y algunos estados ('SPC: Inicial', 'SPC: Asignada'...) con InzTareasSpc ('SSII: Correctivo'), por lo que
    /// en un mismo entorno se ha de ejecutar una de las dos, no ambas.
    /// </summary>
    public static class InzTareasSoporte
    {
        private static readonly string n_ssii = "SSII";
        private static readonly string n_spc = "SPC";

        /// <param name="nombreDePuestoParaUrgencias">Si se indica, al aplicar 'SPC: Urgente' se envía un correo a ese puesto de trabajo</param>
        public static void ModeloDeTareasSoporte(ContextoSe contexto, string nombreDePuestoParaUrgencias = null)
        {
            var tran = contexto.IniciarTransaccion();
            try
            {
                Estados(contexto);
                Transiciones(contexto);
                Tipos(contexto);
                DefinirEtapas(contexto);
                if (!nombreDePuestoParaUrgencias.IsNullOrEmpty())
                    AccionesAlTransitar(contexto, nombreDePuestoParaUrgencias);
                contexto.Commit(tran);
            }
            catch
            {
                contexto.Rollback(tran);
                throw;
            }
        }

        public static readonly string n_estado_ssii_sin_flujo = $"{n_ssii}: Sin Flujo";

        public static readonly string n_estado_spc_inicial = $"{n_spc}: Inicial";
        public static readonly string n_estado_spc_asignada = $"{n_spc}: Asignada";
        public static readonly string n_estado_spc_en_proceso = $"{n_spc}: En proceso";
        public static readonly string n_estado_spc_parada = $"{n_spc}: Parada";
        public static readonly string n_estado_spc_pdt_validar = $"{n_spc}: Pdt. validar";
        public static readonly string n_estado_spc_terminada = $"{n_spc}: Terminada";
        public static readonly string n_estado_spc_cancelada = $"{n_spc}: Cancelada";
        public static readonly string n_estado_spc_validada = $"{n_spc}: Validada";
        private static void Estados(ContextoSe contexto)
        {
            contexto.IniciarTraza("Estados de las tareas de soporte");
            try
            {
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Tarea, n_estado_ssii_sin_flujo, inicial: true, terminado: true, cancelado: false, 10);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Tarea, n_estado_spc_inicial, inicial: true, orden: 10);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Tarea, n_estado_spc_asignada, orden: 15);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Tarea, n_estado_spc_en_proceso, orden: 20);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Tarea, n_estado_spc_parada, orden: 25);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Tarea, n_estado_spc_pdt_validar, orden: 30);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Tarea, n_estado_spc_terminada, terminado: true, orden: 40);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Tarea, n_estado_spc_cancelada, cancelado: true, orden: 50);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Tarea, n_estado_spc_validada, orden: 90);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        public static readonly string n_tran_spc_asignar = $"{n_spc}: Asignar";
        public static readonly string n_tran_spc_urgente = $"{n_spc}: Urgente";
        public static readonly string n_tran_spc_cancelar = $"{n_spc}: Cancelar";
        public static readonly string n_tran_spc_iniciar = $"{n_spc}: Iniciar";
        public static readonly string n_tran_spc_desasignar = $"{n_spc}: Desasignar";
        public static readonly string n_tran_spc_finalizar = $"{n_spc}: Finalizar";
        public static readonly string n_tran_spc_devolver_asignada = $"{n_spc}: Devolver a asignada";
        public static readonly string n_tran_spc_parar = $"{n_spc}: Parar";
        public static readonly string n_tran_spc_reiniciar = $"{n_spc}: Reiniciar";
        public static readonly string n_tran_spc_devolver = $"{n_spc}: Devolver";
        public static readonly string n_tran_spc_terminar = $"{n_spc}: Terminar";
        public static readonly string n_tran_spc_validada = $"{n_spc}: Validada";

        private static void Transiciones(ContextoSe contexto)
        {
            contexto.IniciarTraza("Transiciones de las tareas de soporte");
            try
            {
                var neg = enumNegocio.Tarea;

                GestorDeTransiciones.DefinirTransicion(contexto, neg, n_tran_spc_asignar, n_estado_spc_inicial, n_estado_spc_asignada);
                GestorDeTransiciones.DefinirTransicion(contexto, neg, n_tran_spc_urgente, n_estado_spc_inicial, n_estado_spc_asignada, asunto: "Motivo de urgencia");
                GestorDeTransiciones.DefinirTransicion(contexto, neg, n_tran_spc_cancelar, n_estado_spc_inicial, n_estado_spc_cancelada, asunto: "Motivo de cancelación");

                GestorDeTransiciones.DefinirTransicion(contexto, neg, n_tran_spc_iniciar, n_estado_spc_asignada, n_estado_spc_en_proceso);
                GestorDeTransiciones.DefinirTransicion(contexto, neg, n_tran_spc_desasignar, n_estado_spc_asignada, n_estado_spc_inicial, asunto: "Motivo de desasignar");

                GestorDeTransiciones.DefinirTransicion(contexto, neg, n_tran_spc_finalizar, n_estado_spc_en_proceso, n_estado_spc_pdt_validar);
                GestorDeTransiciones.DefinirTransicion(contexto, neg, n_tran_spc_devolver_asignada, n_estado_spc_en_proceso, n_estado_spc_asignada, asunto: "Motivo de devolución a asignada");
                GestorDeTransiciones.DefinirTransicion(contexto, neg, n_tran_spc_parar, n_estado_spc_en_proceso, n_estado_spc_parada, asunto: "Motivo de parar la tarea");

                GestorDeTransiciones.DefinirTransicion(contexto, neg, n_tran_spc_reiniciar, n_estado_spc_parada, n_estado_spc_en_proceso);

                GestorDeTransiciones.DefinirTransicion(contexto, neg, n_tran_spc_devolver, n_estado_spc_pdt_validar, n_estado_spc_en_proceso, asunto: "Motivo de devolución");
                GestorDeTransiciones.DefinirTransicion(contexto, neg, n_tran_spc_terminar, n_estado_spc_pdt_validar, n_estado_spc_terminada);
                GestorDeTransiciones.DefinirTransicion(contexto, neg, n_tran_spc_validada, n_estado_spc_pdt_validar, n_estado_spc_validada, porDefecto: true);

                GestorDeTransiciones.DefinirTransicion(contexto, neg, n_tran_spc_devolver, n_estado_spc_terminada, n_estado_spc_pdt_validar, asunto: "Motivo de validación");

                GestorDeTransiciones.DefinirTransicion(contexto, neg, n_tran_spc_devolver, n_estado_spc_validada, n_estado_spc_pdt_validar, asunto: "Motivo devolución");
                GestorDeTransiciones.DefinirTransicion(contexto, neg, n_tran_spc_terminar, n_estado_spc_validada, n_estado_spc_terminada, porDefecto: true);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        public static readonly string n_tipo_ssii = $"{n_ssii}: Estatum";
        public static readonly string n_tipo_soporte = $"{n_ssii}: Soporte";
        private static void Tipos(ContextoSe contexto)
        {
            contexto.IniciarTraza("Tipos de las tareas de soporte");
            try
            {
                var padre = GestorDeTiposDeTarea.PersistirTipo(contexto, enumClaseDeTarea.Control, n_tipo_ssii, enumNegocio.Tarea.Estado(contexto, n_estado_ssii_sin_flujo).Id,
                    clsLibro: enumClaseDeLibro.POR_CG_TIPO,
                    sigla: n_ssii,
                    tipoAri: null,
                    usaPlanificacion: true);

                GestorDeTiposDeTarea.PersistirTipo(contexto, enumClaseDeTarea.Control, n_tipo_soporte, enumNegocio.Tarea.Estado(contexto, n_estado_spc_inicial).Id,
                    enumClaseDeLibro.POR_CG_TIPO,
                    n_spc,
                    tipoAri: null,
                    usaPlanificacion: true,
                    padre,
                    esFacturable: true);
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

            enumNegocio.Tarea.IncluirEnParametro(contexto, enumEtapasDeTareas.TAR_Etapa_Asignada, contexto.SeleccionarEstado<EstadoDeUnaTareaDtm>(n_estado_spc_asignada).Id.ToString());
            enumNegocio.Tarea.IncluirEnParametro(contexto, enumEtapasDeTareas.TAR_Etapa_En_Resolucion, contexto.SeleccionarEstado<EstadoDeUnaTareaDtm>(n_estado_spc_en_proceso).Id.ToString());
            enumNegocio.Tarea.IncluirEnParametro(contexto, enumEtapasDeTareas.TAR_Etapa_En_Espera, contexto.SeleccionarEstado<EstadoDeUnaTareaDtm>(n_estado_spc_parada).Id.ToString());

            // 'Validada' sigue en la etapa de validación: sólo falta darla por terminada
            var envalidacion = $"{contexto.SeleccionarEstado<EstadoDeUnaTareaDtm>(n_estado_spc_pdt_validar).Id},{contexto.SeleccionarEstado<EstadoDeUnaTareaDtm>(n_estado_spc_validada).Id}";
            enumNegocio.Tarea.IncluirEnParametro(contexto, enumEtapasDeTareas.TAR_Etapa_Validacion, envalidacion);
        }

        private static void AccionesAlTransitar(ContextoSe contexto, string nombreDePuesto)
        {
            var puesto = contexto.SeleccionarPorPropiedad<PuestoDtm>(nameof(PuestoDtm.Nombre), nombreDePuesto);
            var json = $@"[
  {{ ""parametro"": ""IdPuesto"", ""valor"": {puesto.Id} }},
  {{ ""parametro"": ""Asunto"", ""valor"": ""Tarea urgente para realizar"" }},
  {{ ""parametro"": ""Cuerpo"", ""valor"": ""Por favor se le ha asignado la tarea de resolución, al finalizar nos lo comunican, gracias."" }}
]";
            var transicion = enumNegocio.Tarea.Transicion(contexto, n_tran_spc_urgente);
            GestorDeAccionesDeTrn.CrearAccionTrn(contexto, enumNegocio.Tarea, transicion, AccionesDeTs.N_CrearCorreoParaPt, enumMomentoDeEjecucion.D, json, 10, "Enviar un correo al responsable de secify");
        }
    }
}
