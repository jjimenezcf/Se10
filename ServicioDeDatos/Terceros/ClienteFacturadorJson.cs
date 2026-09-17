using Newtonsoft.Json;

namespace ServicioDeDatos.Terceros
{
    public class ClienteFacturadorJson
    {
        public string TipoDeCliente { get; set; }
        public string NIF { get; set; }
        public string Nombre { get; set; }
        public string Apellidos { get; set; }
        public string eMail { get; set; }
        public string Telefono { get; set; }

        public string Municipio { get; set; }
        public string CodigoPostal { get; set; }
        public string TipoDeVia { get; set; }
        public string Calle { get; set; }
        public int Numero { get; set; }

        public static ClienteFacturadorJson Parsear(string clienteJson)
        =>
        JsonConvert.DeserializeObject<ClienteFacturadorJson>(clienteJson);
    }
}
