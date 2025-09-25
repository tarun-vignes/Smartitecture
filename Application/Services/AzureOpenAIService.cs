using System;
using System.Threading.Tasks;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Options;

namespace Smartitecture.Services
{
    /// <summary>
    /// Configuration class for Azure OpenAI service settings.
    /// These settings should be configured in appsettings.json.
    /// </summary>
    public class AzureOpenAIConfiguration
    {
        /// <summary>Gets or sets the Azure OpenAI endpoint URL</summary>
        public string Endpoint { get; set; }

        /// <summary>Gets or sets the Azure OpenAI API key</summary>
        public string ApiKey { get; set; }

        /// <summary>Gets or sets the model deployment name</summary>
        public string DeploymentName { get; set; }
    }

    /// <summary>
    /// Implementation of ILLMService using Azure OpenAI.
    /// Provides natural language processing and command parsing capabilities
    /// using Azure's hosted language models.
    /// </summary>
    public class AzureOpenAIService : ILLMService
    {
        private readonly OpenAIClient _client;
        private readonly string _deploymentName;

        /// <summary>
        /// Initializes a new instance of AzureOpenAIService.
        /// Sets up the OpenAI client with retry policies and authentication.
        /// </summary>
        /// <param name="config">Configuration options for Azure OpenAI</param>
        public AzureOpenAIService(IOptions<AzureOpenAIConfiguration> config)
        {
            // Configure client options with retry policy for reliability
            var options = new OpenAIClientOptions
            {
                RetryPolicy = new Azure.Core.RetryPolicy(maxRetries: 3, delay: TimeSpan.FromSeconds(2))
            };

            // Initialize the OpenAI client with endpoint and credentials
            _client = new OpenAIClient(
                new Uri(config.Value.Endpoint),
                new AzureKeyCredential(config.Value.ApiKey),
                options);

            _deploymentName = config.Value.DeploymentName;
        }

        /// <summary>
        /// Gets a response from the AI model for general conversation.
        /// Configures the model to act as AIPal and provide Windows-focused responses.
        /// </summary>
        /// <param name="userInput">The user's message or query</param>
        /// <returns>The AI model's response, or an error message if the call fails</returns>
        public async Task<string> GetResponseAsync(string userInput)
        {
            try
            {
                // Configure chat completion options
                var chatCompletionsOptions = new ChatCompletionsOptions
                {
                    Messages =
                    {
                        new ChatMessage(ChatRole.System, "You are AIPal, a Windows AI assistant. Keep responses concise and focused on Windows tasks."),
                        new ChatMessage(ChatRole.User, userInput)
                    },
                    Temperature = 0.7f,  // Balance between creativity and consistency
                    MaxTokens = 800      // Limit response length
                };

                // Get completion from Azure OpenAI
                var response = await _client.GetChatCompletionsAsync(
                    _deploymentName,
                    chatCompletionsOptions);

                return response.Value.Choices[0].Message.Content;
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        /// <summary>
        /// Attempts to parse user input into a system command and parameters.
        /// Uses a more constrained temperature setting for more precise command parsing.
        /// </summary>
        /// <param name="userInput">The user's message or command</param>
        /// <returns>
        /// A tuple containing the command name and parameters.
        /// Returns ("Unknown", []) if parsing fails or no command is detected.
        /// </returns>
        public async Task<(string commandName, string[] parameters)> ParseCommandAsync(string userInput)
        {
            try
            {
                // Configure chat completion options for command parsing
                var chatCompletionsOptions = new ChatCompletionsOptions
                {
                    Messages =
                    {
                        new ChatMessage(ChatRole.System, "Parse the user input into a command name and parameters. Respond in format: CommandName|param1,param2"),
                        new ChatMessage(ChatRole.User, userInput)
                    },
                    Temperature = 0.1f,   // Lower temperature for more deterministic results
                    MaxTokens = 100       // Short response for command parsing
                };

                // Get completion from Azure OpenAI
                var response = await _client.GetChatCompletionsAsync(
                    _deploymentName,
                    chatCompletionsOptions);

                // Parse the response into command and parameters
                var result = response.Value.Choices[0].Message.Content.Split('|');
                if (result.Length != 2)
                    return ("Unknown", Array.Empty<string>());

                return (result[0], result[1].Split(',', StringSplitOptions.RemoveEmptyEntries));
            }
            catch (Exception)
            {
                return ("Unknown", Array.Empty<string>());
            }
        }
    }
}
