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
using ServicioDeDatos.Contabilidad;

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

        // pedidos con alguna línea del unitario
        public static IQueryable<PedidoDtm> FiltroPorUnitario(this IQueryable<PedidoDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnPedido.FiltroPorUnitario.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                var idUnitario = filtro.Valor.Entero();
                consulta = consulta.Where(p => contexto.Set<LineaDeUnPedidoDtm>().Any(l => l.IdElemento == p.Id && l.IdUnitario == idUnitario));
                filtro.Aplicado = true;
            }
            return consulta;
        }

        // pedidos con alguna línea de la naturaleza; las líneas de un unitario llevan la naturaleza del unitario (ver GestorDeLineasDeUnPedido)
        public static IQueryable<PedidoDtm> FiltroPorNaturaleza(this IQueryable<PedidoDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnPedido.FiltroPorNaturaleza.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                var idNaturaleza = filtro.Valor.Entero();
                consulta = consulta.Where(p => contexto.Set<LineaDeUnPedidoDtm>().Any(l => l.IdElemento == p.Id && l.IdNaturaleza == idNaturaleza));
                filtro.Aplicado = true;
            }
            return consulta;
        }

        // Importe del pedido (sin iva): lo pedido en sus líneas con su descuento, como LineaDeUnPedidoDtm.ImporteDeLinea; un pedido sin líneas suma 0
        public static IQueryable<PedidoDtm> FiltroPorImporte(this IQueryable<PedidoDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnPedido.FiltroPorImporte.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                var (desde, hasta) = filtro.ParsearRango();
                consulta = consulta.Where(p => (desde == null || desde <=
                                                    (contexto.Set<LineaDeUnPedidoDtm>().Where(l => l.IdElemento == p.Id && l.Precio != null && l.Cantidad != null)
                                                        .Sum(l => (decimal?)((decimal)l.Precio * (decimal)l.Cantidad * (1 - (l.Descuento ?? 0) / 100))) ?? 0))
                                            && (hasta == null || hasta >=
                                                    (contexto.Set<LineaDeUnPedidoDtm>().Where(l => l.IdElemento == p.Id && l.Precio != null && l.Cantidad != null)
                                                        .Sum(l => (decimal?)((decimal)l.Precio * (decimal)l.Cantidad * (1 - (l.Descuento ?? 0) / 100))) ?? 0)));
                filtro.Aplicado = true;
            }
            return consulta;
        }

        // Importe entregado (sin iva): todas las entregas de sus líneas a su precio y descuento, como RecepcionDeLineaDtm.Importe, sin restar
        // lo devuelto; un pedido sin entregas suma 0
        public static IQueryable<PedidoDtm> FiltroPorImporteEntregado(this IQueryable<PedidoDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnPedido.FiltroPorImporteEntregado.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                var (desde, hasta) = filtro.ParsearRango();
                consulta = consulta.Where(p => (desde == null || desde <=
                                                    (contexto.Set<RecepcionDeLineaDtm>().Where(r => r.Linea.IdElemento == p.Id)
                                                        .Sum(r => (decimal?)(r.Cantidad * r.Precio * (1 - (r.Descuento ?? 0) / 100))) ?? 0))
                                            && (hasta == null || hasta >=
                                                    (contexto.Set<RecepcionDeLineaDtm>().Where(r => r.Linea.IdElemento == p.Id)
                                                        .Sum(r => (decimal?)(r.Cantidad * r.Precio * (1 - (r.Descuento ?? 0) / 100))) ?? 0)));
                filtro.Aplicado = true;
            }
            return consulta;
        }

        // Importe devuelto (sin iva): todas las devoluciones de sus entregas al precio y descuento de la entrega, como
        // ExtensorDeDevolucionesDePedido.Importe; un pedido sin devoluciones suma 0
        public static IQueryable<PedidoDtm> FiltroPorImporteDevuelto(this IQueryable<PedidoDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnPedido.FiltroPorImporteDevuelto.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                var (desde, hasta) = filtro.ParsearRango();
                consulta = consulta.Where(p => (desde == null || desde <=
                                                    (contexto.Set<DevolucionDeEntregaDtm>().Where(d => d.LineaDeRecepcion.Linea.IdElemento == p.Id)
                                                        .Sum(d => (decimal?)(d.Cantidad * d.LineaDeRecepcion.Precio * (1 - (d.LineaDeRecepcion.Descuento ?? 0) / 100))) ?? 0))
                                            && (hasta == null || hasta >=
                                                    (contexto.Set<DevolucionDeEntregaDtm>().Where(d => d.LineaDeRecepcion.Linea.IdElemento == p.Id)
                                                        .Sum(d => (decimal?)(d.Cantidad * d.LineaDeRecepcion.Precio * (1 - (d.LineaDeRecepcion.Descuento ?? 0) / 100))) ?? 0)));
                filtro.Aplicado = true;
            }
            return consulta;
        }

        // Pedidos con algún movimiento de almacén preasentado en el preasiento: la entrada de compra de una entrega o la salida de una
        // devolución, que también lleva la entrega (ver ExtensorDeDevolucionesDePedido.SalidaPorDevolucion)
        public static IQueryable<PedidoDtm> FiltroPorPreasiento(this IQueryable<PedidoDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnPedido.FiltroPorPreasiento.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                var idPreasiento = filtro.Valor.Entero();
                consulta = consulta.Where(p => contexto.Set<MovimientoDeAlmacenDtm>()
                    .Any(m => m.IdPreasiento == idPreasiento && m.LineaDeRecepcion.Linea.IdElemento == p.Id));
                filtro.Aplicado = true;
            }
            return consulta;
        }

        // Pedidos con algún movimiento de almacén cuyo preasiento está en el lote contable
        public static IQueryable<PedidoDtm> FiltroPorLoteContable(this IQueryable<PedidoDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnPedido.FiltroPorLoteContable.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                var idLote = filtro.Valor.Entero();
                consulta = consulta.Where(p => contexto.Set<MovimientoDeAlmacenDtm>()
                    .Any(m => m.IdPreasiento != null && m.LineaDeRecepcion.Linea.IdElemento == p.Id
                           && contexto.Set<CircuitoDocDeUnPreasientoDtm>().Any(cp => cp.idElemento2 == idLote && cp.idElemento1 == m.IdPreasiento)));
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
            // los devueltos no se excluyen porque se totalizan por separado
            var filtroPorEstados = new ClausulaDeFiltrado(nameof(IUsaEstado.IdEstado), enumCriteriosDeFiltrado.noEsNingunoDe,
                enumEtapasDePedido.PED_Etapa_Cancelado.Estados());
            consulta = consulta.AplicarFiltroPorEntero(filtroPorEstados);
            return consulta;
        }
    }
}
