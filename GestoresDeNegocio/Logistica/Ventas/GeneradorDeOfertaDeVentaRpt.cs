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
    // Información para imprimir la oferta de un documento de venta (ver ReporteDeOfertaDeVenta), como la del pedido (GeneradorDePedidoRpt)
    public class GeneradorDeOfertaDeVentaRpt : IGeneradorRpt<VentaDto>
    {
        private ContextoSe Contexto { get; }
        private VentaDtm Venta { get; }

        public GeneradorDeOfertaDeVentaRpt(ContextoSe contexto, VentaDtm venta)
        {
            Contexto = contexto;
            Venta = venta;
        }

        public IInformacionRpt<VentaDto> ObtenerInformacionDeRpt(string plantilla)
        {
            var informacionRpt = new OfertaDeVentaRpt();
            informacionRpt.Datos = Venta.MapearDto<VentaDto>(Contexto);

            var lineas = Venta.Detalles<LineaDeUnaVentaDtm>(Contexto, aplicarJoin: true).OrderBy(linea => linea.Orden).ToList();
            informacionRpt.Lineas = lineas.Select(linea => linea.MapearDto<LineaDeUnaVentaDto>(Contexto)).ToList();
            informacionRpt.Total = Venta.TotalSinIva(Contexto);

            var cg = Venta.Cg(Contexto, aplicarJoin: true);
            informacionRpt.Sociedad = Contexto.SeleccionarDto<SociedadDto, SociedadDtm>(
                          id: cg.IdSociedad,
                          parametros: new ParametrosDeNegocio(enumTipoOperacion.LeerSinBloqueo, new Dictionary<string, object> { { ltrParametrosNeg.ObtenerDatosFiscales, true } }));

            informacionRpt.Cliente = Contexto.SeleccionarDto<ClienteDto, ClienteDtm>(
                          id: Venta.IdCliente,
                          parametros: new ParametrosDeNegocio(enumTipoOperacion.LeerSinBloqueo, new Dictionary<string, object> { { ltrParametrosNeg.ObtenerDatosFiscales, true } }));

            informacionRpt.Logo = informacionRpt.Sociedad.IdArchivo is null
            ? string.Empty
            : ServidorDocumental.DescargarArchivo(Contexto, (int)informacionRpt.Sociedad.IdArchivo, solicitadoPorLaCola: false, erroSiNoEstaEnLaruta: false);

            if (informacionRpt.Logo == ApiDeArchivos.FicheroNoEncontrado)
                informacionRpt.Logo = string.Empty;

            var datos = enumNegocio.Venta.LeerCrearParametro(Contexto, enumParametrosDeVentas.DSA_DatosDeImpresion, valor: OfertaDeVentaRpt.ParametrosPorDefecto());
            informacionRpt.Parametros = JsonConvert.DeserializeObject<Dictionary<string, object>>(datos.Valor);
            if (!informacionRpt.VerificarVersionDeParametros())
            {
                informacionRpt.ActualizarVersionDeParametros(enumNegocio.Venta.IdNegocio(), enumParametrosDeVentas.DSA_DatosDeImpresion);
            }

            return informacionRpt;
        }
    }
}
