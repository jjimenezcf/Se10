using System;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace Utilidades
{
    public class IaPerplexity : IIa, IIaPromptFactura, IIaPromptResumen, IDisposable, IIaPromptFiltrar
    {
        private enum enumModelosPerplexity
        {
            [Description("sonar")] sonar,
            [Description("sonar-pro")] sonar_pro,
            [Description("sonar-legal")] sonar_legal,
            [Description("sonar-medical")] sonar_medical,
            [Description("sonar-code")] sonar_code,
            [Description("sonar-creative")] sonar_creative,
            [Description("sonar-technical")] sonar_technical,
            [Description("sonar-technical-pro")] sonar_technical_pro
        };

        private string _apiKey { get; }
        private enumModelosPerplexity _modelo { get; }

        private string _apiChat = "https://api.perplexity.ai/chat/completions";

        public string PromptFactura { get; set; }
        public string PromptResumen { get; set; }
        public string PromptFiltrar { get; set; }

        private HttpClient _cliente;

        private HttpClient Cliente
        {
            get
            {
                _cliente = new HttpClient();
                _cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(ltrIa.Bearer, _apiKey);
                _cliente.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MimeTypeMap.ApplicationJson));
                return _cliente;
            }
        }


        public IaPerplexity(string apiKey, string modelo)
        {
            if (apiKey == ltrIa.ApiKey_NoDefinida)
                throw new Exception(IIa.IA_ApiKey_No_Valida);

            _apiKey = apiKey;
            if (modelo == ltrIa.Modelo_PorDefecto) _modelo = enumModelosPerplexity.sonar_pro;
            else
            {
                enumModelosPerplexity? enumModelo = ApiDeEnsamblados.DescripcionToEnumerado<enumModelosPerplexity>(modelo, null);
                if (enumModelo == null)
                    throw new Exception(IIa.IA_Modelo_No_Valido + $": El modelo '{modelo}' no es válido para la ia de '{nameof(IaPerplexity)}'");

                _modelo = (enumModelosPerplexity)enumModelo;
            }
                        
            bool valida = Task.Run(() => IsApiKeyValid()).Result;
            if (!valida)
                throw new Exception($"{IIa.IA_ApiKey_No_Valida} con un valor válido");
        }

        public async Task<bool> IsApiKeyValid()
        {

            var data = new
            {
                model = _modelo.Descripcion(),
                messages = new[]
                {
                    new { role = "system", content = "Be precise and concise." },
                    new { role = "user", content = "Is my API key valid?" }
                }
            };

            string jsonData = JsonConvert.SerializeObject(data);
            var content = new StringContent(jsonData, Encoding.UTF8, MimeTypeMap.ApplicationJson);

            try
            {
                HttpResponseMessage response = await Cliente.PostAsync(_apiChat, content);
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> Resumir(string promptConElContenido)
        {
            var requestBody = new
            {
                model = _modelo.Descripcion(), // Use the most relevant model
                messages = new[] { new { role = "user", content = promptConElContenido } },
                max_tokens = 500 // Adjust as needed
            };

            var json = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, Encoding.UTF8, MimeTypeMap.ApplicationJson);

            var response = await Cliente.PostAsync(_apiChat, content);

            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error al llamar a la API de Perplexity. Código de estado: {response.StatusCode}, Contenido del error: {errorContent}");
            }

            string responseBody = await response.Content.ReadAsStringAsync();

            try
            {
                JObject jsonResponse = JObject.Parse(responseBody);
                // Adjust the path to the summarized text based on the API response structure
                if (jsonResponse["choices"] != null && jsonResponse["choices"].Type == JTokenType.Array && jsonResponse["choices"].Count() > 0)
                {
                    JToken firstChoice = jsonResponse["choices"][0];
                    if (firstChoice["message"] != null && firstChoice["message"]["content"] != null)
                    {
                        return LimpiarRespuesta(firstChoice["message"]["content"].ToString());
                    }
                }
                throw new Exception("No se pudo encontrar el resumen en la respuesta de la API de Perplexity.");
            }
            catch (JsonReaderException ex)
            {
                throw new Exception($"Error al analizar la respuesta JSON: {ex.Message}, Respuesta recibida: {responseBody}");
            }

        }

        public async Task<string> AnalizarFactura(string invoiceContent)
        {

            string prompt = $"Del contenido que te adjunto entre los los 5 guiones {Environment.NewLine}-----{Environment.NewLine}{invoiceContent}{Environment.NewLine}-----{Environment.NewLine}{PromptFactura}";

            var requestBody = new
            {
                model = _modelo.Descripcion(),
                messages = new[]
                {
                        new { role = "system", content = "Be precise and concise." },
                        new { role = "user", content = prompt }
                    },
                response_format = new
                {
                    type = "json_schema",
                    json_schema = new
                    {
                        schema = new // Este subcampo es obligatorio según el mensaje de error
                        {
                            type = "object",
                            properties = new
                            {
                                Proveedor = new { type = "string" },
                                Nif = new { type = "string" },
                                eMail = new { type = "string" },
                                Telefono = new { type = "string" },
                                NumeroFactura = new { type = "string" },
                                Concepto = new { type = "string" },
                                fecha = new { type = "string" },
                                FechaVencimiento = new { type = "string" },
                                total = new { type = "number" },
                                bi = new { type = "number" },
                                totalIva = new { type = "number" },
                                totalIrpf = new { type = "number" },
                                FormaDePago = new { type = "string" },
                                ClaseDePago = new { type = "string" },
                                CuentaBancaria = new { type = "string" },
                                CodigoPostal = new { type = "string" },
                                Pais = new { type = "string" },
                                Provincia = new { type = "string" },
                                Municipio = new { type = "string" },
                                TipodeVia = new { type = "string" },
                                Calle = new { type = "string" },
                                NumeroPolicia = new { type = "string" },
                                RestoDireccion = new { type = "string" }
                            },
                            required = new[] { "Proveedor", "Nif","eMail", "Telefono",  "NumeroFactura",  "Concepto",
                                    "fecha","FechaVencimiento", "total", "bi", "totalIva", "totalIrpf",
                                    "FormaDePago", "ClaseDePago","CuentaBancaria",
                                    "CodigoPostal", "Pais", "Provincia", "Municipio", "TipodeVia", "Calle", "NumeroPolicia", "RestoDireccion"
                                }
                        }
                    }
                },
                max_tokens = 1000,
                temperature = 0.0
            };


            var json = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, Encoding.UTF8, MimeTypeMap.ApplicationJson);

            var response = await Cliente.PostAsync(_apiChat, content);

            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error: Status Code: {response.StatusCode}, Error Content: {errorContent}");
            }

            string responseBody = await response.Content.ReadAsStringAsync();

            try
            {
                JObject jsonResponse = JObject.Parse(responseBody);
                if (jsonResponse["choices"] != null && jsonResponse["choices"].Type == JTokenType.Array && jsonResponse["choices"].Count() > 0)
                {
                    JToken firstChoice = jsonResponse["choices"][0];
                    if (firstChoice["message"] != null && firstChoice["message"]["content"] != null)
                    {
                        string jsonResult = LimpiarRespuesta(firstChoice["message"]["content"].ToString().Trim());

                        try
                        {
                            JObject.Parse(jsonResult);
                            return jsonResult;
                        }
                        catch (JsonReaderException)
                        {
                            throw new Exception("El Json devuelto no es válido." + Environment.NewLine + jsonResult);
                        }
                    }
                }
                throw new Exception("No 'choices' or 'message' found in Perplexity API response.");
            }
            catch (JsonReaderException ex)
            {
                throw new Exception($"Error parsing Perplexity JSON response: {ex.Message}, Response body: {responseBody}");
            }
        }

        private static string LimpiarRespuesta(string jsonResult)
        {
            if (jsonResult.StartsWith("\"") && jsonResult.EndsWith("\""))
            {
                jsonResult = jsonResult.Substring(1, jsonResult.Length - 2);
            }

            while (jsonResult.StartsWith("`"))
            {
                jsonResult = jsonResult.Substring(1, jsonResult.Length - 2).Trim();
            }
            if (jsonResult.StartsWith("json\n")) jsonResult = jsonResult.Replace("json\n", "");
            return jsonResult;
        }

        public void Dispose()
        {
            _cliente?.Dispose();
        }
        public async Task<string> AnalizarTextoParaFiltros(string origen)
        {
            try
            {
                var promptFinal = ((IIaPromptFiltrar)this).PromptFiltrar;

                // 1. Simplificamos el cuerpo eliminando el response_format que causa el error 400
                var requestBody = new
                {
                    model = _modelo.Descripcion(),
                    messages = new[]
                    {
                // Reforzamos la instrucción en el system prompt
                new {
                    role = "system",
                    content = "Eres un asistente técnico. Tu salida debe ser exclusivamente un array JSON de objetos 'ClausulaDeFiltrado'. No escribas prosa, ni explicaciones, ni etiquetas markdown."
                },
                new { role = "user", content = promptFinal }
            },
                    temperature = 0.0, // Bajamos a 0.0 para máxima consistencia
                    max_tokens = 1000
                };

                string jsonContent = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, MimeTypeMap.ApplicationJson);

                // 2. Llamada a la API
                var response = await Cliente.PostAsync(_apiChat, content);
                string responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Error al llamar a Perplexity (Filtros). Código: {response.StatusCode}, Detalles: {responseBody}");
                }

                // 3. Extracción del contenido
                JObject jsonResponse = JObject.Parse(responseBody);
                if (jsonResponse["choices"] != null && jsonResponse["choices"].Any())
                {
                    string textoResult = jsonResponse["choices"][0]["message"]?["content"]?.ToString();

                    if (!string.IsNullOrEmpty(textoResult))
                    {
                        // Tu método LimpiarRespuesta ya se encarga de quitar ```json o comillas extra
                        string textoLimpio = LimpiarRespuesta(textoResult.Trim());

                        // Validamos que sea un JSON antes de devolverlo
                        JToken.Parse(textoLimpio);

                        return textoLimpio;
                    }
                }

                throw new Exception("No se recibió contenido válido de Perplexity.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error en AnalizarTextoParaFiltros (Perplexity): {ex.Message}", ex);
            }
        }
    }
}
