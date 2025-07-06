using System.ComponentModel;
using System.Text.Json.Serialization;

namespace ExpenseClaimProject.Bot.Agents;

public enum ExpenseClaimAgentResponseType
{
    [JsonPropertyName("text")]
    Text,

    [JsonPropertyName("adaptive-card")]
    AdaptiveCard
}

public class ExpenseClaimAgentResponse
{
    [JsonPropertyName("contentType")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ExpenseClaimAgentResponseType ContentType { get; set; }

    [JsonPropertyName("content")]
    [Description("The content of the response, may be plain text, or JSON based adaptive card but must be a string.")]
    public string Content { get; set; }
}
