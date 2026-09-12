using System.Linq;
using System.Collections.Generic;
using GestorDeElementos;
using ServicioDeDatos;
using ServicioDeDatos.Logistica;
using Utilidades;

namespace GestoresDeNegocio.Logistica
{
    public static class FiltrosDeMovimientosDeAlmacen
    {
        public static IQueryable<MovimientoDeAlmacenDtm> FiltroPorAlmacen(this IQueryable<MovimientoDeAlmacenDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnMovimientoDeAlmacen.FiltroPorAlmacen.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                consulta = consulta.Where(x => x.IdAlmacen == filtro.Valor.Entero());
                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<MovimientoDeAlmacenDtm> FiltroPorTipoMovimiento(this IQueryable<MovimientoDeAlmacenDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnMovimientoDeAlmacen.FiltroPorTipoMovimiento.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                consulta = consulta.Where(x => x.IdTipoMovimiento == filtro.Valor.Entero());
                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<MovimientoDeAlmacenDtm> FiltroPorUnitario(this IQueryable<MovimientoDeAlmacenDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnMovimientoDeAlmacen.FiltroPorUnitario.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                consulta = consulta.Where(x => x.IdUnitario == filtro.Valor.Entero());
                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<MovimientoDeAlmacenDtm> FiltroPorRealizadoEl(this IQueryable<MovimientoDeAlmacenDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnMovimientoDeAlmacen.FiltroPorRealizadoEl.ToLower() && !x.Aplicado);
            if (filtro != null)
                consulta = consulta.AplicarFiltroEntreFechas(filtro, nameof(MovimientoDeAlmacenDtm.RealizadoEl));
            return consulta;
        }
    }
}
