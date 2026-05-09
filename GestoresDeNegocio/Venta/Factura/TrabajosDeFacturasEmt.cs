using DocumentFormat.OpenXml.Office2016.Drawing.Charts;
using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.TrabajosSometidos;
using GestoresDeNegocio.Venta.Factura;
using ModeloDeDto;
using ModeloDeDto.Ventas;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.TrabajosSometidos;
using ServicioDeDatos.Ventas;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Utilidades;
using VeriFactu.Common.Exceptions;
using static ServicioDeDatos.Ventas.VariablesDePlfsDeVenta;

namespace GestoresDeNegocio.Ventas
{
    public enum enumTrabajosDeFacturasEmt
    {
        [Description("Pasar a vencidas las facturas impagadas")]
        VencerFacturasImpagadas,
        [Description("Enviar la factura a la AEAT")]
        EnvioDeFacturaAeat,
        [Description("Enviar un lote de facturas a la AEAT")]
        EnvioDeLoteDeFacturaAeat,
        [Description("Firmar factura")]
        FirmarFactura,
        [Description("Cancelar Emision")]
        CancelarEmision,
        [Description("Generar factura pdf")]
        GenerarFacturaPdf,
        [Description("Actualización de facturas emitidas")]
        ActualizacionDeFacturas
    }

    public class TrabajosDeFacturasEmt
    {
        public static TrabajoDeUsuarioDtm SometerVencerFacturasImpagadas(ContextoSe contexto)
        {
            var dll = Assembly.GetExecutingAssembly().GetName().Name;
            var clase = typeof(TrabajosDeFacturasEmt).FullName;
            var ts = GestorDeTrabajosSometido.CrearObtener(contexto, enumTrabajosDeFacturasEmt.VencerFacturasImpagadas.Descripcion(), dll, clase, nameof(enumTrabajosDeFacturasEmt.VencerFacturasImpagadas), comunicarFin: true
            );
            var datosDeCreacion = new Dictionary<string, object>
            {
                { nameof(TrabajoDeUsuarioDtm.Planificado), DateTime.Now.AddDays(1) },
                { nameof(TrabajoDeUsuarioDtm.Periodicidad), 86400}
            };
            return GestorDeTrabajosDeUsuario.CrearSiNoEstaPendiente(contexto, ts, datosDeCreacion);
        }

        public static void VencerFacturasImpagadas(EntornoDeTrabajo entorno)
        {
            var contexto = entorno.contextoDelProceso;
            contexto.IniciarTraza(nameof(VencerFacturasImpagadas));
            try
            {
                entorno.CrearTraza("Inicio del proceso");
                var trazaInfDtm = entorno.CrearTraza($"Traza informativa del proceso");

                //seleccinar las facturas vencidas emitidas o parcialmente cobradas
                var estadosEmitida = VariableDeFacturasEmt.Estados(enumEtapasDeFacturasEmt.FAE_Etapa_Emitida);
                var estadosParcial = VariableDeFacturasEmt.Estados(enumEtapasDeFacturasEmt.FAE_Etapa_Pago_Parcial);
                var estados = estadosEmitida + $"{(estadosEmitida.IsNullOrEmpty() ? "" : $"{(estadosParcial.IsNullOrEmpty() ? "" : Simbolos.Coma)}")}" + estadosParcial;

                var filtros = new List<ClausulaDeFiltrado>
                {
                    new ClausulaDeFiltrado(nameof(FacturaEmtDtm.IdEstado), enumCriteriosDeFiltrado.esAlgunoDe, estados),
                    new ClausulaDeFiltrado(nameof(FacturaEmtDtm.VenceEl), enumCriteriosDeFiltrado.menorIgual, DateTime.Now.ToString()),
                };
                var faes = enumNegocio.FacturaEmitida.SeleccionarPorFiltro<FacturaEmtDtm>(contexto, filtros, parametros: new Dictionary<string, object> { { ltrParametrosNeg.ValidarPermisosDeConsulta, false } });

                //para cada factura
                foreach (var fae in faes)
                {
                    var tran = contexto.IniciarTransaccion();
                    try
                    {
                        if (fae.EstaRectificada(contexto) && fae.RectificadaPor(contexto).ClaseRectificativa == enumClaseDeRectificativa.OR)
                            continue;

                        if (fae.EsRectificativa && fae.ClaseRectificativa == enumClaseDeRectificativa.OR)
                            continue;

                        entorno.ActualizarTraza(trazaInfDtm, $"transitando la {fae.Referencia}");
                        if (fae.EstaEnLaEtapa(enumEtapasDeFacturasEmt.FAE_Etapa_Emitida))
                        {
                            var transicion = fae.TransicionPosible(contexto, enumEtapasDeFacturasEmt.FAE_Etapa_Emitida, enumEtapasDeFacturasEmt.FAE_Etapa_De_Reclamacion.Lista());
                            fae.Transitar(contexto, transicion.Id);
                        }
                        else
                        if (fae.EstaEnLaEtapa(enumEtapasDeFacturasEmt.FAE_Etapa_Pago_Parcial))
                        {
                            fae.IntentarAplicarTransicion(contexto, TransicionAplicable.Transiciones(VariableDeFacturasEmt.TransicionesPorMotivo, VariableDeFacturasEmt.enumMotivoTransicion.VenceFactura, errorSiNoHay: true));
                        }
                        entorno.CrearTraza($"factura {fae.Referencia} vencida");
                        contexto.Commit(tran);
                    }
                    catch (Exception ex)
                    {
                        entorno.AnotarError($"No se ha podido transitar la factura '{fae.Referencia}'", ex);
                        contexto.Rollback(tran);
                    }
                }
            }
            finally
            {
                entorno.CrearTraza($"Fin del proceso realizado");
                contexto.CerrarTraza();
            }
        }

        public static TrabajoDeUsuarioDtm SometerEnvioDeFacturaAeat(ContextoSe contexto, int idFactura)
        {
            var factura = contexto.SeleccionarPorId<FacturaEmtDtm>(idFactura);
            if (!factura.UsaVerifactu(contexto))
                GestorDeErrores.Emitir($"la sociedad '{factura.Sociedad(contexto).NIFSinIsoEs}' no usa Verifactu, por tanto no puede someter el envío a la Aeat de la factura '{factura.Referencia}'");

            var generadorSii = ValidacionesQueExigeLaAeat(contexto, factura);

            if (factura.EsRectificativa && factura.MotivoDeRectificacion == enumMotivoDeRectificacion.DatosErroneos)
                generadorSii.CancelarLaRectificada();

            var dll = Assembly.GetExecutingAssembly().GetName().Name;
            var clase = typeof(TrabajosDeFacturasEmt).FullName;
            var ts = GestorDeTrabajosSometido.CrearObtener(contexto, enumTrabajosDeFacturasEmt.EnvioDeFacturaAeat.Descripcion(), dll, clase, nameof(enumTrabajosDeFacturasEmt.EnvioDeFacturaAeat), comunicarFin: true);
            var parametrosEntrada = new Dictionary<string, object> { { nameof(FacturaEmtDtm.Id), idFactura } };
            var datosDeCreacion = new Dictionary<string, object>
                {
                    { nameof(TrabajoDeUsuarioDtm.Parametros), parametrosEntrada.ToJson() },
                    { nameof(TrabajoDeUsuarioDtm.Planificado), DateTime.Now.AddMinutes(-1) }
                };
            return GestorDeTrabajosDeUsuario.CrearSiNoEstaPendiente(contexto, ts, datosDeCreacion);
        }

        public static TrabajoDeUsuarioDtm SometerEnvioDeLoteDeFacturaAeat(ContextoSe contexto, List<int> loteDeFacturas, int idSociedad, int idsemaforo)
        {
            var quitarElSemaforo = false;
            try
            {
                if (idsemaforo > 0)
                {
                    var semaforos = SemaforoDeProcesoSql.LeerSemaforos(contexto, enumNegocio.Sociedad.IdNegocio(), enumOpercionesDeSemaforo.VERI);
                    if (semaforos.Count == 0)
                    {
                        var sociedaEnProceso = contexto.SeleccionarPorId<SociedadDtm>(idSociedad);
                        quitarElSemaforo = true;
                        GestorDeErrores.Emitir($"No está el semáforo indicado para activar el proceso de verifactu de la sociedad '{sociedaEnProceso.NIF}'");

                    }
                    var semDiferente = semaforos.FirstOrDefault(sem => sem.Id != idsemaforo);
                    if (semDiferente != null)
                    {
                        var sociedaEnProceso = contexto.SeleccionarPorId<SociedadDtm>(semDiferente.IdElemento);
                        quitarElSemaforo = true;
                        GestorDeErrores.Emitir($"Se está activando el verifactu para la sociedad '{sociedaEnProceso.NIF}', espere a que termine");
                    }
                }

                var dll = Assembly.GetExecutingAssembly().GetName().Name;
                var clase = typeof(TrabajosDeFacturasEmt).FullName;
                var ts = GestorDeTrabajosSometido.CrearObtener(contexto, enumTrabajosDeFacturasEmt.EnvioDeLoteDeFacturaAeat.Descripcion(), dll, clase, nameof(enumTrabajosDeFacturasEmt.EnvioDeLoteDeFacturaAeat), comunicarFin: true);
                var parametrosEntrada = new Dictionary<string, object> {
                { ltrDeLogDeEnvioDeFactura.lote, loteDeFacturas },
                { ltrDeLogDeEnvioDeFactura.IdSociedad, idSociedad },
                { ltrDeLogDeEnvioDeFactura.IdSemaforo, idsemaforo }
            };
                var datosDeCreacion = new Dictionary<string, object>
                {
                    { nameof(TrabajoDeUsuarioDtm.Parametros), parametrosEntrada.ToJson() },
                    { nameof(TrabajoDeUsuarioDtm.Planificado), DateTime.Now.AddMinutes(1) }
                };
                var trabajo = GestorDeTrabajosDeUsuario.CrearSiNoEstaPendiente(contexto, ts, datosDeCreacion);
                return trabajo;
            }
            finally
            {
                if (quitarElSemaforo)
                {
                    SemaforoDeProcesoSql.QuitarSemaforo(contexto, idsemaforo);
                }
            }
        }

        public static void EnvioDeLoteDeFacturaAeat(EntornoDeTrabajo entorno)
        {
            var contexto = entorno.contextoDelProceso;
            var parametros = entorno.TrabajoDeUsuario.Parametros.ToDiccionarioDeParametros();
            var lote = parametros.LeerValor<List<int>>(ltrDeLogDeEnvioDeFactura.lote);
            var idSociedad = parametros.LeerValor<int>(ltrDeLogDeEnvioDeFactura.IdSociedad);
            var sociedad = contexto.SeleccionarPorId<SociedadDtm>(idSociedad);
            var idSemaforoDeSociedad = parametros.LeerValor(ltrDeLogDeEnvioDeFactura.IdSemaforo, 0);

            if (idSemaforoDeSociedad > 0)
            {
                var semaforos = SemaforoDeProcesoSql.LeerSemaforos(contexto, enumNegocio.Sociedad.IdNegocio(), enumOpercionesDeSemaforo.VERI);
                if (semaforos.Count != 1 || !semaforos.Any(sem => sem.IdElemento == idSociedad))
                {
                    SemaforoDeProcesoSql.QuitarSemaforo(entorno.ContextoDelEntorno, idSemaforoDeSociedad);
                    GestorDeErrores.Emitir($"Debería haber un proceso de activación de verifactur para la sociedad '{sociedad.NIF}', solvéntelo antes de continuar");
                }
            }

            var comunicadas = 0;
            contexto.IniciarTraza(nameof(EnvioDeLoteDeFacturaAeat));
            var pendientes = new List<int>();
            try
            {
                var traza = entorno.CrearTraza("Validando factura");
                foreach (var idFactura in lote)
                {
                    var factura = contexto.SeleccionarPorId<FacturaEmtDtm>(idFactura);
                    if (factura.Verifactu(contexto, errorSiNoHay: false) is not null)
                    {
                        entorno.CrearTraza($"La factura '{factura.Referencia}' con id '{factura.Id}' ya había sido enviada");
                        continue;
                    }
                    pendientes.Add(idFactura);
                    ValidacionesQueExigeLaAeat(contexto, factura);
                    traza.Actualizar(entorno.ContextoDelEntorno, $"Factura '{factura.Referencia}' validada");
                }

                traza = entorno.CrearTraza("Enviando facturas");
                foreach (var idFactura in pendientes)
                {
                    var factura = contexto.SeleccionarPorId<FacturaEmtDtm>(idFactura);

                    if (factura.ClaseDeEmision == enumClaseDeEmision.Impresa) factura.ClaseDeEmision = enumClaseDeEmision.eFactura322;

                    var idSemaforo = SemaforoDeProcesoSql.PonerSemaforo(entorno.ContextoDelEntorno, enumNegocio.FacturaEmitida.IdNegocio(), idFactura, enumOpercionesDeSemaforo.ASII, factura.Referencia).Id;
                    var tran = contexto.IniciarTransaccion();
                    try
                    {
                        new GeneradorSii(contexto, factura).AltaDeFacturaSif(esUnLote: true);
                        traza.Actualizar(entorno.ContextoDelEntorno, $"Factura '{factura.Referencia}' enviada");
                        comunicadas++;
                        contexto.Commit(tran);
                    }
                    catch (Exception e)
                    {
                        entorno.AnotarError(e);
                        if (e.Message.Contains(ltrSii.NoSePuedeReenviar) || e.Message.Contains(ltrSii.YaExisteUnaEntradaConSellerId))
                        {
                            contexto.Commit(tran);
                        }
                        else
                        {
                            contexto.Rollback(tran);
                            throw new Exception($"Se ha cancelado el envió a la AEAT desde la factura '{factura.Referencia}', se han comunicado '{comunicadas} de {lote.Count}'", e);
                        }
                    }
                    finally
                    {
                        contexto.CerrarTraza();
                        SemaforoDeProcesoSql.QuitarSemaforo(entorno.ContextoDelEntorno, idSemaforo);
                    }
                }
            }
            catch
            {
                if (idSemaforoDeSociedad > 0)
                {
                    var valor = sociedad.SII_DesactivarVerifactuEnLaSociedad(contexto);
                    enumNegocio.FacturaEmitida.ResetearParametro(contexto, enumParametrosDeFacturasEmt.SII_SSII_IMPLANTACION, valor);
                }
                throw;
            }
            finally
            {
                if (idSemaforoDeSociedad > 0)
                {
                    SemaforoDeProcesoSql.QuitarSemaforo(entorno.ContextoDelEntorno, idSemaforoDeSociedad);
                    enumNegocio.FacturaEmitida.ResetearParametro(contexto, enumParametrosDeFacturasEmt.FAE_SII_Activo, "S");
                }
            }
        }

        public static void EnvioDeFacturaAeat(EntornoDeTrabajo entorno)
        {
            var contexto = entorno.contextoDelProceso;
            var parametros = entorno.TrabajoDeUsuario.Parametros.ToDiccionarioDeParametros();
            var idFactura = (int)parametros.LeerValor<long>(nameof(FacturaEmtDtm.Id));
            var factura = contexto.SeleccionarPorId<FacturaEmtDtm>(idFactura);
            contexto.IniciarTraza(nameof(EnvioDeFacturaAeat));

            var someterImpresion = false;
            var idSemaforo = SemaforoDeProcesoSql.PonerSemaforo(entorno.ContextoDelEntorno, enumNegocio.FacturaEmitida.IdNegocio(), idFactura, enumOpercionesDeSemaforo.ASII, factura.Referencia).Id;
            var tran = contexto.IniciarTransaccion();
            try
            {
                try
                {
                    var generadorSii = ValidacionesQueExigeLaAeat(contexto, factura);
                    generadorSii.AltaDeFacturaSif(esUnLote: false);
                }
                catch (Exception e)
                {
                    if (e.GetType() == typeof(FaultException))
                    {
                        SometerCancelarEmision(entorno.ContextoDelEntorno, idFactura, motivo: $"Los servidores de la AEAT indican: {((FaultException)e).Fault.faultstring}");
                    }
                    else
                    if (!e.Message.Contains(ltrSii.NoSePuedeReenviar))
                    {
                        SometerCancelarEmision(entorno.ContextoDelEntorno, idFactura, motivo: GestorDeErrores.Detalle(e));
                    }
                    throw;
                }
                finally
                {
                    SemaforoDeProcesoSql.QuitarSemaforo(entorno.ContextoDelEntorno, idSemaforo);
                }
                var log = contexto.SeleccionarPorPropiedad<LogDeEnvioDeFacturaDtm>(nameof(LogDeEnvioDeFacturaDtm.IdFactura), factura.Id);
                log.EnviadaEl = DateTime.Now;
                log.Modificar(contexto);
                contexto.Commit(tran);
                someterImpresion = true;
            }
            catch (Exception e)
            {
                contexto.Rollback(tran);
                entorno.AnotarError(e);
            }
            finally
            {
                contexto.CerrarTraza();
            }

            if (someterImpresion)
            {
                var tu = SometerGenerarFacturaPdf(entorno.ContextoDelEntorno, idFactura);
                var nuevo = ContextoSe.Crear(entorno.ContextoDelEntorno);
                Task.Run(() => tu.EjecutarTrabajo(nuevo));
            }
        }

        public static TrabajoDeUsuarioDtm SometerFirmarFactura(ContextoSe contexto, int idFactura)
        {
            var factura = contexto.SeleccionarPorId<FacturaEmtDtm>(idFactura);

            ValidacionesQueExigeLaAeat(contexto, factura);

            var dll = Assembly.GetExecutingAssembly().GetName().Name;
            var clase = typeof(TrabajosDeFacturasEmt).FullName;
            var ts = GestorDeTrabajosSometido.CrearObtener(contexto, enumTrabajosDeFacturasEmt.FirmarFactura.Descripcion(), dll, clase, nameof(enumTrabajosDeFacturasEmt.FirmarFactura), comunicarFin: true);
            var parametrosEntrada = new Dictionary<string, object> { { nameof(FacturaEmtDtm.Id), idFactura } };
            var datosDeCreacion = new Dictionary<string, object>
                {
                    { nameof(TrabajoDeUsuarioDtm.Parametros), parametrosEntrada.ToJson() },
                    { nameof(TrabajoDeUsuarioDtm.Planificado), DateTime.Now.AddMinutes(-1) }
                };
            return GestorDeTrabajosDeUsuario.CrearSiNoEstaPendiente(contexto, ts, datosDeCreacion);
        }

        public static void FirmarFactura(EntornoDeTrabajo entorno)
        {
            var contexto = entorno.contextoDelProceso;
            Dictionary<string, object> parametros = entorno.TrabajoDeUsuario.Parametros.ToDiccionarioDeParametros();
            var idFactura = (int)parametros.LeerValor<long>(nameof(FacturaEmtDtm.Id));
            var factura = contexto.SeleccionarPorId<FacturaEmtDtm>(idFactura);
            contexto.IniciarTraza(nameof(FirmarFactura));

            var tran = contexto.IniciarTransaccion();
            try
            {
                factura.Firma = factura.Firmar(contexto);
                factura.ModificarComoAdministrador(contexto, accionQueSeEjecuta: nameof(FirmarFactura));
                contexto.Commit(tran);
            }
            catch (Exception e)
            {
                contexto.Rollback(tran);
                entorno.AnotarError(e);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        public static void SometerCancelarEmision(ContextoSe contexto, int idFactura, string motivo)
        {
            var factura = contexto.SeleccionarPorId<FacturaEmtDtm>(idFactura);

            var dll = Assembly.GetExecutingAssembly().GetName().Name;
            var clase = typeof(TrabajosDeFacturasEmt).FullName;
            var ts = GestorDeTrabajosSometido.CrearObtener(contexto, enumTrabajosDeFacturasEmt.CancelarEmision.Descripcion(), dll, clase, nameof(enumTrabajosDeFacturasEmt.CancelarEmision), comunicarFin: true);
            var parametrosEntrada = new Dictionary<string, object> { { nameof(FacturaEmtDtm.Id), idFactura }, { ltrParametrosEp.detalleAsunto, motivo.Left(1000) } };
            var datosDeCreacion = new Dictionary<string, object>
                {
                    { nameof(TrabajoDeUsuarioDtm.Parametros), parametrosEntrada.ToJson() },
                    { nameof(TrabajoDeUsuarioDtm.Planificado), DateTime.Now.AddMinutes(-1) }
                };
            var tu = GestorDeTrabajosDeUsuario.CrearSiNoEstaPendiente(contexto, ts, datosDeCreacion);
            var nuevo = ContextoSe.Crear(contexto);
            Task.Run(() => tu.EjecutarTrabajo(nuevo));
        }

        public static void CancelarEmision(EntornoDeTrabajo entorno)
        {
            var contexto = entorno.contextoDelProceso;
            Dictionary<string, object> parametros = entorno.TrabajoDeUsuario.Parametros.ToDiccionarioDeParametros();
            var idFactura = (int)parametros.LeerValor<long>(nameof(FacturaEmtDtm.Id));
            var factura = contexto.SeleccionarPorId<FacturaEmtDtm>(idFactura);
            var detalle = parametros.LeerValor<string>(ltrParametrosEp.detalleAsunto);
            contexto.IniciarTraza(nameof(CancelarEmision));

            var tran = contexto.IniciarTransaccion();
            try
            {
                var verifactu = factura.Verifactu(contexto, errorSiNoHay: false);
                if (verifactu is not null)
                {
                    entorno.AnotarError($"Factura '{factura.Referencia}', ha sido enviada a la AEAT, no se puede cancelar la emisión", detalle);
                }
                else
                {

                    factura.AnularVerifactu(contexto);
                    factura.Devolver(contexto, parametros: new Dictionary<string, object> {
                                  { ltrParametrosEp.asunto , "Fallo al comunicar la factura a la AEAT"},
                                  { ltrParametrosEp.detalleAsunto, detalle } ,
                    },
                    errorSiMasDeUnaTransicion: false);
                }

                contexto.Commit(tran);
            }
            catch (Exception e)
            {
                contexto.Rollback(tran);
                entorno.AnotarError(e);
            }
            finally
            {
                contexto.CerrarTraza();
            }
        }

        public static TrabajoDeUsuarioDtm SometerGenerarFacturaPdf(ContextoSe contexto, int idFactura)
        {
            var factura = contexto.SeleccionarPorId<FacturaEmtDtm>(idFactura);

            var dll = Assembly.GetExecutingAssembly().GetName().Name;
            var clase = typeof(TrabajosDeFacturasEmt).FullName;
            var ts = GestorDeTrabajosSometido.CrearObtener(contexto, enumTrabajosDeFacturasEmt.GenerarFacturaPdf.Descripcion(), dll, clase, nameof(enumTrabajosDeFacturasEmt.GenerarFacturaPdf), comunicarFin: true);
            var parametrosEntrada = new Dictionary<string, object> { { nameof(FacturaEmtDtm.Id), idFactura } };
            var datosDeCreacion = new Dictionary<string, object>
                {
                    { nameof(TrabajoDeUsuarioDtm.Parametros), parametrosEntrada.ToJson() },
                    { nameof(TrabajoDeUsuarioDtm.Planificado), DateTime.Now.AddMinutes(-1) }
                };
            return GestorDeTrabajosDeUsuario.CrearSiNoEstaPendiente(contexto, ts, datosDeCreacion);
        }

        public static void GenerarFacturaPdf(EntornoDeTrabajo entorno)
        {
            var contexto = entorno.contextoDelProceso;
            Dictionary<string, object> parametros = entorno.TrabajoDeUsuario.Parametros.ToDiccionarioDeParametros();
            var idFactura = (int)parametros.LeerValor<long>(nameof(FacturaEmtDtm.Id));
            var factura = contexto.SeleccionarPorId<FacturaEmtDtm>(idFactura);
            try
            {
                contexto.IniciarTraza(nameof(GenerarFacturaPdf));
                var tran = contexto.IniciarTransaccion();
                try
                {
                    GestorDeFacturasEmt.EmitirPdfFactura(contexto, factura.MapearDto<FacturaEmtDto>(contexto));
                    contexto.Commit(tran);
                }
                catch (Exception e)
                {
                    contexto.Rollback(tran);
                    entorno.AnotarError(e);
                    throw;
                }
                finally
                {
                    contexto.CerrarTraza();
                }
            }
            catch (Exception e)
            {
                var peticion = contexto.SeleccionarPorPropiedad<PeticionDeFacturaEmtDtm>(nameof(PeticionDeFacturaEmtDtm.IdFactura), factura.Id, errorSiNoHay: false);
                peticion?.RegistrarError(contexto, ltrFacturador.ErrorAlImprimir + Environment.NewLine + e.MensajeCompleto());
            }
        }

        private static GeneradorSii ValidacionesQueExigeLaAeat(ContextoSe contexto, FacturaEmtDtm factura)
        {
            var cliente = factura.Cliente(contexto);
            var df = cliente.DireccionFiscal(contexto);
            if (df == null)
                GestorDeErrores.Emitir($"Por tener activo el Verifactu, la AEAT exige que el cliente '{factura.Cliente(contexto).NIF(contexto)}' tenga una dirección fiscal, dela de alta");

            var generadorSii = new GeneradorSii(contexto, factura);
            //generadorSii.ValidarConexionConLaAeat();
            if (df.IntraComunitaria)
            {
                if (!cliente.VAT.IsNullOrEmpty())
                {
                    generadorSii.ValidarVat();
                    var ivas = factura.Ivas(contexto);
                    foreach (var iva in ivas)
                    {
                        if (!iva.EsIsp)
                            GestorDeErrores.Emitir($"El Iva '{contexto.SeleccionarPorId<IvaRepercutidoDtm>(iva.IdIva).Detalle}' de la factura '{factura.Referencia}' ha de ser '{enumClasesDeIvaRep.ISP.Descripcion()}' ya que el cliente es intracomunitario y está dado de alta en el ROI");
                    }
                }
            }
            else if (df.EsNacional)
                generadorSii.ValidarNif();


            return generadorSii;
        }

        public static TrabajoDeUsuarioDtm SometerActualizacionDeFacturas(ContextoSe contexto)
        {
            var dll = Assembly.GetExecutingAssembly().GetName().Name;
            var clase = typeof(TrabajosDeFacturasEmt).FullName;
            var ts = GestorDeTrabajosSometido.CrearObtener(contexto, enumTrabajosDeFacturasEmt.ActualizacionDeFacturas.Descripcion(), dll, clase, nameof(enumTrabajosDeFacturasEmt.ActualizacionDeFacturas), comunicarFin: false);

            var parametrosEntrada = new Dictionary<string, object> { };

            var datosDeCreacion = new Dictionary<string, object>
            {
                { nameof(TrabajoDeUsuarioDtm.Parametros), parametrosEntrada.ToJson() },
                { nameof(TrabajoDeUsuarioDtm.Planificado), DateTime.Now.AddDays(1).Date.AddMinutes(1) },
                { nameof(TrabajoDeUsuarioDtm.Periodicidad), 86400}
            };
            return GestorDeTrabajosDeUsuario.CrearSiNoEstaPendiente(contexto, ts, datosDeCreacion);
        }

        public static void ActualizacionDeFacturas(EntornoDeTrabajo entorno)
        {
            TransitarFacturasEnReclamacion(entorno);
            TransitarLasRectificativasAbonadas(entorno);
        }

        private static void TransitarLasRectificativasAbonadas(EntornoDeTrabajo entorno)
        {
            var contexto = entorno.contextoDelProceso;
            var rectificativas = contexto.SeleccionarTodos<FacturaEmtDtm>(new List<ClausulaDeFiltrado> { new ClausulaDeFiltrado {
                      Clausula = ltrFiltros.FiltroPorEtapa,
                      Criterio = enumCriteriosDeFiltrado.igual,
                      Valor = enumEtapasDeFacturasEmt.FAE_Etapa_Emitida.ToString()}
                }, negocio: enumNegocio.FacturaEmitida);

            foreach (var rectificativa in rectificativas)
            {
                if (!rectificativa.EsRectificativa)
                    continue;

                if (rectificativa.EstaAbonada(contexto, errorSiNoEsRectificativa: true) && rectificativa.EstaEnLaEtapa(enumEtapasDeFacturasEmt.FAE_Etapa_Emitida))
                {
                    var tran = contexto.IniciarTransaccion();
                    try
                    {
                        rectificativa.TransitarALaEtapa(contexto, enumEtapasDeFacturasEmt.FAE_Etapa_Abonada.EstadosDeLaEtapa());
                        entorno.CrearTraza($"Rectificativa '{rectificativa.Referencia}' dada por abonada");
                        contexto.Commit(tran);
                    }
                    catch (Exception ex)
                    {
                        contexto.Rollback(tran);
                        entorno.AnotarError(ex);
                    }
                }
            }
        }

        private static void TransitarFacturasEnReclamacion(EntornoDeTrabajo entorno)
        {
            var contexto = entorno.contextoDelProceso;
            var facturas = contexto.SeleccionarTodos<FacturaEmtDtm>(new List<ClausulaDeFiltrado> { new ClausulaDeFiltrado {
                      Clausula = ltrFiltros.FiltroPorEtapa,
                      Criterio = enumCriteriosDeFiltrado.igual,
                      Valor = enumEtapasDeFacturasEmt.FAE_Etapa_De_Reclamacion.ToString()}
                }, negocio: enumNegocio.FacturaEmitida);

            foreach (var factura in facturas)
            {
                if (factura.EsRectificativa)
                    continue;

                if (factura.EstaCobrada(contexto))
                {
                    var tran = contexto.IniciarTransaccion();
                    try
                    {
                        if (factura.EstaRectificada(contexto))
                        {
                            factura.TransitarALaEtapa(contexto, enumEtapasDeFacturasEmt.FAE_Etapa_Rectificada.EstadosDeLaEtapa(), new Dictionary<string, object> {
                                {
                                    ltrParametrosNeg.AccionQueSeEjecuta, VariableDeFacturasEmt.enumMotivoTransicion.RectificarFactura.Descripcion()
                                }
                            });
                            entorno.CrearTraza($"Factura '{factura.Referencia}' transitada a rectificada");
                        }
                        else
                        {
                            factura.TransitarALaEtapa(contexto, enumEtapasDeFacturasEmt.FAE_Etapa_Cobrada.EstadosDeLaEtapa());
                            entorno.CrearTraza($"Factura '{factura.Referencia}' transitada a cobrada");
                        }

                        contexto.Commit(tran);
                    }
                    catch (Exception ex)
                    {
                        contexto.Rollback(tran);
                        entorno.AnotarError(ex);
                    }
                }
            }
        }
    }
}
