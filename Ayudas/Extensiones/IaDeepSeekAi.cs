using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Utilidades
{
    public class IaDeepSeek : IIa, IIaPromptFactura, IDisposable
    {
        private string _apiKey { get; }
        private string _modelo { get; }
        private const string _apiBaseUrl = "https://api.deepseek.com/v1/"; // DeepSeek API base URL

        public string PromptFactura { get; set; }


        private HttpClient _cliente;

        private HttpClient Cliente
        {
            get
            {
                if (_cliente == null)
                {
                    _cliente = new HttpClient();
                    _cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(ltrIa.Bearer, _apiKey);
                }
                return _cliente;
            }
        }

        public IaDeepSeek(string apiKey, string modelo)
        {
            _apiKey = apiKey.IsNullOrEmpty() ? "sk-0034f12f2cba4c2583d2ad4bb72157fa" : apiKey;
            _modelo = modelo.IsNullOrEmpty() ? modelo : "DeepSeek-V3";
        }

        public async Task<string> Resumir(string contenido)
        {
            string endpoint = "chat/completions";
            string prompt = $"Por favor, resume el siguiente texto:\n\n{contenido}";
            var requestBody = new
            {
                model = _modelo,
                messages = new[] { new { role = "user", content = prompt } },
                max_tokens = 500
            };

            var json = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, Encoding.UTF8, MimeTypeMap.ApplicationJson);

            var response = await Cliente.PostAsync(_apiBaseUrl + endpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error al llamar a la API de DeepSeek. Código de estado: {response.StatusCode}, Contenido del error: {errorContent}");
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
                        return firstChoice["message"]["content"].ToString();
                    }
                }
                throw new Exception("No se pudo encontrar el resumen en la respuesta de la API de DeepSeek.");
            }
            catch (JsonReaderException ex)
            {
                throw new Exception($"Error al analizar la respuesta JSON: {ex.Message}, Respuesta recibida: {responseBody}");
            }
        }

        public async Task<string> AnalizarFactura(string filePath)
        {
            try
            {
                // Leer el archivo como un array de bytes
                byte[] fileBytes = await File.ReadAllBytesAsync(filePath);

                // Crear el contenido multipart para enviar el archivo
                using var content = new MultipartFormDataContent();
                var fileContent = new ByteArrayContent(fileBytes);
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/pdf"); // Ajusta el tipo MIME según el tipo de archivo
                content.Add(fileContent, "file", Path.GetFileName(filePath));

                // Endpoint de la API de DeepSeek para OCR
                string endpoint = "ocr/analyze"; // Ajusta el endpoint según la API de DeepSeek

                // Enviar el archivo a la API de DeepSeek para OCR
                var response = await Cliente.PostAsync(_apiBaseUrl + endpoint, content);

                if (!response.IsSuccessStatusCode)
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Error al llamar a la API de DeepSeek para OCR. Código de estado: {response.StatusCode}, Contenido del error: {errorContent}");
                }

                // Leer la respuesta de la API de DeepSeek (texto extraído del OCR)
                string ocrText = await response.Content.ReadAsStringAsync();

                // Ahora que tenemos el texto, procedemos a analizarlo como antes
                // Construct the prompt.
                string prompt = PromptFactura.Replace("[CONTENIDO_FACTURA]", ocrText);

                // Llamar al endpoint de chat/completions de DeepSeek para analizar el texto
                string chatEndpoint = "chat/completions";
                var requestBody = new
                {
                    model = _modelo,
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
                            schema = new
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
                var chatContent = new StringContent(json, Encoding.UTF8, MimeTypeMap.ApplicationJson);

                var chatResponse = await Cliente.PostAsync(_apiBaseUrl + chatEndpoint, chatContent);

                if (!chatResponse.IsSuccessStatusCode)
                {
                    string errorContent = await chatResponse.Content.ReadAsStringAsync();
                    throw new Exception($"Error: Status Code: {chatResponse.StatusCode}, Error Content: {errorContent}");
                }

                string responseBody = await chatResponse.Content.ReadAsStringAsync();

                try
                {
                    JObject jsonResponse = JObject.Parse(responseBody);
                    if (jsonResponse["choices"] != null && jsonResponse["choices"].Type == JTokenType.Array && jsonResponse["choices"].Count() > 0)
                    {
                        JToken firstChoice = jsonResponse["choices"][0];
                        if (firstChoice["message"] != null && firstChoice["message"]["content"] != null)
                        {
                            string jsonResult = firstChoice["message"]["content"].ToString().Trim();

                            if (jsonResult.StartsWith("\"") && jsonResult.EndsWith("\""))
                            {
                                jsonResult = jsonResult.Substring(1, jsonResult.Length - 2);
                            }

                            while (jsonResult.StartsWith("`"))
                            {
                                jsonResult = jsonResult.Substring(1, jsonResult.Length - 2).Trim();
                            }
                            if (jsonResult.StartsWith("json\n")) jsonResult = jsonResult.Replace("json\n", "");

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
                    throw new Exception("No 'choices' or 'message' found in DeepSeek API response.");
                }
                catch (JsonReaderException ex)
                {
                    throw new Exception($"Error parsing DeepSeek JSON response: {ex.Message}, Response body: {responseBody}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in AnalizarFactura: {ex.Message}");
                throw;
            }
        }

        public void Dispose()
        {
            _cliente?.Dispose();
        }

        public Task<string> AnalizarTextoParaFiltros(string origen)
        {
            throw new NotImplementedException();
        }
    }
}