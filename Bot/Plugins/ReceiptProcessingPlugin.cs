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

        //    [KernelFunction("ExtractReceiptData")]
        //    [Description("Processes receipt images and extracts structured data using OCR.")]
        //    public async Task<string> ExtractReceiptDataAsync(string imageUrl)
        //    {
        //        using var resp = await _httpClient.GetAsync(imageUrl);
        //        resp.EnsureSuccessStatusCode();
        //        var imageBytes = await resp.Content.ReadAsByteArrayAsync();


        //        var data = BinaryData.FromBytes(imageBytes);

        //        var operation = await _ocrClient.AnalyzeDocumentAsync(
        //            WaitUntil.Completed,
        //            modelId: "prebuilt-receipt",
        //            bytesSource: data);

        //        var result = operation.Value;
        //        var options = new JsonSerializerOptions { WriteIndented = true };
        //        return JsonSerializer.Serialize(result, options);
        //    }
        //}

        [KernelFunction("ExtractReceiptData")]
        [Description("Processes receipt images and extracts structured data using OCR.")]
        public async Task<string> ExtractReceiptDataAsync(string imageUrl)
        {
            try
            {
                Console.WriteLine($"[ReceiptProcessingPlugin] Downloading image from: {imageUrl}");
                using var resp = await _httpClient.GetAsync(imageUrl);
                resp.EnsureSuccessStatusCode();

                Console.WriteLine($"[ReceiptProcessingPlugin] Image downloaded. Reading bytes...");
                var imageBytes = await resp.Content.ReadAsByteArrayAsync();

                Console.WriteLine($"[ReceiptProcessingPlugin] Image size: {imageBytes.Length} bytes. Calling Document Intelligence...");
                var data = BinaryData.FromBytes(imageBytes);

                var operation = await _ocrClient.AnalyzeDocumentAsync(
                    WaitUntil.Completed,
                    modelId: "prebuilt-receipt",
                    bytesSource: data);

                Console.WriteLine($"[ReceiptProcessingPlugin] Document Intelligence call completed. Serializing result...");
                var result = operation.Value;
                var options = new JsonSerializerOptions { WriteIndented = true };
                string jsonResult = JsonSerializer.Serialize(result, options);

                Console.WriteLine($"[ReceiptProcessingPlugin] Serialization complete. Returning result.");
                return jsonResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ReceiptProcessingPlugin] ERROR: {ex}");
                // Optionally, return the error as a string if you want to surface it to the user:
                return $"Error processing receipt: {ex.Message}";
                // Or rethrow if you want the bot framework to handle it.
                // throw;
            }
        }

    }

}



