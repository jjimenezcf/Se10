using Mailjet.Client.Resources;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Threading.Tasks;
using Utilidades;
using static ServicioDeCorreos.Oficce365;

namespace ServicioDeCorreos
{
    public enum enumSistemaDeCorreo { GRAPH, CON_CREDENCIALES, SIN_CREDENCIALES, MAILJET, SMTP }
    public class ServicioDeCorreo
    {
        public static bool EnviandoCorreo { get; set; } = false;

        public static int CorreoEnviados { get; set; } = 0;

        private SmtpClient _SmtpCliente;

        private IDistribuidorDeCorreos _Distribuidor;
        private IConfigurationSection ServidorDeCorreo { get; set; }

        private enumSistemaDeCorreo enumSistema => ApiDeEnsamblados.ToEnumerado<enumSistemaDeCorreo>(ServidorDeCorreo[ltrAppSetting.ServidorDeCorreo.Sistema].ToUpper());
        public string Emisor => ServidorDeCorreo[ltrAppSetting.ServidorDeCorreo.Usuario];
        private string Servidor => ServidorDeCorreo[ltrAppSetting.ServidorDeCorreo.Servidor];
        private bool SSL => ServidorDeCorreo[ltrAppSetting.ServidorDeCorreo.ConCredenciales.sslActivo].EsTrue();
        private int Puerto => ServidorDeCorreo[ltrAppSetting.ServidorDeCorreo.ConCredenciales.Puerto].Entero();
        private string Password => ServidorDeCorreo[ltrAppSetting.ServidorDeCorreo.ConCredenciales.Clave];

        private string TenantId => ServidorDeCorreo[ltrAppSetting.ServidorDeCorreo.Office365.TenantId];
        private string GrantType => ServidorDeCorreo[ltrAppSetting.ServidorDeCorreo.Office365.GrantType];
        private string ClientId => ServidorDeCorreo[ltrAppSetting.ServidorDeCorreo.Office365.ClientId];
        private string ClientSecret => ServidorDeCorreo[ltrAppSetting.ServidorDeCorreo.Office365.ClientSecret];
        private string Scope => ServidorDeCorreo[ltrAppSetting.ServidorDeCorreo.Office365.Scope];


        private string ServidorSmtp => ServidorDeCorreo[ltrAppSetting.ServidorDeCorreo.Smtp.ServidorSmtp];
        private string ServidorImap => ServidorDeCorreo[ltrAppSetting.ServidorDeCorreo.Smtp.ServidorImap];
        private int PuertoSmtp => ServidorDeCorreo[ltrAppSetting.ServidorDeCorreo.Smtp.PuertoSmtp].Entero();
        private int PuertoImap => ServidorDeCorreo[ltrAppSetting.ServidorDeCorreo.Smtp.PuertoImap].Entero();

        private string eMailEmisor => Emisor;
        private string ApiKey => ClientId;
        private string ApiSecret => ClientSecret;

        private Oficce365.Tenant TenanteOficce365
        {
            get
            {
                return new Oficce365.Tenant
                {
                    tenantId = TenantId,
                    grantType = GrantType,
                    clientId = ClientId,
                    clientSecret = ClientSecret,
                    scope = Scope,
                };
            }
        }


        private MailJet.Tenante TenanteMailJet
        {
            get
            {
                return new MailJet.Tenante
                {
                    tenantId = eMailEmisor,
                    clientId = ApiKey,
                    clientSecret = ApiSecret,
                };
            }
        }

        private SmtpMail.Tenante TenanteSmtp
        {
            get
            {
                return new SmtpMail.Tenante
                {
                    clientId = eMailEmisor,
                    clientSecret = Password,
                    ServidorSmtp = ServidorSmtp,
                    PuertoSmtp = PuertoSmtp,
                    ServidorImap = ServidorImap,
                    PuertoImap = PuertoImap,
                };
            }
        }

        public ServicioDeCorreo(string servidor)
        {
            InicializaConfiguracion(servidor);
            switch (enumSistema)
            {
                case enumSistemaDeCorreo.SIN_CREDENCIALES:
                    _SmtpCliente = new SmtpClient(Servidor);
                    break;
                case enumSistemaDeCorreo.CON_CREDENCIALES:
                    _SmtpCliente = new SmtpClient(Servidor, Puerto)
                    {
                        EnableSsl = SSL,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        UseDefaultCredentials = false,
                        Credentials = new NetworkCredential(Emisor, Password),
                        TargetName = $"STARTTLS/{Servidor}"
                    };
                    break;
                case enumSistemaDeCorreo.GRAPH:
                    break;
                case enumSistemaDeCorreo.MAILJET:
                    break;
                case enumSistemaDeCorreo.SMTP:
                    break;
                default: throw new Exception($"Sistema de correo {enumSistema} no definido");
            }
        }


        public ServicioDeCorreo(string servidor, IDistribuidorDeCorreos distribuidor)
        : this(servidor)
        {
            _Distribuidor = distribuidor;
        }


        private void InicializaConfiguracion(string servidor)
        {
            var generador = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                 .AddJsonFile("appsettings.json");
            var configuration = generador.Build();
            ServidorDeCorreo = configuration.GetSection(typeof(ltrAppSetting.ServidorDeCorreo).Name).GetSection(servidor);
        }

        public void EnviarPara(string login, List<string> receptores, string asunto, string mensaje, bool esHtlm, List<string> archivos, ManejadorDeCorreo manejador)
        =>
        EnviarDe(login, Emisor, receptores, asunto, mensaje, esHtlm, archivos, manejador).Wait();

        public async Task EnviarDe(string login, string emisor, List<string> receptores, string asunto, string mensaje, bool esHtlm, List<string> archivos, ManejadorDeCorreo manejador)
        {

            if (enumSistema != enumSistemaDeCorreo.SIN_CREDENCIALES && enumSistema != enumSistemaDeCorreo.CON_CREDENCIALES &&
                enumSistema != enumSistemaDeCorreo.GRAPH && enumSistema != enumSistemaDeCorreo.MAILJET && enumSistema != enumSistemaDeCorreo.SMTP)
                throw new Exception($"Sistema de correo '{enumSistema}' no definido");

            if (!emisor.Split("@")[0].Equals(Emisor.Split("@")[0], StringComparison.CurrentCultureIgnoreCase))
            {
                mensaje = mensaje + Environment.NewLine + $"Enviado por: '{login}: {emisor}'";
                emisor = Emisor;
            }
            var dejarElReceptore = receptores.Count == 1 && receptores[0] == emisor;
            
            if (!dejarElReceptore)
                receptores.RemoveAll(item => item == emisor);
            
            receptores = receptores.Select(r => r.Trim()).Distinct().ToList();
            if (receptores.Count > 1)
            {
                mensaje = mensaje + Environment.NewLine + $"Receptores: '{string.Join(Simbolos.separadorDeCorreos, receptores)}'";
            }

            if (esHtlm) mensaje = extCadenas.ConvertPlainTextToHtml(mensaje);

            if (enumSistema == enumSistemaDeCorreo.GRAPH)
            {
                CorreoEnviados++;
                if (CorreoEnviados > 4)
                {
                    Task.Delay(20000).Wait();
                    CorreoEnviados = 0;
                }

                await EnviarCorreo(TenanteOficce365, emisor, receptores, asunto, mensaje, esHtlm, archivos, manejador);
                return;
            }

            if (enumSistema == enumSistemaDeCorreo.MAILJET)
            {
                asunto = $"({emisor}) {asunto}";
                await EnviarCorreo(TenanteMailJet, TenanteMailJet.eMailEmisor, receptores, asunto, mensaje, esHtlm, archivos, manejador);
                return;
            }

            if (enumSistema == enumSistemaDeCorreo.SMTP)
            {
                asunto = $"({emisor}) {asunto}";
                await EnviarCorreo(TenanteSmtp, TenanteSmtp.clientId, receptores, asunto, mensaje, esHtlm, archivos, manejador);
                return;
            }

            var destinos = receptores.ToString(";");
            if (destinos.IsNullOrEmpty())
            {
                throw new Exception($"No se ha definido el destinatario del correo '{asunto}'");
            }

            MailMessage email = new MailMessage(new MailAddress(emisor), new MailAddress(destinos))
            {
                BodyEncoding = System.Text.Encoding.UTF8,
                IsBodyHtml = esHtlm,
                SubjectEncoding = System.Text.Encoding.UTF8,
                Subject = asunto,
                Body = mensaje

            };

            if (archivos != null)
                foreach (var archivo in archivos)
                {
                    var attach = new Attachment(archivo);
                    email.Attachments.Add(attach);
                }

            _SmtpCliente.SendCompleted += new SendCompletedEventHandler(DespuesDeEnviarElCorreo);

            _SmtpCliente.SendAsync(email, manejador);
        }

        private async Task EnviarCorreo(ITenant tenant, string emisor, List<string> receptores, string asunto, string mensaje, bool esHtlm, List<string> archivos, ManejadorDeCorreo manejador)
        {
            AsyncCompletedEventArgs args = null;
            try
            {
                foreach (var receptor in receptores)
                {
                    await _Distribuidor.EnviarAsyn(tenant,
                        emisor,
                        receptor,
                        asunto,
                        mensaje,
                        archivos);
                }
                args = new AsyncCompletedEventArgs(null, false, manejador);
            }
            catch (Exception ex)
            {
                args = new AsyncCompletedEventArgs(ex, false, manejador);
            }
            finally
            {
                DespuesDeEnviarElCorreo(this, args);
            }
        }

        private static void DespuesDeEnviarElCorreo(object sender, AsyncCompletedEventArgs e)
        {
            EnviandoCorreo = true;
            var manejador = (ManejadorDeCorreo)e.UserState;
            if (manejador != null)
                try
                {
                    if (e.Cancelled)
                    {
                        manejador.GestorDeCorreo.InvokeMember("AnotarTraza", BindingFlags.InvokeMethod, null, null, new object[] { manejador.Contexto, manejador.CorreoDtm, "Se ha cancelado el envío de correos" });
                    }
                    if (e.Error != null)
                    {
                        manejador.GestorDeCorreo.InvokeMember("AnotarExcepcion", BindingFlags.InvokeMethod, null, null, new object[] { manejador.Contexto, manejador.CorreoDtm, e.Error });
                    }
                    else
                    {
                        manejador.GestorDeCorreo.InvokeMember("IndicarQueElCorreoHaSidoEnviado", BindingFlags.InvokeMethod, null, null, new object[] { manejador.Contexto, manejador.CorreoDtm });
                    }
                }
                finally
                {
                    EnviandoCorreo = false;
                }
            else
            {

            }
        }

        public class ManejadorDeCorreo
        {
            public Type GestorDeCorreo;
            public object Contexto;
            public object CorreoDtm;
        }
        public async Task<int> EliminarCorreos(int anterioresA)
        {
            if (enumSistema != enumSistemaDeCorreo.SIN_CREDENCIALES && enumSistema != enumSistemaDeCorreo.CON_CREDENCIALES &&
                enumSistema != enumSistemaDeCorreo.GRAPH && enumSistema != enumSistemaDeCorreo.MAILJET && enumSistema != enumSistemaDeCorreo.SMTP)
                throw new Exception($"Sistema de correo '{enumSistema}' no definido");

            if (enumSistema == enumSistemaDeCorreo.GRAPH)
            {
                return 0;
            }

            if (enumSistema == enumSistemaDeCorreo.MAILJET)
            {
                return 0;
            }

            if (enumSistema == enumSistemaDeCorreo.SMTP)
            {
                int correosBorrados = await ((IDistribuidorSmtp)_Distribuidor).EliminarAsyn(TenanteSmtp, anterioresA);
                return correosBorrados;
            }

            return 0;
        }


    }


}
