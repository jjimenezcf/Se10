using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Utilidades;

namespace MVCSistemaDeElementos.Controllers
{
    public class PdfServerSettings
    {
        public string BaseUrl { get; set; } = "https://pdf-api.javiercampos.info/";
    }

    public class PdfServerConvertReportDto
    {
        public ReportData reportData { get; set; }
    }
    public class ReportData
    {
        public string Header { get; set; } = "";
        public string Footer { get; set; } = "";
        public string Body { get; set; } = "";
    }

    public interface IPdfServerClient
    {
        Task<Stream> ConvertReportAsync(ReportData report, CancellationToken cancellationToken = default);
        Task<Stream> ConvertReportAsync(ReportData[] report, CancellationToken cancellationToken = default);
        Stream ConvertReport(PdfServerConvertReportDto report);
        Stream ConvertReport(PdfServerConvertReportDto[] report);
    }

    public class PdfServerApiClient : IPdfServerClient
    {
        private readonly HttpClient _client;
        
        public PdfServerApiClient(HttpClient client)
        {
            _client = client;
            var options = new PdfServerSettings();
            _client.BaseAddress = new Uri(options.BaseUrl);
        }

        public Task<Stream> ConvertReportAsync(ReportData report, CancellationToken cancellationToken = default) =>
            ConvertReportAsync(new[] { report }, cancellationToken);
        public async Task<Stream> ConvertReportAsync(ReportData[] report, CancellationToken cancellationToken = default)
        {
            var response = await _client.PostAsJsonAsync("convert-report", report, cancellationToken: cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStreamAsync(cancellationToken);
        }
        
        public Stream ConvertReport(PdfServerConvertReportDto report) => ConvertReport(new[] { report });
        public Stream ConvertReport(PdfServerConvertReportDto[] report)
        {
            var message = new HttpRequestMessage(HttpMethod.Post, "convert-report");
            var json = JsonSerializer.Serialize(report[0], new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            message.Content = new StringContent(json, Encoding.UTF8, MimeTypeMap.ApplicationJson);
            var response  = _client.Send(message);
            response.EnsureSuccessStatusCode();
            return response.Content.ReadAsStream();
        }
    }
}