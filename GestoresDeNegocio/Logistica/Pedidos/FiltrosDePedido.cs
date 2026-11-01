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
using ServicioDeDatos.MaestrosTecnico;
using static ServicioDeDatos.Elemento.Enumerados;

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

        // Porcentaje (0-100) del importe del pedido que ya se ha entregado: importe entregado (como FiltroPorImporteEntregado, sin restar lo
        // devuelto) entre importe pedido (como FiltroPorImporte); un pedido sin importe no tiene porcentaje y no se incluye. Si en la entrega se
        // cambió el precio, el 100% puede no coincidir exactamente con un pedido entregado por completo
        public static IQueryable<PedidoDtm> FiltroPorPorcentajeEntregado(this IQueryable<PedidoDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnPedido.FiltroPorPorcentajeEntregado.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                var (desde, hasta) = filtro.ParsearRango();
                consulta = consulta.Where(p => (contexto.Set<LineaDeUnPedidoDtm>().Where(l => l.IdElemento == p.Id && l.Precio != null && l.Cantidad != null)
                                                    .Sum(l => (decimal?)((decimal)l.Precio * (decimal)l.Cantidad * (1 - (l.Descuento ?? 0) / 100))) ?? 0) > 0);
                if (desde != null)
                {
                    var minimo = desde.Value;
                    consulta = consulta.Where(p => (contexto.Set<RecepcionDeLineaDtm>().Where(r => r.Linea.IdElemento == p.Id)
                                                        .Sum(r => (decimal?)(r.Cantidad * r.Precio * (1 - (r.Descuento ?? 0) / 100))) ?? 0) * 100
                                                >= minimo * (contexto.Set<LineaDeUnPedidoDtm>().Where(l => l.IdElemento == p.Id && l.Precio != null && l.Cantidad != null)
                                                        .Sum(l => (decimal?)((decimal)l.Precio * (decimal)l.Cantidad * (1 - (l.Descuento ?? 0) / 100))) ?? 0));
                }
                if (hasta != null)
                {
                    var maximo = hasta.Value;
                    consulta = consulta.Where(p => (contexto.Set<RecepcionDeLineaDtm>().Where(r => r.Linea.IdElemento == p.Id)
                                                        .Sum(r => (decimal?)(r.Cantidad * r.Precio * (1 - (r.Descuento ?? 0) / 100))) ?? 0) * 100
                                                <= maximo * (contexto.Set<LineaDeUnPedidoDtm>().Where(l => l.IdElemento == p.Id && l.Precio != null && l.Cantidad != null)
                                                        .Sum(l => (decimal?)((decimal)l.Precio * (decimal)l.Cantidad * (1 - (l.Descuento ?? 0) / 100))) ?? 0));
                }
                filtro.Aplicado = true;
            }
            return consulta;
        }

        // Pedidos con (5) o sin (6) alguna línea con cantidad pendiente de recibir: lo pedido menos lo recibido que no se ha devuelto, como
        // ExtensorDeRecepciones.CantidadPendienteDeEntregar; los comentarios no se entregan
        public static IQueryable<PedidoDtm> FiltroPorPendienteDeRecibir(this IQueryable<PedidoDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnPedido.FiltroPorPendienteDeRecibir.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                if (filtro.Valor.Entero() != 6)
                    consulta = consulta.Where(p => contexto.Set<LineaDeUnPedidoDtm>().Any(l => l.IdElemento == p.Id
                            && l.TipoDeLinea != enumTipoDeLinea.Comentario
                            && (l.Cantidad ?? 0) > (contexto.Set<RecepcionDeLineaDtm>().Where(r => r.IdLinea == l.Id).Sum(r => (decimal?)r.Cantidad) ?? 0)
                                                 - (contexto.Set<DevolucionDeEntregaDtm>().Where(d => d.LineaDeRecepcion.IdLinea == l.Id).Sum(d => (decimal?)d.Cantidad) ?? 0)));
                else
                    consulta = consulta.Where(p => !contexto.Set<LineaDeUnPedidoDtm>().Any(l => l.IdElemento == p.Id
                            && l.TipoDeLinea != enumTipoDeLinea.Comentario
                            && (l.Cantidad ?? 0) > (contexto.Set<RecepcionDeLineaDtm>().Where(r => r.IdLinea == l.Id).Sum(r => (decimal?)r.Cantidad) ?? 0)
                                                 - (contexto.Set<DevolucionDeEntregaDtm>().Where(d => d.LineaDeRecepcion.IdLinea == l.Id).Sum(d => (decimal?)d.Cantidad) ?? 0)));
                filtro.Aplicado = true;
            }
            return consulta;
        }

        // Pedidos con alguna línea de un unitario (material) cuyo nombre o referencia contiene alguno de los textos; es la versión por texto de
        // FiltroPorUnitario, que necesita el id
        public static IQueryable<PedidoDtm> FiltroPorUnitariosDePedidos(this IQueryable<PedidoDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnPedido.UnitariosDePedidos.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                var valores = filtro.Valor.Split(Simbolos.separadorDeCadenasDeFiltrado, StringSplitOptions.RemoveEmptyEntries).Select(v => v.Trim().ToLower()).ToList();
                consulta = consulta.Where(p => contexto.Set<LineaDeUnPedidoDtm>().Any(l => l.IdElemento == p.Id && l.IdUnitario != null
                    && contexto.Set<UnitarioDtm>().Any(u => u.Id == l.IdUnitario
                        && valores.Any(v => u.Nombre.ToLower().Contains(v) || u.Referencia.ToLower().Contains(v)))));
                filtro.Aplicado = true;
            }
            return consulta;
        }

        // Pedidos imputados a un expediente cuyo nombre o referencia contiene alguno de los textos
        public static IQueryable<PedidoDtm> FiltroPorExpedientesDePedidos(this IQueryable<PedidoDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnPedido.ExpedientesDePedidos.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                var valores = filtro.Valor.Split(Simbolos.separadorDeCadenasDeFiltrado, StringSplitOptions.RemoveEmptyEntries).Select(v => v.Trim().ToLower()).ToList();
                consulta = consulta.Where(p => p.IdExpediente != null && contexto.Set<ExpedienteDtm>().Any(e => e.Id == p.IdExpediente
                    && valores.Any(v => e.Nombre.ToLower().Contains(v) || e.Referencia.ToLower().Contains(v))));
                filtro.Aplicado = true;
            }
            return consulta;
        }

        // Pedidos con destino a un almacén cuyo nombre o referencia contiene alguno de los textos
        public static IQueryable<PedidoDtm> FiltroPorAlmacenesDePedidos(this IQueryable<PedidoDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnPedido.AlmacenesDePedidos.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                var valores = filtro.Valor.Split(Simbolos.separadorDeCadenasDeFiltrado, StringSplitOptions.RemoveEmptyEntries).Select(v => v.Trim().ToLower()).ToList();
                consulta = consulta.Where(p => p.IdAlmacen != null && contexto.Set<AlmacenDtm>().Any(a => a.Id == p.IdAlmacen
                    && valores.Any(v => a.Nombre.ToLower().Contains(v) || a.Referencia.ToLower().Contains(v))));
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
