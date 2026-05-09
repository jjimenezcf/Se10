using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Gestor.Errores;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using ModeloDeDto;
using ModeloDeDto.Negocio;
using ModeloDeDto.Ventas;
using ServicioDeDatos;
using ServicioDeDatos.Callejero;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Gastos;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.MaestrosTecnico;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.Presupuesto;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Tarea;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Ventas;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;
using Utilidades;
using static Gestor.Errores.GestorDeErrores;
using static ServicioDeDatos.Elemento.Enumerados;

namespace GestorDeElementos.Extensores
{
    public static class ExtensorDeFacturasEmt
    {
        public static bool UsaElDetalleDe(Type tipoDeDetalle)
        {
            if (tipoDeDetalle == typeof(LineaDeUnaFaeDtm))
                return true;

            if (tipoDeDetalle == typeof(CobroDeFaeDtm))
                return true;

            if (tipoDeDetalle == typeof(RectificativaEmtDtm))
                return true;

            return false;
        }

        public static int? UltimoNumeroDeLaSerie(this SociedadDtm sociedad, ContextoSe contexto, int ano, string serie)
        {
            return contexto.Set<FacturaEmtDtm>().Where(x => x.Ano == ano && x.Serie == serie &&
                                     contexto.Set<CentroGestorDtm>().First(cg => cg.Id == x.IdCg).IdSociedad == sociedad.Id)
                                   .Max(x => x.Numero);
        }

        public static FacturaEmtDtm UltimaFactura(this SociedadDtm sociedad, ContextoSe contexto, int ano, bool errorSiNoHay = false)
        {
            var factura = contexto.Set<FacturaEmtDtm>().Where(f =>
                                     contexto.Set<CentroGestorDtm>().First(cg => cg.Id == f.IdCg).IdSociedad == sociedad.Id && f.Ano == ano && f.FacturadaEl != null)
                                     .OrderByDescending(x => x.FacturadaEl)
                                     .FirstOrDefault();

            if (factura == null && errorSiNoHay) Emitir($"La sociedad '{sociedad.NIF}' no tiene ninguna factura emitida");
            return factura;
        }

        public static FacturaEmtDtm Anterior(this FacturaEmtDtm factura, ContextoSe contexto)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Fae_Anterior);
            if (!cache.ContainsKey(factura.Id.ToString()))
            {
                var idSociedad = factura.Sociedad(contexto).Id;

                var anteriorEnMismoAno = contexto.Set<FacturaEmtDtm>().Where(anterior =>
                      anterior.Serie == factura.Serie &&
                      anterior.Ano == factura.Ano &&
                      anterior.Id != factura.Id &&
                      anterior.Numero < factura.Numero &&
                      contexto.Set<CentroGestorDtm>().First(cg => cg.Id == anterior.IdCg).IdSociedad == idSociedad &&
                      anterior.FacturadaEl != null)
                .OrderByDescending(x => x.FacturadaEl)
                .FirstOrDefault();

                cache[factura.Id.ToString()] = anteriorEnMismoAno != null
                ? anteriorEnMismoAno
                : contexto.Set<FacturaEmtDtm>().Where(anterior =>
                                     anterior.Serie == factura.Serie &&
                                     anterior.Ano < factura.Ano &&
                                     contexto.Set<CentroGestorDtm>().First(cg => cg.Id == anterior.IdCg).IdSociedad == idSociedad)
                                     .OrderByDescending(x => x.FacturadaEl)
                                     .FirstOrDefault();
            }
            return (FacturaEmtDtm)cache[factura.Id.ToString()];
        }

        public static FacturaEmtDtm UltimaEmitidaDelAno(this FacturaEmtDtm factura, ContextoSe contexto, string serie = null)
        {
            var idSociedad = factura.Sociedad(contexto).Id;
            return contexto.Set<FacturaEmtDtm>().Where(x => x.Serie == (serie == null ? factura.Serie : serie) &&
                                                     x.Id != factura.Id &&
                                                     x.Ano == factura.Ano &&
                                                     contexto.Set<CentroGestorDtm>().First(cg => cg.Id == x.IdCg).IdSociedad == idSociedad &&
                                                     x.EmitidaEl != null)
                    .OrderByDescending(x => x.FacturadaEl)
                    .FirstOrDefault();
        }


        public static string CalcularHuella(this FacturaEmtDtm factura, ContextoSe contexto)
        {
            var facturaAnterior = factura.Anterior(contexto);

            var tipoFacturaAeat = !factura.EsRectificativa ? enumTipoFacturaAeat.F1.ToString()
                : factura.ClaseRectificativa == enumClaseDeRectificativa.OR ? enumTipoFacturaAeat.R1.ToString()
            : enumTipoFacturaAeat.R2.ToString();

            var datosParaHash = $"{ltrSii.IDEmisorFactura}={factura.Sociedad(contexto).NIFSinIsoEs}&" +
                 $"{ltrSii.NumSerieFactura}={factura.NumeroDeFactura}&" +
                 $"{ltrSii.FechaExpedicionFactura}={factura.FacturadaEl.FechaCorta()}&" +
                 $"{ltrSii.TipoFactura}={tipoFacturaAeat}&" +
                 $"{ltrSii.CuotaTotal}={factura.TotalDeIva(contexto).Formatear(alineacion: false, separadorDecimal: ".")}&" +
                 $"{ltrSii.ImporteTotal}={factura.Bi(contexto) + factura.TotalDeIva(contexto).Formatear(alineacion: false, separadorDecimal: ".")}&" +
                 $"{ltrSii.Huella}={facturaAnterior?.Huella ?? ""}&" +
                 $"{ltrSii.FechaHoraHusoGenRegistro}={((DateTime?)DateTime.Now).FechaFormatoIso8601()}";

            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(datosParaHash));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public static byte[] Firmar(this FacturaEmtDtm factura, ContextoSe contexto)
        {
            if (!factura.Sociedad(contexto).HayQueFirmarFacturas(contexto))
                return null;

            try
            {
                factura.FirmadaEl = DateTime.UtcNow;
                byte[] firma = FirmarRegistro(factura, contexto);
                return firma;
            }
            catch (Exception e)
            {
                throw e.Emitir($"No se ha podido firmar, actualice el ceriticado de la empresa '{factura.Sociedad(contexto).NIF}'");
            }
        }


        public static bool ValidarFirma(this FacturaEmtDtm factura, ContextoSe contexto)
        {
            try
            {
                byte[] firma = FirmarRegistro(factura, contexto);
                return factura.Firma != null && firma != null && factura.Firma.SequenceEqual(firma);
            }
            catch (Exception e)
            {
                throw new Exception($"No se ha podido validar la firmar, ceriticado de la empresa '{factura.Sociedad(contexto).NIF}' no válido", e);
            }
        }

        private static byte[] FirmarRegistro(FacturaEmtDtm factura, ContextoSe contexto)
        {
            var facturaAnterior = factura.Anterior(contexto);
            var datos = PrepararDatosParaFirma(factura.Huella, facturaAnterior?.Firma ?? null, (DateTime)factura.FacturadaEl, fechaDeFirma: (DateTime)factura.FirmadaEl);
            var certificado = factura.Sociedad(contexto).ObtenerCertificado(contexto);
            var firma = new Firmante(contexto, certificado).FirmarRegistro(datos);
            return firma;
        }

        private static byte[] PrepararDatosParaFirma(string huellaActual, byte[] firmaAnterior, DateTime fechaEmision, DateTime fechaDeFirma)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                // Huella actual
                byte[] huellaBytes = huellaActual.HexStringToByteArray();
                ms.Write(huellaBytes, 0, huellaBytes.Length);

                // Firma anterior (si existe)
                if (firmaAnterior != null)
                    ms.Write(firmaAnterior, 0, firmaAnterior.Length);

                // Fecha de emisión (ISO 8601)
                byte[] fechaEmisionBytes = Encoding.UTF8.GetBytes(fechaEmision.ToString("dd-MM-yyyy HH:mm:ss"));
                ms.Write(fechaEmisionBytes, 0, fechaEmisionBytes.Length);

                // Fecha del sistema (ISO 8601)
                try
                {
                    byte[] fechaSistemaBytes = Encoding.UTF8.GetBytes(fechaDeFirma.ToString("dd-MM-yyyy HH:mm:ss"));
                    ms.Write(fechaSistemaBytes, 0, fechaSistemaBytes.Length);
                }
                catch (AggregateException ex)
                {
                    // Manejar la excepción real
                    throw ex.InnerException ?? ex;
                }


                return ms.ToArray();
            }
        }

        public static bool UsaLaAmpliacionDe(ContextoSe contexto, int idTipo, Type tipoAmpliacion)
        {

            if (tipoAmpliacion == typeof(IrpfEmtDtm))
                return true;

            if (tipoAmpliacion == typeof(VerifactuDtm))
                return true;

            var tipoDtm = (TipoDeFacturaEmtDtm)enumNegocio.FacturaEmitida.CrearGestorDeTipo(contexto).LeerRegistroPorId(idTipo, aplicarJoin: false);
            if (tipoDtm.ConPeriodoEmt && tipoAmpliacion == typeof(PeriodoEmtDtm))
                return true;

            return false;
        }

        public static bool TieneCentroAdministrativo(this FacturaEmtDtm factura, ContextoSe contexto)
        {
            CentroAdministrativoDtm centro = CentroAdministrativo(factura, contexto);
            return centro is not null;
        }

        public static CentroAdministrativoDtm CentroAdministrativo(this FacturaEmtDtm factura, ContextoSe contexto)
        {
            if (factura.IdCentroAdministrativo == null)
                return null;

            if (factura.CentroAdministrativo != null && factura.IdCentroAdministrativo == factura.CentroAdministrativo.Id)
                return factura.CentroAdministrativo;

            return factura.CentroAdministrativo = factura.Cliente(contexto).Detalles<CentroAdministrativoDtm>(contexto).First(x => x.Id == factura.IdCentroAdministrativo.Entero());

        }

        public static bool EstaEnLaEtapa(this FacturaEmtDtm fae, string etapa) => etapa.ToLista<int>(Simbolos.Coma).Contains(fae.IdEstado);

        public static decimal SinDescuento(this FacturaEmtDtm factura, ContextoSe contexto) => factura.Total(contexto, enumImporteEmt.SinDescuento);

        public static decimal Bi(this FacturaEmtDtm factura, ContextoSe contexto) => factura.Total(contexto, enumImporteEmt.BaseImponible);

        public static decimal BiConIva(this FacturaEmtDtm factura, ContextoSe contexto) => factura.Total(contexto, enumImporteEmt.BiConIva);

        public static decimal APagar(this FacturaEmtDtm factura, ContextoSe contexto) => factura.Total(contexto, enumImporteEmt.Apagar);

        public static decimal TotalDeIva(this FacturaEmtDtm factura, ContextoSe contexto) => factura.Total(contexto, enumImporteEmt.Iva);

        public static decimal TotalDeIrpf(this FacturaEmtDtm factura, ContextoSe contexto) => factura.Total(contexto, enumImporteEmt.Irpf);

        public static decimal Total(this FacturaEmtDtm factura, ContextoSe contexto, enumImporteEmt importeSolicitado)
        {
            var cacheSinDto = ServicioDeCaches.Obtener(CacheDe.Fae_BiSinDto);
            var cacheBi = ServicioDeCaches.Obtener(CacheDe.Fae_Bi);
            var cacheBiConIva = ServicioDeCaches.Obtener(CacheDe.Fae_BiConIva);
            var cacheApagar = ServicioDeCaches.Obtener(CacheDe.Fae_Apagar);
            var cacheDeIva = ServicioDeCaches.Obtener(CacheDe.Fae_TotalDeIva);
            var cacheDeIrpf = ServicioDeCaches.Obtener(CacheDe.Fae_TotalDeIrpf);
            if (!cacheBi.ContainsKey(factura.Id.ToString()))
            {
                decimal Bi = 0;
                decimal totalSinDto = 0;
                decimal iva = 0;
                decimal irpf = factura.Irpf(contexto);

                var totales = contexto.Set<LineaDeUnaFaeDtm>()
                          .Where(l => l.IdElemento == factura.Id && l.TipoDeLinea != enumTipoDeLinea.Comentario)
                          .GroupBy(l => l.IdElemento)
                          .Select(g => new
                          {
                              // ImporteSinDto = Precio * Cantidad
                              TotalSinDto = g.Sum(l => (l.Precio ?? 0) * (l.Cantidad ?? 0)),

                              // BI (ImporteConDto) = SinDto - (SinDto * Descuento / 100)
                              BI = g.Sum(l =>
                                  ((l.Precio ?? 0) * (l.Cantidad ?? 0)) -
                                  (((l.Precio ?? 0) * (l.Cantidad ?? 0)) * (l.Descuento ?? 0) / 100)
                              ),

                              // TotalIva = BI * Iva / 100
                              TotalIva = g.Sum(l =>
                                  (((l.Precio ?? 0) * (l.Cantidad ?? 0)) -
                                   (((l.Precio ?? 0) * (l.Cantidad ?? 0)) * (l.Descuento ?? 0) / 100))
                                  * (l.Iva ?? 0) / 100
                              )
                          })
                          .FirstOrDefault();

                if (totales != null)
                {
                    totalSinDto = totales.TotalSinDto;
                    Bi = totales.BI;
                    iva = totales.TotalIva;
                }

                cacheSinDto[$"{factura.Id}"] = totalSinDto;
                cacheBi[$"{factura.Id}"] = Bi;
                cacheBiConIva[$"{factura.Id}"] = Bi + iva;
                cacheDeIva[$"{factura.Id}"] = iva;
                cacheDeIrpf[$"{factura.Id}"] = irpf;
                cacheApagar[$"{factura.Id}"] = (decimal)cacheBiConIva[$"{factura.Id}"] - irpf;
            }

            switch (importeSolicitado)
            {
                case enumImporteEmt.BaseImponible: return (decimal)cacheBi[$"{factura.Id}"];
                case enumImporteEmt.BiConIva: return (decimal)cacheBiConIva[$"{factura.Id}"];
                case enumImporteEmt.Iva: return (decimal)cacheDeIva[$"{factura.Id}"];
                case enumImporteEmt.Irpf: return (decimal)cacheDeIrpf[$"{factura.Id}"];
                case enumImporteEmt.Apagar: return (decimal)cacheApagar[$"{factura.Id}"];
            }
            return (decimal)cacheSinDto[$"{factura.Id}"];
        }

        public static void CargarCacheDeTotales(ContextoSe contexto, List<int> idsFacturas)
        {
            // 1. Obtener cachés de importes
            var cacheSinDto = ServicioDeCaches.Obtener(CacheDe.Fae_BiSinDto);
            var cacheBi = ServicioDeCaches.Obtener(CacheDe.Fae_Bi);
            var cacheBiConIva = ServicioDeCaches.Obtener(CacheDe.Fae_BiConIva);
            var cacheApagar = ServicioDeCaches.Obtener(CacheDe.Fae_Apagar);
            var cacheDeIva = ServicioDeCaches.Obtener(CacheDe.Fae_TotalDeIva);
            var cacheDeIrpf = ServicioDeCaches.Obtener(CacheDe.Fae_TotalDeIrpf);

            // 2. Obtener cachés de Ampliaciones (Importante para el IRPF)
            var cacheAmpConJoin = ServicioDeCaches.Obtener(CacheDe.AmpliacionesConJoin);
            var cacheAmpSinJoin = ServicioDeCaches.Obtener(CacheDe.AmpliacionesSinJoin);

            // 3. Filtrar IDs que no estén procesados
            var idsNuevos = idsFacturas
                .Where(id => !cacheBi.ContainsKey(id.ToString()))
                .Distinct()
                .ToList();

            if (!idsNuevos.Any()) return;

            // --- PASO A: CARGA MASIVA DE AMPLIACIONES (IRPF) ---
            var listaIrpfs = contexto.Set<IrpfEmtDtm>()
                .Include(i => i.TipoIrpf) // Para que el .TipoIrpf?.Detalle no falle
                .Where(i => idsNuevos.Contains(i.IdElemento))
                .ToList();

            foreach (var amp in listaIrpfs)
            {
                var indice = $"{nameof(IrpfEmtDtm)}-{amp.IdElemento}";
                cacheAmpConJoin[indice] = amp;
                cacheAmpSinJoin[indice] = amp;
            }

            // --- PASO B: CARGA MASIVA DE LÍNEAS (BI e IVA) ---
            var listaTotalesLineas = contexto.Set<LineaDeUnaFaeDtm>()
                .Where(l => idsNuevos.Contains(l.IdElemento) && l.TipoDeLinea != enumTipoDeLinea.Comentario)
                .GroupBy(l => l.IdElemento)
                .Select(g => new
                {
                    IdFactura = g.Key,
                    TotalSinDto = g.Sum(l => (l.Precio ?? 0) * (l.Cantidad ?? 0)),
                    BI = g.Sum(l => ((l.Precio ?? 0) * (l.Cantidad ?? 0)) - (((l.Precio ?? 0) * (l.Cantidad ?? 0)) * (l.Descuento ?? 0) / 100)),
                    TotalIva = g.Sum(l => (((l.Precio ?? 0) * (l.Cantidad ?? 0)) - (((l.Precio ?? 0) * (l.Cantidad ?? 0)) * (l.Descuento ?? 0) / 100)) * (l.Iva ?? 0) / 100)
                })
                .ToList();

            // --- PASO C: POBLAR CACHÉS DE IMPORTES ---
            foreach (var id in idsNuevos)
            {
                string key = id.ToString();
                var totalesLinea = listaTotalesLineas.FirstOrDefault(t => t.IdFactura == id);
                var datosIrpf = listaIrpfs.FirstOrDefault(i => i.IdElemento == id);

                decimal bi = totalesLinea?.BI ?? 0;
                decimal sinDto = totalesLinea?.TotalSinDto ?? 0;
                decimal iva = totalesLinea?.TotalIva ?? 0;
                decimal irpfVal = (decimal)(datosIrpf?.Importe ?? 0);

                cacheSinDto[key] = sinDto;
                cacheBi[key] = bi;
                cacheDeIva[key] = iva;
                cacheDeIrpf[key] = irpfVal;

                decimal biConIva = bi + iva;
                cacheBiConIva[key] = biConIva;
                cacheApagar[key] = biConIva - irpfVal;
            }
        }


        public static void VaciarCachesDeImportes(this FacturaEmtDtm factura)
        {
            ServicioDeCaches.EliminarElemento(CacheDe.Fae_BiSinDto, factura.Id.ToString());
            ServicioDeCaches.EliminarElemento(CacheDe.Fae_Bi, factura.Id.ToString());
            ServicioDeCaches.EliminarElemento(CacheDe.Fae_BiConIva, factura.Id.ToString());
            ServicioDeCaches.EliminarElemento(CacheDe.Fae_Apagar, factura.Id.ToString());
            ServicioDeCaches.EliminarElemento(CacheDe.Fae_Cobrado, factura.Id.ToString());
            ServicioDeCaches.EliminarElemento(CacheDe.Fae_TotalDeIva, factura.Id.ToString());
            ServicioDeCaches.EliminarElemento(CacheDe.Fae_TotalDeIrpf, factura.Id.ToString());
            ServicioDeCaches.EliminarElemento(CacheDe.Fae_Abonado, factura.Id.ToString());
            ServicioDeCaches.EliminarElemento(CacheDe.Fae_TotalDeDescuento, factura.Id.ToString());
        }

        public static List<ImportePorTipoDeIrpf> TotalesPorTipoDeIrpf(List<FacturaEmtDtm> facturas, ContextoSe contexto)
        {
            var totalesIrpf = new List<ImportePorTipoDeIrpf>();

            foreach (var factura in facturas)
            {
                if (factura.TienenIrpf(contexto))
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
            }

            return totalesIrpf;
        }

        public static decimal Irpf(this FacturaEmtDtm factura, ContextoSe contexto)
        =>
        factura.TienenIrpf(contexto) ? factura.Irpfs(contexto).Sum(x => x.Importe) : 0;


        public static List<ImportePorTipoDeIrpf> Irpfs(this FacturaEmtDtm factura, ContextoSe contexto)
        {
            var irpfs = new List<ImportePorTipoDeIrpf>();
            var irpf = factura.Ampliacion<IrpfEmtDtm>(contexto, errorSiNoHay: false, aplicarJoin: true);
            if (irpf == null)
                return irpfs;
            irpfs.Add(new ImportePorTipoDeIrpf
            {
                IdIrpf = irpf.IdIrpf.Entero(),
                Tipo = irpf.TipoIrpf?.Detalle ?? contexto.SeleccionarPorId<IrpfDtm>((int)irpf.IdIrpf).Detalle,
                BI = (decimal)irpf.BiSujeta,
                Porcentaje = (decimal)irpf.Irpf,
                Importe = (decimal)irpf.Importe
            });
            return irpfs;
        }

        public static List<ImportePorTipoDeIva> Ivas(this FacturaEmtDtm factura, ContextoSe contexto)
        {
            var ivas = new List<ImportePorTipoDeIva>();
            var lineas = factura.Detalles<LineaDeUnaFaeDtm>(contexto, aplicarJoin: true);
            var direcionFiscal = factura.DireccionFiscal(contexto, erroSiNoHay: false);
            var esExtraComunitario = direcionFiscal == null ? false : direcionFiscal.ExtraComunitaria;
            var esIntraComunitario = direcionFiscal == null ? false : direcionFiscal.IntraComunitaria;
            foreach (var linea in lineas) ivas.TotalizarIvaRepercutido(linea, esExtraComunitario, esIntraComunitario);
            return ivas;
        }


        public static List<BiDelIvaPorNaturaleza> BiDelIvaPorNaturaleza(this FacturaEmtDtm factura, ContextoSe contexto)
        {
            var naturalezas = new List<BiDelIvaPorNaturaleza>();
            var lineas = factura.Detalles<LineaDeUnaFaeDtm>(contexto, aplicarJoin: true);
            foreach (var linea in lineas.Where(l => l.TipoDeLinea != enumTipoDeLinea.Comentario))
            {
                naturalezas.TotalizarNaturalezasIva(contexto, linea);
            }
            return naturalezas;
        }

        private static void TotalizarNaturalezasIva(this List<BiDelIvaPorNaturaleza> naturalezas, ContextoSe contexto, LineaDeUnaFaeDtm linea)
        {
            var naturalezaDtm = contexto.SeleccionarPorId<NaturalezaDtm>(linea.IdNaturaleza.Entero(), aplicarJoin: true);
            var naturaleza = naturalezas.FirstOrDefault(i => i.IdNaturaleza == linea.IdNaturaleza.Entero() && i.idIva == linea.IdIvaR.Entero());
            if (naturaleza == null)
            {
                naturaleza = new BiDelIvaPorNaturaleza
                {
                    IdNaturaleza = linea.IdNaturaleza.Entero(),
                    idIva = linea.IdIvaR.Entero()
                };
                naturalezas.Add(naturaleza);
            }
            naturaleza.Bi += linea.ImporteConDto;
            naturaleza.ImporteDeIva += linea.ImporteDeIva;
            naturaleza.Concepto = naturaleza.Concepto.IsNullOrEmpty()
            ? linea.Concepto
            : naturaleza.Concepto == linea.Concepto
            ? naturaleza.Concepto
            : naturalezaDtm.Expresion;
        }

        public static void CopiarLineasDelPpt(this FacturaEmtDtm factura, ContextoSe contexto)
        =>
        factura.CopiarLineas(contexto, contexto.SeleccionarPorId<PresupuestoDtm>((int)factura.IdPresupuesto));

        public static void FacturarTareas(this FacturaEmtDtm factura, ContextoSe contexto, bool soloTerminadas = true)
        {
            if (factura.IdPresupuesto > 0)
                factura.FacturarTareas(contexto, factura.Presupuesto(contexto).Tareas(contexto).ToList(), soloTerminadas);
            if (factura.IdContrato > 0)
                factura.FacturarTareas(contexto, factura.Contrato(contexto).Tareas(contexto).ToList(), soloTerminadas);
            if (factura.IdParteTr > 0)
                factura.FacturarTareas(contexto, factura.ParteTr(contexto).Tareas(contexto).ToList(), soloTerminadas);
        }

        public static void FacturarTareas(this FacturaEmtDtm factura, ContextoSe contexto, List<TareaDtm> tareas, bool soloTerminadas = true)
        {
            foreach (var tarea in tareas.Where(t => soloTerminadas ? t.Estado(contexto).Terminado : true))
            {
                if (tarea.IdFacturaEmt is not null)
                    continue;

                tarea.IdFacturaEmt = factura.Id;
                tarea.Modificar(contexto);
            }
        }

        public static bool EstaRectificada(this FacturaEmtDtm fae, ContextoSe contexto)
        =>
        fae.RectificadaPor(contexto, errorSiNoHay: false) == null ? false : true;

        public static FacturaEmtDtm RectificadaPor(this FacturaEmtDtm fae, ContextoSe contexto, bool errorSiNoHay = true)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Fae_RectificadaPor);
            if (!cache.ContainsKey(fae.Id.ToString()))
            {
                var datosDeRectificacion = contexto.SeleccionarPorFk<RectificativaEmtDtm>(nameof(RectificativaEmtDtm.IdRectificada), fae.Id, errorSiNoHay, aplicarJoin: true);
                cache[fae.Id.ToString()] = datosDeRectificacion == null ? null : (FacturaEmtDtm)datosDeRectificacion.Elemento(contexto);
            }
            return (FacturaEmtDtm)cache[fae.Id.ToString()];
        }


        public static void CargarCacheDeRectificadaPor(ContextoSe contexto, List<int> idsFacturas)
        {
            var cacheRectificadaPor = ServicioDeCaches.Obtener(CacheDe.Fae_RectificadaPor);

            // 1. Filtramos: solo procesamos los IDs que NO están ya en la caché
            var idsNuevos = idsFacturas
                .Where(id => !cacheRectificadaPor.ContainsKey(id.ToString()))
                .Distinct()
                .ToList();

            if (!idsNuevos.Any()) return;

            // 2. Buscamos en BD solo para los IDs que realmente faltan
            var rectificativas = contexto.Set<RectificativaEmtDtm>()
                .Include(x => x.Elemento) // Traemos la cabecera de la factura rectificadora
                .Where(x => idsNuevos.Contains(x.IdRectificada))
                .ToList();

            // 3. Poblamos la caché con los resultados encontrados
            foreach (var rectificada in rectificativas)
            {
                cacheRectificadaPor[rectificada.IdRectificada.ToString()] = rectificada.Elemento;
            }

            // 4. Marcado de "no tiene": Para los IDs consultados que no devolvieron ninguna rectificativa,
            // guardamos un null. Si no, el método individual RectificadaPor() intentará ir a BD.
            foreach (var id in idsNuevos)
            {
                string key = id.ToString();
                if (!cacheRectificadaPor.ContainsKey(key))
                {
                    cacheRectificadaPor[key] = null;
                }
            }
        }


        public static void CargarCacheDeRectificadaA(ContextoSe contexto, List<int> idsFacturas)
        {
            var cacheRectificadaA = ServicioDeCaches.Obtener(CacheDe.Fae_RectificaA);

            // 1. Filtramos: solo procesamos los IDs que NO están ya en la caché
            var idsNuevos = idsFacturas
                .Where(id => !cacheRectificadaA.ContainsKey(id.ToString()))
                .Distinct()
                .ToList();

            if (!idsNuevos.Any()) return;
            ExtensorDeElementosDeUnProceso.CebarCacheDeIds<FacturaEmtDtm>(contexto, idsNuevos);
            ApiDeDetalles.CebarCacheDeDetalles<RectificativaEmtDtm>(contexto, idsNuevos, leerElemento: true, incluyeExtras: query => query.Include(x => x.Rectificada));

            // 3. Poblamos la caché con los resultados encontrados
            foreach (var idFactura in idsNuevos)
            {
                contexto.SeleccionarPorId<FacturaEmtDtm>(idFactura).RectificaA(contexto, errorSiNoHay: false);
            }
        }


        public static FacturaEmtDtm RectificaA(this FacturaEmtDtm fae, ContextoSe contexto, bool errorSiNoHay = true, bool errorSiHayMasDeUna = true)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Fae_RectificaA);
            if (!cache.ContainsKey(fae.Id.ToString()))
            {
                var datosDeRectificacion = fae.Detalles<RectificativaEmtDtm>(contexto, errorSiNoHay: errorSiNoHay, aplicarJoin: true);
                if (datosDeRectificacion != null && datosDeRectificacion.Count > 1 && errorSiHayMasDeUna)
                    Emitir($"La factura '{fae.Referencia}' rectifica a más de una factura, debe indicar cual de ellas quiere");
                cache[fae.Id.ToString()] = datosDeRectificacion.Count() == 0 ? null : datosDeRectificacion[0].Rectificada;
            }
            return (FacturaEmtDtm)cache[fae.Id.ToString()];
        }


        public static bool EsPeriodica(this FacturaEmtDtm fae, ContextoSe contexto, bool errorSiNoHay = true)
        {
            if (fae.Periodo is not null) return true;
            return UsaLaAmpliacionDe(contexto, fae.IdTipo, typeof(PeriodoEmtDtm));
        }


        public static PeriodoEmtDtm Periodo(this FacturaEmtDtm fae, ContextoSe contexto, bool errorSiNoHay = true)
        {
            if (fae.Periodo is not null) return fae.Periodo;

            if (UsaLaAmpliacionDe(contexto, fae.IdTipo, typeof(PeriodoEmtDtm)))
                return fae.Periodo = fae.Ampliacion<PeriodoEmtDtm>(contexto, errorSiNoHay);
            else
                return null;
        }

        public static bool TienenIrpf(this FacturaEmtDtm fae, ContextoSe contexto, bool aplicarJoin = false)
        =>
        fae.IrpfEmt(contexto, errorSiNoHay: false, aplicarJoin: aplicarJoin) != null;

        public static IrpfEmtDtm IrpfEmt(this FacturaEmtDtm fae, ContextoSe contexto, bool errorSiNoHay = true, bool aplicarJoin = false)
        {
            if (aplicarJoin && fae.IrpfEmt != null && fae.IrpfEmt.TipoIrpf is null)
                fae.IrpfEmt = null;

            if (fae.IrpfEmt is not null) return fae.IrpfEmt;

            if (UsaLaAmpliacionDe(contexto, fae.IdTipo, typeof(IrpfEmtDtm)))
                return fae.IrpfEmt = fae.Ampliacion<IrpfEmtDtm>(contexto, errorSiNoHay, aplicarJoin);
            else
                return null;
        }



        public static FacturaEmtDtm CrearPrefacturaDeUnJson(ContextoSe contexto, FacturadorDeSociedadDtm facturador, string facturaJson)
        {
            var facturaParseada = FacturaEmtJson.Parsear(facturaJson);

            var mapeos = facturador.ParsearMapeos();


            var Cliente = contexto.SeleccionarPorPropiedad<ClienteDtm>(nameof(SociedadDtm.NIF), facturaParseada.NifDelCliente, errorSiNoHay: false);
            if (Cliente == null)
                Emitir($"El cliente indicado '{facturaParseada.NifDelCliente}' no se ha localizado en la BD");

            FacturaEmtDtm factura = new FacturaEmtDtm();
            factura.IdCg = facturador.IdCg;
            factura.IdTipo = facturador.IdTipoDeFactura;
            factura.Nombre = facturaParseada.Nombre;
            factura.Descripcion = facturaParseada.Descripcion;
            factura.IdCliente = Cliente.Id;
            factura.Contacto = facturaParseada.Contacto;
            factura.Telefono = facturaParseada.Telefono;
            factura.eMail = facturaParseada.eMail;
            factura.IdPresupuesto = null;
            factura.IdContrato = null;
            factura.IdParteTr = null;
            factura.Ano = null;
            factura.Serie = null;
            factura.ClaseRectificativa = null;
            factura.MotivoDeRectificacion = null;
            factura = factura.Insertar(contexto);

            factura.CopiarLineas(contexto, facturador, mapeos, facturaParseada);

            if (facturaParseada.IrpfEmt is not null)
            {
                if (!mapeos.Irpfs.Any(x => x.Clave == facturaParseada.IrpfEmt.Irpf))
                    Emitir($"La clave del irpf '{facturaParseada.IrpfEmt.Irpf}' no está contenida en el mapeador del facturador '{facturador.Nombre(contexto)}'");

                int idIrpf = mapeos.Irpfs.First(x => x.Clave == facturaParseada.IrpfEmt.Irpf).Valor;
                var irpf = contexto.SeleccionarPorId<IrpfDtm>(idIrpf, errorSiNoHay: false);

                if (irpf is null)
                {
                    Emitir($"El id asociado a la clave del irpf '{facturaParseada.IrpfEmt.Irpf}' definida en el mapeador del facturador '{facturador.Nombre(contexto)}', no se localiza en la BD");
                }

                new IrpfEmtDtm
                {
                    IdElemento = factura.Id,
                    BiSujeta = facturaParseada.IrpfEmt.BiSujeta,
                    IdIrpf = idIrpf,
                    Irpf = irpf.Porcentaje
                }.Insertar(contexto);
            }

            return factura;
        }



        private static void CopiarLineas(this FacturaEmtDtm factura, ContextoSe contexto, FacturadorDeSociedadDtm facturador, MapeosDelFacturador mapeos, FacturaEmtJson facturaParseada)
        {

            if (facturaParseada.Lineas.Count == 0)
                Emitir($"la {factura.Referencia} no tiene definido el detalle a facturar, defínalo previamente");

            foreach (var linea in facturaParseada.Lineas)
            {

                if (linea.TipoDeLinea == enumTipoDeLinea.Alzada && !linea.Iva.IsNullOrEmpty() && !mapeos.Ivas.Any(x => x.Clave == linea.Iva))
                    Emitir($"La clave del Iva '{linea.Iva}' no está contenida en el mapeador del facturador '{facturador.Nombre(contexto)}'");

                if (linea.TipoDeLinea == enumTipoDeLinea.Alzada && !mapeos.Unidades.Any(x => x.Clave == linea.Unidad))
                    Emitir($"La clave de la unidad '{linea.Unidad}' no está contenida en el mapeador del facturador '{facturador.Nombre(contexto)}'");

                if (linea.TipoDeLinea == enumTipoDeLinea.Alzada && !mapeos.Naturalezas.Any(x => x.Clave == linea.Naturaleza))
                    Emitir($"La clave de la naturaleza '{linea.Naturaleza}' no está contenida en el mapeador del facturador '{facturador.Nombre(contexto)}'");

                int? idIva = linea.TipoDeLinea == enumTipoDeLinea.Comentario ? null : mapeos.Ivas.FirstOrDefault(x => x.Clave == linea.Iva)?.Valor;
                int? idUnidad = linea.TipoDeLinea == enumTipoDeLinea.Comentario ? null : mapeos.Unidades.First(x => x.Clave == linea.Unidad).Valor;
                int? idNaturaleza = linea.TipoDeLinea == enumTipoDeLinea.Comentario ? null : mapeos.Naturalezas.First(x => x.Clave == linea.Naturaleza).Valor;

                new LineaDeUnaFaeDtm
                {
                    IdElemento = factura.Id,
                    Orden = linea.Orden,
                    TipoDeLinea = linea.TipoDeLinea,
                    IdUnitario = null,
                    Concepto = linea.Concepto,
                    Cantidad = linea.Cantidad,
                    Precio = linea.Precio,
                    Anotacion = linea.Anotacion,
                    Descuento = linea.Descuento,
                    IdIvaR = idIva,
                    Clase = linea.Clase,
                    IdUnidad = idUnidad,
                    IdNaturaleza = idNaturaleza,
                    Iva = idIva is null ? null : contexto.SeleccionarPorId<IvaRepercutidoDtm>((int)idIva).Porcentaje,
                }
                .Insertar(contexto);
            }
        }

        public static FacturaEmtDtm CrearPrefactura(this PresupuestoDtm ppt, ContextoSe contexto)
        {
            var idTipoFacturaEmt = ppt.Tipo<TipoDePresupuestoDtm>(contexto).IdTipoFacturaEmt;
            if (idTipoFacturaEmt == null)
                Emitir($"El tipo de presupuesto {ppt.Tipo<TipoDePresupuestoDtm>(contexto).Nombre} no tiene indicado que tipo de factura a de generar");

            var Cliente = contexto.SeleccionarPorFk<ClienteDtm>(nameof(ClienteDtm.IdInterlocutor), ppt.IdSolicitante, errorSiNoHay: false);
            if (Cliente == null)
                Emitir($"Para crear la factura sobre el presupuesto '{ppt.Referencia}' ha de crear el solicitante del presupuesto como cliente");

            FacturaEmtDtm factura = new FacturaEmtDtm();
            factura.IdCg = ppt.IdCg;
            factura.IdTipo = (int)idTipoFacturaEmt;
            factura.Nombre = $"Factura correspondiente al presupuesto {ppt.Referencia}";
            factura.Descripcion = ppt.Descripcion;
            factura.IdCliente = Cliente.Id;
            factura.Contacto = ppt.Contacto;
            factura.Telefono = ppt.Telefono;
            factura.eMail = ppt.eMail;
            factura.IdPresupuesto = ppt.Id;
            factura.IdContrato = null;
            factura.IdParteTr = null;
            factura.Ano = null;
            factura.Serie = null;
            factura.ClaseRectificativa = null;
            factura.MotivoDeRectificacion = null;
            factura = factura.Insertar(contexto);

            factura.CopiarLineas(contexto, ppt);
            return factura;
        }

        public static FacturaEmtDtm CrearPrefactura(this ContratoDtm ctr, ContextoSe contexto, string nombre)
        {
            var idTipoFacturaEmt = ctr.Tipo<TipoDeContratoDtm>(contexto).IdTipoFacturaEmt;
            if (idTipoFacturaEmt == null)
                Emitir($"El tipo de {enumNegocio.Contrato.Singular(true)} '{ctr.Tipo<TipoDeContratoDtm>(contexto).Nombre}' no tiene indicado que tipo de {enumNegocio.FacturaEmitida.Singular(true)} a de generar");

            var datos = ctr.Ampliacion<DatosDelContratoDtm>(contexto);
            var Cliente = contexto.SeleccionarPorId<ClienteDtm>(datos.IdCliente.Entero(), errorSiNoHay: false);
            if (Cliente == null)
                Emitir($"Para crear la {enumNegocio.FacturaEmitida.Singular(true)} sobre el {enumNegocio.Contrato.Singular(true)} '{ctr.Referencia}' ha de indicar el {enumNegocio.Cliente.Singular(true)}");

            FacturaEmtDtm factura = new FacturaEmtDtm();
            factura.IdCg = ctr.IdCg;
            factura.IdTipo = (int)idTipoFacturaEmt;
            factura.Nombre = nombre.IsNullOrEmpty() ? $"{enumNegocio.FacturaEmitida.Singular()} correspondiente al {enumNegocio.Contrato.Singular(true)} '{ctr.Referencia}'" : nombre;
            factura.Descripcion = ctr.Descripcion;
            factura.IdCliente = Cliente.Id;
            factura.Contacto = datos.Contacto;
            factura.Telefono = datos.Telefono;
            factura.eMail = datos.eMail;
            factura.IdPresupuesto = null;
            factura.IdContrato = ctr.Id;
            factura.IdParteTr = null;
            factura.Ano = null;
            factura.Serie = null;
            factura.ClaseRectificativa = null;
            factura.MotivoDeRectificacion = null;
            factura = factura.Insertar(contexto);
            return factura;
        }

        public static FacturaEmtDtm CrearPrefactura(this ParteTrDtm parte, ContextoSe contexto)
        {
            var idTipoFacturaEmt = parte.Tipo<TipoDeParteTrDtm>(contexto).IdTipoFacturaEmt;
            if (idTipoFacturaEmt == null)
                Emitir($"El tipo de parte de trabajo {parte.Tipo<TipoDeParteTrDtm>(contexto).Nombre} no tiene indicado que tipo de factura a de generar");

            //Crear prefactura a partír del pt
            FacturaEmtDtm factura = new FacturaEmtDtm();
            factura.IdCg = parte.IdCg;
            factura.IdTipo = (int)idTipoFacturaEmt;
            factura.Nombre = $"Factura correspondiente al parte {parte.Referencia}";
            factura.Descripcion = parte.Descripcion;
            factura.IdCliente = parte.IdCliente;
            factura.Contacto = parte.Contacto;
            factura.Telefono = parte.Telefono;
            factura.eMail = parte.eMail;
            factura.IdPresupuesto = parte.IdPresupuesto;
            factura.IdContrato = parte.IdContrato;
            factura.IdParteTr = parte.Id;
            factura.Ano = null;
            factura.Serie = null;
            factura.ClaseRectificativa = null;
            factura.MotivoDeRectificacion = null;
            factura = factura.Insertar(contexto);

            var lineas = parte.Detalles<LineaDeUnPtrDtm>(contexto);

            if (lineas.Count == 0)
                Emitir($"El {parte.Referencia} no tiene definido el detalle de lo que se ha de realizar, no se puede genar una factura");

            foreach (var linea in lineas)
            {
                new LineaDeUnaFaeDtm
                {
                    IdElemento = factura.Id,
                    Orden = linea.Orden,
                    TipoDeLinea = linea.TipoDeLinea,
                    IdUnitario = linea.IdUnitario,
                    Concepto = linea.Concepto,
                    Cantidad = linea.Cantidad,
                    Precio = linea.Precio,
                    Anotacion = linea.Anotacion,
                    Descuento = linea.Descuento,
                    IdIvaR = linea.IdIvaR,
                    Iva = linea.Iva,
                    Clase = linea.Clase,
                    IdUnidad = linea.IdUnidad,
                    IdNaturaleza = linea.IdNaturaleza
                }
                .Insertar(contexto);
            }
            return factura;
        }

        public static FacturaEmtDtm CrearPrefactura(this PlanificacionDeVentaDtm plfDeVenta, ContextoSe contexto)
        {
            if (plfDeVenta.IdTipoDeFactura == null)
                Emitir($"La planificación '{plfDeVenta.Referencia}' no tiene indicado que tipo de factura a de generar");

            FacturaEmtDtm factura = new FacturaEmtDtm();
            factura.IdCg = plfDeVenta.IdCg;
            factura.IdTipo = plfDeVenta.IdTipoDeFactura.Entero();
            factura.Nombre = $"Factura correspondiente a la planificación {plfDeVenta.Referencia}";
            factura.Descripcion = plfDeVenta.Descripcion;
            factura.IdCliente = plfDeVenta.IdCliente;
            factura.Contacto = plfDeVenta.Contacto;
            factura.Telefono = plfDeVenta.Telefono;
            factura.eMail = plfDeVenta.eMail;
            factura.IdPresupuesto = null;
            factura.IdParteTr = null;
            factura.IdContrato = plfDeVenta.IdContrato;
            factura.Ano = null;
            factura.Serie = null;
            factura.ClaseRectificativa = null;
            factura.MotivoDeRectificacion = null;
            factura = factura.Insertar(contexto);

            var lineas = plfDeVenta.Detalles<LineaDeUnaPlfVentaDtm>(contexto);

            if (lineas.Count == 0)
                Emitir($"El {plfDeVenta.Referencia} no tiene definido el detalle de lo que se ha de realizar, no se puede genar una factura");

            foreach (var linea in lineas)
            {
                new LineaDeUnaFaeDtm
                {
                    IdElemento = factura.Id,
                    Orden = linea.Orden,
                    TipoDeLinea = linea.TipoDeLinea,
                    IdUnitario = linea.IdUnitario,
                    Concepto = linea.Concepto,
                    Cantidad = linea.Cantidad,
                    Precio = linea.Venta,
                    Anotacion = linea.Anotacion,
                    Descuento = linea.Descuento,
                    IdIvaR = linea.IdIvaR,
                    Iva = linea.Iva,
                    Clase = linea.Clase,
                    IdUnidad = linea.IdUnidad,
                    IdNaturaleza = linea.IdNaturaleza
                }
                .Insertar(contexto);
            }
            factura.CrearTraza(contexto, "Factura creada por planificación", $"Se ha creado la factura al ejecutar la planificación '{plfDeVenta.Expresion}'");
            return factura;
        }

        public static FacturaEmtDtm Copiar(this FacturaEmtDtm faeOrigen, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            if (faeOrigen.EsRectificativa)
                Emitir($"La factura '{faeOrigen.Referencia}' es rectificativa, y por tanto no se puede copiar");

            var transaccion = contexto.IniciarTransaccion();
            try
            {
                var faeNuevo = new FacturaEmtDtm
                {
                    IdCg = contexto.SeleccionarPorId<CentroGestorDtm>(parametros.LeerValor<int>(nameof(CopiarFaeDto.IdCg))).Id,
                    IdCliente = contexto.SeleccionarPorId<ClienteDtm>(parametros.LeerValor<int>(nameof(CopiarFaeDto.IdCliente))).Id,
                    IdTipo = contexto.SeleccionarPorId<TipoDeFacturaEmtDtm>(parametros.LeerValor<int>(nameof(CopiarFaeDto.IdTipo))).Id,
                    Nombre = parametros.LeerValor<string>(nameof(CopiarFaeDto.Nombre)),
                    Descripcion = parametros.LeerValor<string>(nameof(CopiarFaeDto.Descripcion)),
                    IdPresupuesto = faeOrigen.IdPresupuesto,
                    IdContrato = faeOrigen.IdContrato,
                    ClaseRectificativa = null,
                    MotivoDeRectificacion = null,
                    IdArchivadorParaLaReclamacion = null,
                    IdArchivo = null
                }.Insertar(contexto, parametros);

                var lineasFaes = contexto.SeleccionarTodos<LineaDeUnaFaeDtm>(nameof(LineaDeUnaFaeDtm.IdElemento), faeOrigen.Id);
                foreach (var linea in lineasFaes)
                {
                    linea.Id = 0;
                    linea.IdElemento = faeNuevo.Id;
                    linea.Insertar(contexto);
                }

                var irpfEmt = faeOrigen.Ampliacion<IrpfEmtDtm>(contexto, errorSiNoHay: false);
                if (irpfEmt != null)
                {
                    irpfEmt.IdElemento = faeNuevo.Id;
                    irpfEmt.InsertarComoAdministrador(contexto);
                }

                contexto.Commit(transaccion);
                return faeNuevo;
            }
            catch
            {
                contexto.Rollback(transaccion);
                throw;
            }
        }

        public static FacturaEmtDtm CrearRectificativa(this FacturaEmtDtm aRectificar, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            //if (!aRectificar.EstaEnLaEtapa(enumEtapasDeFacturasEmt.FAE_Etapa_Emitida))
            //    Emitir($"Para rectificar la factura '{(aRectificar.NumeroDeFactura.IsNullOrEmpty() ? aRectificar.Referencia : aRectificar.NumeroDeFactura)}' ha de estar en la etapa de '{enumEtapasDeFacturasEmt.FAE_Etapa_Emitida.Nombre(true)}'");

            if (aRectificar.EstaRectificada(contexto))
                Emitir($"no puede rectificar la factura '{aRectificar.Referencia}' por estar ya rectificada por '{aRectificar.RectificadaPor(contexto).Referencia}'");

            var clase = ApiDeEnsamblados.ToEnumerado<enumClaseDeRectificativa>(parametros.LeerValor<string>(nameof(HacerRectificativaDto.ClaseRectificativa)));
            var motivo = ApiDeEnsamblados.ToEnumerado<enumMotivoDeRectificacion>(parametros.LeerValor<string>(nameof(HacerRectificativaDto.Motivo)));
            var motivoDeRectificacion = parametros.LeerValor(nameof(HacerRectificativaDto.MotivoDeRectificacion), motivo.Descripcion());
            parametros[ltrDeFacturaRectificada.Rectificada] = aRectificar;

            if (motivo != enumMotivoDeRectificacion.PorImportes && clase == enumClaseDeRectificativa.OC)
                Emitir($"no puede rectificar la factura '{aRectificar.Referencia}' " +
                    $"usando la clase '{enumClaseDeRectificativa.OC.Descripcion()}' " +
                    $"si el motivo es por '{motivo.Descripcion()}'");

            if (clase == enumClaseDeRectificativa.OC && !VariableDeFacturasEmt.PermiteRectificativasParciales)
            {
                Emitir($"no puede rectificar la factura '{aRectificar.Referencia}' " +
                    $"usando la clase '{enumClaseDeRectificativa.OC.Descripcion()}' " +
                    $"no está permitido, modifique el parámetro '{enumParametrosDeFacturasEmt.FAE_PermiteRectificativasParciales}'");
            }


            if (aRectificar.TienenIrpf(contexto) && clase != enumClaseDeRectificativa.OR)
                Emitir($"no puede rectificar la factura '{aRectificar.Referencia}' usando la clase '{clase.Descripcion()}' ya que tiene Irpf, debe usar la clase '{enumClaseDeRectificativa.OR.Descripcion()}'");

            var transaccion = contexto.IniciarTransaccion();
            try
            {
                var rectificativa = new FacturaEmtDtm
                {
                    IdCg = aRectificar.IdCg,
                    IdCliente = aRectificar.IdCliente,
                    IdTipo = aRectificar.IdTipo,
                    Nombre = $"Rectifica a: {aRectificar.NumeroDeFactura}. Emitida el: {((DateTime)aRectificar.FacturadaEl):dd-MM-yyyy}. Importe: {aRectificar.Total(contexto, enumImporteEmt.Apagar).ToMoneda()}",
                    Descripcion = $"Se rectifica la factura por: {motivoDeRectificacion}",
                    ClaseRectificativa = clase,
                    MotivoDeRectificacion = motivo,
                    Serie = ltrDeFacturaRectificada.Serie,
                    IdPresupuesto = aRectificar.IdPresupuesto,
                    IdContrato = aRectificar.IdContrato
                }.Insertar(contexto, parametros);

                var lineasFaes = contexto.SeleccionarTodos<LineaDeUnaFaeDtm>(nameof(LineaDeUnaFaeDtm.IdElemento), aRectificar.Id);
                foreach (var linea in lineasFaes)
                {
                    linea.Id = 0;
                    linea.IdElemento = rectificativa.Id;
                    linea.Cantidad = (-1) * linea.Cantidad;
                    linea.Insertar(contexto);
                }

                var datosDeRectificacion = new RectificativaEmtDtm
                {
                    IdElemento = rectificativa.Id,
                    IdRectificada = aRectificar.Id,
                    Concepto = rectificativa.Nombre
                }.Insertar(contexto);

                var periodo = aRectificar.Periodo(contexto, errorSiNoHay: false);
                if (periodo is not null)
                {
                    var periodoDeLaRectificativa = rectificativa.Periodo(contexto);
                    periodoDeLaRectificativa.Inicio = periodo.Inicio;
                    periodoDeLaRectificativa.Fin = periodo.Fin;
                    periodoDeLaRectificativa.Notacion = periodo.Notacion;
                    periodoDeLaRectificativa.Modificar(contexto);
                }

                if (aRectificar.TienenIrpf(contexto))
                {
                    var irpf = aRectificar.IrpfEmt(contexto);
                    irpf.Id = 0;
                    irpf.IdElemento = rectificativa.Id;
                    irpf.BiSujeta = (-1) * irpf.BiSujeta;
                    irpf.InsertarComoAdministrador(contexto);
                }

                contexto.Commit(transaccion);
                return rectificativa;
            }
            catch
            {
                contexto.Rollback(transaccion);
                throw;
            }
        }

        public static FacturaEmtDtm AntesDeAnular(this FacturaEmtDtm factura, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            var transicionesPosibles = TransicionAplicable.Transiciones(VariableDePartesTr.TransicionesPorMotivo, VariableDePartesTr.enumMotivoTransicion.AnularPrefactura, errorSiNoHay: false);

            //Si está enganchada a un parte, retrocedemos el parte
            if (factura.IdParteTr != null)
            {
                if (transicionesPosibles.Count == 0)
                {
                    FaltaDefinirParametro(factura, contexto);
                }

                contexto.SeleccionarPorId<ParteTrDtm>((int)factura.IdParteTr).IntentarAplicarTransicion(contexto, transicionesPosibles
                    , new Dictionary<string, object> { { ltrDeUnaFacturaEmt.FacturaDelParte, factura } }
                    , errorSiNoSeAplica: false);
                factura.IdParteTr = null;
                factura.ParteTr = null;
            }

            var lineas = factura.Detalles<LineaDeUnaFaeDtm>(contexto, errorSiNoHay: false);
            foreach (var linea in lineas)
            {
                if (linea.IdParteTr != null)
                {
                    if (transicionesPosibles.Count == 0)
                    {
                        FaltaDefinirParametro(factura, contexto);
                    }

                    contexto.SeleccionarPorId<ParteTrDtm>((int)linea.IdParteTr).IntentarAplicarTransicion(contexto, transicionesPosibles
                        , new Dictionary<string, object> { { ltrDeUnaFacturaEmt.FacturaDelParte, factura } }
                        , errorSiNoSeAplica: false);
                    linea.IdParteTr = null;
                    linea.Modificar(contexto, esUnaAccion: true);
                }

            }

            //si tiene tareas facturadas, las excluimos
            var tareas = factura.Tareas(contexto);
            foreach (var tarea in tareas)
            {
                tarea.IdFacturaEmt = null;
                tarea.Modificar(contexto, accionEjecutada: ltrDeUnaTarea.Accion_ExluirDeLaFactura);
                tarea.CrearTraza(contexto, $"Cancelación de facturación", $"El usuario '{contexto.DatosDeConexion.Login}' ha anulado la prefactura '{factura.Referencia}' asociada a la tarea '{tarea.Referencia}'");
            }

            //Si está asociada a un ppt, lo desengachamos
            if (factura.IdPresupuesto != null)
            {
                var ppt = contexto.SeleccionarPorId<PresupuestoDtm>((int)factura.IdPresupuesto);
                ppt.CrearTraza(contexto, "Cancelación de facturación", $"El usuario '{contexto.DatosDeConexion.Login}' ha anulado la prefactura {factura.Referencia} asociada al presupuesto '{ppt.Referencia}'");
                factura.IdPresupuesto = null;
                factura.Presupuesto = null;
            }

            //anulamos la rectificación
            if (factura.EsRectificativa)
            {
                var rectificadas = factura.Detalles<RectificativaEmtDtm>(contexto, aplicarJoin: true);
                foreach (var rectificada in rectificadas)
                    rectificada.Eliminar(contexto);
            }

            //si está enganchada a un contrato, lo desengachamos

            return factura;
        }

        private static void FaltaDefinirParametro(FacturaEmtDtm factura, ContextoSe contexto)
        {
            var faltaParametrizacion = $"No se puede anular la prefactura '{factura.Referencia}' " +
                    $"porque está asociada al parte de trabajo '{factura.ParteTr(contexto).Referencia}' " +
                    $"y no existen transiciones definidas para el motivo '{VariableDePartesTr.enumMotivoTransicion.AnularPrefactura}";
            Emitir(VariableDePartesTr.FaltaDefinirParametro(faltaParametrizacion, enumParametrosDePartes.PTR_Aplicar_Transicion));
        }

        private static List<TareaDtm> Tareas(this FacturaEmtDtm factura, ContextoSe contexto)
        =>
        contexto.SeleccionarTodos<TareaDtm>(nameof(TareaDtm.IdFacturaEmt), factura.Id, parametros: new Dictionary<string, object> { { ltrParametrosNeg.ValidarPermisosDeConsulta, false } });

        public static void AntesDeEmitirFactura(this FacturaEmtDtm factura, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            var lineas = factura.Detalles<LineaDeUnaFaeDtm>(contexto, errorSiNoHay: false, aplicarJoin: true);
            if (lineas.Count == 0) Emitir($"No puede emitir la factura '{factura.Referencia}', debe definir las líneas de lo facturado");

            factura.ValidarIva(lineas);
            factura.ValidarIrpf(contexto);
            factura.ValidarRequisitosSii(contexto);
            factura.ValidarDireccionFiscal(contexto);

            if (factura.TieneCentroAdministrativo(contexto) && !factura.CentroAdministrativo(contexto).Activa)
            {
                Emitir($"El centro administrativo '{factura.CentroAdministrativo(contexto).Expresion}' seleccionado no está activo");
            }

            var tipoFactura = factura.Tipo<TipoDeFacturaEmtDtm>(contexto);
            if (tipoFactura.ConPeriodoEmt)
            {
                var periodo = factura.Ampliacion<PeriodoEmtDtm>(contexto);
                if (periodo.Inicio is null || periodo.Fin is null)
                    Emitir($"Para el tipo '{tipoFactura.Nombre}' se ha indicado que es obligatorio indicar el periodo de facturación");
            }

            var tareas = factura.Tareas(contexto);
            foreach (var tarea in tareas)
                if (!tarea.Estado(contexto).Terminado)
                    Emitir($"No puede emitir la factura '{factura.Referencia}' porque la tarea asociada '{tarea.Referencia}' no está terminada");

            factura.EmitidaEl = DateTime.Now;
            factura.Serie = factura.EsRectificativa ? ltrDeFacturaRectificada.Serie : tipoFactura.Serie;
            if (factura.FacturadaEl is not null)
            {
                factura.Ano = factura.FacturadaEl.Fecha().Year;
            }
            else
            {
                factura.Ano = DateTime.Now.Year;
                factura.FacturadaEl = factura.EmitidaEl;
            }
            var numero = factura.Sociedad(contexto).UltimoNumeroDeLaSerie(contexto, factura.Ano.Entero(), factura.Serie);

            factura.Numero = numero == default(int?) ? 1 : numero + 1;
            factura.ValidarFechaFactura(contexto);
            factura = factura.CalcularVencimiento(contexto);
            factura.Huella = factura.CalcularHuella(contexto);
            factura.ValidarClaseDeEmision(contexto);
            if (factura.EsRectificativa) ValidarRectificativa(factura, contexto);

            factura = factura.Preasentar(contexto);

            factura.Firma = factura.Firmar(contexto);

            parametros[ltrParametrosNeg.AccionQueSeEjecuta] = ltrDeUnaFacturaEmt.Accion_EmitirFactura;
        }

        private static void ValidarDireccionFiscal(this FacturaEmtDtm factura, ContextoSe contexto)
        {
            var tipoFactura = factura.Tipo<TipoDeFacturaEmtDtm>(contexto);
            var direccion = factura.DireccionFiscal(contexto);
            if (direccion.EsNacional && tipoFactura.EsExportacion)
                Emitir($"No puede emitir la factura '{factura.Referencia}' ya que el tipo '{tipoFactura.Nombre}' está declarado como de exportación y se está facturando a una dirección fiscal nacional");

            var lineas = factura.Detalles<LineaDeUnaFaeDtm>(contexto, errorSiNoHay: false, aplicarJoin: true);
            if (tipoFactura.EsExportacion && direccion.IntraComunitaria)
            {
                if (factura.Cliente.VAT.IsNullOrEmpty())
                    Emitir($"No puede emitir la factura '{factura.Referencia}' ya que el cliente debe tener un VAT asignado");
                //validar con una llamada http que el vat es válido
                //todo: https://ec.europa.eu/taxation_customs/vies/#/technical-information
                factura.ValidarIvaIntraComunitario(lineas);
            }
            if (direccion.ExtraComunitaria) factura.ValidarIvaNoSujeto(lineas);
            if (factura.TieneIvaExento(lineas) && factura.InformacionFiscal.IsNullOrEmpty())
                factura.InformacionFiscal = factura.MotivosDeExeccion(lineas);
        }

        public static void ValidarClaseDeEmision(this FacturaEmtDtm factura, ContextoSe contexto, string errorSiNoValida = null)
        =>
        factura.UsaVerifactu(contexto, errorSiNoValida);

        private static void ValidarRectificativa(FacturaEmtDtm rectificativa, ContextoSe contexto)
        {
            var datosDeLasRectificadas = rectificativa.Detalles<RectificativaEmtDtm>(contexto, aplicarJoin: true, usarLaCache: false);
            if (datosDeLasRectificadas.Count == 0)
                Emitir($"No puede emitir la factura '{rectificativa.Referencia}' porque es una rectificativa, y no rectifica a ninguna");

            foreach (var datos in datosDeLasRectificadas)
            {
                ValidarImportesDeLaRectificativa(contexto, datos);
            }

            if (rectificativa.ClaseRectificativa == enumClaseDeRectificativa.OC)
                return;

            foreach (var datos in datosDeLasRectificadas)
            {
                var rectificada = datos.Rectificada(contexto);
                if (!rectificada.EstaEnLaEtapa(enumEtapasDeFacturasEmt.FAE_Etapa_Emitida))
                    continue;
                rectificada.Transitar(contexto, VariableDeFacturasEmt.TransicionesPorMotivo,
                    VariableDeFacturasEmt.enumMotivoTransicion.RectificarFactura,
                    parametros: new Dictionary<string, object>
                    {
                            {ltrParametrosNeg.AccionQueSeEjecuta, VariableDeFacturasEmt.enumMotivoTransicion.RectificarFactura.Descripcion() }
                    });
            }
        }

        private static void ValidarImportesDeLaRectificativa(ContextoSe contexto, RectificativaEmtDtm datos)
        {
            var rectificativa = datos.Rectificativa(contexto);
            var rectificada = datos.Rectificada(contexto);

            if (rectificada.TienenIrpf(contexto) != rectificativa.TienenIrpf(contexto))
            {
                Emitir($"No puede emitir la rectificativa '{rectificativa.Referencia}' porque la rectificada '{rectificada.Referencia}' {(rectificada.TienenIrpf(contexto) ? "tiene" : "no tiene")} irpf");
            }

            if (rectificativa.ClaseRectificativa == enumClaseDeRectificativa.OR && rectificada.TotalDeIrpf(contexto) + rectificada.TotalDeIrpf(contexto) != 0)
            {
                Emitir($"No puede emitir la rectificativa '{rectificativa.Referencia}' porque la rectificada '{rectificada.Referencia}' tiene distinto irpf '{rectificada.TotalDeIrpf(contexto).ToMoneda()}' a la rectificativa '{rectificativa.TotalDeIrpf(contexto).ToMoneda()}'");
            }


            if (rectificativa.MotivoDeRectificacion == enumMotivoDeRectificacion.DatosErroneos)
            {
                if (rectificativa.Total(contexto, enumImporteEmt.Apagar) != -1 * rectificada.Total(contexto, enumImporteEmt.Apagar))
                    Emitir($"No puede emitir la factura '{rectificativa.Referencia}' porque el motivo es " +
                        $"por '{enumMotivoDeRectificacion.DatosErroneos.Descripcion()}' y el importe de la rectificada '{rectificada.Referencia}' " +
                        $"es de '{rectificada.Total(contexto, enumImporteEmt.Apagar).ToMoneda()}' " +
                        $"y el de la rectificativa es de '{rectificativa.Total(contexto, enumImporteEmt.Apagar).ToMoneda()}' " +
                        $"y debe ser el mismo pero de importe con signo contrario");
            }
            else
                if (rectificativa.MotivoDeRectificacion == enumMotivoDeRectificacion.PorImpago)
                {
                    if (rectificativa.Total(contexto, enumImporteEmt.Apagar) != -1 * rectificada.Total(contexto, enumImporteEmt.Apagar))
                        Emitir($"No puede emitir la factura '{rectificativa.Referencia}' porque el motivo es " +
                            $"por '{enumMotivoDeRectificacion.PorImpago.Descripcion()}' y el importe de la rectificada '{rectificada.Referencia}' " +
                            $"es de '{rectificada.Total(contexto, enumImporteEmt.Apagar).ToMoneda()}' " +
                            $"y el de la rectificativa es de '{rectificativa.Total(contexto, enumImporteEmt.Apagar).ToMoneda()}' " +
                            $"y debe ser el mismo pero de importe con signo contrario");
                }
                else
                    if (rectificativa.MotivoDeRectificacion == enumMotivoDeRectificacion.PorImportes && rectificativa.ClaseRectificativa == enumClaseDeRectificativa.OR)
                    {
                        if (rectificativa.Total(contexto, enumImporteEmt.Apagar) != -1 * rectificada.Total(contexto, enumImporteEmt.Apagar))
                            Emitir($"No puede emitir la factura '{rectificativa.Referencia}' con el motivo " +
                                $"'{enumMotivoDeRectificacion.PorImportes.Descripcion()}' y clase '{enumClaseDeRectificativa.OR.Descripcion()}' ya que el importe " +
                                $"ha de ser el de la rectificada '{rectificada.Total(contexto, enumImporteEmt.Apagar).ToMoneda()}' pero de signo contrario");
                    }
                    else
                        if (rectificativa.MotivoDeRectificacion == enumMotivoDeRectificacion.PorImportes && rectificativa.ClaseRectificativa == enumClaseDeRectificativa.OC)
                        {
                            if (rectificativa.Total(contexto, enumImporteEmt.Apagar) + rectificada.Total(contexto, enumImporteEmt.Apagar) <= 0)
                                Emitir($"No puede emitir la factura '{rectificativa.Referencia}' con el motivo " +
                                    $"'{enumMotivoDeRectificacion.PorImportes.Descripcion()}' y clase '{enumClaseDeRectificativa.OC.Descripcion()}' ya que la suma de los importes  " +
                                    $"'{rectificada.Total(contexto, enumImporteEmt.Apagar).ToMoneda()}' y " +
                                    $"'{rectificativa.Total(contexto, enumImporteEmt.Apagar).ToMoneda()}' " +
                                    $"es menor de cero");
                        }
        }

        public static FacturaEmtDtm Rectificativa(this RectificativaEmtDtm rectificacion, ContextoSe contexto)
        {
            if (rectificacion.Elemento is null)
                rectificacion.Elemento = contexto.SeleccionarPorId<FacturaEmtDtm>(rectificacion.IdElemento);
            return rectificacion.Elemento;
        }

        public static FacturaEmtDtm Rectificada(this RectificativaEmtDtm rectificacion, ContextoSe contexto)
        {
            if (rectificacion.Rectificada is null)
                rectificacion.Rectificada = contexto.SeleccionarPorId<FacturaEmtDtm>(rectificacion.IdRectificada);
            return rectificacion.Rectificada;
        }

        public static void ValidarIrpf(this FacturaEmtDtm factura, ContextoSe contexto)
        {
            if (!factura.TienenIrpf(contexto))
            {
                if (factura.DebeTenerIrpf(contexto))
                    Emitir($"la factura '{factura.Referencia}' debe tener irpf indicado");
                return;
            }

            if (factura.IrpfEmt.BiSujeta == null)
                Emitir($"la factura '{factura.Referencia}' no tiene indicado la B.I. del IRPF, indíquela");

            var biSujeto = Math.Abs((decimal)factura.IrpfEmt.BiSujeta);
            if (biSujeto == 0)
                Emitir($"la factura '{factura.Referencia}' tiene B.I. para el IRPF 0, no está permitido");

            if (factura.IrpfEmt.IdIrpf == null)
                Emitir($"la factura '{factura.Referencia}' no tiene indicado el tipo de IRPF, indíquelo");

            var bi = Math.Abs(factura.Bi(contexto));

            if (biSujeto > bi)
                Emitir($"la factura '{factura.Referencia}' tiene una B.I. '{bi.ToMoneda()}' menor que la del irpf '{biSujeto.ToMoneda()}'");

            if (factura.EsRectificativa)
            {
                var irpfDeLaRectificada = factura.RectificaA(contexto).TotalDeIrpf(contexto);
                var irpfDeLaRectificativa = factura.TotalDeIrpf(contexto);
                if (irpfDeLaRectificada + irpfDeLaRectificativa != 0)
                    Emitir($"la factura '{factura.Referencia}' tiene irpf de '{irpfDeLaRectificativa.ToMoneda()}' y debe ser igual al de la rectificada '{irpfDeLaRectificada.ToMoneda()}'");
            }
        }

        public static bool DebeTenerIrpf(this FacturaEmtDtm factura, ContextoSe contexto)
        {
            var sociedad = factura.Sociedad(contexto);
            if (!sociedad.Autonomo)
                return false;

            if (factura.Cliente(contexto).Interlocutor(contexto).EsSociedad)
            {
                return (bool)sociedad.FacturarConIrpf();
            }

            return false;
        }

        public static void ValidarIrpf(this FacturaEmtDtm factura, ContextoSe contexto, decimal biSujeto)
        {
            var bi = Math.Abs(factura.Bi(contexto));

            if (biSujeto > bi)
                Emitir($"la factura '{factura.Referencia}' tiene una B.I. '{bi.ToMoneda()}' menor que la del irpf '{biSujeto.ToMoneda()}'");

        }

        private static void ValidarFechaFactura(this FacturaEmtDtm factura, ContextoSe contexto)
        {
            var ultimaFactura = factura.Anterior(contexto);
            if (ultimaFactura != null && ultimaFactura.FacturadaEl > factura.FacturadaEl)
            {
                if (ultimaFactura.FacturadaEl.Fecha().Year - 1 == factura.FacturadaEl.Fecha().Year)
                {
                    var serie = factura.Tipo(contexto).SerieAnoAnterior(contexto);
                    var ultimaDelAno = factura.UltimaEmitidaDelAno(contexto, serie);
                    if (ultimaDelAno != null && ultimaDelAno.FacturadaEl > factura.FacturadaEl)
                    {
                        Emitir($"No puede emitir la factura '{factura.Referencia}' porque la última factura emitidad '{ultimaDelAno.NumeroDeFactura}' fue con fecha '{ultimaDelAno.FacturadaEl.Fecha().ToString("dd-MM-yyyy")}'");
                    }
                    factura.Serie = serie;
                    var numero = factura.Sociedad(contexto).UltimoNumeroDeLaSerie(contexto, ultimaFactura.FacturadaEl.Fecha().Year - 1, serie);
                    factura.Numero = numero == default(int?) ? 1 : numero + 1;
                }
                else
                    Emitir($"No puede emitir la factura '{factura.Referencia}' porque la última factura emitidad '{ultimaFactura.NumeroDeFactura}' fue con fecha '{ultimaFactura.FacturadaEl.Fecha().ToString("dd-MM-yyyy")}'");
            }
        }

        public static FacturaEmtDtm TrasEmitirFactura(this FacturaEmtDtm factura, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            if (factura.IdParteTr is not null && factura.IdContrato is not null)
            {
                var contrato = factura.Contrato(contexto);
                var parte = factura.ParteTr(contexto);
                contrato.RecalcularAvance(contexto, enumAvaceOperacion.FacturarRealizado, factura.Bi(contexto), parte.Total(contexto, conIva: false));
            }
            if (factura.IdParteTr is null && factura.IdContrato is not null)
            {
                var planificacion = factura.Planificacion(contexto);
                var contrato = contexto.SeleccionarPorId<ContratoDtm>(factura.IdContrato.Entero());
                if (planificacion == null)
                    contrato.RecalcularAvance(contexto, enumAvaceOperacion.FacturarContrato, factura.Bi(contexto), 0);
                else
                    contrato.RecalcularAvance(contexto, enumAvaceOperacion.FacturarPlanificado, factura.Bi(contexto), planificacion.Total(contexto, conIva: false));
            }
            if (!factura.EsRectificativa)
            {
                factura.DarPorFacturadoLosPartes(contexto);
                factura.AnotarEventoDeVencimiento(contexto);
            }

            if (factura.EsRectificativa)
            {
                var rectificada = factura.RectificaA(contexto);
                if (rectificada.APagar(contexto) == factura.APagar(contexto))
                    rectificada.EliminarEventoDeVencimiento(contexto);
            }
            return factura;
        }

        public static void CalcularSaldosContrato(this FacturaEmtDtm factura, ContextoSe contexto, ContratoDtm contrato)
        {

            var planificacion = factura.Planificacion(contexto);
            if (planificacion == null)
                contrato.RecalcularAvance(contexto, enumAvaceOperacion.FacturarContrato, factura.Bi(contexto), 0);
            else
                contrato.RecalcularAvance(contexto, enumAvaceOperacion.FacturarPlanificado, factura.Bi(contexto), planificacion.Total(contexto, conIva: false));
        }


        public static void AntesDeRectificar(this FacturaEmtDtm factura, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            factura.VenceEl = null;
            factura.EliminarEventoDeVencimiento(contexto);
        }

        public static void AntesDeAbonarUnaRectificativa(this FacturaEmtDtm factura, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            if (!factura.EstaAbonada(contexto, errorSiNoEsRectificativa: true))
                Emitir($"La factura '{factura.Referencia}' no se puede dar por abonada ya que tiene pendiente de devolver '{factura.PendientePorAbonar(contexto).ToMoneda()}'");

            factura.EliminarEventoDeVencimiento(contexto);
        }

        public static void AntesDeDevolverAPrefactura(this FacturaEmtDtm factura, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            var numero = factura.Sociedad(contexto).UltimoNumeroDeLaSerie(contexto, DateTime.Now.Year, factura.Serie);
            if (factura.Numero != numero.Entero())
                Emitir($"No se puede retroceder la factura '{factura.Referencia}' por haber facturas posteriores");

            if (factura.EsRectificativa)
                Emitir($"No se puede retroceder la factura '{factura.Referencia}' por ser rectificativa de la '{factura.RectificaA(contexto).Referencia}'");

            if (factura.ParteTr(contexto) is not null)
                Emitir($"No se puede retroceder la factura '{factura.Referencia}' por ser la factura del {enumNegocio.ParteDeTrabajo.Singular(true)} '{factura.ParteTr(contexto).Referencia}'");

            foreach (var linea in factura.Detalles<LineaDeUnaFaeDtm>(contexto))
            {
                if (linea.IdParteTr is not null)
                {
                    var parte = linea.ParteTr(contexto);
                    Emitir($"No se puede retroceder la factura '{factura.Referencia}' ya que en una de sus líneas se ha facturado el {enumNegocio.ParteDeTrabajo.Singular(true)} '{parte.Referencia}'");
                }
            }

            if (factura.Detalles<CobroDeFaeDtm>(contexto).Any())
            {
                Emitir($"La factura '{factura.Referencia}' tiene cobros realizados, eliminelos primero'");
            }

            var log = contexto.SeleccionarPorPropiedad<LogDeEnvioDeFacturaDtm>(nameof(LogDeEnvioDeFacturaDtm.IdFactura), factura.Id, errorSiNoHay: false);
            if (log is not null)
            {
                if (log.EnviadaEl is null)
                    Emitir($"La factura '{factura.Referencia}' no puede ser anulada, espere a que termine el proceso de comunicación a la AEAT'");
                Emitir($"La factura '{factura.Referencia}' no puede ser anulada, ya se ha comunicado a la AEAT'");
            }

            factura.EliminarEventoDeVencimiento(contexto);
            parametros[ltrDeUnaFacturaEmt.IdArchivoFactura] = factura.IdArchivo.Entero();
            factura.IdArchivo = null;
            factura.InicializarPrefactura();
            factura.AnularPreasiento(contexto, parametros);
            factura.RegistrarErrorDePeticion(contexto, ltrFacturador.ErrorAlComunicarALaAEAT);
        }


        public static void AnularVerifactu(this FacturaEmtDtm factura, ContextoSe contexto)
        {
            var log = contexto.SeleccionarPorPropiedad<LogDeEnvioDeFacturaDtm>(nameof(LogDeEnvioDeFacturaDtm.IdFactura), factura.Id, errorSiNoHay: false);
            if (log is not null)
                log.EliminarComoAdministrador(contexto);

            var verifactu = factura.Verifactu(contexto, errorSiNoHay: false);
            if (verifactu != null)
            {
                verifactu.EliminarComoAdministrador(contexto);
                factura.Verifactu = null;
            }
        }

        public static bool EstaComunicandose(this FacturaEmtDtm factura, ContextoSe contexto)
        {
            var log = contexto.SeleccionarPorPropiedad<LogDeEnvioDeFacturaDtm>(nameof(LogDeEnvioDeFacturaDtm.IdFactura), factura.Id, errorSiNoHay: false);
            if (log is null)
                return false;

            return log.EnviadaEl is null;
        }

        public static void AntesDeReclamar(this FacturaEmtDtm factura, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            if (factura.IdArchivadorParaLaReclamacion is null)
                Emitir($"La {enumNegocio.FacturaEmitida.Singular(true)} '{factura.Referencia}' no tiene archivador de reclamación");

            if (!factura.ArchivadorParaLaReclamacion(contexto).TieneArchivos(contexto))
                Emitir($"La {enumNegocio.FacturaEmitida.Singular(true)} '{factura.Referencia}' no tiene archivos en el {enumNegocio.Archivador.Singular(true)} '{factura.ArchivadorParaLaReclamacion(contexto).Referencia}' del tipo '{factura.ArchivadorParaLaReclamacion(contexto).Tipo<TipoDeArchivadorDtm>(contexto).Nombre}'");
        }

        public static FacturaEmtDtm DespuesDeDevolverAPrefactura(this FacturaEmtDtm factura, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            var idArchivo = parametros.LeerValor<int>(ltrDeUnaFacturaEmt.IdArchivoFactura);
            var firmado = contexto.Set<FirmadoDtm>().Where(x => x.IdOriginal == idArchivo).FirstOrDefault();
            if (firmado != null)
            {
                contexto.EliminarPorId<FirmadoDtm>(firmado.Id);
                factura.QuitarAnexado(contexto, firmado.IdFirmado, validarPersistencia: false);
                contexto.EliminarPorId<ArchivoDtm>(firmado.IdFirmado);
            }

            if (idArchivo > 0) factura.QuitarAnexado(contexto, idArchivo);
            factura.CrearTraza(contexto, "Factura retrocedida para corrección", $"Se ha retrocedido la factura por el usuario '{contexto.DatosDeConexion.Login}'");
            ServicioDeCaches.EliminarCache(CacheDe.Fae_Anterior);
            return factura;
        }

        public static FacturaEmtDtm DespuesDePasaAReclamacion(this FacturaEmtDtm factura, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            if (factura.IdArchivadorParaLaReclamacion is not null)
                return factura;
            var variable = enumNegocio.FacturaEmitida.LeerParametro(contexto, enumParametrosDeFacturasEmt.FAE_TipoArchivadorDeReclamacion);
            var tipo = contexto.SeleccionarPorId<TipoDeArchivadorDtm>(variable.Valor.Entero());
            factura.IdArchivadorParaLaReclamacion = factura.CrearArchivador(contexto, tipo.Id, $"Archivador de reclamación para la {enumNegocio.FacturaEmitida.Singular(true)} '{factura.Referencia}'").Id;
            parametros.Add(ltrParametrosNeg.AccionQueSeEjecuta, ltrDeUnaFacturaEmt.Accion_CrearArchivadorDeReclamacion);
            factura = factura.Modificar(contexto, parametros: parametros, esUnaAccion: true);
            return factura;
        }

        public static int IncluirParteTr(ContextoSe contexto, int idFactura, SelectorDto parteTrSeleccionado, Dictionary<string, object> parametros)
        {
            var parteTr = contexto.SeleccionarPorId<ParteTrDtm>(parteTrSeleccionado.IdElemento);
            parteTr.IncluirParteEnFactura(contexto, idFactura, parametros);
            return idFactura;
        }

        public static int IncluirParteEnFactura(this ParteTrDtm parteTr, ContextoSe contexto, int idFactura, Dictionary<string, object> parametros)
        {
            var importe = parteTr.Total(contexto, conIva: false);
            var factura = contexto.SeleccionarPorId<FacturaEmtDtm>(idFactura, aplicarJoin: true);

            if (factura.IdContrato != null && parteTr.IdContrato != null && factura.IdContrato != parteTr.IdContrato)
                Emitir($"No puede incluir el parte {parteTr.Referencia} en la factura {factura.Referencia} ya que pertenecen a contratos diferentes");

            if (factura.Tipo.IdIvaRDefecto == default(int))
                Emitir($"No se puede facturar el parte de trabajo porque el tipo de factura '{factura.Tipo.Nombre}' no informa del Iva por defecto a usar");

            if (factura.Tipo.ClaseDefecto == null)
                Emitir($"No se puede facturar el parte de trabajo porque el tipo de factura '{factura.Tipo.Nombre}' no informa de la clase de unitario por defecto a usar");

            if (factura.Tipo.IdUnidadDefecto == null)
                Emitir($"No se puede facturar el parte de trabajo porque el tipo de factura '{factura.Tipo.Nombre}' no informa de la unidad de medida por defecto a usar");

            if (factura.Tipo.IdNaturalezaDefecto == null)
                Emitir($"No se puede facturar el parte de trabajo porque el tipo de factura '{factura.Tipo.Nombre}' no informa de la naturaleza contable por defecto a usar");

            var lineas = factura.Detalles<LineaDeUnaFaeDtm>(contexto);
            var orden = lineas.Count() == 0 ? 10 : lineas.Max(x => x.Orden) + 10;
            var linea = new LineaDeUnaFaeDtm
            {
                IdElemento = idFactura,
                Orden = orden,
                TipoDeLinea = Enumerados.enumTipoDeLinea.Alzada,
                IdUnitario = null,
                Concepto = $"Parte de trabajo: {parteTr.Referencia}",
                Cantidad = 1,
                Precio = importe,
                Anotacion = parteTr.Descripcion,
                Descuento = null,
                IdIvaR = factura.Tipo.IdIvaRDefecto,
                Iva = contexto.SeleccionarPorId<IvaRepercutidoDtm>((int)factura.Tipo.IdIvaRDefecto).Porcentaje,
                Clase = factura.Tipo.ClaseDefecto,
                IdUnidad = factura.Tipo.IdUnidadDefecto,
                IdNaturaleza = factura.Tipo.IdNaturalezaDefecto,
                IdParteTr = parteTr.Id
            }.Insertar(contexto);

            return idFactura;
        }

        public static List<PlanificacionDeVentaDtm> PlanificacionesDeLosPartesTr(this FacturaEmtDtm factura, ContextoSe contexto)
        {
            var planificaciones = new List<PlanificacionDeVentaDtm>();
            if (factura.IdParteTr != null)
            {
                var plf = ExtensorDePartesTr.Planificacion(contexto, (int)factura.IdParteTr);
                if (plf != null) planificaciones.Add(plf);
            }
            var lineas = factura.Detalles<LineaDeUnaFaeDtm>(contexto);
            foreach (var linea in lineas)
            {
                if (linea.IdParteTr == null) continue;
                var plf = ExtensorDePartesTr.Planificacion(contexto, (int)linea.IdParteTr);
                if (plf != null) planificaciones.Add(plf);
            }

            return planificaciones;
        }


        public static void CargarCacheDeCobros(ContextoSe contexto, List<int> idsFacturas)
        {           
            var cacheCobrado = ServicioDeCaches.Obtener(CacheDe.Fae_Cobrado);
            ExtensorDeElementosDeUnProceso.CebarCacheDeIds<FacturaEmtDtm>(contexto, idsFacturas);
            ApiDeDetalles.CebarCacheDeDetalles<CobroDeFaeDtm>(contexto, idsFacturas);

            foreach (var id in idsFacturas)
            {
                _ = contexto.SeleccionarPorId<FacturaEmtDtm>(id, aplicarJoin: false).Cobrado(contexto);
            }
        }

        public static void CargarCacheDeAbonos(ContextoSe contexto, List<int> idsFacturas)
        {
            var cacheAbonado = ServicioDeCaches.Obtener(CacheDe.Fae_Abonado);
            var cacheVinculos = ServicioDeCaches.Obtener(CacheDe.LeerVinculosCon);
            var cacheRegistrosPagos = ServicioDeCaches.ObtenerCache(typeof(PagoDtm).FullName, nameof(IRegistro.Id));

            var idsNuevos = idsFacturas
                .Where(id => !cacheAbonado.ContainsKey(id.ToString()))
                .Distinct()
                .ToList();

            if (!idsNuevos.Any()) return;

            var todosLosVinculos = ExtencionDeVinculos.CebarCacheDeVinculosCon<AbonoDeFaeDtm, FacturaEmtDtm, PagoDtm>(contexto, idsNuevos);

            var idsPagos = todosLosVinculos.Select(v => v.idElemento2).Distinct().ToList();
            ExtensorDeElementosDeUnProceso.CebarCacheDeIds<PagoDtm>(contexto, idsPagos);

            // 3. Poblado de Cachés
            foreach (var idFactura in idsNuevos)
            {
                var vinculosDeEstaFae = todosLosVinculos.Where(v => v.idElemento1 == idFactura).ToList();

                decimal totalAbonado = 0;
                foreach (var v in vinculosDeEstaFae)
                {
                    var pago = contexto.SeleccionarPorId<PagoDtm>(v.idElemento2, aplicarJoin: false);
                    totalAbonado += pago.Importe;
                }

                // Guardamos el total final
                cacheAbonado[idFactura.ToString()] = totalAbonado;
            }
        }

        public static decimal Cobrado(this FacturaEmtDtm factura, ContextoSe contexto)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Fae_Cobrado);
            if (!cache.ContainsKey(factura.Id.ToString()))
            {
                var cobros = factura.Detalles<CobroDeFaeDtm>(contexto);
                cache[factura.Id.ToString()] = cobros.Any() ? cobros.Sum(c => c.Cobrado) : (decimal)0.0;
            }
            return (decimal)cache[factura.Id.ToString()];
        }

        public static CobroDeFaeDtm Cobrar(this FacturaEmtDtm factura, ContextoSe contexto, decimal? cobro = null, DateTime? fecha = null, int? idCtaIngreso = null, int? idFacturaRemesada = null)
        {
            if (factura.EstaCobrada(contexto)) Emitir($"la {enumNegocio.FacturaEmitida} '{factura.Referencia}' ya está cobrada");
            var idCtaCargo = (int?)null;

            var clase = idCtaIngreso is null ? enumClaseDeCobro.Contado : enumClaseDeCobro.Transferencia;
            if (idFacturaRemesada.HasValue)
            {
                if (idCtaIngreso.HasValue)
                    throw new Exception("Error de implementación. No se puede indicar una cuenta de ingreso y el id de una factura remesada");

                idCtaCargo = factura.Cliente(contexto).CuentaDeCliente(contexto, enumClaseDeCuentaBancaria.Pago).Id;
                var remesa = contexto.SeleccionarPorId<FacturaEmtDeUnaRemesaDtm>((int)idFacturaRemesada, aplicarJoin: true).Elemento;
                idCtaIngreso = remesa.CuentaDeAbono(contexto).Id;
                clase = enumClaseDeCobro.Remesa;
            }

            cobro = cobro is null ? factura.PendientePorCobrar(contexto) : cobro;
            var cobroDtm = new CobroDeFaeDtm
            {
                IdElemento = factura.Id,
                CobradoEl = fecha is null ? DateTime.Now : (DateTime)fecha,
                Cobrado = (decimal)cobro,
                Clase = clase,
                IdCuentaDeIngreso = idCtaIngreso,
                IdCuentaDeCargo = idCtaCargo,
                IdFacturaRemesada = idFacturaRemesada
            }.Insertar(contexto);

            return cobroDtm;
        }

        public static CobroDeFaeDtm UltimoCobro(this FacturaEmtDtm factura, ContextoSe contexto)
        {
            if (!factura.HayDetallesDe<CobroDeFaeDtm>(contexto))
                Emitir($"La {enumNegocio.FacturaEmitida} '{factura.Referencia}' no tiene cobros");

            return factura.Detalles<CobroDeFaeDtm>(contexto).OrderByDescending(x => x.CobradoEl).ToList()[0];
        }

        public static bool EstaParcialmenteCobrada(this FacturaEmtDtm factura, ContextoSe contexto)
        =>
        factura.EstaPendiente(contexto) && factura.HayDetallesDe<CobroDeFaeDtm>(contexto);

        public static void ValidarTieneParte(ContextoSe contexto, List<int> idsDeFaes)
        {
            foreach (var id in idsDeFaes)
            {
                var fae = contexto.SeleccionarPorId<FacturaEmtDtm>(id);
                if (fae.IdParteTr == null)
                {
                    var lineas = fae.Detalles<LineaDeUnaFaeDtm>(contexto);
                    foreach (var linea in lineas) if (linea.IdParteTr != null) return;
                    Emitir($"La factura {fae.Referencia} no tiene partes de trabajo");
                }
            }
        }

        public static IvaRepercutidoDtm IvaRepercutido(this LineaDeUnaFaeDtm linea, ContextoSe contexto) => linea.IvaRepercutido is null
            ? linea.IdIvaR is null ? null : contexto.SeleccionarPorId<IvaRepercutidoDtm>((int)linea.IdIvaR)
            : linea.IvaRepercutido;

        public static DireccionDto DireccionFiscal(this FacturaEmtDtm factura, ContextoSe contexto, bool erroSiNoHay = true)
        {
            var direcciones = GestorDeDirecciones.LeerRegistros(contexto, enumNegocio.FacturaEmitida, factura.Id).ToList();
            var fiscal = direcciones.FirstOrDefault(x => x.Calificador == enumCalificadorDireccion.fiscal && x.Activo);
            return fiscal == null
            ? factura.Cliente(contexto).DireccionFiscal(contexto, erroSiNoHay)
            : fiscal.MapearDto(contexto, enumNegocio.FacturaEmitida);
        }

        public static ParteTrDtm ParteTr(this LineaDeUnaFaeDtm linea, ContextoSe contexto, bool validarPermisos = false)
        {
            if (linea.IdParteTr is null)
                return null;
            if (linea.ParteTr is not null)
                return linea.ParteTr;
            var parametros = new Dictionary<string, object> { { ltrParametrosNeg.ValidarPermisosDeConsulta, validarPermisos } };
            return contexto.SeleccionarPorId<ParteTrDtm>((int)linea.IdParteTr, parametros: parametros);
        }

        public static PresupuestoDtm Presupuesto(this FacturaEmtDtm factura, ContextoSe contexto, bool validarPermisos = false)
        {
            if (factura.IdPresupuesto is null)
                return null;
            if (factura.Presupuesto is not null && factura.IdPresupuesto == factura.Presupuesto.Id)
                return factura.Presupuesto;
            return factura.Presupuesto = Presupuesto(contexto, (int)factura.IdPresupuesto, validarPermisos);
        }

        public static PresupuestoDtm Presupuesto(ContextoSe contexto, int idPresupuesto, bool validarPermisos = false)
        {
            var parametros = new Dictionary<string, object> { { ltrParametrosNeg.ValidarPermisosDeConsulta, validarPermisos } };
            return contexto.SeleccionarPorId<PresupuestoDtm>(idPresupuesto, parametros: parametros);
        }

        public static ParteTrDtm ParteTr(this FacturaEmtDtm factura, ContextoSe contexto, bool validarPermisos = false)
        {
            if (factura.IdParteTr is null)
                return null;
            if (factura.ParteTr is not null && factura.IdParteTr == factura.ParteTr.Id)
                return factura.ParteTr;

            var parametros = new Dictionary<string, object> { { ltrParametrosNeg.ValidarPermisosDeConsulta, validarPermisos } };
            return contexto.SeleccionarPorId<ParteTrDtm>((int)factura.IdParteTr, parametros: parametros);
        }

        public static ContratoDtm Contrato(this FacturaEmtDtm factura, ContextoSe contexto, bool validarPermisos = false)
        {
            if (factura.IdContrato is null)
                return null;
            if (factura.Contrato is not null && factura.IdContrato == factura.Contrato.Id)
                return factura.Contrato;
            var parametros = new Dictionary<string, object> { { ltrParametrosNeg.ValidarPermisosDeConsulta, validarPermisos } };
            return contexto.SeleccionarPorId<ContratoDtm>((int)factura.IdContrato, parametros: parametros);
        }

        public static ArchivadorDtm ArchivadorParaLaReclamacion(this FacturaEmtDtm factura, ContextoSe contexto, bool validarPermisos = false)
        {
            if (factura.IdArchivadorParaLaReclamacion is null)
                return null;
            if (factura.ArchivadorParaLaReclamcion is not null && factura.IdArchivadorParaLaReclamacion == factura.ArchivadorParaLaReclamcion.Id)
                return factura.ArchivadorParaLaReclamcion;

            var parametros = new Dictionary<string, object> { { ltrParametrosNeg.ValidarPermisosDeConsulta, validarPermisos } };
            return contexto.SeleccionarPorId<ArchivadorDtm>((int)factura.IdArchivadorParaLaReclamacion, parametros: parametros);
        }

        public static bool ConSoloUnCobro(this FacturaEmtDtm factura, ContextoSe contexto) => factura.Detalles<CobroDeFaeDtm>(contexto).Count() == 1;

        public static CuentaBancariaDtm CuentaDeCargo(this FacturaEmtDtm factura, ContextoSe contexto)
        {
            var cuentaDeCliente = factura.Cliente(contexto).CuentaDeCliente(contexto, enumClaseDeCuentaBancaria.Pago);
            cuentaDeCliente.ValidarElCertificado(contexto);
            return cuentaDeCliente.CuentaBancaria(contexto);
        }

        public static bool EstaCobrada(this FacturaEmtDtm factura, ContextoSe contexto) => factura.PendientePorCobrar(contexto) <= VariableDeFacturasEmt.ToleranciaDeCobro;

        public static bool EstaPendiente(this FacturaEmtDtm factura, ContextoSe contexto) => !factura.EstaCobrada(contexto);

        public static decimal PendientePorCobrar(this FacturaEmtDtm factura, ContextoSe contexto)
        {
            if (factura.EsRectificativa)
                return 0;

            var rectificadaPor = factura.RectificadaPor(contexto, errorSiNoHay: false);
            var valorRectificado = rectificadaPor is not null ? rectificadaPor.APagar(contexto) : 0;
            return factura.APagar(contexto) + valorRectificado - factura.Cobrado(contexto) + (rectificadaPor?.Abonado(contexto) ?? 0);
        }


        private static void CopiarLineas(this FacturaEmtDtm factura, ContextoSe contexto, PresupuestoDtm ppt)
        {
            var lineas = ppt.Detalles<LineaDeUnPptDtm>(contexto);

            if (lineas.Count == 0)
                Emitir($"El {ppt.Referencia} no tiene definido el detalle afacturar, defínalo previamente");

            foreach (var linea in lineas)
            {
                linea.ValidarIva(ppt.Referencia);

                new LineaDeUnaFaeDtm
                {
                    IdElemento = factura.Id,
                    Orden = linea.Orden,
                    TipoDeLinea = linea.TipoDeLinea,
                    IdUnitario = linea.IdUnitario,
                    Concepto = linea.Concepto,
                    Cantidad = linea.Cantidad,
                    Precio = linea.Precio,
                    Anotacion = linea.Anotacion,
                    Descuento = linea.Descuento,
                    IdIvaR = linea.IdIvaR,
                    Iva = linea.Iva,
                    Clase = linea.Clase,
                    IdUnidad = linea.IdUnidad,
                    IdNaturaleza = linea.IdNaturaleza
                }
                .Insertar(contexto);
            }
        }

        public static PlanificacionDeVentaDtm Planificacion(this FacturaEmtDtm factura, ContextoSe contexto)
        =>
        Planificacion(contexto, factura.Id);

        private static FacturaEmtDtm CalcularVencimiento(this FacturaEmtDtm factura, ContextoSe contexto)
        {
            if (factura.EsRectificativa)
            {
                factura.VenceEl = factura.FacturadaEl.Fecha();
            }
            else
            {
                var tipo = factura.Tipo<TipoDeFacturaEmtDtm>(contexto);
                factura.VenceEl = DateTime.Now.AddDays(tipo.Vencimiento);
            }
            return factura;
        }

        public static FacturaEmtDtm RecalcularVencimiento(this FacturaEmtDtm factura, ContextoSe contexto, bool estoyCobrando = true)
        {
            var tipo = factura.Tipo<TipoDeFacturaEmtDtm>(contexto);
            if (estoyCobrando)
                factura.VenceEl = DateTime.Now.AddDays(tipo.Vencimiento);
            else
            {
                var ultimoCobro = factura.Detalles<CobroDeFaeDtm>(contexto).OrderByDescending(t => t.CobradoEl).FirstOrDefault();
                if (ultimoCobro is null)
                    factura = factura.CalcularVencimiento(contexto);
                else
                    factura.VenceEl = ultimoCobro.CobradoEl.AddDays(tipo.Vencimiento);
            }
            return factura;
        }

        public static bool EstaVencida(this FacturaEmtDtm factura, ContextoSe contexto)
        =>
        factura.RecalcularVencimiento(contexto, estoyCobrando: false).VenceEl <= DateTime.Now;


        public static FacturaEmtDtm TransitarTrasHacerUnPagoParcial(this FacturaEmtDtm factura, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            factura = factura.RecalcularVencimiento(contexto, estoyCobrando: true);
            return factura.TransitarALaEtapa(contexto, enumEtapasDeFacturasEmt.FAE_Etapa_Pago_Parcial.EstadosDeLaEtapa(), parametros);
        }

        public static FacturaEmtDtm EliminarUnCobroParcialDeUnaFacturaCobrada(this FacturaEmtDtm factura, ContextoSe contexto, Dictionary<string, object> parametros)
        =>
        factura.EstaVencida(contexto)
        ? factura.TransitarALaEtapa(contexto, enumEtapasDeFacturasEmt.FAE_Etapa_De_Reclamacion.EstadosDeLaEtapa(), parametros)
        : factura.TransitarALaEtapa(contexto, enumEtapasDeFacturasEmt.FAE_Etapa_Pago_Parcial.EstadosDeLaEtapa(), parametros);

        public static FacturaEmtDtm DevolverAVencidaTrasAnularUnCobroParcial(this FacturaEmtDtm factura, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            var resultado = factura.IntentarAplicarTransicion(contexto, TransicionAplicable.Transiciones(VariableDeFacturasEmt.TransicionesPorMotivo, VariableDeFacturasEmt.enumMotivoTransicion.AnularPago, errorSiNoHay: true));

            return resultado.elemento;
        }

        private static PlanificacionDeVentaDtm Planificacion(ContextoSe contexto, int idFactura)
        {
            var cachePtr = ServicioDeCaches.Obtener(CacheDe.PlvDeUnFactura);
            if (!cachePtr.ContainsKey(idFactura.ToString()))
            {
                cachePtr[idFactura.ToString()] = contexto.SeleccionarPorFk<PlanificacionDeVentaDtm>(nameof(PlanificacionDeVentaDtm.IdFacturaEmt), idFactura, errorSiNoHay: false);
            }
            return (PlanificacionDeVentaDtm)cachePtr[idFactura.ToString()];
        }

        public static void ModificarEventoDeVencimiento(this FacturaEmtDtm factura, ContextoSe contexto)
        {
            var sociedad = factura.Sociedad(contexto);
            if (sociedad.IdAgenda is not null)
                factura.EliminarEventoDeVencimiento(contexto);
            factura.AnotarEventoDeVencimiento(contexto);
        }

        private static void AnotarEventoDeVencimiento(this FacturaEmtDtm factura, ContextoSe contexto)
        {
            var sociedad = factura.Sociedad(contexto);
            var agenda = sociedad.IdAgenda == null ? sociedad.CrearAgenda(contexto) : contexto.SeleccionarPorId<AgendaDtm>((int)sociedad.IdAgenda);

            var evento = new EventoDeAgendaDtm();
            evento.IdAgenda = agenda.Id;
            evento.IdElemento = factura.Id;
            evento.IdNegocio = enumNegocio.FacturaEmitida.IdNegocio();
            evento.EsDelSistema = true;
            evento.Inicio = ((DateTime)factura.VenceEl).Date;
            evento.Nombre = ltrDeUnaFacturaEmt.EventoDeVencimiento.Replace($"[{nameof(FacturaEmtDtm.NumeroDeFactura)}]", factura.NumeroDeFactura);
            evento.Descripcion = $"Hoy vence la factura {factura.NumeroDeFactura}";
            evento.Fin = evento.Inicio;
            GestorDeVinculos.Vincular(contexto, factura, evento, new Dictionary<string, object> { { ltrParametrosNeg.EstaEjecutandoUnaAccion, true } });
        }

        private static void EliminarEventoDeVencimiento(this FacturaEmtDtm factura, ContextoSe contexto)
        {
            var nombreEvento = ltrDeUnaFacturaEmt.EventoDeVencimiento.Replace($"[{nameof(FacturaEmtDtm.NumeroDeFactura)}]", factura.NumeroDeFactura);
            var eventos = contexto.SeleccionarEventos(factura.Id, nombreEvento);
            var p = new Dictionary<string, object> { { ltrParametrosNeg.EstaEjecutandoUnaAccion, true } };
            foreach (var e in eventos.Where(e => e.EsDelSistema)) factura.Desvincular(contexto, e, p);
        }

        private static void DarPorFacturadoLosPartes(this FacturaEmtDtm factura, ContextoSe contexto)
        {
            var parte = factura.ParteTr(contexto);
            if (parte is not null)
            {
                var transicion = parte.TransicionPosible(contexto, enumEtapasDePartesTr.PTR_Etapa_Facturado, enumEtapasDePartesTr.PTR_Etapa_Facturado.Lista());
                parte.Transitar(contexto, transicion.Id);
            }

            foreach (var linea in factura.Detalles<LineaDeUnaFaeDtm>(contexto))
            {
                if (linea.IdParteTr is not null)
                {
                    parte = linea.ParteTr(contexto);
                    var transicion = parte.TransicionPosible(contexto, enumEtapasDePartesTr.PTR_Etapa_Facturado, enumEtapasDePartesTr.PTR_Etapa_Facturado.Lista());
                    parte.Transitar(contexto, transicion.Id);
                }
            }
        }

        private static void ValidarRequisitosSii(this FacturaEmtDtm factura, ContextoSe contexto)
        {
            if (!factura.UsaVerifactu(contexto))
                return;

            factura.Sociedad(contexto).ValidarCertificado(contexto);
            ValidarEstructuraParaAlmacenarAuditoriaSii(contexto);
            var log = contexto.SeleccionarTodos<LogDeEnvioDeFacturaDtm>(new List<ClausulaDeFiltrado> {
                         new ClausulaDeFiltrado
                             {
                                 Clausula = nameof(LogDeEnvioDeFacturaDtm.EnviadaEl),
                                 Criterio = enumCriteriosDeFiltrado.esNulo,
                                 Valor = ""
                             }
                         }, aplicarJoin: true);

            if (log.Count() > 0)
            {
                Emitir($"la factura '{log.First().Factura.Referencia}' está pendiente de comunicar a la AEAT, una vez comunicada prodrá emitir esta factura");
            }

            if (factura.EsRectificativa && factura.MotivoDeRectificacion != enumMotivoDeRectificacion.DatosErroneos)
            {
                var rectificada = factura.RectificaA(contexto);
                if (rectificada.Verifactu(contexto).Cancelada == true)
                    throw new Exception($"No se puede emitir la factura '{factura.Referencia}' ya que la rectificada '{rectificada.Referencia}' está cancelada en la AEAT, cree una nueva");
            }
        }

        public static void ValidarEstructuraParaAlmacenarAuditoriaSii(ContextoSe contexto)
        {
            var id = ExtensorDeArchivadores.Cfg_Id_Tipo_De_Archivador_Sii(contexto);
            var cg = ExtensionCentrosGestores.Cfg_CG_De_Documentacion(contexto);
            if (id <= 0)
                Emitir($"No se ha definido el tipo de archivador para los ficheros de auditoría SII");
            if (cg is null)
                Emitir($"No se ha definido el centro gestor de documentación para los ficheros de auditoría SII");
        }

        public static VerifactuDtm Verifactu(this FacturaEmtDtm factura, ContextoSe contexto, bool errorSiNoHay = true)
        =>
        factura.Verifactu is null ? factura.Verifactu = factura.Ampliacion<VerifactuDtm>(contexto, errorSiNoHay) : factura.Verifactu;


        public static void AsociarAuditoriaSii(this FacturaEmtDtm factura, ContextoSe contexto, AuditoriaSii auditoriaSii)
        {
            var fecha = DateTime.Now;
            var mes = fecha.Month.ToString().PadLeft(2, '0');
            var archivoDelBlockChain = $"{fecha.Year}{mes}.{enumExtensiones.csv}";

            var rutaLog = System.IO.Path.Combine(CacheDeVariable.CFG_Ruta_Fichero_De_Sii_Log, factura.Sociedad(contexto).NIF);
            contexto.IniciarTraza(rutaLog, $"Ficheros_{factura.NumeroDeFactura}", debugar: true);
            VerifactuDtm verifactu = null;
            try
            {
                verifactu = RegistrarVerifactu(factura, contexto, auditoriaSii, fecha, auditoriaSii.Huella);
                var enviado = System.IO.File.Exists(auditoriaSii.FicheroEnviado) ? ApiDeArchivos.SubirArchivoInterno(contexto, auditoriaSii.FicheroEnviado, copiar: true, sanitizar: false) : null;
                var respuesta = System.IO.File.Exists(auditoriaSii.FicheroDeRespuesta) ? ApiDeArchivos.SubirArchivoInterno(contexto, auditoriaSii.FicheroDeRespuesta, copiar: true, sanitizar: false) : null;
                if (enviado is not null) verifactu.ObtenerArchivador(contexto).Vincular(contexto, enviado);
                if (respuesta is not null) verifactu.ObtenerArchivador(contexto).Vincular(contexto, respuesta);

                var anexados = verifactu.ObtenerBlockChain(contexto).LeerAnexados(contexto);
                var blockChain = anexados.FirstOrDefault(x => x.Nombre == archivoDelBlockChain);
                if (blockChain is not null)
                {
                    verifactu.ObtenerBlockChain(contexto).QuitarAnexado(contexto, blockChain.Id);
                }
                var ficheroBc = System.IO.Path.Combine(auditoriaSii.RutaBlockChain, archivoDelBlockChain);
                blockChain = System.IO.File.Exists(ficheroBc) ? ApiDeArchivos.SubirArchivoInterno(contexto, ficheroBc, copiar: true, sanitizar: false) : null;
                if (blockChain is not null) verifactu.ObtenerBlockChain(contexto).Vincular(contexto, blockChain);
            }
            catch (Exception ex)
            {
                if (verifactu != null)
                {
                    contexto.AnotarTraza($"Error al subir a la GD la infomación de {factura.NumeroDeFactura}", Detalle(ex));
                }
                throw;
            }
            finally
            {
                contexto.CerrarTraza();
            }

        }

        public static void RecomponerVerifactu(this FacturaEmtDtm factura, ContextoSe contexto, string huella)
        {
            var fecha = DateTime.Now;
            var mes = fecha.Month.ToString().PadLeft(2, '0');
            var archivoDelBlockChain = $"{fecha.Year}{mes}.{enumExtensiones.csv}";

            var verifactu = RegistrarVerifactu(factura, contexto, auditoriaSii: null, fecha, huella);

            var anexados = verifactu.ObtenerBlockChain(contexto).LeerAnexados(contexto);
            var blockChain = anexados.FirstOrDefault(x => x.Nombre == archivoDelBlockChain);
            if (blockChain is not null)
            {
                verifactu.ObtenerBlockChain(contexto).QuitarAnexado(contexto, blockChain.Id);
            }
            var rutaBlockChain = Path.Combine(CacheDeVariable.CFG_Ruta_Fichero_De_Sii_Blockchains, factura.Sociedad(contexto).NIFSinIsoEs);
            var ficheroBc = System.IO.Path.Combine(rutaBlockChain, archivoDelBlockChain);
            blockChain = System.IO.File.Exists(ficheroBc) ? ApiDeArchivos.SubirArchivoInterno(contexto, ficheroBc, copiar: true, sanitizar: false) : null;
            if (blockChain is not null) verifactu.ObtenerBlockChain(contexto).Vincular(contexto, blockChain);
        }

        private static VerifactuDtm RegistrarVerifactu(FacturaEmtDtm factura, ContextoSe contexto, AuditoriaSii auditoriaSii, DateTime fecha, string huella)
        {
            VerifactuDtm verifactu = null;

            var respuestaSii = auditoriaSii is null ? "Recompuesto el registro" : auditoriaSii.Error.IsNullOrEmpty() ? auditoriaSii.Respuesta : $"{auditoriaSii.Codigo}-{auditoriaSii.Error}";
            var archivador = factura.CrearArchivadorSii(contexto, auditoriaSii is null ? "Recompuesto, no se tiene el CSV" : auditoriaSii.CSV);
            var archivadorBc = ExtensorDeArchivadores.ObtenerArchivadorBlockChain(contexto, fecha.Year, factura.Sociedad(contexto).NIFSinIsoEs);
            var urlDeVerificacion = factura.GenerarURLParaVerifactu(contexto);
            var csv = auditoriaSii is null ? $"R:'{factura.Referencia}'" : auditoriaSii.CSV;
            try
            {
                verifactu = new VerifactuDtm()
                {
                    IdElemento = factura.Id,
                    IdArchivador = archivador.Id,
                    IdBlockChain = archivadorBc.Id,
                    CSV = csv.Left(IDominio.Longitud(IDominio.VARCHAR_20)),
                    Respuesta = respuestaSii.Left(IDominio.Longitud(IDominio.VARCHAR_255)),
                    Url = urlDeVerificacion.Left(IDominio.Longitud(IDominio.VARCHAR_2000)),
                    Huella = huella,
                    Cancelada = false
                };
                verifactu.InsertarComoAdministrador(contexto);
            }
            catch (Exception ex)
            {
                var valoresVerifactu = new StringBuilder();
                valoresVerifactu.AppendLine($"Factura: {factura.Referencia}");
                valoresVerifactu.AppendLine($"Archivador: {(archivador is null ? "no se ha podido crear" : archivador.Referencia)}");
                valoresVerifactu.AppendLine($"BlockChain: {(archivadorBc is null ? "no se ha podido crear" : archivadorBc.Referencia)}");
                valoresVerifactu.AppendLine($"CSV: {csv}, longitud:{csv.Length}");
                valoresVerifactu.AppendLine($"Respuesta: {respuestaSii}, longitud:{respuestaSii.Length}");
                valoresVerifactu.AppendLine($"Url: {urlDeVerificacion}, longitud:{urlDeVerificacion.Length}");
                valoresVerifactu.AppendLine($"Huella: {huella}");
                contexto.AnotarTraza($"Error al crear el registro verifactu '{factura.NumeroDeFactura}'", $"Valores{Environment.NewLine}{valoresVerifactu}{Environment.NewLine}{Detalle(ex)}");
                throw;
            }
            return verifactu;
        }

        public static string GenerarURLParaVerifactu(this FacturaEmtDtm facturaEmt, ContextoSe contexto)
        {
            var baseUrl = facturaEmt.UsaVerifactu(contexto) == false ? ParametrosDelSii.SII_URLDeValidarQrNoVerifactu : ParametrosDelSii.SII_URLDeValidarQr;
            var url = CompletarUrl(facturaEmt, contexto, baseUrl);
            return url.ToString();
        }


        public static string GenerarURLParaSe(this FacturaEmtDtm facturaEmt, ContextoSe contexto)
        {
            var baseUrl = new UriBuilder(CacheDeVariable.Cfg_UrlBase) { Path = $"/{enumControladoresVentas.FacturasEmt}/{enumVistasVentas.epValidarQr}" }.ToString();
            var url = CompletarUrl(facturaEmt, contexto, baseUrl);
            return url.ToString();
        }

        private static StringBuilder CompletarUrl(FacturaEmtDtm facturaEmt, ContextoSe contexto, string baseUrl)
        {
            if (!baseUrl.EndsWith("?")) baseUrl = baseUrl + "?";

            // Crear un diccionario para los parámetros
            var parametros = new Dictionary<string, string>
            {
                { "nif", facturaEmt.Sociedad(contexto).NIF.Replace(ltrIsoPaises.Spain, "") },
                { "numserie", facturaEmt.NumeroDeFactura },
                { "fecha", facturaEmt.FacturadaEl.Fecha().ToString("dd-MM-yyyy") }, // Formatear la fecha
                { "importe", (facturaEmt.BiConIva(contexto) - facturaEmt.Irpf(contexto)).Formatear(alineacion:false, separadorDecimal:Simbolos.Punto) } //} // Formatear el importe --ToString("F2", CultureInfo.InvariantCulture) 
            };

            // Construir la URL con URL encoding
            var urlBuilder = new StringBuilder(baseUrl);
            foreach (var param in parametros)
            {
                urlBuilder.Append($"{WebUtility.UrlEncode(param.Key)}={WebUtility.UrlEncode(param.Value)}&");
            }

            // Eliminar el último '&'
            urlBuilder.Length--;

            return urlBuilder;
        }

        public static string LeyendaVerifactu(this FacturaEmtDtm factura, ContextoSe contexto)
        {
            return factura.UsaVerifactu(contexto) ? "VERI*FACTU" : ""; // "Factura verificable en la sede electrónica de la AEAT"
        }

        public static string LeyendaSe(this FacturaEmtDtm factura, ContextoSe contexto)
        {
            return "Verificable en el S.E.";
        }
        private static ArchivadorDtm CrearArchivadorSii(this FacturaEmtDtm factura, ContextoSe contexto, string csv)
        {
            var archivador = new ArchivadorDtm
            {
                Nombre = factura.Referencia + ": " + factura.NumeroDeFactura,
                Descripcion = csv,
                IdCg = factura.IdCg,
                IdTipo = ExtensorDeArchivadores.Cfg_Id_Tipo_De_Archivador_Sii(contexto),
            }
            .InsertarComoAdministrador(contexto, accionEjecutada: nameof(ltrDeUnArchivador.Accion_GenerarSii));
            return archivador;
        }

        public static IEnumerable<TransicionDto> ExcluirTransiciones(IEnumerable<TransicionDto> transiciones, List<ClausulaDeFiltrado> filtros)
        {
            var clausulaOrigen = filtros.First(f => f.Clausula.ToLower() == nameof(TransicionDto.Origen).ToLower() && f.Criterio == enumCriteriosDeFiltrado.igual);
            if (VariableDeFacturasEmt.EstadosDeLaEtapa(enumEtapasDeFacturasEmt.FAE_Etapa_Emitida).estados.Contains(clausulaOrigen.Valor.Entero()))
            {
                if (VariableDeFacturasEmt.Fae_Sii_Activo())
                {
                    var transicionesAdmitidas = new List<TransicionDto>();
                    var estadosPrefactura = VariableDeFacturasEmt.EstadosDeLaEtapa(enumEtapasDeFacturasEmt.FAE_Etapa_Prefactura).estados;
                    foreach (var transicion in transiciones)
                    {
                        if (estadosPrefactura.Contains(transicion.IdDestino))
                            continue;
                        transicionesAdmitidas.Add(transicion);
                    }
                    return transicionesAdmitidas;
                }
            }
            return transiciones;
        }

        public static FacturaEmtDtm LeerFacturaEmt(this SociedadDtm sociedad, ContextoSe contexto, string numeroFactura, bool errorSiNoHay = true, bool validarPermisos = false)
        {
            var i = sociedad.Id.ToString() + "-" + numeroFactura;
            var cache = ServicioDeCaches.Obtener(CacheDe.Fae_FacturaPorNumero);
            if (cache.ContainsKey(i) && validarPermisos == false)
                return (FacturaEmtDtm)cache[i];

            var filtros = new List<ClausulaDeFiltrado>
            {
                new ClausulaDeFiltrado { Clausula = ltrDeUnaFacturaEmt.FiltroPorNumeroDeFactura, Criterio = enumCriteriosDeFiltrado.igual, Valor = numeroFactura},
                new ClausulaDeFiltrado { Clausula = ltrDeUnaFacturaEmt.IdSociedad, Criterio = enumCriteriosDeFiltrado.igual, Valor = sociedad.Id.ToString()}
            };

            var facturas = contexto.SeleccionarTodos<FacturaEmtDtm>(filtros, parametros: new Dictionary<string, object> { { ltrParametrosNeg.ValidarPermisosDeConsulta, validarPermisos } });
            if (facturas.Count() == 0 && errorSiNoHay)
                throw new Exception($"La factura '{numeroFactura}' de la sociedad '{sociedad.NIFSinIsoEs}' no se ha localizado");
            if (facturas.Count() > 1)
                throw new Exception($"Hay más de una factura en la sociedad '{sociedad.NIFSinIsoEs}' con número '{numeroFactura}'");

            if (facturas.Count == 0)
                return null;

            cache[i] = facturas.First();

            return (FacturaEmtDtm)cache[i];

        }
    }
}
