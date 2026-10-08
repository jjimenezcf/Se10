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
        // archivo que representa al nodo (p.e. el logo de una sociedad), si la consulta lo devuelve
        public int? IdArchivo { get; set; }
        // fichero de /images/menu que representa al nodo (p.e. el icono de un menú), si la consulta lo devuelve
        public string Icono { get; set; }

        public enumModoDeAccesoDeDatos modoAcceso { get; set; } = enumModoDeAccesoDeDatos.Consultor;

        public string Expresion => Nombre;
    }

}
