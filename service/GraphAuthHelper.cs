


using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;

using ExpenseClaimProject;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;

namespace ExpenseClaimProject.service;

public class GraphAuthHelper
{
    private readonly AzureAdOptions _azureAdOptions;

    public GraphAuthHelper(IOptions<AzureAdOptions> azureAdOptions)
    {
        _azureAdOptions = azureAdOptions.Value;
    }

    public async Task<string> GetAccessTokenAsync()
    {
        var authority = $"https://login.microsoftonline.com/{_azureAdOptions.TenantId}";
        var app = ConfidentialClientApplicationBuilder.Create(_azureAdOptions.ClientId)
            .WithClientSecret(_azureAdOptions.ClientSecret)
            .WithAuthority(new Uri(authority))
            .Build();

        string[] scopes = new[] { "https://graph.microsoft.com/.default" };
        var result = await app.AcquireTokenForClient(scopes).ExecuteAsync();
        return result.AccessToken;
    }
}
