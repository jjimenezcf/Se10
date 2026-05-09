using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace ServicioDeReportes.Base
{
    public class PieDePaginaCentrado : IComponent
    {

        public PieDePaginaCentrado(float tamanoFont=10, bool mostrarFechaDeImpresion = true)
        {
            TamanoFont = tamanoFont;
            MostrarFechaDeImpresion = mostrarFechaDeImpresion;
        }

        public float TamanoFont { get; }
        public bool MostrarFechaDeImpresion { get; }

        public event EventHandler? Disposed;

        public void Compose(IContainer pie)
        {
            pie.AlignCenter().Text(text =>
            {
                text.Span("Página: ").FontSize(TamanoFont);
                text.CurrentPageNumber().FontSize(TamanoFont);
                text.Span(" de ").FontSize(TamanoFont);
                text.TotalPages().FontSize(TamanoFont);
                if (MostrarFechaDeImpresion) text.Span(" - Impreso el: " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString()).FontSize(TamanoFont);
            });
        }
    }
}
