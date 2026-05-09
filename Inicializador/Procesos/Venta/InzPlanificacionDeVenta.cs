using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Negocio;
using GestoresDeNegocio.Ventas;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Ventas;
using Utilidades;
using static ServicioDeDatos.Ventas.VariablesDePlfsDeVenta;

namespace Inicializador.Ventas
{
    public static class InzPlanificacionesDeVenta
    {
        public static readonly string n_plg = "PLG";

        public static void ModeloDePlanificacionesDeVenta(ContextoSe contexto)
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

        public static readonly string n_estado_plg_pendiente = $"{n_plg}: Pendiente";
        public static readonly string n_estado_plg_generada = $"{n_plg}: Generada";
        public static readonly string n_estado_plg_anulada = $"{n_plg}: Anulada";

        private static void Estados(ContextoSe contexto)
        {
            contexto.IniciarTraza("Estados del expediente");
            try
            {
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.PlanificacionDeVenta, n_estado_plg_pendiente, inicial: true, terminado: false, cancelado: false, 10);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.PlanificacionDeVenta, n_estado_plg_generada, false, true, false, 40);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.PlanificacionDeVenta, n_estado_plg_anulada, false, false, true, 50);            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        private static void DefinirEtapas(ContextoSe contexto)
        {
            var estadosIniciales = contexto.Estados<EstadoDeUnaPlanificacionDeVentaDtm>(nameof(EstadoDtm.Inicial), true);
            var iniciales = "";
            foreach (EstadoDtm estado in estadosIniciales)
                iniciales = $"{(iniciales.IsNullOrEmpty() ? estado.Id.ToString() : $"{iniciales},{estado.Id}")}";
            CacheDeVariable.BorrarVariable(contexto, enumEtapasDePlfsDeVenta.PLF_Etapa_Pendiente);
            enumNegocio.PlanificacionDeVenta.IncluirEnParametro(contexto, enumEtapasDePlfsDeVenta.PLF_Etapa_Pendiente, iniciales);

            var estadosAnulados = "";
            var cancelados = contexto.Estados<EstadoDeUnaPlanificacionDeVentaDtm>(nameof(EstadoDtm.Cancelado), true);
            foreach (EstadoDtm anulado in cancelados)
                estadosAnulados = $"{(estadosAnulados.IsNullOrEmpty() ? anulado.Id.ToString() : $"{estadosAnulados},{anulado.Id}")}";
            CacheDeVariable.BorrarVariable(contexto, enumEtapasDePlfsDeVenta.PLF_Etapa_Anulada);
            enumNegocio.PlanificacionDeVenta.IncluirEnParametro(contexto, enumEtapasDePlfsDeVenta.PLF_Etapa_Anulada, estadosAnulados);

            var estadosGenerados = "";
            var terminados = contexto.Estados<EstadoDeUnaPlanificacionDeVentaDtm>(nameof(EstadoDtm.Terminado), true);
            foreach (EstadoDtm terminado in terminados)
                estadosGenerados = $"{(estadosGenerados.IsNullOrEmpty() ? terminado.Id.ToString() : $"{estadosGenerados},{terminado.Id}")}";
            CacheDeVariable.BorrarVariable(contexto, enumEtapasDePlfsDeVenta.PLF_Etapa_Generada);
            enumNegocio.PlanificacionDeVenta.IncluirEnParametro(contexto, enumEtapasDePlfsDeVenta.PLF_Etapa_Generada, estadosGenerados);
        }

        public static readonly string n_tran_plg_generar = $"{n_plg}: Generar";
        public static readonly string n_tran_plg_generar_auto = $"{n_plg}: Generar (del sistema)";
        public static readonly string n_tran_plg_anular = $"{n_plg}: Anular";

        private static void Transiciones(ContextoSe contexto)
        {
            contexto.IniciarTraza("Transiciones de planificaciones");
            try
            {
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.PlanificacionDeVenta, n_tran_plg_generar, n_estado_plg_pendiente, n_estado_plg_generada);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.PlanificacionDeVenta, n_tran_plg_anular, n_estado_plg_pendiente, n_estado_plg_anulada, asunto:  "Motivo de anulación");
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.PlanificacionDeVenta, n_tran_plg_generar_auto, n_estado_plg_pendiente, n_estado_plg_generada, delSistema: true);

            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        public static readonly string n_plg_tipo_general = $"{n_plg}: General";
        private static void Tipos(ContextoSe contexto)
        {
            contexto.IniciarTraza("Tipos de planificaciones");
            try
            {
                var plg_estadoInicial = enumNegocio.PlanificacionDeVenta.Estado(contexto, n_estado_plg_pendiente);
                //new TipoDePlanificacionDeVentaDtm
                //{
                //  IdEstado = fae_estadoInicial.Id,
                //  ClaseDeLibro = enumClaseDeLibro.POR_CG_TIPO,
                //  Nombre = n_fae_tipo_general,
                //  Sigla = n_plg
                //}.PersistirPorNombre(contexto);
                GestorDeTiposPlanificacionDeVenta.PersistirTipo(contexto, n_plg_tipo_general, plg_estadoInicial.Id, enumClaseDeLibro.POR_CG_TIPO, n_plg);            }
            finally
            {
                contexto.CerrarTraza();
            }
        }
    }
}
