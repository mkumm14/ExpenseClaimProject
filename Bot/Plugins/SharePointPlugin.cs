using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.Security;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ExpenseClaimProject.Bot.Plugins
{
    public class SharePointPlugin
    {
        private readonly GraphServiceClient _graph;
        private readonly SharepointConfigOptions _sharepointOptions;
        private readonly Lazy<Task<Site>> _siteLazy;

        public SharePointPlugin(GraphServiceClient graphClient, IOptions<ConfigOptions> configOptions)
        {
            _graph = graphClient;
            _sharepointOptions = configOptions.Value.Sharepoint;

            _siteLazy= new Lazy<Task<Site>>(async () =>
            {
                // Compose the site key for SharePoint
                string siteKey = $"{_sharepointOptions.HostName}:{_sharepointOptions.SitePath}";
                return await _graph.Sites[siteKey].GetAsync();
            });
        }

        private async Task<Site> GetSiteAsync()
        {
            // Ensure the site is loaded
            return await _siteLazy.Value;
        }

        private async Task<List> GetListAsync()
        {
            var site = await GetSiteAsync();
            return await _graph.Sites[site.Id].Lists[_sharepointOptions.ListName].GetAsync();
        }



        [KernelFunction("UploadExpenseData")]
        [Description("Uploads expense data to a SharePoint list.")]
        public async Task<string> UploadExpenseDataAsync(string expenseJson)
        {
            Console.WriteLine($"[SharePointPlugin] ⚙️ UploadExpenseDataAsync called with payload:\n{expenseJson}");


            Console.WriteLine("sharepointOptions: " + JsonSerializer.Serialize(_sharepointOptions));  

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
            string employeeId = root.TryGetProperty("submittedById", out var empIdProp) ? empIdProp.GetString() ?? "" : ""; 
            var site = await GetSiteAsync();
            var list = await GetListAsync();




            Console.WriteLine($"[SharePointPlugin] ⚙️ Site found: {site.Id} ({site.Name})");

            Console.WriteLine($"[SharePointPlugin] ⚙️ List found: {list.Id} ({list.Name})");

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




        [KernelFunction("GetClaimsByStatus")]
        [Description("Gets claims filtered by status")]
        public async Task<string> GetClaimsByStatusAsync(string submittedByName, string status)
        {
            try
            {

                var site = await GetSiteAsync();
                var list = await GetListAsync();

                // Graph API call with status filter
                var listItems = await _graph.Sites[site.Id].Lists[list.Id].Items
                    .GetAsync(requestConfiguration =>
                    {
                        requestConfiguration.QueryParameters.Expand = new string[] { "fields" };
                        requestConfiguration.QueryParameters.Filter = $"fields/EmployeeName eq '{submittedByName}' and fields/Status eq '{status}'";
                    });

                Console.WriteLine($"[SharePointPlugin] ⚙️ Found {listItems.Value.Count} claims for {submittedByName} with status {status}");


                // Replace the usage of GetValueOrDefault with a safe access pattern
                var claims = listItems.Value.Select(item => new
                {
                    Id = item.Id,
                    MerchantName = item.Fields.AdditionalData.ContainsKey("MerchantName") ? item.Fields.AdditionalData["MerchantName"]?.ToString() : null,
                    TransactionDate = item.Fields.AdditionalData.ContainsKey("TransactionDate") ? item.Fields.AdditionalData["TransactionDate"]?.ToString() : null,
                    Total = item.Fields.AdditionalData.ContainsKey("Total") ? item.Fields.AdditionalData["Total"]?.ToString() : null,
                    Currency = item.Fields.AdditionalData.ContainsKey("Currency") ? item.Fields.AdditionalData["Currency"]?.ToString() : null,
                    Category = item.Fields.AdditionalData.ContainsKey("Category") ? item.Fields.AdditionalData["Category"]?.ToString() : null,
                    Status = item.Fields.AdditionalData.ContainsKey("Status") ? item.Fields.AdditionalData["Status"]?.ToString() : null
                }).ToList();

                return JsonSerializer.Serialize(claims, new JsonSerializerOptions { WriteIndented = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving claims by status: {ex.Message}");
                return JsonSerializer.Serialize(new { Error = ex.Message });
            }
        }
    }
}
