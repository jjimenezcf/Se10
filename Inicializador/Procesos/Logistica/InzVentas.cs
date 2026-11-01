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
    public static class InzVentas
    {
        public static readonly string n_dsa = "DSA";

        public static void ModeloDeVentas(ContextoSe contexto)
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

        public static readonly string n_estado_dsa_solicitado = $"{n_dsa}: Solicitado";
        public static readonly string n_estado_dsa_sirviendo = $"{n_dsa}: Sirviendo";
        public static readonly string n_estado_dsa_facturado = $"{n_dsa}: Facturado";
        public static readonly string n_estado_dsa_cancelado = $"{n_dsa}: Cancelado";

        private static void Estados(ContextoSe contexto)
        {
            contexto.IniciarTraza("Estados del documento de venta");
            try
            {
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Venta, n_estado_dsa_solicitado, inicial: true, orden: 10);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Venta, n_estado_dsa_sirviendo, orden: 20);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Venta, n_estado_dsa_facturado, terminado: true, orden: 30);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Venta, n_estado_dsa_cancelado, cancelado: true, orden: 90);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        private static void DefinirEtapas(ContextoSe contexto)
        {
            var etapaSolicitud = contexto.SeleccionarEstado<EstadoDeUnaVentaDtm>(n_estado_dsa_solicitado).Id.ToString();
            enumNegocio.Venta.ResetearParametro(contexto, enumEtapasDeVenta.DSA_Etapa_Solicitud, etapaSolicitud);

            var etapaServicio = contexto.SeleccionarEstado<EstadoDeUnaVentaDtm>(n_estado_dsa_sirviendo).Id.ToString();
            enumNegocio.Venta.ResetearParametro(contexto, enumEtapasDeVenta.DSA_Etapa_Servicio, etapaServicio);

            var etapaFacturacion = contexto.SeleccionarEstado<EstadoDeUnaVentaDtm>(n_estado_dsa_facturado).Id.ToString();
            enumNegocio.Venta.ResetearParametro(contexto, enumEtapasDeVenta.DSA_Etapa_Facturacion, etapaFacturacion);

            var etapaCancelacion = contexto.SeleccionarEstado<EstadoDeUnaVentaDtm>(n_estado_dsa_cancelado).Id.ToString();
            enumNegocio.Venta.ResetearParametro(contexto, enumEtapasDeVenta.DSA_Etapa_Cancelacion, etapaCancelacion);
        }


        public static readonly string n_tran_dsa_servir = $"{n_dsa}: Servir";
        public static readonly string n_tran_dsa_cancelar = $"{n_dsa}: Cancelar";
        public static readonly string n_tran_dsa_facturar = $"{n_dsa}: Facturar";
        public static readonly string n_tran_dsa_devolver_a_solicitado = $"{n_dsa}: Devolver a solicitado";
        public static readonly string n_tran_dsa_cancelar_prefactura = $"{n_dsa}: Cancelar prefactura";

        private static void Transiciones(ContextoSe contexto)
        {
            contexto.IniciarTraza("Transiciones de documentos de venta");
            try
            {
                //solicitado --> sirviendo (del sistema: al hacer la 1ª salida del almacén), cancelado
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Venta, n_tran_dsa_servir, n_estado_dsa_solicitado, n_estado_dsa_sirviendo, delSistema: true);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Venta, n_tran_dsa_cancelar, n_estado_dsa_solicitado, n_estado_dsa_cancelado, asunto: "Motivo de cancelación");

                //sirviendo --> facturado (crea la prefactura), solicitado (del sistema: tras anular el último movimiento de salida)
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Venta, n_tran_dsa_facturar, n_estado_dsa_sirviendo, n_estado_dsa_facturado);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Venta, n_tran_dsa_devolver_a_solicitado, n_estado_dsa_sirviendo, n_estado_dsa_solicitado, delSistema: true);

                //facturado --> sirviendo (del sistema: al cancelar la prefactura)
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Venta, n_tran_dsa_cancelar_prefactura, n_estado_dsa_facturado, n_estado_dsa_sirviendo, delSistema: true);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        public static readonly string n_dsa_tipo_general = $"{n_dsa}: General";
        public static readonly string n_dsa_sigla_general = "DSA";

        private static void Tipos(ContextoSe contexto)
        {
            contexto.IniciarTraza("Tipos de documentos de venta");
            try
            {
                var estadoInicial = enumNegocio.Venta.Estado(contexto, n_estado_dsa_solicitado);
                GestorDeTiposDeVenta.PersistirTipo(contexto, n_dsa_tipo_general, estadoInicial.Id, enumClaseDeLibro.POR_CG_TIPO, n_dsa_sigla_general, permiteCrear: true, editarTrasCrear: true);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }
    }
}
