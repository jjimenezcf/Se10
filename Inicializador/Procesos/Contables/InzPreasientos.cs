using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Negocio;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using Utilidades;
using static GestorDeElementos.Extensores.ExtensorDeParmetrosDeNegocio;
using ServicioDeDatos.Contabilidad;
using GestoresDeNegocio.Contabilidad;
using Inicializador.SistemaDocumental;

namespace Inicializador.Contabilidad
{
    public static class InzPreasientos
    {
        public static readonly string n_spr = "SPR";

        public static void ModeloDePreasientos(ContextoSe contexto)
        {
            var tran = contexto.IniciarTransaccion();
            try
            {
                Estados(contexto);
                Transiciones(contexto);
                Tipos(contexto);
                DefinirEtapas(contexto);
                InzLoteContable.ModeloDeLotesContables(contexto);
                InzLoteEstimacionDirecta.ModeloDeLotesEstimacion(contexto);
                contexto.Commit(tran);
            }
            catch(Exception ex) 
            {
                contexto.Rollback(tran, ex);
                throw;
            }
        }

        public static readonly string n_estado_spr_pendiente = $"{n_spr}: Pendiente";
        public static readonly string n_estado_spr_contabilizado = $"{n_spr}: Contabilizado";
        public static readonly string n_estado_spr_cancelado = $"{n_spr}: Cancelado";
        public static readonly string n_estado_spr_anulado = $"{n_spr}: Anulado";
        

        private static void Estados(ContextoSe contexto)
        {
            contexto.IniciarTraza("Estados del preasiento");
            try
            {
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Preasiento, n_estado_spr_pendiente, inicial: true, orden: 10);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Preasiento, n_estado_spr_contabilizado, terminado: true, orden: 20);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Preasiento, n_estado_spr_cancelado, cancelado: true, orden: 90);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Preasiento, n_estado_spr_anulado, cancelado: true, orden: 91);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        private static void DefinirEtapas(ContextoSe contexto)
        {
            var etapaDePendiente = contexto.SeleccionarEstado<EstadoDeUnPreasientoDtm>(n_estado_spr_pendiente).Id.ToString();
            enumNegocio.Preasiento.ResetearParametro(contexto, enumEtapasDePreasiento.SPR_Etapa_Pendiente, etapaDePendiente);

            var etapaDeContabilizado = contexto.SeleccionarEstado<EstadoDeUnPreasientoDtm>(n_estado_spr_contabilizado).Id.ToString();
            enumNegocio.Preasiento.ResetearParametro(contexto, enumEtapasDePreasiento.SPR_Etapa_Contabilizado, etapaDeContabilizado);

            var etapaDeCancelado = contexto.SeleccionarEstado<EstadoDeUnPreasientoDtm>(n_estado_spr_cancelado).Id.ToString();
            enumNegocio.Preasiento.ResetearParametro(contexto, enumEtapasDePreasiento.SPR_Etapa_Cancelado, etapaDeCancelado);

            var etapaDeAnulado = contexto.SeleccionarEstado<EstadoDeUnPreasientoDtm>(n_estado_spr_anulado).Id.ToString();
            enumNegocio.Preasiento.ResetearParametro(contexto, enumEtapasDePreasiento.SPR_Etapa_Anulado, etapaDeAnulado);

        }


        public static readonly string n_tran_spr_contabilizar = $"{n_spr}: Contabilizar";
        public static readonly string n_tran_spr_no_contabilizar = $"{n_spr}: No contabilizar";
        public static readonly string n_tran_spr_descontabilizar = $"{n_spr}: Descontabilizar";
        public static readonly string n_tran_spr_cancelar = $"{n_spr}: Cancelar";
        public static readonly string n_tran_spr_anular = $"{n_spr}: Anular";


        private static void Transiciones(ContextoSe contexto)
        {
            contexto.IniciarTraza("Transiciones de preasientos");
            try
            {
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Preasiento, n_tran_spr_contabilizar, n_estado_spr_pendiente, n_estado_spr_contabilizado, delSistema: true);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Preasiento, n_tran_spr_no_contabilizar, n_estado_spr_pendiente, n_estado_spr_cancelado, asunto: "Motivo de no contabilización",  delSistema: false);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Preasiento, n_tran_spr_descontabilizar, n_estado_spr_contabilizado, n_estado_spr_pendiente, delSistema: true);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Preasiento, n_tran_spr_cancelar, n_estado_spr_pendiente, n_estado_spr_cancelado, delSistema: true);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Preasiento, n_tran_spr_anular, n_estado_spr_contabilizado, n_estado_spr_anulado, delSistema: true);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        private static void Tipos(ContextoSe contexto)
        {
            contexto.IniciarTraza("Tipos de preasientos");
            try
            {
                var estadoInicial = enumNegocio.Preasiento.Estado(contexto, n_estado_spr_pendiente); ;
                GestorDeTiposDePreasiento.PersistirTipo(contexto, "GAS: Pagos", estadoInicial.Id, enumClaseDeLibro.POR_CG_TIPO, "PAG", permitirCrear: true);
                GestorDeTiposDePreasiento.PersistirTipo(contexto, "GAS: Recibidas", estadoInicial.Id, enumClaseDeLibro.POR_CG_TIPO, "FAR" , permitirCrear: true);
                GestorDeTiposDePreasiento.PersistirTipo(contexto, "ING: Emitidas", estadoInicial.Id, enumClaseDeLibro.POR_CG_TIPO, "FAE", permitirCrear: true);
                GestorDeTiposDePreasiento.PersistirTipo(contexto, "ING: Cobros", estadoInicial.Id, enumClaseDeLibro.POR_CG_TIPO, "COB", permitirCrear: true);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }
    }
}
