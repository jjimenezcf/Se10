using ServicioDeDatos.Seguridad;

namespace ServicioDeDatos.Elemento
{
    public class NodoDtm : INombre
    {
        public int? IdPadre { get; set; }
        public bool Activo { get; set; }
        public string TipoDtm { get; set; }
        public string Nombre { get; set; }
        public int Id { get; set; }

        public enumModoDeAccesoDeDatos modoAcceso { get; set; } = enumModoDeAccesoDeDatos.Consultor;

        public string Expresion => Nombre;
    }

}
