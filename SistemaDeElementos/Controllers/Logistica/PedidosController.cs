using ServicioDeDatos;
using Gestor.Errores;
using Microsoft.AspNetCore.Mvc;
using System;
using Utilidades;
using System.Collections.Generic;
using GestoresDeNegocio.Logistica;
using ServicioDeDatos.Logistica;
using ModeloDeDto.Logistica;
using MVCSistemaDeElementos.Descriptores;
using GestorDeElementos;
using ServicioDeDatos.Seguridad;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Inicializador.Logistica;
using ServicioDeDatos.MaestrosTecnico;
using static ServicioDeDatos.Elemento.Enumerados;
using GestorDeElementos.Extensores;
using ServicioDeDatos.Negocio;

namespace MVCSistemaDeElementos.Controllers
{
    public class PedidosController : EntidadController<ContextoSe, PedidoDtm, PedidoDto>
    {
        public IPdfServerClient PdfServerClient { get; }

        public PedidosController(GestorDePedidos gestorDePedidos, GestorDeErrores gestorDeErrores, IPdfServerClient pdfServerClient)
         : base
         (
           gestorDePedidos,
           gestorDeErrores
         )
        {
            PdfServerClient = pdfServerClient;
        }


        public IActionResult CrudPedidos()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);

            var modo = ModoDescriptor.Mantenimiento;
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{modo}-{typeof(DescriptorDePedidos).FullName}";
            var cache = ServicioDeCaches.Obtener(CacheDe.RenderCrud);
            try
            {
                if (cache.ContainsKey(indice))
                {
                    ViewBag.DatosDeConexion = DatosDeConexion;
                    var destino = $"../{enumNameSpaceTs.Logistica}/{nameof(CrudPedidos)}";
                    return base.View(destino, new DescriptorDePedidos(Contexto, (string)cache[indice]));
                }
                else
                {
                    var descriptor = DescriptorDeCrud<PedidoDto>.CrearDescriptor(Contexto, modo, () => new DescriptorDePedidos(Contexto, modo));
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
            indicadores.Add(IndPedido.UnidadDeMedida, enumNegocio.Pedido.Parametro(enumParametrosDePedidos.PED_Unidad_Medida, crearParametro: true, valorPorDefecto: Literal.Cero).Valor.Entero());
            indicadores.Add(IndPedido.Naturaleza, enumNegocio.Pedido.Parametro(enumParametrosDePedidos.PED_Naturaleza, crearParametro: true, valorPorDefecto: Literal.Cero).Valor.Entero());
            indicadores.Add(IndPedido.TipoDeLinea, enumNegocio.Pedido.Parametro(enumParametrosDePedidos.PED_TipoDeLinea, crearParametro: true, valorPorDefecto: enumTipoDeLinea.Alzada.ToString()).Valor);
            indicadores.Add(IndPedido.ClaseDeUnitario, enumNegocio.Pedido.Parametro(enumParametrosDePedidos.PED_ClaseDeUnitario, crearParametro: true, valorPorDefecto: enumClaseUnitario.Servicio.ToString()).Valor);
            return indicadores;
        }


        public IActionResult MaestrosDePedidos()
        {
            var r = new Resultado();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);

                if (!Contexto.SePuedeParametrizar())
                    GestorDeErrores.Emitir("Esta opción sólo se permite a parametrizadores");

                InzPedidos.ModeloDePedidos(Contexto);
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
                var totales = await ((ITotalizador<TotalesDePedidos>)_GestorDeElementos).ObtenerTotalesAsync(filtros, posicion, cantidad);
                r.Consola = $"Totalización realizada";
                r.Datos = totales;
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "Error al totalizar los pedidos.");
            }
            finally
            {
                Contexto.CerrarTraza();
            }
            return new JsonResult(r);
        }

    }
}
