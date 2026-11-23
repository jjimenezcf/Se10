using ModeloDeDto;
using ModeloDeDto.Logistica;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ServicioDeReportes.Base;
using Utilidades;

namespace ServicioDeReportes.Logistica
{
    // Documento de una devolución de venta, como la oferta del documento de venta (ReporteDeOfertaDeVenta) sin iva: título
    // "Devolución: (referencia) nombre", quién recibe la devolución y quién devuelve, las líneas (documento de venta y material) y el total
    public class ReporteDeDevolucionDeVenta : IDocument
    {
        public DevolucionDeVentaRpt Devolucion { get; }

        public ReporteDeDevolucionDeVenta(IInformacionRpt<DevolucionDeVentaDto> devolucionRpt)
        {
            Devolucion = (DevolucionDeVentaRpt)devolucionRpt;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container
                .Page(page =>
                {
                    page.Margin(Devolucion.Marco);

                    page.Header().Element(Cabecera);
                    page.Content().Element(Cuerpo);

                    page.Footer().Component(new PieDePaginaCentrado(Devolucion.TamanoPieDePagina, Devolucion.MostrarImpresoEl));
                });
        }

        private void Cabecera(IContainer encabezado)
        {
            encabezado.Row(cabecera =>
            {
                cabecera.RelativeItem().AlignLeft().Column(columna =>
                {
                    columna.Item().Text($"Devolución: ({Devolucion.Datos.Referencia}) {Devolucion.Datos.Nombre}")
                        .FontSize(Devolucion.TamanoTitulo).SemiBold().FontColor(Devolucion.ColorTitulo);

                    if (Devolucion.Datos.DevueltaEl is not null)
                    {
                        columna.Item().Text(text =>
                        {
                            text.Span("Fecha: ").SemiBold();
                            text.Span(((DateTime)Devolucion.Datos.DevueltaEl).ToString("dd-MM-yyyy"));
                        });
                    }
                });

                cabecera.ConstantItem(10);
                cabecera.ConstantItem(Devolucion.AnchoLogo).AlignRight().Row(fila =>
                    ApiDeReportes.RenderLogo(fila, Devolucion.MostrarLogo ? Devolucion.Logo : string.Empty, Devolucion.AnchoLogo, Devolucion.AnchoLogo, texto: " "));
            });
        }

        private void Cuerpo(IContainer container)
        {
            container.PaddingVertical(30).Column(column =>
            {
                column.Spacing(20);

                column.Item().Row(row =>
                {
                    row.RelativeItem().Element(x => EstilosRpt.Encabezado(x, Devolucion.TamanoEncabezado)).Component(new DireccionReporte("Recibe", new Direccion
                    {
                        Empresa = Devolucion.Sociedad.RazonSocial,
                        Calle = ModeloDeDto.Reporte.ExtensorDeRpt.Imprimirla(Devolucion.Sociedad.DireccionFiscal, Devolucion.MostrarCalificadorDireccion),
                        eMail = Devolucion.Sociedad.eMail,
                        Telefono = Devolucion.Sociedad.Telefono,
                        NIF = Devolucion.Sociedad.Nif
                    }));
                    row.ConstantItem(50);
                    row.RelativeItem().Element(x => EstilosRpt.Encabezado(x, Devolucion.TamanoEncabezado)).Component(new DireccionReporte("Devuelve", new Direccion
                    {
                        Empresa = Devolucion.Datos.Contacto.IsNullOrEmpty()
                            ? (Devolucion.Cliente.RazonSocial.IsNullOrEmpty() ? Devolucion.Cliente.Nombre : Devolucion.Cliente.RazonSocial)
                            : Devolucion.Datos.Contacto,
                        Calle = ModeloDeDto.Reporte.ExtensorDeRpt.Imprimirla(Devolucion.Cliente.DireccionFiscal, Devolucion.MostrarCalificadorDireccion),
                        eMail = Devolucion.Datos.eMail.IsNullOrEmpty() ? Devolucion.Cliente.eMail : Devolucion.Datos.eMail,
                        Telefono = Devolucion.Datos.Telefono.IsNullOrEmpty() ? Devolucion.Cliente.Telefono : Devolucion.Datos.Telefono,
                        NIF = Devolucion.Cliente.NIF
                    }));
                });

                if (!Devolucion.Datos.Descripcion.IsNullOrEmpty())
                    column.Item().Text(Devolucion.Datos.Descripcion).FontSize(8).FontColor(Colors.Grey.Darken2);

                column.Item().Element(LineasDeLaDevolucion);

                column.Item().PaddingRight(5).AlignRight().Text($"Base imponible devuelta: {Devolucion.Total.Moneda(alineacion: false)}").SemiBold();
            });
        }

        private void LineasDeLaDevolucion(IContainer container)
        {
            var headerStyle = TextStyle.Default.SemiBold();

            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    if (Devolucion.IndicarFila) columns.ConstantColumn(25);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(3);
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    if (Devolucion.HayDescuento) columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                table.Header(header =>
                {
                    if (Devolucion.IndicarFila) header.Cell().Element(EstilosRpt.Negrita).Text("#");
                    header.Cell().Element(EstilosRpt.Negrita).Text("Documento de venta").Style(headerStyle);
                    header.Cell().Element(EstilosRpt.Negrita).Text("Material").Style(headerStyle);
                    header.Cell().Element(EstilosRpt.Negrita).AlignRight().Text("Cantidad").Style(headerStyle);
                    header.Cell().Element(EstilosRpt.Negrita).AlignRight().Text("Precio").Style(headerStyle);
                    if (Devolucion.HayDescuento) header.Cell().Element(EstilosRpt.Negrita).AlignRight().Text("Dto.").Style(headerStyle);
                    header.Cell().Element(EstilosRpt.Negrita).AlignRight().Text("Base").Style(headerStyle);
                });

                var numeroDeLinea = 1;
                foreach (var linea in Devolucion.Lineas)
                {
                    if (Devolucion.IndicarFila) table.Cell().Element(EstilosRpt.Celda).Text($"{numeroDeLinea}");
                    table.Cell().Element(EstilosRpt.Celda).PaddingRight(5).Text(linea.Venta ?? string.Empty);
                    table.Cell().Element(EstilosRpt.Celda).PaddingRight(5).Text(linea.LineaDeVenta ?? string.Empty);
                    table.Cell().Element(EstilosRpt.Celda).AlignRight().Text(linea.Cantidad.Formatear(decimales: Decimales(linea.Cantidad), alineacion: false));
                    table.Cell().Element(EstilosRpt.Celda).AlignRight().Text(linea.Precio.Moneda(decimales: Decimales(linea.Precio), alineacion: false));
                    if (Devolucion.HayDescuento) table.Cell().Element(EstilosRpt.Celda).AlignRight().Text(linea.Descuento.Porcentaje(alineacion: false));
                    table.Cell().Element(EstilosRpt.Celda).AlignRight().Text(linea.Importe.Moneda(alineacion: false));
                    numeroDeLinea++;
                }
            });
        }

        private static int Decimales(decimal? valor)
        {
            if (valor is null) return 2;
            var texto = ((decimal)valor).ToString("0.######", System.Globalization.CultureInfo.InvariantCulture);
            var posicionDelPunto = texto.IndexOf('.');
            return posicionDelPunto < 0 ? 2 : Math.Max(2, texto.Length - posicionDelPunto - 1);
        }
    }
}
