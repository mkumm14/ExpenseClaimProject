using Azure.AI.DocumentIntelligence;
using Azure.Identity;
using ExpenseClaimProject;
using ExpenseClaimProject.Bot.Agents;
using ExpenseClaimProject.Bot.Plugins;
using ExpenseClaimProject.service;
using Microsoft.Agents.Builder;
using Microsoft.Agents.Builder.App;
using Microsoft.Agents.Hosting.AspNetCore;
using Microsoft.Agents.Storage;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.SemanticKernel;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHttpClient("WebClient", client => client.Timeout = TimeSpan.FromSeconds(600));
builder.Services.AddHttpContextAccessor();
builder.Logging.AddConsole();


// Register Semantic Kernel
builder.Services.AddKernel();

// Register the AI service of your choice. AzureOpenAI and OpenAI are demonstrated...
var config = builder.Configuration.Get<ConfigOptions>();


builder.Services.Configure<ConfigOptions>(builder.Configuration);

builder.WebHost.ConfigureKestrel(options =>
{
    // Remove any limit (or set to whatever makes sense, e.g. 100 MB)
    options.Limits.MaxRequestBodySize = null;
});


builder.Services.Configure<AzureAdOptions>(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddAzureOpenAIChatCompletion(
    deploymentName: config.Azure.OpenAIDeploymentName,
    endpoint: config.Azure.OpenAIEndpoint,
    apiKey: config.Azure.OpenAIApiKey
);


builder.Services.AddSingleton<GraphServiceClient>(sp =>
{

    var opts = sp.GetRequiredService<IOptions<AzureAdOptions>>().Value;

    var cred = new ClientSecretCredential(
        opts.TenantId, opts.ClientId, opts.ClientSecret);

   
    var scopes = new[] { "https://graph.microsoft.com/.default" };

    return new GraphServiceClient(cred, scopes);
});


builder.Services.AddSingleton<DocumentIntelligenceClient>(sp =>
{
    return new DocumentIntelligenceClient(new Uri(config.Azure.DocumentIntelligenceEndpoint), new Azure.AzureKeyCredential(config.Azure.DocumentIntelligenceKey));
});



builder.Services.AddSingleton<ReceiptProcessingPlugin>();

builder.Services.AddSingleton<AdaptiveCardPlugin>();



builder.Services.AddSingleton<SharePointPlugin>();


builder.Services.AddTransient<ExpenseAgent>();

// Add AspNet token validation
builder.Services.AddBotAspNetAuthentication(builder.Configuration);

// Register IStorage.  For development, MemoryStorage is suitable.
// For production Agents, persisted storage should be used so
// that state survives Agent restarts, and operate correctly
// in a cluster of Agent instances.
builder.Services.AddSingleton<IStorage, MemoryStorage>();

// Add AgentApplicationOptions from config.
builder.AddAgentApplicationOptions();

// Add AgentApplicationOptions.  This will use DI'd services and IConfiguration for construction.
builder.Services.AddTransient<AgentApplicationOptions>();

// Add the bot (which is transient)
builder.AddAgent<ExpenseClaimProject.Bot.ExpenseClaimAgentBot>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/api/messages", async (HttpRequest request, HttpResponse response, IAgentHttpAdapter adapter, IAgent agent, CancellationToken cancellationToken) =>
{
    await adapter.ProcessAsync(request, response, agent, cancellationToken);
});

if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Playground")
{
    app.MapGet("/", () => "Weather Bot");
    app.UseDeveloperExceptionPage();
    app.MapControllers().AllowAnonymous();
}
else
{
    app.MapControllers();
}

app.Run();

