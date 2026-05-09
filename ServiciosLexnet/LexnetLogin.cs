using GestorDeElementos;
using GestorDeElementos.Extensores;
using Lexnet.Login;
using ServicioDeDatos;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Terceros;
using System.ServiceModel;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Lexnet
{
    public class LexNetLogin
    {
        private ContextoSe _contexto { get; }

        private readonly string _idAplicacion;


        // Propiedad para guardar la sesión activa
        public string IdSesion { get; private set; } = string.Empty;
        public CertificadoDtm _certificadoDtm { get; }

        private string _password = string.Empty;

        private Certificado _certificado;


        public LexNetLogin(ContextoSe contexto, SociedadDtm sociedad)
        {
            _contexto = contexto;
            _certificadoDtm = ExtensorDeSociedades.ObtenerCertificado(sociedad, contexto);
            var datosLogin = contexto.SeleccionarPorId<SociedadDtm>(sociedad.Id).LexnetLogin();

            _idAplicacion = datosLogin.IdAplicacion;


            _password = ApiDeCertificados.LeerPasswordDeCertificado(_contexto, _certificadoDtm.Id);
            _certificado = ApiDeCertificados.ObtenerCertificado(_contexto, _certificadoDtm.Id, _password); 
        }


        public void Desconectar()
        {
            IdSesion = string.Empty;
            if (_certificado != null)
                _certificado.Dispose();
        }
        public async Task<string> Conectar()
        {
            // 1. Configuración del Binding (HTTPS con certificado)
            var binding = new BasicHttpBinding(BasicHttpSecurityMode.Transport);
            binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Certificate;

            // 2. la URL de Integración
            string urlLexnet = "https://intlexnetws.justicia.es/services/ws/SolicitudLoginToken";
            var endpoint = new EndpointAddress(urlLexnet);

            // 3. Instanciamos el cliente
            var client = new SolicitudLoginTokenPortTypeClient(binding, endpoint);

            // Adjuntamos tu certificado para la autenticación mutua SSL
            client.ClientCredentials.ClientCertificate.Certificate = _certificado.X509Certificado;

            try
            {
                string xmlSolicitud = $@"
               <solicitudLoginServicios xmlns=""https://wslexnet.webservices.lexnet/3.27"">
                   <idAplicacion>{_idAplicacion}</idAplicacion>
                   <rolDefecto>true</rolDefecto>
                   <firmaUsuario></firmaUsuario>
               </solicitudLoginServicios>";

                // 5. Empaquetamos en el objeto Request del Connected Service
                var request = new SolicitudLoginTokenRequest(xmlSolicitud.Trim());

                // 6. Llamada al servicio
                var response = await client.SolicitudLoginTokenAsync(request.SolicitudLoginTokenIn);

                // 7. Extraemos el idSesion del XML de salida (SolicitudLoginTokenOut)
                this.IdSesion = ExtraerIdSesion(response.SolicitudLoginTokenOut);

                if (string.IsNullOrEmpty(this.IdSesion))
                {
                    throw new Exception("El servidor no devolvió un IdSesion válido. Revisa el XML de respuesta: " + response.SolicitudLoginTokenOut);
                }

                return this.IdSesion;
            }
            catch (FaultException fex)
            {
                // Errores propios de SOAP
                throw new Exception($"Error SOAP de LexNET: {fex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error de conexión con la URL {urlLexnet}: {ex.Message}", ex);
            }
            finally
            {
                if (client.State == CommunicationState.Opened)
                    await client.CloseAsync();
            }
        }

        private string ExtraerIdSesion(string xmlOut)
        {
            try
            {
                var doc = XDocument.Parse(xmlOut);
                // Buscamos el nodo idSesion ignorando el namespace para evitar fallos por prefijos
                return doc.Descendants().FirstOrDefault(x => x.Name.LocalName == "idSesion")?.Value ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private string GenerarSolicitudLoginXml()
        {
            return string.Empty;
        }


        public void ProcesarRespuestaLogin(string xmlRespuesta)
        {

        }

    }
}