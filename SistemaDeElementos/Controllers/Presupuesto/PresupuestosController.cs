using ServicioDeDatos;
using Gestor.Errores;
using Microsoft.AspNetCore.Mvc;
using MVCSistemaDeElementos.Descriptores;
using GestoresDeNegocio.Presupuesto;
using ModeloDeDto.Presupuesto;
using ServicioDeDatos.Presupuesto;
using System;
using Utilidades;
using System.Collections.Generic;
using GestorDeElementos;
using ServicioDeDatos.Seguridad;
using Inicializador.Presupuestos;
using GestoresDeNegocio.Ventas;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.SistemaDocumental;
using GestoresDeNegocio.Entorno;
using ServicioDeReportes.Ventas;
using System.IO;
using QuestPDF.Fluent;
using ModeloDeDto;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Ventas;
using static ServicioDeDatos.Elemento.Enumerados;
using ServicioDeDatos.MaestrosTecnico;
using ServicioDeDatos.Negocio;
using ModeloDeDto.Ventas;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace MVCSistemaDeElementos.Controllers
{
    public class PresupuestosController : EntidadController<ContextoSe, PresupuestoDtm, PresupuestoDto>
    {
        public PresupuestosController(GestorDePresupuestos gestorDePresupuestos, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDePresupuestos,
           gestorDeErrores
         )
        {
        }

        public IActionResult CrudPresupuestos()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);

            var modo = ModoDescriptor.Mantenimiento;
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{modo}-{typeof(DescriptorDePresupuestos).FullName}";
            var cache = ServicioDeCaches.Obtener(CacheDe.RenderCrud);
            try
            {

                if (cache.ContainsKey(indice))
                {
                    ViewBag.DatosDeConexion = DatosDeConexion;
                    var destino = $"../{enumNameSpaceTs.Presupuesto}/{nameof(CrudPresupuestos)}";
                    return base.View(destino, new DescriptorDePresupuestos(Contexto, (string)cache[indice]));
                }
                else
                {
                    var descriptor = DescriptorDeCrud<PresupuestoDto>.CrearDescriptor(Contexto, modo, () => new DescriptorDePresupuestos(Contexto, modo));
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
            indicadores.Add(IndPresupuesto.UnidadDeMedida, enumNegocio.Presupuesto.Parametro(enumParametrosDePresupuesto.PPT_Unidad_Medida, crearParametro: true, valorPorDefecto: Literal.Cero).Valor.Entero());
            indicadores.Add(IndPresupuesto.Naturaleza, enumNegocio.Presupuesto.Parametro(enumParametrosDePresupuesto.PPT_Naturaleza, crearParametro:true, valorPorDefecto: Literal.Cero).Valor.Entero());
            indicadores.Add(IndPresupuesto.TipoDeLinea, enumNegocio.Presupuesto.Parametro(enumParametrosDePresupuesto.PPT_TipoDeLinea, crearParametro:true, valorPorDefecto: enumTipoDeLinea.Alzada.ToString()).Valor);
            indicadores.Add(IndPresupuesto.ClaseDeUnitario, enumNegocio.Presupuesto.Parametro(enumParametrosDePresupuesto.PPT_ClaseDeUnitario, crearParametro:true, valorPorDefecto: enumClaseUnitario.Servicio.ToString()).Valor);
            return indicadores;
        }

        protected override int Vincular(int idNegocio, int idVinculado, int idElemento1, SelectorDto elemento2, Dictionary<string, object> parametros)
        {
            
            if (NegociosDeSe.ToEnumerado(idNegocio) == enumNegocio.Expediente && NegociosDeSe.ToEnumerado(idVinculado) == enumNegocio.Presupuesto)
            {
                var ppt = Contexto.SeleccionarPorId<PresupuestoDtm>(elemento2.IdElemento);
                if (ppt.IdExpediente.Entero() > 0)
                    GestorDeErrores.Emitir($"El presupuesto '{ppt.Referencia}' seleccionado ya está asociado al expediente '{Contexto.SeleccionarPorId<ExpedienteDtm>((int)ppt.IdExpediente).Referencia}'");

                ppt.IdExpediente = idElemento1;
                ppt.Modificar(Contexto, accionEjecutada: ltrDeUnPresupuesto.AsociarExpediente);
                return ppt.Id;
            }

            return base.Vincular(idNegocio, idVinculado, idElemento1, elemento2, parametros);
        }
        public JsonResult epAsociarUnPpt(string parametrosJson)
        {
            var r = new Resultado();
            Dictionary<string, object> parametros = parametrosJson.ToDiccionarioDeParametros();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                if (!parametros.ContieneClave(nameof(PresupuestoDto.Id))) throw new Exception("Debe indicar el presupuesto");
                if (!parametros.ContieneClave(nameof(PresupuestoDto.idExpediente))) throw new Exception("Debe indicar el expediente al que asociar");
                
                var idPresupuesto = (int)parametros.LeerValor<long>(nameof(PresupuestoDto.Id));
                var idExpediente = (int)parametros.LeerValor<long>(nameof(PresupuestoDto.idExpediente));

                var ppt = Contexto.SeleccionarPorId<PresupuestoDtm>(idPresupuesto);
                if (ppt.IdExpediente.Entero() > 0 && (int)ppt.IdExpediente == idExpediente)
                    GestorDeErrores.Emitir("El expediente a asociar al presupuesto es el que ya tiene");

                ppt.IdExpediente = idExpediente;
                ppt.Modificar(Contexto);

                r.Mensaje = $"presupuesto asociado al expediente";
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "Error al asociar el presupuesto al expediente.");
            }
            return new JsonResult(r);
        }

        public JsonResult epRenombarUnPpt(string parametrosJson)
        {
            var r = new Resultado();
            Dictionary<string, object> parametros = parametrosJson.ToDiccionarioDeParametros();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                if (!parametros.ContieneClave(nameof(RenombrarPptDto.IdElemento))) throw new Exception("Debe indicar el presupuesto");
                if (!parametros.ContieneClave(nameof(RenombrarPptDto.Nombre))) throw new Exception("Debe indicar el nuevo nombre");

                var idPresupuesto = (int)parametros.LeerValor<long>(nameof(RenombrarPptDto.IdElemento));
                var nombre = parametros.LeerValor<string>(nameof(RenombrarPptDto.Nombre));
                var ppt = Contexto.SeleccionarPorId<PresupuestoDtm>(idPresupuesto);
                ppt.Renombrar(Contexto, nombre);

                r.Mensaje = $"presupuesto renombrado";
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "Error al renombrar el presupuesto.");
            }
            return new JsonResult(r);
        }

        public JsonResult epGenerarUnPpt(string parametrosJson)
        {
            var r = new Resultado();
            Dictionary<string, object> parametros = parametrosJson.ToDiccionarioDeParametros();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                r.Datos = GestorDePresupuestos.CopiarPpt(Contexto, parametros);
                r.Mensaje = $"presupuesto copiado";
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "Error al copiar el presupuesto.");
            }
            return new JsonResult(r);
        }

        public JsonResult epCrearDireccion(string parametrosJson)
        {
            var r = new Resultado();
            Dictionary<string, object> parametros = parametrosJson.ToDiccionarioDeParametros();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                r.Datos = "";
                r.Mensaje = $"Dirección creada";
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "Error al copiar el presupuesto.");
            }
            return new JsonResult(r);
        }


        protected override dynamic ProcesarOpcionMf(enumNegocio negocio, string opcion, Dictionary<string, object> parametros)
        {
            switch (opcion)
            {
                case eventosDeMf.Ppt_AsociarUnExpedienteAUnPpt:
                    return null;
                case eventosDeMf.Ppt_Renombrar:
                    return null;
                case eventosDeMf.Ppt_CopiarPpt:
                    return null;
                case eventosDeMf.Ppt_IrATareas:
                    return null;
                case eventosDeMf.Totalizador_Mostrar:
                    return null;
                case eventosDeMf.Ppt_IrAPartesTr:
                    ExtensorDePresupuestos.ValidarQueLosPptsEstanEnEtapaDe(Contexto, (List<int>)parametros[ltrParametrosEp.ids], ServicioDeDatos.Ventas.enumEtapasDePpts.PPT_Etapa_AsociarParteTr);
                    return null;
                case eventosDeMf.Ppt_IrAFacturasEmt:
                    ExtensorDePresupuestos.ValidarQueLosPptsEstanEnEtapaDe(Contexto, (List<int>)parametros[ltrParametrosEp.ids], ServicioDeDatos.Ventas.enumEtapasDePpts.PPT_Etapa_PermiteFacturar);
                    return null;
                case eventosDeMf.Ppt_ModalDeImprimir:
                    ImprimirPresupuesto((List<int>)parametros[ltrParametrosEp.ids]);
                    return new ServicioDePlantillas(Contexto, enumNegocio.Presupuesto).Plantillas();
            }
            return base.ProcesarOpcionMf(negocio, opcion, parametros);
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
                    GestorDeErrores.Emitir("Esta opción es para desasociar un expediente de un presupuesto");
                var ppt = Contexto.SeleccionarPorId<PresupuestoDtm>(id);
                parametros[ltrParametrosNeg.EsUnaPeticion] = true;
                parametros[ltrParametrosNeg.Peticion] = enumPeticion.epModificarPorId;
                ppt.IdExpediente = null;
                ppt.Modificar(Contexto);
                r.Total = 1;
                r.Consola = $"presupuesto '{ppt.Referencia}' desasociado";
                r.Estado = enumEstadoPeticion.Ok;
                _GestorDeElementos.Commit(tran);
            }
            catch (Exception e)
            {
                _GestorDeElementos.Rollback(tran);
                ApiController.PrepararError(e, r, "Error en el proceso de desasociar presupuesto del expediente.");
            }

            return new JsonResult(r);
        }

        public IActionResult MaestrosDePresupuestos()
        {
            var r = new Resultado();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);

                if (!Contexto.SePuedeParametrizar())
                    GestorDeErrores.Emitir("Esta opción sólo se permite a parametrizadores");

                InzPresupuestos.ModeloDePresupuestos(Contexto);
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

        public IActionResult SometerTrabajos()
        {
            var r = new Resultado();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);

                if (!Contexto.DatosDeConexion.EsAdministrador)
                    GestorDeErrores.Emitir("Esta opción sólo se permite a administradores");

                TrabajosDePlfsDeVenta.SometerGenerarPlanificacionesDeVenta(Contexto);
                TrabajosDeFacturasEmt.SometerVencerFacturasImpagadas(Contexto);
                TrabajosDeRemesasFae.SometerProcesosDeRemesasFae(Contexto);
                ViewBag.Mensaje = "Trabajos sometidos";
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                return RenderMensaje($"No se ha podido someter los trabajos de ventas.{Environment.NewLine}{GestorDeErrores.Detalle(e)}");
            }
            return VistaDelPanelDeControl(Contexto);
        }

        private void ImprimirPresupuesto(List<int> idsDePpt)
        {
            foreach (var id in idsDePpt)
            {
                var ppt = Contexto.SeleccionarPorId<PresupuestoDtm>(id);

                var rutaConFichero = Path.Combine(GestorDeVariables.RutaDeDescarga, $"Ppt-{ppt.Referencia}.pdf".NormalizarFichero());
                var presupuestoRpt = new GeneradorDePresupuestoRpt(Contexto, ppt).ObtenerInformacionDeRpt(plantilla: null);
                new ReporteDePresupuesto(presupuestoRpt).GeneratePdf(rutaConFichero);
                var idArchivo = ServidorDocumental.SubirArchivo(Contexto, rutaConFichero, sanitizar: false);
                GestorDeVinculos.Vincular(Contexto, enumNegocio.Presupuesto, enumNegocio.Archivos, ppt.Id, idArchivo);
            }
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
                var totales = await ((ITotalizador<TotalesDePresupuestos>)_GestorDeElementos).ObtenerTotalesAsync(filtros, posicion, cantidad);
                r.Consola = $"Totalización realizada";
                r.Datos = totales;
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "Error al totalizar los presupuestos.");
            }
            finally
            {
                Contexto.CerrarTraza();
            }
            return new JsonResult(r);
        }

    }
}
