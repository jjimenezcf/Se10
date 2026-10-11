using ServicioDeDatos;
using Gestor.Errores;
using Microsoft.AspNetCore.Mvc;
using System;
using Utilidades;
using System.Collections.Generic;
using System.Linq;
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
using GestoresDeNegocio.SistemaDocumental;
using ModeloDeDto.SistemaDocumental;

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

        protected override dynamic ProcesarOpcionMf(enumNegocio negocio, string opcion, Dictionary<string, object> parametros)
        {
            switch (opcion)
            {
                case eventosDeMf.Ped_ModalDeImprimir:
                    var plantillas = new ServicioDePlantillas(Contexto, enumNegocio.Pedido, (List<int>)parametros[ltrParametrosEp.ids]).Plantillas();
                    if (!plantillas.Abrir)
                        ImprimirPedidos((List<int>)parametros[ltrParametrosEp.ids]);
                    return plantillas;
                case eventosDeMf.Totalizador_Mostrar:
                    return null;
                case eventosDeMf.Ped_ModificarCabecera:
                    // la modal se propone con los datos de la edición
                    return null;
                case eventosDeMf.Ped_GuardarCabecera:
                    GestorDePedidos.ModificarCabecera(Contexto, new ModificarCabeceraDePedidoDto
                    {
                        IdElemento = ((List<int>)parametros[ltrParametrosEp.ids]).Single(),
                        Nombre = parametros.LeerValor<string>(nameof(ModificarCabeceraDePedidoDto.Nombre), null),
                        eMail = parametros.LeerValor<string>(nameof(ModificarCabeceraDePedidoDto.eMail), null),
                        GuardarEnElProveedor = parametros.LeerValor(nameof(ModificarCabeceraDePedidoDto.GuardarEnElProveedor), false),
                        EntregarEl = parametros.LeerValor<DateTime?>(nameof(ModificarCabeceraDePedidoDto.EntregarEl), null),
                        RecibidoEl = parametros.LeerValor<DateTime?>(nameof(ModificarCabeceraDePedidoDto.RecibidoEl), null),
                        CerradoEl = parametros.LeerValor<DateTime?>(nameof(ModificarCabeceraDePedidoDto.CerradoEl), null),
                        RowVersion = parametros.LeerValor<string>(nameof(ModificarCabeceraDePedidoDto.RowVersion), null)
                    });
                    return new Resultado { Estado = enumEstadoPeticion.Ok, Mensaje = "Cabecera del pedido modificada" };
            }
            return base.ProcesarOpcionMf(negocio, opcion, parametros);
        }

        protected override bool Imprimir(int idNegocio, Dictionary<string, object> parametros)
        {
            if (base.Imprimir(idNegocio, parametros))
                return true;

            var plantilla = parametros.LeerValor<string>(ltrParametrosEp.Plantilla);
            if (plantilla != EstandarPlt.Estandard)
                return false;

            ImprimirPedidos(new List<int> { (int)parametros.LeerValor<long>(ltrParametrosEp.idElemento) });
            return true;
        }

        private void ImprimirPedidos(List<int> idsDePedidos)
        {
            foreach (var id in idsDePedidos)
            {
                ImprimirPedido(id);
            }
        }

        private void ImprimirPedido(int idPedido)
        {
            GestorDePedidos.ImprimirPedido(Contexto, Contexto.SeleccionarPorId<PedidoDtm>(idPedido));
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

        public JsonResult epRecibirLinea(string parametrosJson)
        {
            var r = new Resultado();
            Contexto.IniciarTraza(GetType().Name + "_" + nameof(epRecibirLinea));
            var tran = Contexto.IniciarTransaccion();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                var parametros = parametrosJson.ToDiccionarioDeParametros();
                var linea = LeerLineaParaRecepcionar(parametros);
                var cantidad = parametros.LeerValor<decimal?>(nameof(RecepcionarLineaDto.Cantidad), null);
                var precio = parametros.LeerValor<decimal?>(nameof(RecepcionarLineaDto.Precio), null);
                var descuento = parametros.LeerValor<decimal?>(nameof(RecepcionarLineaDto.Descuento), null);
                if (cantidad == null || cantidad <= 0)
                    GestorDeErrores.Emitir("Ha de indicar la cantidad entregada, si no se ha entregado nada marque la línea como no entregada");
                if (precio == null || precio < 0)
                    GestorDeErrores.Emitir("Ha de indicar el precio de compra unitario sin iva");
                if (descuento != null && (descuento < 0 || descuento > 100))
                    GestorDeErrores.Emitir("El descuento ha de estar entre 0 y 100");
                var importeEntregado = cantidad * precio * (1 - (descuento ?? 0) / 100);

                //TODO: registrar la recepción de la línea
                r.Consola = $"Línea {linea.Orden} recibida: cantidad {cantidad}, precio {precio}, descuento {descuento}, importe entregado {importeEntregado:N2}";
                r.Estado = enumEstadoPeticion.Ok;
                Contexto.Commit(tran);
            }
            catch (Exception e)
            {
                Contexto.Rollback(tran);
                ApiController.PrepararError(e, r, "Error al recibir la línea del pedido.");
            }
            finally
            {
                Contexto.CerrarTraza();
            }
            return new JsonResult(r);
        }

        public JsonResult epLineaNoEntregada(string parametrosJson)
        {
            var r = new Resultado();
            Contexto.IniciarTraza(GetType().Name + "_" + nameof(epLineaNoEntregada));
            var tran = Contexto.IniciarTransaccion();
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                var parametros = parametrosJson.ToDiccionarioDeParametros();
                var linea = LeerLineaParaRecepcionar(parametros);

                //TODO: registrar que la línea no se ha entregado
                r.Consola = $"Línea {linea.Orden} marcada como no entregada";
                r.Estado = enumEstadoPeticion.Ok;
                Contexto.Commit(tran);
            }
            catch (Exception e)
            {
                Contexto.Rollback(tran);
                ApiController.PrepararError(e, r, "Error al marcar la línea del pedido como no entregada.");
            }
            finally
            {
                Contexto.CerrarTraza();
            }
            return new JsonResult(r);
        }

        // Lee la línea sin cache, valida que es la versión que el usuario vio en el grid y que el pedido admite recepciones;
        // se ha de llamar antes de cualquier modificación de la operación
        private LineaDeUnPedidoDtm LeerLineaParaRecepcionar(Dictionary<string, object> parametros)
        {
            var idLinea = (int)parametros.LeerValor<long>(nameof(RecepcionarLineaDto.IdLineaPedido));
            var linea = Contexto.SeleccionarPorId<LineaDeUnPedidoDtm>(idLinea, usarLaCache: false);
            linea.ValidarRowId(Contexto, parametros.LeerValor<string>(nameof(LineaDeUnPedidoDtm.RowVersion), null), releer: false);

            var pedido = Contexto.SeleccionarPorId<PedidoDtm>(linea.IdElemento, usarLaCache: false);
            if (!pedido.EstaEnAlgunaDeLasEtapa(new List<enumEtapasDePedido> { enumEtapasDePedido.PED_Etapa_De_Solicitud, enumEtapasDePedido.PED_Etapa_De_Recepcion }))
                GestorDeErrores.Emitir($"Las líneas del pedido '{pedido.Referencia}' solo se pueden recepcionar con el pedido {enumEtapasDePedido.PED_Etapa_De_Solicitud.Nombre()} o {enumEtapasDePedido.PED_Etapa_De_Recepcion.Nombre()}");

            return linea;
        }

    }
}
