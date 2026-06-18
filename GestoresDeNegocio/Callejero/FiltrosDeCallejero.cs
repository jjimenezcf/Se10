using GestorDeElementos;
using ModeloDeDto.Callejero;
using ServicioDeDatos;
using ServicioDeDatos.Callejero;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Gastos;
using System.Collections.Generic;
using System.Linq;
using Utilidades;

namespace GestoresDeNegocio.Callejero
{
    internal static class FiltrosDeCallejero
    {
        public static IQueryable<CalleDtm> FiltroPorMunicipio(this IQueryable<CalleDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == nameof(CalleDto.Municipio).ToLower());
            if (filtro != null)
            {
                if (filtro.Valor.Entero() > 0)
                {
                    consulta = consulta.Where(x => x.IdMunicipio == filtro.Valor.Entero());
                }
                else
                {
                    var municipios = contexto.Set<MunicipioDtm>();
                    consulta = consulta.Where(calle => municipios.Any(mun => calle.IdMunicipio == mun.Id && mun.Nombre.ToLower() == filtro.Valor.ToLower()));
                }
                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<CalleDtm> FiltroPorTipoDeVia(this IQueryable<CalleDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(calle => calle.Clausula.ToLower() == nameof(CalleDto.TipoDeVia).ToLower());
            if (filtro != null)
            {
                if (filtro.Valor.Entero() > 0)
                {
                    consulta = consulta.Where(x => x.IdTipoDeVia == filtro.Valor.Entero());
                }
                else
                {
                    var tiposdevia = contexto.Set<TipoDeViaDtm>();
                    consulta = consulta.Where(calle => tiposdevia.Any(tv => calle.IdTipoDeVia == tv.Id && tv.Nombre.ToLower() == filtro.Valor.ToLower()));
                }
                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<CalleDtm> FiltroPorCp(this IQueryable<CalleDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == nameof(CalleDto.CodigoPostal).ToLower());
            if (filtro != null)
            {
                IQueryable<CpsDeUnaCalleDtm> calleDeUnCp;

                var cps = contexto.Set<CodigoPostalDtm>();
                calleDeUnCp = contexto.Set<CpsDeUnaCalleDtm>().Where(cc => cps.Any(cp => cp.Id == cc.IdCp && cp.Codigo == filtro.Valor));

                consulta = consulta.Where(calle => calleDeUnCp.Any(c => c.IdCalle == calle.Id));
                filtro.Aplicado = true;
            }
            return consulta;
        }

    }
}
