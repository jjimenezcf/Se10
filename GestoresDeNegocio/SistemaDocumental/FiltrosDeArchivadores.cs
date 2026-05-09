using GestorDeElementos;
using ServicioDeDatos;
using System.Linq;
using System.Collections.Generic;
using ModeloDeDto;
using Utilidades;
using GestorDeElementos.Extensores;
using ServicioDeDatos.SistemaDocumental;

namespace GestoresDeNegocio.SistemaDocumental
{
    internal static class FiltrosDeArchivadores
    {
        public static IQueryable<ArchivadorDtm> FiltroDeVinculadosA(this IQueryable<ArchivadorDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrFiltros.VinculadosA.ToLower());
            if (filtro != null)
            {
                var negocio = NegociosDeSe.ToEnumerado(filtro.Valor.Split(Simbolos.separadorDeValores)[0]);
                var idElemento = filtro.Valor.Split(Simbolos.separadorDeValores)[1].Entero();

                consulta = consulta.Where(elemento => negocio.Archivadores(contexto).Where(x => x.idElemento1 == idElemento).Any(vinculo => vinculo.idElemento2 == elemento.Id));
                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<ArchivadorDtm> FiltroDeVincularCon(this IQueryable<ArchivadorDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrFiltros.VincularCon.ToLower());
            if (filtro != null)
            {
                var vinculos = NegociosDeSe.ToEnumerado(filtro.IdNegocio).Archivadores(contexto).Where(x => x.idElemento1 == filtro.IdElemento);
                consulta = consulta.ElementosNoVinculadosDeLaMismaSociedad(contexto, filtro, vinculos);
                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<TipoDeArchivadorDtm> FiltroDeTiposCopiables(this IQueryable<TipoDeArchivadorDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrTipoArchivador.SeleccionarParaCopiar.ToLower());
            if (filtro != null)
            {
                consulta = consulta.FiltroParaExcluirLosDelSistema(contexto, excluirDelSistema: true);
                filtro.Aplicado = true;
                filtros.Add(new ClausulaDeFiltrado { Clausula = nameof(TipoDeArchivadorDtm.Nombre), Criterio = enumCriteriosDeFiltrado.contiene, Valor = filtro.Valor });
            }
            return consulta;
        }

        public static IQueryable<TipoDeArchivadorDtm> FiltroParaExcluirLosDelSistema(this IQueryable<TipoDeArchivadorDtm> consulta, ContextoSe contexto,  bool excluirDelSistema)
        {
            if (excluirDelSistema == true)
            {
                consulta = consulta.Where(tipo => tipo.DelSistema != true);
            }
            return consulta;
        }


    }
}
