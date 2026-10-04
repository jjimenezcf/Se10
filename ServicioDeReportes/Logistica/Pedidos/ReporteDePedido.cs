using ModeloDeDto;
using ModeloDeDto.Logistica;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ServicioDeReportes.Base;
using Utilidades;
using static ServicioDeDatos.Elemento.Enumerados;

namespace ServicioDeReportes.Logistica
{
    public class ReporteDePedido : IDocument
    {
        public PedidoRpt Pedido { get; }

        public ReporteDePedido(IInformacionRpt<PedidoDto> pedidoRpt)
        {
            Pedido = (PedidoRpt)pedidoRpt;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container
                .Page(page =>
                {
                    page.Margin(Pedido.Marco);

                    page.Header().Element(CabeceraPedido);
                    page.Content().Element(Cuerpo);

                    page.Footer().Component(new PieDePaginaCentrado(Pedido.TamanoPieDePagina, Pedido.MostrarImpresoEl));
                });
        }

        // Alto de una línea de texto (fuente Lato) por cada punto de tamaño de fuente y separación entre líneas de los datos de la cabecera
        private const float AltoDeLineaPorPunto = 1.2F;
        private const float EspacioEntreLineas = 2F;

        // A la izquierda la sociedad que pide, con su logo delante ocupando el alto de sus datos, y a la derecha el proveedor al que se pide
        private void CabeceraPedido(IContainer encabezado)
        {
            var solicitante = new List<string>
            {
                Pedido.Sociedad.RazonSocial,
                ModeloDeDto.Reporte.ExtensorDeRpt.Imprimirla(Pedido.Sociedad.DireccionFiscal, Pedido.MostrarCalificadorDireccion),
                Pedido.Sociedad.eMail,
                Pedido.Sociedad.Telefono,
                Pedido.Sociedad.Nif
            }.Where(linea => !linea.IsNullOrEmpty()).ToList();

            var proveedor = new List<string>
            {
                Pedido.Proveedor.RazonSocial.IsNullOrEmpty() ? Pedido.Proveedor.Nombre : Pedido.Proveedor.RazonSocial,
                ModeloDeDto.Reporte.ExtensorDeRpt.Imprimirla(Pedido.Proveedor.DireccionFiscal, Pedido.MostrarCalificadorDireccion),
                Pedido.Datos.eMail.IsNullOrEmpty() ? Pedido.Proveedor.eMail : Pedido.Datos.eMail,
                Pedido.Datos.Telefono.IsNullOrEmpty() ? Pedido.Proveedor.Telefono : Pedido.Datos.Telefono,
                Pedido.Proveedor.NIF
            }.Where(linea => !linea.IsNullOrEmpty()).ToList();

            var logo = Pedido.MostrarLogo ? ApiDeImagenes.RecortarMargenes(Pedido.Logo) : null;
            var altoDelLogo = solicitante.Count * Pedido.TamanoEncabezado * AltoDeLineaPorPunto + (solicitante.Count - 1) * EspacioEntreLineas;
            var anchoDelLogo = logo is null ? 0F : Math.Min(altoDelLogo * logo.Value.Proporcion, Pedido.AnchoLogo);

            encabezado.Element(x => EstilosRpt.Encabezado(x, Pedido.TamanoEncabezado)).Table(tabla =>
            {
                tabla.ColumnsDefinition(columnas =>
                {
                    if (logo is not null)
                    {
                        columnas.ConstantColumn(anchoDelLogo);
                        columnas.ConstantColumn(10);
                    }
                    columnas.RelativeColumn();
                    columnas.ConstantColumn(30);
                    columnas.RelativeColumn();
                });

                tabla.Cell().ColumnSpan(logo is null ? 1u : 3u).Element(x => Titulo(x, "Solicitante"));
                tabla.Cell();
                tabla.Cell().Element(x => Titulo(x, "Proveedor"));

                if (logo is not null)
                {
                    tabla.Cell().Height(altoDelLogo).AlignMiddle().Image(logo.Value.Imagen).FitArea();
                    tabla.Cell();
                }
                tabla.Cell().Element(x => Lineas(x, solicitante));
                tabla.Cell();
                tabla.Cell().Element(x => Lineas(x, proveedor));
            });
        }

        private static void Titulo(IContainer container, string titulo)
        {
            container.Column(columna =>
            {
                columna.Spacing(EspacioEntreLineas);
                columna.Item().Text(titulo).SemiBold();
                columna.Item().PaddingBottom(5).LineHorizontal(1);
            });
        }

        private static void Lineas(IContainer container, List<string> lineas)
        {
            container.Column(columna =>
            {
                columna.Spacing(EspacioEntreLineas);
                foreach (var linea in lineas)
                    columna.Item().Text(linea);
            });
        }

        private void Cuerpo(IContainer container)
        {
            container.PaddingVertical(30).Column(column =>
            {
                column.Spacing(20);

                column.Item().Element(DatosDelPedido);
                column.Item().Element(LineasDelPedido);
                column.Item().AlignRight().Text($"Total del pedido (sin IVA): {Pedido.Total.Moneda(alineacion: false)}").SemiBold();
            });
        }

        private void DatosDelPedido(IContainer container)
        {
            container.Column(columna =>
            {
                columna.Item().Text($"Pedido: {Pedido.Datos.Nombre}").FontSize(Pedido.TamanoTitulo).SemiBold().FontColor(Pedido.ColorTitulo);

                columna.Item().Text(text =>
                {
                    text.Span("Referencia: ").SemiBold();
                    text.Span(Pedido.Datos.Referencia);

                    if (Pedido.Datos.PedidoEl is not null)
                    {
                        text.Span("    Fecha de pedido: ").SemiBold();
                        text.Span(((DateTime)Pedido.Datos.PedidoEl).ToString("dd-MM-yyyy"));
                    }

                    if (Pedido.Datos.EntregarEl is not null)
                    {
                        text.Span("    Entregar el: ").SemiBold();
                        text.Span(((DateTime)Pedido.Datos.EntregarEl).ToString("dd-MM-yyyy"));
                    }
                });

                if (!Pedido.Datos.Descripcion.IsNullOrEmpty())
                    columna.Item().PaddingTop(5).Text(Pedido.Datos.Descripcion).FontSize(8).FontColor(Colors.Grey.Darken2);
            });
        }

        private void LineasDelPedido(IContainer container)
        {
            var headerStyle = TextStyle.Default.SemiBold();
            var columnasDeDatos = (uint)(Pedido.HayDescuento ? 6 : 5);

            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    if (Pedido.IndicarFila) columns.ConstantColumn(25);
                    columns.RelativeColumn(3);
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    if (Pedido.HayDescuento) columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                table.Header(header =>
                {
                    if (Pedido.IndicarFila) header.Cell().Element(EstilosRpt.Negrita).Text("#");
                    header.Cell().Element(EstilosRpt.Negrita).Text("Concepto").Style(headerStyle);
                    header.Cell().Element(EstilosRpt.Negrita).Text("Unidad").Style(headerStyle);
                    header.Cell().Element(EstilosRpt.Negrita).AlignRight().Text("Cantidad").Style(headerStyle);
                    header.Cell().Element(EstilosRpt.Negrita).AlignRight().Text("Tarifa").Style(headerStyle);
                    if (Pedido.HayDescuento) header.Cell().Element(EstilosRpt.Negrita).AlignRight().Text("Dto.").Style(headerStyle);
                    header.Cell().Element(EstilosRpt.Negrita).AlignRight().Text("Importe").Style(headerStyle);
                });

                var numeroDeLinea = 1;
                foreach (var linea in Pedido.Lineas)
                {
                    if (linea.TipoDeLinea == enumTipoDeLinea.Comentario.ToString())
                    {
                        if (Pedido.IndicarFila) table.Cell().Element(EstilosRpt.Celda).Text(string.Empty);
                        table.Cell().ColumnSpan(columnasDeDatos).Element(EstilosRpt.Cometarios).Text(linea.Concepto + $"{(linea.Anotacion.IsNullOrEmpty() ? "" : $"{Environment.NewLine}{linea.Anotacion}")}");
                        continue;
                    }

                    Func<IContainer, IContainer> celda = linea.Anotacion.IsNullOrEmpty() ? EstilosRpt.Celda : EstilosRpt.CeldaSinBordeAbajo;

                    if (Pedido.IndicarFila) table.Cell().Element(celda).Text($"{numeroDeLinea}");
                    table.Cell().Element(celda).PaddingRight(5).Text(linea.Concepto ?? string.Empty);
                    table.Cell().Element(celda).PaddingRight(5).Text(linea.Unidad ?? string.Empty);
                    table.Cell().Element(celda).AlignRight().Text(linea.Cantidad.Formatear(decimales: Decimales(linea.Cantidad), alineacion: false));
                    table.Cell().Element(celda).AlignRight().Text(linea.Precio.Moneda(decimales: Decimales(linea.Precio), alineacion: false));
                    if (Pedido.HayDescuento) table.Cell().Element(celda).AlignRight().Text(linea.Descuento.Porcentaje(alineacion: false));
                    table.Cell().Element(celda).AlignRight().Text(linea.ImporteDeLinea.Moneda(alineacion: false));

                    if (!linea.Anotacion.IsNullOrEmpty())
                    {
                        if (Pedido.IndicarFila) table.Cell().Element(EstilosRpt.Celda).Text(string.Empty);
                        table.Cell().ColumnSpan(columnasDeDatos).Element(EstilosRpt.Cometarios).Text(linea.Anotacion);
                    }

                    numeroDeLinea++;
                }
            });
        }

        // Tarifas y cantidades se editan con hasta 6 decimales: se muestran todos los significativos para que cuadren con el importe
        private static int Decimales(decimal? valor)
        {
            if (valor is null) return 2;
            var texto = ((decimal)valor).ToString("0.######", System.Globalization.CultureInfo.InvariantCulture);
            var posicionDelPunto = texto.IndexOf('.');
            return posicionDelPunto < 0 ? 2 : Math.Max(2, texto.Length - posicionDelPunto - 1);
        }

    }
}
