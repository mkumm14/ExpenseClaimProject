namespace ExpenseClaimProject
{
    public class ConfigOptions
    {
        public AzureConfigOptions Azure { get; set; }

        public SharepointConfigOptions Sharepoint { get; set; }

    }

    /// <summary>
    /// Options for Azure OpenAI and Azure Content Safety
    /// </summary>
    public class AzureConfigOptions
    {
        public string OpenAIApiKey { get; set; }
        public string OpenAIEndpoint { get; set; }
        public string OpenAIDeploymentName { get; set; }


        public string DocumentIntelligenceKey { get; set; }

        public string DocumentIntelligenceEndpoint { get; set; }
    }


    public class SharepointConfigOptions
    {
        public string HostName { get; set; }
        public string SitePath { get; set; }
        public string ListName { get; set; }

    }


}