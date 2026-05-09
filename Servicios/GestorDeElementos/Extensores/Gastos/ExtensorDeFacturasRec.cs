using DocumentFormat.OpenXml.Drawing.Diagrams;
using Microsoft.EntityFrameworkCore;
using ModeloDeDto.Gastos;
using ModeloDeDto.Negocio;
using ModeloDeDto.Terceros;
using Newtonsoft.Json;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Gastos;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.MaestrosTecnico;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Ventas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utilidades;
using static Gestor.Errores.GestorDeErrores;
using static ServicioDeDatos.Elemento.Enumerados;

namespace GestorDeElementos.Extensores
{
    public class TotalesFactura
    {
        public decimal TotalBi { get; set; }
        public decimal TotalIva { get; set; }
        public decimal TotalIrpf { get; set; }
    }

    public static class ExtensorDeFacturasRec
    {
        public static bool UsaElDetalleDe(Type tipoDeDetalle)
        {
            if (tipoDeDetalle == typeof(LineaDeUnaFarDtm))
                return true;

            return false;
        }

        public static bool EstaEnLaEtapa(this FacturaRecDtm far, string etapa) => etapa.ToLista<int>(Simbolos.Coma).Contains(far.IdEstado);

        public static string ImpuestosToString(this FacturaRecDtm factura, ContextoSe contexto)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Far_Impuestos);
            if (!cache.ContainsKey(factura.Id.ToString()))
            {
                var impuestos = factura.Impuestos(contexto);
                if (impuestos == null || impuestos.Count == 0)
                {
                    cache[factura.Id.ToString()] = string.Empty;
                }

                cache[factura.Id.ToString()] = string.Join(Simbolos.separadorParaMostrarEncolumnado, impuestos.Select(kvp => $"{kvp.Key} - {kvp.Value.ToMoneda()}"));
            }
            return (string)cache[factura.Id.ToString()];
        }

        public static string NaturalezasToString(this FacturaRecDtm factura, ContextoSe contexto)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Far_Naturalezas);
            if (!cache.ContainsKey(factura.Id.ToString()))
            {
                var lineas = factura.Detalles<LineaDeUnaFarDtm>(contexto);
                if (lineas.Count == 0)
                {
                    cache[factura.Id.ToString()] = string.Empty;
                }
                else
                {
                    var naturalezas = new List<int>();
                    foreach (var linea in lineas)
                    {
                        if (linea.Clase == enumClaseDeLineaFar.LineaDeIva || linea.Clase == enumClaseDeLineaFar.LineaDeIrpf)
                            continue;
                        var id = linea.IdNaturaleza.Entero();
                        if (naturalezas.Contains(id) || id == 0) continue;
                        naturalezas.Add(id);
                    }

                    var cadena = "";
                    foreach (var naturaleza in naturalezas)
                    {
                        cadena = cadena + contexto.SeleccionarPorId<NaturalezaDtm>(naturaleza).Sigla + "; ";
                    }
                    cache[factura.Id.ToString()] = cadena.EndsWith("; ") ? cadena.Substring(0, cadena.Length - 2) : cadena;
                }

            }
            return (string)cache[factura.Id.ToString()];
        }


        public static string FormasDePagoToString(this FacturaRecDtm factura, ContextoSe contexto)
        {
            var pagos = factura.PagosRealizados(contexto);
            if (pagos.Count == 0)
            {
                return string.Empty;
            }

            return string.Join(Simbolos.separadorParaMostrarEncolumnado, pagos.Select(pago => $"{pago.DetalleDeFormaDePago(contexto)}"));
        }

        public static void CargarCacheDeTotales(ContextoSe contexto, List<int> idsFacturas)
        {
            // 1. Obtenemos las cachés primero
            var cacheBi = ServicioDeCaches.Obtener(CacheDe.Far_Base);
            var cacheIva = ServicioDeCaches.Obtener(CacheDe.Far_Iva);
            var cacheIrpf = ServicioDeCaches.Obtener(CacheDe.Far_Irpf);

            // 2. Filtramos la lista de IDs: solo nos quedamos con los que NO están en la caché
            // Comprobamos cacheBi (si está en una, asumimos que está en todas por tu lógica de guardado)
            var idsNuevos = idsFacturas
                .Where(id => !cacheBi.ContainsKey(id.ToString()))
                .ToList();

            // 3. Si no hay facturas nuevas que procesar, salimos para ahorrar la consulta SQL
            if (!idsNuevos.Any())
                return;

            // 4. Ejecutamos la select solo para los IDs faltantes
            var listaTotales = contexto.Set<LineaDeUnaFarDtm>()
                .Where(l => idsNuevos.Contains(l.IdElemento)) // Usamos la lista filtrada
                .GroupBy(l => l.IdElemento)
                .Select(g => new
                {
                    IdFactura = g.Key,
                    Valores = new TotalesFactura
                    {
                        TotalBi = g.Where(l => l.Clase == enumClaseDeLineaFar.BaseImponible ||
                                               l.Clase == enumClaseDeLineaFar.BiExenta ||
                                               l.Clase == enumClaseDeLineaFar.BiConIva)
                                   .Sum(l => (decimal?)l.BaseImponible) ?? 0,

                        TotalIva = g.Where(l => (l.Clase == enumClaseDeLineaFar.BiConIva || l.Clase == enumClaseDeLineaFar.LineaDeIva) &&
                                                l.IvaSoportado.Clase != enumClasesDeIvaSop.ISP)
                                    .Sum(l => (decimal?)(l.BaseImponible * (l.PorcentajeIva ?? 0) / 100)) ?? 0,

                        TotalIrpf = g.Where(l => l.Clase == enumClaseDeLineaFar.LineaDeIrpf)
                                     .Sum(l => (decimal?)(l.BaseImponible * (l.PorcentajeIrpf ?? 0) / 100)) ?? 0
                    }
                })
                .ToList();

            // 5. Poblamos las cachés con los nuevos resultados
            foreach (var item in listaTotales)
            {
                string key = item.IdFactura.ToString();
                cacheBi[key] = item.Valores.TotalBi;
                cacheIva[key] = item.Valores.TotalIva;
                cacheIrpf[key] = item.Valores.TotalIrpf;
            }
        }

        public static void CargarCacheDeLineasMasivamente(ContextoSe contexto, List<int> idsFacturas)
        {
            var cacheDetalle = ServicioDeCaches.Obtener(CacheDe.Detalle);
            var tipoNombre = typeof(LineaDeUnaFarDtm).Name;

            // Forzamos la lectura si falta cualquiera de las dos versiones del índice
            var idsNuevos = idsFacturas
                .Where(id => !cacheDetalle.ContainsKey($"{tipoNombre}-{id}-S") ||
                             !cacheDetalle.ContainsKey($"{tipoNombre}-{id}-N"))
                .Distinct()
                .ToList();

            if (!idsNuevos.Any()) return;

            var todasLasLineas = contexto.Set<LineaDeUnaFarDtm>()
                .Include(l => l.IvaSoportado)
                .Include(l => l.Irpf)
                .Where(l => idsNuevos.Contains(l.IdElemento))
                .ToList();

            var grupos = todasLasLineas.GroupBy(l => l.IdElemento);

            foreach (var grupo in grupos)
            {
                int idFactura = grupo.Key;
                var listaDetalles = grupo.Cast<IDetalle>().ToList();

                cacheDetalle[$"{tipoNombre}-{idFactura}-S"] = listaDetalles;
                cacheDetalle[$"{tipoNombre}-{idFactura}-N"] = listaDetalles;
            }
        }

        private static Dictionary<string, decimal> Impuestos(this FacturaRecDtm factura, ContextoSe contexto)
        {
            var impuestos = new Dictionary<string, decimal>();

            var ivas = factura.Ivas(contexto);
            var irpfs = factura.Irpfs(contexto);

            foreach (var iva in ivas)
            {
                impuestos.Add($"Iva: {contexto.SeleccionarPorId<IvaSoportadoDtm>(iva.IdIva).Expresion}", iva.Importe);
            }

            foreach (var irpf in irpfs)
            {
                impuestos.Add($"Irpf: {contexto.SeleccionarPorId<IrpfDtm>(irpf.IdIrpf).Expresion}", irpf.Importe);
            }

            return impuestos;
        }


        public static List<ImportePorTipoDeIva> TotalesPorTipoDeIva(this List<FacturaRecDtm> facturas, ContextoSe contexto)
        {
            var totalesIva = new List<ImportePorTipoDeIva>();

            foreach (var factura in facturas)
            {
                var esIntraComunitario = factura.Proveedor(contexto).EsIntraComunitario(contexto);
                var esExtraComunitario = factura.Proveedor(contexto).EsExtraComunitario(contexto);

                var ivasFactura = factura.Ivas(contexto);
                foreach (var iva in ivasFactura)
                {
                    var existente = totalesIva.FirstOrDefault(x => x.IdIva == iva.IdIva);
                    if (existente != null)
                    {
                        existente.BI += iva.BI;
                        existente.Importe += iva.Importe;
                    }
                    else
                    {
                        totalesIva.Add(new ImportePorTipoDeIva
                        {
                            IdIva = iva.IdIva,
                            Tipo = iva.Tipo,
                            BI = iva.BI,
                            Porcentaje = iva.Porcentaje,
                            Importe = iva.Importe,
                            ClaseDeIvaRep = null,
                            ClaseDeIvaSop = iva.ClaseDeIvaSop,
                            EsIntraComunitario = esIntraComunitario,
                            EsExtraComunitario = esExtraComunitario
                        });
                    }
                }
            }

            return totalesIva;
        }

        public static List<ImportePorTipoDeIva> Ivas(this FacturaRecDtm factura, ContextoSe contexto)
        {
            var esIntraComunitario = factura.Proveedor(contexto).EsIntraComunitario(contexto);
            var esExtraComunitario = factura.Proveedor(contexto).EsExtraComunitario(contexto);
            var ivas = new List<ImportePorTipoDeIva>();
            var lineas = factura.Detalles<LineaDeUnaFarDtm>(contexto, aplicarJoin: true);
            foreach (var linea in lineas)
            {
                if (linea.Clase == enumClaseDeLineaFar.BaseImponible || linea.Clase == enumClaseDeLineaFar.LineaDeIrpf)
                    continue;
                ivas.TotalizarIvaSoportado(linea, esIntraComunitario, esExtraComunitario);
            }
            return ivas;
        }

        public static List<ImportePorTipoDeIrpf> TotalesPorTipoDeIrpf(this List<FacturaRecDtm> facturas, ContextoSe contexto)
        {
            var totalesIrpf = new List<ImportePorTipoDeIrpf>();

            foreach (var factura in facturas)
            {
                var irpfsFactura = factura.Irpfs(contexto);
                foreach (var irpf in irpfsFactura)
                {
                    var existente = totalesIrpf.FirstOrDefault(x => x.IdIrpf == irpf.IdIrpf);
                    if (existente != null)
                    {
                        existente.BI += irpf.BI;
                        existente.Importe += irpf.Importe;
                    }
                    else
                    {
                        totalesIrpf.Add(new ImportePorTipoDeIrpf
                        {
                            IdIrpf = irpf.IdIrpf,
                            Tipo = irpf.Tipo,
                            BI = irpf.BI,
                            Porcentaje = irpf.Porcentaje,
                            Importe = irpf.Importe
                        });
                    }
                }
            }

            return totalesIrpf;
        }

        public static List<ImportePorTipoDeIrpf> Irpfs(this FacturaRecDtm factura, ContextoSe contexto)
        {
            var irpfs = new List<ImportePorTipoDeIrpf>();
            var lineas = factura.Detalles<LineaDeUnaFarDtm>(contexto, aplicarJoin: true);
            foreach (var linea in lineas)
            {
                if (linea.Clase != enumClaseDeLineaFar.LineaDeIrpf)
                    continue;

                irpfs.TotalizarIrpf(contexto, linea);
            }
            return irpfs;
        }

        private static void TotalizarIrpf(this List<ImportePorTipoDeIrpf> irpfs, ContextoSe contexto, LineaDeUnaFarDtm linea)
        {
            var irpf = irpfs.FirstOrDefault(i => i.IdIrpf == linea.Irpf.Id);
            if (irpf == null)
            {
                irpf = new ImportePorTipoDeIrpf
                {
                    IdIrpf = linea.IdIrpf.Entero(),
                    Tipo = linea.Irpf.Expresion,
                    Porcentaje = linea.Irpf.Porcentaje
                };
                irpfs.Add(irpf);
            }
            irpf.BI += linea.BaseImponible;
            irpf.Importe += linea.ImporteDeIrpf ?? 0;
        }

        public static decimal ImportesDePagosConfirmados(this FacturaRecDtm factura, ContextoSe contexto)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Far_TotalPagado);
            if (!cache.ContainsKey(factura.Id.ToString()))
            {
                cache[factura.Id.ToString()] = factura.PagosConfirmados(contexto).Sum(x => x.Importe);
            }
            return (decimal)cache[factura.Id.ToString()];
        }

        public static decimal ImportesDevueltosoEnCurso(this FacturaRecDtm factura, ContextoSe contexto)
        {
            return factura.ImportesDevueltosConfirmados(contexto) + factura.DevolucionesEnCurso(contexto).Sum(d => d.Importe);
        }

        public static decimal ImportesDevueltosConfirmados(this FacturaRecDtm factura, ContextoSe contexto)
        {
            var quienMeRectifica = factura.QuienMeRectifica(contexto);
            if (quienMeRectifica == null) return 0;

            return quienMeRectifica.ImportesDePagosConfirmados(contexto);
        }

        public static List<PagoDtm> PagosConfirmados(this FacturaRecDtm factura, ContextoSe contexto)
        {
            var estadosTerminados = enumNegocio.Pago.Estados(contexto).Where(e => e.Terminado).Select(e => e.Id).ToList();
            return factura.PagosRealizados(contexto).Where(x => estadosTerminados.Contains(x.IdEstado)).ToList();
        }

        public static List<PagoDtm> PagosRealizados(this FacturaRecDtm factura, ContextoSe contexto)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Far_PagosNoCancelados);
            if (!cache.ContainsKey(factura.Id.ToString()))
            {
                var estadosCancelados = enumNegocio.Pago.Estados(contexto).Where(e => e.Cancelado).Select(e => e.Id).ToList();
                cache[factura.Id.ToString()] = contexto.Set<PagoDtm>().Where(pago => pago.IdFacturaRec == factura.Id && !estadosCancelados.Contains(pago.IdEstado)).ToList();
            }
            return (List<PagoDtm>)cache[factura.Id.ToString()];
        }

        public static List<PagoDtm> PagosEnCurso(this FacturaRecDtm factura, ContextoSe contexto)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Far_PagosEnCurso);
            if (!cache.ContainsKey(factura.Id.ToString()))
            {
                var enCurso = enumNegocio.Pago.Estados(contexto).Where(e => !e.Cancelado && !e.Terminado).Select(e => e.Id).ToList();
                cache[factura.Id.ToString()] = contexto.Set<PagoDtm>().Where(pago => pago.IdFacturaRec == factura.Id && enCurso.Contains(pago.IdEstado)).ToList();
            }
            return (List<PagoDtm>)cache[factura.Id.ToString()];
        }

        public static List<PagoDtm> DevolucionesEnCurso(this FacturaRecDtm factura, ContextoSe contexto)
        {
            var quienMeRectifica = factura.QuienMeRectifica(contexto);
            if (quienMeRectifica == null) return new List<PagoDtm>();

            return quienMeRectifica.PagosEnCurso(contexto);
        }

        public static List<PagoDtm> PagosContadosEnCurso(this FacturaRecDtm factura, ContextoSe contexto)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Far_PagosContadosEnCurso);
            if (!cache.ContainsKey(factura.Id.ToString()))
            {
                var enCurso = enumNegocio.Pago.Estados(contexto).Where(e => !e.Cancelado && !e.Terminado).Select(e => e.Id).ToList();
                cache[factura.Id.ToString()] = contexto.Set<PagoDtm>().Where(pago => pago.IdFacturaRec == factura.Id
                                && enCurso.Contains(pago.IdEstado)
                                && pago.Clase.Equals(enumClaseDePago.Contado)
                                && pago.IdTarjetaDePago == null).ToList();
            }
            return (List<PagoDtm>)cache[factura.Id.ToString()];
        }

        public static List<PagoDtm> DevolucionesContadosEnCurso(this FacturaRecDtm factura, ContextoSe contexto)
        {
            var quienMeRectifica = factura.QuienMeRectifica(contexto);
            if (quienMeRectifica == null) return new List<PagoDtm>();

            return quienMeRectifica.PagosContadosEnCurso(contexto);
        }

        public static bool EstaPagada(this FacturaRecDtm factura, ContextoSe contexto)
        {
            decimal importePorPagar = factura.TotalDelPago;
            decimal importeYaPagado = factura.ImportesDePagosConfirmados(contexto);
            decimal importePdtDePago = importePorPagar - importeYaPagado;

            decimal porDevolver = 0;
            decimal yaDevuelto = 0;
            decimal pdtDeQueMeDevuelban = 0;


            decimal porPagar = 0;
            decimal yaPagado = 0;
            decimal pdtDePago = 0;


            if (factura.EsRectificativa)
            {
                var aQuienRectifico = factura.Rectificada(contexto, errorSiNoHay: false);
                porPagar = aQuienRectifico?.TotalDelPago ?? 0;
                yaPagado = aQuienRectifico?.ImportesDePagosConfirmados(contexto) ?? 0;
                pdtDePago = porPagar - yaPagado;
                pdtDeQueMeDevuelban = Math.Abs(importePdtDePago);
                return Math.Abs(pdtDePago - pdtDeQueMeDevuelban) <= VariableDeFacturasRec.ToleranciaEnImportes();
            }

            var quienMeRectifica = factura.QuienMeRectifica(contexto);
            if (quienMeRectifica != null)
            {
                porDevolver = Math.Abs(quienMeRectifica.TotalDelPago);
                yaDevuelto = Math.Abs(quienMeRectifica.ImportesDePagosConfirmados(contexto));
                pdtDeQueMeDevuelban = porDevolver - yaDevuelto;
            }

            return Math.Abs(importePdtDePago - pdtDeQueMeDevuelban) <= VariableDeFacturasRec.ToleranciaEnImportes();
        }


        public static List<BiDelIvaPorNaturaleza> BiDelIvaPorNaturaleza(this FacturaRecDtm factura, ContextoSe contexto)
        {
            var naturalezas = new List<BiDelIvaPorNaturaleza>();
            var lineas = factura.Detalles<LineaDeUnaFarDtm>(contexto, aplicarJoin: true);
            foreach (var linea in lineas.Where(l => l.Clase != enumClaseDeLineaFar.LineaDeIva))
            {
                if (linea.Clase == enumClaseDeLineaFar.BaseImponible)
                {
                    naturalezas.TotalizarNaturalezaPorBi(contexto, linea);
                }
                if (linea.Clase == enumClaseDeLineaFar.BiConIva || linea.Clase == enumClaseDeLineaFar.BiExenta)
                {
                    naturalezas.TotalizarNaturalezasIva(contexto, linea);
                }
            }

            if (naturalezas.Count == 0) Emitir($"No se han indicado la naturaleza en la factura '{factura.Referencia}'");
            var lineasDeIva = lineas.Where(l => l.Clase == enumClaseDeLineaFar.LineaDeIva);

            if (lineasDeIva.Count() > 0)
            {
                if (naturalezas.Count == 1 && lineasDeIva.Sum(x => x.BaseImponible) == naturalezas.First().Bi)
                {
                    var i = 0;
                    foreach (var lineaDeIva in lineasDeIva)
                    {
                        if (i > 0)
                        {
                            naturalezas.Add(new BiDelIvaPorNaturaleza
                            {
                                IdNaturaleza = naturalezas[0].IdNaturaleza,
                                Concepto = naturalezas[0].Concepto,
                                Bi = 0,
                                idIva = 0,
                                ImporteDeIva = 0
                            });
                        }
                        naturalezas[i].idIva = lineaDeIva.IdIvaS.Entero();
                        naturalezas[i].ImporteDeIva = lineaDeIva.ImporteDeIva.Decimal();
                        naturalezas[i].Bi = lineaDeIva.BaseImponible;
                        i++;
                    }
                    return naturalezas;
                }

                var naturalezasPendientes = naturalezas.Where(n => n.idIva == 0);
                if (naturalezasPendientes.Count() == 0)
                    Emitir($"No hay BI pendientes de asociar Iva y en la factura hay líneas de Iva pendientes de repartir al generar el asiento contable");

                if (lineasDeIva.Count() == 1 && naturalezasPendientes.Sum(x => x.Bi) == lineasDeIva.First().BaseImponible)
                {
                    foreach (var naturaleza in naturalezasPendientes)
                    {
                        var lineaDeIva = lineasDeIva.First();
                        naturaleza.idIva = lineaDeIva.IdIvaS.Entero();
                        naturaleza.ImporteDeIva = naturaleza.Bi * contexto.SeleccionarPorId<IvaSoportadoDtm>(lineaDeIva.IdIvaS.Entero()).Porcentaje / 100;
                    }
                }
                else
                {
                    var asociada = false;
                    foreach (var lineaDeIva in lineasDeIva)
                    {
                        foreach (var naturaleza in naturalezas.Where(n => n.idIva == 0))
                        {
                            if (naturaleza.Bi == lineaDeIva.BaseImponible)
                            {
                                naturaleza.idIva = lineaDeIva.IdIvaS.Entero();
                                naturaleza.ImporteDeIva = lineaDeIva.ImporteDeIva.Decimal();
                                asociada = true;
                                break;
                            }
                        }
                    }

                    if (!asociada)
                    {
                        Emitir($"No puedo asociar las líneas de Iva de la factura '{factura.Referencia}' con las líneas de BI de iva por naturaleza. Es necesario para el libro de Iva, reescriba el detalle de la factura");
                    }
                }
            }

            return naturalezas;
        }

        private static void TotalizarNaturalezasIva(this List<BiDelIvaPorNaturaleza> naturalezas, ContextoSe contexto, LineaDeUnaFarDtm linea)
        {
            if (linea.IdNaturaleza.Entero() == 0)
                return;

            var naturalezaDtm = contexto.SeleccionarPorId<NaturalezaDtm>(linea.IdNaturaleza.Entero(), aplicarJoin: true);
            var naturaleza = naturalezas.FirstOrDefault(i => i.IdNaturaleza == linea.IdNaturaleza.Entero() && i.idIva == linea.IdIvaS.Entero());
            if (naturaleza == null)
            {
                naturaleza = new BiDelIvaPorNaturaleza
                {
                    IdNaturaleza = linea.IdNaturaleza.Entero(),
                    idIva = linea.IdIvaS.Entero()
                };
                naturalezas.Add(naturaleza);
            }
            naturaleza.Bi += linea.BaseImponible;
            naturaleza.ImporteDeIva += linea.ImporteDeIva.Decimal();
            naturaleza.Concepto = naturaleza.Concepto.IsNullOrEmpty()
            ? linea.Concepto
            : naturaleza.Concepto == linea.Concepto
            ? naturaleza.Concepto
            : naturalezaDtm.Expresion;
        }

        private static void TotalizarNaturalezaPorBi(this List<BiDelIvaPorNaturaleza> naturalezas, ContextoSe contexto, LineaDeUnaFarDtm linea)
        {
            if (linea.IdNaturaleza.Entero() == 0)
                return;

            var naturalezaDtm = contexto.SeleccionarPorId<NaturalezaDtm>(linea.IdNaturaleza.Entero(), aplicarJoin: true);
            var naturaleza = naturalezas.FirstOrDefault(i => i.IdNaturaleza == linea.IdNaturaleza.Entero());
            naturaleza = new BiDelIvaPorNaturaleza
            {
                IdNaturaleza = linea.IdNaturaleza.Entero(),
                idIva = 0,
                Bi = linea.BaseImponible,
                ImporteDeIva = 0,
                Concepto = naturalezaDtm.Expresion
            };
            naturalezas.Add(naturaleza);
        }

        public static void Descuedre(this FacturaRecDtm factura, ContextoSe contexto, StringBuilder errores)
        {
            decimal totalBi = 0;
            decimal totalIva = 0;
            decimal totalIrpf = 0;
            decimal totalBIDeLineasDe_BiConIva = 0;
            decimal totalBIDeLineasDe_LineaIva = 0;
            decimal totalBIDeLineasDe_LineaSinIva = 0;
            bool tieneLineaDeBi = false;
            bool tieneLineaExenta = false;
            bool tieneLineaDeBiConIva = false;
            bool tieneLineaDeIva = false;


            var lineas = factura.Detalles<LineaDeUnaFarDtm>(contexto);
            foreach (var linea in lineas)
            {
                if (linea.Clase == enumClaseDeLineaFar.BaseImponible || linea.Clase == enumClaseDeLineaFar.BiExenta)
                {
                    if (linea.Clase == enumClaseDeLineaFar.BaseImponible) tieneLineaDeBi = true;
                    if (linea.Clase == enumClaseDeLineaFar.BiExenta) tieneLineaExenta = true;
                    totalBi = totalBi + linea.BaseImponible;
                    totalBIDeLineasDe_LineaSinIva = totalBIDeLineasDe_LineaSinIva + linea.BaseImponible;
                }
                else if (linea.Clase == enumClaseDeLineaFar.BiConIva)
                {
                    if (linea.Clase == enumClaseDeLineaFar.BiConIva) tieneLineaDeBiConIva = true;
                    totalBi = totalBi + linea.BaseImponible;
                    totalBIDeLineasDe_BiConIva = totalBIDeLineasDe_BiConIva + linea.BaseImponible;
                    totalIva = totalIva + (linea.BaseImponible * (linea.PorcentajeIva is null ? 0 : (decimal)linea.PorcentajeIva / 100));
                }
                else if (linea.Clase == enumClaseDeLineaFar.LineaDeIva)
                {
                    if (linea.Clase == enumClaseDeLineaFar.LineaDeIva) tieneLineaDeIva = true;
                    totalBIDeLineasDe_LineaIva = totalBIDeLineasDe_LineaIva + linea.BaseImponible;
                    totalIva = totalIva + (linea.BaseImponible * (linea.PorcentajeIva is null ? 0 : (decimal)linea.PorcentajeIva / 100));
                }
                else if (linea.Clase == enumClaseDeLineaFar.LineaDeIrpf)
                {
                    totalIrpf = totalIrpf + (linea.BaseImponible * (linea.PorcentajeIrpf is null ? 0 : (decimal)linea.PorcentajeIrpf / 100));
                }
            }

            if (tieneLineaDeBi && tieneLineaDeBiConIva)
                errores.AppendLine($"{factura.Referencia}: tiene línea de BI y líneas de BI con IVA");

            if ((tieneLineaDeBi || tieneLineaExenta) && tieneLineaDeBiConIva && totalBi != totalBIDeLineasDe_BiConIva + totalBIDeLineasDe_LineaSinIva)
                errores.AppendLine($"{factura.Referencia}: tiene línea exenta o sin IVA y líneas de BI con IVA y difieren el total");


            if (tieneLineaDeIva && tieneLineaDeBiConIva)
                errores.AppendLine($"{factura.Referencia}: tiene línea de IVA y líneas de BI con IVA");

            if (tieneLineaDeBi && tieneLineaDeIva && totalBIDeLineasDe_LineaSinIva != totalBIDeLineasDe_LineaIva)
                errores.AppendLine($"{factura.Referencia}: tiene línea de BI y líneas de IVA con BI diferentes");
        }

        public static decimal Total(this FacturaRecDtm factura, ContextoSe contexto, enumImporteFar importeSolicitado)
        {
            var cacheBi = ServicioDeCaches.Obtener(CacheDe.Far_Base);
            var cacheIva = ServicioDeCaches.Obtener(CacheDe.Far_Iva);
            var cacheIrpf = ServicioDeCaches.Obtener(CacheDe.Far_Irpf);

            if (!cacheBi.ContainsKey(factura.Id.ToString()))
            {
                decimal totalBi = 0;
                decimal totalIva = 0;
                decimal totalIrpf = 0;


                var totales = contexto.Set<LineaDeUnaFarDtm>()
                   .Where(l => l.IdElemento == factura.Id)
                   .GroupBy(l => l.IdElemento)
                   .Select(g => new
                   {
                       // Sumamos BaseImponible solo si la clase es BaseImponible, BiExenta o BiConIva
                       TotalBi = g.Where(l => l.Clase == enumClaseDeLineaFar.BaseImponible ||
                                             l.Clase == enumClaseDeLineaFar.BiExenta ||
                                             l.Clase == enumClaseDeLineaFar.BiConIva)
                                  .Sum(l => (decimal?)l.BaseImponible) ?? 0,

                       // Sumamos IVA calculando el porcentaje solo si no es ISP y la clase es BiConIva o LineaDeIva
                       TotalIva = g.Where(l => (l.Clase == enumClaseDeLineaFar.BiConIva || l.Clase == enumClaseDeLineaFar.LineaDeIva) &&
                                              l.IvaSoportado.Clase != enumClasesDeIvaSop.ISP)
                                   .Sum(l => (decimal?)(l.BaseImponible * (l.PorcentajeIva ?? 0) / 100)) ?? 0,

                       // Sumamos IRPF calculando el porcentaje solo para líneas de IRPF
                       TotalIrpf = g.Where(l => l.Clase == enumClaseDeLineaFar.LineaDeIrpf)
                                    .Sum(l => (decimal?)(l.BaseImponible * (l.PorcentajeIrpf ?? 0) / 100)) ?? 0
                   })
                   .FirstOrDefault();

                if (totales != null)
                {
                    totalBi = totales.TotalBi;
                    totalIva = totales.TotalIva;
                    totalIrpf = totales.TotalIrpf;
                }
                cacheIva[$"{factura.Id}"] = totalIva;
                cacheIrpf[$"{factura.Id}"] = totalIrpf;
                cacheBi[$"{factura.Id}"] = totalBi;
            }

            switch (importeSolicitado)
            {
                case enumImporteFar.BaseImponible: return (decimal)cacheBi[factura.Id.ToString()];
                case enumImporteFar.TotalPagar: return (decimal)cacheBi[factura.Id.ToString()] + (decimal)cacheIva[factura.Id.ToString()] - (decimal)cacheIrpf[factura.Id.ToString()];
                case enumImporteFar.TotalFactura: return (decimal)cacheBi[factura.Id.ToString()] + (decimal)cacheIva[factura.Id.ToString()];
                case enumImporteFar.TotalIrpf: return (decimal)cacheIrpf[factura.Id.ToString()];
                case enumImporteFar.TotalIva: return (decimal)cacheIva[factura.Id.ToString()];
            }

            throw new Exception($"Se ha solicitado un importe no implemetado '{importeSolicitado.Descripcion()}'");
        }

        public static void ValidarFacturaBienFormada(this FacturaRecDtm factura, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            var mensaje = factura.DimeSiEstaBien(contexto);
            if (!mensaje.IsNullOrEmpty())
                Emitir(mensaje);
        }

        public static string DimeSiEstaBien(this FacturaRecDtm factura, ContextoSe contexto)
        {
            ApiDeTerceros.ValidarCif(factura.Proveedor(contexto).Interlocutor(contexto).Sociedad(contexto));

            var lineas = factura.Detalles<LineaDeUnaFarDtm>(contexto, errorSiNoHay: false);
            if (lineas.Count == 0) return $"La factura '{factura.Referencia}' ha de tener el detalle de lo facturado";

            if (factura.EsRectificativa && factura.IdRectificada is null && !VariableDeFacturasRec.PermitirFacturasNegativa())
                return $"La factura '{factura.Referencia}' es negativa y no indica a quién rectifica, cancélela y creela usando la opción de 'rectificar factura' o habilite el parámetro '{enumParametrosDeFacturasRec.FAR_PermitirFacturasNegativas}'";

            factura.BiDelIvaPorNaturaleza(contexto);

            if (factura.HayDiferenciaEntreLasBiConLasBiDelIva(contexto))
                return $"La suma de las Bi '{factura.Total(contexto, enumImporteFar.BaseImponible).ToMoneda()}' de la factura '{factura.Referencia}' no coincide con la declarada en el Iva '{factura.Ivas(contexto).Sum(x => x.BI).ToMoneda()}'";

            if (factura.HayDiferenciaConLaBi(contexto))
                return $"La factura '{factura.Referencia}' no tiene la misma BI indicada en la cabecera '{factura.BaseImponible.ToMoneda()}' que en el detalle '{factura.Total(contexto, enumImporteFar.BaseImponible).ToMoneda()}'";

            if (!PuedoAsociarElIvaACadaBi(lineas))
                return $"La factura '{factura.Referencia}' tiene varias líneas de BI y varias líneas de IVA y el sistema no es capaz de relacionarlas, use líneas de clase '{enumClaseDeLineaFar.LineaDeIva}' para introducirlas";

            if (factura.HayDiferenciaConElTotalPagar(contexto))
                return $"La factura '{factura.Referencia}' indica un total a pagar '{factura.TotalDelPago.ToMoneda()}' que no coincide con el indicado en sus líneas '{factura.Total(contexto, enumImporteFar.TotalPagar).ToMoneda()}'";

            if (factura.IdArchivo is null)
                return $"La factura '{factura.Referencia}' no se le ha asociado el archivo de la factura";

            if (factura.FacturadaEl == DateTime.MinValue)
                return $"La fecha de emisión '{factura.FacturadaEl.ToString("dd/MM/yyyy")}' de la factura '{factura.Referencia}' no es válida";

            if (factura.FacturadaEl.Year < DateTime.Now.Year - 1)
                return $"La factura '{factura.Referencia}' se ha facturado con año '{factura.FacturadaEl.Year}', no se puede facturar con fecha anterior al año '{DateTime.Now.Year - 1}', devuélvala y que la corrija";

            return "";
        }

        private static bool PuedoAsociarElIvaACadaBi(List<LineaDeUnaFarDtm> lineas)
        {
            var lineasDeIva = lineas.Where(x => x.Clase == enumClaseDeLineaFar.LineaDeIva).ToList();
            if (!lineasDeIva.Any())
                return true;

            var lineasDeBi = lineas.Where(x => x.Clase == enumClaseDeLineaFar.BaseImponible).ToList();

            if (lineasDeBi.Count == 1)
                return true;

            if (lineasDeIva.Count == lineasDeBi.Count)
            {
                foreach (var iva in lineasDeIva)
                {
                    var biEncontrada = false;
                    foreach (var bi in lineasDeBi)
                    {
                        if (bi.BaseImponible == iva.BaseImponible)
                            biEncontrada = true;
                    }
                    if (!biEncontrada)
                        return false;
                }
                return true;
            }

            return false;
        }

        private static bool HayDiferenciaEntreLasBiConLasBiDelIva(this FacturaRecDtm factura, ContextoSe contexto)
        {
            var sumaDeLasBi = factura.Total(contexto, enumImporteFar.BaseImponible);
            var ivas = factura.Ivas(contexto);
            var sumaDeLasBiDelIva = ivas.Sum(x => x.BI);
            return sumaDeLasBi - sumaDeLasBiDelIva != 0;
        }

        private static bool HayDiferenciaConLaBi(this FacturaRecDtm factura, ContextoSe contexto)
        {
            var totalBi = factura.Total(contexto, enumImporteFar.BaseImponible);
            return factura.HayDiferenciaEntreLasBi(totalBi);
        }
        public static bool HayDiferenciaEntreLasBi(this FacturaRecDtm factura, decimal biAnteriror)
        {
            return Math.Abs(factura.BaseImponible - biAnteriror) > VariableDeFacturasRec.ToleranciaEnImportes();
        }
        private static bool HayDiferenciaConElTotalPagar(this FacturaRecDtm factura, ContextoSe contexto)
        {
            var totalPagar = factura.Total(contexto, enumImporteFar.TotalPagar);
            return factura.HayDiferenciaEntreLosTotalPagar(totalPagar);
        }
        public static bool HayDiferenciaEntreLosTotalPagar(this FacturaRecDtm factura, decimal totalAnterior)
        {
            return Math.Abs(factura.TotalDelPago - totalAnterior) > VariableDeFacturasRec.ToleranciaEnImportes();
        }

        public static void AjustarSaldosDelContrato(this FacturaRecDtm factura, ContextoSe contexto, FacturaRecDtm anterior)
        {
            var contrato = factura.Contrato(contexto);
            var avance = contrato.Ampliacion<AvanceDtm>(contexto);
            avance.Facturado = avance.Facturado - (anterior is null ? 0 : anterior.BaseImponible) + factura.BaseImponible;
        }

        public static void ModificarSaldosDelContrato(this FacturaRecDtm factura, ContextoSe contexto, FacturaRecDtm anterior)
        {
            var contrato = factura.IdContrato is null ? anterior.Contrato(contexto) : factura.Contrato(contexto);
            var avance = contrato.Ampliacion<AvanceDtm>(contexto);
            if (factura.Contrato is null)
            {
                if (anterior.EstaEnLaEtapa(enumEtapasDeFacturasRec.FAR_Etapa_Pagada))
                    avance.Cobrado = avance.Cobrado - anterior.BaseImponible;
                else
                    avance.Facturado = avance.Facturado - anterior.BaseImponible;
                factura.CrearTraza(contexto, "factura excluida del contrato", $"El usuario {contexto.DatosDeConexion.Login} ha excluido la factura '{factura.Referencia}' del contrato {contrato.Referencia}");
            }
            else
            {
                if (factura.EstaEnLaEtapa(enumEtapasDeFacturasRec.FAR_Etapa_Pagada))
                    avance.Cobrado = avance.Cobrado + anterior.BaseImponible;
                else
                    avance.Facturado = avance.Facturado + anterior.BaseImponible;
            }
            avance.Modificar(contexto);
        }

        public static void ModificarEventoDeVencimiento(this FacturaRecDtm factura, ContextoSe contexto)
        {
            var sociedad = factura.Sociedad(contexto);
            if (sociedad.IdAgenda is not null)
                factura.EliminarEventoDeVencimiento(contexto);
            factura.AnotarEventoDeVencimiento(contexto);
        }

        private static void AnotarEventoDeVencimiento(this FacturaRecDtm factura, ContextoSe contexto)
        {
            var sociedad = factura.Sociedad(contexto);
            var agenda = sociedad.IdAgenda == null ? sociedad.CrearAgenda(contexto) : contexto.SeleccionarPorId<AgendaDtm>((int)sociedad.IdAgenda);

            var evento = new EventoDeAgendaDtm();
            evento.IdAgenda = agenda.Id;
            evento.IdElemento = factura.Id;
            evento.IdNegocio = enumNegocio.FacturaRecibida.IdNegocio();
            evento.EsDelSistema = true;
            evento.Inicio = ((DateTime)factura.VenceEl).Date;
            evento.Nombre = ltrDeUnaFacturaRec.EventoDeVencimiento.Replace($"[{nameof(FacturaRecDtm.Numero)}]", factura.Numero);
            evento.Descripcion = $"Hoy vence la factura {factura.Numero}";
            evento.Fin = evento.Inicio;
            GestorDeVinculos.Vincular(contexto, factura, evento, new Dictionary<string, object> { { ltrParametrosNeg.EstaEjecutandoUnaAccion, true } });
        }

        private static void EliminarEventoDeVencimiento(this FacturaRecDtm factura, ContextoSe contexto)
        {
            var nombreEvento = ltrDeUnaFacturaRec.EventoDeVencimiento.Replace($"[{nameof(FacturaRecDtm.Numero)}]", factura.Numero);
            var eventos = contexto.SeleccionarEventos(factura.Id, nombreEvento);
            var p = new Dictionary<string, object> { { ltrParametrosNeg.EstaEjecutandoUnaAccion, true } };
            foreach (var e in eventos.Where(e => e.EsDelSistema)) factura.Desvincular(contexto, e, p);
        }

        public static DireccionDto DireccionFiscal(this FacturaRecDtm factura, ContextoSe contexto, bool errorSiNoHay = true)
        {
            var direcciones = GestorDeDirecciones.LeerRegistros(contexto, enumNegocio.FacturaEmitida, factura.Id).ToList();
            var fiscal = direcciones.FirstOrDefault(x => x.Calificador == enumCalificadorDireccion.fiscal && x.Activo);
            return fiscal == null
            ? factura.Proveedor(contexto).DireccionFiscal(contexto, errorSiNoHay: errorSiNoHay)
            : fiscal.MapearDto(contexto, enumNegocio.FacturaEmitida);
        }

        public static void ValidarNumeroDeFactura(this FacturaRecDtm far, ContextoSe contexto)
        {
            var facturas = contexto.SeleccionarTodos<FacturaRecDtm>(filtros: new Dictionary<string, object> {
                      {ltrDeUnaFacturaRec.FiltroPorNumerosDeFactura, far.Numero },
                      {ltrDeUnaFacturaRec.FiltroPorProveedor, far.IdProveedor },
                      {ltrDeUnaFacturaRec.FiltroPorEjercicioDeFactura, far.FacturadaEl.Year }
                    },
                parametros: new Dictionary<string, object> {
                        //{ ltrParametrosNeg.NombreDelFicheroParaTrazarLaConsulta, nameof(ValidarNumeroDeFactura) },
                        { ltrParametrosNeg.ValidarPermisosDeConsulta, false },
                        { ltrParametrosNeg.ExcluirCancelados, true }
                });
            if (facturas.Count > 0)
            {
                var diferente = facturas.FirstOrDefault(f => f.Id != far.Id);
                if (diferente != null)
                    Emitir($"ya existe la factura '{diferente.Referencia}' del proveedor '{far.Proveedor(contexto).Referencia(contexto)}' con el mismo número '{diferente.Numero}' para el mismo ejercicio '{diferente.FacturadaEl.Year}'");
            }
        }

        public static void AntesDeEnviarAContabilidad(this FacturaRecDtm factura, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            if (factura.IdRectificada is not null)
            {
                factura.ClaseRectificativa = factura.Rectificada(contexto).BaseImponible != factura.BaseImponible
                ? enumClaseDeRectificativa.OC
                : enumClaseDeRectificativa.OR;
            }


            var lineas = factura.Detalles<LineaDeUnaFarDtm>(contexto, aplicarJoin: true);
            foreach (var linea in lineas)
            {
                if (linea.Clase == enumClaseDeLineaFar.LineaDeIva || linea.Clase == enumClaseDeLineaFar.LineaDeIrpf)
                    continue;

                if (linea.IdNaturaleza is null)
                {
                    Emitir($"La línea '{linea.Orden}' de la factura '{factura.Referencia}' con BI '{linea.BaseImponible.ToMoneda()}' debe tener indicada la naturaleza");
                }
            }

            if (factura.Proveedor(contexto).Baja)
            {
                factura.Proveedor(contexto).IndicarQueEstaDeBaja(contexto);
            }
        }

        public static void QuitarPreasiento(this FacturaRecDtm factura, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            factura.AnularPreasiento(contexto, parametros);
        }

        public static void AntesDeDevolverAprobar(this FacturaRecDtm factura, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            factura.ContabilizadaEl = null;
            factura.ClaseRectificativa = null;
            var pagos = factura.PagosConfirmados(contexto);
            if (pagos.Count > 0 && pagos.Any(pago => pago.FechaCreacion >= factura.FechaCreacion))
                Emitir($"No se puede devolver la factura ya que tiene algún pago asociado: '{string.Join(",", pagos.Select(objeto => objeto.Referencia))}', cancele primero dichos pagos");
        }

        public static void AntesDeEnviarAPagar(this FacturaRecDtm factura, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            if (factura.ContabilizadaEl is null)
            {
                var automatizar = enumNegocio.FacturaRecibida.LeerCrearParametro(contexto, enumParametrosDeFacturasRec.FAR_Al_Contabilizar_Indicar_Fecha, "S").Valor.EsTrue();
                if (!automatizar)
                    Emitir($"No se puede enviar al pago la factura '{factura.Referencia}' ya que no ha indicado la fecha contable");
                factura.ContabilizadaEl = DateTime.Now;
            }

            factura = factura.Preasentar(contexto);
            parametros[ltrParametrosNeg.AccionQueSeEjecuta] = ltrDeUnaFacturaRec.Accion_EnviarAPagar;
        }
        public static void AntesDeDarDevolverAContabilidad(this FacturaRecDtm factura, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            if (factura.IdPreasiento is not null)
            {
                factura.CancelarPreasiento(contexto);
            }
        }

        public static void AntesDeDarPorPagada(this FacturaRecDtm factura, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            factura = factura.Preasentar(contexto);
            factura.AntesDeArchivarFactura(contexto, parametros);
        }

        public static void AntesDeArchivarFactura(this FacturaRecDtm factura, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            if (factura.EstaPagada(contexto))
            {
                return;
            }

            if (factura.PagosEnCurso(contexto).Count > 0)
                Emitir($"La factura '{factura.Referencia}' tiene pagos en curso, de los por pagados o cancelelos antes de ejecutar esta transición");

            if (factura.ContabilizadaEl is null)
            {
                var automatizar = enumNegocio.FacturaRecibida.LeerCrearParametro(contexto, enumParametrosDeFacturasRec.FAR_Al_Contabilizar_Indicar_Fecha, "S").Valor.EsTrue();
                if (!automatizar)
                    Emitir($"No se puede enviar al pago la factura '{factura.Referencia}' ya que no ha indicado la fecha contable");
                factura.ContabilizadaEl = DateTime.Now;
            }

            var pago = factura.DarPorPagada(contexto);
            if (!pago.EstaEnLaEtapa(enumEtapasDePagos.PAG_Etapa_Pagado))
                pago.TransitarALaEtapa(contexto, enumEtapasDePagos.PAG_Etapa_Pagado.EstadosDeLaEtapa(), new Dictionary<string, object> {
                        { ltrParametrosNeg.AccionQueSeEjecuta, ltrDeUnaFacturaRec.Accion_DarPorPagada },
                        { ltrParametrosEp.asunto,"Dar por pagada" },
                        { ltrParametrosEp.detalleAsunto, $"Se da por pagada la factura '{factura.Referencia}'" }
                    }, delSistema: true);
        }

        public static ContratoDtm Contrato(this FacturaRecDtm factura, ContextoSe contexto, bool aplicarJoin = false, bool errorSiNoHay = true)
        {
            if (factura.Contrato != null) return factura.Contrato;

            if (factura.IdContrato is null && errorSiNoHay)
                Emitir($"La factura '{factura.Referencia}' no tiene contrato");

            if (factura.IdContrato is null) return null;

            return factura.Contrato = contexto.SeleccionarPorId<ContratoDtm>((int)factura.IdContrato, aplicarJoin);
        }

        public static ExpedienteDtm Expediente(this FacturaRecDtm factura, ContextoSe contexto, bool aplicarJoin = false, bool errorSiNoHay = true)
        {
            if (factura.Expediente != null) return factura.Expediente;

            if (factura.IdExpediente is null && errorSiNoHay)
                Emitir($"La factura '{factura.Referencia}' no tiene expediente asociado");

            if (factura.IdExpediente is null) return null;

            return factura.Expediente = contexto.SeleccionarPorId<ExpedienteDtm>((int)factura.IdExpediente, aplicarJoin);
        }

        public static void CargarCacheQuienMeRectificaMasivamente(ContextoSe contexto, List<int> idsFacturas)
        {
            var cacheRectificativas = ServicioDeCaches.Obtener(CacheDe.Far_QuienMeRectifica);

            // 1. Filtramos: solo procesamos IDs que NO están en la caché.
            // Si ya está (aunque sea como null), significa que ya sabemos el resultado.
            var idsNuevos = idsFacturas
                .Where(id => !cacheRectificativas.ContainsKey(id.ToString()))
                .Distinct()
                .ToList();

            // 2. Si no hay nada nuevo que buscar, salimos
            if (!idsNuevos.Any())
                return;

            // 3. Buscamos facturas que rectifiquen a las nuestras y no estén canceladas
            var rectificadoras = contexto.Set<FacturaRecDtm>()
                .Include(f => f.Estado)
                .Where(f => f.IdRectificada != null &&
                            idsNuevos.Contains(f.IdRectificada.Value) &&
                            f.Estado.Cancelado == false)
                .ToList();

            // 4. Llenamos la caché con los resultados encontrados
            foreach (var facturaRectificadora in rectificadoras)
            {
                string key = facturaRectificadora.IdRectificada.ToString();
                // Guardamos la relación: ID_Original -> Factura_Que_Rectifica
                cacheRectificativas[key] = facturaRectificadora;
            }

            // 5. MARCADO CRÍTICO: Para los IDs que buscamos y NO tienen rectificativa, 
            // guardamos un null explícito. 
            // Así, el método individual QuienMeRectifica() sabrá que ya se miró y no hay nada.
            foreach (var id in idsNuevos)
            {
                string key = id.ToString();
                if (!cacheRectificativas.ContainsKey(key))
                {
                    cacheRectificativas[key] = null;
                }
            }
        }

        public static FacturaRecDtm Rectificada(this FacturaRecDtm factura, ContextoSe contexto, bool aplicarJoin = false, bool errorSiNoHay = true)
        {
            if (factura.Rectificada != null) return factura.Rectificada;

            if (factura.IdRectificada is null && errorSiNoHay)
                Emitir($"La factura '{factura.Referencia}' no rectifica a ninguna otra factura");

            if (factura.IdRectificada is null) return null;

            return factura.Rectificada = contexto.SeleccionarPorId<FacturaRecDtm>((int)factura.IdRectificada, aplicarJoin);
        }

        public static FacturaRecDtm QuienMeRectifica(this FacturaRecDtm factura, ContextoSe contexto, bool aplicarJoin = false)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Far_QuienMeRectifica);
            if (!cache.ContainsKey(factura.Id.ToString()))
            {
                var quieMeRectifica = contexto.SeleccionarActivosPorPropiedad<FacturaRecDtm>(nameof(FacturaRecDtm.IdRectificada), factura.Id, aplicarJoin: true, errorSiNoHay: false, errorSiMasDeuno: true);
                if (quieMeRectifica is null)
                {
                    return null;
                }
                cache[factura.Id.ToString()] = quieMeRectifica;
            }
            return (FacturaRecDtm)cache[factura.Id.ToString()];

        }


        public static void AntesDeCancelar(this FacturaRecDtm factura, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            ValidarAntesDeDevolverOCancelar(factura, contexto);
        }

        public static void AntesDeDevolverAlProveedor(this FacturaRecDtm factura, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            ValidarAntesDeDevolverOCancelar(factura, contexto);
        }

        private static void ValidarAntesDeDevolverOCancelar(FacturaRecDtm factura, ContextoSe contexto)
        {
            if (factura.IdContrato is not null && !enumNegocio.FacturaRecibida.Parametro(enumParametrosDeFacturasRec.FAR_Quitar_Contrato_Al_Anular, crearParametro: true, valorPorDefecto: "S").Valor.EsTrue())
                Emitir($"La factura '{factura.Referencia}' está imputada al contrato '{factura.Contrato(contexto).Referencia}', desasociela primero");
            else
                factura.IdContrato = null;


            if (factura.IdExpediente is not null && !enumNegocio.FacturaRecibida.Parametro(enumParametrosDeFacturasRec.FAR_Quitar_Expediente_Al_Anular, crearParametro: true, valorPorDefecto: "S").Valor.EsTrue())
                Emitir($"La factura '{factura.Referencia}' está imputada al expediente '{factura.Expediente(contexto).Referencia}', desasociela primero");
            else
                factura.IdExpediente = null;

            var pagos = factura.PagosRealizados(contexto);
            if (pagos.Count > 0 && !enumNegocio.FacturaRecibida.Parametro(enumParametrosDeFacturasRec.FAR_Quitar_Pagos_Al_Anular, crearParametro: true, valorPorDefecto: "S").Valor.EsTrue())
            {
                Emitir($"La factura '{factura.Referencia}' tiene pagos asociados, anúlelos o desasocielos primero");
            }
            foreach (var pago in pagos)
            {
                var preasientoDelPago = pago.Preasiento(contexto, errorSiNoHay: false);
                if (preasientoDelPago != null && preasientoDelPago.EstaEnLaEtapa(enumEtapasDePreasiento.SPR_Etapa_Contabilizado))
                {
                    Emitir($"La factura '{factura.Referencia}' tiene el pago '{pago.Referencia}' ya contabilizado, anúle el preasiento '{pago.Preasiento(contexto).Referencia}' antes de cancelar la factura");
                }

                if (pago.Clase == enumClaseDePago.Contado)
                {

                    if (pago.EstaEnLaEtapa(enumEtapasDePagos.PAG_Etapa_Pagado))
                    {
                        pago.TransitarALaEtapa(contexto, enumEtapasDePagos.PAG_Etapa_Pendiente.EstadosDeLaEtapa(), new Dictionary<string, object> {
                        { ltrParametrosNeg.EstaEjecutandoUnaAccion, true },
                        { ltrParametrosEp.asunto,"Reabrir pago" },
                        { ltrParametrosEp.detalleAsunto, $"Reabierto el pago al cancelar la factura '{factura.Referencia}'" }
                    }, delSistema: false);
                    }

                    if (pago.EstaEnLaEtapa(enumEtapasDePagos.PAG_Etapa_Pendiente))
                    {
                        pago.PagadoEl = null;
                        pago.TransitarALaEtapa(contexto, enumEtapasDePagos.PAG_Etapa_Cancelado.EstadosDeLaEtapa(), new Dictionary<string, object> {
                        { ltrParametrosNeg.EstaEjecutandoUnaAccion, true },
                        { ltrParametrosEp.asunto,"Cancelar pago" },
                        { ltrParametrosEp.detalleAsunto, $"Cancelado el pago al cancelar la factura '{factura.Referencia}'" }
                    }, delSistema: false);
                    }
                }
                else
                {
                    Emitir($"La factura '{factura.Referencia}' tiene el pago '{pago.Referencia}' cancelelo antes de cancelar la factura");
                }
            }

            factura.IdRectificada = null;
        }

        public static void TrasDarPorPagada(this FacturaRecDtm factura, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            if (factura.IdContrato is not null)
            {
                var contrato = contexto.SeleccionarPorId<ContratoDtm>(factura.IdContrato.Entero());
                var avance = contrato.Ampliacion<AvanceDtm>(contexto);
                avance.Facturado = avance.Facturado - factura.BaseImponible;
                avance.Cobrado = avance.Cobrado + factura.BaseImponible;
                avance.Modificar(contexto);
            }
        }

        public static void TrasAnularElPago(this FacturaRecDtm factura, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            if (factura.IdContrato is not null)
            {
                var contrato = contexto.SeleccionarPorId<ContratoDtm>(factura.IdContrato.Entero());
                var avance = contrato.Ampliacion<AvanceDtm>(contexto);
                avance.Facturado = avance.Facturado + factura.BaseImponible;
                avance.Cobrado = avance.Cobrado - factura.BaseImponible;
                avance.Modificar(contexto);
            }
        }

        public static bool EsIncorporada(this FacturaRecDtm factura, ContextoSe contexto)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Far_Incorporadas);
            if (!cache.ContainsKey(factura.Id.ToString()))
            {
                cache[factura.Id.ToString()] = contexto.Set<TrazasDeUnaFacturaRecDtm>().Any(far => far.IdElemento == factura.Id && far.Nombre == ltrDeUnaFacturaRec.TrazaDeIncorporacion);
            }
            return (bool)cache[factura.Id.ToString()];
        }

        public static PagoDtm PrepararPagoContado(this FacturaRecDto facturaDto, ContextoSe contexto)
        {
            ParametroDeNegocioDtm tipoPagoPorDefecto = ExtensorDePagos.TipoDePagoPorDefecto(contexto);
            if (facturaDto.ModoDePago == enumModoDePagoContado.Tarjeta && facturaDto.IdTarjeta == 0)
                Emitir("Ha de indicar la tarjeta con la que se ha realizado el pago");

            if (facturaDto.ModoDePago == enumModoDePagoContado.Domiciliacion && facturaDto.IdDomiciliadaEn == 0)
                Emitir("Ha de indicar la cuenta donde está domiciliado el pago");

            var tarjeta = facturaDto.ModoDePago == enumModoDePagoContado.Tarjeta ? contexto.SeleccionarPorId<TarjetaDeMiSociedadDtm>(facturaDto.IdTarjeta) : null;

            var CtaSociedad = contexto.SeleccionarPorId<CuentaDeMiSociedadDtm>(facturaDto.IdDomiciliadaEn, errorSiNoHay: false);
            var idCtaSociedad = CtaSociedad?.Id;

            var idCuentaDePago = facturaDto.ModoDePago == enumModoDePagoContado.Tarjeta
                ? tarjeta.IdCuentaDeCargo
                : facturaDto.ModoDePago == enumModoDePagoContado.Domiciliacion
                ? idCtaSociedad
                : null;
            var pago = new PagoDtm
            {
                IdCg = facturaDto.IdCg,
                IdTipo = tipoPagoPorDefecto.Valor.Entero(),
                Clase = enumClaseDePago.Contado,
                IdProveedor = facturaDto.IdProveedor,
                IdTarjetaDePago = facturaDto.ModoDePago == enumModoDePagoContado.Tarjeta ? facturaDto.IdTarjeta : null,
                IdCuentaDePago = idCuentaDePago,
                PagadoEl = facturaDto.FacturadaEl,
                PagarEl = facturaDto.FacturadaEl,
                Importe = facturaDto.TotalDelPago,
                IdSolicitante = contexto.SeleccionarPorId<ProveedorDtm>(facturaDto.IdProveedor).IdInterlocutor
            };

            pago.Nombre = $"Pago al contado de la factura '{facturaDto.Numero}'";
            if (facturaDto.ModoDePago == enumModoDePagoContado.Tarjeta)
                pago.Nombre = pago.Nombre + $" con la tarjeta '{contexto.SeleccionarPorId<TarjetaDeMiSociedadDtm>(facturaDto.IdTarjeta).Expresion}'";
            else if (facturaDto.ModoDePago == enumModoDePagoContado.Domiciliacion)
                pago.Nombre = $"Pago por domiciliación en '{contexto.SeleccionarPorId<CuentaDeMiSociedadDtm>(facturaDto.IdDomiciliadaEn).Cuenta(contexto).NumeroIban}'";

            return pago;
        }

        public static FacturaRecDtm Copiar(this FacturaRecDtm farOrigen, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            var idExpediente = (int)parametros.LeerValor<long>(nameof(CopiarFarDto.idExpediente), 0);
            var idContrato = (int)parametros.LeerValor<long>(nameof(CopiarFarDto.IdContrato), 0);
            var idArchivo = (int)parametros.LeerValor<long>(nameof(CopiarFarDto.IdArchivoAlCopiar), 0);
            var pagadaEl = parametros.LeerValor<DateTime?>(nameof(CopiarFarDto.PagadaEl), null);
            var facturadaEl = parametros.LeerValor<DateTime>(nameof(CopiarFarDto.FacturadaEl));
            var recibidaEl = EstimarFechaDeRecepcion(contexto, facturadaEl);

            var transaccion = contexto.IniciarTransaccion();
            try
            {
                var farNueva = new FacturaRecDtm
                {
                    IdCg = contexto.SeleccionarPorId<CentroGestorDtm>((int)(long)parametros[nameof(CopiarFarDto.IdCg)]).Id,
                    IdProveedor = contexto.SeleccionarPorId<ProveedorDtm>((int)(long)parametros[nameof(CopiarFarDto.IdProveedor)]).Id,
                    IdTipo = contexto.SeleccionarPorId<TipoDeFacturaRecDtm>((int)(long)parametros[nameof(CopiarFarDto.IdTipo)]).Id,
                    Nombre = parametros.LeerValor<string>(nameof(CopiarFarDto.Nombre)),
                    Numero = parametros.LeerValor<string>(nameof(CopiarFarDto.Numero)),
                    FacturadaEl = facturadaEl,
                    RecibidaEl = recibidaEl,
                    BaseImponible = parametros.LeerValor<decimal>(nameof(CopiarFarDto.BaseImponible)),
                    TotalDelPago = parametros.LeerValor<decimal>(nameof(CopiarFarDto.TotalDelPago)),
                    Descripcion = parametros.LeerValor<string>(nameof(CopiarFarDto.Descripcion)),
                    IdExpediente = idExpediente == 0 ? null : idExpediente,
                    IdContrato = idContrato == 0 ? null : idContrato,
                    IdArchivo = idArchivo == 0 ? null : idArchivo,
                    IdEstado = 0
                }.Insertar(contexto, new Dictionary<string, object> { { ltrDeUnaFacturaRec.TrazaDeCopiaDeFactura, $"Copia de la factura {farOrigen.Expresion}" } });

                var lineasFar = contexto.SeleccionarTodos<LineaDeUnaFarDtm>(nameof(LineaDeUnaFarDtm.IdElemento), farOrigen.Id);

                var ajustarBi = false;
                if (farOrigen.BaseImponible != farNueva.BaseImponible &&
                    lineasFar.Count == 2 &&
                    lineasFar.Where(x => x.BaseImponible == farOrigen.BaseImponible && x.Clase == enumClaseDeLineaFar.BaseImponible).Count() == 1 &&
                    lineasFar.Where(x => x.Clase == enumClaseDeLineaFar.LineaDeIva).Count() == 1)
                    ajustarBi = true;

                foreach (var linea in lineasFar)
                {
                    linea.Id = 0;
                    linea.IdElemento = farNueva.Id;
                    if (ajustarBi || (lineasFar.Count() == 1 && farNueva.BaseImponible != farOrigen.BaseImponible))
                    {
                        if (linea.Clase == enumClaseDeLineaFar.BaseImponible)
                        {
                            linea.Cantidad = linea.Cantidad * (farNueva.BaseImponible / linea.BaseImponible);
                        }
                        linea.BaseImponible = farNueva.BaseImponible;
                    }

                    linea.Insertar(contexto);
                }

                if (pagadaEl.HasValue)
                {
                    var pagos = farOrigen.PagosRealizados(contexto);

                    if (pagos.Count == 0)
                        Emitir($"Se ha indicado copiar la factura '{farOrigen.Referencia}' y el pago, y la factura está sin pagos, blanquee la fecha de pago '{pagadaEl.Fecha().ToString("dd/MM/yyyy")}' y cree los pagos tras editarla.");
                    if (pagos.Count > 1)
                        Emitir($"Se ha indicado copiar la factura '{farOrigen.Referencia}' y el pago, y la factura tiene más de un pago, blanquee la fecha de pago '{pagadaEl.Fecha().ToString("dd/MM/yyyy")}' y cree los pagos tras editarla.");

                    pagos[0].Id = 0;
                    pagos[0].IdEstado = 0;
                    pagos[0].IdFacturaRec = farNueva.Id;
                    pagos[0].FacturaRec = farNueva;
                    pagos[0].Nombre = $"Pago de la factura: {farNueva.Referencia}";
                    pagos[0].PagarEl = farNueva.FacturadaEl;
                    pagos[0].PagadoEl = null;
                    pagos[0].Importe = farNueva.TotalDelPago;
                    pagos[0].IdPreasiento = null;
                    if (pagos[0].Clase == enumClaseDePago.Contado)
                        pagos[0].PagadoEl = (pagos[0].ModoDePago == enumModoDePagoContado.Contado || pagos[0].ModoDePago == enumModoDePagoContado.Domiciliacion)
                        ? pagadaEl.Fecha()
                        : pagos[0].ModoDePago == enumModoDePagoContado.Tarjeta && pagos[0].PagadoEl == pagos[0].PagarEl
                        ? pagos[0].PagadoEl = pagos[0].PagarEl
                        : pagos[0].PagadoEl = null;


                    var nuevoPago = pagos[0].Insertar(contexto);

                    if (nuevoPago.Clase == enumClaseDePago.Contado && farNueva.IdArchivo is not null)
                    {
                        nuevoPago.Vincular(contexto, farNueva.Archivo(contexto), new Dictionary<string, object> { { ltrParametrosNeg.AccionQueSeEjecuta, ltrDeUnaFacturaRec.Accion_CopiarFactura } });
                    }

                }

                contexto.Commit(transaccion);
                return farNueva;
            }
            catch
            {
                contexto.Rollback(transaccion);
                throw;
            }
        }

        public static FacturaRecDtm ModificarAsunto(this FacturaRecDtm factura, ContextoSe contexto, string asunto)
        {
            if (!factura.EsInterventor(contexto))
                Emitir($"Para poder cambiar el asunto de la factura '{factura.Referencia}' necesita permisos de intervención");
            var tran = contexto.IniciarTransaccion();
            try
            {
                var asuntoAnterior = factura.Nombre;
                factura.Nombre = asunto;
                factura = factura.Modificar(contexto, accionEjecutada: ltrDeUnElemento.Accion_Renombrar);
                factura.CrearTraza(contexto, "Asunto modificado", $"El usuario '{contexto.DatosDeConexion.Login}' ha modificado el asunto de la factura, antes llamado '{asuntoAnterior}'");
                var lineas = factura.Detalles<LineaDeUnaFarDtm>(contexto);
                foreach (var line in lineas.Where(x => x.Concepto.StartsWith(asuntoAnterior)))
                {
                    line.Concepto = line.Concepto.Replace(asuntoAnterior, asunto);
                    line.Modificar(contexto, accionEjecutada: ltrDeUnElemento.Accion_Renombrar);
                }

                contexto.Commit(tran);
                return factura;
            }
            catch
            {
                contexto.Rollback(tran);
                throw;
            }
        }


        public static bool ModificarNaturaleza(this FacturaRecDtm factura, ContextoSe contexto, int idNaturalezaAnterior, int idNaturalezaNueva)
        {
            if (!factura.EsInterventor(contexto))
                Emitir($"Para poder cambiar la naturaleza de la factura '{factura.Referencia}' necesita permisos de intervención");

            if (idNaturalezaAnterior == 0 && idNaturalezaNueva == 0)
                return false;

            if (idNaturalezaAnterior > 0 && idNaturalezaNueva == 0)
                Emitir($"Ha de indicar la naturaleza por la que cambiar la anterior '{contexto.SeleccionarPorId<NaturalezaDtm>(idNaturalezaAnterior).Expresion}'");

            var tran = contexto.IniciarTransaccion();
            try
            {
                if (tran && factura.IdPreasiento.HasValue)
                    Emitir($"No puede modificar la naturaleza de la factura '{factura.Referencia}' ya que tiene el preasiento '{factura.Preasiento(contexto).Referencia}' asociado");

                var lineas = factura.Detalles<LineaDeUnaFarDtm>(contexto);
                bool cambiadas = false;
                foreach (var linea in lineas)
                {
                    if (linea.Clase == enumClaseDeLineaFar.LineaDeIva || linea.Clase == enumClaseDeLineaFar.LineaDeIrpf || linea.IdNaturaleza.Entero() == 0)
                        continue;

                    if (idNaturalezaNueva == linea.IdNaturaleza.Entero())
                        continue;

                    if (idNaturalezaAnterior == 0 || idNaturalezaAnterior == linea.IdNaturaleza)
                        linea.IdNaturaleza = idNaturalezaNueva;

                    linea.Modificar(contexto, accionEjecutada: ltrDeUnaFacturaRec.Accion_ModificarNaturalezas);
                    cambiadas = true;
                }
                if (!cambiadas)
                    Emitir($"No se ha modificado la factura la factura '{factura.Referencia}' ya que los criterios de cambia de naturaleza no son aplicables a la factura");

                if (cambiadas)
                    factura.CrearTraza(contexto, "Naturalezas modificadas", $"El usuario '{contexto.DatosDeConexion.Login}' ha modificado las naturalezas de la factura{Environment.NewLine}" +
                        $"Naturaleza anterior: '{(idNaturalezaAnterior == 0 ? "Todas" : contexto.SeleccionarPorId<NaturalezaDtm>(idNaturalezaAnterior).Expresion)}'{Environment.NewLine}" +
                        $"Naturaleza nueva: '{contexto.SeleccionarPorId<NaturalezaDtm>(idNaturalezaNueva).Expresion}'");

                contexto.Commit(tran);
            }
            catch
            {
                contexto.Rollback(tran);
                throw;
            }

            return factura.IdPreasiento.HasValue;
        }


        public static (bool regenerarPreasiento, bool modificarPago) ModificarIva(this FacturaRecDtm factura, ContextoSe contexto, int idIvaAnterior, int idIvaNuevo)
        {
            if (!factura.EsInterventor(contexto))
                Emitir($"Para poder cambiar el iva de la factura '{factura.Referencia}' necesita permisos de intervención");

            if (idIvaAnterior == 0 && idIvaNuevo == 0)
                return (regenerarPreasiento: false, modificarPago: false);

            if (idIvaAnterior > 0 && idIvaNuevo == 0)
                Emitir($"Ha de indicar el nuevo iva por el que cambiar el anterior '{contexto.SeleccionarPorId<IvaSoportadoDtm>(idIvaAnterior).Expresion}'");

            if (idIvaAnterior == 0 && idIvaNuevo > 0)
                Emitir($"Ha de indicar el iva anterior por el que cambiar el que ha indicado '{contexto.SeleccionarPorId<IvaSoportadoDtm>(idIvaNuevo).Expresion}'");

            var anteriorIva = contexto.SeleccionarPorId<IvaSoportadoDtm>(idIvaAnterior);
            var nuevoIva = contexto.SeleccionarPorId<IvaSoportadoDtm>(idIvaNuevo);

            if (factura.PagosRealizados(contexto).Count > 1)
                Emitir($"No se puede modificar el Iva de la factura '{factura.Referencia}' por tener más de un pago realizado");

            if (factura.PagosRealizados(contexto).Count == 1 && Math.Abs(factura.TotalDelPago - factura.PagosRealizados(contexto)[0].Importe) > VariableDeFacturasRec.ToleranciaEnImportes())
                Emitir($"No se puede modificar el Iva de la factura '{factura.Referencia}' por que el pago asociado no coincide con el de la factura");

            var tran = contexto.IniciarTransaccion();
            try
            {
                if (tran && factura.IdPreasiento.HasValue)
                    Emitir($"No puede modificar el Iva de la factura '{factura.Referencia}' ya que tiene el preasiento '{factura.Preasiento(contexto).Referencia}' asociado");

                if (tran && factura.PagosRealizados(contexto).Count == 1)
                    Emitir($"No puede modificar el Iva de la factura '{factura.Referencia}' ya que tiene el pago '{factura.PagosRealizados(contexto)[0].Referencia}' asociado");

                var lineas = factura.Detalles<LineaDeUnaFarDtm>(contexto);
                bool cambiadas = false;
                foreach (var linea in lineas)
                {
                    if (linea.Clase == enumClaseDeLineaFar.BaseImponible || linea.Clase == enumClaseDeLineaFar.LineaDeIrpf)
                        continue;

                    if ((linea.Clase == enumClaseDeLineaFar.LineaDeIva || linea.Clase == enumClaseDeLineaFar.BiExenta || linea.Clase == enumClaseDeLineaFar.BiConIva)
                        && linea.IdIvaS != idIvaAnterior)
                        continue;


                    if (linea.PorcentajeIva != anteriorIva.Porcentaje && factura.IdPreasiento.HasValue)
                        Emitir($"No se puede modificar el Iva de la factura '{factura.Referencia}' por que el %iva indicado '{linea.PorcentajeIva.Porcentaje()}' en ella " +
                               $"no coincide con el del maestro '{anteriorIva.Porcentaje.Porcentaje()}' y tiene preasiento generado");

                    linea.IdIvaS = idIvaNuevo;
                    linea.PorcentajeIva = nuevoIva.Porcentaje;
                    linea.Modificar(contexto, accionEjecutada: ltrDeUnaFacturaRec.Accion_ModificarIva);
                    cambiadas = true;
                }

                if (!cambiadas)
                    Emitir($"No se ha modificado la factura la factura '{factura.Referencia}' ya que los criterios de IVA no son aplicables a la factura");

                if (cambiadas)
                    factura.CrearTraza(contexto, "Iva modificado", $"El usuario '{contexto.DatosDeConexion.Login}' ha modificado el Iva de la factura{Environment.NewLine}" +
                        $"Iva anterior: '{contexto.SeleccionarPorId<IvaSoportadoDtm>(idIvaAnterior).Expresion}'{Environment.NewLine}" +
                        $"Iva nuevo: '{contexto.SeleccionarPorId<IvaSoportadoDtm>(idIvaNuevo).Expresion}'");

                factura.TotalDelPago = factura.Total(contexto, enumImporteFar.BaseImponible) + factura.Total(contexto, enumImporteFar.TotalIva) - factura.Total(contexto, enumImporteFar.TotalIrpf);
                factura.ModificarComoAdministrador(contexto, accionQueSeEjecuta: ltrDeUnaFacturaRec.Accion_ModificarIva);
                contexto.Commit(tran);
            }
            catch
            {
                contexto.Rollback(tran);
                throw;
            }

            return (regenerarPreasiento: factura.IdPreasiento.HasValue, modificarPago: factura.PagosRealizados(contexto).Count > 0);
        }

        public static FacturaRecDtm CambiarProveedor(this FacturaRecDtm factura, ContextoSe contexto, int idProveedor)
        {
            if (factura.EsRectificativa)
                Emitir($"No se le puede cambiar el proveedor a una factura rectificativa");

            if (!factura.EsInterventor(contexto))
                Emitir($"Para poder cambiar el proveedor de la factura '{factura.Referencia}' necesita permisos de intervención");

            if (!factura.EstaEnLaEtapa(enumEtapasDeFacturasRec.FAR_Etapa_De_Cumplimentacion))
                Emitir($"Para poder cambiar el proveedor de la factura '{factura.Referencia}' ha de estar en '{enumEtapasDeFacturasRec.FAR_Etapa_De_Cumplimentacion.Nombre()}'");

            if (factura.PagosConfirmados(contexto).Count() > 0)
                Emitir($"Para poder cambiar el proveedor de la factura '{factura.Referencia}' primero a de reabrir los pagos realizados");

            if (factura.IdPreasiento.HasValue)
                Emitir($"No puede cambiar el proveedor de la factura '{factura.Referencia}' por tener el preasiento '{factura.Preasiento(contexto).Referencia}' asociado");

            var tran = contexto.IniciarTransaccion();
            try
            {
                var proveedor = contexto.SeleccionarPorId<ProveedorDtm>(idProveedor);
                var proveedorAnterior = factura.Proveedor(contexto);
                factura.IdProveedor = idProveedor;
                factura.Contacto = proveedor.Expresion;
                factura.eMail = proveedor.eMail;
                factura.Telefono = proveedor.Telefono;
                factura = factura.Modificar(contexto, accionEjecutada: ltrDeUnaFacturaRec.Accion_CambiarProveedor);
                factura.CrearTraza(contexto, "Proveedor cambiado", $"El usuario '{contexto.DatosDeConexion.Login}' ha cambiado el proveedor de la factura, antes '{proveedorAnterior.RazonSocial(contexto)}'");

                foreach (var pago in factura.PagosRealizados(contexto))
                {
                    if (pago.IdPreasiento is not null)
                        Emitir($"Para poder cambiar el proveedor de la factura '{factura.Referencia}' primero a de anular el asiento del pago '{pago.Preasiento(contexto).Referencia}'");

                    if (pago.IdProveedor is null || pago.IdProveedor != proveedorAnterior.Id)
                        Emitir($"El pago '{pago.Referencia}' de la factura '{factura.Referencia}' no es del mismo proveedor o no está indicado, anule el pago antes de modificar la factura'");

                    pago.IdProveedor = proveedor.Id;
                    pago.IdSolicitante = proveedor.IdInterlocutor;
                    pago.Contacto = proveedor.Interlocutor(contexto).RazonSocial(contexto);
                    pago.eMail = proveedor.eMail;
                    pago.Telefono = proveedor.Telefono;
                    pago.PagarEl = null;
                    pago.Modificar(contexto, accionEjecutada: ltrDeUnPago.Accion_CambiarProveedor);
                    pago.CrearTraza(contexto, "Proveedor cambiado", $"El usuario '{contexto.DatosDeConexion.Login}' ha cambiado el proveedor del pago, antes '{proveedorAnterior.RazonSocial(contexto)}'");
                }

                contexto.Commit(tran);
                return factura;
            }
            catch
            {
                contexto.Rollback(tran);
                throw;
            }
        }


        public static FacturaRecDtm Rectificar(this FacturaRecDtm farOrigen, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            var quienMeRestifica = farOrigen.QuienMeRectifica(contexto);
            if (quienMeRestifica is not null)
            {
                Emitir($"No puede rectificar la factura '{farOrigen.Referencia}' por estar ya rectificada por '{quienMeRestifica.Referencia}'");
            }

            var baseImponible = parametros.LeerValor<decimal>(nameof(RectificarFarDto.BaseImponible));
            var totalDelPago = parametros.LeerValor<decimal>(nameof(RectificarFarDto.TotalDelPago));

            if (Math.Abs(baseImponible) > farOrigen.BaseImponible || Math.Abs(totalDelPago) > farOrigen.TotalDelPago)
                Emitir($"No puede rectificar la factura '{farOrigen.Referencia}' ya que el total a pagar o la bi que ha indicado es mayor");

            if (baseImponible >= 0 || totalDelPago >= 0)
                Emitir($"No puede rectificar la factura '{farOrigen.Referencia}' ya que los importes han de ser negativos");

            var idArchivo = (int)parametros.LeerValor<long>(nameof(RectificarFarDto.IdArchivoRectificativa), 0);
            var facturadaEl = parametros.LeerValor<DateTime>(nameof(RectificarFarDto.FacturadaEl));
            if (facturadaEl < farOrigen.FacturadaEl)
                Emitir($"No puede rectificar la factura '{farOrigen.Referencia}' ya que la fecha de esta '{farOrigen.FacturadaEl.ToString("yyyy-MM-dd")}' es posterior a la indicada '{facturadaEl.ToString("yyyy-MM-dd")}'");

            DateTime recibidaEl = EstimarFechaDeRecepcion(contexto, facturadaEl);
            var nuevoNumero = parametros.LeerValor<string>(nameof(RectificarFarDto.Numero));
            if (nuevoNumero == farOrigen.Numero)
                Emitir($"No puede rectificar la factura '{farOrigen.Referencia}' ya que le ha indicado el mismo número de factura '{nuevoNumero}'");

            var transaccion = contexto.IniciarTransaccion();
            try
            {
                var farNueva = new FacturaRecDtm
                {
                    IdCg = farOrigen.IdCg,
                    IdProveedor = farOrigen.IdProveedor,
                    IdTipo = farOrigen.IdTipo,
                    Nombre = parametros.LeerValor<string>(nameof(RectificarFarDto.Nombre)),
                    Numero = nuevoNumero,
                    FacturadaEl = facturadaEl,
                    RecibidaEl = recibidaEl,
                    BaseImponible = baseImponible,
                    TotalDelPago = totalDelPago,
                    Descripcion = parametros.LeerValor<string>(nameof(RectificarFarDto.Descripcion)),
                    IdRectificada = farOrigen.Id,
                    IdExpediente = farOrigen.IdExpediente,
                    IdContrato = farOrigen.IdExpediente,
                    IdArchivo = idArchivo == 0 ? null : idArchivo,
                    IdEstado = 0
                }.Insertar(contexto, new Dictionary<string, object> { { ltrDeUnaFacturaRec.TrazaDeRectificacionDeFactura, $"Rectificación de la factura {farOrigen.Expresion}" } });

                decimal factor = Math.Abs(farNueva.BaseImponible / farNueva.Rectificada(contexto).BaseImponible);
                var lineasFar = contexto.SeleccionarTodos<LineaDeUnaFarDtm>(nameof(LineaDeUnaFarDtm.IdElemento), farOrigen.Id);
                foreach (var linea in lineasFar)
                {
                    linea.Id = 0;
                    linea.IdElemento = farNueva.Id;
                    linea.BaseImponible = -1 * linea.BaseImponible * factor;
                    linea.Insertar(contexto);
                }

                contexto.Commit(transaccion);
                return farNueva;
            }
            catch
            {
                contexto.Rollback(transaccion);
                throw;
            }
        }

        private static DateTime EstimarFechaDeRecepcion(ContextoSe contexto, DateTime facturadaEl)
        {
            var recibidaEl = facturadaEl;
            var regla = enumParametrosDeFacturasRec.FAR_Como_Tratar_La_Fecha_De_Recepcion.Cadena(errorSiNoDefinido: false, valorPorDefecto: enumValoresDeComoTratarRecibidoEl.mismafecha.ToString());
            var enumRegla = ApiDeEnsamblados.ToEnumerado<enumValoresDeComoTratarRecibidoEl>(regla);
            if (enumRegla == enumValoresDeComoTratarRecibidoEl.fechadehoy) recibidaEl = DateTime.Now.Date;
            else if (enumRegla == enumValoresDeComoTratarRecibidoEl.fechadehoy15 && facturadaEl <= DateTime.Now.Date.AddDays(-15))
                recibidaEl = DateTime.Now.Date;
            else if (enumRegla == enumValoresDeComoTratarRecibidoEl.fechadehoy30 && facturadaEl <= DateTime.Now.Date.AddDays(-30))
                recibidaEl = DateTime.Now.Date;
            return recibidaEl;
        }


        public static ArchivadorDtm CrearArchivadorDeFacturas(ContextoSe contexto, CentroGestorDtm cg, string nombre, ProveedorDtm proveedor)
        {
            var idTipoArchivador = enumParametrosDeFacturasRec.FAR_Tipo_De_Archivador.Entero(); // VariableDeFacturasRec. IdTipoArchivadorFacturaRec(contexto);

            var descripcion = proveedor?.Nombre ?? null;
            var archivador = new ArchivadorDtm
            {
                IdCg = cg.Id,
                IdTipo = idTipoArchivador,
                Nombre = nombre,
                Descripcion = descripcion.Left(2000)
            }.InsertarComoAdministrador(contexto);

            return archivador;
        }

        public static FacturaRecDtm CrearFactura(ContextoSe contexto, ArchivadorDtm archivador, int idTipo, string jsonFactura, ProveedorDtm proveedor, ArchivoDtm archivo, int? idCg = null)
        {
            FacturaJson facturaJson = JsonConvert.DeserializeObject<FacturaJson>(jsonFactura);

            var telefono = facturaJson?.Telefono.Replace(" ", "").Replace(".", "").Replace("-", "") ?? ".";
            if (telefono.StartsWith("("))
                telefono = "+" + telefono.Replace("(", "").Replace(")", "");

            if (proveedor is null)
            {
                if (ApiClasesComunes.EsNulo(facturaJson.Nif))
                    Emitir("No se ha identificado el nif del proveedor");


                if (ApiClasesComunes.EsNulo(facturaJson.Proveedor))
                    Emitir("No se ha identificado la razón social del proveedor");

                proveedor = contexto.SeleccionarPorPropiedad<ProveedorDtm>(ltrProveedor.NIF.ToLower(), facturaJson.Nif, errorSiNoHay: false);
                if (proveedor is null)
                {
                    var sociedad = new SociedadDtm
                    {
                        Nombre = facturaJson.Proveedor,
                        RazonSocial = facturaJson.Proveedor,
                        NIF = facturaJson.Nif,
                        eMail = facturaJson?.eMail ?? "@",
                        Telefono = telefono
                    };
                    proveedor = ExtensorDeProveedores.CrearProveedor(contexto, sociedad);
                }
            }

            // Validación de campo obligatorio
            if (ApiClasesComunes.EsNulo(facturaJson.NumeroFactura))
                Emitir("No se ha identificado el nº de factura");

            // Conversión segura de fechas
            DateTime tempFecha;
            DateTime? facturadaEl = DateTime.TryParse(facturaJson.Fecha, out tempFecha)
                                    ? tempFecha
                                    : (DateTime?)null;

            DateTime? venceEl = DateTime.TryParse((string)facturaJson.FechaVencimiento, out tempFecha)
                                ? tempFecha
                                : (DateTime?)null;

            // Validación de fecha obligatoria
            if (!facturadaEl.HasValue)
                Emitir("La fecha de factura es obligatoria");

            // Creación del objeto
            FacturaRecDtm factura = new FacturaRecDtm
            {
                IdCg = idCg is null ? archivador.IdCg : (int)idCg,
                IdTipo = idTipo,
                Nombre = proveedor.Concepto.IsNullOrEmpty() ? facturaJson.Concepto : proveedor.Concepto,
                Descripcion = proveedor.Concepto.IsNullOrEmpty() ? null : facturaJson.Concepto,
                Numero = facturaJson.NumeroFactura,
                IdProveedor = proveedor.Id,
                Contacto = proveedor.Expresion,
                Telefono = telefono,
                eMail = facturaJson.eMail,
                RecibidaEl = DateTime.Now,
                FacturadaEl = facturadaEl.Value,
                BaseImponible = facturaJson.Bi,
                TotalDelPago = facturaJson.Total,
                IdArchivo = archivo.Id
            };
            archivo.AuditarEnlazar(contexto, archivador.Referencia);

            if (venceEl.HasValue)
            {
                factura.VenceEl = venceEl.Value;
            }

            factura = factura.InsertarComoAdministrador(contexto);

            if (facturaJson.Bi != 0)
            {
                var lineaBi = new LineaDeUnaFarDtm
                {
                    IdElemento = factura.Id,
                    Orden = 10,
                    Clase = enumClaseDeLineaFar.BaseImponible,
                    Concepto = facturaJson.Concepto,
                    IdNaturaleza = enumParametrosDeFacturasRec.FAR_Naturaleza.Entero(),
                    IdUnidad = enumParametrosDeFacturasRec.FAR_Unidad_Medida.Entero(),
                    Cantidad = 1,
                    BaseImponible = facturaJson.Bi
                }.InsertarComoAdministrador(contexto);
            }
            decimal ivaPorcentaje = 0;
            if (facturaJson.TotalIva != 0 && Math.Abs(facturaJson.Total - (facturaJson.Bi + facturaJson.TotalIva - facturaJson.TotalIrpf)) < VariableDeFacturasRec.ToleranciaEnImportes())
            {
                ivaPorcentaje = Math.Round((facturaJson.TotalIva / facturaJson.Bi) * 100, 0);
                var iva = contexto.Set<IvaSoportadoDtm>().Where(i => i.Porcentaje == ivaPorcentaje).FirstOrDefault();
                if (iva is not null)
                {
                    var lineaIva = new LineaDeUnaFarDtm
                    {
                        IdElemento = factura.Id,
                        Orden = 20,
                        Clase = enumClaseDeLineaFar.LineaDeIva,
                        Concepto = facturaJson.Concepto,
                        BaseImponible = facturaJson.Bi,
                        IdIvaS = iva.Id,
                        PorcentajeIva = ivaPorcentaje
                    }.InsertarComoAdministrador(contexto);
                }
                if (facturaJson.TotalIrpf != 0)
                {
                    var irpfPorcentaje = Math.Round((facturaJson.TotalIrpf / facturaJson.Bi) * 100, 0);
                    var irpf = contexto.Set<IrpfDtm>().Where(i => i.Porcentaje == irpfPorcentaje).FirstOrDefault();
                    if (irpf is not null)
                    {
                        var lineaIrpf = new LineaDeUnaFarDtm
                        {
                            IdElemento = factura.Id,
                            Orden = 20,
                            Clase = enumClaseDeLineaFar.LineaDeIrpf,
                            BaseImponible = facturaJson.Bi,
                            IdIrpf = irpf.Id,
                            PorcentajeIrpf = irpfPorcentaje
                        }.InsertarComoAdministrador(contexto);
                    }
                }
            }

            return factura;
        }

    }

}
