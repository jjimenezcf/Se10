using GestorDeElementos;
using GestorDeElementos.Extensores;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Ventas;
using ServicioDeDatos.Terceros;
using System.Collections.Generic;
using System.Linq;

namespace GestoresDeNegocio.Ventas
{
    internal static class AgrupadosDeFacturasEmt
    {
        public static decimal? ResolverCampoCalculado(ElementoDeProcesoDtm registro, MetricaDeTotales metrica, ContextoSe contexto)
        {
            if (!(registro is FacturaEmtDtm factura)) return null;
            var campo = metrica.Campo.Substring("calculado:".Length).Trim().ToLowerInvariant();
            if (campo is "total" or "importe" or "importetotal")
            {
                var lineas = contexto.Set<LineaDeUnaFaeDtm>()
                    .Where(l => l.IdElemento == factura.Id)
                    .ToList();
                var total = lineas.Where(l => l.ImporteDeLinea.HasValue).Sum(l => l.ImporteDeLinea!.Value);
                return total > 0 ? total : (decimal?)null;
            }
            return null;
        }

        public static string ResolverEtiqueta(
            string prop, object id, ContextoSe contexto,
            List<ITipoDeElementoDtm> tipos, List<EstadoDtm> estados, List<CentroGestorDtm> cgs)
            => AgrupadosDeUnProcesoHelper.ResolverEtiquetaComun(prop, id, contexto, tipos, estados, cgs);
    }
}
