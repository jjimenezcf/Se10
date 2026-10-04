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
    public class GeneradorDePedidoRpt : IGeneradorRpt<PedidoDto>
    {
        private ContextoSe Contexto { get; }
        private PedidoDtm Pedido { get; }

        public GeneradorDePedidoRpt(ContextoSe contexto, PedidoDtm pedido)
        {
            Contexto = contexto;
            Pedido = pedido;
        }

        public IInformacionRpt<PedidoDto> ObtenerInformacionDeRpt(string plantilla)
        {
            var informacionRpt = new PedidoRpt();
            informacionRpt.Datos = Pedido.MapearDto<PedidoDto>(Contexto);

            var lineas = Pedido.Detalles<LineaDeUnPedidoDtm>(Contexto, aplicarJoin: true).OrderBy(linea => linea.Orden).ToList();
            informacionRpt.Lineas = lineas.Select(linea => linea.MapearDto<LineaDeUnPedidoDto>(Contexto)).ToList();
            informacionRpt.Total = lineas.Sum(linea => linea.ImporteDeLinea);

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
