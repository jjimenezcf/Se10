
namespace ModeloDeDto.Terceros
{
    public class NegociosDeUnCgDto: ElementoDto
    {
        public int IdCg { get; set; }
        public int IdNegocio { get; set; }
        public string Negocio { get; set; }
        public string Gestor { get; set; }
        public string Consultor { get; set; }
    }
}
