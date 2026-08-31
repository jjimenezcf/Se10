using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Negocio;
using GestoresDeNegocio.Logistica;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Logistica;
using Utilidades;
using static GestorDeElementos.Extensores.ExtensorDeParmetrosDeNegocio;

namespace Inicializador.Logistica
{
    public static class InzRegularizaciones
    {
        public static readonly string n_ral = "RAL";

        public static void ModeloDeRegularizaciones(ContextoSe contexto)
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
            catch (Exception ex)
            {
                contexto.Rollback(tran, ex);
                throw;
            }
        }

        public static readonly string n_estado_ral_abierto = $"{n_ral}: Abierto";
        public static readonly string n_estado_ral_recontando = $"{n_ral}: Recontando";
        public static readonly string n_estado_ral_aplicado = $"{n_ral}: Aplicado";
        public static readonly string n_estado_ral_cancelado = $"{n_ral}: Cancelado";

        private static void Estados(ContextoSe contexto)
        {
            contexto.IniciarTraza("Estados de la regularización");
            try
            {
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Regularizacion, n_estado_ral_abierto, inicial: true, orden: 10);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Regularizacion, n_estado_ral_recontando, orden: 20);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Regularizacion, n_estado_ral_aplicado, terminado: true, orden: 30);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Regularizacion, n_estado_ral_cancelado, cancelado: true, orden: 90);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        private static void DefinirEtapas(ContextoSe contexto)
        {
            var etapaInicia = contexto.SeleccionarEstado<EstadoDeUnaRegularizacionDtm>(n_estado_ral_abierto).Id.ToString();
            enumNegocio.Regularizacion.ResetearParametro(contexto, enumEtapasDeRegularizacion.RAL_Inicial, etapaInicia);

            var etapaRecontando = contexto.SeleccionarEstado<EstadoDeUnaRegularizacionDtm>(n_estado_ral_recontando).Id.ToString();
            enumNegocio.Regularizacion.ResetearParametro(contexto, enumEtapasDeRegularizacion.RAL_Recontando, etapaRecontando);

            var etapaCerrado = contexto.SeleccionarEstado<EstadoDeUnaRegularizacionDtm>(n_estado_ral_aplicado).Id.ToString();
            enumNegocio.Regularizacion.ResetearParametro(contexto, enumEtapasDeRegularizacion.RAL_Cerrado, etapaCerrado);

            var etapaCancelado = contexto.SeleccionarEstado<EstadoDeUnaRegularizacionDtm>(n_estado_ral_cancelado).Id.ToString();
            enumNegocio.Regularizacion.ResetearParametro(contexto, enumEtapasDeRegularizacion.RAL_Cancelar, etapaCancelado);
        }


        public static readonly string n_tran_ral_cancelar = $"{n_ral}: Cancelar";
        public static readonly string n_tran_ral_iniciar = $"{n_ral}: Iniciar";
        public static readonly string n_tran_ral_aplicar = $"{n_ral}: Aplicar";
        public static readonly string n_tran_ral_devolver = $"{n_ral}: Devolver";

        private static void Transiciones(ContextoSe contexto)
        {
            contexto.IniciarTraza("Transiciones de regularizaciones");
            try
            {
                //abierto --> recontando, cancelado
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Regularizacion, n_tran_ral_iniciar, n_estado_ral_abierto, n_estado_ral_recontando);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Regularizacion, n_tran_ral_cancelar, n_estado_ral_abierto, n_estado_ral_cancelado, asunto: "Motivo de cancelación");

                //recontando --> aplicado, abierto
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Regularizacion, n_tran_ral_aplicar, n_estado_ral_recontando, n_estado_ral_aplicado);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Regularizacion, n_tran_ral_devolver, n_estado_ral_recontando, n_estado_ral_abierto);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        public static readonly string n_ral_tipo_inicial = "Inventario Inicial";
        public static readonly string n_ral_tipo_recuento = "Recuentos";
        public static readonly string n_ral_tipo_ajuste = "Ajustes de precio";

        public static readonly string n_ral_sigla_inicial = "INV";
        public static readonly string n_ral_sigla_recuento = "RCT";
        public static readonly string n_ral_sigla_ajuste = "APR";

        private static void Tipos(ContextoSe contexto)
        {
            contexto.IniciarTraza("Tipos de regularizaciones");
            try
            {
                var estadoInicial = enumNegocio.Regularizacion.Estado(contexto, n_estado_ral_abierto);
                GestorDeTiposDeRegularizacion.PersistirTipo(contexto, n_ral_tipo_inicial, estadoInicial.Id, enumClaseDeLibro.POR_CG_TIPO, n_ral_sigla_inicial, permiteCrear: true, enumRegularizacionAlm.Inicial);
                GestorDeTiposDeRegularizacion.PersistirTipo(contexto, n_ral_tipo_recuento, estadoInicial.Id, enumClaseDeLibro.POR_CG_TIPO, n_ral_sigla_recuento, permiteCrear: true, enumRegularizacionAlm.Recuento);
                GestorDeTiposDeRegularizacion.PersistirTipo(contexto, n_ral_tipo_ajuste, estadoInicial.Id, enumClaseDeLibro.POR_CG_TIPO, n_ral_sigla_ajuste, permiteCrear: true, enumRegularizacionAlm.AjusteDePrecio);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }
    }
}
