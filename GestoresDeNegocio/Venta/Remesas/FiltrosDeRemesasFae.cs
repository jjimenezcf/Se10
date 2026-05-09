using System.Linq;
using ServicioDeDatos.Ventas;
using ServicioDeDatos;
using System.Collections.Generic;
using Utilidades;
using GestorDeElementos;

namespace GestoresDeNegocio.Ventas
{
    public static class FiltrosDeRemesasFae
    {


        public static IQueryable<RemesaFaeDtm> FiltroParaBuscarRemesasDeUnaFactura(this IQueryable<RemesaFaeDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaRemesaFae.IdFacturaEnRemesa.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                consulta = consulta.Where(remesa => contexto.Set<FacturaEmtDeUnaRemesaDtm>().Any(b => b.IdFactura == filtro.Valor.Entero() && b.IdElemento == remesa.Id));
                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<RemesaFaeDtm> FiltroPorFechaDeGeneracion(this IQueryable<RemesaFaeDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaRemesaFae.FiltroPorFechaDeGeneracion.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                consulta = consulta.AplicarFiltroEntreFechas(filtro, nameof(RemesaFaeDtm.GeneradaEl));
                consulta = consulta.Where(rem => !(rem.Estado.Inicial || rem.Estado.Cancelado));
            }

            return consulta;
        }

        public static IQueryable<RemesaFaeDtm> FiltroPorFechaDeCargo(this IQueryable<RemesaFaeDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaRemesaFae.FiltroPorFechaDeCargo.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                consulta = consulta.AplicarFiltroEntreFechas(filtro, nameof(RemesaFaeDtm.CargarEl));
                consulta = consulta.Where(rem => !(rem.Estado.Inicial || rem.Estado.Cancelado));
            }

            return consulta;
        }

        public static IQueryable<RemesaFaeDtm> FiltroPorImporteDeRemesa(this IQueryable<RemesaFaeDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaRemesaFae.FiltroPorImporte.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                var importePorFactura = contexto.Set<LineaDeUnaFaeDtm>().GroupBy(lin => lin.IdElemento)
                    .Select(tot => new { 
                        IdFactura = tot.Key, 
                        Importe = tot.Sum(l => (l.Cantidad * l.Precio) 
                                             - (l.Cantidad * l.Precio * (l.Descuento == null ? 0 :(decimal) l.Descuento) / 100)
                                             + ((l.Cantidad * l.Precio) - (l.Cantidad * l.Precio * (l.Descuento == null ? 0 : (decimal)l.Descuento) / 100)) * (l.Iva == null ? 0 : (decimal)l.Iva/100)
                                         ) 
                    });

                var facturasDeUnaRemesa = contexto.Set<FacturaEmtDeUnaRemesaDtm>();
                var importePorRemesa = facturasDeUnaRemesa
                        .Join(importePorFactura, t1 => t1.IdFactura, t2 => t2.IdFactura, (t1, t2) => new { t1, t2 })
                        .GroupBy(x => x.t1.IdElemento)
                        .Select(grouped => new
                        {
                            IdRemesa = grouped.Key,
                            Importe = grouped.Sum(x => x.t2.Importe)
                        });

                var rango = filtro.ParsearRango();
                consulta = consulta.Where(rem => importePorRemesa.Any(sum => sum.IdRemesa == rem.Id &&
                                            (rango.desde == default ? true : (decimal)rango.desde <= sum.Importe) &&
                                            (rango.hasta == default ? true : (decimal)rango.hasta >= sum.Importe)));
                filtro.Aplicado = true;
            }
            return consulta;
        }
    }
}
