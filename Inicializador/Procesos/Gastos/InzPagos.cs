using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Gastos;
using GestoresDeNegocio.Negocio;
using Newtonsoft.Json;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Gastos;
using Utilidades;

namespace Inicializador.Gastos
{
    public static class InzPagos
    {
        public static readonly string n_pag = "PAG";
        public static readonly string n_abo = "ABO";

        public static void ModeloDePagos(ContextoSe contexto)
        {
            var tran = contexto.IniciarTransaccion();
            try
            {
                Estados(contexto);
                Transiciones(contexto);
                TiposPagos(contexto);
                TiposDeAbono(contexto);
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

        public static readonly string n_estado_pag_pendiente = $"{n_pag}: Pendiente";
        public static readonly string n_estado_pag_pagado = $"{n_pag}: Pagado";
        public static readonly string n_estado_pag_cancelado = $"{n_pag}: Cancelado";
        public static readonly string n_estado_pag_remesado = $"{n_pag}: Remesado";
        public static readonly string n_estado_pag_anulado = $"{n_pag}: Anulado";

        public static readonly string n_estado_abo_pendiente = $"{n_abo}: Pendiente";
        public static readonly string n_estado_abo_pagado = $"{n_abo}: Abonado";
        public static readonly string n_estado_abo_cancelado = $"{n_abo}: Cancelado";
        private static void Estados(ContextoSe contexto)
        {
            contexto.IniciarTraza("Estados de un pago");
            try
            {
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Pago, n_estado_pag_pendiente, orden: 10, inicial: true);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Pago, n_estado_pag_remesado, orden: 20);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Pago, n_estado_pag_anulado, orden: 55, terminado: true);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Pago, n_estado_pag_pagado, orden: 50, terminado: true);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Pago, n_estado_pag_cancelado, orden: 60, cancelado: true);

                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Pago, n_estado_abo_pendiente, orden: 10, inicial: true);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Pago, n_estado_abo_pagado, orden: 51, terminado: true);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.Pago, n_estado_abo_cancelado, orden: 61, cancelado: true);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        private static void DefinirEtapas(ContextoSe contexto)
        {
            var etapaDePendiente = contexto.SeleccionarEstado<EstadoDeUnPagoDtm>(n_estado_pag_pendiente).Id.ToString() + "," +
                contexto.SeleccionarEstado<EstadoDeUnPagoDtm>(n_estado_abo_pendiente).Id.ToString();
            enumNegocio.Pago.ResetearParametro(contexto, enumEtapasDePagos.PAG_Etapa_Pendiente, etapaDePendiente);

            var etapaDePagado = contexto.SeleccionarEstado<EstadoDeUnPagoDtm>(n_estado_pag_pagado).Id.ToString() + "," +
                contexto.SeleccionarEstado<EstadoDeUnPagoDtm>(n_estado_abo_pagado).Id.ToString();
            enumNegocio.Pago.ResetearParametro(contexto, enumEtapasDePagos.PAG_Etapa_Pagado, etapaDePagado);

            var etapaDeCancelado = contexto.SeleccionarEstado<EstadoDeUnPagoDtm>(n_estado_pag_cancelado).Id.ToString() + "," +
                contexto.SeleccionarEstado<EstadoDeUnPagoDtm>(n_estado_abo_cancelado).Id.ToString(); 
            enumNegocio.Pago.ResetearParametro(contexto, enumEtapasDePagos.PAG_Etapa_Cancelado, etapaDeCancelado);

            var etapaDeRemesado = contexto.SeleccionarEstado<EstadoDeUnPagoDtm>(n_estado_pag_remesado).Id.ToString();
            enumNegocio.Pago.ResetearParametro(contexto, enumEtapasDePagos.PAG_Etapa_Remesado, etapaDeRemesado);

            var etapaDeAnulacion = contexto.SeleccionarEstado<EstadoDeUnPagoDtm>(n_estado_pag_anulado).Id.ToString();
            enumNegocio.Pago.ResetearParametro(contexto, enumEtapasDePagos.PAG_Etapa_Anulacion, etapaDeAnulacion);
        }

        private static void DefinirVariables(ContextoSe contexto)
        {
            var alPagarRemesa = new TransicionPorMotivo
            {
                Motivo = VariableDePagos.enumMotivoTransicion.PagarRemesa.ToString(),
                IdEstado = contexto.SeleccionarEstado<EstadoDeUnPagoDtm>(n_estado_pag_remesado).Id,
                IdTransicion = enumNegocio.Pago.Transicion(contexto, n_tran_pag_pagar_remesa, n_estado_pag_remesado, n_estado_pag_pagado, delSistema: true).Id
            };
            var alCerrarRemesa = new TransicionPorMotivo
            {
                Motivo = VariableDePagos.enumMotivoTransicion.CerrarRemesa.ToString(),
                IdEstado = contexto.SeleccionarEstado<EstadoDeUnPagoDtm>(n_estado_pag_remesado).Id,
                IdTransicion = enumNegocio.Pago.Transicion(contexto, n_tran_pag_cerrar_remesa, n_estado_pag_remesado, n_estado_pag_pagado, delSistema: true).Id
            };
            var alRetrocederRemesa = new TransicionPorMotivo
            {
                Motivo = VariableDePagos.enumMotivoTransicion.RetrocederRemesa.ToString(),
                IdEstado = contexto.SeleccionarEstado<EstadoDeUnPagoDtm>(n_estado_pag_pagado).Id,
                IdTransicion = enumNegocio.Pago.Transicion(contexto, n_tran_pag_retroceder_remesa, n_estado_pag_pagado, n_estado_pag_remesado, delSistema: true).Id
            };
            var alAnularUnPago = new TransicionPorMotivo
            {
                Motivo = VariableDePagos.enumMotivoTransicion.AnularPago.ToString(),
                IdEstado = contexto.SeleccionarEstado<EstadoDeUnPagoDtm>(n_estado_pag_pagado).Id,
                IdTransicion = enumNegocio.Pago.Transicion(contexto, n_tran_pag_anular_pago, n_estado_pag_pagado, n_estado_pag_anulado, delSistema: true).Id
            };
            var alAnularAnulacion = new TransicionPorMotivo
            {
                Motivo = VariableDePagos.enumMotivoTransicion.AnularAnulacion.ToString(),
                IdEstado = contexto.SeleccionarEstado<EstadoDeUnPagoDtm>(n_estado_pag_anulado).Id,
                IdTransicion = enumNegocio.Pago.Transicion(contexto, n_tran_pag_anular_anulacion, n_estado_pag_anulado, n_estado_pag_pagado, delSistema: true).Id
            };

            enumParametrosDePagos.PAG_Aplicar_Transicion.Persistir(contexto, JsonConvert.SerializeObject(new List<TransicionPorMotivo> {
                alPagarRemesa,
                alCerrarRemesa,
                alRetrocederRemesa,
                alAnularUnPago,
                alAnularAnulacion
            }));
            enumNegocio.Pago.ResetearParametro(contexto, enumParametrosDePagos.PAG_Aplicar_Transicion, JsonConvert.SerializeObject(new List<TransicionPorMotivo> {
                alPagarRemesa,
                alCerrarRemesa,
                alRetrocederRemesa,
                alAnularUnPago,
                alAnularAnulacion
            }));
        }

        public static readonly string n_tran_pag_pagar = $"{n_pag}: Pagar";
        public static readonly string n_tran_al_dar_por_pagada_la_factura = $"{n_pag}: Dar por pagada la factura";        
        public static readonly string n_tran_pag_cancelar = $"{n_pag}: Cancelar";
        public static readonly string n_tran_pag_reabrir = $"{n_pag}: Reabrir";
        public static readonly string n_tran_pag_remesar = $"{n_pag}: Remesar";
        public static readonly string n_tran_pag_sacar_de_remesa = $"{n_pag}: Sacar de la remesar";
        public static readonly string n_tran_pag_cerrar_remesa = $"{n_pag}: Cerrar remesa";
        public static readonly string n_tran_pag_pagar_remesa = $"{n_pag}: Pagar remesa";
        public static readonly string n_tran_pag_retroceder_remesa = $"{n_pag}: Retroceder remesa";
        
        public static readonly string n_tran_pag_anular_pago = $"{n_pag}: Anular pago";
        public static readonly string n_tran_pag_anular_anulacion = $"{n_pag}: Anular anulacion";

        public static readonly string n_tran_abo_pagar = $"{n_abo}: Abonar";
        public static readonly string n_tran_abo_cancelar = $"{n_abo}: Cancelar";
        public static readonly string n_tran_abo_reabrir = $"{n_abo}: Reabrir";

        private static void Transiciones(ContextoSe contexto)
        {
            contexto.IniciarTraza("Transiciones de pagos");
            try
            {
                //pendiente --> pagado, remesado, cancelado
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Pago, n_tran_pag_pagar, n_estado_pag_pendiente, n_estado_pag_pagado);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Pago, n_tran_al_dar_por_pagada_la_factura, n_estado_pag_pendiente, n_estado_pag_pagado, delSistema:true);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Pago, n_tran_pag_remesar, n_estado_pag_pendiente, n_estado_pag_remesado, delSistema: true);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Pago, n_tran_pag_cancelar, n_estado_pag_pendiente, n_estado_pag_cancelado, asunto: "Motivo de cancelación");

                //remesado --> pagado, anulado, pendiente
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Pago, n_tran_pag_cerrar_remesa, n_estado_pag_remesado, n_estado_pag_pagado, delSistema: true);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Pago, n_tran_pag_pagar_remesa, n_estado_pag_remesado, n_estado_pag_pagado, delSistema: true);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Pago, n_tran_pag_sacar_de_remesa, n_estado_pag_remesado, n_estado_pag_pendiente, delSistema: true);


                //Anulado --> pagado
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Pago, n_tran_pag_anular_anulacion, n_estado_pag_anulado, n_estado_pag_pagado, delSistema: true);


                //Pagado --> pendiente, remesado, anulado
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Pago, n_tran_pag_anular_pago, n_estado_pag_pagado, n_estado_pag_anulado, delSistema: true);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Pago, n_tran_pag_reabrir, n_estado_pag_pagado, n_estado_pag_pendiente, asunto: "Pago reabierto por:");
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Pago, n_tran_pag_retroceder_remesa, n_estado_pag_pagado, n_estado_pag_remesado, delSistema: true);

                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Pago, n_tran_abo_pagar, n_estado_abo_pendiente, n_estado_abo_pagado);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Pago, n_tran_abo_cancelar, n_estado_abo_pendiente, n_estado_abo_cancelado, asunto: "Motivo de cancelación");
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.Pago, n_tran_abo_reabrir, n_estado_abo_pagado, n_estado_abo_pendiente, asunto: "Abono reabierto por:");
            }
            finally

            {
                contexto.CerrarTraza();
            }
        }

        public static readonly string n_pag_tipo_general = $"{n_pag}: General";
        private static void TiposPagos(ContextoSe contexto)
        {
            contexto.IniciarTraza("Tipos de pagos");
            try
            {
                var estadoInicial = enumNegocio.Pago.Estado(contexto, n_estado_pag_pendiente);
                GestorDeTiposDePago.PersistirTipo(contexto, n_pag_tipo_general, estadoInicial.Id, enumClaseDeLibro.POR_CG_TIPO, n_pag);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        public static readonly string n_abo_tipo_general = $"{n_abo}: General";
        private static void TiposDeAbono(ContextoSe contexto)
        {
            contexto.IniciarTraza("Tipos de abonos");
            try
            {
                var estadoInicial = enumNegocio.Pago.Estado(contexto, n_estado_abo_pendiente);
                GestorDeTiposDePago.PersistirTipo(contexto, n_abo_tipo_general, estadoInicial.Id, enumClaseDeLibro.POR_CG_TIPO, n_abo);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }
    }
}
