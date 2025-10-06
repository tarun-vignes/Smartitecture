namespace AIPal.Services
{
    /// <summary>
    /// Options for configuring the Azure OpenAI service.
    /// </summary>
    public class AzureOpenAIOptions
    {
        /// <summary>
        /// Gets or sets the Azure OpenAI endpoint URL.
        /// </summary>
        public string Endpoint { get; set; }

        /// <summary>
        /// Gets or sets the API key for Azure OpenAI.
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// Gets or sets the deployment name (model) to use.
        /// </summary>
        public string DeploymentName { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of tokens to generate.
        /// </summary>
        public int MaxTokens { get; set; } = 1000;

        /// <summary>
        /// Gets or sets the temperature for generation (0.0 to 1.0).
        /// </summary>
        public float Temperature { get; set; } = 0.7f;
    }
}
