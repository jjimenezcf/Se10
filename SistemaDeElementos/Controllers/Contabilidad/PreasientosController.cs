using Gestor.Errores;
using GestorDeElementos;
using GestoresDeNegocio.Contabilidad;
using Inicializador.Contabilidad;
using Microsoft.AspNetCore.Mvc;
using ModeloDeDto;
using ModeloDeDto.Contabilidad;
using MVCSistemaDeElementos.Descriptores;
using Newtonsoft.Json;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Ventas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utilidades;
using GestorDeElementos.Extensores;

namespace MVCSistemaDeElementos.Controllers
{
    public class PreasientosController : EntidadController<ContextoSe, PreasientoDtm, PreasientoDto>
    {
        public PreasientosController(GestorDePreasientos gestorDePreasientos, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDePreasientos,
           gestorDeErrores
         )
        {
        }

        public IActionResult CrudPreasientos()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);

            var modo = ModoDescriptor.Mantenimiento;
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{modo}-{typeof(DescriptorDePreasientos).FullName}";
            var cache = ServicioDeCaches.Obtener(CacheDe.RenderCrud);
            try
            {
                if (cache.ContainsKey(indice))
                {
                    ViewBag.DatosDeConexion = DatosDeConexion;
                    var destino = $"../{enumNameSpaceTs.Contabilidad}/{nameof(CrudPreasientos)}";
                    return base.View(destino, new DescriptorDePreasientos(Contexto, (string)cache[indice]));
                }
                else
                {
                    var descriptor = DescriptorDeCrud<PreasientoDto>.CrearDescriptor(Contexto, modo, () => new DescriptorDePreasientos(Contexto, modo));
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
            indicadores[IndCrud.SiempreEnConsulta] = true;
            return indicadores;
        }

        public IActionResult MaestrosDePreasientos()
        {
            var r = new Resultado();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);

                if (!Contexto.SePuedeParametrizar())
                    GestorDeErrores.Emitir("Esta opción sólo se permite a parametrizadores");

                InzPreasientos.ModeloDePreasientos(Contexto);
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

        protected override dynamic ProcesarOpcionMf(enumNegocio negocio, string opcion, Dictionary<string, object> parametros)
        {
            switch (opcion)
            {
                case eventosDeMf.Spr_CrearLote:
                    return null;
                case eventosDeMf.Totalizador_Mostrar:
                    return null;
                case eventosDeMf.Spr_CrearLoteConUnPreasiento:
                    ((GestorDePreasientos)_GestorDeElementos).CrearLoteConUnPreasiento((List<int>)parametros[ltrParametrosEp.ids]);
                    return null;
                case eventosDeMf.Spr_RegenerarPreasiento:
                    ((GestorDePreasientos)_GestorDeElementos).RegenerarPreasientos((List<int>)parametros[ltrParametrosEp.ids]);
                    return null;
            }
            return base.ProcesarOpcionMf(negocio, opcion, parametros);
        }

        public JsonResult epObtenerUrlAlOrigen(string parametrosJson)
        {
            var r = new Resultado();

            Dictionary<string, object> parametros = parametrosJson.ToDiccionarioDeParametros();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);

                var idPreasiento = (int)parametros.LeerValor<long>(nameof(PreasientoDtm.Id));
                var spr = Contexto.SeleccionarPorId<PreasientoDtm>(idPreasiento);
                if (spr.NegocioReferenciado == enumNegocio.Cobro)
                {
                    var vista = enumNegocio.FacturaEmitida.VistaMvc(Contexto);
                    var cobro = Contexto.SeleccionarPorId<CobroDeFaeDtm>(spr.IdReferenciado.Entero(), errorSiNoHay: false);
                    if (cobro is null)
                        GestorDeErrores.Emitir("El cobro ha sido eliminado de la BD, por tanto no se puede editar");
                    r.Datos = $"{enumNegocio.FacturaEmitida.Controlador()}/{vista.Accion}?Id={cobro.IdElemento}";
                }
                else
                {
                    var vista = spr.NegocioReferenciado.VistaMvc(Contexto);
                    r.Datos = $"{spr.NegocioReferenciado.Controlador()}/{vista.Accion}?Id={spr.IdReferenciado.Entero()}";
                }
                r.Consola = $"Origen obtenido correctamente";
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "No se puede obtener el origen del preasiento.");
            }
            return new JsonResult(r);
        }

        public JsonResult epObtenerUrlAlLoteContable(string parametrosJson)
        {
            var r = new Resultado();

            Dictionary<string, object> parametros = parametrosJson.ToDiccionarioDeParametros();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);

                var idPreasiento = (int)parametros.LeerValor<long>(nameof(PreasientoDtm.Id));
                var spr = Contexto.SeleccionarPorId<PreasientoDtm>(idPreasiento);

                var lotes = GestorDeVinculos.RegistrosVinculados<CircuitoDocDtm>(Contexto, enumNegocio.Preasiento, idPreasiento);
                if (lotes.Count() == 0)
                    GestorDeErrores.Emitir($"El preasiento '{spr.Referencia}' no está en ningún lote contable");

                var idlote = lotes.Max(l => l.Id);
                var vista = Contexto.SeleccionarPorPropiedad<VistaMvcDtm>(nameof(VistaMvcDtm.Accion), nameof(CircuitosDocController.CrudLotesContables));
                r.Datos = $"{nameof(CircuitosDocController).Replace(ltrEndPoint.Controller, "")}/{vista.Accion}?Id={idlote}";

                r.Consola = $"Se ha localizado en '{lotes.Count()}' lotes contables, las referencias de dichos lotes son '{string.Join(',', lotes.Select(l => l.Referencia))}'";
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "No se puede obtener el origen del preasiento.");
            }
            return new JsonResult(r);
        }
        public JsonResult epCrearLoteContable(string parametrosJson)
        {
            var r = new Resultado();

            Dictionary<string, object> parametros = parametrosJson.ToDiccionarioDeParametros();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);

                var idSociedad = (int)parametros.LeerValor<long>(nameof(CrearLoteContableDto.IdSociedad));
                var ejercicio = (int)parametros.LeerValor<long>(nameof(CrearLoteContableDto.Ejercicio));
                var fechaContable = parametros.LeerValor<DateTime?>(nameof(CrearLoteContableDto.FechaContable), null);
                var descontabilizar = parametros.LeerValor<bool>(nameof(CrearLoteContableDto.Descontabilizar), false);
                var respetarFechaContable = parametros.LeerValor<bool>(nameof(CrearLoteContableDto.RespetarFechaContable), true);
                var filtros = parametros.LeerValor<string>(ltrFiltros.filtro);

                TrabajosContables.SometerCrearLoteContable(Contexto, idSociedad, ejercicio, descontabilizar, respetarFechaContable, fechaContable, filtros);

                r.Mensaje = $"Trabajo de creación de lote contable sometido correctamente";
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "No ha podido someter la creación del lote contable.");
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
                var totales = await ((ITotalizador<TotalesPorCuenta>)_GestorDeElementos).ObtenerTotalesAsync(filtros, posicion, cantidad);
                r.Consola = $"Totalización realizada";
                r.Datos = totales;
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "Error al totalizar las tareas.");
            }
            finally
            {
                Contexto.CerrarTraza();
            }
            return new JsonResult(r);
        }

    }
}
