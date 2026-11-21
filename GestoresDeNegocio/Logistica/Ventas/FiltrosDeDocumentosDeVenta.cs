using GestorDeElementos;
using ServicioDeDatos;
using ServicioDeDatos.Logistica;
using System.Collections.Generic;
using System.Linq;
using Utilidades;

namespace GestoresDeNegocio.Logistica
{
    public static class FiltrosDeDocumentosDeVenta
    {
        // documentos de venta prefacturados en una factura emitida (al navegar desde las relaciones de la factura) o con/sin factura.
        // Al filtrar por su factura se omite el filtro por estado (se muestran aunque estén facturados, que es una etapa terminada)
        public static IQueryable<VentaDtm> FiltroPorFactura(this IQueryable<VentaDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            return consulta.FiltroSiHayDependenciaDe(filtros, filtrarPor: ltrDeUnaVenta.IdFacturaEmt, filtroDeAsociacion: ltrDeUnaVenta.Facturado, parametros, aplicarFiltroDeEstado: false);
        }

        // documentos de venta con alguna línea del unitario
        public static IQueryable<VentaDtm> FiltroPorUnitario(this IQueryable<VentaDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaVenta.FiltroPorUnitario.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                var idUnitario = filtro.Valor.Entero();
                consulta = consulta.Where(v => contexto.Set<LineaDeUnaVentaDtm>().Any(l => l.IdElemento == v.Id && l.IdUnitario == idUnitario));
                filtro.Aplicado = true;
            }
            return consulta;
        }

        // Base imponible del documento de venta (sin iva): lo vendido en sus líneas con su descuento, como LineaDeUnaVentaDtm.Importe; un
        // documento sin líneas suma 0
        public static IQueryable<VentaDtm> FiltroPorImporte(this IQueryable<VentaDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaVenta.FiltroPorImporte.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                var (desde, hasta) = filtro.ParsearRango();
                consulta = consulta.Where(v => (desde == null || desde <=
                                                    (contexto.Set<LineaDeUnaVentaDtm>().Where(l => l.IdElemento == v.Id)
                                                        .Sum(l => (decimal?)(l.Precio * l.Cantidad * (1 - (l.Descuento ?? 0) / 100))) ?? 0))
                                            && (hasta == null || hasta >=
                                                    (contexto.Set<LineaDeUnaVentaDtm>().Where(l => l.IdElemento == v.Id)
                                                        .Sum(l => (decimal?)(l.Precio * l.Cantidad * (1 - (l.Descuento ?? 0) / 100))) ?? 0)));
                filtro.Aplicado = true;
            }
            return consulta;
        }
    }
}
