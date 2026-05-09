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
    public static class InzPedidos
    {
        public static readonly string n_ped = "PED";

        public static void ModeloDePedidos(ContextoSe contexto)
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
            catch(Exception ex) 
            {
                contexto.Rollback(tran, ex);
                throw;
            }
        }

        public static readonly string n_estado_ped_en_cumplimentacion = $"{n_ped}: En cumplimentación";
        public static readonly string n_estado_ped_en_aprobacion = $"{n_ped}: En aprobación";
        public static readonly string n_estado_ped_solicitado = $"{n_ped}: Solicitado";
        public static readonly string n_estado_ped_en_recepcion = $"{n_ped}: En recepción";
        public static readonly string n_estado_ped_devuelto = $"{n_ped}: Devuelto";
        public static readonly string n_estado_ped_cerrado = $"{n_ped}: Cerrado";
        public static readonly string n_estado_ped_cancelado = $"{n_ped}: Cancelado";

        private static void Estados(ContextoSe contexto)
        {
            contexto.IniciarTraza("Estados del pedido");
            try
            {
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Pedido, n_estado_ped_en_cumplimentacion, inicial: true, orden: 10);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Pedido, n_estado_ped_en_aprobacion, orden: 20);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Pedido, n_estado_ped_solicitado, orden: 30);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Pedido, n_estado_ped_en_recepcion, orden: 45);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Pedido, n_estado_ped_cerrado, terminado: true, orden: 50);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Pedido, n_estado_ped_devuelto, terminado: true, orden: 51);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Pedido, n_estado_ped_cancelado, cancelado: true, orden: 90);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        private static void DefinirEtapas(ContextoSe contexto)
        {
            var etapaEnCumplimentacion = contexto.SeleccionarEstado<EstadoDeUnPedidoDtm>(n_estado_ped_en_cumplimentacion).Id.ToString();
            enumNegocio.Pedido.ResetearParametro(contexto, enumEtapasDePedido.PED_Etapa_De_Cumplimentacion, etapaEnCumplimentacion);

            var etapaEnAprobacion = contexto.SeleccionarEstado<EstadoDeUnPedidoDtm>(n_estado_ped_en_aprobacion).Id.ToString() + "," + etapaEnCumplimentacion;
            enumNegocio.Pedido.ResetearParametro(contexto, enumEtapasDePedido.PED_Etapa_De_Aprobacion, etapaEnAprobacion);

            var etapaDeContabilizacion = contexto.SeleccionarEstado<EstadoDeUnPedidoDtm>(n_estado_ped_solicitado).Id.ToString();
            enumNegocio.Pedido.ResetearParametro(contexto, enumEtapasDePedido.PED_Etapa_De_Solicitud, etapaDeContabilizacion);

            var etapaPdtDePago = contexto.SeleccionarEstado<EstadoDeUnPedidoDtm>(n_estado_ped_en_recepcion).Id.ToString();
            enumNegocio.Pedido.ResetearParametro(contexto, enumEtapasDePedido.PED_Etapa_De_Recepcion, etapaPdtDePago);

            var etapaDePago = contexto.SeleccionarEstado<EstadoDeUnPedidoDtm>(n_estado_ped_cerrado).Id.ToString();
            enumNegocio.Pedido.ResetearParametro(contexto, enumEtapasDePedido.PED_Etapa_Cerrado, etapaDePago);

            var etapaDeDevuelta = contexto.SeleccionarEstado<EstadoDeUnPedidoDtm>(n_estado_ped_devuelto).Id.ToString();
            enumNegocio.Pedido.ResetearParametro(contexto, enumEtapasDePedido.PED_Etapa_Devuelto, etapaDeDevuelta);

            var etapaDeAnulacion = contexto.SeleccionarEstado<EstadoDeUnPedidoDtm>(n_estado_ped_cancelado).Id.ToString();
            enumNegocio.Pedido.ResetearParametro(contexto, enumEtapasDePedido.PED_Etapa_Cancelado, etapaDeAnulacion);

        }


        public static readonly string n_tran_ped_aprobar = $"{n_ped}: Aprobar";
        public static readonly string n_tran_ped_cancelar = $"{n_ped}: Cancelar";
        public static readonly string n_tran_ped_solicitar = $"{n_ped}: Solicitar";

        public static readonly string n_tran_ped_devolver_al_cumplimentador = $"{n_ped}: Devolver a cumplimentación";

        public static readonly string n_tran_ped_enviar_a_proveedor = $"{n_ped}: Enviar al proveedor";
        public static readonly string n_tran_ped_devolver_a_cumplimentacion = $"{n_ped}: Devolver a cumplimentación";

        public static readonly string n_tran_ped_recibir = $"{n_ped}: Cerrar";
        public static readonly string n_tran_ped_dar_por_recibido = $"{n_ped}: Dar por recibido";
        public static readonly string n_tran_ped_cerrado = $"{n_ped}: Cerrar";
        public static readonly string n_tran_ped_reabrir = $"{n_ped}: Reabrir";
        public static readonly string n_tran_ped_devolver_a_solicitado = $"{n_ped}: Devolver al proveedor";
        

        private static void Transiciones(ContextoSe contexto)
        {
            contexto.IniciarTraza("Transiciones de pedidos");
            try
            {
                //en cumplimentación --> en aprobación, cancelado, solicitado
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Pedido, n_tran_ped_aprobar, n_estado_ped_en_cumplimentacion, n_estado_ped_en_aprobacion);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Pedido, n_tran_ped_cancelar, n_estado_ped_en_cumplimentacion, n_estado_ped_cancelado,  asunto: "Motivo de cancelación"); 
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Pedido, n_tran_ped_solicitar, n_estado_ped_en_cumplimentacion, n_estado_ped_solicitado);

                //en aprobación --> en contabiliadad
                //en aprobación --> en cumplimentación
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Pedido, n_tran_ped_enviar_a_proveedor, n_estado_ped_en_aprobacion, n_estado_ped_solicitado);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Pedido, n_tran_ped_devolver_a_cumplimentacion, n_estado_ped_en_aprobacion, n_estado_ped_en_cumplimentacion);

                //solicitado --> en recepción, en aprobación, cerrado
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Pedido, n_tran_ped_recibir, n_estado_ped_solicitado, n_estado_ped_en_recepcion);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Pedido, n_tran_ped_devolver_al_cumplimentador, n_estado_ped_solicitado, n_estado_ped_en_cumplimentacion, asunto: "Comunicar al proveedor");
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Pedido, n_tran_ped_dar_por_recibido, n_estado_ped_solicitado, n_estado_ped_cerrado);

                // en recepción --> cerrado, en solicitud
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Pedido, n_tran_ped_cerrado, n_estado_ped_en_recepcion, n_estado_ped_cerrado);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Pedido, n_tran_ped_devolver_a_solicitado, n_estado_ped_en_recepcion, n_estado_ped_solicitado, asunto: "Devolver al proveedor");

                //pagada --> pdt de pago
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Pedido, n_tran_ped_reabrir, n_estado_ped_cerrado, n_estado_ped_en_recepcion);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        public static readonly string n_ped_tipo_general = $"{n_ped}: General";
        private static void Tipos(ContextoSe contexto)
        {
            contexto.IniciarTraza("Tipos de pedidos");
            try
            {
                var estadoInicial = enumNegocio.Pedido.Estado(contexto, n_estado_ped_en_cumplimentacion);
                GestorDeTiposDePedido.PersistirTipo(contexto, n_ped_tipo_general, estadoInicial.Id, enumClaseDeLibro.POR_CG_TIPO, n_ped, permiteCrear: true); 
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }
    }
}
