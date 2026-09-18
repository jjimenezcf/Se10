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

        // Valida el NIF/CIF y la razón social del cliente contra la AEAT antes de darlo de alta o actualizarlo.
        public bool ValidarEnLaAeat { get; set; }
        // Si el cliente ya existe y difieren el nombre/apellidos (persona) o la razón social (sociedad), los actualiza.
        public bool SustituirDatosIdentificativos { get; set; }
        // Si el cliente ya existe y difieren el eMail/Telefono del ClienteDtm, los actualiza.
        public bool SustituirDatosDeContacto { get; set; }
        // Si la calle indicada no existe en el municipio, la crea; si es false, se emite un error.
        public bool CrearCalleSiNoExiste { get; set; }
        // Al crear la calle, exige que exista en el callejero del Catastro (ver GestorDeCalles.AntesDePersistir).
        public bool ValidarEnCatastro { get; set; }

        public static ClienteFacturadorJson Parsear(string clienteJson)
        =>
        JsonConvert.DeserializeObject<ClienteFacturadorJson>(clienteJson);
    }
}
