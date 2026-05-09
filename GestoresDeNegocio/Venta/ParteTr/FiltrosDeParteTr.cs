using GestorDeElementos;
using ServicioDeDatos.Ventas;
using ServicioDeDatos;
using System;
using System.Linq;
using Utilidades;
using ServicioDeDatos.Juridico;
using System.Collections.Generic;
using ModeloDeDto;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Presupuesto;
using System.Linq.Dynamic.Core;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using Gestor.Errores;

namespace GestoresDeNegocio.Ventas
{
    internal static class FiltrosDeParteTr
    {

        public static IQueryable<ParteTrDtm> AplicarFiltroPorUnitario(this IQueryable<ParteTrDtm> consulta, ContextoSe Contexto, ClausulaDeFiltrado filtro)
        {
            if (filtro.Aplicado) return consulta;
            if (filtro.Clausula.Equals(ltrDeUnParteTr.IdUnitario, StringComparison.CurrentCultureIgnoreCase))
            {
                consulta = consulta.Where(x => Contexto.Set<LineaDeUnPtrDtm>().Any(y => y.IdElemento == x.Id && y.IdUnitario == filtro.Valor.Entero()));
                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<ParteTrDtm> AplicarFiltroPorContratos(this IQueryable<ParteTrDtm> consulta, ClausulaDeFiltrado filtro)
        {
            if (filtro.Aplicado) return consulta;
            if (filtro.Clausula.Equals(ltrDeUnParteTr.FiltroPorConOSinContrato, StringComparison.CurrentCultureIgnoreCase))
            {
                if (filtro.Valor.Entero() == ltrParametrosNeg.ConRelacion)
                    consulta = consulta.Where(x => x.IdContrato != null);
                if (filtro.Valor.Entero() == ltrParametrosNeg.SinRelacion)
                    consulta = consulta.Where(x => x.IdContrato == null);
                filtro.Aplicado = true;
                return consulta;
            }

            if (filtro.Clausula.Equals(ltrDeUnParteTr.IdContrato, StringComparison.CurrentCultureIgnoreCase))
            {
                consulta = consulta.Where(x => x.IdContrato == filtro.Valor.Entero());
                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<ParteTrDtm> AplicarFiltroPorTarea(this IQueryable<ParteTrDtm> consulta, ContextoSe contexto, ClausulaDeFiltrado filtro)
        {
            if (filtro.Aplicado) return consulta;
            if (filtro.Clausula.Equals(ltrDeUnParteTr.FiltroPorConOSinTarea, StringComparison.CurrentCultureIgnoreCase))
            {
                if (filtro.Valor.Entero() == ltrParametrosNeg.ConRelacion)
                    consulta = consulta.Where(x => contexto.Set<TareasDeUnParteTrDtm>().Any(y => y.idElemento1 == x.Id));
                if (filtro.Valor.Entero() == ltrParametrosNeg.SinRelacion)
                    consulta = consulta.Where(x => !contexto.Set<TareasDeUnParteTrDtm>().Any(y => y.idElemento1 == x.Id));
                filtro.Aplicado = true;
            }
            if (filtro.Clausula.Equals(ltrDeUnParteTr.IdTarea, StringComparison.CurrentCultureIgnoreCase))
            {
                consulta = consulta.Where(x => x.Id == contexto.Set<TareasDeUnParteTrDtm>().First(y => y.idElemento2 == filtro.Valor.Entero()).idElemento1);
                filtro.Aplicado = true;
            }

            return consulta;
        }

        public static IQueryable<ParteTrDtm> AplicarFiltroPorPpt(this IQueryable<ParteTrDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnParteTr.IdPresupuesto.ToLower());
            if (filtro != null)
            {
                consulta = consulta.AplicarFiltroPorEntero(filtro);
                return consulta;
            }

            filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnParteTr.FiltroPorConOSinPresupuesto.ToLower());
            return filtro != null ? consulta.FiltroSiHayDependenciaDe(filtro, ltrDeUnParteTr.IdPresupuesto) : consulta;
        }

        public static IQueryable<ParteTrDtm> EjecutadoDeUnPpt(this IQueryable<ParteTrDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnParteTr.EjecutadoDeUnPpt.ToLower());
            if (filtro != null)
            {
                var listaEstados = enumEtapasDePartesTr.PTR_Etapa_Pdt_Facturar.Lista();

                if (!listaEstados.Any())
                    GestorDeErrores.Emitir($"Debe definir el parámetro '{enumEtapasDePartesTr.PTR_Etapa_Pdt_Facturar}' del negocio de '{enumNegocio.ParteDeTrabajo}'");

                consulta = consulta.Where(ptr => ptr.IdPresupuesto == filtro.Valor.Entero() && listaEstados.Contains(ptr.IdEstado));
                filtro.Aplicado = true;
            }

            return consulta;
        }


        public static IQueryable<ParteTrDtm> AplicarFiltroPorFacturaEmt(this IQueryable<ParteTrDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnParteTr.IdFacturaEmt.ToLower());
            if (filtro != null)
            {
                consulta = consulta.AplicarFiltroPorEntero(filtro);
                return consulta;
            }

            filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnParteTr.FiltroPorConOSinFacturaEmt.ToLower());
            if (filtro != null)
            {
                if (filtro.Valor.Entero() == ltrParametrosNeg.ConRelacion)
                    consulta = consulta.Where(x => x.IdFacturaEmt != null || contexto.Set<LineaDeUnaFaeDtm>().Any(l => l.IdParteTr == x.Id));
                if (filtro.Valor.Entero() == ltrParametrosNeg.SinRelacion)
                    consulta = consulta.Where(x => x.IdFacturaEmt == null || contexto.Set<LineaDeUnaFaeDtm>().All(l => l.IdParteTr != x.Id));
                filtro.Aplicado = true;
            }

            return consulta;
        }

        public static IQueryable<ParteTrDtm> AplicarFiltroPorPlfDeVenta(this IQueryable<ParteTrDtm> consulta, ContextoSe Contexto, ClausulaDeFiltrado filtro)
        {
            if (filtro.Aplicado) return consulta;
            if (filtro.Clausula.Equals(ltrDeUnParteTr.FiltroPorConOSinPlfDeVenta, StringComparison.CurrentCultureIgnoreCase))
            {
                if (filtro.Valor.Entero() == ltrParametrosNeg.ConRelacion)
                    consulta = consulta.Where(x => Contexto.Set<PlanificacionDeVentaDtm>().Any(y => y.IdParteTr == x.Id));
                if (filtro.Valor.Entero() == ltrParametrosNeg.SinRelacion)
                    consulta = consulta.Where(x => !Contexto.Set<PlanificacionDeVentaDtm>().Any(y => y.IdParteTr == x.Id));
                filtro.Aplicado = true;
                return consulta;
            }

            if (filtro.Clausula.Equals(ltrDeUnParteTr.IdPlfDeVenta, StringComparison.CurrentCultureIgnoreCase))
            {
                consulta = consulta.Where(x => x.Id == Contexto.Set<PlanificacionDeVentaDtm>().First(y => y.Id == filtro.Valor.Entero()).IdParteTr);
                filtro.Aplicado = true;
            }
            return consulta;
        }

        //public static IQueryable<ParteTrDtm> SeleccionarPartesTrPdtFacturar(this IQueryable<ParteTrDtm> consulta, List<ClausulaDeFiltrado> filtros)
        //{
        //    var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrFiltros.VincularCon);
        //    if (filtro == null) return consulta;
        //    var filtroPorEtapas = new ClausulaDeFiltrado(ltrDeUnParteTr.FiltroPorEtapa, enumCriteriosDeFiltrado.igual, enumEtapasDePartesTr.PTR_Etapa_Pdt_Facturar);

        //    consulta = consulta.AplicarFiltroPorEtapa(filtroPorEtapas);
        //    filtro.Aplicado = true;
        //    return consulta;
        //}

        public static IQueryable<ParteTrDtm> AplicarFiltroPorEtapas(this IQueryable<ParteTrDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnParteTr.FiltroPorEtapa.ToLower() && !x.Aplicado);
            if (filtro == null) return consulta;
            return consulta.AplicarFiltroPorEtapa(filtro);
        }

        private static IQueryable<ParteTrDtm> AplicarFiltroPorEtapa(this IQueryable<ParteTrDtm> consulta, ClausulaDeFiltrado filtro)
        {
            if (filtro.Clausula.Equals(ltrDeUnParteTr.FiltroPorEtapa, StringComparison.CurrentCultureIgnoreCase) && filtro.Criterio == enumCriteriosDeFiltrado.igual)
            {
                var filtroPorEstados = new ClausulaDeFiltrado(nameof(IUsaEstado.IdEstado), enumCriteriosDeFiltrado.esAlgunoDe, ApiDeEnsamblados.ToEnumerado<enumEtapasDePartesTr>(filtro.Valor).Estados());
                consulta = consulta.AplicarFiltroPorEntero(filtroPorEstados);
                filtro.Aplicado = true;
                return consulta;
            }
            return consulta;
        }
    }
}
