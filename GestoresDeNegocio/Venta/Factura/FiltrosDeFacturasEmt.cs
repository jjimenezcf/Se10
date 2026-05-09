using Gestor.Errores;
using GestorDeElementos;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.Presupuesto;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Tarea;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Ventas;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Utilidades;

namespace GestoresDeNegocio.Ventas
{
    public static class FiltrosDeFacturasEmt
    {
        public static IQueryable<FacturaEmtDtm> FiltroPorExpediente(this IQueryable<FacturaEmtDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaFacturaEmt.IdExpediente.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                var ppts = contexto.Set<PresupuestoDtm>().Where(p => p.IdExpediente == filtro.Valor.Entero());
                consulta = consulta.Where(fae => ppts.Any(ppt => ppt.Id == fae.IdPresupuesto));
                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<FacturaEmtDtm> FiltroPorPresupuestos(this IQueryable<FacturaEmtDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaFacturaEmt.IdPresupuesto.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                consulta = consulta.AplicarFiltroPorEntero(filtro);
                return consulta;
            }
            var filtroNombre = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaFacturaEmt.NombrePresupuesto.ToLower() && !x.Aplicado);
            if (filtroNombre != null && !string.IsNullOrEmpty(filtroNombre.Valor))
            {
                var busqueda = filtroNombre.Valor.ToLower();
                consulta = consulta.Where(fae => fae.IdPresupuesto != null && (fae.Presupuesto.Nombre.ToLower().Contains(busqueda) || fae.Presupuesto.Referencia.ToLower().Contains(busqueda)));
                filtroNombre.Aplicado = true;
                return consulta;
            }

            filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaFacturaEmt.AsociadaAUnPpt.ToLower());
            return filtro != null ? consulta.FiltroSiHayDependenciaDe(filtro, nameof(FacturaEmtDtm.IdPresupuesto)) : consulta;
        }

        public static IQueryable<FacturaEmtDtm> FiltroPorPlanificacionesDeVenta(this IQueryable<FacturaEmtDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaFacturaEmt.IdPlfDeVenta.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                var plfs = contexto.Set<PlanificacionDeVentaDtm>().Where(plf => plf.Id == filtro.Valor.Entero());
                consulta = consulta.Where(fae => plfs.Any(plf => plf.IdFacturaEmt == fae.Id));
                filtro.Aplicado = true;
                return consulta;
            }
            var filtroNombre = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaFacturaEmt.NombrePlfDeVenta.ToLower() && !x.Aplicado);
            if (filtroNombre != null && !string.IsNullOrEmpty(filtroNombre.Valor))
            {
                var busqueda = filtroNombre.Valor.ToLower();
                // Buscamos las planificaciones que coincidan con el texto
                var plfsCoincidentes = contexto.Set<PlanificacionDeVentaDtm>().Where(plf =>
                    plf.Nombre.ToLower().Contains(busqueda) ||
                    plf.Referencia.ToLower().Contains(busqueda));

                // Filtramos las facturas que estén vinculadas a esas planificaciones
                consulta = consulta.Where(fae => plfsCoincidentes.Any(plf => plf.IdFacturaEmt == fae.Id));
                filtroNombre.Aplicado = true;
                return consulta;
            }

            filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaFacturaEmt.AsociadaAUnaPlv.ToLower());
            if (filtro != null)
            {
                if (filtro.Valor.Entero() == ltrParametrosNeg.ConRelacion)
                {
                    var plfsConFae = contexto.Set<PlanificacionDeVentaDtm>().Where(x => x.IdFacturaEmt != null);
                    consulta = consulta.Where(fae => plfsConFae.Any(plf => plf.IdFacturaEmt == fae.Id));
                }

                if (filtro.Valor.Entero() == ltrParametrosNeg.SinRelacion)
                {
                    var plfsConFae = contexto.Set<PlanificacionDeVentaDtm>().Where(x => x.IdFacturaEmt != null).Select(x => x.IdFacturaEmt);
                    consulta = consulta.Where(fae => !plfsConFae.Contains(fae.Id));
                }
                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<FacturaEmtDtm> FiltroPorFacturaNoIncluidaEnRemesa(this IQueryable<FacturaEmtDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaFacturaEmt.ExcluirFacturasDeUnaRemesa.ToLower() && x.Criterio == enumCriteriosDeFiltrado.diferente && !x.Aplicado);
            if (filtro != null)
            {
                var remesa = contexto.SeleccionarPorId<RemesaFaeDtm>(filtro.Valor.Entero());

                var estadosEmitida = enumEtapasDeFacturasEmt.FAE_Etapa_Emitida.Lista();
                var estadosDevueltos = enumEtapasDeFacturasEmt.FAE_Etapa_Devuelta.Lista();

                if (!estadosEmitida.Any())
                    GestorDeErrores.Emitir($"Debe definir el parámetro '{enumEtapasDeFacturasEmt.FAE_Etapa_Emitida}' del negocio de '{enumNegocio.FacturaEmitida}'");

                if (!estadosDevueltos.Any())
                    GestorDeErrores.Emitir($"Debe definir el parámetro '{enumEtapasDeFacturasEmt.FAE_Etapa_Devuelta}' del negocio de '{enumNegocio.FacturaEmitida}'");

                if (remesa.Clase == enumClaseDeRemesaFae.Emitidas)
                {
                    consulta = consulta.Where(x => estadosEmitida.Contains(x.IdEstado) && x.ClaseRectificativa == null);
                }
                else
                {
                    consulta = consulta.Where(x => estadosDevueltos.Contains(x.IdEstado) && x.ClaseRectificativa == null);
                }

                var facturasDeLaRemesa = contexto.Set<FacturaEmtDeUnaRemesaDtm>().Where(fr => fr.IdElemento == filtro.Valor.Entero());
                consulta = consulta.Where(factura => !facturasDeLaRemesa.Any(b => b.IdFactura == factura.Id));
                filtro.Aplicado = true;
                return consulta;
            }
            return consulta;
        }

        public static IQueryable<FacturaEmtDtm> FiltroParaSeleccionarFacturaEnRemesas(this IQueryable<FacturaEmtDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaRemesaFae.IdFacturaEnRemesa.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                consulta = consulta.Where(factura => contexto.Set<FacturaEmtDeUnaRemesaDtm>().Any(b => b.IdFactura == factura.Id));
                filtro.Aplicado = true;
                filtros.Add(new ClausulaDeFiltrado(nameof(INombre.Expresion), enumCriteriosDeFiltrado.contiene, filtro.Valor));
                return consulta;
            }
            return consulta;
        }

        public static IQueryable<FacturaEmtDtm> FiltroParaSeleccionarFacturaDeUnaTarea(this IQueryable<FacturaEmtDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaFacturaEmt.FacturaDeUnaTarea.ToLower() && !x.Valor.IsNullOrEmpty() && !x.Aplicado);
            if (filtro != null)
            {
                consulta = consulta.Where(factura => contexto.Set<TareaDtm>().Any(tarea => tarea.IdFacturaEmt == factura.Id));
                filtro.Clausula = nameof(INombre.Expresion);
            }
            return consulta;
        }

        public static IQueryable<FacturaEmtDtm> FiltroFacturasEnUnaEstimacion(this IQueryable<FacturaEmtDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            return consulta.FiltroDeElementosConVinculosA<FacturaEmtDtm, CircuitoDocDeUnaFacturaEmtDtm>(contexto, filtros, ltrDeUnaFacturaEmt.IdEstimacionDirecta, ltrDeUnaEstimacion.VinculosAUnaEstimacion);
        }

        public static IQueryable<FacturaEmtDtm> FiltroFacturasEnUnLoteContable(this IQueryable<FacturaEmtDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaFacturaEmt.IdLoteContable.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                var preasientosDeUnLote = contexto.Set<PreasientoDtm>().Where(p => p.NegocioReferenciado.Equals(enumNegocio.FacturaEmitida) &&
                                                   contexto.Set<CircuitoDocDeUnPreasientoDtm>().Any(lote => lote.idElemento2 == filtro.Valor.Entero() && lote.idElemento1 == p.Id));
                consulta = consulta.Where(f => preasientosDeUnLote.Any(p => p.IdReferenciado == f.Id));
                filtros.MostrarTodos();
                filtro.Aplicado = true;
                return consulta;
            }

            filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnLoteContable.VinculosAUnLote.ToLower());
            if (filtro != null)
            {
                var preasientosEnLotes = contexto.Set<PreasientoDtm>().Where(p => p.NegocioReferenciado.Equals(enumNegocio.FacturaEmitida) &&
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

        public static IQueryable<FacturaEmtDtm> FiltroDeFacturasRectificadas(this IQueryable<FacturaEmtDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaFacturaEmt.IdRectificativa.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                var rectificadas = contexto.Set<RectificativaEmtDtm>().Where(r => r.IdElemento == filtro.Valor.Entero());
                consulta = consulta.Where(f => rectificadas.Any(r => r.IdRectificada == f.Id));
                filtros.MostrarTodos();
                filtro.Aplicado = true;
                return consulta;
            }

            var filtroNombre = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaFacturaEmt.NombreRectificativa.ToLower() && !x.Aplicado);
            if (filtroNombre != null && !string.IsNullOrEmpty(filtroNombre.Valor))
            {
                var busqueda = filtroNombre.Valor.ToLower();
                var partes = busqueda.Split("-");

                // Buscamos las facturas rectificativas (IdElemento) que coincidan con el texto
                // Accedemos a la propiedad 'Factura' de la tabla RectificativaEmtDtm (asumiendo que existe la navegación)
                var rectificativasCoincidentes = contexto.Set<RectificativaEmtDtm>().Where(r =>
                    r.Rectificada.Nombre.ToLower().Contains(busqueda) ||
                    r.Rectificada.Referencia.ToLower().Contains(busqueda) ||
                    (partes.Length == 3 ? r.Rectificada.Ano == partes[0].Entero() && r.Rectificada.Serie.Equals(partes[1]) && r.Rectificada.Numero == partes[2].Entero()
                    : false));

                // Filtramos las facturas originales (IdRectificada) vinculadas a esas rectificativas
                consulta = consulta.Where(f => rectificativasCoincidentes.Any(r => r.IdRectificada == f.Id));

                filtroNombre.Aplicado = true;
                return consulta;
            }

            filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaFacturaEmt.Rectificadas.ToLower());
            if (filtro != null)
            {
                var rectificadas = contexto.Set<RectificativaEmtDtm>().Where(r => true);
                if (filtro.Valor.ToLower() == ltrParametrosNeg.ConRelacion.ToString().ToLower())
                {
                    consulta = consulta.Where(factura => rectificadas.Any(r => r.IdRectificada == factura.Id));
                }
                if (filtro.Valor.ToLower() == ltrParametrosNeg.SinRelacion.ToString().ToLower())
                {
                    consulta = consulta.Where(factura => !rectificadas.Any(r => r.IdRectificada == factura.Id));
                }
                filtro.Aplicado = true;
            }

            return consulta;
        }
        public static IQueryable<FacturaEmtDtm> FiltroPorVarios(this IQueryable<FacturaEmtDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaFacturaEmt.BuscarPorVarios.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                if (filtro.Valor.StartsWith("c:") || filtro.Valor.StartsWith("d:") || filtro.Valor.StartsWith("n:"))
                {
                    if (filtro.Valor.StartsWith("d:"))
                    {
                        filtro.Clausula = ltrDeUnaFacturaEmt.FiltrarPorNombreCliente;
                        filtro.Valor = filtro.Valor.Substring(2);
                        return consulta.FiltroPorDeudor(contexto, filtros);
                    }

                    if (filtro.Valor.StartsWith("n:"))
                    {
                        filtro.Clausula = ltrDeUnaFacturaEmt.FiltroPorNumeroDeFactura;
                        filtro.Valor = filtro.Valor.Substring(2);
                        return consulta.FiltroPorNumeroFactura(filtros);
                    }
                    filtro.Valor = filtro.Valor.Substring(2);
                    filtro.Clausula = nameof(FacturaEmtDtm.Nombre);
                }
                else
                {
                    filtro.Clausula = ltrDeUnaFacturaEmt.FiltroPorExprexion;
                    return consulta.FiltroPorExpresion(filtros);
                }
            }
            return consulta;
        }
        public static IQueryable<FacturaEmtDtm> FiltroPorExpresion(this IQueryable<FacturaEmtDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == nameof(ltrDeUnaFacturaEmt.FiltroPorExprexion).ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                var numeroDeFactura = filtro.Valor.Split("-");
                if (numeroDeFactura.Length == 3 && numeroDeFactura[0].EsEntero() && numeroDeFactura[2].EsEntero())
                {
                    filtro.Clausula = ltrDeUnaFacturaEmt.FiltroPorNumeroDeFactura;
                    return consulta.FiltroPorNumeroFactura(filtros);
                }
                else
                {
                    consulta = consulta.Where(x => x.Referencia.Contains(filtro.Valor) || x.Nombre.Contains(filtro.Valor));
                }
                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<FacturaEmtDtm> FiltroPorNumeroFactura(this IQueryable<FacturaEmtDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaFacturaEmt.FiltroPorNumeroDeFactura.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                var numeroDeFactura = filtro.Valor.Split("-");
                if (numeroDeFactura.Length != 3)
                    GestorDeErrores.Emitir($"El número de factura para filtrar es incorrecto, '{filtro.Valor.ToString()}', formato: año-serie-nº");

                consulta = consulta.Where(x => x.Ano == numeroDeFactura[0].Entero() && x.Serie.Equals(numeroDeFactura[1]) && x.Numero == numeroDeFactura[2].Entero());
                filtros.MostrarTodos();

                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<FacturaEmtDtm> FiltroPorPartesTr(this IQueryable<FacturaEmtDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaFacturaEmt.IdParteTr.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                consulta = consulta.AplicarFiltroPorEntero(filtro);
                return consulta;
            }
            var filtroNombre = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaFacturaEmt.NombreParteTr.ToLower() && !x.Aplicado);
            if (filtroNombre != null && !string.IsNullOrEmpty(filtroNombre.Valor))
            {
                var busqueda = filtroNombre.Valor.ToLower();

                // Buscamos los PTRs que coincidan con el texto
                var ptrsCoincidentes = contexto.Set<ParteTrDtm>().Where(p =>
                    p.Nombre.ToLower().Contains(busqueda) ||
                    p.Referencia.ToLower().Contains(busqueda));

                // Filtramos facturas que tengan ese PTR en cabecera O en alguna de sus líneas
                consulta = consulta.Where(fae =>
                    ptrsCoincidentes.Any(p => p.Id == fae.IdParteTr) ||
                    contexto.Set<LineaDeUnaFaeDtm>().Any(l => l.IdElemento == fae.Id && ptrsCoincidentes.Any(p => p.Id == l.IdParteTr))
                );

                filtroNombre.Aplicado = true;
                return consulta;
            }
            filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaFacturaEmt.AsociadaAUnPtr.ToLower());
            if (filtro != null)
            {
                if (filtro.Valor.Entero() == ltrParametrosNeg.ConRelacion)
                    consulta = consulta.Where(x => x.IdParteTr != null || contexto.Set<LineaDeUnaFaeDtm>().Where(l => l.IdParteTr != null && l.IdElemento == x.Id).Any());

                if (filtro.Valor.Entero() == ltrParametrosNeg.SinRelacion)
                    consulta = consulta.Where(x => x.IdParteTr == null && contexto.Set<LineaDeUnaFaeDtm>().Where(l => l.IdParteTr == null && l.IdElemento == x.Id).Any());
                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<FacturaEmtDtm> FiltroPorContratos(this IQueryable<FacturaEmtDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaFacturaEmt.IdContrato.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                consulta = consulta.AplicarFiltroPorEntero(filtro);
                return consulta;
            }

            filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaFacturaEmt.AsociadaAUnContrato.ToLower());
            return filtro != null ? consulta.FiltroSiHayDependenciaDe(filtro, nameof(FacturaEmtDtm.IdContrato)) : consulta;
        }

        public static IQueryable<FacturaEmtDtm> FiltroPorCliente(this IQueryable<FacturaEmtDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaFacturaEmt.IdCliente.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                consulta = consulta.AplicarFiltroPorEntero(filtro);
            }
            return consulta;
        }

        public static IQueryable<FacturaEmtDtm> FiltroPorDeudor(this IQueryable<FacturaEmtDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaFacturaEmt.FiltrarPorNombreCliente.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                consulta = consulta.Where(fae => contexto.Set<ClienteDtm>().Any(cliente => cliente.Nombre.Contains(filtro.Valor) && cliente.Id == fae.IdCliente));
                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<FacturaEmtDtm> FiltroPorRemesas(this IQueryable<FacturaEmtDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaFacturaEmt.IdRemesaFae.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                var facturasDeLaRemesa = contexto.Set<FacturaEmtDeUnaRemesaDtm>().Where(x => x.IdElemento == filtro.Valor.Entero());
                consulta = consulta.Where(x => facturasDeLaRemesa.Any(fr => fr.IdFactura == x.Id));
                return consulta;
            }

            filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaFacturaEmt.IncluidaEnRemesa.ToLower());
            if (filtro != null)
            {
                if (filtro.Valor.Entero() == ltrParametrosNeg.ConRelacion)
                    consulta = consulta.Where(x => contexto.Set<FacturaEmtDeUnaRemesaDtm>().Any(fr => fr.IdFactura == x.Id));

                if (filtro.Valor.Entero() == ltrParametrosNeg.SinRelacion)
                    consulta = consulta.Where(x => !contexto.Set<FacturaEmtDeUnaRemesaDtm>().Any(fr => fr.IdFactura == x.Id));
            }
            return consulta;
        }

        public static IQueryable<FacturaEmtDtm> FiltroPorImportesSinIva(this IQueryable<FacturaEmtDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaFacturaEmt.FiltroPorImporteSinIva.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                var rango = filtro.ParsearRango();
                var sumatorio = contexto.Set<LineaDeUnaFaeDtm>()
                              .GroupBy(lin => lin.IdElemento)
                              .Select(tot => new
                              {
                                  IdElemento = tot.Key,
                                  ImporteBase = tot.Sum(l =>
                              (
                                (l.Precio == null || l.Cantidad == null) ? 0 : (decimal)l.Precio * (decimal)l.Cantidad)
                                -
                                (
                                 (l.Precio == null || l.Cantidad == null) ? 0 : (decimal)l.Precio * (decimal)l.Cantidad)
                                  *
                                 (l.Descuento == null ? 0 : (decimal)l.Descuento / 100)
                                )
                              }
                              );

                consulta = consulta.Where(fae => sumatorio.Any(sum => sum.IdElemento == fae.Id &&
                                                                      (rango.desde == default ? true : (decimal)rango.desde <= sum.ImporteBase) &&
                                                                      (rango.hasta == default ? true : (decimal)rango.hasta >= sum.ImporteBase)));
                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<FacturaEmtDtm> FiltroPorCobrado(this IQueryable<FacturaEmtDtm> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaFacturaEmt.FiltroPorCobrado.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                var rango = filtro.ParsearRango();
                var sumatorio = contexto.Set<CobroDeFaeDtm>()
                    .GroupBy(cob => cob.IdElemento)
                    .Select(tot => new { IdElemento = tot.Key, Cobrado = tot.Sum(l => l.Cobrado) });

                consulta = consulta.Where(fae =>
                    // Si buscamos 0;0 y no hay cobros, la factura debe salir
                    ((rango.desde == 0 && rango.hasta == 0) && !contexto.Set<CobroDeFaeDtm>().Any(c => c.IdElemento == fae.Id))
                    ||
                    // O bien, el sumatorio normal para rangos con valor
                    sumatorio.Any(sum => sum.IdElemento == fae.Id &&
                                        (rango.desde == default ? true : (decimal)rango.desde <= sum.Cobrado) &&
                                        (rango.hasta == default ? true : (decimal)rango.hasta >= sum.Cobrado))
                ); filtro.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<FacturaEmtDtm> FiltroPorFechaDeFacturacion(this IQueryable<FacturaEmtDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaFacturaEmt.FiltroPorFechaDeEmision.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                consulta = consulta.AplicarFiltroEntreFechas(filtro, nameof(FacturaEmtDtm.FacturadaEl));
                consulta = consulta.Where(fae => !(fae.Estado.Inicial || fae.Estado.Cancelado));
            }

            return consulta;
        }

        public static IQueryable<FacturaEmtDtm> FiltroPorFechaDeVencimiento(this IQueryable<FacturaEmtDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaFacturaEmt.FiltroPorFechaDeVencimiento.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                consulta = consulta.AplicarFiltroEntreFechas(filtro, nameof(FacturaEmtDtm.VenceEl));
                consulta = consulta.Where(fae => !(fae.Estado.Inicial || fae.Estado.Cancelado));
            }
            return consulta;
        }

        public static IQueryable<FacturaEmtDtm> FiltroPorNumerosDeFacturas(this IQueryable<FacturaEmtDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaFacturaEmt.FiltroPorNumerosDeFactura.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                var rangos = filtro.Valor.Split(Simbolos.PuntoComa);
                if (rangos.Length != 2)
                    GestorDeErrores.Emitir($"Para filtrar por numeros se ha de indicar un patrón 'yyyy-s-número;yyyy-s-número' donde el primero o el último puede ser nulo");

                var rango1 = rangos[0] == "undefined" ? null : rangos[0].Split('-');
                var rango2 = rangos[1] == "undefined" ? null : rangos[1].Split('-');

                if (rango1 != null && rango1.Length != 3)
                    GestorDeErrores.Emitir($"El número desde ha de seguir el patrón 'yyyy-s-número'");

                if (rango2 != null && rango2.Length != 3)
                    GestorDeErrores.Emitir($"El número hasta ha de seguir el patrón 'yyyy-s-número'");

                if (rango1 != null && rango2 != null && rango1[1] != rango2[1])
                    GestorDeErrores.Emitir($"Los numeros de facturas han de indicar la misma serie");

                if (rango1 != null && rango2 != null && rango1[0].Entero() != rango2[0].Entero())
                    GestorDeErrores.Emitir($"Los numeros de facturas han de indicar el mismo año");

                if (rango1 != null && rango2 != null && rango1[2].Entero() > rango2[2].Entero())
                    GestorDeErrores.Emitir($"El número de la primera factura no puede ser mayor que la de la segunda");

                if (rango1 != null)
                    consulta = consulta.Where(x => x.Serie == rango1[1] && x.Ano == rango1[0].Entero() && x.Numero >= rango1[2].Entero());
                if (rango2 != null)
                    consulta = consulta.Where(x => x.Serie == rango2[1] && x.Ano == rango2[0].Entero() && x.Numero <= rango2[2].Entero());
            }
            return consulta;
        }

        public static IQueryable<FacturaEmtDtm> FiltroPorPrefacturasDeUnPpt(this IQueryable<FacturaEmtDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaFacturaEmt.PrefacturaDeUnPpt.ToLower());
            if (filtro != null)
            {
                consulta = consulta.Where(fae => fae.IdPresupuesto == filtro.Valor.Entero() && enumEtapasDeFacturasEmt.FAE_Etapa_Prefactura.Lista().Contains(fae.IdEstado));
            }
            return consulta;
        }
        public static IQueryable<FacturaEmtDtm> FiltroPorEtapa(this IQueryable<FacturaEmtDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            // Buscamos todos los filtros de etapa que aún no han sido aplicados
            var filtrosEtapa = filtros.Where(x => x.Clausula.ToLower() == ltrDeUnaFacturaEmt.FiltroPorEtapa.ToLower() && !x.Aplicado).ToList();

            foreach (var filtro in filtrosEtapa)
            {
                consulta = consulta.FiltrosPorEtapa<FacturaEmtDtm>(filtro);
            }

            return consulta;
        }

        public static IQueryable<FacturaEmtDtm> FiltroPorSociedad(this IQueryable<FacturaEmtDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDeUnaFacturaEmt.IdSociedad.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                consulta = consulta.Where(fae => fae.Cg.IdSociedad == filtro.Valor.Entero());
                filtro.Aplicado = true;
            }
            return consulta;
        }
    }
}
