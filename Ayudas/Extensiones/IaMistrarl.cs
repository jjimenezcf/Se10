using DocumentFormat.OpenXml.Wordprocessing;
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
using Utilidades;

public class IaMistral : IIa, IIaPromptFactura, IIaPromptResumen, IIaPromptFiltrar, IDisposable, IIaTiposMimesAdmitidos

{

    private string _apiKey { get; }
    private enumModelosMistral _modelo { get; }
    private const string ApiChatUrl = "https://api.mistral.ai/v1/chat/completions";

    public string PromptFactura { get; set; } = IIaPromptFactura.Prompt;
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
                _cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(ltrIa.Bearer, _apiKey);
            }
            return _cliente;
        }
    }

    private enum enumModelosMistral
    {
        [Description("mistral-large-latest")] mistral_large,

        [Description("mistral-small-latest")] mistral_small,

        [Description("mistral-ocr-latest")] mistral_ocr,
    }

    public HashSet<string> TiposMimeAdmitidosParaResumen { get; set; }
    public HashSet<string> TiposMimeAdmitidosParaFacturas { get; set; }

    public IaMistral(string apiKey, string modelo)
    {
        if (apiKey == ltrIa.ApiKey_NoDefinida)
            throw new Exception(IIa.IA_ApiKey_No_Valida);
        _apiKey = apiKey;

        if (modelo == ltrIa.Modelo_PorDefecto) _modelo = enumModelosMistral.mistral_large;
        else
        {
            enumModelosMistral? enumModelo = ApiDeEnsamblados.DescripcionToEnumerado<enumModelosMistral>(modelo, null);
            if (enumModelo == null)
                throw new Exception(IIa.IA_Modelo_No_Valido + $": El modelo '{modelo}' no es válido para la ia de '{nameof(IaMistral)}'");

            _modelo = (enumModelosMistral)enumModelo;
        }

        TiposMimeAdmitidosParaFacturas = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
             "image/png",
             "image/jpeg",
            };

        TiposMimeAdmitidosParaResumen = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {

            };
    }

    public async Task<string> AnalizarFactura(string contenidoFactura)
    {
        string prompt = $"Del contenido que te adjunto entre los los 5 guiones {Environment.NewLine}-----{Environment.NewLine}{contenidoFactura}{Environment.NewLine}-----{Environment.NewLine}{PromptFactura}";

        try
        {
            var requestBody = new
            {
                model = _modelo.Descripcion(),
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = new object[]
                        {
                            new
                            {
                                type = "text",
                                text = prompt
                            }
                        }
                    }
                },
                response_format = new { type = "json_object" }
            };

            string jsonContent = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await Cliente.PostAsync(ApiChatUrl, content);
            string responseBody = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();

            var parsedResponse = JObject.Parse(responseBody);
            var jsonResult = parsedResponse["choices"][0]["message"]["content"].ToString().Trim();

            return jsonResult;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al analizar el texto de la factura con Mistral: {ex.Message}", ex);
        }
    }

    public async Task<string> AnalizarFacturaConOcr(string rutaArchivo)
    {
        if (!File.Exists(rutaArchivo))
        {
            throw new FileNotFoundException($"Archivo de factura no encontrado: {rutaArchivo}");
        }

        // Obtener el tipo MIME del archivo
        string mimeType = MimeTypeMap.GetMimeType(Path.GetExtension(rutaArchivo));
        if (string.IsNullOrEmpty(mimeType))
        {
            throw new ArgumentException("No se pudo determinar el tipo MIME del archivo.");
        }

        // Leer el archivo y convertirlo a Base64
        byte[] fileBytes = await File.ReadAllBytesAsync(rutaArchivo);
        string base64Data = Convert.ToBase64String(fileBytes);

        string prompt = $"Del fichero que te adjunto{Environment.NewLine}{PromptFactura}";
        try
        {
            // Construir el cuerpo de la petición con texto y datos del archivo
            var requestBody = new
            {
                model = _modelo.Descripcion(),
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = new object[]
                        {
                            new
                            {
                                type = "text",
                                text = prompt
                            },
                            new
                            {
                                type = "image_url", // Mistral espera "image_url" para archivos de imagen
                                image_url = new
                                {
                                    url = $"data:{mimeType};base64,{base64Data}"
                                }
                            }
                        }
                    }
                },
                response_format = new { type = "json_object" }
            };

            // Si el archivo no es una imagen, la estructura de la petición cambia.
            // Mistral no tiene soporte nativo para PDFs y otros tipos, por lo que este método
            // solo es realmente útil para imágenes y tal vez audio/video dependiendo del modelo.
            if (!mimeType.StartsWith("image/"))
            {
                // Para otros tipos de archivo, el enfoque multimodal directo no funcionará.
                // En un escenario real, necesitarías procesar el archivo fuera de la IA.
                // Aquí, simplemente lanzamos una excepción para ser explícitos.
                throw new NotSupportedException($"El tipo de archivo '{mimeType}' no es soportado por este método multimodal.");
            }

            string jsonContent = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await Cliente.PostAsync(ApiChatUrl, content);
            string responseBody = await response.Content.ReadAsStringAsync();

            response.EnsureSuccessStatusCode();

            var parsedResponse = JObject.Parse(responseBody);
            var jsonResult = parsedResponse["choices"][0]["message"]["content"].ToString().Trim();

            return jsonResult;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al analizar la factura con Mistral: {ex.Message}", ex);
        }
    }

    public async Task<string> Resumir(string promtConElContenido)
    {
        var requestBody = new
        {
            model = _modelo.Descripcion(),
            messages = new[]
            {
               new
               {
                   role = "user",
                   content = promtConElContenido
               }
            },
            temperature = 0.5 // Ajusta la creatividad del resumen
        };

        string jsonContent = JsonConvert.SerializeObject(requestBody);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        var response = await Cliente.PostAsync(ApiChatUrl, content);
        string responseBody = await response.Content.ReadAsStringAsync();
        response.EnsureSuccessStatusCode();

        var parsedResponse = JObject.Parse(responseBody);
        var resumen = parsedResponse["choices"][0]["message"]["content"].ToString().Trim();

        return resumen;
    }

    public async Task<string> ResumirConOcr(string rutaArchivo)
    {
        if (!File.Exists(rutaArchivo))
        {
            throw new FileNotFoundException($"Archivo no encontrado para resumir: {rutaArchivo}");
        }

        try
        {
            string contenidoDocumento;
            string extension = Path.GetExtension(rutaArchivo).ToLowerInvariant();

            // 1. Decidir el método de extracción de texto
            if (extension == enumExtensiones.pdf.Render())
            {
                // Para PDFs, usar la API de OCR especializada de Mistral
                contenidoDocumento = await PasarOcrDeMistral(rutaArchivo);
            }
            else if (extension == enumExtensiones.jpg.Render() || extension == enumExtensiones.jpeg.Render() || extension == enumExtensiones.png.Render())
            {
                // Para imágenes, usar la capacidad multimodal de la API de Chat de Mistral
                // y pedirle que resuma la imagen.
                contenidoDocumento = await ExtraerTextoYResumirDeImagenConMistral(rutaArchivo);
            }
            else
            {
                // Para otros formatos, lanzar una excepción
                throw new NotSupportedException($"El formato de archivo '{extension}' no es compatible para la operación de resumen.");
            }

            // 2. Si es una imagen, ya obtuvimos el resumen. Si es un PDF, ahora lo resumimos.
            if (extension == ".pdf")
            {
                // Pasar el texto extraído del PDF al método de resumen de texto
                return await Resumir(PromptResumen.Replace("[CONTENIDO]", contenidoDocumento)); 
            }

            // Si es una imagen, el resultado ya es el resumen
            return contenidoDocumento;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al resumir el archivo con Mistral: {ex.Message}", ex);
        }
    }

    private async Task<string> PasarOcrDeMistral(string rutaArchivo)
    {
        var ocrClient = new HttpClient();
        ocrClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(ltrIa.Bearer, _apiKey);

        string ocrApiUrl = "https://api.mistral.ai/v1/document_extracts/async";
        byte[] fileBytes = await File.ReadAllBytesAsync(rutaArchivo);
        string base64Data = Convert.ToBase64String(fileBytes);

        var requestBody = new
        {
            model = enumModelosMistral.mistral_ocr.Descripcion(),
            document = new
            {
                data = base64Data,
                mime_type = MimeTypeMap.pdf
            }
        };

        var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, MimeTypeMap.ApplicationJson);
        var response = await ocrClient.PostAsync(ocrApiUrl, content);
        string responseBody = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();

        var parsedResponse = JObject.Parse(responseBody);
        return parsedResponse["text"]?.ToString() ?? throw new Exception("No se pudo extraer el texto del PDF.");
    }

    private async Task<string> ExtraerTextoYResumirDeImagenConMistral(string rutaArchivo)
    {
        string mimeType = MimeTypeMap.GetMimeType(Path.GetExtension(rutaArchivo));
        byte[] imageBytes = await File.ReadAllBytesAsync(rutaArchivo);
        string base64Image = Convert.ToBase64String(imageBytes);

        var requestBody = new
        {
            model = _modelo.Descripcion(), // ej. "mistral-large-latest"
            messages = new[]
            {
            new
            {
                role = "user",
                content = new object[]
                {
                    new { type = "text", text = IIaPromptResumen.PromptDeResumenDeFichero },
                    new { type = "image_url", image_url = new { url = $"data:{mimeType};base64,{base64Image}" } }
                }
            }
        }
        };

        string jsonContent = JsonConvert.SerializeObject(requestBody);
        var content = new StringContent(jsonContent, Encoding.UTF8, MimeTypeMap.ApplicationJson);

        var response = await Cliente.PostAsync(ApiChatUrl, content);
        string responseBody = await response.Content.ReadAsStringAsync();

        response.EnsureSuccessStatusCode();

        var parsedResponse = JObject.Parse(responseBody);
        return parsedResponse["choices"]?[0]?["message"]?["content"]?.ToString()?.Trim() ?? "No se pudo extraer el resumen de la imagen.";
    }

    public async Task<string> MistralOcrUpload(string rutaArchivo)
    {
        if (!File.Exists(rutaArchivo))
        {
            throw new FileNotFoundException($"Archivo no encontrado: {rutaArchivo}");
        }

        var mistralClient = new HttpClient();
        mistralClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(ltrIa.Bearer, _apiKey);

        using var content = new MultipartFormDataContent();
        content.Add(new StringContent("ocr"), "purpose");
        content.Add(new StreamContent(File.OpenRead(rutaArchivo)), "file", System.IO.Path.GetFileName(rutaArchivo));

        var uploadResponse = await mistralClient.PostAsync("https://api.mistral.ai/v1/files", content);

        if (!uploadResponse.IsSuccessStatusCode)
        {
            var errorContent = await uploadResponse.Content.ReadAsStringAsync();
            throw new Exception($"Error en la API de Mistral al subir archivo: {uploadResponse.StatusCode}, {errorContent}");
        }

        var uploadResult = await uploadResponse.Content.ReadAsStringAsync();
        var jsonResponse = JObject.Parse(uploadResult);
        var fileId = jsonResponse["id"]?.ToString();

        if (string.IsNullOrEmpty(fileId))
        {
            throw new Exception("No se pudo obtener el ID del archivo subido.");
        }

        // Una vez que tienes el ID del archivo, puedes iniciar el proceso de OCR
        return await MistralOcrProcess(fileId);
    }

    private async Task<string> MistralOcrProcess(string fileId)
    {
        var mistralClient = new HttpClient();
        mistralClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(ltrIa.Bearer, _apiKey);

        var requestBody = new
        {
            model = enumModelosMistral.mistral_ocr,
            document = new
            {
                type = "file_id",
                file_id = fileId
            }
        };


        var jsonContent = JsonConvert.SerializeObject(requestBody);
        var content = new StringContent(jsonContent, Encoding.UTF8, MimeTypeMap.ApplicationJson);

        var ocrResponse = await mistralClient.PostAsync("https://api.mistral.ai/v1/ocr", content);

        if (!ocrResponse.IsSuccessStatusCode)
        {
            var errorContent = await ocrResponse.Content.ReadAsStringAsync();
            throw new Exception($"Error en la API de Mistral al procesar OCR: {ocrResponse.StatusCode}, {errorContent}");
        }

        var ocrResult = await ocrResponse.Content.ReadAsStringAsync();

        var jsonResponseOcr = JObject.Parse(ocrResult);
        var ocrText = jsonResponseOcr["text"]?.ToString() ?? "";

        return ocrText;
    }

    public async Task<string> MistralOcr(string rutaArchivo)
    {
        var documentUrl = await SubirArchivo(rutaArchivo);

        var mistralClient = new HttpClient();
        mistralClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(ltrIa.Bearer, _apiKey);

        // Crear el objeto JSON para la solicitud
        var requestBody = new
        {
            model = enumModelosMistral.mistral_ocr,
            document = new
            {
                type = "document_url",
                document_url = documentUrl
            }
        };

        var jsonContent = JsonConvert.SerializeObject(requestBody);
        var content = new StringContent(jsonContent, Encoding.UTF8, MimeTypeMap.ApplicationJson);

        var ocrResponse = await mistralClient.PostAsync("https://api.mistral.ai/v1/ocr", content);

        if (!ocrResponse.IsSuccessStatusCode)
        {
            var errorContent = await ocrResponse.Content.ReadAsStringAsync();
            throw new Exception($"Error al procesar OCR en Mistral: {ocrResponse.StatusCode}, {errorContent}");
        }

        var ocrResult = await ocrResponse.Content.ReadAsStringAsync();

        // Extraer texto del JSON de respuesta
        var jsonResponseOcr = JObject.Parse(ocrResult);
        var ocrText = jsonResponseOcr["text"]?.ToString() ?? "";

        return ocrText;
    }

    private async Task<string> SubirArchivo(string rutaArchivo)
    {
        if (!File.Exists(rutaArchivo))
        {
            throw new FileNotFoundException($"Archivo no encontrado: {rutaArchivo}");
        }

        var mistralClient = new HttpClient();
        mistralClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(ltrIa.Bearer, _apiKey);

        using var content = new MultipartFormDataContent();
        content.Add(new StringContent("ocr"), "purpose");
        content.Add(new StreamContent(File.OpenRead(rutaArchivo)), "file", System.IO.Path.GetFileName(rutaArchivo));

        var uploadResponse = await mistralClient.PostAsync("https://api.mistral.ai/v1/files", content);

        if (!uploadResponse.IsSuccessStatusCode)
        {
            var errorContent = await uploadResponse.Content.ReadAsStringAsync();
            throw new Exception($"Error al subir archivo a Mistral: {uploadResponse.StatusCode}, {errorContent}");
        }

        var uploadResult = await uploadResponse.Content.ReadAsStringAsync();
        var jsonResponse = JObject.Parse(uploadResult);

        // Verifica si la respuesta incluye una URL
        var documentUrl = jsonResponse["url"]?.ToString();
        if (string.IsNullOrEmpty(documentUrl))
        {
            throw new Exception("No se pudo obtener la URL del archivo subido.");
        }

        return documentUrl;
    }

    public void Dispose()
    {
        _cliente?.Dispose();
    }

    public async Task<string> AnalizarTextoParaFiltros(string origen)
    {
        try
        {
            var requestBody = new
            {
                model = _modelo.Descripcion(),
                messages = new[]
                {
                new
                {
                    role = "user",
                    content = new object[]
                    {
                        new
                        {
                            type = "text",
                            text = ((IIaPromptFiltrar)this).PromptFiltrar
                        }
                    }
                }
            },
                response_format = new { type = "json_object" }
            };

            string jsonContent = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await Cliente.PostAsync(ApiChatUrl, content);
            string responseBody = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();

            var parsedResponse = JObject.Parse(responseBody);
            var resultado = parsedResponse["choices"][0]["message"]["content"].ToString().Trim();

            resultado = resultado.Replace("```json", "").Replace("```", "");
            return resultado;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error en AnalizarTextoParaFiltros con Mistral: {ex.Message}", ex);
        }
    }
}