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
    public static class InzCircuitosDoc
    {
        public static readonly string n_cad = "CAD";

        public static void ModeloDeCircuitosDoc(ContextoSe contexto)
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

        public static readonly string n_estado_cad_abierto = $"{n_cad}: Abierto";
        public static readonly string n_estado_cad_cerrado = $"{n_cad}: Cerrado";
        public static readonly string n_estado_cad_cancelado = $"{n_cad}: Cancelada";

        private static void Estados(ContextoSe contexto)
        {
            contexto.IniciarTraza("Estados de un circuito documental");
            try
            {
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.CircuitoDoc, n_estado_cad_abierto, orden: 10, inicial: true);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.CircuitoDoc, n_estado_cad_cerrado, orden: 50, terminado: true);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.CircuitoDoc, n_estado_cad_cancelado, orden: 60, cancelado: true);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        private static void DefinirEtapas(ContextoSe contexto)
        {
            var estadoAbierto = contexto.SeleccionarEstado<EstadoDeUnCircuitoDocDtm>(n_estado_cad_abierto).Id.ToString();
            enumNegocio.CircuitoDoc.IncluirEnParametro(contexto, enumEtapasDeCircuitosDoc.CAD_Etapa_Abierto, estadoAbierto);

            var estadoCerrado = contexto.SeleccionarEstado<EstadoDeUnCircuitoDocDtm>(n_estado_cad_cerrado).Id.ToString();
            enumNegocio.CircuitoDoc.IncluirEnParametro(contexto, enumEtapasDeCircuitosDoc.CAD_Etapa_Cerrado, estadoCerrado);

            var estadoCancelado = contexto.SeleccionarEstado<EstadoDeUnCircuitoDocDtm>(n_estado_cad_cancelado).Id.ToString();
            enumNegocio.CircuitoDoc.IncluirEnParametro(contexto, enumEtapasDeCircuitosDoc.CAD_Etapa_Cancelado, estadoCancelado);
        }

        private static void DefinirVariables(ContextoSe contexto)
        {
        }

        public static readonly string n_tran_cad_cerrar = $"{n_cad}: Cerrar";
        public static readonly string n_tran_cad_cancelar = $"{n_cad}: Cancelar";
        public static readonly string n_tran_cad_reabrir = $"{n_cad}: Reabrir";

        private static void Transiciones(ContextoSe contexto)
        {
            contexto.IniciarTraza("Transiciones de circuitos documentales");
            try
            {
                //Abierto --> cerrado, cancelado
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.CircuitoDoc, n_tran_cad_cerrar, n_estado_cad_abierto, n_estado_cad_cerrado);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.CircuitoDoc, n_tran_cad_cancelar, n_estado_cad_abierto, n_estado_cad_cancelado, asunto: "Motivo de cancelación");


                //Cerrado --> abiero
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.CircuitoDoc, n_tran_cad_reabrir, n_estado_cad_cerrado, n_estado_cad_abierto, asunto: "Motivo de apertura");
            }
            finally

            {
                contexto.CerrarTraza();
            }
        }

        public static readonly string n_cad_tipo_general = $"{n_cad}: General";
        private static void Tipos(ContextoSe contexto)
        {
            contexto.IniciarTraza("Tipos de circuitos documentales");
            try
            {
                var estadoInicial = enumNegocio.CircuitoDoc.Estado(contexto, n_estado_cad_abierto);
                GestorDeTiposDeCircuitoDoc.PersistirTipo(contexto, n_cad_tipo_general, estadoInicial.Id, enumClaseDeLibro.POR_CG_TIPO, n_cad);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }
    }
}
