using GestorDeElementos;
using ServicioDeDatos;
using ServicioDeDatos.Logistica;
using System.Collections.Generic;
using System.Linq;

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
    }
}
