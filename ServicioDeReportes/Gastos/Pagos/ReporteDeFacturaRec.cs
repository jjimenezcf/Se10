using GestorDeElementos;
using ModeloDeDto;
using ModeloDeDto.Gastos;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ServicioDeReportes.Base;
using Utilidades;

namespace ServicioDeReportes.Gastos
{
    public class ReporteDeFacturaRec : IDocument
    {
        public FacturaRecRpt Factura { get; }

        public ReporteDeFacturaRec(IInformacionRpt<FacturaRecDto> facturaRpt)
        {
            Factura = (FacturaRecRpt)facturaRpt;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container
                .Page(page =>
                {
                    page.Margin(50);

                    page.Header().Element(CabeceraFactura);
                    page.Content().Element(Cuerpo);

                    page.Footer().Component(new PieDePaginaCentrado());
                });
        }

        private void CabeceraFactura(IContainer encabezado)
        {
            encabezado.Row(cabecera =>
            {
                cabecera.RelativeItem().Column(columna =>
                {
                    columna
                        .Item().Text($"Factura recibida: {Factura.Datos.Referencia}")
                        .FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);

                    columna.Item().Text(text =>
                    {
                        text.Span("Fecha de emisión: ").SemiBold();
                        text.Span($"{Factura.Datos.FacturadaEl:d}");
                    });

                    columna.Item().Text(text =>
                    {
                        text.Span("Fecha de vencimiento: ").SemiBold();
                        text.Span($"{Factura.Datos.VenceEl:d}");
                    });
                });

                ApiDeReportes.RenderLogo(cabecera, Factura.Logo);
            });
        }

        void Cuerpo(IContainer container)
        {
            container.PaddingVertical(60).Column(column =>
            {
                column.Spacing(20);

                column.Item().Row(row =>
                {
                    row.RelativeItem().Component(new DireccionReporte("De", new Direccion
                    {
                        Empresa = Factura.Proveedor.Expresion,
                        Calle = Factura.Direccion?.Expresion ?? "Sin dirección fiscal",
                        eMail = Factura.Proveedor.eMail,
                        Telefono = Factura.Proveedor.Telefono,
                        NIF = Factura.Proveedor.NIF
                    }));
                    row.ConstantItem(50);
                    row.RelativeItem().Component(new DireccionReporte("A", new Direccion
                    {
                        Empresa = Factura.Sociedad.Expresion,
                        Calle = Factura.Sociedad.DireccionFiscal,
                        eMail = Factura.Sociedad.eMail,
                        Telefono = Factura.Sociedad.Telefono,
                        NIF = Factura.Sociedad.Nif
                    }));
                });

                column.Item().PaddingRight(5).AlignRight().Text($"Base imponible: {Factura.Datos.BaseImponible.Moneda()}").SemiBold();

                foreach (var iva in Factura.Ivas)
                    column.Item().PaddingRight(5).AlignRight().Text($"{iva.Tipo}: {iva.Importe.Moneda()}");

                column.Item().PaddingRight(5).AlignRight().Text($"Total a pagar: {Factura.Datos.TotalDelPago.Moneda()}").SemiBold();

                column.Item().Element(LineasDeFactura);

                column.Item().Element(Historial);
            });
        }

        void LineasDeFactura(IContainer container)
        {
            var headerStyle = TextStyle.Default.SemiBold();

            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(25);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn();
                    columns.RelativeColumn(); 
                    columns.ConstantColumn(8);
                    columns.RelativeColumn();
                });

                table.Header(header =>
                {
                    header.Cell().Element(CellStyle).Text("#");
                    header.Cell().Element(CellStyle).Text("Concepto").Style(headerStyle);
                    header.Cell().Element(CellStyle).AlignRight().Text("BI").Style(headerStyle);
                    header.Cell().Element(CellStyle).AlignRight().Text("%").Style(headerStyle); 
                    header.Cell().Element(CellStyle).Height(8).PaddingVertical(0); 
                    header.Cell().Element(CellStyle).AlignRight().Text("Clase").Style(headerStyle);


                    static IContainer CellStyle(IContainer container)
                    {
                        return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                    }

                });
                
                foreach (var linea in Factura.Lineas)
                {
                    var bi = linea.BaseImponible is null ? 0M : (decimal)linea.BaseImponible;
                    var porcentaje = bi == 0M 
                    ? null
                    : linea.PorcentajeIva is not null && linea.PorcentajeIva > 0
                    ? linea.PorcentajeIva
                    : linea.PorcentajeIrpf is not null && linea.PorcentajeIrpf > 0
                    ? linea.PorcentajeIrpf
                    : null;

                    table.Cell().Element(CellStyle).Text($"{linea.Orden}");
                    table.Cell().Element(CellStyle).DefaultTextStyle(x => x.FontSize(8)).Text(linea.Concepto);
                    table.Cell().Element(CellStyle).AlignRight().Text(bi.Moneda());
                    table.Cell().Element(CellStyle).AlignRight().Text($"{(porcentaje is null ? "" : porcentaje.Porcentaje())}");
                    table.Cell().Element(CellStyle).Height(8).PaddingVertical(0);
                    table.Cell().Element(CellStyle).AlignRight().Text($"{(porcentaje is null ? "" : linea.DescripcionDeClase)}");

                    static IContainer CellStyle(IContainer container) => container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                }
                
            });
        }

        void Historial(IContainer container)
        {
            var headerStyle = TextStyle.Default.SemiBold();

            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn(2);
                });

                table.Header(header =>
                {
                    header.Cell().Element(CellStyle).Text("Fecha").Style(headerStyle);
                    header.Cell().Element(CellStyle).Text("Usuario").Style(headerStyle);
                    header.Cell().Element(CellStyle).Text("Clase").Style(headerStyle);
                    header.Cell().Element(CellStyle).Text("Asunto").Style(headerStyle);
                    header.Cell().Element(CellStyle).Text("Descripción").Style(headerStyle);


                    static IContainer CellStyle(IContainer container)
                    {
                        return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                    }

                });

                foreach (var hito in Factura.Historial)
                {
                    table.Cell().Element(CellStyle).Text($"{hito.CreadaEl}");
                    table.Cell().Element(CellStyle).Text($"{hito.Creador}");
                    table.Cell().Element(CellStyle).Text($"{hito.Clase}");
                    table.Cell().Element(CellStyle).Text($"{hito.Nombre}");
                    table.Cell().Element(CellStyle).Text($"{hito.Descripcion}");

                    static IContainer CellStyle(IContainer container) => container.DefaultTextStyle(x => x.FontSize(8)).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                }

            });
        }

    }


}