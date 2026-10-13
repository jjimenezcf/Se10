

namespace Utilidades
{
    public static class ltrAppSetting
    {
        public const string UsarBundle = nameof(UsarBundle);
        public const string Usar2F = nameof(Usar2F);
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

        // Sección con la configuración de cada canal de mensajes web (ver IServicioDeMensajesWeb), una subsección por canal con el nombre del canal:
        //   "ServidorDeMensajes": {
        //     "WHATSAPP": { "PhoneNumberId": "", "Token": "", "Version": "v21.0", "Plantilla": "", "Idioma": "es" },
        //     "TELEGRAM": { "BotToken": "", "ChatIds": { "34600123456": "123456789" } },
        //     "SMS":      { "Proveedor": "TWILIO", "AccountSid": "", "AuthToken": "", "Remitente": "" }
        //   }
        // Qué canal y quién envía (cliente o servidor) lo indican las variables CFG_Mensajeria_Canal y CFG_Mensajeria_Modo
        public static class ServidorDeMensajes
        {
            public const string Seccion = nameof(ServidorDeMensajes);
            public static class Whatsapp
            {
                public const string PhoneNumberId = nameof(PhoneNumberId);
                public const string Token = nameof(Token);
                public const string Version = nameof(Version);
                public const string Plantilla = nameof(Plantilla);
                public const string Idioma = nameof(Idioma);
            }
            public static class Telegram
            {
                public const string BotToken = nameof(BotToken);
                public const string ChatIds = nameof(ChatIds);
            }
            public static class Sms
            {
                public const string Proveedor = nameof(Proveedor);
                public const string AccountSid = nameof(AccountSid);
                public const string AuthToken = nameof(AuthToken);
                public const string Remitente = nameof(Remitente);
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
            public const string ServidorDeCorreo = nameof(ServidorDeCorreo);
            public const string Host = nameof(Host);
            public const string Protocolo = nameof(Protocolo);
            public const string MiCorreo = nameof(MiCorreo);
            public const string ClienteSecreto = nameof(ClienteSecreto);
        }
    }
}
