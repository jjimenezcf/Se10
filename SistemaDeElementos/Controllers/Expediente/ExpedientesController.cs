using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Expediente;
using Microsoft.AspNetCore.Mvc;
using ModeloDeDto.Expediente;
using ModeloDeDto.Presupuesto;
using ModeloDeDto.SistemaDocumental;
using MVCSistemaDeElementos.Descriptores;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RtfPipe.Tokens;
using ServicioDeDatos;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.SistemaDocumental;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utilidades;

namespace MVCSistemaDeElementos.Controllers
{
    public class ExpedientesController : EntidadController<ContextoSe, ExpedienteDtm, ExpedienteDto>
    {
        public ExpedientesController(GestorDeExpedientes gestorDeExpedientes, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeExpedientes,
           gestorDeErrores
         )
        {
        }

        protected override Dictionary<string, object> IndicadoresParaInicializarLaVistaMnt(ContextoSe contexto, Dictionary<string, object> parametros)
        {
            var indicadores = base.IndicadoresParaInicializarLaVistaMnt(contexto, parametros);
            if (parametros.LeerValor(ltrParametrosEp.Descriptor, string.Empty) == typeof(DescriptorDeActividades).Name)
            {
                var idTipo = VariablesDeExpedientes.IdDelTipoParaActividades(errorSiNoEstaDefinido: true);
                indicadores.Add(IndExpediente.IdTipoActividad, idTipo);
                indicadores.Add(IndExpediente.TipoActividad, contexto.SeleccionarPorId<TipoDeExpedienteDtm>(idTipo).Nombre);
            }
            return indicadores;
        }

        public IActionResult CrudActividades()
        {
            var modo = ModoDescriptor.Mantenimiento;
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{modo}-{typeof(DescriptorDeActividades).FullName}";
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);

                (bool redirigir, IActionResult dondeRedirigir, ExpedienteDtm expediente) = RedirigirSiNoEsActividad();
                if (redirigir) return dondeRedirigir;

                var cache = ServicioDeCaches.Obtener(CacheDe.RenderCrud);
                if (cache.ContainsKey(indice))
                {
                    ViewBag.DatosDeConexion = DatosDeConexion;
                    var destino = $"../{enumNameSpaceTs.Administracion}/{nameof(CrudActividades)}";
                    return base.View(destino, new DescriptorDeActividades(Contexto, (string)cache[indice]));
                }
                else
                {
                    var descriptor = DescriptorDeCrud<ExpedienteDto>.CrearDescriptor(Contexto, modo, () => new DescriptorDeActividades(Contexto, modo));
                    return ViewCrud(descriptor);
                }
            }
            catch (Exception e)
            {
                return RenderizarErrorDe(indice, e);
            }
        }

        public IActionResult CrudExpedientes(string clase)
        {
            var modo = ModoDescriptor.Mantenimiento;
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{modo}-{typeof(DescriptorDeExpedientes).FullName}";

            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);

                (bool redirigir, IActionResult dondeRedirigir, ExpedienteDtm expediente) = RedirigirSiEsActividad();
                if (redirigir) return dondeRedirigir;

                if (clase.IsNullOrEmpty())
                    clase = expediente != null ? expediente.ClaseDeExpediente.ToString() : ltrDeUnExpediente.ExpedieteNoJuridico;

                indice = $"{indice}-{clase}";
                var cache = ServicioDeCaches.Obtener(CacheDe.RenderCrud);
                if (cache.ContainsKey(indice))
                {
                    ViewBag.DatosDeConexion = DatosDeConexion;
                    var destino = $"../{enumNameSpaceTs.Administracion}/{nameof(CrudExpedientes)}";
                    return base.View(destino, new DescriptorDeExpedientes(Contexto, (string)cache[indice], clase));
                }
                else
                {
                    var descriptor = DescriptorDeCrud<ExpedienteDto>.CrearDescriptor(Contexto, modo, () => new DescriptorDeExpedientes(Contexto, modo, clase));
                    return ViewCrud(descriptor);
                }
            }
            catch (Exception e)
            {
                return RenderizarErrorDe(indice, e);
            }
        }

        private (bool redirigir, IActionResult dondeRedirigir, ExpedienteDtm expediente) RedirigirSiEsActividad()
        {
            HttpContext.Request.Query.TryGetValue(ltrParametrosEp.id, out var idValor);
            var idExpediente = !Microsoft.Extensions.Primitives.StringValues.IsNullOrEmpty(idValor)
                ? idValor.ToString().Entero()
                : 0;
            var expediente = Contexto.SeleccionarPorId<ExpedienteDtm>(idExpediente, errorSiNoHay: false);
            if (expediente != null && expediente.EsUnaActividad())
            {
                var queryString = HttpContext.Request.QueryString.Value;
                return (true, Redirect($"{nameof(CrudActividades)}{queryString}"), expediente);
            }

            return (redirigir: false, dondeRedirigir: null, expediente);
        }

        private (bool redirigir, IActionResult dondeRedirigir, ExpedienteDtm expediente) RedirigirSiNoEsActividad()
        {
            HttpContext.Request.Query.TryGetValue(ltrParametrosEp.id, out var idValor);
            var idExpediente = !Microsoft.Extensions.Primitives.StringValues.IsNullOrEmpty(idValor)
                ? idValor.ToString().Entero()
                : 0;

            var expediente = Contexto.SeleccionarPorId<ExpedienteDtm>(idExpediente, errorSiNoHay: false);
            if (expediente != null && expediente.EsUnaActividad() == false)
            {
                var queryString = HttpContext.Request.QueryString.Value;
                return (true, Redirect($"{nameof(CrudExpedientes)}{queryString}"), expediente);
            }

            return (redirigir: false, dondeRedirigir: null, expediente);
        }

        [HttpPost]
        public JsonResult epCrearValoracion(int idNegocio, int idElemento)
        {
            var r = new Resultado();
            var body = ApiController.LeerBody(HttpContext);
            Contexto.IniciarTraza(GetType().Name + "_" + nameof(epCrearValoracion));
            try
            {
                Contexto.AnotarParametros(new List<object> { body.parametrosJson });
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                var valoracion = ((JObject)body.parametros[ltrParametrosEp.Elemento]).ToObject<ValoracionDto>();
                ExtensorDePresupuestos.CrearValoracion(Contexto, valoracion);
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "Error al crear la valoración.");
            }
            finally
            {
                Contexto.CerrarTraza();
            }
            return new JsonResult(r);
        }


        protected override dynamic ProcesarOpcionMf(enumNegocio negocio, string opcion, Dictionary<string, object> parametros)
        {
            List<ClausulaDeFiltrado> filtros = JsonConvert.DeserializeObject<List<ClausulaDeFiltrado>>(parametros.LeerValor(ltrParametrosEp.Filtro, "[]"));
            List<int> ids = (List<int>)parametros.LeerValor(ltrParametrosEp.ids, new List<int>());
            switch (opcion)
            {
                case eventosDeMf.Exp_VincularRegistroEntrada:
                    return null;
                case eventosDeMf.Exp_IrATareas:
                    //ExtensorDeFacturasEmt.ValidarTieneParte(Contexto, (List<int>)parametros[ltrParametrosEp.ids]);
                    return null;
                case eventosDeMf.Exp_IrAFacturasRec:
                    return null;
                case eventosDeMf.Totalizador_Mostrar:
                    return null;
                case eventosDeMf.Exp_ImputarFacturas:
                    GestorDeExpedientes.ValidarPuedeImputarFacturas(Contexto, ids);
                    return null;
            }
            return base.ProcesarOpcionMf(negocio, opcion, parametros);
        }

        public IActionResult MaestrosDeExpedientes()
        {
            var r = new Resultado();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);

                if (!Contexto.SePuedeParametrizar())
                    GestorDeErrores.Emitir("Esta opción sólo se permite a parametrizadores");

                InicializadorController.InzProcesosDeExpedientes(Contexto);
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

        public IActionResult MaestrosDeProcedimientos()
        {
            var r = new Resultado();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);

                if (!Contexto.SePuedeParametrizar())
                    GestorDeErrores.Emitir("Esta opción sólo se permite a parametrizadores");

                InicializadorController.InzProcedimientosJudiciales(Contexto);
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
                    var imputado = ((IEsImputable)gestor).Imputar(id, enumNegocio.Expediente, idDondeImputar);
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
                var totales = await ((ITotalizador<TotalesDeExpedientes>)_GestorDeElementos).ObtenerTotalesAsync(filtros, posicion, cantidad);
                r.Consola = $"Totalización realizada";
                r.Datos = totales;
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "Error al totalizar los expedientes.");
            }
            finally
            {
                Contexto.CerrarTraza();
            }
            return new JsonResult(r);
        }

        public JsonResult epSometerEnvioParaLexnet()
        {
            var r = new Resultado();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                r.Total = 0;
                r.Mensaje = $"Se sometido el envío de los archivos del procedimiento";
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "Error al someter el envío del procedimiento a Lexnet.");
            }

            return new JsonResult(r);
        }

    }
}
