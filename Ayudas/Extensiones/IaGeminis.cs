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
    public class IaGeminis : IIa, IIaPromptFactura, IIaPromptResumen, IDisposable, IIaTiposMimesAdmitidos, IIaPromptFiltrar
    {
        //gemini-1.5-flash
        private enum enumModelosGeminis
        {
            [Description("gemini-1.0-pro")] gemini_pro,
            [Description("gemini-2.5-flash")] geminis_flash
        }

        //// Puedes añadir una propiedad para obtener la lista ordenada de modelos a probar
        //public static string[] ModelosPrioritariosParaOcr = new string[]
        //{
        //   "gemini-2.5-flash", // Mejor opción multimodal y rápida
        //   "gemini-2.0-flash", // La versión anterior, también multimodal
        //   "gemini-2.5-flash-lite", // Versión más ligera, como fallback
        //};

        ///curl "https://generativelanguage.googleapis.com/v1/models?key=_apiKey"

        private string _apiKey { get; }
        private enumModelosGeminis _modelo { get; }

        public HashSet<string> TiposMimeAdmitidosParaResumen { get; set; }
        public HashSet<string> TiposMimeAdmitidosParaFacturas { get; set; }

        private const string ApiGenerarUrl = "https://generativelanguage.googleapis.com/v1/models/{0}:generateContent?key={1}";


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
                }
                return _cliente;
            }
        }

        public IaGeminis(string apiKey, string modelo)
        {
            if (apiKey == ltrIa.ApiKey_NoDefinida)
                throw new Exception(IIa.IA_ApiKey_No_Valida);

            _apiKey = apiKey;
            if (modelo == ltrIa.Modelo_PorDefecto) _modelo = enumModelosGeminis.geminis_flash;
            else
            {
                enumModelosGeminis? enumModelo = ApiDeEnsamblados.DescripcionToEnumerado<enumModelosGeminis>(modelo, null);
                if (enumModelo == null)
                    throw new Exception(IIa.IA_Modelo_No_Valido + $": El modelo '{modelo}' no es válido para la ia de '{nameof(IaGeminis)}'");

                _modelo = (enumModelosGeminis)enumModelo;
            }

            TiposMimeAdmitidosParaResumen = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
             "application/pdf",
             "audio/mpeg",
             "audio/mp3",
             "audio/wav",
             "image/png",
             "image/jpeg",
             "text/plain",
             "video/mov",
             "video/mpeg",
             "video/mp4",
             "video/mpg",
             "video/avi",
             "video/wmv",
             "video/mpegps",
             "video/flv"
            };

            TiposMimeAdmitidosParaFacturas = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
             "application/pdf",
             "image/png",
             "image/jpeg",
             "text/plain",
            };
        }

        public async Task<string> ResumirConOcr(string rutaArchivo)
        {
            if (!File.Exists(rutaArchivo))
            {
                throw new FileNotFoundException($"Archivo no encontrado para resumir: {rutaArchivo}");
            }

            try
            {
                // 1. Leer el archivo y convertirlo a Base64
                byte[] imageBytes = await File.ReadAllBytesAsync(rutaArchivo);
                string base64Image = Convert.ToBase64String(imageBytes);

                // 2. Construir el cuerpo de la petición con el prompt y la imagen
                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new object[]
                            {
                                new { text = IIaPromptResumen.PromptDeResumenDeFichero }, // Usa el prompt para resumir
                                new
                                {
                                    inlineData = new
                                    {
                                        mimeType = MimeTypeMap.GetMimeType(Path.GetExtension(rutaArchivo)),
                                        data = base64Image
                                    }
                                }
                            }
                        }
                    }
                };

                // 3. Serializar y enviar la petición a la API de Gemini
                string url = string.Format(ApiGenerarUrl, _modelo.Descripcion(), _apiKey);
                string jsonContent = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await Cliente.PostAsync(url, content);
                string responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Error al resumir con Gemini: {responseBody}");
                }

                // 4. Analizar la respuesta y extraer el texto resumido
                var parsedResponse = JObject.Parse(responseBody);
                var resumen = parsedResponse["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString();

                return resumen ?? "No se pudo encontrar el resumen en la respuesta de la API de Gemini.";
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al resumir el archivo con Gemini: {ex.Message}", ex);
            }
        }

        public async Task<string> Resumir(string contenido)
        {
            string prompt = PromptResumen.Replace("[CONTENIDO]", contenido);
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            };

            var json = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            string url = string.Format(ApiGenerarUrl, _modelo.Descripcion(), _apiKey);
            var response = await Cliente.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error al llamar a la API de Gemini. Código de estado: {response.StatusCode}, Contenido del error: {errorContent}");
            }

            string responseBody = await response.Content.ReadAsStringAsync();
            try
            {
                JObject jsonResponse = JObject.Parse(responseBody);
                string resumen = jsonResponse["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString();
                return resumen ?? "No se pudo encontrar el resumen en la respuesta de la API de Gemini.";
            }
            catch (JsonReaderException ex)
            {
                throw new Exception($"Error al analizar la respuesta JSON: {ex.Message}, Respuesta recibida: {responseBody}");
            }
        }

        public async Task<string> AnalizarFactura(string contenidoFactura)
        {
            string prompt = $"Del contenido que te adjunto entre los los 5 guiones {Environment.NewLine}-----{Environment.NewLine}{contenidoFactura}{Environment.NewLine}-----{Environment.NewLine}{PromptFactura}";

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            };

            var json = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_modelo.Descripcion()}:generateContent?key={_apiKey}";
            var response = await Cliente.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error al analizar la factura con la API de Gemini. Código de estado: {response.StatusCode}, Contenido del error: {errorContent}");
            }

            string responseBody = await response.Content.ReadAsStringAsync();
            try
            {
                JObject jsonResponse = JObject.Parse(responseBody);
                string jsonResult = jsonResponse["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString();

                // Limpiar la respuesta para asegurar que es un JSON válido
                if (!string.IsNullOrEmpty(jsonResult))
                {
                    // Remover posibles bloques de código y espacios extra
                    jsonResult = jsonResult.Trim().Replace("```json", "").Replace("```", "");
                    JObject.Parse(jsonResult); // Validar que es un JSON
                    return jsonResult;
                }
                throw new Exception("No se pudo extraer el JSON de la respuesta de la API de Gemini.");
            }
            catch (JsonReaderException ex)
            {
                throw new Exception($"Error al analizar la respuesta JSON: {ex.Message}, Respuesta recibida: {responseBody}");
            }
        }

        public async Task<string> AnalizarTextoParaFiltros(string origen)
        {
            try
            {
                // 1. Usar el prompt que ya fue preparado con los reemplazos de CG y Tipos
                var promptFinal = ((IIaPromptFiltrar)this).PromptFiltrar;

                // 2. Construir el cuerpo de la petición (idéntico a tus otros métodos)
                var requestBody = new
                {
                    contents = new[]
                    {
                new
                {
                    parts = new[]
                    {
                        new { text = promptFinal }
                    }
                }
            },
                    // IMPORTANTE: Gemini 1.5 acepta generationConfig para forzar JSON
                    generationConfig = new
                    {
                        response_mime_type = "application/json",
                        temperature = 0.1
                    }
                };

                string jsonContent = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // 3. CONSTRUCCIÓN DE LA URL (Causa del 404)
                // Usamos el mismo patrón que en 'AnalizarFactura' o 'Resumir'
                string url = $"https://generativelanguage.googleapis.com/v1beta/models/{_modelo.Descripcion()}:generateContent?key={_apiKey}";

                // Si tienes la variable 'ApiGenerarUrl' definida como en Resumir, úsala mejor:
                // string url = string.Format(ApiGenerarUrl, _modelo.Descripcion(), _apiKey);

                var response = await Cliente.PostAsync(url, content);
                string responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Error al llamar a Gemini (Filtros). Código: {response.StatusCode}, Detalles: {responseBody}");
                }

                // 4. Extraer el texto usando JObject como en tus otros métodos
                var jsonResponse = JObject.Parse(responseBody);
                string textoLimpio = jsonResponse["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString();

                if (!string.IsNullOrEmpty(textoLimpio))
                {
                    // Limpiar posibles bloques de código markdown si la IA los incluyera
                    textoLimpio = textoLimpio.Trim().Replace("```json", "").Replace("```", "");
                    return textoLimpio;
                }

                throw new Exception("No se pudo extraer el JSON de filtros de la respuesta de Gemini.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error en AnalizarTextoParaFiltros: {ex.Message}", ex);
            }
        }

        public async Task<string> AnalizarFacturaConOcr(string rutaArchivo)
        {
            if (!File.Exists(rutaArchivo))
            {
                throw new FileNotFoundException($"Archivo de factura no encontrado: {rutaArchivo}");
            }

            // 1. Leer el archivo y convertirlo a Base64
            byte[] imageBytes = await File.ReadAllBytesAsync(rutaArchivo);
            string base64Image = Convert.ToBase64String(imageBytes);
            string mimeType = MimeTypeMap.GetMimeType(Path.GetExtension(rutaArchivo));
            string prompt = $"Del fichero que te adjunto{Environment.NewLine}{PromptFactura}";

            // Construir el cuerpo de la petición (es el mismo para todos los modelos)
            var requestBody = new
            {
                contents = new[]
                {
            new
            {
                parts = new object[]
                {
                    new { text = prompt },
                    new
                    {
                        inlineData = new
                        {
                            mimeType = mimeType,
                            data = base64Image
                        }
                    }
                }
            }
        }
            };
            string jsonContent = JsonConvert.SerializeObject(requestBody);

            // Lista para acumular errores si todos los modelos fallan
            List<string> erroresAcumulados = new List<string>();
            var modeloAlias = _modelo.Descripcion();
            // 2. Iterar sobre los modelos prioritarios
            //foreach (var modeloAlias in ModelosPrioritariosParaOcr)
            //{
            try
            {
                // 3. Reconstruir la URL con el alias del modelo actual (usando v1)
                string url = string.Format(ApiGenerarUrl, modeloAlias, _apiKey);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // 4. Enviar la petición
                var response = await Cliente.PostAsync(url, content);
                string responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    // Éxito: Procesar y retornar el resultado
                    var parsedResponse = JObject.Parse(responseBody);
                    var jsonResult = parsedResponse["candidates"][0]["content"]["parts"][0]["text"].ToString().Trim();

                    // Limpieza de bloques de código
                    if (jsonResult.StartsWith("```json") && jsonResult.EndsWith("```"))
                    {
                        jsonResult = jsonResult.Substring("```json".Length, jsonResult.Length - "```json".Length - "```".Length).Trim();
                    }

                    return jsonResult; // Retorna el resultado del primer modelo exitoso
                }
                else
                {
                    // Fallo de la API (ej: 404, 400, 500)
                    erroresAcumulados.Add($"Modelo '{modeloAlias}' falló con código {response.StatusCode}. Respuesta: {responseBody}");
                }
            }
            catch (Exception ex)
            {
                // Fallo de la red, serialización, etc.
                erroresAcumulados.Add($"Modelo '{modeloAlias}' falló con excepción: {ex.Message}");
            }
            //}

            // 5. Si el bucle termina, significa que todos los modelos fallaron.
            throw new Exception("Todos los modelos de IA fallaron al analizar la factura. Errores: " + Environment.NewLine + string.Join(Environment.NewLine, erroresAcumulados));
        }

        public void Dispose()
        {
            _cliente?.Dispose();
        }

    }
}