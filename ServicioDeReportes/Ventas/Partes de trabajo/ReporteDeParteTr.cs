using Azure;
using ModeloDeDto;
using ModeloDeDto.Ventas;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ServicioDeReportes.Base;
using Utilidades;

namespace ServicioDeReportes.Ventas
{
    public class ReporteDePartesTr : IDocument
    {
        public ParteTrRpt Parte { get; }
        public string Plantilla { get; }

        private bool MostrarImportes => new ParteTrValoradoPlt().Plantilla.Equals(Plantilla);

        public ReporteDePartesTr(IInformacionRpt<ParteTrDto> parteRpt, string plantilla)
        {
            Parte = (ParteTrRpt)parteRpt;
            Plantilla = plantilla;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container
                .Page(page =>
                {
                    page.Margin(50);

                    page.Header().Element(Cabecera);
                    page.Content().Element(Cuerpo);

                    page.Footer().Component(new PieDePaginaCentrado());
                });
        }

        private void Cabecera(IContainer encabezado)
        {
            encabezado.Row(cabecera =>
            {
                cabecera.RelativeItem().Column(columna =>
                {
                    columna
                        .Item().Text($"{enumNegocio.ParteDeTrabajo.Singular()} :{Parte.Datos.Referencia}")
                        .FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);

                    columna.Item().Text(text =>
                    {
                        text.Span("CG: ").SemiBold();
                        text.Span($"{Parte.Datos.Cg}");
                    });

                    columna.Item().Text(text =>
                    {
                        text.Span("Tipo: ").SemiBold();
                        text.Span($"{Parte.Datos.Tipo}");
                    });

                    columna.Item().Text(text =>
                    {
                        text.Span("Estado: ").SemiBold();
                        text.Span($"{Parte.Datos.Estado}");
                    });

                });

                ApiDeReportes.RenderLogo(cabecera, Parte.Logo);
            });
        }

        void Cuerpo(IContainer container)
        {
            container.PaddingVertical(60).Column(column =>
            {
                column.Spacing(20);

                column.Item().Row(row =>
                {
                    row.RelativeItem().Component(new DireccionReporte("De", new Direccion { 
                        Empresa = Parte.Sociedad.Expresion, 
                        Calle = Parte.Sociedad.DireccionFiscal,
                        eMail = Parte.Sociedad.eMail,
                        Telefono = Parte.Sociedad.Telefono ,
                        NIF = Parte.Sociedad.Nif}));
                    row.ConstantItem(50);
                    row.RelativeItem().Component(new DireccionReporte("A", new Direccion
                    {
                        Empresa = Parte.Cliente.Expresion,
                        Calle = Parte.Direccion.Expresion,
                        eMail = Parte.Cliente.eMail,
                        Telefono = Parte.Cliente.Telefono,
                        NIF = Parte.Cliente.NIF
                    }));
                });

                column.Item().Element(LineasDeParte);
                if (MostrarImportes)
                {
                    column.Item().PaddingRight(5).AlignRight().Text($"Base imponible: {Parte.Datos.TotalSinIva.Moneda()}").SemiBold();

                    column.Item().PaddingRight(5).AlignRight().Text($"Total a pagar: {Parte.Datos.TotalConIva.Moneda()}").SemiBold();
                }

                column.Item().PaddingTop(25).Element(Comentario);
            });
        }

        void LineasDeParte(IContainer container)
        {
            var headerStyle = TextStyle.Default.SemiBold();

            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(25);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn();
                    if (MostrarImportes)
                    {
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                    }
                });

                table.Header(header =>
                {
                    header.Cell().Element(CellStyle).Text("#");
                    header.Cell().Element(CellStyle).Text("Concepto").Style(headerStyle);
                    header.Cell().Element(CellStyle).AlignRight().Text("Cantidad").Style(headerStyle);
                    if (MostrarImportes)
                    {
                        header.Cell().Element(CellStyle).AlignRight().Text("Precio").Style(headerStyle);
                        header.Cell().Element(CellStyle).AlignRight().Text("Descuento").Style(headerStyle);
                        header.Cell().Element(CellStyle).AlignRight().Text("Base").Style(headerStyle);
                    }


                    static IContainer CellStyle(IContainer container)
                    {
                        return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                    }

                    //header.Cell().ColumnSpan(1).PaddingTop(5).BorderBottom(1).BorderColor(Colors.Black);
                });

                foreach (var linea in Parte.Lineas)
                {
                    var index = Parte.Lineas.IndexOf(linea) + 1;
                    var impConDto =  
                      linea.ImporteSinDto  is null 
                      ? 0M 
                      : (decimal)linea.ImporteSinDto - (linea.ImporteDeDto is null ? 0M : (decimal)linea.ImporteDeDto)
                    ;
                    table.Cell().Element(CellStyle).Text($"{index}");
                    table.Cell().Element(CellStyle).Text(linea.Concepto);
                    table.Cell().Element(CellStyle).AlignRight().Text($"{linea.Cantidad.Formatear()}");
                    if (MostrarImportes)
                    {
                        table.Cell().Element(CellStyle).AlignRight().Text($"{linea.Precio.Moneda()}");
                        table.Cell().Element(CellStyle).AlignRight().Text($"{linea.Descuento.Porcentaje()}");
                        table.Cell().Element(CellStyle).AlignRight().Text(impConDto.Moneda());
                    }

                    static IContainer CellStyle(IContainer container) => container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                }
            });
        }

        void Comentario(IContainer container)
        {
            container.ShowEntire().Background(Colors.Grey.Lighten3).Padding(10).Column(column =>
            {
                column.Spacing(5);
                column.Item().Text("Observación").FontSize(14).SemiBold();
                column.Item().Text("Firmar a su realización por el cliente");
            });
        }

    }

}