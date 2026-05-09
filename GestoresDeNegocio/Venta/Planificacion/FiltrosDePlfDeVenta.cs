using GestorDeElementos;
using ServicioDeDatos.Ventas;
using ServicioDeDatos;
using System;
using System.Linq;
using Utilidades;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Juridico;
using System.Collections.Generic;
using static ServicioDeDatos.Ventas.VariablesDePlfsDeVenta;

namespace GestoresDeNegocio.Ventas
{
    internal static class FiltrosDePlfDeVenta
    {

        public static IQueryable<PlanificacionDeVentaDtm> AplicarFiltroPorUnitario(this IQueryable<PlanificacionDeVentaDtm> consulta, ContextoSe Contexto, ClausulaDeFiltrado filtro)
        {
            if (filtro.Aplicado) return consulta;
            if (filtro.Clausula.Equals(ltrDeUnaPlanificacionDeVenta.IdUnitario, StringComparison.CurrentCultureIgnoreCase))
            {
                consulta = consulta.Where(x => Contexto.Set<LineaDeUnaPlfVentaDtm>().Any(y => y.IdElemento == x.Id && y.IdUnitario == filtro.Valor.Entero()));
                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<PlanificacionDeVentaDtm> AplicarFiltroPorPlanificador(this IQueryable<PlanificacionDeVentaDtm> consulta, ClausulaDeFiltrado filtro)
        {
            if (filtro.Aplicado) return consulta;
            if (filtro.Clausula.Equals(ltrDeUnaPlanificacionDeVenta.FiltroPorConOSinPlanificador, StringComparison.CurrentCultureIgnoreCase))
            {
                if (filtro.Valor.Entero() == ltrParametrosNeg.ConRelacion)
                    consulta = consulta.Where(x => x.IdPlanificador != null);
                if (filtro.Valor.Entero() == ltrParametrosNeg.SinRelacion)
                    consulta = consulta.Where(x => x.IdPlanificador == null);
                filtro.Aplicado = true;
                return consulta;
            }

            if (filtro.Clausula.Equals(ltrDeUnaPlanificacionDeVenta.IdPlanificador, StringComparison.CurrentCultureIgnoreCase))
            {
                consulta = consulta.Where(x => x.IdPlanificador == filtro.Valor.Entero());
                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<PlanificacionDeVentaDtm> AplicarFiltroPorContratos(this IQueryable<PlanificacionDeVentaDtm> consulta, ClausulaDeFiltrado filtro)
        {
            if (filtro.Aplicado) return consulta;
            if (filtro.Clausula.Equals(ltrDeUnaPlanificacionDeVenta.FiltroPorConOSinContrato, StringComparison.CurrentCultureIgnoreCase))
            {
                if (filtro.Valor.Entero() == ltrParametrosNeg.ConRelacion)
                    consulta = consulta.Where(x => x.IdContrato != null);
                if (filtro.Valor.Entero() == ltrParametrosNeg.SinRelacion)
                    consulta = consulta.Where(x => x.IdContrato == null);
                filtro.Aplicado = true;
                return consulta;
            }

            if (filtro.Clausula.Equals(ltrDeUnaPlanificacionDeVenta.IdContrato, StringComparison.CurrentCultureIgnoreCase))
            {
                consulta = consulta.Where(x => x.IdContrato == filtro.Valor.Entero());
                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<PlanificacionDeVentaDtm> AplicarFiltroPorPartesTr(this IQueryable<PlanificacionDeVentaDtm> consulta, ClausulaDeFiltrado filtro)
        {
            if (filtro.Aplicado) return consulta;
            if (filtro.Clausula.Equals(ltrDeUnaPlanificacionDeVenta.FiltroPorConOSinParteTr, StringComparison.CurrentCultureIgnoreCase))
            {
                if (filtro.Valor.Entero() == ltrParametrosNeg.ConRelacion)
                    consulta = consulta.Where(x => x.IdParteTr != null);
                if (filtro.Valor.Entero() == ltrParametrosNeg.SinRelacion)
                    consulta = consulta.Where(x => x.IdParteTr == null);
                filtro.Aplicado = true;
                return consulta;
            }

            if (filtro.Clausula.Equals(ltrDeUnaPlanificacionDeVenta.IdParteTr, StringComparison.CurrentCultureIgnoreCase))
            {
                consulta = consulta.Where(x => x.IdParteTr == filtro.Valor.Entero());
                filtro.Aplicado = true;
            }

            return consulta;
        }

        public static IQueryable<PlanificacionDeVentaDtm> AplicarFiltroPorFacturaEmt(this IQueryable<PlanificacionDeVentaDtm> consulta, ClausulaDeFiltrado filtro)
        {
            if (filtro.Aplicado) return consulta;
            if (filtro.Clausula.Equals(ltrDeUnaPlanificacionDeVenta.FiltroPorConOSinFacturaEmt, StringComparison.CurrentCultureIgnoreCase))
            {
                if (filtro.Valor.Entero() == ltrParametrosNeg.ConRelacion)
                    consulta = consulta.Where(x => x.IdFacturaEmt != null);
                if (filtro.Valor.Entero() == ltrParametrosNeg.SinRelacion)
                    consulta = consulta.Where(x => x.IdFacturaEmt == null);
                filtro.Aplicado = true;
                return consulta;
            }

            if (filtro.Clausula.Equals(ltrDeUnaPlanificacionDeVenta.IdFacturaEmt, StringComparison.CurrentCultureIgnoreCase))
            {
                consulta = consulta.Where(x => x.IdFacturaEmt == filtro.Valor.Entero());
                filtro.Aplicado = true;
            }

            return consulta;
        }

        public static IQueryable<PlanificacionDeVentaDtm> AplicarFiltroPorEtapa(this IQueryable<PlanificacionDeVentaDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaPlanificacionDeVenta.FiltroPorEtapa.ToLower() && !x.Aplicado);
            return filtro != null ? consulta.AplicarFiltroPorEtapa(filtro) : consulta;
        }

        private static IQueryable<PlanificacionDeVentaDtm> AplicarFiltroPorEtapa(this IQueryable<PlanificacionDeVentaDtm> consulta, ClausulaDeFiltrado filtro)
        {
            if (filtro.Clausula.Equals(ltrDeUnaPlanificacionDeVenta.FiltroPorEtapa, StringComparison.CurrentCultureIgnoreCase) && filtro.Criterio == enumCriteriosDeFiltrado.igual)
            {
                var filtroPorEstados = new ClausulaDeFiltrado(nameof(IUsaEstado.IdEstado), enumCriteriosDeFiltrado.esAlgunoDe, ApiDeEnsamblados.ToEnumerado<enumEtapasDePlfsDeVenta>(filtro.Valor).Estados());
                consulta = consulta.AplicarFiltroPorEntero(filtroPorEstados);
                filtro.Aplicado = true;
                return consulta;
            }
            return consulta;
        }
    }
}
