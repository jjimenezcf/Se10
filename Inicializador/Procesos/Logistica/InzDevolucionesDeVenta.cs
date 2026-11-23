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
    public static class InzDevolucionesDeVenta
    {
        public static readonly string n_ddv = "DDV";

        public static void ModeloDeDevolucionesDeVenta(ContextoSe contexto)
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

        public static readonly string n_estado_ddv_en_tramitacion = $"{n_ddv}: En tramitación";
        public static readonly string n_estado_ddv_aceptada = $"{n_ddv}: Aceptada";
        public static readonly string n_estado_ddv_cancelada = $"{n_ddv}: Cancelada";

        private static void Estados(ContextoSe contexto)
        {
            contexto.IniciarTraza("Estados de la devolución de venta");
            try
            {
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.DevolucionVenta, n_estado_ddv_en_tramitacion, inicial: true, orden: 10);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.DevolucionVenta, n_estado_ddv_aceptada, terminado: true, orden: 20);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.DevolucionVenta, n_estado_ddv_cancelada, cancelado: true, orden: 30);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        private static void DefinirEtapas(ContextoSe contexto)
        {
            var etapaTramitacion = contexto.SeleccionarEstado<EstadoDeUnaDevolucionDeVentaDtm>(n_estado_ddv_en_tramitacion).Id.ToString();
            enumNegocio.DevolucionVenta.ResetearParametro(contexto, enumEtapasDeDevolucionDeVenta.DDV_Etapa_Tramitacion, etapaTramitacion);

            var etapaAceptada = contexto.SeleccionarEstado<EstadoDeUnaDevolucionDeVentaDtm>(n_estado_ddv_aceptada).Id.ToString();
            enumNegocio.DevolucionVenta.ResetearParametro(contexto, enumEtapasDeDevolucionDeVenta.DDV_Etapa_Aceptada, etapaAceptada);

            var etapaCancelada = contexto.SeleccionarEstado<EstadoDeUnaDevolucionDeVentaDtm>(n_estado_ddv_cancelada).Id.ToString();
            enumNegocio.DevolucionVenta.ResetearParametro(contexto, enumEtapasDeDevolucionDeVenta.DDV_Etapa_Cancelada, etapaCancelada);
        }

        public static readonly string n_tran_ddv_aceptar = $"{n_ddv}: Aceptar";
        public static readonly string n_tran_ddv_cancelar = $"{n_ddv}: Cancelar";

        private static void Transiciones(ContextoSe contexto)
        {
            contexto.IniciarTraza("Transiciones de devoluciones de venta");
            try
            {
                //en tramitación --> aceptada (entradas en el almacén, su preasiento y el barrido, y las prefacturas rectificativas)
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.DevolucionVenta, n_tran_ddv_aceptar, n_estado_ddv_en_tramitacion, n_estado_ddv_aceptada);
                //en tramitación --> cancelada (desde el grid de devoluciones del documento de venta o la edición de la devolución: no hace nada más, lo cancelado no cuenta como devuelto)
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.DevolucionVenta, n_tran_ddv_cancelar, n_estado_ddv_en_tramitacion, n_estado_ddv_cancelada);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        public static readonly string n_ddv_tipo_general = $"{n_ddv}: General";
        public static readonly string n_ddv_sigla_general = "DDV";

        private static void Tipos(ContextoSe contexto)
        {
            contexto.IniciarTraza("Tipos de devoluciones de venta");
            try
            {
                var estadoInicial = enumNegocio.DevolucionVenta.Estado(contexto, n_estado_ddv_en_tramitacion);
                GestorDeTiposDeDevolucionDeVenta.PersistirTipo(contexto, n_ddv_tipo_general, estadoInicial.Id, enumClaseDeLibro.POR_CG_TIPO, n_ddv_sigla_general, permiteCrear: true, editarTrasCrear: true);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }
    }
}
