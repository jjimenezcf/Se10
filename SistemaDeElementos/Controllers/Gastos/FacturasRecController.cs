using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Entorno;
using GestoresDeNegocio.Gastos;
using GestoresDeNegocio.SistemaDocumental;
using GestoresDeNegocio.Terceros;
using Inicializador.Gastos;
using Microsoft.AspNetCore.Mvc;
using ModeloDeDto;
using ModeloDeDto.Gastos;
using MVCSistemaDeElementos.Descriptores;
using Newtonsoft.Json;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Gastos;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.Seguridad;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Utilidades;

namespace MVCSistemaDeElementos.Controllers
{
    public class FacturasRecController : EntidadController<ContextoSe, FacturaRecDtm, FacturaRecDto>
    {
        public IPdfServerClient PdfServerClient { get; }

        public FacturasRecController(GestorDeFacturasRec gestorDeFacturasRec, GestorDeErrores gestorDeErrores, IPdfServerClient pdfServerClient)
         : base
         (
           gestorDeFacturasRec,
           gestorDeErrores
         )
        {
            PdfServerClient = pdfServerClient;
        }

        public IActionResult CrudFacturasRec()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);

            var modo = ModoDescriptor.Mantenimiento;
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{modo}-{typeof(DescriptorDeFacturasRec).FullName}";
            var cache = ServicioDeCaches.Obtener(CacheDe.RenderCrud);
            try
            {
                if (cache.ContainsKey(indice))
                {
                    ViewBag.DatosDeConexion = DatosDeConexion;
                    var destino = $"../{enumNameSpaceTs.Gasto}/{nameof(CrudFacturasRec)}";
                    return base.View(destino, new DescriptorDeFacturasRec(Contexto, (string)cache[indice]));
                }
                else
                {
                    var descriptor = DescriptorDeCrud<FacturaRecDto>.CrearDescriptor(Contexto, modo, () => new DescriptorDeFacturasRec(Contexto, modo));
                    return ViewCrud(descriptor);
                }
            }
            catch (Exception e)
            {
                return RenderizarErrorDe(indice, e);
            }
        }

        protected override Dictionary<string, object> IndicadoresParaInicializarLaVistaMnt(ContextoSe contexto, Dictionary<string, object> parametros)
        {
            var indicadores = base.IndicadoresParaInicializarLaVistaMnt(contexto, parametros);
            indicadores.Add(IndFacturaRec.UnidadDeMedida, enumParametrosDeFacturasRec.FAR_Unidad_Medida.Entero(errorSiNoDefinido: false));
            indicadores.Add(IndFacturaRec.Naturaleza, enumParametrosDeFacturasRec.FAR_Naturaleza.Entero(errorSiNoDefinido: false));
            indicadores.Add(IndFacturaRec.ComoTratarLaFechaDeRecepcion, enumParametrosDeFacturasRec.FAR_Como_Tratar_La_Fecha_De_Recepcion.Cadena(errorSiNoDefinido: false, valorPorDefecto: enumValoresDeComoTratarRecibidoEl.mismafecha.ToString()));

            return indicadores;
        }

        protected override dynamic ProcesarOpcionMf(enumNegocio negocio, string opcion, Dictionary<string, object> parametros)
        {
            switch (opcion)
            {
                case eventosDeMf.Far_ImportarXml:
                    return null;
                case eventosDeMf.Far_ImportarPrv:
                    return null;
                case eventosDeMf.Far_CrearFarConIa:
                    return null;
                case eventosDeMf.Far_Renombrar:
                    return null;
                case eventosDeMf.Far_CambiarProveedor:
                    return null;
                case eventosDeMf.Totalizador_Mostrar:
                    return null;
                case eventosDeMf.Far_CopiarFar:
                    return null;
                case eventosDeMf.Far_RectificarFar:
                    return null;
                case eventosDeMf.Far_IrAExpediente:
                    enumNegocio.FacturaRecibida.ValidarTieneRefearenciaA(Contexto, (List<int>)parametros[ltrParametrosEp.ids], nameof(FacturaRecDtm.IdExpediente));
                    return null;
                case eventosDeMf.Far_IrAContrato:
                    enumNegocio.FacturaRecibida.ValidarTieneRefearenciaA(Contexto, (List<int>)parametros[ltrParametrosEp.ids], nameof(FacturaRecDtm.IdContrato));
                    return null;
                case eventosDeMf.Far_IrAPago:
                    var pagos = Contexto.SeleccionarTodos<PagoDtm>(nameof(PagoDtm.IdFacturaRec), ((List<int>)parametros[ltrParametrosEp.ids])[0], parametros: new Dictionary<string, object> { { nameof(ltrParametrosNeg.ExcluirTerminados), false } });
                    if (pagos.Count == 0)
                        GestorDeErrores.Emitir($"la factura seleccionada no tiene pagos realizados");
                    return null;
                case eventosDeMf.Far_QuitarContrato:
                    ((GestorDeFacturasRec)_GestorDeElementos).QuitarContrato((List<int>)parametros[ltrParametrosEp.ids]);
                    return null;
                case eventosDeMf.Far_QuitarExpediente:
                    ((GestorDeFacturasRec)_GestorDeElementos).QuitarExpediente((List<int>)parametros[ltrParametrosEp.ids]);
                    return null;
                case eventosDeMf.Far_GenerarPreasiento:
                    ((GestorDeFacturasRec)_GestorDeElementos).GenerarPreasiento((List<int>)parametros[ltrParametrosEp.ids]);
                    return null;
                case eventosDeMf.Far_CancelarPreasientos:
                    var ids = parametros.LeerValor<List<int>>(ltrParametrosEp.ids);
                    var cancelados = ((GestorDeFacturasRec)_GestorDeElementos).CancelarPreasientos(ids);
                    var r = new Resultado();
                    r.Mensaje = $"Se han cancelado '{cancelados}' preasientos";
                    return r;
                case eventosDeMf.Far_ModalDeImprimir:
                    ImprimirFactura((List<int>)parametros[ltrParametrosEp.ids]);
                    return new ServicioDePlantillas(Contexto, enumNegocio.FacturaRecibida).Plantillas();
            }
            return base.ProcesarOpcionMf(negocio, opcion, parametros);
        }


        protected override int Vincular(int idNegocio, int idVinculado, int idElemento1, SelectorDto elemento2, Dictionary<string, object> parametros)
        {
            if (NegociosDeSe.ToEnumerado(idNegocio) == enumNegocio.Expediente && NegociosDeSe.ToEnumerado(idVinculado) == enumNegocio.FacturaRecibida)
            {
                ((GestorDeFacturasRec)_GestorDeElementos).ImputarExpediente(elemento2.IdElemento, idElemento1);
                return elemento2.IdElemento;
            }

            else if (NegociosDeSe.ToEnumerado(idNegocio) == enumNegocio.Contrato && NegociosDeSe.ToEnumerado(idVinculado) == enumNegocio.FacturaRecibida)
            {
                ((GestorDeFacturasRec)_GestorDeElementos).ImputarContrato(elemento2.IdElemento, idElemento1);
                return elemento2.IdElemento;
            }

            return base.Vincular(idNegocio, idVinculado, idElemento1, elemento2, parametros);
        }

        public IActionResult MaestrosDeFacturasRec()
        {
            var r = new Resultado();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);

                if (!Contexto.SePuedeParametrizar())
                    GestorDeErrores.Emitir("Esta opción sólo se permite a parametrizadores");

                InzFacturasRec.ModeloDeFacturasRec(Contexto);
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

        public JsonResult epCrearFarConIa(string parametrosJson)
        {
            var r = new Resultado();
            Dictionary<string, object> parametros = parametrosJson.ToDiccionarioDeParametros();
            var tran = Contexto.IniciarTransaccion();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);

                var idArchivo = (int)parametros.LeerValor<long>(nameof(CrearFarConIaDto.IdArchivo));
                var idTipo = (int)parametros.LeerValor<long>(nameof(CrearFarConIaDto.IdTipoFarPropuesto));
                var idCg = (int)parametros.LeerValor<long>(nameof(CrearFarConIaDto.IdCgPropuesto));
                var idProveedor = (int?)parametros.LeerValor<long?>(nameof(CrearFarConIaDto.IdProveedor), valorPorDefecto: (long?)null);

                if (idArchivo == 0 || idCg == 0 || idTipo == 0)
                    GestorDeErrores.Emitir("Debe indicar el archivo, el centro gestor y el tipo de factura");
                ((GestorDeFacturasRec)_GestorDeElementos).CrearFarConIa(idCg, idTipo, idArchivo, idProveedor);
                r.Consola = $"Trabajo de creación de facturas sometido correctamente.";
                r.Datos = null;
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
                Contexto.Commit(tran);
            }
            catch (Exception e)
            {
                Contexto.Rollback(tran);
                ApiController.PrepararError(e, r, "Error al someter el trabajo para crear facturas con Ia.");
            }
            return new JsonResult(r);
        }

        public JsonResult epImportarPrvDesdeXml(string parametrosJson)
        {
            var r = new Resultado();
            Dictionary<string, object> parametros = parametrosJson.ToDiccionarioDeParametros();
            var tran = Contexto.IniciarTransaccion();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);

                var idArchivo = (int)parametros.LeerValor<long>(nameof(ImportarPrvXml.IdArchivo));
                var idMunicipio = parametros.LeerValor(nameof(ImportarPrvXml.IdMunicipio), (int?)null);
                var idTipoDeVida = parametros.LeerValor(nameof(ImportarPrvXml.IdTipoDeVia), (int?)null);
                var idTipo = parametros.LeerValor(nameof(ImportarPrvXml.IdTipoFarPropuesto), (int?)null);
                var idCg = parametros.LeerValor(nameof(ImportarPrvXml.IdCgPropuesto), (int?)null);
                var email = parametros.LeerValor(nameof(ImportarPrvXml.eMail), (string)null);
                var telefono = parametros.LeerValor(nameof(ImportarPrvXml.Telefono), (string)null);

                var facturaRec = GestorDeProveedores.CrearProveedor(Contexto, idArchivo, telefono, email, idMunicipio, idTipoDeVida);

                r.Consola = $"proveedor importado correctamente.";
                r.Datos = facturaRec;
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
                Contexto.Commit(tran);
            }
            catch (Exception e)
            {
                Contexto.Rollback(tran);
                ApiController.PrepararError(e, r, "Error al importar el proveedor.");
            }
            return new JsonResult(r);
        }

        public JsonResult epImportarFarDesdeXml(string parametrosJson)
        {
            var r = new Resultado();
            Dictionary<string, object> parametros = parametrosJson.ToDiccionarioDeParametros();
            var tran = Contexto.IniciarTransaccion();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                if (!parametros.ContieneClave(nameof(ImportarFarXml.IdProveedor))) throw new Exception("Debe indicar el id del proveedor de la factura");
                if (!parametros.ContieneClave(nameof(ImportarFarXml.IdArchivo))) throw new Exception("Debe indicar el id del archivo xml a importar");
                if (!parametros.ContieneClave(nameof(ImportarFarXml.IdCgPropuesto))) throw new Exception("Debe indicar el id del cg de la factura");
                if (!parametros.ContieneClave(nameof(ImportarFarXml.IdTipoFarPropuesto))) throw new Exception("Debe indicar el id de la factura");

                var idTipo = (int)parametros.LeerValor<long>(nameof(ImportarFarXml.IdTipoFarPropuesto));
                var idCg = (int)parametros.LeerValor<long>(nameof(ImportarFarXml.IdCgPropuesto));
                var idArchivo = (int)parametros.LeerValor<long>(nameof(ImportarFarXml.IdArchivo));
                var idProveedor = (int)parametros.LeerValor<long>(nameof(ImportarFarXml.IdProveedor));

                var facturaRec = GestorDeFacturasRec.ImportarFarDesdeXml(Contexto, idCg, idTipo, idProveedor, idArchivo);

                r.Consola = $"Factura creada correctamente";
                r.Datos = facturaRec;
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
                Contexto.Commit(tran);
            }
            catch (Exception e)
            {
                Contexto.Rollback(tran);
                ApiController.PrepararError(e, r, "Error al crear la factura.");
            }
            return new JsonResult(r);
        }

        public JsonResult epGenerarUnaFar(string parametrosJson)
        {
            var r = new Resultado();
            Dictionary<string, object> parametros = parametrosJson.ToDiccionarioDeParametros();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                r.Datos = GestorDeFacturasRec.CopiarFar(Contexto, parametros);
                r.Mensaje = $"Factura copiada";
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "Error al copiar la factura.");
            }
            return new JsonResult(r);
        }

        public JsonResult epRectificarUnaFar(string parametrosJson)
        {
            var r = new Resultado();
            Dictionary<string, object> parametros = parametrosJson.ToDiccionarioDeParametros();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                r.Datos = GestorDeFacturasRec.RectificarFar(Contexto, parametros);
                r.Mensaje = $"Factura rectificada";
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "Error al rectificar la factura.");
            }
            return new JsonResult(r);
        }

        public JsonResult epCambiarProveedor(string parametrosJson)
        {
            var r = new Resultado();
            Dictionary<string, object> parametros = parametrosJson.ToDiccionarioDeParametros();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                if (!parametros.ContieneClave(nameof(CambiarProveedorDto.IdElemento))) throw new Exception("Debe indicar la factura a la que cambiar el proveedor");
                if (!parametros.ContieneClave(nameof(CambiarProveedorDto.IdProveedor))) throw new Exception("Debe indicar el nuevo proveedor");

                var idFactura = (int)parametros.LeerValor<long>(nameof(CambiarProveedorDto.IdElemento));
                var idProveedor = (int)parametros.LeerValor<long>(nameof(CambiarProveedorDto.IdProveedor));
                var far = Contexto.SeleccionarPorId<FacturaRecDtm>(idFactura);
                far.CambiarProveedor(Contexto, idProveedor);

                r.Mensaje = $"proveedor cambiado";
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "Error al cambiar el proveedor.");
            }
            return new JsonResult(r);
        }

        public JsonResult epRenombarUnaFar(string parametrosJson)
        {
            var r = new Resultado();
            Dictionary<string, object> parametros = parametrosJson.ToDiccionarioDeParametros();
            var tran = Contexto.IniciarTransaccion();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                if (!parametros.ContieneClave(nameof(RenombrarFarDto.IdElemento))) throw new Exception("Debe indicar la factura a la que cambiar el asunto");
                if (!parametros.ContieneClave(nameof(RenombrarFarDto.Nombre))) throw new Exception("Debe indicar el nuevo asunto");

                var idFactura = (int)parametros.LeerValor<long>(nameof(RenombrarFarDto.IdElemento));
                var far = Contexto.SeleccionarPorId<FacturaRecDtm>(idFactura);

                if (far.EsRectificativa)
                    GestorDeErrores.Emitir($"No se le puede cambiar el proveedor a una factura rectificativa");

                var asunto = parametros.LeerValor<string>(nameof(RenombrarFarDto.Nombre));
                if (!asunto.IsNullOrEmpty())
                    far.ModificarAsunto(Contexto, asunto);

                var idNaturalezaAnterior = parametros.LeerValor<int?>(nameof(RenombrarFarDto.IdNaturalezaAnterior));
                var idNaturalezaNueva = parametros.LeerValor<int?>(nameof(RenombrarFarDto.IdNaturalezaNueva));
                var regenerarPreasiento = far.ModificarNaturaleza(Contexto, idNaturalezaAnterior.Entero(), idNaturalezaNueva.Entero());

                var idIvaAnterior = parametros.LeerValor<int?>(nameof(RenombrarFarDto.IdIvaSoportadoAnterior));
                var idIvaNuevo = parametros.LeerValor<int?>(nameof(RenombrarFarDto.IdIvaSoportadoNuevo));
                var modificaciones = far.ModificarIva(Contexto, idIvaAnterior.Entero(), idIvaNuevo.Entero());

                if (regenerarPreasiento || modificaciones.regenerarPreasiento)
                {
                    far.Preasentar(Contexto);
                    far.ModificarComoAdministrador(Contexto, accionQueSeEjecuta: ltrDeUnaFacturaRec.Accion_GenerarPreasiento);
                }

                if (modificaciones.modificarPago)
                {
                    far.PagosRealizados(Contexto)[0].ModificarImporte(Contexto, far.TotalDelPago);
                }

                r.Mensaje = $"factura cambiada";
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;

                Contexto.Commit(tran);
            }
            catch (Exception e)
            {
                Contexto.Rollback(tran);
                ApiController.PrepararError(e, r, "Error al cambiar los datos de la factura.");
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
                var totales = await ((ITotalizador<TotalesDeFacturasRec>)_GestorDeElementos).ObtenerTotalesAsync(filtros, posicion, cantidad);
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

        public JsonResult epBorrarRelacionPorId(int id, string parametrosJson)
        {
            var r = new Resultado();
            var tran = _GestorDeElementos.IniciarTransaccion();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, _GestorDeElementos.Mapeador, HttpContext);
                var parametros = Utilidades.extJson.ToDiccionarioDeParametros(parametrosJson);
                var idNegocio = (int)parametros.LeerValor<long>(ltrParametrosEp.idNegocio);
                if (NegociosDeSe.ToEnumerado(idNegocio) != enumNegocio.Expediente)
                    GestorDeErrores.Emitir("Esta opción es para desasociar un expediente de un factura");
                var far = Contexto.SeleccionarPorId<FacturaRecDtm>(id);
                parametros[ltrParametrosNeg.EsUnaPeticion] = true;
                parametros[ltrParametrosNeg.Peticion] = enumPeticion.epModificarPorId;
                if (far.IdExpediente == null)
                {
                    if (far.IdContrato == null)
                        GestorDeErrores.Emitir($"la factura '{far.Referencia}' no está asociada a ningún expediente");
                    else
                    {
                        var contrato = Contexto.SeleccionarPorId<ContratoDtm>(far.IdContrato.Value);
                        GestorDeErrores.Emitir($"la factura '{far.Referencia}' no está asociada directamente al expediente, lo está al contrato '{contrato.Referencia}'");
                    }
                }
                far.IdExpediente = null;
                far.Modificar(Contexto, parametros);
                r.Total = 1;
                r.Consola = $"factura '{far.Referencia}' desasociada";
                r.Estado = enumEstadoPeticion.Ok;
                _GestorDeElementos.Commit(tran);
            }
            catch (Exception e)
            {
                _GestorDeElementos.Rollback(tran);
                ApiController.PrepararError(e, r, "Error en el proceso de desasociar factura del expediente.");
            }

            return new JsonResult(r);
        }

        protected override ElementoDeUnProcesoDto DespuesDeTransitar(TransicionDtm transicion, enumNegocio negocio, ElementoDeUnProcesoDto factura, Dictionary<string, object> parametros)
        {
            factura = base.DespuesDeTransitar(transicion, negocio, factura, parametros);
            return factura;
        }

        private void ImprimirFactura(List<int> idsDeFars)
        {
            foreach (var id in idsDeFars)
            {
                var far = Contexto.SeleccionarPorId<FacturaRecDtm>(id);
                GestorDeFacturasRec.Imprimir(far, Contexto);
            }
        }

        protected override void ErrorAlTransitar(ContextoSe contexto, Exception excepcion, Resultado resultado, ElementoDeUnProcesoDto facturaDto)
        {
            base.ErrorAlTransitar(contexto, excepcion, resultado, facturaDto);
            var factura = (FacturaRecDto)facturaDto;
            if (factura != null)
            {
                var rutaConFichero = Path.Combine(GestorDeVariables.RutaDeDescarga, $"{factura.Referencia}.pdf".NormalizarFichero());
                ServidorDocumental.EliminarFichero(Contexto, rutaConFichero);
            }
        }

        protected override IEnumerable<FacturaRecDto> LeerElementos(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden, Dictionary<string, object> parametros)
        {
            var facturas = base.LeerElementos(posicion, cantidad, filtros, orden, parametros).ToList();
            if (parametros.LeerValor(ltrParametrosEp.filtrarPara, "").ToLower() == ltrFiltros.CargarGridDeRelacion.ToLower())
            {
                var filtro = filtros.FirstOrDefault(f => f.Clausula.ToLower() == ltrParametrosNeg.IdNegocio.ToLower());
                if (filtro is not null && NegociosDeSe.LeerNegocioPorId(filtro.Valor.Entero()).ToEnumerado() == enumNegocio.Expediente)
                {
                    var expediente = Contexto.SeleccionarPorId<ExpedienteDtm>(filtros.First(f => f.Clausula.ToLower() == ltrDeUnaFacturaRec.IdExpediente.ToLower()).Valor.Entero());
                    expediente.IncluirLasFacturasRecDeSusContratos(Contexto, facturas);
                }
            }
            return facturas;
        }
    }
}
