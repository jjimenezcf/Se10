using ModeloDeDto;
using ModeloDeDto.Logistica;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ServicioDeReportes.Base;
using Utilidades;

namespace ServicioDeReportes.Logistica
{
    // Oferta de un documento de venta: lo que se le ofrece al cliente para que lo apruebe antes de servirlo. Como la prefactura
    // (ReporteDeFacturaEmt) pero sin iva, todo con la base imponible: título "Oferta: (referencia) nombre", quién ofrece y a quién, las
    // líneas y el total sin iva
    public class ReporteDeOfertaDeVenta : IDocument
    {
        public OfertaDeVentaRpt Oferta { get; }

        public ReporteDeOfertaDeVenta(IInformacionRpt<VentaDto> ofertaRpt)
        {
            Oferta = (OfertaDeVentaRpt)ofertaRpt;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container
                .Page(page =>
                {
                    page.Margin(Oferta.Marco);

                    page.Header().Element(Cabecera);
                    page.Content().Element(Cuerpo);

                    page.Footer().Component(new PieDePaginaCentrado(Oferta.TamanoPieDePagina, Oferta.MostrarImpresoEl));
                });
        }

        // A la izquierda el título y la fecha de la oferta, a la derecha el logo de la sociedad que vende (si no tiene, el hueco queda vacío:
        // su nif ya va en el bloque "De")
        private void Cabecera(IContainer encabezado)
        {
            encabezado.Row(cabecera =>
            {
                cabecera.RelativeItem().AlignLeft().Column(columna =>
                {
                    columna.Item().Text($"Oferta: ({Oferta.Datos.Referencia}) {Oferta.Datos.Nombre}")
                        .FontSize(Oferta.TamanoTitulo).SemiBold().FontColor(Oferta.ColorTitulo);

                    if (Oferta.Datos.SolicitadoEl is not null)
                    {
                        columna.Item().Text(text =>
                        {
                            text.Span("Fecha: ").SemiBold();
                            text.Span(((DateTime)Oferta.Datos.SolicitadoEl).ToString("dd-MM-yyyy"));
                        });
                    }
                });

                cabecera.ConstantItem(10);
                cabecera.ConstantItem(Oferta.AnchoLogo).AlignRight().Row(fila =>
                    ApiDeReportes.RenderLogo(fila, Oferta.MostrarLogo ? Oferta.Logo : string.Empty, Oferta.AnchoLogo, Oferta.AnchoLogo, texto: " "));
            });
        }

        private void Cuerpo(IContainer container)
        {
            container.PaddingVertical(30).Column(column =>
            {
                column.Spacing(20);

                column.Item().Row(row =>
                {
                    row.RelativeItem().Element(x => EstilosRpt.Encabezado(x, Oferta.TamanoEncabezado)).Component(new DireccionReporte("De", new Direccion
                    {
                        Empresa = Oferta.Sociedad.RazonSocial,
                        Calle = ModeloDeDto.Reporte.ExtensorDeRpt.Imprimirla(Oferta.Sociedad.DireccionFiscal, Oferta.MostrarCalificadorDireccion),
                        eMail = Oferta.Sociedad.eMail,
                        Telefono = Oferta.Sociedad.Telefono,
                        NIF = Oferta.Sociedad.Nif
                    }));
                    row.ConstantItem(50);
                    row.RelativeItem().Element(x => EstilosRpt.Encabezado(x, Oferta.TamanoEncabezado)).Component(new DireccionReporte("A", new Direccion
                    {
                        Empresa = Oferta.Datos.Contacto.IsNullOrEmpty()
                            ? (Oferta.Cliente.RazonSocial.IsNullOrEmpty() ? Oferta.Cliente.Nombre : Oferta.Cliente.RazonSocial)
                            : Oferta.Datos.Contacto,
                        Calle = ModeloDeDto.Reporte.ExtensorDeRpt.Imprimirla(Oferta.Cliente.DireccionFiscal, Oferta.MostrarCalificadorDireccion),
                        eMail = Oferta.Datos.eMail.IsNullOrEmpty() ? Oferta.Cliente.eMail : Oferta.Datos.eMail,
                        Telefono = Oferta.Datos.Telefono.IsNullOrEmpty() ? Oferta.Cliente.Telefono : Oferta.Datos.Telefono,
                        NIF = Oferta.Cliente.NIF
                    }));
                });

                if (!Oferta.Datos.Descripcion.IsNullOrEmpty())
                    column.Item().Text(Oferta.Datos.Descripcion).FontSize(8).FontColor(Colors.Grey.Darken2);

                column.Item().Element(LineasDeLaOferta);

                column.Item().PaddingRight(5).AlignRight().Text($"Base imponible: {Oferta.Total.Moneda(alineacion: false)}").SemiBold();
            });
        }

        private void LineasDeLaOferta(IContainer container)
        {
            var headerStyle = TextStyle.Default.SemiBold();

            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    if (Oferta.IndicarFila) columns.ConstantColumn(25);
                    columns.RelativeColumn(3);
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    if (Oferta.HayDescuento) columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                table.Header(header =>
                {
                    if (Oferta.IndicarFila) header.Cell().Element(EstilosRpt.Negrita).Text("#");
                    header.Cell().Element(EstilosRpt.Negrita).Text("Material").Style(headerStyle);
                    header.Cell().Element(EstilosRpt.Negrita).AlignRight().Text("Cantidad").Style(headerStyle);
                    header.Cell().Element(EstilosRpt.Negrita).AlignRight().Text("Precio").Style(headerStyle);
                    if (Oferta.HayDescuento) header.Cell().Element(EstilosRpt.Negrita).AlignRight().Text("Dto.").Style(headerStyle);
                    header.Cell().Element(EstilosRpt.Negrita).AlignRight().Text("Base").Style(headerStyle);
                });

                var numeroDeLinea = 1;
                foreach (var linea in Oferta.Lineas)
                {
                    if (Oferta.IndicarFila) table.Cell().Element(EstilosRpt.Celda).Text($"{numeroDeLinea}");
                    table.Cell().Element(EstilosRpt.Celda).PaddingRight(5).Text(linea.Unitario ?? string.Empty);
                    table.Cell().Element(EstilosRpt.Celda).AlignRight().Text(linea.Cantidad.Formatear(decimales: Decimales(linea.Cantidad), alineacion: false));
                    table.Cell().Element(EstilosRpt.Celda).AlignRight().Text(linea.Precio.Moneda(decimales: Decimales(linea.Precio), alineacion: false));
                    if (Oferta.HayDescuento) table.Cell().Element(EstilosRpt.Celda).AlignRight().Text(linea.Descuento.Porcentaje(alineacion: false));
                    table.Cell().Element(EstilosRpt.Celda).AlignRight().Text(linea.Importe.Moneda(alineacion: false));
                    numeroDeLinea++;
                }
            });
        }

        // Cantidades y precios se editan con hasta 6 decimales: se muestran todos los significativos para que cuadren con la base
        private static int Decimales(decimal? valor)
        {
            if (valor is null) return 2;
            var texto = ((decimal)valor).ToString("0.######", System.Globalization.CultureInfo.InvariantCulture);
            var posicionDelPunto = texto.IndexOf('.');
            return posicionDelPunto < 0 ? 2 : Math.Max(2, texto.Length - posicionDelPunto - 1);
        }
    }
}
