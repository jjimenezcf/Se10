using System;
using System.Linq;
using System.Collections.Generic;
using GestorDeElementos;
using ServicioDeDatos;
using ServicioDeDatos.MaestrosTecnico;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.Terceros;
using Utilidades;

namespace GestoresDeNegocio.MaestrosTecnico
{
    public static class ltrDeUnUnitario
    {
        public const string PreciosDelLote = nameof(PreciosDelLote);
        public const string IdPlanificador = nameof(IdPlanificador);
        public const string FiltrosPorClaseDeUnitario = nameof(FiltrosPorClaseDeUnitario);
        public const string ObtenerTarifaProveedor = nameof(ObtenerTarifaProveedor);
        public const string FiltroPorProveedor = nameof(FiltroPorProveedor);
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

        /// <summary>
        /// Solo los unitarios relacionados con el proveedor, es decir, los que tienen una tarifa de dicho proveedor
        /// </summary>
        public static IQueryable<UnitarioDtm> FiltrarPorProveedor(this IQueryable<UnitarioDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.Equals(ltrDeUnUnitario.FiltroPorProveedor, StringComparison.CurrentCultureIgnoreCase) && !x.Aplicado);
            if (filtro != null)
            {
                IQueryable<TarifaDtm> tarifas = contexto.Set<TarifaDtm>();
                if (filtro.Valor.Entero() > 0)
                {
                    var idProveedor = filtro.Valor.Entero();
                    tarifas = tarifas.Where(t => t.IdProveedor == idProveedor);
                }
                else
                {
                    filtro.Clausula = nameof(ProveedorDtm.Nombre);
                    var proveedores = contexto.Set<ProveedorDtm>().AplicarFiltroDeCadena(filtro);
                    tarifas = tarifas.Where(t => proveedores.Any(p => p.Id == t.IdProveedor));
                }
                consulta = consulta.Where(x => tarifas.Any(t => t.IdElemento == x.Id));
                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<UnitarioDtm> FiltrarPorClaseDeUnitario(this IQueryable<UnitarioDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.Equals(ltrDeUnUnitario.FiltrosPorClaseDeUnitario, StringComparison.CurrentCultureIgnoreCase));
            if (filtro != null)
            {
                var clase = ApiDeEnsamblados.ToEnumerado<enumClaseUnitario>(filtro.Valor);
                consulta = consulta.Where(x => x.Naturaleza.Clase == clase);
                filtro.Aplicado = true;
            }
            return consulta;
        }
    }
}
