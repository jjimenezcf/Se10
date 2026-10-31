using ModeloDeDto;
using ModeloDeDto.Logistica;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ServicioDeReportes.Base;
using Utilidades;

namespace ServicioDeReportes.Logistica
{
    // Impreso de las devoluciones de un pedido: la cabecera y el pie del pedido (ver ReporteDePedido) y en el cuerpo lo devuelto
    public class ReporteDeDevolucionDePedido : ReporteDePedido
    {
        private DevolucionDePedidoRpt Devolucion => (DevolucionDePedidoRpt)Pedido;

        public ReporteDeDevolucionDePedido(IInformacionRpt<PedidoDto> devolucionRpt) : base(devolucionRpt)
        {
        }

        protected override void Cuerpo(IContainer container)
        {
            container.PaddingVertical(30).Column(column =>
            {
                column.Spacing(20);

                column.Item().Element(DatosDeLaDevolucion);
                column.Item().Element(LineasDevueltas);
                column.Item().AlignRight().Text($"Total devuelto (sin IVA): {Devolucion.Total.Moneda(alineacion: false)}").SemiBold();
            });
        }

        private void DatosDeLaDevolucion(IContainer container)
        {
            container.Column(columna =>
            {
                columna.Item().Text($"Devolución: {Devolucion.Datos.Nombre}").FontSize(Devolucion.TamanoTitulo).SemiBold().FontColor(Devolucion.ColorTitulo);

                columna.Item().Text(text =>
                {
                    text.Span("Referencia: ").SemiBold();
                    text.Span(Devolucion.Datos.Referencia);
                });
            });
        }

        // Cada devolución con su fecha, lo devuelto valorado al precio y descuento de la entrega y, debajo, el motivo
        private void LineasDevueltas(IContainer container)
        {
            var headerStyle = TextStyle.Default.SemiBold();
            var hayDescuento = Devolucion.HayDescuentoEnLoDevuelto;
            var columnasDeDatos = (uint)(hayDescuento ? 6 : 5);

            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    if (Devolucion.IndicarFila) columns.ConstantColumn(25);
                    columns.RelativeColumn();
                    columns.RelativeColumn(3);
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    if (hayDescuento) columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                table.Header(header =>
                {
                    if (Devolucion.IndicarFila) header.Cell().Element(EstilosRpt.Negrita).Text("#");
                    header.Cell().Element(EstilosRpt.Negrita).Text("Devuelto el").Style(headerStyle);
                    header.Cell().Element(EstilosRpt.Negrita).Text("Concepto").Style(headerStyle);
                    header.Cell().Element(EstilosRpt.Negrita).AlignRight().Text("Cantidad").Style(headerStyle);
                    header.Cell().Element(EstilosRpt.Negrita).AlignRight().Text("Precio").Style(headerStyle);
                    if (hayDescuento) header.Cell().Element(EstilosRpt.Negrita).AlignRight().Text("Dto.").Style(headerStyle);
                    header.Cell().Element(EstilosRpt.Negrita).AlignRight().Text("Importe").Style(headerStyle);
                });

                var numeroDeLinea = 1;
                foreach (var devolucion in Devolucion.Devoluciones)
                {
                    Func<IContainer, IContainer> celda = devolucion.Motivo.IsNullOrEmpty() ? EstilosRpt.Celda : EstilosRpt.CeldaSinBordeAbajo;

                    if (Devolucion.IndicarFila) table.Cell().Element(celda).Text($"{numeroDeLinea}");
                    table.Cell().Element(celda).Text(devolucion.DevueltaEl.ToString("dd-MM-yyyy"));
                    table.Cell().Element(celda).PaddingRight(5).Text(devolucion.Concepto ?? string.Empty);
                    table.Cell().Element(celda).AlignRight().Text(devolucion.Cantidad.Formatear(decimales: Decimales(devolucion.Cantidad), alineacion: false));
                    table.Cell().Element(celda).AlignRight().Text(devolucion.Precio.Moneda(decimales: Decimales(devolucion.Precio), alineacion: false));
                    if (hayDescuento) table.Cell().Element(celda).AlignRight().Text(devolucion.Descuento.Porcentaje(alineacion: false));
                    table.Cell().Element(celda).AlignRight().Text(devolucion.Importe.Moneda(alineacion: false));

                    if (!devolucion.Motivo.IsNullOrEmpty())
                    {
                        if (Devolucion.IndicarFila) table.Cell().Element(EstilosRpt.Celda).Text(string.Empty);
                        table.Cell().ColumnSpan(columnasDeDatos).Element(EstilosRpt.Cometarios).Text($"Motivo: {devolucion.Motivo}");
                    }

                    numeroDeLinea++;
                }
            });
        }
    }
}
