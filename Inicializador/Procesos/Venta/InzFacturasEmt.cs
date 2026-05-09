using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Negocio;
using GestoresDeNegocio.SistemaDocumental;
using GestoresDeNegocio.Ventas;
using Newtonsoft.Json;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Ventas;
using Utilidades;
using static GestorDeElementos.Extensores.ExtensorDeParmetrosDeNegocio;

namespace Inicializador.Ventas
{
    public static class InzFacturasEmt
    {
        public static readonly string n_fae = "FAE";

        public static void ModeloDeFacturasEmt(ContextoSe contexto)
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
            catch(Exception ex) 
            {
                contexto.Rollback(tran, ex);
                throw;
            }
        }

        public static readonly string n_estado_fae_prefactura = $"{n_fae}: Prefactura";
        public static readonly string n_estado_fae_emitida = $"{n_fae}: Emitida";
        public static readonly string n_estado_fae_parcial_cobrada = $"{n_fae}: Prcl. Cobrada";
        public static readonly string n_estado_fae_vencida = $"{n_fae}: Vencida";
        public static readonly string n_estado_fae_reclamada = $"{n_fae}: Reclamada";
        public static readonly string n_estado_fae_cobrada = $"{n_fae}: Cobrada";
        public static readonly string n_estado_fae_no_cobrable = $"{n_fae}: No Cobrable";
        public static readonly string n_estado_fae_anulada = $"{n_fae}: Cancelada";
        public static readonly string n_estado_fae_rectificada = $"{n_fae}: Rectificada";
        public static readonly string n_estado_fae_remesada = $"{n_fae}: Remesada";
        public static readonly string n_estado_fae_devuelta = $"{n_fae}: Devuelta";
        public static readonly string n_estado_fae_abonada = $"{n_fae}: Abonada";

        private static void Estados(ContextoSe contexto)
        {
            contexto.IniciarTraza("Estados del facturas");
            try
            {
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.FacturaEmitida, n_estado_fae_prefactura, inicial: true, terminado: false, cancelado: false, 10);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.FacturaEmitida, n_estado_fae_emitida, false, terminado: false, false, 20);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.FacturaEmitida, n_estado_fae_parcial_cobrada, false, terminado: false, false, 30);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.FacturaEmitida, n_estado_fae_vencida, false, terminado: false, false, 45);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.FacturaEmitida, n_estado_fae_reclamada, false, terminado: false, false, 50);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.FacturaEmitida, n_estado_fae_cobrada, false, terminado: true, false, 40);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.FacturaEmitida, n_estado_fae_no_cobrable, false, terminado: true, false, 80);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.FacturaEmitida, n_estado_fae_abonada, terminado: true, orden: 90);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.FacturaEmitida, n_estado_fae_anulada, false, terminado: false, true, 90);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.FacturaEmitida, n_estado_fae_rectificada, false, terminado: true, cancelado:false, 65);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.FacturaEmitida, n_estado_fae_remesada, orden: 60);
                GestorDeEstados.PersistirEstado(contexto, enumNegocio.FacturaEmitida, n_estado_fae_devuelta, orden: 42);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        private static void DefinirEtapas(ContextoSe contexto)
        {
            var etapaDePendiente = contexto.SeleccionarEstado<EstadoDeUnaFacturaEmtDtm>(n_estado_fae_prefactura).Id.ToString();
            //enumEtapasDeFacturasEmt.FAE_Etapa_Prefactura.Persistir(contexto, etapaDePendiente);
            enumNegocio.FacturaEmitida.IncluirEnParametro(contexto, enumEtapasDeFacturasEmt.FAE_Etapa_Prefactura, etapaDePendiente);

            var etapaDeRealizado = contexto.SeleccionarEstado<EstadoDeUnaFacturaEmtDtm>(n_estado_fae_emitida).Id.ToString();
            //enumEtapasDeFacturasEmt.FAE_Etapa_Emitida.Persistir(contexto, etapaDeRealizado);
            enumNegocio.FacturaEmitida.IncluirEnParametro(contexto, enumEtapasDeFacturasEmt.FAE_Etapa_Emitida, etapaDeRealizado);

            var etapaDeCobro = contexto.SeleccionarEstado<EstadoDeUnaFacturaEmtDtm>(n_estado_fae_emitida).Id.ToString() + "," +
                               contexto.SeleccionarEstado<EstadoDeUnaFacturaEmtDtm>(n_estado_fae_parcial_cobrada).Id.ToString() + "," +
                               contexto.SeleccionarEstado<EstadoDeUnaFacturaEmtDtm>(n_estado_fae_vencida).Id.ToString() + "," +
                               contexto.SeleccionarEstado<EstadoDeUnaFacturaEmtDtm>(n_estado_fae_reclamada).Id.ToString();
            //enumEtapasDeFacturasEmt.FAE_Etapa_De_Cobro.Persistir(contexto, etapaDeCobro);
            enumNegocio.FacturaEmitida.IncluirEnParametro(contexto, enumEtapasDeFacturasEmt.FAE_Etapa_De_Cobro, etapaDeCobro);

            var etapaDeReclamacion = contexto.SeleccionarEstado<EstadoDeUnaFacturaEmtDtm>(n_estado_fae_vencida).Id.ToString() + "," +
                                     contexto.SeleccionarEstado<EstadoDeUnaFacturaEmtDtm>(n_estado_fae_reclamada).Id.ToString();
            //enumEtapasDeFacturasEmt.FAE_Etapa_De_Reclamacion.Persistir(contexto, etapaDeReclamacion);
            enumNegocio.FacturaEmitida.IncluirEnParametro(contexto, enumEtapasDeFacturasEmt.FAE_Etapa_De_Reclamacion, etapaDeReclamacion);

            var etapaDeCancelado = contexto.SeleccionarEstado<EstadoDeUnaFacturaEmtDtm>(n_estado_fae_anulada).Id.ToString();
            //enumEtapasDeFacturasEmt.FAE_Etapa_Anulada.Persistir(contexto, etapaDeCancelado);
            enumNegocio.FacturaEmitida.IncluirEnParametro(contexto, enumEtapasDeFacturasEmt.FAE_Etapa_Anulada, etapaDeCancelado);

            var etapaDeNoCobrable = contexto.SeleccionarEstado<EstadoDeUnaFacturaEmtDtm>(n_estado_fae_no_cobrable).Id.ToString();
            //enumEtapasDeFacturasEmt.FAE_Etapa_No_Cobrable.Persistir(contexto, etapaDeNoCobrable);
            enumNegocio.FacturaEmitida.IncluirEnParametro(contexto, enumEtapasDeFacturasEmt.FAE_Etapa_No_Cobrable, etapaDeNoCobrable);

            var etapaTerminada = contexto.SeleccionarEstado<EstadoDeUnaFacturaEmtDtm>(n_estado_fae_cobrada).Id.ToString();
            //enumEtapasDeFacturasEmt.FAE_Etapa_Cobrada.Persistir(contexto, etapaTerminada);
            enumNegocio.FacturaEmitida.IncluirEnParametro(contexto, enumEtapasDeFacturasEmt.FAE_Etapa_Cobrada, etapaTerminada);

            var etapaRectificada = contexto.SeleccionarEstado<EstadoDeUnaFacturaEmtDtm>(n_estado_fae_rectificada).Id.ToString();
            //enumEtapasDeFacturasEmt.FAE_Etapa_Rectificada.Persistir(contexto, etapaRectificada);
            enumNegocio.FacturaEmitida.IncluirEnParametro(contexto, enumEtapasDeFacturasEmt.FAE_Etapa_Rectificada, etapaRectificada);

            var etapaRemesada = contexto.SeleccionarEstado<EstadoDeUnaFacturaEmtDtm>(n_estado_fae_remesada).Id.ToString();
            //enumEtapasDeFacturasEmt.FAE_Etapa_Remesada.Persistir(contexto, etapaRemesada);
            enumNegocio.FacturaEmitida.IncluirEnParametro(contexto, enumEtapasDeFacturasEmt.FAE_Etapa_Remesada, etapaRemesada);

            var etapaDevuelta = contexto.SeleccionarEstado<EstadoDeUnaFacturaEmtDtm>(n_estado_fae_devuelta).Id.ToString();
            //enumEtapasDeFacturasEmt.FAE_Etapa_Devuelta.Persistir(contexto, etapaDevuelta);
            enumNegocio.FacturaEmitida.IncluirEnParametro(contexto, enumEtapasDeFacturasEmt.FAE_Etapa_Devuelta, etapaDevuelta);

            var etapaAbonada = contexto.SeleccionarEstado<EstadoDeUnaFacturaEmtDtm>(n_estado_fae_abonada).Id.ToString();
            //enumEtapasDeFacturasEmt.FAE_Etapa_Abonada.Persistir(contexto, etapaAbonada);
            enumNegocio.FacturaEmitida.IncluirEnParametro(contexto, enumEtapasDeFacturasEmt.FAE_Etapa_Abonada, etapaAbonada);
        }

        private static void DefinirVariables(ContextoSe contexto)
        {
            var alAnularUnPagoParcialVence = new TransicionPorMotivo
            {
                Motivo = VariableDeFacturasEmt.enumMotivoTransicion.AnularPago.ToString(),
                IdEstado = contexto.SeleccionarEstado<EstadoDeUnaFacturaEmtDtm>(n_estado_fae_parcial_cobrada).Id,
                IdTransicion = enumNegocio.FacturaEmitida.Transicion(contexto, n_tran_fae_anular_cobro, n_estado_fae_parcial_cobrada, n_estado_fae_vencida, delSistema: true).Id
            };
            var alVencerUnaFacturaConPagosParciales = new TransicionPorMotivo
            {
                Motivo = VariableDeFacturasEmt.enumMotivoTransicion.VenceFactura.ToString(),
                IdEstado = contexto.SeleccionarEstado<EstadoDeUnaFacturaEmtDtm>(n_estado_fae_parcial_cobrada).Id,
                IdTransicion = enumNegocio.FacturaEmitida.Transicion(contexto, n_tran_fae_enviar_a_vencida, n_estado_fae_parcial_cobrada, n_estado_fae_vencida, delSistema: true).Id
            };
            var alModificarUnVencimientoSinCobros = new TransicionPorMotivo
            {
                Motivo = VariableDeFacturasEmt.enumMotivoTransicion.ModificarVencimiento.ToString(),
                IdEstado = contexto.SeleccionarEstado<EstadoDeUnaFacturaEmtDtm>(n_estado_fae_vencida).Id,
                IdTransicion = enumNegocio.FacturaEmitida.Transicion(contexto, n_tran_ampliar_vencimiento, n_estado_fae_vencida, n_estado_fae_emitida, delSistema: true).Id
            };
            var alModificarUnVencimientoConCobros = new TransicionPorMotivo
            {
                Motivo = VariableDeFacturasEmt.enumMotivoTransicion.ModificarVencimiento.ToString(),
                IdEstado = contexto.SeleccionarEstado<EstadoDeUnaFacturaEmtDtm>(n_estado_fae_vencida).Id,
                IdTransicion = enumNegocio.FacturaEmitida.Transicion(contexto, n_tran_ampliar_vencimiento, n_estado_fae_vencida, n_estado_fae_parcial_cobrada, delSistema: true).Id
            };

            var alRealizarPagoTotal = new TransicionPorMotivo
            {
                Motivo = VariableDeFacturasEmt.enumMotivoTransicion.RealizarPagoTotal.ToString(),
                IdEstado = contexto.SeleccionarEstado<EstadoDeUnaFacturaEmtDtm>(n_estado_fae_emitida).Id,
                IdTransicion = enumNegocio.FacturaEmitida.Transicion(contexto, n_tran_fae_realizar_cobro, n_estado_fae_emitida, n_estado_fae_cobrada, delSistema: true).Id
            };
            var alRealizarPagoParcialSobreFaeVencida = new TransicionPorMotivo
            {
                Motivo = VariableDeFacturasEmt.enumMotivoTransicion.RealizarPagoParcial.ToString(),
                IdEstado = contexto.SeleccionarEstado<EstadoDeUnaFacturaEmtDtm>(n_estado_fae_vencida).Id,
                IdTransicion = enumNegocio.FacturaEmitida.Transicion(contexto, n_tran_fae_realizar_cobro, n_estado_fae_vencida, n_estado_fae_parcial_cobrada, delSistema: true).Id
            };
            var alRealizarPagoParcialSobreFaeReclamada= new TransicionPorMotivo
            {
                Motivo = VariableDeFacturasEmt.enumMotivoTransicion.RealizarPagoParcial.ToString(),
                IdEstado = contexto.SeleccionarEstado<EstadoDeUnaFacturaEmtDtm>(n_estado_fae_reclamada).Id,
                IdTransicion = enumNegocio.FacturaEmitida.Transicion(contexto, n_tran_fae_realizar_cobro, n_estado_fae_reclamada, n_estado_fae_parcial_cobrada, delSistema: true).Id
            };
            var alRectificarUnaFactura = new TransicionPorMotivo
            {
                Motivo = VariableDeFacturasEmt.enumMotivoTransicion.RectificarFactura.ToString(),
                IdEstado = contexto.SeleccionarEstado<EstadoDeUnaFacturaEmtDtm>(n_estado_fae_emitida).Id,
                IdTransicion = enumNegocio.FacturaEmitida.Transicion(contexto, n_tran_fae_rectificar, n_estado_fae_emitida, n_estado_fae_rectificada, delSistema: true).Id
            };
            var alRemesarUnaFactura = new TransicionPorMotivo
            {
                Motivo = VariableDeFacturasEmt.enumMotivoTransicion.RemesarFactura.ToString(),
                IdEstado = contexto.SeleccionarEstado<EstadoDeUnaFacturaEmtDtm>(n_estado_fae_emitida).Id,
                IdTransicion = enumNegocio.FacturaEmitida.Transicion(contexto, n_tran_fae_remesar, n_estado_fae_emitida, n_estado_fae_remesada, delSistema: true).Id
            };
            var alDesremesarUnaFactura = new TransicionPorMotivo
            {
                Motivo = VariableDeFacturasEmt.enumMotivoTransicion.DesremesarFactura.ToString(),
                IdEstado = contexto.SeleccionarEstado<EstadoDeUnaFacturaEmtDtm>(n_estado_fae_remesada).Id,
                IdTransicion = enumNegocio.FacturaEmitida.Transicion(contexto, n_tran_fae_desremesar, n_estado_fae_remesada, n_estado_fae_emitida, delSistema: true).Id
            };
            var alDevolverPagoRemesado = new TransicionPorMotivo
            {
                Motivo = VariableDeFacturasEmt.enumMotivoTransicion.DevolverPagoRemesado.ToString(),
                IdEstado = contexto.SeleccionarEstado<EstadoDeUnaFacturaEmtDtm>(n_estado_fae_cobrada).Id,
                IdTransicion = enumNegocio.FacturaEmitida.Transicion(contexto, n_tran_fae_devolver_pago_remesado, n_estado_fae_cobrada, n_estado_fae_devuelta, delSistema: true).Id
            };
            var alAbonarPagoRemesado = new TransicionPorMotivo
            {
                Motivo = VariableDeFacturasEmt.enumMotivoTransicion.AbonarPagoRemesado.ToString(),
                IdEstado = contexto.SeleccionarEstado<EstadoDeUnaFacturaEmtDtm>(n_estado_fae_remesada).Id,
                IdTransicion = enumNegocio.FacturaEmitida.Transicion(contexto, n_tran_fae_remesa_abonada, n_estado_fae_remesada, n_estado_fae_cobrada, delSistema: true).Id
            };
            var alAnularPresentacionDeRemesa = new TransicionPorMotivo
            {
                Motivo = VariableDeFacturasEmt.enumMotivoTransicion.AnularPresentacionDeRemesa.ToString(),
                IdEstado = contexto.SeleccionarEstado<EstadoDeUnaFacturaEmtDtm>(n_estado_fae_cobrada).Id,
                IdTransicion = enumNegocio.FacturaEmitida.Transicion(contexto, n_tran_fae_anular_presentacion_de_remesa, n_estado_fae_cobrada, n_estado_fae_remesada, delSistema: true).Id
            };

            var motivos = JsonConvert.SerializeObject(new List<TransicionPorMotivo> {
                alAnularUnPagoParcialVence,
                alVencerUnaFacturaConPagosParciales,
                alModificarUnVencimientoSinCobros,
                alModificarUnVencimientoConCobros,
                alRealizarPagoParcialSobreFaeVencida,
                alRealizarPagoParcialSobreFaeReclamada,
                alRectificarUnaFactura,
                alRemesarUnaFactura,
                alDesremesarUnaFactura,
                alRealizarPagoTotal,
                alDevolverPagoRemesado,
                alAbonarPagoRemesado,
                alAnularPresentacionDeRemesa
            });

            //enumVariablesDeFacturasEmt.FAE_Aplicar_Transicion.Persistir(contexto, motivos );
            enumNegocio.FacturaEmitida.ResetearParametro(contexto, enumParametrosDeFacturasEmt.FAE_Aplicar_Transicion, motivos);

            var tipoArchivador = GestorDeTiposDeArchivadores.PersistirTipo(contexto, ltrDeUnaFacturaEmt.NombreTipoArchivadorDeReclamacion(n_fae), enumClaseDeLibro.POR_CG_TIPO, sigla: n_fae, visible: false, delSistema: false);
            enumNegocio.FacturaEmitida.IncluirEnParametro(contexto, enumParametrosDeFacturasEmt.FAE_TipoArchivadorDeReclamacion, tipoArchivador.Id.ToString());
        }

        public static readonly string n_tran_fae_emitir = $"{n_fae}: Emitir";
        public static readonly string n_tran_fae_corregir = $"{n_fae}: Corregir";
        public static readonly string n_tran_fae_anular = $"{n_fae}: Anular";
        public static readonly string n_tran_fae_realizar_cobro = $"{n_fae}: Realizar cobro";
        public static readonly string n_tran_fae_anular_cobro = $"{n_fae}: Anular cobro";
        public static readonly string n_tran_fae_enviar_a_vencida = $"{n_fae}: Enviar a vencida";
        public static readonly string n_tran_fae_iniciar_reclamacion = $"{n_fae}: Iniciar reclamación";
        public static readonly string n_tran_fae_no_cobrable = $"{n_fae}: No cobrable";
        public static readonly string n_tran_ampliar_vencimiento = $"{n_fae}: Ampliar vencimiento";
        public static readonly string n_tran_fae_rectificar = $"{n_fae}: Rectificar";
        public static readonly string n_tran_fae_rectificada = $"{n_fae}: Rectificada";
        public static readonly string n_tran_fae_remesar = $"{n_fae}: Remesar";
        public static readonly string n_tran_fae_desremesar = $"{n_fae}: Desremesar";
        public static readonly string n_tran_fae_remesa_abonada = $"{n_fae}: Remesa abonada";
        public static readonly string n_tran_fae_devolver_pago_remesado = $"{n_fae}: Devolver pago remesado";
        public static readonly string n_tran_fae_anular_presentacion_de_remesa = $"{n_fae}: Anular presentacion de remesa";
        public static readonly string n_tran_fae_devolver_emitida = $"{n_fae}: Devolver a emitida";
        public static readonly string n_tran_fae_enviar_a_reclamada = $"{n_fae}: Enviar a reclamar";
        public static readonly string n_tran_fae_no_abonada_la_remesa = $"{n_fae}: Por no abonarse en la remesa";
        public static readonly string n_tran_fae_anular_devolucion = $"{n_fae}: Anular devolución";
        public static readonly string n_tran_fae_abonar_rectificativa = $"{n_fae}: Abonar";
        public static readonly string n_tran_fae_abonar_al_saldar = $"{n_fae}: Abonar al saldar";
        public static readonly string n_tran_cancelar_abono = $"{n_fae}: Al cancelar abono";

        private static void Transiciones(ContextoSe contexto)
        {
            contexto.IniciarTraza("Transiciones de facturas");
            try
            {
                //Prefactura --> emitida, anulada, prefactura
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.FacturaEmitida, n_tran_fae_emitir, n_estado_fae_prefactura, n_estado_fae_emitida, delSistema: false);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.FacturaEmitida, n_tran_fae_anular, n_estado_fae_prefactura, n_estado_fae_anulada, delSistema: false, asunto: "Motivo de anulación"); 
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.FacturaEmitida, n_tran_fae_corregir, n_estado_fae_emitida, n_estado_fae_prefactura, delSistema: false, asunto: "Motivo de correción");
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.FacturaEmitida, n_tran_fae_abonar_rectificativa, n_estado_fae_emitida, n_estado_fae_abonada, delSistema: false);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.FacturaEmitida, n_tran_fae_abonar_al_saldar, n_estado_fae_emitida, n_estado_fae_abonada, delSistema: true);

                //Emitida --> (parcialmente cobrada - cobrada, vencida, rectificada, remesada)
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.FacturaEmitida, n_tran_fae_realizar_cobro, n_estado_fae_emitida, n_estado_fae_parcial_cobrada, delSistema: true);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.FacturaEmitida, n_tran_fae_realizar_cobro, n_estado_fae_emitida, n_estado_fae_cobrada, delSistema: true);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.FacturaEmitida, n_tran_fae_enviar_a_vencida, n_estado_fae_emitida, n_estado_fae_vencida, delSistema: true);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.FacturaEmitida, n_tran_fae_rectificar, n_estado_fae_emitida, n_estado_fae_rectificada, delSistema: true);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.FacturaEmitida, n_tran_fae_remesar, n_estado_fae_emitida, n_estado_fae_remesada, delSistema: true);


                //Remesada --> (cobrada, emitida, devuelta)
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.FacturaEmitida, n_tran_fae_remesa_abonada,       n_estado_fae_remesada, n_estado_fae_cobrada, delSistema: true);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.FacturaEmitida, n_tran_fae_desremesar,           n_estado_fae_remesada, n_estado_fae_emitida, delSistema: true);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.FacturaEmitida, n_tran_fae_no_abonada_la_remesa, n_estado_fae_remesada, n_estado_fae_devuelta, delSistema: true);


                //Devuelta --> (Emitida, Reclamada, Remesada, cobrada)
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.FacturaEmitida, n_tran_fae_devolver_emitida,   n_estado_fae_devuelta, n_estado_fae_emitida, asunto: "Motivo de devolución");
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.FacturaEmitida, n_tran_fae_enviar_a_reclamada, n_estado_fae_devuelta, n_estado_fae_reclamada, asunto: "Motivo de reclamación");
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.FacturaEmitida, n_tran_fae_anular_devolucion,  n_estado_fae_devuelta, n_estado_fae_remesada, delSistema: true);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.FacturaEmitida, n_tran_fae_anular_devolucion,  n_estado_fae_devuelta, n_estado_fae_cobrada, delSistema: true);

                //parcialmente cobrada --> (cobrada,  emitida, vencida, devuelta)
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.FacturaEmitida, n_tran_fae_realizar_cobro, n_estado_fae_parcial_cobrada, n_estado_fae_cobrada, delSistema: true);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.FacturaEmitida, n_tran_fae_anular_cobro, n_estado_fae_parcial_cobrada, n_estado_fae_emitida, delSistema: true);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.FacturaEmitida, n_tran_fae_enviar_a_vencida, n_estado_fae_parcial_cobrada, n_estado_fae_vencida, delSistema: true);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.FacturaEmitida, n_tran_fae_anular_cobro, n_estado_fae_parcial_cobrada, n_estado_fae_vencida, delSistema: true);

                //Vencida --> (parcialmente cobrada - cobrada, reclamada)
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.FacturaEmitida, n_tran_fae_realizar_cobro, n_estado_fae_vencida, n_estado_fae_parcial_cobrada, delSistema: true);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.FacturaEmitida, n_tran_fae_realizar_cobro, n_estado_fae_vencida, n_estado_fae_cobrada, delSistema: true);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.FacturaEmitida, n_tran_fae_iniciar_reclamacion, n_estado_fae_vencida, n_estado_fae_reclamada, delSistema: false, asunto: "Motivos de pase a reclamación");

                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.FacturaEmitida, n_tran_ampliar_vencimiento, n_estado_fae_vencida, n_estado_fae_emitida, delSistema: true);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.FacturaEmitida, n_tran_ampliar_vencimiento, n_estado_fae_vencida, n_estado_fae_parcial_cobrada, delSistema: true);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.FacturaEmitida, n_tran_fae_rectificada, n_estado_fae_vencida, n_estado_fae_rectificada, delSistema: true);

                //Reclamada --> (no cobrable, parcialmente cobrada - cobrada)
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.FacturaEmitida, n_tran_fae_realizar_cobro, n_estado_fae_reclamada, n_estado_fae_parcial_cobrada, delSistema: true);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.FacturaEmitida, n_tran_fae_realizar_cobro, n_estado_fae_reclamada, n_estado_fae_cobrada, delSistema: true);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.FacturaEmitida, n_tran_fae_no_cobrable, n_estado_fae_reclamada, n_estado_fae_no_cobrable, delSistema: false, asunto: "Motivos de por qué no es cobrable");

                //Cobrada --> (parcialmente cobrada - vencida - emitida - devuelta - remesada)
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.FacturaEmitida, n_tran_fae_anular_cobro, n_estado_fae_cobrada, n_estado_fae_parcial_cobrada, delSistema: true);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.FacturaEmitida, n_tran_fae_anular_cobro, n_estado_fae_cobrada, n_estado_fae_vencida, delSistema: true);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.FacturaEmitida, n_tran_fae_anular_cobro, n_estado_fae_cobrada, n_estado_fae_emitida, delSistema: true);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.FacturaEmitida, n_tran_fae_devolver_pago_remesado, n_estado_fae_cobrada, n_estado_fae_devuelta, delSistema: true);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.FacturaEmitida, n_tran_fae_anular_presentacion_de_remesa, n_estado_fae_cobrada, n_estado_fae_remesada, delSistema: true);
                GestorDeTransiciones.DefinirTransicion(contexto, enumNegocio.FacturaEmitida, n_tran_cancelar_abono, n_estado_fae_abonada, n_estado_fae_emitida, delSistema: true);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        public static readonly string n_fae_tipo_general = $"{n_fae}: General";
        private static void Tipos(ContextoSe contexto)
        {
            contexto.IniciarTraza("Tipos de facturas emitidas");
            try
            {
                var fae_estadoInicial = enumNegocio.FacturaEmitida.Estado(contexto, n_estado_fae_prefactura);
                GestorDeTiposDeFacturaEmt.PersistirTipo(contexto, n_fae_tipo_general, fae_estadoInicial.Id, enumClaseDeLibro.POR_CG_TIPO, n_fae, serie: "S", vencimiento:30); 
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }
    }
}
