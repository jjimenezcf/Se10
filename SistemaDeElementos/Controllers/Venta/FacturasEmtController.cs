using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Entorno;
using GestoresDeNegocio.SistemaDocumental;
using GestoresDeNegocio.Venta.Factura;
using GestoresDeNegocio.Ventas;
using Inicializador.Ventas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModeloDeDto;
using ModeloDeDto.Ventas;
using MVCSistemaDeElementos.Descriptores;
using Newtonsoft.Json;
using QuestPDF.Fluent;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Gastos;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Ventas;
using ServicioDeReportes.Ventas;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Utilidades;
using static Gestor.Errores.GestorDeErrores;

namespace MVCSistemaDeElementos.Controllers
{
    public class FacturasEmtController : EntidadController<ContextoSe, FacturaEmtDtm, FacturaEmtDto>
    {
        public IPdfServerClient PdfServerClient { get; }

        public FacturasEmtController(GestorDeFacturasEmt gestorDeFacturasEmt, GestorDeErrores gestorDeErrores, IPdfServerClient pdfServerClient)
         : base
         (
           gestorDeFacturasEmt,
           gestorDeErrores
         )
        {
            PdfServerClient = pdfServerClient;
        }

        protected override Dictionary<string, object> IndicadoresParaInicializarLaVistaMnt(ContextoSe contexto, Dictionary<string, object> parametros)
        {
            var indicadores = base.IndicadoresParaInicializarLaVistaMnt(contexto, parametros);
            indicadores.Add(IndFacturaEmt.UsaVerifactu, enumNegocio.FacturaEmitida.Parametro(enumParametrosDeFacturasEmt.FAE_SII_Activo, crearParametro: true, valorPorDefecto: "N").Valor.EsTrue());
            return indicadores;
        }


        [AllowAnonymous]
        public IActionResult epValidarQr(string nif, string numserie, string fecha, string importe)
        {
            ViewBag.Nif = nif;
            ViewBag.NumSerie = numserie;
            ViewBag.Fecha = fecha;
            ViewBag.Importe = importe;
            ViewBag.DatosDeConexion = DatosDeConexion;
            ViewBag.AccesoAnonimo = true;
            try
            {
                ViewBag.Mensaje = ((GestorDeFacturasEmt)_GestorDeElementos).ValidarFactura(nif, numserie, fecha, importe);
                return View($"../{enumNameSpaceTs.Venta}/FacturaValidada");
            }
            catch (Exception ex)
            {
                ViewBag.Mensaje = ex.MensajeCompleto();
                return View($"../{enumNameSpaceTs.Venta}/FacturaNOValidada");
            }
        }

        public IActionResult CrudFacturasEmt()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);

            var modo = ModoDescriptor.Mantenimiento;
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{modo}-{typeof(DescriptorDeFacturasEmt).FullName}";
            var cache = ServicioDeCaches.Obtener(CacheDe.RenderCrud);
            try
            {
                if (cache.ContainsKey(indice))
                {
                    ViewBag.DatosDeConexion = DatosDeConexion;
                    var destino = $"../{enumNameSpaceTs.Venta}/{nameof(CrudFacturasEmt)}";
                    return base.View(destino, new DescriptorDeFacturasEmt(Contexto, (string)cache[indice]));
                }
                else
                {
                    var descriptor = DescriptorDeCrud<FacturaEmtDto>.CrearDescriptor(Contexto, modo, () => new DescriptorDeFacturasEmt(Contexto, modo));
                    return ViewCrud(descriptor);
                }
            }
            catch (Exception e)
            {
                return RenderizarErrorDe(indice, e);
            }
        }

        public IActionResult CrudFacturasAeat()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);

            var modo = ModoDescriptor.Consulta;
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{modo}-{typeof(DescriptorFacturasAeat).FullName}";
            var cache = ServicioDeCaches.Obtener(CacheDe.RenderCrud);
            try
            {
                if (cache.ContainsKey(indice))
                {
                    ViewBag.DatosDeConexion = DatosDeConexion;
                    var destino = $"../{enumNameSpaceTs.Venta}/{nameof(CrudFacturasAeat)}";
                    return base.View(destino, new DescriptorFacturasAeat(Contexto, (string)cache[indice]));
                }
                else
                {
                    var descriptor = DescriptorDeCrud<FacturaAeatDto>.CrearDescriptor(Contexto, modo, () => new DescriptorFacturasAeat(Contexto, modo));
                    return ViewCrud(descriptor);
                }
            }
            catch (Exception e)
            {
                return RenderizarErrorDe(indice, e);
            }
        }

        protected override (IEnumerable<IElementoDto> elementos, int total, dynamic infoObtenida)
        LeerOtrosElementos(int pos, int can, string filtro, string orden, string seleccionadas, Dictionary<string, object> parametros)
        {
            if (parametros.LeerValor(ltrParametrosEp.Vista, "") == enumVistasVentas.CrudFacturasAeat)
            {
                var facturasAeat = new List<FacturaAeatDto>();
                List<ClausulaDeFiltrado> filtros = filtro == null ? new List<ClausulaDeFiltrado>() : JsonConvert.DeserializeObject<List<ClausulaDeFiltrado>>(filtro);
                var mensaje = "";
                if (filtros.Count() > 0)
                {
                    var porSociedad = filtros.FirstOrDefault(f => f.Clausula.ToLower() == nameof(ltrDeSociedad.FiltroParaSociedadesGestionadas).ToLower());
                    if (porSociedad != null && porSociedad.Valor.Entero() > 0)
                    {
                        var ano = filtros.FirstOrDefault(f => f.Clausula.ToLower() == ltrDeUnaFacturaEmt.AnoDeEmisison.ToLower());
                        var mes = filtros.FirstOrDefault(f => f.Clausula.ToLower() == ltrDeUnaFacturaEmt.MesDeEmision.ToLower());

                        if (ano is null || ano.Valor.Entero() == 0 || mes is null || mes.Valor.Entero() == 0)
                            Emitir("Debe indicar el año y el mes para consultar las facturas AEAT de una sociedad.");

                        var sociedad = Contexto.SeleccionarPorId<SociedadDtm>(porSociedad.Valor.Entero());
                        var generadorSii = new GeneradorSii(Contexto, sociedad);
                        facturasAeat = generadorSii.ConsultaDeFacturas(ano.Valor.Entero(), mes.Valor.Entero());
                        if (facturasAeat.Count == 0) mensaje = $"No hay facturas registradas de la empresa '{sociedad.NIF}' para el priodo '{ano.Valor}-{mes.Valor}'";
                    }
                }
                var registrosPaginados = facturasAeat
                            .Skip(pos >= 0 ? pos : 0)
                            .Take(can > 0 ? can : 0)
                            .ToList();

                var resultado = new ResultadoDeLectura<FacturaAeatDto>
                {
                    registros = registrosPaginados,
                    posicion = pos,
                    cantidad = can,
                    total = facturasAeat.Count,
                    Mensaje = mensaje
                };
                return (elementos: facturasAeat, facturasAeat.Count, resultado);
            }
            throw new NotImplementedException($"Este método no está implementado para obtener los datos de la vista '{parametros.LeerValor(ltrParametrosEp.Vista, "")}'");
        }

        protected override dynamic ProcesarOpcionMf(enumNegocio negocio, string opcion, Dictionary<string, object> parametros)
        {
            switch (opcion)
            {
                case eventosDeMf.Fae_IrAPartesTr:
                    ExtensorDeFacturasEmt.ValidarTieneParte(Contexto, (List<int>)parametros[ltrParametrosEp.ids]);
                    return null;
                case eventosDeMf.Fae_IrAPpts:
                    enumNegocio.FacturaEmitida.ValidarTieneRefearenciaA(Contexto, (List<int>)parametros[ltrParametrosEp.ids], nameof(FacturaEmtDtm.IdPresupuesto));
                    return null;
                case eventosDeMf.Fae_IrAContrato:
                    enumNegocio.FacturaEmitida.ValidarTieneRefearenciaA(Contexto, (List<int>)parametros[ltrParametrosEp.ids], nameof(FacturaEmtDtm.IdContrato));
                    return null;
                case eventosDeMf.Fae_CambiarDatos:
                    return null;
                case eventosDeMf.Fae_CambiarVencimiento:
                    return null;
                case eventosDeMf.Fae_FacturarTareas:
                    return null;
                case eventosDeMf.Fae_CopiarFae:
                    return null;
                case eventosDeMf.Fae_Rectificativa:
                    return null;
                case eventosDeMf.Totalizador_Mostrar:
                    return null;
                case eventosDeMf.Fae_GenerarPreasiento:
                    try
                    {
                        ((GestorDeFacturasEmt)_GestorDeElementos).GenerarPreasiento((List<int>)parametros[ltrParametrosEp.ids]);
                    }
                    catch (Exception exc)
                    {
                        if (exc.Data != null && exc.Data.Contains(Datos.CodigoError) && (enumCodigoDeError)exc.Data[Datos.CodigoError] == enumCodigoDeError.MensajeInformativo)
                            return new Resultado { Estado = enumEstadoPeticion.Ok, Mensaje = exc.Message };
                        throw;
                    }
                    return null;
                case eventosDeMf.Fae_GenerarUbl:
                    Resultado resultado = GenerarUbl(parametros);
                    return resultado;
                case eventosDeMf.Fae_CopiarLa:
                    var ids = (List<int>)parametros[ltrParametrosEp.ids];
                    if (ids.Count != 1)
                        GestorDeErrores.Emitir("Debe indicar la factura a copiar");
                    return GestorDeFacturasEmt.CopiarLa(Contexto, ids[0]);
                case eventosDeMf.Fae_ModalDeImprimir:
                    var plantillas = new ServicioDePlantillas(Contexto, enumNegocio.FacturaEmitida, (List<int>)parametros[ltrParametrosEp.ids]).Plantillas();
                    if (!plantillas.Abrir)
                        ImprimirPrefacturas((List<int>)parametros[ltrParametrosEp.ids]);
                    return plantillas;
                case eventosDeMf.Fae_SincronizarConAeat:
                    ids = (List<int>)parametros[ltrParametrosEp.ids];
                    if (ids.Count != 1) GestorDeErrores.Emitir("Solo ha de indicar el id de una factura");
                    ((GestorDeFacturasEmt)_GestorDeElementos).SincronizarConDatosDeLaAeat(Contexto, ids[0]);
                    return new Resultado { Mensaje = "Factura sincronizada con los datos de la AEAT" };
            }
            return base.ProcesarOpcionMf(negocio, opcion, parametros);
        }

        private Resultado GenerarUbl(Dictionary<string, object> parametros)
        {
            try
            {
                ((GestorDeFacturasEmt)_GestorDeElementos).GenerarUbl(parametros);
            }
            catch (Exception exc)
            {
                if (exc.Data != null && exc.Data.Contains(Datos.CodigoError) && (enumCodigoDeError)exc.Data[Datos.CodigoError] == enumCodigoDeError.MensajeInformativo)
                    return new Resultado { Estado = enumEstadoPeticion.Ok, Mensaje = exc.Message };
                throw;
            }
            return null;
        }

        public IActionResult MaestrosDeFacturasEmt()
        {
            var r = new Resultado();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);

                if (!Contexto.SePuedeParametrizar())
                    GestorDeErrores.Emitir("Esta opción sólo se permite a parametrizadores");

                InzFacturasEmt.ModeloDeFacturasEmt(Contexto);
                ViewBag.Mensaje = "Maestros inicializados";
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                return RenderMensaje($"No se ha podido inicializar los maestros.{Environment.NewLine}{GestorDeErrores.Detalle(e)}");
            }
            finally
            {
                ServicioDeCaches.EliminarTodas();
            }
            return VistaDelPanelDeControl(Contexto);
        }

        protected override ElementoDeUnProcesoDto DespuesDeTransitar(TransicionDtm transicion, enumNegocio negocio, ElementoDeUnProcesoDto factura, Dictionary<string, object> parametros)
        {
            try
            {
                factura = base.DespuesDeTransitar(transicion, negocio, factura, parametros);
                if (transicion.EntreEtapas(enumEtapasDeFacturasEmt.FAE_Etapa_Prefactura.Estados(), enumEtapasDeFacturasEmt.FAE_Etapa_Emitida.Estados()))
                {
                    var facturaDtm = Contexto.SeleccionarPorId<FacturaEmtDtm>(factura.Id);
                    if (facturaDtm.UsaVerifactu(Contexto) && GeneradorSii.VerifactuActivo(Contexto, facturaDtm))
                    {
                        GestorDeFacturasEmt.EnviarFacturaAeat(Contexto, factura.Id, someterEnvio: false);
                    }
                    else
                    {
                        GestorDeFacturasEmt.EmitirPdfFactura(Contexto, (FacturaEmtDto)factura);
                    }
                }
                return factura;
            }
            catch
            {
                throw;
            }
            finally
            {
            }
        }


        protected override bool Imprimir(int idNegocio, Dictionary<string, object> parametros)
        {
            if (base.Imprimir(idNegocio, parametros))
                return true;

            List<int> idFacturasEmt = new List<int> { (int)parametros.LeerValor<long>(ltrParametrosEp.idElemento) };
            var plantilla = parametros.LeerValor<string>(ltrParametrosEp.Plantilla);

            if (!ExtensorDeEnum.Existe<enumPltFacturaEmtRpt>(plantilla))
                return false;

            var enumPlantilla = ExtensorDeEnum.Enumerado<enumPltFacturaEmtRpt>(plantilla);

            foreach (var id in idFacturasEmt)
            {
                if (enumPlantilla == enumPltFacturaEmtRpt.Prefactura)
                    ImprimirPrefactura(id);
                else if (enumPlantilla == enumPltFacturaEmtRpt.CopiaDeFactura)
                    ImprimirCopiaFactura(id);
                else
                    GestorDeErrores.Emitir($"No se ha definido como imprimir la plantilla '{plantilla}' para la factura");
            }

            return true;
        }

        private void ImprimirPrefacturas(List<int> idsDeFaes)
        {
            foreach (var id in idsDeFaes)
            {
                ImprimirPrefactura(id);
            }
        }

        private void ImprimirPrefactura(int idFacturaEmt)
        {
            var fae = Contexto.SeleccionarPorId<FacturaEmtDtm>(idFacturaEmt);
            if (!fae.EstaEnLaEtapa(enumEtapasDeFacturasEmt.FAE_Etapa_Prefactura))
            {
                ImprimirCopiaFactura(idFacturaEmt);
            }
            else
            {
                var nombrePropuesto = fae.ProponerNombreDeArchivo(Contexto, $"Pre-{fae.Referencia}.pdf".NormalizarFichero());
                var rutaConFichero = Path.Combine(GestorDeVariables.RutaDeDescarga, nombrePropuesto);
                var facturaEmtRpt = new GeneradorDeFacturaEmtRpt(Contexto, fae).ObtenerInformacionDeRpt(plantilla: null);
                new ReporteDeFacturaEmt(facturaEmtRpt).GeneratePdf(rutaConFichero);
                var idArchivo = ServidorDocumental.SubirArchivo(Contexto, rutaConFichero, sanitizar: false);
                GestorDeVinculos.Vincular(Contexto, enumNegocio.FacturaEmitida, enumNegocio.Archivos, fae.Id, idArchivo);
            }
        }

        private void ImprimirCopiaFactura(int idFacturaEmt)
        {
            var fae = Contexto.SeleccionarPorId<FacturaEmtDtm>(idFacturaEmt);
            if (fae.EstaEnLaEtapa(enumEtapasDeFacturasEmt.FAE_Etapa_Prefactura))
                GestorDeErrores.Emitir($"No se puede emitir una copia de la factura '{fae.Referencia}' por ser prefactura");
            var nombrePropuesto = fae.ProponerNombreDeArchivo(Contexto, $"Copia-{fae.Referencia}.pdf".NormalizarFichero());
            var rutaConFichero = Path.Combine(GestorDeVariables.RutaDeDescarga, nombrePropuesto);
            var facturaEmtRpt = (FacturaEmtRpt)new GeneradorDeFacturaEmtRpt(Contexto, fae).ObtenerInformacionDeRpt(plantilla: null);
            new ReporteDeFacturaEmt(facturaEmtRpt).GeneratePdf(rutaConFichero);
            var idArchivo = ServidorDocumental.SubirArchivo(Contexto, rutaConFichero, sanitizar: false);
            GestorDeVinculos.Vincular(Contexto, enumNegocio.FacturaEmitida, enumNegocio.Archivos, fae.Id, idArchivo);
        }

        private void ImprimirAnexoDeTareas(int idFacturaEmt)
        {
            var fae = Contexto.SeleccionarPorId<FacturaEmtDtm>(idFacturaEmt);

            var nombrePropuesto = fae.ProponerNombreDeArchivo(Contexto, $"Anexo-{fae.Referencia}.pdf".NormalizarFichero());
            var rutaConFichero = Path.Combine(GestorDeVariables.RutaDeDescarga, nombrePropuesto);
            var facturaEmtRpt = new GeneradorDeFacturaEmtRpt(Contexto, fae).ObtenerInformacionDeRpt(plantilla: null);
            new ReporteDeFacturaEmt(facturaEmtRpt).GeneratePdf(rutaConFichero);
            var idArchivo = ServidorDocumental.SubirArchivo(Contexto, rutaConFichero, sanitizar: false);
            GestorDeVinculos.Vincular(Contexto, enumNegocio.FacturaEmitida, enumNegocio.Archivos, fae.Id, idArchivo);
        }

        protected override void ErrorAlTransitar(ContextoSe contexto, Exception excepcion, Resultado resultado, ElementoDeUnProcesoDto facturaDto)
        {
            base.ErrorAlTransitar(contexto, excepcion, resultado, facturaDto);
            var factura = (FacturaEmtDto)facturaDto;
            if (factura != null)
            {
                var rutaConFichero = Path.Combine(GestorDeVariables.RutaDeDescarga, $"{factura.Referencia}.pdf".NormalizarFichero());
                ServidorDocumental.EliminarFichero(Contexto, rutaConFichero);
            }
        }

        public JsonResult epCambiarVencimiento(string parametrosJson)
        {
            var r = new Resultado();
            Dictionary<string, object> parametros = parametrosJson.ToDiccionarioDeParametros();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                if (!parametros.ContieneClave(nameof(CambiarVencimientoDto.IdElemento))) throw new Exception("Debe indicar la factura");
                if (!parametros.ContieneClave(nameof(CambiarVencimientoDto.NuevoVencimiento))) throw new Exception("Debe indicar la fecha de nuevo vencimiento");

                var idFactura = (int)parametros.LeerValor<long>(nameof(CambiarVencimientoDto.IdElemento));
                var nuevoVencimiento = parametros.LeerValor<DateTime>(nameof(CambiarVencimientoDto.NuevoVencimiento));

                GestorDeFacturasEmt.CambiarVencimiento(Contexto, idFactura, nuevoVencimiento);

                r.Consola = $"Vencimiento cambiado correctamente";
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "No se puede cambiar el vencimiento.");
            }
            return new JsonResult(r);
        }


        public JsonResult epCambiarDatos(string parametrosJson)
        {
            var r = new Resultado();
            Dictionary<string, object> parametros = parametrosJson.ToDiccionarioDeParametros();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                if (!parametros.ContieneClave(nameof(CambiarDatosFae.IdElemento))) throw new Exception("Debe indicar la factura");
                var idFactura = (int)parametros.LeerValor<long>(nameof(CambiarDatosFae.IdElemento));
                var datos = new CambiarDatosFae
                {
                    IdCliente = (int)parametros.LeerValor<long>(nameof(CambiarDatosFae.IdCliente)),
                    IdContrato = (int?)parametros.LeerValor<long?>(nameof(CambiarDatosFae.IdContrato), valorPorDefecto: null),
                    IdPresupuesto = (int?)parametros.LeerValor<long?>(nameof(CambiarDatosFae.IdPresupuesto), valorPorDefecto: null),
                    Presupuesto = parametros.LeerValor<string>(nameof(CambiarDatosFae.Presupuesto), valorPorDefecto: null),
                };
                GestorDeFacturasEmt.CambiarDatos(Contexto, idFactura, datos);

                r.Consola = $"Datos cambiado correctamente";
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "No se puede cambiar los datos.");
            }
            return new JsonResult(r);
        }

        public JsonResult epFacturarTareas(string parametrosJson)
        {
            var r = new Resultado();
            Dictionary<string, object> parametros = parametrosJson.ToDiccionarioDeParametros();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                if (!parametros.ContieneClave(nameof(CambiarVencimientoDto.IdElemento))) throw new Exception("Debe indicar la factura");

                var idFactura = (int)parametros.LeerValor<long>(nameof(CambiarVencimientoDto.IdElemento));

                GestorDeFacturasEmt.FacturarTareas(Contexto, idFactura, parametros);

                r.Consola = $"tareas facturadas correctamente";
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "No se han podido facturar las tareas.");
            }
            return new JsonResult(r);
        }

        public JsonResult epCopiarUnaFae(string parametrosJson)
        {
            var r = new Resultado();
            Dictionary<string, object> parametros = parametrosJson.ToDiccionarioDeParametros();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                r.Datos = GestorDeFacturasEmt.CopiarFae(Contexto, parametros);
                r.Consola = $"factura creada";
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "Error al copiar la factura.");
            }
            return new JsonResult(r);
        }


        public JsonResult epHacerRectificativa(string parametrosJson)
        {
            var r = new Resultado();
            Dictionary<string, object> parametros = parametrosJson.ToDiccionarioDeParametros();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                r.Datos = GestorDeFacturasEmt.CrearRectificativa(Contexto, parametros);
                r.Consola = $"factura rectificativa creada";
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "Error al crear la factura rectificativa.");
            }
            return new JsonResult(r);
        }



        [HttpPost]
        public async Task<JsonResult> epTotales(int posicion, int cantidad)
        {
            var body = ApiController.LeerBody(HttpContext);
            var filtro = body.parametros.LeerValor<string>(ltrParametrosEp.Filtro);
            var r = new Resultado();
            Contexto.IniciarTraza(GetType().Name + "_" + nameof(epTotales));
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                List<ClausulaDeFiltrado> filtros = filtro == null ? new List<ClausulaDeFiltrado>() : JsonConvert.DeserializeObject<List<ClausulaDeFiltrado>>(filtro);
                Contexto.AnotarParametros(new List<object> { filtro, posicion, cantidad });
                var totales = await ((ITotalizador<TotalesDeFacturasEmt>)_GestorDeElementos).ObtenerTotalesAsync(filtros, posicion, cantidad);
                r.Consola = $"Totalización realizada";
                r.Datos = totales;
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "Error al totalizar las facturas.");
            }
            finally
            {
                Contexto.CerrarTraza();
            }
            return new JsonResult(r);
        }

        public ActionResult epDescargarDeclaracionResponsable()
        {
            Contexto.IniciarTraza(nameof(epDescargarDeclaracionResponsable));
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(_GestorDeElementos.Contexto, _GestorDeElementos.Mapeador, HttpContext);
                return DescargarArchivo(VariableDeFacturasEmt.DeclaracíonResponsable(), auditarDescarga: false, errorSiNoEsta: true);
            }
            catch (Exception ex)
            {
                Contexto.AnotarExcepcion(ex);
                return DevolverPaginaWebConMensaje(ex.Message);
            }
            finally
            {

                Contexto.CerrarTraza();
            }
        }


        public JsonResult epValidarDeclaracionResponsable()
        {
            var r = new Resultado();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                var idArchivo = VariableDeFacturasEmt.DeclaracíonResponsable();
                if (!Contexto.Existe<ArchivoDtm>(idArchivo))
                    throw Excepciones.Emitir($"El archivo de declaración responsable indicado en el parámetro '{enumParametrosDeFacturasEmt.FAE_SII_Declaracion_Responsable}' no existe en la BD");
                r.Consola = $"Archivo de declaración responsable validado";
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "Declaración responsable no localizada");
            }
            return new JsonResult(r);
        }

        public JsonResult epIrARectificadaPor(string parametrosJson)
        {
            var r = new Resultado();

            Dictionary<string, object> parametros = parametrosJson.ToDiccionarioDeParametros();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);

                var idRegistro = (int)parametros.LeerValor<long>(ltrParametrosEp.id);
                var factura = Contexto.SeleccionarPorId<FacturaEmtDtm>(idRegistro);
                var rectificadaPor = factura.RectificadaPor(Contexto, errorSiNoHay: true);

                r.Datos = $"{enumNegocio.FacturaEmitida.Controlador()}/{enumNegocio.FacturaEmitida.VistaMvc(Contexto).Accion}?Id={rectificadaPor.Id}";
                r.Consola = $"factura que me rectifica obtenida correctamente";
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "Error al obtener la factura que 'me' rectifica.");
            }
            return new JsonResult(r);
        }


        public JsonResult epIrARectificoA(string parametrosJson)
        {
            var r = new Resultado();

            Dictionary<string, object> parametros = parametrosJson.ToDiccionarioDeParametros();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);

                var idRegistro = (int)parametros.LeerValor<long>(ltrParametrosEp.id);
                var factura = Contexto.SeleccionarPorId<FacturaEmtDtm>(idRegistro);
                var rectificoA = factura.RectificaA(Contexto, errorSiNoHay: true);

                r.Datos = $"{enumNegocio.FacturaEmitida.Controlador()}/{enumNegocio.FacturaEmitida.VistaMvc(Contexto).Accion}?Id={rectificoA.Id}";
                r.Consola = $"factura que me rectifica obtenida correctamente";
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "Error al obtener la factura a la que rectifico.");
            }
            return new JsonResult(r);
        }

        protected override IEnumerable<FacturaEmtDto> LeerElementos(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden, Dictionary<string, object> parametros)
        {
            var facturas = base.LeerElementos(posicion, cantidad, filtros, orden, parametros).ToList();
            if (parametros.LeerValor(ltrParametrosEp.filtrarPara, "").ToLower() == ltrFiltros.CargarGridDeRelacion.ToLower())
            {
                var filtro = filtros.FirstOrDefault(f => f.Clausula.ToLower() == ltrParametrosNeg.IdNegocio.ToLower());
                if (filtro is not null && NegociosDeSe.LeerNegocioPorId(filtro.Valor.Entero()).ToEnumerado() == enumNegocio.Expediente)
                {
                    var expediente = Contexto.SeleccionarPorId<ExpedienteDtm>(filtros.First(f => f.Clausula.ToLower() == ltrDeUnaFacturaEmt.IdExpediente.ToLower()).Valor.Entero());
                    expediente.IncluirLasFacturasEmtDeSusPresupuestos(Contexto, facturas);
                    expediente.IncluirLasFacturasEmtDeSusContratos(Contexto, facturas);
                }
            }
            return facturas;
        }


    }
}
