using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace ServicioDeReportes.Base
{

    public class Direccion
    {
        public string? Empresa { get; set; }
        public string? Calle { get; set; }
        public string? Provincia { get; set; }
        public string? eMail { get; set; }
        public string? Telefono { get; set; }
        public string? NIF { get; set; }
    }

    public class DireccionReporte : IComponent
    {
        private string _Titulo { get; }
        private Direccion _Direccion { get; }
        public DireccionReporte(string title, Direccion direccion)
        {
            _Titulo = title;
            _Direccion = direccion;
        }

        public event EventHandler? Disposed;

        public void Compose(IContainer container)
        {
            container.ShowEntire().Column(column =>
            {
                column.Spacing(2);

                column.Item().Text(_Titulo).SemiBold();
                column.Item().PaddingBottom(5).LineHorizontal(1);

                column.Item().Text(_Direccion.Empresa);
                column.Item().Text(_Direccion.Calle);
                column.Item().Text(_Direccion.Provincia);
                column.Item().Text(_Direccion.eMail);
                column.Item().Text(_Direccion.Telefono);
                column.Item().Text(_Direccion.NIF);
            });
        }

    }
}
