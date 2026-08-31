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
    public static class InzAlmacenes
    {
        public static readonly string n_alm = "ALM";

        public static void ModeloDeAlmacenes(ContextoSe contexto)
        {
            var tran = contexto.IniciarTransaccion();
            try
            {
                Estados(contexto);
                Transiciones(contexto);
                Tipos(contexto);
                TiposDeMovimiento(contexto);
                DefinirEtapas(contexto);
                contexto.Commit(tran);
            }
            catch (Exception ex)
            {
                contexto.Rollback(tran, ex);
                throw;
            }
        }

        public static readonly string n_estado_alm_abierto = $"{n_alm}: Abierto";
        public static readonly string n_estado_alm_cerrado = $"{n_alm}: Cerrado";
        public static readonly string n_estado_alm_en_inventario = $"{n_alm}: En inventario";
        public static readonly string n_estado_alm_cancelado = $"{n_alm}: Cancelado";

        private static void Estados(ContextoSe contexto)
        {
            contexto.IniciarTraza("Estados del almacén");
            try
            {
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Almacen, n_estado_alm_abierto, inicial: true, orden: 10);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Almacen, n_estado_alm_en_inventario, orden: 20);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Almacen, n_estado_alm_cerrado, terminado: true, orden: 30);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Almacen, n_estado_alm_cancelado, cancelado: true, orden: 90);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        private static void DefinirEtapas(ContextoSe contexto)
        {
            var etapaActivo = contexto.SeleccionarEstado<EstadoDeUnAlmacenDtm>(n_estado_alm_abierto).Id.ToString();
            enumNegocio.Almacen.ResetearParametro(contexto, enumEtapasDeAlmacen.ALM_Etapa_Activo, etapaActivo);

            var etapaEnInventario = contexto.SeleccionarEstado<EstadoDeUnAlmacenDtm>(n_estado_alm_en_inventario).Id.ToString();
            enumNegocio.Almacen.ResetearParametro(contexto, enumEtapasDeAlmacen.ALM_Etapa_En_Inventario, etapaEnInventario);

            var etapaCerrado = contexto.SeleccionarEstado<EstadoDeUnAlmacenDtm>(n_estado_alm_cerrado).Id.ToString();
            enumNegocio.Almacen.ResetearParametro(contexto, enumEtapasDeAlmacen.ALM_Etapa_Cerrado, etapaCerrado);

            var etapaCancelado = contexto.SeleccionarEstado<EstadoDeUnAlmacenDtm>(n_estado_alm_cancelado).Id.ToString();
            enumNegocio.Almacen.ResetearParametro(contexto, enumEtapasDeAlmacen.ALM_Etapa_Cancelado, etapaCancelado);
        }


        public static readonly string n_tran_alm_cerrar = $"{n_alm}: Cerrar";
        public static readonly string n_tran_alm_reabrir = $"{n_alm}: Reabrir";
        public static readonly string n_tran_alm_inventariar = $"{n_alm}: Inventariar";
        public static readonly string n_tran_alm_cancelar = $"{n_alm}: Cancelar";

        private static void Transiciones(ContextoSe contexto)
        {
            contexto.IniciarTraza("Transiciones de almacenes");
            try
            {
                //abierto --> cerrado, en inventario, cancelado
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Almacen, n_tran_alm_cerrar, n_estado_alm_abierto, n_estado_alm_cerrado);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Almacen, n_tran_alm_inventariar, n_estado_alm_abierto, n_estado_alm_en_inventario, delSistema: true);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Almacen, n_tran_alm_cancelar, n_estado_alm_abierto, n_estado_alm_cancelado, asunto: "Motivo de cancelación");

                //cerrado --> abierto
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Almacen, n_tran_alm_reabrir, n_estado_alm_cerrado, n_estado_alm_abierto);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        public static readonly string n_alm_tipo_general = $"{n_alm}: General";
        private static void Tipos(ContextoSe contexto)
        {
            contexto.IniciarTraza("Tipos de almacenes");
            try
            {
                var estadoInicial = enumNegocio.Almacen.Estado(contexto, n_estado_alm_abierto);
                GestorDeTiposDeAlmacen.PersistirTipo(contexto, n_alm_tipo_general, estadoInicial.Id, enumClaseDeLibro.POR_CG_TIPO, n_alm, permiteCrear: true);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        public static readonly string n_tpm_stock_inicial = "Stock inicial";
        public static readonly string n_tpm_compra = "Compra de materiales";
        public static readonly string n_tpm_venta = "Venta de materiales";
        public static readonly string n_tpm_consumo = "Consumo";
        public static readonly string n_tpm_dev_compra = "Devolución de compra";
        public static readonly string n_tpm_dev_consumo = "Devolución de consumo";
        public static readonly string n_tpm_dev_venta = "Devolución de venta";
        public static readonly string n_tpm_incremento = "Incremento de stock";
        public static readonly string n_tpm_decremento = "Decremento de stock";
        public static readonly string n_tpm_ajuste_precio = "Ajuste de precio";
        public static readonly string n_tpm_reserva = "Reserva de stock";
        public static readonly string n_tpm_cancela_reserva = "Cancelación de reserva";

        private static void TiposDeMovimiento(ContextoSe contexto)
        {
            contexto.IniciarTraza("Tipos de movimiento de almacén");
            try
            {
                PersistirTipoDeMovimiento(contexto, n_tpm_stock_inicial, enumClaseDeMovimiento.Inicial);
                PersistirTipoDeMovimiento(contexto, n_tpm_compra, enumClaseDeMovimiento.Entrada);
                PersistirTipoDeMovimiento(contexto, n_tpm_venta, enumClaseDeMovimiento.Salida);
                PersistirTipoDeMovimiento(contexto, n_tpm_consumo, enumClaseDeMovimiento.Salida);
                PersistirTipoDeMovimiento(contexto, n_tpm_dev_compra, enumClaseDeMovimiento.Salida);
                PersistirTipoDeMovimiento(contexto, n_tpm_dev_consumo, enumClaseDeMovimiento.Entrada);
                PersistirTipoDeMovimiento(contexto, n_tpm_dev_venta, enumClaseDeMovimiento.Entrada);
                PersistirTipoDeMovimiento(contexto, n_tpm_incremento, enumClaseDeMovimiento.Regularizacion);
                PersistirTipoDeMovimiento(contexto, n_tpm_decremento, enumClaseDeMovimiento.Regularizacion);
                PersistirTipoDeMovimiento(contexto, n_tpm_ajuste_precio, enumClaseDeMovimiento.Ajuste);
                PersistirTipoDeMovimiento(contexto, n_tpm_reserva, enumClaseDeMovimiento.Nulo);
                PersistirTipoDeMovimiento(contexto, n_tpm_cancela_reserva, enumClaseDeMovimiento.Nulo);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        private static void PersistirTipoDeMovimiento(ContextoSe contexto, string nombre, enumClaseDeMovimiento clase)
        {
            new TipoMovimientoDtm { Nombre = nombre, ClaseMovimiento = clase }.PersistirPorAk(contexto, nameof(TipoMovimientoDtm.Nombre));
        }
    }
}
