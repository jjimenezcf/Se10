using ModeloDeDto;
using ModeloDeDto.Ventas;
using QRCoder;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ServicioDeReportes.Base;
using Utilidades;
using static ServicioDeDatos.Elemento.Enumerados;
using System.Net;
using System.Text;
using ServicioDeDatos.Ventas;
using GestorDeElementos.Extensores;

namespace ServicioDeReportes.Ventas
{
    public class ReporteDeFacturaEmt : IDocument
    {
        public FacturaEmtRpt Factura { get; }

        public ReporteDeFacturaEmt(IInformacionRpt<FacturaEmtDto> facturaRpt)
        {
            Factura = (FacturaEmtRpt)facturaRpt;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container
                .Page(page =>
                {
                    page.Margin(Factura.Marco);

                    page.Header().Element(CabeceraFactura);
                    page.Content().Element(Cuerpo);

                    page.Footer().Component(new PieDePaginaCentrado(Factura.TamanoPieDePagina, Factura.MostrarImpresoEl));
                });
        }

        private void CabeceraFactura(IContainer encabezado)
        {
            bool esPrefactura = Factura.Datos.Numero is null || Factura.FacturaDtm.EstaEnLaEtapa(enumEtapasDeFacturasEmt.FAE_Etapa_Prefactura);

            encabezado.Row(cabecera =>
            {
                // Primera columna: información de la factura/prefactura (alineada a la izquierda)
                cabecera.ConstantItem(150).AlignLeft().Column(columna =>
                {
                    var tipoFacturaEmitida = Factura.Datos.Numero is null
                        ? $"Prefactura: {Factura.Datos.Referencia}"
                        : Environment.NewLine +( Factura.Datos.ClaseDeEmision == ServicioDeDatos.Ventas.enumClaseDeEmision.eFactura322
                            ? $"{(Factura.MostrarPalabraCopia ? "Copia" : "Factura")}: {Factura.Datos.NumeroFactura}"
                            : $"Factura: {Factura.Datos.NumeroFactura}");

                    columna.Item().Text(tipoFacturaEmitida).FontSize(Factura.TamanoTitulo).SemiBold().FontColor(Factura.ColorTitulo);

                    columna.Item().Text(text =>
                    {
                        text.Span("Fecha de emisión: ").SemiBold();
                        text.Span($"{Factura.Datos.FacturadaEl?.ToString("dd-MM-yyyy")}");
                    });

                    if (Factura.ImprimirVencimiento)
                    {
                        columna.Item().Text(text =>
                        {
                            text.Span("Fecha de vencimiento: ").SemiBold();
                            text.Span($"{Factura.Datos.VenceEl?.ToString("dd-MM-yyyy")}");
                        });
                    }
                });

                if (!esPrefactura)
                {
                    // Segunda columna: QR AEAT (centrada)
                    cabecera.RelativeItem().AlignCenter().Column(columna =>
                    {
                        byte[] qrImage = GenerarCodigoQr(Factura.UrlAeat);
                        if (qrImage != null)
                        {
                            columna.Item().AlignCenter().Text("QR tributario:").FontSize(7).FontColor(Colors.Grey.Darken2);
                            columna.Item().AlignCenter().Width(85.04f).Height(85.04f).Image(qrImage);
                            columna.Item().AlignCenter().Text(Factura.LeyendaAeat).FontSize(7).FontColor(Colors.Grey.Darken2);
                        }
                    });

                    // Tercera columna: QR SIF (centrada)
                    cabecera.RelativeItem().AlignRight().Column(columna =>
                    {
                        byte[] qrImage = GenerarCodigoQr(Factura.UrlSe);
                        if (qrImage != null)
                        {
                            columna.Item().AlignCenter().Text("QR del SIF:").FontSize(7).FontColor(Colors.Grey.Darken2);
                            columna.Item().AlignCenter().Width(85.04f).Height(85.04f).Image(qrImage);
                            columna.Item().AlignCenter().Text(Factura.LeyendaSe).FontSize(7).FontColor(Colors.Grey.Darken2);
                        }
                    });
                }

                // Última columna: logo (alineada a la derecha)
                cabecera.RelativeItem().AlignRight().Column(columna =>
                {
                    ApiDeReportes.RenderLogo(cabecera, Factura.Logo, 85.04f, 85.04f, texto: Factura.Sociedad.Nif);
                });
            });
        }

        public byte[] GenerarCodigoQr(string textoQR)
        {
            using (var qrGenerator = new QRCodeGenerator())
            {
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(textoQR, QRCodeGenerator.ECCLevel.Q);
                using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
                {
                    byte[] qrCodeImage = qrCode.GetGraphic(20);
                    return qrCodeImage;
                }
            }
        }

        public string GenerarURLQR(string nif, string numSerie, DateTime fecha, decimal importe)
        {
            var baseUrl = ParametrosDelSii.SII_URLDeValidarQr;
            if (!baseUrl.EndsWith("?")) baseUrl = baseUrl + "?";

            // Crear un diccionario para los parámetros
            var parametros = new Dictionary<string, string>
            {
                { "nif", nif },
                { "numserie", numSerie },
                { "fecha", fecha.ToString("dd-MM-yyyy") }, // Formatear la fecha
                { "importe", importe.Formatear(alineacion:false, separadorDecimal:".") } //} // Formatear el importe --ToString("F2", CultureInfo.InvariantCulture) 
            };

            // Construir la URL con URL encoding
            var urlBuilder = new StringBuilder(baseUrl);
            foreach (var param in parametros)
            {
                urlBuilder.Append($"{WebUtility.UrlEncode(param.Key)}={WebUtility.UrlEncode(param.Value)}&");
            }

            // Eliminar el último '&'
            urlBuilder.Length--;

            return urlBuilder.ToString();
        }

        void Cuerpo(IContainer container)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(0.1F);
                    columns.RelativeColumn(1.9F);
                });


                table.Cell().Element(x => ApiDeReportes.MargenIzquierdo(x, Factura.InscritoEn, Factura.PaddingMargenIzquierdo));
                table.Cell().Element(ContenidoDelCuerpo);

                if (!Factura.InformacionFiscal.IsNullOrEmpty())
                    table.Cell().ColumnSpan(2).PaddingTop(Factura.PaddingTopPie).Element(x => InformacionFiscal(x, Factura.InformacionFiscal));

                table.Cell().ColumnSpan(2).PaddingTop(Factura.PaddingTopPie).Element(x => ApiDeReportes.ComentarioEnPie(x, Factura.PieDeFactura));
            });
        }

        private static void InformacionFiscal(IContainer container, string informacionFiscal)
        {
            container.ShowEntire().BorderColor(Colors.Grey.Lighten3).Padding(10).Column(column =>
            {
                column.Spacing(5);
                column.Item().Element(EstilosRpt.Cometarios).Text($"Información Fiscal:{Environment.NewLine}{informacionFiscal}");
            });
        }

        private void ContenidoDelCuerpo(IContainer container)
        {
            container.PaddingVertical(60).Column(column =>
            {
                column.Spacing(20);

                column.Item().Row(row =>
                {
                    row.RelativeItem().Element(x => EstilosRpt.Encabezado(x, Factura.TamanoEncabezado)).Component(new DireccionReporte("De", new Direccion
                    {
                        Empresa = Factura.Sociedad.RazonSocial,
                        Calle = ModeloDeDto.Reporte.ExtensorDeRpt.Imprimirla(Factura.Sociedad.DireccionFiscal, Factura.MostrarCalificadorDireccion),
                        eMail = Factura.Sociedad.eMail,
                        Telefono = Factura.Sociedad.Telefono,
                        NIF = Factura.Sociedad.Nif
                    }));
                    row.ConstantItem(50);
                    row.RelativeItem().Element(x => EstilosRpt.Encabezado(x, Factura.TamanoEncabezado)).Component(new DireccionReporte("A", new Direccion
                    {
                        Empresa = Factura.Datos.Contacto,
                        Calle = ModeloDeDto.Reporte.ExtensorDeRpt.Imprimir(Factura.Direccion, Factura.MostrarCalificadorDireccion),
                        eMail = Factura.Datos.eMail,
                        Telefono = Factura.Datos.Telefono,
                        NIF = Factura.Cliente.NIF
                    }));
                });

                column.Item().Element(LineasDeFactura);

                column.Item().PaddingRight(5).AlignRight().Text($"Base imponible: {Factura.Datos.TotalSinIva.Moneda()}").SemiBold();

                foreach (var iva in Factura.Ivas)
                    column.Item().PaddingRight(5).AlignRight().Text($"{iva.Tipo}: {iva.Importe.Moneda()}");

                foreach (var retencion in Factura.Retenciones)
                    column.Item().PaddingRight(5).AlignRight().Text($"{retencion.Tipo}: {(-1 * retencion.Importe).Moneda()}");

                foreach (var exento in Factura.Exenciones)
                    column.Item().Element(container => EstilosRpt.Resumen(container, 8)).PaddingRight(5).AlignRight().Text($"{exento.Tipo} {exento.BI.Moneda()}");

                column.Item().PaddingRight(5).AlignRight().Text($"Total a pagar: {Factura.Datos.APagar.Moneda()}").SemiBold();
            });
        }


        void LineasDeFactura(IContainer container)
        {
            var headerStyle = TextStyle.Default.SemiBold();

            container.Table(table =>
            {

                table.ColumnsDefinition(columns =>
                {
                    if (Factura.IndicarFila) columns.ConstantColumn(25);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    if (Factura.HayDescuento) columns.RelativeColumn();
                });

                table.Header(header =>
                {
                    if (Factura.IndicarFila) header.Cell().Element(EstilosRpt.Negrita).Text("#");
                    header.Cell().Element(EstilosRpt.Negrita).Text("Concepto").Style(headerStyle);
                    header.Cell().Element(EstilosRpt.Negrita).AlignRight().Text("Precio").Style(headerStyle);
                    header.Cell().Element(EstilosRpt.Negrita).AlignRight().Text("Cantidad").Style(headerStyle);
                    if (Factura.HayDescuento) header.Cell().Element(EstilosRpt.Negrita).AlignRight().Text("Descuento").Style(headerStyle);
                    header.Cell().Element(EstilosRpt.Negrita).AlignRight().Text("Base").Style(headerStyle);
                });

                var numeroDelinea = 1;
                foreach (var linea in Factura.Lineas)
                {
                    var index = Factura.Lineas.IndexOf(linea) + 1;
                    var impConDto =
                      linea.ImporteSinDto is null
                      ? 0M
                      : (decimal)linea.ImporteSinDto - (linea.ImporteDeDto is null ? 0M : (decimal)linea.ImporteDeDto)
                    ;

                    var span = (uint)(Factura.HayDescuento ? 4 : 3);
                    if (!Factura.IndicarFila) span = span - 1;

                    if (linea.TipoDeLinea == enumTipoDeLinea.Comentario.ToString())
                    {
                        table.Cell().Element(EstilosRpt.Celda).Text(string.Empty);
                        table.Cell().ColumnSpan(span).Element(EstilosRpt.Cometarios).Text(linea.Concepto + $"{(linea.Anotacion.IsNullOrEmpty() ? "" : $"{Environment.NewLine}{linea.Anotacion}")}");
                        table.Cell().Element(EstilosRpt.Celda).Text(string.Empty);
                    }
                    else
                    {
                        if (Factura.IndicarFila) table.Cell().Element(linea.Anotacion.IsNullOrEmpty() ? EstilosRpt.Celda : EstilosRpt.CeldaSinBordeAbajo).Text($"{numeroDelinea}");
                        table.Cell().Element(linea.Anotacion.IsNullOrEmpty() ? EstilosRpt.Celda : EstilosRpt.CeldaSinBordeAbajo).Text(linea.Concepto);
                        table.Cell().Element(linea.Anotacion.IsNullOrEmpty() ? EstilosRpt.Celda : EstilosRpt.CeldaSinBordeAbajo).AlignRight().Text($"{linea.Precio.Moneda()}");
                        table.Cell().Element(linea.Anotacion.IsNullOrEmpty() ? EstilosRpt.Celda : EstilosRpt.CeldaSinBordeAbajo).AlignRight().Text($"{linea.Cantidad.Formatear()}");
                        if (Factura.HayDescuento) table.Cell().Element(linea.Anotacion.IsNullOrEmpty() ? EstilosRpt.Celda : EstilosRpt.CeldaSinBordeAbajo).AlignRight().Text($"{linea.Descuento.Porcentaje()}");
                        table.Cell().Element(EstilosRpt.Celda).AlignRight().Text(impConDto.Moneda());

                        if (!linea.Anotacion.IsNullOrEmpty())
                        {
                            if (Factura.IndicarFila) table.Cell().Element(EstilosRpt.Celda).Text(string.Empty);
                            table.Cell().ColumnSpan(span).Element(EstilosRpt.Cometarios).Text(linea.Anotacion);
                            table.Cell().Element(EstilosRpt.Celda).Text(string.Empty);
                            if (!Factura.IndicarFila) table.Cell().Element(EstilosRpt.Celda).Text(string.Empty);
                        }
                        numeroDelinea++;
                    }
                }
            });
        }


    }


}