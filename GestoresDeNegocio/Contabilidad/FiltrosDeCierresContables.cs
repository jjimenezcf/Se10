using GestorDeElementos;
using ModeloDeDto.Contabilidad;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using System.Collections.Generic;
using System.Linq;
using Utilidades;

namespace GestoresDeNegocio.Contabilidad
{
    public static class FiltrosDeCierresContables
    {
        // Lista de valores de sociedades del mantenimiento: sin sociedad seleccionada no se filtra
        public static IQueryable<CierreContableDtm> FiltrarPorSociedad(this IQueryable<CierreContableDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(f => f.Clausula.ToLower() == nameof(CierreContableDtm.IdSociedad).ToLower());
            if (filtro != null)
            {
                var idSociedad = filtro.Valor.Entero();
                if (idSociedad > 0)
                    consulta = consulta.Where(x => x.IdSociedad == idSociedad);

                filtro.Aplicado = true;
            }
            return consulta;
        }

        // Filtro entre fechas sobre la fecha en la que se realizó el cierre
        public static IQueryable<CierreContableDtm> FiltrarPorFechaDeCierre(this IQueryable<CierreContableDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(f => f.Clausula.ToLower() == nameof(CierreContableDtm.CerradaEl).ToLower());
            if (filtro != null)
                consulta = consulta.AplicarFiltroPorFecha(filtro, nameof(CierreContableDtm.CerradaEl));

            return consulta;
        }

        // Busca por la referencia o el nombre del lote contable del cierre
        public static IQueryable<CierreContableDtm> FiltrarPorLote(this IQueryable<CierreContableDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(f => f.Clausula.ToLower() == ltrDeUnCierreContable.FiltroPorLote.ToLower());
            if (filtro != null)
            {
                var valor = filtro.Valor.Trim();
                if (!valor.IsNullOrEmpty())
                    consulta = consulta.Where(x => x.Lote.Referencia.Contains(valor) || x.Lote.Nombre.Contains(valor));

                filtro.Aplicado = true;
            }
            return consulta;
        }
    }
}
