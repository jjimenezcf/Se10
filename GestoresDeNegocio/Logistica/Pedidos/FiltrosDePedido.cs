using System.Linq;
using ServicioDeDatos.Logistica;
using ServicioDeDatos;
using System.Collections.Generic;
using Utilidades;
using GestorDeElementos;
using ServicioDeDatos.Juridico;
using System.Linq.Dynamic.Core;
using ServicioDeDatos.Terceros;
using ModeloDeDto;
using ServicioDeDatos.Elemento;
using System;
using ServicioDeDatos.Expediente;

namespace GestoresDeNegocio.Logistica
{
    public static class FiltrosDePedido
    {

        public static IQueryable<PedidoDtm> FiltroPorProveedor(this IQueryable<PedidoDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnPedido.FiltroPorProveedor.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                consulta = consulta.Where(x => x.IdProveedor == filtro.Valor.Entero());
                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<PedidoDtm> FiltroPorEjercicio(this IQueryable<PedidoDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnPedido.FiltroPorEjercicio.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                consulta = consulta.Where(p => p.PedidoEl != null && ((DateTime)p.PedidoEl).Year == filtro.Valor.Entero());
                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<PedidoDtm> FiltroPorFechaDeEntrega(this IQueryable<PedidoDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnPedido.FiltroPorFechaDeEntrega.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                consulta = consulta.AplicarFiltroEntreFechas(filtro, nameof(PedidoDtm.EntregarEl));
            }
            return consulta;
        }

        public static IQueryable<PedidoDtm> FiltroPorFechaPedido(this IQueryable<PedidoDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnPedido.FiltroPorFechaDePedido.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                consulta = consulta.AplicarFiltroEntreFechas(filtro, nameof(PedidoDtm.PedidoEl));
                //consulta = consulta.Where(rem => !(rem.Estado.Inicial || rem.Estado.Cancelado));
            }
            return consulta;
        }


        public static IQueryable<PedidoDtm> FiltroPorAsuntoReferenciaPedido(this IQueryable<PedidoDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtroPorNombre = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrFiltros.Nombre.ToLower() && !x.Aplicado);
            if (filtroPorNombre != null && !filtros.Exists(x => x.Clausula.ToLower() == ltrDeUnPedido.FiltroPorPedidoReferencia.ToLower()))
                filtroPorNombre.Clausula = ltrDeUnPedido.FiltroPorPedidoReferencia;

            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnPedido.FiltroPorPedidoReferencia.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                if (filtro.Valor.ToLower().StartsWith("n:"))
                    consulta = consulta.Where(p => p.Nombre.ToLower().Contains(filtro.Valor.Substring(2).Trim().ToLower()));
                else if (filtro.Valor.ToLower().StartsWith("r:"))
                    consulta = consulta.Where(p => p.Referencia.ToLower().Contains(filtro.Valor.Substring(2).Trim().ToLower()));
                else if (filtro.Valor.ToLower().StartsWith("p:"))
                    consulta = consulta.Where(p => contexto.Set<ProveedorDtm>().Any(prv => prv.Nombre.ToLower().Contains(filtro.Valor.Substring(2).Trim().ToLower()) && p.IdProveedor == prv.Id));
                else
                    consulta = consulta.Where(p => p.Nombre.ToLower().Contains(filtro.Valor.ToLower())
                     || p.Referencia.ToLower().Contains(filtro.Valor.ToLower()));
                filtro.Aplicado = true;
            }

            return consulta;
        }


        public static IQueryable<PedidoDtm> FiltroPedidosPosiblesDelContrato(this IQueryable<PedidoDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnPedido.PedidosImputablesAlContrato.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                var datos = enumNegocio.Contrato.Ampliacion<DatosDelContratoDtm>(contexto, filtro.Valor.Entero(), aplicarJoin: true);
                consulta = consulta.Where(pedido => pedido.IdProveedor == datos.IdProveedor.Entero() 
                                    && pedido.IdContrato == null
                                    && contexto.Set<ContratoDtm>().Any(contrato => contrato.Cg.IdSociedad == pedido.Cg.IdSociedad)
                                    && contexto.Set<EstadoDeUnPedidoDtm>().Any(estado => !estado.Cancelado && estado.Id == pedido.IdEstado));
                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<PedidoDtm> FiltroPedidosPosiblesDeUnExpediente(this IQueryable<PedidoDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnPedido.PedidosImputablesAlExpediente.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                consulta = consulta.Where(pedido => pedido.IdExpediente == null 
                               && contexto.Set<ExpedienteDtm>().Any(expediente => expediente.Cg.IdSociedad == pedido.Cg.IdSociedad) 
                               && contexto.Set<EstadoDeUnPedidoDtm>().Any(estado => !estado.Cancelado && estado.Id == pedido.IdEstado));
                parametros.AplicarFiltroQueMostrar = false;
                filtro.Aplicado = true;
            }
            return consulta;
        }


        public static IQueryable<PedidoDtm> FiltroPorEtapa(this IQueryable<PedidoDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnPedido.FiltroPorEtapa.ToLower());
            if (filtro != null)
            {
                var filtroPorEstados = new ClausulaDeFiltrado(nameof(IUsaEstado.IdEstado),
                    filtro.Criterio == enumCriteriosDeFiltrado.igual ? enumCriteriosDeFiltrado.esAlgunoDe : enumCriteriosDeFiltrado.noEsNingunoDe,
                    ApiDeEnsamblados.ToEnumerado<enumEtapasDePedido>(filtro.Valor).Estados());
                consulta = consulta.AplicarFiltroPorEntero(filtroPorEstados);
                filtro.Aplicado = true;
                return consulta;
            }
            return consulta;
        }

        public static IQueryable<PedidoDtm> ExcluirLosNoTotalizables(this IQueryable<PedidoDtm> consulta)
        {
            var filtroPorEstados = new ClausulaDeFiltrado(nameof(IUsaEstado.IdEstado), enumCriteriosDeFiltrado.noEsNingunoDe,  
                enumEtapasDePedido.PED_Etapa_Cancelado.Estados() + "," + enumEtapasDePedido.PED_Etapa_Devuelto.Estados());
            consulta = consulta.AplicarFiltroPorEntero(filtroPorEstados);
            return consulta;
        }
    }
}
