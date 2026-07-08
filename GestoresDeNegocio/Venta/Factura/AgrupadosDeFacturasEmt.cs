using GestorDeElementos;
using GestorDeElementos.Extensores;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Ventas;
using ServicioDeDatos.Terceros;
using System;
using System.Collections.Generic;
using System.Linq;
using static ServicioDeDatos.Ventas.enumEtiquetasDeFacturasEmt;

namespace GestoresDeNegocio.Ventas
{
    internal static class AgrupadosDeFacturasEmt
    {
        public static decimal? ResolverCampoCalculado(ElementoDeProcesoDtm registro, MetricaDeTotales metrica, ContextoSe contexto)
        {
            if (!(registro is FacturaEmtDtm factura)) return null;
            var campo = metrica.Campo.Substring("calculado:".Length).Trim().ToLowerInvariant();

            if (campo == nameof(Total).ToLowerInvariant() || campo is "importe" or "importetotal")
            {
                var lineas = contexto.Set<LineaDeUnaFaeDtm>()
                    .Where(l => l.IdElemento == factura.Id)
                    .ToList();
                var total = lineas.Where(l => l.ImporteDeLinea.HasValue).Sum(l => l.ImporteDeLinea!.Value);
                return total > 0 ? total : (decimal?)null;
            }

            if (campo == nameof(ImporteIrpf).ToLowerInvariant() || campo is "irpf" or "cuotairpf")
            {
                var irpf = contexto.Set<IrpfEmtDtm>()
                    .FirstOrDefault(i => i.IdElemento == factura.Id);
                return irpf?.Importe;
            }

            return null;
        }

        public static string ResolverEtiqueta(
            string prop, object id, ContextoSe contexto,
            List<ITipoDeElementoDtm> tipos, List<EstadoDtm> estados, List<CentroGestorDtm> cgs)
        {
            if (prop.Equals(nameof(PorcentajeIrpf), StringComparison.OrdinalIgnoreCase))
                return id?.ToString() is string s && s.Length > 0 ? s : "Sin IRPF";

            return AgrupadosDeUnProcesoHelper.ResolverEtiquetaComun(prop, id, contexto, tipos, estados, cgs);
        }

        // Batch-load de claves virtuales: porcentaje de IRPF agrupado por factura.
        public static Func<string, ElementoDeProcesoDtm, string> ObtenerResolverDeClaves(
            IEnumerable<int> idElementos, ContextoSe contexto, IEnumerable<string> agruparPor)
        {
            var ids   = idElementos.ToList();
            var props = new HashSet<string>(agruparPor, StringComparer.OrdinalIgnoreCase);

            var necesitaIrpf = props.Contains(nameof(PorcentajeIrpf));
            var cacheIrpf = necesitaIrpf
                ? contexto.Set<IrpfEmtDtm>()
                    .Where(i => ids.Contains(i.IdElemento))
                    .Select(i => new { i.IdElemento, i.Irpf })
                    .ToList()
                    .ToDictionary(x => x.IdElemento, x => x.Irpf.HasValue ? x.Irpf.Value.ToString("G29") + " %" : "Sin IRPF")
                : null;

            return (prop, registro) =>
            {
                if (prop.Equals(nameof(PorcentajeIrpf), StringComparison.OrdinalIgnoreCase) && cacheIrpf != null)
                    return cacheIrpf.TryGetValue(registro.Id, out var pct) ? pct : "Sin IRPF";
                return null;
            };
        }
    }
}
