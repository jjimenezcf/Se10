using GestorDeElementos;
using GestorDeElementos.Extensores;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Gastos;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Ventas;
using System.Collections.Generic;
using System.Linq;
using Utilidades;

namespace GestoresDeNegocio.Gastos
{
    public static class FiltrosDePagos
    {

        public static IQueryable<PagoDtm> FiltroPorAcreedor(this IQueryable<PagoDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnPago.ClaseDeAcreedor.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                consulta = filtro.Valor == ltrDeUnPago.SoloProveedores
                ? consulta.Where(x => x.IdProveedor != null)
                : filtro.Valor == ltrDeUnPago.SoloTrabajadores
                ? consulta.Where(x => x.IdTrabajador != null) :
                consulta;

                filtro.Aplicado = true;
            }

            filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnPago.IdAcreedor.ToLower() && !x.Aplicado);
            if (filtro != null) consulta = consulta.AplicarFiltroPorEntero(filtro);
            return consulta;
        }
        public static IQueryable<PagoDtm> FiltroPorFacturaRec(this IQueryable<PagoDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnPago.FiltroPorFactura.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                consulta = filtro.Valor == ltrDeUnPago.SoloConFacturas
                ? consulta.Where(x => x.IdFacturaRec != null)
                : filtro.Valor == ltrDeUnPago.SoloSinFacturas
                ? consulta.Where(x => x.IdFacturaRec == null) :
                consulta;

                filtro.Aplicado = true;
            }

            filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnPago.IdFacturaRec.ToLower() && !x.Aplicado);
            if (filtro != null) consulta = consulta.AplicarFiltroPorEntero(filtro);
            return consulta;
        }

        public static IQueryable<PagoDtm> FiltroPorCuentaDePago(this IQueryable<PagoDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnPago.IdCuentaDePago.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                consulta = consulta.Where(x => x.IdCuentaDePago == filtro.Valor.Entero());
                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<PagoDtm> FiltroPorPagarEl(this IQueryable<PagoDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnPago.FiltroPorPagarEl.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                consulta = consulta.AplicarFiltroEntreFechas(filtro, nameof(PagoDtm.PagarEl));
            }

            return consulta;
        }

        public static IQueryable<PagoDtm> FiltroPorPagadoEl(this IQueryable<PagoDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnPago.FiltroPorPagadoEl.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                consulta = consulta.AplicarFiltroEntreFechas(filtro, nameof(PagoDtm.PagadoEl));
            }

            return consulta;
        }

        public static IQueryable<PagoDtm> FiltroPorImporte(this IQueryable<PagoDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnPago.FiltroPorImporte.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                consulta = Filtrar.AplicarFiltroEntreNumeros(consulta, filtro, nameof(PagoDtm.Importe), usarAbs: true);
                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<PagoDtm> FiltroPorFormaDePago(this IQueryable<PagoDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnPago.FiltroPorFormaDePago.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                if (filtro.Valor == ltrDeUnPago.FiltroDePagosContado)
                    consulta = consulta.Where(p => p.Clase.Equals(enumClaseDePago.Contado) &&  p.IdTarjetaDePago == null && p.IdCuentaDePago == null);
                else if (filtro.Valor == ltrDeUnPago.FiltroDePagosTarjeta)
                    consulta = consulta.Where(p => p.Clase.Equals(enumClaseDePago.Contado) && p.IdTarjetaDePago != null);
                else if (filtro.Valor == ltrDeUnPago.FiltroDePagosDomiciliado)
                    consulta = consulta.Where(p => p.Clase.Equals(enumClaseDePago.Contado) && p.IdCuentaDePago != null && p.IdTarjetaDePago == null);
                else if (filtro.Valor == ltrDeUnPago.FiltroDePagosTransferencia)
                    consulta = consulta.Where(p => p.Clase.Equals(enumClaseDePago.Transferencia));
                else if (filtro.Valor == ltrDeUnPago.FiltroDePagosRemesa)
                    consulta = consulta.Where(p => p.Clase.Equals(enumClaseDePago.Remesa));

                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<PagoDtm> FiltroSiHayPreasiento(this IQueryable<PagoDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => (x.Clausula.ToLower() == ltrDeUnPago.FiltroSiHayPreasiento.ToLower()) && !x.Aplicado);
            if (filtro != null)
            {
                var cancelados = enumNegocio.Preasiento.Estados(contexto).Where(c => c.Cancelado).Select(c => c.Id).ToList();
                if (filtro.Valor == ltrDeUnPago.FiltroSinSpr)
                    consulta = consulta.Where(x => x.IdPreasiento == null || contexto.Set<PreasientoDtm>().Any(p => p.Id == x.IdPreasiento && cancelados.Contains(p.IdEstado)));
                else if (filtro.Valor == ltrDeUnPago.FiltroConSpr)
                    consulta = consulta.Where(x => x.IdPreasiento != null && contexto.Set<PreasientoDtm>().Any(p => p.Id == x.IdPreasiento && !cancelados.Contains(p.IdEstado)));
                else if (filtro.Valor == ltrDeUnPago.FiltroConSprCan)
                    consulta = consulta.Where(x => x.IdPreasiento != null && contexto.Set<PreasientoDtm>().Any(p => p.Id == x.IdPreasiento && cancelados.Contains(p.IdEstado)));
                else if (filtro.Valor == ltrDeUnPago.FiltroConFacSin)
                {
                    var facturasSinPreasiento = contexto.Set<FacturaRecDtm>().Where(factura => factura.IdPreasiento == null || contexto.Set<PreasientoDtm>().Any(p => p.Id == factura.IdPreasiento && cancelados.Contains(p.IdEstado)));
                    consulta = consulta.Where(x => x.IdPreasiento != null
                                               && contexto.Set<PreasientoDtm>().Any(p => p.Id == x.IdPreasiento && !cancelados.Contains(p.IdEstado))
                                               && contexto.Set<FacturaRecDtm>().Any(f => f.Id == x.IdFacturaRec && facturasSinPreasiento.Any(x => x.Id == f.Id)));
                }
                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<PagoDtm> FiltroPorRemesas(this IQueryable<PagoDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnPago.IdRemesaPag.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                var pagos = contexto.Set<PagoDeUnaRemesaDtm>().Where(x => x.IdElemento == filtro.Valor.Entero());
                consulta = consulta.Where(x => pagos.Any(fr => fr.IdPago == x.Id));
                return consulta;
            }

            filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnPago.IncluidaEnRemesa.ToLower());
            if (filtro != null)
            {
                if (filtro.Valor.Entero() == ltrParametrosNeg.ConRelacion)
                    consulta = consulta.Where(x => contexto.Set<PagoDeUnaRemesaDtm>().Any(fr => fr.IdPago == x.Id));

                if (filtro.Valor.Entero() == ltrParametrosNeg.SinRelacion)
                    consulta = consulta.Where(x => !contexto.Set<PagoDeUnaRemesaDtm>().Any(fr => fr.IdPago == x.Id));
            }
            return consulta;
        }

        public static IQueryable<PagoDtm> FiltroFacturasEnUnLoteContable(this IQueryable<PagoDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnPago.IdLoteContable.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                var preasientosDeUnLote = contexto.Set<PreasientoDtm>().Where(p => p.NegocioReferenciado.Equals(enumNegocio.Pago) &&
                                                   contexto.Set<CircuitoDocDeUnPreasientoDtm>().Any(lote => lote.idElemento2 == filtro.Valor.Entero() && lote.idElemento1 == p.Id));
                consulta = consulta.Where(f => preasientosDeUnLote.Any(p => p.IdReferenciado == f.Id));
                filtros.MostrarTodos();
                filtro.Aplicado = true;
                return consulta;
            }

            filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnLoteContable.VinculosAUnLote.ToLower());
            if (filtro != null)
            {

                var preasientosEnLotes = contexto.Set<PreasientoDtm>().Where(p => p.NegocioReferenciado.Equals(enumNegocio.Pago) &&
                                                   contexto.Set<CircuitoDocDeUnPreasientoDtm>().Any(lote => lote.idElemento1 == p.Id));
                if (filtro.Valor.ToLower() == ltrParametrosNeg.ConRelacion.ToString().ToLower())
                {
                    consulta = consulta.Where(pago => preasientosEnLotes.Any(prea => prea.IdReferenciado == pago.Id));
                }
                if (filtro.Valor.ToLower() == ltrParametrosNeg.SinRelacion.ToString().ToLower())
                {
                    consulta = consulta.Where(pago => !preasientosEnLotes.Any(prea => prea.IdReferenciado == pago.Id));
                }
                filtro.Aplicado = true;
            }

            return consulta;
        }


        public static IQueryable<PagoDtm> FiltroPorPagosNoIncluidoEnRemesa(this IQueryable<PagoDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnPago.ExcluirPagosDeUnaRemesa.ToLower() && x.Criterio == enumCriteriosDeFiltrado.diferente && !x.Aplicado);
            if (filtro != null)
            {
                var remesa = contexto.SeleccionarPorId<RemesaPagDtm>(filtro.Valor.Entero());
                var listaEstados = enumEtapasDePagos.PAG_Etapa_Pendiente.Lista();
                consulta = listaEstados.Count == 0 ? consulta.Where(x => false) : consulta.Where(x => listaEstados.Contains(x.IdEstado));

                //var pagosDeLaRemesa = contexto.Set<PagoDeUnaRemesaDtm>().Where(pr => pr.IdElemento == filtro.Valor.Entero()); pagosDeLaRemesa.Any(pr => pr.IdPago == pago.Id)
                consulta = consulta.Where(pago => !contexto.Set<PagoDeUnaRemesaDtm>().Any(pr => pr.IdPago == pago.Id) && 
                                                   pago.Clase == enumClaseDePago.Remesa 
                                                   );
                filtro.Aplicado = true;
                return consulta;
            }
            return consulta;
        }

        public static IQueryable<PagoDtm> FiltroParaSeleccionarPagoEnRemesas(this IQueryable<PagoDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaRemesaPag.IdPagoEnRemesa.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                consulta = consulta.Where(pago => contexto.Set<PagoDeUnaRemesaDtm>().Any(b => b.IdPago == pago.Id));
                filtro.Aplicado = true;
                filtros.Add(new ClausulaDeFiltrado(nameof(INombre.Expresion), enumCriteriosDeFiltrado.contiene, filtro.Valor));
                return consulta;
            }
            return consulta;
        }

    }
}
