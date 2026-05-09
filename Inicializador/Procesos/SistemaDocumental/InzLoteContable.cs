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
    public static class InzLoteContable
    {
        public static readonly string n_lpr = "LPR";

        public static void ModeloDeLotesContables(ContextoSe contexto)
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

        public static readonly string n_estado_lpr_pendiente = $"{n_lpr}: Pendiente";
        public static readonly string n_estado_lpr_contabilizado = $"{n_lpr}: Contabilizado";
        public static readonly string n_estado_lpr_no_contabilizado = $"{n_lpr}: No contabilizado";

        private static void Estados(ContextoSe contexto)
        {
            contexto.IniciarTraza("Estados de lotes contables");
            try
            {
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.CircuitoDoc, n_estado_lpr_pendiente, orden: 10, inicial: true);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.CircuitoDoc, n_estado_lpr_contabilizado, orden: 50, terminado: true);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.CircuitoDoc, n_estado_lpr_no_contabilizado, orden: 60, cancelado: true);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        private static void DefinirEtapas(ContextoSe contexto)
        {
            var estadoAbierto = contexto.SeleccionarEstado<EstadoDeUnCircuitoDocDtm>(n_estado_lpr_pendiente).Id.ToString();
            enumNegocio.CircuitoDoc.IncluirEnParametro(contexto, enumEtapasDeCircuitosDoc.CAD_Etapa_Abierto, estadoAbierto);

            var estadoCerrado = contexto.SeleccionarEstado<EstadoDeUnCircuitoDocDtm>(n_estado_lpr_contabilizado).Id.ToString();
            enumNegocio.CircuitoDoc.IncluirEnParametro(contexto, enumEtapasDeCircuitosDoc.CAD_Etapa_Cerrado, estadoCerrado);

            var estadoCancelado = contexto.SeleccionarEstado<EstadoDeUnCircuitoDocDtm>(n_estado_lpr_no_contabilizado).Id.ToString();
            enumNegocio.CircuitoDoc.IncluirEnParametro(contexto, enumEtapasDeCircuitosDoc.CAD_Etapa_Cancelado, estadoCancelado);
        }

        private static void DefinirVariables(ContextoSe contexto)
        {
        }

        public static readonly string n_tran_lpr_contabilizar = $"{n_lpr}: Contabilizar";
        public static readonly string n_tran_lpr_no_contabilizar = $"{n_lpr}: Anular lote";
        public static readonly string n_tran_lpr_descontabilizar = $"{n_lpr}: Descontabilizar";

        private static void Transiciones(ContextoSe contexto)
        {
            contexto.IniciarTraza("Transiciones de circuitos documentales");
            try
            {
                //Abierto --> cerrado, cancelado
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.CircuitoDoc, n_tran_lpr_contabilizar, n_estado_lpr_pendiente, n_estado_lpr_contabilizado);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.CircuitoDoc, n_tran_lpr_no_contabilizar, n_estado_lpr_pendiente, n_estado_lpr_no_contabilizado, asunto: "Motivo de no contabilización");


                //Cerrado --> abiero
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.CircuitoDoc, n_tran_lpr_descontabilizar, n_estado_lpr_contabilizado, n_estado_lpr_no_contabilizado, asunto: "Motivo de descontabilización");
            }
            finally

            {
                contexto.CerrarTraza();
            }
        }

        public static readonly string n_lpr_tipo_lote_contable = $"{n_lpr}: Preasientos";
        private static void Tipos(ContextoSe contexto)
        {
            contexto.IniciarTraza("Tipos de lote contable");
            try
            {
                var estadoInicial = enumNegocio.CircuitoDoc.Estado(contexto, n_estado_lpr_pendiente);
                var tipo = GestorDeTiposDeCircuitoDoc.PersistirTipo(contexto, n_lpr_tipo_lote_contable, estadoInicial.Id, enumClaseDeLibro.POR_CG_TIPO, n_lpr);
                enumNegocio.CircuitoDoc.ResetearParametro(contexto, enumParametrosDeCircuitosDoc.CAD_Tipo_Para_Lote_de_Preasientos, tipo.Id.ToString());
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }
    }
}
