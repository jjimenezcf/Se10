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
    // Información para imprimir una devolución de venta (ver ReporteDeDevolucionDeVenta), como la oferta (GeneradorDeOfertaDeVentaRpt)
    public class GeneradorDeDevolucionDeVentaRpt : IGeneradorRpt<DevolucionDeVentaDto>
    {
        private ContextoSe Contexto { get; }
        private DevolucionDeVentaDtm Devolucion { get; }

        public GeneradorDeDevolucionDeVentaRpt(ContextoSe contexto, DevolucionDeVentaDtm devolucion)
        {
            Contexto = contexto;
            Devolucion = devolucion;
        }

        public IInformacionRpt<DevolucionDeVentaDto> ObtenerInformacionDeRpt(string plantilla)
        {
            var informacionRpt = new DevolucionDeVentaRpt();
            informacionRpt.Datos = Devolucion.MapearDto<DevolucionDeVentaDto>(Contexto);

            var lineas = Devolucion.Detalles<LineaDeUnaDevolucionDeVentaDtm>(Contexto, aplicarJoin: true).OrderBy(linea => linea.Orden).ToList();
            informacionRpt.Lineas = lineas.Select(linea => linea.MapearDto<LineaDeUnaDevolucionDeVentaDto>(Contexto)).ToList();
            informacionRpt.Total = Devolucion.TotalSinIva(Contexto);

            var cg = Devolucion.Cg(Contexto, aplicarJoin: true);
            informacionRpt.Sociedad = Contexto.SeleccionarDto<SociedadDto, SociedadDtm>(
                          id: cg.IdSociedad,
                          parametros: new ParametrosDeNegocio(enumTipoOperacion.LeerSinBloqueo, new Dictionary<string, object> { { ltrParametrosNeg.ObtenerDatosFiscales, true } }));

            informacionRpt.Cliente = Contexto.SeleccionarDto<ClienteDto, ClienteDtm>(
                          id: Devolucion.IdCliente,
                          parametros: new ParametrosDeNegocio(enumTipoOperacion.LeerSinBloqueo, new Dictionary<string, object> { { ltrParametrosNeg.ObtenerDatosFiscales, true } }));

            informacionRpt.Logo = informacionRpt.Sociedad.IdArchivo is null
            ? string.Empty
            : ServidorDocumental.DescargarArchivo(Contexto, (int)informacionRpt.Sociedad.IdArchivo, solicitadoPorLaCola: false, erroSiNoEstaEnLaruta: false);

            if (informacionRpt.Logo == ApiDeArchivos.FicheroNoEncontrado)
                informacionRpt.Logo = string.Empty;

            var datos = enumNegocio.DevolucionVenta.LeerCrearParametro(Contexto, enumParametrosDeDevolucionesDeVenta.DDV_DatosDeImpresion, valor: DevolucionDeVentaRpt.ParametrosPorDefecto());
            informacionRpt.Parametros = JsonConvert.DeserializeObject<Dictionary<string, object>>(datos.Valor);
            if (!informacionRpt.VerificarVersionDeParametros())
            {
                informacionRpt.ActualizarVersionDeParametros(enumNegocio.DevolucionVenta.IdNegocio(), enumParametrosDeDevolucionesDeVenta.DDV_DatosDeImpresion);
            }

            return informacionRpt;
        }
    }
}
