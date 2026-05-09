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
    public static class InzFichadas
    {
        public static readonly string n_fic = "FIC";

        public static void ModeloDeFichadas(ContextoSe contexto)
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

        public static readonly string n_estado_fic_abierto = $"{n_fic}: Entrada";
        public static readonly string n_estado_fic_cerrado = $"{n_fic}: Salida";
        public static readonly string n_estado_fic_cancelado = $"{n_fic}: Anulada";

        private static void Estados(ContextoSe contexto)
        {
            contexto.IniciarTraza("Estados de fichadas");
            try
            {
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.CircuitoDoc, n_estado_fic_abierto, orden: 10, inicial: true);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.CircuitoDoc, n_estado_fic_cerrado, orden: 50, terminado: true);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.CircuitoDoc, n_estado_fic_cancelado, orden: 60, cancelado: true);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        private static void DefinirEtapas(ContextoSe contexto)
        {
            var estadoAbierto = contexto.SeleccionarEstado<EstadoDeUnCircuitoDocDtm>(n_estado_fic_abierto).Id.ToString();
            enumNegocio.CircuitoDoc.IncluirEnParametro(contexto, enumEtapasDeCircuitosDoc.CAD_Etapa_Abierto, estadoAbierto);

            var estadoCerrado = contexto.SeleccionarEstado<EstadoDeUnCircuitoDocDtm>(n_estado_fic_cerrado).Id.ToString();
            enumNegocio.CircuitoDoc.IncluirEnParametro(contexto, enumEtapasDeCircuitosDoc.CAD_Etapa_Cerrado, estadoCerrado);

            var estadoCancelado = contexto.SeleccionarEstado<EstadoDeUnCircuitoDocDtm>(n_estado_fic_cancelado).Id.ToString();
            enumNegocio.CircuitoDoc.IncluirEnParametro(contexto, enumEtapasDeCircuitosDoc.CAD_Etapa_Cancelado, estadoCancelado);
        }

        private static void DefinirVariables(ContextoSe contexto)
        {
        }

        public static readonly string n_tran_fic_salir = $"{n_fic}: Salir";
        public static readonly string n_tran_fic_anular = $"{n_fic}: Anular";
        public static readonly string n_tran_fic_reabrir = $"{n_fic}: Reabrir";

        private static void Transiciones(ContextoSe contexto)
        {
            contexto.IniciarTraza("Transiciones de circuitos documentales");
            try
            {
                //Abierto --> cerrado, cancelado
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.CircuitoDoc, n_tran_fic_salir, n_estado_fic_abierto, n_estado_fic_cerrado, delSistema: true);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.CircuitoDoc, n_tran_fic_anular, n_estado_fic_abierto, n_estado_fic_cancelado, asunto: "Motivo de cancelación");


                //Cerrado --> abiero
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.CircuitoDoc, n_tran_fic_reabrir, n_estado_fic_cerrado, n_estado_fic_abierto, asunto: "Motivo de apertura");
            }
            finally

            {
                contexto.CerrarTraza();
            }
        }

        public static readonly string n_fic_tipo_fichada = $"{n_fic}: Fichada";
        private static void Tipos(ContextoSe contexto)
        {
            contexto.IniciarTraza("Tipos de fichas");
            try
            {
                var estadoInicial = enumNegocio.CircuitoDoc.Estado(contexto, n_estado_fic_abierto);
                var tipo = GestorDeTiposDeCircuitoDoc.PersistirTipo(contexto, n_fic_tipo_fichada, estadoInicial.Id, enumClaseDeLibro.POR_CG_TIPO, n_fic);
                enumNegocio.CircuitoDoc.ResetearParametro(contexto, enumParametrosDeCircuitosDoc.CAD_IdsDeTiposDeFichadas, tipo.Id.ToString());
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }
    }
}
