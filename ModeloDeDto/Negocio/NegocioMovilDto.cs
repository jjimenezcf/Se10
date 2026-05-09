using System.Collections.Generic;
using ServicioDeDatos.Seguridad;
using Utilidades;

namespace ModeloDeDto.Negocio;

public class NegocioMovilDto
{
    public int Id { get; set; }
    public string Controlador { get; set; }
    public enumNegocio Negocio { get; set; }
    public enumModoDeAccesoDeDatos ModDeAcceso { get; set; }
    //public int Id { get; set; }
    public string Nombre { get; set; }
    //public string Enumerado { get; set; }
    //public enumNegocio ValorDeEnumerado { get; set; }
    //public string Controlador { get; set; }
    //public enumModoDeAccesoDeDatos ValorDeModDeAcceso { get; set; }
    //public string ModDeAcceso { get; set; }
    public List<Parametro> Parametros{ get; set; } = new();
}

