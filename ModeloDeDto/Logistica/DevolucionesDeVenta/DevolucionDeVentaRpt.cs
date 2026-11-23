using ModeloDeDto.Reporte;
using ModeloDeDto.Terceros;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace ModeloDeDto.Logistica
{
    // Datos del documento de una devolución de venta (ver ReporteDeDevolucionDeVenta): lo que devuelve el cliente, de qué documento de venta
    // y a qué precio, sólo con la base imponible (sin iva), como la oferta del documento de venta (OfertaDeVentaRpt)
    public class DevolucionDeVentaRpt : InformacionBaseRpt<DevolucionDeVentaDto>
    {
        public List<LineaDeUnaDevolucionDeVentaDto> Lineas { get; set; }
        public SociedadDto Sociedad { get; set; }
        public ClienteDto Cliente { get; set; }
        public override string Logo { get; set; }
        public decimal Total { get; set; }
        public bool HayDescuento => Lineas.Any(linea => linea.Descuento is not null && linea.Descuento > 0);

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
