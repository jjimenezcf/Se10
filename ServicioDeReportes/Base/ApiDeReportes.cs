using GestorDeElementos;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ServicioDeReportes.Base
{
    public static class EstilosRpt
    {
        public static IContainer Negrita(IContainer container) => container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
        public static IContainer Celda(IContainer container) => container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
        public static IContainer CeldaSinBordeAbajo(IContainer container) => container.PaddingVertical(5);
        public static IContainer Cometarios(IContainer container) => container.DefaultTextStyle(estilo => estilo.FontSize(8)).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
        public static IContainer Resumen(IContainer container, int fontSize) => container.DefaultTextStyle(estilo => estilo.FontSize(fontSize)).PaddingVertical(5);
        public static IContainer Vertical(IContainer container) => container.DefaultTextStyle(estilo => estilo.FontSize(6));
        public static IContainer Encabezado(IContainer container, float tamano) => container.DefaultTextStyle(estilo => estilo.FontSize(tamano));

    }

    public static class ApiDeReportes
    {
        public static void RenderLogo(RowDescriptor descriptorDeFila, string logo, float anchoLogo = 100F, float altoLogo = 50F, string texto = " ")
        {
            try
            {
                descriptorDeFila.RelativeItem().AlignRight().Column(columna =>
                {
                    columna.Item().AlignCenter().Text(texto).FontSize(7).FontColor(Colors.Grey.Darken2);
                    if (File.Exists(logo))
                        columna.Item().AlignCenter().Width(anchoLogo).Height(altoLogo).Image(logo).FitArea();
                    else if (logo != ApiDeArchivos.FicheroNoEncontrado && File.Exists(ApiDeArchivos.FicheroNoEncontrado))
                        columna.Item().AlignCenter().Width(85.04f).Height(85.04f).Image(ApiDeArchivos.FicheroNoEncontrado).FitArea();
                });
            }
            catch
            {
            }
        }

        public static void MargenIzquierdo(IContainer container, string texto, float padding = 20F)
        {
            container.PaddingVertical(padding).RotateLeft().Element(EstilosRpt.Vertical).AlignCenter().Text(texto);
        }


        public static void ComentarioEnPie(IContainer container, string comentario)
        {
            container.ShowEntire().Background(Colors.Grey.Lighten3).Padding(10).Column(column =>
            {
                column.Spacing(5);
                //column.Item().Text("Observación").FontSize(14).SemiBold();
                column.Item().Element(EstilosRpt.Cometarios).Text(comentario);
            });
        }

    }
}
