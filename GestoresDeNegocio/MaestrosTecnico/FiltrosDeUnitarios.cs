using System;
using System.Linq;
using System.Collections.Generic;
using GestorDeElementos;
using ServicioDeDatos;
using ServicioDeDatos.MaestrosTecnico;
using ServicioDeDatos.Juridico;
using Utilidades;

namespace GestoresDeNegocio.MaestrosTecnico
{
    public static class ltrDeUnUnitario
    {
        public const string PreciosDelLote = nameof(PreciosDelLote);
        public const string IdPlanificador = nameof(IdPlanificador);
        public const string FiltrosPorClaseDeUnitario = nameof(FiltrosPorClaseDeUnitario);
    }

    internal static class FiltrosDeUnitarios
    {
        public static IQueryable<UnitarioDtm> FiltrarPorLote(this IQueryable<UnitarioDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            var soloLosDelLote = filtros.Where(x => x.Clausula.Equals(nameof(UnitariosDeUnLoteDtm.IdLote), StringComparison.InvariantCultureIgnoreCase) && x.Criterio == enumCriteriosDeFiltrado.igual).FirstOrDefault();
            if (soloLosDelLote != null)
            {
                var idLote = soloLosDelLote.Valor.Entero();
                var idPlanificador = (int)parametros.Parametros.LeerValor<long>(ltrDeUnUnitario.IdPlanificador, 0);
                var unitariosDelLote = contexto.Set<UnitariosDeUnLoteDtm>().Where(x => x.IdLote == idLote);
                var lineasPlanificadas = contexto.Set<LineaDeUnPlfVentaDtm>().Where(x => x.IdElemento == idPlanificador);
                consulta = consulta.Where(x => unitariosDelLote.Any(y => y.IdUnitario == x.Id) && lineasPlanificadas.All(y => y.IdUnitario != x.Id));
                soloLosDelLote.Aplicado = true;
                parametros.Parametros[ltrDeUnUnitario.PreciosDelLote] = idLote;
            }

            var noEstanEnElLote = filtros.Where(x => x.Clausula.Equals(nameof(UnitariosDeUnLoteDtm.IdLote), StringComparison.InvariantCultureIgnoreCase)
            && (x.Criterio == enumCriteriosDeFiltrado.noEstaRelacionado || x.Criterio == enumCriteriosDeFiltrado.diferente)).FirstOrDefault();
            if (noEstanEnElLote != null)
            {
                var idLote = noEstanEnElLote.Valor.Entero();
                var unitariosDelLote = contexto.Set<UnitariosDeUnLoteDtm>().Where(x => x.IdLote == idLote);
                consulta = consulta.Where(x => unitariosDelLote.All(y => y.IdUnitario != x.Id));

                noEstanEnElLote.Aplicado = true;
            }

            return consulta;
        }

        public static IQueryable<UnitarioDtm> FiltrarPorClaseDeUnitario(this IQueryable<UnitarioDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.Equals(ltrDeUnUnitario.FiltrosPorClaseDeUnitario, StringComparison.CurrentCultureIgnoreCase));
            if (filtro != null)
            {
                var clase = ApiDeEnsamblados.ToEnumerado<enumClaseUnitario>(filtro.Valor);
                consulta = consulta.Where(x => x.Clase == clase);
                filtro.Aplicado = true;
            }
            return consulta;
        }
    }
}
