using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Negocio;
using GestoresDeNegocio.Ventas;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Ventas;
using Utilidades;

namespace Inicializador.Ventas
{
    public static class InzRemesasFae
    {
        public static readonly string n_rem = "REM";

        public static void ModeloDeRemesasFae(ContextoSe contexto)
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

        public static readonly string n_estado_rem_inicial = $"{n_rem}: Inicial";
        public static readonly string n_estado_rem_generada = $"{n_rem}: Generada";
        public static readonly string n_estado_rem_presentada = $"{n_rem}: Presentada";
        public static readonly string n_estado_rem_cerrada = $"{n_rem}: Cerrada";
        public static readonly string n_estado_rem_cancelada = $"{n_rem}: Cancelada";

        private static void Estados(ContextoSe contexto)
        {
            contexto.IniciarTraza("Estados de la remesa");
            try
            {
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.RemesaFae, n_estado_rem_inicial, orden: 10, inicial: true);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.RemesaFae, n_estado_rem_generada,orden: 20);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.RemesaFae, n_estado_rem_presentada, orden: 30);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.RemesaFae, n_estado_rem_cerrada, orden: 50, terminado: true);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.RemesaFae, n_estado_rem_cancelada, orden: 60, cancelado: true);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        private static void DefinirEtapas(ContextoSe contexto)
        {
            var etapaDeCumplimentacion = contexto.SeleccionarEstado<EstadoDeUnaRemesaFaeDtm>(n_estado_rem_inicial).Id.ToString();
            enumNegocio.RemesaFae.ResetearParametro(contexto, enumEtapasDeRemesasFae.REM_Etapa_De_Cumplimentacion, etapaDeCumplimentacion);

            var etapaDeGenerada = contexto.SeleccionarEstado<EstadoDeUnaRemesaFaeDtm>(n_estado_rem_generada).Id.ToString();
            enumNegocio.RemesaFae.ResetearParametro(contexto, enumEtapasDeRemesasFae.REM_Etapa_Generada, etapaDeGenerada);

            var etapaDePresentacion = contexto.SeleccionarEstado<EstadoDeUnaRemesaFaeDtm>(n_estado_rem_presentada).Id.ToString();
            enumNegocio.RemesaFae.ResetearParametro(contexto,enumEtapasDeRemesasFae.REM_Etapa_De_Presentacion, etapaDePresentacion);

            var etapaDeCierre = contexto.SeleccionarEstado<EstadoDeUnaRemesaFaeDtm>(n_estado_rem_cerrada).Id.ToString();
            enumNegocio.RemesaFae.ResetearParametro(contexto,enumEtapasDeRemesasFae.REM_Etapa_De_Cierre, etapaDeCierre);

            var etapaDeCancelada = contexto.SeleccionarEstado<EstadoDeUnaRemesaFaeDtm>(n_estado_rem_cancelada).Id.ToString();
            enumNegocio.RemesaFae.ResetearParametro(contexto,enumEtapasDeRemesasFae.REM_Etapa_Cancelada, etapaDeCancelada);
        }

        private static void DefinirVariables(ContextoSe contexto)
        {
            enumNegocio.RemesaFae.ResetearParametro(contexto, enumParametrosDeRemesasFae.REM_DiasDeEsperaDeCargo, "4");
            enumNegocio.RemesaFae.ResetearParametro(contexto, enumParametrosDeRemesasFae.REM_MesesDeEsperaDeDevolucionDeCargos, "4");
        }

        public static readonly string n_tran_rem_generar = $"{n_rem}: Generar";
        public static readonly string n_tran_rem_corregir = $"{n_rem}: Corregir";
        public static readonly string n_tran_rem_anular = $"{n_rem}: Anular";
        public static readonly string n_tran_rem_presentar = $"{n_rem}: Presentar";
        public static readonly string n_tran_rem_anular_presentacion = $"{n_rem}: Anular presentacion";
        public static readonly string n_tran_rem_cerrar = $"{n_rem}: Cerrar";
        public static readonly string n_tran_rem_reabrir = $"{n_rem}: Reabrir";

        private static void Transiciones(ContextoSe contexto)
        {
            contexto.IniciarTraza("Transiciones de remesas");
            try
            {
                //Inicial --> generada, anulada
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.RemesaFae, n_tran_rem_generar, n_estado_rem_inicial, n_estado_rem_generada);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.RemesaFae, n_tran_rem_anular, n_estado_rem_inicial, n_estado_rem_cancelada, asunto: "Motivo de anulación");


                //Generada --> (presentada, inicial)
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.RemesaFae, n_tran_rem_corregir, n_estado_rem_generada, n_estado_rem_inicial, asunto: "Motivo de apertura");
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.RemesaFae, n_tran_rem_presentar, n_estado_rem_generada, n_estado_rem_presentada);

                //Presentada --> (Generada,  Cerrada)
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.RemesaFae, n_tran_rem_cerrar, n_estado_rem_presentada, n_estado_rem_cerrada);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.RemesaFae, n_tran_rem_anular_presentacion, n_estado_rem_presentada, n_estado_rem_generada, asunto: "Motivo de anulación de presentación");

                //Cerrada --> (Presentada )
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.RemesaFae, n_tran_rem_reabrir, n_estado_rem_cerrada, n_estado_rem_presentada, asunto: "Motivo de reapertura");

            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        public static readonly string n_rem_tipo_general = $"{n_rem}: General";
        private static void Tipos(ContextoSe contexto)
        {
            contexto.IniciarTraza("Tipos de remesas emitidas");
            try
            {
                var estadoInicial = enumNegocio.RemesaFae.Estado(contexto, n_estado_rem_inicial);
                GestorDeTiposDeRemesaFae.PersistirTipo(contexto, n_rem_tipo_general, estadoInicial.Id,enumClaseDeLibro.POR_CG_TIPO, n_rem); 
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }
    }
}
