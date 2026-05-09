using Gestor.Errores;
using GestorDeElementos;
using ModeloDeDto;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.SistemaDocumental;
using System.Collections.Generic;
using System.Linq;
using Utilidades;

namespace GestoresDeNegocio.SistemaDocumental
{
    public static class FiltrosDeCircuitosDoc
    {
        public static IQueryable<CircuitoDocDtm> FiltroPorExpedientePadre(this IQueryable<CircuitoDocDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        =>
        consulta.FiltroDeElementosConVinculosCon<CircuitoDocDtm, CircuitoDocDeUnExpedienteDtm>(contexto, filtros, ltrDeUnCircuito.IdExpedientePadre, ltrDeUnCircuito.AsociadosAUnExpediente);

        public static IQueryable<CircuitoDocDtm> FiltroPorLoteContable(this IQueryable<CircuitoDocDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(f => f.Clausula.ToLower() == ltrDeUnCircuito.SeleccionarParaFiltrarPorLoteContable.ToLower());
            if (filtro != null)
            {
                var filtroValor = filtro.Valor.ToLower().Trim();
                var idtipo = VariablesDeCircuitosDoc.IdDelTipoCircuitoDocParaLoteDePreasientos();
                consulta = consulta.Where(x => x.IdTipo == idtipo);
                filtro.Criterio = enumCriteriosDeFiltrado.porReferencia;
                consulta = Filtrar.AplicarFiltroDeCadena(consulta, filtro);
                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<CircuitoDocDtm> FiltroPorLotePreasiento(this IQueryable<CircuitoDocDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtroPorNombre = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrFiltros.Nombre.ToLower() && !x.Aplicado);
            if (filtroPorNombre != null && !filtros.Exists(x => x.Clausula.ToLower() == ltrDeUnLoteContable.BuscarPorLotePreasiento.ToLower()))
                filtroPorNombre.Clausula = ltrDeUnLoteContable.BuscarPorLotePreasiento;

            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnLoteContable.BuscarPorLotePreasiento.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                if (filtro.Valor.ToLower().StartsWith("l:"))
                {
                    filtro.Clausula = nameof(INombre.Nombre);
                    filtro.Valor = filtro.Valor.Substring(2).Trim();
                }
                else if (filtro.Valor.ToLower().StartsWith("p:"))
                {
                    var preasiento = contexto.Set<PreasientoDtm>().Where(p => p.Referencia.ToLower().Equals(filtro.Valor.Substring(2).Trim().ToLower())).FirstOrDefault();
                    if (preasiento == null)
                        consulta = consulta.Where(lote => false);
                    else
                    {
                        consulta = consulta.Where(lote => contexto.Set<CircuitoDocDeUnPreasientoDtm>().Any(pl => pl.idElemento2 == lote.Id && pl.idElemento1 == preasiento.Id));
                        filtros.MostrarTodos();
                    }
                    filtro.Aplicado = true;
                }
                else filtro.Clausula = nameof(INombre.Nombre);
            }

            return consulta;
        }

        public static IQueryable<CircuitoDocDtm> FiltroPorEstimacion(this IQueryable<CircuitoDocDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(f => f.Clausula.ToLower() == ltrDeUnCircuito.SeleccionarParaFiltrarPorEstimacion.ToLower());
            if (filtro != null)
            {
                var filtroValor = filtro.Valor.ToLower().Trim();
                var idtipo = VariablesDeCircuitosDoc.IdDelTipoCircuitoDocParaEstimacionDirecta();
                consulta = consulta.Where(x => x.IdTipo == idtipo);
                filtro.Criterio = enumCriteriosDeFiltrado.porReferencia;
                consulta = Filtrar.AplicarFiltroDeCadena(consulta, filtro);
                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<CircuitoDocDtm> FiltroPorFichada(this IQueryable<CircuitoDocDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(f => f.Clausula.ToLower() == ltrDeUnCircuito.SeleccionarParaFiltrarPorFichada.ToLower());
            if (filtro != null)
            {
                var filtroValor = filtro.Valor.ToLower().Trim();
                var idtipo = VariablesDeCircuitosDoc.IdDelTipoCircuitoDocParaFichada();
                consulta = consulta.Where(x => x.IdTipo == idtipo);
                filtro.Criterio = enumCriteriosDeFiltrado.porReferencia;
                consulta = Filtrar.AplicarFiltroDeCadena(consulta, filtro);
                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<CircuitoDocDtm> FiltroPorActividadFormativa(this IQueryable<CircuitoDocDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(f => f.Clausula.ToLower() == ltrDeUnCircuito.SeleccionarParaFiltrarPorActividad.ToLower());
            if (filtro != null)
            {
                var filtroValor = filtro.Valor.ToLower().Trim();
                var idtipo = VariablesDeCircuitosDoc.IdDelTipoCircuitoDocParaActividadesFormativas();
                consulta = consulta.Where(x => x.IdTipo == idtipo);
                filtro.Criterio = enumCriteriosDeFiltrado.porReferencia;
                consulta = Filtrar.AplicarFiltroDeCadena(consulta, filtro);
                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<TipoDeCircuitoDocDtm> FiltroParaSeleccionarTipo(this IQueryable<TipoDeCircuitoDocDtm> consulta, Dictionary<string, object> parametros)
        {
            var vista = parametros.LeerValor(ltrParametrosEp.Vista, string.Empty);
            if (vista != string.Empty)
            {
                var idTipoEd = VariablesDeCircuitosDoc.IdDelTipoCircuitoDocParaEstimacionDirecta(errorSiNoEstaDefinido: vista == enumVistasSistemaDocumental.CrudEstimacionesDirectas);
                var idTipoLp = VariablesDeCircuitosDoc.IdDelTipoCircuitoDocParaLoteDePreasientos(errorSiNoEstaDefinido: vista == enumVistasSistemaDocumental.CrudLotesContables);
                var idTipoFi = VariablesDeCircuitosDoc.IdDelTipoCircuitoDocParaFichada(errorSiNoEstaDefinido: vista == enumVistasSistemaDocumental.CrudFichadas);
                var idtipoAf = VariablesDeCircuitosDoc.IdDelTipoCircuitoDocParaActividadesFormativas(errorSiNoEstaDefinido: vista == enumVistasSistemaDocumental.CrudActividadesFormativas);

                if (vista == enumVistasSistemaDocumental.CrudCircuitosDoc)
                {
                    consulta = consulta.Where(tipo => tipo.Id != idTipoEd && tipo.Id != idTipoLp && tipo.Id != idTipoFi);
                }
                else if (vista == enumVistasSistemaDocumental.CrudEstimacionesDirectas)
                {
                    consulta = consulta.Where(tipo => tipo.Id == idTipoEd);
                }
                else if (vista == enumVistasSistemaDocumental.CrudLotesContables)
                {
                    consulta = consulta.Where(tipo => tipo.Id == idTipoLp);
                }
                else if (vista == enumVistasSistemaDocumental.CrudFichadas)
                {
                    consulta = consulta.Where(tipo => tipo.Id == idTipoFi);
                }
                else if (vista == enumVistasSistemaDocumental.CrudActividadesFormativas)
                {
                    consulta = consulta.Where(tipo => tipo.Id == idtipoAf);
                }
                else
                {
                    GestorDeErrores.Emitir($"Falta por definir como aplicar el fitro por tipo de circuito documental para la vista '{vista}'");
                }
            }
            return consulta;
        }


    }
}
