using Gestor.Errores;
using Newtonsoft.Json.Linq;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Terceros;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Utilidades;

namespace ServicioDeDatos.Entorno
{

    public enum enumParametrosDeUsuarios
    {
        [Description("Indica los ids de los usuarios parametrizadores")]
        USU_Parametrizadores,
        [Description("Indica los ids de los usuarios que pueden archivar documentación histórica")]
        USU_PuedenArchivarDocumentacionHistorica,
        [Description("Indica los minutos que estará el usuario bloqueado tras errar por teres veces en el cambio de contraseña")]
        USU_TiempoDeBloqeo,
        [Description("Indica la contraseña que se asignará por defecto a un usuario cuando se crea")]
        USU_PaswordPorDefecto,
        [Description("para un usuario se almacenan las ips desde las cuales no se exije el 2FA")]
        USU_Dispositivos2FA
    }

    public class Dispositivos2FA
    {
        public int IdUsuario { get; set; }
        public List<string> Ips { get; set; }
    }

    public class VariableDeUsuario
    {
        private static readonly string _jsonDispositivos2FA = @"[{""IdUsuario"": 0,""Ips"": ""[]""}]";

        public static string PasswordPorDefecto()
        {
            return enumNegocio.Usuario.Parametro(enumParametrosDeUsuarios.USU_PaswordPorDefecto, valorPorDefecto: ltrDeUnUsuario.PasswordPorDefecto).Valor;
        }

        public static List<string> ObtenerDispositivos2FA(ContextoSe contexto, int idUsuario)
        {
            var json = enumNegocio.Usuario.Parametro(enumParametrosDeUsuarios.USU_Dispositivos2FA, crearParametro: true, valorPorDefecto: _jsonDispositivos2FA).Valor;
            if (string.IsNullOrWhiteSpace(json) || json == _jsonDispositivos2FA) 
                return new List<string>();

            var dispositivos = ParsearDispositivos2FA(json);

            if (dispositivos.Count == 0 || (dispositivos.Count == 1 && dispositivos[0].IdUsuario == 0))
            {
                return new List<string>();
            }

            var dispositivosDelUsuario = dispositivos.FirstOrDefault(d => d.IdUsuario == idUsuario);

            return dispositivosDelUsuario?.Ips ?? new List<string>();
        }

        public static void GuardarDispositivos2FA(ContextoSe contexto, int idUsuario, string ip)
        {
            if (ip == Literal.IpNula)
                return;

            var json = enumNegocio.Usuario.Parametro(enumParametrosDeUsuarios.USU_Dispositivos2FA, crearParametro: true, valorPorDefecto: _jsonDispositivos2FA).Valor;
            var dispositivos = ParsearDispositivos2FA(json);

            dispositivos.RemoveAll(d => d.IdUsuario == 0);

            var dispositivosDelUsuario = dispositivos.FirstOrDefault(d => d.IdUsuario == idUsuario);

            if (dispositivosDelUsuario == null)
            {
                dispositivos.Add(new Dispositivos2FA { IdUsuario = idUsuario, Ips = new List<string> { ip } });
                enumNegocio.Usuario.Actualizar(enumParametrosDeUsuarios.USU_Dispositivos2FA, SerializarDispositivos2FA(dispositivos));
            }
            else if (!dispositivosDelUsuario.Ips.Contains(ip))
            {
                dispositivosDelUsuario.Ips.Add(ip);
                enumNegocio.Usuario.Actualizar(enumParametrosDeUsuarios.USU_Dispositivos2FA, SerializarDispositivos2FA(dispositivos));
            }
        }

        private static List<Dispositivos2FA> ParsearDispositivos2FA(string json)
        {
            try
            {
                var jsonArray = JArray.Parse(json);
                return jsonArray.Select(item => new Dispositivos2FA
                {
                    IdUsuario = item["IdUsuario"].Value<int>(),
                    Ips = item["Ips"].Value<string>().Split(',').ToList()
                }).ToList();
            }
            catch (Exception ex)
            {
               throw Excepciones.Emitir($"Error al parsear el json: '{json}' al objeto del tipo '{typeof(Dispositivos2FA).Name}', debe definirlo en el parámetro de negocio '{enumParametrosDeUsuarios.USU_Dispositivos2FA}'", ex);
            }
        }

        private static string SerializarDispositivos2FA(List<Dispositivos2FA> dispositivos)
        {
            var jsonArray = new JArray(dispositivos.Select(d => new JObject
            {
                ["IdUsuario"] = d.IdUsuario,
                ["Ips"] = string.Join(",", d.Ips)
            }));

            return jsonArray.ToString(Newtonsoft.Json.Formatting.None);
        }
    }

    public enum enumParametrosDeMenus
    {
        [Description("Indica las ias disponibles para el entorno")]
        Menu_Ias_Disponibles
    }

    public class IaDeEntorno
    {
        public string Nombre { get; set; }
        public enumIa Enumerado { get; set; }
        public string ApiKey { get; set; }
        public string Modelo { get; set; }
    }

    public class VariableDeMenu
    {

        private static readonly string _jsonIasDisponibles = $"[{{\"Nombre\": \"Geminis\",\"Enumerado\": \"IaGeminis\",\"ApiKey\": \"{ltrIa.ApiKey_NoDefinida}\",\"Modelo\": \"{ltrIa.Modelo_PorDefecto}\"}}, " +
                                                   $"{{\"Nombre\": \"Mistral\",\"Enumerado\": \"IaMistral\",\"ApiKey\": \"{ltrIa.ApiKey_NoDefinida}\",\"Modelo\": \"{ltrIa.Modelo_PorDefecto}\"}}, " +
                                                   $"{{\"Nombre\": \"Perplexity\",\"Enumerado\": \"{enumIa.IaPerplexity.ToString()}\",\"ApiKey\": \"{ltrIa.ApiKey_NoDefinida}\",\"Modelo\": \"{ltrIa.Modelo_PorDefecto}\"}}]";

        public static List<IaDeEntorno> Ias(bool errorSiNoHay = false)
        {
            var json = enumNegocio.Menu.Parametro(enumParametrosDeMenus.Menu_Ias_Disponibles, valorPorDefecto: _jsonIasDisponibles).Valor;
            List<IaDeEntorno> ias = ParsearIas(json);

            if (!ias.Any() && errorSiNoHay)
                GestorDeErrores.Emitir($"Ha de configurar el parámetro '{enumParametrosDeMenus.Menu_Ias_Disponibles.ToString()}' indicando el Nombre, el enumerado de Ia, el Apikey y el Modelo");

            return ias;
        }

        private static List<IaDeEntorno> ParsearIas(string json)
        {
            try
            {
                var jsonArray = JArray.Parse(json);
                return jsonArray.Select(item => new IaDeEntorno
                {
                    Nombre = item["Nombre"].Value<string>(),
                    Enumerado = ApiDeEnsamblados.ToEnumerado<enumIa>(item["Enumerado"].Value<string>()),
                    ApiKey = item["ApiKey"].Value<string>(),
                    Modelo = item["Modelo"].Value<string>(),
                }).ToList();
            }
            catch (Exception ex)
            {
               throw Excepciones.Emitir($"Error al parsear el json: '{json}' al objeto del tipo '{_jsonIasDisponibles}', debe definirlo en el parámetro de negocio '{enumParametrosDeMenus.Menu_Ias_Disponibles.ToString()}'",ex);
            }
        }
    }
}
