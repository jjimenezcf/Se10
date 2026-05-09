using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Negocio;
using GestoresDeNegocio.SistemaDocumental;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.SistemaDocumental;
using Utilidades;

namespace Inicializador.SistemaDocumental
{
    public static class InzActividadesFormativa
    {
        public static readonly string n_afr = "AFR";

        public static void ModeloDeActividadesFormativa(ContextoSe contexto)
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

        public static readonly string n_estado_afr_abierta = $"{n_afr}: Abierta";
        public static readonly string n_estado_afr_cerrada = $"{n_afr}: Cerrada";
        public static readonly string n_estado_afr_cancelada = $"{n_afr}: Cancelada";

        private static void Estados(ContextoSe contexto)
        {
            contexto.IniciarTraza("Estados de actividades formativas");
            try
            {
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.CircuitoDoc, n_estado_afr_abierta, orden: 10, inicial: true);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.CircuitoDoc, n_estado_afr_cerrada, orden: 50, terminado: true);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.CircuitoDoc, n_estado_afr_cancelada, orden: 60, cancelado: true);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        private static void DefinirEtapas(ContextoSe contexto)
        {
            var estadoAbierto = contexto.SeleccionarEstado<EstadoDeUnCircuitoDocDtm>(n_estado_afr_abierta).Id.ToString();
            enumNegocio.CircuitoDoc.IncluirEnParametro(contexto, enumEtapasDeCircuitosDoc.CAD_Etapa_Abierto, estadoAbierto);

            var estadoCerrado = contexto.SeleccionarEstado<EstadoDeUnCircuitoDocDtm>(n_estado_afr_cerrada).Id.ToString();
            enumNegocio.CircuitoDoc.IncluirEnParametro(contexto, enumEtapasDeCircuitosDoc.CAD_Etapa_Cerrado, estadoCerrado);

            var estadoCancelado = contexto.SeleccionarEstado<EstadoDeUnCircuitoDocDtm>(n_estado_afr_cancelada).Id.ToString();
            enumNegocio.CircuitoDoc.IncluirEnParametro(contexto, enumEtapasDeCircuitosDoc.CAD_Etapa_Cancelado, estadoCancelado);
        }

        private static void DefinirVariables(ContextoSe contexto)
        {
        }

        public static readonly string n_tran_afr_cerrar = $"{n_afr}: Cerrar";
        public static readonly string n_tran_afr_cancelar = $"{n_afr}: Cancelar";
        public static readonly string n_tran_afr_reabrir = $"{n_afr}: Reabrir";

        private static void Transiciones(ContextoSe contexto)
        {
            contexto.IniciarTraza("Transiciones de actividades formativas");
            try
            {
                //Abierta --> cerrada, cancelada
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.CircuitoDoc, n_tran_afr_cerrar, n_estado_afr_abierta, n_estado_afr_cerrada, delSistema: true);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.CircuitoDoc, n_tran_afr_cancelar, n_estado_afr_abierta, n_estado_afr_cancelada, asunto: "Motivo de cancelación");


                //Cerrada --> abierta
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.CircuitoDoc, n_tran_afr_reabrir, n_estado_afr_cerrada, n_estado_afr_abierta, asunto: "Motivo de apertura");
            }
            finally

            {
                contexto.CerrarTraza();
            }
        }

        public static readonly string n_afr_tipo_afrhada = $"{n_afr}: Actividad formativa";
        private static void Tipos(ContextoSe contexto)
        {
            contexto.IniciarTraza("Tipos de actividades formativas");
            try
            {
                var estadoInicial = enumNegocio.CircuitoDoc.Estado(contexto, n_estado_afr_abierta);
                var tipo = GestorDeTiposDeCircuitoDoc.PersistirTipo(contexto, n_afr_tipo_afrhada, estadoInicial.Id, enumClaseDeLibro.POR_CG_TIPO, n_afr);
                enumNegocio.CircuitoDoc.ResetearParametro(contexto, enumParametrosDeCircuitosDoc.CAD_Tipos_Para_Actividades_Formativas, tipo.Id.ToString());
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }
    }
}
