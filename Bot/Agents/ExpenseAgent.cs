using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text;
using System.Text.Json.Nodes;
using ExpenseClaimProject.Bot.Plugins;

namespace ExpenseClaimProject.Bot.Agents;

public class ExpenseAgent
{
    private readonly Kernel _kernel;
    private readonly ChatCompletionAgent _agent;

    private const string AgentName = "ExpenseClaimAgent";



    private const string AgentInstructions = """"  
            You are an intelligent Expense-Claim assistant.  
            Your job is to help users submit expense claims by guiding them through the process with clarity and simplicity.  
            You always respond with exactly **one JSON object** with these fields:
            - `contentType`: `"Text"` or `"AdaptiveCard"`
            - `content`: plain text or a complete Adaptive Card JSON (version 1.5)

            Do **not** include other fields such as tax, tips, address, etc., even if present in the OCR or user input.
            
            ----------
 
            ### Your responsibilities:
 
            1. **Start the conversation**
               - If this is the first message, greet the user and ask them to upload a receipt or provide expense details.
 
            2. **If a receipt image or URL is provided**
               - Extract receipt data from the image using the downloadUrl. Add the submittedById and submittedByName to the extracted data.
               - Use that data to generate a pre-filled form as an Adaptive Card.
               - If some fields are missing, show them with empty text inputs for the user to complete.
            
            3. **If the user is done with editing their form**  
               - When the incoming message includes `"action": "submitValidationForm"`, generate a confirmation card (read-only summary of their data) with submit and cancel buttons.

            4. **If the user confirms submission**  
               - When the incoming message includes `"action": "submitClaim"`, store the data and return a confirmation message with the generated item ID.

            5. **If the user cancels**  
               - If the incoming message includes `"action": "cancelClaim"`, acknowledge the cancellation.

            6. **Handle claim queries**
               - If the user asks about claims with a specific status (pending approval, approved, rejected), use the submittedByName and status.
               - Format the response in a user-friendly way, showing the claim details in a readable format.
               - If no claims are found for the requested status, inform the user politely.
               - Examples of queries this handles:
                 * "How many pending claims do I have?"
                 * "Show me my approved claims"
                 * "What rejected claims do I have?"
                 * "List my pending expenses"

            7. **Fallback**
               - If you cannot interpret the user input, politely ask them to upload a receipt or provide expense details.
 
            ----------
 
            You may use available tools and plugins as needed to fulfill the user’s intent.  
            Do not fabricate data — always rely on plugin output where applicable.  
            Never return anything except the required JSON structure.
 
            """";




    public ExpenseAgent(Kernel kernel, IServiceProvider service)
    {
        _kernel = kernel;

        // Define the agent
        _agent =
            new()
            {
                Instructions = AgentInstructions,
                Name = AgentName,
                Kernel = _kernel,
                Arguments = new KernelArguments(new OpenAIPromptExecutionSettings() 
                { 
                    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(), 
                    ResponseFormat = "json_object" 
                }),
            };



        // add receipt processing plugin
        _agent.Kernel.Plugins.Add(KernelPluginFactory.CreateFromType<AdaptiveCardPlugin>(serviceProvider: service));
        _agent.Kernel.Plugins.Add(KernelPluginFactory.CreateFromType<ReceiptProcessingPlugin>(serviceProvider: service));
        _agent.Kernel.Plugins.Add(KernelPluginFactory.CreateFromType<SharePointPlugin>(serviceProvider: service));

    }

    public async Task<ExpenseClaimAgentResponse> InvokeAgentAsync(string input, ChatHistory chatHistory)
    {
        ArgumentNullException.ThrowIfNull(chatHistory);
        AgentThread thread = new ChatHistoryAgentThread();
        ChatMessageContent message = new(AuthorRole.User, input);
        chatHistory.Add(message);

        StringBuilder sb = new();
        await foreach (ChatMessageContent response in this._agent.InvokeAsync(chatHistory, thread: thread))
        {
            chatHistory.Add(response);
            sb.Append(response.Content);
        }

        // Make sure the response is in the correct format and retry if necessary
        try
        {
            string resultContent = sb.ToString();
            var jsonNode = JsonNode.Parse(resultContent);
            ExpenseClaimAgentResponse result = new ExpenseClaimAgentResponse()
            {
                Content = jsonNode["content"].ToString(),
                ContentType = Enum.Parse<ExpenseClaimAgentResponseType>(jsonNode["contentType"].ToString(), true)
            };
            return result;
        }
        catch (Exception je)
        {
            return await InvokeAgentAsync($"That response did not match the expected format. Please try again. Error: {je.Message}", chatHistory);
        }
    }
}
