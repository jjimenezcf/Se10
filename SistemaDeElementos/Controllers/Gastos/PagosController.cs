using ServicioDeDatos;
using Gestor.Errores;
using Microsoft.AspNetCore.Mvc;
using System;
using Utilidades;
using MVCSistemaDeElementos.Descriptores;
using GestoresDeNegocio.Gastos;
using ModeloDeDto.Gastos;
using ServicioDeDatos.Gastos;
using Inicializador.Gastos;
using GestoresDeNegocio.SistemaDocumental;
using System.Collections.Generic;
using GestorDeElementos;
using Newtonsoft.Json;
using ServicioDeDatos.Seguridad;
using System.Threading.Tasks;
using GestorDeElementos.Extensores;

namespace MVCSistemaDeElementos.Controllers
{
    public class PagosController : EntidadController<ContextoSe, PagoDtm, PagoDto>
    {
        public PagosController(GestorDePagos gestorDePagos, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDePagos,
           gestorDeErrores
         )
        {
        }

        public IActionResult CrudPagos()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);

            var modo = ModoDescriptor.Mantenimiento;
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{modo}-{typeof(DescriptorDePagos).FullName}";
            var cache = ServicioDeCaches.Obtener(CacheDe.RenderCrud);
            try
            {
                if (cache.ContainsKey(indice))
                {
                    ViewBag.DatosDeConexion = DatosDeConexion;
                    var destino = $"../{enumNameSpaceTs.Gasto}/{nameof(CrudPagos)}";
                    return base.View(destino, new DescriptorDePagos(Contexto, (string)cache[indice]));
                }
                else
                {
                    var descriptor = DescriptorDeCrud<PagoDto>.CrearDescriptor(Contexto, modo, () => new DescriptorDePagos(Contexto, modo));
                    return ViewCrud(descriptor);
                }
            }
            catch (Exception e)
            {
                return RenderizarErrorDe(indice, e);
            }
        }


        public IActionResult MaestrosDePagos()
        {
            var r = new Resultado();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);

                if (!Contexto.SePuedeParametrizar())
                    GestorDeErrores.Emitir("Esta opción sólo se permite a parametrizadores");

                InzPagos.ModeloDePagos(Contexto);
                TrabajosDePagos.SometerProcesosDePagos(Contexto);
                ViewBag.Mensaje = "Maestros inicializados";
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                return DevolverPanelDeControlConMensaje(e.Message);
            }
            finally
            {
                ServicioDeCaches.EliminarTodas();
            }
            return VistaDelPanelDeControl(Contexto);
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
                var totales = await ((ITotalizador<TotalesDePago>)_GestorDeElementos).ObtenerTotalesAsync(filtros, posicion, cantidad);
                r.Consola = $"Totalización realizada";
                r.Datos = totales;
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "Error al totalizar los pagos.");
            }
            finally
            {
                Contexto.CerrarTraza();
            }
            return new JsonResult(r);
        }


        protected override dynamic ProcesarOpcionMf(enumNegocio negocio, string opcion, Dictionary<string, object> parametros)
        {
            switch (opcion)
            {
                case eventosDeMf.Pag_ModalDeImprimir:
                    ImprimirPagos((List<int>)parametros[ltrParametrosEp.ids]);
                    return new ServicioDePlantillas(Contexto, enumNegocio.Pago).Plantillas();
                case eventosDeMf.Pag_GenerarPreasiento:
                    ((GestorDePagos)_GestorDeElementos).GenerarPreasiento((List<int>)parametros[ltrParametrosEp.ids]);
                    return null;
                case eventosDeMf.Pag_CancelarPreasientos:
                    var ids = parametros.LeerValor<List<int>>(ltrParametrosEp.ids);
                    var cancelados = ((GestorDePagos)_GestorDeElementos).CancelarPreasientos(ids);
                    var r = new Resultado();
                    r.Mensaje = $"Se han cancelado '{cancelados}' preasientos";
                    return r;
                case eventosDeMf.Totalizador_Mostrar:
                    return null;
            }
            return base.ProcesarOpcionMf(negocio, opcion, parametros);
        }
        private void ImprimirPagos(List<int> idsDePago)
        {
            foreach (var id in idsDePago)
                GestorDePagos.GenerarJustificante(Contexto, id);
        }
    }
}
