using ServicioDeDatos;
using ServicioDeDatos.Logistica;
using ServicioDeDatos.MaestrosTecnico;
using ServicioDeDatos.Ventas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace GestoresDeNegocio.Ventas
{
    // Lo facturado y lo devuelto de cada artículo en una factura emitida
    internal class CifrasDeUnArticulo
    {
        public decimal UnidadesFacturadas { get; set; }
        public decimal ImporteFacturado { get; set; }
        public decimal UnidadesDevueltas { get; set; }
        public decimal ImporteDevuelto { get; set; }
    }

    // Cifras por artículo de las facturas emitidas para las preguntas a la IA (agrupación por Articulo y métricas UnidadesFacturadas,
    // UnidadesDevueltas, ImporteDevuelto e ImporteDeMaterial, ver AgrupadosDeFacturasEmt). El artículo es el "(referencia) nombre" del unitario:
    //  - facturado: líneas de tipo unitario (su unitario) y las que facturan una línea de un documento de venta (IdLineaDeVenta: el unitario
    //    vendido; las prefacturas anteriores las crearon alzadas, ver ExtensorDeVentas.CrearPrefactura, y se copian en rectificativas y sustitutas). Las unidades llevan el
    //    signo del importe, así que las rectificativas restan
    //  - devuelto: líneas de las devoluciones de venta aceptadas de los documentos de venta que apuntan a la factura (IdFacturaEmt)
    // Se cachean por contexto (una petición) para no releer en cada grupo y métrica
    internal static class CifrasDeArticulos
    {
        private static readonly ConditionalWeakTable<ContextoSe, Dictionary<int, Dictionary<string, CifrasDeUnArticulo>>> _cache = new();

        public static Dictionary<string, CifrasDeUnArticulo> De(int idFactura, ContextoSe contexto)
        {
            var cache = _cache.GetOrCreateValue(contexto);
            if (!cache.ContainsKey(idFactura))
                Cargar(new[] { idFactura }, contexto);
            return cache[idFactura];
        }

        public static Dictionary<int, Dictionary<string, CifrasDeUnArticulo>> Cargar(IEnumerable<int> idsDeFacturas, ContextoSe contexto)
        {
            var cache = _cache.GetOrCreateValue(contexto);
            var ids = idsDeFacturas.Distinct().Where(id => !cache.ContainsKey(id)).ToList();
            foreach (var id in ids)
                cache[id] = new Dictionary<string, CifrasDeUnArticulo>();

            if (ids.Count > 0)
            {
                CargarFacturado(ids, contexto, cache);
                CargarDevuelto(ids, contexto, cache);
            }

            return cache;
        }

        private static void CargarFacturado(List<int> ids, ContextoSe contexto, Dictionary<int, Dictionary<string, CifrasDeUnArticulo>> cache)
        {
            var lineas = contexto.Set<LineaDeUnaFaeDtm>()
                .Where(l => ids.Contains(l.IdElemento) && (l.IdUnitario != null || l.IdLineaDeVenta != null))
                .ToList();

            // el unitario de las líneas alzadas que facturan una línea vendida es el de ésta
            var idsDeLineasVendidas = lineas.Where(l => l.IdUnitario == null).Select(l => (int)l.IdLineaDeVenta).Distinct().ToList();
            var unitarioDeLaVendida = contexto.Set<LineaDeUnaVentaDtm>()
                .Where(l => idsDeLineasVendidas.Contains(l.Id))
                .Select(l => new { l.Id, l.IdUnitario })
                .ToList()
                .ToDictionary(l => l.Id, l => l.IdUnitario);

            int? IdUnitarioDe(LineaDeUnaFaeDtm l) => l.IdUnitario ?? (unitarioDeLaVendida.TryGetValue((int)l.IdLineaDeVenta, out var id) ? (int?)id : null);

            var idsDeUnitarios = lineas.Select(IdUnitarioDe).Where(id => id != null).Select(id => (int)id).Distinct().ToList();
            var articuloDelUnitario = contexto.Set<UnitarioDtm>().Where(u => idsDeUnitarios.Contains(u.Id)).ToList().ToDictionary(u => u.Id, u => u.Expresion);

            foreach (var linea in lineas)
            {
                var idUnitario = IdUnitarioDe(linea);
                if (idUnitario == null || !articuloDelUnitario.TryGetValue((int)idUnitario, out var articulo))
                    continue;

                var importe = linea.ImporteConDto;
                var signo = importe != 0 ? Math.Sign(importe) : Math.Sign(linea.Cantidad ?? 0);
                var cifras = Cifras(cache, linea.IdElemento, articulo);
                cifras.UnidadesFacturadas += Math.Abs(linea.Cantidad ?? 0) * signo;
                cifras.ImporteFacturado += importe;
            }
        }

        private static void CargarDevuelto(List<int> ids, ContextoSe contexto, Dictionary<int, Dictionary<string, CifrasDeUnArticulo>> cache)
        {
            var ventas = contexto.Set<VentaDtm>()
                .Where(v => v.IdFacturaEmt != null && ids.Contains((int)v.IdFacturaEmt))
                .Select(v => new { v.Id, IdFactura = (int)v.IdFacturaEmt })
                .ToList();
            if (ventas.Count == 0)
                return;

            var idsDeVentas = ventas.Select(v => v.Id).ToList();
            var facturaDeLaVenta = ventas.ToDictionary(v => v.Id, v => v.IdFactura);
            var lineasDeVenta = contexto.Set<LineaDeUnaVentaDtm>()
                .Where(l => idsDeVentas.Contains(l.IdElemento))
                .Select(l => new { l.Id, l.IdElemento, l.IdUnitario })
                .ToList();
            var idsDeLineas = lineasDeVenta.Select(l => l.Id).ToList();

            var aceptadas = enumEtapasDeDevolucionDeVenta.DDV_Etapa_Aceptada.Lista();
            var devueltas = contexto.Set<LineaDeUnaDevolucionDeVentaDtm>()
                .Where(l => idsDeLineas.Contains(l.IdLineaDeVenta)
                         && contexto.Set<DevolucionDeVentaDtm>().Any(d => d.Id == l.IdElemento && aceptadas.Contains(d.IdEstado)))
                .ToList();
            if (devueltas.Count == 0)
                return;

            var idsDeUnitarios = lineasDeVenta.Select(l => l.IdUnitario).Distinct().ToList();
            var articuloDelUnitario = contexto.Set<UnitarioDtm>().Where(u => idsDeUnitarios.Contains(u.Id)).ToList().ToDictionary(u => u.Id, u => u.Expresion);
            var lineaDeVenta = lineasDeVenta.ToDictionary(l => l.Id);

            foreach (var devuelta in devueltas)
            {
                var vendida = lineaDeVenta[devuelta.IdLineaDeVenta];
                if (!articuloDelUnitario.TryGetValue(vendida.IdUnitario, out var articulo))
                    continue;
                var cifras = Cifras(cache, facturaDeLaVenta[vendida.IdElemento], articulo);
                cifras.UnidadesDevueltas += devuelta.Cantidad;
                cifras.ImporteDevuelto += devuelta.Importe;
            }
        }

        private static CifrasDeUnArticulo Cifras(Dictionary<int, Dictionary<string, CifrasDeUnArticulo>> cache, int idFactura, string articulo)
        {
            var articulos = cache[idFactura];
            if (!articulos.TryGetValue(articulo, out var cifras))
                articulos[articulo] = cifras = new CifrasDeUnArticulo();
            return cifras;
        }
    }
}
