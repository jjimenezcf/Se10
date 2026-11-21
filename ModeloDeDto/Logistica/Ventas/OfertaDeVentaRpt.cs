using ModeloDeDto.Reporte;
using ModeloDeDto.Terceros;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace ModeloDeDto.Logistica
{
    // Datos de la oferta de un documento de venta (ver ReporteDeOfertaDeVenta): lo que se le ofrece al cliente antes de servirlo, sólo con
    // la base imponible (sin iva)
    public class OfertaDeVentaRpt : InformacionBaseRpt<VentaDto>
    {
        public List<LineaDeUnaVentaDto> Lineas { get; set; }
        public SociedadDto Sociedad { get; set; }
        public ClienteDto Cliente { get; set; }
        public override string Logo { get; set; }
        public decimal Total { get; set; }
        public bool HayDescuento => Lineas.Any(linea => linea.Descuento is not null && linea.Descuento > 0);

        // Como en el pedido, el calificador de la dirección no se muestra y el logo no supera este ancho
        public static Dictionary<string, object> ParametrosDeImpresion
        {
            get
            {
                var parametros = ltrParametrosRpt.Parametros;
                parametros[ltrParametrosRpt.MostrarCalificadorDireccion] = false;
                parametros[ltrParametrosRpt.AnchoLogo] = 80F;
                return parametros;
            }
        }

        public static string ParametrosPorDefecto() => JsonConvert.SerializeObject(ParametrosDeImpresion, Formatting.Indented);
    }
}
