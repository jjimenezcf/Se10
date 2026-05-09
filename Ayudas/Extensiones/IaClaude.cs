using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Utilidades
{
    public class IaClaude : IIa, IIaPromptFactura, IIaPromptResumen, IDisposable, IIaTiposMimesAdmitidos, IIaPromptFiltrar
    {
        private enum enumModelosClaude
        {
            [Description("claude-haiku-4-5-20251001")] claude_haiku,
            [Description("claude-sonnet-4-6")] claude_sonnet,
            [Description("claude-opus-4-6")] claude_opus
        }

        private string _apiKey { get; }
        private enumModelosClaude _modelo { get; }

        public HashSet<string> TiposMimeAdmitidosParaResumen { get; set; }
        public HashSet<string> TiposMimeAdmitidosParaFacturas { get; set; }

        private const string ApiGenerarUrl = "https://api.anthropic.com/v1/messages";
        private const string AnthropicVersion = "2023-06-01";
        private const int MaxTokens = 4096;

        public string PromptFactura { get; set; }
        public string PromptResumen { get; set; }
        public string PromptFiltrar { get; set; }

        private HttpClient _cliente;

        private HttpClient Cliente
        {
            get
            {
                if (_cliente == null)
                {
                    _cliente = new HttpClient();
                    _cliente.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MimeTypeMap.ApplicationJson));
                    _cliente.DefaultRequestHeaders.Add("x-api-key", _apiKey);
                    _cliente.DefaultRequestHeaders.Add("anthropic-version", AnthropicVersion);
                }
                return _cliente;
            }
        }

        public IaClaude(string apiKey, string modelo)
        {
            if (apiKey == ltrIa.ApiKey_NoDefinida)
                throw new Exception(IIa.IA_ApiKey_No_Valida);

            _apiKey = apiKey;
            if (modelo == ltrIa.Modelo_PorDefecto) _modelo = enumModelosClaude.claude_sonnet;
            else
            {
                enumModelosClaude? enumModelo = ApiDeEnsamblados.DescripcionToEnumerado<enumModelosClaude>(modelo, null);
                if (enumModelo == null)
                    throw new Exception(IIa.IA_Modelo_No_Valido + $": El modelo '{modelo}' no es válido para la ia de '{nameof(IaClaude)}'");

                _modelo = (enumModelosClaude)enumModelo;
            }

            TiposMimeAdmitidosParaResumen = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "application/pdf",
                "image/png",
                "image/jpeg",
                "image/gif",
                "image/webp",
                "text/plain",
            };

            TiposMimeAdmitidosParaFacturas = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "application/pdf",
                "image/png",
                "image/jpeg",
                "image/gif",
                "image/webp",
                "text/plain",
            };
        }

        /// <summary>
        /// Llama a la API de Claude y devuelve el texto de la respuesta.
        /// </summary>
        private async Task<string> LlamarApiAsync(object requestBody)
        {
            string jsonContent = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await Cliente.PostAsync(ApiGenerarUrl, content);
            string responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Error al llamar a la API de Claude. Código: {response.StatusCode}, Detalles: {responseBody}");

            var jsonResponse = JObject.Parse(responseBody);
            string texto = jsonResponse["content"]?[0]?["text"]?.ToString();

            if (string.IsNullOrEmpty(texto))
                throw new Exception($"No se pudo extraer el texto de la respuesta de Claude. Respuesta: {responseBody}");

            return texto;
        }

        public async Task<string> Resumir(string contenido)
        {
            string prompt = PromptResumen.Replace("[CONTENIDO]", contenido);

            var requestBody = new
            {
                model = _modelo.Descripcion(),
                max_tokens = MaxTokens,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };

            return await LlamarApiAsync(requestBody);
        }

        public async Task<string> ResumirConOcr(string rutaArchivo)
        {
            if (!File.Exists(rutaArchivo))
                throw new FileNotFoundException($"Archivo no encontrado para resumir: {rutaArchivo}");

            string mimeType = MimeTypeMap.GetMimeType(Path.GetExtension(rutaArchivo));

            if (mimeType == "text/plain")
            {
                string textoPlano = await File.ReadAllTextAsync(rutaArchivo);
                return await Resumir(textoPlano);
            }

            byte[] fileBytes = await File.ReadAllBytesAsync(rutaArchivo);
            string base64Data = Convert.ToBase64String(fileBytes);

            object requestBody;

            if (mimeType == "application/pdf")
            {
                requestBody = new
                {
                    model = _modelo.Descripcion(),
                    max_tokens = MaxTokens,
                    messages = new[]
                    {
                        new
                        {
                            role = "user",
                            content = new object[]
                            {
                                new
                                {
                                    type = "document",
                                    source = new { type = "base64", media_type = mimeType, data = base64Data }
                                },
                                new { type = "text", text = IIaPromptResumen.PromptDeResumenDeFichero }
                            }
                        }
                    }
                };
            }
            else
            {
                // Imagen
                requestBody = new
                {
                    model = _modelo.Descripcion(),
                    max_tokens = MaxTokens,
                    messages = new[]
                    {
                        new
                        {
                            role = "user",
                            content = new object[]
                            {
                                new
                                {
                                    type = "image",
                                    source = new { type = "base64", media_type = mimeType, data = base64Data }
                                },
                                new { type = "text", text = IIaPromptResumen.PromptDeResumenDeFichero }
                            }
                        }
                    }
                };
            }

            return await LlamarApiAsync(requestBody);
        }

        public async Task<string> AnalizarFactura(string contenidoFactura)
        {
            string prompt = $"Del contenido que te adjunto entre los 5 guiones {Environment.NewLine}-----{Environment.NewLine}{contenidoFactura}{Environment.NewLine}-----{Environment.NewLine}{PromptFactura}";

            var requestBody = new
            {
                model = _modelo.Descripcion(),
                max_tokens = MaxTokens,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };

            string resultado = await LlamarApiAsync(requestBody);

            // Limpiar posibles bloques de código markdown
            resultado = resultado.Trim().Replace("```json", "").Replace("```", "");
            JObject.Parse(resultado); // Validar que es JSON válido
            return resultado;
        }

        public async Task<string> AnalizarFacturaConOcr(string rutaArchivo)
        {
            if (!File.Exists(rutaArchivo))
                throw new FileNotFoundException($"Archivo de factura no encontrado: {rutaArchivo}");

            string mimeType = MimeTypeMap.GetMimeType(Path.GetExtension(rutaArchivo));

            if (mimeType == "text/plain")
            {
                string textoPlano = await File.ReadAllTextAsync(rutaArchivo);
                return await AnalizarFactura(textoPlano);
            }

            byte[] fileBytes = await File.ReadAllBytesAsync(rutaArchivo);
            string base64Data = Convert.ToBase64String(fileBytes);
            string prompt = $"Del fichero que te adjunto{Environment.NewLine}{PromptFactura}";

            object requestBody;

            if (mimeType == "application/pdf")
            {
                requestBody = new
                {
                    model = _modelo.Descripcion(),
                    max_tokens = MaxTokens,
                    messages = new[]
                    {
                        new
                        {
                            role = "user",
                            content = new object[]
                            {
                                new
                                {
                                    type = "document",
                                    source = new { type = "base64", media_type = mimeType, data = base64Data }
                                },
                                new { type = "text", text = prompt }
                            }
                        }
                    }
                };
            }
            else
            {
                // Imagen
                requestBody = new
                {
                    model = _modelo.Descripcion(),
                    max_tokens = MaxTokens,
                    messages = new[]
                    {
                        new
                        {
                            role = "user",
                            content = new object[]
                            {
                                new
                                {
                                    type = "image",
                                    source = new { type = "base64", media_type = mimeType, data = base64Data }
                                },
                                new { type = "text", text = prompt }
                            }
                        }
                    }
                };
            }

            string resultado = await LlamarApiAsync(requestBody);

            // Limpiar posibles bloques de código markdown
            resultado = resultado.Trim().Replace("```json", "").Replace("```", "");
            return resultado;
        }

        public async Task<string> AnalizarTextoParaFiltros(string origen)
        {
            try
            {
                string promptFinal = ((IIaPromptFiltrar)this).PromptFiltrar;

                var requestBody = new
                {
                    model = _modelo.Descripcion(),
                    max_tokens = MaxTokens,
                    messages = new[]
                    {
                        new { role = "user", content = promptFinal }
                    }
                };

                string resultado = await LlamarApiAsync(requestBody);

                // Limpiar posibles bloques de código markdown
                resultado = resultado.Trim().Replace("```json", "").Replace("```", "");
                return resultado;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error en AnalizarTextoParaFiltros: {ex.Message}", ex);
            }
        }

        public void Dispose()
        {
            _cliente?.Dispose();
        }
    }
}