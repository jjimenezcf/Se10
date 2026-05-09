using ModeloDeDto;
using ModeloDeDto.Presupuesto;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using ServicioDeReportes.Base;
using Utilidades;
using static ServicioDeDatos.Elemento.Enumerados;

namespace ServicioDeReportes.Ventas
{
    public class ReporteDePresupuesto : IDocument
    {
        public PresupuestoRpt Presupuesto { get; }

        public ReporteDePresupuesto(IInformacionRpt<PresupuestoDto> presupuestoRpt)
        {
            Presupuesto = (PresupuestoRpt)presupuestoRpt;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container
                .Page(page =>
                {
                    page.Margin(Presupuesto.Marco);

                    page.Header().Element(CabeceraPresupuesto);
                    page.Content().Element(Cuerpo);

                    page.Footer().Component(new PieDePaginaCentrado(Presupuesto.TamanoPieDePagina, Presupuesto.MostrarImpresoEl));
                });
        }

        private void CabeceraPresupuesto(IContainer encabezado)
        {
            encabezado.Row(cabecera =>
            {
                cabecera.RelativeItem().Column(columna =>
                {
                    columna
                        .Item().Text($"{$"Presupuesto: {Presupuesto.Datos.Referencia}"}")
                        .FontSize(Presupuesto.TamanoTitulo).SemiBold().FontColor(Presupuesto.ColorTitulo);

                    if (Presupuesto.ImprimirFechaCreacion)
                    {
                        columna.Item().Text(text =>
                        {
                            text.Span("Fecha de creación: ").SemiBold();
                            text.Span(Presupuesto.Datos.CreadoEl.ToString("dd-MM-yyyy"));
                        });
                    }

                    columna.Item().Text(text =>
                    {
                        text.Span("Fecha de emisión: ").SemiBold();
                        text.Span(DateTime.Now.ToString("dd-MM-yyyy"));
                    });

                    columna.Item().Text(text =>
                    {
                        text.Span("Asunto: ").SemiBold();
                        text.Span(Presupuesto.Datos.Nombre.ToString());
                    });
                });
                ApiDeReportes.RenderLogo(cabecera, Presupuesto.Logo, Presupuesto.AnchoLogo, Presupuesto.AltoLogo, Presupuesto.Sociedad.Nif);
            });
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

                table.Cell().Element(x => ApiDeReportes.MargenIzquierdo(x, Presupuesto.InscritoEn, Presupuesto.PaddingMargenIzquierdo));
                table.Cell().Element(ContenidoDelCuerpo);

                table.Cell().ColumnSpan(2).PaddingTop(Presupuesto.PaddingTopPie).Element(x => ApiDeReportes.ComentarioEnPie(x, Presupuesto.PieDePresupuesto));
            });
        }

        private void ContenidoDelCuerpo(IContainer container)
        {
            container.PaddingVertical(60).Column(column =>
            {
                column.Spacing(20);

                column.Item().Row(row =>
                {
                    row.RelativeItem().Element(x => EstilosRpt.Encabezado(x, Presupuesto.TamanoEncabezado)).Component(new DireccionReporte("De", new Direccion
                    {
                        Empresa = Presupuesto.Sociedad.RazonSocial,
                        Calle = ModeloDeDto.Reporte.ExtensorDeRpt.Imprimirla(Presupuesto.Sociedad.DireccionFiscal, Presupuesto.MostrarCalificadorDireccion),
                        eMail = Presupuesto.Sociedad.eMail,
                        Telefono = Presupuesto.Sociedad.Telefono,
                        NIF = Presupuesto.Sociedad.Nif
                    }));
                    row.ConstantItem(50);
                    row.RelativeItem().Element(x => EstilosRpt.Encabezado(x, Presupuesto.TamanoEncabezado)).Component(new DireccionReporte("A", new Direccion
                    {
                        Empresa = Presupuesto.Datos.Contacto,
                        Calle = ModeloDeDto.Reporte.ExtensorDeRpt.Imprimir(Presupuesto.Direccion, Presupuesto.MostrarCalificadorDireccion),
                        eMail = Presupuesto.Datos.eMail,
                        Telefono = Presupuesto.Datos.Telefono,
                        NIF = Presupuesto.Solicitante.NIF
                    })); 
                });

                column.Item().Element(LineasPresupuesto);

                column.Item().PaddingRight(5).AlignRight().Text($"Base imponible: {Presupuesto.Datos.TotalSinIva.Moneda()}").SemiBold();

                foreach (var iva in Presupuesto.Ivas)
                    column.Item().PaddingRight(5).AlignRight().Text($"{iva.Tipo}: {iva.Importe.Moneda()}");

                column.Item().PaddingRight(5).AlignRight().Text($"Total a pagar: {Presupuesto.Datos.TotalConIva.Moneda()}").SemiBold();
            });
        }

        void LineasPresupuesto(IContainer container)
        {
            var headerStyle = TextStyle.Default.SemiBold();

            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    if (Presupuesto.IndicarFila) columns.ConstantColumn(25);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    if (Presupuesto.HayDescuento) columns.RelativeColumn();
                });

                table.Header(header =>
                {
                    if (Presupuesto.IndicarFila) header.Cell().Element(EstilosRpt.Negrita).Text("#");
                    header.Cell().Element(EstilosRpt.Negrita).Text("Concepto").Style(headerStyle);
                    header.Cell().Element(EstilosRpt.Negrita).AlignRight().Text("Precio").Style(headerStyle);
                    header.Cell().Element(EstilosRpt.Negrita).AlignRight().Text("Cantidad").Style(headerStyle);
                    if (Presupuesto.HayDescuento) header.Cell().Element(EstilosRpt.Negrita).AlignRight().Text("Descuento").Style(headerStyle);
                    header.Cell().Element(EstilosRpt.Negrita).AlignRight().Text("Base").Style(headerStyle);
                });

                var numeroDelinea = 1;
                foreach (var linea in Presupuesto.Lineas)
                {
                    var index = Presupuesto.Lineas.IndexOf(linea) + 1;
                    var impConDto =
                      linea.ImporteSinDto is null
                      ? 0M
                      : (decimal)linea.ImporteSinDto - (linea.ImporteDeDto is null ? 0M : (decimal)linea.ImporteDeDto)
                    ;

                    var span = (uint)(Presupuesto.HayDescuento ? 4 : 3);
                    if (!Presupuesto.IndicarFila) span = span - 1;
                    if (linea.TipoDeLinea == enumTipoDeLinea.Comentario.ToString())
                    {
                        table.Cell().Element(EstilosRpt.Celda).Text(string.Empty);
                        table.Cell().ColumnSpan(span).Element(EstilosRpt.Cometarios).Text(linea.Concepto + $"{(linea.Anotacion.IsNullOrEmpty() ? "" : $"{Environment.NewLine}{linea.Anotacion}")}");
                        table.Cell().Element(EstilosRpt.Celda).Text(string.Empty);
                    }
                    else
                    {
                        if (Presupuesto.IndicarFila) table.Cell().Element(linea.Anotacion.IsNullOrEmpty() ? EstilosRpt.Celda : EstilosRpt.CeldaSinBordeAbajo).Text($"{numeroDelinea}");
                        table.Cell().Element(linea.Anotacion.IsNullOrEmpty() ? EstilosRpt.Celda : EstilosRpt.CeldaSinBordeAbajo).Text(linea.Concepto);
                        table.Cell().Element(linea.Anotacion.IsNullOrEmpty() ? EstilosRpt.Celda : EstilosRpt.CeldaSinBordeAbajo).AlignRight().Text($"{linea.Precio.Moneda()}");
                        table.Cell().Element(linea.Anotacion.IsNullOrEmpty() ? EstilosRpt.Celda : EstilosRpt.CeldaSinBordeAbajo).AlignRight().Text($"{linea.Cantidad.Formatear()}");
                        if (Presupuesto.HayDescuento) table.Cell().Element(linea.Anotacion.IsNullOrEmpty() ? EstilosRpt.Celda : EstilosRpt.CeldaSinBordeAbajo).AlignRight().Text($"{linea.Descuento.Porcentaje()}");
                        table.Cell().Element(linea.Anotacion.IsNullOrEmpty() ? EstilosRpt.Celda : EstilosRpt.CeldaSinBordeAbajo).AlignRight().Text(impConDto.Moneda());

                        if (!linea.Anotacion.IsNullOrEmpty())
                        {
                            if (Presupuesto.IndicarFila) table.Cell().Element(EstilosRpt.Celda).Text(string.Empty);
                            table.Cell().ColumnSpan(span).Element(EstilosRpt.Cometarios).Text(linea.Anotacion);
                            table.Cell().Element(EstilosRpt.Celda).Text(string.Empty);
                            if (!Presupuesto.IndicarFila) table.Cell().Element(EstilosRpt.Celda).Text(string.Empty);
                        }

                        numeroDelinea++;
                    }
                }
            });
        }

    }


}