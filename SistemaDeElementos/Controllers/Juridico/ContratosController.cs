using ServicioDeDatos;
using Gestor.Errores;
using Microsoft.AspNetCore.Mvc;
using MVCSistemaDeElementos.Descriptores;
using GestoresDeNegocio.Juridico;
using ModeloDeDto.Juridico;
using ServicioDeDatos.Juridico;
using System;
using Utilidades;
using System.Collections.Generic;
using GestorDeElementos;
using ServicioDeDatos.Seguridad;
using Inicializador.ContratosVnt;
using GestorDeElementos.Extensores;
using Newtonsoft.Json;
using Inicializador.ContratosCmp;
using Newtonsoft.Json.Linq;

namespace MVCSistemaDeElementos.Controllers
{
    public class ContratosController : EntidadController<ContextoSe, ContratoDtm, ContratoDto>
    {
        public ContratosController(GestorDeContratos gestorDeContratos, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeContratos,
           gestorDeErrores
         )
        {
        }

        public IActionResult CrudContratosDeCompras() => CrudContratos(enumClaseDeContrato.Compra.ToString());
        public IActionResult CrudContratosDeVenta() => CrudContratos(enumClaseDeContrato.Venta.ToString());

        public IActionResult CrudContratos(string clase)
        {
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);
                if (clase.IsNullOrEmpty())
                {
                    if (HttpContext.Request.Query.ContainsKey(ltrParametrosEp.id))
                    {
                        var idContrato = HttpContext.Request.Query[ltrParametrosEp.id].ToString().Entero();
                        var contrato = Contexto.SeleccionarPorId<ContratoDtm>(idContrato, errorSiNoHay: false);
                        if (contrato == null)
                            throw new Exception("El id de contrato no existe en el sistema");

                        clase = contrato.ClaseDeContrato.ToString();
                    }
                    else throw new Exception("Debe indicar la clase de contrato");
                }

                var claseDeContrato = ApiDeEnsamblados.ToEnumerado<enumClaseDeContrato>(clase);
                return ViewCrud(new DescriptorDeContratos(Contexto, ModoDescriptor.Mantenimiento, claseDeContrato));
            }
            catch (Exception e)
            {
                return RenderMensaje(e.Message);
            }
        }

        public JsonResult epImputar(int idNegocio, string propiedadId, int idDondeImputar, string idsJson)
        {
            var r = new Resultado();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                List<int> listaIds = JsonConvert.DeserializeObject<List<int>>(idsJson);
                var imputados = 0;
                var mensajeInformativo = "";
                var parametros = new Dictionary<string, object>();
                parametros[ltrParametrosNeg.Peticion] = enumPeticion.epImputar;
                var gestor = NegociosDeSe.ToEnumerado(idNegocio).CrearGestor(Contexto);
                foreach (var id in listaIds)
                {
                    var imputado = ((IEsImputable)gestor).Imputar(id, enumNegocio.Contrato, idDondeImputar);
                    if (imputado.EstabaSinImputar)
                        imputados++;
                    else
                        mensajeInformativo = mensajeInformativo + Environment.NewLine + imputado.Mensaje;
                }
                r.Total = imputados;
                r.Consola = $"Se han inputado {imputados} de los {listaIds.Count} marcados" +
                              Environment.NewLine + mensajeInformativo;
                r.Mensaje = $"Se han inputado {imputados} de los marcados ({listaIds.Count})";
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "Error en el proceso de imputación.");
            }

            return new JsonResult(r);
        }


        protected override dynamic ProcesarOpcionMf(enumNegocio negocio, string opcion, Dictionary<string, object> parametros)
        {
            List<ClausulaDeFiltrado> filtros = JsonConvert.DeserializeObject<List<ClausulaDeFiltrado>>(parametros.LeerValor(ltrParametrosEp.Filtro, "[]"));
            List<int> ids = (List<int>)parametros.LeerValor(ltrParametrosEp.ids, new List<int>());
            switch (opcion)
            {
                case eventosDeMf.Ctr_VincularRegistroEntrada:
                    return null;
                case eventosDeMf.Ctr_IrAPlvDeVenta:
                    return null;
                case eventosDeMf.Ctr_IrAPartesTr:
                    return null;
                case eventosDeMf.Ctr_IrAFacturasEmt:
                    return null;
                case eventosDeMf.Ctr_IrAFacturasRec:
                    return null;
                case eventosDeMf.Ctr_ImputarFacturas:
                    GestorDeContratos.ValidarPuedeImputarFacturas(Contexto, ids);
                    return null;
                case eventosDeMf.GenerarPlanificadoresDeUnContrato:
                    TrabajosDeContratos.SometerGenerarPlanificadoresDeUnContrato(Contexto, ids[0]);
                    return null;
                case eventosDeMf.GenerarLosPlanificadores:
                    return GestorDeContratos.ValidarExistenPlanificadoresPendientes(Contexto, filtros);
                case eventosDeMf.PrepararPartesDeTrabajo:
                    return GestorDeContratos.ValidarExistenPlanificaciones(Contexto, filtros);
                case eventosDeMf.EmitirPrefacturasPorParteTr:
                    return GestorDeContratos.ValidarExistenPartesRealizados(Contexto, filtros);
                case eventosDeMf.EmitirPrefacturasPorContrato:
                    return GestorDeContratos.ValidarExistenPartesRealizados(Contexto, filtros);
            }
            return base.ProcesarOpcionMf(negocio, opcion, parametros);
        }

        public JsonResult epAsociarUnContrato(string parametrosJson)
        {
            var r = new Resultado();
            Dictionary<string, object> parametros = parametrosJson.ToDiccionarioDeParametros();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                if (!parametros.ContieneClave(nameof(ContratoDto.Id))) throw new Exception("Debe indicar el Contrato");
                if (!parametros.ContieneClave(nameof(ContratoDto.IdExpediente))) throw new Exception("Debe indicar el expediente al que asociar");

                var idContrato = (int)parametros.LeerValor<long>(nameof(ContratoDto.Id));
                var idExpediente = (int)parametros.LeerValor<long>(nameof(ContratoDto.IdExpediente));

                var ppt = Contexto.SeleccionarPorId<ContratoDtm>(idContrato);
                if (ppt.IdExpediente.Entero() > 0 && (int)ppt.IdExpediente == idExpediente)
                    GestorDeErrores.Emitir("El expediente a asociar al Contrato es el que ya tiene");

                ppt.IdExpediente = idExpediente;
                ppt.Modificar(Contexto);

                r.Mensaje = $"Contrato asociado al expediente";
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "Error al asociar el Contrato al expediente.");
            }
            return new JsonResult(r);
        }

        public JsonResult epGenerarPlanificadores(string parametrosJson)
        {
            var r = new Resultado();
            Dictionary<string, object> parametros = parametrosJson.ToDiccionarioDeParametros();
            List<ClausulaDeFiltrado> filtros = JsonConvert.DeserializeObject<List<ClausulaDeFiltrado>>(parametros.LeerValor(ltrParametrosEp.Filtro, "[]"));
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                GestorDeContratos.GenerarPlanificadores(Contexto, filtros, parametros.LeerValor<DateTime>(nameof(FechasDeGeneracionDto.FechaDesde)), parametros.LeerValor<DateTime>(nameof(FechasDeGeneracionDto.FechaHasta)));
                r.Mensaje = $"Planificaciones generadas";
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "Error al generar las planificaciones.");
            }
            return new JsonResult(r);
        }

        public JsonResult epEmitirPrefacturasPorPartesTr(string parametrosJson)
        {
            var r = new Resultado();
            Dictionary<string, object> parametros = parametrosJson.ToDiccionarioDeParametros();
            List<ClausulaDeFiltrado> filtros = JsonConvert.DeserializeObject<List<ClausulaDeFiltrado>>(parametros.LeerValor(ltrParametrosEp.Filtro, "[]"));
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                GestorDeContratos.PrefacturarPartesTr(Contexto, filtros, parametros.LeerValor<DateTime>(nameof(FechasDeGeneracionDto.FechaDesde)), parametros.LeerValor<DateTime>(nameof(FechasDeGeneracionDto.FechaHasta)));
                r.Mensaje = $"Prefacturas emitidas";
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "Error al generar las prefacturas.");
            }
            return new JsonResult(r);
        }

        public JsonResult epEmitirPrefacturasPorContratos(string parametrosJson)
        {
            var r = new Resultado();
            Dictionary<string, object> parametros = parametrosJson.ToDiccionarioDeParametros();
            List<ClausulaDeFiltrado> filtros = JsonConvert.DeserializeObject<List<ClausulaDeFiltrado>>(parametros.LeerValor(ltrParametrosEp.Filtro, "[]"));
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                GestorDeContratos.PrefacturarContratos(Contexto, filtros, parametros.LeerValor<DateTime>(nameof(FechasDeGeneracionDto.FechaDesde)), parametros.LeerValor<DateTime>(nameof(FechasDeGeneracionDto.FechaHasta)));
                r.Mensaje = $"Prefacturas emitidas";
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "Error al generar las prefacturas.");
            }
            return new JsonResult(r);
        }

        public JsonResult epPrepararPartesTr(string parametrosJson)
        {
            var r = new Resultado();
            Dictionary<string, object> parametros = parametrosJson.ToDiccionarioDeParametros();
            List<ClausulaDeFiltrado> filtros = JsonConvert.DeserializeObject<List<ClausulaDeFiltrado>>(parametros.LeerValor(ltrParametrosEp.Filtro, "[]"));
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                GestorDeContratos.PrepararPartesDeTrabajo(Contexto, filtros, parametros.LeerValor<DateTime>(nameof(FechasDeGeneracionDto.FechaDesde)), parametros.LeerValor<DateTime>(nameof(FechasDeGeneracionDto.FechaHasta)));
                r.Mensaje = $"partes preparados";
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "Error al generar los partes de trabajo.");
            }
            return new JsonResult(r);
        }


        [HttpPost]
        public JsonResult epCopiarPlfDeVenta(int idNegocio, int idElemento)
        {
            var r = new Resultado();
            var body = ApiController.LeerBody(HttpContext);
            Contexto.IniciarTraza(GetType().Name + "_" + nameof(epCopiarPlfDeVenta));
            try
            {
                Contexto.AnotarParametros(new List<object> { body.parametrosJson });
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                var copiarPlfDeVenta = ((JObject)body.parametros[ltrParametrosEp.Elemento]).ToObject<CopiarPlfDeVentaDto>();
                GestorDeContratos.CopiarPlanificadores(Contexto, idElemento, copiarPlfDeVenta);
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "Error al copiar un planificador.");
            }
            finally
            {
                Contexto.CerrarTraza();
            }
            return new JsonResult(r);
        }

        public IActionResult InicializarEtapas()
        {
            var r = new Resultado();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);

                if (!Contexto.DatosDeConexion.EsAdministrador)
                    GestorDeErrores.Emitir("Esta opción sólo se permite a administradores");

                ExtensorDeContratos.InicializarEtapas(Contexto);
                ViewBag.Mensaje = "Etapas de contratos definidas";
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                return RenderMensaje($"No se ha podido inicializar las etapas de los contratos.{Environment.NewLine}{GestorDeErrores.Detalle(e)}");
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

                TrabajosDeContratos.SometerNotificarPorcentajeDeAvisoSobrepasado(Contexto);
                TrabajosDeContratos.SometerMotorDeContratos(Contexto);
                ViewBag.Mensaje = "Trabajos sometidos";
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                return RenderMensaje($"No se ha podido someter los trabajos de los contratos.{Environment.NewLine}{GestorDeErrores.Detalle(e)}");
            }
            return VistaDelPanelDeControl(Contexto);
        }

        public IActionResult MaestrosDeContratos()
        {
            var r = new Resultado();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);

                if (!Contexto.SePuedeParametrizar())
                    GestorDeErrores.Emitir("Esta opción sólo se permite a parametrizadores");

                InzContratosVnt.ModeloDeContratosVnt(Contexto);
                InzContratosCmp.ModeloDeContratosCmp(Contexto);
                ExtensorDeContratos.InicializarEtapas(Contexto);
                TrabajosDeContratos.SometerNotificarPorcentajeDeAvisoSobrepasado(Contexto);
                TrabajosDeContratos.SometerMotorDeContratos(Contexto);
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

    }
}
