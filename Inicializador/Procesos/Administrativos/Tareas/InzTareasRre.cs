using GestorDeElementos;
using GestoresDeNegocio.Negocio;
using GestoresDeNegocio.SistemaDocumental;
using GestoresDeNegocio.Tarea;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Tarea;
using Utilidades;
using GestorDeElementos.Extensores;

namespace Inicializador.Procesos
{
    public static class InzTareasRre
    {
        private static readonly string n_trr = "TRR";

        public static void ModeloDeTareasTrr(ContextoSe contexto)
        {
            var tran = contexto.IniciarTransaccion();
            try
            {
                Estados(contexto);
                Transiciones(contexto);
                Tipos(contexto);

                contexto.Commit(tran);
            }
            catch
            {
                contexto.Rollback(tran);
                throw;
            }
        }

        public static readonly string n_estado_trr_pendiente = $"{n_trr}: Pendiente";
        public static readonly string n_estado_trr_cancelada = $"{n_trr}: Cancelada";
        public static readonly string n_estado_trr_en_resolucion = $"{n_trr}: En resolución";
        public static readonly string n_estado_trr_resuelta = $"{n_trr}: Resuelta";
        private static void Estados(ContextoSe contexto)
        {
            contexto.IniciarTraza("Estados de las tareas de TRR");
            try
            {
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Tarea, n_estado_trr_pendiente, true, false, false, 10);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Tarea, n_estado_trr_en_resolucion, false, false, false, 15);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Tarea, n_estado_trr_resuelta, false, true, false, 20);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Tarea, n_estado_trr_cancelada, false, false, true, 50);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        public static readonly string n_tran_trr_iniciar = $"{n_trr}: En resolución";
        public static readonly string n_tran_trr_resolver = $"{n_trr}: Realizado";
        public static readonly string n_tran_trr_cancelar = $"{n_trr}: Cancelar";
        private static void Transiciones(ContextoSe contexto)
        {
            contexto.IniciarTraza("Transiciones de las tareas de TRR");
            try
            {
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Tarea, n_tran_trr_iniciar, n_estado_trr_pendiente, n_estado_trr_en_resolucion, true, false, false, null);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Tarea, n_tran_trr_resolver, n_estado_trr_en_resolucion, n_estado_trr_resuelta, true, false, false, null);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Tarea, n_tran_trr_cancelar, n_estado_trr_pendiente, n_estado_trr_cancelada, true, true, false, "Motivo de cancelación");
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }


        public static readonly string n_tipo_trr = $"{n_trr}: Resolución de {InzRegistroEs.n_ree}";
        private static void Tipos(ContextoSe contexto)
        {
            contexto.IniciarTraza("Tipos de las trareas de TRR");
            try
            {
                var ft = new ClausulaDeFiltrado(nameof(EstadoDtm.Nombre), enumCriteriosDeFiltrado.igual, $"{InzRegistroEs.n_ree}: Interno");
                var tipoArchivador = GestorDeTiposDeArchivadores.Gestor(contexto, contexto.Mapeador)
                    .LeerRegistros(0, -1, new List<ClausulaDeFiltrado> { { ft } })[0];

                var estadoInicial = enumNegocio.Tarea.Estado(contexto, n_estado_trr_pendiente);
                GestorDeTiposDeTarea.PersistirTipo(contexto, enumClaseDeTarea.Registro, n_tipo_trr, estadoInicial.Id, enumClaseDeLibro.POR_CG_TIPO, n_trr
                    , tipoArchivador
                    , usaPlanificacion: false);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        public static void AccionesAlTransitar(ContextoSe contexto)
        {
            contexto.IniciarTraza("Acciones de las transiciones de tareas de TRR");
            try
            {
                IniciarTarea(contexto);
                ResolverTarea(contexto);
                CancelarTarea(contexto);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }


        private static void CancelarTarea(ContextoSe contexto)
        {
            var transicionesParaDevolverElRegistro = enumNegocio.Registro.Transiciones(contexto, estadoDestino: InzRegistroEs.n_estado_ree_pdt_asignar).Select(x => x.Id).ToList();
            var jsonTransicion = $@"[
   {{
     ""parametro"": ""IdsDeTransiciones"",
     ""valor"": {transicionesParaDevolverElRegistro.ToJson()}
   }}
]";
            var alCancelar = enumNegocio.Tarea.Transicion(contexto, n_tran_trr_cancelar);
            GestorDeAccionesDeTrn.CrearAccionTrn(contexto, enumNegocio.Tarea, alCancelar, AccionesDeTareas.N_AlCancelarTodasLasTareasDelRegistroEs, enumMomentoDeEjecucion.D, jsonTransicion, 10, "Si se puede, aplica la transición al registro de entrada");
        }

        private static void ResolverTarea(ContextoSe contexto)
        {

            var resolverElRegistro = enumNegocio.Registro.Transicion(contexto, InzRegistroEs.n_tran_ree_registro_resuelto);
            var jsonTransicion = $@"[
   {{
     ""parametro"": ""IdsDeTransiciones"",
     ""valor"": {new List<int> { resolverElRegistro.Id }.ToJson()}
   }}
]";

            var alResolverLaTarea = enumNegocio.Tarea.Transicion(contexto, n_tran_trr_resolver);
            GestorDeAccionesDeTrn.CrearAccionTrn(contexto, enumNegocio.Tarea, alResolverLaTarea, AccionesDeTareas.N_AlCerraTodasLasTareaDelRegistroEs, enumMomentoDeEjecucion.D, jsonTransicion, 10, $"Transita el registro de entrada si no lo está ya a '{InzRegistroEs.n_estado_ree_pdt_respuesta}'");
        }


        private static void IniciarTarea(ContextoSe contexto)
        {
            var transicionesQueLLevanElRegistroAEnResolucion = enumNegocio.Registro.Transiciones(contexto, estadoDestino: InzRegistroEs.n_estado_ree_en_resolucion);

            var lista = transicionesQueLLevanElRegistroAEnResolucion.Select(x => x.Id).ToList();

            var jsonTransicion = $@"[
   {{
     ""parametro"": ""IdsDeTransiciones"",
     ""valor"": {lista.ToJson()}
   }}
]";

            var alIniciar = enumNegocio.Tarea.Transicion(contexto, n_tran_trr_iniciar);
            GestorDeAccionesDeTrn.CrearAccionTrn(contexto, enumNegocio.Tarea, alIniciar, AccionesDeTareas.N_AlIniciarUnaTareaDelRegistroEs, enumMomentoDeEjecucion.D, jsonTransicion, 10, "Transita el registro de entrada si no lo está ya a 'REE: En resolución'");
        }


    }
}
