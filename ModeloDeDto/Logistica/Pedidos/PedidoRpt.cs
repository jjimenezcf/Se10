using ModeloDeDto.Reporte;
using ModeloDeDto.Terceros;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace ModeloDeDto.Logistica
{

    public class PedidoRpt : InformacionBaseRpt<PedidoDto>
    {
        public List<LineaDeUnPedidoDto> Lineas { get; set; }
        public SociedadDto Sociedad { get; set; }
        public ProveedorDto Proveedor { get; set; }
        public override string Logo { get; set; }
        public decimal Total { get; set; }
        public bool HayDescuento => Lineas.Any(linea => linea.ImporteDeDto is not null && linea.ImporteDeDto > 0);

        // En el pedido el calificador de la dirección no se muestra y el logo, que se ajusta al alto de los datos del solicitante, no supera este ancho
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
