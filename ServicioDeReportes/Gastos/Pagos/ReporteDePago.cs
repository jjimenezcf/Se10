using GestorDeElementos;
using ModeloDeDto;
using ModeloDeDto.Gastos;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ServicioDeDatos.Gastos;
using ServicioDeReportes.Base;
using Utilidades;

namespace ServicioDeReportes.Gastos
{
    public class ReporteDePago : IDocument
    {
        public PagoRpt Pago { get; }

        public ReporteDePago(IInformacionRpt<PagoDto> PagoRpt)
        {
            Pago = (PagoRpt)PagoRpt;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container
                .Page(page =>
                {
                    page.Margin(50);

                    page.Header().Element(CabeceraPago);
                    page.Content().Element(Cuerpo);

                    page.Footer().Component(new PieDePaginaCentrado());
                });
        }

        private void CabeceraPago(IContainer encabezado)
        {
            encabezado.Row(cabecera =>
            {
                cabecera.RelativeItem().Column(columna =>
                {
                    columna
                        .Item().Text($"Pago: {Pago.Datos.Referencia}")
                        .FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);

                    columna.Item().Text(text =>
                    {
                        text.Span("Fecha prevista de pago: ").SemiBold();
                        text.Span($"{Pago.Datos.PagarEl:d}");
                    });

                    columna.Item().Text(text =>
                    {
                        text.Span("Fecha de pago: ").SemiBold();
                        text.Span($"{Pago.Datos.PagadoEl:d}");
                    });
                });

                ApiDeReportes.RenderLogo(cabecera, Pago.Logo);
            });
        }

        void Cuerpo(IContainer contenedor)
        {
            contenedor.PaddingVertical(60).Column(column =>
            {
                column.Spacing(20);

                column.Item().Row(row =>
                {
                    Direcciones(row);
                });

                column.Spacing(20);

                DatosDelPago(column);
            });
        }

        private void DatosDelPago(ColumnDescriptor column)
        {
            column.Item().Text($"Importe: {Pago.Datos.Importe.Moneda()}");
            if (Pago.Datos.Clase == enumClaseDePago.Transferencia)
            {
                column.Item().Text($"Cta. Deudora: {Pago.Datos.CuentaDePago}");
                column.Item().Text($"Cta. Acreedora: {Pago.Datos.CuentaDeAcreedor}");
            }
        }

        private void Direcciones(RowDescriptor row)
        {
            row.RelativeItem().Component(new DireccionReporte("De", new Direccion
            {
                Empresa = Pago.Sociedad.Expresion,
                Calle = Pago.Sociedad.DireccionFiscal,
                eMail = Pago.Sociedad.eMail,
                Telefono = Pago.Sociedad.Telefono,
                NIF = Pago.Sociedad.Nif
            }));
            row.ConstantItem(50);
            row.RelativeItem().Component(new DireccionReporte("A", new Direccion
            {
                Empresa = Pago.Acreedor.Expresion,
                Calle = Pago.Direccion.Expresion,
                eMail = Pago.Datos.eMail,
                Telefono = Pago.Datos.Telefono,
                NIF = Pago.Datos.Nif
            }));
        }

    }
}