using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.SistemaDocumental;
using ModeloDeDto;
using ModeloDeDto.Logistica;
using ModeloDeDto.Terceros;
using Newtonsoft.Json;
using ServicioDeDatos;
using ServicioDeDatos.Logistica;
using ServicioDeDatos.Terceros;
using System.Collections.Generic;
using System.Linq;
using Utilidades;

namespace GestoresDeNegocio.Logistica
{
    // Datos del impreso de las devoluciones de un pedido: la cabecera como la del pedido (ver GeneradorDePedidoRpt) y todas sus
    // devoluciones, por fecha, leídas con el gestor del grid de devoluciones para que se valoren igual
    public class GeneradorDeDevolucionDePedidoRpt : IGeneradorRpt<PedidoDto>
    {
        private ContextoSe Contexto { get; }
        private PedidoDtm Pedido { get; }

        public GeneradorDeDevolucionDePedidoRpt(ContextoSe contexto, PedidoDtm pedido)
        {
            Contexto = contexto;
            Pedido = pedido;
        }

        public IInformacionRpt<PedidoDto> ObtenerInformacionDeRpt(string plantilla)
        {
            var informacionRpt = new DevolucionDePedidoRpt();
            informacionRpt.Datos = Pedido.MapearDto<PedidoDto>(Contexto);

            var filtros = new List<ClausulaDeFiltrado> { new ClausulaDeFiltrado(nameof(IDetalleDto.IdElemento), enumCriteriosDeFiltrado.igual, Pedido.Id.ToString()) };
            informacionRpt.Devoluciones = GestorDeDevolucionesDeUnPedido.Gestor(Contexto, Contexto.Mapeador)
                .LeerElementos(0, -1, filtros, new List<ClausulaDeOrdenacion>(), new Dictionary<string, object>())
                .OrderBy(devolucion => devolucion.DevueltaEl).ThenBy(devolucion => devolucion.Id)
                .ToList();
            informacionRpt.Total = informacionRpt.Devoluciones.Sum(devolucion => devolucion.Importe);

            var cg = Pedido.Cg(Contexto, aplicarJoin: true);
            informacionRpt.Sociedad = Contexto.SeleccionarDto<SociedadDto, SociedadDtm>(
                          id: cg.IdSociedad,
                          parametros: new ParametrosDeNegocio(enumTipoOperacion.LeerSinBloqueo, new Dictionary<string, object> { { ltrParametrosNeg.ObtenerDatosFiscales, true } }));

            informacionRpt.Proveedor = Contexto.SeleccionarDto<ProveedorDto, ProveedorDtm>(
                          id: Pedido.IdProveedor,
                          parametros: new ParametrosDeNegocio(enumTipoOperacion.LeerSinBloqueo, new Dictionary<string, object> { { ltrParametrosNeg.ObtenerDatosFiscales, true } }));

            informacionRpt.Logo = informacionRpt.Sociedad.IdArchivo is null
            ? string.Empty
            : ServidorDocumental.DescargarArchivo(Contexto, (int)informacionRpt.Sociedad.IdArchivo, solicitadoPorLaCola: false, erroSiNoEstaEnLaruta: false);

            if (informacionRpt.Logo == ApiDeArchivos.FicheroNoEncontrado)
                informacionRpt.Logo = string.Empty;

            // los mismos parámetros de impresión que el pedido
            var datos = enumNegocio.Pedido.LeerCrearParametro(Contexto, enumParametrosDePedidos.PED_DatosDeImpresion, valor: PedidoRpt.ParametrosPorDefecto());
            informacionRpt.Parametros = JsonConvert.DeserializeObject<Dictionary<string, object>>(datos.Valor);
            if (!informacionRpt.VerificarVersionDeParametros())
            {
                informacionRpt.ActualizarVersionDeParametros(enumNegocio.Pedido.IdNegocio(), enumParametrosDePedidos.PED_DatosDeImpresion);
            }

            return informacionRpt;
        }
    }
}
