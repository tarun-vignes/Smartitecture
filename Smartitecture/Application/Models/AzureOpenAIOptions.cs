namespace Smartitecture.Application.Models
{
    /// <summary>
    /// Represents the configuration options for Azure OpenAI service.
    /// </summary>
    public class AzureOpenAIOptions
    {
        /// <summary>
        /// Gets or sets the Azure OpenAI endpoint URL.
        /// </summary>
        public string Endpoint { get; set; }

        /// <summary>
        /// Gets or sets the Azure OpenAI API key.
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// Gets or sets the deployment name for the model.
        /// </summary>
        public string DeploymentName { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of tokens to generate in the response.
        /// </summary>
        public int MaxTokens { get; set; } = 1000;

        /// <summary>
        /// Gets or sets the temperature for text generation (0.0 to 1.0).
        /// </summary>
        public float Temperature { get; set; } = 0.7f;

        /// <summary>
        /// Gets or sets the top-p value for nucleus sampling (0.0 to 1.0).
        /// </summary>
        public float TopP { get; set; } = 0.9f;

        /// <summary>
        /// Gets or sets the presence penalty for text generation (-2.0 to 2.0).
        /// </summary>
        public float PresencePenalty { get; set; } = 0f;

        /// <summary>
        /// Gets or sets the frequency penalty for text generation (-2.0 to 2.0).
        /// </summary>
        public float FrequencyPenalty { get; set; } = 0f;
    }
}
