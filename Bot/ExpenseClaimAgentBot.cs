using ExpenseClaimProject.Bot.Agents;
using ExpenseClaimProject.Bot.Plugins;
using ExpenseClaimProject.service;
using Microsoft.Agents.Builder;
using Microsoft.Agents.Builder.App;
using Microsoft.Agents.Builder.State;
using Microsoft.Agents.Core.Models;
using Microsoft.Graph;
using Microsoft.Graph.Privacy;
using Microsoft.Identity;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace ExpenseClaimProject.Bot
{
    public class ExpenseClaimAgentBot : AgentApplication
    {
        private readonly Kernel _kernel;
        private ExpenseAgent _expenseClaimAgent;
        private readonly IHttpClientFactory _httpFactory;
        private readonly IServiceProvider _services;

        private readonly GraphServiceClient _graphClient;

        public ExpenseClaimAgentBot(AgentApplicationOptions options, Kernel kernel, IServiceProvider services, IHttpClientFactory httpFactory, GraphServiceClient graphClient)
            : base(options)
        {
            _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));

            _graphClient = graphClient;
            

            _httpFactory = httpFactory;
            _services = services;

            OnConversationUpdate(ConversationUpdateEvents.MembersAdded, WelcomeMessageAsync);
            OnActivity(ActivityTypes.Message, MessageActivityAsync, rank: RouteRank.Last);
        }

        protected async Task MessageActivityAsync(
            ITurnContext turnContext,
            ITurnState turnState,
            CancellationToken cancellationToken)
        {

            await turnContext.StreamingResponse
                 .QueueInformativeUpdateAsync("Working on your request…");






            _expenseClaimAgent = new ExpenseAgent(_kernel, _services);
            var chatHistory = turnState.GetValue(
                "conversation.chatHistory",
                () => new ChatHistory());




            var submittedById = turnContext.Activity.From?.Id ?? "";
            var submittedByName = turnContext.Activity.From?.Name ?? "";





            Console.WriteLine(turnContext.Activity.Attachments);


            var fileAttachments = turnContext.Activity.Attachments?
                .Where(a =>
                    (a.Content is JObject info && info["downloadUrl"] != null)
                    || !string.IsNullOrEmpty(a.ContentUrl)
                )
                .ToList()
                ?? new List<Attachment>();

            if (fileAttachments.Any())
            {
                // Process each uploaded file
                foreach (var attachment in fileAttachments)
                {
                    if (attachment.Content is JsonElement jsonElem
                        && jsonElem.ValueKind == JsonValueKind.Object)
                    {



                        Console.WriteLine($"This is attachment properties: {attachment.ToString()}");
                        
                    
                        Console.WriteLine(jsonElem.GetRawText());



                        string attachmentName = attachment.Name;



                        //string contentUrl = attachment.ContentUrl;
                        string downloadUrl = null;
                        if (jsonElem.TryGetProperty("downloadUrl", out var urlProp))
                        {
                            downloadUrl = urlProp.GetString();
                        }



                        // Build payload with user info
                        var payload = new Dictionary<string, object>
                        {
                            ["downloadUrl"] = downloadUrl,
                            ["submittedById"] = submittedById,
                            ["submittedByName"] = submittedByName,
                            ["attachmentName"] =attachmentName
                        };



                        string payloadJson = JsonSerializer.Serialize(payload);


                        var agentResponse = await _expenseClaimAgent
                            .InvokeAgentAsync(payloadJson, chatHistory);

                        if (agentResponse.ContentType == ExpenseClaimAgentResponseType.AdaptiveCard)
                        {
                            await turnContext.SendActivityAsync(
                                MessageFactory.Attachment(new Attachment
                                {
                                    ContentType =
                                        "application/vnd.microsoft.card.adaptive",
                                    Content = JsonDocument
                                        .Parse(agentResponse.Content)
                                        .RootElement
                                }),
                                cancellationToken);
                        }
                        else
                        {
                            await turnContext.SendActivityAsync(
                                MessageFactory.Text(agentResponse.Content),
                                cancellationToken);
                        }
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(
                            MessageFactory.Text($"Attachment.Content is type {attachment.Content?.GetType().Name}"),
                            cancellationToken);
                    }


                }



                return;
            }


            if (turnContext.Activity.Value != null)
            {

                var valueDict= JsonSerializer.Deserialize<Dictionary<string,object>>(JsonSerializer.Serialize(turnContext.Activity.Value));
                valueDict["submittedById"] = submittedById;

                valueDict["submmittedByName"] = submittedByName;


                string valueJson = JsonSerializer.Serialize(valueDict);
                var agentResponse = await _expenseClaimAgent.InvokeAgentAsync(valueJson, chatHistory);

                if (agentResponse.ContentType == ExpenseClaimAgentResponseType.AdaptiveCard)
                {

                    Console.WriteLine($"the output is {JsonDocument.Parse(agentResponse.Content).RootElement.GetRawText()}");
                    await turnContext.SendActivityAsync(
                        MessageFactory.Attachment(new Attachment
                        {
                            ContentType = "application/vnd.microsoft.card.adaptive",
                            Content = JsonDocument.Parse(agentResponse.Content).RootElement.GetRawText()
                        }),
                        cancellationToken);
                }
                else
                {
                    await turnContext.SendActivityAsync(
                        MessageFactory.Text(agentResponse.Content),
                        cancellationToken);
                }
                return;
            }





            var userText = turnContext.Activity.Text?.Trim();
            if (!string.IsNullOrEmpty(userText))
            {

                var textPayload = new Dictionary<string, object>
                {
                    ["text"] = userText,
                    ["submittedById"] = submittedById,
                    ["submittedByName"] = submittedByName,
                };

                string payloadJson = JsonSerializer.Serialize(textPayload);


                var agentResponse = await _expenseClaimAgent
                    .InvokeAgentAsync(payloadJson, chatHistory);

                if (agentResponse.ContentType ==
                    ExpenseClaimAgentResponseType.AdaptiveCard)
                {
                    await turnContext.SendActivityAsync(
                        MessageFactory.Attachment(new Attachment
                        {
                            ContentType =
                                "application/vnd.microsoft.card.adaptive",
                            Content = JsonDocument
                                .Parse(agentResponse.Content)
                                .RootElement
                                .GetRawText()
                        }),
                        cancellationToken);
                }
                else
                {
                    await turnContext.SendActivityAsync(
                        MessageFactory.Text(agentResponse.Content),
                        cancellationToken);
                }
            }
            else
            {
                // 4) Neither text nor files? prompt the user
                await turnContext.SendActivityAsync(
                    MessageFactory.Text(
                        "Please send some text or upload a receipt image to proceed."),
                    cancellationToken);
            }
            await turnContext.StreamingResponse.EndStreamAsync(cancellationToken); // End the streaming response

        }


        protected async Task WelcomeMessageAsync(
            ITurnContext turnContext,
            ITurnState turnState,
            CancellationToken cancellationToken)
        {
            foreach (var member in turnContext.Activity.MembersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(
                        MessageFactory.Text(
                            "Hello and Welcome! I'm here to help with your expense claims!"),
                        cancellationToken);
                }
            }
        }
    }
}
