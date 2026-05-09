

namespace Utilidades
{
    public static class ltrAppSetting
    {
        public const string UsarBundle = nameof(UsarBundle);
        public static class ServidorDeCorreo
        {
            public const string Sistema = nameof(Sistema);
            public static readonly string Usuario = nameof(Usuario).ToLower();
            public static readonly string Servidor = nameof(Servidor).ToLower();
            public static class ConCredenciales
            {
                public static readonly string sslActivo = nameof(sslActivo);
                public static readonly string Puerto = nameof(Puerto).ToLower();
                public static readonly string Clave = nameof(Clave).ToLower();
            }
            public static class Office365
            {
                public static readonly string TenantId = nameof(TenantId);
                public static readonly string GrantType = nameof(GrantType);
                public static readonly string ClientId = nameof(ClientId);
                public static readonly string ClientSecret = nameof(ClientSecret);
                public static readonly string Scope = nameof(Scope);
            }
            public static class MailJet
            {
                public static readonly string ApiKey = nameof(Office365.ClientId);
                public static readonly string ApiSecret = nameof(Office365.ClientSecret);
                public static readonly string eMailEmisor = Usuario;
            }
            public static class Smtp
            {
                public static readonly string ServidorSmtp = "servidor_smtp";
                public static readonly string ServidorImap = "servidor_imap";
                public static readonly string PuertoSmtp = "puerto_smtp";
                public static readonly string PuertoImap = "puerto_imap";
                public static readonly string Password = ConCredenciales.Clave;
            }
        }

        public static class OpcionesDeEF
        {
            public const string EnableSensitiveDataLogging = nameof(EnableSensitiveDataLogging);
            public const string EnableDetailedErrors = nameof(EnableDetailedErrors);
        }

        public static class DatosIniciales
        {
            public const string AlmacenDocumental = nameof(AlmacenDocumental);
            public const string EjecutorDeLaCola = nameof(EjecutorDeLaCola);
            public const string UrlBase = nameof(UrlBase);
            public const string Protocolo = nameof(Protocolo);
            public const string MiCorreo = nameof(MiCorreo);
            public const string ClienteSecreto = nameof(ClienteSecreto);            
        }
    }
}
