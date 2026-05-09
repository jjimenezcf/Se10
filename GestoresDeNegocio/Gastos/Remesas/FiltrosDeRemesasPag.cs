using System.Linq;
using ServicioDeDatos.Gastos;
using ServicioDeDatos;
using System.Collections.Generic;
using Utilidades;
using GestorDeElementos;

namespace GestoresDeNegocio.Gastos
{
    public static class FiltrosDeRemesasPag
    {

        public static IQueryable<RemesaPagDtm> FiltroPorAcreedor(this IQueryable<RemesaPagDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaRemesaPag.ClaseDeAcreedor.ToLower() && !x.Aplicado);


            if (filtro != null)
            {
                if (filtro.Valor == ltrDeUnaRemesaPag.SoloProveedores)
                {
                    var pagosAProveedores = contexto.Set<PagoDtm>().Where(x => x.IdProveedor != null);
                    var pagosRemesados = contexto.Set<PagoDeUnaRemesaDtm>().Where(x => pagosAProveedores.Any(y => y.Id == x.IdPago));
                    consulta = consulta.Where(x => pagosRemesados.Any(y => y.IdElemento == x.Id));
                }

                if (filtro.Valor == ltrDeUnaRemesaPag.SoloTrabajadores)
                {
                    var pagosATrabajadores = contexto.Set<PagoDtm>().Where(x => x.IdTrabajador != null);
                    var pagosRemesados = contexto.Set<PagoDeUnaRemesaDtm>().Where(x => pagosATrabajadores.Any(y => y.Id == x.IdPago));
                    consulta = consulta.Where(x => pagosRemesados.Any(y => y.IdElemento == x.Id));
                }

                filtro.Aplicado = true;
            }

            filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaRemesaPag.IdAcreedor.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                var pagosAUnAcreedor = contexto.Set<PagoDtm>().Where(x => x.IdSolicitante == filtro.Valor.Entero());
                var pagosRemesados = contexto.Set<PagoDeUnaRemesaDtm>().Where(x => pagosAUnAcreedor.Any(y => y.Id == x.IdPago));
                consulta = consulta.Where(x => pagosRemesados.Any(y => y.IdElemento == x.Id));
            }
            return consulta;

        }

        public static IQueryable<RemesaPagDtm> FiltroParaBuscarRemesasDeUnPago(this IQueryable<RemesaPagDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaRemesaPag.IdPagoEnRemesa.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                consulta = consulta.Where(remesa => contexto.Set<PagoDeUnaRemesaDtm>().Any(b => b.IdPago == filtro.Valor.Entero() && b.IdElemento == remesa.Id));
                filtro.Aplicado = true;
            }
            return consulta;
        }
        public static IQueryable<RemesaPagDtm> FiltroParaBuscarRemesasDeUnaFactura(this IQueryable<RemesaPagDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaRemesaPag.IdFacturaRec.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                var PagosRemesadoDeUnaFactura = contexto.Set<PagoDtm>().Where(x => x.IdFacturaRec == filtro.Valor.Entero() && contexto.Set<PagoDeUnaRemesaDtm>()
                .Any(y => y.IdPago == x.Id));


                consulta = consulta.Where(remesa => contexto.Set<PagoDeUnaRemesaDtm>().Any(b => PagosRemesadoDeUnaFactura.Any(p => p.Id == b.IdPago && b.IdElemento == remesa.Id)));
                filtro.Aplicado = true;
                return consulta;
            }


            filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaRemesaPag.FiltroPorFactura.ToLower() && !x.Aplicado);

            if (filtro != null)
            {
                if (filtro.Valor.Entero() == ltrParametrosNeg.MostrarTodos)
                    filtro.Aplicado = true;
                else
                {
                    var PagosRemesadosDeFacturas = contexto.Set<PagoDtm>()
                        .Where(x => x.IdFacturaRec != null
                        && contexto.Set<PagoDeUnaRemesaDtm>()
                        .Any(y => y.IdPago == x.Id)).Select(x => new { x.Id });                    

                    if (filtro.Valor.Entero() == ltrParametrosNeg.ConRelacion)
                        consulta = consulta.Where(rem => contexto.Set<PagoDeUnaRemesaDtm>().Any(pr => PagosRemesadosDeFacturas.Any(y => y.Id == pr.IdPago) && pr.IdElemento == rem.Id));
                    else
                        consulta = consulta.Where(rem => !contexto.Set<PagoDeUnaRemesaDtm>().Any(pr => PagosRemesadosDeFacturas.Any(y => y.Id == pr.IdPago) && pr.IdElemento == rem.Id));

                    filtro.Aplicado = true;
                }
            }
            return consulta;
        }


        public static IQueryable<RemesaPagDtm> FiltroPorFechaDeGeneracion(this IQueryable<RemesaPagDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaRemesaPag.FiltroPorFechaDeGeneracion.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                consulta = consulta.AplicarFiltroEntreFechas(filtro, nameof(RemesaPagDtm.GeneradaEl));
                consulta = consulta.Where(rem => !(rem.Estado.Inicial || rem.Estado.Cancelado));
            }

            return consulta;
        }

        public static IQueryable<RemesaPagDtm> FiltroPorFechaDePago(this IQueryable<RemesaPagDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaRemesaPag.FiltroPorFechaDePago.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                consulta = consulta.AplicarFiltroEntreFechas(filtro, nameof(RemesaPagDtm.PagarEl));
                consulta = consulta.Where(rem => !(rem.Estado.Inicial || rem.Estado.Cancelado));
            }

            return consulta;
        }

        public static IQueryable<RemesaPagDtm> FiltroPorImporteDeRemesa(this IQueryable<RemesaPagDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaRemesaPag.FiltroPorImporte.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                var pagosDeUnaRemesa = contexto.Set<PagoDeUnaRemesaDtm>();
                var importePorRemesa = pagosDeUnaRemesa
                        .Join(contexto.Set<PagoDtm>(), t1 => t1.IdPago, t2 => t2.Id, (t1, t2) => new { t1, t2 })
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
