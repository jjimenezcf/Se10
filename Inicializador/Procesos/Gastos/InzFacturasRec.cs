using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Negocio;
using GestoresDeNegocio.Gastos;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Gastos;
using Utilidades;
using static GestorDeElementos.Extensores.ExtensorDeParmetrosDeNegocio;
using ServicioDeDatos.Contabilidad;

namespace Inicializador.Gastos
{
    public static class InzFacturasRec
    {
        public static readonly string n_far = "FAR";

        public static void ModeloDeFacturasRec(ContextoSe contexto)
        {
            var tran = contexto.IniciarTransaccion();
            try
            {
                Estados(contexto);
                Transiciones(contexto);
                Tipos(contexto);
                DefinirEtapas(contexto);
                DefinirIrpfs(contexto);
                contexto.Commit(tran);
            }
            catch(Exception ex) 
            {
                contexto.Rollback(tran, ex);
                throw;
            }
        }

        public static readonly string n_estado_far_en_cumplimentacion = $"{n_far}: En cumplimentación";
        public static readonly string n_estado_far_en_aprobacion = $"{n_far}: En aprobación";
        public static readonly string n_estado_far_en_contabilidad = $"{n_far}: En contabilidad";
        public static readonly string n_estado_far_pdt_de_pago = $"{n_far}: Pdt. de pago";
        public static readonly string n_estado_far_devuelta = $"{n_far}: Devuelta";
        public static readonly string n_estado_far_pagada = $"{n_far}: Pagada";
        public static readonly string n_estado_far_anulada = $"{n_far}: Anulada";

        private static void Estados(ContextoSe contexto)
        {
            contexto.IniciarTraza("Estados del facturas");
            try
            {
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.FacturaRecibida, n_estado_far_en_cumplimentacion, inicial: true, orden: 10);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.FacturaRecibida, n_estado_far_en_aprobacion, orden: 20);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.FacturaRecibida, n_estado_far_en_contabilidad, orden: 30);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.FacturaRecibida, n_estado_far_pdt_de_pago, orden: 45);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.FacturaRecibida, n_estado_far_pagada, terminado: true, orden: 50);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.FacturaRecibida, n_estado_far_devuelta, terminado: true, orden: 51);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.FacturaRecibida, n_estado_far_anulada, cancelado: true, orden: 90);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        private static void DefinirEtapas(ContextoSe contexto)
        {
            var etapaEnCumplimentacion = contexto.SeleccionarEstado<EstadoDeUnaFacturaRecDtm>(n_estado_far_en_cumplimentacion).Id.ToString();
            enumNegocio.FacturaRecibida.ResetearParametro(contexto, enumEtapasDeFacturasRec.FAR_Etapa_De_Cumplimentacion, etapaEnCumplimentacion);

            var etapaEnAprobacion = contexto.SeleccionarEstado<EstadoDeUnaFacturaRecDtm>(n_estado_far_en_aprobacion).Id.ToString() + "," + etapaEnCumplimentacion;
            enumNegocio.FacturaRecibida.ResetearParametro(contexto, enumEtapasDeFacturasRec.FAR_Etapa_De_Aprobacion, etapaEnAprobacion);

            var etapaDeContabilizacion = contexto.SeleccionarEstado<EstadoDeUnaFacturaRecDtm>(n_estado_far_en_contabilidad).Id.ToString();
            enumNegocio.FacturaRecibida.ResetearParametro(contexto, enumEtapasDeFacturasRec.FAR_Etapa_De_Contabilizacion, etapaDeContabilizacion);

            var etapaPdtDePago = contexto.SeleccionarEstado<EstadoDeUnaFacturaRecDtm>(n_estado_far_pdt_de_pago).Id.ToString();
            enumNegocio.FacturaRecibida.ResetearParametro(contexto, enumEtapasDeFacturasRec.FAR_Etapa_De_Pago, etapaPdtDePago);

            var etapaDePago = contexto.SeleccionarEstado<EstadoDeUnaFacturaRecDtm>(n_estado_far_pagada).Id.ToString();
            enumNegocio.FacturaRecibida.ResetearParametro(contexto, enumEtapasDeFacturasRec.FAR_Etapa_Pagada, etapaDePago);

            var etapaDeDevuelta = contexto.SeleccionarEstado<EstadoDeUnaFacturaRecDtm>(n_estado_far_devuelta).Id.ToString();
            enumNegocio.FacturaRecibida.ResetearParametro(contexto, enumEtapasDeFacturasRec.FAR_Etapa_Devuelta, etapaDeDevuelta);

            var etapaDeAnulacion = contexto.SeleccionarEstado<EstadoDeUnaFacturaRecDtm>(n_estado_far_anulada).Id.ToString();
            enumNegocio.FacturaRecibida.ResetearParametro(contexto, enumEtapasDeFacturasRec.FAR_Etapa_Anulada, etapaDeAnulacion);

        }

        private static void DefinirIrpfs(ContextoSe contexto)
        {
            var cuenta = contexto.SeleccionarPorPropiedad<CuentaDtm>(nameof(CuentaDtm.Codigo), ltrCuenta.IrpfPersonaFisica, errorSiNoHay: false);
            if ( cuenta == null)
            {
                 cuenta = new CuentaDtm { Codigo = ltrCuenta.IrpfPersonaFisica, Nombre = ltrCuenta.IrpfPersonaFisicadescripcion }.PersistirPorAk(contexto, nameof(CuentaDtm.Codigo));
            }

            new IrpfDtm
            {
                Codigo = "G",
                Nombre = "General",
                Porcentaje = 15,
                IdCuenta = cuenta.Id,
            }.PersistirPorAk(contexto, nameof(CuentaDtm.Codigo));

            new IrpfDtm
            {
                Codigo = "R",
                Nombre = "Reducido",
                Porcentaje = 7,
                IdCuenta = cuenta.Id,
            }.PersistirPorAk(contexto, nameof(CuentaDtm.Codigo));
        }

        public static readonly string n_tran_far_aprobar = $"{n_far}: Aprobar";
        public static readonly string n_tran_far_devolver = $"{n_far}: Devolver";
        public static readonly string n_tran_far_anular = $"{n_far}: Anular";
        public static readonly string n_tran_far_contabilizar = $"{n_far}: Contabilizar";
        public static readonly string n_tran_far_exportar_a_contabilida = $"{n_far}: Exportar para contabilizar";

        public static readonly string n_tran_far_devolver_al_cumplimentador = $"{n_far}: Devolver a aprobación";

        public static readonly string n_tran_far_enviar_a_contabilidad = $"{n_far}: Enviar a contabilidad";
        public static readonly string n_tran_far_exportar_para_contabilizar = $"{n_far}: Exportar para contabilizar";
        public static readonly string n_tran_far_devolver_a_cumplimentacion = $"{n_far}: Devolver a cumplimentación";

        public static readonly string n_tran_far_enviar_a_pagar = $"{n_far}: Pagar";
        public static readonly string n_tran_far_dar_por_contabilizada = $"{n_far}: Dar por contabilizada";
        public static readonly string n_tran_far_pagado = $"{n_far}: Pagada";
        public static readonly string n_tran_far_saldada = $"{n_far}: Saldada";
        public static readonly string n_tran_far_pago_anulado = $"{n_far}: Anular pago";
        public static readonly string n_tran_far_modificar_pagos = $"{n_far}: Modificar pagos";
        public static readonly string n_tran_far_devolver_a_contabilizacion = $"{n_far}: Devolver a contabilidad";
        

        private static void Transiciones(ContextoSe contexto)
        {
            contexto.IniciarTraza("Transiciones de facturas");
            try
            {
                //en cumplimentación --> en aprobación, devuelta, anulada, en contabilidad
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.FacturaRecibida, n_tran_far_aprobar, n_estado_far_en_cumplimentacion, n_estado_far_en_aprobacion);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.FacturaRecibida, n_tran_far_anular, n_estado_far_en_cumplimentacion, n_estado_far_anulada,  asunto: "Motivo de anulación"); 
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.FacturaRecibida, n_tran_far_devolver, n_estado_far_en_cumplimentacion, n_estado_far_devuelta, asunto: "Motivo de correción");
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.FacturaRecibida, n_tran_far_contabilizar, n_estado_far_en_cumplimentacion, n_estado_far_en_contabilidad);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.FacturaRecibida, n_tran_far_exportar_a_contabilida, n_estado_far_en_cumplimentacion, n_estado_far_en_contabilidad, delSistema: true);

                //en aprobación --> en contabiliadad
                //en aprobación --> en cumplimentación
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.FacturaRecibida, n_tran_far_enviar_a_contabilidad, n_estado_far_en_aprobacion, n_estado_far_en_contabilidad);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.FacturaRecibida, n_tran_far_devolver_a_cumplimentacion, n_estado_far_en_aprobacion, n_estado_far_en_cumplimentacion);

                //en contabilidad --> pdt de pago, en aprobación, pagada
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.FacturaRecibida, n_tran_far_enviar_a_pagar, n_estado_far_en_contabilidad, n_estado_far_pdt_de_pago);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.FacturaRecibida, n_tran_far_devolver_al_cumplimentador, n_estado_far_en_contabilidad, n_estado_far_en_cumplimentacion, asunto: "Rechazada por contabilidad");
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.FacturaRecibida, n_tran_far_dar_por_contabilizada, n_estado_far_en_contabilidad, n_estado_far_pdt_de_pago, delSistema:true);

                // pdt de pago --> pagada, en contabilidad
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.FacturaRecibida, n_tran_far_pagado, n_estado_far_pdt_de_pago, n_estado_far_pagada, delSistema: true);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.FacturaRecibida, n_tran_far_saldada, n_estado_far_pdt_de_pago, n_estado_far_pagada, delSistema: false);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.FacturaRecibida, n_tran_far_devolver_a_contabilizacion, n_estado_far_pdt_de_pago, n_estado_far_en_contabilidad, asunto: "Devolver a contabilidad");

                //pagada --> pdt de pago
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.FacturaRecibida, n_tran_far_pago_anulado, n_estado_far_pagada, n_estado_far_pdt_de_pago, delSistema: true);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.FacturaRecibida, n_tran_far_modificar_pagos, n_estado_far_pagada, n_estado_far_pdt_de_pago, delSistema: false, asunto: "Motivo de modificación");
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        public static readonly string n_far_tipo_general = $"{n_far}: General";
        private static void Tipos(ContextoSe contexto)
        {
            contexto.IniciarTraza("Tipos de facturas recibidas");
            try
            {
                var fae_estadoInicial = enumNegocio.FacturaRecibida.Estado(contexto, n_estado_far_en_cumplimentacion);
                GestorDeTiposDeFacturaRec.PersistirTipo(contexto, n_far_tipo_general, fae_estadoInicial.Id, enumClaseDeLibro.POR_CG_TIPO, n_far); 
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }
    }
}
