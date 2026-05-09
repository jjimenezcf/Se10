using Gestor.Errores;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json.Linq;
using ServicioDeDatos.Negocio;
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
        USU_PaswordPorDefecto
    }

    public class VariableDeUsuario
    {
        public static string PasswordPorDefecto()
        {
            return enumNegocio.Usuario.Parametro(enumParametrosDeUsuarios.USU_PaswordPorDefecto, valorPorDefecto: ltrDeUnUsuario.PasswordPorDefecto).Valor;
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
