using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Negocio;
using GestoresDeNegocio.SistemaDocumental;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.SistemaDocumental;
using Utilidades;

namespace Inicializador.SistemaDocumental
{
    public static class InzLoteEstimacionDirecta
    {
        public static readonly string n_led = "LED";

        public static void ModeloDeLotesEstimacion(ContextoSe contexto)
        {
            var tran = contexto.IniciarTransaccion();
            try
            {
                Estados(contexto);
                Transiciones(contexto);
                Tipos(contexto);
                DefinirEtapas(contexto);
                DefinirVariables(contexto);
                contexto.Commit(tran);
            }
            catch
            {
                contexto.Rollback(tran);
                throw;
            }
        }

        public static readonly string n_estado_led_pendiente = $"{n_led}: Pendiente";
        public static readonly string n_estado_led_contabilizado = $"{n_led}: Contabilizado";
        public static readonly string n_estado_led_no_contabilizado = $"{n_led}: No contabilizado";

        private static void Estados(ContextoSe contexto)
        {
            contexto.IniciarTraza("Estados de lotes contables");
            try
            {
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.CircuitoDoc, n_estado_led_pendiente, orden: 10, inicial: true);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.CircuitoDoc, n_estado_led_contabilizado, orden: 50, terminado: true);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.CircuitoDoc, n_estado_led_no_contabilizado, orden: 60, cancelado: true);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        private static void DefinirEtapas(ContextoSe contexto)
        {
            var estadoAbierto = contexto.SeleccionarEstado<EstadoDeUnCircuitoDocDtm>(n_estado_led_pendiente).Id.ToString();
            enumNegocio.CircuitoDoc.IncluirEnParametro(contexto, enumEtapasDeCircuitosDoc.CAD_Etapa_Abierto, estadoAbierto);

            var estadoCerrado = contexto.SeleccionarEstado<EstadoDeUnCircuitoDocDtm>(n_estado_led_contabilizado).Id.ToString();
            enumNegocio.CircuitoDoc.IncluirEnParametro(contexto, enumEtapasDeCircuitosDoc.CAD_Etapa_Cerrado, estadoCerrado);

            var estadoCancelado = contexto.SeleccionarEstado<EstadoDeUnCircuitoDocDtm>(n_estado_led_no_contabilizado).Id.ToString();
            enumNegocio.CircuitoDoc.IncluirEnParametro(contexto, enumEtapasDeCircuitosDoc.CAD_Etapa_Cancelado, estadoCancelado);
        }

        private static void DefinirVariables(ContextoSe contexto)
        {
        }

        public static readonly string n_tran_led_contabilizar = $"{n_led}: Contabilizar";
        public static readonly string n_tran_led_cancelar = $"{n_led}: Anular lote";
        public static readonly string n_tran_led_descontabilizar = $"{n_led}: Descontabilizar";
        public static readonly string n_tran_led_tras_desvincular = $"{n_led}: Tras desvincular";

        private static void Transiciones(ContextoSe contexto)
        {
            contexto.IniciarTraza("Transiciones de circuitos documentales");
            try
            {
                //Abierto --> cerrado, cancelado
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.CircuitoDoc, n_tran_led_contabilizar, n_estado_led_pendiente, n_estado_led_contabilizado);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.CircuitoDoc, n_tran_led_cancelar, n_estado_led_pendiente, n_estado_led_no_contabilizado, asunto: "Motivo de no contabilización");
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.CircuitoDoc, n_tran_led_tras_desvincular, n_estado_led_pendiente, n_estado_led_no_contabilizado, delSistema: true);


                //Cerrado --> abiero
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.CircuitoDoc, n_tran_led_descontabilizar, n_estado_led_contabilizado, n_estado_led_no_contabilizado, asunto: "Motivo de descontabilización");
            }
            finally

            {
                contexto.CerrarTraza();
            }
        }

        public static readonly string n_led_tipo_lote_contable = $"{n_led}: Estimación directa";
        private static void Tipos(ContextoSe contexto)
        {
            contexto.IniciarTraza("Tipos de lote contable");
            try
            {
                var estadoInicial = enumNegocio.CircuitoDoc.Estado(contexto, n_estado_led_pendiente);
                var tipo = GestorDeTiposDeCircuitoDoc.PersistirTipo(contexto, n_led_tipo_lote_contable, estadoInicial.Id, enumClaseDeLibro.POR_CG_TIPO, n_led, permiteCrear: true );
                enumNegocio.CircuitoDoc.ResetearParametro(contexto, enumParametrosDeCircuitosDoc.CAD_Tipo_Para_Estimacion_Directa, tipo.Id.ToString());
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }
    }
}
