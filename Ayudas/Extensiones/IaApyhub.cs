using System;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;

namespace Utilidades
{

    public class IaApyhub : IIa
    {
        private string _apiKey = null;

        private const string data = nameof(data);
        private const string summary = nameof(summary);

        HttpClient _cliente = null;
        private HttpClient Cliente
        {
            get
            {
                if (_cliente is null)
                {
                    _cliente = new HttpClient();
                    _cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(ltrIa.Bearer, _apiKey);
                }
                return _cliente;
            }
        }

        public IaApyhub(string apikey)
        {
            _apiKey = apikey;
        }

        public async Task<string> Resumir(string contenido)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://api.apyhub.com//ai/summarize-text");

            request.Headers.Add("apy-token", _apiKey);

            var jsonObject = new { text = contenido };
            var jsonContent = JsonConvert.SerializeObject(jsonObject);
            request.Content = new StringContent(jsonContent, Encoding.UTF8, MimeTypeMap.ApplicationJson );

            HttpResponseMessage response = await Cliente.SendAsync(request);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            JObject jsonResultado = JObject.Parse(responseBody);

            if (jsonResultado[data] == null || jsonResultado[data].Type != JTokenType.Object || !((JObject)jsonResultado[data]).ContainsKey(summary))
            {
                throw new Exception($"La respuesta JSON no contiene el campo '{summary}' dentro de '{data}' o '{data}' no es un objeto");
            }


            return (string)jsonResultado[data][summary];
        }

        public async Task<string> AnalizarFactura(string rutaFichero)
        {
            using (var fileStream = new FileStream(rutaFichero, FileMode.Open, FileAccess.Read))
            {
                if (extJson.EsJsonValido(fileStream))
                {
                    // Reset the stream position to the beginning
                    fileStream.Position = 0;
                    using (var reader = new StreamReader(fileStream))
                    {
                        return await reader.ReadToEndAsync();
                    }
                }
            }

            var url = "https://api.apyhub.com//ai/document/extract/invoice/file";

            using (var client = new HttpClient())
            using (var form = new MultipartFormDataContent())
            {
                client.DefaultRequestHeaders.Add("apy-token", _apiKey);
                using (var fileStream = new FileStream(rutaFichero, FileMode.Open, FileAccess.Read))
                {
                    var fileContent = new StreamContent(fileStream);
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");

                    form.Add(fileContent, "file", System.IO.Path.GetFileName(rutaFichero));
                    form.Add(new StringContent("apyhub"), "requested_service");

                    var response = await client.PostAsync(url, form);
                    response.EnsureSuccessStatusCode();

                    var responseData = await response.Content.ReadAsStringAsync();

                    string cleanedJson = QuitarPropiedadesDeLaFactura(responseData);
                    var parsedJson = JsonDocument.Parse(cleanedJson);

                    // Navegamos hasta el array de Items
                    var itemsArray = parsedJson.RootElement
                        .GetProperty("documents")[0]
                        .GetProperty("fields")
                        .GetProperty("Items")
                        .GetProperty("valueArray");

                    // Calculamos la suma total de los Amount
                    decimal baseImponible = itemsArray.EnumerateArray()
                        .Sum(item => item.GetProperty("valueObject")
                                     .GetProperty("Amount")
                                     .GetProperty("valueCurrency")
                                     .GetProperty("amount")
                                     .GetDecimal());

                    string contenido = "";
                    if (itemsArray.GetArrayLength() > 0)
                    {
                        var primerItem = itemsArray[0];
                        if (primerItem.TryGetProperty("content", out JsonElement contentElement))
                        {
                            contenido = contentElement.GetString();
                        }
                    }

                    var datosDeFacturaJson = new
                    {
                        Proveedor = parsedJson.ObtenerPropiedad("documents[0].fields.VendorName.valueString"),
                        Nif = parsedJson.ObtenerPropiedad("documents[0].fields.VendorTaxId.valueString"),
                        NumeroFactura = parsedJson.ObtenerPropiedad("documents[0].fields.InvoiceId.valueString"),
                        fecha = parsedJson.ObtenerPropiedad("documents[0].fields.InvoiceDate.valueDate"),
                        total = parsedJson.ObtenerPropiedad("documents[0].fields.InvoiceTotal.valueCurrency.amount"),
                        Calle = parsedJson.ObtenerPropiedad("documents[0].fields.VendorAddress.valueAddress.road"),
                        NumeroPolicia = parsedJson.ObtenerPropiedad("documents[0].fields.VendorAddress.valueAddress.houseNumber"),
                        CodigoPostal = parsedJson.ObtenerPropiedad("documents[0].fields.VendorAddress.valueAddress.postalCode"),
                        Municipio = parsedJson.ObtenerPropiedad("documents[0].fields.VendorAddress.valueAddress.city"),
                        bi = baseImponible,
                        ClaseDePago = parsedJson.ObtenerPropiedad("documents[0].fields.PaymentTerm.valueString"),
                        totalIva = parsedJson.ObtenerPropiedad("documents[0].fields.TotalTax.valueCurrency.amount"),
                        Concepto = contenido,
                    };

                    return JsonConvert.SerializeObject(datosDeFacturaJson, Formatting.Indented);

                }
            }
        }



        private static string QuitarPropiedadesDeLaFactura(string json)
        {
            JObject facturaJson = JObject.Parse(json);

            JArray paginasJson = Paginas(facturaJson);

            foreach (JObject paginaJson in paginasJson)
            {
                extJson.QuitarPropiedades(paginaJson, new List<string> { "boundingRegions", "confidence", "spans" });
            }

            JObject result = new JObject();
            result["documents"] = paginasJson;

            return result.ToString(Formatting.Indented);
        }


        private static JArray Paginas(JObject invoice)
        {
            if (invoice["data"] is JObject data &&
                data["apyhub"] is JObject apyhub &&
                apyhub["documents"] is JArray documents)
            {
                return documents;
            }
            else
            {
                return new JArray();
            }
        }

        public void Dispose()
        {
            if (_cliente is not null)
                _cliente.Dispose();
        }

        public Task<string> AnalizarTextoParaFiltros(string origen)
        {
            throw new NotImplementedException();
        }
    }
}
