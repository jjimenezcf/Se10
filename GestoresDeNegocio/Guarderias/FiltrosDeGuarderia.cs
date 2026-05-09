using ServicioDeDatos;
using System;
using System.Collections.Generic;
using System.Linq;
using ServicioDeDatos.Guarderias;
using GestorDeElementos;
using Utilidades;
using ModeloDeDto.Guarderias;
using ServicioDeDatos.Elemento;

namespace GestoresDeNegocio.Guarderias
{
    internal static class FiltrosDeGuarderia
    {
        public static IQueryable<CursoDeGuarderiaDtm> FiltrarPorCursosActivos(this IQueryable<CursoDeGuarderiaDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtrarPorActivo = filtros.FirstOrDefault(filtro => filtro.Clausula.ToLower() == ltrDeCursosDeGuarderia.FiltrarPorActivo.ToLower());
            if (filtrarPorActivo != null && !filtrarPorActivo.Aplicado)
            {
                if (!filtros.Exists(filtro => filtro.Clausula.ToLower() == ltrDeCursosDeGuarderia.FiltrarPorPeriodo.ToLower()))
                    filtros.Add(new ClausulaDeFiltrado
                    {
                        Clausula = ltrDeCursosDeGuarderia.FiltrarPorPeriodo,
                        Criterio = filtrarPorActivo.Criterio == enumCriteriosDeFiltrado.igual ? enumCriteriosDeFiltrado.contiene : enumCriteriosDeFiltrado.noContiene,
                        Valor = $"{DateTime.Now.Date}{Simbolos.separadorDeFechas}{DateTime.Now.Date}"
                    });
                consulta = consulta.FiltrarPorPeriodo(contexto, filtros);
                filtrarPorActivo.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<CursoDeGuarderiaDtm> FiltrarPorInfante(this IQueryable<CursoDeGuarderiaDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtroPorInfante = filtros.FirstOrDefault(filtro => filtro.Clausula.ToLower() == ltrDeCursosDeGuarderia.FiltrarPorInfante.ToLower());
            if (filtroPorInfante != null && !filtroPorInfante.Aplicado)
            {
                var cursosDeUnInfante = contexto.Set<InfanteDeUnCursoDtm>().Where(cursos => cursos.IdInfante == filtroPorInfante.Valor.Entero());
                consulta = consulta.Where(curso => cursosDeUnInfante.Any(elCurso => elCurso.Id == curso.Id));
                filtroPorInfante.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<InfanteDtm> FiltrarPorInfanteSinCurso(this IQueryable<InfanteDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtroPorInfante = filtros.FirstOrDefault(filtro => filtro.Clausula.ToLower() == ltrDeInfante.SeleccionarParaUnCurso.ToLower());
            if (filtroPorInfante != null && !filtroPorInfante.Aplicado)
            {
                var infantesAsignados = contexto.Set<InfanteDeUnCursoDtm>().Select(x => x.IdInfante);
                consulta = consulta.Where(infante => !infantesAsignados.Contains(infante.Id));
                filtroPorInfante.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<CursoDeGuarderiaDtm> FiltrarParaAsociarCurso(this IQueryable<CursoDeGuarderiaDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(filtro => filtro.Clausula.ToLower() == ltrDeCursosDeGuarderia.FiltrarParaAsociarCurso.ToLower());
            if (filtro != null && !filtro.Aplicado)
            {
                var filtroDeInfante = filtros.FirstOrDefault(x => x.Clausula.ToLower() == nameof(AsociarCursoDto.IdInfante).ToLower());
                if (filtroDeInfante != null)
                {
                    filtroDeInfante.Aplicado = true;
                    var cursoDeInfante = contexto.Set<InfanteDeUnCursoDtm>().Where(x => x.IdInfante == filtroDeInfante.Valor.Entero());
                    consulta = consulta.Where(x => !cursoDeInfante.Any(y => y.IdElemento == x.Id));
                }
                filtros.Add(new ClausulaDeFiltrado(nameof(INombre.Nombre), enumCriteriosDeFiltrado.contiene, filtro.Valor));
                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<CursoDeGuarderiaDtm> FiltrarPorPeriodo(this IQueryable<CursoDeGuarderiaDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtrarPorPeriodo = filtros.FirstOrDefault(filtro => filtro.Clausula.ToLower() == ltrDeCursosDeGuarderia.FiltrarPorPeriodo.ToLower());
            if (filtrarPorPeriodo != null && !filtrarPorPeriodo.Aplicado)
            {
                consulta = filtrarPorPeriodo.Criterio == enumCriteriosDeFiltrado.contiene
                    ? consulta.AplicarFiltroPorEstarEnElPeriodo(filtrarPorPeriodo, $"{nameof(CursoDeGuarderiaDtm.Inicio)}{Simbolos.separadorDePeriodos}{nameof(CursoDeGuarderiaDtm.Fin)}")
                    : consulta.AplicarFiltroPorNoEstarEnElPeriodo(filtrarPorPeriodo, $"{nameof(CursoDeGuarderiaDtm.Inicio)}{Simbolos.separadorDePeriodos}{nameof(CursoDeGuarderiaDtm.Fin)}");
                filtrarPorPeriodo.Aplicado = true;
            }
            return consulta;
        }
    }
}
