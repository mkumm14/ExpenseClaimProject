using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.Security;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ExpenseClaimProject.Bot.Plugins
{
    public class SharePointPlugin
    {
        private readonly GraphServiceClient _graph;
        private readonly SharepointConfigOptions _sharepointOptions;

        public SharePointPlugin(GraphServiceClient graphClient, IOptions<ConfigOptions> configOptions)
        {
            _graph = graphClient;
            _sharepointOptions = configOptions.Value.Sharepoint;
        }

        [KernelFunction("UploadExpenseData")]
        public async Task<string> UploadExpenseDataAsync(string expenseJson)
        {
            Console.WriteLine($"[SharePointPlugin] ⚙️ UploadExpenseDataAsync called with payload:\n{expenseJson}");


            Console.WriteLine("sarepointOptions: " + JsonSerializer.Serialize(_sharepointOptions));  

            // Parse the expense JSON
            using var doc = JsonDocument.Parse(expenseJson);
            var root = doc.RootElement;

            // Get properties safely
            string merchantName = root.TryGetProperty("merchantName", out var merchantNameProp) ? merchantNameProp.GetString() ?? "" : "";
            string transactionDate = root.TryGetProperty("transactionDate", out var dateProp) ? dateProp.GetString() ?? "" : "";
            string total = root.TryGetProperty("total", out var totalProp) ? totalProp.ToString() : "0";
            string currency = root.TryGetProperty("currency", out var currProp) ? currProp.GetString() ?? "" : "";
            string category = root.TryGetProperty("category", out var catProp) ? catProp.GetString() ?? "" : "";
            string paymentMethod = root.TryGetProperty("paymentMethod", out var pmProp) ? pmProp.GetString() ?? "" : "";
            string fourDigits = root.TryGetProperty("fourDigits", out var fourProp) ? fourProp.GetString() ?? "" : "";
            string expenseType = root.TryGetProperty("expenseType", out var expenseProp) ? expenseProp.GetString() ?? "" : "";
            string notes = root.TryGetProperty("notes", out var notesProp) ? notesProp.GetString() ?? "" : "";
            string employeeName= root.TryGetProperty("submittedByName", out var empProp) ? empProp.GetString() ?? "" : "";

            // Compose the site key for SharePoint
            string siteKey = $"{_sharepointOptions.HostName}:{_sharepointOptions.SitePath}";
            var site = await _graph.Sites[siteKey].GetAsync();


            Console.WriteLine($"[SharePointPlugin] ⚙️ Site found: {site.Id} ({site.Name})");

            // Prepare SharePoint field values (adjust field names as per your SharePoint list schema)
            var fieldValues = new Dictionary<string, object>
            {
                ["MerchantName"] = merchantName,
                ["ExpenseDate"] = transactionDate,
                ["ExpenseAmount"] = total,
                ["ExpenseCurrency"] = currency,
                ["ItemCategory"] = category,
                ["ExpenseType"] = expenseType,
                ["PaymentMethod"] = paymentMethod,
                ["FourDigits"] = paymentMethod.ToLower().Contains("card") ? fourDigits : "",
                ["EmployeeName"] = employeeName,
                ["Notes"] = notes

            };

            // Create a FieldValueSet and ListItem
            var fields = new FieldValueSet { AdditionalData = fieldValues };
            var listItem = new ListItem { Fields = fields };

            var list = await _graph.Sites[site.Id].Lists[_sharepointOptions.ListName].GetAsync();

            var listId = list.Id; // if you want to use the ID later


            Console.WriteLine($"[SharePointPlugin] ⚙️ List found: {list.Name} ({list.Id})");


            // Add item to SharePoint List
            var item = await _graph.Sites[site.Id].Lists[_sharepointOptions.ListName].Items.PostAsync(listItem);

            // Return confirmation JSON
            var result = new
            {
                siteId = site.Id,
                itemId = item.Id
            };
            return JsonSerializer.Serialize(result);
        }
    }
}
