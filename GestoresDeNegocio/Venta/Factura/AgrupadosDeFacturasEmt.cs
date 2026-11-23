using GestorDeElementos;
using GestorDeElementos.Extensores;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
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

            // Unidades facturadas / devueltas e importe devuelto de los artículos de la factura: agrupando por Articulo, los del artículo del
            // grupo; con :referencia, los de los artículos cuya referencia o nombre la contiene; si no, los de todos sus artículos
            var esUnidadesFacturadas = EsCampo(campo, UnidadesFacturadas, out var materialFacturado);
            var esUnidadesDevueltas = EsCampo(campo, UnidadesDevueltas, out var materialDevuelto);
            var esImporteDevuelto = EsCampo(campo, ImporteDevuelto, out var materialDeImporteDevuelto);
            if (esUnidadesFacturadas || esUnidadesDevueltas || esImporteDevuelto)
            {
                var material = esUnidadesFacturadas ? materialFacturado : esUnidadesDevueltas ? materialDevuelto : materialDeImporteDevuelto;
                var cifras = CifrasDeArticulos.De(factura.Id, contexto)
                    .Where(c => c.Key.CumpleArticulo(metrica, material))
                    .Select(c => c.Value)
                    .ToList();
                if (cifras.Count == 0)
                    return null;
                var valor = esUnidadesFacturadas ? cifras.Sum(c => c.UnidadesFacturadas)
                          : esUnidadesDevueltas ? cifras.Sum(c => c.UnidadesDevueltas)
                          : cifras.Sum(c => c.ImporteDevuelto);
                return valor != 0 ? valor : (decimal?)null;
            }

            // ImporteDeMaterial sin referencia agrupando por Articulo: la base imponible facturada del artículo del grupo
            if (campo == nameof(ImporteDeMaterial).ToLowerInvariant() && ArticuloDelGrupo(metrica) != null)
            {
                var cifras = CifrasDeArticulos.De(factura.Id, contexto).Where(c => c.Key.CumpleArticulo(metrica, "")).Select(c => c.Value).ToList();
                var bi = cifras.Sum(c => c.ImporteFacturado);
                return bi != 0 ? bi : (decimal?)null;
            }

            // ImporteDeMaterial:referencia — base imponible (sin iva, con descuento) de las líneas de la factura del material: las de tipo
            // unitario cuyo unitario tiene esa referencia o nombre y las alzadas cuyo concepto lo contiene (las prefacturas de los documentos
            // de venta llevan como concepto "(referencia) nombre"). Sin referencia, la de todas las líneas con material de un documento de salida
            var prefijoMaterial = nameof(ImporteDeMaterial).ToLowerInvariant();
            if (campo == prefijoMaterial || campo.StartsWith(prefijoMaterial + ":"))
            {
                var material = campo.Length > prefijoMaterial.Length ? campo.Substring(prefijoMaterial.Length + 1).Trim() : "";
                var lineas = contexto.Set<LineaDeUnaFaeDtm>().Where(l => l.IdElemento == factura.Id).ToList();
                if (material.Length == 0)
                {
                    if (!contexto.Set<ServicioDeDatos.Logistica.VentaDtm>().Any(v => v.IdFacturaEmt == factura.Id))
                        return null;
                    var biDeLaVenta = lineas.Sum(l => l.ImporteConDto);
                    return biDeLaVenta != 0 ? biDeLaVenta : (decimal?)null;
                }

                var idsDeUnitarios = contexto.Set<ServicioDeDatos.MaestrosTecnico.UnitarioDtm>()
                    .Where(u => u.Referencia.ToLower().Contains(material) || u.Nombre.ToLower().Contains(material))
                    .Select(u => u.Id)
                    .ToList();
                var bi = lineas
                    .Where(l => (l.IdUnitario != null && idsDeUnitarios.Contains((int)l.IdUnitario))
                             || (l.Concepto != null && l.Concepto.ToLowerInvariant().Contains(material)))
                    .Sum(l => l.ImporteConDto);
                return bi != 0 ? bi : (decimal?)null;
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

        // campo es la etiqueta o la etiqueta seguida de ":referencia" (devuelta en material, en minúsculas; "" si no se indica)
        private static bool EsCampo(string campo, enumEtiquetasDeFacturasEmt etiqueta, out string material)
        {
            var prefijo = etiqueta.ToString().ToLowerInvariant();
            material = "";
            if (campo == prefijo)
                return true;
            if (!campo.StartsWith(prefijo + ":"))
                return false;
            material = campo.Substring(prefijo.Length + 1).Trim();
            return true;
        }

        private static string ArticuloDelGrupo(MetricaDeTotales metrica)
            => metrica.ClavesDelGrupo != null && metrica.ClavesDelGrupo.TryGetValue(nameof(Articulo), out var articulo) ? articulo : null;

        // un artículo cuenta si es el del grupo (agrupando por Articulo) y, si se indica material, su "(referencia) nombre" lo contiene
        private static bool CumpleArticulo(this string articulo, MetricaDeTotales metrica, string material)
        {
            var delGrupo = ArticuloDelGrupo(metrica);
            if (delGrupo != null && articulo != delGrupo)
                return false;
            return material.Length == 0 || articulo.ToLowerInvariant().Contains(material);
        }

        // Claves multivaluadas: Articulo — los artículos de cada factura, de sus líneas y de lo devuelto de sus documentos de venta
        public static Func<string, ElementoDeProcesoDtm, IEnumerable<string>> ObtenerResolverDeClavesMultiples(
            IEnumerable<int> idElementos, ContextoSe contexto, IEnumerable<string> agruparPor)
        {
            if (!agruparPor.Any(p => p.Equals(nameof(Articulo), StringComparison.OrdinalIgnoreCase)))
                return null;

            var cifras = CifrasDeArticulos.Cargar(idElementos, contexto);
            return (prop, registro) =>
            {
                if (!prop.Equals(nameof(Articulo), StringComparison.OrdinalIgnoreCase))
                    return null;
                return cifras.TryGetValue(registro.Id, out var articulos) ? articulos.Keys.ToList() : new List<string>();
            };
        }

        public static string ResolverEtiqueta(
            string prop, object id, ContextoSe contexto,
            List<ITipoDeElementoDtm> tipos, List<EstadoDtm> estados, List<CentroGestorDtm> cgs)
        {
            if (prop.Equals(nameof(PorcentajeIrpf), StringComparison.OrdinalIgnoreCase))
                return id?.ToString() is string s && s.Length > 0 ? s : "Sin IRPF";

            if (prop.Equals(nameof(PorcentajeIva), StringComparison.OrdinalIgnoreCase))
                return id?.ToString() is string v && v.Length > 0 ? v : "Sin IVA";

            if (prop.Equals(nameof(Articulo), StringComparison.OrdinalIgnoreCase))
                return id?.ToString() is string a && a.Length > 0 ? a : "—";

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
