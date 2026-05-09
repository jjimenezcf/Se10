using GestorDeElementos;
using GestorDeElementos.Extensores;
using ModeloDeDto;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Gastos;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Tarea;
using ServicioDeDatos.Terceros;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Utilidades;

namespace GestoresDeNegocio.Gastos
{
    public static class FiltrosDeFacturasRec
    {

        public static IQueryable<FacturaRecDtm> FiltroPorProveedor(this IQueryable<FacturaRecDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaFacturaRec.FiltroPorProveedor.ToLower() || x.Clausula.ToLower() == nameof(ltrDeUnaFacturaRec.FiltroPorProveedor).ToLower());
            if (filtro != null)
            {
                if (filtro.Valor.Entero() > 0)
                {
                    consulta = consulta.Where(x => x.IdProveedor == filtro.Valor.Entero());
                }
                else
                {
                    filtro.Clausula = nameof(ProveedorDtm.Nombre);
                    var filtrosDeProveedor = contexto.Set<ProveedorDtm>().AplicarFiltroDeCadena(filtro);
                    consulta = consulta.Where(f => filtrosDeProveedor.Any(prv => f.IdProveedor == prv.Id));
                }
            }
            else
            {
                filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == nameof(FacturaRecDtm.Proveedor).ToLower() && x.Valor.Entero() > 0 && !x.Aplicado);
                if (filtro != null)
                {
                    consulta = consulta.Where(x => x.IdProveedor == filtro.Valor.Entero());
                    filtro.Aplicado = true;
                }
            }
            return consulta;
        }

        public static IQueryable<FacturaRecDtm> FiltroPorBaseImponible(this IQueryable<FacturaRecDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaFacturaRec.FiltroPorImporteSinIva.ToLower() && !x.Valor.IsNullOrEmpty());
            if (filtro != null)
            {
                consulta = Filtrar.AplicarFiltroEntreNumeros(consulta, filtro, nameof(FacturaRecDtm.BaseImponible), usarAbs: true);
                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<FacturaRecDtm> FiltroPorTotalFactura(this IQueryable<FacturaRecDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaFacturaRec.FiltroPorTotalFactura.ToLower() && !x.Valor.IsNullOrEmpty());
            if (filtro != null)
            {
                consulta = Filtrar.AplicarFiltroEntreNumeros(consulta, filtro, nameof(FacturaRecDtm.TotalDelPago), usarAbs: true);
                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<FacturaRecDtm> FiltroSiHayPreasiento(this IQueryable<FacturaRecDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => (x.Clausula.ToLower() == ltrDeUnaFacturaRec.FiltroSiHayPreasiento.ToLower()) && !x.Aplicado);
            if (filtro != null)
            {
                var cancelados = enumNegocio.Preasiento.Estados(contexto).Where(c => c.Cancelado).Select(c => c.Id).ToList();
                if (filtro.Valor == ltrDeUnaFacturaRec.FiltroSinSpr)
                    consulta = consulta.Where(x => x.IdPreasiento == null || contexto.Set<PreasientoDtm>().Any(p => p.Id == x.IdPreasiento && cancelados.Contains(p.IdEstado)));
                else if (filtro.Valor == ltrDeUnaFacturaRec.FiltroConSpr)
                    consulta = consulta.Where(x => x.IdPreasiento != null && contexto.Set<PreasientoDtm>().Any(p => p.Id == x.IdPreasiento && !cancelados.Contains(p.IdEstado)));
                else if (filtro.Valor == ltrDeUnaFacturaRec.FiltroConSprCan)
                    consulta = consulta.Where(x => x.IdPreasiento != null && contexto.Set<PreasientoDtm>().Any(p => p.Id == x.IdPreasiento && cancelados.Contains(p.IdEstado)));
                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<FacturaRecDtm> FiltroPorEjercicioDeFactura(this IQueryable<FacturaRecDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaFacturaRec.FiltroPorEjercicioDeFactura.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                consulta = consulta.Where(f => f.FacturadaEl.Year == filtro.Valor.Entero());
                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<FacturaRecDtm> FiltroPorFechaDeVencimiento(this IQueryable<FacturaRecDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaFacturaRec.FiltroPorFechaDeVencimiento.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                consulta = consulta.AplicarFiltroEntreFechas(filtro, nameof(FacturaRecDtm.VenceEl));
                //consulta = consulta.Where(rem => !(rem.Estado.Inicial || rem.Estado.Cancelado));
            }
            return consulta;
        }

        public static IQueryable<FacturaRecDtm> FiltroPorFechaDeEmision(this IQueryable<FacturaRecDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaFacturaRec.FiltroPorFechaDeEmision.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                consulta = consulta.AplicarFiltroEntreFechas(filtro, nameof(FacturaRecDtm.FacturadaEl));
                //consulta = consulta.Where(rem => !(rem.Estado.Inicial || rem.Estado.Cancelado));
            }
            return consulta;
        }

        public static IQueryable<FacturaRecDtm> FiltroPorNumeroDeFactura(this IQueryable<FacturaRecDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaFacturaRec.FiltroPorNumerosDeFactura.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                consulta = consulta.Where(f => f.Numero.ToLower().Replace(" ", "") == filtro.Valor.ToLower().Replace(" ", ""));
                filtro.Aplicado = true;
            }

            return consulta;
        }

        public static IQueryable<FacturaRecDtm> FiltroPorAsuntoReferenciaNumeroDeFactura(this IQueryable<FacturaRecDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtroPorNombre = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrFiltros.Nombre.ToLower() && !x.Aplicado);
            if (filtroPorNombre != null && !filtros.Exists(x => x.Clausula.ToLower() == ltrDeUnaFacturaRec.AsuntoNumeroReferencia.ToLower()))
                filtroPorNombre.Clausula = ltrDeUnaFacturaRec.AsuntoNumeroReferencia;

            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaFacturaRec.AsuntoNumeroReferencia.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                if (filtro.Valor.ToLower().StartsWith("a:")) // consulta.Where(f => f.Nombre.ToLower().Contains(filtro.Valor.Substring(2).Trim().ToLower()));
                {
                    filtro.Clausula = nameof(FacturaRecDtm.Nombre);
                    filtro.Valor = filtro.Valor.Substring(2).Trim().ToLower();
                    consulta = consulta.AplicarFiltroDeCadena(filtro);
                }
                else if (filtro.Valor.ToLower().StartsWith("f:")) //consulta = consulta.Where(f => f.Numero.ToLower().Contains(filtro.Valor.Substring(2).Trim().ToLower()));
                {
                    filtro.Valor = filtro.Valor.Substring(2).Trim().ToLower();
                    filtro.Clausula = nameof(FacturaRecDtm.Numero);
                    consulta = consulta.AplicarFiltroDeCadena(filtro);
                }
                else if (filtro.Valor.ToLower().StartsWith("r:")) //consulta = consulta.Where(f => f.Referencia.ToLower().Contains(filtro.Valor.Substring(2).Trim().ToLower()));
                {
                    filtro.Valor = filtro.Valor.Substring(2).Trim().ToLower();
                    filtro.Clausula = nameof(FacturaRecDtm.Referencia);
                    consulta = consulta.AplicarFiltroDeCadena(filtro);
                }
                else if (filtro.Valor.ToLower().StartsWith("p:")) //consulta = consulta.Where(f => contexto.Set<ProveedorDtm>().Any(prv => prv.Nombre.ToLower().Contains(filtro.Valor.Substring(2).Trim().ToLower()) && f.IdProveedor == prv.Id));
                {
                    filtro.Valor = filtro.Valor.Substring(2).Trim().ToLower();
                    filtro.Clausula = nameof(ProveedorDtm.Nombre);
                    var filtrosDeProveedor = contexto.Set<ProveedorDtm>().AplicarFiltroDeCadena(filtro);
                    consulta = consulta.Where(f => filtrosDeProveedor.Any(prv => f.IdProveedor == prv.Id));
                }
                else
                {
                    filtro.Valor = filtro.Valor.Trim().ToLower();
                    filtro.Clausula = $"{nameof(FacturaRecDtm.Numero)}{Simbolos.separadorDePropiedades}{nameof(FacturaRecDtm.Nombre)}{Simbolos.separadorDePropiedades}{nameof(FacturaRecDtm.Referencia)}";
                    consulta = consulta.AplicarFiltroDeCadena(filtro);
                }
                //consulta = consulta.Where(f => f.Numero.ToLower().Replace(" ", "") == filtro.Valor.ToLower().Replace(" ", "")
                //     || f.Nombre.ToLower().Contains(filtro.Valor.ToLower())
                //     || f.Referencia.ToLower().Contains(filtro.Valor.ToLower()));
                filtro.Aplicado = true;
            }

            return consulta;
        }

        public static IQueryable<FacturaRecDtm> FiltroParaExcluirLasRectificadas(this IQueryable<FacturaRecDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaFacturaRec.SelectorDeFacturasNoRecificadas.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                var editado = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrFiltros.IdEditado.ToLower() && !x.Aplicado);
                if (editado is not null)
                {
                    editado.Aplicado = true;
                    consulta = consulta.Where(factura => factura.Id != editado.Valor.Entero());
                }
                filtro.Clausula = nameof(INombre);
                consulta = consulta.Where(factura => !contexto.Set<FacturaRecDtm>().Any(rectificada => rectificada.IdRectificada == factura.Id));
            }

            return consulta;
        }

        public static IQueryable<FacturaRecDtm> FiltroSelectorDeFacturaRemesadas(this IQueryable<FacturaRecDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaFacturaRec.SelectorDeFacturaRemesada.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                var idsDeFacturasDeSusPagosRemesados = contexto.Set<PagoDtm>()
                    .Where(x => x.IdFacturaRec != null
                    && contexto.Set<PagoDeUnaRemesaDtm>()
                    .Any(y => y.IdPago == x.Id)).Select(x => new { x.IdFacturaRec });

                consulta = consulta.Where(f => idsDeFacturasDeSusPagosRemesados.Any(x => x.IdFacturaRec == f.Id));
                consulta = consulta.AplicarFiltroDeCadena(filtro);
            }

            return consulta;
        }

        public static IQueryable<FacturaRecDtm> FiltroFacturasPosiblesDelContrato(this IQueryable<FacturaRecDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaFacturaRec.FacturasPosiblesDelContrato.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                var datos = enumNegocio.Contrato.Ampliacion<DatosDelContratoDtm>(contexto, filtro.Valor.Entero(), aplicarJoin: true);
                consulta = consulta.Where(factura => factura.IdProveedor == datos.IdProveedor.Entero()
                                    && factura.IdContrato == null
                                    && contexto.Set<ContratoDtm>().Any(contrato => contrato.Cg.IdSociedad == factura.Cg.IdSociedad)
                                    && contexto.Set<EstadoDeUnaFacturaRecDtm>().Any(estado => !estado.Cancelado && estado.Id == factura.IdEstado));
                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<FacturaRecDtm> FiltroFacturasPosiblesDeUnExpediente(this IQueryable<FacturaRecDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaFacturaRec.FacturasImputablesEnUnExpediente.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                consulta = consulta.Where(factura => factura.IdExpediente == null
                              && contexto.Set<ExpedienteDtm>().Any(expediente => expediente.Cg.IdSociedad == factura.Cg.IdSociedad)
                              && contexto.Set<EstadoDeUnaFacturaRecDtm>().Any(estado => !estado.Cancelado && estado.Id == factura.IdEstado));
                parametros.AplicarFiltroQueMostrar = false;
                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<FacturaRecDtm> FiltroFacturasEnUnaEstimacion(this IQueryable<FacturaRecDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            return consulta.FiltroDeElementosConVinculosA<FacturaRecDtm, CircuitoDocDeUnaFacturaRecDtm>(contexto, filtros, ltrDeUnaFacturaRec.IdEstimacionDirecta, ltrDeUnaEstimacion.VinculosAUnaEstimacion);
        }

        public static IQueryable<FacturaRecDtm> FiltroFacturasAsociablesAUnElemento(this IQueryable<FacturaRecDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaFacturaRec.AsociadaAUnElemento.ToLower() && !x.Aplicado);
            if (filtro is not null)
            {
                var filtroDeVincular = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrFiltros.VincularCon.ToLower());
                var negocio = NegociosDeSe.ToEnumerado(filtroDeVincular.IdNegocio);
                if (negocio == enumNegocio.Contrato)
                {
                    var estadosValidos = (enumEtapasDeContratos.CTR_Etapa_Vigente.Estados() + Simbolos.Coma + enumEtapasDeContratos.CTR_Etapa_Finalizacion.Estados()).ToLista<int>(separador: Simbolos.Coma);

                    consulta = consulta.Where(factura => factura.IdContrato == null &&
                               contexto.Set<ContratoDtm>().Any(contrato => contrato.Cg.IdSociedad == factura.Cg.IdSociedad &&
                                                                           contrato.Datos.IdProveedor == factura.Proveedor.Id &&
                                                                           estadosValidos.Contains(contrato.IdEstado) &&
                                                                           contrato.Id == filtroDeVincular.IdElemento));

                }
                else if (negocio == enumNegocio.Expediente)
                {
                    consulta = consulta.Where(factura => factura.IdExpediente == null &&
                    contexto.Set<ExpedienteDtm>().Any(expediente => expediente.Cg.IdSociedad == factura.Cg.IdSociedad && expediente.Id == filtroDeVincular.IdElemento));
                }
                filtroDeVincular.Aplicado = true;
                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<FacturaRecDtm> FiltroFacturasEnUnLoteContable(this IQueryable<FacturaRecDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaFacturaRec.IdLoteContable.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                var preasientosDeUnLote = contexto.Set<PreasientoDtm>().Where(p => p.NegocioReferenciado.Equals(enumNegocio.FacturaRecibida) &&
                                                   contexto.Set<CircuitoDocDeUnPreasientoDtm>().Any(lote => lote.idElemento2 == filtro.Valor.Entero() && lote.idElemento1 == p.Id));
                consulta = consulta.Where(f => preasientosDeUnLote.Any(p => p.IdReferenciado == f.Id));
                filtros.MostrarTodos();
                filtro.Aplicado = true;
                return consulta;
            }

            filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnLoteContable.VinculosAUnLote.ToLower());
            if (filtro != null)
            {

                var preasientosEnLotes = contexto.Set<PreasientoDtm>().Where(p => p.NegocioReferenciado.Equals(enumNegocio.FacturaRecibida) &&
                                                   contexto.Set<CircuitoDocDeUnPreasientoDtm>().Any(lote => lote.idElemento1 == p.Id));
                if (filtro.Valor.ToLower() == ltrParametrosNeg.ConRelacion.ToString().ToLower())
                {
                    consulta = consulta.Where(factura => preasientosEnLotes.Any(prea => prea.IdReferenciado == factura.Id));
                }
                if (filtro.Valor.ToLower() == ltrParametrosNeg.SinRelacion.ToString().ToLower())
                {
                    consulta = consulta.Where(factura => !preasientosEnLotes.Any(prea => prea.IdReferenciado == factura.Id));
                }
                filtro.Aplicado = true;
            }

            return consulta;
        }

        public static IQueryable<FacturaRecDtm> FiltroPorRemesa(this IQueryable<FacturaRecDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaFacturaRec.IdRemesaPag.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                var idsDeFacturasEnLaRemesaPasada =
                     contexto.Set<PagoDtm>()
                             .Where(x => x.IdFacturaRec != null
                  && contexto.Set<PagoDeUnaRemesaDtm>()
                             .Any(y => y.IdPago == x.Id && y.IdElemento == filtro.Valor.Entero()))
                    .Select(x => new { x.IdFacturaRec });

                consulta = consulta.Where(f => idsDeFacturasEnLaRemesaPasada.Any(x => x.IdFacturaRec == f.Id));
                filtro.Aplicado = true;
                return consulta;
            }

            filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaFacturaRec.FiltroPorRemesaPag.ToLower() && !x.Aplicado);

            if (filtro != null)
            {
                if (filtro.Valor.Entero() == ltrParametrosNeg.MostrarTodos)
                    filtro.Aplicado = true;
                else
                {
                    var idsDeFacturasDeSusPagosRemesados = contexto.Set<PagoDtm>()
                        .Where(x => x.IdFacturaRec != null
                        && contexto.Set<PagoDeUnaRemesaDtm>()
                        .Any(y => y.IdPago == x.Id)).Select(x => new { x.IdFacturaRec });

                    if (filtro.Valor.Entero() == ltrParametrosNeg.ConRelacion)
                        consulta = consulta.Where(f => idsDeFacturasDeSusPagosRemesados.Any(x => x.IdFacturaRec == f.Id));
                    else
                        consulta = consulta.Where(f => !idsDeFacturasDeSusPagosRemesados.Any(x => x.IdFacturaRec == f.Id));

                    filtro.Aplicado = true;
                }
            }

            return consulta;
        }

        public static IQueryable<FacturaRecDtm> FiltroPorEtapa(this IQueryable<FacturaRecDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            // Buscamos todos los filtros de etapa que aún no han sido aplicados
            var filtrosEtapa = filtros.Where(x => x.Clausula.ToLower() == ltrDeUnaFacturaRec.FiltroPorEtapa.ToLower() && !x.Aplicado).ToList();

            foreach (var filtro in filtrosEtapa)
            {
                consulta = consulta.FiltrosPorEtapa<FacturaRecDtm>(filtro);
            }

            return consulta;
        }

        public static IQueryable<FacturaRecDtm> FiltroPorFormaDePago(this IQueryable<FacturaRecDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnPago.FiltroPorFormaDePago.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                var cancelados = enumNegocio.Pago.TipoDtm().Cancelados(contexto);

                if (filtro.Valor == ltrDeUnPago.FiltroDePagosContado)
                    consulta = consulta.Where(f => contexto.Set<PagoDtm>().Where(p => !cancelados.Contains(p.IdEstado))
                                  .Any(p => contexto.Set<PagosDeUnaFacturaRecDtm>()
                                  .Any(pf => pf.idElemento1 == f.Id && pf.idElemento2 == p.Id && p.Clase.Equals(enumClaseDePago.Contado) && p.IdTarjetaDePago == null && p.IdCuentaDePago == null)
                                  ));

                else if (filtro.Valor == ltrDeUnPago.FiltroDePagosTarjeta)
                    consulta = consulta.Where(f => contexto.Set<PagoDtm>().Where(p => !cancelados.Contains(p.IdEstado))
                                  .Any(p => contexto.Set<PagosDeUnaFacturaRecDtm>()
                                  .Any(pf => pf.idElemento1 == f.Id && pf.idElemento2 == p.Id && p.Clase.Equals(enumClaseDePago.Contado) && p.IdTarjetaDePago != null)
                                  ));

                else if (filtro.Valor == ltrDeUnPago.FiltroDePagosDomiciliado)
                    consulta = consulta.Where(f => contexto.Set<PagoDtm>().Where(p => !cancelados.Contains(p.IdEstado))
                                  .Any(p => contexto.Set<PagosDeUnaFacturaRecDtm>()
                                  .Any(pf => pf.idElemento1 == f.Id && pf.idElemento2 == p.Id && p.Clase.Equals(enumClaseDePago.Contado) && p.IdCuentaDePago != null && p.IdTarjetaDePago == null)
                                  ));

                else if (filtro.Valor == ltrDeUnPago.FiltroDePagosTransferencia)
                    consulta = consulta.Where(f => contexto.Set<PagoDtm>().Where(p => !cancelados.Contains(p.IdEstado))
                                  .Any(p => contexto.Set<PagosDeUnaFacturaRecDtm>()
                                  .Any(pf => pf.idElemento1 == f.Id && pf.idElemento2 == p.Id && p.Clase.Equals(enumClaseDePago.Transferencia))
                                  ));

                else if (filtro.Valor == ltrDeUnPago.FiltroDePagosRemesa)
                    consulta = consulta.Where(f => contexto.Set<PagoDtm>().Where(p => !cancelados.Contains(p.IdEstado))
                                  .Any(p => contexto.Set<PagosDeUnaFacturaRecDtm>()
                                  .Any(pf => pf.idElemento1 == f.Id && pf.idElemento2 == p.Id && p.Clase.Equals(enumClaseDePago.Remesa))
                                  ));


                filtro.Aplicado = true;
                return consulta;
            }
            return consulta;
        }

        public static IQueryable<FacturaRecDtm> FiltroPorIvaRetencion(this IQueryable<FacturaRecDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnPago.FiltroDeIvaIrpf.ToLower() && !x.Aplicado);
            if (filtro != null)
            {

                if (filtro.Valor == ltrDeUnPago.FiltroConIrpf)
                    consulta = consulta.Where(f => contexto.Set<LineaDeUnaFarDtm>().Any(linea => linea.IdIrpf != null && linea.IdElemento == f.Id));
                else if (filtro.Valor == ltrDeUnPago.FiltroConIva)
                    consulta = consulta.Where(f => contexto.Set<LineaDeUnaFarDtm>().Any(linea => linea.IdIvaS != null && linea.IdElemento == f.Id));
                else if (filtro.Valor == ltrDeUnPago.FiltroConIvaExento)
                    consulta = consulta.Where(f => contexto.Set<LineaDeUnaFarDtm>().Any(linea => linea.IdIvaS != null && linea.IdElemento == f.Id &&
                                                   contexto.Set<IvaSoportadoDtm>().Any(iva => iva.Exento && iva.Id == linea.IdIvaS)));
                else if (filtro.Valor == ltrDeUnPago.FiltroSinIvaNiIrpf)
                    consulta = consulta.Where(f => contexto.Set<LineaDeUnaFarDtm>().Count(linea => linea.IdIvaS == null && linea.IdIrpf == null && linea.IdElemento == f.Id) ==
                    contexto.Set<LineaDeUnaFarDtm>().Count(linea => linea.IdElemento == f.Id));
                else if (filtro.Valor == ltrDeUnPago.FiltroConIvaNsj)
                    consulta = consulta.Where(f => contexto.Set<LineaDeUnaFarDtm>().Any(linea => linea.IdIvaS != null && linea.IdElemento == f.Id &&
                                               contexto.Set<IvaSoportadoDtm>().Any(iva => (int)iva.Clase == (int)enumClasesDeIvaSop.NSJ && iva.Id == linea.IdIvaS)));
                else if (filtro.Valor == ltrDeUnPago.FiltroConIvaIsp)
                    consulta = consulta.Where(f => contexto.Set<LineaDeUnaFarDtm>().Any(linea => linea.IdIvaS != null && linea.IdElemento == f.Id &&
                                               contexto.Set<IvaSoportadoDtm>().Any(iva => (int)iva.Clase == (int)enumClasesDeIvaSop.ISP && iva.Id == linea.IdIvaS)));

                filtro.Aplicado = true;
                return consulta;
            }
            return consulta;
        }

        public static IQueryable<FacturaRecDtm> FiltroPorNaturaleza(this IQueryable<FacturaRecDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaFacturaRec.FiltroPorNaturaleza.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                consulta = consulta.Where(x => contexto.Set<LineaDeUnaFarDtm>().Any(linea => linea.IdNaturaleza != null && linea.IdNaturaleza == filtro.Valor.Entero() && linea.IdElemento == x.Id));
                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<FacturaRecDtm> ExcluirLasNoTotalizables(this IQueryable<FacturaRecDtm> consulta)
        {
            var filtroPorEstados = new ClausulaDeFiltrado(nameof(IUsaEstado.IdEstado), enumCriteriosDeFiltrado.noEsNingunoDe,
                enumEtapasDeFacturasRec.FAR_Etapa_Anulada.Estados() + "," + enumEtapasDeFacturasRec.FAR_Etapa_Devuelta.Estados());
            consulta = consulta.AplicarFiltroPorEntero(filtroPorEstados);
            return consulta;
        }
    }
}
