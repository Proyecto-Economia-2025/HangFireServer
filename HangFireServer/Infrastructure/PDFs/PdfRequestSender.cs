using HangFireServer.Core.DTOs;
using HangFireServer.Domain.Interfaces;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;

namespace HangFireServer.Infrastructure.PDFs
{
    public class PdfRequestSender : IPdfRequestSender
    {
        private readonly HttpClient _httpClient;
        private readonly string _pdfServiceBaseUrl;

        public PdfRequestSender(HttpClient httpClient, IOptions<PdfServiceSettings> pdfServiceSettings)
        {
            _httpClient = httpClient;
            _pdfServiceBaseUrl = pdfServiceSettings.Value.BaseUrl;
        }

        public async Task<string> SendPdfRequestAsync(BaseRequest request)
        {
            var jsonContent = JsonConvert.SerializeObject(request);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // URL fija del PDF service
            string url = $"{_pdfServiceBaseUrl}/api/PDF/get-top-products";

            Console.WriteLine($"[INFO] Enviando request a PDF Service: {url}");

            var response = await _httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[INFO] Request exitoso a PDF Service: {url}");
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[ERROR] Error HTTP {response.StatusCode} en {url}: {errorBody}");
                throw new Exception($"Error HTTP {response.StatusCode}: {errorBody}");
            }
        }
    }

    // Configuración simplificada para PDF Service http://localhost:7298/api/PDF/get-top-products
    public class PdfServiceSettings
    {
        public string BaseUrl { get; set; } = "https://localhost:7298";
        //la u me bloquea la red asi que ha usar la local "https://pdf.escritorio.tonyml.com";
    }
}