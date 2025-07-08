using System;
using System.ComponentModel;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Azure;
using Azure.AI.DocumentIntelligence;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Diagnostics;

namespace ExpenseClaimProject.Bot.Plugins
{
    /// <summary>
    /// Plugin to download a receipt image from a URL and extract line-item data using Azure Document Intelligence.
    ///// </summary>
    public class ReceiptProcessingPlugin
    {
        private readonly DocumentIntelligenceClient _ocrClient;
        private readonly HttpClient _httpClient;

        public ReceiptProcessingPlugin(DocumentIntelligenceClient ocrClient, IHttpClientFactory httpFactory)
        {
            _ocrClient = ocrClient ?? throw new ArgumentNullException(nameof(ocrClient));
            if (httpFactory is null) throw new ArgumentNullException(nameof(httpFactory));
            _httpClient = httpFactory.CreateClient("WebClient");
        }

        [KernelFunction("ExtractReceiptData")]
        [Description("Processes receipt images and extracts structured data using OCR.")]
        public async Task<string> ExtractReceiptDataAsync(string imageUrl)
        {
            using var resp = await _httpClient.GetAsync(imageUrl);
            resp.EnsureSuccessStatusCode();
            var imageBytes = await resp.Content.ReadAsByteArrayAsync();


            var data = BinaryData.FromBytes(imageBytes);

            Console.WriteLine(_ocrClient);

            var operation = await _ocrClient.AnalyzeDocumentAsync(
                WaitUntil.Completed,
                modelId: "prebuilt-receipt",
                bytesSource: data);

            var result = operation.Value;
            var options = new JsonSerializerOptions { WriteIndented = true };
            return JsonSerializer.Serialize(result, options);
        }


    }

}



