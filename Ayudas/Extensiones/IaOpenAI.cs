using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Utilidades
{
    public class IaOpenAI : IIa, IIaPromptFactura, IDisposable
    {
        private List<string> _apiKeys = new List<string>()
    {
        "sk-abcdef1234567890abcdef1234567890abcdef12",
        "sk-1234567890abcdef1234567890abcdef12345678",
        "sk-abcdefabcdefabcdefabcdefabcdefabcdef12",
        "sk-7890abcdef7890abcdef7890abcdef7890abcd",
        "sk-1234abcd1234abcd1234abcd1234abcd1234abcd",
        "sk-abcd1234abcd1234abcd1234abcd1234abcd1234",
        "sk-5678efgh5678efgh5678efgh5678efgh5678efgh",
        "sk-efgh5678efgh5678efgh5678efgh5678efgh5678",
        "sk-ijkl1234ijkl1234ijkl1234ijkl1234ijkl1234",
        "sk-mnop5678mnop5678mnop5678mnop5678mnop5678",
        "sk-qrst1234qrst1234qrst1234qrst1234qrst1234",
        "sk-uvwx5678uvwx5678uvwx5678uvwx5678uvwx5678",
        "sk-1234ijkl1234ijkl1234ijkl1234ijkl1234ijkl",
        "sk-5678mnop5678mnop5678mnop5678mnop5678mnop",
        "sk-qrst5678qrst5678qrst5678qrst5678qrst5678",
        "sk-uvwx1234uvwx1234uvwx1234uvwx1234uvwx1234",
        "sk-1234abcd5678efgh1234abcd5678efgh1234abcd",
        "sk-5678ijkl1234mnop5678ijkl1234mnop5678ijkl",
        "sk-abcdqrstefghuvwxabcdqrstefghuvwxabcdqrst",
        "sk-ijklmnop1234qrstijklmnop1234qrstijklmnop",
        "sk-1234uvwx5678abcd1234uvwx5678abcd1234uvwx",
        "sk-efghijkl5678mnopabcd1234efghijkl5678mnop",
        "sk-mnopqrstuvwxabcdmnopqrstuvwxabcdmnopqrst",
        "sk-ijklmnopqrstuvwxijklmnopqrstuvwxijklmnop",
        "sk-abcd1234efgh5678abcd1234efgh5678abcd1234",
        "sk-1234ijklmnop5678ijklmnop1234ijklmnop5678",
        "sk-qrstefghuvwxabcdqrstefghuvwxabcdqrstefgh",
        "sk-uvwxijklmnop1234uvwxijklmnop1234uvwxijkl",
        "sk-abcd5678efgh1234abcd5678efgh1234abcd5678",
        "sk-ijklmnopqrstuvwxijklmnopqrstuvwxijklmnop",
        "sk-1234qrstuvwxabcd1234qrstuvwxabcd1234qrst",
        "sk-efghijklmnop5678efghijklmnop5678efghijkl",
        "sk-mnopabcd1234efghmnopabcd1234efghmnopabcd",
        "sk-ijklqrst5678uvwxijklqrst5678uvwxijklqrst",
        "sk-1234ijkl5678mnop1234ijkl5678mnop1234ijkl",
        "sk-abcdqrstefgh5678abcdqrstefgh5678abcdqrst",
        "sk-ijklmnopuvwx1234ijklmnopuvwx1234ijklmnop",
        "sk-efgh5678abcd1234efgh5678abcd1234efgh5678",
        "sk-mnopqrstijkl5678mnopqrstijkl5678mnopqrst",
        "sk-1234uvwxabcd5678uvwxabcd1234uvwxabcd5678",
        "sk-ijklmnop5678efghijklmnop5678efghijklmnop",
        "sk-abcd1234qrstuvwxabcd1234qrstuvwxabcd1234",
        "sk-1234efgh5678ijkl1234efgh5678ijkl1234efgh",
        "sk-5678mnopqrstuvwx5678mnopqrstuvwx5678mnop",
        "sk-abcdijkl1234uvwxabcdijkl1234uvwxabcdijkl",
        "sk-ijklmnopabcd5678ijklmnopabcd5678ijklmnop",
        "sk-1234efghqrstuvwx1234efghqrstuvwx1234efgh",
        "sk-5678ijklmnopabcd5678ijklmnopabcd5678ijkl",
        "sk-abcd1234efgh5678abcd1234efgh5678abcd1234",
        "sk-ijklmnopqrstuvwxijklmnopqrstuvwxijklmnop"
    };
        private const string _apiBaseUrl = "https://api.openai.com/v1/"; // OpenAI API base URL
        private HttpClient _cliente;

        private HttpClient Cliente
        {
            get
            {
                if (_cliente == null)
                {
                    _cliente = new HttpClient();
                }
                return _cliente;
            }
        }

        public string PromptFactura { get; set; }

        public IaOpenAI()
        {
        }

        private void SetApiKey(string apiKey)
        {
            // Clear existing Authorization header
            Cliente.DefaultRequestHeaders.Authorization = null;

            // Set the new API key
            Cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(ltrIa.Bearer, apiKey);
        }

        public async Task<string> Resumir(string contenido)
        {
            string endpoint = "chat/completions";
            string prompt = $"Por favor, resume el siguiente texto:\n\n{contenido}";

            for (int i = 0; i < _apiKeys.Count; i++)
            {
                string apiKey = _apiKeys[i];
                SetApiKey(apiKey);

                try
                {
                    var requestBody = new
                    {
                        model = "gpt-3.5-turbo", // Use the most relevant model
                        messages = new[] { new { role = "user", content = prompt } },
                        max_tokens = 500, // Adjust as needed
                        temperature = 0.7  // Adjust as needed
                    };

                    var json = JsonConvert.SerializeObject(requestBody);
                    var content = new StringContent(json, Encoding.UTF8, MimeTypeMap.ApplicationJson);

                    var response = await Cliente.PostAsync(_apiBaseUrl + endpoint, content);

                    if (response.IsSuccessStatusCode)
                    {
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
                            throw new Exception("No se pudo encontrar el resumen en la respuesta de la API de OpenAI.");
                        }
                        catch (JsonReaderException ex)
                        {
                            throw new Exception($"Error al analizar la respuesta JSON: {ex.Message}, Respuesta recibida: {responseBody}");
                        }
                    }
                    else if (response.StatusCode == HttpStatusCode.Forbidden)
                    {
                        Console.WriteLine($"OpenAI API key {_apiKeys[i]} is forbidden. Trying the next key.");
                        continue; // Try the next API key
                    }
                    else
                    {
                        string errorContent = await response.Content.ReadAsStringAsync();
                        throw new Exception($"OpenAI API call failed with key {_apiKeys[i]}. Status Code: {response.StatusCode}, Error Content: {errorContent}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred with OpenAI API key {_apiKeys[i]}: {ex.Message}");
                    // If it's last key or APIKey exception - rethrow it
                    if (i == _apiKeys.Count - 1 || ex is ArgumentException)
                    {
                        throw;
                    }
                }
            }

            throw new Exception("All OpenAI API keys failed to summarize the content.");
        }

        public async Task<string> AnalizarFactura(string rutaFichero)
        {
            try
            {
                string invoiceContent = await File.ReadAllTextAsync(rutaFichero);

                string prompt = PromptFactura.Replace("[CONTENIDO_FACTURA]", invoiceContent);

                string endpoint = "chat/completions";

                for (int i = 0; i < _apiKeys.Count; i++)
                {
                    string apiKey = _apiKeys[i];
                    SetApiKey(apiKey);

                    try
                    {
                        var requestBody = new
                        {
                            model = "gpt-3.5-turbo-1106", // Or another suitable model.  Use versions supporting response_format
                            messages = new[] { new { role = "user", content = prompt } },
                            max_tokens = 1500, // Adjust based on the expected JSON size.
                            temperature = 0.0, // Reduce hallucination
                            response_format = new { type = "json_object" } // Force JSON output!
                        };

                        var json = JsonConvert.SerializeObject(requestBody);
                        var content = new StringContent(json, Encoding.UTF8, MimeTypeMap.ApplicationJson);

                        var response = await Cliente.PostAsync(_apiBaseUrl + endpoint, content);

                        if (response.IsSuccessStatusCode)
                        {
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
                                throw new Exception("No 'choices' or 'message' found in OpenAI API response.");
                            }
                            catch (JsonReaderException ex)
                            {
                                throw new Exception($"Error parsing OpenAI JSON response: {ex.Message}, Response body: {responseBody}");
                            }
                        }
                        else if (response.StatusCode == HttpStatusCode.Forbidden)
                        {
                            Console.WriteLine($"OpenAI API key {_apiKeys[i]} is forbidden. Trying the next key.");
                            continue; // Try the next API key
                        }
                        else
                        {
                            string errorContent = await response.Content.ReadAsStringAsync();
                            throw new Exception($"OpenAI API call failed with key {_apiKeys[i]}. Status Code: {response.StatusCode}, Error Content: {errorContent}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"An error occurred with OpenAI API key {_apiKeys[i]}: {ex.Message}");
                        // If it's last key or APIKey exception - rethrow it
                        if (i == _apiKeys.Count - 1 || ex is ArgumentException)
                        {
                            throw;
                        }
                    }
                }

                throw new Exception("All OpenAI API keys failed to analyze the invoice.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in AnalizarFactura: {ex.Message}");
                throw; // Re-throw the exception so the caller knows something went wrong.
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
