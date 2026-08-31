using System;
using System.Collections.Generic;
using System.Linq;
using GestorDeElementos;
using ModeloDeDto;
using ServicioDeDatos;
using ServicioDeDatos.Logistica;
using Utilidades;

namespace GestoresDeNegocio.Logistica
{
    internal static class FiltrosDeAlmacen
    {
        public static IQueryable<AlmacenDtm> FiltrarParaRegularizar(this IQueryable<AlmacenDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.Equals(ltrDeUnAlmacen.FiltrarParaRegularizar, StringComparison.CurrentCultureIgnoreCase));
            if (filtro != null)
            {
                consulta = consulta.AplicarFiltroDeCadena(filtro);
                filtro.Aplicado = false;
                filtro.Clausula = ltrFiltros.FiltroPorEtapa;
                filtro.Criterio = enumCriteriosDeFiltrado.igual;
                filtro.Valor = nameof(enumEtapasDeAlmacen.ALM_Etapa_Activo);
                consulta = consulta.FiltrosPorEtapas(enumNegocio.Almacen, filtros);
            }
            return consulta;
        }
    }
}
