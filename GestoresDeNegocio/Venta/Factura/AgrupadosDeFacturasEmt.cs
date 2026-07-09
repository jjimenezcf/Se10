using GestorDeElementos;
using GestorDeElementos.Extensores;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
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

            if (campo == nameof(BiIvaExento).ToLowerInvariant() || campo is "biexento" or "baseexenta")
            {
                // Suma la BI de todas las líneas cuyo IVA repercutido sea exento (Porcentaje == 0)
                var ivas = factura.Ivas(contexto);
                var bi = ivas.Where(i => i.EsExento).Sum(i => i.BI);
                return bi > 0 ? bi : (decimal?)null;
            }

            return null;
        }

        public static string ResolverEtiqueta(
            string prop, object id, ContextoSe contexto,
            List<ITipoDeElementoDtm> tipos, List<EstadoDtm> estados, List<CentroGestorDtm> cgs)
        {
            if (prop.Equals(nameof(PorcentajeIrpf), StringComparison.OrdinalIgnoreCase))
                return id?.ToString() is string s && s.Length > 0 ? s : "Sin IRPF";

            if (prop.Equals(nameof(PorcentajeIva), StringComparison.OrdinalIgnoreCase))
                return id?.ToString() is string v && v.Length > 0 ? v : "Sin IVA";

            return AgrupadosDeUnProcesoHelper.ResolverEtiquetaComun(prop, id, contexto, tipos, estados, cgs);
        }

        // Batch-load de claves virtuales: porcentaje de IRPF y de IVA agrupados por factura.
        public static Func<string, ElementoDeProcesoDtm, string> ObtenerResolverDeClaves(
            IEnumerable<int> idElementos, ContextoSe contexto, IEnumerable<string> agruparPor)
        {
            var ids   = idElementos.ToList();
            var props = new HashSet<string>(agruparPor, StringComparer.OrdinalIgnoreCase);

            // --- IRPF ---
            var necesitaIrpf = props.Contains(nameof(PorcentajeIrpf));
            var cacheIrpf = necesitaIrpf
                ? contexto.Set<IrpfEmtDtm>()
                    .Where(i => ids.Contains(i.IdElemento))
                    .Select(i => new { i.IdElemento, i.Irpf })
                    .ToList()
                    .ToDictionary(x => x.IdElemento, x => x.Irpf.HasValue ? x.Irpf.Value.ToString("G29") + " %" : "Sin IRPF")
                : null;

            // --- IVA: porcentaje(s) de IVA repercutido de cada factura ---
            // Una factura puede tener líneas con distintos tipos de IVA.
            // Agrupamos como "21 % / 10 %" si tiene varios, "Exento" si Porcentaje==0.
            var necesitaIva = props.Contains(nameof(PorcentajeIva));
            Dictionary<int, string> cacheIva = null;
            if (necesitaIva)
            {
                var lineasConIva = contexto.Set<LineaDeUnaFaeDtm>()
                    .Where(l => ids.Contains(l.IdElemento) && l.IdIvaR != null)
                    .Join(contexto.Set<IvaRepercutidoDtm>(),
                          l => l.IdIvaR, iva => iva.Id,
                          (l, iva) => new { l.IdElemento, iva.Porcentaje, iva.Exento })
                    .ToList();

                cacheIva = lineasConIva
                    .GroupBy(x => x.IdElemento)
                    .ToDictionary(
                        g => g.Key,
                        g =>
                        {
                            var porcentajes = g
                                .Select(x => x.Exento ? "Exento" : x.Porcentaje.ToString("G29") + " %")
                                .Distinct()
                                .OrderBy(s => s)
                                .ToList();
                            return string.Join(" / ", porcentajes);
                        });
            }

            return (prop, registro) =>
            {
                if (prop.Equals(nameof(PorcentajeIrpf), StringComparison.OrdinalIgnoreCase) && cacheIrpf != null)
                    return cacheIrpf.TryGetValue(registro.Id, out var pct) ? pct : "Sin IRPF";

                if (prop.Equals(nameof(PorcentajeIva), StringComparison.OrdinalIgnoreCase) && cacheIva != null)
                    return cacheIva.TryGetValue(registro.Id, out var iva) ? iva : "Sin IVA";

                return null;
            };
        }
    }
}
